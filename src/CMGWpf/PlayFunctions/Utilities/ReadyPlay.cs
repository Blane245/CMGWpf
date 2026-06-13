using CMGWpf.Model;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class ReadyPlay
    {
        /// <summary>
        /// Develop the list of generators to be played based in filtering rules. The filtering rules are as follows: If a generator is passed in, it will return that generator. If there is a time interval, return all generators whose start and stop times are within the interval. If there is no time interval, return all generators on tracks that are soloed, unless the track is also muted, in which case skip all generators on that track. If there are no soloed tracks, return all generators on non-muted tracks. In all cases, skip any generators that are muted. Also validate the selected generators and return any error messages.
        /// </summary>
        /// <param name="generator">The generator to check.</param>
        /// <returns>A ReadyPlayOutput object containing the results of the check.</returns>
        public static ReadyPlayOutput Check(Model.Generators.Generator? generator)
        {
            CMGFile file = FileViewModel.Instance.File;
            TimeLine timeLine = file.TimeLine;
            if (file == null || timeLine == null)
            {
                return new ReadyPlayOutput
                {
                    Generators = [],
                    Duration = 0,
                    ErrorMessages = [new() { Text = "No file or timeline loaded.", Error = true }]
                };
            }
            // filter the selected generators. First, if the generator parameter is not null, the return that generator. Second, if there is a Time Interval include only those generators that are within its bounds. Third, in all other cases, process track solo and mute and generator mute settings
            else if (generator != null)
            {
                var errors = generator.Validate();
                return new ReadyPlayOutput
                {
                    Generators = [generator],
                    Duration = generator.GetEndTime(),
                    ErrorMessages = errors
                };
            }
            // if there is a time interval, select all generators whose start and stop times are within the interval
            else if (timeLine.TimeInterval.StartTime != timeLine.TimeInterval.EndTime)
            {
                TimeInterval interval = timeLine.TimeInterval;
                ObservableCollection<Model.Generators.Generator> generators = [];
                var errors = new ObservableCollection<Message>();
                foreach (var track in file.Tracks)
                {
                    foreach (var gen in track.Generators)
                    {
                        foreach (var error in gen.Validate()) { errors.Add(error); }
                        if (errors.Count == 0 && gen.StartTime >= interval.StartTime && gen.StopTime <= interval.EndTime) // time interval selection is based on stop time not end time for stochastic generators
                            generators.Add(gen);
                    }
                }
                return new ReadyPlayOutput()
                {
                    Generators = generators,
                    Duration = generators.Count > 0 ? generators.Max(g => g.GetEndTime()) : 0,
                    ErrorMessages = errors
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
                // if no soloed tracks, include all non-muted tracks
                List<Track> selectedTracks = [];
                if (soloedTracks.Count != 0) selectedTracks = soloedTracks;
                else
                {
                    foreach (var t in file.Tracks)
                    {
                        if (!t.Mute) selectedTracks.Add(t);
                    }
                }

                // pick up all non-muted generators on the selected tracks
                var errors = new ObservableCollection<Message>();
                foreach (var track in selectedTracks)
                {
                    foreach (var gen in track.Generators)
                    {
                        if (!gen.Mute)
                        {
                            foreach (var error in gen.Validate()) { errors.Add(error); }
                            if (errors.Count == 0) generators.Add(gen);
                        }
                    }
                }
                if (generators.Count == 0) errors.Add(new Message { Text = "No valid generators selected for playback.", Error = true });
                ReadyPlayOutput output = new()
                {
                    Generators = generators,
                    Duration = generators.Count > 0 ? generators.Max(g => g.GetEndTime()) : 0,
                    ErrorMessages = errors
                };
                return output;
            }
        }

    }
}
