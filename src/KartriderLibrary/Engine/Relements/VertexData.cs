using System.Diagnostics;
using System.Numerics;
using System.Text;
using eP.Text;
using KartCity.Common.Engine;
using KartCity.Common.IO;
using KartLibrary.Game.Engine.Tontrollers;
using KartLibrary.IO;

namespace KartLibrary.Engine.Relements
{
    public class VertexData
    {
        public Vector3[]? Vertices;
        public Vector3[]? Unknown1;
        public float[]? Unknown2;
        public short TexCoordPerVertex;
        public Vector2[,]? TextureUVs;
        public short[]? Indexes;
        public MorphTontroller? MorphTontroller { get; set; }

        public byte u9;

        public static VertexData Deserialize(BinaryReader reader, KartObjectBuffer? buffer)
        {
            VertexData output = new VertexData();
            output.DecodeObject(reader, buffer);
            return output;
        }

        public void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            int u2 = reader.ReadInt16();
            Vertices = new Vector3[u2];
            if (reader.ReadByte() != 0)
            {
                for (int i = 0; i < u2; i++)
                {
                    Vertices[i] = reader.ReadVector3();
                }
            }
            Unknown1 = new Vector3[u2];
            if (reader.ReadByte() != 0)
            {
                for (int i = 0; i < u2; i++)
                {
                    Unknown1[i] = reader.ReadVector3();
                }
            }
            if (reader.ReadByte() != 0)
            {
                Unknown2 = new float[u2];
                for (int i = 0; i < u2; i++)
                {
                    Unknown2[i] = reader.ReadSingle();
                }
            }
            TexCoordPerVertex = reader.ReadInt16();
            TextureUVs = new Vector2[u2, TexCoordPerVertex];
            for (int i = 0; i < u2; i++)
            {
                for (int j = 0; j < TexCoordPerVertex; j++)
                {
                    TextureUVs[i, j] = reader.ReadVector2();
                }
            }

            if (TexCoordPerVertex > 1)
            {
                Debug.Print($"TexCoordPerVertex > 1: {TexCoordPerVertex}");    
            }

            if (reader.ReadBoolean())
            {
                MorphTontroller = reader.ReadKartObject<MorphTontroller>(buffer);
            }
            
            // MorphTontroller
            short indexCount = reader.ReadInt16();
            Indexes = new short[indexCount];
            for (int i = 0; i < indexCount; i++)
                Indexes[i] = reader.ReadInt16();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<VertexData>");
            stringBuilder.ConstructPropertyString(1, "Vertices", Vertices);
            stringBuilder.ConstructPropertyString(1, "Unknown1", Unknown1);
            stringBuilder.ConstructPropertyString(1, "Unknown2", Unknown2);
            stringBuilder.ConstructPropertyString(1, "TexCoordPerVertex", TexCoordPerVertex);
            stringBuilder.ConstructPropertyString(1, "TextureUVs", TextureUVs);
            stringBuilder.ConstructPropertyString(1, "Indexes", Indexes);
            stringBuilder.ConstructPropertyString(1, "u9", u9);
            stringBuilder.AppendLine("</VertexData>");
            return stringBuilder.ToString();
        }

        public BoundingBox GetRenderBoundingBox()
        {
            if (Vertices is null)
                return new BoundingBox();
            Vector3 minPos = new Vector3(float.MaxValue);
            Vector3 maxPos = new Vector3(float.MinValue);
            foreach (var vertex in Vertices)
            {
                minPos = Vector3.Min(minPos, vertex);
                maxPos = Vector3.Max(maxPos, vertex);
            }

            return new BoundingBox(minPos, maxPos);
        }
    }
}
