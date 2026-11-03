# BFP-A3480-F Error Messages

Source: [BFP-A3480-F.pdf](BFP-A3480-F.pdf), Table 1-1. Consult the manual for safety instructions and supplementary diagrams.

Regex: "[HLC][0-9]{4}\*?"

- [HLC]: error severity level
    - H: high-level error (servo off)
    - L: low-level error (operation stops)
    - C: warning (operation continues)
- [0-9]{4}: error number
- \*?: A starred error requires turning the power off and on to reset, except where noted.

In codes ending in `n` or `m`, the letter identifies a component as indicated by the manual. This text is mostly (over 95% i believe) automatically extracted; missing symbols and diagrams require checking the PDF.

# Error List

- H0001: Fail safe error (SRVOFF)
   - Cause: The system may be abnormal.
   - Measures: Turn the power OFF and ON once. If it comes back, contact to your service provider.

- H0002: Fail safe error (STOP)
   - Cause: The system may be abnormal.
   - Measures: Turn the power OFF and ON once. If it comes back, contact to your service provider.

- H0003: The system is abnormal.
   - Cause: The problem of the system is the cause.
   - Measures: If it comes back, contact to your service provider.

- H0004: CPU Watch dog error
   - Cause: CPU was not normally treatable
   - Measures: It is necessary to change some parts when not improvement. If it comes back, contact to your service provider. \* The "CPU" part in each message varies according to the CPU in which an error is detected. RCPU\_main, CCPU\_main, RCPU\_sub, CCPU\_sub, CPU\_main, CPU\_sub, FPGA, RCPU\_PLD, CCPU\_PLD
   - Reset: Power off and on, unless otherwise noted.

- H0006: CPU process error
   - Cause: CPU was not normally treatable
   - Measures: It is necessary to change some parts when not improvement. \* The "CPU" part in each message varies according to the CPU in which an error is detected. RCPU\_main, CCPU\_main, RCPU\_sub, CCPU\_sub, CPU\_main, CPU\_sub, FPGA
   - Reset: Power off and on, unless otherwise noted.

- H0008: CPU synchronization error
   - Cause: CPU synchronization was failed
   - Measures: Turn the power OFF and ON once.
   - Reset: Power off and on, unless otherwise noted.

- H0009: Version UP (ALL)
   - Cause: Version UP (ALL)
   - Measures: Turn the power OFF and power ON once
   - Reset: Power off and on, unless otherwise noted.

- H0009: Version UP (MAIN)
   - Cause: Version UP (MAIN)
   - Measures: Turn the power OFF and power ON once
   - Reset: Power off and on, unless otherwise noted.

- H0009: Version UP (SERVO)
   - Cause: Version UP (SERVO)
   - Measures: Turn the power OFF and power ON once
   - Reset: Power off and on, unless otherwise noted.

- H0009: The servo s/w was written
   - Cause: The servo s/w was written
   - Measures: Please release an error by reset operation \* This error can be reset without turning on the power supply again.
   - Reset: Power off and on, unless otherwise noted.

- H0009: Rewriting failed servo(Er.\*)
   - Cause: Servo S/W couldn't be read.
   - Measures: When it occurs, contact your service provider \* The number (1 to 7) which indicates the cause of the error is shown in "\*" in the error message.
   - Reset: Power off and on, unless otherwise noted.

- C0010: Illegal Version (file)
   - Cause: The version is inconsistent.
   - Measures: The file has been automatically initialized. The program is being deleted.

- C0011: Illeagal Version (system data)
   - Cause: The version is inconsistent.
   - Measures: The file has been automatically initialized. Turn the power OFF and ON once.

- C0012: Initialize (error log)
   - Cause: The error log has been initialized because of version mismatch or the error log file is abnormal.
   - Measures: Reset the alarm, and continue the operation.

- C0013: Illeagal file
   - Cause: Data including programs may have been damaged.
   - Measures: Contact your service provider as the initialization operation is required.
   - Reset: Power off and on, unless otherwise noted.

- C0013: Illegal system SD card
   - Cause: File Crash
   - Measures: File Initial operation
   - Reset: Power off and on, unless otherwise noted.

- H0014: System error (illegal MECHA)
   - Cause: A character string cannot exceed 14 characters.
   - Measures: Re-input the correct name.
   - Reset: Power off and on, unless otherwise noted.

- H0015: Illeagal Version (file)
   - Cause: Illeagal Version (file)
   - Measures: Contact the maker.
   - Reset: Power off and on, unless otherwise noted.

- L0016: Turn the power OFF and ON once
   - Cause: The time from turning the power OFF to turning the power ON again is too short.
   - Measures: Give more time before turning the power ON again after turning the power OFF.
   - Reset: Power off and on, unless otherwise noted.

- C0018: The robot model was selected.
   - Cause: The robot model was selected.
   - Measures: Reset the error.

- H0020: System Error (same name is Backup data.)
   - Cause: The data of the system backup area is abnormal.
   - Measures: Please consult your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0021: System Error (Backup data is Count over.)
   - Cause: The control region is overflowing.
   - Measures: Please consult your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0022: System Error (Backup data is no area.)
   - Cause: The region is too small.
   - Measures: Please consult your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0028: Memory error (ECC)
   - Cause: 2bit error of ECC was detected (\*\*\*)
   - Measures: Turn the power OFF and ON once \* The part where an error is detected is shown in "\*\*\*" in the message. nvSRAM, FROM, RCPU\_DRAM, RCPU\_L2C, CCPU\_DRAM, CCPU\_L2C, R\_nvSRAM, R\_DRAM, R\_L2C
   - Reset: Power off and on, unless otherwise noted.

- C0029: Memory error (ECC)
   - Cause: 1bit error of ECC was detected (\*\*\*)
   - Measures: When it frequently occurs, contact your service provider \* The part where an error is detected is shown in "\*\*\*" in the message. nvSRAM, FROM, RCPU\_DRAM, RCPU\_L2C, CCPU\_DRAM, CCPU\_L2C, R\_nvSRAM, R\_DRAM, R\_L2C

- L0030: Hand error. LS release
   - Cause: This is a user setting error.
   - Measures: Reset the error after removing the cause.

- L0031: Air pressure error
   - Cause: This is a user setting error.
   - Measures: Reset the error after releasing the cause.

- C0032: HIOTYPE parameter not set
   - Cause: HIOTYPE parameter needs to be changed
   - Measures: Please set the HIOTYPE parameter (-1: Not set/0: Source/1: Sink)

- H0039: Door Switch Signal line is faulty.
   - Cause: The one point of contact in 2 points of contact of the door switch has broken. Or wiring is not the double lines.
   - Measures: Turn off the power supply. Confirm whether there is any problem in wiring of the switch. And, please confirm whether it is wiring of the double line. Refer to the "Examples of safety measures" given in separate "Standard Specifications Manual" for door switch wiring. Turn on the power supply again after checking.

- H0040: Door Switch Signal is Input
   - Cause: The door switch is open.
   - Measures: Confirm whether the door switch input signal is connected correctly. And close the door connected to the input signal of door switch.

