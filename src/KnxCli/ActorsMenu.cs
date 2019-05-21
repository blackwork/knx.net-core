
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KnxCli.Core;
using KnxCli.Core.Actors;
using Nim.Console;

namespace KnxCli
{
    public class ActorsMenu : IDisposable
    {
        public bool Init()
        {
            actorsModel = Utilities.LoadActors();
            if (actorsModel == null)
            {
                Console.Error.WriteLine($"No actors available.");
                return false;
            }

            var settings = Utilities.LoadSettings();
            if (settings == null)
            {
                Console.Error.WriteLine($"No settings available.");
                return false;
            }

            executor = new ActionExecutorService(settings);
            if (!executor.Connect(settings.ConnectionMode, false))
            {
                Console.Error.WriteLine($"Cannot connect.");
                return false;
            }

            return true;
        }

        private ActorsModel actorsModel;


        private ActionExecutorService executor;

        private Menu Menu;

        public ExitCode ShowMenu()
        {
            Console.OutputEncoding = Encoding.Default;

            Menu = new Menu
            (
                "Main",
                new[]
                {
                    new Menu.Item("Lights", ShowLights),
                    new Menu.Item("Exit", () => Menu.Close()),
                }
            );

            Menu.Main.MaxColumns = 1;

            Menu.WriteLine("Use ←↑↓→ for navigation.");
            Menu.WriteLine("Press Esc for return to main menu.");
            Menu.WriteLine("Press Backspace for return to parent menu.");

            Menu.Begin();

            return ExitCode.OK;
        }

        private void ShowLights()
        {
            if (Menu.Selected.Items.Count > 0)
            {
                return;
            }


            var actorsAndGroups = new List<Tuple<string, string, Actor>>();
            actorsAndGroups.AddRange(
                actorsModel.GetActors()
                .Select(l => new Tuple<string, string, Actor>("Actor", l.Name, l)));
            actorsAndGroups.AddRange(
                actorsModel.GetActorsGroups()
                .Select(l => new Tuple<string, string, Actor>("Group", l, null)));

            foreach (var actor in actorsAndGroups)
            {
                var subOn = new Menu.Item(
                    $"{actor.Item1}: {actor.Item2} On",
                    SwitchActor)
                {
                    Tag = new Tuple<Actor, string, string>(actor.Item3, actor.Item2, "true")
                };

                Menu.Selected.Add(subOn);

                var subOff = new Menu.Item(
                    $"{actor.Item1}: {actor.Item2} Off",
                    SwitchActor)
                {
                    Tag = new Tuple<Actor, string, string>(actor.Item3, actor.Item2, "false")
                };

                Menu.Selected.Add(subOff);
            }
        }

        private void SwitchActor()
        {
            var actorInfo = (Tuple<Actor, string, string>)Menu.Selected.Tag;

            var actors = (actorInfo.Item1 == null) ?
                actorsModel.GetActorsAndGroupsByName(actorInfo.Item2) :
                new[] { actorInfo.Item1 };


            var actionParser = new ActionTypeParserService();
            var action = actionParser.Parse(actors.First(), actorInfo.Item3);

            if (!executor.Execute(actors, action))
            {
                Console.Error.WriteLine($"Action execution failed.");
            }
        }

        public void Dispose()
        {
            executor.Disconnect();
            executor = null;
        }
    }
}