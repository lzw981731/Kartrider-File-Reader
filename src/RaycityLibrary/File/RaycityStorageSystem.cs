using System.Diagnostics;

namespace RayCityLibrary.File;

public class RaycityStorageSystem: IDisposable
{
    public readonly string[] DefaultMountDirectlyPaths =
    [
        "ai",
        "effect",
        "mov",
        "sound/bgm",
        "track"
    ];

    private string dataPath;
    private HashSet<JmdArchive> openedArchives = new HashSet<JmdArchive>();
    
    public RaycityStorageFolder RootFolder { get; private set; }
    

    public RaycityStorageSystem(string dataPath)
    {
        this.dataPath = dataPath;
    }

    public void Initialize()
    {
        RootFolder = new RaycityStorageFolder(isRootFolder: true);
        DirectoryInfo dataFolderInfo = new DirectoryInfo(dataPath);
        Queue<(DirectoryInfo, RaycityStorageFolder)> processingQueue =
            new Queue<(DirectoryInfo, RaycityStorageFolder)>();
        processingQueue.Enqueue((dataFolderInfo, RootFolder));
        while (processingQueue.Count > 0)
        {
            var curObj = processingQueue.Dequeue();
            var curDirectory = curObj.Item1;
            var mountToFolder = curObj.Item2;
            foreach (DirectoryInfo childFolder in curDirectory.GetDirectories())
            {
                RaycityStorageFolder newFolder = new RaycityStorageFolder(isRootFolder: false);
                newFolder.Name = childFolder.Name;
                mountToFolder.AddFolder(newFolder);
                processingQueue.Enqueue((childFolder, newFolder));
            }

            foreach (FileInfo fileInfo in curDirectory.GetFiles("*.jmd"))
            {
                try
                {
                    JmdArchive newJmdArchive = new JmdArchive();
                    newJmdArchive.Open(fileInfo.FullName);
                    openedArchives.Add(newJmdArchive);
                    bool mountDirectly = false;
                    foreach (string path in DefaultMountDirectlyPaths)
                        if ((mountToFolder.FullName + "/").StartsWith($"{path}/"))
                        {
                            mountDirectly = true;
                            break;
                        }

                    RaycityStorageFolder jmdMountFolder;
                    if (mountDirectly)
                        jmdMountFolder = mountToFolder;
                    else
                    {
                        jmdMountFolder = new RaycityStorageFolder(isRootFolder: false, newJmdArchive.RootFolder);
                        jmdMountFolder.Name = fileInfo.Name[..^4];
                        mountToFolder.AddFolder(jmdMountFolder);
                    }

                    Queue<(RaycityStorageFolder, JmdFolder)> jmdConvertQueue =
                        new Queue<(RaycityStorageFolder, JmdFolder)>();
                    jmdConvertQueue.Enqueue((jmdMountFolder, newJmdArchive.RootFolder));
                    while (jmdConvertQueue.Count > 0)
                    {
                        var curJmdConvertObj = jmdConvertQueue.Dequeue();
                        var curJmdMountTo = curJmdConvertObj.Item1;
                        var curJmdMountFrom = curJmdConvertObj.Item2;
                        foreach (JmdFolder childFolder in curJmdMountFrom.Folders)
                        {
                            RaycityStorageFolder newFolder = new RaycityStorageFolder(isRootFolder: false, childFolder);
                            newFolder.Name = childFolder.Name;
                            jmdConvertQueue.Enqueue((newFolder, childFolder));
                            curJmdMountTo.AddFolder(newFolder);
                        }

                        foreach (JmdFile jmdFile in curJmdMountFrom.Files)
                        {
                            RaycityStorageFile newFile = new RaycityStorageFile(jmdFile);
                            newFile.Name = jmdFile.Name;
                            curJmdMountTo.AddFile(newFile);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.Print($"Error: {ex.Message}\nStack Trace:\n{ex.StackTrace}");
                }
            }
        }
    }
    
    public void Dispose()
    {
            
    }
}