- H0041: Comm. error (Remote I/O #1).
   - Cause: Communication line is illegal.
   - Measures: In CR800-R/CR800-Q controller, confirm that the remote I/O cable between the CPU and the controller is grounded and connected correctly.
   - Reset: Power off and on, unless otherwise noted.

- H0041: The CRC error of remote I/O channel 1 occurs
   - Cause: An error was found in the communication line for remote I/O channel 1.
   - Measures: In CR800-R/CR800-Q controller, confirm that the remote I/O cable between the CPU and the controller is grounded and connected correctly.
   - Reset: Power off and on, unless otherwise noted.

- H0042: Comm. error (Remote I/O #2).
   - Cause: Communication line is illegal.
   - Measures: Checks the remote I/O cable connection in the CR750-Q/CR751-Q controller.
   - Reset: Power off and on, unless otherwise noted.

- H0044: Comm. error (Remote I/O #4)
   - Cause: Communication line is illegal.
   - Measures: Check the cable or power supply
   - Reset: Power off and on, unless otherwise noted.

- H0045: Comm. error (Remote I/O #5)
   - Cause: Communication line is illegal.
   - Measures: Check the cable or power supply
   - Reset: Power off and on, unless otherwise noted.

- H0046: Faulty wiring (Mode sel. switch).
   - Cause: The state of doubled wiring is not matching (Mode sel. switch).
   - Measures: Turn off the power and confirm whether wiring of the switch is right. Wiring needs to be doubled. Refer to the separate manual, "Standard Specifications Manual" for wiring of the mode selector switch.

- H0047: OCL detect
   - Cause: OCL detect
   - Measures: Please check the line (O/P Emergency Stop)

- H0048: Faulty line (Mode Selector switch)
   - Cause: The state of doubled line is not matching (Mode Selector switch)
   - Measures: Please check the line (Mode Selector switch)

- H0049: Faulty Line (T/B Enable Switch)
   - Cause: The state of doubled line is not matching (T/B Enable Switch)
   - Measures: Please check the line (T/B Enable Switch)

- H0050: EMG signal is input. (external)
   - Cause: The external emergency stop is being input. If the emergency stop of T/B turns on, this error may occur simultaneously.
   - Measures: Release the external emergency stop signal.

- H0051: Wiring of the external emergency stop is abnormal.
   - Cause: If the emergency stop of T/B turns on, this error may occur simultaneously.
   - Measures: Turn OFF the power supply. Confirm whether there is any problem in wiring of the external emergency stop switch. And, please confirm whether it is wiring of the dual line. Refer to the "Examples of safety measures" given in separate "Standard Specifications Manual" for external emergency stop switch wiring. Turn on the power supply again after checking.

- H0053: EMG signal is input.(Add.Axis2)
   - Cause: The external emergency stop to addition axis amplifier is inputting.
   - Measures: Check the emergency stop of Additional Axis servo amp. Or the EM1 (forced outage) line of the addition axis may be open. Please confirm connection. In addition, the External Emergency Stop 1 and 2 are sepa- rated. The "External Emergency Stop 1" is for I/F card, and the "External Emergency Stop 2" is for main device of the amplifier

- H0054: Faulty wiring (External EMG power)
   - Cause: The state of doubled wiring is not matching (External EMG)
   - Measures: Please check the wiring (External Emergency Stop)

- H0061: EMG line is faulty.(O.Panel)
   - Cause: The emergency stop line isn't stable.
   - Measures: Confirm whether there is any problem in wiring of the external emergency stop switch. And, please confirm whether it is wiring of the double line. Refer to the "Examples of safety measures" given in separate "Standard Specifications Manual" for external emergency stop switch wiring. Turn on the power supply again after checking.

- H0070: EMG signal is input. (T.Box)
   - Cause: EMG signal is input. (T.Box) Or when using the UL specification, the brake release switch is turning ON.
   - Measures: Cancel the T/B emergency stop. Check the emergency stop switch of teaching pendant.. Or when using the UL specification, turn OFF the brake release switch. If the alarm cannot be canceled, check the fuse of the safe unit (TZ348). If the fuse broke off, exchange new fuse.

- H0071: EMG line is faulty.(T.Box)
   - Cause: The emergency stop line isn't stable.
   - Measures: Confirm whether there is any problem in wiring of the external emergency stop switch. And, please confirm whether it is wiring of the double line. Refer to the "Examples of safety measures" given in separate "Standard Specifications Manual" for external emergency stop switch wiring. Turn on the power supply again after checking.

- H0074: Faulty line (T/B Enable/Disable).
   - Cause: The state of doubled line is not matching (T/B Enable/Disable).
   - Measures: Confirm whether T/B is connected correctly. If it comes back, contact to your service provider.

- H0075: TB communication error
   - Cause: Communication between the RC and TB was cut off.
   - Measures: If it comes back, contact your service provider.

- H0077: Electric double layer overvolt.
   - Cause: A power supply of a substrate in the robot is broken
   - Measures: Turn the power OFF and contact your service provider
   - Reset: Power off and on, unless otherwise noted.

- H0083: Fuse is blown (hand)
   - Cause: The pneumatic hand's power fuse has broken. Possibly the power supply line of the hand input/output sig- nal short-circuited.
   - Measures: Confirm that the hand input/output cables shown below are connected correctly with no short circuit caused by biting of the cables. Hand input cables (HC1 to HC8) and cables connected to them. Hand output cables (GR1 to GR8) and cables connected to them. Exchange the fuse. Refer to Page 65, "2.1.2 Place where hand fuse replacement is required" for details. If the error recurs after replacing the fuse, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0086: Hand module overcurrent
   - Cause: The motorized hand's motor or circuit board has broken.
   - Measures: Exchange the motorized hand's motor or circuit board.

- H0090: Fuse is blown (brake)
   - Cause: A brake failure or a ground fault of brake cable may have caused the error.
   - Measures: There are two brake fuses inside the robot controller. Refer to Page 64, "2.1.1 Place where a brake fuse replacement is required." and replace both the fuses. If it comes back after replacing the fuses, contact to your dealer.

- H0090: Fuse is blown (T/B)
   - Cause: The cable may be disconnected or ground fault.
   - Measures: Confirm whether there is any problem in cable. Turn on the power supply again after checking. If it comes back, contact to your dealer.

- L0091: Can't access the Special signal
   - Cause: The dedicated output signal is assigned to the specified signal. This signal cannot be used in duplicate.
   - Measures: Confirm whether the same dedicated output number is assigned to the separate dedicated output signal. Change the output No., or change the dedicated output assignment parameter.

- H0095: Brake release switch is turning on.
   - Cause: Cannot execute while brake release switch is turning on.
   - Measures: Please check the brake release switch and turn it off.

- H0097: Abnormal MC status
   - Cause: The duplex magnetic contactor status signal status is not consistent.
   - Measures: The status of the duplex system for the magnetic contactor (MC) is not consistent. After turning OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0098: Abnormal SR status
   - Cause: The duplex safety relay status signal status is not consistent.
   - Measures: The status of the duplex system for the safety relay (SR) is not consistent. After turning OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0099: S/W ver. is illegal (\*\*)
   - Cause: Servo software with an old version is installed.
   - Measures: Change to the correct version of software is necessary. Contact your service provider. \* The correct version of servo software is shown in (\*\*). \* The error message and "software" of the cause are changed as follows according to the software in which an error occurs. CCPU main system, OS, RCPU sub system, CCPU sub system, servo S/W, FPGA OS, sub system, FPGA system, boot
   - Reset: Power off and on, unless otherwise noted.

- C0099: R/C ver. is illegal (\*\*)
   - Cause: R/C with an old version is installed.
   - Measures: Change to the correct version of software is necessary. Contact your service provider. \* The correct version of software is shown in (\*\*).

- H0100: Temperature in RC is too high
   - Cause: The intake fan is not operating, or the fan filter is clogged.
   - Measures: Check the operation of the intake fan, or clean or replace the fan filter if necessary. Confirms that the environmental temperature is the specification range. When it comes back, contact to the dealer.
   - Reset: Power off and on, unless otherwise noted.

- L0101: Temperature in RC is too high
   - Cause: The intake fan is not operating, or the fan filter is clogged.
   - Measures: Check the operation of the intake fan, or clean or replace the fan filter if necessary. Confirms that the environmental temperature is the specification range. When it comes back, contact to the dealer.

- C0102: Temperature in RC is too high
   - Cause: The intake fan is not operating, or the fan filter is clogged.
   - Measures: Check the operation of the intake fan, or clean or replace the fan filter if necessary. Confirms that the environmental temperature is the specification range. When it comes back, contact to the dealer.

- H0103: Temp. in robot CPU is too high
   - Cause: Ambient temperature of robot CPU becomes high
   - Measures: Please lower ambient temperature to the specification range
   - Reset: Power off and on, unless otherwise noted.

- L0104: Temp. in robot CPU is too high
   - Cause: Ambient temperature of robot CPU becomes high
   - Measures: Please lower ambient temperature to the specification range

- C0105: Temp. in robot CPU is too high
   - Cause: Ambient temperature of robot CPU becomes high
   - Measures: Please lower ambient temperature to the specification range

- H0117: 12V power supply error (brake)
   - Cause: The output of the power supply deviated from the specified range
   - Measures: Turn the power OFF and contact your service provider
   - Reset: Power off and on, unless otherwise noted.

- C0120: Instantaneous power failure
   - Cause: The power was OFF for 20msec or more
   - Measures: Check the power supply connection and power supply state
   - Reset: Power off and on, unless otherwise noted.

- H0130: The initialization connection error of system remote I/O.
   - Cause: Communication line of System Remote I/O is illegal
   - Measures: Confirm whether the communication cable is grounded correctly or connection correctly. In CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU when turning the power ON.
   - Reset: Power off and on, unless otherwise noted.

- C0150: Undefined robot serial number
   - Cause: Undefined robot serial number
   - Measures: Input the robot serial number Refer to separate manual: "Controller setup, basic operation, and mainte- nance" for the setting method.

- C0151: Undefined robot serial number
   - Cause: Undefined robot serial number
   - Measures: Input the robot serial number

- C0152: Unmatched robot serial number
   - Cause: Robot arm that connection was replaced
   - Measures: Please confirm the connected robot body is not replaced. If replaced, correct the combination. If this error occurs after purchasing the robot arm or changing robot arm and controller combination, please input the serial number. Refer to "Input the serial number" in separate manual: "Controller setup, basic operation, and maintenance" for the setting method. After correcting the combination, please confirm that the programs and parameters you use are set to the robot controller before starting robot operation.

- H016m (m = 1 ~ 3): Cannot use PIO I/F and Unit
   - Cause: The I/O Channel number of PIO interface and PIO unit overlaps.
   - Measures: Select either of Parallel I/O interface or Parallel I/O unit.

- H017m (m = 1 ~ 2): Parallel I/O interface H/W error.
   - Cause: The parallel-input/output card broke or the external power source for the parallel input/outputs was cut off.
   - Measures: If it comes back, exchange the Parallel I/O interface card.

- H018m (m = 1 ~ 2): Fuse is blown.(PIO) Slot m-Fn
   - Cause: Fuse is blown.(Parallel I/O interface)
   - Measures: Change fuse (Parallel I/O interface) Details The electric fuse Fn (n= 1-4) of the parallel input output interface installed to the option slot m (m= 1-2) is open. Removes the cause by which the fuse open and replaces the fuse. Fuse Fuse F1F1 Fuse F2F2 Fuse F3F3 Fuse F4F4

- H0210: Power supply error (\*\*\*)
   - Cause: The power supply output voltage is out of the specified range.
   - Measures: The output voltage of the power supply in the robot controller is the specified value or higher/lower. After turning OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the manufacturer. \* The part where an error is detected is shown in "\*\*\*" in the message. 24V, 5V, 3.3V, 2.5V, 1.8V, RCPU 1.5V, CCPU 1.5V, 1.2V, RCPU 1.15V, CCPU 1.15V, 1.1V, Ether 1V, ServoM 1V, ServoS 1V
   - Reset: Power off and on, unless otherwise noted.

- H0211: Power supply error (\*\*\*)
   - Cause: The power supply output voltage is out of the specified range.
   - Measures: The output voltage of the power supply in the robot CPU is the specified value or higher/lower. After turn- ing OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the man- ufacturer. \* The part where an error is detected is shown in "\*\*\*" in the message. 5V 3.3V 2.5V 1.8V 1.5V 1.3V 1.15V 1V FPGA
   - Reset: Power off and on, unless otherwise noted.

- H0212: Power supply error
   - Cause: The output of the power supply deviated from the specified range.
   - Measures: Turn the power OFF and ON once. After turning OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0213: Power supply error
   - Cause: The output of the power supply deviated from the specified range.
   - Measures: Turn the power OFF and ON once. After turning OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0220: Memory fault (main CPU)
   - Cause: A memory fault is detected.
   - Measures: A fault (inconsistent data between writing/reading, broken retention data) is detected for the memory (DRAM) to which the main CPU has access. After turning OFF the power, turn ON the power again to reset the error. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0230: Safety parameter error (xxxxx)
   - Cause: Detect errors in safety parameter value
   - Measures: The safety parameter setting is not correct. Check the parameter setting shown in the "(xxxxx)" part of the error message, and set an appropriate value.
   - Reset: Power off and on, unless otherwise noted.

- H0231: Parameter CRC error (xxxxx)
   - Cause: Detect CRC errors in safety parameter value
   - Measures: Please check the safety parameter settings and write parameters.
   - Reset: Power off and on, unless otherwise noted.

- C0240: Setting mismatch of safety function
   - Cause: The safety function is disabled.
   - Measures: Safety logic settings is set but safety function is disabled. Delete Safety Logic settings or Enable Safety Function.
   - Reset: Power off and on, unless otherwise noted.

- H0241: Fault in Safety Communication
   - Cause: Detect a fault in safety communication.
   - Measures: Turn the power OFF and ON once If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0242: Fault in Safety Data
   - Cause: Detect a fault in safety data.
   - Measures: Turn the power OFF and ON once If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0243: XXX function is not available
   - Cause: The Safety Function is not supported by this robot
   - Measures: Disable the safety function

- H0260: EMG circuit diagnosis error
   - Cause: Voltage diagnosis error
   - Measures: Turn the power OFF and ON once
   - Reset: Power off and on, unless otherwise noted.

- H0270: Voltage diagnosis error
   - Cause: Voltage diagnosis error
   - Measures: Turn the power OFF and ON once
   - Reset: Power off and on, unless otherwise noted.

- H0280: STO circuit diagnosis error
   - Cause: STO citcuit error
   - Measures: Turn the power OFF and ON once
   - Reset: Power off and on, unless otherwise noted.

- H0290: PLD control error
   - Cause: PLD control error
   - Measures: Turn the power OFF and ON once
   - Reset: Power off and on, unless otherwise noted.

- H0310: Mecha board access error
   - Cause: Failed in access to a Mecha board
   - Measures: Please confirm the connection with Mecha board

- H0310: Mecha board buffer access error
   - Cause: Cannot store more than 100 of the data to send/receive buffer
   - Measures: Please confirm the communication with Mecha board

- H0311: RemoteI/O unit config error
   - Cause: The multiple RemoteI/O units which do not support are connected
   - Measures: Please confirm the constitution of RemoteI/O unit When extended safety unit is connected, restart the unit.
   - Reset: Power off and on, unless otherwise noted.

- H0315: Can't access the D-device area
   - Cause: This area cannot be used in duplicate
   - Measures: Please change the D-device start number in parameter DDEVVLn

- H0316: The D-device area is out of range
   - Cause: An area corresponding to the variable type can't be allocated
   - Measures: Please change the D-device start number in parameter DDEVVLn

- H0317: The D-device variable duplication
   - Cause: Cannot be doubly allotted to one variable
   - Measures: Please change the variable name in parameter DDEVVLn

- C043n: Servo amplifier motor overheat
   - Cause: The motor or encoder's thermal protector activated.
   - Measures: Reduce the speed and acceleration of the robot.
   - Code note: (n indicates the axis number (1 to 8).)

- C049n: Alarm of fan in the robot
   - Cause: Fan in the robot might be out of order
   - Measures: Please exchange the fan in the robot
   - Code note: (n indicates the fan num- ber (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H050n: Servo axis setting error
   - Cause: The setting of the axis number selection switch is illegal
   - Measures: Confirm the setting of the axis selection switch
   - Code note: (n indicates the axis num- ber (1 to 8).
   - Reset: Power off and on, unless otherwise noted.

- H0510: The converter setting is illegal
   - Cause: The external emergency-stop input power was detected except external emergency-stop mode.
   - Measures: Setting is wrong. When it comes back, contact to the dealer.
   - Reset: Power off and on, unless otherwise noted.

- H0520: Robot axis setting illegal
   - Cause: The setting of the servo axis used by the mechanism is duplicated with another mechanism's axis.
   - Measures: Correctly set.
   - Reset: Power off and on, unless otherwise noted.

- H053n: Servo sys. error (memory)
   - Cause: The servo amplifier memory IC's check sum is illegal.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H054n: Servo sys. error (over run)
   - Cause: The servo amplifier software data process did not end within the specified time.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H055n: Servo sys. error (mag. pole pos)
   - Cause: An error was detected in the magnetic pole position detection signal of the detector.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H056n: Servo sys. error (A/D)
   - Cause: 1) An error was found in the servo amplifier's A/D converter during initialization. 2) The stop signal or servo OFF signal was input when the door switch signal or EMG signal was input.
   - Measures: 1) Turn the power OFF and ON once. If it comes back, contact your service provider. 2) Do not input the door switch signal or EMG signal with another signal simultaneously.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H057n: Encoder error (EEPROM)
   - Cause: An error was detected in EEPROM data of the serial pulse encoder.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H058n: Encoder error (LED)
   - Cause: The LED of the serial pulse encoder has been deteriorated.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H059n: Encoder error (position data)
   - Cause: An error was detected in the position data within a single rotation of the encoder.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H060n: Encoder no-signal detection 1
   - Cause: An error was detected in the operating input of the detector mounted on the edge of the motor.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H061n: Encoder no-signal detection 2
   - Cause: An error was detected in the operating input of the detector mounted on the edge of the machine.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H062n: Servo amplifier LSI error
   - Cause: An operation error was detected in the LSI of the servo amplifier.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H063n: Unused axis servo error
   - Cause: A power module error occurred in an axis which not use the movement control.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H064n: System error (ABS CPU)
   - Cause: An error in the CPU of the absolute position linear scale was detected.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H065n: Absolute position error
   - Cause: An error was detected in the absolute position detection circuit within the absolute position linear scale.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H066n: Incremental position error
   - Cause: An error was detected in the relative position detection circuit within the absolute position linear scale.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H067n: Encoder CPU error
   - Cause: An error was detected in the CPU of the position detector.
   - Measures: Turn the power OFF and ON once. Also, carefully check whether there is no deviation in the operating position of the robot. If it is deviated, set the origin position (OP) again. For more information about the operating procedure, refer to the separate volume, "Instruction Manual/Robot Arm Setup to Maintenance." If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H068n: Encoder LED error
   - Cause: Deterioration of the position detector's LED was detected.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H0690: Regeneration circuit error
   - Cause: A regenerative transistor or resistor error was detected. This error may also occur when the power supply voltage is high.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0700: P.S. external contactor fusing
   - Cause: The contactor was turned ON even though READY is OFF.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0710: Servo amp. relay error.
   - Cause: The relay on servo cpu card did not turn ON.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0711: Discharge resistance relay error
   - Cause: Discharge resistance circuit on converter is state-of-discharge
   - Measures: Turn the power OFF and ON once
   - Reset: Power off and on, unless otherwise noted.

- H0712: Converter fuse fusing
   - Cause: Fuse of a converter fused for a ground fault or short circuit
   - Measures: Please exchange a fuse of a converter Investigate and correct the ground fault or short circuit portion in the wiring made by the customer. Then, replace the 3.2 A fuse inside the controller. Refer to Page 67, "2.1.4 Place where a converter fuse replacement is required", for details. (On details of the 3.2 A fuse, contact the manufacturer.) If no improvement is made after carrying out the above measures, please contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H0713: Encoder power supply fuse trip
   - Cause: Encoder power supply fuse tripped by ground fault, short circuit
   - Measures: Turn the power OFF, wait a while, and then turn ON again
   - Reset: Power off and on, unless otherwise noted.

- H0720: Power supply watch dog
   - Cause: The converter software process did not end within the specified time.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0730: Power supply rush relay fusing
   - Cause: The rush resistance short-circuit relay did not turn OFF.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0740: Power supply main circuit error
   - Cause: The charge operation of the main circuit capacitor is not normal. Connection of the external emergency stop has the mistake.
   - Measures: Turn the power OFF and ON once. Confirm that the power supply voltage is in the specification value and the connection of the external emergency stop is correct. If it comes back, confirm the time of occurrence of this error being "servo ON/OFF", or being "power sup- ply OFF/ON", and contact your service provider
   - Reset: Power off and on, unless otherwise noted.

- H0742: Power supply main circuit error
   - Cause: A main circuit voltage has decreased because of a failure of the Safety relay on a converter card.
   - Measures: Turn the power OFF and ON once. Confirm whether there is any problem in wiring of the external emergency stop switch. Refer to the "Examples of safety measures" given in separate "Standard Specifications Manual" for exter- nal emergency stop switch wiring. If it comes back, contact to your service provider.

- H0743: Power supply main circuit error3.
   - Cause: A main circuit voltage has decreased because of contactor fail.
   - Measures: Turns off the power once and turns on on the power supply again. Confirm power supply voltage and the connection of the external emergency stop. When it comes back, contact to the dealer.
   - Reset: Power off and on, unless otherwise noted.

- H0750: Power supply memory error
   - Cause: An error in the memory circuit of converter or AD converter was detected.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H0760: Power supply error
   - Cause: An error was detected in the data comm. with the power supply
   - Measures: Turn the power OFF and ON once
   - Reset: Power off and on, unless otherwise noted.

- H0770: Power supply process error
   - Cause: An error occurred in the process cycle of power supply
   - Measures: Turn the power OFF and ON once. Check that there is not a source of noise. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H078n: Servo amplifier watch dog
   - Cause: The servo amplifier software process is not operating correctly.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H079n: Servo amplifier board error
   - Cause: An error was detected in the servo amplifier's PCB.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H080n: Servo amplifier clock error
   - Cause: An error was detected in the servo amplifier's clock.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H081n: Servo amplifier under voltage
   - Cause: The PN bus voltage dropped to 200V or less. Momentary power failure may have occurred.
   - Measures: Check the primary voltage.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H081n: Voltage error at accel/decel
   - Cause: A motor control error was detected due to an input voltage drop
   - Measures: Turn the power OFF and ON once. Check the primary voltage.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H0820: Motor ground fault
   - Cause: A motor ground fault was detected. A connection or conductance error may have occurred in the motor cable.
   - Measures: Turn the power OFF and ON once. Check connection and continuity of motor cable.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H082n: Motor ground fault
   - Cause: A motor ground fault was detected. A connection or conductance error may have occurred in the motor cable.
   - Measures: Turn the power OFF and ON once. Check connection and continuity of motor cable.
   - Reset: Power off and on, unless otherwise noted.

- H083n: Servo amplifier overvoltage
   - Cause: The PN bus voltage rose to 400V or more.
   - Measures: Check the primary power supply voltage. Turn the power OFF and ON once.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H0840: Instantaneous power failure (SRV)
   - Cause: A power shutdown status of 50 msec or longer has occurred.
   - Measures: Check the power voltage. Turn the power OFF and ON once.
   - Reset: Power off and on, unless otherwise noted.

- H0850: Power supply voltage incorrect
   - Cause: The input power (L1, L2, L3) has an open phase, the voltage is not within the specifications, or the 100V/ 200V specifications changeover setting is incorrect.
   - Measures: Check the power connection, power state or the setting.
   - Reset: Power off and on, unless otherwise noted.

- H0860: Power supply overvoltage
   - Cause: The voltage across the converter's L+ and L- exceeded 410V.
   - Measures: Check the power supply connection and power supply state.

- H0870: Power module overheat (Fan Stop)
   - Cause: A cooling fan stopped, and overheat occurred
   - Measures: Check the rotation of fan, and the connector connected to a fan
   - Reset: Power off and on, unless otherwise noted.

- H0880: Power module overheat
   - Cause: Overheating of the power module regenerative resistor was detected.
   - Measures: Turn the controller power OFF, wait a while, and then turn ON again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H088n: Power module overheat
   - Cause: Overheating of the power module regenerative resistor was detected.
   - Measures: Turn the controller power OFF, wait a while, and then turn ON again. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H089n: Servo amplifier motor overheat
   - Cause: The position detector's thermal protector activated.
   - Measures: Turn the controller power OFF, wait a while, and then turn ON again. Decrease the acceleration/ deceleration time of the operation speed, for instance. Refer to "Detailed explanation of command words"/"Accel (Accelerate)," "Ovrd (Override)" and "Spd (Speed)," or "Detailed explanation of Robot Status Variable"/"M\_SetAdl," "M\_LdfAct" and "Functions set with parameters"/"JADL (Optimum acceleration/deceleration adjustment rate)" of the Separate Volume, "INSTRUCTION MANUAL/Detailed Explanation of Functions and Operations."
   - Code note: (n indicates the axis number (1 to 8).)

- H090n: Absolute position overspeed
   - Cause: It moved 45 mm/sec or faster with the absolute position linear scale during initialization.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H091n: Servo amplifier overspeed
   - Cause: A speed exceeding the motor's tolerable speed was detected.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H0920: Power module overcurrent
   - Cause: A servo amplifier or power supply overcurrent was detected. An error was detected in the servo amplifier's gate circuit. A connection or conductance error may have occurred in the motor cable.
   - Measures: Confirms the connection of the machine cable and the locomotion-axis cable. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H092n: Power module overcurrent
   - Cause: A servo amplifier or power supply overcurrent was detected. An error was detected in the servo amplifier's gate circuit. A connection or conductance error may have occurred in the motor cable.
   - Measures: Confirms the connection of the machine cable and the locomotion-axis cable. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H093n: Motor overcurrent
   - Cause: An excessive current flowed to the motor, or the A/D converter output is abnormal. An abnormality may have occurred in the connection of the motor's power line.
   - Measures: Turn the power OFF and ON once. Confirms the connection of the machine cable and the locomotion-axis cable etc. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H093n: Motor overcurrent (Grounding)
   - Cause: The motor power cable is in contact with ground
   - Measures: Turn the power OFF and ON once. Confirms the connection of the machine cable and the locomotion-axis cable etc. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H094n: Overload (over weight 1)
   - Cause: Operation tight for a motor (operation with high duty) was performed more than fixed time.
   - Measures: Decrease the acceleration/deceleration time of the operation speed, for instance. Refer to "Detailed explanation of command words"/"Accel (Accelerate)," "Ovrd (Override)" and "Spd (Speed)," or "Detailed explanation of Robot Status Variable"/"M\_SetAdl," "M\_LdfAct" and "Functions set with parameters"/"JADL (Optimum acceleration/deceleration adjustment rate)" of the Separate Volume, "INSTRUCTION MANUAL/Detailed Explanation of Functions and Operations." Confirms that conveyance conditions (hand mass, work-piece mass) are less than specification values. When it comes back, contact to the dealer.
   - Code note: (n indicates the axis number (1 to 8).)

- H095n: Overload (over weight 2)
   - Cause: The maximum output current continued for more than one second.
   - Measures: Check the load weight and the robot pressing, etc. Confirms that conveyance conditions (hand mass, work-piece mass) are less than specification values. When it comes back, contact to the dealer.
   - Code note: (n indicates the axis number (1 to 8).)

- H096n: Excessive error 1
   - Cause: The position error exceeded at servo ON. Moreover, this error may occur during the emergency-stop deceleration.
   - Measures: Check the load weight and press, etc. Confirms the connection of the machine cable and the locomotion-axis cable etc. If the surrounding temperature is low, or starting after stopping operation for an extended period of time, perform running-in operation at low speed or use the warm-up operation mode. When hand offset is long and acceleration-and-deceleration control is fixed or tracking is active in RH- 20FRH series. please reduce the acceleration and deceleration speeds (Accel command) and movement speed (Ovrd command). Refer to separate "Instruction Manual/Detailed Explanation of Functions and Operations" for details of each command. (This error may occur during the emergency-stop deceleration.) While operation is performed in the compliance mode of the joint coordinate system, if the Excessive error 1 (H096n) occurs, increase the set value of parameter CMPJCLL to suppress this error. Refer to "Detailed explanation of command words"/"Cmp Jnt (Compliance Joint)" and "Movement parameter"/"CMPJCLL (Current Limit Level for Cmp Jnt)" given in separate "INSTRUCTION MANUAL/ Detailed explanation of functions and operations".
   - Code note: (n indicates the axis number (1 to 8).)

- H097n: Excessive error 2
   - Cause: The position error exceeded at servo OFF.
   - Measures: Check the moving robot arm by something power. When it comes back, contact to the dealer.
   - Code note: (n indicates the axis number (1 to 8).)

- H098n: Excessive error 3
   - Cause: Abnormal motor power line connection.
   - Measures: Check the connection of motor power line. When the excessive error 1 was detected, the current of the motor is off.
   - Code note: (n indicates the axis number (1 to 8).)

- H101n: Collision detection
   - Cause: A collision was detected.
   - Measures: 1) If the robot has stopped by interference with peripheral equipment, move the arm to part from peripheral equipment using jog operation. Depending on the level of collision, the collision detection error may occur again. In that case, turn on the servo power again and do jog operation. If it still recurs, release the brake and move the arm by hand. 2) If this error occurs without having collided, please adjust the collision detection level. If collision is detected incorrectly during automatic operation, enlarge the setting value of the parameter (COLLVL) corresponding to axis. If collision is detected incorrectly during jog operation, enlarge the setting value of the parameter (COLLVLJG) corresponding to axis. However, since the detection level drops by enlarging the set value, don't enlarge too much. And, the incorrect detection can be reduced when setup value of parameter (HNDDATn WRKDATn) is correct. 3) If the speed excessive error has occurred at the same time, the torque alteration by rapid speed change may be detected as a collision state. Remove other causes of the error and confirm movement again. 4) In case of operation under the environment of low temperature or after the long term stoppage, the col- lision detection error may occur by viscous transmutation of the grease used. In such a case, operate by accustoming at low speed (warm-up), or use the warm-up operation mode.
   - Code note: (n indicates the axis number (1 to 8).)

- H102n: Servo AMP over-regeneration
   - Cause: The additional axis exceeded the regenerative performance limit.
   - Measures: Check the regenerative capacity and parameters for the additional axis. If it comes back, contact your service provider. Regeneration resistance may be disconnected.
   - Code note: (n indicates the axis number (1 to 8).)

- H1030: Power supply over-regeneration
   - Cause: The converter's regenerative performance limit was exceeded.
   - Measures: Wait at least 15 minutes in the power ON state, and then turn the power OFF and ON. If it comes back, contact your service provider. Regeneration resistance may be disconnected.
   - Reset: Power off and on, unless otherwise noted.

- H104n: Encoder init communication error
   - Cause: An abnormality may have occurred in the position detector cable connection.
   - Measures: 1) Turn the power OFF and ON once. If it comes back, contact your service provider. 2) When instantaneous power failure errors occur simultaneously, turn on the power supply again.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H105n: Encoder init communication error
   - Cause: Initial communication with the encoder was not possible
   - Measures: Check the encoder cable connection
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H106n: Encoder communication error
   - Cause: Communication between the encoder and detector was cut off
   - Measures: Check the encoder cable connection
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H107n: Encoder communication error
   - Cause: Communication with the position detector was cut off.
   - Measures: Confirms the signal cable of the machine cable and the locomotion-axis cable etc. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H108n: Servo AMP communication error
   - Cause: An abnormality may have occurred in the communication cable connection.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider. CR800-R/CR800-Q: Check the connection between the robot CPU system and the controller.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1090: Servo AMP initialization error
   - Cause: An abnormality may have occurred in the servo axis settings (parameters, rotary switches).
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider. CR800-R/CR800-Q: Check the connection between the robot CPU system and the robot controller. And when turning the power ON, turn on the switch of the robot controller, then of the robot CPU. Using the additional axis: Confirms the axis setting switch, cable connection with robot controller, setting parameter, condition of power supply of additional axis amplifier, type of additional axis amplifier etc. When applying a power supply, turn on the additional axis amplifier first, then turn on the controller. And when turning the power ON, turn on the switch of the drive unit, then of the robot CPU.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H109n: Servo AMP initialization error
   - Cause: An abnormality may have occurred in the servo axis settings (parameters, rotary switches).
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider. CR800-R/CR800-Q: Check the connection between the robot CPU system and the robot controller. And when turning the power ON, turn on the switch of the robot controller, then of the robot CPU. Using the additional axis: Confirms the axis setting switch, cable connection with robot controller, setting parameter, condition of power supply of additional axis amplifier, type of additional axis amplifier etc. When applying a power supply, turn on the additional axis amplifier first, then turn on the controller. And when turning the power ON, turn on the switch of the drive unit, then of the robot CPU.
   - Reset: Power off and on, unless otherwise noted.

