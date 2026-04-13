namespace Brimborium.Worker;

public class BWInvokerToListTests {
    [Test]
    public async Task Test001() {
        CancellationTokenSource cts = new CancellationTokenSource();
        IBWMonitor monitor = BWMonitorNull.Instance;
        BWInvokerToList<IBWMessageWithValue<int>> target = new(null, monitor);
        TestWorkerStringLength sut = new(target, monitor);
        await sut.ExecuteMessageAsync(new BWMessageWithValue<string>("a"), cts.Token);
        await Assert.That(target.ListMessages).Count().IsEqualTo(1);
        await Assert.That(target.ListMessages[0].Value).IsEqualTo(1);
    }
}

public class TestWorkerStringLength : BWWorker<IBWMessageWithValue<string>> {
    public static readonly BWIdentifier ClassIdentifier = new();
    private readonly IBWInvoker<IBWMessageWithValue<int>> _Next;

    public TestWorkerStringLength(
            IBWInvoker<IBWMessageWithValue<int>> next,
            IBWMonitor monitor
        ) : base(
            ClassIdentifier, monitor
        ) {
        this._Next = this.RegisterNext(next, this.Identifier.CreateChild("Next"));
    }

    public override async Task ExecuteLogic(IBWMessageWithValue<string> message, CancellationToken cancellationToken) {
        var nextValue = message.Value.Length;
        var nextMessage = new BWMessageWithValue<int>(nextValue);
        await this._Next.ExecuteAsync(nextMessage, cancellationToken);
    }
}

