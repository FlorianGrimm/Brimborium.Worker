namespace Brimborium.Worker;

public class BWSequenceTests {
    [Test]
    public async Task BWSequenceTest001() {
        CancellationTokenSource cts = new CancellationTokenSource();
        IBWMonitor monitor = BWMonitorNull.Instance;
        //BWInvokerToList<IBWMessageWithValue<int>> target = new(null, monitor);
        //TestWorkerStringLength sut = new(target, monitor);
        //await sut.ExecuteMessage(new BWMessageWithValue<string>("a"), cts.Token);
        //await Assert.That(target.ListMessages).Count().IsEqualTo(1);
        //await Assert.That(target.ListMessages[0].Value).IsEqualTo(1);
    }
}