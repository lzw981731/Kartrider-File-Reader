using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.IO;
using KartLibrary.IO;

namespace KartLibrary.Game.Engine.Tontrollers
{
    public static class RotateKeyDataFactory
    {
        public static IRotateKeyData CreateRotateKeyframeData(RotateKeyDataType keyDataType)
        {
            switch (keyDataType)
            {
                case RotateKeyDataType.Cubic:
                    return new CubicRotateKeyData();
                case RotateKeyDataType.Linear:
                    return new LinearRotateKeyData();
                case RotateKeyDataType.CubicAlt:
                    return new CubicAltRotateKeyData();
                case RotateKeyDataType.NoEasing:
                    return new NoEasingRotateKeyData();
                case RotateKeyDataType.ThreeAxis:
                    return new ThreeAxisRotateKeyData();
                default:
                    throw new Exception($"Couldn't find any FloatKeyData type for dataType:{keyDataType}");
            }
        }
    }
    
    // Keyframe Type 2, KeyData having 0, 1, 2, 3, 4 types
    public interface IRotateKeyData : IKeyData<Quaternion>
    {
        RotateKeyDataType KeyDataType { get; }
    }
    public abstract class RotateKeyData: IRotateKeyData
    {
        protected RotateKeyData()
        {

        }
        
        public bool IsReadOnly => false;

        public abstract RotateKeyDataType KeyDataType { get; }

        public abstract Quaternion GetValue(float time);

        public abstract void DecodeObject(BinaryReader reader, int count);

    }

    public abstract class RotateKeyData<TKeyframe> : RotateKeyData, IList<TKeyframe> where TKeyframe: IKeyframe<Quaternion>
    {
        protected RotateKeyData()
        {

        }

        private List<TKeyframe> container = new List<TKeyframe>();

        public TKeyframe this[int index] { get => container[index]; set => container[index] = value; }

        public bool IsReadOnly => false;

        public int Count => container.Count;
        
        public override abstract RotateKeyDataType KeyDataType { get; }

        private int _previousIndex = -1;

        private float _previousTime = -1;
        
        public override Quaternion GetValue(float time)
        {
            int start = 0;
            int end = Count - 1;
            if (_previousIndex >= 0)
                if(_previousTime <= time)
                    start = _previousIndex;
                else
                    end = _previousIndex;
            while (Math.Abs(start - end) > 1)
            {
                int mid = (start + end) >> 1;
                if (time < this[mid].Time)
                {
                    end = mid;
                }
                else if (this[mid].Time < time)
                {
                    start = mid;
                }
                else
                {
                    start = mid;
                    while (start >= 1 && this[start - 1].Time == this[start].Time)
                    {
                        start--;
                    }
                    break;
                }
            }
            if (this[end].Time < time)
                return this[end].Value;
            else if (this[start].Time > time)
                return this[start].Value;
            else
            {
                IKeyframe<Quaternion> curKeyFrame = this[start];
                IKeyframe<Quaternion>? nextKeyFrame = start + 1 >= Count ? null : this[start + 1];
                float duration = (nextKeyFrame?.Time ?? curKeyFrame.Time) - curKeyFrame.Time;
                float t = (time - curKeyFrame.Time) / (duration == 0 ? 1 : duration);
                _previousIndex = start;
                _previousTime = time;
                return curKeyFrame.CalculateKeyFrame(t, nextKeyFrame);
            }
        }

        public void Add(TKeyframe item)
        {
            if (IsReadOnly)
                throw new InvalidOperationException();
            container.Add(default);
            int i;
            for (i = Count - 1; i > 0; i--)
            {
                TKeyframe curObj = container[i - 1];
                if (curObj.Time > item.Time)
                    container[i] = curObj;
                else
                    break;
            }
            container[i] = item;
        }

        public void Clear()
        {
            container.Clear();
        }

        public bool Contains(TKeyframe item)
        {
            return container.Contains(item);
        }

        public void CopyTo(TKeyframe[] array, int arrayIndex)
        {
            container.CopyTo(array, arrayIndex);
        }

