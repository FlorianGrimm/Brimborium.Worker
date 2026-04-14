using System.Diagnostics;

namespace Brimborium.Worker;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record class BWIdentifier(
    [System.Runtime.CompilerServices.CallerMemberName] string Identifier = ""
) {
    public BWIdentifier CreateChild(
        [System.Runtime.CompilerServices.CallerMemberName] string childName=""
    ) {
        return new BWIdentifier($"{this.Identifier}/{childName}");
    }

    public override string ToString() => this.Identifier;
    private string GetDebuggerDisplay() => this.Identifier;

    private static BWIdentifier? _Empty;
    public static BWIdentifier Empty => (_Empty ??= new BWIdentifier(string.Empty));
}