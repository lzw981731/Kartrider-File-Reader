using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.FileType;

namespace KartLibrary.File
{
    public class Rho5DataSource : IDataSource
    {
        #region Members
        private bool _disposed;
        private Rho5FileHandler _fileHandler;
        private DataSourceStreamPool _streamPool;
        #endregion

        #region Properties
        public bool Locked => false; // BufferedDataSource don't require lock.

        public int Size => _fileHandler._decompressedSize;
        #endregion

        #region Constructors
        internal Rho5DataSource(Rho5FileHandler fileHandler)
        {
            _disposed = false;
            _streamPool = new DataSourceStreamPool();
            _fileHandler = fileHandler;
        }
        #endregion

        #region Methods
        public Stream CreateStream()
        {
            return _streamPool.CreateStream(new MemoryStream(_fileHandler.getData(), false));
        }

        public void WriteTo(Stream stream)
        {
            if (!stream.CanWrite)
                throw new Exception("This stream is not writeable");
            stream.Write(_fileHandler.getData());
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (!stream.CanWrite)
                throw new Exception("This stream is not writeable");
            await stream.WriteAsync(_fileHandler.getData(), cancellationToken);
        }

        public void WriteTo(byte[] buffer, int offset, int count)
        {
            if (buffer.Length - offset < count)
                throw new IndexOutOfRangeException("given buffer is not enough to store the required data.");
            if (count > _fileHandler._decompressedSize)
                throw new IndexOutOfRangeException("size is greater than file.");
            byte[] data = _fileHandler.getData();
            Array.Copy(data, 0, buffer, offset, count);
        }

        public async Task WriteToAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (buffer.Length - offset < count)
                throw new IndexOutOfRangeException("given buffer is not enough to store the required data.");
            if (count > _fileHandler._decompressedSize)
                throw new IndexOutOfRangeException("size is greater than file.");
            byte[] data = _fileHandler.getData();
            await Task.Run(() => Array.Copy(data, 0, buffer, offset, count), cancellationToken);
        }


        public byte[] GetBytes()
        {
            byte[] data = _fileHandler.getData();
            return data;
        }

        public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
        {
            return _fileHandler.getData();
        }

        public void Dispose()
        {
            _streamPool.Dispose();
            _disposed = true;
        }
        #endregion
    }
}
