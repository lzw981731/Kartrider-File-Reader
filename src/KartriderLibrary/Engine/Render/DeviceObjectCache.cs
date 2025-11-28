using KartLibrary.Game.Engine.Render.Store;
using Veldrid;

namespace KartLibrary.Game.Engine.Render
{
    public class DeviceObjectCache
    {
        private Dictionary<string, VeldridShader> _shaders = new Dictionary<string, VeldridShader>();
        private Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
        private Dictionary<string, VertexLayoutDescription> _vertexLayoutDescs = new Dictionary<string, VertexLayoutDescription>();
        private List<TextureResourceStore> _textureResourceStores = [];
        
        public VeldridShader? GetShaders(string name)
        {
            if (!_shaders.ContainsKey(name))
                return null;
            return _shaders[name];
        }
        
        public VeldridShader GetOrCreateShaders(string name, Func<VeldridShader> createFunc)
        {
            VeldridShader? outShaders;
            if (!_shaders.TryGetValue(name, out outShaders))
            {
                outShaders = createFunc();
                _shaders.Add(name, outShaders);
            }
            return outShaders;
        }

        public void PushTextureResourceStore(TextureResourceStore textureResourceStore)
        {
            _textureResourceStores.Add(textureResourceStore);
        }
        
        public Texture? GetTexture(string name)
        {
            if (!_textures.TryGetValue(name, out var texture1))
            {
                foreach (var textureResourceStore in _textureResourceStores)
                {
                    Texture? texture = textureResourceStore.GetTexture(name);
                    if (texture is not null)
                        return texture;
                }
                return null;
            }
            
            return texture1;
        }

        public Texture GetOrCreateTexture(string name, Func<Texture> createFunc)
        {
            Texture? outTexture;
            if (!_textures.TryGetValue(name, out outTexture))
            {
                outTexture = createFunc();
                AddTexture(name, outTexture);
            }
            return outTexture;
        }

        public void AddShaders(string name, VeldridShader shader)
        {
            if (_shaders.ContainsKey(name))
                throw new InvalidOperationException($"There are exist shaders called \"{name}\".");
            lock(_shaders)
                _shaders.Add(name, shader);
        }

        public void AddTexture(string name, Texture texture)
        {
            if (_textures.ContainsKey(name))
                throw new InvalidOperationException($"There are exist texture called \"{name}\".");
            lock (_textures)
                _textures.Add(name, texture);
        }

        public bool ContainsShaders(string name) => _shaders.ContainsKey(name);

        public bool ContainsTexture(string name) => _textures.ContainsKey(name);

        public void RemoveShaders(string name)
        {
            if (!_shaders.ContainsKey(name))
                throw new InvalidOperationException($"There are no any shaders called \"{name}\".");
            _shaders.Remove(name);
        }

        public void RemoveTexture(string name)
        {
            if (!_textures.ContainsKey(name))
                throw new InvalidOperationException($"There are no any texture called \"{name}\".");
            _textures.Remove(name);
        }

        public void DestroyAllDeviceObjects()
        {
            foreach(var item in _textures)
                item.Value.Dispose();
        }
    }
}
