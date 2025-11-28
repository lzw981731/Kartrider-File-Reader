using System.Numerics;
using ImGuiNET;
using KartLibrary.Engine.Relements;
using KartLibrary.Engine.Render.Renderables;
using KartLibrary.Game.Engine.Render;
using KartRiderLibrary.Tests;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace KartLibrary.Tests.Engine;

public class TestingWindow: IDisposable
{
    private Sdl2Window _window;
    private InputCapturer _inputCapturer;
    private InteractiveCamera _interactiveCamera;
    private GraphicsDevice _graphicsDevice;
    private bool _hasCreatedWindow = false;
    
    private KartEngineScene _baseScene;
    private SceneContext _sceneContext;
    private CommandList _commandList;

    private FramebufferRenderable _framebufferRenderable;
    private DeviceObjectCache _framebufferObjectCache;
    
    private DateTime _prevousUpdateTime = DateTime.Now;
    private int _prevousUpdateTimeTick = Environment.TickCount;

    private ImGuiRenderer _imGuiRenderer;
    private float[] _fpsSampleWindow = new float[60];
    private int _fpsSampleWindowPos = 0;
    
    public KartEngineScene BaseScene => _baseScene;
    public SceneContext SceneContext => _sceneContext;
    public GraphicsDevice GraphicsDevice => _graphicsDevice;

    public bool WindowExists => _window?.Exists ?? false;
    
    private Vector3 _previousCamPos = Vector3.Zero;
    public InteractiveCamera InteractiveCamera => _interactiveCamera;

    private float _updateTime = 0;
    
    private float _renderTime = 0;
    
    public TestingWindow()
    {
        _baseScene = new KartEngineScene();
        _sceneContext = new SceneContext();
        _sceneContext.SetScene(_baseScene);
        
        _inputCapturer = new InputCapturer();
        _interactiveCamera = new InteractiveCamera(_inputCapturer, _sceneContext.SceneCamera);
        _framebufferRenderable = new FramebufferRenderable();
        _framebufferObjectCache = new DeviceObjectCache();
    }

