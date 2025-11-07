using System;
using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet.Modules
{
    public class ReliableConnectionHistory<T>
    {
        private readonly Dictionary<Connection, T> _lastRead = new ();
        private readonly Dictionary<Connection, T> _lastWritten = new ();

        public T GetLastWrittenReliableId(Connection conn)
        {
            return _lastWritten.GetValueOrDefault(conn);
        }

        public T GetLastReadReliableId(Connection conn)
        {
            return _lastRead.GetValueOrDefault(conn);
        }

        public void UpdateLastRead(Connection conn, T data)
        {
            if (_lastRead.TryGetValue(conn, out var old) && old is IDisposable disposable)
                disposable.Dispose();
            _lastRead[conn] = Packer.Copy(data);
        }

        public void UpdateLastWritten(Connection conn, T data)
        {
            if (_lastWritten.TryGetValue(conn, out var old) && old is IDisposable disposable)
                disposable.Dispose();
            _lastWritten[conn] = Packer.Copy(data);
        }

        public void Clear(Connection conn)
        {
            if (_lastRead.Remove(conn, out var old) && old is IDisposable disposable)
                disposable.Dispose();

            if (_lastWritten.Remove(conn, out var oldRead) && oldRead is IDisposable read)
                read.Dispose();
        }
    }
}
