using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using KartCity.Common.FileType;
using KartCity.Common.IO;
using RayCityLibrary.Encrypt;
using RayCityLibrary.File;

namespace RayCityLibrary.File
{
    /// <summary lang="en-us">
    /// <see cref="JmdFile"/> represents a Jmd type archive. You can open and save Rho file with this class.
    /// </summary>
    /// <summary lang="zh-tw">
    /// <see cref="JmdFile"/>用來表示一個Jmd檔案。你能藉此類型來開啟及儲存類型檔案.
    /// </summary>
    public partial class JmdArchive : IRhoArchive<JmdFolder, JmdFile>
    {
        #region Members
        private int _layerVersion;
        private FileStream? _jmdStream;

        private Dictionary<uint, JmdDataInfo> _dataInfoMap;
        private JmdFolder _rootFolder;

        private Dictionary<uint, JmdFileHandler> _fileHandlers;

        private uint _jmdKey;

        private uint _dataHash;

        private bool _disposed;
        private bool _closed;
        private bool _locked; // if this instance is locked, calling any methods of this instance is not allowed.
        #endregion

        #region Properties
        /// <summary>
        /// Root folder of current <see cref="JmdArchive"/>
        /// </summary>
        public JmdFolder RootFolder => _rootFolder;
        public bool IsClosed => _closed;

        /// <summary>
        /// If this instance is locked, 
        /// calling any method of this instance is not allowed until the asynchronous method that sets lock is done.
        /// </summary>
        public bool IsLocked => _locked;
        #endregion

