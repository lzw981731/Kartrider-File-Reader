using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using eP.Text;
using KartCity.Common.Engine;
using KartCity.Common.IO;
using KartCity.Common.Xml;
using KartLibrary.Engine.Properities;
using KartLibrary.Game.Engine;
using KartLibrary.Game.Engine.Render;
using KartLibrary.Game.Engine.Tontrollers;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Engine.Relements
{
    [KartObjectImplement]
    public class Relement : NamedKartObject, IList<Relement>
    {
        public override string ClassName => "Relement";

        #region Members
        private Relement? _parent;

        private List<Relement> _container = new List<Relement>();
        
        private Matrix4x4 _rotation;
        private Vector3 _position;
        private Vector3 _scale;

        private Matrix4x4 _prevTransform;
        private Vector3 _prevPosition;
        private Vector3 _prevScale;

        private BoundingBox _boundingBox;
        private byte _unknownByte_70;
        private float _unknownFloat_74;
        private BoundingBox _unknownBB_78;
        private float _unknownFloat_90;
        private byte _unknownByte_94;
        private BoundingBox _renderBoundingBox;

        private VisTontroller? _visTontroller;
        private PRSTontroller? _prsTontroller;
        private KartObject? _unknownKartObj_a4;

        private AlphaProperty? _alphaProperty;
        private BackFaceProperty? _backfaceProperty;
        private FogProperty? _fogProperty; // fogProperty
        private MtlProperty? _mtlProperty; // mtlProperty
        private TexProperty? _texProperty;
        private ToonProperty? _toonProperty; // toonProperty
        private WireProperty? _wireProperty; // wireProperty
        private ZBufProperty? _zbufProperty;

        private BinaryXmlTag? _additionalProp;

        private Matrix4x4 _viewMat;
        #endregion

        #region Properties
        public Relement? Parent => _parent;
        
        public Matrix4x4 CurrentModelMatrix => _viewMat;
        protected ref Matrix4x4 CurrentModelMatrixRef => ref _viewMat;
        
        public Matrix4x4 Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        public Vector3 Position
        {
            get => _position;
            set => _position = value;
        }

        public Vector3 Scale
        {
            get => _scale;
            set => _scale = value;
        }

        public BoundingBox Bounding
        {
            get => _boundingBox;
            set => _boundingBox = value;
        }

        public int Count => _container.Count;

        public Relement this[int index] { get => _container[index]; set => _container[index] = value; }

        public bool IsReadOnly => false;

        protected bool HasFirstUpdate { get; private set; } = false; 

        public VisTontroller? VisTontroller
        {
            get => _visTontroller;
            set => _visTontroller = value;
        }

        public PRSTontroller? PRSTontroller
        {
            get => _prsTontroller;
            set => _prsTontroller = value;
        }

        public KartObject? UnknownTontroller
        {
            get => _unknownKartObj_a4;
            set => _unknownKartObj_a4 = value;
        }

        public AlphaProperty? Alpha
        {
            get => _alphaProperty;
            set => _alphaProperty = value;
        }

        public BackFaceProperty? BackFace
        {
            get => _backfaceProperty;
            set => _backfaceProperty = value;
        }

        public FogProperty? Fog
        {
            get => _fogProperty;
            set => _fogProperty = value;
        }

        public MtlProperty? MTL
        {
            get => _mtlProperty;
            set => _mtlProperty = value;
        }

        public TexProperty? Tex
        {
            get => _texProperty;
            set => _texProperty = value;
        }

        public ToonProperty? Toon
        {
            get => _toonProperty;
            set => _toonProperty = value;
        }

        public WireProperty? Wire
        {
            get => _wireProperty;
            set => _wireProperty = value;
        }

        public ZBufProperty? ZBuf
        {
            get => _zbufProperty;
            set => _zbufProperty = value;
        }

        public BinaryXmlTag? Additional
        {
            get => _additionalProp;
            set => _additionalProp = value;
        }
        #endregion

        #region Constructors
        public Relement()
        {

        }
        #endregion

        #region Relement Methods

        public TexProperty? GetTexPropertyOrInHerit()
        {
            Relement? curRelement = this;
            while (curRelement is not null)
            {
                if (curRelement.Tex is not null)
                    return curRelement.Tex;

                curRelement = curRelement.Parent;
            }

            return null;
        }
        
        public override string ToString()
        {
            
            StringBuilder stringBuilder = new StringBuilder();
            constructString(stringBuilder, 0);
            return stringBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Update(ITimeSource timeSource)
        {
            Matrix4x4 initModelMat = Matrix4x4.Identity;
            Update(ref initModelMat, timeSource, !HasFirstUpdate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected void Update(ref Matrix4x4 parentModelMatrix, ITimeSource timeSource, bool parentUpdated)
        {
            float time = timeSource.GetTimeStamp();
            Vector3 newPosition = PRSTontroller?.GetPosition(time) ?? Position;
            Quaternion? newQuaternion = PRSTontroller?.GetRotation(time);
            Vector3 newScale = PRSTontroller?.GetScale(time) ?? Scale;
            parentUpdated |= newPosition != _prevPosition || newScale != _scale || newQuaternion is not null;
            Matrix4x4 currentMatrix =
                parentUpdated 
                    ? newQuaternion is { } quaternion 
                        ? Matrix4x4.CreateScale(newScale) * Matrix4x4.CreateFromQuaternion(quaternion) * Matrix4x4.CreateTranslation(newPosition) * parentModelMatrix
                        : Matrix4x4.CreateScale(newScale) * _rotation * Matrix4x4.CreateTranslation(newPosition) * parentModelMatrix
                    : _viewMat;
            UpdateRelement(ref currentMatrix, timeSource, parentUpdated);
            foreach(Relement child in this)
                child.Update(ref currentMatrix, timeSource, parentUpdated);

            _prevPosition = newPosition;
            _prevScale = newScale;
            
            HasFirstUpdate = true;
        }

        private void constructString(StringBuilder stringBuilder, int indentLevel)
        {
            string indendStr = "".PadLeft(indentLevel << 2, ' ');
            stringBuilder.AppendLine($"{indendStr}<{ClassName} name=\"{Name}\">");

            // Print Relement properties
            stringBuilder.AppendLine($"{indendStr}    <RelementProperties>");
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Transform", _rotation);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Position", _position);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Scale", _scale);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "BoundingBox", _boundingBox);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownByte_70", _unknownByte_70);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownFloat_74", _unknownFloat_74);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownBB_78", _unknownBB_78);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownFloat_90", _unknownFloat_90);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownByte_94", _unknownByte_94);

            stringBuilder.ConstructPropertyString(indentLevel + 2, "PRSTontroller", PRSTontroller);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "VisTontroller", VisTontroller);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "UnknownTontroller", UnknownTontroller);

            stringBuilder.ConstructPropertyString(indentLevel + 2, "Alpha", Alpha);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Fog", Fog);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Mtl", MTL);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Tex", Tex);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Toon", Toon);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Wire", Wire);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "ZBuf", ZBuf);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "Additional", Additional);
            stringBuilder.AppendLine($"{indendStr}    </RelementProperties>");

            // Print other info
            ConstructOtherInfo(stringBuilder, indentLevel);

            // Print children
            stringBuilder.AppendLine($"{indendStr}    <Children>");
            foreach (Relement child in this)
                child.constructString(stringBuilder, indentLevel + 2);
            stringBuilder.AppendLine($"{indendStr}    </Children>");

            stringBuilder.AppendLine($"{indendStr}</{ClassName}>");
        }

        protected virtual void ConstructOtherInfo(StringBuilder stringBuilder, int indentLevel)
        {
            
        }

        protected virtual void UpdateRelement(ref Matrix4x4 modelMatrix, ITimeSource timeSource, bool updated)
        {
            if(updated)
                _viewMat = modelMatrix;
        }
        #endregion

        #region Implements of KartObject abstract methods
        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            Debug.Print($"{Name} {reader.BaseStream.Position:x8}");
            int elementCount = reader.ReadInt32();
            for (int i = 0; i < elementCount; i++)
            {
                Relement? child = reader.ReadKartObject<Relement>(buffer);
                if (child != null)
                    Add(child);
                else
                    Debug.Print("ERROR!");
            }

            _rotation = new Matrix4x4();
            _rotation[3, 3] = 1;
            for (int i = 0; i < 3; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                _rotation[0, i] = x;
                _rotation[1, i] = y;
                _rotation[2, i] = z;
            }

            _position = reader.ReadVector3();
            _scale = reader.ReadVector3();

            _boundingBox = reader.ReadBoundBox();
            _unknownByte_70 = reader.ReadByte();
            _unknownFloat_74 = reader.ReadSingle();
            _unknownBB_78 = reader.ReadBoundBox();
            _unknownFloat_90 = reader.ReadSingle();
            _unknownByte_94 = reader.ReadByte();
            if(_unknownByte_70 != 0)
                Debug.Print($"{Name}: 70: {_unknownByte_70}");
            if(_unknownByte_94 != 1)
                Debug.Print($"{Name}: 94: {_unknownByte_94}");

            if (reader.ReadByte() == 1)
                _visTontroller = reader.ReadKartObject<VisTontroller>(buffer);
            if (reader.ReadByte() == 1)
                _prsTontroller = reader.ReadKartObject<PRSTontroller>(buffer);
            if (reader.ReadByte() == 1)
                _unknownKartObj_a4 = reader.ReadKartObject(buffer);
            if (reader.ReadByte() == 1)
                _alphaProperty = reader.ReadKartObject<AlphaProperty>(buffer);
            if (reader.ReadByte() == 1)
                _backfaceProperty = reader.ReadKartObject<BackFaceProperty>(buffer);// backface property
            if (reader.ReadByte() == 1)
                _fogProperty = reader.ReadKartObject<FogProperty>(buffer);
            if (reader.ReadByte() == 1)
                _mtlProperty = reader.ReadKartObject<MtlProperty>(buffer);
            if (reader.ReadByte() == 1)
                _texProperty = reader.ReadKartObject<TexProperty>(buffer);
            if (reader.ReadByte() == 1)
                _toonProperty = reader.ReadKartObject<ToonProperty>(buffer);
            if (reader.ReadByte() == 1)
                _wireProperty = reader.ReadKartObject<WireProperty>(buffer);
            if (reader.ReadByte() == 1)
                _zbufProperty = reader.ReadKartObject<ZBufProperty>(buffer);

            if (reader.ReadByte() == 1)
            {
                _additionalProp = reader.ReadField(buffer, (reader, buffer) =>
                {
                    return reader.ReadBinaryXmlTag(Encoding.Unicode);
                });
                Debug.Print($"{Name}: _addProp {_additionalProp}");
            }
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
        #endregion

        #region Implelements of IList 
        public int IndexOf(Relement item)
        {
            return _container.IndexOf(item);
        }

        public void Insert(int index, Relement item)
        {
            item._parent = this;
            _container.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (_container.Count <= index)
                throw new IndexOutOfRangeException();
            _container[index]._parent = null;
            _container.RemoveAt(index);
        }

        public void Add(Relement item)
        {
            item._parent = this;
            _container.Add(item);
        }

        public void Clear()
        {
            foreach (Relement item in _container)
                item._parent = null;
            _container.Clear();
        }

        public bool Contains(Relement item)
        {
            return _container.Contains(item);
        }

        public void CopyTo(Relement[] array, int arrayIndex)
        {
            _container.CopyTo(array, arrayIndex);
        }

        public bool Remove(Relement item)
        {
            bool result = _container.Remove(item);
            if (result)
                item._parent = null;
            return result;
        }

        public IEnumerator<Relement> GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public void ConstructAllRenderable(List<IRenderable> renderables)
        {
            if(this is IRenderable renderable)
                renderables.Add(renderable);
            foreach(Relement relement in this)
                relement.ConstructAllRenderable(renderables);
        }
        
        public void Dispose()
        {
            foreach (Relement child in this)
                child.Dispose();
        }
    }
}
