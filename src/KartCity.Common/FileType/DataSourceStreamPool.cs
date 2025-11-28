namespace KartCity.Common.FileType;

public class DataSourceStreamPool: IDisposable
{
    private readonly HashSet<DataSourceStream> _createdStreamPool = [];

    public bool IsEmpty => _createdStreamPool.Count == 0;
    public int Count => _createdStreamPool.Count;

    public DataSourceStreamPool()
    {
        
    }

    public DataSourceStream CreateStream(Stream baseStream)
    {
        DataSourceStream outStream = new DataSourceStream(baseStream);
        outStream.StreamDisposed += createdStreamDisposed;
        _createdStreamPool.Add(outStream);
        return outStream;
    }
    
    public void Dispose()
    {
        foreach(var item in _createdStreamPool)
            item?.Dispose();
        _createdStreamPool.Clear();
    }

    private void createdStreamDisposed(DataSourceStream obj)
    {
        _createdStreamPool.Remove(obj);
    }
}