    public void CreateWindow()
    {
        try
        {
            WindowCreateInfo createInfo = new WindowCreateInfo()
            {
                WindowWidth = 1920,
                WindowHeight = 1080,
                X = 100,
                Y = 100
            };
            _window = VeldridStartup.CreateWindow(ref createInfo);
            _window.MouseDown += windowOnMouseDown;
            _window.MouseUp += windowOnMouseUp;
            _window.MouseMove += windowOnMouseMove;
            _window.MouseWheel += windowOnMouseWheel;
            _window.KeyDown += windowOnKeyDown;
            _window.KeyUp += windowOnKeyUp;
            _window.Resized += windowOnResized;
            _inputCapturer.OnResize(new Vector2(_window.Width, _window.Height));

            GraphicsDeviceOptions options = new GraphicsDeviceOptions()
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                SwapchainDepthFormat = PixelFormat.R32_Float,
                ResourceBindingModel = ResourceBindingModel.Improved,
                SyncToVerticalBlank = false,
            };
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(
                _window, options, GraphicsBackend.Vulkan);

            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

            _commandList.Begin();
            _sceneContext.CreateDeviceObjects(_graphicsDevice, 1920, 1080);
            _framebufferRenderable.CreateDeviceObjects(_graphicsDevice, _commandList, _sceneContext,
                _framebufferObjectCache);
            _imGuiRenderer = new ImGuiRenderer(_graphicsDevice, _graphicsDevice.SwapchainFramebuffer.OutputDescription,
                _window.Width, _window.Height);
            _imGuiRenderer.CreateDeviceResources(_graphicsDevice,
                _graphicsDevice.SwapchainFramebuffer.OutputDescription);
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _hasCreatedWindow = true;
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            if(!_hasCreatedWindow)
                _window?.Close();
        }
    }

    public void Update()
    {
        if (_window.Exists)
        {
            InputSnapshot inputSnapshot = _window.PumpEvents();

            double updateDuration = (DateTime.Now - _prevousUpdateTime).TotalMilliseconds;
            double updateDurationTick = Environment.TickCount - _prevousUpdateTimeTick;
            double fps = 1000.0f / updateDuration;
            _fpsSampleWindow[_fpsSampleWindowPos] = (float)fps;
            _fpsSampleWindowPos = (_fpsSampleWindowPos + 1) % _fpsSampleWindow.Length;
            _prevousUpdateTime = DateTime.Now;
            _prevousUpdateTimeTick = Environment.TickCount;
            
            _imGuiRenderer.Update((float)updateDuration, inputSnapshot);
            
            _previousCamPos = _baseScene.Camera.CameraPosition;
            float orgSpeed = _sceneContext.TimeSource.PlaySpeed;
            float adjustedSpeed = orgSpeed;
            float far = _baseScene.Camera.Far;
            float near = _baseScene.Camera.Near;
            int debugLimiterBegin = _baseScene.DebugRenderLimiterBegin;
            int debugLimiterEnd = _baseScene.DebugRenderLimiterEnd;
            
            ImGui.Begin("KartLibrary Testing window");
            ImGui.SetWindowSize(new Vector2(500.0f, 500.0f), ImGuiCond.FirstUseEver);
            ImGui.Value("FPS", (float)_fpsSampleWindow.Average());
            ImGui.Value("Time", _sceneContext.TimeSource.GetTimeStamp() / 1000f);
            ImGui.LabelText("Backend", _graphicsDevice.BackendType.ToString());
            ImGui.LabelText("Device", _graphicsDevice.DeviceName);
            ImGui.LabelText("Vendor", _graphicsDevice.VendorName);
            ImGui.LabelText("Render Size", $"{_sceneContext.SceneWidth} x {_sceneContext.SceneHeight}");
            ImGui.LabelText("Window Size", $"{_window.Width} x {_window.Height}");
            ImGui.LabelText("Update time", $"{_updateTime:0.00} ms");
            ImGui.LabelText("Render time", $"{_renderTime:0.00} ms");
            ImGui.LabelText("Relement update time", $"{_baseScene.DebugGetWorldUpdateTime:0.00} ms");
            ImGui.LabelText("ReObj update time", $"{_baseScene.DebugGetWorldObjUpdateTime:0.00} ms");
            ImGui.LabelText("View mat updates", $"{_sceneContext.DebugNumViewMatUpdates}");
            ImGui.LabelText("Proj mat updates", $"{_sceneContext.DebugNumProjectMatUpdates}");
            ImGui.InputFloat3("Camera Position", ref _previousCamPos);
            ImGui.DragFloat("Camera Moving Speed", ref _interactiveCamera.MovingSpeed, 0.01f, 0.1f, 3f);
            ImGui.InputFloat("Camera Far", ref far);
            ImGui.InputFloat("Camera Near", ref near);
            ImGui.DragFloat("Time speed", ref adjustedSpeed, 0.01f, 0.001f, 5f);
            ImGui.InputInt("Limiter begin", ref debugLimiterBegin, 1, 1);
            ImGui.InputInt("Limiter end", ref debugLimiterEnd, 1, 1);
            bool resetTimeSource = ImGui.Button("Reset TimeSource");
            bool changeCameraControlStatus = ImGui.Button(_interactiveCamera.IsEnabled ? "Disable camera control" : "Enable camera control");
            ImGui.End();

            if (changeCameraControlStatus)
                _interactiveCamera.IsEnabled ^= true;
            
            if(resetTimeSource)
                _sceneContext.TimeSource.ResetTimeStamp();

            if (MathF.Abs(adjustedSpeed - orgSpeed) >= 0.001f)
                _sceneContext.TimeSource.PlaySpeed = adjustedSpeed;

            if (MathF.Abs(far - _baseScene.Camera.Far) >= 0.001f && far > near  && far > 0)
                _sceneContext.SceneCamera.Far = far;
            
            if (MathF.Abs(near - _baseScene.Camera.Near) >= 0.001f && near < far && near > 0)
                _sceneContext.SceneCamera.Near = near;

            _baseScene.DebugRenderLimiterBegin = debugLimiterBegin;
            _baseScene.DebugRenderLimiterEnd = debugLimiterEnd;
            
            _interactiveCamera.UpdateMovingEvent();
            
            _commandList.Begin();
            _commandList.SetFramebuffer(_sceneContext.MainSceneFramebuffer);
            _commandList.SetFullViewport(0);
            _commandList.ClearColorTarget(0, RgbaFloat.Clear);
            _commandList.ClearDepthStencil(1.0f);
            
            DateTime updateBeginTime = DateTime.Now;
            _baseScene.UpdatePerFrameResources(_graphicsDevice, _commandList, _sceneContext);
            DateTime updateEndTime = DateTime.Now;
            
            DateTime renderBeginTime = DateTime.Now;
            _baseScene.Render(_graphicsDevice, _commandList, _sceneContext);
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            DateTime renderEndTime = DateTime.Now;
            
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewport(0);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);
            _framebufferRenderable.UpdatePerFrameResources(_graphicsDevice, _commandList, _sceneContext, _framebufferObjectCache);
            _framebufferRenderable.Render(_graphicsDevice, _commandList, _sceneContext);
            _imGuiRenderer.Render(_graphicsDevice, _commandList);
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();

            _updateTime = (float)(updateEndTime - updateBeginTime).TotalMilliseconds;
            _renderTime = (float)(renderEndTime - renderBeginTime).TotalMilliseconds;
        }
    }

    public void Close()
    {
        _baseScene?.DestroyDeviceObjects();
        _window.Close();
    }
    
    public void Dispose()
    {
        _window?.Close();
        _framebufferObjectCache.DestroyAllDeviceObjects();
        _framebufferRenderable.DestroyAllDeviceObjects();
        _baseScene?.DestroyDeviceObjects();
    }
    
    private void windowOnResized()
    {
        _inputCapturer.OnResize(new Vector2(_window.Width, _window.Height));
        _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
        _sceneContext.SetMainSceneSize(_graphicsDevice, _window.Width, _window.Height);
        _imGuiRenderer.WindowResized(_window.Width, _window.Height);
    }

    private void windowOnKeyUp(KeyEvent obj)
    {
        _inputCapturer.OnKeyUp(obj);
    }

    private void windowOnKeyDown(KeyEvent obj)
    {
        _inputCapturer.OnKeyDown(obj);
    }

    private void windowOnMouseWheel(MouseWheelEventArgs obj)
    {
        _inputCapturer.OnMouseWheel(obj);
    }

    private void windowOnMouseMove(MouseMoveEventArgs obj)
    {
        _inputCapturer.OnMouseMove(obj);
    }

    private void windowOnMouseUp(MouseEvent obj)
    {
        _inputCapturer.OnMouseButtonUp(obj);
    }

    private void windowOnMouseDown(MouseEvent obj)
    {
        _inputCapturer.OnMouseButtonDown(obj);
    }
}

