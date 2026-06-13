
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Types;
using CMGWpf.Utilities;
using static CMGWpf.Model.Generators.StochasticTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public static class Intensity
    {
        /// <summary>
        /// Applies an intensity transition to a sample based on the provided option and parameters. The function first builds continuous probability arrays to determine the duration of each intensity transition. It then randomly selects an intensity transition from a predefined list and applies it to the sample. If the option is set to persistent, it ensures that the next transition starts where the previous one ended. The gain for each sample is calculated using linear interpolation based on the current duration and the selected intensity transition's profile.
        /// </summary>
        /// <param name="sample">The audio sample array to which the intensity transition will be applied.</param>
        /// <param name="option">The intensity transition option, which can be random or persistent.</param>
        /// <param name="parameters">The average cycle time that the intensity will change.</param>
        /// <param name="Rn">A random number generator used for selecting transitions and durations.</param>
        public static void Apply(double[] sample, INTENSITYTRANSITIONOPTION option, IntensityParameters parameters, FastRandom Rn)
        {
            double cycleTime = parameters.CycleTime;
            double deltaT = 1.0D / PlayTypes.SampleRate;
            double sampleLength = sample.Length / 2;
            double sampleDuration = sample.Length / PlayTypes.SampleRate;
            int nPoints = (int)Math.Round(sampleDuration / cycleTime);
            if (nPoints == 0) return; // not enough sound duration to make one cycle
            // build the continuous probability arrays
            (var Nd, var Pd) = Probability.Continuous(nPoints, sampleDuration, sampleDuration / StochasticConstants.UNIT);
            if (Pd.Length <= 1) return; // duration or nPoints is probability too small

            // pick a random intensity
            int nTransitions = IntensityTransitions.Length;
            int index = Rn.Next(0, nTransitions - 1);
            IntensityTransition transition = IntensityTransitions[index];
            DebugLog.Write($"Intensity Apply: transition picked {transition}");
            (var start, var middle, var end) = GetGainFromIntensityProfile(transition);

            // get the end intensity for persistence
            INTENSITY endIntensity = transition.End;

            // determine the duration for a probability lookup in the probability table. 
            double duration = 0;
            while (duration == 0) duration = Probability.Lookup(Pd, Nd, Rn.NextDouble());
            double currentDuration = 0;
            // apply the intensity profile to all of the samples
            for (int i = 0; i < sampleLength; i++)
            {
                // when current duration > duration, pick a random transition that has the ending transition as the starting and reset current duration
                if (currentDuration >= duration)
                {
                    currentDuration = 0;
                    // for persistence, find a transition that starts where the previous one ends
                    if (option == INTENSITYTRANSITIONOPTION.persistent)
                    {
                        int tIndex = Rn.Next(0, nTransitions - 1);
                        while (endIntensity != IntensityTransitions[tIndex].Start) { tIndex = Math.Min(Rn.Next(0, nTransitions), nTransitions - 1); }
                        transition = IntensityTransitions[tIndex];
                    }
                    else // for random just pick any one that can be found
                    {
                        transition = IntensityTransitions[Math.Min(Rn.Next(0, nTransitions), nTransitions - 1)];

                    }
                    DebugLog.Write($"Intensity Apply: apply intensity transition for option {option} -  ({transition.Start}, {transition.Middle}, {transition.End} at sample {i}");

                    (start, middle, end) = GetGainFromIntensityProfile(transition);
                    endIntensity = transition.End;
                    DebugLog.Write($"Intensity Apply: picked new end transition {endIntensity}");
                    duration = 0;
                    while (duration == 0) duration = Probability.Lookup(Pd, Nd, Rn.NextDouble());
                }

                // process the intensity transition based on the current duration, duration, and intensity transition
                double gain = currentDuration < duration / 2 ?
                    Interpolation.Linear(currentDuration, 0, duration / 2, start, middle) :
                    Interpolation.Linear(currentDuration, duration / 2, duration, middle, end);

                // update the sample with the new gain
                sample[2 * i] *= gain;
                sample[2 * i + 1] *= gain;
                currentDuration += deltaT;
            }
        }

        private static (double, double, double) GetGainFromIntensityProfile(IntensityTransition transition)
        {
            double start = IntensityProfiles[transition.Start].DB;
            double end = IntensityProfiles[transition.End].DB;
            double middle = transition.Middle != null ? IntensityProfiles[transition.Middle.Value].DB : (start + end) / 2;
            return (Sf2Units.VolumeDbToGain(start), Sf2Units.VolumeDbToGain(middle), Sf2Units.VolumeDbToGain(end));
        }
    }
}
