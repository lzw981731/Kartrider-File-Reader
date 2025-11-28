using System.Numerics;
using KartLibrary.Engine.Render.DataModel;
using KartLibrary.Engine.Render.Shaders;
using KartLibrary.Game.Engine.Render;
using Veldrid;
using BufferDescription = Veldrid.BufferDescription;

namespace KartLibrary.Engine.Render.Renderables;

public class FramebufferRenderable: IRenderable
{
    private RenderVertex[] _renderVertices;
    private ushort[] _renderIndexes;
    
    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _indexBuffer;
    private DeviceBuffer _framebufferInfoBuffer;

    private FramebufferInfo _framebufferInfo;

    private Texture _framebufferTexture;
    private Texture _framebufferDepthTexture;
    private TextureView _textureView;
    private TextureView _depthTextureView;
    private ResourceSet _resourceSet;
    private Pipeline _pipeline;

    private FramebufferShader _framebufferShader;
    
    public void Dispose()
    {
        DestroyAllDeviceObjects();
    }

    public void CreateDeviceObjects(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext,
        DeviceObjectCache localDeviceObjectCache)
    {
        ResourceFactory factory = graphicsDevice.ResourceFactory;

        _renderVertices =
        [
            new RenderVertex() { Position = new Vector3(-1.0f, 1.0f, 0.0f), TextureCoord = new Vector2(0.0f, 0.0f) },
            new RenderVertex() { Position = new Vector3(1.0f, 1.0f, 0.0f), TextureCoord = new Vector2(1.0f, 0.0f) },
            new RenderVertex() { Position = new Vector3(-1.0f, -1.0f, 0.0f), TextureCoord = new Vector2(0.0f, 1.0f) },
            new RenderVertex() { Position = new Vector3(1.0f, -1.0f, 0.0f), TextureCoord = new Vector2(1.0f, 1.0f) },
        ];

        _framebufferInfo = new FramebufferInfo()
        {
            SampleCount = 1
        };
        
        VeldridShader veldridShader = localDeviceObjectCache.GetOrCreateShaders(
            "shader_framebuffer",
            () => FramebufferShader.CreateReToonRigidShader(graphicsDevice)
        );
            
        if (veldridShader is not FramebufferShader framebufferShader)
            throw new Exception($"shader_relement expected RelementShader, but not.");

        _framebufferShader = framebufferShader;
        
        _renderIndexes = [(ushort)0u, (ushort)1u, (ushort)2u, (ushort)3u];

        _vertexBuffer =
            factory.CreateBuffer(new BufferDescription((uint)(RenderVertex.SizeOfStruct * _renderVertices.Length),
                BufferUsage.VertexBuffer));
        _indexBuffer =
            factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * _renderIndexes.Length),
                BufferUsage.IndexBuffer));

        _framebufferInfoBuffer =
            factory.CreateBuffer(new BufferDescription(FramebufferInfo.SizeOfStruct, BufferUsage.UniformBuffer));
        
        graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _renderVertices);
        graphicsDevice.UpdateBuffer(_indexBuffer, 0, _renderIndexes);
        graphicsDevice.UpdateBuffer(_framebufferInfoBuffer, 0, _framebufferInfo);
        updateTextureView(graphicsDevice, commandList, sceneContext);
        updateResourceSet(graphicsDevice);
    }

    public void UpdatePerFrameResources(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext,
        DeviceObjectCache localDeviceObjectCache)
    {
        Texture colorTarget = sceneContext.MainSceneFramebuffer.ColorTargets[0].Target;
        Texture depthTarget = sceneContext.MainSceneFramebuffer.DepthTarget?.Target;
        ResourceFactory factory = graphicsDevice.ResourceFactory;
        if (_pipeline is null
            || _framebufferTexture.Width != colorTarget.Width
            || _framebufferTexture.Height != colorTarget.Height
            || _framebufferTexture.ArrayLayers != colorTarget.ArrayLayers
            || _framebufferTexture.MipLevels != colorTarget.MipLevels
           )
        {
            updateTextureView(graphicsDevice, commandList, sceneContext);
            updateResourceSet(graphicsDevice);
            updatePipeline(graphicsDevice);    
        }
        // if(colorTarget.SampleCount != TextureSampleCount.Count1)
        //     commandList.ResolveTexture(colorTarget, _framebufferTexture);
        // else
        //     commandList.CopyTexture(colorTarget, _framebufferTexture);
        
        commandList.CopyTexture(colorTarget, _framebufferTexture);
        
        if(depthTarget is not null)
            if(depthTarget.SampleCount != TextureSampleCount.Count1)
                commandList.ResolveTexture(depthTarget, _framebufferDepthTexture);
            else
                commandList.CopyTexture(depthTarget, _framebufferDepthTexture);
    }

    public void Render(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _resourceSet);
        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        commandList.DrawIndexed(
            indexCount: (uint)_renderIndexes.Length,
            instanceCount: 1,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0);
    }

    public double GetDistance(Camera camera, bool useFar)
    {
        return float.MinValue;
    }

    private void updateTextureView(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        _textureView?.Dispose();
        _framebufferTexture?.Dispose();
        
        ResourceFactory factory = graphicsDevice.ResourceFactory;
        Texture colorTarget = sceneContext.MainSceneFramebuffer.ColorTargets[0].Target;
        Texture depthTarget = sceneContext.MainSceneFramebuffer.DepthTarget?.Target;
        _framebufferTexture = factory.CreateTexture(new TextureDescription()
        {
            Width = colorTarget.Width,
            Height = colorTarget.Height,
            ArrayLayers = colorTarget.ArrayLayers,
            Depth = colorTarget.Depth,
            Format = colorTarget.Format,
            MipLevels = colorTarget.MipLevels,
            SampleCount = colorTarget.SampleCount,
            Type = TextureType.Texture2D,
            Usage = TextureUsage.Sampled
        });

        int sampleCount = 1 << (int)colorTarget.SampleCount;
        if (sampleCount != _framebufferInfo.SampleCount)
        {
            _framebufferInfo.SampleCount = sampleCount;
            graphicsDevice.UpdateBuffer(_framebufferInfoBuffer, 0, _framebufferInfo);
        }
        
        if (depthTarget is not null)
        {
            _framebufferDepthTexture = factory.CreateTexture(new TextureDescription()
            {
                Width = depthTarget.Width,
                Height = depthTarget.Height,
                ArrayLayers = depthTarget.ArrayLayers,
                Depth = 1,
                Format = depthTarget.Format,
                MipLevels = depthTarget.MipLevels,
                SampleCount = TextureSampleCount.Count1,
                Type = TextureType.Texture2D,
                Usage = TextureUsage.Sampled,
            });
            _depthTextureView = factory.CreateTextureView(_framebufferDepthTexture);
        }
        
        // _textureView = factory.CreateTextureView(_framebufferTexture);
    }

    private void updateResourceSet(GraphicsDevice graphicsDevice)
    {
        _resourceSet?.Dispose();
        
        _resourceSet = _framebufferShader.CreateShaderResourceSet(
            graphicsDevice,
            _framebufferTexture,
            graphicsDevice.PointSampler,
            _depthTextureView,
            graphicsDevice.PointSampler,
            _framebufferInfoBuffer
        );
    }
    
    public bool IsTranslucencyObject() => false;
    
    private void updatePipeline(GraphicsDevice graphicsDevice)
    {
        ResourceFactory factory = graphicsDevice.ResourceFactory;
        GraphicsPipelineDescription graphicsPipelineDesc = new GraphicsPipelineDescription();
            
        graphicsPipelineDesc.BlendState = BlendStateDescription.SingleDisabled;
        
        graphicsPipelineDesc.DepthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            comparisonKind: ComparisonKind.Less);
            
        graphicsPipelineDesc.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.Back,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
            depthClipEnabled: true,
            scissorTestEnabled: false);

        graphicsPipelineDesc.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        graphicsPipelineDesc.ResourceLayouts = new[]
        {
            _framebufferShader.ShaderResourceLayout
        };

        graphicsPipelineDesc.ShaderSet = _framebufferShader.ShaderSetDesc;
        graphicsPipelineDesc.Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(graphicsPipelineDesc);
    }
    
    public void DestroyAllDeviceObjects()
    {
        
    }
}