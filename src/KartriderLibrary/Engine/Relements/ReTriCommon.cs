using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using KartCity.Common.Engine;
using KartCity.Common.Engine.Enums;
using KartCity.Common.Engine.Extension;
using KartLibrary.Engine.Enums;
using KartLibrary.Engine.Properities;
using KartLibrary.Engine.Render.DataModel;
using KartLibrary.Engine.Render.Shaders;
using KartLibrary.Game.Engine;
using KartLibrary.Game.Engine.Render;
using Veldrid;
using Vortice.Direct3D11;
using BufferDescription = Veldrid.BufferDescription;
using MapMode = Veldrid.MapMode;
using SamplerDescription = Veldrid.SamplerDescription;

namespace KartLibrary.Engine.Relements;

/// <summary>
/// <see cref="ReTriCommon"/> is not exist in KartRider class.
/// </summary>
public abstract class ReTriCommon: Relement, IRenderable
{
    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _indexBuffer;
    private DeviceBuffer _modelUniformBuffer;
    private DeviceBuffer _texPropInfoBuffer;
    private DeviceBuffer _alphaPropInfoBuffer;
    private ResourceSet _shaderResourceSet;
    private Pipeline _pipeline;

    private AlphaPropertyInfo _alphaPropInfo;
    private TexPropertyInfo _texPropertyInfo;

    private Lazy<BoundingBox> _renderBoundingBox;
    
    
    
    public abstract VertexData Vertex { get; }

    public bool RequireUpdate { get; private set; } = true;

    private PrimitiveTopology _primitiveTopology; 
    
    protected ReTriCommon(PrimitiveTopology primitiveTopology)
    {
        _alphaPropInfo = new AlphaPropertyInfo();
        _texPropertyInfo = new TexPropertyInfo();
        _primitiveTopology = primitiveTopology;
        _renderBoundingBox = new Lazy<BoundingBox>(() => Vertex.GetRenderBoundingBox());
    }

    #region For veldrid render methods
    public virtual void CreateDeviceObjects(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, DeviceObjectCache localDeviceObjectCache)
    {
        if (Vertex is null || Vertex.Vertices is null || Vertex.Indexes is null)
            throw new Exception();
        ResourceFactory factory = graphicsDevice.ResourceFactory;

        GraphicsPipelineDescription graphicsPipelineDesc = new GraphicsPipelineDescription();
        
        graphicsPipelineDesc.DepthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            comparisonKind: ComparisonKind.Less);

        graphicsPipelineDesc.RasterizerState.FillMode = PolygonFillMode.Solid;
        
