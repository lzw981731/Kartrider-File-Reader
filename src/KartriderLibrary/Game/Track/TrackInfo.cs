using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Track
{
    public struct TrackInfo
    {
        public string TrackId { get; set; }
        public int Laps { get; set; }
        public int Level { get; set; }
        public int Difficulty { get; set; }
        public bool IsOnlyItemTrack { get; set; }
        public int F1Speed { get; set; }
        public bool Choosable { get; set; }
        public int Length { get; set; }
        public string? BgmTheme { get; set; }
        public string[]? TexThemes { get; set; }
    }
}
