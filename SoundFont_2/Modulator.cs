using System;
using System.IO;

namespace CMGWpf.SoundFont_2
{
    public static class Modulator
    {
        // Since modulators will not be supported, we discard the data.
        public static void DiscardData(BinaryReader reader, int size)
        {
            if (size % 10 != 0)
            {
                throw new InvalidDataException("The modulator list is invalid.");
            }

            reader.BaseStream.Position += size;
        }
    }
}
