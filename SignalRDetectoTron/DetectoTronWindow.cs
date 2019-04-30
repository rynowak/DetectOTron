using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;

namespace SignalRDetectoTron
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("a3ee0b8c-8194-4cc8-8ffe-c75744db78ff")]
    public class DetectoTronWindow : ToolWindowPane
    {
        private IProjectService _projectService;
        private ObservableCollection<DetectoTronItemViewModel> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectoTronWindow"/> class.
        /// </summary>
        public DetectoTronWindow() : base(null)
        {
            this.Caption = "ToolWindow1";

            this.Content = new DetectoTronControl();
        }

        protected override void Initialize()
        {
            var componentModel = GetService(typeof(SComponentModel)) as IComponentModel;
            var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
            _projectService = projectServiceAccessor.GetProjectService();

            _items = new ObservableCollection<DetectoTronItemViewModel>();

            ((DetectoTronControl)this.Content).DataContext = new DetectoTronViewModel()
            {
                Items = _items,
                RefreshCommand = new RelayCommand<object>(Refresh),
            };

            base.Initialize();
        }

        private async void Refresh(object value)
        {
            _items.Clear();

            foreach (var project in _projectService.LoadedUnconfiguredProjects)
            {
                var stopwatch = Stopwatch.StartNew();

                var detector = project.Services.ExportProvider.GetExportedValueOrDefault<IServerProjectFeatureDetector>(ExportContractNames.Scopes.UnconfiguredProject);
                if (detector == null)
                {
                    _items.Add(new DetectoTronItemViewModel()
                    {
                        ProjectName = Path.GetFileNameWithoutExtension(project.FullPath),
                        Elapsed = stopwatch.Elapsed,
                        Features = ImmutableHashSet<string>.Empty,
                    });

                    continue;
                }

                var features = await detector.DetectFeaturesAsync().ConfigureAwait(true);
                _items.Add(new DetectoTronItemViewModel()
                {
                    ProjectName = Path.GetFileNameWithoutExtension(project.FullPath),
                    Elapsed = stopwatch.Elapsed,
                    Features = features,
                });
            }
        }
    }
}
