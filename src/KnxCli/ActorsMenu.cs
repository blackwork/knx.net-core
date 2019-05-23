
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

            using (Menu = new Menu(
                    "Main",
                    new[]
                    {
                        new Menu.Item("Lights", ShowLights),
                        new Menu.Item("Groups", ShowGroups),
                        new Menu.Item("Exit", () => Menu.Close()),
                    }
                ))
            {

                Menu.Main.MaxColumns = 1;

                Menu.WriteLine("Use ←↑↓→ for navigation.");
                Menu.WriteLine("Press Esc for return to main menu.");
                Menu.WriteLine("Press Backspace for return to parent menu.");

                Menu.Begin();

                return ExitCode.OK;
            }
        }

        private void ShowLights()
        {
            if (Menu.Selected.Items.Count > 0)
            {
                return;
            }


            var actorsAndGroups = actorsModel.GetActors();

            foreach (var actor in actorsAndGroups)
            {
                var subOn = new Menu.Item($"{actor.Name} On", SwitchActorOrGroup)
                {
                    Tag = new Tuple<Actor, string, string>(actor, actor.Name, "true")
                };

                Menu.Selected.Add(subOn);

                var subOff = new Menu.Item($"{actor.Name} Off", SwitchActorOrGroup)
                {
                    Tag = new Tuple<Actor, string, string>(actor, actor.Name, "false")
                };

                Menu.Selected.Add(subOff);
            }
        }

        private void ShowGroups()
        {
            if (Menu.Selected.Items.Count > 0)
            {
                return;
            }


            var groups = actorsModel.GetActorsGroups();

            foreach (var group in groups)
            {
                var subOn = new Menu.Item($"{group} On", SwitchActorOrGroup)
                {
                    Tag = new Tuple<Actor, string, string>(null, group, "true")
                };

                Menu.Selected.Add(subOn);

                var subOff = new Menu.Item($"{group} Off", SwitchActorOrGroup)
                {
                    Tag = new Tuple<Actor, string, string>(null, group, "false")
                };

                Menu.Selected.Add(subOff);
            }
        }

        private void SwitchActorOrGroup()
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