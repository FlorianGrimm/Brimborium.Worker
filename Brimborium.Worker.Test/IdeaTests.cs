namespace Brimborium.Worker;

public class IdeaTests {
    [Test]
    public async Task Test001() {
        await Assert.That(1).IsEqualTo(1);
    }

    class TestWorker : BWWorker<IBWMessageWithValue<string>> {
        private readonly IBWWorker<IBWMessageWithValue<string>> _Next;

        public TestWorker(
                IBWWorker<IBWMessageWithValue<string>> next,
                BWIdentifier identifier, IBWMonitor monitor
            ) : base(
                null,
                identifier, monitor
            ) {
            this._Next = next;
        }
        public override async Task ExecuteMessage(IBWMessageWithValue<string> message, CancellationToken cancellationToken) {
            var nextValue = message.Value.Length;
            var nextMessage = message.CreateWithValue(nextValue);
            // await message.AddNextMessage(nextMessage);
            await this._Next.HandleMessage(message, cancellationToken);            
        }
    }


}

