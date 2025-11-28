using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KartCity.Common.FileType;
using KartLibrary.File;
using osu.Framework.IO.Stores;

namespace KartCityStudio.Game.IO.Stores
{
    public class RhoFileResourceStore : IResourceStore<byte[]>
    {
        private IRhoFile rhoFile;

        public RhoFileResourceStore(IRhoFile rhoFile)
        {
            this.rhoFile = rhoFile;
        }

        public void Dispose()
        {

        }

        public byte[] Get(string name)
        {
            return rhoFile.GetBytes();
        }

        public async Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            return await rhoFile.GetBytesAsync(cancellationToken);
        }

        public IEnumerable<string> GetAvailableResources()
        {
            return [rhoFile.FullName];
        }

        public Stream GetStream(string name)
        {
            return rhoFile.CreateStream();
        }
    }
}