        public int IndexOf(TKeyframe item)
        {
            return container.IndexOf(item);
        }

        public void Insert(int index, TKeyframe item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKeyframe item)
        {
            return container.Remove(item);
        }

        public void RemoveAt(int index)
        {
            container.RemoveAt(index);
        }

        public IEnumerator<TKeyframe> GetEnumerator()
        {
            return container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override abstract void DecodeObject(BinaryReader reader, int count);

        public class Enumerator : IEnumerator<IKeyframe<Quaternion>>
        {
            private IEnumerator<TKeyframe> baseEnumerator;

            public TKeyframe Current => baseEnumerator.Current;

            object IEnumerator.Current => baseEnumerator.Current;

            IKeyframe<Quaternion> IEnumerator<IKeyframe<Quaternion>>.Current => baseEnumerator.Current;

            public Enumerator(IEnumerator<TKeyframe> baseEnumerator)
            {
                this.baseEnumerator = baseEnumerator;
            }

            public void Dispose()
            {
                baseEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return baseEnumerator.MoveNext();
            }

            public void Reset()
            {
                baseEnumerator.Reset();
            }
        }
    }

    public class CubicRotateKeyData : RotateKeyData<CubicRotateKeyframeData>
    {
        public CubicRotateKeyData()
        {

        }

