using KartCityStudio.Common.Converter;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Utility;

public delegate void ExtractorProgressReport(object sender, ExtractProgressEventArgs e);

public class ArchiveExtractor
{
    private IArchiveElement[] _extractElements;
    private string _destinationFolder;
    private Dictionary<string, IFileConverter> _fileConverters = [];
    private Task? _extractWorker;

    private int _progressReportCounter = -1;

    public int ProgressReportDuration { get; set; } = 5;
    
    public event ExtractorProgressReport? ProgressReport;
    public event Action<Exception>? ExtractFailured;
    public event Action? ExtractCompleted;

    public ArchiveExtractor(string path, params IArchiveElement[] extractElements)
    {
        _destinationFolder = path;
        _extractElements = extractElements;
    }

    public void AddConverter(IFileConverter converter)
    {
        _fileConverters.TryAdd(converter.SourceExtension, converter);
    }

    public void BeginExtract(CancellationToken token)
    {
        _extractWorker = ExtractWork(token);
    }

    private async Task ExtractWork(CancellationToken token)
    {
        Queue<(string, IArchiveFolder)> prepareQueue = [];
        Queue<(string, IArchiveFile)> extractQueue = [];

        try
        {
            if(!Directory.Exists(_destinationFolder))
                Directory.CreateDirectory(_destinationFolder);
        
            foreach (var extractElement in _extractElements)
            {
                if (extractElement is IArchiveFolder folder)
                    prepareQueue.Enqueue((_destinationFolder, folder));
                else if(extractElement is IArchiveFile file)
                    extractQueue.Enqueue((_destinationFolder, file));
                ReportProgress(new ExtractProgressEventArgs()
                {
                    State = ExtractState.Preparing,
                    CurrentDumpFileName = extractElement.FullName,
                    Progress = 0
                });
            }
        
            while (prepareQueue.Any())
            {
                var queueEle =  prepareQueue.Dequeue();
                string newPath = Path.Combine(queueEle.Item1, queueEle.Item2.Name);

                if(!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);
            
                foreach (var folder in queueEle.Item2.Folders)
                    prepareQueue.Enqueue((newPath, folder));
            
                foreach (var file in queueEle.Item2.Files)
                    extractQueue.Enqueue((newPath, file));
                ReportProgress(new ExtractProgressEventArgs()
                {
                    State = ExtractState.Preparing,
                    CurrentDumpFileName = queueEle.Item2.FullName,
                    Progress = 0
                });
            }
            int totalFiles = extractQueue.Count;
            int extractedFiles = 0;

            CancellationTokenSource parallelCancellation = new CancellationTokenSource();
            
            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 3)
            };
            
            await Parallel.ForEachAsync(extractQueue, parallelOptions, async (queueEle, tokenParallel) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                        await parallelCancellation.CancelAsync();
                    if(tokenParallel.IsCancellationRequested)
                        return;
                    Interlocked.Increment(ref extractedFiles);
                    ReportProgress(new ExtractProgressEventArgs()
                    {
                        State = ExtractState.Extracting,
                        CurrentDumpFileName = queueEle.Item2.FullName,
                        ExtractedFilesCount = extractedFiles,
                        TotalFilesCount = totalFiles,
                        Progress = ((double)extractedFiles / totalFiles)
                    });
            
                    await using var inStream = queueEle.Item2.CreateStream();
                    byte[] buffer = new byte[queueEle.Item2.FileSize];
                    int readLen = await inStream.ReadAsync(buffer, tokenParallel);
                    if(readLen != buffer.Length)
                        Array.Resize(ref buffer, readLen);

                    string fileName = queueEle.Item2.Name;
                    string ext = Path.GetExtension(fileName);

                    if (_fileConverters.TryGetValue(ext, out var converter))
                    {
                        buffer = converter.ConvertFile(buffer);
                        fileName = $"{Path.GetFileNameWithoutExtension(fileName)}{converter.DestinationExtension}";
                    }
            
                    if(!Directory.Exists(queueEle.Item1))
                        Directory.CreateDirectory(queueEle.Item1);
            
                    string outPath =  Path.Combine(queueEle.Item1, fileName);
                    await using var outStream = new FileStream(outPath, FileMode.Create);
                    await outStream.WriteAsync(buffer, tokenParallel);
                }
                catch (Exception ex)
                {
                    await parallelCancellation.CancelAsync();
                    ExtractFailured?.Invoke(ex);
                }
            });
            ExtractCompleted?.Invoke();
        }
        catch(Exception ex)
        {
            ExtractFailured?.Invoke(ex);       
        }
    }

    private void ReportProgress(ExtractProgressEventArgs e)
    {
        int counter = Interlocked.Increment(ref _progressReportCounter);
        if((counter % ProgressReportDuration) == 0)
            ProgressReport?.Invoke(this, e);
    }
}

public class ExtractProgressEventArgs : EventArgs
{
    public ExtractState State { get; set; }
    
    public string CurrentDumpFileName { get; set; } = "";
    
    public double Progress { get; set; }
    
    public int ExtractedFilesCount { get; set; }
    
    public int TotalFilesCount { get; set; }
}

public enum ExtractState
{
    Padding,
    Preparing,
    Extracting,
    Finished
}