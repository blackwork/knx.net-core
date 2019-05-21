
using System;
using System.IO;
using KnxCli.Core;

namespace KnxCli
{
    public static class Utilities
    {
        public static KnxSettings LoadSettings()
        {
            var settingsFile = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), "settings.json");

#if DEBUG
            if (!File.Exists(settingsFile))
            {
                settingsFile = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), "..", "..", "..", "settings.json");
            }
#endif

            var loaderService = new LoaderService();
            var settings = loaderService.LoadSettings(settingsFile);

            return settings;
        }

        public static ActorsModel LoadActors()
        {
            var actorsFile = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), "actors.json");

#if DEBUG
            if (!File.Exists(actorsFile))
            {
                actorsFile = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), "..", "..", "..", "actors.json");
            }
#endif

            var loaderService = new LoaderService();
            var actors = loaderService.LoadActors(actorsFile);

            return actors;
        }
    }
}