using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SignalRDetectoTron
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SignalRDetectoTronPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(DetectoTronWindow))]
    public sealed class SignalRDetectoTronPackage : AsyncPackage
    {
        public const string PackageGuidString = "0b8dc88e-9c73-4877-a325-a02db5e672d0";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
        
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await DetectoTronCommand.InitializeAsync(this);
        }
    }
}
