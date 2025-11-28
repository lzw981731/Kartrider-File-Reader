using System.Diagnostics;
using eP.Extension;
using KartLibrary.Engine.Relements;
using Veldrid;

namespace KartLibrary.Game.Engine.Render;

public class KartEngineScene
{
    private List<(IRenderable, DeviceObjectCache)> _renderables = [];
    private List<(IRenderable, DeviceObjectCache)> _skyboxRenderables = [];
    private List<(IRenderable, DeviceObjectCache)> _worldRenderables = [];

    private List<Relement> _otherRelements = new List<Relement>(); 
    
    private Queue<(IRenderable, DeviceObjectCache)> _newRenderable = [];
    private Queue<(IRenderable, DeviceObjectCache)> _newSkyboxRenderable = [];
    private Queue<(IRenderable, DeviceObjectCache)> _newWorldRenderable = [];

    private Queue<IRenderable> _skydomeSolidRenderQueue = new();
    private PriorityQueue<IRenderable, double> _skydomeTranslucpancyRenderQueue = new();
    private PriorityQueue<IRenderable, double> _translucpancyRenderQueue = new();
    private Queue<IRenderable> _solidRenderQueue = new();

    private Mutex _isRenderStateMutex = new Mutex();
    private bool _isRenderState = false;
    
    public Camera Camera { get; init; }
    
    public Relement SkyboxRelement { get; private set; }
    
    public Relement WorldRelement { get; private set; }
    
    internal float DebugGetCpuRenderTime { get; private set; }
    
    internal float DebugGetGpuRenderTime { get; private set; }
    
    public float DebugGetWorldUpdateTime { get; private set; }
    
    public float DebugGetWorldObjUpdateTime { get; private set; }

    public int DebugRenderLimiterBegin { get; set; } = -1;
    public int DebugRenderLimiterEnd { get; set; } = -1;
    
    public KartEngineScene()
    {
        Camera = new Camera();
    }

    public void AddRenderable(IRenderable renderable, DeviceObjectCache localObjCache)
    {
        _newRenderable.Enqueue((renderable, localObjCache));
    }

    public void SetSkybox(Relement skyboxRelement, DeviceObjectCache localObjectCache)
    {
        SkyboxRelement = skyboxRelement;
        constructRenderable(skyboxRelement, localObjectCache, _newSkyboxRenderable);
    }

    public void SetWorld(Relement worldRelement, DeviceObjectCache localObjectCache)
    {
        WorldRelement = worldRelement;
        constructRenderable(worldRelement, localObjectCache, _newWorldRenderable);
    }

    public void AddRelement(Relement relement, DeviceObjectCache localObjectCache)
    {
        _otherRelements.Add(relement);
        constructRenderable(relement, localObjectCache, _newRenderable);
    }

    public void UpdatePerFrameResources(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        createNewRenderableDeviceObjs(graphicsDevice, commandList, sceneContext);
        
        sceneContext.UpdateCameraBuffers(commandList);
        
        SkyboxRelement?.Update(sceneContext.TimeSource);
        foreach (var item in _skyboxRenderables)
        {
            item.Item1.UpdatePerFrameResources(graphicsDevice, commandList, sceneContext, item.Item2);
            if (item.Item1.IsTranslucencyObject())
            {
                double priority = item.Item1.GetDistance(sceneContext.SceneCamera, true);
                _skydomeTranslucpancyRenderQueue.Enqueue(item.Item1, -priority);
            }
            else
            {
                _skydomeSolidRenderQueue.Enqueue(item.Item1);
            }
            // _renderQueue.Enqueue(item.Item1, float.MinValue);
        }
        
        DateTime time = DateTime.Now;
        WorldRelement?.Update(sceneContext.TimeSource);
        DebugGetWorldUpdateTime = (float)(DateTime.Now - time).TotalMilliseconds;
        
        time = DateTime.Now;

        foreach (var item in _worldRenderables)
        {
            double priority = item.Item1.GetDistance(sceneContext.SceneCamera, false);
            if (!double.IsNaN(priority))
            {
                item.Item1.UpdatePerFrameResources(graphicsDevice, commandList, sceneContext, item.Item2);
                if (item.Item1.IsTranslucencyObject())
                {
                    _translucpancyRenderQueue.Enqueue(item.Item1, -priority);
                }
                else
                {
                    _solidRenderQueue.Enqueue(item.Item1);
                }
            }
        }
        
        DebugGetWorldObjUpdateTime = (float)(DateTime.Now - time).TotalMilliseconds;
        foreach(var otherRelement in _otherRelements)
            otherRelement?.Update(sceneContext.TimeSource);

        foreach (var item in _renderables)
        {
            double priority = item.Item1.GetDistance(sceneContext.SceneCamera, false);
            if (!double.IsNaN(priority))
            {
                item.Item1.UpdatePerFrameResources(graphicsDevice, commandList, sceneContext, item.Item2);
                if (item.Item1.IsTranslucencyObject())
                {
                    _translucpancyRenderQueue.Enqueue(item.Item1, -priority);
                }
                else
                {
                    _solidRenderQueue.Enqueue(item.Item1);
                }
            }
        }
    }

