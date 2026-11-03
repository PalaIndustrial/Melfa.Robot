# BFP-A4288-V (Unofficial Structured Reference)

> Source: [BFP-A4288-V.pdf](BFP-A4288-V.pdf), revision V (2018-11-15),
> using a noisy copy-and-paste text extraction. This is an unofficial companion,
> not a replacement for the manual. Verify safety-critical details, diagrams,
> tables and uncertain syntax against the PDF and the controller documentation.

The [command reference](BFP-A4288-V-commands.md) covers the individual commands
in manual sections 5.2.1 through 5.2.144.

## 1) Scope and protocol overview

- Document describes robot controller communication for PC/peripheral and controller-to-controller communication.
- Controller behavior is request/response only (command-driven).
- Protocol kinds listed:
  - Non-procedural
  - Procedural (RS-232)
- The configuration illustration lists the CRnD-700, CR-750 and CR-800 series.

## 2) Key transport format notes

### Command frame (non-procedural payload)

`[RobotNo];[SlotNo];<Command><Argument>`

- `RobotNo`:
  - values include `0,1,2,3`
  - omit => default `1`
  - some commands affect all robots when `0`
- `SlotNo`:
  - values include `0,1..33`
  - omit => default `1`
  - some commands affect all slots when `0`
  - `TASKMAX` defines multitask slots; edit slot is `TASKMAX+1` (example says default `TASKMAX=8` => edit slot `9`)
- Delimiters in command/answer can be:
  - semicolon `;`
  - VT (`0x0B`) for multi-line/more-data forms
- Maximum command size: `255` bytes.
- For non-procedural communication, the terminal character is set by parameter:
  CR or CR+LF.

### Procedural protocol (RS-232)

The manual shows a frame containing `STX`, ID, sub-ID, length high/low,
robot ID, command, checksum high/low and `ETX`. It also shows an ENQ/ACK/NAK/EOT
handshake and retry state chart. The extracted diagram does not reliably retain
the field annotations or state transitions; consult section 3.2 of the PDF
before implementing this protocol.

### Response frame

- Success: `QoK<Answer>`
- Error: `QeR<ErrorNo>`

### Timing guidance

- Recommended command interval while robot program is running: **>= 100 ms**.
  The manual describes this as the interval before the next request after the
  response has been received.

## 3) Command index (extracted, normalized)

Source: `BFP-A4288-V.pdf`, section 5.2, using the local copy/paste extraction. The extraction duplicates and interleaves text; the PDF remains authoritative. Obvious duplication has been removed, but source contradictions, damaged examples and uncertain field boundaries are flagged rather than silently resolved. A PDF text reader was not available for independent verification. Check the original before sending commands, especially motion, brake, initialization and reset commands.

## Conventions

Transmit format is `[<Robot No.>];[<Slot No.>];<Command><Argument>`; omitted robot and slot numbers default to 1. Robot 0 or slot 0 can affect all robots or slots for some commands. The edit slot is `TASKMAX+1` (normally 9). Non-procedural termination is configurable as CR or CR+LF; maximum command size is 255 bytes. The manual also describes a separate RS-232 procedural protocol.

`<...>` denotes a field, `[...]` an optional part, and `<VT>` in this reference means the single vertical-tab byte `0x0B`, **not** the characters `0b`. Responses generally start `QoK` on success or `QeR<Error No.>` on error. Some individual sections spell the error prefix `Qer` or example success `Qok`; these source inconsistencies are retained. Examples show request → response. `*` preserves the manual's “enable commands” marker; `CNTLON` enables the operation right required by commands such as program start and servo ON, but see the explicit exception in 5.2.135. The command list also has an “Only FR/F/SD/SQ” column: its table layout is uncertain in the extraction, so consult the PDF for model-specific marks.

The manual recommends at least 100 ms between successive communications while a robot program is running. It also says `PRGLOAD=` and `PRGUP` cannot be used while a program is paused; first reset the program with `SLOTINIT`.

Axis masks use bits 0–7 for X/J1, Y/J2, Z/J3, A/J4, B/J5, C/J6, L1/J7 and L2/J8, respectively, unless a section specifies otherwise.

## 5.2.1 OPEN= — Communication open

- Function: Open communication; send first when communicating from peripheral equipment.
- Syntax: `OPEN=<Device name>[;<LANG>]`. Device name is arbitrary alphanumeric; `LANG` selects the error-message language.
- Response: `QoK<XYZ axes pattern>;<JOINT axes pattern>;<Structure flag1>,<Structure flag2>;<JOG speed>;<Program ext.>;<Parameter ext.>;<Robot type>;<Controller>;<Series>;<DATE>;<Version>;<Language>;<Copyright>;<Robot info.>;<Serial No>;<Multitask No>`. The two axis masks are hex `00`–`FF`; flags are default structure flags; JOG speed contains seven hex values; extensions include `MB4`, `MB5`, `MB6` and `PRM`. Controller examples are `CRn-5xx`, `CRn-7xx`, `CR75x`, `CR800`; series is fixed `MELFA`; language is `JPN` or `ENG`; robot info is multi-robot information; serial number is fixed 1; multitask number is parameter `TASKMAX`. Date and version are the release date and system software version.
- Example: `1;1;OPEN=USERTOOL` → `QoK3F;3F;7,0;3,5,A,1E,32,46,64;MB5;PRM;RV-4A;CRn-5xx;MELFA;03-11-19;Ver.J4;ENG;COPYRIGHT(C)1999-2003 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;1;8;` (line wrapping and trailing semicolon reconstructed from duplicated extraction).
- Related: `CLOSE`.

## 5.2.2 CLOSE — Communication close

- Syntax: `CLOSE`; ends peripheral communication. Response: `QoK`.
- Example: `1;1;CLOSE` → `QoK`. Related: `OPEN=`.

## 5.2.3 CNTL — Operation enable/disable

- Syntax: `CNTL<ON/OFF>`; choose `ON` or `OFF`. Enables/disables operation rights. Response: `QoK`.
- Example: `1;1;CNTLON` → `QoK`.

## 5.2.4 LOAD= — Program open

- Syntax: `LOAD=<Program name>`; opens the named program for editing. Response: `QoK`.
- Example: `1;1;LOAD=100` → `QoK`. Related: `SAVE`, `NEW`.

## 5.2.5 SAVE — Program save and close

- Syntax: `SAVE`; saves edits and closes the program. Response: `QoK`.
- Example: `1;1;SAVE` → `QoK`. Related: `LOAD=`, `NEW`.

## 5.2.6 NEW — Program quit and close

- Syntax: `NEW`; closes the program, discarding edits. Response: `QoK`.
- Example: `1;1;NEW` → `QoK`. Related: `LOAD=`, `SAVE`.

## 5.2.7 EDATA — Edit program

- Syntax: `EDATA<Line or Position>`; register line or position data in the edit slot.
- Response: `QoK` or `Qer<Error no>;<Error character>`; error number and character position identify the registration failure.
- Example: `1;1;EDATA10 MOV P1` → `QoK`. Related: `EMDAT`.

## 5.2.8 EMDAT — Multiple-line program edit

- Syntax: `EMDAT<Line or Position>[<VT><Line or Position>…]`; register multiple lines/positions in the edit slot.
- Response: `QoK` or `Qer<Error no>;<Error line no>;<Error character>`; the latter includes the ordinal of the failing line and character position.
- Example: `1;1;EMDAT10 MOV P1<VT>20 MOV P2<VT>30 MOV P3` → `QoK`. Related: `EDATA`.

## 5.2.9 EDINS — Insert program line

- Syntax: `EDINS=<Insert line>;<Line data>`; line number and line data, in the edit slot.
- Response: `QoK` or `Qer<Error no>;<Error character>`.
- Example: `1;1;EDINS=3;MOV P1` → `QoK`. Related: `EMDAT`.

## 5.2.10 VAL= — Variable data write

- Syntax: `VAL=<Variable name>=<Value>`; changes a variable in the edit slot. Response: `QoK`.
- Example: `1;9;VAL=M1=3` → `QoK`.

## 5.2.11 LISTI — Read program line

- Syntax: `LISTI<Line posi.>`; edit slot. Position: `TOP`, `END`, `+1`, `-1`, or a line number.
- Response: `QoK[<Line contents>]`; only `QoK` if no specified line exists.
- Example: `1;1;LISTITOP` → `QoK10 MOV P1`. Related: `LISTL`.

## 5.2.12 LISTL — Read multiple program lines

- Syntax: `LISTL<Line pos.>`; positions as in LISTI; edit slot.
- Response: `QoK<Line contents>[<VT><Line contents>…];<Count>;<Continue>`; count is lines read; continuation is 1 if more, 0 otherwise. Only `QoK` if no specified line.
- Example: `1;1;LISTLTOP` → `QoK10 MOV P1<VT>20 MOV P2<VT>30 M1=1;3;0`. Related: `LISTI`.

## 5.2.13 LISTP — Read program position

- Syntax: `LISTP<Position posi.>`; `TOP`, `END`, `+1`, `-1`, or position name; edit slot.
- Response: `QoK<Position contents>`; only `QoK` if absent.
- Example: `1;1;LISTPTOP` → `QoKP1=(+400.13,+0.00,+644.62,+180.00,+0.00,+180.00)(7,0)`.

## 5.2.14 VTYPRD — Read variable type

- Syntax: `VTYPRD<Variable name>`; edit slot.
- Response: `QoK<Type>`, with `M` integer, `C` character, `P` position, `J` joint; only `QoK` if absent.
- Example: `1;9;VTYPRDP1` → `QoKP`.

## 5.2.15 LISTCNT — Count program lines

- Syntax: `LISTCNT<Start line>;<End line>`; count between specified line numbers.
- Response: `QoK<Number>`. Example: `1;1;LISTCNT10;50` → `QoK0005`.

## 5.2.16 EXEC * — Direct execution

- Syntax: `EXEC<Instruction>`; MELFA-BASIC IV–VI instruction or MOVEMASTER command. Responds when accepted.
- Response: `QoK`. Example: `1;9;EXECMOV P1` → `QoK`. Related: `EXEC2=`.

## 5.2.17 EXEC2= * — Direct execution

- Syntax: `EXEC2=<Instruction>`; MELFA-BASIC IV–VI instruction or MOVEMASTER command. Responds when execution completes.
- Response: `QoK`. Example: `1;9;EXEC2=MOV P1` → `QoK`. Related: `EXEC`.

## 5.2.18 STEP * — Step execution

- Syntax: `STEP<Method>`; edit slot. `1`: one step forward; `R`: continuous forward; `B`: one step backward; `S`: stop stepping.
- Response: `QoK`. Example: `1;9;STEP1` → `QoK`.

## 5.2.19 ECLR — Clear program contents

- Syntax: `ECLR`; clears the edit-slot program. Response: `QoK`.
- Example: `1;9;ECLR` → `QoK`. Related: `DELETE`.

## 5.2.20 DELETE — Delete program lines

- Syntax: `DELETE<Start line>;<End line>`; deletes the specified line range. Response: `QoK`.
- Example: `1;1;DELETE10;50` → `QoK`. Related: `ECLR`.

## 5.2.21 RENUM — Renumber

- Syntax: `RENUM<New line no>;<Old start line no>;<Step>;<Old end line no>`; edit slot. Zero means 10 for new line and step, top for old start, bottom for old end.
- Response: `QoK`. Example: `1;9;RENUM100;0;0;0` → `QoK`.

## 5.2.22 PDIR — Program directory

- Syntax: `PDIR<Posi.>`; `TOP` for first information or a numeric position.
- Response for first information: `QoK<Program name>;<Size>;<DATE><TIME>;<Count>;<Remain size>;<Attrib>;;<Line count>;<Position Count>;<Operation time>;<Latest tact>;<Avg.tact>;<Cycle count>`.
- Response for a position: `QoK<Program name>;<Size>;<DATE><TIME>;<Attrib>;;<Line count>;<Position Count>;<Running>;<Latest tact>;<Avg.tact>;<Cycle count>`. The manual calls the corresponding field operation time in its definitions; the format says `Running`.
- Size and remaining size are bytes/file-system space; count is number of programs; date `yy-mm-dd`, time `hh:mm:ss`; attrib is program attribute; operation and tact times are milliseconds; cycle count is operation count.
- Examples: `1;1;PDIRTOP` → `QoK1.MB5;235;04-02-0519:31:28;34;105984;12;;2;0;115850;60;60;1903`; `1;1;PDIR1` → `QoK1.MB5;235;04-02-0519:31:28;12;;2;0;115850;60;60;1903`. Related: `FDIR`.

## 5.2.23 FDIR — File directory

- Syntax: `FDIR<Posi.>`; `TOP` for first information, a numeric position, or `<*.*` for first information for all files.
- Response: `QoK<File name>;<Size>;<DATE><TIME>;<Count>;<Remain size>;<Attrib>` for first information, or `QoK<File name>;<Size>;<DATE><TIME>;<Attrib>` for a position. Size is bytes; date `yy-mm-dd`, time `hh:mm:ss`; count is file count, remaining size is file-system space, attrib is attribute.
- Examples: `1;1;FDIRTOP` → `QoK1.MB5;235;04-02-0519:31:28;46;105984;12`; `1;1;FDIR1` → `QoK1.MB5;235;04-02-0519:31:28;12`; `1;1;FDIR<*.*` → same first-information example. Related: `PDIR`.

## 5.2.24 FCHECK — File check

- Syntax: `FCHECK<File name>`; omitted extension defaults to `MB5`.
- Response: `QoK<Status>`: `N` exists, `F` does not.
- Example: `1;1;FCHECK100` → `QoKN` (request reconstructed from duplicated extraction; verify PDF).

## 5.2.25 FPATH= — File path

- Function: Obtain a file's saved path; omitted extension defaults to `MB5`.
- The PDF's format says `FPATH <File name>` (without `=`), but its heading and example use `FPATH=`. This is a confirmed inconsistency in the manual; verify the accepted syntax against the controller documentation or controller.
- Response: `QoK<File name>` including path. Example: `1;1;FPATH=1.MB5` → `QoK/robprg/dat/1.MB5`.

## 5.2.26 FCOPY — File copy

