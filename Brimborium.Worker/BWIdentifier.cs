namespace Brimborium.Worker;

public record class BWIdentifier(
    [System.Runtime.CompilerServices.CallerMemberName] string Identifier = ""
);
