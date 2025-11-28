using KartLibrary.Consts;
using KartLibrary.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.IO.SmartStream;
using KartCity.Common.Xml;

namespace KartLibrary.File
{
    /// <summary>
    /// Provices 
    /// </summary>
    /* 
     * Saving Logic:
     *      If a file adds to a KartStorageFolder that binds a RhoFolder only, 
     *      this new added file will be add to a RhoFolder binded by KartStorageFolder this file add to.
     *      
     *      Similarly, If a file adds to a KartStorageFolder that binds a Rho5Folder only, 
     *      this new added file will be add to a Rho5Folder binded by KartStorageFolder this file add to.
     *      
     *      But if a file adds to a KartStorageFolder that binds a RhoFolder and a Rho5Folder (likes kart_, track_ and so on),
     *      this new added file will be add to a Rho5Folder binded by KartStorageFolder this file add to
     *      because the structure of Rho5 file doesn't have a "complex" tree structure to store files and folders.
    */
    public class KartStorageSystem : IDisposable
    {
        #region Members
        private bool _useRho;
        private bool _useRho5;
        private bool _usePackFolderListFile;
        private CountryCode? _regionCode;
        private string? _clientPath;
        private string? _dataPath;

        private KartStorageFolder _rootFolder;

        private HashSet<RhoArchive> _rhoArchives;
        private HashSet<Rho5Archive> _rho5Archives;

        private Dictionary<KartStorageFolder, RhoArchive> _rhoMapping = [];
        private Dictionary<KartStorageFolder, Rho5Archive> _rho5Mapping = [];

        private bool _initialized;
        private bool _disposed;

        private bool _initializing;

        #endregion

        #region Events
        public event ProgressChangedEventHandler InitializingProgressChanged;
        #endregion
        
        #region Properties
        public KartStorageFolder RootFolder => _rootFolder;

        public bool IsInitialized => _initialized;

        public bool IsDisposed => _disposed;

        #endregion

        #region Constructors
        /// <summary>
        /// We are recommend use <see cref="KartStorageSystemBuilder"/> instand of this constructor.
        /// </summary>
        /// <param name="useRho"></param>
        /// <param name="useRho5"></param>
        /// <param name="usePackFolderListFile"></param>
        /// <param name="regionCode"></param>
        /// <param name="clientPath"></param>
        /// <param name="dataPath"></param>
        public KartStorageSystem(bool useRho, bool useRho5, bool usePackFolderListFile, CountryCode? regionCode, string? clientPath, string? dataPath)
        {
            _useRho = useRho;
            _useRho5 = useRho5;
            _usePackFolderListFile = usePackFolderListFile;
            _regionCode = regionCode;
            _clientPath = clientPath;
            _dataPath = dataPath;
            _rhoArchives = new HashSet<RhoArchive>();
            _rho5Archives = new HashSet<Rho5Archive>();
            _rootFolder = new KartStorageFolder(true);
            _initialized = false;
            _initializing = false;
            _disposed = false;
        }
        #endregion

        #region Methods
        public void Initialize(int maxThreads = 0)
        {
            if (_disposed)
                throw new ObjectDisposedException("Cannot initialize a disposed KartStorageSystem.");
            _initializing = true;
            try
            {
                if (_useRho)
                    initializeRho(maxThreads: maxThreads);
                if (_useRho5)
                    initializeRho5(maxThreads: maxThreads);
            }
            finally
            {
                _initializing = false;
            }
            _initialized = true;
        }

        public KartStorageFolder? GetFolder(string folderPath)
        {
            if (!_initialized)
                throw new InvalidOperationException("This KartStorageSystem haven't been initialized.");
            if (_disposed)
                throw new InvalidOperationException("This KartStorageSystem has been disposed.");
            return _rootFolder.GetFolder(folderPath);
        }

        public KartStorageFile? GetFile(string filePath)
        {
            return _rootFolder.GetFile(filePath);
        }