- Syntax: `FCOPY<Src.file>;<Dst.file>`; source and destination names; omitted extension defaults to `MB5`.
- Response: `QoK`. Example: `1;1;FCOPY1;2` → `QoK`.

## 5.2.27 FDEL — File delete

- Syntax: `FDEL<File name>`; omitted extension defaults to `MB5`.
- Response: `QoK`. Example: `1;1;FDEL1` → `QoK`.

## 5.2.28 FRENAME — File rename

- Syntax: `FRENAME<Src.file>;<Dst.file>`; omitted extension defaults to `MB5`.
- Response: `QoK`. Example: `1;1;FRENAME1;2` → `QoK`.

## 5.2.29 FATTRIB — File attribute

- Syntax: `FATTRIB<File name>;<Attrib>`; `+p` read-only lines, `-p` read/write lines, `+q` read-only variables, `-q` read/write variables.
- Response: `QoK`. Example: `1;1;FATTRIB1;+p` → `QoK`.

## 5.2.30 FINIT — File initialization

- Syntax: `FINIT`; initializes files. Response: `QoK`.
- Example: `1;1;FINIT` → `QoK`.

## 5.2.31 FOPEN — File block open

- Syntax: `FOPEN<File name>;<Mode>`; `w` block write, `r` block read.
- Response: `QoK`. Example: `1;1;FOPEN1.MB5;r` → `QoK`. Related: `FCLOSE`, `FREAD`, `FWRITE`.

## 5.2.32 FCLOSE — File block close

- Syntax: `FCLOSE`; closes the file opened for block I/O. Response: `QoK`.
- Example: `1;1;FCLOSE` → `QoK`. Related: `FOPEN`, `FREAD`, `FWRITE`.

## 5.2.33 FREAD — Block read

- Syntax: `FREAD`; reads a file block.
- Response: `QoK<Contents>;<Continue>`; contents are HEX ASCII; 1 means more data, 0 means no continuation.
- Example: `1;1;FREAD` → a long HEX ASCII block ending `;1`. The extraction interleaves the example's hex digits; consult the PDF for the exact bytes. Related: `FOPEN`, `FCLOSE`, `FWRITE`.

## 5.2.34 FWRITE — Block write

- Syntax: `FWRITE<Data size>;<Contents>`; size of content and content in HEX ASCII.
- Response: `QoK`. Example begins `1;1;FWRITE120;524F42505247` and returns `QoK`; the remaining hex is interleaved in the extraction and cannot be reproduced reliably. The source lists `FOPEN`, `FCLOSE`, `FWRITE` as related commands.

## 5.2.35 EFREE — Read file-system size

- Syntax: `EFREE`; response `QoK<Total size>;<Used size>;<Use rate>`, rate in percent.
- Example: `1;1;EFREE` → `QoK125696;15616;12`.

## 5.2.36 ESEARCH — Search string

- Syntax: `ESEARCH<String>`; searches the program being edited.
- Response: `QoK<Contents>;<Column>`, line content and matching column.
- Example: `1;9;ESEARCHMov` → `QoK3 Mov P1;2`.

## 5.2.37 PRGLOAD= * — Program load

- Syntax: `PRGLOAD=<Program name>`; loads the program into a task slot. Response: `QoK`.
- Example: `1;1;PRGLOAD=100` → `QoK`. Related: `PRG<UP/DOWN>`.

## 5.2.38 PRG&lt;UP/DOWN&gt; * — Program select

- Syntax: `PRG<UP/DOWN>`; select `UP` or `DOWN` in the task slot. Response: `QoK`.
- Example: `1;1;PRGUP` → `QoK`. Related: `PRGLOAD`.

## 5.2.39 PRGRD — Read execution program name

- Syntax: `PRGRD`; response `QoK<Program name>` for the task slot.
- Example: `1;1;PRGRD` → `QoK100.MB5`.

## 5.2.40 LINENO= * — Change execution line number

- Syntax: `LINENO=<Line no.>`; response `QoK`.
- Example: `1;1;LINENO=100` → `QoK`. Related: `LINENO`.

## 5.2.41 LINENO — Read execution line number

- Syntax: `LINENO`; response `QoK<Line no.>`.
- Example: `1;1;LINENO` → `QoK100`. Related: `LINENO=`.

## 5.2.42 LINERD — Read execution line contents

- Syntax: `LINERD`; response `QoK<Contents>`.
- Example: `1;1;LINERD` → `QoK10 MOV P1`. Related: `LINESRD`.

## 5.2.43 LINESRD — Read multiple execution lines

- Syntax: `LINESRD<Continue>;<Prev.line>;<Next line>`; continue 0 starts a read, 1 continues; the other fields give counts before and after the executing line.
- Response: `QoK<Continue>;<Line count>;<Contents>[<VT><Contents>…];<Line no.>`; continuation 1 means more, 0 means none; line number is execution line.
- Example: `1;1;LINESRD0;2;2` → `QoK0;5;10 '### Program ###<VT>20 '<VT>30 *SMAIN<VT>40 MOV P0<VT>50 DLY 1;0010`. Related: `LINERD`.

## 5.2.44 SRV&lt;ON/OFF&gt; * — Servo power

- Syntax: `SRV<ON/OFF>`; select `ON` or `OFF`. Response: `QoK`.
- Example: `1;1;SRVON` → `QoK`.

## 5.2.45 OVRD= * — Change OP override

- Syntax: `OVRD=<Override>`; range 1–100.
- Response: `QoK<Override>`, the set value. Example: `1;1;OVRD=50` → `QoK50`. Related: `OVRD`.

## 5.2.46 OVRD — Read OP override

- Syntax: `OVRD`; response `QoK<Override>`, current value.
- Example: `1;1;OVRD` → `QoK50`. Related: `OVRD=`.

## 5.2.47 RUN * — Program start

- Syntax: `RUN[<Program name>;<Mode>]`; omission selects current program; mode `0` repeat or `1` cycle, default repeat.
- Response: `QoK`. Example: `1;1;RUN100;1` → `QoK`. Related: `STOP`, `CSTOP`.

## 5.2.48 STOP — Stop

- Syntax: `STOP`; stops execution. Response: `QoK`.
- Example: `1;1;STOP` → `QoK`. Related: `STOP<ON/OFF>`, `RUN`.

## 5.2.49 STOP&lt;ON/OFF&gt; — Stop ON/OFF

- Syntax: `STOP<ON/OFF>`; after `STOPON`, starting is prohibited until `STOPOFF`.
- Response: `QoK`. Example: `1;1;STOPON` → `QoK`. Related: `STOP`, `CSTOP`, `RUN`.

## 5.2.50 CSTOP — Cycle stop

- Syntax: `CSTOP`; stops the running program at the cycle. Response: `QoK`.
- Example: `1;1;CSTOP` → `QoK`. Related: `STOP`, `STOP<ON/OFF>`, `RUN`.

## 5.2.51 RSTALRM — Error reset

- Syntax: `RSTALRM`; response `QoK`. Example: `1;1;RSTALRM` → `QoK`.

## 5.2.52 SLOTINIT — Reset all programs

- Syntax: `SLOTINIT`; resets programs in all slots. Response: `QoK`.
- Example: `1;1;SLOTINIT` → `QoK`. Related: `RSTPRG`.

## 5.2.53 RSTPRG — Reset each program

- Syntax: `RSTPRG`; resets the program in the specified slot. Response: `QoK`.
- Example: `1;1;RSTPRG` → `QoK`. Related: `SLOTINIT`.

## 5.2.54 RSTIO — Reset output signals

- Syntax: `RSTIO`; resets general-purpose outputs. Response: `QoK`.
- Example: `1;1;RSTIO` → `QoK`.

## 5.2.55 MLOCK&lt;ON/OFF&gt; — Machine lock

- Syntax: `MLOCK<ON/OFF>`; choose `ON` or `OFF`. Response: `QoK`.
- Example: `1;1;MLOCKON` → `QoK`.

## 5.2.56 HND&lt;ON/OFF&gt; — Hand open/close

- Syntax: `HND<ON/OFF><Hand no.>`; select `ON` or `OFF` and hand number 1–8.
- Response: `QoK`. Example: `1;1;HNDON1` → `QoK`.

## 5.2.57 ALIGN * — Align hand

- Syntax: `ALIGN`; aligns the hand. Response: `QoK`.
- Example: `1;1;ALIGN` → `QoK`.

## 5.2.58 MOVSP * — Move to safe position

- Syntax: `MOVSP`; moves to the safe position in parameter `JSAFE`. Response: `QoK`.
- Example: `1;1;MOVSP` → `QoK`.

## 5.2.59 JOG * — Jog operation

- Syntax: `JOG<Jog mode>;<Reserve>;<-Dire.>;<+Dire.>;<Inch>`. Modes: `00` JOINT, `01` XYZ, `02` TOOL, `03` reserved, `04` 3-axis XYZ, `05` CYLNDER. Reserve is fixed `00`; direction fields specify negative and positive axis masks (see conventions). Inch: `00` off, `01` high, `02` low.
- Jog stops automatically if the command is not received for about 140 ms. Response: `QoK`.
- Source example: `JOG01;00;01;00;00;00` → `QoK`. **Inconsistency:** example has one more field than the documented format; do not assume its meaning.

## 5.2.60 JOG * — Multifunctional electric hand

- Syntax: `JOG<Jog mode>;<Reserve>;<Speed>;<Force>;<Point No>`. Reserve fixed `00`; speed and force `01`–`64` hex; point `00`–`20` hex.
- Jog-mode bits 0–2: `000B` origin (`EHOrg`), `001B` open (`EHOpen`), `010B` close (`EHClose`), `011B` move (`EHMov`), `100B` hold (`EHHold`). The extraction does not explain the other mode bits.
- Stops automatically if the command is not received for about 140 ms. Response: `QoK`. Example: `JOG44;00;46;32;0A` → `QoK`.

## 5.2.61 LS&lt;ON/OFF&gt; — Limit switch

- Syntax: `LS<ON/OFF>`; `ON` enables, `OFF` disables. Response: `QoK`.
- Example: `1;1;LSON` → `QoK`.

## 5.2.62 AUE&lt;ON/OFF&gt; — Program start enable/disable

- Syntax: `AUE<ON/OFF>`; `ON` enables, `OFF` disables. Response: `QoK`.
- Example: `1;1;AUEON` → `QoK`. Related: `ATENA`.

## 5.2.63 ATENA — Read start eligibility

- Syntax: `ATENA`; response `QoK<Status>`: `0` start enabled, `1` disabled.
- Example: `1;1;ATENA` → `QoK1`. Related: `AUE<ON/OFF>`.

## 5.2.64 BRKPTSET — Set breakpoint

- Syntax: `BRKPTSET=<Program name>;<Line number>;<Breakpoint type>`; type `0` continuous, `1` one-time. Cannot set while running; if multiple settings target the same line, the last type applies.
- Response: `QoK`. Source example `1;1; BRKPTSET10;20;0` → `QoK1` conflicts with both documented format (`=`) and response; space is also shown in extraction. Verify PDF.
- Related: `BRKPTCLR`, `BRKPTGET`.

## 5.2.65 BRKPTCLR — Delete breakpoint

- Syntax: `BRKPTCLR=<Program name>;<Line number>`; line `0` deletes all.
- Response: `QoK`. Source example `1;1; BRKPTCLR10;20` → `QoK1` conflicts with documented format and response; verify PDF.
- Related: `BRKPTSET`, `BRKPTGET`.

## 5.2.66 BRKPTGET — List breakpoints

- Syntax: `BRKPTGET=<Program name>;<Breakpoint number>;<Start line>;<End line>`; omitted program name means all programs; breakpoint number 1–128; line bounds specify read range.
- Response: `QoK<Program name>;<Line number>;<Breakpoint type>`, type `0` continuous, `1` one-time.
- Source example is `1;1; BRKPTSET` → `QoK1`, apparently inconsistent with this command; verify PDF. Related: `BRKPTSET`, `BRKPTCLR`.

## 5.2.67 TOOLSET — Set tool number

- Syntax: `TOOLSET=<Tool number>`; 0–16. Response: `QoK`.
- Source example `1;1;TOOLSET2` → `QoK` omits the documented `=`. Related: `TOOLRD`.

## 5.2.68 TOOLRD — Read tool number

- Syntax: `TOOLRD`; response `QoK<Tool number>`, 0–16.
- Example: `1;1;TOOLRD` → `QoK2`. Related: `TOOLSET`.

## 5.2.69 SAFE= — Low-speed mode

- Syntax: `SAFE=<1/0>`; `1` setting valid, `0` invalid.
- The source calls the response `QoK<Tool number>`; example: `1;1;SAFE=1` → `QoK2`. The tool-number response is inconsistent with this function and requires verification.

## 5.2.70 STATE — Read run status

- Syntax: `STATE`.
- Response format as extracted: `QoK<Program name>;<Line no.>;<Override>;<Edit sts.>;<Run sts.><Stop sts.><Error no.>;<Step no.>;<Mech info.>;;;;;;;;;<Task prg.name>;<Task mode>;<Task cond.>;<Task pri.>;<Mech no.>`. The exact number of reserved semicolons is uncertain due to interleaved extraction; verify PDF.
- Program name is loaded in the task slot; line and step are executing line/step; error number 0 means no error; mech info bits 0–2 represent mechanisms 1–3. Task fields are slot-table program, mode `REP`/`CYC`, start condition `START`/`ALWAYS`/`ERROR`, priority 1–31 and mechanism in use.
- Edit status bits 0–2: editing, running, changed. Run and stop status bit definitions are in 5.2.71.
- Source example: `1;1;STATE` → `Qok2.MB5;100;100;5;A1124220;1;1;;;;;;;;;REP;START;1;0`; boundaries and capitalization require verification.

## 5.2.71 DSTATE — Read stop status

- Syntax: `DSTATE`; response `QoK<Run sts.><Stop sts.><Error no.>;<Step no.>;<Mech no.>`. Run and stop status are each two hex digits; error 0 means no error; other fields are executing step and mechanism in use.
- Run bits 0–7, with source's 0/1 descriptions: cycle/repeat; cycle stop ON/OFF; machine lock OFF/ON; auto/teach; running in teach mode; servo OFF/ON; STOP/RUN; operation disable/enable.
- Stop bits 0–6: emergency stop, stop, wait, stop signal ON/OFF, program select enabled, reserved, pseudo input.
- Example: `1;1;DSTATE` → `QoK09060;17;1`.