- H1100: Servo com. receive error
   - Cause: Abnormality occurred in data reception from a servo amplifier
   - Measures: Check the communication cable connection and conductivity
   - Reset: Power off and on, unless otherwise noted.

- H1110: Servo communication timeout
   - Cause: Data from a servo amplifier is not received
   - Measures: Check the communication cable connection and conductivity
   - Reset: Power off and on, unless otherwise noted.

- H111n: SRV-AMP comm. error
   - Cause: A communication error was detected between the servo amplifier and PC.
   - Measures: Check the communication cable connection and conductivity. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- H112n: Encoder ABS position data lost
   - Cause: The absolute position data in the position detector was lost. The voltage of the robot-arm or additional axis's backup battery may be dropping.
   - Measures: Please set up the origin by ABS method after replacing the batteries. (Since position data can be perfectly restored if the origin is set up by the ABS method, re-teaching is unnecessary.) Refer to separate "Instruction Manual/ROBOT ARM SETUP &amp; MAINTENANCE" for ABS method. If the voltage of robot battery is low, reset the error, and set up the origin by ABS method. Then, the robot can be operated without replacing the battery. However, this error occurs again when the controller is re- turned on. It is recommended to replace the battery at the earliest opportunity.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H113n: Encoder per rotation data error
   - Cause: An error was detected in the position detector's one rotation data.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H113n: Encoder data scattered
   - Cause: Position deviation by scattered encoder data occurred
   - Measures: Turns off the power supply once and turns on again. Check that there is not a source of excessive vibration. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H114n: SRV-AMP Comm. data error (CRC)
   - Cause: A CRC error was detected in the data from servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H115n: Large command position
   - Cause: The command position from the RC is abnormally large.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H116n: SRV-AMP Comm. error (frame)
   - Cause: An error was detected in the communication frame from servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H117n: SRV-AMP Comm. error (info)
   - Cause: An error was detected in the communication information from the RC.
   - Measures: Check the communication cable connection and conductivity. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H118n: Servo amplifier feedback error 1
   - Cause: Pulses skipped in the position detector's feedback signal.
   - Measures: Check the detector cable connection and conductivity. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H119n: Servo amplifier feedback error 2
   - Cause: Displacement occurred in the feedback amounts between the detectors on the motor edge and on the machine edge.
   - Measures: Check the detector cable connection and conductivity. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1200: SRV-AMP Comm. data error (CRC)
   - Cause: A CRC error was detected in the communication data from the servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H121n: SRV-AMP Comm. data error (ID)
   - Cause: A data ID error was detected in the communication data from the servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H122n: SRV-AMP Comm. data error (axis No)
   - Cause: An axis No. error was detected in the communication data from the servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H123n: SRV-AMP Comm. data error (SubID)
   - Cause: A Sub ID error was detected in the communication data from the servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1240: SRV-AMP Comm. data error (frame)
   - Cause: An No. of received frames error was detected in the communication data from the servo amplifier.
   - Measures: Turns off the power supply once and turns on again. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H125n: Servo amplifier parameter error
   - Cause: An error was detected in the servo parameter.
   - Measures: Confirms whether the type displayed on the rated name plate of controller and T/B is the same. If different, restores the type data (backup data). If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- C126n: Encoder communication error
   - Cause: Initial communication could not be established with the low-speed serial type absolute position linear scale.
   - Measures: Check the detector cable connection and conductivity. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C127n: Encoder communication error
   - Cause: The serial data of absolute position was abnormally transmitted.
   - Measures: Check the detector cable connection and conductivity. Moreover, confirms carefully that the moving posi- tion of the robot has not deviated, and if it has deviated, sets up the origin again. Refer to separate "Instruction Manual/ROBOT ARM SETUP &amp; MAINTENANCE" for operation method. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C128n: Encoder serial format error
   - Cause: Absolute position serial data format was incorrect.
   - Measures: Check the detector cable connection and conductivity. Moreover, confirms carefully that the moving posi- tion of the robot has not deviated, and if it has deviated, sets up the origin again. Refer to separate "Instruction Manual/ROBOT ARM SETUP &amp; MAINTENANCE" for operation method. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C129n: Absolute position fluctuation
   - Cause: The absolute position data fluctuated when the power was turned ON.
   - Measures: Check whether the axis moved due to arm dropping or external force when the power was turned ON.
   - Code note: (n indicates the axis number (1 to 8).)

- C130n: Servo AMP MP scale F/B error
   - Cause: Excessive displacement was detected in the feedback amounts between the detector and the MP scale.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C131n: Servo AMP MP scale offset error
   - Cause: Excessive displacement was detected in the feedback amounts between the detector and the MP scale.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C132n: Multi-rotation data error
   - Cause: An error was detected in the position detector's multi-rotation data.
   - Measures: When the power supply is turned on next time, the current position data may not be correctly detected. Please re-turn on the controller power supply and set up the origin by ABS method. Refer to separate "Instruction Manual/ROBOT ARM SETUP &amp; MAINTENANCE" for ABS method. (Even if this warning occurs, unless the controller is re-turned on, it can operate perfectly. However, when re-turning on the controller, the position gap may occur. Please be sure to set up the origin by ABS method after re-turning on the controller power supply. Current position data returns to the normal posi- tion before error occurrence.) When it comes back, contact to the dealer.
   - Code note: (n indicates the axis number (1 to 8).)

- C133n: Encoder battery voltage low
   - Cause: The battery voltage supplied to the position detector dropped.
   - Measures: Replace the backup battery. For more information about the replacement procedure, refer to the separate volumes, "Instruction Manual/Robot Arm Setup to Maintenance". Even if this warning occurs, unless the controller is re-turned on, it can operate perfectly. However, if the battery consumption is intense, the Encoder ABS position data lost error (H112n) may occur when the controller is re-turned on. It is recommended to replace the battery at the earliest opportunity.
   - Code note: (n indicates the axis number (1 to 8).)

- C134n: Over-regeneration warning
   - Cause: The regenerative level of the additional axis has risen to 80% or more.
   - Measures: Check the regenerative capacity and parameters for the additional axis. If it comes back, contact your service provider. Regeneration resistance may be disconnected.
   - Code note: (n indicates the axis number (1 to 8).)

- C135n: Overload warning
   - Cause: The overload level reached 95% or more.
   - Measures: Check the load weight and the robot for collisions, etc.
   - Code note: (n indicates the axis number (1 to 8).)

- H136n: Absolute position counter error
   - Cause: The counter of absolute position is illegal.
   - Measures: Confirm whether the connection of encoder cable and the battery voltage of arm is falling.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- C137n: Illegal parameter (servo)
   - Cause: A parameter was set exceeding the setting range.
   - Measures: The parameter has not been changed. Reset the correct value. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C138n: Removing control axis (servo)
   - Cause: An instruction to remove the axis was issued by the controller.
   - Measures: Cancel the instruction of removing axis.
   - Code note: (n indicates the axis number (1 to 8).)

- H1390: Emergency stop (Servo amplifier)
   - Cause: The emergency stop command has been input from the RC
   - Measures: Release the emergency stop state

- C1400: regeneration(AUX) frequency over
   - Cause: Regeneration at the limit has occurred frequently
   - Measures: Check the regeneration capacity

- H1410: Instantaneous power interruption
   - Cause: The power was momentarily interrupted
   - Measures: Check power specification, and wiring of power and external emergency stop
   - Reset: Power off and on, unless otherwise noted.

- C1420: Over-regeneration warning
   - Cause: The regeneration level reached 80 or more.
   - Measures: Lower the robot's movement speed. If it comes back, contact your service provider. Regeneration resistance may be disconnected.

- C1430: Servo amplifier main circuit OFF
   - Cause: The servo turned ON while the main circuit power was OFF
   - Measures: Turn the main circuit power ON

- H144n: System error (servo 2)
   - Cause: Trouble occurred in the current processing processor.
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1450: Instantaneous power interruption
   - Cause: The power was momentarily interrupted at the power supply
   - Measures: Check power specification, and wiring of power and external emergency stop
   - Reset: Power off and on, unless otherwise noted.

- H1460: Power supply overcurrent
   - Cause: Overcurrent in the power module in the power supply.
   - Measures: Check the AC power line. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H1470: Frequency error
   - Cause: AC frequency is out of range.
   - Measures: Check the AC power line frequency. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H148n: Power supply parameter error
   - Cause: Illegal at the power supply parameter
   - Measures: Turn the power OFF and ON once. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1490: Power supply Power module overheat.
   - Cause: The temperature protection function of the power module operated.
   - Measures: Clean up or replaces the air filters. When it comes back, contact to the dealer.
   - Reset: Power off and on, unless otherwise noted.

- H1491: Illegal converter thermal servo AMP.
   - Cause: Overheating of servo AMP converter.
   - Measures: Turn the power OFF, wait a while, and then turn ON again.

- H1492: Power supply converter overheat
   - Cause: Overheating of the converter was detected
   - Measures: Turn the power OFF, wait a while, and then turn ON again
   - Reset: Power off and on, unless otherwise noted.

- H1493: Rush resistance overheat
   - Cause: Overheating of the rush resistance was detected
   - Measures: Turn the power OFF, wait a while, and then turn ON again
   - Reset: Power off and on, unless otherwise noted.

- H1494: Discharge resistance overheat
   - Cause: Overheating of the discharge resistance was detected
   - Measures: Turn the power OFF, wait a while, and then turn ON again
   - Reset: Power off and on, unless otherwise noted.

- H150n: Motor combination error
   - Cause: The servo motor which does not correspond to addition axis servo amplifier is connected.
   - Measures: Confirm the specifications of servo amplifier and the motor.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H151n: SRV-AMP USB comm. error
   - Cause: A communication error was detected between the servo amp and PC
   - Measures: Check the communication cable connection and conductivity
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H152n: Servo motor output watt over
   - Cause: The output wattage of the servo motor exceeded ratings
   - Measures: Lower the rotation speed of the servo motor
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H154n: Communication error between units
   - Cause: The transmission data between servo amplifier units is illegal
   - Measures: Check the communication cable connection and conductivity
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1550: Emergency stop is a breakdown
   - Cause: The external emergency stop input is illegal
   - Measures: Turn the power OFF and ON once. When it comes back, contact to the dealer.
   - Reset: Power off and on, unless otherwise noted.

- H156n: Excessive error 4
   - Cause: The axis moved while executing servo ON processing.
   - Measures: If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- H157n: Non-registered servo error. (This error may be caused by the error on the amplifier for additional axis.)
   - Cause: A non-registered servo alarm occurred.
   - Measures: Confirms the code by LED of servo additional axis amplifier, and refer to the instruction manual of servo amplifier. If the alarm cannot be reset, turn the power OFF and ON. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- C158n: Non-registered servo warning. (This caution may be caused by the warning on the amplifier for additional axis.)
   - Cause: A non-registered servo warning occurred.
   - Measures: Confirms the code by LED of servo additional axis amplifier, and refer to the instruction manual of servo amplifier. If the alarm cannot be reset, turn the power OFF and ON. If it comes back, contact your service provider.
   - Code note: (n indicates the axis number (1 to 8).)

- H1600: Mechanism un-setting.
   - Cause: The mechanism is not set up.
   - Measures: Set up one or more of mechanism. If this alarm occur after the restoring the data of controller check the restored data. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H1601: Unmatched robot model
   - Cause: Robot arm that connection is not correct
   - Measures: Please confirm the connected robot arm
   - Reset: Power off and on, unless otherwise noted.

- H1610: System error (illegal MEMECH)
   - Cause: The mechanism module name is illegal or not registered.
   - Measures: Correctly set. If it comes back, contact your service provider. If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- C1620: Illegal robot No.
   - Cause: When specifying the mechanism number by the external communications protocol, the mechanism number not existing was specified.
   - Measures: Specify the existing mechanism number.

- C1630: Cannot servo ON (during error)
   - Cause: The servo cannot be turned ON during a servo error.
   - Measures: Reset the servo error before turning the servo ON.

- C1640: Cannot servo ON (DEADMAN OFF)
   - Cause: The servo cannot be turned ON while the enable switch is OFF.
   - Measures: Turn the enable switch ON before turning the servo ON.

- C1650: Cannot servo ON (brake OFF)
   - Cause: The servo cannot be turned ON when there is an axis with the brakes released.
   - Measures: Lock the brakes for all axes before turning the servo ON.

- C1660: Cannot servo ON (SRVON process)
   - Cause: The servo cannot be turned ON during the servo ON process.
   - Measures: Operate it after servo ON process is finished.

- C1670: Cannot servo ON (SRVOFF process)
   - Cause: The servo OFF process is being carried out.
   - Measures: Operate it after servo OFF process is finished.

- H1680: Cannot servo ON (timeout)
   - Cause: The servo did not turn ON within the specified time.
   - Measures: Turn the power supply OFF and ON once. If it comes back, confirm that power supply voltage is in the specification value, and connection of the external emergency stop is correct. And, when using the addition axis, confirm that the power supply volt- age to the servo amplifier of addition axes is in the specification value and connection of AXMC is correct. If it comes back, contact your service provider.

- H1681: Unexpected servo OFF
   - Cause: The servo turned OFF unexpectedly.
   - Measures: Confirm that primary power supply voltage is in the specification value and wiring of the external emergency stop line is correct. When using the additional axis, confirms whether the alarm occur on the additional axis. If it comes back, contact your service provider.

- H1682: Servo ON Timeout (Safety relay).
   - Cause: A main circuit voltage did not rise because of a failure of the Safety relay on a converter card.
   - Measures: Turn the power OFF and ON once. Confirm that wiring of the external emergency stop line is correct. Confirm whether there is any problem in wiring of the external emergency stop switch. Refer to the "Exam- ples of safety measures" given in separate "Standard Specifications Manual" for external emergency stop switch wiring. Please confirm whether in use of addition axis, there is any failure in the servo amplifier for addition axes. If it comes back, contact to your service provider.

- H1683: Servo ON Timeout. (Contactor)
   - Cause: A main circuit voltage did not rise because of contactor welded.
   - Measures: Turns off the power supply once and turns on again. Confirm that primary power supply voltage is in the specification value and wiring of the external emer- gency stop line is correct. When it comes back, contact to the dealer.

- C1690: Cannot brake operation (DEADMAN)
   - Cause: The servo cannot be turned ON while the enable switch is OFF.
   - Measures: Turn the enable switch ON before turning the servo ON.

- C1700: Cannot brake operation (EMG)
   - Cause: The brakes cannot be released while the emergency stop is input.
   - Measures: Release the emergency stop state before operating.

- C1710: Cannot brake operation (SRVON)
   - Cause: The brakes cannot be operated during servo ON.
   - Measures: Turn the servo OFF before operating.

- C1720: Cannot brake operation (BRK OFF)
   - Cause: The brakes cannot be released during the brake release process.
   - Measures: Operate it after brake is released.

- C1730: Cannot brake operation (BRK ON)
   - Cause: The brakes cannot be locked during the brake lock process.
   - Measures: Operate it after brake is locked.

- C1740: Servo parameter change failure
   - Cause: Other parameters cannot be changed during the parameter change process.
   - Measures: Carry out the parameter change process again.

- C1750: Servo parameter change failure
   - Cause: Changes of the servo parameter failed.
   - Measures: Carry out the parameter change process again.

- C1760: Illegal origin data
   - Cause: The origin setting data is not correct.
   - Measures: Set the correct origin setting data. Confirm mistakes such as "1(one)" and "I(alphabet)", or "O(alphabet)" and "0(zero)", etc.

- C1761: Illegal origin data in robot arm
   - Cause: The origin data is illegal in robot arm
   - Measures: Setting the origin

- C1770: Origin setting incomplete
   - Cause: The origin is not set.
   - Measures: Re-execute after setting the origin.

- C1780: Cannot set origin (illegal axis)
   - Cause: The origin was not set simultaneously for the interference axis.
   - Measures: Set the origin simultaneously for the interference axis. For example, the J3 axis and the J4 axis of the RH type robot. Please refer to the section of origin setting of separate manual: "ROBOT ARM SETUP &amp; MAIN- TENANCE" for details.

- C1781: Cannot set origin (SRVON)
   - Cause: The origin was set during servo ON.
   - Measures: Turn the servo OFF before setting the origin.

- H179n: Illegal parameter (MEJAR)
   - Cause: The parameter (MEAJAR) setting is illegal. Or the setting value of the parameter related to addition axis control have exceeded the controllable oper- ating range.
   - Measures: The useful range of the operating range setting parameter (MEAJAR) is -131072.00 to +131072.00. Correct, if the set value is over the range. Or confirm the setting value of the parameter related to addition axis control.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H1800: Illegal parameter (MEMAR)
   - Cause: The ABS operation range setting parameter MEMAR setting is incorrect. (Minus side value is larger than "0", or plus side value is smaller than "0")
   - Measures: Set the value of the parameter "MEMAR" within the limits.
   - Reset: Power off and on, unless otherwise noted.

- H1810: Illegal parameter (USERORG)
   - Cause: The user origin setting parameter USERORG setting is incorrect.
   - Measures: Correct the parameter USERORG.
   - Reset: Power off and on, unless otherwise noted.

- L182n: Pos. data disagree.Check origin
   - Cause: Position data changed during power off.
   - Measures: Check the origin, re-install if shifting. Re-set up the origin only for the axis which deviated by the ABS method. Refer to separate "Instruction Manual/ROBOT ARM SETUP &amp; MAINTENANCE" for ABS method.
   - Code note: (n indicates the axis number (1 to 8).)

- L1830: JRC. Exseeds the Pos. limit
   - Cause: The JRC instruction exceeding the operation range was executed.
   - Measures: Check the current position and the operating range.

- L184n: JRC Qtt. Setting Too Large
   - Cause: The movement amount setting of the JRC is excessive.
   - Measures: Correct the JRCQTT parameter.
   - Code note: (n indicates the axis number (1 to 8).)

- C1850: Instantaneous power failure
   - Cause: There was the momentary power failure.
   - Measures: Check the power supply connection and power supply state.

- L1860: Illegal parameter (TLC)
   - Cause: The setting of the TLC parameter that sets the approach direction was incorrect.
   - Measures: Correct the TLC parameter. (=X/Y/Z)

- L1864: FTP parameter setting error (\*\*) Note) "\*\*" is substituted with the "parameter name".
   - Cause: The FTP communication parameter setting lies outside the range.
   - Measures: Check the setting and correct.

