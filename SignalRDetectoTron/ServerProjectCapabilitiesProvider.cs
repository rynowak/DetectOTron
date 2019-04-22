using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace SignalRDetectoTron
{
    [Export(ExportContractNames.Scopes.ConfiguredProject, typeof(IProjectCapabilitiesProvider))]
    [AppliesTo("CPS")]
    internal class ServerProjectCapabilitiesProvider : ConfiguredProjectCapabilitiesProviderBase
    {
        private readonly Lazy<VisualStudioWorkspace> _workspace;
        private ImmutableHashSet<string> _capabilities;

        [ImportingConstructor]
        public ServerProjectCapabilitiesProvider(ConfiguredProject configuredProject, Lazy<VisualStudioWorkspace> workspace)
            : base("ServerProjectCapabilitiesProvider", configuredProject)
        {
            _workspace = workspace;

            _capabilities = ImmutableHashSet<string>.Empty;
        }

        protected override Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            StartupAnalyzerEventSink.MiddlewareAnalysisCompleted += StartupAnalyzerEventSink_MiddlewareAnalysisCompleted;
            return base.InitializeCoreAsync(cancellationToken);
        }

        protected override Task DisposeCoreAsync(bool initialized)
        {
            StartupAnalyzerEventSink.MiddlewareAnalysisCompleted += StartupAnalyzerEventSink_MiddlewareAnalysisCompleted;
            return base.DisposeCoreAsync(initialized);
        }

        protected override async Task<ImmutableHashSet<string>> GetCapabilitiesAsync(CancellationToken cancellationToken)
        {
            return await ProjectLockService.ReadLockAsync((releaser) =>
            {
                using (releaser)
                {
                    return _capabilities;
                }
            }, cancellationToken);
        }

        public async Task UpdateCapabilitiesAsync(ImmutableHashSet<string> capabilities, CancellationToken cancellationToken)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }

            await ProjectLockService.WriteLockAsync((releaser) =>
            {
                _capabilities = capabilities;
                Refresh();
            }, cancellationToken);
        }

        private void StartupAnalyzerEventSink_MiddlewareAnalysisCompleted(object sender, MiddlewareAnalysis e)
        {
            // Fire-and-forget. We need to get to the UI thread for DTE/IVsHierarchy.
            JoinableCollection.Add(JoinableFactory.RunAsync(async () =>
            {
                await JoinableFactory.SwitchToMainThreadAsync();

                var solution = _workspace.Value.CurrentSolution;
                var syntaxTrees = new HashSet<SyntaxTree>();
                foreach (var syntax in e.ConfigureMethod.DeclaringSyntaxReferences)
                {
                    syntaxTrees.Add(syntax.SyntaxTree);
                }

                var documentIds = syntaxTrees.Select(s => solution.GetDocumentId(s));
                var hierarchies = documentIds.Select(d => _workspace.Value.GetHierarchy(d.ProjectId)).Where(h => h != null);

                foreach (var hierarchy in hierarchies)
                {
                    if (hierarchy is IVsBrowseObjectContext context && context.UnconfiguredProject == ConfiguredProject.UnconfiguredProject)
                    {
                        var capabilities = ImmutableHashSet<string>.Empty;
                        if (e.Middleware.Any(m => m.UseMethod.Name == "UseSignalR"))
                        {
                            capabilities = ImmutableHashSet.Create("_PublishSignalRService");
                        }

                        await UpdateCapabilitiesAsync(capabilities, CancellationToken.None).ConfigureAwait(false);
                    }
                }
            }));
        }
    }
}
