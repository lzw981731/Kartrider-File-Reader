namespace KartLibrary.IO;

public class KartObjectBuffer
{
    public bool UseInt32Index { get; set; } = false;
    
    public bool UseBuffering { get; set; } = true;
    
    public BufferMode BufferMode { get; }

    public Dictionary<int, KartObject> IndexToKartObjectMap { get; } = [];

    public Dictionary<KartObject, int> KartObjectToIndexMap { get; } = [];
    
    public Dictionary<int, object> IndexToObjectMap { get; } = [];
    
    public Dictionary<object, int> ObjectToIndexMap { get; } = [];

    public bool IsKartObjectFull => (!UseInt32Index && IndexToKartObjectMap.Count >= short.MaxValue) ||
                                  (UseInt32Index && IndexToKartObjectMap.Count >= int.MaxValue);
    
    public bool IsObjectFull => (!UseInt32Index && IndexToObjectMap.Count >= short.MaxValue) ||
                                  (UseInt32Index && IndexToObjectMap.Count >= int.MaxValue);

    public KartObjectBuffer(BufferMode bufferMode)
    {
        BufferMode = bufferMode;
    }
    
    public int AddKartObjectForWrite(KartObject rcObj, out bool isNew)
    {
        isNew = true;
        if (BufferMode != BufferMode.ForWrite)
            throw new Exception("Only available for write-mode.");
        
        if (IsKartObjectFull)
            throw new Exception("Exceeded index limit.");

        if (!UseBuffering)
            return -1;
        
        lock(IndexToKartObjectMap)
        lock(KartObjectToIndexMap)
        {
            if (KartObjectToIndexMap.TryGetValue(rcObj, out int index))
            {
                isNew = false;
                return index;
            }

            index = IndexToKartObjectMap.Count;
            IndexToKartObjectMap.TryAdd(index, rcObj);
            KartObjectToIndexMap.TryAdd(rcObj, index);

            return index;
        }
    }

    public bool AddKartObjectForRead(int index, KartObject rcObj)
    {
        if (BufferMode != BufferMode.ForRead)
            throw new Exception("Only available for read-mode.");
        
        if (!UseBuffering)
            return true;
        
        lock(IndexToKartObjectMap)
        lock (KartObjectToIndexMap)
        {
            if (KartObjectToIndexMap.TryGetValue(rcObj, out _))
                return false;
            
            if (IndexToKartObjectMap.TryGetValue(index, out _))
                return false;

            return IndexToKartObjectMap.TryAdd(index, rcObj) && KartObjectToIndexMap.TryAdd(rcObj, index);
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

    public KartObject? GetKartObject(int index)
    {
        return UseBuffering && IndexToKartObjectMap.TryGetValue(index, out var value) ? value : null;
    }
    
    public object? GetObject(int index)
    {
        return UseBuffering && IndexToObjectMap.TryGetValue(index, out var value) ? value : null;
    }
    
    public int GetKartObjectIndex(KartObject rcObj)
    {
        return UseBuffering && KartObjectToIndexMap.TryGetValue(rcObj, out var value) ? value : -1;
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