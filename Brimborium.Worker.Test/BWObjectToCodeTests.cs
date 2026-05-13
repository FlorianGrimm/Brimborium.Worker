namespace Brimborium.Worker;

public class BWObjectToCodeTests {
    [Test]
    public async Task StringToCodeTest() {
        var given = "abc";
        var sut = new BWConvertToCode();
        var act = sut.Convert(given).ToString();
        await Assert.That(act).IsEqualTo("\"abc\"");
    }

    [Test]
    public async Task DoubleToCodeTest() {
        var given = 1.5;
        var sut = new BWConvertToCode();
        var act = sut.Convert(given).ToString();
        // expected C# literal: 1.5
        await Assert.That(act).IsEqualTo("1.5");
    }

    [Test]
    public async Task ListOfStringToCodeTest() {
        var given = new List<string> { "abc", "def" };
        var sut = new BWConvertToCode();
        var act = sut.Convert(given).ToString();
        // expected C# initializer: new List<string> { "abc", "def" }
        await Assert.That(act).IsEqualTo("new List<string> { \"abc\", \"def\" }");
    }

    [Test]
    public async Task ListOfIntToCodeTest() {
        var given = new List<int> { 1, 2, 3 };
        var sut = new BWConvertToCode();
        var act = sut.Convert(given).ToString();
        // expected C# initializer: new List<int> { 1, 2, 3 }
        await Assert.That(act).IsEqualTo("new List<int> { 1, 2, 3 }");
    }

    [Test]
    public async Task EmptyListOfStringToCodeTest() {
        var given = new List<string>();
        var sut = new BWConvertToCode();
        var act = sut.Convert(given).ToString();
        // expected C# initializer: new List<string> { }
        await Assert.That(act).IsEqualTo("new List<string> { }");
    }
}