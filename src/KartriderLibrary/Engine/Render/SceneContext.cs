using System.Diagnostics;
using Veldrid;

namespace KartLibrary.Game.Engine.Render
{
    // Reference to Veldrid/NeoDemo/SceneContext.cs.
    // https://github.com/veldrid/veldrid/blob/master/src/NeoDemo/SceneContext.cs
    public class SceneContext
    {
        private bool _isCameraChanged = true;
        private bool _useFarObjRes = false;
        private ResourceSet _sceneResourceSet;
        private ResourceSet _farObjectSceneResourceSet;
        
        public Camera SceneCamera { get; private set; }
        public FrameTimeSource TimeSource { get; private set; } = new FrameTimeSource();
        
        public Sampler DefaultSampler { get; private set; }
        public DeviceBuffer ViewMatrixBuffer { get; private set; }
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        
        public DeviceBuffer FarProjectionMatrixBuffer { get; private set; }
        
        public DeviceObjectCache SceneObjectCache { get; private set; }
        
        public ResourceLayout SceneResourceLayout { get; private set; }
        public ResourceSet SceneResourceSet => _sceneResourceSet;

        public ResourceSet FarObjectSceneResourceSet => _farObjectSceneResourceSet; 
        public Framebuffer MainSceneFramebuffer { get; private set; }

        public bool UseFarObjectResourceSet => _useFarObjRes;

        public int SceneWidth { get; private set; }
        public int SceneHeight { get; private set; }

        public int DebugNumViewMatUpdates { get; private set; } = 0;
        public int DebugNumProjectMatUpdates { get; private set; } = 0;
        
        public void CreateDeviceObjects(GraphicsDevice graphicsDevice, int width = 1920, int height = 1080)
        {
            ResourceFactory factory = graphicsDevice.ResourceFactory;
            DefaultSampler = factory.CreateSampler(new SamplerDescription()
            {
                AddressModeU = SamplerAddressMode.Wrap,
                AddressModeV = SamplerAddressMode.Wrap,
                AddressModeW = SamplerAddressMode.Wrap,
                Filter = SamplerFilter.MinLinear_MagLinear_MipPoint,
            });
            ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer));
            FarProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer));
            SceneResourceLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("ProjectionInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    ));
            _sceneResourceSet = factory.CreateResourceSet(new ResourceSetDescription(SceneResourceLayout, ViewMatrixBuffer, ProjectionMatrixBuffer));
            _farObjectSceneResourceSet = factory.CreateResourceSet(new ResourceSetDescription(SceneResourceLayout, ViewMatrixBuffer, FarProjectionMatrixBuffer));
            SetMainSceneSize(graphicsDevice, width, height);
        }

        public void DestroyDeviceObjects()
        {
            ViewMatrixBuffer?.Dispose();
            ProjectionMatrixBuffer?.Dispose();
        }

        public void UpdateCameraBuffers(CommandList commandList)
        {
            bool viewMatModified = SceneCamera.IsViewModified;
            bool projMatModified = SceneCamera.IsProjectionModified;
            SceneCamera.UpdateMatrix();
            if (_isCameraChanged || viewMatModified)
            {
                commandList.UpdateBuffer(ViewMatrixBuffer, 0, ref SceneCamera.ViewMatrixRef);
                DebugNumViewMatUpdates++;
            }
            if (_isCameraChanged || projMatModified)
            {
                commandList.UpdateBuffer(ProjectionMatrixBuffer, 0, ref SceneCamera.ProjectionMatrixRef);
                commandList.UpdateBuffer(FarProjectionMatrixBuffer, 0, SceneCamera.FarObjectProjectionViewMatrix);
                DebugNumProjectMatUpdates++;
            }

            _isCameraChanged = false;
        }

        public void SetScene(KartEngineScene scene)
        {
            SceneCamera = scene.Camera;
            SceneCamera.UpdateSceneSize(SceneWidth, SceneHeight);
            _isCameraChanged = true;
        }

        public void SetMainSceneSize(GraphicsDevice graphicsDevice, int width, int height)
        {
            MainSceneFramebuffer?.Dispose();
            MainSceneFramebuffer = createFrameBuffer(graphicsDevice, width, height);
            SceneWidth = width;
            SceneHeight = height;
            SceneCamera.UpdateSceneSize(SceneWidth, SceneHeight);
        }

        public void EnterToFarObjectMode()
        {
            _useFarObjRes = true;
        }
        
        public void ExitFarObjectMode()
        {
            _useFarObjRes = false;
        }
        
        private Framebuffer createFrameBuffer(GraphicsDevice graphicsDevice, int width, int height)
        {
            TextureSampleCount maxSampleCount =
                graphicsDevice.GetSampleCountLimit(PixelFormat.R8_G8_B8_A8_UNorm, false);
            Texture renderTarget = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription()
            {
                Width = (uint) width,
                Height = (uint) height,
                ArrayLayers = 1,
                Depth = 1,
                Format = PixelFormat.R8_G8_B8_A8_UNorm,
                MipLevels = 1,
                SampleCount = maxSampleCount,
                Type = TextureType.Texture2D,
                Usage = TextureUsage.RenderTarget | TextureUsage.Sampled,
            });
            Debug.Print($"sample count: {Enum.GetName(typeof(TextureSampleCount), maxSampleCount)}");
            Texture depthStencil = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription()
            {
                Width = (uint) width,
                Height = (uint) height,
                ArrayLayers = 1,
                Depth = 1,
                Format = PixelFormat.R32_Float,
                MipLevels = 1,
                SampleCount = maxSampleCount,
                Type = TextureType.Texture2D,
                Usage = TextureUsage.DepthStencil
            });
            FramebufferDescription framebufferDescription = new FramebufferDescription()
            {
                DepthTarget = new FramebufferAttachmentDescription(depthStencil, 0),
                ColorTargets = [new FramebufferAttachmentDescription(renderTarget, 0)],
            };
            return graphicsDevice.ResourceFactory.CreateFramebuffer(framebufferDescription);
        }
        
    }
}