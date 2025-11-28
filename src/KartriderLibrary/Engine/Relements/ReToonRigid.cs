using System.Diagnostics;
using System.Numerics;
using System.Text;
using eP.Text;
using KartCity.Common.Engine.Enums;
using KartCity.Common.IO;
using KartLibrary.Engine.Enums;
using KartLibrary.Engine.Render.DataModel;
using KartLibrary.Engine.Render.Shaders;
using KartLibrary.Game.Engine;
using KartLibrary.Game.Engine.Render;
using KartLibrary.IO;
using Veldrid;
using Vortice.Direct3D11;
using BufferDescription = Veldrid.BufferDescription;

namespace KartLibrary.Engine.Relements
{
    [KartObjectImplement]
    public class ReToonRigid : Relement, IRenderable
    {
        public override string ClassName => "ReToonRigid";

        private int _unknownInt_1;
        private Vector3[] _vertices;
        private Vector3[] _normalVecs;
        private Vector3[] _texCoords;
        private ReToonRigidMeshFace[] _meshFaces;
        
        private RenderMesh[] _renderMeshes;
        private ushort[] _renderIndexes;
        
        // Veldrid device objects
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _modelUniformBuffer;
        private DeviceBuffer _alphaPropInfoBuffer;
        private ResourceSet _shaderResourceSet;
        private DeviceBuffer _toonPropInfoBuffer;
        private Pipeline _pipeline;

        private AlphaPropertyInfo _alphaPropInfo;
        private ToonPropertyInfo _toonPropertyInfo;
        
        public bool RequireUpdate { get; private set; } = true;

        public int UnknownInt1 => _unknownInt_1;

        public Vector3[] Vertices => _vertices;
        public Vector3[] NormalVectors => _normalVecs;
        public Vector3[] TexCoords => _texCoords;
        public ReToonRigidMeshFace[] MeshFaces => _meshFaces;

        public ReToonRigid()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            _unknownInt_1 = reader.ReadInt32();
            (Vector3[] vertices, Vector3[] normalVecs, Vector3[] texCoords, ReToonRigidMeshFace[] meshFaces) meshData = reader.ReadField(buffer, (reader, buffer) =>
            {
                int vertexCount = reader.ReadInt32();
                Vector3[] vertices = new Vector3[vertexCount];
                for(int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = reader.ReadVector3();
                }

                int normalVecCount = reader.ReadInt32();
                Vector3[] normalVecs = new Vector3[normalVecCount];
                for (int i = 0; i < normalVecCount; i++)
                {
                    normalVecs[i] = reader.ReadVector3();
                }

                int texCoordCount = reader.ReadInt32();
                Vector3[] texCoords = new Vector3[texCoordCount];
                for (int i = 0; i < texCoordCount; i++)
                {
                    texCoords[i] = reader.ReadVector3();
                }

                int meshFaceCount = reader.ReadInt32();
                ReToonRigidMeshFace[] meshFaces = new ReToonRigidMeshFace[meshFaceCount];
                for(int i = 0; i < meshFaceCount; i++)
                {
                    meshFaces[i] = new ReToonRigidMeshFace()
                    {
                        TexCoordIndex1 = reader.ReadInt16(),
                        TexCoordIndex2 = reader.ReadInt16(),
                        TexCoordIndex3 = reader.ReadInt16(),
                        NormalVectorIndex1 = reader.ReadInt16(),
                        NormalVectorIndex2 = reader.ReadInt16(),
                        NormalVectorIndex3 = reader.ReadInt16(),
                        VertexIndex1 = reader.ReadInt16(),
                        VertexIndex2 = reader.ReadInt16(),
                        VertexIndex3 = reader.ReadInt16(),
                        Unknown = reader.ReadInt16(),
                    };
                }
                return (vertices, normalVecs, texCoords, meshFaces);
            });
            _vertices = meshData.vertices;
            _normalVecs = meshData.normalVecs;
            _texCoords = meshData.texCoords;
            _meshFaces = meshData.meshFaces;
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