- H188n: Jn addition axis amplifier cooling fan stop.
   - Cause: The cooling fan of the addition axis amplifier of Jn axis may be out of order.
   - Measures: Please replace the cooling fan of addition axis amplifier. \* Please also refer to the instruction manual of the servo amplifier of usage.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- C189n: Jn addition axis amplifier cooling fan rev fall.
   - Cause: The cooling fan of the addition axis amplifier of Jn axis may be out of order.
   - Measures: Please replace the cooling fan of addition axis amplifier. \* Please also refer to the instruction manual of the servo amplifier of usage.
   - Code note: (n indicates the axis number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- C1940: Agitating fan stopped
   - Cause: The fan for agitating inside the robot controller has stopped.
   - Measures: Confirm rotation of the target cooling fan, and if out of order, replace them. Please refer to Page 68, "2.2 Fan installation place of robot controller." for the fan's mounting place.

- H195n: Additional axis AMP error xx
   - Cause: Alarm of the addition axis servo amplifier was detected.
   - Measures: Refer to the instruction manuals of addition axis servo amplifier for the details of alarm. "xx" of the error message corresponds to the alarm number of addition axis servo amplifier (MR-J4- B). (The error reset methods differ for each alarm number of addition axis servo amplifier.)

- C196n: Additional axis AMP Warning xx
   - Cause: Warning of the addition axis servo amplifier was detected.
   - Measures: Refer to the instruction manuals of addition axis servo amplifier for the details of warning. "xx" of the error message corresponds to the warning number of addition axis servo amplifier (MR-J4- B).

- C1970: Agitating fan stopped (robot)
   - Cause: The agitating fan in the robot stopped
   - Measures: Confirm the rotation of the agitating fan \* According to the type of the stopped fan, "inside the robot arm" and "inside the robot base" are shown separately.

- L2000: The servo is OFF
   - Cause: Because servo is turned off, the robot can't move.
   - Measures: Turn the servo ON and then restart.

- L2010: Pulse output was not possible
   - Cause: There is an error in the pulse output designation.
   - Measures: Correct the program.

- L2020: Reading external position data
   - Cause: A command that cannot be executed while reading the external commands was executed.
   - Measures: Correct the program.

- L2030: JOG operation cannot be accepted
   - Cause: The JOG operation request was issued when the JOG operation request could not be accepted.
   - Measures: Changes the JOG mode after the JOG operation.

- H2031: "Illegal parameter(JOGTSJ,JOGJSP)"
   - Cause: The parameter JOGTSJ, JOGJSP settings are not correct. \[JOGPSP\] \[JOGJSP\] = (element 1, element 2) = (constant high, constant low)
   - Measures: Set the set dimension to 5 or less.
   - Reset: Power off and on, unless otherwise noted.

- H2040: Teaching position is not correct.
   - Cause: Change the teaching position.
   - Measures: The work coordinates were not able to be calculated from the teaching position. The cause is the following content. 1. Two points are the same position. 2. Three points line up on the straight line. Confirm and correct the teaching position

- L2041: Can't calculate frame transformation coordinates.
   - Cause: The position data that defines the coordinate system used with frame transformation are on the same point or arranged on a straight line so the coordinate system could not be calculated.
   - Measures: Change the position data to appropriate positions so the coordinate system can be calculated.

- L2042: Frame transformation coordinates are not set
   - Cause: Frame transformation was attempted even though the coordinate system for frame transformation was not set.
   - Measures: Set the coordinate system or cancel frame transformation. Check that the MvSpl command argument &lt;Frame transformation&gt; designation is correct.

- L2050: CPU processing time exceeds
   - Cause: Because the function made effective is many at the same time
   - Measures: The CPU processing time exceeded the limiting value. Please take measures of either of following. 1. If interference avoidance function is activated, changes some cylinder models into the sphere model, or reduces the number of the models for checking . 2. Invalidate some functions of following. Interference avoidance, User definition area, Free plane limit, Collision detection, Compliance, Tracking, Force sense 3. Reduces the Interrupt definition function currently used by the program.

- H2090: In interference zone n. (n indicates the zone number (1 to 32).)
   - Cause: Movement outside the user-defined area "n" range was attempted.
   - Measures: Adjust the position.

- H211n: Free plane n overrun
   - Cause: Movement outside the plane defined with free plane "n" was attempted. plane number (1
   - Measures: Adjust the position.
   - Code note: (n indicates the to 8).)

- H212n: Free plane n overrun
   - Cause: Movement outside the plane defined with free plane "n" was attempted. plane number (1
   - Measures: Adjust the position.
   - Code note: (n indicates the to 8).)

- H2129: Free plane limit data illegal
   - Cause: The free plane data setting value is illegal.
   - Measures: The two points are the same in the three points of parameter: SFCnp. The value of parameter: SFCnAT is except "0, 1, -1".

- H213n: Jn Speed is excessive (command)
   - Cause: The speed instruction value of n axis exceeded the permissible value. When moving by the linear interpolation (or circle interpolation), needs to rotate the motor at the big speed depending on the pose. If the specified speed is large, the speed instruction value to the motor may exceed the permissible value.
   - Measures: Please lower moving speed by the Ovrd command and Spd command or change the movement position.
   - Code note: (n indicates the axis number (1 to 8).)

- H214n: Jn +ABS limit over
   - Cause: The axis "n" + ABS limit was exceeded.
   - Measures: Referring to "Operation to Temporarily Reset an Error that Cannot Be Canceled" in the separate volume, "Instruction Manual/Detailed Explanation of Functions and Operations", reset the error and move the axis "n" within the operation range using JOG operation.
   - Code note: (n indicates the axis number (1 to 8).)

- H215n: Jn -ABS limit over
   - Cause: The axis "n" - ABS limit was exceeded.
   - Measures: Referring to "Operation to Temporarily Reset an Error that Cannot Be Canceled" in the separate volume, "Instruction Manual/Detailed Explanation of Functions and Operations", reset the error and move the axis "n" within the operation range using JOG operation.
   - Code note: (n indicates the axis number (1 to 8).)

- H216n: Jn (+) angle exceeds the limit
   - Cause: The axis "n" + Joint limit was exceeded. When the teach mode, numerical "0" is set to n.
   - Measures: Adjust the position.
   - Code note: (n indicates the axis number (1 to 8).)

- H217n: Jn (-) angle exceeds the limit
   - Cause: The axis "n" - Joint limit was exceeded. When the teach mode, numerical "0" is set to n.
   - Measures: Adjust the position.
   - Code note: (n indicates the axis number (1 to 8).)

- H2181: X(+) data exceeds the limit
   - Cause: The X axis + XYZ limit was exceeded.
   - Measures: Adjust the position.

- H2182: Y(+) data exceeds the limit
   - Cause: The Yaxis + XYZ limit was exceeded.
   - Measures: Adjust the position.

- H2183: Z(+) data exceeds the limit
   - Cause: The Z axis + XYZ limit was exceeded.
   - Measures: Adjust the position.

- H2191: X(-) data exceeds the limit
   - Cause: X(-) data exceeds the limit
   - Measures: Adjust the position.

- H2192: Y(-) data exceeds the limit
   - Cause: Y(-) data exceeds the limit
   - Measures: Adjust the position.

- H2193: Z(-) data exceeds the limit
   - Cause: Z(-) data exceeds the limit
   - Measures: Adjust the position.

- H220m: SLP (ROBOT position error:FB Plane)
   - Cause: SLP detected the robot position FB over the monitoring plane
   - Measures: Please confirm the robot movement or related parameter setting
   - Code note: (m indicates number (1 to 8).)

- H220m: SLP (ROBOT position error:CMD Area)
   - Cause: SLP detected the robot position CMD over the monitoring area
   - Measures: Please confirm the robot movement or related parameter setting
   - Code note: (m indicates number (1 to 8).)

- H220m: SLP (ROBOT position error:FB Area)
   - Cause: SLP detected the robot position FB over the monitoring area
   - Measures: Please confirm the robot movement or related parameter setting
   - Code note: (m indicates number (1 to 8).)

- H221n: STR (ROBOT torque error)
   - Cause: Safety Torque Range monitoring detected over the torque limit
   - Measures: A feedback torque in excess of the predetermined allowable torque width is detected by the torque width monitoring function.
   - Code note: (n indicates the axis number (1 to 8).)

- H222m: DSI inconsistency
   - Cause: The duplex DSI status is not consistent.
   - Measures: The duplex DSI signal ON/OFF status is not consistent. Check the following for the DSI. DSI wiring Duplex signal ON/OFF status ON/OFF status switching timing (The error occurs when the ON/OFF status remains inconsistent between the duplex signals for about 0.1 seconds or more.)
   - Code note: (m indicates the DSI number (1 to 8).)
   - Reset: Power off and on, unless otherwise noted.

- H2230: Mismatch of Dual Safety Output
   - Cause: State of the redundant wiring do not match (Dual Safty Output)
   - Measures: Turn the power OFF and ON once If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H2231: Mismatch of Dual Safety Output Feedback
   - Cause: State of Dual Safty Output and the feeedback do not match
   - Measures: Turn the power OFF and ON once If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H2240: Origin data change
   - Cause: The origin data is changed during execution of the position monitoring function.
   - Measures: The error occurs when the origin data is changed by setting the origin during execution of the position monitoring function. After turning OFF the power, turn ON the power again to reset the error.
   - Reset: Power off and on, unless otherwise noted.

- C2250: Safety function execution disabled (No origin setting)
   - Cause: The safety function cannot be executed because the origin is not set.
   - Measures: The safety monitoring function is not activated when the origin is not set. Set the origin.

- H2260: Safety function execution disabled (No extended safety unit)
   - Cause: The extended safety unit of the robot safety option is not connected.
   - Measures: To use the safety monitoring function, it is necessary to connect the extended safety unit of the robot safety option to the controller. Connect the extended safety unit. If the error occurs even if the extended safety unit is connected, the extended safety unit may be faulty. Contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H2261: Extended safety unit's No. error
   - Cause: The extended safety unit's station No. is illegal
   - Measures: Set the extended safety unit's station No. to 2
   - Reset: Power off and on, unless otherwise noted.

- H2270: Cannot servo ON (SS1/STO active)
   - Cause: The servo cannot be turned ON while SS1/STO is active
   - Measures: Disable SS1 before turning the servo ON

- H2280: SS1 deceleration time exceeded
   - Cause: The robot didn't stop within deceleration time from SS1 enabled
   - Measures: Please confirm the robot movement and the load, stop speed parameter (SFSPZERO) setting

- H2281: SS2 deceleration time exceeded
   - Cause: The robot didn't stop within deceleration time from SS2 enabled
   - Measures: Please confirm the robot movement and the load, stop speed parameter (SFSPZERO) setting

- H2282: SOS (Position error)
   - Cause: Detect the change of position FB on SOS
   - Measures: An error that the position FB has changed on SOS due to external force was detected. Eliminate the cause of the external force or perform the risk assessment. Then modify the value of the parameter "SOSTLRNC" to extend the acceptable range of SOS.

- H2282: SOS (Speed error)
   - Cause: Detect FB speed over on SOS
   - Measures: Please check external force or related parameter settings

- H2282: SOS (Position command error)
   - Cause: Detect the change of position CMD on SOS.
   - Measures: Please confirm the robot movement or related parameter setting

- H2282: SOS (Speed command error)
   - Cause: Detect CMD speed over on SOS.
   - Measures: Please confirm the robot movement or related parameter setting

- H230n: SLS (Joint Speed Error)
   - Cause: Speed monitor detected the speed over
   - Measures: A speed feedback exceeding the limit speed is detected by the speed monitoring function. Check the robot movement or the monitoring speed setting. Or else, check the delay time (parameterSLSDLY) setting.
   - Code note: (n indicates the axis number (1 to 8).)

- H231n: SLS (XYZ Speed Error)
   - Cause: Speed monitor detected the speed over
   - Measures: A speed feedback exceeding the limit speed is detected by the speed monitoring function. Check the robot movement or the monitoring speed setting. Or else, check the delay time (parameterSLSDLY) setting.
   - Code note: (n indicates the direction in which an error is detected, 1: Composite speed, 2: X+, 3: X-, 4: Y+, 5: Y-, 6: Z+, 7: Z-)

- H2320: SF robot control error
   - Cause: The robot motion command and the feedback are inconsistent.
   - Measures: The robot position command and the feedback position are inconsistent. Check the details of the robot movement, the terminal load setting, or interference with the peripheral equipment.

- H2370: SF (Process error)
   - Cause: The Safety Function is not normally executed
   - Measures: Turn the power OFF and ON once. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- H2370: SF (Process Counter error)
   - Cause: The Safety Function of servo CPU is not normally executed
   - Measures: Turn the power OFF and ON once. If the same error recurs, contact the manufacturer.
   - Reset: Power off and on, unless otherwise noted.

- L240n: Collision avoidance detect (\*) Note) "(\*)": shows the detected model. (the the lower digit of the parameter for the model registry)
   - Cause: A collision avoidance was detected
   - Measures: Release the collision avoidance state
   - Code note: (n: Robot CPU No.)

- L241n: Collision avoidance detect (\*) Note) "(\*)": shows the detected model. (the the lower digit of the parameter for the model registry)
   - Cause: A collision avoidance was detected
   - Measures: Release the collision avoidance state
   - Code note: (n: Robot CPU No.)

- L2420: Collision avoidance comm. error
   - Cause: Collision avoidance comm. error
   - Measures: Check the robot controller

- L2421: A lot of colli. avoidance models
   - Cause: A lot of collision avoidance models
   - Measures: Reduce the collision avoidance models

- L2430: Collision avoidance re-detect
   - Cause: Collision avoidance was already detected
   - Measures: Move the robot arm from the interference area and resume the operation.

- L2500: Tracking encoder data error
   - Cause: An error was detected in the data of tracking encoder.
   - Measures: 1) The conveyor rotates at the fixed velocity. 2) The connection of the encoder. 3) The earth of the earth wire.

- L2510: Tracking parameter reverses
   - Cause: Tracking parameter\[EXCRGM\*\] Setting value reverses
   - Measures: Check the parameter\[EXCRGM\*\] value.

- L2520: Tracking parameter is range over
   - Cause: Tracking parameter\[TRBUF\] Setting value is range over. Setting range: element 1=1-8, element 2=1-64
   - Measures: Check the parameter\[TRUBF\] value.

- L2530: There is no area where data is written.
   - Cause: There is no area where data is written.
   - Measures: Please read the data by using TrRd

- L2540: There is no read data.
   - Cause: There is no read data.
   - Measures: Please use TrRd after executing TrWrt.

- L2560: Ilegal parameter of Tracking.
   - Cause: The value of the parameter: EXTENC is outside the range. setting range: 1-8.
   - Measures: Please check the value of parameter.

- L2580: No workpiece in the tracking area.
   - Cause: There is no workpiece in the tracking buffer or "TrkMv On" command is executed. Before the workpiece enters to the tracking area.
   - Measures: Execute "TrkMv On" command when the workpiece is in the tracking area.

- L2601: Start pos. exseeds the limit
   - Cause: The start position is outside the operation range.
   - Measures: Adjust the position.

- L2602: DSTN pos. exseeds the limit
   - Cause: The target position is outside the operation range.
   - Measures: Adjust the position.

- L2603: Med pos. data exseeds the limit
   - Cause: The intermediate position is outside the operation range.
   - Measures: Adjust the position.

- L2610: Spline interpolation error (Spline file)
   - Cause: An error related to the spline file occurred.
   - Measures: Refer to the error details No. and check the details of the occurring error. Then take actions for those error details.

- L2610: Can't open spline file
   - Cause: The spline file corresponding to the spline No. designated with the MvSpl command could not be opened.
   - Measures: Check that the spline No. designation is correct, and that the target spline file is saved in the controller.

- L2610: Spline file is broken
   - Cause: The spline file contents did not match the checksum.
   - Measures: Open the target spline file and check the contents. Then, save the file again to recreate the spline file. Write the new file to the controller again.

- L2610: Spline file is not supported.
   - Cause: The designated spline file cannot be used with the current controller.
   - Measures: Check the spline file and controller versions. It may be necessary to upgrade the software version.

- L2610: Can't change spline file.
   - Cause: The target spline file is currently being used for spline interpolation (file is open).
   - Measures: A spline file currently being used for spline interpolation (file is open) cannot be exported to the controller, deleted or renamed. Carry these out after spline interpolation ends.

- L2610: Can't get data.
   - Cause: The spline file is closed so the data cannot be retrieved.
   - Measures: Reset the program, and then execute the MvSpl command again.

- L2610: The setting of Ex-T is illegal
   - Cause: The setting of Ex-T is illegal
   - Measures: Please confirm the number and the content of the file

- L2610: Cannot make the spline file
   - Cause: The specified spline file is opened already
   - Measures: Please confirm the specified number

- L2611: Spline interpolation error (path point)
   - Cause: An error related to the path point registered in the spline file has occurred.
   - Measures: Refer to the error details No. and check the details of the occurring error. Then take actions for those error details.

- L2611: Path points are too close (nnnn)
   - Cause: The distance between path points is too short, or the speed command in respect to the path point distance is too high.
   - Measures: "nnnn" in the error message indicates the path point No. causing the error. Review this path point's posi- tion, or review the spline interpolation command speed. \* The high speed spline interpolation command has been added to the software version A3a. With this function enabled, a command speed higher than what was previously possible can be specified for the distance between the respective path points. Refer to the separate manual "Instruction Manual/Detailed Explanation of Functions and Operations" in details.

- L2611: The posture variation is too large (nnnn)
   - Cause: The variation in posture between path points is too large. (The posture variation angle exceeds 150 degrees.)
   - Measures: "nnnn" in the error message indicates the path point No. causing the error. Review the posture so that the posture variation amount for this path point is smaller, or add a path point to reduce the posture variation amount in the single block.

- L2611: Path point's configuration flag is incorrect (nnnn)
   - Cause: A different path point is registered for the configuration flag value.
   - Measures: "nnnn" in the error message indicates the path point No. causing the error. Review this path point's posi- tion, and change it so it is the same configuration flag as the other path points.

- L2611: Path point is not registered
   - Cause: The Specified path point is not registered
   - Measures: Please confirm the number and the content of the file

- L2611: Cannot open the path point file
   - Cause: The specified path point file does not exist
   - Measures: Please confirm the file name and existence of the file

- L2611: The format of file is different
   - Cause: The format of file is different
   - Measures: Please confirm the format of the file

- L2611: The number of point is illegal
   - Cause: The number of path point is illegal
   - Measures: Please confirm the file and the content of the file

- L2612: Spline interpolation error (Execution error)
   - Cause: An error related to the spline interpolation execution conditions occurred.
   - Measures: Refer to the error detail No. and check the details of the occuring error. Then take action for those error details.

- L2612: The robot does not support spline interpolation.
   - Cause: Spline interpolation was attempted with a robot that does not support spline interpolation.
   - Measures: Use a different command than spline interpolation.

- L2612: Can't execute with these start condition
   - Cause: Spline interpolation was attempted with a robot program in a slot having the start conditions ALWAYS ERROR.
   - Measures: Spline interpolation cannot be executed with a slot having the start conditions ALWAYS ERROR. Delete spline interpolation or change the start conditions to START.

- L2612: Another spline interpolation is being executed
   - Cause: Multiple spline interpolations cannot be executed simultaneously.
   - Measures: Check whether spline interpolation was attempted with a different robot during spline interpolation, or whether spline interpolation was directly attempted while spline interpolation was halted.

- L2612: Can't execute step return
   - Cause: Step return was attempted in respect to spline interpolation.
   - Measures: Spline interpolation does not support the step return operation. Do not attempt step return.

- L2613: Spline interpolation error (interpolation process)
   - Cause: An error occurred during the spline interpolation process.
   - Measures: Refer to the error details No. and check the details of the occurring error. Then take actions for those error details.

