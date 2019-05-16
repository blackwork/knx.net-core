
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


        public bool Execute(string connectionMode, IEnumerable<Actor> actors, ActorAction action, bool verbose)
        {
            if (actors == null)
            {
                throw new ArgumentNullException(nameof(actors));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }


            var finalConnectionMode = CheckAndGetConnectionMode(connectionMode);

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

                    if (finalConnectionMode == 1)
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
                    else if (finalConnectionMode == 2)
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

        private static int CheckAndGetConnectionMode(string connectionMode)
        {
            if (string.IsNullOrEmpty(connectionMode))
            {
                throw new ArgumentException(nameof(connectionMode));
            }

            if (connectionMode.Equals("Tunneling", StringComparison.InvariantCultureIgnoreCase))
            {
                return 1;
            }

            if (connectionMode.Equals("Routing", StringComparison.InvariantCultureIgnoreCase))
            {
                return 2;
            }

            throw new ArgumentException("Unknown connection mode.", nameof(connectionMode));
        }
    }
}