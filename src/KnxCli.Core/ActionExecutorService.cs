
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using KnxCli.Core.Actors;
using KNXLib;

namespace KnxCli.Core
{
    public class ActionExecutorService : IDisposable
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

        private KnxConnection connection;

        public bool Connect(
            string connectionMode,
            bool verbose)
        {
            if (connection != null)
            {
                return true;
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
                    KnxConnection conn = null;
                    if (finalConnectionMode == 1)
                    {
                        conn = new KnxConnectionTunneling(
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
                        conn = new KnxConnectionRouting(
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

                    if (verbose)
                    {
                        conn.KnxEventDelegate += Event;
                        conn.KnxStatusDelegate += Status;
                    }

                    conn.Connect();

                    connection = conn;

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

        public bool Execute(
            IEnumerable<Actor> actors,
            ActorAction action)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (actors == null)
            {
                throw new ArgumentNullException(nameof(actors));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }


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
            return true;
        }

        public void Disconnect()
        {
            if (connection != null)
            {
                connection.Disconnect();
                connection = null;
            }
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

        private static void Event(string address, string state)
        {
            Debug.WriteLine("New Event: device " + address + " has status " + state);
        }

        private static void Status(string address, string state)
        {
            Debug.WriteLine("New Status: device " + address + " has status " + state);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}