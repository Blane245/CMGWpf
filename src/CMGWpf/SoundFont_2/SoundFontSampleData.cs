using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CMGWpf.SoundFont_2
{
    public sealed class SoundFontSampleData
    {
        private readonly int bitsPerSample;
        private readonly short[] samples;

        public SoundFontSampleData(BinaryReader reader)
        {
            var chunkId = reader.ReadFourCC();
            if (chunkId != "LIST")
            {
                throw new InvalidDataException("The LIST chunk was not found.");
            }

            var end = (long)reader.ReadInt32();
            end += reader.BaseStream.Position;

            var listType = reader.ReadFourCC();
            if (listType != "sdta")
            {
                throw new InvalidDataException($"The type of the LIST chunk must be 'sdta', but was '{listType}'.");
            }

            while (reader.BaseStream.Position < end)
            {
                var id = reader.ReadFourCC();
                var size = reader.ReadInt32();

                switch (id)
                {
                    case "smpl":
                        bitsPerSample = 16;
                        samples = new short[size / 2];
                        //TODO notify auithor of a problem with the origianl code. The original code read the sample data as bytes and then cast it to shorts, which is not correct. We need to read the sample data directly as shorts.
                        reader.Read(MemoryMarshal.AsBytes(samples.AsSpan()));
                        break;
                    case "sm24":
                        // 24 bit audio is not supported.
                        reader.BaseStream.Position += size;
                        break;
                    default:
                        throw new InvalidDataException($"The INFO list contains an unknown ID '{id}'.");
                }
            }

            if (samples == null)
            {
                throw new InvalidDataException("No valid sample data was found.");
            }

            if (Encoding.ASCII.GetString(MemoryMarshal.Cast<short, byte>(samples).Slice(0, 4)) == "OggS")
            {
                throw new NotSupportedException("SoundFont3 is not yet supported.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                // TODO: Insert the byte swapping code here.
                throw new NotSupportedException("Big endian architectures are not yet supported.");
            }
        }

        public int BitsPerSample => bitsPerSample;
        public short[] Samples => samples;
    }
}
