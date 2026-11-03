# Migration Notes

## Upcoming behavior change: `MelfaRobot.Disconnect()` now throws on cleanup failures

`MelfaRobot.Disconnect()` no longer swallows all exceptions from stream/socket teardown.

### Previous behavior

- `Disconnect()` used broad `catch { }` blocks.
- Teardown failures were silently ignored.
- Callers could not observe disconnect cleanup issues.

### New behavior

- `Disconnect()` still attempts all cleanup steps for both the network stream and TCP client.
- The internal fields are reset deterministically (`_Stream = null`, `_Client = null`) even when teardown operations fail.
- If one or more teardown operations fail, `Disconnect()` throws an `AggregateException` containing all captured errors.

### Why this changed

This improves reliability and diagnosability by surfacing cleanup failures instead of hiding them.

### Required caller action

If your code currently assumes `Disconnect()` never throws, update call sites to handle `AggregateException` (or `Exception`) around disconnect/dispose flows when needed.

Example:

```csharp
try
{
	robot.Disconnect();
}
catch (AggregateException ex)
{
	// Log or inspect ex.InnerExceptions
	throw;
}
```
