using System.Numerics;
using KartLibrary.Engine.Render.DataModel;
using KartLibrary.Engine.Render.Shaders;
using KartLibrary.Game.Engine.Render;
using Veldrid;

namespace KartLibrary.Engine.Render.Renderables;

public abstract class GeneralRenderable: IRenderable
{
    private RenderGeneralVertex[] _renderVertices;
    private uint[] _renderIndexes;
    private Matrix4x4 _modelMat = Matrix4x4.Identity;
    
    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _indexBuffer;
    private DeviceBuffer _modelMatBuffer;
    private GeneralShader _generalShader;
    
    private ResourceSet _shaderResourceSet;
    private Texture _texture;
    private TextureView _textureView;
    private Pipeline _pipeline;

    private bool _modelMatModified = true;
    public RenderGeneralVertex[] RenderVertices => _renderVertices;
    public uint[] RenderIndexes => _renderIndexes;
    public Matrix4x4 ModelMatrix => _modelMat;

    protected ref Matrix4x4 ModelMatrixRef => ref _modelMat;
    
    public void CreateDeviceObjects(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext,
        DeviceObjectCache localDeviceObjectCache)
    {
        ResourceFactory factory = graphicsDevice.ResourceFactory;

        _renderVertices = CreateRenderVertices();
        _renderIndexes = Enumerable.Range(0, _renderVertices.Length).Select(x => (uint)x).ToArray();

        _vertexBuffer =
            factory.CreateBuffer(new BufferDescription(
                (uint)(RenderGeneralVertex.SizeOfStruct * _renderVertices.Length), BufferUsage.VertexBuffer));
        
        _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * _renderIndexes.Length),
            BufferUsage.IndexBuffer));

        _modelMatBuffer = factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer));
        
        VeldridShader shader = localDeviceObjectCache.GetOrCreateShaders(
            "shader_general",
            () => GeneralShader.CreateGeneralShader(graphicsDevice)
        );
        if (shader is not GeneralShader generalShader)
            throw new Exception("Not general shader.");
        _generalShader = generalShader;

        ShaderSetDescription shaderSetDescription = generalShader.GetShaderDescWithConst(false, true);

        _texture = factory.CreateTexture(new TextureDescription()
        {
            Width = 1,
            Height = 1,
            MipLevels = 1,
            Depth = 1,
            Format = PixelFormat.R8_G8_B8_A8_UNorm,
            ArrayLayers = 1,
            Type = TextureType.Texture2D,
            Usage = TextureUsage.Sampled
        });
        _textureView = factory.CreateTextureView(_texture);

        _shaderResourceSet = generalShader.CreateShaderResourceSet(
            graphicsDevice,
            _modelMatBuffer,
            _textureView,
            sceneContext.DefaultSampler
        );
        
        GraphicsPipelineDescription graphicsPipelineDesc = new GraphicsPipelineDescription();
            
        graphicsPipelineDesc.BlendState = BlendStateDescription.SingleAlphaBlend;
        
        graphicsPipelineDesc.DepthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            comparisonKind: ComparisonKind.Less);
            
        graphicsPipelineDesc.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.None,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
            depthClipEnabled: true,
            scissorTestEnabled: true);

        graphicsPipelineDesc.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        graphicsPipelineDesc.ResourceLayouts = new[]
        {
            _generalShader.ShaderResourceLayout,
            sceneContext.SceneResourceLayout
        };

        graphicsPipelineDesc.ShaderSet = shaderSetDescription;
        graphicsPipelineDesc.Outputs = sceneContext.MainSceneFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(graphicsPipelineDesc);
        
        graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _renderVertices);
        graphicsDevice.UpdateBuffer(_indexBuffer, 0, _renderIndexes);
    }

    public void UpdatePerFrameResources(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext,
        DeviceObjectCache localDeviceObjectCache)
    {
        updateModelMatBuffer(graphicsDevice);
    }

    public void Render(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _shaderResourceSet);
        commandList.SetGraphicsResourceSet(1, sceneContext.SceneResourceSet);
        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed((uint)_renderIndexes.Length, 1, 0, 0, 0);
    }

    public virtual double GetDistance(Camera camera, bool useFar)
    {
        return float.MinValue;
        
    }
    
    public virtual bool IsTranslucencyObject() => false;

    public void DestroyAllDeviceObjects()
    {
        
    }
    
    public void Dispose()
    {
        
    }

    public void UpdateModelMat(Matrix4x4 mat)
    {
        _modelMat = mat;
        _modelMatModified = true;
    }

    protected abstract RenderGeneralVertex[] CreateRenderVertices();


    private void updateModelMatBuffer(GraphicsDevice graphicsDevice)
    {
        if (_modelMatModified)
        {
            graphicsDevice.UpdateBuffer(_modelMatBuffer, 0, _modelMat);
            _modelMatModified = false;    
        }
        
    }
}