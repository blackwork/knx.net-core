using System.Collections.Generic;

namespace KnxCli.Core.Actors
{
    public class Actor
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Address { get; set; }
        public string StatusAddress { get; set; }

        public ActorAction Action { get; set; }

        public List<string> Groups { get; set; }
    }
}