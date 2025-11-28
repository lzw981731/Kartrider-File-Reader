namespace RayCityLibrary.IO;

public class RayCityObjectBuffer
{
    public bool UseInt32Index { get; set; } = false;
    
    public bool UseBuffering { get; set; } = true;
    
    public BufferMode BufferMode { get; }

    public Dictionary<int, RayCityObject> IndexToRcObjectMap { get; } = [];

    public Dictionary<RayCityObject, int> RcObjectToIndexMap { get; } = [];
    
    public Dictionary<int, object> IndexToObjectMap { get; } = [];
    
    public Dictionary<object, int> ObjectToIndexMap { get; } = [];

    public bool IsRcObjectFull => (!UseInt32Index && IndexToRcObjectMap.Count >= short.MaxValue) ||
                                  (UseInt32Index && IndexToRcObjectMap.Count >= int.MaxValue);
    
    public bool IsObjectFull => (!UseInt32Index && IndexToObjectMap.Count >= short.MaxValue) ||
                                  (UseInt32Index && IndexToObjectMap.Count >= int.MaxValue);

    public RayCityObjectBuffer(BufferMode bufferMode)
    {
        BufferMode = bufferMode;
    }
    
    public int AddRcObjectForWrite(RayCityObject rcObj, out bool isNew)
    {
        isNew = true;
        if (BufferMode != BufferMode.ForWrite)
            throw new Exception("Only available for write-mode.");
        
        if (IsRcObjectFull)
            throw new Exception("Exceeded index limit.");

        if (!UseBuffering)
            return -1;
        
        lock(IndexToRcObjectMap)
        lock(RcObjectToIndexMap)
        {
            if (RcObjectToIndexMap.TryGetValue(rcObj, out int index))
            {
                isNew = false;
                return index;
            }

            index = IndexToRcObjectMap.Count;
            IndexToRcObjectMap.TryAdd(index, rcObj);
            RcObjectToIndexMap.TryAdd(rcObj, index);

            return index;
        }
    }

    public bool AddRcObjectForRead(int index, RayCityObject rcObj)
    {
        if (BufferMode != BufferMode.ForRead)
            throw new Exception("Only available for read-mode.");
        
        if (!UseBuffering)
            return true;
        
        lock(IndexToRcObjectMap)
        lock (RcObjectToIndexMap)
        {
            if (RcObjectToIndexMap.TryGetValue(rcObj, out _))
                return false;
            
            if (IndexToRcObjectMap.TryGetValue(index, out _))
                return false;

            return IndexToRcObjectMap.TryAdd(index, rcObj) && RcObjectToIndexMap.TryAdd(rcObj, index);
        }
    }
    
    public int AddObjectForWrite(object obj)
    {
        if (BufferMode != BufferMode.ForRead)
            throw new Exception("Only available for read-mode.");
        
        if (IsObjectFull)
            throw new Exception("Exceeded index limit.");

        if (!UseBuffering)
            return -1;
        
        lock(IndexToObjectMap)
        lock(ObjectToIndexMap)
        {
            if (ObjectToIndexMap.TryGetValue(obj, out int index))
                return index;

            index = IndexToObjectMap.Count;
            IndexToObjectMap.TryAdd(index, obj);
            ObjectToIndexMap.TryAdd(obj, index);

            return index;
        }
    }
    
    public bool AddObjectForRead(int index, object obj)
    {
        if (BufferMode != BufferMode.ForRead)
            throw new Exception("Only available for read-mode.");
        
        if (!UseBuffering)
            return true;
        
        lock(IndexToObjectMap)
        lock (ObjectToIndexMap)
        {
            if (ObjectToIndexMap.TryGetValue(obj, out _))
                return false;
            
            if (IndexToObjectMap.TryGetValue(index, out _))
                return false;

            return IndexToObjectMap.TryAdd(index, obj) && ObjectToIndexMap.TryAdd(obj, index);
        }
    }

    public RayCityObject? GetRcObject(int index)
    {
        return UseBuffering && IndexToRcObjectMap.TryGetValue(index, out var value) ? value : null;
    }
    
    public object? GetObject(int index)
    {
        return UseBuffering && IndexToObjectMap.TryGetValue(index, out var value) ? value : null;
    }
    
    public int GetRcObjectIndex(RayCityObject rcObj)
    {
        return UseBuffering && RcObjectToIndexMap.TryGetValue(rcObj, out var value) ? value : -1;
    }
    
    public int GetObjectIndex(object obj)
    {
        return UseBuffering && ObjectToIndexMap.TryGetValue(obj, out var value) ? value : -1;
    }
}

public enum BufferMode
{
    ForRead,
    ForWrite,
}