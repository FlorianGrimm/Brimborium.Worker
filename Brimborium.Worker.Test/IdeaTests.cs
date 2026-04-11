namespace Brimborium.Worker;

public class IdeaTests {
    [Test]
    public async Task Test001() {
        CancellationTokenSource cts = new CancellationTokenSource();
        BWInvokerToList<IBWMessageWithValue<int>> target = new();
        IBWMonitor monitor = BWMonitorNull.Instance;
        TestWorker sut = new(target, monitor);
        await sut.ExecuteMessage(new BWMessageWithValue<string>("a"), cts.Token);
        await Assert.That(target.ListMessages).Count().IsEqualTo(1);
        await Assert.That(target.ListMessages[0].Value).IsEqualTo(1);
    }

    class TestWorker : BWWorker<IBWMessageWithValue<string>> {
        public static readonly BWIdentifier ClassIdentifier = new ();
        private readonly IBWInvoker<IBWMessageWithValue<int>> _Next;

        public TestWorker(
                IBWInvoker<IBWMessageWithValue<int>> next,
                IBWMonitor monitor
            ) : base(
                ClassIdentifier, monitor
            ) {
            this._Next = next;
        }

        public override async Task ExecuteLogic(IBWMessageWithValue<string> message, CancellationToken cancellationToken) {
            var nextValue = message.Value.Length;
            var nextMessage = new BWMessageWithValue<int>(nextValue);
            await this._Next.ExecuteAsync(nextMessage, cancellationToken);            
        }
    }


}

