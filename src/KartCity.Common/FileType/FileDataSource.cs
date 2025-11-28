namespace KartCity.Common.FileType
{
    public class FileDataSource: IDataSource
    {
        private string _fileName;
        private int _size;
        private bool _disposed;
        private DataSourceStreamPool _streamPool;
        private FileStream _baseStream;

        public bool Locked => false;

        public int Size => _size;
        
        /// <summary>
        /// Initializes a <see cref="FileDataSource"/> instance.
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public FileDataSource(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                throw new FileNotFoundException("file not found", fileName);
            _fileName = fileName;
            _streamPool = new DataSourceStreamPool();
            _baseStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (_baseStream.Length > 0x7FFFFFFF)
                throw new NotSupportedException("FileDataSource doesn't support the file that its size is more than 4GiB.");
            _size = (int)_baseStream.Length;
            _disposed = false;
        }

        public Stream CreateStream()
        {
            return _streamPool.CreateStream(new FileStream(_baseStream.SafeFileHandle, FileAccess.Read));
        }

        public void WriteTo(Stream stream)
        {
            using (FileStream tmpFileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                tmpFileStream.CopyTo(stream);
            }
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using (FileStream tmpFileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                await tmpFileStream.CopyToAsync(stream, cancellationToken);
            }
        }

        public void WriteTo(byte[] buffer, int offset, int count)
        {
            using (FileStream tmpFileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                tmpFileStream.Read(buffer, offset, count);
            }
        }

        public async Task WriteToAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            using (FileStream tmpFileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                await tmpFileStream.ReadAsync(buffer, offset, count, cancellationToken);
            }
        }

        public byte[] GetBytes()
        {
            using (FileStream tmpFileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] output = new byte[_size];
                tmpFileStream.Read(output);
                return output;
            }
        }

        public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
        {
            byte[] output = new byte[_size];
            await WriteToAsync(output, 0, output.Length);
            return output;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _streamPool.Dispose();
                _baseStream?.Dispose();
            }
            _disposed = true;
        }
    }
}