- L2613: Not enough path points
   - Cause: There are less than four path points registered in the spline file.
   - Measures: At least four path points are required to execute spline interpolation. Use a spline file in which four or more path points are registered.

- L2613: Not enough arc designated points (nnnn)
   - Cause: Three consecutive path points are not designated for the spline interpolation arc.
   - Measures: The number of points in the arc containing the path point No. "nnnn" displayed in the error message is insufficient. To generate an arc, an arc for three consecutive path points must be designated. This error occurs if there are only two consecutive points. Add another path point in the arc designation to move with an arc path.

- L2613: Block data calculation error (ssssssss)
   - Cause: An error occurred in the process to calculate the data related to the spline interpolation block.
   - Measures: The details of the error differ according to "ssssssss" in the error message.
      - Reg.Pt.: The information on the number of path points saved in the spline file does not match the number of path points actually registered. Open the spline file in the Spline File Edit screen once, save it again and export it to the controller.
      - Cir.Arc: The arc could not be generated. Check that the path points for the arc designation are not arranged on a straight line.
      - Frm.Cnv.: The frame transformation calculation failed. Review the path point positions.
      - Blk.Stp.: Generation of the data for the block stopped, and spline interpolation could not be executed. Reset the program.
      - PtoJ.: The joint angle cannot be calculated at the position. Review the path point positions.

- L2613: Block data does not exist
   - Cause: The load in the process for the controller during spline interpolation was large, and the data for the spline interpolation block could not be generated in time.
   - Measures: Check whether the load in the spline interpolation execution process can be reduced in the following ways.
      - Stop simultaneous execution of functions related to movement such as the collision detection function or visual control function.
      - Review the multi-tasks and reduce the number of slots executed simultaneously.
      - Reduce the dedicated output signal assignments.
      - Reduce the spline interpolation command speed.

- L2613: Spline interpolation command calculation error (nnnn)
   - Cause: An error occurred in the process for calculating the spline interpolation position commands.
   - Measures: Review the position of path point No. "nnnn" indicated in the error message or the MvSpl command argument’s setting value. Check that the cancel angle is set correctly. (Does the spline curve bend suddenly?)

- L2614: Spline interpolation error (other functions)
   - Cause: A function that cannot be used with the spline interpolation was executed.
   - Measures: Refer to the error details No. and check the details of the occurring error. Then take actions for those error details.

- L2614: Tracking function is enabled
   - Cause: The tracking function was enabled when attempting spline interpolation.
   - Measures: Spline interpolation and tracking function cannot be executed simultaneously. Disable the tracking function before executing spline interpolation.

- L2614: Can't change tool/base setting
   - Cause: The tool/base setting was changed during spline interpolation execution.
   - Measures: The tool/base setting cannot be changed during spline interpolation (including when halted). Change the settings after spline interpolation ends.

- L2614: Can't execute Jrc command
   - Cause: The Jrc command was executed during spline interpolation.
   - Measures: The Jrc command cannot be executed during spline interpolation (including when halted). Execute the command after spline interpolation ends.

- L2615: Spline interpolation error (exceeds setting range)
   - Cause: A setting value related to spline interpolation exceeds the setting range.
   - Measures: Refer to the error details No. and check the details of the occurring error. Then take actions for those error details.

- L2615: M_SplVar setting value exceeds setting range
   - Cause: A value exceeding the range was substituted into M_SplVar.
   - Measures: Substitute a value within the setting range (0 to 32767).

- L2615: M_SplVar setting range exceeds setting range (nnnn)
   - Cause: A value exceeding the setting range is set in the spline file.
   - Measures: Change the setting for the path point No. "nnnn" indicated in the error message so it is within the range (-1 to 32767).

- L2615: Output signal exceeds range (nnnn)
   - Cause: The head No. for the signal output in the spline file exceeds the range.
   - Measures: Change the head No. of the signal output for the path point No. "nnnn" indicated in the error message so it is within the range (-1 to 32767).

- L2615: Tolerance setting value exceeds range (nnnn)
   - Cause: The tolerance designation in the spline file exceeds the range.
   - Measures: Change the tolerance designation for the path point No. "nnnn" indicated in the error message so it is within the range (0 to 100).

- L2615: Interpolation setting information exceeds range
   - Cause: Data with value exceeding the range was found in the spline file interpolation setting information.
   - Measures: Open the target spline file and check the contents. Then, save the file again to recreate the spline file. Write the new file to the controller again.

- L2615: Header information exceeds range
   - Cause: Data with value exceeding the range was found in the spline file header information.
   - Measures: Open the target spline file and check the contents. Then, save the file again to recreate the spline file. Write the new file to the controller again.

- L2621: Tracking function is enabled.
   - Cause: The tracking function and Ex-T control function cannot be enabled simultaneously.
   - Measures: When using the Ex-T control function, disable the tracking function.

- L2622: Singular point passage function is enabled.
   - Cause: The singular point passage function and Ex-T control function cannot be enabled simultaneously.
   - Measures: When using the Ex-T control function, disable the singular point passage function.

- H264n: Add axis FLS signal is input
   - Cause: Add axis FLS signal is input
   - Measures: Release the brake and move the arm by hand. Or, referring to "Operation to Temporarily Reset an Error that Cannot Be Canceled" in the separate volume "Instruction Manual/Detailed Explanation of Functions and Operations", reset the error and move the axis "n" within the operation range using JOG operation.
   - Code note: (n indicates the axis number (1 to 8))

- H265n: Add axis RLS signal is input
   - Cause: Add axis RLS signal is input
   - Measures: Release the brake and move the arm by hand. Or, referring to "Operation to Temporarily Reset an Error that Cannot Be Canceled" in the separate volume "Instruction Manual/Detailed Explanation of Functions and Operations", reset the error and move the axis "n" within the operation range using JOG operation.
   - Code note: (n indicates the axis number (1 to 8))

- L2660: Error concerning axis coop
   - Cause: The error concerning the axis coop occurred
   - Measures: Please Confirm the content by a detail number of the error

- L2660: Illegal robot No.(axis trk)
   - Cause: The designated robot No. is illegal
   - Measures: Set the correct robot No.

- L2660: Illegal axis No.(axis trk)
   - Cause: The designated axis No. is illegal
   - Measures: Set the correct axis No.

- L2660: Illegal unit system (axis trk)
   - Cause: A rotary axis is designated
   - Measures: Please designate a linear drive axis

- L2660: Origin unsetting (axis trk)
   - Cause: Axis tracking cannot be executed because of origin unsetting
   - Measures: Please set the origin and turn the power OFF and ON

- L2660: Illegal robot No.(base coop)
   - Cause: The designated robot No. is illegal
   - Measures: Set the correct robot No.

- L2660: Illegal axis No.(base coop)
   - Cause: The designated axis No. is illegal
   - Measures: Set the correct axis No.

- L2660: Illegal unit system (base coop)
   - Cause: A rotary axis is designated
   - Measures: Please designate a linear drive axis

- L2660: Origin unsetting (base coop)
   - Cause: Base coop cannot be executed because of origin unsetting
   - Measures: Please set the origin and turn the power OFF and ON

- L2661: Error concerning axis coop (combi.)
   - Cause: The function cannot be used at the same time with the axis coop
   - Measures: Please confirm the content by a detailed number of the error

- L2661: Cannot be used (axis trk)
   - Cause: A synchronous addition axis control is effective
   - Measures: Invalidate a synchronous addition axis control

- L2661: Cannot be used (base coop)
   - Cause: Tracking function is effective
   - Measures: Invalidate the tracking function

- L2661: Cannot be used (base coop)
   - Cause: A synchronous addition axis control is effective
   - Measures: Invalidate a synchronous addition axis control

- L2661: Jrc cannot be executed
   - Cause: The base coop is executing
   - Measures: Please do not use Jrc command

- L2661: Interpolation cannot be executed
   - Cause: An addition axis is going to move
   - Measures: Please do not move an addition axis

- L2661: Cannot be used (base coop)
   - Cause: Interference avoidance function is effective
   - Measures: Invalidate the interference avoidance function

- L2662: Work setting cannot be changed
   - Cause: The additional axis tracking is executing
   - Measures: Please change after the work coop is invalid

- L2662: This work cannot use
   - Cause: The setting is not done(axis trk)
   - Measures: Please confirm a set valuse and then execute

- H2663: Origin data was changed
   - Cause: Origin data was changed about the base cooperation target axis
   - Measures: Turn the power OFF and ON once

- L2700: Cmp error (different mode)
   - Cause: The designated mode is different from the current mode.
   - Measures: Execute Cmp Off and then designate.

- C2710: Cmp error (displacement)
   - Cause: The displacement magnitude of the compliance operation exceeded the specified value.
   - Measures: Correct the program, position or other item so that the displacement magnitude can be reduced.

- H2720: Cmp error (joint angle)
   - Cause: Cmp Command exceeds the limit of a joint angle.
   - Measures: Adjust the position data or reduce the displacement.

- C272n: Cmp error (Jn joint angle)
   - Cause: Cmp Command exceeds the limit of joint angle of joint "n" axis
   - Measures: Change the position data or reduce displacement.
   - Code note: (n indicates the axis number (1 to 8).)

- C273n: Cmp error (Jn axis speed)
   - Cause: Cmp Command exceeds the limit of speed of joint "n" axis.
   - Measures: Change the position data or slow down.
   - Code note: (n indicates the axis number (1 to 8).)

- C2740: Cmp error (coordinates conv.)
   - Cause: An error was detected in the coordinates conversion of Cmp command.
   - Measures: Adjust the position data.

- L2750: Cannot execute while tracking
   - Cause: Cannot execute while tracking
   - Measures: Execute Trk Off and then execute

- L2750: Cannot invalidate F.Ctrl (Trk)
   - Cause: Force control cannot be invalidated while executing tracking
   - Measures: Please invalidate the tracking function

- L2750: Unable to disable force sense control. (Tracking being performed)
   - Cause: Unable to disable force sense control while tracking function being executed.
   - Measures: Try again after disabling the tracking function.

- H2760: The force sense control offset limit was reached.
   - Cause: The robot attempted to move beyond the force sense control offset limit.
   - Measures: Check whether there is a problem with robot movement while force sense control is enabled. (The offset limit is the value set in parameter FSCORMX.)

- H2770: Outside offset position movement range (\*\*) Note) "\*\*" is substituted with "±Jn" (n is axis No.)
   - Cause: The position after force sense control offset lies outside the range. The robot may have been moved near the movement range limit.
   - Measures: Review the movement position or force sense control settings, and ensure that the offset position does not exceed the movement range.

- H2780: Offset position speed over (\*\*) Note) "\*\*" is substituted with "Jn" (n is axis No.)
   - Cause: The speed of movement to the position after offsetting with force sense control exceeded the speed limit. The movement speed may be too fast, or the robot may have been moved at the singular point adjacent.
   - Measures: Review the movement speed and movement position, or the force sense control settings.

- L2800: Illegal position data
   - Cause: This may occur for a position to which the robot cannot reach.
   - Measures: Adjust the position.

- L2801: Illegal position data (start)
   - Cause: This may occur for a starting position to which the robot cannot reach.
   - Measures: Adjust the position.

- L2802: Illegal position data (dstn)
   - Cause: This may occur for a ending position to which the robot cannot reach.
   - Measures: Adjust the position.

- L2803: Illegal assisting position data (intmed)
   - Cause: The intermediate path at the straight line interpolation and the route at circle interpolation are the position which the robot cannot move.
   - Measures: Confirm the error occurrence line and confirm that there is no position which the robot cannot move. And please correct the data of starting position, midway position, or ending position .

- L2810: Posture flag is disagree
   - Cause: The structure flag of the start point and end point don't match.
   - Measures: Adjust the position data.

- H2820: Illegal Accel ratio
   - Cause: This occurs when the acceleration/deceleration ratio is too small.
   - Measures: Adjust the acceleration/deceleration ratio to a larger value.

- H2830: System error (ipol posture type)
   - Cause: The Type argument of the Mov instruction was set to -1 or a similar value.
   - Measures: Change the Type argument of the Mov instruction to a correct value (0, 1, etc).

- H2840: System error (ipol parameters)
   - Cause: The parameter may have been damaged.
   - Measures: If it comes back, contact your service provider.

- H2850: System error (ipol norm)
   - Cause: The norm is illegal. A problem occurred in internal computation processing.
   - Measures: If it comes back, contact your service provider.

- H2860: System error (ipol type)
   - Cause: An illegal interpolation process method is being used. A problem occurred in internal computation process- ing.
   - Measures: If it comes back, contact your service provider.

- H2870: System error (ipol data undef)
   - Cause: The interpolation position data has not been defined. A problem occurred in internal computation process- ing.
   - Measures: If it comes back, contact your service provider.

- H2880: System error (ipol data area)
   - Cause: The memory is insufficient for the inside operation.
   - Measures: If it comes back, contact your service provider.
   - Reset: Power off and on, unless otherwise noted.

- H2890: System error (undefined err)
   - Cause: An undefined error number was generated in internal computation processing.
   - Measures: If it comes back, contact your service provider.

- L2900: System ERROR M00 to M04
   - Cause: An error occurred in the internal processing.
   - Measures: If it comes back, contact your service provider.

- L3100: Processor Call Stack over
   - Cause: Too many function calls or local variables.
   - Measures: Correct the program and re-execute.

- L3100: System ERROR (Call Stack)
   - Cause: Processor Call Stack defect
   - Measures: Possible problem with execution sequence of Function command.

- L3110: Arg. value range over
   - Cause: Arg. value is outside range. For the PtoJ function, a position which the robot cannot reach or a singular point was set as the arg. value.
   - Measures: Please confirm the range of the argument and set a correct value

- L3110: The force sense control command argument lies outside the range.
   - Cause: A value outside the range was set for the force sense control command argument.
   - Measures: Check the argument range and set a correct value.

- L3110: The force sense control status variable argument lies outside the range.
   - Cause: A value outside the range was set for the force sense control status variable argument.
   - Measures: Check the argument range and set a correct value.

- L3110: The force sense control related argument lies outside the range.
   - Cause: A value outside the range was set for the force sense control related argument.
   - Measures: Check the argument range and set a correct value.

- L3110: The Mo trigger No. lies outside the range.
   - Cause: A value outside the range was set for the Mo trigger No.
   - Measures: Check the setting range and set a correct value.

- L3110: Def MoTrg command argument error
   - Cause: An unusable variable or different mechanical No. was set.
   - Measures: Set a usable variable or same mechanical No.

- L3110: Arg. value range over (MVA2)
   - Cause: Argument value range is over (MVA2)
   - Measures: Check the argument range and reinput

- L3120: No. of arg. is over
   - Cause: No. of argument is over
   - Measures: Please confirm the range of the argument and set a correct value

- L3130: COM file is already opened
   - Cause: Opening of a file already opened was attempted.
   - Measures: Check the file No. and re-execute.

- L3140: Can't open COM file
   - Cause: The file cannot be opened.
   - Measures: Check the file No. and re-execute.

- L3141: The NVOpen command is not executed.
   - Cause: No NVOpen command was executed before execution of a command communicating with the vision sensor.
   - Measures: Revise the robot program to execute the NVOpen command.

- L3142: The communication line can not be opened.
   - Cause: The line for communication with the vision sensor can not be opened.
   - Measures: Check the communication cable or the communications parameters.

- L3150: Cannot Print (INPUT mode)
   - Cause: The file open mode is INPUT, so writing is not possible.
   - Measures: Check the file No. and open mode, and re-execute.

- L3170: Cannot Input (OUTPUT mode)
   - Cause: The file open mode is OUTPUT, so writing is not possible.
   - Measures: Check the file No. and open mode, and re-execute.

- L3180: System error (array range over)
   - Cause: System error (array range over)
   - Measures: If it comes back, contact your service provider

- L3200: This file is read only
   - Cause: The file cannot be read.
   - Measures: Check the contents of the file.

- L3210: This variable is write protected
   - Cause: Writing of this variable is prohibited.
   - Measures: Check the variable protection setting.

- L3220: Nesting over
   - Cause: A nest-over error occurred in If of the If instruction or For of the For instruction.
   - Measures: Correct the program and re-execute.

- L3230: For Next statements unmatch
   - Cause: The No. of For and Next statements do not match.
   - Measures: Correct the program and re-execute.

- L3240: Nesting over (For, While)
   - Cause: Nesting over (For, While)
   - Measures: Correct the program and re-execute.

- L3250: While WEnd statements unmatch
   - Cause: The No. of While and WEnd statements do not match.
   - Measures: Correct the program and re-execute.

- L3251: Number of jump destination exceeds 32
   - Cause: The number of branches defined exceeded 32.
   - Measures: Correct the program and re-execute.

- L3252: If EndIf statements unmatch
   - Cause: If EndIf statements unmatch.
   - Measures: Correct the program and re-execute.

- L3253: Nesting over (If, EndIf)
   - Cause: Nesting over (If, EndIf)
   - Measures: Correct the program and re-execute.

- L3254: Select - End Select statements unmatch
   - Cause: Select - End Select statements unmatch.
   - Measures: Correct the program and re-execute.

- L3255: If Else statements unmatch
   - Cause: If Else statements unmatch.
   - Measures: Correct the program and re-execute.

- L3256: Function definition error
   - Cause: The error concerning the function definition occurred.
   - Measures: Please confirm the content by a detailed number of the error.

- L3256: Function definitions exceeds max.
   - Cause: The number of function definitions exceeds 256.
   - Measures: Correct the program and re-execute.

- L3256: Function statement is incorrect
   - Cause: Function statement position is incorrect.
   - Measures: Correct the program and re-execute.

- L3256: Function FEnd statemets unmatch
   - Cause: Function FEnd statemets unmatch.
   - Measures: Correct the program and re-execute.

- L3256: Undefined function
   - Cause: The called function or the assigned function is undefined.
   - Measures: Please confirm the function procedure name.

- L3256: Function Main called
   - Cause: Function Main called.
   - Measures: Function Main cannot be called.

- L3256: Function Main does not exist
   - Cause: Programs that there is no Function Main cannot be executed.
   - Measures: Please add Function Main (parameterless) to the program.

- L3256: Local var. in function was used
   - Cause: A local variable in the function procedure was used.
   - Measures: Local variables in a function procedure cannot be used.

- L3257: Include declaration error
   - Cause: The error concerning the include declaration occurred.
   - Measures: Please confirm the content by a detailed number of the error.

- L3257: Include declaration exceeds max.
   - Cause: The number of include declarations exceeds 64.
   - Measures: Correct the program and re-execute.

- L3257: Include statement is incorrect
   - Cause: Include statement position is incorrect.
   - Measures: Correct the program and re-execute.

- L3258: Preprocessor error
   - Cause: The error concerning the preprocessor occurred.
   - Measures: Please confirm the content by a detailed number of the error.

- L3258: Preprocessor run line exceeds max.
   - Cause: Preprocessor execution line exceeds 1000 lines.
   - Measures: Correct the program and re-execute.

- L3258: Preprocessor run error
   - Cause: Commands that can not execute by the preprocessor are included.
   - Measures: Correct the program and re-execute.

- L3260: Cannot exec for all slots
   - Cause: Execution with all slots designated is not possible.
   - Measures: Designate an individual slot and try again.

- L3270: The command size is exceeded
   - Cause: The command size is exceeded.
   - Measures: Specify within single-byte 256 characters.

- L3280: Cannot execute without GetM
   - Cause: The command you attempted to execute cannot be executed without GetM. Or, a non-existing mechanical number was specified.
   - Measures: Execute it after executing the RelM and GetM commands in another task slot.