## 5.2.72 CALIB — Read installation status

- Syntax: `CALIB`; response `QoK<Status><Axis pattern><Reserve>`. Status `N` defined, `F` not defined; completion-axis mask is two hex digits, bits 0–7 J1–J8.
- Example: `1;1; CALIB` → `QoKN3F3F` (space reproduced from source example).

## 5.2.73 IOSIGNAL — Read input/output signals

- Syntax: `IOSIGNAL<IN no.>;<OUT no.>`; input/output signal numbers.
- Response: `QoK<IN state><OUT state>`; each state is four hex digits.
- Example: `1;1;IOSIGNAL0;0` → `QoK00000002`.

## 5.2.74 IN — Read input signal

- Syntax: `IN<IN no.>`; input signal number.
- Response: `QoK<IN state>`, four hex digits. Example: `1;1;IN0` → `QoK0000`.

## 5.2.75 OUT — Read output signal

- Syntax: `OUT<OUT no.>`; output signal number.
- Response: `QoK<OUT state>`, four hex digits. Example: `1;1;OUT0` → `QoK0001`.

## 5.2.76 OUT= — Write output signal

- Syntax: `OUT=<OUT no.>;<OUT val.>`; signal number and forced-output value, four hex digits.
- Response: `QoK`. Example: `1;1;OUT=0;0802` → `QoK`.

## 5.2.77 DIN — Read CC-Link input registers

- Syntax: `DIN<IN no.>`; CC-Link input register number. Requires CC-Link option.
- Response: `QoK<IN reg.state>;…` for 16 registers, in decimal.
- Example: `1;1;DIN6000` → `QoK0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0`. Related: `DOUT`.

## 5.2.78 DOUT — Read CC-Link output registers

- Syntax: `DOUT<OUT no.>`; CC-Link output register number. Requires CC-Link option.
- Response: `QoK<OUT reg.state>;…` for 16 registers, in decimal.
- Example: `1;1;DOUT6000` → `QoK0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0`. Source function text says “write” despite read heading, format and response. Related: `DIN`.

## 5.2.79 DOUT= — Write CC-Link output register

- Syntax: `DOUT=<OUT no.>;<OUT val.>`; register number and forced output value, four hex digits. Requires CC-Link option.
- Response: `QoK`. Example: `1;1;DOUT=6000;0002` → `QoK`. Related: `DIN`, `DOUT`.

## 5.2.80 INDMY — Set pseudo input

- Syntax: `INDMY`; enables pseudo input of input signals. Response: `QoK`.
- Example: `1;1;INDMY` → `QoK`. Related: `INSET`, `IN=`, `DIN=`.

## 5.2.81 INSET — Reset pseudo input

- Syntax: `INSET`; resets pseudo input. Response: `QoK`.
- Example: `1;1;INSET` → `QoK`. Related: `INDMY`, `IN=`, `DIN=`.

## 5.2.82 IN= — Write pseudo input

- Syntax: `IN=<IN no.>;<IN val.>`; signal number and pseudo-input value, four hex digits.
- Response: `QoK`. Example: `1;1;IN=0;0802` → `QoK`. Related: `INDMY`, `INSET`, `DIN=`.

## 5.2.83 DIN= — Write pseudo input register

- Syntax: `DIN=<IN no.>;<IN reg.val>`; input-register number and four-hex-digit pseudo-input value. Requires CC-Link option.
- Response: `QoK`. Example: `1;1;DIN=6000;0001` → `QoK`. Related: `INDMY`, `INSET`, `IN=`.

## 5.2.84 STPSIG — Read stop signals

- Syntax: `STPSIG`; response `QoK<Stop sts.>;<EMG sts.>`, hexadecimal masks.
- Stop bits 0–3: T/B (RS-422), P/C (RS-232C), I/O, O/P. Emergency-stop bits 0–2: I/O EMG, O/P EMG, T/B EMG.
- Example: `1;1;STPSIG` → `QoK0;0`.

## 5.2.85 HNDSTS — Read hand output

- Syntax: `HNDSTS`; response `QoK<Output no.>;<Hand sts.>;<Hand type>;…` for eight hands. Output number `-1` means unused; status `1` open, `2` closed; type `0` single-solenoid, `1` double-solenoid.
- Example begins `1;1;HNDSTS` → `QoK900;2;1;902;1;1;904;1;1;906;1;1;…`; the unused-hand part is interleaved in the extraction and cannot be reproduced reliably. The format's hand-type field is written in Japanese in the source.

## 5.2.86 USERAREASTS — Read user-specified areas

- Syntax: `USERAREASTS`; response `QoK<Status>` as a hex mask, bits 0–7 corresponding to areas 1–8.
- Example: `1;1;USERAREASTS` → `QoK0`.

## 5.2.87 JPOS, PPOS, XPOS, RPOS — Read current position

- Syntax: `<Type>POS<Axis info.>`; `J` joint, `P` XYZ, `X` 3-axis XYZ, `R` CYLNDER; axis info `1`–`8` single axis, `F` all axes.
- Response: `QoK<Axis name>;<Axis data>;…;<Flag1>,<Flag2>;<Override>;<End speed>;<+Limit><-Limit><reserve>`. Axis name/data repeat for the axis count; structure flags are significant only for XYZ; override is current, end speed mm/sec; limit masks are each two hex digits using the axis map in conventions. Check exact separator placement against PDF.
- Example: `1;1;PPOSF` → `QoKX;290.62;Y;-0.09;Z;11.26;A;-179.94;B;-0.26;C;179.93;L1;0.00;;6,0;100;0.00;00000000`.

## 5.2.88 GJPOS, GPPOS — Read destination position

- Syntax: `G<Type>POS<Axis info.>`; `J` joint or `P` XYZ; axis info `1`–`8` single axis, `F` all axes.
- Response and field meanings match 5.2.87. The source heading says destination, while its function text says current position; verify.
- Example: `1;1;GPPOSF` → `QoKX;290.62;Y;-0.09;Z;11.26;A;-179.94;B;-0.26;C;179.93;L1;0.00;;6,0;100;0.00;00000000`.

## 5.2.89 TIME — Read time

- Syntax: `TIME`; response `QoK<DATE><TIME>`, date `yy-mm-dd`, time `hh:mm:ss`.
- Example: `1;1;TIME` → `QoK04-02-0917:54:32`. Related: `TIME=`.

## 5.2.90 TIME= — Change time

- Syntax: `TIME=<DATE><TIME>`, date `yy-mm-dd`, time `hh:mm:ss`. Response: `QoK`.
- Example: `1;1;TIME=04-02-0917:00:00` → `QoK`.

## 5.2.91 PTIME — Read hour meter

- Syntax: `PTIME`; response fields are power-on time, all-mechanism servo-on time, mechanism 1–3 servo-on times, reserved zero fields, program operation time, another zero, battery accumulation time, battery remaining time, and mechanism-information mask.
- Times are hours; mechanism mask bits 0–2 identify mechanisms 1–3. The extracted response format merges a reserved `0` with `<Operation time>`; precise separator placement is uncertain.
- Example: `1;1;PTIME` → `QoK149;51;51;0;0;0;0;0;0;0;0;0;0;0;0;0;0;102;0;2715;11885;1`. Related: `PTIMEDEL=`.

## 5.2.92 PTIMEDEL= — Clear hour meter

- Syntax: `PTIMEDEL=<Kind>`; `T` power-on, `S` servo-on, `D` program operation, `B` battery accumulation, `Z` all.
- Response: `QoK`. Example: `1;1;PTIMEDEL=B` → `QoK`. Related: `PTIME`.

## 5.2.93 CYCLETIME — Read cycle time

- Syntax: `CYCLETIME<Program name>`.
- Response: `QoK<Operation time>;<Latest tact>;<Avg.tact>;<Cycle count>`; first three in milliseconds, last is operation count.
- Example: `1;1;CYCLETIME100` → `QoK224745;156;154;1454`. Related: `PDIR`, `CYCLECLR`.

## 5.2.94 CYCLECLR — Clear cycle time

- Syntax: `CYCLECLR<Program name>`; clears program operating time. Response: `QoK`.
- Example: `1;1;CYCLECLR100` → `QoK`. Related: `PDIR`, `CYCLETIME`.

## 5.2.95 ERROR — Read error number

- Syntax: `ERROR`; response `QoK[<Error no.>;<Error level>]`; only `QoK` if no error. Level `1` high, `2` low, `3` caution.
- Source example: `1;1;ERROR` → `Qok4140;2`. Related: `ERRORMES`.

## 5.2.96 ERRORMES — Read error contents

- Syntax: `ERRORMES<Error no.>`; response `QoK<Contents>`.
- Example: `1;1;ERRORMES4140` → `QoKThe program was not found`. Related: `ERROR`.

## 5.2.97 ERRORLOG — Read error history

- Syntax: `ERRORLOG<Hist.no.>`; `TOP` newest, `END` oldest, `+1` previous history, `-1` next history.
- Response: `QoK<DATE>;<TIME>;<Error no.>;<Error contents>;<Error level>;<Program name>;<Line no.>;<Error detail no.>`; date `yy-mm-dd`, time `hh:mm:ss`, level `1` high, `2` low, `3` caution. Only `QoK` if history empty.
- Example: `1;1;ERRORLOGTOP` → `QoK04-02-09;18:49:52;4140;The program was not found;2;S1;0;414000000`. Related: `ERRLOGCLR`, `ERRLOG2=`.

## 5.2.98 ERRLOG2= — Filtered error history

- Syntax: `ERRLOG2=<Hist. No.>;<Error level>;<Error details number>`; history position as 5.2.97; level `0` all (default), `1` high, `2` low, `3` caution; detail filter is 6–9 digits. For `+`/`-` history selection the last filter is used.
- Response and field meanings match 5.2.97; `QoK` alone if empty.
- Unavailable on CRn-500; available from CRnQ-700/CRnD-700 main software R2/S2. The source notes last position and reference error number are shared with `ERRORLOG` and `ERRORRD` when using `+`/`-`.
- Example: `1;1;ERRLOG2=TOP;0;414000000` → `QoK04-02-09;18:49:52;4140;The program was not found;2;S1;0;414000000`. Related: `ERRLOGCLR`, `ERRORLOG`.

## 5.2.99 ERRLOGCLR — Clear error history

- Syntax: `ERRLOGCLR<Error level>`; `1` high, `2` low, `3` caution, `0` all.
- Response: `QoK`. Example: `1;1;ERRLOGCLR1` → `QoK`. Source related command: `ERRLOG`.

## 5.2.100 ERRSUM — Error summary

- Syntax: `ERRSUM<No.>`; number 1–100 in numerical order.
- Response: `QoK<Number of errors>;<Detailed error number>;<Error level>;<Error contents>;<Cause>;<Treat>;<Count of error>`; level `H` high, `L` low, `C` caution. Detailed number `000000000` means no history; marked text fields are empty when there is no history.
- Examples: `1;1;ERRSUM1` → `QoK4;006000000;H;EMG signal is input. (O.Panel);EMG signal is input. (O.Panel);Check the O.Panel emergency stop;4`; `1;1;ERRSUM5` → `QoK4;000000000;;;;;0`. Related: `ERRSUMCLR`, `SUMDATE`, `ERRSUM2=`.

## 5.2.101 ERRSUM2= — Error summary with last occurrence

- Syntax: `ERRSUM2=<Hist. no.>`; number 1–100 in numerical order.
- Response: `QoK<Number of errors>;<Detailed error number>;<Error level>;<Error contents>;<Cause>;<Treat>;<Count of error>;<date>;<time>`; fields as in 5.2.100, followed by last occurrence date and time. No-history detailed number is `000000000`, marked text fields are empty.
- Unavailable on CRn-500; available from CRnQ-700/CRnD-700 main software R2/S2.
- Example: `1;1;ERRSUM2=1` → `QoK4;006000000;H;EMG signal is input. (O.Panel);EMG signal is input. (O.Panel);Check the O.Panel emergency stop;4;10-08-19;18:49:52`. A second source example uses `1;1;ERRSUM5` and returns `QoK4;000000000;;;;;0;;`; the request conflicts with this section and the extraction is ambiguous. Related: `ERRSUMCLR`, `SUMDATE`, `ERRSUM`.

## 5.2.102 ERRSUMCLR — Clear error summary

- Function: Clear error summary. The source's format and example instead both say `ERRLOGCLR` without an argument, unlike the section heading and 5.2.99; do not assume which spelling is correct.
- Source example: `1;1;ERRLOGCLR` → `QoK`. Related: `ERRSUM`, `ERRSUM2=`.

## 5.2.103 SUMDATE — Error logging start date

- Syntax: `SUMDATE<function no.>`; `0` error log, `1` error summary.
- Response: `QoK<date>;<time>`, data-collection start date `yy-mm-dd` and time `hh:mm:ss`.
- Example: `1;1;SUMDATE0` → `QoK10-08-09;20:23:46`. Related: `ERRLOGCLR`, `ERRSUMCLR`.

## 5.2.104 VAL — Read variable

- Syntax: `VAL<Variable posi.>;<Class>`; position `<` top, `>` bottom, `+1` next, `-1` previous, or variable name. Class `M` integer, `C` character, `P` position, `J` joint.
- Response: `QoK<Variable name>=<Value>`. Example: `1;1;VAL<;M` → `QoKM1=+3`. Related: `VALS`.

## 5.2.105 VALS — Read multiple variables

- Syntax: `VALS<Variable posi.>;<Class>`; position `<`, `>`, `+1`, `-1`; classes as in 5.2.104.
- Response: `QoK<Variable name>=<Value>[<VT><Variable name>=<Value>…];<Count>;<Continue>`; count is variables read, continue `1` means more, `0` none.
- Source example: `1;1;VALS<;M` → `QoKM1=+1;<VT>M2=+2;<VT>M3=+3;3;0`. The extra semicolons before VT are in the example but not the format. Related: `VAL`.

## 5.2.106 GVAL — Read global variable

- Syntax: `GVAL<Variable posi.>;<Class>`; same position and class options as 5.2.104.
- Response: `QoK<Variable name>=<Value>`. Example: `1;1;GVAL<;M` → `QoKM_01=+3`. Related: `GVALS`.

