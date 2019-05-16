using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using KnxCli.Core;
using KnxCli.Core.Actors;

namespace KnxCli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var res = -1;
            var parser = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => res = HandleParsedOptions(options))
                .WithNotParsed(errs => HandleParseError(errs));

            return res;
        }

        private static int HandleParsedOptions(Options options)
        {
            if (string.IsNullOrEmpty(options.Actor))
            {
                Console.Error.WriteLine($"No actor specified.");
                return 1;
            }

            if (string.IsNullOrEmpty(options.Action))
            {
                Console.Error.WriteLine($"No action specified.");
                return 2;
            }


            var settings = LoadSettings();
            if (settings == null)
            {
                Console.Error.WriteLine($"No settings available.");
                return 3;
            }


            var actorsModel = LoadActors();
            var actors = new List<Actor>();
            var actor = actorsModel.GetActorByName(options.Actor);
            if (actor != null)
            {
                actors.Add(actor);
            }
            else
            {
                actors.AddRange(actorsModel.GetActorsGroupByName(options.Actor));
            }

            if (actors.Count == 0)
            {
                Console.Error.WriteLine($"Actor or group '{options.Actor}' not found.");
                return 4;
            }


            WriteLine(options, $"Actor action: '{options.Action}'");


            var actionParser = new ActionTypeParserService();
            var action = actionParser.Parse(actor, options.Action);

            if (action == null)
            {
                Console.Error.WriteLine($"Cannot parse action: '{options.Action}'");
                return 5;
            }

            WriteLine(options, "Executing action...");

            var executor = new ActionExecutorService(settings);
            if (!executor.Execute(actors, action, options.Verbose))
            {
                Console.Error.WriteLine($"Action execution failed.");
                return 6;
            }

            WriteLine(options, "Executed.");

            return 0;
        }

        public static void WriteLine(Options options, string msg)
        {
            if (options.Verbose)
            {
                Console.WriteLine(msg);
            }
        }

        private static KnxSettings LoadSettings()
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

        private static ActorsModel LoadActors()
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

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }
}