- L3281: Cannot execute during RUN
   - Cause: Cannot execute during operation.
   - Measures: Cannot execute during operation.

- L3282: "Can't RUN (not select, attribute)"
   - Cause: The program is not selected or the attribute is illegal.
   - Measures: Load the program into the specified task slot. Or, change the program attributes.

- L3285: Cannot execute (RUN or WAI)
   - Cause: can't execute in the state of stopping or executing.
   - Measures: Reset the program (cancel the abort status).

- L3286: Program is empty
   - Cause: Execution of an empty program was attempted.
   - Measures: Make the program or select the correct program.

- L3287: Cannot execute (ERROR ALWAYS)
   - Cause: This command cannot be used when the start conditions are ERROR and ALWAYS.
   - Measures: Correct the program.

- L3288: Cannot execute while editing
   - Cause: That program cannot be executed because it is being edited.
   - Measures: Finish editing the program first, and then start it.

- L3289: Program does not exist (SLT\*)
   - Cause: The program designated in the slot table does not exist.
   - Measures: Correct the slot parameter.

- L3290: System slot cannot be executed
   - Cause: The system slot cannot be executed.
   - Measures: Check whether another slot (user slot) is being operated.

- L3300: User slot cannot be executed
   - Cause: A user slot cannot be executed.
   - Measures: Check whether the system slot is being operated.

- L3310: Cannot execute XRun (Runing)
   - Cause: XRun is not possible as the designated slot is operating.
   - Measures: Stop the specification slot, and execute.

- L3320: Cannnot execute XRun (empty)
   - Cause: XRun is not possible as the program has not been selected.
   - Measures: Specify program name to the argument or execute XLoad.

- L3330: Cannnot execute XStp (empty)
   - Cause: XStp is not possible as the program has not been selected.
   - Measures: correct the program, and execute.

- L3340: Cannnot execute XRst (empty)
   - Cause: XRst is not possible as the program has not been selected.
   - Measures: Resetting is possible in the state of waiting only.

- L3350: Cannnot execute XRst (Running)
   - Cause: Xrst is not possible as the program is executing.
   - Measures: Stop execution, and do it.

- L3360: Cannnot execute XLoad (not PSA)
   - Cause: XLoad cannot be executed when the program cannot be selected.
   - Measures: Execute XRst, and do it.

- L3361: Can not load the program (SLT\*)
   - Cause: A non-existing program was specified in the slot parameter (SLTn).
   - Measures: A non-existing program was specified in the slot parameter (SLTn).

- L3370: Cannnot execute XClr (empty)
   - Cause: XClr is not possible as the program has not been selected.
   - Measures: XClr can only be executed to enable program selection.

- L3380: Cannnot execute XClr (not PSA)
   - Cause: Program selection is not enabled.
   - Measures: Execute XClr after resetting the program (canceling the abort status).

- L3390: Cannot use arc pallet
   - Cause: Cannot use arc pallet.
   - Measures: Change to another method.

- L3391: Cannot use MVA2
   - Cause: Cannot use MVA2
   - Measures: Change to another method

- L3400: System error (PROC stack over)
   - Cause: System error. (Processor stack overflow)
   - Measures: If it comes back, contact your service provider

- L3500: Illegal format input (Input)
   - Cause: The type of the variable specified by Input and the type of the received data do not match.
   - Measures: Check the format.

- L3501: Illegal Receive data (EBREAD)
   - Cause: Type is different (receive data and specified variable)
   - Measures: Please confirm specified tag data of the vision.

- L3600: Jump destination does not exist
   - Cause: No jump destination was found for the Def Act, On Com and On GoTo commands.
   - Measures: Check the jump destination.

- L3601: Jump destination is incorrect
   - Cause: Jumping into program control block is prohibited.
   - Measures: Correct the program and re-execute.

- L3700: Undefined variable
   - Cause: It was attempted to reference a variable that has not been initialized.
   - Measures: Define a variable, enter an initial value, and then use it.

- L3710: Nesting over (CallP)
   - Cause: Program Call is used more than the limitation.
   - Measures: Reduce the call count of CallP (nesting).

- L3720: RC NX statements unmatch
   - Cause: RC NX statements unmatch
   - Measures: Match the numbers of RC and NX.

- L3750: Illegal positions (Def Plt)
   - Cause: Multi rotation flag (FL2 J1/J4 axis) is different
   - Measures: Specify position to become the same multi rotation flag.

- L3760: Illegal positions (Def Plt)
   - Cause: J1 or J4 axis is greatly changed (Def Plt)
   - Measures: Specify positions to be not different greatly

- L3770: This is an undefined Mo trigger.
   - Cause: An attempt was made to use an undefined Mo trigger.
   - Measures: Define the specified Mo trigger before use.

- L3780: Cannot use the MELFA Smart Plus.
   - Cause: The MELFA Smart Plus card or MELFA Smart Plus card pack is not installed in the controller. When the MELFA Smart Plus card is installed, the setting of the parameter SMART+1 may be set incor- rectly.
   - Measures: Install the MELFA Smart Plus card or MELFA Smart Plus card pack. Set the parameter SMART+1 correctly.

- L3781: Cannot use the MELFA Smart Plus.
   - Cause: The MELFA Smart Plus card or MELFA Smart Plus card pack is not installed in the controller. When the MELFA Smart Plus card is installed, the setting of the parameter SMART+1 may be set incor- rectly.
   - Measures: Install the MELFA Smart Plus card or MELFA Smart Plus card pack. Set the parameter SMART+1 correctly.

- L3810: Different argument type
   - Cause: The type of an argument in an arithmetic operation, monadic operation, comparison operation or each func- tion is different.
   - Measures: Designate the correct argument.

- L3820: Undefined intermediate code
   - Cause: A program or system status variable may have been damaged.
   - Measures: Restore using the backup data. If the backup data is not available, it is necessary to create a program again.

- L3821: A compilation error occurred
   - Cause: There is an error in the input syntax.
   - Measures: Check the error number in the error message.

- L3830: Cannot execute GetM
   - Cause: GET of the mechanisms is not possible.
   - Measures: Check whether the designated mechanisms are being used with a different slot.

- L3840: Return without GoSub
   - Cause: Return was executed without using GoSub.
   - Measures: Check the program.

- L3850: Undefined PLT
   - Cause: The Def Plt command was not executed.
   - Measures: Use it after defining a pallet with the Def Plt command.

- L3860: Illegal position data defined
   - Cause: There is an error in the position data.
   - Measures: Check the position data definition.

- L3870: Illegal mecha No. (\*\*\*\*) Note) "\*\*\*\*" is substituted with the "robot status variable".
   - Cause: The designated mecha No. is an invalid value
   - Measures: Please set a correct mecha No.

- L3870: The force sense control status variable mechanical No. is an invalid value.
   - Cause: An invalid variable was set for the force sense control status variable mechanical No.
   - Measures: Set a correct mechanical No.

- L3870: The mechanical No. specified with the Def MoTrg command is an invalid value.
   - Cause: The mechanical No. specified with the Def MoTrg command is an invalid value.
   - Measures: Set a correct mechanical No.

- L3880: Illegal slot No.
   - Cause: The task slot number specified in the argument of the system status variables was invalid.
   - Measures: Input the correct slot No.

- L3890: System error (make MCODE)
   - Cause: An error has occurred when creating an operation instruction. The program may have been damaged.
   - Measures: Restore using the backup data. If the backup data is not available, it is necessary to create a program again.

- L3900: JRC Command is disable
   - Cause: The JRCEXE parameter is disabled, so it cannot be used.
   - Measures: Change the JRCEXE parameter, and then execute.

- L3910: Cannnot execute (JRC 0)
   - Cause: JRC 0 can not execute for robot arm axis.
   - Measures: Correctly set.

- L3930: This command cannot be executed
   - Cause: Collision detection is effective
   - Measures: Repeal collision detection (execute ColChk Off)

- L3940: ColChk cannot be executed
   - Cause: An exclusive function is performing with ColChk
   - Measures: Repeal the corresponding function

- L3950: NOERR cannot be executed
   - Cause: Interruption using M\_ColSts is invalid
   - Measures: Define interruption using M\_ColSts and confirm it

- L3960: This Act No. cannot be repealed
   - Cause: NOERR of collision detection is performed
   - Measures: Repeal this interruption after canceling NOERR

- L3970: ColChk cannot be performed
   - Cause: Collision detection serves as prohibition of use
   - Measures: Change parameter COL into use permission

- L3980: Load mode cannot be specified.
   - Cause: Prec command is executed
   - Measures: Repeal high accuracy mode (execute Prec Off)

- L3982: Cannot be used (singular point)
   - Cause: 1 This robot does not correspond to the singular point function 2 Cmp command is executed 3 A synchronous addition axis control is effective 4 Tracking mode is effective 5 Pre-fetch execution is effective 6 This robot is a setting of the multi mechanism 7 ColChk On command is executed
   - Measures: 1 Check the argument of Type specification 2 Invalidate a compliance mode (execute Cmp Off) 3 Invalidate a synchronous addition axis control 4 Invalidate a tracking mode (execute Trk Off) 5 Invalidate a pre-fetch execution 6 Do not use the function of passage singular point 7 Invalidate a collision detection (execute ColChk Off)

- H3983: Cannot execute Cnt movement
   - Cause: A structural flag or the angle of the joint is not corresponding
   - Measures: It surely positions it by Dly command etc

- L3984: Cannot be passsed (singularity)
   - Cause: Robot passes position which is very near the singular point
   - Measures: Adjust the teaching position

- H3985: Cannot be passsed (singularity)
   - Cause: Robot passes position which is very near the singular point
   - Measures: Adjust the teaching position

- L3986: Unable to enable force sense control. (Sensor)
   - Cause: Unable to execute because the force sensor is not connected.
   - Measures: Connect the force sensor, or delete the command that cause the error.

- L3986: Unable to enable force sense control. (Cmp command)
   - Cause: It is not possible to enable force sense control function while the compliance control function is enabled.
   - Measures: The force sense control function and compliance control function cannot be enabled simultaneously. If using the force sense control function, disable the compliance control function.

- L3986: Unable to execute the Cmp command. (Force sense control)
   - Cause: It is not possible to enable the compliance control function while the force sense control function is enabled.
   - Measures: The force sense control function and compliance control function cannot be enabled simultaneously. If using the compliance control function, disable the force sense control function.

- L3986: Unable to enable the collision detection function. (Force sense control)
   - Cause: It is not possible to enable the collision detection function while force sense control function is enabled.
   - Measures: The force sense control function and collision detection function cannot be enabled simultaneously. If using the collision detection function, disable the force sense control function.

- L3986: Unable to enable force sense control. (Initialization)
   - Cause: It is not possible to perform initialization when starting force sense control.
   - Measures: Check the parameter settings.

- L3986: This is the singular point adjacent area. (Force sense control)
   - Cause: It is not possible to move the singular point adjacent area while the force sense control function is enabled.
   - Measures: If moving the singular point adjacent area, disable the force sense control function.

- L3986: The force sense control status is different.
   - Cause: The force sense control enabled/disabled status when resuming program operation differs from that during program operation.
   - Measures: Set the force sense control enabled/disabled status to the correct status. (This occurs only once when resuming program operation.)

- L3986: This function cannot be used.
   - Cause: This model is not compatible with the executed force sense control function.
   - Measures: Do not use this force sense control function. Contact the maker for details on the latest compatibility status.

- L3986: Unable to change tool conversion data. (Force sense control)
   - Cause: It is not possible to change tool conversion data while the force sense control function is enabled.
   - Measures: If changing tool conversion data, disable the force sense control function.

- L3986: Unable to change base conversion data. (Force sense control)
   - Cause: It is not possible to change base conversion data while the force sense control function is enabled.
   - Measures: If changing base conversion data, disable the force sense control function.

- L3986: Unable to execute the Jrc command. (Force sense control)
   - Cause: It is not possible to execute the Jrc command while the force sense control function is enabled.
   - Measures: To execute the Jrc command, disable the force sense control function.

- L3986: Disable force sense control.
   - Cause: JOG operation cannot be performed on your model while the force sense control function is enabled.
   - Measures: Disable the force sense control function.

- L3986: Unable to perform offset cancel. (Force sense control)
   - Cause: It is not possible to perform offset cancel while the force sense control function is enabled.
   - Measures: If performing offset cancel, disable the force sense control function.

- L3987: Force sense control is disabled.
   - Cause: Force sense control is disabled, and so unable to execute the command.
   - Measures: Enable the force sense control function.

- L3987: Force sense control is enabled.
   - Cause: It is not possible to enable force sense control again while already enabled.
   - Measures: First disable the force sense control function, and then enable again.

- L3987: Unable to execute the FsGChg command.
   - Cause: It is not possible to execute the FsGChg command when force control gain change is not complete.
   - Measures: Review the program so that the FsGChg command is executed after force control gain change is complete.

- L3987: Unable to output log file.
   - Cause: FTP processing was not properly performed.
   - Measures: Check the FTP related parameter setting. Check the Ethernet cable connection. Check the FTP server settings at the computer.

- L3987: The specified log file does not exist.
   - Cause: The log file for the No. specified with the FsOutLog command does not exist.
   - Measures: Check whether the log file No. is incorrect.

- L3987: Unable to change the force sense status variable.
   - Cause: The status variable setting is currently being used by the force sense control function and so cannot be changed.
   - Measures: If changing the setting, disable the force sense control function.

- L3987: Force sense log commands executed simultaneously.
   - Cause: It is not possible to create/output another file while creating (FsLog Off command) or outputting (FsOutLog command) a force sense log file.
   - Measures: Process after force sense log file creation/output is complete.

- L3987: Unable to create log file.
   - Cause: Unable to create a force sense log file.
   - Measures: Check the amount of available record space in the robot controller.

- L3987: Unable to execute the Fsc ON command.
   - Cause: It is not possible to execute the Fsc On command while changing the force control gain.
   - Measures: Execute the Fsc On command after force control gain change is complete.

- L3987: Unable to perform log data related processing.
   - Cause: It is not possible to execute FsLog On command file while recording force sense control log data.
   - Measures: Execute the FsLog On command after log data recording is complete.

- L3987: Force sense control is enabled.
   - Cause: The parameter setting is currently being used by the force sense control function and so cannot be changed.
   - Measures: If changing the parameter, disable the force sense control function once.

- L3987: Unable to enable the Mo trigger.
   - Cause: The Mo trigger for the FsCtrg command executed first is enabled.
   - Measures: Execute after changing the control characteristics.

- L3987: Mo trigger timeout
   - Cause: The Mo trigger did not turn ON within the specified time.
   - Measures: Review the Mo trigger conditions and robot program.

- L3987: Unable to execute the FsCTrg command.
   - Cause: The control characteristics is currently being changed. The Mo trigger for the FsCTrg command executed first is enabled.
   - Measures: Execute after changing the control characteristics.

- L3987: Unable to specify the control characteristics change.
   - Cause: The control characteristics change has been set with another command.
   - Measures: Change the program so that the command is not executed at the same time as another command.

- H3988: Unable to create a position command.
   - Cause: It is not possible to convert linear position data to joint angle after offsetting with force control. The position after offsetting lies outside the movement range or is a singular point.
   - Measures: Review the movement and, settings so that adjacents outside the movement range and singular point adjacents are avoided.

- L3990: ColChk cannot be used
   - Cause: This robot is a setting of the multi mechanism
   - Measures: Change parameter COL into use prohibition

- L3991: Mainte.Forecast cannot be used
   - Cause: This robot is a setting of the multi mechanism
   - Measures: Invalidate the maintenance forecast

- L3995: Can't use SQ Direct and ALWENA
   - Cause: SQ Direct function (parameter: IQMEM valid setting) can't be used Xrun, Xload, Xstp, Xrst, Servo and Reset Error command with always running program (parameter: ALWENA=1).
   - Measures: Change ALWENA parameter to 0

- L4000: System error (time out)
   - Cause: There is a problem in the program's exclusive process.
   - Measures: If it comes back, contact your service provider.

- L4100: No. of registered file is full
   - Cause: The No. of program has been exceeded.
   - Measures: Delete any unnecessary programs.

- L4110: Memory area is full
   - Cause: The program and data have exceeded the capacity. Capacity of a program execution area is insufficient.
   - Measures: Delete any unnecessary programs or data.

- L4120: Too long program name
   - Cause: The program name is a maximum of 12 characters with 3 extension characters.
   - Measures: Set the program name to within 12 characters and 3 extension characters.

- L4130: Illegal program name
   - Cause: An illegal character was used in the program name.
   - Measures: Only numbers and alphabetic characters can be used.

- L4140: The program was not found
   - Cause: The designated program was not found.
   - Measures: Designate a different program, or create the designated program.

- L4150: Program is faulty
   - Cause: Power shutdown may have occurred during the write operation.
   - Measures: The file may have been damaged. Delete the file.

- L4160: Not a robot program
   - Cause: The designated program is not a robot program.
   - Measures: Designate a different program.

- L4170: The program is being edited
   - Cause: The program is being edited.
   - Measures: Close the program being edited.

- L4180: Program is running
   - Cause: The program is running.
   - Measures: Stop the program.

- L4190: The program is selected
   - Cause: The program is preparing to execute.
   - Measures: Reset the program. The user base program is selected. Or the program to be executed is used with a different slot.

- L4200: Cannot write to file
   - Cause: Write operation is prohibited, or the file capacity is insufficient.
   - Measures: 1. Enable file writing. 2. Delete unnecessary files.

- L4210: Too long statement
   - Cause: The command statement length is limited to 240 characters.
   - Measures: Delete the No. of characters in the command statement to within 240 characters.

- L4220: Syntax error
   - Cause: There is an error in the syntax of the input command statement.
   - Measures: Re-input in the correct syntax after checking the contents.

- L4230: The line No. does not exist
   - Cause: There is no specified line number.
   - Measures: Check the contents, and reinput the correct line No.

- L4240: The statement is write protected
   - Cause: The command statement is write protected.
   - Measures: Cancel the write protection.

- L4250: No more lines or variables
   - Cause: Reading of lines and variables exceeding the registered ones.
   - Measures: Check the programs.

- L4300: Too long variable name
   - Cause: The variable name length is limited to 16 characters
   - Measures: Shorten the variable name to within 16 characters

- L4300: Too long variable/function name
   - Cause: The variable/function name length is limited to 32 characters.
   - Measures: Shorten the variable/function name to within 32 characters.

- L4310: Illegal character is used
   - Cause: A character other than A to Z or 0 to 9 was used.
   - Measures: Use the character which can be used.

- L4320: The variable is write protected
   - Cause: The variable is write protected.
   - Measures: 1. Use a writable variable. 2. Cancel the write protection.

- L4330: The variable is read protected
   - Cause: The variable is read protected.
   - Measures: 1. Use a writable variable. 2. Cancel the write protection.

- L4340: The variable is not defined
   - Cause: The variable has not been defined.
   - Measures: Define the variable.

- L4341: The user external variable is not defined.
   - Cause: The user external variable has not been defined
   - Measures: Define the user external variable

- L4350: Duplicate definition (Val.)
   - Cause: Variables already defined cannot be redefined with the Dim or Def statements.
   - Measures: 1. Change the variable name and define. 2. Delete the defined variable.

- L4360: Same variable used (65535 times)
   - Cause: Example: 1 P1=P1+P2 references P1 twice and P2 once.
   - Measures: Change the program to reduce the No. of times the same variable is used.

- L4370: Error in the array element
   - Cause: 1. The array elements exceeds the define range. 2. The specified variable is not an array.
   - Measures: 1. Correct the number of array elements within one to the maximum elements. 2. Do not specify array elements.

