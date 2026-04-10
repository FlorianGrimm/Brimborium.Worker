# Brimborium.Worker

## Definition

A System to communicate between workers (IBWWorker) with messages (IBWMessage).

A message has a value/payload The value/payload of the message should be immutable, the message might change.

A worker receives Messages, (optional in a queue), and process it with its own logic.

Every activity with the message should be reported to the Monitor (IBWMonitor).

A worker logic that filters a message should report if it matches or not.

A variant of a message can have result value that can be set from a worker.

A worker logic can generate other messages based on it's needs.

If a worker generates child messages that will be consolidated later by an other worker the message can track this.

Different behaviours can be enabled by different message classes. The worker should handle these behaviours if needed.

The implementation Monitor might use this information for meassurments, logging, debugging or providing data to visualize them.

## Implementation ideas

TODO

## Test

```cmd
dotnet run --project Brimborium.Worker.Test\Brimborium.Worker.Test.csproj
```