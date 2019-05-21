﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using KnxCli.Core;
using KnxCli.Core.Actors;

namespace KnxCli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var res = (int)ExitCode.Error;
            try
            {
                if (args.Length == 0)
                {
                    return (int)ShowMenu();
                }

                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(options => res = (int)HandleParsedOptions(options))
                    .WithNotParsed(errs => HandleParseError(errs));

                return res;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                return res;
            }
        }

        private static ExitCode HandleParsedOptions(Options options)
        {
            var actorsModel = Utilities.LoadActors();

            if (options.List)
            {
                ListActors(actorsModel);
                return ExitCode.OK;
            }

            if (string.IsNullOrEmpty(options.Actor))
            {
                Console.Error.WriteLine($"No actor specified.");
                return ExitCode.NoActorSpecified;
            }

            if (string.IsNullOrEmpty(options.Action))
            {
                Console.Error.WriteLine($"No action specified.");
                return ExitCode.NoActionSpecified;
            }

            var settings = Utilities.LoadSettings();
            if (settings == null)
            {
                Console.Error.WriteLine($"No settings available.");
                return ExitCode.NoSettingsAvailable;
            }
            if (string.IsNullOrEmpty(settings.ConnectionMode))
            {
                Console.Error.WriteLine($"No KNX connection mode specified (Tunneling or Routing).");
                return ExitCode.NoActionSpecified;
            }

            var actors = actorsModel.GetActorsAndGroupsByName(options.Actor);
            if (!actorsModel.CheckActorsAction(actors))
            {
                Console.Error.WriteLine($"Actors configuration issue.");
                return ExitCode.DifferentActorsActionConfiguration;
            }
            if (!actors.Any())
            {
                Console.Error.WriteLine($"Actor or group '{options.Actor}' not found.");
                return ExitCode.ActionOrGroupNotFound;
            }


            WriteLine(options, $"Actor action: '{options.Action}'");


            var actionParser = new ActionTypeParserService();
            var action = actionParser.Parse(actors.First(), options.Action);
            if (action == null)
            {
                Console.Error.WriteLine($"Cannot parse action: '{options.Action}'");
                return ExitCode.CannotParseAction;
            }


            WriteLine(options, "Executing action...");

            using (var executor = new ActionExecutorService(settings))
            {
                executor.Connect(settings.ConnectionMode, options.Verbose);

                if (!executor.Execute(actors, action))
                {
                    Console.Error.WriteLine($"Action execution failed.");
                    return ExitCode.ActionExecutionFailed;
                }
            }

            WriteLine(options, "Executed.");

            return 0;
        }

        private static void ListActors(ActorsModel actorsModel)
        {
            var names = actorsModel.AllActors.Select(l => l.Name).Distinct().OrderBy(l => l);
            Console.WriteLine();
            Console.WriteLine("Actors:");
            foreach (var name in names)
            {
                Console.WriteLine(name);
            }

            var groups = actorsModel.AllActors.SelectMany(l => l.Groups).Distinct().OrderBy(l => l);
            Console.WriteLine();
            Console.WriteLine("Groups:");
            foreach (var name in groups)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();
        }

        public static void WriteLine(Options options, string msg)
        {
            if (options.Verbose)
            {
                Console.WriteLine(msg);
            }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                Console.WriteLine(err.ToString());
            }
        }

        private static ExitCode ShowMenu()
        {
            using (var actorsMenu = new ActorsMenu())
            {
                if (!actorsMenu.Init())
                {
                    return ExitCode.Error;
                }

                return actorsMenu.ShowMenu();
            }
        }
    }
}
