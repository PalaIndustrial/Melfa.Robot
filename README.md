# Melfa.Robot

This is a implementation of [Mitsubishi Electronics Factory Automation (MELFA)
](https://www.mitsubishielectric.com/fa/) robot R-protocol client in C#.

The R-protocol is described in **BFP-A4288**, the version this project consult is 
**BFP-A4288-V (2018/11/15)**.

# Configure the robot controller

Go to "Parameter -> Communication Parameterr -> Ethernet", Check the "Device & Line" tab.

Make sure one of OPT11 ~ OPT19 is configured as `Server`. For example:
- Mode (NETMODE): 1 - Server
- Port (NETPORT): 12345 # This is the port used to connect to the controller
- Protocol (OPCE11): 0 - No-procedure # This is probably irrelevant
- Packet Type (CTERME): 0 - CR

The client sends commands terminated with CR. By default it accepts responses
without a terminator using a short-read convention, for compatibility with
controllers and simulators that omit one. This is a heuristic: TCP reads do not
preserve packet or message boundaries, so fragmented responses can be truncated
or confused with the next response. A response whose read exactly fills the
1536-byte buffer may wait for more data and time out.

If the controller reliably terminates every response with CR, set
`robot.RequireResponseTerminator = true` before sending commands. This mode
handles fragmented responses but does not support CR+LF. Do not rely on the
short-read convention where an incomplete response could lead to unsafe actions.

# Usage
## Create the robot object
```C#
var robot = new MelfaRobot("localhost", 10001, 1); // use the port configured above
robot.Connect();
var info = robot.Open("ClientName");
```

## Do something with the robot
```C#
robot.OperationEnabled = true; // CTRLON
robot.ServoOn = true; // SRVON
robot.Override = 10; // OVRD=10
robot.Execute("MOV P1");
```

## Load a program and run
```C#
robot.RunProgram("blah");
```

## Read variables
```C#
var value = robot.ReadGlobalVariable("M_01", VariableClass.Integer);
var page = robot.ReadVariables(LocationType.Top, VariableClass.Integer, slot: 1);
foreach (var variable in page.Variables)
    Console.WriteLine($"{variable.Key} = {variable.Value}");
while (page.HasMore)
{
    page = robot.ReadVariables(LocationType.Next, VariableClass.Integer, slot: 1);
    // Process page.Variables.
}
```

`ReadGlobalVariables` works the same way for global variables. Pages retain
the controller's variable order and continuation flag; use the same class and
slot for subsequent requests. Controller variable browsing is stateful, so do
not interleave other variable reads while paging. The protocol manual shows
both VT and semicolon-plus-VT separators; a semicolon immediately before VT is
treated as a separator, making an unquoted value ending in semicolon ambiguous.

## Read servo monitor data
```C#
var encoder = robot.ServoEncoder;
var j1PositionFeedback = encoder[0].PositionFeedback;
var commandedCurrent = robot.ServoCurrentCommands;
var feedbackCurrent = robot.ServoCurrentFeedback;
var voltage = robot.ServoVoltage;
```

Servo monitor arrays contain J1 through J8 in order, including unused axes.
Each property reads a new controller snapshot.

## Read error history and summaries
```C#
var latest = robot.GetErrorHistory(LocationType.Top);
var filtered = robot.GetFilteredErrorHistory(LocationType.Top, "414000000", ErrorSeverity.Low);
var summary = robot.GetErrorSummaryWithLastOccurrence(1);
var start = robot.ErrorLogStartDate;
```

History reads return `null` when no entry is available. Summary responses retain
the total count even when `Entry` is `null`. Dates and times are returned as
controller strings (`yy-MM-dd` and `HH:mm:ss`) without assuming a century or
time zone. History navigation and filters use a controller-side cursor shared
with other history reads; do not interleave independent traversals. `ERRLOG2=`
and `ERRSUM2=` are not available on all controller models.

## Clean-up
```C#
robot.Close();
robot.Disconnect();
robot.Dispose();
```

`Dispose` disconnects immediately; call `Close` first when a protocol-level
close is required. `CommandTimeout` sets the maximum command exchange time in
milliseconds (default 5000); increase it for operations such as
`ExecuteBlocking`. A failed or incomplete exchange disconnects the client:
the command may already have executed, so reconnect and check controller state
before deciding whether to retry.

`Disconnect` can interrupt a pending exchange, but commands on one connection
are serialized. Do not rely on this library or network connection as an
emergency stop; use the controller's independent safety systems.

# Tests

Run `dotnet test .\Melfa.Robot.Tests\Melfa.Robot.Tests.csproj`.
The default tests use a loopback controller and do not require robot hardware.
Robot and simulator integration tests are opt-in; see
[`Melfa.Robot.Tests\README.md`](Melfa.Robot.Tests/README.md).

# TODO
- Finish implement of all functionalities
- Add more error log items (which is tedious...)

# Disclaimer

**The author of this software is not affiliated with
[Mitsubishi Electronics Factory Automation (MELFA)
](https://www.mitsubishielectric.com/fa/) in any way. All
trademarks and registered trademarks are property of their respective owners,
and company, product and service names mentioned in this readme or appearing in
source code or other artifacts in this repository are used for identification
purposes only.**

**Use of these names does not imply endorsement by either [Mitsubishi Electronics Factory Automation (MELFA)
](https://www.mitsubishielectric.com/fa/).**