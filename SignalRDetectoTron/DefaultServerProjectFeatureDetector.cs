using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace SignalRDetectoTron
{
    [AppliesTo(ProjectCapabilities.Cps)]
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IServerProjectFeatureDetector))]
    internal class DefaultServerProjectFeatureDetector : IServerProjectFeatureDetector
    {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly Lazy<VisualStudioWorkspace> _workspace;

        [ImportingConstructor]
        public DefaultServerProjectFeatureDetector(UnconfiguredProject unconfiguredProject, Lazy<VisualStudioWorkspace> workspace)
        {
            _unconfiguredProject = unconfiguredProject;
            _workspace = workspace;
        }

        public async Task<IImmutableSet<string>> DetectFeaturesAsync(CancellationToken cancellationToken = default)
        {
            // If the workspace is uninitialized, we need to do the first access on the UI thread.
            //
            // This is very unlikely to occur, but doing it here for completeness.
            if (!_workspace.IsValueCreated)
            {
                await _unconfiguredProject.Services.ThreadingPolicy.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                GC.KeepAlive(_workspace.Value);
                await TaskScheduler.Default;
            }

            var workspace = _workspace.Value;
            var solution = workspace.CurrentSolution;

            var project = GetProject(solution, _unconfiguredProject.FullPath);
            if (project == null)
            {
                // Cannot find matching project.
                return ImmutableHashSet<string>.Empty;
            }

            var features = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            var symbols = new StartupSymbols(compilation);
            if (!symbols.HasRequiredSymbols)
            {
                // Cannot find ASP.NET Core types.
                return ImmutableHashSet<string>.Empty;
            }

            // Find configure methods in the project's assembly
            var configureMethods = new List<IMethodSymbol>();
            new StartupSymbolVisitor(symbols, configureMethods).Visit(compilation.Assembly);
            for (var i = 0; i < configureMethods.Count; i++)
            {
                var configureMethod = configureMethods[i];

                // Handles the case where a method is using partial definitions. We don't expect this to occur, but still handle it correctly.
                var syntaxReferences = configureMethod.DeclaringSyntaxReferences;
                for (var j = 0; j < syntaxReferences.Length; j++)
                {
                    var semanticModel = compilation.GetSemanticModel(syntaxReferences[j].SyntaxTree);

                    var syntax = await syntaxReferences[j].GetSyntaxAsync().ConfigureAwait(false);
                    var operation = semanticModel.GetOperation(syntax);

                    var matches = new List<IInvocationOperation>();
                    foreach (var invocation in operation.Descendants().OfType<IInvocationOperation>())
                    {
                        if (string.Equals(invocation.TargetMethod.Name, "UseSignalR", StringComparison.Ordinal) ||
                            string.Equals(invocation.TargetMethod.Name, "MapHub", StringComparison.Ordinal) ||
                            string.Equals(invocation.TargetMethod.Name, "MapBlazorHub", StringComparison.Ordinal))
                        {
                            matches.Add(invocation);
                        }
                    }

                    if (matches.Count > 0)
                    {
                        features.Add(WellKnownFeatures.SignalR);
                    }
                }
            }

            return features.ToImmutable();
        }

        private static Project GetProject(Solution solution, string projectFilePath)
        {
            foreach (var project in solution.Projects)
            {
                if (string.Equals(projectFilePath, project.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    return project;
                }
            }

            return null;
        }
    }
}
