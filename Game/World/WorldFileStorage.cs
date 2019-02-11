using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using MessagePack.Internal;
using Xenko.Core.Mathematics;

namespace Game.World {
    public partial class World
    {
        private FileStream _indexFileStream;
        private FileStream _dataFileStream;
        private Dictionary<Int3, Int64> _index;
        private Mutex _indexLock;
        private Mutex _dataLock;

        private void EnsureFileStorageInitialized()
        {
            if (_indexFileStream == null)
            { 
                _indexFileStream = new FileStream(Name + ".index",
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                if (_indexFileStream.Length != 0)
                    _index = new BinaryFormatter().Deserialize(_indexFileStream) as Dictionary<Int3, Int64>;
                else
                    _index = new Dictionary<Int3, Int64>();
            }
            if (_dataFileStream == null)
                _dataFileStream = new FileStream(Name + ".data",
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        private Int64 FindChunkOffsetInFile(Int3 position)
        {
            if(_index.TryGetValue(position, out var ret))
            {
                return ret;
            }

            lock (_indexLock)
            {
                var offset = _index.Count;
                _index[position] = offset;
                new BinaryFormatter().Serialize(_indexFileStream, _index);

                return offset;
            }
        }

        public void SaveChunkToDisk(Chunk chunk)
        {
            EnsureFileStorageInitialized();

            var buffer = new byte[32768 * 4];
            chunk.SerializeTo(buffer);
            lock (_dataLock) {
                _dataFileStream.Seek(FindChunkOffsetInFile(chunk.Position), SeekOrigin.Begin);
                _dataFileStream.Write(buffer, 0, buffer.Length);
            }
        }

        public bool ChunkExistsInDisk(Chunk chunk)
        {
            lock (_indexLock) {
                return _index.ContainsKey(chunk.Position);
            }
        }

        public bool LoadChunkFromDisk(ref Chunk chunk)
        {
            EnsureFileStorageInitialized();

            var buffer = new byte[32768 * 4];
            lock (_dataLock)
            {
                _dataFileStream.Seek(FindChunkOffsetInFile(chunk.Position), SeekOrigin.Begin);
                _dataFileStream.Read(buffer, 0, buffer.Length);
            }
            chunk.DeserializeFrom(buffer);
            return true;
        }
    }
}
