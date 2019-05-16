
using System;
using KnxCli.Core.Actors;

namespace KnxCli.Core
{
    public class ActionTypeParserService
    {
        public ActorAction Parse(Actor actor, string value)
        {
            var action = new ActorAction();

            // TODO: currenty only switch actions are supported.

            if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                value.Equals("on", StringComparison.InvariantCultureIgnoreCase))
            {
                action.Type = "Switch";
                action.Value = true;
            }
            else if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase) ||
                value.Equals("off", StringComparison.InvariantCultureIgnoreCase))
            {
                action.Type = "Switch";
                action.Value = false;
            }
            else
            {
                Console.WriteLine($"Can't parse action: '{action}'");
                return null;
            }

            return action;
        }
    }
}