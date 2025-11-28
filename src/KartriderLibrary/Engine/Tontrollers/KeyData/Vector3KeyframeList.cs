using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    // Type: 1, having 0, 1, 2, 3, 5 KeyDataType.
    // KeyDataType 5 is 3 FloatKeyData
    public class Vector3KeyDataFactory
    {
        public IVector3KeyData CreateVector3KeyData(Vector3KeyDataType dataType)
        {
            switch(dataType)
            {
                case Vector3KeyDataType.Linear:
                    return new LinearVector3KeyData();
                case Vector3KeyDataType.Cubic:
                    return new CubicVector3KeyData();
                case Vector3KeyDataType.CubicAlt:
                    return new CubicAltVector3KeyData();
                case Vector3KeyDataType.NoEasing:
                    return new NoEasingVector3KeyData();
                case Vector3KeyDataType.Discret:
                    return new DiscretVector3KeyData();
                default:
                    throw new Exception();
            }
        }
    }
    
    public interface IVector3KeyData : IKeyData<Vector3>
    {
        Vector3KeyDataType KeyDataType { get; }
    }

    public abstract class Vector3KeyData<TKeyframe> : IVector3KeyData, IList<TKeyframe> where TKeyframe : IKeyframe<Vector3>
    {
        protected Vector3KeyData()
        {

        }

        private List<TKeyframe> container = new List<TKeyframe>();

        public TKeyframe this[int index] { get => container[index]; set => container[index] = value; }

        public bool IsReadOnly => false;

        public int Count => container.Count;

        public abstract Vector3KeyDataType KeyDataType { get; }

        private int _previousIndex = -1;

        private float _previousTime = -1;
        
        public Vector3 GetValue(float time)
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
                IKeyframe<Vector3> curKeyFrame = this[start];
                IKeyframe<Vector3>? nextKeyFrame = start + 1 >= Count ? null : this[start + 1];
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

        public abstract void DecodeObject(BinaryReader reader, int count);

        public class Enumerator : IEnumerator<IKeyframe<Vector3>>
        {
            private IEnumerator<TKeyframe> baseEnumerator;

            public TKeyframe Current => baseEnumerator.Current;

            object IEnumerator.Current => baseEnumerator.Current;

            IKeyframe<Vector3> IEnumerator<IKeyframe<Vector3>>.Current => baseEnumerator.Current;

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

    public class LinearVector3KeyData : Vector3KeyData<LinearVector3Keyframe>
    {
        public override Vector3KeyDataType KeyDataType => Vector3KeyDataType.Linear;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                Add(new LinearVector3Keyframe
                {
                    Time = time,
                    Value = new Vector3(x, y, z)
                });
            }
        }
    }

    public class CubicVector3KeyData : Vector3KeyData<CubicVector3Keyframe>
    {
        public override Vector3KeyDataType KeyDataType => Vector3KeyDataType.Cubic;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int uu1 = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                float leftSlopX = reader.ReadSingle();
                float leftSlopY = reader.ReadSingle();
                float leftSlopZ = reader.ReadSingle();
                float rightSlopX = reader.ReadSingle();
                float rightSlopY = reader.ReadSingle();
                float rightSlopZ = reader.ReadSingle();
                Add(new CubicVector3Keyframe
                {
                    Time = uu1,
                    Value = new Vector3(x, y, z),
                    LeftSlop = new Vector3(leftSlopX, leftSlopY, leftSlopZ),
                    RightSlop = new Vector3(rightSlopX, rightSlopY, rightSlopZ),
                });
            }
        }
    }
    
    public class CubicAltVector3KeyData : Vector3KeyData<CubicAltVector3Keyframe>
    {
        public override Vector3KeyDataType KeyDataType => Vector3KeyDataType.CubicAlt;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                float x1 = reader.ReadSingle();
                float x2 = reader.ReadSingle();
                float x3 = reader.ReadSingle();
                Add(new CubicAltVector3Keyframe
                {
                    Time = time,
                    Value = new Vector3(x, y, z),
                    X1 = x1,
                    X2 = x2,
                    X3 = x3,
                });
            }
        }
    }
    
    public class NoEasingVector3KeyData : Vector3KeyData<NoEasingVector3Keyframe>
    {
        public override Vector3KeyDataType KeyDataType => Vector3KeyDataType.NoEasing;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                Add(new NoEasingVector3Keyframe()
                {
                    Time = time,
                    Value = new Vector3(x, y, z),
                });
            }
        }
    }

    public class DiscretVector3KeyData : IVector3KeyData
    {
        public Vector3KeyDataType KeyDataType => Vector3KeyDataType.Discret;

        private IFloatKeyData _x1KeyData;
        private IFloatKeyData _x2KeyData;
        private IFloatKeyData _x3KeyData;
        
        public Vector3 GetValue(float time)
        {
            return new Vector3(
                _x1KeyData.GetValue(time),
                _x2KeyData.GetValue(time),
                _x3KeyData.GetValue(time)
            );
        }

        public void DecodeObject(BinaryReader reader, int count)
        {
            IFloatKeyData[] floatKeyDataArr = new IFloatKeyData[3];
            for (int i = 0; i < 3; i++)
            {
                FloatKeyDataType keyDataType = (FloatKeyDataType)reader.ReadInt32();
                int keyDataCount = reader.ReadInt32();
                IFloatKeyData floatKeyData = new FloatKeyDataFactory().CreateFloatKeyframeData(keyDataType);
                floatKeyData.DecodeObject(reader, keyDataCount);
                floatKeyDataArr[i] = floatKeyData;
            }

            _x1KeyData = floatKeyDataArr[0];
            _x2KeyData = floatKeyDataArr[1];
            _x3KeyData = floatKeyDataArr[2];
        }
    }
    
    public class LinearVector3Keyframe : IKeyframe<Vector3>
    {
        public int Time { get; set; }
        public Vector3 Value { get; set; }

        public Vector3 CalculateKeyFrame(float t, IKeyframe<Vector3>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not LinearVector3Keyframe)
                throw new ArgumentException();
            Vector3 result = Value * (1 - t) + nextKeyframe.Value * t;
            return result;
        }
    }

    public class CubicVector3Keyframe : IKeyframe<Vector3>
    {
        public int Time { get; set; }
        public Vector3 Value { get; set; }
        public Vector3 LeftSlop { get; set; }
        public Vector3 RightSlop { get; set; }

        public Vector3 CalculateKeyFrame(float t, IKeyframe<Vector3>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not CubicVector3Keyframe)
                throw new ArgumentException();
            CubicVector3Keyframe next = (CubicVector3Keyframe)nextKeyframe;
            Vector3 delta = nextKeyframe.Value - Value;
            Vector3 a = RightSlop + next.LeftSlop - 2 * delta;
            Vector3 b = 3 * delta - next.LeftSlop - 2 * RightSlop;
            Vector3 c = RightSlop;
            Vector3 result = ((a * t + b) * t + c) * t + Value;
            return result;
        }
    }
    
    public class CubicAltVector3Keyframe : IKeyframe<Vector3>
    {
        public int Time { get; set; }
        public Vector3 Value { get; set; }
        
        public float X1 { get; set; }
        
        public float X2 { get; set; }
        
        public float X3 { get; set; }

        public Vector3 CalculateKeyFrame(float t, IKeyframe<Vector3>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            if (nextKeyframe is not CubicAltVector3Keyframe)
                throw new ArgumentException();
            CubicAltVector3Keyframe next = (CubicAltVector3Keyframe)nextKeyframe;
            Vector3 delta = nextKeyframe.Value - Value;
            Vector3 result = ((X1 * t + X2) * t + X3) * t * delta + Value;
            return result;
        }
    }
    
    public class NoEasingVector3Keyframe : IKeyframe<Vector3>
    {
        public int Time { get; set; }
        
        public Vector3 Value { get; set; }

        public Vector3 CalculateKeyFrame(float t, IKeyframe<Vector3>? nextKeyframe)
        {
            if (nextKeyframe is null)
                return Value;
            
            if (nextKeyframe is not CubicAltVector3Keyframe)
                throw new ArgumentException();
            
            return Value;
        }
    }

    public enum Vector3KeyDataType
    {
        Cubic,
        Linear,
        CubicAlt,
        NoEasing,
        Discret = 5,
    }
}
