
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

        public Actor GetActorByName(string name)
        {
            return actors.FirstOrDefault(l => l.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Actor> GetActorsGroupByName(string name)
        {
            return actors.Where(
                l => l.Groups != null &&
                    l.Groups.FirstOrDefault(
                        x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != null
                );
        }

        public IEnumerable<Actor> AllActors
        {
            get { return actors; }
        }
    }

}