- L4380: Cannot delete variables (used)
   - Cause: Variables used in a command statement cannot be deleted.
   - Measures: Delete the command statement using the variable.

- L4390: Variable type combination error
   - Cause: The type of the user-defined external variable is different.
   - Measures: Match the variable types.

- L4400: Program is faulty.
   - Cause: The content of the program is abnormal.
   - Measures: Delete the program.

- L4420: Line No exceeds 32767
   - Cause: The new line No. or line gap is large.
   - Measures: Do not use line No. exceeding 32767.

- L4430: Not found the string searched
   - Cause: The character string searched for was not found.
   - Measures: Check the program.

- L4440: Duplicate definition (label)
   - Cause: A label already defined cannot be redefined.
   - Measures: 1. Change the label name. 2. Delete the defined label line.

- L4460: Argument value range over
   - Cause: Argument value range over.
   - Measures: Confirm the argument range, and correct the value.

- L4470: Line No can't be used
   - Cause: Line No can't be used
   - Measures: Please use the label

- L4800: System error (System base prog)
   - Cause: The base program for the system could not be opened. Or, the system base program name was not speci- fied correctly in the parameter.
   - Measures: If it comes back, contact your service provider.

- L4810: The global variable defined by user is not available
   - Cause: The parameter "PRGUSR" is not correct.
   - Measures: To use user-defined external variables, it is necessary to set the name of the program describing only variable definitions in the PRGUSR parameter.

- L4811: The global variable redefined
   - Cause: A system global variable is defined in user global.
   - Measures: Correct the program.
   - Reset: Power off and on, unless otherwise noted.

- L4820: No editing program
   - Cause: The program was closed while editing. For example, the program being edited is closed when a key switch enable/disable operation is performed via the T/B during online editing on a PC.
   - Measures: Try editing the program again.

- L4900: System error (Prog Hndl)
   - Cause: The program name used by internal processing is not normal.
   - Measures: If it comes back, contact your service provider.

- L4910: Robot Language is mismatched
   - Cause: The parameter RLNG is mismatched. (2:MELFA-BASIC V, 3:MELFA-BASIC VI)
   - Measures: Re-set RLNG or select an other program.

- L4922: Unsupported program version
   - Cause: This program was created with the new version.
   - Measures: This program cannot be opened with this robot.

- L4920: There is no backup data in ROM
   - Cause: There is no backup data in ROM.
   - Measures: Please operate after backup.

- L4930: CavChk cannot be executed (\*)
   - Cause: Collision avoidance serves as prohibition of use
   - Measures: Change parameter CAV into use permission

- L4930: CavChk cannot be used
   - Cause: This robot does not correspond to collision avoidance
   - Measures: Delete CavChk command

- L4931: Cmp command cannot be executed
   - Cause: Collision avoidance is effective
   - Measures: Repeal collision avoidance (execute CavChk Off)

- L4932: Trk command cannot be executed
   - Cause: Collision avoidance is effective
   - Measures: Repeal collision avoidance (execute CavChk Off)

- L4933: CavChk cannot be executed
   - Cause: Cmp command is executed
   - Measures: Repeal compliance mode (execute Cnp Off)

- L4934: CavChk cannot be executed
   - Cause: Trk command is executed
   - Measures: Repeal tracking mode (execute Trk Off)

- L4935: NOERR cannot be executed
   - Cause: Interruption using M\_CavSts is invalid
   - Measures: Define interruption using M\_CavSts and confirm it

- L4936: This ACT No. cannot be repealed
   - Cause: NoErr of collision avoidance is performed
   - Measures: Repeal this interruption after canceling NoErr

- L4937: A user free area size is lack
   - Cause: The size of a user free area is smaller than that of 2K
   - Measures: Please set 2K or more to the size of a user free area
   - Reset: Power off and on, unless otherwise noted.

- L4938: This robot does not correspond to collision avoidance.
   - Cause: This robot does not correspond to collision avoidance.
   - Measures: Parameter CAV returns disapproval.

- L4939: CAV version of Robot No.n is old
   - Cause: Software version of Robot No.n is old
   - Measures: Interference avoidance function cannot be used because the software version of interference avoidance of the robot-n is old. Software version upgrade is required. Contact the maker.

- L4941: MvSpl command cannot be executed
   - Cause: Collision avoidance is effective
   - Measures: The spline interpolation function and collision avoidance function cannot be enabled simultaneously. If using the spline interpolation function, disable the collision avoidance function.

- L4950: Error in a function syntax
   - Cause: Error in the definition of a function procedure.
   - Measures: Correct the definition of the function procedure.

- L4950: Error in a parameter
   - Cause: Error in a parameter of the function.
   - Measures: Correct the parameter of the function.

- L4950: Error in a parameter
   - Cause: The elements exceeds the range.
   - Measures: Correct the array element.

- H5000: TB Enable key is ON
   - Cause: The TB Enable key was ON in the AUTO mode
   - Measures: OFF the TB Enable key, or enter the TEACH mode

- L5010: AUTOENA signal is OFF
   - Cause: The automatic operation possible signal is OFF.
   - Measures: Turn the automatic operation possible signal ON, or enter the teach mode.

- L5100: No program is selected
   - Cause: A program is not selected for the designated slot.
   - Measures: Select a program for the designated slot.

- L5110: Continuous RUN is not possible
   - Cause: A different program name has been designated.
   - Measures: Designate the correct program name.

- L5120: Cannot select program (not PSA)
   - Cause: The specified slot is not in the program selection state.
   - Measures: Reset the program.

- L5130: Cannot execute servo on
   - Cause: A servo OFF process is taking place.
   - Measures: Wait for the servo to turn OFF before turning the servo ON.

- L5140: Cannot read the file
   - Cause: Reading is being carried out, or editing is being carried out.
   - Measures: Close the file being edited, or read after the reading is completed.

- L5150: The origin has not been set
   - Cause: The origin has not been set.
   - Measures: Refer to the chapters of "Setting the origin" and "Resetting the origin" which are described in the sepa- rate volumes "ROBOT ARM SETUP &amp; MAINTENANCE". And set the origin.

- L5200: Parameter error (TASKMAX)
   - Cause: The TASKMAX parameter setting value has been exceeded (initial value: 8, maximum value: 32).
   - Measures: Reduce the number of multi tasks, or change the TASKMAX parameter.
   - Reset: Power off and on, unless otherwise noted.

- L5210: Parameter error (MECHAMAX)
   - Cause: The No. of multi mechanisms has exceeded the limit.
   - Measures: Reduce the No. of multi mechanisms.
   - Reset: Power off and on, unless otherwise noted.

- L5400: All robot cannot be designated
   - Cause: All mechanisms cannot be specified.
   - Measures: Specify an independent mechanism number.

- L5410: Non-existent mode
   - Cause: It has been changed to a mode other than Auto/Teach.
   - Measures: Contact the maker.

- L5420: Illegal slot No.
   - Cause: A task slot number other than 1 through TASKMAX (parameter) is specified.
   - Measures: Designate the correct task slot.

- L5430: Illegal robot No.
   - Cause: The mechanism designation is illegal.
   - Measures: Designate the correct mechanism.

- L5600: Cannot execute during an error
   - Cause: Cannot execute during an error.
   - Measures: Reset the error.

- C5610: Cannot execute during STOP ON
   - Cause: Cannot execute during stop signal input.
   - Measures: Turn the stop signal OFF and execute.

- L5620: Cannot execute during CSTOP ON
   - Cause: Cannot execute during cycle stop signal input.
   - Measures: Turn the cycle stop signal OFF.

- L5630: Cannot execute during SRVOFF ON
   - Cause: Cannot execute during servo OFF signal input.
   - Measures: Turn the servo OFF signal OFF.

- L5640: Cannot execute during RUN
   - Cause: Cannot execute during operation.
   - Measures: Stop the operation, and then execute.

- L5650: Cannot execute during STOP
   - Cause: Cannot execute during stop operation.
   - Measures: Complete the stop, and then execute.

- L5660: Edit during RUN (include ALWAYS)
   - Cause: Editing cannot be performed while in operation (including continuous execution).
   - Measures: Stop the program, and then execute.

- C5670: Cannot execute during wait
   - Cause: Cannot execute during wait
   - Measures: Execute Program Reset

- L5990: System error (Illegal command)
   - Cause: Not exist command was executed.
   - Measures: Execute the correct command.

- L6010: Illegal command
   - Cause: This may have occurred because data was sent before the communication line was opened via a data link, or an unregistered communication command was sent due to the mismatch between the versions of the controller and support software.
   - Measures: Send after the communication line is opened. Or, match the versions.

- L6011: Cannot connect to CR800
   - Cause: Cannot connect to CR800
   - Measures: Please connect to CPU unit

- L6020: The Operation is disable
   - Cause: The operation rights have not been acquired.
   - Measures: Acquire the operation rights.

- L6030: The editing operation is disable
   - Cause: The editing operation rights have not been acquired.
   - Measures: Acquire the editing operation rights.

- L6040: System error (illegal device No)
   - Cause: A non-registered device No. was set.
   - Measures: Set a valid device.

- C6050: The file cannot be opened
   - Cause: The block file cannot be opened.
   - Measures: Check the file, and designate the correct file.

- C6060: The mode is not TEACH
   - Cause: Carry out parameter writing in the teach mode.
   - Measures: Change to the teaching mode, and then execute.

- C6070: The time cannot be set
   - Cause: The time setting can only be executed when the program is stopped and the servo is OFF.
   - Measures: Stop the program and turn the servo OFF, and then set the time.

- C6080: Com mesg is too long
   - Cause: The character string of a communication text exceeded the maximum number allowed.
   - Measures: Verify the number of characters in a comment in various settings such as parameters.

- C6090: Can not power reset of robot
   - Cause: Can not power reset of robot
   - Measures: Can not power reset of robot

- C6500: Not opened COM line
   - Cause: Open was not executed by the program.
   - Measures: Execute Open, and then send PRN.

- H6530: COMDEV parameter is illegal
   - Cause: Illegal parameter (COMDEV).
   - Measures: Correct COMDEV parameter.
   - Reset: Power off and on, unless otherwise noted.

- L6600: Signal number is out of range.
   - Cause: The designated signal No. has not been defined.
   - Measures: Change the signal No. to the correct No.

- L6610: Cannot output (hand input)
   - Cause: The hand input signal cannot be written.
   - Measures: Use the correct output signal.

- L6620: Cannot write (special Input)
   - Cause: The input signal cannot be written into the robot dedicated area.
   - Measures: Use an actual signal.

- L6630: Input signal cannot be written
   - Cause: This is the actual signal input mode.
   - Measures: Set a pseudo-input signal.

- H6640: Illegal param (special signal)
   - Cause: The parameter setting is illegal.
   - Measures: Correct the changed parameter.
   - Reset: Power off and on, unless otherwise noted.

- H6640: Illegal setting of the dedicated signal parameter SFMODE
   - Cause: The setting of the dedicated signal parameter SFMODE (safety mode output) is illegal.
   - Measures: An error (output bit width of the output signal is less than 3 bits) exists in the dedicated output SFMODE setting. Correct the parameter setting.
   - Reset: Power off and on, unless otherwise noted.

- L6641: Duplicate setting (special IN)
   - Cause: The parameter setting is illegal.
   - Measures: Correct the parameter of the dedicated input signal you changed.

- H6642: STOP is fixed signal No. 0
   - Cause: The parameter setting is illegal.
   - Measures: Set the parameter STOP(input) to 0.
   - Reset: Power off and on, unless otherwise noted.

- L6643: Illegal parameter (special Sig.)
   - Cause: The parameter setting is illegal.
   - Measures: Make the ending number larger than the starting number.

- L6650: Duplicate setting (special OUT)
   - Cause: The parameter setting is illegal.
   - Measures: Correct the parameters.
   - Reset: Power off and on, unless otherwise noted.

- L6651: Duplicate setting (HANDTYPE)
   - Cause: The parameter setting is illegal.
   - Measures: Correct the parameter.
   - Reset: Power off and on, unless otherwise noted.

- L6660: Cannot output (SPECIAL OUT)
   - Cause: The program setting is illegal.
   - Measures: Correct the program.

- L6670: Illegal OUT reset pattern
   - Cause: The parameters are not defined in sets of 8 characters.
   - Measures: Correct the parameters.

- L6800: "Cancel pseudo-input mode, PW OFF"
   - Cause: If pseudo input was canceled, it is necessary to turn the power ON again in order to prevent the erroneous operation of the robot by external input signals.
   - Measures: Turn the power OFF and then ON once. It switches to an external input signal.
   - Reset: Power off and on, unless otherwise noted.

- C6900: Pseudo-input signal mode
   - Cause: Set with the parameters.
   - Measures: To set a real signal, reset the parameter and then turn the power ON again.

- C7000: Copy source file was not found
   - Cause: The copy source file was not found.
   - Measures: Input the correct file name.

- C7010: Delete target file was not found
   - Cause: The delete target file was not found.
   - Measures: Input the correct file name.

- C7020: Rename target file was not found
   - Cause: The rename target file was not found.
   - Measures: Input the correct file name.

- H7030: System error (param size over)
   - Cause: The change capacity is too large.
   - Measures: Contact the maker.
   - Reset: Power off and on, unless otherwise noted.

- C7040: Parameter changes prohibited
   - Cause: Changing this parameter is prohibited as it is a dangerous parameter.
   - Measures: Contact the maker.

- H7050: File is illegal
   - Cause: This file is damaged.
   - Measures: Contact to the dealer.

- H7060: System error (RAM area full)
   - Cause: The capacity has been exceeded.
   - Measures: Contact the maker.
   - Reset: Power off and on, unless otherwise noted.

- C7070: Memory area is full
   - Cause: The program and data have already exceeded the capacity.
   - Measures: Delete any unnecessary programs or data.

- L7071: Not enough memory area for CTN
   - Cause: Continue function needs more than 100Kbytes memory area.
   - Measures: Delete any unnecessary programs.

- C7080: Can not read parameter
   - Cause: Non-existent parameter or illegal password.
   - Measures: 1. Input the correct parameter name. 2. Input the correct password

- C7081: Can not write parameter
   - Cause: Non-existent parameter or illegal password.
   - Measures: 1. Input the correct parameter name. 2. Input the correct password

- C7081: Illegal CRC during parameter writing
   - Cause: The parameter CRC value is illegal.
   - Measures: An error is detected in the CRC check in the writing process of a parameter related to the safety monitoring function. Check the communication environment between the robot controller and the personal computer, and perform the writing operation again.

- C7090: Parameter comment illgal
   - Cause: The number of parameter comment characters exceeded the limit.
   - Measures: Operation will not be affected, so continue the operation.

- L7099: Can not read parameter"xxx". "xxx": Parameter name.
   - Cause: The parameter doesn't exist.
   - Measures: Please confirm the parameter name.

- H7300: Loading the parameter file.
   - Cause: It is necessary to turn the power ON again to reflect the parameters.
   - Measures: Turn the power OFF and ON once.
   - Reset: Power off and on, unless otherwise noted.

- C7310: Changed variables weren't saved
   - Cause: The power was OFF during program execution.
   - Measures: Don't turn the power OFF during program execution.

- L7311: The power was OFF during file saving
   - Cause: The power was OFF during file saving.
   - Measures: Don't turn the power OFF during file saving.

- L7332: Can't change to RAM mode in CTN mode
   - Cause: Can't change to RAM mode in continue mode.
   - Measures: Please change to RAM mode and perform again.

- L7340: Contine function cannot be used in DRAM mode
   - Cause: Contine function cannot be used in DRAM mode
   - Measures: Please change to RAM mode and perform again

- L7341: Can't change to DRAM mode in CTN mode
   - Cause: Can't change to DRAM mode in CTN mode
   - Measures: Please change to RAM mode and perform again

- L7342: Global extension cannot be used in DRAM mode
   - Cause: Global extension cannot be used in DRAM mode
   - Measures: Please change to RAM mode and perform again

- L7343: Can't change to DRAM mode in PRGGBL mode
   - Cause: Can't change to DRAM mode in PRGGBL mode
   - Measures: Please change to RAM mode and perform again

- L7370: The password is 8 to 32 char.
   - Cause: The password is 8 to 32 characters
   - Measures: Please input the password 8 to 32 characters

- L7371: Only alphanumeric character
   - Cause: It input it excluding the alphanumeric character
   - Measures: Please input the password in the alphanumeric character

- L7372: The password is unmatch
   - Cause: The password is unmatch
   - Measures: Please input the password again

- L7373: Password lock to the programs
   - Cause: Password lock to the programs
   - Measures: Please release password

- L7374: Password lock to the parameter
   - Cause: Password lock to the parameter
   - Measures: Please release password

- L7375: Password lock to the files
   - Cause: Password lock to the files
   - Measures: Please release password

- L7378: Change password
   - Cause: The password has not been changed from the initial value.
   - Measures: The password to change functional safety related parameters has not been changed from the initial value. Change the password to new one, and perform parameter setting. The factory default password is "MEL- FASafetyPSWD".

- C7450: No battery voltage (robot CPU)
   - Cause: The battery is spent.
   - Measures: The battery voltage of the robot CPU dropped to 2.5V or less. Replace the battery, and then restore the robot program and parameters. For more information about the replacement procedure, refer to the separate volumes, "Controller setup, basic operation, and maintenance".

- C7451: Battery voltage low (robot CPU)
   - Cause: The battery will be spent soon.
   - Measures: The battery voltage of the robot CPU dropped to 2.7V or less. Back up the robot program and the parameters, and replace the battery immediately. Otherwise, they may be lost due to low voltage. For more information about the replacement procedure, refer to the separate volumes, "Controller setup, basic operation, and maintenance".

- C7500: No battery voltage (robot)
   - Cause: The battery is spent.
   - Measures: Replace the battery and set the origin. For more information about the replacement procedure, refer to the separate volumes, "Instruction Manual/Robot Arm Setup to Maintenance".

- C7510: Battery voltage low (robot)
   - Cause: The battery will be spent soon.
   - Measures: Replace the battery. For more information about the replacement procedure, refer to the separate volumes, "Instruction Manual/Robot Arm Setup to Maintenance".

- C753n: Replenishment time of grease
   - Cause: It seems that grease reached longevity
   - Measures: Replenish grease
   - Code note: (n indicates the axis number (1 to 6).)

- C754n: Exchange time of the belt
   - Cause: It seems that the belt reached longevity
   - Measures: Execute the check and the exchange of the belt
   - Code note: (n indicates the axis number (1 to 6).)

- H7600: Mechanism number of additional axis is illegal.
   - Cause: The value of AXMENO (mechanism No. used) parameter is illegal.
   - Measures: On the mechanical additional axis, change "0" to a value which is smaller than set to AXUNUM (number of mechanisms used).
   - Reset: Power off and on, unless otherwise noted.

- H7601: Axis number of additional axis is illegal.
   - Cause: The value of AXJNO (setting axis No.) is illegal.
   - Measures: Change the value of this parameter from 1 to 3. Change the value of this parameter to a order from 1.
   - Reset: Power off and on, unless otherwise noted.

- H7602: Axis numbers of additional axis are overlap.
   - Cause: As the parameter value of AXJNO (setting axis No.), the same value is set at two or more elements.
   - Measures: Change the values of the element Nos. which set the same values at this pa-rameter AXMENO, to all different values.
   - Reset: Power off and on, unless otherwise noted.

- H7603: Unit of additional axis is illegal.
   - Cause: The value of AXUNT (unit axis) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to "0" or "1".
   - Reset: Power off and on, unless otherwise noted.

- H7604: Acceleration time of additional axis is illegal.
   - Cause: The value of AXACC (acceleration time) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive real numbers.
   - Reset: Power off and on, unless otherwise noted.

