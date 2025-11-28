using System.Diagnostics;
using System.Text;
using KartCity.Common.IO;
using KartCity.Common.Xml;
using KartLibrary.Engine.Enums;
using KartLibrary.Game.Engine.Tontrollers;
using KartLibrary.IO;
using KartLibrary.Xml;
using Vortice.Direct3D11;

namespace KartLibrary.Engine.Properities
{
    [KartObjectImplement]
    public class TexProperty : KartObject
    {
        public D3DTextureOp TextureOp { get; set; }
        public string TexName { get; set; } = "";
        public TextureAddressMode AddressU { get; set; }
        public TextureAddressMode AddressV { get; set; }

        public D3DTextureFilterType MinFilter { get; set; }
        public D3DTextureFilterType MagFilter { get; set; }
        public D3DTextureFilterType MipFilter { get; set; }

        public int MaxAnisotropy;

        private int u1;
        public string TextureName;
        public float TextureAlpha { get; set; }
        private BinaryXmlTag tmpTag;

        public FloatTontroller? TextureOffsetXTontroller;
        public FloatTontroller? TextureOffsetYTontroller;
        public FloatTontroller? uObj3;
        public FloatTontroller? uObj4;
        public FloatTontroller? uObj5;
        public FloatTontroller? AlphaTontroller { get; private set;  } // FloatTontroller

        public override string ClassName => "TexProperty";

        public TexProperty()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            TextureOp = (D3DTextureOp)reader.ReadInt32(); //colorOpAlphaOp, if == 1, colorOp = selectOp1, alphaOp = Mul, 
            if (reader.ReadByte() != 0)
            {
                TextureName = reader.ReadField(buffer, (reader, buffer) => 
                {
                    return reader.ReadKRString();
                });
            }

            AddressU = (TextureAddressMode)reader.ReadInt32(); // ADDRESSU
            AddressV = (TextureAddressMode)reader.ReadInt32(); // ADDRESSV
            MinFilter = (D3DTextureFilterType)reader.ReadInt32(); // MINFILTER if == 3, MAXANISOTROPY = u9
            MagFilter = (D3DTextureFilterType)reader.ReadInt32(); // MAGFILTER
            MipFilter = (D3DTextureFilterType)reader.ReadInt32(); // MIPFILTER
            MaxAnisotropy = reader.ReadInt32(); // MAXANISOTROPY
            if (reader.ReadByte() != 0)
                TextureOffsetXTontroller = reader.ReadKartObject<FloatTontroller>(buffer);
            if (reader.ReadByte() != 0)
                TextureOffsetYTontroller = reader.ReadKartObject<FloatTontroller>(buffer);
            if (reader.ReadByte() != 0)
                uObj3 = reader.ReadKartObject<FloatTontroller>(buffer);
            if (reader.ReadByte() != 0)
                uObj4 = reader.ReadKartObject<FloatTontroller>(buffer);
            if (reader.ReadByte() != 0)
                uObj5 = reader.ReadKartObject<FloatTontroller>(buffer);
            TextureAlpha = reader.ReadSingle();
            if (reader.ReadByte() != 0)
                AlphaTontroller = reader.ReadKartObject<FloatTontroller>(buffer);
            if (reader.ReadByte() != 0)
            {
                tmpTag = reader.ReadField<BinaryXmlTag>(buffer, (reader, buffer) =>
                {
                    return reader.ReadBinaryXmlTag(Encoding.Unicode);
                });
            }

            // if (uObj3 is not null)
            //     Debug.Print($"tex:{TextureName} uObj3! {uObj3.GetValue(0)}");
            // if (uObj4 is not null)
            //     Debug.Print($"tex:{TextureName} uObj4! {uObj4.GetValue(0)}");
            // if (uObj5 is not null)
            //     Debug.Print($"tex:{TextureName} uObj5! {uObj5.GetValue(0)}");
            // if(tmpTag is not null)
            //     Debug.Print($"tex:{TextureName} tmpTag {tmpTag}");
            // Debug.Print($"tex:{TextureName} u15: {TextureAlpha}");
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {

        }

        public float GetTextureOffsetX(float time)
        {
            return TextureOffsetXTontroller?.GetValue(time) ?? 0;
        }
        
        public float GetTextureOffsetY(float time)
        {
            return TextureOffsetYTontroller?.GetValue(time) ?? 0;
        }

        public float GetAlpha(float time)
        {
            return AlphaTontroller?.GetValue(time) ?? 1.0f;
        }
        
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"<u1>{u1}({u1:x8})</u1>");
            stringBuilder.AppendLine($"<u3>{TextureName}</u3>");
            stringBuilder.AppendLine($"<u4>{AddressU}</u4>");
            stringBuilder.AppendLine($"<u5>{AddressV}</u5>");
            stringBuilder.AppendLine($"<u6>{MinFilter}</u6>");
            stringBuilder.AppendLine($"<u7>{MagFilter}</u7>");
            stringBuilder.AppendLine($"<u8>{MipFilter}</u8>");
            stringBuilder.AppendLine($"<u9>{MaxAnisotropy}</u9>");
            stringBuilder.AppendLine($"<u15>{TextureAlpha}</u15>");
            stringBuilder.AppendLine($"<tmpTag>{tmpTag}</tmpTag>");

            stringBuilder.AppendLine($"<uObj1>{TextureOffsetXTontroller}</uObj1>");
            stringBuilder.AppendLine($"<uObj2>{TextureOffsetYTontroller}</uObj2>");
            stringBuilder.AppendLine($"<uObj3>{uObj3}</uObj3>");
            stringBuilder.AppendLine($"<uObj4>{uObj4}</uObj4>");
            stringBuilder.AppendLine($"<uObj5>{uObj5}</uObj5>");
            return stringBuilder.ToString();
        }
    }
}
