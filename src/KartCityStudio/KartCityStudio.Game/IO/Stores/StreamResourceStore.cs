using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KartLibrary.File;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;

namespace KartCityStudio.Game.IO.Stores
{
    public class StreamResourceStore : IResourceStore<byte[]>
    {
        private Dictionary<string, Stream> streams = new Dictionary<string, Stream>();
        public StreamResourceStore()
        {

        }

        public void AddStreamSource(string name, Stream stream)
        {
            streams.TryAdd(name, stream);
        }

        public void Dispose()
        {

        }

        public byte[] Get(string name)
        {
            return (streams.TryGetValue(name, out var value) ? value : null)?.ReadAllBytesToArray();
        }

        public async Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            return await (streams.TryGetValue(name, out var value) ? value : null)?.ReadAllBytesToArrayAsync();
        }

        public IEnumerable<string> GetAvailableResources()
        {
            return streams.Keys;
        }

        public Stream GetStream(string name)
        {
            return (streams.TryGetValue(name, out var value) ? value : null);
        }
    }
}
