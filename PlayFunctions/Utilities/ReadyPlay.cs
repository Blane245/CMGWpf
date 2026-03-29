using CMGWpf.Model;
using CMGWpf.View;
using static CMGWpf.Types.PlayTypes;
using System.Collections.ObjectModel;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class ReadyPlay
    {
                // first is to select the generators to be played. In the prototype all generators are selected and the duration of the composition si set the to graeatest stoptim of all of them. Only error is that there aren't any generators.
        public static ReadyPlayOutput Check(Model.Generators.Generator? generator)
        {
            //TODO Generator and TimeInterval selection are ignored in the prototype
            CMGFile file = FileViewModel.Instance.File;
            TimeLine timeLine = file.TimeLine;
            if (file == null || timeLine == null)
            {
                return new ReadyPlayOutput
                {
                    Generators = [],
                    Duration = 0,
                    ErrorMessage = "No file or timeline loaded."
                };
            }
            ObservableCollection<Model.Generators.Generator> generators = [];
            foreach (var track in file.Tracks)
            {
                foreach (var gen in track.Generators)
                {
                    generators.Add(gen);
                }
            }
            ReadyPlayOutput output = new()
            {
                Generators = generators,
                Duration = generators.Count > 0 ? generators.Max(g => g.StopTime) : 0,
                ErrorMessage = generators.Count > 0 ? "" : "No generators to play."
            };
            return output;
        }

    }
}