        public override RotateKeyDataType KeyDataType => RotateKeyDataType.Cubic;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            // Ignore count parameter.
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                Vector4 quaternion = reader.ReadVector4();
                Add(new CubicRotateKeyframeData()
                {
                    Time = time,
                    Value = new Quaternion(quaternion.Y, quaternion.Z, quaternion.W, quaternion.X)
                });
            }
        }
    }
    
    public class LinearRotateKeyData : RotateKeyData<LinearRotateKeyframeData>
    {
        public LinearRotateKeyData()
        {

        }

        public override RotateKeyDataType KeyDataType => RotateKeyDataType.Linear;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            // Ignore count parameter.
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                Vector4 quaternion = reader.ReadVector4();
                Add(new LinearRotateKeyframeData()
                {
                    Time = time,
                    Value = new Quaternion(quaternion.Y, quaternion.Z, quaternion.W, quaternion.X)
                });
            }
        }
    }
    
    public class CubicAltRotateKeyData : RotateKeyData<CubicAltRotateKeyframeData>
    {
        public CubicAltRotateKeyData()
        {

        }

        public override RotateKeyDataType KeyDataType => RotateKeyDataType.CubicAlt;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            // Ignore count parameter.
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                Vector4 quaternion = reader.ReadVector4();
                Vector3 x1 = reader.ReadVector3();
                Add(new CubicAltRotateKeyframeData()
                {
                    Time = time,
                    Value = new Quaternion(quaternion.Y, quaternion.Z, quaternion.W, quaternion.X)
                });
            }
        }
    }
    
    public class NoEasingRotateKeyData : RotateKeyData<NoEasingRotateKeyframeData>
    {
        public NoEasingRotateKeyData()
        {

        }

        public override RotateKeyDataType KeyDataType => RotateKeyDataType.CubicAlt;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            // Ignore count parameter.
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                Vector4 quaternion = reader.ReadVector4();
                Add(new NoEasingRotateKeyframeData()
                {
                    Time = time,
                    Value = new Quaternion(quaternion.Y, quaternion.Z, quaternion.W, quaternion.X)
                });
            }
        }
    }

    public class CubicRotateKeyframeData : IKeyframe<Quaternion>
    {
        public int Time { get; set; }
        public Quaternion Value { get; set; }
        public Quaternion CalculateKeyFrame(float t, IKeyframe<Quaternion>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not CubicRotateKeyframeData)
                throw new ArgumentException();
            return Quaternion.Slerp(Value, nextKeyframe.Value, t);
        }
    }
    
    public class LinearRotateKeyframeData : IKeyframe<Quaternion>
    {
        public int Time { get; set; }
        public Quaternion Value { get; set; }
        public Quaternion CalculateKeyFrame(float t, IKeyframe<Quaternion>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not LinearRotateKeyframeData)
                throw new ArgumentException();
            return Quaternion.Slerp(Value, nextKeyframe.Value, t); 
        }
    }
    
    public class CubicAltRotateKeyframeData : IKeyframe<Quaternion>
    {
        public int Time { get; set; }
        public Quaternion Value { get; set; }
        public Quaternion CalculateKeyFrame(float t, IKeyframe<Quaternion>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not LinearRotateKeyframeData)
                throw new ArgumentException();
            return Quaternion.Slerp(Value, nextKeyframe.Value, t);
        }
    }
    
    public class NoEasingRotateKeyframeData : IKeyframe<Quaternion>
    {
        public int Time { get; set; }
        public Quaternion Value { get; set; }
        public Quaternion CalculateKeyFrame(float t, IKeyframe<Quaternion>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not LinearRotateKeyframeData)
                throw new ArgumentException();
            return Value;
        }
    }
    
    // DataType 4
    public class ThreeAxisRotateKeyData : RotateKeyData
    {
        public IFloatKeyData XAxisKeyData { get; set; }

        public IFloatKeyData YAxisKeyData { get; set; }

        public IFloatKeyData ZAxisKeyData { get; set; }

        private Quaternion _baseQuaternion;

        public ThreeAxisRotateKeyData()
        {
            XAxisKeyData = YAxisKeyData = ZAxisKeyData = new NoEasingFloatKeyData()
            {
                new (){ Time = 0, Value = 0 }
            };
        }

        public ThreeAxisRotateKeyData(IFloatKeyData xAxisKeyData, IFloatKeyData yAxisKeyData, IFloatKeyData zAxisKeyData)
        {
            XAxisKeyData = xAxisKeyData;
            YAxisKeyData = yAxisKeyData;
            ZAxisKeyData = zAxisKeyData;
        }

        public override RotateKeyDataType KeyDataType => RotateKeyDataType.ThreeAxis;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            // Ignore count parameter.
            int time = reader.ReadInt32();
            float x = (float)reader.ReadSingle();
            float y = (float)reader.ReadSingle();
            float z = (float)reader.ReadSingle();
            float w = (float)reader.ReadSingle();
            _baseQuaternion = new Quaternion(x, y, z, w);
            
            // x-axis
            FloatKeyDataType keyFrameType = (FloatKeyDataType) reader.ReadInt32();
            int floatKeyFrameCount = reader.ReadInt32();
            XAxisKeyData = new FloatKeyDataFactory().CreateFloatKeyframeData(keyFrameType);
            XAxisKeyData.DecodeObject(reader, floatKeyFrameCount);
            
            // y-axis
            keyFrameType = (FloatKeyDataType) reader.ReadInt32();
            floatKeyFrameCount = reader.ReadInt32();
            YAxisKeyData = new FloatKeyDataFactory().CreateFloatKeyframeData(keyFrameType);
            YAxisKeyData.DecodeObject(reader, floatKeyFrameCount);

            // z-axis
            keyFrameType = (FloatKeyDataType) reader.ReadInt32();
            floatKeyFrameCount = reader.ReadInt32();
            ZAxisKeyData = new FloatKeyDataFactory().CreateFloatKeyframeData(keyFrameType);
            ZAxisKeyData.DecodeObject(reader, floatKeyFrameCount);
        }

        public override Quaternion GetValue(float time)
        {
            float xAngle = XAxisKeyData.GetValue(time);
            float yAngle = YAxisKeyData.GetValue(time);
            float zAngle = ZAxisKeyData.GetValue(time);
            Quaternion xAxisQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, xAngle);
            Quaternion yAxisQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yAngle);
            Quaternion zAxisQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, zAngle);
            // return Quaternion.CreateFromYawPitchRoll(yAngle, xAngle, zAngle);
            return zAxisQuaternion * yAxisQuaternion * xAxisQuaternion;
        }
    }

    public enum RotateKeyDataType
    {
        Cubic = 0,
        Linear = 1,
        CubicAlt = 2,
        NoEasing = 3,
        ThreeAxis = 4
    }
}