## 5.2.107 GVALS — Read multiple global variables

- Syntax: `GVALS<Variable posi.>;<Class>`; position `<`, `>`, `+1`, `-1`; classes as in 5.2.104.
- Response: `QoK<Variable name>=<Value>[<VT><Variable name>=<Value>…];<Count>;<Continue>`; continue `1` more, `0` none.
- Source example: `1;1;GVALS<;M` → `QoKM_01=+1;<VT>M_02=+2;<VT>M_03=+3;3;0`. As in 5.2.105 the example includes semicolons not shown in the format. Related: `GVAL`.

## 5.2.108 HOT — Write variable data

- Syntax: `HOT<Program name>;<Variable name>=<Value>`; changes a variable; an integer variable can be changed even during execution.
- Response: `QoK`. Example: `1;1;HOT100;M1=1` → `QoK`.

## 5.2.109 OPNUMRD — Read option-slot count

- Syntax: `OPNUMRD`; response `QoK<Slot no.>`, number of option slots.
- Example: `1;1;OPNUMRD` → `QoK3`. Related: `OPSTSRD`.

## 5.2.110 OPSTSRD — Read option information

- Syntax: `OPSTSRD<Slot no.>`; response: `QoK<Option info.>`. The manual lists slot numbers 1, 2 and 3, but slot 4 has also been observed.
- The unindexed `OPSTSRD` command is supported by the protocol and simulator, but its slot and behavior are unknown; one simulator returned only `;`. The library exposes only the indexed form.
- Source example request `1;1;OPSTSRD` omits the documented slot argument; response contains Ethernet address, real-time port, OPT11–19 ports and connection counts, MAC address and H/W version separated by VT. Extracted response: `QoKEthernet;IP Addr 192.168.0.1<VT>PortNo(R-time) 10000<VT>PortNo(OPT11-19) 10001/10002/10003/10004/10005/10006/10007/10008/10009<VT>Connect Count(OPT11-19) 2/0/0/0/0/0/0/0/0<VT>MAC Addr 00:00:00:00:00:00<VT>H/W Ver: 0`.
- Source related command: `OPSTSRD`.

## 5.2.111 THMRD — Read controller temperature

- Syntax: `THMRD`; response `QoK<Temperature>`.
- Example: `1;1;THMRD` → `QoK39.5`. Related: `ETEMP`.

## 5.2.112 ETEMP — Read encoder temperature

- Syntax: `ETEMP`; response `QoK<J1 Temperature>;<J2 Temperature>;…;<J8 Temperature>;<J1 Max.Temperature>;…;<J8 Max.Temperature>`. Maximum is since power-on.
- Example: `1;1;ETEMP` → `QoK38;39;41;39;36;36;0;0;41;42;44;41;38;37;0;0`. Related: `THMRD`.

## 5.2.113 EMISS — Read encoder miscount

- Syntax: `EMISS`; response `QoK<J1 Miscount>;<J2 Miscount>;…;<J8 Miscount>`.
- Example: `1;1;EMISS` → `QoK0;1;1;0;0;0;0;0`. The source's field description incorrectly labels miscount as temperature.

## 5.2.114 SRVENC — Read servo encoder

- Syntax: `SRVENC`; monitors axis-motor position.
- Response: `QoK<J1 pos. feedback>;<J1 pos. in 1 rot.>;<J1 Fdt command>;<J1 pos. command>;…` repeating those four fields for J2–J8: encoder position feedback, position in one rotation, Fdt command and position command.
- Example: `1;1;SRVENC` → `QoK-4475890;0;0;-4487192;4912356;0;0;4892959;10225275;0;0;10323293;-940046;0;0;-947494;-3330789;0;0;-3358040;6508941;0;0;6502064;0;0;0;0;0;0;0;0`. Example digits are reconstructed from interleaved extraction; verify against PDF.

## 5.2.115 SRVDRP — Read servo droop

- Syntax: `SRVDRP`; response `QoK<J1 pos. droop>;<J1 max. pos. droop>;…`, repeating current and maximum position droop for J1–J8.
- Example: `1;1;SRVDRP` → `QoK16860;1525531;22463;-1686967;126622;-543044;-11984;-318476;36426;2421963;9865;-2205319;0;0;0;0`.

## 5.2.116 SRVSPD — Read servo speed

- Syntax: `SRVSPD`; response `QoK<J1 spd. feedback>;<J1 speed max.>;<J1 speed command>;…`, repeating speed feedback, maximum and command for J1–J8.
- Example: `1;1;SRVSPD` → `QoK16;3755;68;29;-4148;115;147;-1150;-587;-11;-783;45;41;5964;163;10;-5429;41;0;0;0;0;0;0`.

## 5.2.117 SRVCUR — Read servo current

- Syntax: `SRVCUR<type>`; `0` command, `1` feedback.
- Type 1 response: `QoK<J1 cur. feedback>;<J1 max. curr. cmd1>;<J1 max. curr. cmd2>;…`, three fields per axis J1–J8: current feedback and two maximum current commands.
- Type 0 response: `QoK<J1 cur. command>;<J1 tolerable cmd+>;<J1 tolerable cmd->;<J1 RMS Current>;…`, four fields per axis J1–J8: current command, positive/negative tolerable command, RMS current.
- Example: `1;1;SRVCUR1` → `QoK-0.42;0.49;3.30;0.80;1.73;2.63;-0.57;0.67;0.83;0.24;0.24;0.86;0.13;0.39;0.87;-0.24;0.24;0.93;0.00;0.00;0.00;0.00;0.00;0.00`.

## 5.2.118 SRVLCR — Servo load current

- Syntax: `SRVLCR`; response `QoK<J1 load level>;<J1 max. load level>;…` repeating current and maximum load level for J1–J8. Function text also mentions encoder temperature, but the listed response does not.
- Example: `1;1;SRVLCR` → `QoK3.00;3.00;35.00;35.00;30.00;30.00;14.00;14.00;32.00;32.00;8.00;8.00;0.00;0.00;0.00;0.00`.

## 5.2.119 SRVVOL — Servo voltage

- Syntax: `SRVVOL`; monitors main-circuit power supply.
- Response: `QoK<Power voltate>;<Power voltate (max)>;<Power voltate(min)>;<J1 regen. level>;…;<J8 regen. level>`; power voltage, maximum/minimum and per-axis regeneration level. Field names preserve the source's `voltate` spelling.
- Example: `1;1;SRVVOL` → `QoK0;0;0;0;0;0;0;0;0;0;0`.

## 5.2.120 SVMONRST= — Reset servo monitor maximum

- Syntax: `SVMONRST=`; clears servo monitor data. Response: `QoK`.
- Example: `1;1;SVMONRST=` → `QoK`.

## 5.2.121 RAREAD= — Read serial number

- Syntax: `RAREAD=`; reads robot/controller serial number.
- Response: `QoK<Number>;<Serial number>;<Set date>`; robot number, robot or controller serial, setting date.
- Example: `1;1; RAREAD=` → `QoK1;xxxxxxxxxx;2014/04/09 17:14:02` (the number of `x` characters is uncertain in the duplicated extraction).

## 5.2.122 PRMINIT — Initialize parameters

- Syntax: `PRMINIT`; initializes parameters to shipping state. Response: `QoK`.
- Example: `1;1;PRMINIT` → `QoK`.

## 5.2.123 PNR — Read parameter

- Syntax: `PNR<Parameter name>`; reads parameters at the level corresponding to `KEYWD`.
- Response: `QoK<Parameter name>;<Value>;<Value count>`.
- Example: `1;1;PNRBZR` → `QoKBZR;1;1`. Related: `PRM=`.

## 5.2.124 PRM= — Write parameter

- Syntax: `PRM=<Parameter name>;<Value>`; changes parameters at the level corresponding to `KEYWD`. Response: `QoK`.
- Example: `1;1;PRM=BZR;1` → `QoK`. Related: `PNR`.

## 5.2.125 PAR — Read parameter

- Syntax: `PAR<Parameter name>`; reads all parameters regardless of `KEYWD`.
- Response: `QoK<Parameter name>;<Value>;<Value count>`.
- Example: `1;1;PARBZR` → `QoKBZR;1;1`. Related: `PAW=`.

## 5.2.126 PAW= — Write parameter

- Syntax: `PAW=<Parameter name>;<Value>`; changes all parameters regardless of `KEYWD`. Response: `QoK`.
- Example: `1;1;PAW=BZR;1` → `QoK`. Related: `PAR`.

## 5.2.127 PAW2= — Write parameter, reboot required

- Syntax: `PAW2=<name>;<value>`; writes any parameter regardless of `KEYWD`. Power must be turned off and on for the value to take effect.
- Unavailable on CRn-500; available from CRnQ-700/CRnD-700 main software R2/S2.
- Response: `QoK`. Example: `1;1;PAW2=BZR;1` → `QoK`. Related: `PAR`, `PAW=`.

## 5.2.128 PRMUNDO — Restore parameter

- Syntax: `PRMUNDO<Mech No.>;<Parameter name>`; restores shipping value; mechanism `0` selects common parameter.
- Response: `QoK`. Example: `1;1;PRMUNDO0;BZR` → `QoK`. Related: `PRMINIT`.

## 5.2.129 PRM= / PMR= — Read changed-parameter list

- The heading says `PRM=`, but format and example say `PMR=`; this conflicts with 5.2.124. Verify the PDF/controller documentation rather than assuming either is correct.
- Documented format: `PMR=<Continuation specification>`; `0` new read, `1` continued read. Available from main software N6/P6.
- Response: `QoK<Continuation>;<num>;<name>[<VT><name>…]`; continuation `0` off, `1` on; num is names read, separated by byte `0x0B`.
- Example: `1;1;PMR=0` → `QoK0;6;SVSSFR<VT>MEOFFZ<VT>MEINST<VT>MEINSZ<VT>MEINSD<VT>MEJAR`.

## 5.2.130 KEYWD — Keyword input

- Syntax: `KEYWDp<Keyword>`; changes parameter access level. Response: `QoK`.
- Example: `1;1;KEYWDpMELFA` → `QoK`. Related: `PNR`, `PRM=`.

## 5.2.131 SLOTRD — Read slot table

- Syntax: `SLOTRD`; response `QoK<Task prg.name>;<Task mode>;<Task cond.>;<Task pri>`. Program name; mode `REP`/`CYC`; start condition `START`/`ALWAYS`/`ERROR`; priority 1–31.
- Example: `1;2;SLOTRD` → `QoK2;REP;START;1`. Related: `SLOTSET`.

## 5.2.132 SLOTSET — Write slot table

- Syntax: `SLOTSET<Task prg.name>;<Task mode>;<Task cond.>;<Task pri>`; fields as in 5.2.131. Changes `SLTn` parameter; reboot required.
- Response: `QoK`. Example: `1;2;SLOTSET2;REP;START;1` → `QoK`. Related: `SLOTRD`.

## 5.2.133 ENCBATTM — Battery remaining time

- Syntax: `ENCBATTM`; response `QoK<Power time>;<Remain time>`, power-on and battery remaining times in hours.
- Example: `1;1;ENCBATTM` → `QoK51;11885`. Related: `PTIMEDEL=`.

## 5.2.134 BREAKON * — Release brake

- Syntax: `BREAKON<Axis pattern>`; hex mask bits 0–7 J1–J8. `00` locks the brake.
- Response: `QoK`. Example: `1;1;BREAKON3F` → `QoK`. Related: `BREAKONF`.

## 5.2.135 BREAKONF * — Release brake without operation right

- Syntax: `BREAKONF<Axis pattern>`; hex mask bits 0–7 J1–J8; `00` locks the brake. The function explicitly says no operation right is required despite the `*` marker.
- Response: `QoK`. Example: `1;1;BREAKONF3F` → `QoK`. Related: `BREAKON`.

## 5.2.136 HOME * — Set origin

- Syntax: `HOME<Org.type>;<Axis pattern>`; hex mask bits 0–7 J1–J8. Type `0` user origin, `1` mechanical stopper, `2` reserved, `3` calibration jig, `4` ABS, `5` eye mark, `6` origin data input (parenthesized in source), `7` user ABS.
- Response: `QoK`. Example: `1;1;HOME3;3F` → `QoK`. Related: `DATINST`.

## 5.2.137 AXDATINST * — Add additional axes for origin data

- Syntax: `AXDATINST`; adds additional axes to data-input origin settings. After execution, the `DATINST` arguments and `DATRD` response include the additional axes.
- Response: `QoK`. Example: `1;1;AXDATINST` → `QoK`. Related: `DATINST`, `DATRD`.

## 5.2.138 DATINST * — Set origin by data input

- Syntax: `DATINST<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<Checksum>` or `DATINST<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<J7 data>;<J8 data>;<Checksum>`.
- Response: `QoK`. Example: `1;1;DATINSTZ2UOGQ;Z2VX5I;Z2#46Z;Z2NXW5;004QA5;0XB462;Y91%K5` → `QoK`. Related: `AXDATINST`, `DATRD`.

## 5.2.139 DATRD — Read data-input origin

- Syntax: `DATRD`; response `QoK<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<Checksum>` or, with additional axes, `QoK<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<J7 data>;<J8 data>;<Checksum>`.
- Example: `1;1;DATRD` → `QoKZ2UOGQ;Z2VX5I;Z2#46Z;Z2NXW5;004QA5;0XB462;Y91%K5`. Related: `AXDATINST`, `DATINST`.

## 5.2.140 RSTPWR — Reset power

- Syntax: `RSTPWR`; reboots controller. Response: `QoK`.
- Example: `1;1;RSTPWR` → `QoK`. Related: `RPWRCHK=`.

## 5.2.141 RPWRCHK= — Check reset power

- Syntax: `RPWRCHK=`; response `QoK<Status>`: `0` impossible, `1` possible.
- Example: `1;1;RPWRCHK=` → `QoK1`. Related: `RSTPWR`.

## 5.2.142 MFTIME= — Maintenance forecast reset date

