using System.Numerics;
using KartCity.Common.IO;
using KartLibrary.IO;

namespace KartLibrary.Game.Engine.Track;

[KartObjectImplement]
public class ToRoad: TrackObject
{
    public override string ClassName => "ToRoad";

    public byte Unknown0 { get; set; }

    public List<ToRoadInternal> Unknown1 { get; set; } = new List<ToRoadInternal>();

    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        byte u0 = reader.ReadByte();
        int len = reader.ReadInt32();
        for (int i = 0; i < len; i++)
        {
            ToRoadInternal obj = new ToRoadInternal();
            obj.u1 = reader.ReadKRString();
            obj.u2 = new Vector3[reader.ReadInt32()];
            for (int j = 0; j < obj.u2.Length; j++)
                obj.u2[j] = reader.ReadVector3();
            // 970
            obj.u3 = new (short, short, short)[reader.ReadInt32()];
            for (int j = 0; j < obj.u3.Length; j++)
                obj.u3[j] = (reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            obj.u4 = reader.ReadKRString();
            obj.u5 = new (short, short, short)[reader.ReadInt32()];
            for (int j = 0; j < obj.u5.Length; j++)
                obj.u5[j] = (reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            
            obj.u6 = new Vector3[reader.ReadInt32(), 3];
            for (int j = 0; j < obj.u6.GetLength(0); j++)
            {
                for (int k = 0; k < 3; k++)
                    obj.u6[j, k] = reader.ReadVector3();
            }

            Unknown1.Add(obj);
        }
    }

    public struct ToRoadInternal
    {
        public string u1 { get; set; }
        
        public Vector3[] u2 { get; set; }
        
        public (short, short, short)[] u3 { get; set; }
        
        public string u4 { get; set; }
        
        public (short, short, short)[] u5 { get; set; }
        
        public Vector3[,] u6 { get; set; }


    }
}