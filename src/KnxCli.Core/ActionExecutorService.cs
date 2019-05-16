
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using KnxCli.Core.Actors;
using KNXLib;

namespace KnxCli.Core
{
    public class ActionExecutorService
    {
        public ActionExecutorService(KnxSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.settings = settings;
        }

        private KnxSettings settings;


        public bool Execute(string mode, IEnumerable<Actor> actors, ActorAction action, bool verbose)
        {
            if (actors == null)
            {
                throw new ArgumentNullException(nameof(actors));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }


            var finalMode = CheckAndGetMode(mode);

            var hostName = Dns.GetHostName();
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            var myIPs = localIPs
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                .Select(l => l.ToString());

            foreach (var myIP in myIPs)
            {
                try
                {
                    KnxConnection connection = null;

                    if (finalMode == 1)
                    {
                        connection = new KnxConnectionTunneling(
                            settings.KnxGatewayIP,
                            settings.KnxGatewayPort,
                            myIP,
                            settings.KnxLocalPort)
                        {
                            Debug = verbose
                        };
                    }
                    else if (finalMode == 2)
                    {
                        connection = new KnxConnectionRouting(
                            settings.KnxGatewayIP,
                            settings.KnxGatewayPort)
                        {
                            Debug = verbose
                        };
                    }
                    else
                    {
                        throw new ArgumentException("Unknown mode.");
                    }

                    connection.Connect();
                    //connection.KnxEventDelegate += Event;

                    foreach (var actor in actors)
                    {
                        if (actor.Action.Type.Equals("Switch", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var val = (bool)action.Value;
                            connection.Action(actor.Address, val);
                        }
                        else
                        {
                            Console.Error.WriteLine("Unknown actor action type");
                            return false;
                        }
                    }

                    connection.Disconnect();

                    return true;
                }
                catch (Exception ex)
                {
                    if (verbose)
                    {
                        Console.Error.WriteLine($"Can't use local IP '{myIP}'.");
                        Console.Error.WriteLine(ex.Message);
                    }
                }
            }

            return false;
        }

        private static int CheckAndGetMode(string mode)
        {
            if (string.IsNullOrEmpty(mode))
            {
                throw new ArgumentException(nameof(mode));
            }

            if (mode.Equals("Tunneling", StringComparison.InvariantCultureIgnoreCase))
            {
                return 1;
            }

            if (mode.Equals("Routing", StringComparison.InvariantCultureIgnoreCase))
            {
                return 2;
            }

            throw new ArgumentException("Unknown mode.", nameof(mode));
        }
    }
}