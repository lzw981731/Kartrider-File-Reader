using System.Collections.Concurrent;
using KartLibrary.Engine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace KartLibrary.Game.Engine.Render.Store;

public abstract class TextureResourceStore: IDisposable
{
    private ConcurrentDictionary<string, Texture> _textureCache = [];
    private Texture? _defaultTexture;
    private GraphicsDevice _graphicsDevice;
    
    protected abstract Stream? CreateTextureStream(string resourceName, out bool isDds);

    public TextureResourceStore(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }
    
    public Texture? GetTexture(string resourceName)
    {
        if (_textureCache.TryGetValue(resourceName, out var texture))
            return texture;

        bool isDds = false;
        using var textureStream = CreateTextureStream(resourceName, out isDds);
        if (textureStream is null)
            return null;
        
        CommandList commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
        commandList.Begin();
        Texture newTexture = TextureUtility.CreateTexture(_graphicsDevice, commandList, textureStream, isDds);
        commandList.End();
        
        _graphicsDevice.SubmitCommands(commandList);
        // commandList.Dispose();

        _textureCache.GetOrAdd(resourceName, newTexture);

        return newTexture;
    }
    
    public Texture GetTextureOrDefault(string resourceName)
    {
        return GetTexture(resourceName) ?? GetDefaultTexture();
    }

    private Texture GetDefaultTexture()
    {
        if (_defaultTexture is null)
        {
            Image<Rgba32> emptyImage = new Image<Rgba32>(1, 1);

            CommandList commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            _defaultTexture = TextureUtility.CreateTexture(_graphicsDevice, commandList, emptyImage);
            commandList.Dispose();
        }

        return _defaultTexture;
    }

    public void Dispose()
    {
        _defaultTexture?.Dispose();
        
        foreach(var texturePair in _textureCache)
            texturePair.Value.Dispose();
    }
}