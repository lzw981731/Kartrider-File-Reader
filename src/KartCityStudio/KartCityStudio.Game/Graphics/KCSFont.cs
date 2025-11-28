using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;

namespace KartCityStudio.Game.Graphics
{
    public static class KCSFont
    {
        public const string DefaultWeight = "ExtraBold";
        public static FontUsage Default => new FontUsage(family: "RedHatDisplay", size: 17f, weight: DefaultWeight);
        public static FontUsage DefaultMono => new FontUsage(family: "RedHatMono", size: 17f, weight: "SemiBold");
        public static FontUsage DefaultM => new FontUsage(family: "RedHatDisplay", size: 22f, weight: DefaultWeight);
        public static FontUsage DefaultL => new FontUsage(family: "RedHatDisplay", size: 55f, weight: DefaultWeight);
        public static FontUsage DefaultXL => new FontUsage(family: "RedHatDisplay", size: 58f, weight: DefaultWeight);

        public static FontUsage RubikSemiBold => new FontUsage(family: "Rubik", size: 20f, weight: "SemiBold");
        public static FontUsage Rubik => new FontUsage(family: "Rubik", size: 20f, weight: DefaultWeight);
        public static FontUsage RubikM => new FontUsage(family: "Rubik", size: 25f, weight: DefaultWeight);
        public static FontUsage RubikLight => new FontUsage(family: "Rubik", size: 20f, weight: "Light");
        public static FontUsage RubikLightM => new FontUsage(family: "Rubik", size: 25f, weight: "Light");
    }
}
