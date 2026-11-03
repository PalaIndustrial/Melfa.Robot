# Robot integration tests

These tests are **disabled by default** and appear as skipped in xUnit discovery.
The default run has no robot network access. The unit tests use fakes.
The unit and protocol tests run on .NET 6, 8, and 10; robot integration tests
are available only on .NET 10 to avoid running them concurrently across frameworks.
The motion-command protocol tests use an in-process loopback controller only.
They check request framing and response handling, not motion, instruction
validity, or controller-specific slot selection.
The `RobotReadOnly` category communicates with either a real controller or a
simulator; it sends only `OPEN`, `PRGRD`, `PPOSF`, `JPOSF`, `HNDSTS`,
`OPNUMRD`, indexed `OPSTSRD`, `FOPEN` in read mode, `FREAD`, `FCLOSE`, and
`CLOSE`. The position tests report Cartesian and joint positions, checking
that X, Y, Z and the configured joint axes are finite. The hand test reports
output status without operating the hand. No motion, servo, I/O writes,
execution, or reset commands are sent. `OPEN`/`CLOSE` do change communication
session state. Run against an isolated, idle system under operator supervision.
The option-status test reports each indexed slot. Option availability and
response formats may vary.

The `SimulatorWrite` tests are temporarily skipped because creating a new
scratch program file has not been verified. They must not be enabled until a
safe creation method is established.

The `SimulatorWrite` category is **simulator only** when re-enabled. A string declaration is
not proof that an endpoint is a simulator: the operator must independently verify
the endpoint, network routing, and simulator identity. Never enable writes against
real hardware. Run and inspect the read-only tests successfully on that same
simulator first. Stop all tasks, isolate other clients and users, and ensure the
edit slot has no unsaved changes. These conditions cannot be verified reliably
by this library. Work from a backed-up simulator image.

The current write-test setup requires an absent scratch program name. It checks the exact
filename using the extension returned by `OPEN`, clears the confirmed-empty
edit slot, opens the name, adds an `END` line, and saves the program. It reopens
the program for the test, discards test edits, and removes the saved scratch
file. The joint-variable test initializes a scratch variable and reads it back
with `VAL` before testing changes and restoration. Creation and joint
initialization behavior may vary by simulator; failures are reported. The
joint-variable test reads `VAL` from the edit slot (`TASKMAX+1`), not task slot 1.

**Known slot limitation:** the library currently sends `ProgramEdit`,
`ProgramGetLine`, `ProgramSetVariable`, `ProgramGetVariableType`, and
`ProgramGetPosition` to slot 1, whereas the manual identifies the edit slot as
`TASKMAX+1`. The tests use these library APIs as-is; they do not bypass them with
raw protocol commands. A controller may reject these commands or read/write a
different slot, so a failed test can expose this existing library issue. Even a
pass does not establish that every edit command accessed the intended slot. To reduce the
risk of affecting an unrelated program, the write tests refuse to proceed unless
`PRGRD` reports slot 1 has no selected program. If the controller cannot report
an empty slot, the tests fail closed. This is not proof of isolation; the operator
must still confirm the simulator and all conditions above.

The preflight refuses existing or unknown `FCHECK` results, malformed extensions,
ambiguous task names, a selected scratch name, or a nonempty slot 1. These checks
are not an atomic lock and cannot prove isolation. If a command or connection
fails, its outcome may be unknown: especially after a failed `SAVE`, inspect
the edit slot and scratch filename manually before running anything else. The
tests attempt to discard an open edit and remove a confirmed saved scratch
file, reporting cleanup errors alongside primary failures. They do not retry
or reconnect automatically. Do not run multiple test processes or other clients
concurrently. Tests within one process are serialized across both categories.

## Configuration

For `ReadPreparedProgramFile`, manually create a program on the controller, preferably
containing only `Hlt` lines, and set `MELFA_TEST_READ_PROGRAM` to its name without
the extension. The test reads the file bytes and checks that the file is nonempty;
it prints ASCII bytes and escapes non-ASCII bytes as `\xNN`, up to ten lines,
with the number of remaining lines. It does not open the edit slot. Program
files may contain binary data rather than source text.

Set environment variables in the shell that launches discovery and execution, or
use a Visual Studio runsettings file as described below:

