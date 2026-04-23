using CMGWpf.SoundFont_2;
using static CMGWpf.Types.PlayTypes;
using static CMGWpf.Types.PresetTypes;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class PresetUtilities
    {
        /// <summary>
        /// Given a preset, a key (pitch), and a velocity (attack), get the SF generators for the voice (instrument) by merging the preset and instrument generators, taking into account globals, and additive versus absolute values. Only a subset of the generators are used by this application
        /// </summary>
        /// <param name="preset" type="Preset">A SF Preset</param>
        /// <param name="key" type="int">The pitch to be selected (0-127)</param>
        /// <param name="vel" type="int">The veoloity to be selected (1-127)</param>
        /// <returns></returns>
        public static List<FinalVoice> BuildVoicesForPresetAtKeyVel(
            Preset preset,
            int key,
            int vel)
        {
            var result = new List<FinalVoice>();

            foreach (var presetRegion in preset.Regions)
            {
                if (!presetRegion.Contains(key, vel))
                    continue;

                var instrument = presetRegion.Instrument;
                var pg = BuildGeneratorsFromPresetRegion(presetRegion);

                foreach (var instrumentRegion in instrument.Regions)
                {
                    if (!instrumentRegion.Contains(key, vel))
                        continue;

                    var ig = BuildGeneratorsFromInstrumentRegion(instrumentRegion);

                    var keyRange = Sf2Range.Intersect(
                        GetRangeFromGenerators(pg, GenOp.keyRange),
                        GetRangeFromGenerators(ig, GenOp.keyRange)
                    );
                    if (keyRange.IsEmpty) continue;

                    var velRange = Sf2Range.Intersect(
                        GetRangeFromGenerators(pg, GenOp.velRange),
                        GetRangeFromGenerators(ig, GenOp.velRange)
                    );
                    if (velRange.IsEmpty) continue;

                    var merged = MergeGeneratorsClamped(pg, ig);

                    result.Add(new FinalVoice() {
                        InstrumentName = instrument.Name,
                        SampleHeader = instrumentRegion.Sample,
                        Generators = merged
                    });
                }
            }

            return result;
        }

        private static Dictionary<GenOp, short> MergeGeneratorsClamped(
            Dictionary<GenOp, Sf2GenAmount> preset,
            Dictionary<GenOp, Sf2GenAmount> inst)
        {
            var final = new Dictionary<GenOp, short>();

            // copy preset first
            foreach (var (op, pv) in preset)
            {
                if (GenOpRules.IsIgnorable(op) || GenOpRules.IsSelector(op) ||
                    GenOpRules.IsRange(op) || GenOpRules.IsFixedKeyVel(op))
                    continue;

                if (!ShouldIncludeGenerator(op))
                    continue;

                final[op] = pv.Int16;
            }
            // merge instrument
            foreach (var (op, iv) in inst)
            {
                if (GenOpRules.IsIgnorable(op) || GenOpRules.IsSelector(op) ||
                    GenOpRules.IsRange(op) || GenOpRules.IsFixedKeyVel(op))
                    continue;

                if (!ShouldIncludeGenerator(op))
                    continue;

                if (final.TryGetValue(op, out var pvShort) && GenOpRules.IsAdditive(op))
                {
                    var pv = Sf2GenAmount.FromInt16(pvShort);
                    var result = Sf2GenMath.AddClampedInt16(op, pv, iv);
                    final[op] = result.Int16;
                }
                else
                {
                    final[op] = iv.Int16; // instrument overrides
                }
            }
            return final;
        }

        private static bool ShouldIncludeGenerator(GenOp op)
        {
            // Customize this list based on what you need for playback
            return op switch
            {
                // Sample address offsets
                GenOp.startAddrOffset => true,
                GenOp.endAddrOffset => true,
                GenOp.startloopAddrsOffset => true,
                GenOp.endloopAddrsOffset => true,
                GenOp.startAddrsCoarseOffset => true,
                GenOp.endAddrsCoarseOffset => true,
                GenOp.startloopAddrsCoarseOffset => true,
                GenOp.endloopAddrsCoarseOffset => true,

                // Volume
                GenOp.initialAttenuation => true,

                // Tuning
                GenOp.coarseTune => true,
                GenOp.fineTune => true,
                GenOp.scaleTuning => true,
                GenOp.overridingRootKey => true,

                // Volume Envelope
                GenOp.delayVolEnv => true,
                GenOp.attackVolEnv => true,
                GenOp.holdVolEnv => true,
                GenOp.decayVolEnv => true,
                GenOp.sustainVolEnv => true,
                GenOp.releaseVolEnv => true,

                // Sample modes
                GenOp.sampleModes => true,
                _ => false
            };
        }

        private static Dictionary<GenOp, Sf2GenAmount> BuildGeneratorsFromPresetRegion(PresetRegion region)
        {
            var result = new Dictionary<GenOp, Sf2GenAmount>();

            for (int i = 0; i < 61; i++)
            {
                var generatorType = (GeneratorType)i;
                var genOp = MapGeneratorTypeToGenOp(generatorType);

                if (genOp.HasValue)
                {
                    var value = region[generatorType];
                    result[genOp.Value] = Sf2GenAmount.FromInt16(value);
                }
            }

            return result;
        }

        private static Dictionary<GenOp, Sf2GenAmount> BuildGeneratorsFromInstrumentRegion(InstrumentRegion region)
        {
            var result = new Dictionary<GenOp, Sf2GenAmount>();

            for (int i = 0; i < 61; i++)
            {
                var generatorType = (GeneratorType)i;
                var genOp = MapGeneratorTypeToGenOp(generatorType);

                if (genOp.HasValue)
                {
                    var value = region[generatorType];
                    result[genOp.Value] = Sf2GenAmount.FromInt16(value);
                }
            }

            return result;
        }

        private static GenOp? MapGeneratorTypeToGenOp(GeneratorType generatorType)
        {
            return generatorType switch
            {
                GeneratorType.StartAddressOffset => GenOp.startAddrOffset,
                GeneratorType.EndAddressOffset => GenOp.endAddrOffset,
                GeneratorType.StartLoopAddressOffset => GenOp.startloopAddrsOffset,
                GeneratorType.EndLoopAddressOffset => GenOp.endloopAddrsOffset,
                GeneratorType.StartAddressCoarseOffset => GenOp.startAddrsCoarseOffset,
                GeneratorType.ModulationLfoToPitch => GenOp.modLfoToPitch,
                GeneratorType.VibratoLfoToPitch => GenOp.vibLfoToPitch,
                GeneratorType.ModulationEnvelopeToPitch => GenOp.modEnvToPitch,
                GeneratorType.InitialFilterCutoffFrequency => GenOp.initialFilterFc,
                GeneratorType.InitialFilterQ => GenOp.initialFilterQ,
                GeneratorType.ModulationLfoToFilterCutoffFrequency => GenOp.modLfoToFilterFc,
                GeneratorType.ModulationEnvelopeToFilterCutoffFrequency => GenOp.modEnvToFilterFc,
                GeneratorType.EndAddressCoarseOffset => GenOp.endAddrsCoarseOffset,
                GeneratorType.ModulationLfoToVolume => GenOp.modLfoToVolume,
                GeneratorType.Unused1 => GenOp.unused1,
                GeneratorType.ChorusEffectsSend => GenOp.chorusEffectsSend,
                GeneratorType.ReverbEffectsSend => GenOp.reverbEffectsSend,
                GeneratorType.Pan => GenOp.pan,
                GeneratorType.Unused2 => GenOp.unused2,
                GeneratorType.Unused3 => GenOp.unused3,
                GeneratorType.Unused4 => GenOp.unused4,
                GeneratorType.DelayModulationLfo => GenOp.delayModLFO,
                GeneratorType.FrequencyModulationLfo => GenOp.freqModLFO,
                GeneratorType.DelayVibratoLfo => GenOp.delayVibLFO,
                GeneratorType.FrequencyVibratoLfo => GenOp.freqVibLFO,
                GeneratorType.DelayModulationEnvelope => GenOp.delayModEnv,
                GeneratorType.AttackModulationEnvelope => GenOp.attackModEnv,
                GeneratorType.HoldModulationEnvelope => GenOp.holdModEnv,
                GeneratorType.DecayModulationEnvelope => GenOp.decayModEnv,
                GeneratorType.SustainModulationEnvelope => GenOp.sustainModEnv,
                GeneratorType.ReleaseModulationEnvelope => GenOp.releaseModEnv,
                GeneratorType.KeyNumberToModulationEnvelopeHold => GenOp.keyNumToModEnvHold,
                GeneratorType.KeyNumberToModulationEnvelopeDecay => GenOp.keyNumToModEnvDecay,
                GeneratorType.DelayVolumeEnvelope => GenOp.delayVolEnv,
                GeneratorType.AttackVolumeEnvelope => GenOp.attackVolEnv,
                GeneratorType.HoldVolumeEnvelope => GenOp.holdVolEnv,
                GeneratorType.DecayVolumeEnvelope => GenOp.decayVolEnv,
                GeneratorType.SustainVolumeEnvelope => GenOp.sustainVolEnv,
                GeneratorType.ReleaseVolumeEnvelope => GenOp.releaseVolEnv,
                GeneratorType.KeyNumberToVolumeEnvelopeHold => GenOp.keyNumToVolEnvHold,
                GeneratorType.KeyNumberToVolumeEnvelopeDecay => GenOp.keyNumToVolEnvDecay,
                GeneratorType.Instrument => GenOp.instrument,
                GeneratorType.Reserved1 => GenOp.reserved1,
                GeneratorType.KeyRange => GenOp.keyRange,
                GeneratorType.VelocityRange => GenOp.velRange,
                GeneratorType.StartLoopAddressCoarseOffset => GenOp.startloopAddrsCoarseOffset,
                GeneratorType.KeyNumber => GenOp.keyNum,
                GeneratorType.Velocity => GenOp.velocity,
                GeneratorType.InitialAttenuation => GenOp.initialAttenuation,
                GeneratorType.Reserved2 => GenOp.reserved2,
                GeneratorType.EndLoopAddressCoarseOffset => GenOp.endloopAddrsCoarseOffset,
                GeneratorType.CoarseTune => GenOp.coarseTune,
                GeneratorType.FineTune => GenOp.fineTune,
                GeneratorType.SampleID => GenOp.sampleID,
                GeneratorType.SampleModes => GenOp.sampleModes,
                GeneratorType.Reserved3 => GenOp.reserved3,
                GeneratorType.ScaleTuning => GenOp.scaleTuning,
                GeneratorType.ExclusiveClass => GenOp.exclusiveClass,
                GeneratorType.OverridingRootKey => GenOp.overridingRootKey,
                GeneratorType.Unused5 => GenOp.unused5,
                GeneratorType.UnusedEnd => GenOp.endOper,
                _ => null
            };
        }

        private static Sf2Range GetRangeFromGenerators(Dictionary<GenOp, Sf2GenAmount> gens, GenOp op)
        {
            if (gens.TryGetValue(op, out var value))
            {
                return Sf2Range.FromGenAmount(value);
            }
            return new Sf2Range(0, 127);
        }
    }
    public static class GenOpRules
    {
        public static bool IsSelector(GenOp op) =>
            op is GenOp.instrument or GenOp.sampleID;

        public static bool IsRange(GenOp op) =>
            op is GenOp.keyRange or GenOp.velRange;

        public static bool IsFixedKeyVel(GenOp op) =>
            op is GenOp.keyNum or GenOp.velocity;

        public static bool IsIgnorable(GenOp op) =>
            op is GenOp.unused1 or GenOp.unused2 or GenOp.unused3 or GenOp.unused4 or GenOp.unused5
            or GenOp.reserved1 or GenOp.reserved2 or GenOp.reserved3
            or GenOp.endOper;

        public static bool IsAdditive(GenOp op) => op switch
        {
            // address offsets
            GenOp.startAddrOffset => true,
            GenOp.endAddrOffset => true,
            GenOp.startloopAddrsOffset => true,
            GenOp.endloopAddrsOffset => true,
            GenOp.startAddrsCoarseOffset => true,
            GenOp.endAddrsCoarseOffset => true,
            GenOp.startloopAddrsCoarseOffset => true,
            GenOp.endloopAddrsCoarseOffset => true,

            // pitch/filter/volume modulation amounts
            GenOp.modLfoToPitch => true,
            GenOp.vibLfoToPitch => true,
            GenOp.modEnvToPitch => true,
            GenOp.initialFilterFc => true,
            GenOp.initialFilterQ => true,
            GenOp.modLfoToFilterFc => true,
            GenOp.modEnvToFilterFc => true,
            GenOp.modLfoToVolume => true,

            // fx / pan / attenuation
            GenOp.chorusEffectsSend => true,
            GenOp.reverbEffectsSend => true,
            GenOp.pan => true,
            GenOp.initialAttenuation => true,

            // LFO / Env
            GenOp.delayModLFO => true,
            GenOp.freqModLFO => true,
            GenOp.delayVibLFO => true,
            GenOp.freqVibLFO => true,

            GenOp.delayModEnv => true,
            GenOp.attackModEnv => true,
            GenOp.holdModEnv => true,
            GenOp.decayModEnv => false,   // Absolute (non-additive) per SF2 spec
            GenOp.sustainModEnv => true,
            GenOp.releaseModEnv => false, // Absolute (non-additive) per SF2 spec
            GenOp.keyNumToModEnvHold => true,
            GenOp.keyNumToModEnvDecay => true,

            GenOp.delayVolEnv => true,
            GenOp.attackVolEnv => true,
            GenOp.holdVolEnv => true,
            GenOp.decayVolEnv => false,   // Absolute (non-additive) per SF2 spec
            GenOp.sustainVolEnv => false, // Absolute (non-additive) per SF2 spec - sustain LEVEL, not time
            GenOp.releaseVolEnv => false, // Absolute (non-additive) per SF2 spec
            GenOp.keyNumToVolEnvHold => true,
            GenOp.keyNumToVolEnvDecay => true,

            // tuning
            GenOp.coarseTune => true,
            GenOp.fineTune => true,

            _ => false
        };

        // instrument wins if present
        public static bool IsOverride(GenOp op) =>
            op is GenOp.sampleModes or GenOp.scaleTuning or GenOp.exclusiveClass or GenOp.overridingRootKey;
    }
    public static class Sf2GenMath
    {
        public static Sf2GenAmount AddClampedInt16(GenOp op, Sf2GenAmount a, Sf2GenAmount b)
        {
            int sum = a.Int16 + b.Int16;

            // clamp to representable storage range
            sum = Math.Clamp(sum, short.MinValue, short.MaxValue);

            // semantic clamps (you can extend this list as needed)
            sum = op switch
            {
                GenOp.initialAttenuation => Math.Clamp(sum, 0, 1440),
                GenOp.sustainVolEnv => Math.Clamp(sum, 0, 1000),
                GenOp.sustainModEnv => Math.Clamp(sum, 0, 1000),
                _ => sum
            };

            return Sf2GenAmount.FromInt16((short)sum);
        }
    }

    //TODO need to routine to convert SF generator values to standard units for processing (eg centibels to gain, volenv to seconds, etc) for the DSP engine. This routine would be used to aide in development of the intensity envelope and playback rate. Each GenOp that is in the final voice will be converted to a standard unit and then the DSP engine can use those standard units to determine how to apply the various effects and envelopes to the sample. This routine would also apply the appropriate tuning adjustments to determine the final pitch of the sample for playback.
    // 
    public static class Sf2Defaults
    {
        public static short GetDefault(GenOp op)
        {
            if (TryGetDefault(op, out var def))
                return def.Int16;
            return 0;
        }

        public static bool TryGetDefault(GenOp op, out Sf2GenAmount def)
        {
            def = default;

            switch (op)
            {
                // Envelope/LFO time cents defaults (~0 seconds)
                case GenOp.delayModLFO:
                case GenOp.delayVibLFO:
                case GenOp.delayModEnv:
                case GenOp.attackModEnv:
                case GenOp.holdModEnv:
                case GenOp.decayModEnv:
                case GenOp.releaseModEnv:
                case GenOp.delayVolEnv:
                case GenOp.attackVolEnv:
                case GenOp.holdVolEnv:
                case GenOp.decayVolEnv:
                case GenOp.releaseVolEnv:
                    def = Sf2GenAmount.FromInt16(-12000);
                    return true;

                // Sustain defaults (0 = full)
                case GenOp.sustainVolEnv:
                case GenOp.sustainModEnv:
                    def = Sf2GenAmount.FromInt16(0);
                    return true;

                // Common zero defaults
                case GenOp.pan:
                case GenOp.initialAttenuation:
                case GenOp.coarseTune:
                case GenOp.fineTune:
                case GenOp.modLfoToPitch:
                case GenOp.vibLfoToPitch:
                case GenOp.modEnvToPitch:
                case GenOp.initialFilterQ:
                case GenOp.modLfoToFilterFc:
                case GenOp.modEnvToFilterFc:
                case GenOp.modLfoToVolume:
                case GenOp.chorusEffectsSend:
                case GenOp.reverbEffectsSend:
                case GenOp.exclusiveClass:
                    def = Sf2GenAmount.FromInt16(0);
                    return true;

                // Standard default
                case GenOp.scaleTuning:
                    def = Sf2GenAmount.FromInt16(100);
                    return true;

                default:
                    return false;
            }
        }
    }
    public static class Sf2Units
    {
        public static double TimecentsToSeconds(int timecents)
            => Math.Pow(2.0, timecents / 1200.0);

        public static double AttenuationCbToGain(int attenuationCb)
        {
            attenuationCb = Math.Clamp(attenuationCb, 0, 1440);
            return VolumeDbToGain((double)attenuationCb / 10.0);
        }
        public static double VolumeDbToGain(double dB)
            => Math.Pow(10.0, dB / 20.0);
    }
    public static class GenGetWithDefaults
    {
        public static Sf2GenAmount GetOrDefault(this IReadOnlyDictionary<GenOp, Sf2GenAmount> gens, GenOp op)
        {
            if (gens.TryGetValue(op, out var v))
                return v;

            if (Sf2Defaults.TryGetDefault(op, out var def))
                return def;

            // If SF2 doesn’t define a default or you haven’t encoded it yet:
            return default;
        }

        public static short GetInt16OrDefault(this IReadOnlyDictionary<GenOp, Sf2GenAmount> gens, GenOp op)
            => gens.GetOrDefault(op).Int16;
    }
}