        public uint DataHash => _dataHash;

        
        #region Constructors
        /// <summary>
        /// Constructs a new instance of <see cref="JmdArchive"/>. 
        /// </summary>
        public JmdArchive()
        {
            _rootFolder = new JmdFolder();
            _fileHandlers = new Dictionary<uint, JmdFileHandler>();
            _dataInfoMap = new Dictionary<uint, JmdDataInfo>();
            _closed = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Opens Rho file.
        /// </summary>
        /// <param name="filePath">The file path of Rho file.</param>
        /// <exception cref="FileNotFoundException"> It will be thrown if required file can't be found. </exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public void Open(string filePath)
        {
            if(!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"");
            if (!_closed)
                throw new Exception("This RhoArchive instance has opened the other jmd file. You should close the opened file to do this operation.");
            _jmdStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (_jmdStream.Length < 0x80)
                throw new InvalidOperationException();

            _jmdKey = JmdKey.GetJmdKey(Path.GetFileNameWithoutExtension(filePath));
            
            BinaryReader reader = new BinaryReader(_jmdStream);

            // Checks identifier
            _jmdStream.Seek(0x0, SeekOrigin.Begin);
            byte[] identifierData = reader.ReadBytes(0x40);
            string converftedStr = Encoding.Unicode.GetString(identifierData, 0, _jmdIdentifiers[0].Length << 1);
            int layerVersion = -1;
            for (int i = 0; i < _jmdIdentifiers.Length; i++)
                if (converftedStr == _jmdIdentifiers[i])
                {
                    layerVersion = i;
                    break;
                }
            if (layerVersion != 0)
                throw new Exception();
            
            _layerVersion = layerVersion;


            // Read jmd archive info
            _jmdStream.Seek(0x80, SeekOrigin.Begin);
            byte[] jmdArchiveInfoData = reader.ReadBytes(0x80);
            jmdArchiveInfoData = JmdEncrypt.DecryptData(_jmdKey, jmdArchiveInfoData);

            int dataInfoCount = 0;
            byte[] dataInfoKey = new byte[0];

            // Decode jmd archive info
            using (MemoryStream memStream = new MemoryStream(jmdArchiveInfoData))
            {
                BinaryReader memReader = new BinaryReader(memStream);   
                uint infoDataChksum = memReader.ReadUInt32();
                uint verifyChkSum = Adler.Adler32(0, jmdArchiveInfoData, 4, 0x7C); 
                if (infoDataChksum != verifyChkSum)
                    throw new Exception("jmd file modified.");
                int versionChkCode = memReader.ReadInt32();
                dataInfoCount = memReader.ReadInt32();
                uint dataInfoWhiteningKey = memReader.ReadUInt32();
                dataInfoKey = memReader.ReadBytes(0x20);
                uint endMagicCode = memReader.ReadUInt32();
                int u4 = memReader.ReadInt32();
                Debug.Print($"u4: {u4}");
                if (endMagicCode != 0xd24e8143u)
                    throw new Exception("invalid archiveInfo end magic code.");
                
            }
            

            // Read data information collection.
            _dataInfoMap.EnsureCapacity(dataInfoCount);
            _fileHandlers.EnsureCapacity(dataInfoCount);
            for (int i = 0; i < dataInfoCount; i++)
            {
                JmdDataInfo dataInfo = reader.ReadBlockInfo(dataInfoKey);
                _dataInfoMap.Add(dataInfo.Index, dataInfo);
            }

            // Read all folders and all files info.
            uint folderKey = JmdKey.GetDirectoryDataKey(_jmdKey);

            Queue<(uint folderDataIndex, JmdFolder folder)> procssQueue = new Queue<(uint folderDataIndex, JmdFolder folder)>();
            procssQueue.Enqueue((0xFFFFFFFF, _rootFolder));

            while(procssQueue.Count > 0)
            {
                var queObj = procssQueue.Dequeue();
                byte[] folderData = getData(queObj.folderDataIndex, folderKey);
                using(MemoryStream memStream = new MemoryStream(folderData))
                {
                    BinaryReader memReader = new BinaryReader(memStream);
                    int folderCount = memReader.ReadInt32();
                    for(int i = 0; i < folderCount; i++)
                    {
                        string name = memReader.ReadNullTerminatedText(true);
                        uint folderDataIndex = memReader.ReadUInt32();
                        
                        JmdFolder subFolder = 
                                name.Length == 0 
                                    ? queObj.folder
                                    : new JmdFolder()
                                    {
                                        Name = name
                                    };
                        
                        procssQueue.Enqueue((folderDataIndex, subFolder));
                        if(subFolder != queObj.folder)
                            queObj.folder.AddFolder(subFolder);
                    }
                    int fileCount = memReader.ReadInt32();
                    for(int i = 0; i < fileCount; i++)
                    {
                        JmdFile subFile = new JmdFile();
                        string fileName = memReader.ReadNullTerminatedText(true);
                        uint extInt = memReader.ReadUInt32();
                        int fileProperty = memReader.ReadInt32();
                        uint dataIndex = memReader.ReadUInt32();
                        int fileSize = memReader.ReadInt32();
                        uint fileKey = JmdKey.GetFileKey(_jmdKey, fileName, extInt);
                        string fileExtension = Encoding.ASCII.GetString(BitConverter.GetBytes(extInt)).TrimEnd('\0');

                        JmdFileHandler fileHandler = new JmdFileHandler(this, (JmdFileProperty)fileProperty, dataIndex, fileSize, fileKey);
                        JmdDataSource bufferedDataSource = new JmdDataSource(fileHandler);
                        subFile.DataSource = bufferedDataSource;
                        subFile.Name = $"{fileName}.{fileExtension}";
                        subFile.FileEncryptionProperty = (JmdFileProperty)fileProperty;

                        _fileHandlers.Add(dataIndex, fileHandler);
                        queObj.folder.AddFile(subFile);
                    }
                }
            }
            
            _closed = false;
        }

        public void Save()
        {
            if (_closed)
                throw new InvalidOperationException("Save operation only available if this RhoArchive instance is open from Rho file.");
            
        }
        /// <summary>
        /// Save current <see cref="JmdArchive"/> instance to Rho file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="Exception"></exception>
        public void SaveTo(string filePath, string? realName = null)
        {
            const uint dataInfoWhiteningKey = 0x3a9213ac;
            string fullName = Path.GetFullPath(filePath);
            string fullDirName = Path.GetDirectoryName(fullName) ?? "";
            if(!Directory.Exists(fullDirName))
            {
                throw new Exception("directory not exists.");
            }
            if(_jmdStream is not null)
            {
                string curRhoFullName = Path.GetFullPath(_jmdStream.Name);
                if (curRhoFullName == fullDirName)
                    System.IO.File.Copy(curRhoFullName, $"{curRhoFullName}.bak");
            }
            string outFileName = realName ?? Path.GetFileNameWithoutExtension(fullName);
            uint outJmdKey = JmdKey.GetJmdKey(outFileName);

            Queue<DataSavingInfo> dataSavingQueue = new Queue<DataSavingInfo>();
            HashSet<uint> usedIndex = new HashSet<uint>();
            int dataEndOffset = 0;
            storeFolderAndFiles(RootFolder, dataSavingQueue, usedIndex, ref dataEndOffset, outJmdKey);
            if (_jmdStream is not null)
            {
                _jmdStream.Close();
                releaseAllHandlers();
            }
            uint outDataHash = 0;
            foreach (DataSavingInfo dataSavingInfo in dataSavingQueue)
                outDataHash = Adler.Adler32Combine(outDataHash, dataSavingInfo.Data, 0, dataSavingInfo.Data.Length);
            if(_dataInfoMap is not null)
                _dataInfoMap.Clear();
            else
                _dataInfoMap = new Dictionary<uint, JmdDataInfo>(dataSavingQueue.Count);
            if(_fileHandlers is null)
                _fileHandlers = new Dictionary<uint, JmdFileHandler>(dataSavingQueue.Count);
            // Begin write to out file.
            FileStream outFileStream = new FileStream(fullName, FileMode.Create);
            int dataInfoSize = (((dataSavingQueue.Count) * 0x20) + 0xFF) & (0x7FFFFF00);
            int dataBeginOffset = 0x100 + dataInfoSize;
            dataEndOffset += dataBeginOffset;

            // Write Identifier Text
            BinaryWriter outWriter = new BinaryWriter(outFileStream);
            outWriter.Write(Encoding.Unicode.GetBytes(_jmdIdentifiers[_layerVersion]));
            outFileStream.Seek(0x40, SeekOrigin.Begin);
            outWriter.Write(Encoding.Unicode.GetBytes(_jmdSecondText));
            
            // Write Header
            outFileStream.Seek(0x80, SeekOrigin.Begin);
            byte[] jmdHeaderData = new byte[0x80]; //Without header checksum
            byte[] dataInfoKey = new byte[0x20];
            using(MemoryStream memStream = new MemoryStream(0x7C))
            {
                BinaryWriter memWriter = new BinaryWriter(memStream);
                memWriter.Write(_layerVersion | 0x100);
                memWriter.Write(dataSavingQueue.Count);
                memWriter.Write(dataInfoWhiteningKey);
                memWriter.Write(dataInfoKey);
                memStream.Seek(0, SeekOrigin.Begin);
                memStream.Read(jmdHeaderData, 4, (int)memStream.Length);
            }
            uint jmdHeaderChksum = Adler.Adler32(0, jmdHeaderData, 4, 0x7C);
            Array.Copy(BitConverter.GetBytes(jmdHeaderChksum), 0, jmdHeaderData, 0, 0x04);
            JmdEncrypt.EncryptData(outJmdKey, jmdHeaderData, 0, jmdHeaderData.Length);
            outWriter.Write(jmdHeaderData);

            // Write Data Info
            outFileStream.Seek(0x100, SeekOrigin.Begin);
            foreach (DataSavingInfo dataSavingInfo in dataSavingQueue)
            {
                byte[] dataInfoEncData = new byte[0x20];
                using(MemoryStream memStream = new MemoryStream(0x20))
                {
                    BinaryWriter memWriter = new BinaryWriter(memStream);
                    memWriter.Write(dataSavingInfo.DataInfo.Index);
                    memWriter.Write((int)((dataSavingInfo.DataInfo.Offset + dataBeginOffset) >> 8));
                    memWriter.Write(dataSavingInfo.DataInfo.DataSize);
                    memWriter.Write(dataSavingInfo.DataInfo.UncompressedSize);
                    memWriter.Write((int)dataSavingInfo.DataInfo.BlockProperty);
                    memWriter.Write(dataSavingInfo.DataInfo.Checksum);
                    
                    memStream.Seek(0, SeekOrigin.Begin);
                    memStream.Read(dataInfoEncData, 0, dataInfoEncData.Length);
                }
                JmdDataInfo jmdDataInfo = new JmdDataInfo();
                jmdDataInfo.Index = dataSavingInfo.DataInfo.Index;
                jmdDataInfo.Offset = dataSavingInfo.DataInfo.Offset + dataBeginOffset;
                jmdDataInfo.DataSize = dataSavingInfo.DataInfo.DataSize;
                jmdDataInfo.UncompressedSize = dataSavingInfo.DataInfo.UncompressedSize;
                jmdDataInfo.BlockProperty = dataSavingInfo.DataInfo.BlockProperty;
                jmdDataInfo.Checksum = dataSavingInfo.DataInfo.Checksum;
                _dataInfoMap.Add(jmdDataInfo.Index, jmdDataInfo);

                dataInfoEncData = JmdEncrypt.EncryptDataInfo(dataInfoEncData, dataInfoKey);
                outWriter.Write(dataInfoEncData);
            }

            // Write Data
            while(dataSavingQueue.Count > 0)
            {
                DataSavingInfo dataSavingInfo = dataSavingQueue.Dequeue();
                outFileStream.Seek(dataSavingInfo.DataInfo.Offset + dataBeginOffset, SeekOrigin.Begin);
                outFileStream.Write(dataSavingInfo.Data, 0, dataSavingInfo.Data.Length);
                if(dataSavingInfo.File is not null)
                {
                    JmdFile file = dataSavingInfo.File;
                    JmdFileHandler fileHandler = new JmdFileHandler(this, file.FileEncryptionProperty, dataSavingInfo.DataInfo.Index, file.Size, JmdKey.GetFileKey(outJmdKey, file.NameWithoutExt, file.getExtNum()));
                    if(file.DataSource is not null)
                        file.DataSource.Dispose();
                    _fileHandlers.Add(dataSavingInfo.DataInfo.Index, fileHandler);
                    file.DataSource = new JmdDataSource(fileHandler);
                }
            }
            if(outFileStream.Position != dataEndOffset)
            {
                outFileStream.Seek(dataEndOffset - 1, SeekOrigin.Begin);
                outFileStream.WriteByte(0x00);
            }
            outFileStream.Close();
            _jmdStream = new FileStream(fullName, FileMode.Open);
            _dataHash = outDataHash;
            _jmdKey = outJmdKey;

            // send applied changes event to RhoFolder instances
            Queue<JmdFolder> folderQueue = new Queue<JmdFolder>();
            folderQueue.Enqueue(RootFolder);
            while(folderQueue.Count > 0)
            {
                JmdFolder folder = folderQueue.Dequeue();
                folder.appliedChanges();
                foreach(JmdFolder subFolder in folder.Folders)
                    folderQueue.Enqueue(subFolder);
            }
        }
        
        public void Close()
        {
            if (_closed)
                throw new Exception("This archive is close or is not open from a file.");
            if(_jmdStream is not null && _jmdStream.CanRead)
                _jmdStream.Close();
            _jmdStream?.Dispose();
            _fileHandlers.Clear();
            _rootFolder.Clear();
        }

        public void Dispose()
        {
            if (!_closed)
                Close();
            releaseAllHandlers();
        }

        internal Stream? getRhoStream()
        {
            return _jmdStream;
        }

        internal byte[] getData(JmdFileHandler handler)
        {
            if (!_dataInfoMap.ContainsKey(handler._fileDataIndex))
                throw new Exception("handler corrupted.");
            return getData(handler._fileDataIndex, handler._key);
        }

        private byte[] getData(uint dataIndex, uint key)
        {
            if (!_dataInfoMap.ContainsKey(dataIndex))
                throw new Exception("index not exist.");
            FileStream clonedRhoStream = new FileStream(_jmdStream.SafeFileHandle, FileAccess.Read);
            JmdDataInfo dataInfo = _dataInfoMap[dataIndex];

            clonedRhoStream.Seek(dataInfo.Offset, SeekOrigin.Begin);
            byte[] outData = new byte[dataInfo.DataSize];
            clonedRhoStream.Read(outData, 0, dataInfo.DataSize);
            
            if ((dataInfo.BlockProperty & JmdBlockProperty.Compressed) != JmdBlockProperty.None)
            {
                using(MemoryStream memStream = new MemoryStream(outData))
                {
                    outData = new byte[dataInfo.UncompressedSize];
                    Ionic.Zlib.ZlibStream decompressStream = new Ionic.Zlib.ZlibStream(memStream, Ionic.Zlib.CompressionMode.Decompress);
                    int readed = decompressStream.Read(outData, 0, outData.Length);
                }
            }
            if((dataInfo.BlockProperty & JmdBlockProperty.PartialEncrypted) != JmdBlockProperty.None)
            {
                JmdEncrypt.DecryptData(key, outData, 0, outData.Length);
            }
            if(dataInfo.BlockProperty == JmdBlockProperty.PartialEncrypted)
            {
                JmdDataInfo? secDatainfo = _dataInfoMap.ContainsKey(dataIndex + 1) ? _dataInfoMap[dataIndex + 1] : null;
                if(secDatainfo is not null)
                {
                    Array.Resize(ref outData, outData.Length + secDatainfo.DataSize);
                    clonedRhoStream.Read(outData, dataInfo.DataSize, secDatainfo.DataSize);
                }
            }
            return outData;
        }

        private void storeFolderAndFiles(JmdFolder folder, Queue<DataSavingInfo> savingInfo, HashSet<uint> usedIndex, ref int dataOffset, uint outJmdKey)
        {
            if (folder.Name == "" && folder.Parent is not null)
                throw new Exception("folder name couldn't be empty.");
            uint folderDataIndex = folder.getFolderDataIndex();
            while(usedIndex.Contains(folderDataIndex))
                folderDataIndex += 0x5F03E367;
            byte[] folderData;
            
            Queue<DataSavingInfo> fileSavingInfoQueue = new Queue<DataSavingInfo>();

            // Encode folder
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryWriter memWriter = new BinaryWriter(memStream);
                IReadOnlyCollection<JmdFolder> subFolders = folder.Folders;
                IReadOnlyCollection<JmdFile> subFiles = folder.Files;
                memWriter.Write(subFolders.Count);
                foreach(JmdFolder subFolder in subFolders)
                {
                    uint subFolderDataIndex = subFolder.getFolderDataIndex();
                    memWriter.WriteNullTerminatedText(subFolder.Name, true);
                    memWriter.Write(subFolderDataIndex);
                }
                memWriter.Write(subFiles.Count);
                foreach (JmdFile subFile in subFiles)
                {
                    if (subFile.DataSource is null)
                        throw new Exception("data source is null.");
                    
                    uint extNum = subFile.getExtNum();
                    uint fileKey = JmdKey.GetFileKey(outJmdKey, subFile.NameWithoutExt, extNum);
                    int fileSize = subFile.Size;
                    uint fileDataIndex = subFile.getDataIndex(folderDataIndex);
                    byte[] fileData = subFile.DataSource.GetBytes();
                    uint fileChksum = 0;

                    while (((usedIndex.Contains(fileDataIndex) || usedIndex.Contains(fileDataIndex + 1))))
                        fileDataIndex += 0x4D21CB4F;
                    
                    if (subFile.FileEncryptionProperty == JmdFileProperty.Encrypted || subFile.FileEncryptionProperty == JmdFileProperty.CompressedEncrypted)
                    {
                        fileChksum = Adler.Adler32(0, fileData, 0, fileData.Length);
                        JmdEncrypt.EncryptData(fileKey, fileData, 0, fileData.Length);
                    }
                    else if (subFile.FileEncryptionProperty == JmdFileProperty.PartialEncrypted)
                    {
                        JmdEncrypt.EncryptData(fileKey, fileData, 0, Math.Min(0x100, fileData.Length));
                    }
                    if (subFile.FileEncryptionProperty == JmdFileProperty.CompressedEncrypted || subFile.FileEncryptionProperty == JmdFileProperty.Compressed)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Ionic.Zlib.ZlibStream compressStream = new Ionic.Zlib.ZlibStream(ms, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestCompression ,true);
                            compressStream.Write(fileData, 0, fileData.Length);
                            compressStream.Flush();
                            compressStream.Close();
                            fileData = ms.ToArray();
                        }
                    }

                    memWriter.WriteNullTerminatedText(subFile.NameWithoutExt, true); 
                    memWriter.Write(extNum);
                    memWriter.Write((int)subFile.FileEncryptionProperty);
                    memWriter.Write(fileDataIndex);
                    memWriter.Write(fileSize);

                    DataSavingInfo fileSavingInfo = new DataSavingInfo();
                    fileSavingInfo.File = subFile;
                    if(subFile.FileEncryptionProperty == JmdFileProperty.PartialEncrypted)
                    {
                        fileSavingInfo.Data = new byte[Math.Min(0x100, fileData.Length)];
                        fileSavingInfo.DataInfo.Index = fileDataIndex;
                        fileSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.PartialEncrypted;
                        fileSavingInfo.DataInfo.DataSize = fileSavingInfo.Data.Length;
                        fileSavingInfo.DataInfo.UncompressedSize = fileSavingInfo.Data.Length;
                        fileSavingInfo.DataInfo.Checksum = 0;
                        Array.Copy(fileData, 0, fileSavingInfo.Data, 0, fileSavingInfo.Data.Length);
                        usedIndex.Add(fileDataIndex);
                        fileSavingInfoQueue.Enqueue(fileSavingInfo);
                        if (fileData.Length > 0x100)
                        {
                            DataSavingInfo secFileSavingInfo = new DataSavingInfo();
                            secFileSavingInfo.Data = new byte[fileData.Length - 0x100];
                            secFileSavingInfo.DataInfo.Index = fileDataIndex + 1;
                            secFileSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.None;
                            secFileSavingInfo.DataInfo.DataSize = secFileSavingInfo.Data.Length;
                            secFileSavingInfo.DataInfo.UncompressedSize = secFileSavingInfo.Data.Length;
                            secFileSavingInfo.DataInfo.Checksum = 0;
                            Array.Copy(fileData, 0x100, secFileSavingInfo.Data, 0, secFileSavingInfo.Data.Length);
                            usedIndex.Add(fileDataIndex + 1);
                            fileSavingInfoQueue.Enqueue(secFileSavingInfo);
                        }
                    }
                    else
                    {
                        fileSavingInfo.Data = fileData;
                        fileSavingInfo.DataInfo.Index = fileDataIndex;
                        fileSavingInfo.DataInfo.Checksum = fileChksum;
                        fileSavingInfo.DataInfo.DataSize = fileData.Length;
                        fileSavingInfo.DataInfo.UncompressedSize = fileSize;
                        switch (subFile.FileEncryptionProperty)
                        {
                            case JmdFileProperty.None:
                                fileSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.None;
                                break;
                            case JmdFileProperty.Encrypted:
                                fileSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.FullEncrypted;
                                break;
                            case JmdFileProperty.Compressed:
                                fileSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.Compressed;
                                break;
                            case JmdFileProperty.CompressedEncrypted:
                                fileSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.CompressedEncrypted;
                                break;
                        }
                        usedIndex.Add(fileDataIndex);
                        fileSavingInfoQueue.Enqueue(fileSavingInfo);
                    }

                }
                folderData = memStream.ToArray();
            }

