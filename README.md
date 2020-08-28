# KNX.net-core (KNX.net for .NET Core >= 2.x)

## General info

KNX.net-core (KNX.net for .NET Core >= 2.x) is a port of [lifeemotions KNX.net] (https://github.com/lifeemotions/knx.net) for [Microsofts .NET Core] (https://www.microsoft.com/net/core).

KNX.net-core provides a [KNX](http://en.wikipedia.org/wiki/KNX_%28standard%29) API for .NET Core based in .NET Standard dll.


## Commandline client

This fork of KNX.net (here called KNX.net-core) now contains a configurable commandline client is available.

Usage examples:
```
dotnet KnxCli.dll -- --help
dotnet KnxCli.dll -- Light1 on 
dotnet KnxCli.dll -- Light1 off 
```

Configuration:
actors.json to configure the actors:
```json
[
    {
        "Name": "Light1",
        "Description": "Light 1 in room 1",
        "Address": "10/5/70",
        "StatusAddress": "10/5/71",
        "Action": {
            "Type": "Switch"
        },
        "Groups": [
            "room1"
        ]
    }
]
```




## API information

This API allows to connect in both modes:
* Tunneling
* Routing

After connecting you will be able to send actions to the bus and receive messages from it.

The following datapoints are available in the API:

| DPT     | input type                                  | input range     | output type | output range    | Description                         |
| ------- |-------------------------------------------- | --------------- | ----------- | --------------- | ----------------------------------- |
| `3.007` | `int`, `float`, `long`, `double`, `decimal` | `[-7,7]`        | `int`       | `[-7,7]`        | Control dimming (steps) [`0` stops] |
| `3.008` | `int`, `float`, `long`, `double`, `decimal` | `[-7,7]`        | `int`       | `[-7,7]`        | Control blinds (steps) [`0` stops] |
| `5.001` | `int`, `float`, `long`, `double`, `decimal` | `[0,100]`       | `decimal`   | `[0,100]`       | Percentage (%)                      |
| `5.003` | `int`, `float`, `long`, `double`, `decimal` | `[0,360]`       | `decimal`   | `[0,360]`       | Angle (°)                           |
| `5.004` | `int`, `float`, `long`, `double`, `decimal` | `[0,255]`       | `int`       | `[0,255]`       | Percentage `[0,255]` (%)          |
| `5.010` | `int`, `float`, `long`, `double`, `decimal` | `[0,255]`       | `int`       | `[0,255]`       | Counter Pulses                      |
| `6.001` | `int`, `float`, `long`, `double`, `decimal` | `[-128,127]`    | `int`       | `[-128,127]`    | Percentage (%)                      |
| `6.010` | `int`, `float`, `long`, `double`, `decimal` | `[-128,127]`    | `int`       | `[-128,127]`    | Counter Pulses                      |
| `9.001` | `int`, `float`, `long`, `double`, `decimal` | `[-273,670760]` | `decimal`   | `[-273,670760]` | Temperature in Celsius (°C)         |

Also working but no implemented as datapoints (see below for better explanation):
* bit (lights, buttons)
* byte (dimmers, temperature difference, RGB)

Examples
--------

### Connecting using Routing (turn off and on a light)

```csharp
static void Main(string[] args)
{
  var connection = new KnxConnectionRouting();
  connection.Connect();
  connection.KnxEventDelegate += Event;
  connection.Action("5/0/2", false);
  Thread.Sleep(5000);
  connection.Action("5/0/2", true);
  Thread.Sleep(5000);
}
static void Event(string address, string state)
{
  Console.WriteLine("New Event: device " + address + " has status " + state);
}
```

### Working with datapoints

Sending an action

```csharp
connection.Action("1/1/16", connection.ToDataPoint("9.001", 24.0f));
connection.Action("1/1/17", connection.ToDataPoint("5.001", 50));
```

Converting state from event

```csharp
static void Event(string address, string state)
{
  if (address == "1/1/16")
  {
    decimal temp = (decimal)connection.FromDataPoint("9.001", state);
    Console.WriteLine("New Event: device " + address + " has status " + temp);
    return;
  }
  if (address == "1/1/17")
  {
    int perc = (int)connection.FromDataPoint("5.001", state);
    Console.WriteLine("New Event: device " + address + " has status " + perc);
    return;
  }
}

```

### Requesting status

Sending an action

```csharp
connection.KnxStatusDelegate += Status;
connection.RequestStatus("1/1/16");
connection.RequestStatus("1/1/17");
```

Converting state from status event

```csharp
static void Status(string address, string state)
{
  if (address == "1/1/16")
  {
    decimal temp = (decimal)connection.FromDataPoint("9.001", state);
    Console.WriteLine("New Event: device " + address + " has status " + temp);
    return;
  }
  if (address == "1/1/17")
  {
    int perc = (int)connection.FromDataPoint("5.001", state);
    Console.WriteLine("New Event: device " + address + " has status " + perc);
    return;
  }
}

```

### Sending actions without using datapoints

```csharp
connection.Action("1/1/19", true);
connection.Action("1/1/20", false);
connection.Action("1/1/21", 60);
connection.Action("1/1/22", 0x4E);
```

### Connecting using Tunneling

The only difference is how the connection object is created

```csharp
connection = new KNXConnectionTunneling(remoteIP, remotePort, localIP, localPort);
```

### Notes

If connecting in routing mode:
* make sure the system firewall allows incoming connections to the routing port (if not specified when connecting, default is `3671`)

If connecting in tunneling mode:
* make sure the system firewall allows incoming connections to the specified `localPort`