- Syntax: `MFTIME=<Type>`; `1` battery, `2` grease, `3` belt.
- Response: `QoK<J1 reset date>;<J2 reset date>;…;<J8 reset date>;<J1 reset count>;…;<J8 reset count>`; reset dates and counts per axis. The example uses separate semicolon-delimited date and time components, unlike the single date field in the format.
- Example request: `1;1;MFTIME=1`; response contains eight repetitions of `2014/04/09;17:06:16`, followed by eight zero counts. Exact bytes and field boundaries are uncertain in the duplicated extraction.

## 5.2.143 MFRST= — Reset maintenance forecast

- Syntax: `MFRST=<Axis bit>;<Reset bit>`; both are hex patterns `00`–`FF`. Axis bits 0–7 select J1–J8; reset bit 0 selects grease and bit 1 belt.
- Response: `QoK`. Example: `1;1;MFRST=20;1` → `QoK`.

## 5.2.144 MFFCST= — Read maintenance forecast

- Syntax: `MFFCST=<Type>`; `0` battery, `1` grease, `2` belt (note the different numbering from `MFTIME=`).
- Response: `QoK<J1 forecast hour>;<J2 forecast hour>;…;<J8 forecast hour>`.
- Example: `1;1;MFFCST=1` → `QoK23999;23999;23999;23999;23999;23999;0;0`.
# BFP-A4288-V command reference

Back to the [protocol overview and command index](BFP-A4288-V.md).

Source: user-provided `BFP-A4288-V.pdf`, section 5.2, using the local copy/paste extraction. The extraction duplicates and interleaves text; the PDF remains authoritative. Obvious duplication has been removed, but source contradictions, damaged examples and uncertain field boundaries are flagged rather than silently resolved. A PDF text reader was not available for independent verification. Check the original before sending commands, especially motion, brake, initialization and reset commands.

## Conventions

Transmit format is `[<Robot No.>];[<Slot No.>];<Command><Argument>`; omitted robot and slot numbers default to 1. Robot 0 or slot 0 can affect all robots or slots for some commands. The edit slot is `TASKMAX+1` (normally 9). Non-procedural termination is configurable as CR or CR+LF; maximum command size is 255 bytes. The manual also describes a separate RS-232 procedural protocol.

`<...>` denotes a field, `[...]` an optional part, and `<VT>` in this reference means the single vertical-tab byte `0x0B`, **not** the characters `0b`. Responses generally start `QoK` on success or `QeR<Error No.>` on error. Some individual sections spell the error prefix `Qer` or example success `Qok`; these source inconsistencies are retained. Examples show request → response. `*` preserves the manual's “enable commands” marker; `CNTLON` enables the operation right required by commands such as program start and servo ON, but see the explicit exception in 5.2.135. The command list also has an “Only FR/F/SD/SQ” column: its table layout is uncertain in the extraction, so consult the PDF for model-specific marks.

The manual recommends at least 100 ms between successive communications while a robot program is running. It also says `PRGLOAD=` and `PRGUP` cannot be used while a program is paused; first reset the program with `SLOTINIT`.

Axis masks use bits 0–7 for X/J1, Y/J2, Z/J3, A/J4, B/J5, C/J6, L1/J7 and L2/J8, respectively, unless a section specifies otherwise.

## 5.2.1 OPEN= — Communication open

- Function: Open communication; send first when communicating from peripheral equipment.
- Syntax: `OPEN=<Device name>[;<LANG>]`. Device name is arbitrary alphanumeric; `LANG` selects the error-message language.
- Response: `QoK<XYZ axes pattern>;<JOINT axes pattern>;<Structure flag1>,<Structure flag2>;<JOG speed>;<Program ext.>;<Parameter ext.>;<Robot type>;<Controller>;<Series>;<DATE>;<Version>;<Language>;<Copyright>;<Robot info.>;<Serial No>;<Multitask No>`. The two axis masks are hex `00`–`FF`; flags are default structure flags; JOG speed contains seven hex values; extensions include `MB4`, `MB5`, `MB6` and `PRM`. Controller examples are `CRn-5xx`, `CRn-7xx`, `CR75x`, `CR800`; series is fixed `MELFA`; language is `JPN` or `ENG`; robot info is multi-robot information; serial number is fixed 1; multitask number is parameter `TASKMAX`. Date and version are the release date and system software version.
- Example: `1;1;OPEN=USERTOOL` → `QoK3F;3F;7,0;3,5,A,1E,32,46,64;MB5;PRM;RV-4A;CRn-5xx;MELFA;03-11-19;Ver.J4;ENG;COPYRIGHT(C)1999-2003 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;1;8;` (line wrapping and trailing semicolon reconstructed from duplicated extraction).
- Related: `CLOSE`.

## 5.2.2 CLOSE — Communication close

- Syntax: `CLOSE`; ends peripheral communication. Response: `QoK`.
- Example: `1;1;CLOSE` → `QoK`. Related: `OPEN=`.

## 5.2.3 CNTL — Operation enable/disable

- Syntax: `CNTL<ON/OFF>`; choose `ON` or `OFF`. Enables/disables operation rights. Response: `QoK`.
- Example: `1;1;CNTLON` → `QoK`.

## 5.2.4 LOAD= — Program open

- Syntax: `LOAD=<Program name>`; opens the named program for editing. Response: `QoK`.
- Example: `1;1;LOAD=100` → `QoK`. Related: `SAVE`, `NEW`.

## 5.2.5 SAVE — Program save and close

- Syntax: `SAVE`; saves edits and closes the program. Response: `QoK`.
- Example: `1;1;SAVE` → `QoK`. Related: `LOAD=`, `NEW`.

## 5.2.6 NEW — Program quit and close

- Syntax: `NEW`; closes the program, discarding edits. Response: `QoK`.
- Example: `1;1;NEW` → `QoK`. Related: `LOAD=`, `SAVE`.

## 5.2.7 EDATA — Edit program

- Syntax: `EDATA<Line or Position>`; register line or position data in the edit slot.
- Response: `QoK` or `Qer<Error no>;<Error character>`; error number and character position identify the registration failure.
- Example: `1;1;EDATA10 MOV P1` → `QoK`. Related: `EMDAT`.

## 5.2.8 EMDAT — Multiple-line program edit

- Syntax: `EMDAT<Line or Position>[<VT><Line or Position>…]`; register multiple lines/positions in the edit slot.
- Response: `QoK` or `Qer<Error no>;<Error line no>;<Error character>`; the latter includes the ordinal of the failing line and character position.
- Example: `1;1;EMDAT10 MOV P1<VT>20 MOV P2<VT>30 MOV P3` → `QoK`. Related: `EDATA`.

## 5.2.9 EDINS — Insert program line

- Syntax: `EDINS=<Insert line>;<Line data>`; line number and line data, in the edit slot.
- Response: `QoK` or `Qer<Error no>;<Error character>`.
- Example: `1;1;EDINS=3;MOV P1` → `QoK`. Related: `EMDAT`.

## 5.2.10 VAL= — Variable data write

- Syntax: `VAL=<Variable name>=<Value>`; changes a variable in the edit slot. Response: `QoK`.
- Example: `1;9;VAL=M1=3` → `QoK`.

## 5.2.11 LISTI — Read program line

- Syntax: `LISTI<Line posi.>`; edit slot. Position: `TOP`, `END`, `+1`, `-1`, or a line number.
- Response: `QoK[<Line contents>]`; only `QoK` if no specified line exists.
- Example: `1;1;LISTITOP` → `QoK10 MOV P1`. Related: `LISTL`.

## 5.2.12 LISTL — Read multiple program lines

- Syntax: `LISTL<Line pos.>`; positions as in LISTI; edit slot.
- Response: `QoK<Line contents>[<VT><Line contents>…];<Count>;<Continue>`; count is lines read; continuation is 1 if more, 0 otherwise. Only `QoK` if no specified line.
- Example: `1;1;LISTLTOP` → `QoK10 MOV P1<VT>20 MOV P2<VT>30 M1=1;3;0`. Related: `LISTI`.

## 5.2.13 LISTP — Read program position

- Syntax: `LISTP<Position posi.>`; `TOP`, `END`, `+1`, `-1`, or position name; edit slot.
- Response: `QoK<Position contents>`; only `QoK` if absent.
- Example: `1;1;LISTPTOP` → `QoKP1=(+400.13,+0.00,+644.62,+180.00,+0.00,+180.00)(7,0)`.

## 5.2.14 VTYPRD — Read variable type

- Syntax: `VTYPRD<Variable name>`; edit slot.
- Response: `QoK<Type>`, with `M` integer, `C` character, `P` position, `J` joint; only `QoK` if absent.
- Example: `1;9;VTYPRDP1` → `QoKP`.

## 5.2.15 LISTCNT — Count program lines

- Syntax: `LISTCNT<Start line>;<End line>`; count between specified line numbers.
- Response: `QoK<Number>`. Example: `1;1;LISTCNT10;50` → `QoK0005`.

## 5.2.16 EXEC * — Direct execution

- Syntax: `EXEC<Instruction>`; MELFA-BASIC IV–VI instruction or MOVEMASTER command. Responds when accepted.
- Response: `QoK`. Example: `1;9;EXECMOV P1` → `QoK`. Related: `EXEC2=`.

## 5.2.17 EXEC2= * — Direct execution

- Syntax: `EXEC2=<Instruction>`; MELFA-BASIC IV–VI instruction or MOVEMASTER command. Responds when execution completes.
- Response: `QoK`. Example: `1;9;EXEC2=MOV P1` → `QoK`. Related: `EXEC`.

## 5.2.18 STEP * — Step execution

- Syntax: `STEP<Method>`; edit slot. `1`: one step forward; `R`: continuous forward; `B`: one step backward; `S`: stop stepping.
- Response: `QoK`. Example: `1;9;STEP1` → `QoK`.

## 5.2.19 ECLR — Clear program contents

- Syntax: `ECLR`; clears the edit-slot program. Response: `QoK`.
- Example: `1;9;ECLR` → `QoK`. Related: `DELETE`.

## 5.2.20 DELETE — Delete program lines

- Syntax: `DELETE<Start line>;<End line>`; deletes the specified line range. Response: `QoK`.
- Example: `1;1;DELETE10;50` → `QoK`. Related: `ECLR`.

## 5.2.21 RENUM — Renumber

- Syntax: `RENUM<New line no>;<Old start line no>;<Step>;<Old end line no>`; edit slot. Zero means 10 for new line and step, top for old start, bottom for old end.
- Response: `QoK`. Example: `1;9;RENUM100;0;0;0` → `QoK`.

## 5.2.22 PDIR — Program directory

- Syntax: `PDIR<Posi.>`; `TOP` for first information or a numeric position.
- Response for first information: `QoK<Program name>;<Size>;<DATE><TIME>;<Count>;<Remain size>;<Attrib>;;<Line count>;<Position Count>;<Operation time>;<Latest tact>;<Avg.tact>;<Cycle count>`.
- Response for a position: `QoK<Program name>;<Size>;<DATE><TIME>;<Attrib>;;<Line count>;<Position Count>;<Running>;<Latest tact>;<Avg.tact>;<Cycle count>`. The manual calls the corresponding field operation time in its definitions; the format says `Running`.
- Size and remaining size are bytes/file-system space; count is number of programs; date `yy-mm-dd`, time `hh:mm:ss`; attrib is program attribute; operation and tact times are milliseconds; cycle count is operation count.
- Examples: `1;1;PDIRTOP` → `QoK1.MB5;235;04-02-0519:31:28;34;105984;12;;2;0;115850;60;60;1903`; `1;1;PDIR1` → `QoK1.MB5;235;04-02-0519:31:28;12;;2;0;115850;60;60;1903`. Related: `FDIR`.

## 5.2.23 FDIR — File directory

- Syntax: `FDIR<Posi.>`; `TOP` for first information, a numeric position, or `<*.*` for first information for all files.
- Response: `QoK<File name>;<Size>;<DATE><TIME>;<Count>;<Remain size>;<Attrib>` for first information, or `QoK<File name>;<Size>;<DATE><TIME>;<Attrib>` for a position. Size is bytes; date `yy-mm-dd`, time `hh:mm:ss`; count is file count, remaining size is file-system space, attrib is attribute.
- Examples: `1;1;FDIRTOP` → `QoK1.MB5;235;04-02-0519:31:28;46;105984;12`; `1;1;FDIR1` → `QoK1.MB5;235;04-02-0519:31:28;12`; `1;1;FDIR<*.*` → same first-information example. Related: `PDIR`.

## 5.2.24 FCHECK — File check

- Syntax: `FCHECK<File name>`; omitted extension defaults to `MB5`.
- Response: `QoK<Status>`: `N` exists, `F` does not.
- Example: `1;1;FCHECK100` → `QoKN` (request reconstructed from duplicated extraction; verify PDF).

## 5.2.25 FPATH= — File path

- Function: Obtain a file's saved path; omitted extension defaults to `MB5`.
- The PDF's format says `FPATH <File name>` (without `=`), but its heading and example use `FPATH=`. This is a confirmed inconsistency in the manual; verify the accepted syntax against the controller documentation or controller.
- Response: `QoK<File name>` including path. Example: `1;1;FPATH=1.MB5` → `QoK/robprg/dat/1.MB5`.

## 5.2.26 FCOPY — File copy

- Syntax: `FCOPY<Src.file>;<Dst.file>`; source and destination names; omitted extension defaults to `MB5`.
- Response: `QoK`. Example: `1;1;FCOPY1;2` → `QoK`.

## 5.2.27 FDEL — File delete

- Syntax: `FDEL<File name>`; omitted extension defaults to `MB5`.
- Response: `QoK`. Example: `1;1;FDEL1` → `QoK`.

## 5.2.28 FRENAME — File rename

- Syntax: `FRENAME<Src.file>;<Dst.file>`; omitted extension defaults to `MB5`.
- Response: `QoK`. Example: `1;1;FRENAME1;2` → `QoK`.

## 5.2.29 FATTRIB — File attribute

- Syntax: `FATTRIB<File name>;<Attrib>`; `+p` read-only lines, `-p` read/write lines, `+q` read-only variables, `-q` read/write variables.
- Response: `QoK`. Example: `1;1;FATTRIB1;+p` → `QoK`.

## 5.2.30 FINIT — File initialization

- Syntax: `FINIT`; initializes files. Response: `QoK`.
- Example: `1;1;FINIT` → `QoK`.

## 5.2.31 FOPEN — File block open

