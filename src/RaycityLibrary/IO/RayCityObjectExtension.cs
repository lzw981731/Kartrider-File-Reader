namespace RayCityLibrary.IO;

public static class RayCityObjectExtension
{
    public static RayCityObject ReadRayCityObject(this BinaryReader br, RayCityObjectBuffer? buffer)
    {
        uint classStamp;
        RayCityObject output;
        if (buffer?.UseBuffering ?? false)
        {
            int signature = br.ReadUInt16();
            if(signature == 0x47BB)
            {
                int objIndex = buffer.UseInt32Index ? br.ReadInt32() : br.ReadInt16();
                output = buffer.GetRcObject(objIndex) ?? throw new IndexOutOfRangeException();
            }
            else if(signature == 0x47AA)
            {
                classStamp = br.ReadUInt32();
                int objIndex = buffer.UseInt32Index ? br.ReadInt32() : br.ReadInt16();
                
                output = RayCityObjectManager.CreateObject(classStamp) ?? throw new Exception("Can't create RcObject!");
                output.DecodeObject(br, buffer);

                if (!buffer.AddRcObjectForRead(objIndex, output))
                    throw new Exception("Can't add RcObject to buffer.");
            }
            else
            {
                throw new Exception($"Unknown signature: {signature:x4}.");
            }
        }
        else
        {
            classStamp = br.ReadUInt32();
            output = RayCityObjectManager.CreateObject(classStamp) ?? throw new Exception("Can't create RcObject!");
            output.DecodeObject(br, buffer);
        }
        
        return output;
    }

    public static TBase ReadRayCityObject<TBase>(this BinaryReader br, RayCityObjectBuffer? buffer) where TBase : RayCityObject
    {
        uint classStamp;
        TBase output;
        if (buffer?.UseBuffering ?? false)
        {
            int signature = br.ReadUInt16();
            if(signature == 0x47BB)
            {
                int objIndex = buffer.UseInt32Index ? br.ReadInt32() : br.ReadInt16();
                RayCityObject tmpObj = buffer.GetRcObject(objIndex) ?? throw new IndexOutOfRangeException();
                if (tmpObj is not TBase castedObj)
                    throw new Exception($"This object can't be casted to {typeof(TBase)}.");

                output = castedObj;
            }
            else if(signature == 0x47AA)
            {
                classStamp = br.ReadUInt32();
                int objIndex = buffer.UseInt32Index ? br.ReadInt32() : br.ReadInt16();
                
                output = RayCityObjectManager.CreateObject<TBase>(classStamp) ?? throw new Exception("Can't create RcObject!");
                output.DecodeObject(br, buffer);

                if (!buffer.AddRcObjectForRead(objIndex, output))
                    throw new Exception("Can't add RcObject to buffer.");
            }
            else
            {
                throw new Exception($"Unknown signature: {signature:x4}.");
            }
        }
        else
        {
            classStamp = br.ReadUInt32();
            output = RayCityObjectManager.CreateObject<TBase>(classStamp) ?? throw new Exception("Can't create RcObject!");
            output.DecodeObject(br, buffer);
        }
        
        return output;
    }

    // AA27 BB27
    public static T ReadObject<T>(this BinaryReader br, Dictionary<short, RayCityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap, DecodeFieldFunc<T> decodeFieldFunc)
    {
        if(decodedFieldMap is not null)
        {
            ushort token = br.ReadUInt16();
            if (token == 0x27AA)
            {
                short fieldObjIndex = br.ReadInt16();
                T decodedField = decodeFieldFunc(br, decodedObjectMap, decodedFieldMap);
                decodedFieldMap.Add(fieldObjIndex, decodedField);
                return decodedField;
            }
            else if (token == 0x27BB)
            {
                short fieldObjIndex = br.ReadInt16();
                if (decodedFieldMap.ContainsKey(fieldObjIndex))
                    if (decodedFieldMap[fieldObjIndex] is T outField)
                        return outField;
                    else
                        throw new InvalidCastException();
                else
                    throw new IndexOutOfRangeException();
            }
            else
                throw new Exception();
        }
        else
        {
            return decodeFieldFunc(br, decodedObjectMap, decodedFieldMap);
        }
    }

    public static byte[] ReadCacheableBytes(this BinaryReader br, Dictionary<short, RayCityObject>? decodedObjectMap,
        Dictionary<short, object>? decodedFieldMap)
    {
        return br.ReadObject(decodedObjectMap, decodedFieldMap, (x, y, z) => x.ReadBytes(x.ReadInt32()));
    }

    public delegate T DecodeFieldFunc<T>(BinaryReader reader, Dictionary<short, RayCityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap);

    public static void WriteRayCityObject(this BinaryWriter writer, RayCityObject rayCityObject,
        RayCityObjectBuffer? buffer)
    {
        bool reqWrite = true;

        if (buffer?.UseBuffering ?? false)
        {
            int index = buffer.AddRcObjectForWrite(rayCityObject, out var isNew);
            writer.Write((short)(isNew ? 0x47AA : 0x47BB));
            if(isNew)
                writer.Write(rayCityObject.ClassStamp);
            if(buffer.UseInt32Index)
                writer.Write(index);
            else if (index >= short.MaxValue)
                throw new Exception("The value of index is too big for 16 bits integer.");
            else
                writer.Write((short)index);

            reqWrite = isNew;
        }
        else
        {
            writer.Write(rayCityObject.ClassStamp);
        }

        if (reqWrite)
        {
            rayCityObject.EncodeObject(writer, buffer);
        }
    }
}
