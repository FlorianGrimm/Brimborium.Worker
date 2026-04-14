namespace Brimborium.Worker.Test;

public class BWIdentifierTests {

    public async Task BWIdentifierWithName() {
        BWIdentifier sut = new("a");
        await Assert.That(sut.Identifier).IsEqualTo("a");
    }

    public async Task BWIdentifierWithCaller() {
        BWIdentifier sut = new();
        await Assert.That(sut.Identifier).IsEqualTo("BWIdentifierWithCaller");
    }


    public async Task BWIdentifierCreateChildWithName() {
        BWIdentifier sutA = new("a");
        BWIdentifier sut = sutA.CreateChild("b");
        await Assert.That(sut.Identifier).IsEqualTo("a/b");
    }

    public async Task BWIdentifierCreateChildWithCaller() {
        BWIdentifier sutA = new("a");
        BWIdentifier sut = sutA.CreateChild();
        await Assert.That(sut.Identifier).IsEqualTo("a/BWIdentifierCreateChildWithCaller");
    }

}