| Variable | Meaning |
|---|---|
| `MELFA_TEST_READ` | `1` enables read-only; unset/empty/`0` disables |
| `MELFA_TEST_WRITE` | `1` enables simulator writes independently; unset/empty/`0` disables |
| `MELFA_TEST_HOST` | Required when enabled; plain hostname, IPv4 or IPv6 |
| `MELFA_TEST_PORT` | 1–65535; default `10001` |
| `MELFA_TEST_ROBOT` | 1–3, never 0; default `1` |
| `MELFA_TEST_DELAY_MS` | 100–10000; default `150`; delay between test commands, except commands within `FileRead` |
| `MELFA_TEST_READ_PROGRAM` | Optional, enables the prepared-program file read test when read-only tests are enabled; 1–8 ASCII alphanumerics, no extension |
| `MELFA_TEST_TARGET` | For writes, exactly `SIMULATOR` |
| `MELFA_TEST_SIMULATOR_READ_CONFIRMED` | For writes, `1` confirms read-only testing completed on this simulator |
| `MELFA_TEST_EXCLUSIVE_SCRATCH_CONFIRMED` | For writes, `1` confirms isolation, idle edit slot, and dedicated scratch data |
| `MELFA_TEST_SCRATCH_PROGRAM` | For writes, intended absent program name, 1–8 ASCII alphanumerics, no extension |
| `MELFA_TEST_SCRATCH_LINE` | For writes, intended line number 1–999999 |
| `MELFA_TEST_SCRATCH_JOINT` | For writes, intended local joint variable, e.g. `J1`, `JPOS1`, or `P1`; its name need not indicate its type |

Malformed flags or enabled configuration fail before connecting. Do not change
the environment between discovery and execution. Command timing is shared across
test sessions in the process; do not run alongside another client or test process.
The file read uses `FileRead`, which sends its `FOPEN`, `FREAD` and `FCLOSE`
commands without the inter-command delay; run it only against an idle system.
Before enabling writes, ensure the chosen name is local to the scratch program
and does not refer to an existing global variable. The test initializes it
before checking its joint-position type.

## Visual Studio Test Explorer

1. Add an XML file named `robot.runsettings` to the solution. For an emulator
   at `192.168.1.192:10001`, use:

   ```xml
   <RunSettings>
     <RunConfiguration>
       <EnvironmentVariables>
         <MELFA_TEST_HOST>192.168.1.192</MELFA_TEST_HOST>
         <MELFA_TEST_PORT>10001</MELFA_TEST_PORT>
         <MELFA_TEST_READ>1</MELFA_TEST_READ>
         <MELFA_TEST_WRITE>0</MELFA_TEST_WRITE>
       </EnvironmentVariables>
     </RunConfiguration>
   </RunSettings>
   ```

2. Select **Test > Configure Run Settings > Select Solution Wide runsettings
   File**, then select `robot.runsettings`.
3. Rebuild and run the `RobotReadOnly` test in Test Explorer. If it still
   appears skipped, rebuild or restart Visual Studio to refresh discovery.

The settings apply to the selected test run, not just this project. Selecting
this file enables read-only connections; remove or deselect it when finished.
The example explicitly disables writes. To configure simulator-only writes,
first follow the isolation and confirmation requirements above and the
configuration table; never enable them for a real controller.

## Commands (PowerShell)

From the repository root, default offline tests:

```powershell
dotnet test .\Melfa.Robot.Tests\Melfa.Robot.Tests.csproj -f net10.0
```

Read-only, after independently verifying the endpoint:

```powershell
$env:MELFA_TEST_HOST = 'simulator-host'
$env:MELFA_TEST_READ = '1'
dotnet test .\Melfa.Robot.Tests\Melfa.Robot.Tests.csproj -f net10.0 --filter 'Category=RobotReadOnly'
```

To run only the option-status test against a verified simulator:

```powershell
$env:MELFA_TEST_HOST = 'simulator-host'
$env:MELFA_TEST_READ = '1'
dotnet test .\Melfa.Robot.Tests\Melfa.Robot.Tests.csproj -f net10.0 --filter 'FullyQualifiedName~ReadOptionStatuses'
```

The test is read-only and can also run against a real controller when configured;
the operator must verify the endpoint before enabling it.

To read a manually prepared program:

```powershell
$env:MELFA_TEST_HOST = 'simulator-host'
$env:MELFA_TEST_READ = '1'
$env:MELFA_TEST_READ_PROGRAM = 'TEST'
dotnet test .\Melfa.Robot.Tests\Melfa.Robot.Tests.csproj -f net10.0 --filter 'FullyQualifiedName~ReadPreparedProgramFile'
```

Remove the opt-in environment variables after use. Filters do not themselves
enable integration tests.