        graphicsPipelineDesc.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.None,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
            depthClipEnabled: true,
            scissorTestEnabled: true);

        _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Vertex.Vertices.Count() * RenderVertex.SizeOfStruct, BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(Vertex.Indexes.Length * sizeof(short)), BufferUsage.IndexBuffer));
        _modelUniformBuffer = factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _alphaPropInfoBuffer = factory.CreateBuffer(new BufferDescription(AlphaPropertyInfo.SizeOfStruct, BufferUsage.UniformBuffer));
        _texPropInfoBuffer = factory.CreateBuffer(new BufferDescription(TexPropertyInfo.SizeOfStruct, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
        RenderVertex[] vertices = new RenderVertex[Vertex.Vertices.Length];
        for (int i = 0; i < Vertex.Vertices.Length; i++)
        {
            vertices[i].Position = Vertex.Vertices[i];
            vertices[i].TextureCoord = Vertex.TextureUVs.Length > 0 ? Vertex.TextureUVs[i, 0] : Vector2.Zero;
        }
        
        VeldridShader veldridShader = localDeviceObjectCache.GetOrCreateShaders(
            "shader_relement",
            () => RelementShader.CreateRelementShader(graphicsDevice)
        );
        
        if (veldridShader is not RelementShader relementShader)
            throw new Exception($"shader_relement expected RelementShader, but not.");

        Texture? surfaceTexture;
        SamplerDescription samplerDesc = new SamplerDescription();
        
        TexProperty? inheritTex = GetTexPropertyOrInHerit();
        
        if (inheritTex is null)
        {
            surfaceTexture = localDeviceObjectCache.GetTexture("transparency");
        }
        else
        {
            surfaceTexture = localDeviceObjectCache.GetTexture(inheritTex.TextureName);
            samplerDesc.AddressModeU = inheritTex.AddressU.ToVeldridAddressMode();
            samplerDesc.AddressModeV = inheritTex.AddressV.ToVeldridAddressMode();
            samplerDesc.Filter = VeldridExtension.ToVeldridFilter(
                inheritTex.MinFilter,
                inheritTex.MagFilter,
                inheritTex.MipFilter
            );
            samplerDesc.LodBias = 0;
            // samplerDesc.Filter = SamplerFilter.MinLinear_MagLinear_MipLinear;
            if (Alpha?.UseAlphaTest ?? false)
            {
                // samplerDesc.AddressModeU = SamplerAddressMode.Border;
                // samplerDesc.AddressModeV = SamplerAddressMode.Border;
                samplerDesc.BorderColor = SamplerBorderColor.TransparentBlack;
            }
            samplerDesc.MaximumAnisotropy = (uint)16;
            samplerDesc.MaximumLod = (uint)(surfaceTexture?.MipLevels ?? 1) - 1;
            samplerDesc.MinimumLod = (uint)0;
            _texPropertyInfo.TexAlpha = inheritTex.TextureAlpha;
            _texPropertyInfo.TexU3 = inheritTex.TextureOffsetXTontroller is not null ? 1 : 0;
        }
        
        if (surfaceTexture is null)
        {
            surfaceTexture = localDeviceObjectCache.GetTexture("transparency");
            Debug.Print($"WARNING: {inheritTex?.TextureName ?? "transparency"} texture can't be found.");
        }
        if (surfaceTexture is null)
        {
            throw new Exception(
                $"Cannot found texture. {inheritTex?.TextureName}" +
                "If this relement doesn't have surface texture, " +
                "please ensure that there are a texture called transparency in deviceObjectCache.");
        }
        
        TextureView surfaceTextureView = factory.CreateTextureView(new TextureViewDescription(surfaceTexture, 0, surfaceTexture.MipLevels, 0, 1));
        Sampler sampler = factory.CreateSampler(samplerDesc);
        
        _shaderResourceSet = relementShader.CreateShaderResourceSet(
            graphicsDevice,
            _modelUniformBuffer,
            _texPropInfoBuffer,
            _alphaPropInfoBuffer,
            surfaceTextureView,
            sampler
        );
        
        graphicsPipelineDesc.BlendState = BlendStateDescription.SingleDisabled;
        if (Alpha is not null)
        {
            if (Alpha.UseAlphaTest)
            {
                graphicsPipelineDesc.BlendState.AlphaToCoverageEnabled = true;
                _alphaPropInfo.AlphaTestEnabled = true;
                _alphaPropInfo.AlphaTestFunction = Alpha.AlphaFunction;
                _alphaPropInfo.AlphaTestRef = Alpha.AlphaTestRef;
            }
            else
            {
                _alphaPropInfo.AlphaTestEnabled = false;
            }
            
            if (Alpha.UseBlendTest)
            {
                graphicsPipelineDesc.BlendState.AlphaToCoverageEnabled = true;
                graphicsPipelineDesc.BlendState = BlendStateDescription.SingleAlphaBlend;
                
                graphicsPipelineDesc.BlendState.AttachmentStates[0].SourceColorFactor =
                    BlendFactorUtility.ConvertFromD3DBlendFactor(Alpha.SourceColorFactor);
                graphicsPipelineDesc.BlendState.AttachmentStates[0].DestinationColorFactor =
                    BlendFactorUtility.ConvertFromD3DBlendFactor(Alpha.DestinationColorFactor);
            }
        }
        else
        {
            _alphaPropInfo.AlphaTestEnabled = false;
        }

        if (BackFace is not null)
        {
            graphicsPipelineDesc.RasterizerState.CullMode = BackFace.CullMode switch
            {
                CullMode.Back => FaceCullMode.Back,
                CullMode.Front => FaceCullMode.Front,
                CullMode.None => FaceCullMode.None,
                _ => FaceCullMode.Front
            };
        }
        else
        {
            graphicsPipelineDesc.RasterizerState.CullMode = FaceCullMode.Front;
        }

        if (Wire is not null)
        {
            graphicsPipelineDesc.RasterizerState.FillMode = Wire.FillMode switch
            {
                D3DFillMode.Solid => PolygonFillMode.Solid,
                D3DFillMode.Wireframe => PolygonFillMode.Wireframe,
                D3DFillMode.FillPoint => PolygonFillMode.Solid,
                _ => PolygonFillMode.Solid
            };
        }
        
        if (ZBuf is not null)
        {
            graphicsPipelineDesc.DepthStencilState.DepthComparison = ZBuf.DepthFunction switch
            {
                ComparisonFunction.Never => ComparisonKind.Never,
                ComparisonFunction.Less => ComparisonKind.Less,
                ComparisonFunction.Equal => ComparisonKind.Equal,
                ComparisonFunction.LessEqual => ComparisonKind.LessEqual,
                ComparisonFunction.Greater => ComparisonKind.Greater,
                ComparisonFunction.NotEqual => ComparisonKind.NotEqual,
                ComparisonFunction.GreaterEqual => ComparisonKind.GreaterEqual,
                ComparisonFunction.Always => ComparisonKind.Always,
                _ => ComparisonKind.Never
            };
            graphicsPipelineDesc.DepthStencilState.DepthWriteEnabled = ZBuf.EnableDepthWrite;
        }

        graphicsPipelineDesc.PrimitiveTopology = _primitiveTopology;
        
        graphicsPipelineDesc.ResourceLayouts = new[]
        {
            relementShader.ShaderResourceLayout,
            sceneContext.SceneResourceLayout
        };

        graphicsPipelineDesc.ShaderSet = relementShader.ShaderSetDesc;
        graphicsPipelineDesc.Outputs = sceneContext.MainSceneFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(graphicsPipelineDesc);
        
        graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);
        graphicsDevice.UpdateBuffer(_indexBuffer, 0,  Vertex.Indexes);
        graphicsDevice.UpdateBuffer(_alphaPropInfoBuffer, 0, _alphaPropInfo);
        graphicsDevice.UpdateBuffer(_texPropInfoBuffer, 0, _texPropertyInfo);
    }

    public virtual unsafe void UpdatePerFrameResources(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, DeviceObjectCache localDeviceObjectCache)
    {
        if (RequireUpdate)
        {
            MappedResource mappedResource = graphicsDevice.Map(_modelUniformBuffer, MapMode.Write);
            fixed (Matrix4x4* ptr = &CurrentModelMatrixRef)
            {
                byte* srcPtr = (byte*)ptr;
                byte* destPtr = (byte*)mappedResource.Data;
            
                Vector256<byte> src1 = Avx.LoadVector256(srcPtr);
                Vector256<byte> src2 = Avx.LoadVector256(srcPtr + 32);
                Avx.Store(destPtr, src1);
                Avx.Store(destPtr + 32, src2);
            }
            graphicsDevice.Unmap(_modelUniformBuffer);    
            // graphicsDevice.UpdateBuffer(_modelUniformBuffer, 0, ref CurrentModelMatrixRef);
        }
        RequireUpdate = false;

        TexProperty? inheritTex = GetTexPropertyOrInHerit();
        
        if ((inheritTex?.TextureOffsetXTontroller is not null) || inheritTex?.TextureOffsetYTontroller is not null || inheritTex?.AlphaTontroller is not null)
        {
            float time = (float)sceneContext.TimeSource.GetTimeStamp();
            _texPropertyInfo.TexOffsetX = inheritTex.GetTextureOffsetX(time);
            _texPropertyInfo.TexOffsetY = inheritTex.GetTextureOffsetY(time);
            _texPropertyInfo.TexAlpha = inheritTex.GetAlpha(time) * inheritTex.TextureAlpha;
            graphicsDevice.UpdateBuffer(_texPropInfoBuffer, 0, ref _texPropertyInfo);
        }
    }

    protected override void UpdateRelement(ref Matrix4x4 modelMatrix, ITimeSource timeSource, bool updated)
    {
        base.UpdateRelement(ref modelMatrix, timeSource, updated);
        RequireUpdate |= updated;
    }

    public virtual double GetDistance(Camera camera, bool useFar)
    {
        double maxZ, minZ;
        if (useFar)
        {
            _renderBoundingBox.Value.GetMinMaxZ(ref camera.FarObjectProjectionViewMatrixRef, ref CurrentModelMatrixRef, out minZ, out maxZ);
        }
        else
        {
            _renderBoundingBox.Value.GetMinMaxZ(ref camera.ProjectionViewMatrixRef, ref CurrentModelMatrixRef, out minZ, out maxZ);
        }
        return maxZ < (-0.01f) ? double.NaN : (Alpha?.UseBlendTest ?? false) ? maxZ : double.MaxValue;
    }

    public bool IsTranslucencyObject() => (Alpha?.UseBlendTest ?? false);
    

    public virtual void Render(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        // commandList.PushDebugGroup($"ReTriStrip: {Name} ");
        if (!(VisTontroller?.GetIsVisible(sceneContext.TimeSource.GetTimeStamp()) ?? true))
            return;
        
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _shaderResourceSet);
        if(sceneContext.UseFarObjectResourceSet)
            commandList.SetGraphicsResourceSet(1, sceneContext.FarObjectSceneResourceSet);
        else
            commandList.SetGraphicsResourceSet(1, sceneContext.SceneResourceSet);
        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        commandList.DrawIndexed(
            indexCount: (uint)Vertex.Indexes.Length,
            instanceCount: 1,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0);
        // commandList.PopDebugGroup();
    }
    
    public virtual void DestroyAllDeviceObjects()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _modelUniformBuffer.Dispose();
        _texPropInfoBuffer.Dispose();
        _alphaPropInfoBuffer.Dispose();
        _shaderResourceSet.Dispose();
        _pipeline.Dispose();
    }
    #endregion
}