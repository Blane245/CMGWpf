using CMGWpf.Model;
using CMGWpf.View;
using static CMGWpf.Types.PlayTypes;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;

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
            // filter the selected generators. First, if the generator parameter is not null, the return that generator. Second, if there is a Time Interval include only those genertors that are within its bounds. Third, in all other cases, process track solo and mute and generator mute settings
            else if (generator != null)
            {
                return new ReadyPlayOutput
                {
                    Generators = [generator],
                    Duration = generator.StopTime - generator.StartTime,
                    ErrorMessage = ""
                };
            }
            // if there is a time interval, select all generators whoes start and stop times are within the interval
            else if (timeLine.TimeInterval.StartTime != timeLine.TimeInterval.EndTime)
            {
                TimeInterval interval = timeLine.TimeInterval;
                ObservableCollection<Model.Generators.Generator> generators = [];
                foreach (var track in file.Tracks)
                {
                    foreach (var gen in track.Generators)
                    {
                        if (gen.StartTime >= interval.StartTime && gen.StopTime <= interval.EndTime)
                            generators.Add(gen);
                    }
                }
                return new ReadyPlayOutput()
                {
                    Generators = generators,
                    Duration = generators.Count > 0 ? generators.Max(g => g.StopTime) : 0,
                    ErrorMessage = generators.Count > 0 ? "" : "No generators to play."
                };
            }
            else // filter for track solo, mute, and generator solo. Track solo takes precedence. A muted track that is also soloed, will be skipped. Generators on selected tracks will be included unless muted
            {
                // first, find out if there are any soloed tracks
                List<Track> soloedTracks = [];
                ObservableCollection<Model.Generators.Generator> generators = [];
                foreach (var track in file.Tracks)
                {
                    if (track.Solo && !track.Mute) soloedTracks.Add(track);
                }
                // if no soloed tracks, inlcude all tracks
                List<Track> selectedTracks = soloedTracks.Count == 0? file.Tracks : soloedTracks;

                // pick up all non-muted generators on the selected tracks
                foreach(var track in selectedTracks)
                {
                    foreach(var gen in track.Generators)
                    {
                        if (!gen.Mute) generators.Add(gen); 
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
}