- Syntax: `FOPEN<File name>;<Mode>`; `w` block write, `r` block read.
- Response: `QoK`. Example: `1;1;FOPEN1.MB5;r` → `QoK`. Related: `FCLOSE`, `FREAD`, `FWRITE`.

## 5.2.32 FCLOSE — File block close

- Syntax: `FCLOSE`; closes the file opened for block I/O. Response: `QoK`.
- Example: `1;1;FCLOSE` → `QoK`. Related: `FOPEN`, `FREAD`, `FWRITE`.

## 5.2.33 FREAD — Block read

- Syntax: `FREAD`; reads a file block.
- Response: `QoK<Contents>;<Continue>`; contents are HEX ASCII; 1 means more data, 0 means no continuation.
- Example: `1;1;FREAD` → a long HEX ASCII block ending `;1`. The extraction interleaves the example's hex digits; consult the PDF for the exact bytes. Related: `FOPEN`, `FCLOSE`, `FWRITE`.

## 5.2.34 FWRITE — Block write

- Syntax: `FWRITE<Data size>;<Contents>`; size of content and content in HEX ASCII.
- Response: `QoK`. Example begins `1;1;FWRITE120;524F42505247` and returns `QoK`; the remaining hex is interleaved in the extraction and cannot be reproduced reliably. The source lists `FOPEN`, `FCLOSE`, `FWRITE` as related commands.

## 5.2.35 EFREE — Read file-system size

- Syntax: `EFREE`; response `QoK<Total size>;<Used size>;<Use rate>`, rate in percent.
- Example: `1;1;EFREE` → `QoK125696;15616;12`.

## 5.2.36 ESEARCH — Search string

- Syntax: `ESEARCH<String>`; searches the program being edited.
- Response: `QoK<Contents>;<Column>`, line content and matching column.
- Example: `1;9;ESEARCHMov` → `QoK3 Mov P1;2`.

## 5.2.37 PRGLOAD= * — Program load

- Syntax: `PRGLOAD=<Program name>`; loads the program into a task slot. Response: `QoK`.
- Example: `1;1;PRGLOAD=100` → `QoK`. Related: `PRG<UP/DOWN>`.

## 5.2.38 PRG&lt;UP/DOWN&gt; * — Program select

- Syntax: `PRG<UP/DOWN>`; select `UP` or `DOWN` in the task slot. Response: `QoK`.
- Example: `1;1;PRGUP` → `QoK`. Related: `PRGLOAD`.

## 5.2.39 PRGRD — Read execution program name

- Syntax: `PRGRD`; response `QoK<Program name>` for the task slot.
- Example: `1;1;PRGRD` → `QoK100.MB5`.

## 5.2.40 LINENO= * — Change execution line number

- Syntax: `LINENO=<Line no.>`; response `QoK`.
- Example: `1;1;LINENO=100` → `QoK`. Related: `LINENO`.

## 5.2.41 LINENO — Read execution line number

- Syntax: `LINENO`; response `QoK<Line no.>`.
- Example: `1;1;LINENO` → `QoK100`. Related: `LINENO=`.

## 5.2.42 LINERD — Read execution line contents

- Syntax: `LINERD`; response `QoK<Contents>`.
- Example: `1;1;LINERD` → `QoK10 MOV P1`. Related: `LINESRD`.

## 5.2.43 LINESRD — Read multiple execution lines

- Syntax: `LINESRD<Continue>;<Prev.line>;<Next line>`; continue 0 starts a read, 1 continues; the other fields give counts before and after the executing line.
- Response: `QoK<Continue>;<Line count>;<Contents>[<VT><Contents>…];<Line no.>`; continuation 1 means more, 0 means none; line number is execution line.
- Example: `1;1;LINESRD0;2;2` → `QoK0;5;10 '### Program ###<VT>20 '<VT>30 *SMAIN<VT>40 MOV P0<VT>50 DLY 1;0010`. Related: `LINERD`.

## 5.2.44 SRV&lt;ON/OFF&gt; * — Servo power

- Syntax: `SRV<ON/OFF>`; select `ON` or `OFF`. Response: `QoK`.
- Example: `1;1;SRVON` → `QoK`.

## 5.2.45 OVRD= * — Change OP override

- Syntax: `OVRD=<Override>`; range 1–100.
- Response: `QoK<Override>`, the set value. Example: `1;1;OVRD=50` → `QoK50`. Related: `OVRD`.

## 5.2.46 OVRD — Read OP override

- Syntax: `OVRD`; response `QoK<Override>`, current value.
- Example: `1;1;OVRD` → `QoK50`. Related: `OVRD=`.

## 5.2.47 RUN * — Program start

- Syntax: `RUN[<Program name>;<Mode>]`; omission selects current program; mode `0` repeat or `1` cycle, default repeat.
- Response: `QoK`. Example: `1;1;RUN100;1` → `QoK`. Related: `STOP`, `CSTOP`.

## 5.2.48 STOP — Stop

- Syntax: `STOP`; stops execution. Response: `QoK`.
- Example: `1;1;STOP` → `QoK`. Related: `STOP<ON/OFF>`, `RUN`.

## 5.2.49 STOP&lt;ON/OFF&gt; — Stop ON/OFF

- Syntax: `STOP<ON/OFF>`; after `STOPON`, starting is prohibited until `STOPOFF`.
- Response: `QoK`. Example: `1;1;STOPON` → `QoK`. Related: `STOP`, `CSTOP`, `RUN`.

## 5.2.50 CSTOP — Cycle stop

- Syntax: `CSTOP`; stops the running program at the cycle. Response: `QoK`.
- Example: `1;1;CSTOP` → `QoK`. Related: `STOP`, `STOP<ON/OFF>`, `RUN`.

## 5.2.51 RSTALRM — Error reset

- Syntax: `RSTALRM`; response `QoK`. Example: `1;1;RSTALRM` → `QoK`.

## 5.2.52 SLOTINIT — Reset all programs

- Syntax: `SLOTINIT`; resets programs in all slots. Response: `QoK`.
- Example: `1;1;SLOTINIT` → `QoK`. Related: `RSTPRG`.

## 5.2.53 RSTPRG — Reset each program

- Syntax: `RSTPRG`; resets the program in the specified slot. Response: `QoK`.
- Example: `1;1;RSTPRG` → `QoK`. Related: `SLOTINIT`.

## 5.2.54 RSTIO — Reset output signals

- Syntax: `RSTIO`; resets general-purpose outputs. Response: `QoK`.
- Example: `1;1;RSTIO` → `QoK`.

## 5.2.55 MLOCK&lt;ON/OFF&gt; — Machine lock

- Syntax: `MLOCK<ON/OFF>`; choose `ON` or `OFF`. Response: `QoK`.
- Example: `1;1;MLOCKON` → `QoK`.

## 5.2.56 HND&lt;ON/OFF&gt; — Hand open/close

- Syntax: `HND<ON/OFF><Hand no.>`; select `ON` or `OFF` and hand number 1–8.
- Response: `QoK`. Example: `1;1;HNDON1` → `QoK`.

## 5.2.57 ALIGN * — Align hand

- Syntax: `ALIGN`; aligns the hand. Response: `QoK`.
- Example: `1;1;ALIGN` → `QoK`.

## 5.2.58 MOVSP * — Move to safe position

- Syntax: `MOVSP`; moves to the safe position in parameter `JSAFE`. Response: `QoK`.
- Example: `1;1;MOVSP` → `QoK`.

## 5.2.59 JOG * — Jog operation

- Syntax: `JOG<Jog mode>;<Reserve>;<-Dire.>;<+Dire.>;<Inch>`. Modes: `00` JOINT, `01` XYZ, `02` TOOL, `03` reserved, `04` 3-axis XYZ, `05` CYLNDER. Reserve is fixed `00`; direction fields specify negative and positive axis masks (see conventions). Inch: `00` off, `01` high, `02` low.
- Jog stops automatically if the command is not received for about 140 ms. Response: `QoK`.
- Source example: `JOG01;00;01;00;00;00` → `QoK`. **Inconsistency:** example has one more field than the documented format; do not assume its meaning.

## 5.2.60 JOG * — Multifunctional electric hand

- Syntax: `JOG<Jog mode>;<Reserve>;<Speed>;<Force>;<Point No>`. Reserve fixed `00`; speed and force `01`–`64` hex; point `00`–`20` hex.
- Jog-mode bits 0–2: `000B` origin (`EHOrg`), `001B` open (`EHOpen`), `010B` close (`EHClose`), `011B` move (`EHMov`), `100B` hold (`EHHold`). The extraction does not explain the other mode bits.
- Stops automatically if the command is not received for about 140 ms. Response: `QoK`. Example: `JOG44;00;46;32;0A` → `QoK`.

## 5.2.61 LS&lt;ON/OFF&gt; — Limit switch

- Syntax: `LS<ON/OFF>`; `ON` enables, `OFF` disables. Response: `QoK`.
- Example: `1;1;LSON` → `QoK`.

## 5.2.62 AUE&lt;ON/OFF&gt; — Program start enable/disable

- Syntax: `AUE<ON/OFF>`; `ON` enables, `OFF` disables. Response: `QoK`.
- Example: `1;1;AUEON` → `QoK`. Related: `ATENA`.

## 5.2.63 ATENA — Read start eligibility

- Syntax: `ATENA`; response `QoK<Status>`: `0` start enabled, `1` disabled.
- Example: `1;1;ATENA` → `QoK1`. Related: `AUE<ON/OFF>`.

## 5.2.64 BRKPTSET — Set breakpoint

- Syntax: `BRKPTSET=<Program name>;<Line number>;<Breakpoint type>`; type `0` continuous, `1` one-time. Cannot set while running; if multiple settings target the same line, the last type applies.
- Response: `QoK`. Source example `1;1; BRKPTSET10;20;0` → `QoK1` conflicts with both documented format (`=`) and response; space is also shown in extraction. Verify PDF.
- Related: `BRKPTCLR`, `BRKPTGET`.

## 5.2.65 BRKPTCLR — Delete breakpoint

- Syntax: `BRKPTCLR=<Program name>;<Line number>`; line `0` deletes all.
- Response: `QoK`. Source example `1;1; BRKPTCLR10;20` → `QoK1` conflicts with documented format and response; verify PDF.
- Related: `BRKPTSET`, `BRKPTGET`.

## 5.2.66 BRKPTGET — List breakpoints

- Syntax: `BRKPTGET=<Program name>;<Breakpoint number>;<Start line>;<End line>`; omitted program name means all programs; breakpoint number 1–128; line bounds specify read range.
- Response: `QoK<Program name>;<Line number>;<Breakpoint type>`, type `0` continuous, `1` one-time.
- Source example is `1;1; BRKPTSET` → `QoK1`, apparently inconsistent with this command; verify PDF. Related: `BRKPTSET`, `BRKPTCLR`.

## 5.2.67 TOOLSET — Set tool number

- Syntax: `TOOLSET=<Tool number>`; 0–16. Response: `QoK`.
- Source example `1;1;TOOLSET2` → `QoK` omits the documented `=`. Related: `TOOLRD`.

## 5.2.68 TOOLRD — Read tool number

- Syntax: `TOOLRD`; response `QoK<Tool number>`, 0–16.
- Example: `1;1;TOOLRD` → `QoK2`. Related: `TOOLSET`.

## 5.2.69 SAFE= — Low-speed mode

- Syntax: `SAFE=<1/0>`; `1` setting valid, `0` invalid.
- The source calls the response `QoK<Tool number>`; example: `1;1;SAFE=1` → `QoK2`. The tool-number response is inconsistent with this function and requires verification.

## 5.2.70 STATE — Read run status

- Syntax: `STATE`.
- Response format as extracted: `QoK<Program name>;<Line no.>;<Override>;<Edit sts.>;<Run sts.><Stop sts.><Error no.>;<Step no.>;<Mech info.>;;;;;;;;;<Task prg.name>;<Task mode>;<Task cond.>;<Task pri.>;<Mech no.>`. The exact number of reserved semicolons is uncertain due to interleaved extraction; verify PDF.
- Program name is loaded in the task slot; line and step are executing line/step; error number 0 means no error; mech info bits 0–2 represent mechanisms 1–3. Task fields are slot-table program, mode `REP`/`CYC`, start condition `START`/`ALWAYS`/`ERROR`, priority 1–31 and mechanism in use.
- Edit status bits 0–2: editing, running, changed. Run and stop status bit definitions are in 5.2.71.
- Source example: `1;1;STATE` → `Qok2.MB5;100;100;5;A1124220;1;1;;;;;;;;;REP;START;1;0`; boundaries and capitalization require verification.

## 5.2.71 DSTATE — Read stop status

- Syntax: `DSTATE`; response `QoK<Run sts.><Stop sts.><Error no.>;<Step no.>;<Mech no.>`. Run and stop status are each two hex digits; error 0 means no error; other fields are executing step and mechanism in use.
- Run bits 0–7, with source's 0/1 descriptions: cycle/repeat; cycle stop ON/OFF; machine lock OFF/ON; auto/teach; running in teach mode; servo OFF/ON; STOP/RUN; operation disable/enable.
- Stop bits 0–6: emergency stop, stop, wait, stop signal ON/OFF, program select enabled, reserved, pseudo input.
- Example: `1;1;DSTATE` → `QoK09060;17;1`.

## 5.2.72 CALIB — Read installation status

- Syntax: `CALIB`; response `QoK<Status><Axis pattern><Reserve>`. Status `N` defined, `F` not defined; completion-axis mask is two hex digits, bits 0–7 J1–J8.
- Example: `1;1; CALIB` → `QoKN3F3F` (space reproduced from source example).

## 5.2.73 IOSIGNAL — Read input/output signals

- Syntax: `IOSIGNAL<IN no.>;<OUT no.>`; input/output signal numbers.
- Response: `QoK<IN state><OUT state>`; each state is four hex digits.
- Example: `1;1;IOSIGNAL0;0` → `QoK00000002`.

## 5.2.74 IN — Read input signal

- Syntax: `IN<IN no.>`; input signal number.
- Response: `QoK<IN state>`, four hex digits. Example: `1;1;IN0` → `QoK0000`.

## 5.2.75 OUT — Read output signal

- Syntax: `OUT<OUT no.>`; output signal number.
- Response: `QoK<OUT state>`, four hex digits. Example: `1;1;OUT0` → `QoK0001`.

