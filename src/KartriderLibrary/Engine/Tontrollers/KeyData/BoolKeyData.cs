using KartLibrary.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    /*
        BoolKeyData:    0
        Vector3KeyData:  1
        RotKeyData:      2
        ColorKeyData:    3
        BoolKeyData:     4
        IntKeyData:      5
     */
    // Type 0 Keyframe
    public class BoolKeyDataFactory
    {
        public IBoolKeyData CreateBoolKeyframeData(BoolKeyDataType keyDataType)
        {
            switch (keyDataType)
            {
                case BoolKeyDataType.NoEasing:
                    return new NoEasingBoolKeyData();
                default:
                    throw new Exception($"Couldn't find any BoolKeyData type for dataType:{keyDataType}");
            }
        }
    }
    
    public interface IBoolKeyData : IKeyData<bool>
    {
        BoolKeyDataType KeyDataType { get; }
    }

    public abstract class BoolKeyData<TKeyframe> : IBoolKeyData, IList<TKeyframe> where TKeyframe : IKeyframe<bool>
    {
        protected BoolKeyData()
        {

        }

        private List<TKeyframe> _container = new List<TKeyframe>();

        public TKeyframe this[int index] { get => _container[index]; set => _container[index] = value; }

        public bool IsReadOnly => false;

        public int Count => _container.Count;

        public abstract BoolKeyDataType KeyDataType { get; }

        public bool GetValue(float time)
        {
            int start = 0;
            int end = Count - 1;
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
                IKeyframe<bool> curKeyFrame = this[start];
                IKeyframe<bool>? nextKeyFrame = start + 1 >= Count ? null : this[start + 1];
                float duration = (nextKeyFrame?.Time ?? curKeyFrame.Time) - curKeyFrame.Time;
                float t = (time - curKeyFrame.Time) / (duration == 0 ? 1 : duration);
                return curKeyFrame.CalculateKeyFrame(t, nextKeyFrame);
            }
        }

        public void Add(TKeyframe item)
        {
            if (IsReadOnly)
                throw new InvalidOperationException();
            _container.Add(default);
            int i;
            for (i = Count - 1; i > 0; i--)
            {
                TKeyframe curObj = _container[i - 1];
                if (curObj.Time > item.Time)
                    _container[i] = curObj;
                else
                    break;
            }
            _container[i] = item;
        }

        public void Clear()
        {
            _container.Clear();
        }

        public bool Contains(TKeyframe item)
        {
            return _container.Contains(item);
        }

        public void CopyTo(TKeyframe[] array, int arrayIndex)
        {
            _container.CopyTo(array, arrayIndex);
        }

        public int IndexOf(TKeyframe item)
        {
            return _container.IndexOf(item);
        }

        public void Insert(int index, TKeyframe item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKeyframe item)
        {
            return _container.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _container.RemoveAt(index);
        }

        public IEnumerator<TKeyframe> GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void DecodeObject(BinaryReader reader, int count);

        public class Enumerator : IEnumerator<IKeyframe<bool>>
        {
            private IEnumerator<TKeyframe> baseEnumerator;

            public TKeyframe Current => baseEnumerator.Current;

            object IEnumerator.Current => baseEnumerator.Current;

            IKeyframe<bool> IEnumerator<IKeyframe<bool>>.Current => baseEnumerator.Current;

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

    public class NoEasingBoolKeyData : BoolKeyData<NoEasingBoolKeyframe>
    {
        public override BoolKeyDataType KeyDataType => BoolKeyDataType.NoEasing;

        public override void DecodeObject(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int time = reader.ReadInt32();
                byte value = reader.ReadByte();
                Add(new NoEasingBoolKeyframe
                {
                    Time = time,
                    Value = value == 0x01,
                });
            }
        }
    }

    public class NoEasingBoolKeyframe : IKeyframe<bool>
    {
        public int Time { get; set; }
        public bool Value { get; set; }
        public bool CalculateKeyFrame(float t, IKeyframe<bool>? nextKeyframe)
        {
            return Value;
        }
    }

    public enum BoolKeyDataType
    {
        NoEasing = 3,
    }
}
