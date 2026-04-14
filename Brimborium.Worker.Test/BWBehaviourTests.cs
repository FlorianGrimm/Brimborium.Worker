namespace Brimborium.Worker;

public class BWBehaviourTests {
    [Test]
    public async Task BWBehaviourTest001() { 
        BWMessage message = new BWMessage();
        var sutTestBehaviour = new TestBehaviour();
        message.SetBehaviour(0, sutTestBehaviour);

        { 
        var success = message.TryGetBehaviour<TestBehaviour>(0, out var actualTestBehaviour);
        await Assert.That(success).IsTrue();
        await Assert.That(actualTestBehaviour).IsSameReferenceAs(sutTestBehaviour);
        }

        message.RemoveBehaviour(sutTestBehaviour);

        {
            var success = message.TryGetBehaviour<TestBehaviour>(0, out var actualTestBehaviour);
            await Assert.That(success).IsFalse();
        }
    }
    
    class TestBehaviour { 
    }
}