## 5.2.76 OUT= — Write output signal

- Syntax: `OUT=<OUT no.>;<OUT val.>`; signal number and forced-output value, four hex digits.
- Response: `QoK`. Example: `1;1;OUT=0;0802` → `QoK`.

## 5.2.77 DIN — Read CC-Link input registers

- Syntax: `DIN<IN no.>`; CC-Link input register number. Requires CC-Link option.
- Response: `QoK<IN reg.state>;…` for 16 registers, in decimal.
- Example: `1;1;DIN6000` → `QoK0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0`. Related: `DOUT`.

## 5.2.78 DOUT — Read CC-Link output registers

- Syntax: `DOUT<OUT no.>`; CC-Link output register number. Requires CC-Link option.
- Response: `QoK<OUT reg.state>;…` for 16 registers, in decimal.
- Example: `1;1;DOUT6000` → `QoK0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0`. Source function text says “write” despite read heading, format and response. Related: `DIN`.

## 5.2.79 DOUT= — Write CC-Link output register

- Syntax: `DOUT=<OUT no.>;<OUT val.>`; register number and forced output value, four hex digits. Requires CC-Link option.
- Response: `QoK`. Example: `1;1;DOUT=6000;0002` → `QoK`. Related: `DIN`, `DOUT`.

## 5.2.80 INDMY — Set pseudo input

- Syntax: `INDMY`; enables pseudo input of input signals. Response: `QoK`.
- Example: `1;1;INDMY` → `QoK`. Related: `INSET`, `IN=`, `DIN=`.

## 5.2.81 INSET — Reset pseudo input

- Syntax: `INSET`; resets pseudo input. Response: `QoK`.
- Example: `1;1;INSET` → `QoK`. Related: `INDMY`, `IN=`, `DIN=`.

## 5.2.82 IN= — Write pseudo input

- Syntax: `IN=<IN no.>;<IN val.>`; signal number and pseudo-input value, four hex digits.
- Response: `QoK`. Example: `1;1;IN=0;0802` → `QoK`. Related: `INDMY`, `INSET`, `DIN=`.

## 5.2.83 DIN= — Write pseudo input register

- Syntax: `DIN=<IN no.>;<IN reg.val>`; input-register number and four-hex-digit pseudo-input value. Requires CC-Link option.
- Response: `QoK`. Example: `1;1;DIN=6000;0001` → `QoK`. Related: `INDMY`, `INSET`, `IN=`.

## 5.2.84 STPSIG — Read stop signals

- Syntax: `STPSIG`; response `QoK<Stop sts.>;<EMG sts.>`, hexadecimal masks.
- Stop bits 0–3: T/B (RS-422), P/C (RS-232C), I/O, O/P. Emergency-stop bits 0–2: I/O EMG, O/P EMG, T/B EMG.
- Example: `1;1;STPSIG` → `QoK0;0`.

## 5.2.85 HNDSTS — Read hand output

- Syntax: `HNDSTS`; response `QoK<Output no.>;<Hand sts.>;<Hand type>;…` for eight hands. Output number `-1` means unused; status `1` open, `2` closed; type `0` single-solenoid, `1` double-solenoid.
- Example begins `1;1;HNDSTS` → `QoK900;2;1;902;1;1;904;1;1;906;1;1;…`; the unused-hand part is interleaved in the extraction and cannot be reproduced reliably. The format's hand-type field is written in Japanese in the source.

## 5.2.86 USERAREASTS — Read user-specified areas

- Syntax: `USERAREASTS`; response `QoK<Status>` as a hex mask, bits 0–7 corresponding to areas 1–8.
- Example: `1;1;USERAREASTS` → `QoK0`.

## 5.2.87 JPOS, PPOS, XPOS, RPOS — Read current position

- Syntax: `<Type>POS<Axis info.>`; `J` joint, `P` XYZ, `X` 3-axis XYZ, `R` CYLNDER; axis info `1`–`8` single axis, `F` all axes.
- Response: `QoK<Axis name>;<Axis data>;…;<Flag1>,<Flag2>;<Override>;<End speed>;<+Limit><-Limit><reserve>`. Axis name/data repeat for the axis count; structure flags are significant only for XYZ; override is current, end speed mm/sec; limit masks are each two hex digits using the axis map in conventions. Check exact separator placement against PDF.
- Example: `1;1;PPOSF` → `QoKX;290.62;Y;-0.09;Z;11.26;A;-179.94;B;-0.26;C;179.93;L1;0.00;;6,0;100;0.00;00000000`.

## 5.2.88 GJPOS, GPPOS — Read destination position

- Syntax: `G<Type>POS<Axis info.>`; `J` joint or `P` XYZ; axis info `1`–`8` single axis, `F` all axes.
- Response and field meanings match 5.2.87. The source heading says destination, while its function text says current position; verify.
- Example: `1;1;GPPOSF` → `QoKX;290.62;Y;-0.09;Z;11.26;A;-179.94;B;-0.26;C;179.93;L1;0.00;;6,0;100;0.00;00000000`.

## 5.2.89 TIME — Read time

- Syntax: `TIME`; response `QoK<DATE><TIME>`, date `yy-mm-dd`, time `hh:mm:ss`.
- Example: `1;1;TIME` → `QoK04-02-0917:54:32`. Related: `TIME=`.

## 5.2.90 TIME= — Change time

- Syntax: `TIME=<DATE><TIME>`, date `yy-mm-dd`, time `hh:mm:ss`. Response: `QoK`.
- Example: `1;1;TIME=04-02-0917:00:00` → `QoK`.

## 5.2.91 PTIME — Read hour meter

- Syntax: `PTIME`; response fields are power-on time, all-mechanism servo-on time, mechanism 1–3 servo-on times, reserved zero fields, program operation time, another zero, battery accumulation time, battery remaining time, and mechanism-information mask.
- Times are hours; mechanism mask bits 0–2 identify mechanisms 1–3. The extracted response format merges a reserved `0` with `<Operation time>`; precise separator placement is uncertain.
- Example: `1;1;PTIME` → `QoK149;51;51;0;0;0;0;0;0;0;0;0;0;0;0;0;0;102;0;2715;11885;1`. Related: `PTIMEDEL=`.

## 5.2.92 PTIMEDEL= — Clear hour meter

- Syntax: `PTIMEDEL=<Kind>`; `T` power-on, `S` servo-on, `D` program operation, `B` battery accumulation, `Z` all.
- Response: `QoK`. Example: `1;1;PTIMEDEL=B` → `QoK`. Related: `PTIME`.

## 5.2.93 CYCLETIME — Read cycle time

- Syntax: `CYCLETIME<Program name>`.
- Response: `QoK<Operation time>;<Latest tact>;<Avg.tact>;<Cycle count>`; first three in milliseconds, last is operation count.
- Example: `1;1;CYCLETIME100` → `QoK224745;156;154;1454`. Related: `PDIR`, `CYCLECLR`.

## 5.2.94 CYCLECLR — Clear cycle time

- Syntax: `CYCLECLR<Program name>`; clears program operating time. Response: `QoK`.
- Example: `1;1;CYCLECLR100` → `QoK`. Related: `PDIR`, `CYCLETIME`.

## 5.2.95 ERROR — Read error number

- Syntax: `ERROR`; response `QoK[<Error no.>;<Error level>]`; only `QoK` if no error. Level `1` high, `2` low, `3` caution.
- Source example: `1;1;ERROR` → `Qok4140;2`. Related: `ERRORMES`.

## 5.2.96 ERRORMES — Read error contents

- Syntax: `ERRORMES<Error no.>`; response `QoK<Contents>`.
- Example: `1;1;ERRORMES4140` → `QoKThe program was not found`. Related: `ERROR`.

## 5.2.97 ERRORLOG — Read error history

- Syntax: `ERRORLOG<Hist.no.>`; `TOP` newest, `END` oldest, `+1` previous history, `-1` next history.
- Response: `QoK<DATE>;<TIME>;<Error no.>;<Error contents>;<Error level>;<Program name>;<Line no.>;<Error detail no.>`; date `yy-mm-dd`, time `hh:mm:ss`, level `1` high, `2` low, `3` caution. Only `QoK` if history empty.
- Example: `1;1;ERRORLOGTOP` → `QoK04-02-09;18:49:52;4140;The program was not found;2;S1;0;414000000`. Related: `ERRLOGCLR`, `ERRLOG2=`.

## 5.2.98 ERRLOG2= — Filtered error history

- Syntax: `ERRLOG2=<Hist. No.>;<Error level>;<Error details number>`; history position as 5.2.97; level `0` all (default), `1` high, `2` low, `3` caution; detail filter is 6–9 digits. For `+`/`-` history selection the last filter is used.
- Response and field meanings match 5.2.97; `QoK` alone if empty.
- Unavailable on CRn-500; available from CRnQ-700/CRnD-700 main software R2/S2. The source notes last position and reference error number are shared with `ERRORLOG` and `ERRORRD` when using `+`/`-`.
- Example: `1;1;ERRLOG2=TOP;0;414000000` → `QoK04-02-09;18:49:52;4140;The program was not found;2;S1;0;414000000`. Related: `ERRLOGCLR`, `ERRORLOG`.

## 5.2.99 ERRLOGCLR — Clear error history

- Syntax: `ERRLOGCLR<Error level>`; `1` high, `2` low, `3` caution, `0` all.
- Response: `QoK`. Example: `1;1;ERRLOGCLR1` → `QoK`. Source related command: `ERRLOG`.

## 5.2.100 ERRSUM — Error summary

- Syntax: `ERRSUM<No.>`; number 1–100 in numerical order.
- Response: `QoK<Number of errors>;<Detailed error number>;<Error level>;<Error contents>;<Cause>;<Treat>;<Count of error>`; level `H` high, `L` low, `C` caution. Detailed number `000000000` means no history; marked text fields are empty when there is no history.
- Examples: `1;1;ERRSUM1` → `QoK4;006000000;H;EMG signal is input. (O.Panel);EMG signal is input. (O.Panel);Check the O.Panel emergency stop;4`; `1;1;ERRSUM5` → `QoK4;000000000;;;;;0`. Related: `ERRSUMCLR`, `SUMDATE`, `ERRSUM2=`.

## 5.2.101 ERRSUM2= — Error summary with last occurrence

- Syntax: `ERRSUM2=<Hist. no.>`; number 1–100 in numerical order.
- Response: `QoK<Number of errors>;<Detailed error number>;<Error level>;<Error contents>;<Cause>;<Treat>;<Count of error>;<date>;<time>`; fields as in 5.2.100, followed by last occurrence date and time. No-history detailed number is `000000000`, marked text fields are empty.
- Unavailable on CRn-500; available from CRnQ-700/CRnD-700 main software R2/S2.
- Example: `1;1;ERRSUM2=1` → `QoK4;006000000;H;EMG signal is input. (O.Panel);EMG signal is input. (O.Panel);Check the O.Panel emergency stop;4;10-08-19;18:49:52`. A second source example uses `1;1;ERRSUM5` and returns `QoK4;000000000;;;;;0;;`; the request conflicts with this section and the extraction is ambiguous. Related: `ERRSUMCLR`, `SUMDATE`, `ERRSUM`.

## 5.2.102 ERRSUMCLR — Clear error summary

- Function: Clear error summary. The source's format and example instead both say `ERRLOGCLR` without an argument, unlike the section heading and 5.2.99; do not assume which spelling is correct.
- Source example: `1;1;ERRLOGCLR` → `QoK`. Related: `ERRSUM`, `ERRSUM2=`.

## 5.2.103 SUMDATE — Error logging start date

- Syntax: `SUMDATE<function no.>`; `0` error log, `1` error summary.
- Response: `QoK<date>;<time>`, data-collection start date `yy-mm-dd` and time `hh:mm:ss`.
- Example: `1;1;SUMDATE0` → `QoK10-08-09;20:23:46`. Related: `ERRLOGCLR`, `ERRSUMCLR`.

## 5.2.104 VAL — Read variable

- Syntax: `VAL<Variable posi.>;<Class>`; position `<` top, `>` bottom, `+1` next, `-1` previous, or variable name. Class `M` integer, `C` character, `P` position, `J` joint.
- Response: `QoK<Variable name>=<Value>`. Example: `1;1;VAL<;M` → `QoKM1=+3`. Related: `VALS`.

## 5.2.105 VALS — Read multiple variables

- Syntax: `VALS<Variable posi.>;<Class>`; position `<`, `>`, `+1`, `-1`; classes as in 5.2.104.
- Response: `QoK<Variable name>=<Value>[<VT><Variable name>=<Value>…];<Count>;<Continue>`; count is variables read, continue `1` means more, `0` none.
- Source example: `1;1;VALS<;M` → `QoKM1=+1;<VT>M2=+2;<VT>M3=+3;3;0`. The extra semicolons before VT are in the example but not the format. Related: `VAL`.

## 5.2.106 GVAL — Read global variable

- Syntax: `GVAL<Variable posi.>;<Class>`; same position and class options as 5.2.104.
- Response: `QoK<Variable name>=<Value>`. Example: `1;1;GVAL<;M` → `QoKM_01=+3`. Related: `GVALS`.

## 5.2.107 GVALS — Read multiple global variables

- Syntax: `GVALS<Variable posi.>;<Class>`; position `<`, `>`, `+1`, `-1`; classes as in 5.2.104.
- Response: `QoK<Variable name>=<Value>[<VT><Variable name>=<Value>…];<Count>;<Continue>`; continue `1` more, `0` none.
- Source example: `1;1;GVALS<;M` → `QoKM_01=+1;<VT>M_02=+2;<VT>M_03=+3;3;0`. As in 5.2.105 the example includes semicolons not shown in the format. Related: `GVAL`.

## 5.2.108 HOT — Write variable data

- Syntax: `HOT<Program name>;<Variable name>=<Value>`; changes a variable; an integer variable can be changed even during execution.
- Response: `QoK`. Example: `1;1;HOT100;M1=1` → `QoK`.

## 5.2.109 OPNUMRD — Read option-slot count

- Syntax: `OPNUMRD`; response `QoK<Slot no.>`, number of option slots.
- Example: `1;1;OPNUMRD` → `QoK3`. Related: `OPSTSRD`.

