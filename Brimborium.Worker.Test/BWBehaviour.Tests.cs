namespace Brimborium.Worker.Test;

public class BWBehaviour {
    [Test]
    public async Task BWBehaviourTest001() { 
        BWMessage message = new BWMessage();
        var sutTestBehaviour = new TestBehaviour();
        message.TryAddBehaviour(sutTestBehaviour);

        var success = message.TryGetBehaviour<TestBehaviour>(out var actualTestBehaviour);
        await Assert.That(success).IsTrue();
        await Assert.That(actualTestBehaviour).IsSameReferenceAs(sutTestBehaviour);
    }
    
    class TestBehaviour : IBWBehaviour { 
    }
}
