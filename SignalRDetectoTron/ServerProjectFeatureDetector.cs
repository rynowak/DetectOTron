using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRDetectoTron
{
    public interface IServerProjectFeatureDetector
    {
        Task<IImmutableSet<string>> DetectFeaturesAsync(CancellationToken cancellationToken = default);
    }

    public static class WellKnownFeatures
    {
        public static readonly string SignalR = "SignalR";
    }
}
