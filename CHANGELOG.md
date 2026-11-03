# Changelog

## Unreleased (after v0.1.22.6)

- Add `net10.0` target framework.
- Change `Disconnect()` to surface teardown failures via `AggregateException` and add migration guidance.
- Replace `lock(this)` with a dedicated private lock object for safer synchronization.
- Fix file transfer reliability: read all `FREAD` chunks and write full streams via repeated `FWRITE` chunks.

## v0.1.22.6

- Bump dependencies.
- Update license year.
- Modernize project format.
- Migrate solution to `.slnx` format.

## v0.1.22.5

- Implement `AXDATINST`, `DATINST`, and `DATRD`.
- Implement `SLOTRD` and `SLOTSET`.
- Implement `FATTRIB` and `IOSIGNAL`.
- Implement electric hand jog support and inline options.
- Implement changed-parameters list support.
- Implement `RENUM` (program renumber).
- Implement additional file operations.
- Implement CC-Link specific I/O functions.
- Parse individual `IOSIGNAL` values.
- Fix `MFRST` reset-target format.
- Fix JOG format.
- Make readonly-struct constructors internal.
- Rename `PositionType` to `LocationType`.
- Refactor regex patterns into shared constants in `RegexHelper`.
- Bump `System.Collections.Immutable` to `8.0.0`.
- Remove package release note metadata.

## v0.1.22.4

- Fix `RobotInformation` parsing.

## v0.1.22.3

- Add more error messages.
- Fix error number handling.

## v0.1.22.2

- Fix `OperationEnable` behavior.

---

## Commit range

- Start tag: `v0.1.22.1`
- End: `HEAD`
- Command reference: `git log --pretty=format:"%H|%ad|%an|%s" --date=short v0.1.22.1..HEAD`