    private async Task<CommandList> updateFrameResAsync(GraphicsDevice graphicsDevice, CommandList commandList,
        SceneContext sceneContext, IEnumerable<(IRenderable, DeviceObjectCache)> updateRenderables)
    {
        commandList.Begin();
        foreach(var item in updateRenderables)
            item.Item1.UpdatePerFrameResources(graphicsDevice, commandList, sceneContext, item.Item2);
        commandList.End();
        return commandList;
    }
    
    public void Render(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        commandList.SetFramebuffer(sceneContext.MainSceneFramebuffer);
        commandList.SetFullViewport(0);
        commandList.ClearDepthStencil(1.0f);
        commandList.ClearColorTarget(0, RgbaFloat.Black);
        
        sceneContext.TimeSource.OnUpdateFrame();

        int debugCounter = 0;
        
        sceneContext.EnterToFarObjectMode();
        while (_skydomeSolidRenderQueue.Count > 0)
        {
            var renderable = _skydomeSolidRenderQueue.Dequeue();
            if (DebugRenderLimiterBegin <= debugCounter &&
                (DebugRenderLimiterEnd < 0 || DebugRenderLimiterEnd >= debugCounter))
            {
                renderable.Render(graphicsDevice, commandList, sceneContext);
            }
            debugCounter++;
        }

        while (_skydomeTranslucpancyRenderQueue.Count > 0)
        {
            var renderable = _skydomeTranslucpancyRenderQueue.Dequeue();
            if (DebugRenderLimiterBegin <= debugCounter &&
                (DebugRenderLimiterEnd < 0 || DebugRenderLimiterEnd >= debugCounter))
            {
                renderable.Render(graphicsDevice, commandList, sceneContext);
            }
            debugCounter++;
        }
        sceneContext.ExitFarObjectMode();

        commandList.ClearDepthStencil(1);

        while (_solidRenderQueue.Count > 0)
        {
            var renderable = _solidRenderQueue.Dequeue();
            if (DebugRenderLimiterBegin <= debugCounter &&
                (DebugRenderLimiterEnd < 0 || DebugRenderLimiterEnd >= debugCounter))
            {
                renderable.Render(graphicsDevice, commandList, sceneContext);
            }
            debugCounter++;
        }

        while (_translucpancyRenderQueue.Count > 0)
        {
            var renderable = _translucpancyRenderQueue.Dequeue();
            if (DebugRenderLimiterBegin <= debugCounter &&
                (DebugRenderLimiterEnd < 0 || DebugRenderLimiterEnd >= debugCounter))
            {
                renderable.Render(graphicsDevice, commandList, sceneContext);
            }
            debugCounter++;
        }
        
        // foreach(var item in _worldRenderables.OrderByDescending(x => x.Item1.GetDistance(sceneContext.SceneCamera)))
        //     item.Item1.Render(graphicsDevice, commandList, sceneContext);
        //
        // foreach(var item in _renderables.OrderByDescending(x => x.Item1.GetDistance(sceneContext.SceneCamera)))
        //     item.Item1.Render(graphicsDevice, commandList, sceneContext);
    }
    
    public void DestroyDeviceObjects()
    {
        foreach(var item in _skyboxRenderables)
            item.Item1.DestroyAllDeviceObjects();
        
        foreach(var item in _worldRenderables)
            item.Item1.DestroyAllDeviceObjects();
        
        foreach(var item in _renderables)
            item.Item1.DestroyAllDeviceObjects();
    }
    
    private void createNewRenderableDeviceObjs(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext)
    {
        while (_newRenderable.Count > 0)
        {
            var curRenderable = _newRenderable.Dequeue();
            curRenderable.Item1.CreateDeviceObjects(graphicsDevice, commandList, sceneContext, curRenderable.Item2);
            _renderables.Add(curRenderable);
        }

        while (_newSkyboxRenderable.Count > 0)
        {
            var curRenderable = _newSkyboxRenderable.Dequeue();
            curRenderable.Item1.CreateDeviceObjects(graphicsDevice, commandList, sceneContext, curRenderable.Item2);
            _skyboxRenderables.Add(curRenderable);
        }
        
        while (_newWorldRenderable.Count > 0)
        {
            var curRenderable = _newWorldRenderable.Dequeue();
            curRenderable.Item1.CreateDeviceObjects(graphicsDevice, commandList, sceneContext, curRenderable.Item2);
            _worldRenderables.Add(curRenderable);
        }
    }

    private void constructRenderable(Relement relement, DeviceObjectCache localObjectCache, Queue<(IRenderable, DeviceObjectCache)> renderables)
    {
        if(relement is IRenderable renderable)
            renderables.Enqueue((renderable, localObjectCache));
        foreach(var child in relement)
            constructRenderable(child, localObjectCache, renderables);
    }
}