// 
// NEWorld/Core: ConnectionHost.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2019 NEWorld Team
// 
// NEWorld is free software: you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// NEWorld is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General 
// Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with NEWorld.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Network
{
    public sealed class Session : IDisposable
    {
        private readonly TcpClient conn;
        private readonly NetworkStream ios;
        private readonly MemoryStream writeBuffer = new MemoryStream(new byte[8192], 0, 8192, true, true);
        private readonly ReaderWriterLockSlim writeLock = new ReaderWriterLockSlim();
        private MemoryStream buffer;
        private byte[] storage = new byte[8192];

        internal Session(TcpClient io)
        {
            conn = io;
            ios = io.GetStream();
            buffer = new MemoryStream(storage, 0, storage.Length, false, true);
        }

        public bool Live => conn.Connected;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Session()
        {
            Dispose(false);
        }

        internal Receive WaitMessage()
        {
            return new Receive(this);
        }

        public Send CreateMessage(uint protocol)
        {
            return new Send(this, protocol);
        }

        private void ReleaseUnmanagedResources()
        {
            ios.Close();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                conn?.Dispose();
                ios?.Dispose();
                writeBuffer?.Dispose();
                buffer?.Dispose();
            }
        }

        public sealed class Receive : IDisposable
        {
            private Stream ios;

            internal Receive(Session s)
            {
                Session = s;
                ios = Session.ios;
                Raw = Session.storage;
            }

            public byte[] Raw { get; private set; }

            public Session Session { get; }

            public void Dispose()
            {
            }

            internal async Task LoadExpected(int length)
            {
                if (length == 0) return;

                if (length > Raw.Length)
                {
                    Session.storage = Raw =
                        new byte[1 << (int) System.Math.Ceiling(System.Math.Log(length) / System.Math.Log(2))];
                    Session.buffer = new MemoryStream(Raw, 0, Raw.Length, false, true);
                }
                else
                {
                    Session.buffer.Seek(0, SeekOrigin.Begin);
                }

                await ReadAsync(Raw, 0, length);
                ios = Session.buffer;
            }

            internal async Task<int> Wait()
            {
                await ReadAsync(Raw, 0, 8);
                if (!VerifyPackageValidity(Raw))
                    throw new Exception("Bad Package Received");
                return GetProtocolId(Raw);
            }

            private static int GetProtocolId(byte[] head)
            {
                return (head[4] << 24) | (head[5] << 16) | (head[6] << 8) | head[7];
            }

            private static bool VerifyPackageValidity(byte[] head)
            {
                return head[0] == 'N' && head[1] == 'W' && head[2] == 'R' && head[3] == 'C';
            }

            public byte ReadU8()
            {
                var ret = ios.ReadByte();
                if (ret >= 0)
                    return (byte) ret;
                throw new EndOfStreamException();
            }

            public char ReadChar()
            {
                return (char) ReadU16();
            }

            public ushort ReadU16()
            {
                return (ushort) ((ReadU8() << 8) | ReadU8());
            }

            public uint ReadU32()
            {
                return (uint) ((ReadU16() << 16) | ReadU16());
            }

            public ulong ReadU64()
            {
                return (ReadU32() << 32) | ReadU32();
            }

            public void Read(byte[] buffer, int begin, int end)
            {
                while (begin != end)
                {
                    var read = ios.Read(buffer, begin, end - begin);
                    if (read > 0)
                        begin += read;
                    else
                        throw new EndOfStreamException();
                }
            }

            public async Task ReadAsync(byte[] buffer, int begin, int end)
            {
                while (begin != end) begin += await ios.ReadAsync(buffer, begin, end - begin);
            }
        }

        public sealed class Send : IDisposable
        {
            private readonly MemoryStream buffer;
            private readonly NetworkStream ios;

            internal Send(Session session, uint protocol)
            {
                Session = session;
                ios = Session.ios;
                buffer = Session.writeBuffer;
                session.writeLock.EnterWriteLock();
                Write((byte) 'N');
                Write((byte) 'W');
                Write((byte) 'R');
                Write((byte) 'C');
                Write(protocol);
            }

            public Session Session { get; }

            public void Dispose()
            {
                ReleaseUnmanagedResources();
                GC.SuppressFinalize(this);
            }


            public void Write(byte val)
            {
                buffer.WriteByte(val);
            }

            public void Write(char val)
            {
                Write((byte) (val >> 8));
                Write((byte) (val & 0xFF));
            }

            public void Write(ushort val)
            {
                Write((byte) (val >> 8));
                Write((byte) (val & 0xFF));
            }

            public void Write(uint val)
            {
                Write((ushort) (val >> 16));
                Write((ushort) (val & 0xFFFF));
            }

            public void Write(ulong val)
            {
                Write((uint) (val >> 32));
                Write((uint) (val & 0xFFFFFFFF));
            }

            public void Write(byte[] input, int begin, int end)
            {
                FlushBuffer();
                ios.Write(input, begin, end - begin);
            }

            public async Task ReadAsync(byte[] input, int begin, int end)
            {
                FlushBuffer();
                await ios.WriteAsync(input, begin, end - begin);
            }

            private void FlushBuffer()
            {
                if (buffer.Length > 0)
                {
                    ios.Write(buffer.GetBuffer(), 0, (int) buffer.Seek(0, SeekOrigin.Current));
                    buffer.Seek(0, SeekOrigin.Begin);
                }
            }

            public void Write(ArraySegment<byte> bytes)
            {
                FlushBuffer();
                Debug.Assert(bytes.Array != null, "bytes.Array != null");
                ios.Write(bytes.Array, bytes.Offset, bytes.Count);
            }

            private void ReleaseUnmanagedResources()
            {
                FlushBuffer();
                Session.writeLock.ExitWriteLock();
            }

            ~Send()
            {
                ReleaseUnmanagedResources();
            }
        }
    }

    [DeclareService("Core.Network.ConnectionHost")]
    public sealed class ConnectionHost : IDisposable
    {
        private const double UtilizationThreshold = 0.25;
        private static int _connectionCounter;
        private static List<Connection> _connections;

        static ConnectionHost()
        {
            _connections = new List<Connection>();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ConnectionHost()
        {
            ReleaseUnmanagedResources();
        }

        private static void SweepInvalidConnectionsIfNecessary()
        {
            var utilization = (double) _connectionCounter / _connections.Count;
            if (utilization < UtilizationThreshold)
                SweepInvalidConnections();
        }

        private static void SweepInvalidConnections()
        {
            var swap = new List<Connection>();
            foreach (var hd in _connections)
                if (hd.Valid)
                    swap.Add(hd);
            _connections = swap;
        }

        public static Connection Add(TcpClient conn, List<Protocol> protocols)
        {
            var connect = new Connection(conn, protocols);
            _connections.Add(connect);
            return connect;
        }

        public static int CountConnections()
        {
            return _connectionCounter;
        }

        private void ReleaseUnmanagedResources()
        {
            foreach (var hd in _connections)
                hd.Close();
        }

        public sealed class Connection
        {
            private readonly Task finalize;
            private readonly List<Protocol> protocols;
            internal readonly Session Session;

            public Connection(TcpClient client, List<Protocol> protocols)
            {
                this.protocols = protocols;
                Session = new Session(client);
                finalize = Start();
            }

            public bool Valid { get; private set; }

            public void Close()
            {
                CloseDown();
                finalize.Wait();
            }

            private async Task Start()
            {
                Valid = true;
                Interlocked.Increment(ref _connectionCounter);
                while (Valid && Session.Live)
                    try
                    {
                        var message = Session.WaitMessage();
                        await ProcessRequest(await message.Wait(), message);
                    }
                    catch (Exception e)
                    {
                        if (Session.Live) LogPort.Debug($"Encountering Exception {e}");
                    }

                CloseDown();
            }

            private async Task ProcessRequest(int protocol, Session.Receive message)
            {
                var handle = protocols[protocol];
                await message.LoadExpected(handle.Expecting);
                handle.HandleRequest(message);
            }

            private void CloseDown()
            {
                if (!Valid) return;
                Valid = false;
                Interlocked.Decrement(ref _connectionCounter);
                SweepInvalidConnectionsIfNecessary();
            }
        }
    }
}