using Veldrid;

namespace KartLibrary.Game.Engine.Render
{
    public interface IRenderable: IDisposable
    {
        void CreateDeviceObjects(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, DeviceObjectCache localDeviceObjectCache);

        void UpdatePerFrameResources(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, DeviceObjectCache localDeviceObjectCache);

        void Render(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext);

        double GetDistance(Camera camera, bool useFar);

        bool IsTranslucencyObject();
        
        void DestroyAllDeviceObjects();
    }
}
