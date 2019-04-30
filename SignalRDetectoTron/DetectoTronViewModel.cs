using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SignalRDetectoTron
{
    public class DetectoTronViewModel
    {
        public ObservableCollection<DetectoTronItemViewModel> Items { get; set; }

        public ICommand RefreshCommand { get; set; }
    }

    public class DetectoTronItemViewModel
    {
        public string ProjectName { get; set; }

        public TimeSpan Elapsed { get; set; }

        public IImmutableSet<string> Features { get; set; }

        public string FeaturesCombined => Features == null ? "" : string.Join(", ", Features);
    }
}