- H7605: Deceleration time of additional axis is illegal.
   - Cause: The value of AXDEC (deceleration time) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive real numbers.
   - Reset: Power off and on, unless otherwise noted.

- H7606: Gear ratio numerator of additional axis is illegal.
   - Cause: The value of AXGRTN (total speed ratio numerator) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive integers.
   - Reset: Power off and on, unless otherwise noted.

- H7607: Gear ratio denominator of additional axis is illegal.
   - Cause: The value of AXGRTD (total speed ratio denominator) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive integers.
   - Reset: Power off and on, unless otherwise noted.

- H7609: Motor rated speed of additional axis is illegal.
   - Cause: The value of AXMREV (rated speed) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive integers.
   - Reset: Power off and on, unless otherwise noted.

- H7610: Motor maximum speed of additional axis is illegal.
   - Cause: The value of AXJMX (maximum speed) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive integers.
   - Reset: Power off and on, unless otherwise noted.

- H7611: Encoder pulse of additional axis is illegal.
   - Cause: The value of AXENCR (encoder resolution) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to positive integers.
   - Reset: Power off and on, unless otherwise noted.

- H7612: JOG smoothening time constant of additional axis is illegal.
   - Cause: The value of AXJOGTS (JOG smoothening time constant) parameter is illegal.
   - Measures: Change the values of all elements of this parameter to 0 or positive real number.
   - Reset: Power off and on, unless otherwise noted.

- H7613: Turn OFF the power supply once, and turn ON it again.
   - Cause: It is necessary to turn OFF the power supply once.
   - Measures: Turn OFF the power supply of the controller, and turn it ON again.
   - Reset: Power off and on, unless otherwise noted.

- H7650: The force sensor quantity setting is incorrect.
   - Cause: Only 1 force sensor can be used for a single robot.
   - Measures: Check the parameter (AXJNO, AXMENO) settings to see whether multiple force sensors have been set.

- H7651: Force sense I/F unit initialization error
   - Cause: The force sense I/F unit was not recognized, and therefore it was not possible to successfully complete initialization.
   - Measures: Check the force sense I/F unit wiring and whether the power supply is ON.
   - Reset: Power off and on, unless otherwise noted.

- H7652: Force sense I/F unit revision illegal
   - Cause: This force sense I/F unit revision is not supported.
   - Measures: Contact the maker.
   - Reset: Power off and on, unless otherwise noted.

- H766n: The force sensor data exceeded the tolerance value.
   - Cause: The force acting on the force sensor exceeded the set tolerance value. sensor axis.)
   - Measures: Check whether too large a force is acting on the force sensor. Check whether an appropriate value has been set for parameter FSLMTMX. Refer to the section "Force Sensor Tolerance" in separate manual, "Instruction Manual/Force Sense Function" for details on the error recovery method.
   - Code note: (n indicates the

- H7700: CC-Link card is illegal (Error Code).
   - Cause: CC-Link card is illegal.
   - Measures: Please exchange the CC-Link card. When it comes back, contact to the dealer.
   - Reset: Power off and on, unless otherwise noted.

- H7710: Cannot set a CC-Link master station.
   - Cause: A master station is already set by the rotary switch.
   - Measures: Set the rotary switch to other than 0.
   - Reset: Power off and on, unless otherwise noted.

- H7720: Two CC-Link interface cards are mounted.
   - Cause: Mount one card in slot 2.
   - Measures: It is not allowed to install two cards. Install only one card.
   - Reset: Power off and on, unless otherwise noted.

- L7730: CC-Link data link error (local station connection error)
   - Cause: There is a line error or the master station's parameter settings are invalid.
   - Measures: Review the line and parameters.

- L7750: A (CC-Link) cable is not connected or parameters do not match.
   - Cause: A cable is not connected or parameters do not match.
   - Measures: Reset the power and start again.

- H7760: CC-Link initialization error
   - Cause: The master station's parameters do not match.
   - Measures: Correct the parameters, and then start again.
   - Reset: Power off and on, unless otherwise noted.

- L7780: A CC-Link register number is outside the range.
   - Cause: A register number entered is outside the allowable range.
   - Measures: Enter the correct value.

- L7781: A signal number for CC-Link was specified.
   - Cause: A signal number for CC-Link was specified.
   - Measures: Install a CC-Link interface card.

- H7810: MONPORT/SLMPPORT parameter error
   - Cause: The element of MONPORT(1/2) and SLMPPORT overlap.
   - Measures: Please set not to overlap to another port number.

- H7820: Mxt Scommand timeout
   - Cause: The setting time of the MXTTOUT parameter has been exceeded.
   - Measures: Make the MXTTOUT parameter value larger.

- H7830: "Ethernet card is not installed, or command disable"
   - Cause: Ethernet card is not installed, or command disable.
   - Measures: Install Ethernet interface card.

- H7840: Mxt command Illegal received data
   - Cause: The command argument and the data type do not match.
   - Measures: Check the command and the data you are sending.

- H7860: SLMP error
   - Cause: An error related to SLMP has occurred.
   - Measures: Please confirm the content by a detailed number of the error.

- H7861: SLMP parameter error
   - Cause: Illegal parameter.
   - Measures: Correct parameter.

- H7861: SLMP SLMPPORT parameter error
   - Cause: Overlap with server port number of NETPORT.
   - Measures: Please set not to overlap to another port number.

- H7861: SLMP SLMPPORT parameter error
   - Cause: Overlap with element of NETPORT(1) or MONPORT(1/2).
   - Measures: Please set not to overlap to another port number.

- L8300: There are a lot of GETPOS define
   - Cause: GETPOS function are up to 8.
   - Measures: Please use the same Act No. or reset an unnecessary program.

- L8310: GETPOS undefined
   - Cause: GETPOS undefined.
   - Measures: Please define GETPOS.

- H8320: System Error (GETPOS)
   - Cause: Internal data of GETPOS is illegal.
   - Measures: Turn the power OFF and ON once.

- H8400: CTN data is illegal (Prec)
   - Cause: CTN data is illegal. It changed to Prec Off.
   - Measures: Try to set be a Prec mode again.

- L8600: The vision is a unconnection
   - Cause: The vision is a unconnection
   - Measures: Please check Com No. and Parameter

- L8601: The vision can't be logged on
   - Cause: The parameter of the user-name or the password is abnormal
   - Measures: Please set the parameter correctly

- L8602: Password is abnormal
   - Cause: The password of the user-name is not corresponding
   - Measures: Please set the password correctly

- L8603: Parameter is abnormal
   - Cause: The parameter of the user-name or the password is abnormal
   - Measures: Please set the parameter correctly

- L8610: The communication is abnormal
   - Cause: The communication was cut
   - Measures: Please check the communications cable

- L8620: The vision number is abnormal
   - Cause: The number is not designated by NVOpen
   - Measures: Please check NVOpen command

- L8621: Vision program name is abnormal
   - Cause: The vision program name has exceeded 15 characters
   - Measures: Please make the program name within 15 characters

- L8622: There is no vision program
   - Cause: There is no specified vision program
   - Measures: Please check the vision program name

- L8623: SKIP number is already used
   - Cause: SKIP number is already used
   - Measures: Please confirm the SKIP number

- L8630: The recognition cell is illegal
   - Cause: There is no value on the cell
   - Measures: Please check the recognition cell

- L8631: The cell is outside the range
   - Cause: The range of the cell is exceeded
   - Measures: Please check the range of the cell

- L8632: The vision is a time-out
   - Cause: There is no response from the vision
   - Measures: Please check the time-out time

- L8633: The vision is a time-out (NVTRG)
   - Cause: There is no response from the vision (NVTRG)
   - Measures: Please decrease the load of the network

- L8634: There is a comma within the range of the cell
   - Cause: There is a comma within the range of the cell
   - Measures: Please check the range of the cell

- L8635: There is no comma within the range of the cell
   - Cause: There is no comma within the range of the cell
   - Measures: Please check the range of the cell

- L8636: Vision Tag name is abnormal
   - Cause: There is no specified vision tag in the vision program
   - Measures: Please correct the vision tag name

- L8640: The image trigger is abnormal
   - Cause: The trigger setting of the vision sensor is abnormal
   - Measures: Please check the trigger setting

- L8650: Please make the vision online
   - Cause: The vision sensor is off-line
   - Measures: Please make vision sensor online

- L8660: There is no authority
   - Cause: The authority of the user-name is not a full access
   - Measures: Please check user name

- L8670: Can not restart
   - Cause: It started without resetting it
   - Measures: Please reset the program

- L8680: Unknown code was received (code no.)
   - Cause: Status code of unregistration was received from vision sensor. (Status cord which is not registered with robot controller was received.)
   - Measures: The status code received from vision sensor is displayed to the code no.. When the code no. is not the numerical value, there is a possibility which received abnormal data. Please check the communication state, the communication setting, and the command argument.

- L8700: Multi CPU system self-check error
   - Cause: Illegal parameter of multi CPU system and CPU is abnormal,etc.
   - Measures: Confirm details and measures of the error with GX Developer etc.
   - Code note: (CR800-R/ CR800-Q only)

- L8710: PC I/O module parameter(m) error. (m): Target parameter numbers 1-4. (correspond to QXYUNIT 1-4)
   - Cause: An illegal setting of the PC I/O module was detected.
   - Measures: Please check the value of parameter.

- L8720: Fuse is blown.(PC I/O module (m)). (m): Target parameter numbers 1-4. (correspond to QXYUNIT 1-4)
   - Cause: Fuse is blown.(PC I/O module)
   - Measures: Please refer to the manual of PC I/O module.

- L8730: I/O module (m) remove error. (m): Target parameter numbers 1-4. (correspond to QXYUNIT 1-4)
   - Cause: PC I/O module was removed.
   - Measures: Please mount PC I/O module.

- H8740: CR800 init communication error
   - Cause: Initial communication with the CR800 was not possible
   - Measures: Check the communication cable connection

- H8741: CPU unit init communication error
   - Cause: Initial communication with the CPU unit was not possible
   - Measures: Check the communication cable connection

- H8751: CR800 communication error
   - Cause: Communication from the CR800 was cut off
   - Measures: Check the communication cable connection

- H8752: CPU unit communication error
   - Cause: Communication from the CPU unit was cut off
   - Measures: Check the communication cable connection

- H8760: PLC CPU parameters are not set
   - Cause: Fixed Scan Communication Area is not set in the PLC CPU
   - Measures: Please set the Fixed Scan Communication Area in the PLC CPU

- H8761: CPU number setting is illegal
   - Cause: The setting of CPU number is different
   - Measures: Please check PLC's multi CPU setting and QMLTCPUN parameter

- H8762: Multi CPU parameter setting is illegal
   - Cause: The multi CPU parameter setting is incorrect.
   - Measures: Synchronization processing in the robot CPU was not properly performed due to the incorrect multi CPU parameter setting. Please check and correct the value of the multi CPU parameter setting, and then turn the power OFF and ON once.
   - Reset: Power off and on, unless otherwise noted.

- H8770: CR800 communication error
   - Cause: Communication error was detected from CR800
   - Measures: Check the communication cable connection

- H8780: CPU unit communication error
   - Cause: Communication error was detected from the CPU unit
   - Measures: Check the communication cable connection

- H8790: CR800 error reset error
   - Cause: Can not reset CR800 error
   - Measures: Check the communication cable connection

- H8800: ASIC communication error
   - Cause: A failure was detected in the communication within the controller. The internal temperature of the controller may be high. For the CR800-R/CR800-Q controller, the following conditions may also be the cause. The power on timing may be incorrect. The cable between the controller and the robot CPU may have a fault.
   - Measures: Confirm that the controller filter has no clogging and the ambient temperature is 40 or less. If the error persists after taking above measures, contact the manufacturer. For the CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU. Check the cable connection between the robot CPU and the controller.
   - Reset: Power off and on, unless otherwise noted.

- H8800: ASIC CRC error
   - Cause: A failure was detected in the communication within the controller. The internal temperature of the controller may be high. For the CR800-R/CR800-Q controller, the following conditions may also be the cause. The power on timing may be incorrect. The cable between the controller and the robot CPU may have a fault.
   - Measures: Confirm that the controller filter has no clogging and the ambient temperature is 40 or less. If the error persists after taking above measures, contact the manufacturer. For the CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU. Check the cable connection between the robot CPU and the controller.
   - Reset: Power off and on, unless otherwise noted.

- H8800: ASIC short frame error
   - Cause: A failure was detected in the communication within the controller. The internal temperature of the controller may be high. For the CR800-R/CR800-Q controller, the following conditions may also be the cause. The power on timing may be incorrect. The cable between the controller and the robot CPU may have a fault.
   - Measures: Confirm that the controller filter has no clogging and the ambient temperature is 40 or less. If the error persists after taking above measures, contact the manufacturer. For the CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU. Check the cable connection between the robot CPU and the controller.
   - Reset: Power off and on, unless otherwise noted.

- H8800: ASIC overrunning error
   - Cause: A failure was detected in the communication within the controller. The internal temperature of the controller may be high. For the CR800-R/CR800-Q controller, the following conditions may also be the cause. The power on timing may be incorrect. The cable between the controller and the robot CPU may have a fault.
   - Measures: Confirm that the controller filter has no clogging and the ambient temperature is 40 or less. If the error persists after taking above measures, contact the manufacturer. For the CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU. Check the cable connection between the robot CPU and the controller.
   - Reset: Power off and on, unless otherwise noted.

- H8800: ASIC connector connection error
   - Cause: A failure was detected in the communication within the controller. The internal temperature of the controller may be high. For the CR800-R/CR800-Q controller, the following conditions may also be the cause. The power on timing may be incorrect. The cable between the controller and the robot CPU may have a fault.
   - Measures: Confirm that the controller filter has no clogging and the ambient temperature is 40 or less. If the error persists after taking above measures, contact the manufacturer. For the CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU. Check the cable connection between the robot CPU and the controller.
   - Reset: Power off and on, unless otherwise noted.

- H8800: ASIC 0byte transmission
   - Cause: A failure was detected in the communication within the controller. The internal temperature of the controller may be high. For the CR800-R/CR800-Q controller, the following conditions may also be the cause. The power on timing may be incorrect. The cable between the controller and the robot CPU may have a fault.
   - Measures: Confirm that the controller filter has no clogging and the ambient temperature is 40 or less. If the error persists after taking above measures, contact the manufacturer. For the CR800-R/CR800-Q controller, turn on the switch of the controller, then of the robot CPU. Check the cable connection between the robot CPU and the controller.
   - Reset: Power off and on, unless otherwise noted.

- H8810: Communication error
   - Cause: Error occurred by the data transmission to the servo
   - Measures: Check the communication cable connection. Check that there is not a source of noise.

- H8810: Illegal ID
   - Cause: ID of the com. data with the servo is not corresponding
   - Measures: Turn the power OFF and ON once

- H8820: Communication timeout
   - Cause: The communication with the servo became a timeout
   - Measures: Turn the power OFF and ON once

- H8830: Amplifier unconnection
   - Cause: The servo amplifier cannot be detected
   - Measures: Check the cable connection and the power supply of amp.

- H8830: Servo axis setting error
   - Cause: The axis setting is not correct
   - Measures: Check the servo axis settings (parameters, rotary switches)

- H8830: Uncorrespondence amplifier
   - Cause: Uncorrespondence amplifier is connected
   - Measures: Remove the uncorrespondence amplifier

- H8830: Uncorrespondence protocol
   - Cause: The communication protocol which does not correspond
   - Measures: Remove the uncorrespondence amplifier

- H8830: Illegal motor ID
   - Cause: Uncorrespondence motor is connected
   - Measures: Remove the uncorrespondence motor

- H8830: Parameter setting failure
   - Cause: A set value of the servo parameter is illegal
   - Measures: Change to a correct value

- H8830: Illegal amp. number
   - Cause: The number of connected amplifiers is not suitable
   - Measures: Check the number of connected amplifiers

- H887n: Transient processing error
   - Cause: Illegal demand data was transmitted
   - Measures: Turn the power OFF and ON once. If it comes back, contact to your service provider.

- H887n: Transient processing timeout
   - Cause: There is no response to the demand from servo
   - Measures: Turn the power OFF and ON once. If it comes back, contact to your service provider.

- H8920: Sensor I/F unit error (\*\*) Note) "\*\*" is substituted with the "sensor I/F unit error No." (2 hexadecimal digits)
   - Cause: An error occurred at the force sensor interface unit.
   - Measures: See Page 73, "Table 2-1: Force sense interface unit errors" based on the error No. in the Err. Message.

- C8921: Sensor I/F unit warning (\*\*) Note) "\*\*" is substituted with the "sensor I/F unit warning No." (2 hexadecimal digits)
   - Cause: A warning occurred at the force sensor interface unit.
   - Measures: See Page 73, "Table 2-1: Force sense interface unit errors" based on the error No. in the Err. Message.

- H9000 through H9099: User High level error
   - Cause: A high-level alarm was issued from the robot program.
   - Measures: Check the program.

- L9100 through L9199: User Low level error
   - Cause: A low-level alarm was issued from the robot program.
   - Measures: Check the program.

- C9200 through C9299: User Caution level error
   - Cause: A warning was issued from the robot program.
   - Measures: Check the program.

## Force sense interface unit errors

These are separate unit error numbers from Table 2-1; the manual does not assign them H/L/C severities. See H8920 and C8921 for the controller messages. For noise countermeasures, install a noise filter on peripheral equipment power supplies and/or add a ferrite core to the 24 V power output cable as appropriate; see the PDF for installation details.

- 12: memory error
   - Cause: Force sense interface unit internal part fault.
   - Measures: Replace the unit.

- 13: S/W processing error
   - Cause: Force sense interface unit internal part fault.
   - Measures: Replace the unit.

- 21: sensor initial communication error
   - Cause: The force sensor connection cable is disconnected or damaged, or noise contamination occurred.
   - Measures: Connect or replace the cable, or perform noise countermeasures.

- 25: sensor communication error
   - Cause: The force sensor connection cable is disconnected or damaged, or noise contamination occurred.
   - Measures: Connect or replace the cable, or perform noise countermeasures.

- 34: communication data error
   - Cause: The SSCNET III cable is disconnected, its end face is dirty, the cable is damaged, or noise contamination occurred.
   - Measures: Turn off the power before connecting the cable; wipe the end face, replace the cable, or perform noise countermeasures as appropriate.

- 36: communication error
   - Cause: The SSCNET III cable is disconnected, its end face is dirty, the cable is damaged, or noise contamination occurred.
   - Measures: Turn off the power before connecting the cable; wipe the end face, replace the cable, or perform noise countermeasures as appropriate.

- 38: communication frame error
   - Cause: The SSCNET III cable is disconnected, its end face is dirty, the cable is damaged, or noise contamination occurred.
   - Measures: Turn off the power before connecting the cable; wipe the end face, replace the cable, or perform noise countermeasures as appropriate.

- 39: communication axis information error
   - Cause: The SSCNET III cable is disconnected, its end face is dirty, the cable is damaged, or noise contamination occurred.
   - Measures: Turn off the power before connecting the cable; wipe the end face, replace the cable, or perform noise countermeasures as appropriate.

- 37: parameter error
   - Cause: The parameter value sent from the robot controller at power-on is incorrect.
   - Measures: Correct the setting value of parameter FSFLCTL.

- 88: watchdog
   - Cause: The robot controller does not operate normally.
   - Measures: Turn on the robot controller power supply again. If it comes back, contact your service provider.

- E4: parameter warning
   - Cause: The input value of the parameter is incorrect.
   - Measures: Correct the value input in parameter FSFLCTL.
