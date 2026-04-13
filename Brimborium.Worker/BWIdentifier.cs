namespace Brimborium.Worker;

public record class BWIdentifier(
    [System.Runtime.CompilerServices.CallerMemberName] string Identifier = ""
) {
    public BWIdentifier CreateChild(string childName) {
        return new BWIdentifier($"{this.Identifier}/{childName}");
    }

    private static BWIdentifier? _Empty;
    public static BWIdentifier Empty => (_Empty ??= new BWIdentifier(string.Empty));
}