            uint folderDataDecChksum = Adler.Adler32(0, folderData, 0, folderData.Length);
            uint folderKey = JmdKey.GetDirectoryDataKey(outJmdKey);
            JmdEncrypt.EncryptData(folderKey, folderData, 0, folderData.Length);

            DataSavingInfo folderSavingInfo = new DataSavingInfo();
            folderSavingInfo.Data = folderData;
            folderSavingInfo.DataInfo.Offset = dataOffset;
            folderSavingInfo.DataInfo.Index = folderDataIndex;
            folderSavingInfo.DataInfo.Checksum = folderDataDecChksum;
            folderSavingInfo.DataInfo.DataSize = folderData.Length;
            folderSavingInfo.DataInfo.UncompressedSize = folderData.Length;
            folderSavingInfo.DataInfo.BlockProperty = JmdBlockProperty.FullEncrypted;
            usedIndex.Add(folderDataIndex);
            savingInfo.Enqueue(folderSavingInfo);
            dataOffset = (dataOffset + folderSavingInfo.DataInfo.DataSize + 0xFF) & 0x7FFFFF00;
            foreach (JmdFolder subFolder in folder.Folders)
                storeFolderAndFiles(subFolder, savingInfo, usedIndex, ref dataOffset, outJmdKey);
            while(fileSavingInfoQueue.Count > 0)
            {
                DataSavingInfo fileSavingInfo = fileSavingInfoQueue.Dequeue();
                fileSavingInfo.DataInfo.Offset = dataOffset;
                savingInfo.Enqueue(fileSavingInfo);
                dataOffset = (dataOffset + fileSavingInfo.DataInfo.DataSize + 0xFF) & 0x7FFFFF00;
            }
        }
        
        private void releaseAllHandlers()
        {
            foreach(JmdFileHandler handler in _fileHandlers.Values)
                handler.releaseHandler();
            _fileHandlers.Clear();
        }
        #endregion

        #region Structs
        private class DataSavingInfo
        {
            public JmdDataInfo DataInfo = new JmdDataInfo();
            public JmdFile? File; 
            public IDataSource DataSource;
            public byte[] Data;
        }
        #endregion
    }

    // Static
    public partial class JmdArchive
    {
        #region Constants
        internal readonly string[] _jmdIdentifiers = new string[]
        {
            "J2m Data Format 1.0",
        };
        internal const string _jmdSecondText = "j2m & raycity fighting!!";
        #endregion
    }
}
