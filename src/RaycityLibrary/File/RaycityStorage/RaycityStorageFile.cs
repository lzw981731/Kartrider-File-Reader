using Microsoft.VisualBasic;
using SharpGen.Runtime.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using KartCity.Common.FileType;

namespace RayCityLibrary.File
{
    public class RaycityStorageFile : IRhoFile, IModifiableRhoFile
    {
        #region Members
        internal RaycityStorageFolder? _parentFolder;
        internal IModifiableRhoFile? _sourceFile;

        private string _name;
        private string _nameWithoutExt;
        private string _fullname;
        private IDataSource? _dataSource;

        private bool _disposed;
        #endregion

        #region Properties
        public RaycityStorageFolder? Parent => _parentFolder;

        IRhoFolder? IRhoFile.Parent => Parent;

        IModifiableRhoFolder? IModifiableRhoFile.Parent => Parent;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (_sourceFile is not null)
                    _sourceFile.Name = value;
                Regex fileNamePattern = new Regex(@"^(.*)\..*");
                Match match = fileNamePattern.Match(_name);
                if (match.Success)
                {
                    _nameWithoutExt = match.Groups[1].Value;
                }
                else
                {
                    _nameWithoutExt = _name;
                }
            }
        }

        public string FullName
        {
            get => Parent is not null ? $"{Parent.FullName}/{_name}" : _name;
        }

        public string NameWithoutExt => _nameWithoutExt;

        public int Size => _sourceFile is not null ? _sourceFile.Size : _dataSource?.Size ?? 0;

        public bool HasDataSource => _sourceFile is not null ? _sourceFile.HasDataSource : _dataSource is not null;

        public bool IsModified => _sourceFile is null or JmdFile { IsModified: true };
        
        public IDataSource? DataSource
        {
            internal get => _dataSource;
            set
            {
                if (_sourceFile is not null)
                    _sourceFile.DataSource = value;
                _dataSource = value;
            }
        }
        #endregion

        #region Constructors
        public RaycityStorageFile()
        {
            _parentFolder = null;
            _name = "";
            _fullname = "";
            _nameWithoutExt = "";
            _dataSource = null;
            _sourceFile = null;
            _disposed = false;
        }

        internal RaycityStorageFile(IModifiableRhoFile sourceFile) : this()
        {
            _sourceFile = sourceFile;
            Name = sourceFile.Name;
        }
        #endregion

        #region Methods
        public Stream CreateStream()
        {
            if (_sourceFile is not null)
                return _sourceFile.CreateStream();
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                    return _dataSource.CreateStream();
            }
        }

        public void WriteTo(Stream stream)
        {
            if (_sourceFile is not null)
                _sourceFile.WriteTo(stream);
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                   _dataSource.WriteTo(stream);
            }
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (_sourceFile is not null)
                await _sourceFile.WriteToAsync(stream, cancellationToken);
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                    await _dataSource.WriteToAsync(stream, cancellationToken);
            }
        }

        public void WriteTo(byte[] array, int offset, int count)
        {
            if (_sourceFile is not null)
                _sourceFile.WriteTo(array, offset, count);
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                    _dataSource.WriteTo(array, offset, count);
            }
        }

        public async Task WriteToAsync(byte[] array, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (_sourceFile is not null)
                await _sourceFile.WriteToAsync(array, offset, count, cancellationToken);
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                    await _dataSource.WriteToAsync(array, offset, count, cancellationToken);
            }
        }

        public byte[] GetBytes()
        {
            if (_sourceFile is not null)
                return _sourceFile.GetBytes();
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                    return _dataSource.GetBytes();
            }
        }

        public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
        {
            if (_sourceFile is not null)
                return await _sourceFile.GetBytesAsync(cancellationToken);
            else
            {
                if (_dataSource is null)
                    throw new InvalidOperationException("There are no any data source.");
                else
                    return await _dataSource.GetBytesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Attach this <see cref="RaycityStorageFile"/> object to RhoFolder which is source folder of this file's parent.
        /// </summary>
        public void AttachToRhoFolder(JmdFileProperty? jmdFileProperty = null)
        {
            if(_parentFolder?._sourceFolder is null)
                throw new InvalidOperationException("The parent folder is not set or not attach to RhoFolder.");
            
            JmdFile newRhoFile = new JmdFile();
            newRhoFile.DataSource = _dataSource;
            newRhoFile.Name = _name;
            newRhoFile.FileEncryptionProperty = jmdFileProperty ?? JmdFileProperty.Encrypted;
            _parentFolder._sourceFolder.AddFile(newRhoFile);
            _sourceFile = newRhoFile;
            _dataSource = null;
        }
        
        public void Dispose()
        {
            _parentFolder = null;
            if (_sourceFile is null)
                _dataSource?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }
        }

        public override string ToString()
        {
            return $"KartStorageFile:{FullName}";
        }
        
        #endregion
    }
}
