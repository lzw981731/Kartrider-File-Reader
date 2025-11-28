using KartCityStudio.Game.Text;
using osu.Framework;
using osu.Framework.Platform;

namespace KartCityStudio.Game.Tests
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost("visual-tests", new HostOptions()))
            using (var game = new KartCityStudioTestBrowser())
            {
                host.Run(game);
            }
            // "\\c:#0000FF;<StepList \\c:#FF0000;modeBonus\\u;=\\c:#FFFF00;\\b;\"1.2\" \\u;\\c:#FF0000;openStep\\u;=\"40\">");
        }
    }
}
