using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    [KartObjectImplement]
    public class PRSTontroller : Tontroller
    {
        public override string ClassName => "PRSTontroller";

        public IVector3KeyData? PositionKeyData { get; set; }
        
        public IRotateKeyData? RotateKeyData { get; set; }
        
        public IVector3KeyData? ScaleKeyData { get; set; }

        public int PositionBeginTime { get; set; }

        public int PositionEndTime { get; set; }
        
        public int RotateBeginTime { get; set; }
        
        public int RotateEndTime { get; set; }
        
        public int ScaleBeginTime { get; set; }
        
        public int ScaleEndTime { get; private set; }

        public PRSTontroller()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            if (reader.ReadByte() == 1)
            {
                PositionKeyData = reader.ReadField(buffer, (reader, buffer) =>
                {
                    Vector3KeyDataType dataType = (Vector3KeyDataType)reader.ReadInt32();
                    int count = reader.ReadInt32();
                    Vector3KeyDataFactory factory = new Vector3KeyDataFactory();
                    IVector3KeyData vector3KeyData = factory.CreateVector3KeyData(dataType);
                    vector3KeyData.DecodeObject(reader, count);
                    return vector3KeyData;
                });
            }
            if (reader.ReadByte() == 1)
            {
                RotateKeyData = reader.ReadField(buffer, (reader, buffer) =>
                {
                    RotateKeyDataType rotateKeyDataType = (RotateKeyDataType)reader.ReadInt32();
                    int count = reader.ReadInt32();
                    IRotateKeyData rotateKeyData = RotateKeyDataFactory.CreateRotateKeyframeData(rotateKeyDataType);
                    rotateKeyData.DecodeObject(reader, count);
                    return rotateKeyData;
                });
            }
            if (reader.ReadByte() == 1)
            {
                ScaleKeyData = reader.ReadField(buffer, (reader, buffer) =>
                {
                    Vector3KeyDataType dataType = (Vector3KeyDataType)reader.ReadInt32();
                    int count = reader.ReadInt32();
                    Vector3KeyDataFactory factory = new Vector3KeyDataFactory();
                    IVector3KeyData vector3KeyData = factory.CreateVector3KeyData(dataType);
                    vector3KeyData.DecodeObject(reader, count);
                    return vector3KeyData;
                });
            }
            PositionBeginTime = reader.ReadInt32();
            PositionEndTime = reader.ReadInt32();
            RotateBeginTime = reader.ReadInt32();
            RotateEndTime = reader.ReadInt32();
            ScaleBeginTime = reader.ReadInt32();
            ScaleEndTime = reader.ReadInt32();
            // Debug.Print($"{PositionBeginTime:x8} {PositionEndTime:x8} {RotateBeginTime:x8} {RotateEndTime:x8} {ScaleBeginTime:x8} {ScaleEndTime:x8}");
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public Vector3? GetPosition(float t)
        {
            if (PositionKeyData == null)
                return null;
            if(t >= PositionEndTime && _loopCount == 0)
            {
                t = ((t - PositionEndTime) % (PositionEndTime - PositionBeginTime)) + PositionBeginTime;
            }
            return PositionKeyData.GetValue(t);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public Quaternion? GetRotation(float t)
        {
            if (RotateKeyData == null)
                return null;
            if(t >= RotateEndTime && _loopCount == 0)
            {
                t = ((t - RotateEndTime) % (RotateEndTime - RotateBeginTime)) + RotateBeginTime;
            }
            return RotateKeyData.GetValue(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public Vector3? GetScale(float t)
        {
            if (ScaleKeyData == null)
                return null;
            if(t >= ScaleEndTime && _loopCount == 0)
            {
                t = ((t - ScaleEndTime) % (ScaleEndTime - ScaleBeginTime)) + ScaleBeginTime;
            }
            return ScaleKeyData.GetValue(t);
        }
    }
}
