using Xenko.Core.Mathematics;

namespace Game.World
{
    public unsafe partial class Chunk
    {
        public BlockData this[int x, int y, int z]
        {
            get => Blocks[(x << BitShiftX) | (y << BitShiftY) | z];
            set
            {
                if (IsCopyOnWrite())
                    ExecuteFullCopy();
                Blocks[(x << BitShiftX) | (y << BitShiftY) | z] = value;
                IsUpdated = true;
            }
        }

        public BlockData this[Int3 pos]
        {
            get => Blocks[(pos.X << BitShiftX) | (pos.Y << BitShiftY) | pos.Z];
            set
            {
                if (IsCopyOnWrite())
                    ExecuteFullCopy();
                Blocks[(pos.X << BitShiftX) | (pos.Y << BitShiftY) | pos.Z] = value;
                IsUpdated = true;
            }
        }

        public void SerializeTo(byte[] data)
        {
            fixed (byte* buffer = data)
            {
                SerializeTo(buffer);
            }
        }

        public void SerializeTo(byte[] data, int offset)
        {
            fixed (byte* buffer = data)
            {
                SerializeTo(buffer + offset);
            }
        }

        public void SerializeTo(byte* buffer)
        {
            for (var i = 0; i < 32768 * 4; ++i)
            {
                var block = Blocks[i >> 2];
                buffer[i++] = (byte) (block.Id >> 4);
                buffer[i++] = (byte) ((block.Id << 4) | block.Brightness);
                buffer[i++] = (byte) (block.Data >> 8);
                buffer[i] = (byte) block.Data;
            }
        }

        public void DeserializeFrom(byte[] data)
        {
            fixed (byte* buffer = data)
            {
                DeserializeFrom(buffer);
            }
        }

        public void DeserializeFrom(byte[] data, int offset)
        {
            fixed (byte* buffer = data)
            {
                DeserializeFrom(buffer + offset);
            }
        }

        public void DeserializeFrom(byte* buffer)
        {
            for (var i = 0; i < 32768 * 4; i += 4)
            {
                ref var block = ref Blocks[i >> 2];
                block.Id = (ushort) ((buffer[i] << 4) | (buffer[i + 1] >> 4));
                block.Brightness = (byte) (buffer[i + 1] | 0xF);
                block.Data = (uint) ((buffer[i + 2] << 8) | buffer[i + 3]);
            }
        }
    }
}