## 5.2.110 OPSTSRD — Read option information

- Syntax: `OPSTSRD<Slot no.>`; response: `QoK<Option info.>`. The manual lists slot numbers 1, 2 and 3, but slot 4 has also been observed. The unindexed command is supported, but its slot and behavior are unknown; one simulator returned only `;`. The library exposes only the indexed form.
- Source example request `1;1;OPSTSRD` omits the documented slot argument; response contains Ethernet address, real-time port, OPT11–19 ports and connection counts, MAC address and H/W version separated by VT. Extracted response: `QoKEthernet;IP Addr 192.168.0.1<VT>PortNo(R-time) 10000<VT>PortNo(OPT11-19) 10001/10002/10003/10004/10005/10006/10007/10008/10009<VT>Connect Count(OPT11-19) 2/0/0/0/0/0/0/0/0<VT>MAC Addr 00:00:00:00:00:00<VT>H/W Ver: 0`.
- Source related command: `OPSTSRD`.

## 5.2.111 THMRD — Read controller temperature

- Syntax: `THMRD`; response `QoK<Temperature>`.
- Example: `1;1;THMRD` → `QoK39.5`. Related: `ETEMP`.

## 5.2.112 ETEMP — Read encoder temperature

- Syntax: `ETEMP`; response `QoK<J1 Temperature>;<J2 Temperature>;…;<J8 Temperature>;<J1 Max.Temperature>;…;<J8 Max.Temperature>`. Maximum is since power-on.
- Example: `1;1;ETEMP` → `QoK38;39;41;39;36;36;0;0;41;42;44;41;38;37;0;0`. Related: `THMRD`.

## 5.2.113 EMISS — Read encoder miscount

- Syntax: `EMISS`; response `QoK<J1 Miscount>;<J2 Miscount>;…;<J8 Miscount>`.
- Example: `1;1;EMISS` → `QoK0;1;1;0;0;0;0;0`. The source's field description incorrectly labels miscount as temperature.

## 5.2.114 SRVENC — Read servo encoder

- Syntax: `SRVENC`; monitors axis-motor position.
- Response: `QoK<J1 pos. feedback>;<J1 pos. in 1 rot.>;<J1 Fdt command>;<J1 pos. command>;…` repeating those four fields for J2–J8: encoder position feedback, position in one rotation, Fdt command and position command.
- Example: `1;1;SRVENC` → `QoK-4475890;0;0;-4487192;4912356;0;0;4892959;10225275;0;0;10323293;-940046;0;0;-947494;-3330789;0;0;-3358040;6508941;0;0;6502064;0;0;0;0;0;0;0;0`. Example digits are reconstructed from interleaved extraction; verify against PDF.

## 5.2.115 SRVDRP — Read servo droop

- Syntax: `SRVDRP`; response `QoK<J1 pos. droop>;<J1 max. pos. droop>;…`, repeating current and maximum position droop for J1–J8.
- Example: `1;1;SRVDRP` → `QoK16860;1525531;22463;-1686967;126622;-543044;-11984;-318476;36426;2421963;9865;-2205319;0;0;0;0`.

## 5.2.116 SRVSPD — Read servo speed

- Syntax: `SRVSPD`; response `QoK<J1 spd. feedback>;<J1 speed max.>;<J1 speed command>;…`, repeating speed feedback, maximum and command for J1–J8.
- Example: `1;1;SRVSPD` → `QoK16;3755;68;29;-4148;115;147;-1150;-587;-11;-783;45;41;5964;163;10;-5429;41;0;0;0;0;0;0`.

## 5.2.117 SRVCUR — Read servo current

- Syntax: `SRVCUR<type>`; `0` command, `1` feedback.
- Type 1 response: `QoK<J1 cur. feedback>;<J1 max. curr. cmd1>;<J1 max. curr. cmd2>;…`, three fields per axis J1–J8: current feedback and two maximum current commands.
- Type 0 response: `QoK<J1 cur. command>;<J1 tolerable cmd+>;<J1 tolerable cmd->;<J1 RMS Current>;…`, four fields per axis J1–J8: current command, positive/negative tolerable command, RMS current.
- Example: `1;1;SRVCUR1` → `QoK-0.42;0.49;3.30;0.80;1.73;2.63;-0.57;0.67;0.83;0.24;0.24;0.86;0.13;0.39;0.87;-0.24;0.24;0.93;0.00;0.00;0.00;0.00;0.00;0.00`.

## 5.2.118 SRVLCR — Servo load current

- Syntax: `SRVLCR`; response `QoK<J1 load level>;<J1 max. load level>;…` repeating current and maximum load level for J1–J8. Function text also mentions encoder temperature, but the listed response does not.
- Example: `1;1;SRVLCR` → `QoK3.00;3.00;35.00;35.00;30.00;30.00;14.00;14.00;32.00;32.00;8.00;8.00;0.00;0.00;0.00;0.00`.

## 5.2.119 SRVVOL — Servo voltage

- Syntax: `SRVVOL`; monitors main-circuit power supply.
- Response: `QoK<Power voltate>;<Power voltate (max)>;<Power voltate(min)>;<J1 regen. level>;…;<J8 regen. level>`; power voltage, maximum/minimum and per-axis regeneration level. Field names preserve the source's `voltate` spelling.
- Example: `1;1;SRVVOL` → `QoK0;0;0;0;0;0;0;0;0;0;0`.

## 5.2.120 SVMONRST= — Reset servo monitor maximum

- Syntax: `SVMONRST=`; clears servo monitor data. Response: `QoK`.
- Example: `1;1;SVMONRST=` → `QoK`.

## 5.2.121 RAREAD= — Read serial number

- Syntax: `RAREAD=`; reads robot/controller serial number.
- Response: `QoK<Number>;<Serial number>;<Set date>`; robot number, robot or controller serial, setting date.
- Example: `1;1; RAREAD=` → `QoK1;xxxxxxxxxx;2014/04/09 17:14:02` (the number of `x` characters is uncertain in the duplicated extraction).

## 5.2.122 PRMINIT — Initialize parameters

- Syntax: `PRMINIT`; initializes parameters to shipping state. Response: `QoK`.
- Example: `1;1;PRMINIT` → `QoK`.

## 5.2.123 PNR — Read parameter

- Syntax: `PNR<Parameter name>`; reads parameters at the level corresponding to `KEYWD`.
- Response: `QoK<Parameter name>;<Value>;<Value count>`.
- Example: `1;1;PNRBZR` → `QoKBZR;1;1`. Related: `PRM=`.

## 5.2.124 PRM= — Write parameter

- Syntax: `PRM=<Parameter name>;<Value>`; changes parameters at the level corresponding to `KEYWD`. Response: `QoK`.
- Example: `1;1;PRM=BZR;1` → `QoK`. Related: `PNR`.

## 5.2.125 PAR — Read parameter

- Syntax: `PAR<Parameter name>`; reads all parameters regardless of `KEYWD`.
- Response: `QoK<Parameter name>;<Value>;<Value count>`.
- Example: `1;1;PARBZR` → `QoKBZR;1;1`. Related: `PAW=`.

## 5.2.126 PAW= — Write parameter

- Syntax: `PAW=<Parameter name>;<Value>`; changes all parameters regardless of `KEYWD`. Response: `QoK`.
- Example: `1;1;PAW=BZR;1` → `QoK`. Related: `PAR`.

## 5.2.127 PAW2= — Write parameter, reboot required

- Syntax: `PAW2=<name>;<value>`; writes any parameter regardless of `KEYWD`. Power must be turned off and on for the value to take effect.
- Unavailable on CRn-500; available from CRnQ-700/CRnD-700 main software R2/S2.
- Response: `QoK`. Example: `1;1;PAW2=BZR;1` → `QoK`. Related: `PAR`, `PAW=`.

## 5.2.128 PRMUNDO — Restore parameter

- Syntax: `PRMUNDO<Mech No.>;<Parameter name>`; restores shipping value; mechanism `0` selects common parameter.
- Response: `QoK`. Example: `1;1;PRMUNDO0;BZR` → `QoK`. Related: `PRMINIT`.

## 5.2.129 PRM= / PMR= — Read changed-parameter list

- The heading says `PRM=`, but format and example say `PMR=`; this conflicts with 5.2.124. Verify the PDF/controller documentation rather than assuming either is correct.
- Documented format: `PMR=<Continuation specification>`; `0` new read, `1` continued read. Available from main software N6/P6.
- Response: `QoK<Continuation>;<num>;<name>[<VT><name>…]`; continuation `0` off, `1` on; num is names read, separated by byte `0x0B`.
- Example: `1;1;PMR=0` → `QoK0;6;SVSSFR<VT>MEOFFZ<VT>MEINST<VT>MEINSZ<VT>MEINSD<VT>MEJAR`.

## 5.2.130 KEYWD — Keyword input

- Syntax: `KEYWDp<Keyword>`; changes parameter access level. Response: `QoK`.
- Example: `1;1;KEYWDpMELFA` → `QoK`. Related: `PNR`, `PRM=`.

## 5.2.131 SLOTRD — Read slot table

- Syntax: `SLOTRD`; response `QoK<Task prg.name>;<Task mode>;<Task cond.>;<Task pri>`. Program name; mode `REP`/`CYC`; start condition `START`/`ALWAYS`/`ERROR`; priority 1–31.
- Example: `1;2;SLOTRD` → `QoK2;REP;START;1`. Related: `SLOTSET`.

## 5.2.132 SLOTSET — Write slot table

- Syntax: `SLOTSET<Task prg.name>;<Task mode>;<Task cond.>;<Task pri>`; fields as in 5.2.131. Changes `SLTn` parameter; reboot required.
- Response: `QoK`. Example: `1;2;SLOTSET2;REP;START;1` → `QoK`. Related: `SLOTRD`.

## 5.2.133 ENCBATTM — Battery remaining time

- Syntax: `ENCBATTM`; response `QoK<Power time>;<Remain time>`, power-on and battery remaining times in hours.
- Example: `1;1;ENCBATTM` → `QoK51;11885`. Related: `PTIMEDEL=`.

## 5.2.134 BREAKON * — Release brake

- Syntax: `BREAKON<Axis pattern>`; hex mask bits 0–7 J1–J8. `00` locks the brake.
- Response: `QoK`. Example: `1;1;BREAKON3F` → `QoK`. Related: `BREAKONF`.

## 5.2.135 BREAKONF * — Release brake without operation right

- Syntax: `BREAKONF<Axis pattern>`; hex mask bits 0–7 J1–J8; `00` locks the brake. The function explicitly says no operation right is required despite the `*` marker.
- Response: `QoK`. Example: `1;1;BREAKONF3F` → `QoK`. Related: `BREAKON`.

## 5.2.136 HOME * — Set origin

- Syntax: `HOME<Org.type>;<Axis pattern>`; hex mask bits 0–7 J1–J8. Type `0` user origin, `1` mechanical stopper, `2` reserved, `3` calibration jig, `4` ABS, `5` eye mark, `6` origin data input (parenthesized in source), `7` user ABS.
- Response: `QoK`. Example: `1;1;HOME3;3F` → `QoK`. Related: `DATINST`.

## 5.2.137 AXDATINST * — Add additional axes for origin data

- Syntax: `AXDATINST`; adds additional axes to data-input origin settings. After execution, the `DATINST` arguments and `DATRD` response include the additional axes.
- Response: `QoK`. Example: `1;1;AXDATINST` → `QoK`. Related: `DATINST`, `DATRD`.

## 5.2.138 DATINST * — Set origin by data input

- Syntax: `DATINST<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<Checksum>` or `DATINST<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<J7 data>;<J8 data>;<Checksum>`.
- Response: `QoK`. Example: `1;1;DATINSTZ2UOGQ;Z2VX5I;Z2#46Z;Z2NXW5;004QA5;0XB462;Y91%K5` → `QoK`. Related: `AXDATINST`, `DATRD`.

## 5.2.139 DATRD — Read data-input origin

- Syntax: `DATRD`; response `QoK<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<Checksum>` or, with additional axes, `QoK<J1 data>;<J2 data>;<J3 data>;<J4 data>;<J5 data>;<J6 data>;<J7 data>;<J8 data>;<Checksum>`.
- Example: `1;1;DATRD` → `QoKZ2UOGQ;Z2VX5I;Z2#46Z;Z2NXW5;004QA5;0XB462;Y91%K5`. Related: `AXDATINST`, `DATINST`.

## 5.2.140 RSTPWR — Reset power

- Syntax: `RSTPWR`; reboots controller. Response: `QoK`.
- Example: `1;1;RSTPWR` → `QoK`. Related: `RPWRCHK=`.

## 5.2.141 RPWRCHK= — Check reset power

- Syntax: `RPWRCHK=`; response `QoK<Status>`: `0` impossible, `1` possible.
- Example: `1;1;RPWRCHK=` → `QoK1`. Related: `RSTPWR`.

## 5.2.142 MFTIME= — Maintenance forecast reset date

- Syntax: `MFTIME=<Type>`; `1` battery, `2` grease, `3` belt.
- Response: `QoK<J1 reset date>;<J2 reset date>;…;<J8 reset date>;<J1 reset count>;…;<J8 reset count>`; reset dates and counts per axis. The example uses separate semicolon-delimited date and time components, unlike the single date field in the format.
- Example request: `1;1;MFTIME=1`; response contains eight repetitions of `2014/04/09;17:06:16`, followed by eight zero counts. Exact bytes and field boundaries are uncertain in the duplicated extraction.

## 5.2.143 MFRST= — Reset maintenance forecast

- Syntax: `MFRST=<Axis bit>;<Reset bit>`; both are hex patterns `00`–`FF`. Axis bits 0–7 select J1–J8; reset bit 0 selects grease and bit 1 belt.
- Response: `QoK`. Example: `1;1;MFRST=20;1` → `QoK`.

## 5.2.144 MFFCST= — Read maintenance forecast

- Syntax: `MFFCST=<Type>`; `0` battery, `1` grease, `2` belt (note the different numbering from `MFTIME=`).
- Response: `QoK<J1 forecast hour>;<J2 forecast hour>;…;<J8 forecast hour>`.
- Example: `1;1;MFFCST=1` → `QoK23999;23999;23999;23999;23999;23999;0;0`.
