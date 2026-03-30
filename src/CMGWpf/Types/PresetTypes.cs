namespace CMGWpf.Types
{
    public class PresetTypes
    {
        public enum GenOp : ushort
        {
            startAddrOffset = 0,
            endAddrOffset = 1,
            startloopAddrsOffset = 2,
            endloopAddrsOffset = 3,
            startAddrsCoarseOffset = 4,
            modLfoToPitch = 5,
            vibLfoToPitch = 6,
            modEnvToPitch = 7,
            initialFilterFc = 8,
            initialFilterQ = 9,
            modLfoToFilterFc = 10,
            modEnvToFilterFc = 11,
            endAddrsCoarseOffset = 12,
            modLfoToVolume = 13,
            unused1 = 14,
            chorusEffectsSend = 15,
            reverbEffectsSend = 16,
            pan = 17,
            unused2 = 18,
            unused3 = 19,
            unused4 = 20,
            delayModLFO = 21,
            freqModLFO = 22,
            delayVibLFO = 23,
            freqVibLFO = 24,
            delayModEnv = 25,
            attackModEnv = 26,
            holdModEnv = 27,
            decayModEnv = 28,
            sustainModEnv = 29,
            releaseModEnv = 30,
            keyNumToModEnvHold = 31,
            keyNumToModEnvDecay = 32,
            delayVolEnv = 33,
            attackVolEnv = 34,
            holdVolEnv = 35,
            decayVolEnv = 36,
            sustainVolEnv = 37,
            releaseVolEnv = 38,
            keyNumToVolEnvHold = 39,
            keyNumToVolEnvDecay = 40,
            instrument = 41,
            reserved1 = 42,
            keyRange = 43,
            velRange = 44,
            startloopAddrsCoarseOffset = 45,
            keyNum = 46,
            velocity = 47,
            initialAttenuation = 48,
            reserved2 = 49,
            endloopAddrsCoarseOffset = 50,
            coarseTune = 51,
            fineTune = 52,
            sampleID = 53,
            sampleModes = 54,
            reserved3 = 55,
            scaleTuning = 56,
            exclusiveClass = 57,
            overridingRootKey = 58,
            unused5 = 59,
            endOper = 60,
        }

        public readonly record struct Sf2GenAmount(ushort Raw)
        {
            public short Int16 => unchecked((short)Raw);
            public ushort UInt16 => Raw;

            public byte LoByte => (byte)(Raw & 0xFF);
            public byte HiByte => (byte)((Raw >> 8) & 0xFF);

            public static Sf2GenAmount FromInt16(short v) => new(unchecked((ushort)v));
            public static Sf2GenAmount FromUInt16(ushort v) => new(v);
        }

        public readonly record struct Sf2Range(byte Lo, byte Hi)
        {
            public bool Contains(int v) => v >= Lo && v <= Hi;
            public bool IsEmpty => Lo > Hi;

            public static Sf2Range FromGenAmount(Sf2GenAmount a)
                => new(a.LoByte, a.HiByte); // little-endian: lo byte then hi byte

            public static Sf2Range Intersect(Sf2Range a, Sf2Range b)
            {
                byte lo = (byte)Math.Max(a.Lo, b.Lo);
                byte hi = (byte)Math.Min(a.Hi, b.Hi);
                return lo <= hi ? new Sf2Range(lo, hi) : new Sf2Range(1, 0);
            }
        }
    }
}