        public void SaveAs(string path)
        {
            ApplyRootFolderChanges();

            lock (_rhoMapping)
            {
                foreach (var rhoArchive in _rhoMapping.Values)
                {
                    if (rhoArchive.FileHasModified())
                    {
                        rhoArchive.SaveTo(Path.Join(path, rhoArchive.FileName));
                    }
                }
            }

            lock (_rho5Mapping)
            {
                foreach (var rho5Archive in _rho5Mapping.Values)
                {
                    if (rho5Archive.FileHasModified())
                    {
                        rho5Archive.Save(path, SavePattern.AlwaysRegeneration);
                    }
                }
            }
        }
        

        public void Close()
        {
            if (!_initialized)
                return;
            if (_initializing)
                throw new InvalidOperationException("There are an initialize task running. Please call \"Close\" method after the initialization task has finished.");
            _initialized = false;
            foreach (RhoArchive rhoArchive in _rhoArchives)
                rhoArchive.Dispose();
            foreach (Rho5Archive rho5Archive in _rho5Archives)
                rho5Archive.Dispose();
            _rhoArchives.Clear();
            _rho5Archives.Clear();
            _rootFolder.Clear();
        }

        public void Dispose()
        {
            dispose(true);
        }

        private void initializeRho(int maxThreads = 0)
        {
            if (maxThreads == 0)
                maxThreads = Environment.ProcessorCount;
            Queue<(KartStorageFolder mountFolder, FileInfo rhoFileInfo)> rhoFilesInfoQueue = new Queue<(KartStorageFolder mountFolder, FileInfo rhoFileInfo)>();
            string dataFolder = _dataPath ?? $"{_clientPath}\\Data";
            if (!Directory.Exists(dataFolder))
                throw new Exception("Data folder does not exist.");
            
            // Enqueue all rho files.
            if (_usePackFolderListFile)
            {
                if(_dataPath is null && _clientPath is null)
                {
                    throw new Exception("You must give Dat folder path or Kartrider client path in constructor or builder.");
                }
                string folderListFilePath = Path.Combine($"{dataFolder}", "aaa.pk") ;
                if (!System.IO.File.Exists(folderListFilePath))
                {
                    folderListFilePath = Path.Combine($"{dataFolder}", "beni.pg") ;
                }
                if (!System.IO.File.Exists(folderListFilePath))
                {
                    throw new Exception($"{folderListFilePath} does not exist");
                }
                using (FileStream listFileStream = new FileStream(folderListFilePath, FileMode.Open))
                {
                    BinaryReader listFileRawReader = new BinaryReader(listFileStream);
                    int krDataLen = listFileRawReader.ReadInt32();
                    byte[] listFileData = listFileRawReader.ReadSmartStreamToBytes(krDataLen);
                    BinaryXmlDocument bmlDoc = new BinaryXmlDocument();
                    bmlDoc.Read(Encoding.Unicode, listFileData);

                    if(bmlDoc.RootTag.Name != "PackFolder")
                        throw new Exception("It is not valid aaa.pk file.");

                    Queue<(KartStorageFolder parentFolder, BinaryXmlTag packFolderTag)> 
                        packFolderQueue = 
                            new Queue<(KartStorageFolder parentFolder, BinaryXmlTag packFolderTag)>();
                    packFolderQueue.Enqueue((_rootFolder, bmlDoc.RootTag));
                    while(packFolderQueue.Count > 0)
                    {
                        var curObj = packFolderQueue.Dequeue();
                        foreach(BinaryXmlTag child in curObj.packFolderTag.Children)
                        {
                            if(child.Name == "PackFolder")
                            {
                                string? childFolderName = child.GetAttribute("name");
                                if (childFolderName is null)
                                    throw new Exception();
                                KartStorageFolder newFolder = new KartStorageFolder();
                                newFolder.Name = childFolderName;
                                newFolder.FolderStoreMode = RhoFolderStoreMode.PackFolder;
                                curObj.parentFolder.AddFolder(newFolder);
                                packFolderQueue.Enqueue((newFolder, child));
                            }
                            else if(child.Name == "RhoFolder")
                            {
                                string? childFolderName = child.GetAttribute("name");
                                string? rhoFileName = child.GetAttribute("fileName");
                                if(childFolderName is null || rhoFileName is null)
                                    throw new Exception();
                                string rhoFileFullname = Path.Combine(dataFolder, rhoFileName);
                                if(!System.IO.File.Exists(rhoFileFullname))
                                    throw new Exception($"Rho: {rhoFileFullname} doesn't exist");
                                FileInfo rhoFileInfo = new FileInfo(rhoFileFullname);
                                if (childFolderName.Length == 0)
                                    rhoFilesInfoQueue.Enqueue((curObj.parentFolder, rhoFileInfo));
                                else
                                {
                                    KartStorageFolder newFolder = new KartStorageFolder();
                                    newFolder.Name = childFolderName;
                                    newFolder.FolderStoreMode = RhoFolderStoreMode.RhoRoot;
                                    curObj.parentFolder.AddFolder(newFolder);
                                    rhoFilesInfoQueue.Enqueue((newFolder, rhoFileInfo));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DirectoryInfo dataFolderInfo = new DirectoryInfo(dataFolder);
                foreach(FileInfo fileInfo in dataFolderInfo.GetFiles())
                {
                    Regex fileNamePattern = new Regex(@"^(\S+?_){0,1}(?:(\S+?)_){0,1}(\S+?){0,1}\.rho$");
                    Match match = fileNamePattern.Match(fileInfo.Name);
                    if (match.Success && match.Groups.Count >= 1)
                    {
                        KartStorageFolder parentFolder = _rootFolder;
                        for(int i = 1; i < match.Groups.Count; i++)
                        {
                            var matchGroup = match.Groups[i];
                            if (!matchGroup.Success)
                                continue;
                            for(int j = 0; j < matchGroup.Captures.Count; j++)
                            {
                                string folderName = matchGroup.Captures[j].Value;
                                KartStorageFolder? curFolder = parentFolder.GetFolder(folderName);
                                if (curFolder is null)
                                {
                                    KartStorageFolder newFolder = new KartStorageFolder();
                                    newFolder.Name = folderName;
                                    parentFolder.AddFolder(newFolder);
                                    curFolder = newFolder;
                                }
                                parentFolder = curFolder;
                            }
                        }
                        rhoFilesInfoQueue.Enqueue((parentFolder, fileInfo));
                    }
                }
            }

            // Mount Rho files
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = maxThreads;

            Parallel.ForEach(rhoFilesInfoQueue, rhoFileInfo =>
            {
                RhoArchive rhoArchive = new RhoArchive();
                mountRho(rhoFileInfo.mountFolder, rhoArchive, rhoFileInfo.rhoFileInfo.FullName);
                lock (_rhoArchives)
                {
                    _rhoArchives.Add(rhoArchive);
                }
            });

            if (_regionCode is null)
            {
                string regionFolderName = _rootFolder.GetFolder("zeta")?.Folders.FirstOrDefault()?.Name ?? "";
                _regionCode = regionFolderName.ToLower() switch
                {
                    "kr" => CountryCode.KR,
                    "cn" => CountryCode.CN,
                    "tw" => CountryCode.TW,
                    _ => CountryCode.None,
                };
            }
            rhoFilesInfoQueue.Clear();
        }

        private void mountRho(KartStorageFolder mountFolder, RhoArchive rhoArchive, string fileName)
        {
            rhoArchive.Open(fileName);
            Queue<(KartStorageFolder mountFolder, RhoFolder)> folderQueue = new Queue<(KartStorageFolder, RhoFolder)>();
            folderQueue.Enqueue((mountFolder, rhoArchive.RootFolder));
            while (folderQueue.Count > 0)
            {
                var curObj = folderQueue.Dequeue();
                lock (curObj.mountFolder)
                {
                    curObj.mountFolder._sourceRhoFolder = curObj.Item2;
                    
                    lock(_rhoMapping)
                        _rhoMapping.Add(curObj.mountFolder, rhoArchive);
                    
                    foreach (RhoFolder subFolder in curObj.Item2.Folders)
                    {
                        KartStorageFolder newFolder = new KartStorageFolder();
                        newFolder.Name = subFolder.Name;
                        folderQueue.Enqueue((newFolder, subFolder));
                        curObj.mountFolder.AddFolder(newFolder);
                    }
                    foreach (RhoFile subFile in curObj.Item2.Files)
                    {
                        KartStorageFile newFile = new KartStorageFile(subFile);
                        curObj.mountFolder.AddFile(newFile);
                    }
                }
            }
        }

        private void initializeRho5(int maxThreads = 0)
        {
            if (maxThreads == 0)
                maxThreads = Environment.ProcessorCount;
            Queue<(KartStorageFolder mountFolder, FileInfo rhoFileInfo)> rhoFilesInfoQueue = new Queue<(KartStorageFolder mountFolder, FileInfo rhoFileInfo)>();
            string dataFolder = _dataPath ?? $"{_clientPath}\\Data";
            dataFolder = Path.GetFullPath(dataFolder);
            if (!Directory.Exists(dataFolder))
                throw new Exception("Data folder does not exist.");
            Regex dataPackPattern = new Regex(@"^(DataPack\d+)_(\d+)\.rho5$");
            DirectoryInfo dataFolderInfo = new DirectoryInfo(dataFolder);
            HashSet<string> dataPackNames = new HashSet<string>();
            foreach(FileInfo fileInfo in dataFolderInfo.GetFiles())
            {
                Match match = dataPackPattern.Match(fileInfo.Name);
                if (match.Success)
                {
                    string dataPackName = match.Groups[1].Value;
                    if (!dataPackNames.Contains(dataPackName))
                        dataPackNames.Add(dataPackName);
                }
            }

            // Mount rho5 files
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = maxThreads;
            Parallel.ForEach(dataPackNames, dataPackName =>
            {
                Rho5Archive rho5Archive = new Rho5Archive();
                mountRho5(rho5Archive, dataFolder, dataPackName);
                lock (_rho5Archives)
                {
                    _rho5Archives.Add(rho5Archive);
                }
            });
        }

        private void mountRho5(Rho5Archive archive, string dataPackPath, string dataPackName)
        {
            archive.Open(dataPackPath, dataPackName, _regionCode ?? CountryCode.None);
            Queue<(KartStorageFolder mountFolder, Rho5Folder rho5Folder)> queue = new();
            queue.Enqueue((_rootFolder, archive.RootFolder));
            while (queue.Count > 0)
            {
                var curObj = queue.Dequeue();
                lock (curObj.mountFolder)
                {
                    foreach (Rho5Folder subfolder in curObj.rho5Folder.Folders)
                    {
                        KartStorageFolder? folder = curObj.mountFolder.GetFolder(subfolder.Name);
                        if (folder is null)
                        {
                            folder = new KartStorageFolder(false, null, subfolder);
                            folder.Name = subfolder.Name;
                            curObj.mountFolder.AddFolder(folder);
                        }
                        queue.Enqueue((folder, subfolder));
                    }
                    foreach (Rho5File file in curObj.rho5Folder.Files)
                    {
                        KartStorageFile newFile = new KartStorageFile(file);
                        curObj.mountFolder.AddFile(newFile);
                    }
                    curObj.mountFolder.appliedChanges();
                }
            }
        }
        
        private void ApplyRootFolderChanges()
        {
            foreach (var folder in _rootFolder.Folders)
            {
                if (folder.FolderStoreMode is RhoFolderStoreMode.None)
                    throw new Exception("Adding folder in RootFolder is not support.");

                if (folder.FolderStoreMode is RhoFolderStoreMode.RhoRoot)
                {
                    ApplyRhoFolderChanges(folder);
                }
                else if (folder.FolderStoreMode is RhoFolderStoreMode.PackFolder)
                {
                    ApplyPackFolderChanges(folder);
                }
                else if (folder.FolderStoreMode is RhoFolderStoreMode.Rho5Root)
                {
                    ApplyRho5FolderChanges(folder);
                }
            }
            
            _rootFolder.appliedChanges();
        }
        
        private void ApplyPackFolderChanges(KartStorageFolder packFolder)
        {
            if (packFolder.FolderStoreMode is not RhoFolderStoreMode.PackFolder)
                throw new ArgumentException($"The store mode of {nameof(packFolder)} is not PackFolder.");
            
            if (packFolder.Files.Count > 0)
            {
                bool isModified = packFolder._sourceRhoFolder is null || packFolder.Files.Any(x => x.IsModified);

                if (packFolder._sourceRhoFolder is null)
                {
                    string archiveName = GetNewRhoName(packFolder) + "_";
                    
                    RhoArchive newArchive = new RhoArchive()
                    {
                        FileName = archiveName
                    };
                    
                    lock(_rhoMapping)
                        _rhoMapping[packFolder] = newArchive;
                    
                    packFolder._sourceRhoFolder = newArchive.RootFolder;
                }

                
                if (isModified)
                {
                    foreach (var childFile in packFolder.Files)
                    {
                        if (childFile._sourceFile is null)
                            childFile.ConvertToRhoFile();
                    }
                }
            }

            foreach (var folder in packFolder.Folders)
            {
                if (folder.FolderStoreMode is RhoFolderStoreMode.Rho5Root)
                    throw new Exception($"PackFolder shouldn't contain the folder with store mode being Rho5Root.");

                if (folder.FolderStoreMode is RhoFolderStoreMode.None)
                {
                    folder.FolderStoreMode = RhoFolderStoreMode.RhoRoot;
                }

                if (folder.FolderStoreMode is RhoFolderStoreMode.RhoRoot)
                {
                    if (folder._sourceRhoFolder is null)
                    {
                        string rhoName = GetNewRhoName(folder);
                        RhoArchive newRhoArchive = new RhoArchive()
                        {
                            FileName = rhoName
                        };

                        folder._sourceRhoFolder = newRhoArchive.RootFolder;
                    
                        lock (_rhoMapping)
                            _rhoMapping[folder] = newRhoArchive;
                    }
                    
                    ApplyRhoFolderChanges(folder);
                }
                else if (folder.FolderStoreMode is RhoFolderStoreMode.PackFolder)
                {
                    ApplyPackFolderChanges(folder);
                }
            }
            
            packFolder.appliedChanges();
        }

        private void ApplyRhoFolderChanges(KartStorageFolder rhoFolder)
        {
            if (rhoFolder._sourceRhoFolder is null)
                throw new Exception("Source rho folder is null.");
            
            foreach (var file in rhoFolder.Files)
            {
                if (file._sourceFile is null)
                    file.ConvertToRhoFile();
            }

            foreach (var folder in rhoFolder.Folders)
            {
                if (folder._sourceRhoFolder is null)
                {
                    RhoFolder newRhoFolder = new RhoFolder()
                    {
                        Name = folder.Name 
                    };

                    folder._sourceRhoFolder = newRhoFolder;
                    rhoFolder._sourceRhoFolder?.AddFolder(newRhoFolder);
                }
                
                ApplyRhoFolderChanges(folder);
            }
        }

        private void ApplyRho5FolderChanges(KartStorageFolder rho5Folder)
        {
            if (rho5Folder._sourceRho5Folder is null)
                throw new Exception("Source rho5 folder is null.");
            
            foreach (var file in rho5Folder.Files)
            {
                if (file._sourceFile is null)
                    file.ConvertToRho5File();
            }

            foreach (var folder in rho5Folder.Folders)
            {
                if (folder._sourceRho5Folder is null)
                {
                    Rho5Folder newRho5Folder = new Rho5Folder()
                    {
                        Name = folder.Name 
                    };

                    folder._sourceRho5Folder = newRho5Folder;
                    rho5Folder._sourceRho5Folder?.AddFolder(newRho5Folder);
                }
                
                ApplyRho5FolderChanges(folder);
            }
        }

        private string GetNewRhoName(KartStorageFolder folder)
        {
            List<string> paths = [folder.Name];
            var curFolder = folder.Parent;

            while (curFolder is not null && curFolder != _rootFolder)
            {
                paths.Add(curFolder.Name);
                curFolder = curFolder.Parent;
            }

            paths.Reverse();

            return string.Join("_", paths);
        }
        
        protected virtual void dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                if (_initializing)
                    throw new InvalidOperationException("You can't dispose KartStorageSystem when asynchronous \"Initialize\" method is running.");
                Close();
            }
            _disposed = true;
        }

        internal void TestAllRho5()
        {
            Rho5Archive rho5Archive = new Rho5Archive();
            Queue<(Rho5Folder, KartStorageFolder)> queue = new Queue<(Rho5Folder, KartStorageFolder)>();
            foreach(var folder in RootFolder.Folders)
                queue.Enqueue((rho5Archive.RootFolder, folder));
            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();
                foreach (var folder in obj.Item2.Folders)
                {
                    Rho5Folder newFolder = new Rho5Folder();
                    newFolder.Name = folder.Name;
                    obj.Item1.AddFolder(newFolder);
                    queue.Enqueue((newFolder, folder));
                }

                foreach (var file in obj.Item2.Files)
                {
                    Rho5File rho5File = new Rho5File();
                    rho5File.Name = file.Name;
                    rho5File.DataSource = file.DataSource;
                    obj.Item1.AddFile(rho5File);
                }
            }

            string outPath = Path.Combine(Environment.CurrentDirectory, "experiment");
            if(!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);
            rho5Archive.Save(outPath, "DataPack1", _regionCode ?? CountryCode.None, SavePattern.AlwaysRegeneration);
        }
        #endregion
    }

    public class KartStorageSystemBuilder
    {
        private bool _useRho;
        private bool _useRho5;
        private bool _usePackFolderListFile;
        private string? _kartriderClientPath;
        private string? _kartriderDataPath;
        private CountryCode? _regionCode;

        public KartStorageSystemBuilder() 
        {
            _useRho = false;
            _useRho5 = false;
            _usePackFolderListFile = false;
            _regionCode = null;
        }

        public KartStorageSystemBuilder UseRho()
        {
            _useRho = true;
            return this;
        }

        public KartStorageSystemBuilder UseRho5()
        {
            _useRho5 = true;
            return this;
        }

        public KartStorageSystemBuilder UsePackFolderListFile()
        {
            _usePackFolderListFile = true;
            return this;
        }

        public KartStorageSystemBuilder SetClientRegion(CountryCode countryCode)
        {
            _regionCode = countryCode;
            return this;
        }

        public KartStorageSystemBuilder SetClientPath(string kartriderClientPath)
        {
            _kartriderClientPath = kartriderClientPath;
            return this;
        }

        public KartStorageSystemBuilder SetDataPath(string kartriderDataPath)
        {
            _kartriderDataPath = kartriderDataPath;
            return this;
        }

        public KartStorageSystem Build()
        {
            return new KartStorageSystem(_useRho, _useRho5, _usePackFolderListFile, _regionCode, _kartriderClientPath, _kartriderDataPath);
        }
    }
}
