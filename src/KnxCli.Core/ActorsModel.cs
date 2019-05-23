
using System;
using System.Collections.Generic;
using System.Linq;
using KnxCli.Core.Actors;

namespace KnxCli.Core
{
    public class ActorsModel
    {

        public ActorsModel(IEnumerable<Actor> actors)
        {
            if (actors == null)
            {
                throw new ArgumentNullException(nameof(actors));
            }

            this.actors.AddRange(actors);
        }

        private List<Actor> actors = new List<Actor>();

        public bool CheckActorsAction(IEnumerable<Actor> actors)
        {
            return actors.Select(l => l.Action).Distinct().Count() == 1;
        }

        public IEnumerable<Actor> GetActorsAndGroupsByName(string name)
        {
            var actors = new List<Actor>();
            var actor = GetActorByName(name);
            if (actor != null)
            {
                actors.Add(actor);
            }
            else
            {
                actors.AddRange(GetActorsGroupByName(name));
            }

            return actors.OrderBy(l => l.Name).ToList();
        }

        public Actor GetActorByName(string name)
        {
            return actors.FirstOrDefault(
                l => l.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Actor> GetActorsGroupByName(string name)
        {
            return actors
                .Where(l =>
                    l.Groups != null &&
                    l.Groups.FirstOrDefault(
                        x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != null
                )
                .OrderBy(l => l.Name);
        }

        public IEnumerable<Actor> GetActors(string actionType = "Switch")
        {
            return actors
            .Where(l => l.Action.Type == actionType)
            .Distinct()
            .OrderBy(l => l.Name)
            .ToList();
        }

        public IEnumerable<string> GetActorsGroups(string actionType = "Switch")
        {
            return actors
                .Where(l => l.Action.Type == actionType)
                .SelectMany(l => l.Groups != null ? l.Groups : new List<string>())
                .Distinct()
                .OrderBy(l => l)
                .ToList();
        }

        public IEnumerable<Actor> AllActors
        {
            get { return actors; }
        }
    }
}