        protected override void ConstructOtherInfo(StringBuilder stringBuilder, int indentLevel)
        {
            base.ConstructOtherInfo(stringBuilder, indentLevel);
            string indentStr = "".PadLeft(indentLevel << 2, ' ');
            stringBuilder.AppendLine($"{indentStr}<ReToonRigidProperties>");
            stringBuilder.ConstructPropertyString(indentLevel + 1, "Vertices", Vertices);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "NormalVectors", NormalVectors);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "TexCoords", TexCoords);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "MeshFaces", MeshFaces);
            stringBuilder.AppendLine($"{indentStr}</ReToonRigidProperties>");
        }
        
        private void createRenderMeshData()
        {
            Dictionary<(int, int, int), short> indexCache = new();
            List<RenderMesh> meshes = [];
            List<short> indexes = [];
            foreach (var mesh in _meshFaces)
            {
                (int, int, int)[] meshIndexPairs = [
                    (mesh.VertexIndex1, mesh.NormalVectorIndex1, mesh.TexCoordIndex1),
                    (mesh.VertexIndex2, mesh.NormalVectorIndex2, mesh.TexCoordIndex2),
                    (mesh.VertexIndex3, mesh.NormalVectorIndex3, mesh.TexCoordIndex3)
                ];
                foreach (var meshIndexPair in meshIndexPairs)
                {
                    short meshIndex = 0;

                    if (indexCache.TryGetValue(meshIndexPair, out var value))
                        meshIndex = value;
                    else
                    {
                        meshIndex = (short)meshes.Count;
                        indexCache.Add(meshIndexPair, meshIndex);
                        meshes.Add(new RenderMesh()
                        {
                            Position = meshIndexPair.Item1 < 0 ? new Vector3(float.NaN) : _vertices[meshIndexPair.Item1],
                            NormalVec = new Vector3(),
                            TexCoord = meshIndexPair.Item3 < 0 ? new Vector3(float.NaN) : _texCoords[meshIndexPair.Item3],
                        });
                    }
                    
                    indexes.Add(meshIndex);
                }
            }

            _renderMeshes = meshes.ToArray();
            _renderIndexes = indexes.Select(x => (ushort)x).ToArray();
        }
        
        public void CreateDeviceObjects(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, DeviceObjectCache localDeviceObjectCache)
        {
            if (MeshFaces is null)
                throw new Exception();
            ResourceFactory factory = graphicsDevice.ResourceFactory;

            createRenderMeshData();
            
            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)_renderMeshes.Length * RenderMesh.SizeOfStruct, BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(_renderIndexes.Length * sizeof(short)), BufferUsage.IndexBuffer));

            commandList.UpdateBuffer(_vertexBuffer, 0, _renderMeshes);
            commandList.UpdateBuffer(_indexBuffer, 0,  _renderIndexes);
            
            _modelUniformBuffer = factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer));
            _alphaPropInfoBuffer = factory.CreateBuffer(new BufferDescription(AlphaPropertyInfo.SizeOfStruct, BufferUsage.UniformBuffer));
            _toonPropInfoBuffer = factory.CreateBuffer(new BufferDescription(ToonPropertyInfo.SizeOfStruct, BufferUsage.UniformBuffer));

            VeldridShader veldridShader = localDeviceObjectCache.GetOrCreateShaders(
                "shader_reToonRigidShader",
                () => ReToonRigidShader.CreateReToonRigidShader(graphicsDevice)
            );
            
            if (veldridShader is not ReToonRigidShader reToonRigidShader)
                throw new Exception($"shader_relement expected ReToonRigidShader, but not.");

            Texture? surfaceTexture;
            Texture? colorMaskingTexture;
            surfaceTexture = localDeviceObjectCache.GetTexture("1");
            if (surfaceTexture is null)
                throw new Exception(
                    $"Cannot found texture. 1" +
                    "If this relement doesn't have surface texture, " +
                    "please ensure that there are a texture called transparency in deviceObjectCache.");
            
            colorMaskingTexture = localDeviceObjectCache.GetTexture("0");
            if (colorMaskingTexture is null)
                throw new Exception(
                    $"Cannot found texture. 0" +
                    "If this relement doesn't have surface texture, " +
                    "please ensure that there are a texture called transparency in deviceObjectCache.");
            
            TextureView surfaceTextureView = factory.CreateTextureView(surfaceTexture);
            TextureView colorMaskingTextureView = factory.CreateTextureView(colorMaskingTexture);
            
            _shaderResourceSet = reToonRigidShader.CreateShaderResourceSet(
                graphicsDevice,
                _modelUniformBuffer,
                _alphaPropInfoBuffer,
                _toonPropInfoBuffer,
                surfaceTextureView,
                graphicsDevice.Aniso4xSampler,
                colorMaskingTextureView,
                graphicsDevice.Aniso4xSampler
            );
            GraphicsPipelineDescription graphicsPipelineDesc = new GraphicsPipelineDescription();
            if (Alpha is not null)
            {
                if (Alpha.UseBlendTest)
                {
                    graphicsPipelineDesc.BlendState = BlendStateDescription.SingleAlphaBlend;
                    graphicsPipelineDesc.BlendState.AttachmentStates[0].SourceColorFactor =
                        BlendFactorUtility.ConvertFromD3DBlendFactor(Alpha.SourceColorFactor);
                    graphicsPipelineDesc.BlendState.AttachmentStates[0].DestinationColorFactor =
                        BlendFactorUtility.ConvertFromD3DBlendFactor(Alpha.DestinationColorFactor);
                }
                else
                {
                    graphicsPipelineDesc.BlendState = BlendStateDescription.SingleDisabled;
                }

                if (Alpha.UseAlphaTest)
                {
                    _alphaPropInfo.AlphaTestEnabled = true;
                    _alphaPropInfo.AlphaTestFunction = Alpha.AlphaFunction;
                    _alphaPropInfo.AlphaTestRef = Alpha.AlphaTestRef;
                }
                else
                {
                    _alphaPropInfo.AlphaTestEnabled = false;
                }
            }
            else
            {
                graphicsPipelineDesc.BlendState = BlendStateDescription.SingleDisabled;
                _alphaPropInfo.AlphaTestEnabled = false;
            }
            
            graphicsPipelineDesc.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.Less);
            
            graphicsPipelineDesc.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Front,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true);

            if (BackFace is not null)
            {
                graphicsPipelineDesc.RasterizerState.CullMode = BackFace.CullMode switch
                {
                    CullMode.Back => FaceCullMode.Back,
                    CullMode.Front => FaceCullMode.Front,
                    CullMode.None => FaceCullMode.None,
                    _ => FaceCullMode.None
                };
            }

            if (ZBuf is not null)
            {
                Debug.Print($"ZBuf: {ZBuf}");
            }

            if (Toon is not null)
            {
                // _toonPropertyInfo.RigidColor = new Vector4(
                //     x: Toon.UnknownColor1.R / 255f,
                //     y: Toon.UnknownColor1.G / 255f,
                //     z: Toon.UnknownColor1.B / 255f,
                //     w: Toon.UnknownColor1.A / 255f
                // );
                Debug.Print($"Toon: {Toon}");
            }

            graphicsPipelineDesc.PrimitiveTopology = PrimitiveTopology.TriangleList;
            graphicsPipelineDesc.ResourceLayouts = new[]
            {
                reToonRigidShader.ShaderResourceLayout,
                sceneContext.SceneResourceLayout
            };

            graphicsPipelineDesc.ShaderSet = reToonRigidShader.ShaderSetDesc;
            graphicsPipelineDesc.Outputs = sceneContext.MainSceneFramebuffer.OutputDescription;

            _pipeline = factory.CreateGraphicsPipeline(graphicsPipelineDesc);
            
            graphicsDevice.UpdateBuffer(_alphaPropInfoBuffer, 0, _alphaPropInfo);
            graphicsDevice.UpdateBuffer(_toonPropInfoBuffer, 0, _toonPropertyInfo);
        }

        public void UpdatePerFrameResources(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, DeviceObjectCache localDeviceObjectCache)
        {
            if(RequireUpdate)
                commandList.UpdateBuffer(_modelUniformBuffer, 0, CurrentModelMatrix);
            RequireUpdate = false;
        }

        protected override void UpdateRelement(ref Matrix4x4 modelMatrix, ITimeSource timeSource, bool updated)
        {
            RequireUpdate = updated;
            base.UpdateRelement(ref modelMatrix, timeSource, updated);
        }

        public void Render(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
        {
            if (!(VisTontroller?.GetIsVisible(sceneContext.TimeSource.GetTimeStamp()) ?? true))
                return;
            // commandList.PushDebugGroup($"ReTriStrip: {Name} ");
            commandList.SetPipeline(_pipeline);
            commandList.SetGraphicsResourceSet(0, _shaderResourceSet);
            commandList.SetGraphicsResourceSet(1, sceneContext.SceneResourceSet);
            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            commandList.DrawIndexed(
                indexCount: (uint)_renderIndexes.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
            // commandList.PopDebugGroup();
        }

        public double GetDistance(Camera camera, bool useFar)
        {
            return !(Alpha?.UseBlendTest ?? false)
                ? float.MaxValue
                : Bounding.GetMinDistance(camera.CameraPosition, ref CurrentModelMatrixRef);
        }
        
        public bool IsTranslucencyObject() => Alpha?.UseBlendTest ?? false;
        
        public void DestroyAllDeviceObjects()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _modelUniformBuffer.Dispose();
            _alphaPropInfoBuffer.Dispose();
            _shaderResourceSet.Dispose();
            _toonPropInfoBuffer.Dispose();
            _pipeline.Dispose();
        }
    }

    public struct ReToonRigidMeshFace
    {
        public int TexCoordIndex1;
        public int TexCoordIndex2;
        public int TexCoordIndex3;
        
        public int NormalVectorIndex1;
        public int NormalVectorIndex2;
        public int NormalVectorIndex3;

        public int VertexIndex1;
        public int VertexIndex2;
        public int VertexIndex3;

        public int Unknown;

        public override string ToString()
        {
            return $"<Face>" +
                 $" v:{VertexIndex1},{VertexIndex2},{VertexIndex3}" +
                 $" n:{NormalVectorIndex1},{NormalVectorIndex2},{NormalVectorIndex1}" +
                 $" t:{TexCoordIndex1},{TexCoordIndex2},{TexCoordIndex3}" +
                 $" un{Unknown}</Face>";
        }
    }
}
