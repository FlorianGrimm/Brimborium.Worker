using Foundatio.Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Worker;

public class BWWorkerWithQueueTests {
    [Test]
    public async Task BWWorkerWithQueueTest001() {
        CancellationTokenSource cts = new CancellationTokenSource();
        Microsoft.Extensions.DependencyInjection.ServiceCollection serviceBuilder = new();
        IBWMonitor monitor = new BWMonitorConsole();
        serviceBuilder.AddSingleton<IBWMonitor>(monitor);
        serviceBuilder.AddMediator((options) => {
            options.SetMediatorLifetime(ServiceLifetime.Scoped);
            options.AddAssembly<IBWMonitor>();
            options.AddAssembly<BWWorkerWithQueueTests>();
        });
        serviceBuilder.AddBrimboriumWorker();
        serviceBuilder.AddBrimboriumWorkerTest();
        var globalServiceProvider = serviceBuilder.BuildServiceProvider();
        using var scope = globalServiceProvider.CreateScope();
        var scopeServiceProvider = scope.ServiceProvider;
        IMediator mediator = scopeServiceProvider.GetRequiredService<IMediator>();

        BWTrackQueue<TestBWWorkerWithQueueQueueItem> trackQueue = new(12);

        BWInvokerMediatorCommand<TestBWWorkerWithQueueQueueItem> invokerNextWorkerWithQueue;
        {
            BWMiddlewareBuilder<TestBWWorkerWithQueueQueueItem> middlewareBuilderInvokerNext = new();
            middlewareBuilderInvokerNext.AddTrackQueueProcessMiddlewareBuilder();
            invokerNextWorkerWithQueue = new BWInvokerMediatorCommand<TestBWWorkerWithQueueQueueItem>(
                mediator,
                middlewareBuilderInvokerNext,
                monitor);
        }

        BWWorkerWithQueue<TestBWWorkerWithQueueQueueItem> workerWithQueue;
        {
            var middlewareBuilderWorkerWithQueue = new BWMiddlewareBuilder<TestBWWorkerWithQueueQueueItem>();
            middlewareBuilderWorkerWithQueue.AddTrackQueueIncomingMiddlewareBuilder(trackQueue);
            workerWithQueue = new BWWorkerWithQueue<TestBWWorkerWithQueueQueueItem>(
                new BWQueue<TestBWWorkerWithQueueQueueItem>(
                    System.Threading.Channels.Channel.CreateUnbounded<TestBWWorkerWithQueueQueueItem>()
                    ),
                invokerNextWorkerWithQueue,
                middlewareBuilderWorkerWithQueue,
                new BWIdentifier(),
                monitor
                );
        }

        for (int i = 1; i <= 10; i++) {
            TestBWWorkerWithQueueQueueItem cmd = new(i);
            await workerWithQueue.ExecuteMessageAsync(cmd, cts.Token);
        }

        await Task.Delay(10, cts.Token);

        {
            var actSnapshot = await trackQueue.GetSnapshotQueueItemAsync(cts.Token);
            await Assert.That(actSnapshot.ListWaiting).Count().IsEqualTo(10);
            await Assert.That(actSnapshot.ListProcessing).Count().IsEqualTo(0);
            await Assert.That(actSnapshot.ListDone).Count().IsEqualTo(0);
        }

        Task taskWorkerWithQueue = workerWithQueue.StartAsync(cts.Token);

        for (int i = 11; i <= 20; i++) {
            TestBWWorkerWithQueueQueueItem cmd = new(i);
            await workerWithQueue.ExecuteMessageAsync(cmd, cts.Token);
        }
        await Task.Delay(10, cts.Token);

        {
            var actSnapshot = await trackQueue.GetSnapshotQueueItemAsync(cts.Token);
            await Assert.That(actSnapshot.ListWaiting).Count().IsEqualTo(0);
            await Assert.That(actSnapshot.ListProcessing).Count().IsEqualTo(0);
            await Assert.That(actSnapshot.ListDone).Count().IsGreaterThanOrEqualTo(4);
            await Assert.That(actSnapshot.ListDone).Count().IsLessThanOrEqualTo(12);
        }

        for (int i = 21; i <= 100; i++) {
            TestBWWorkerWithQueueQueueItem cmd = new(i);
            await workerWithQueue.ExecuteMessageAsync(cmd, cts.Token);
        }

        await workerWithQueue.StopAsync(cts.Token);
        await workerWithQueue.CompletionAsync(cts.Token);
        await taskWorkerWithQueue;

        var handler = scopeServiceProvider.GetRequiredService<TestBWWorkerWithQueueHandler>();
        await Assert.That(handler.List.Last().Value).IsEqualTo(100);
        {
            var actSnapshot = await trackQueue.GetSnapshotQueueItemAsync(cts.Token);
            await Assert.That(actSnapshot.ListWaiting).Count().IsEqualTo(0);
            await Assert.That(actSnapshot.ListProcessing).Count().IsEqualTo(0);
            await Assert.That(actSnapshot.ListDone).Count().IsLessThanOrEqualTo(12);
        }
    }
}
public sealed record TestBWWorkerWithQueueQueueItem(
        int Value
    )
    : BWRecordMessageBase()
    , Foundatio.Mediator.ICommand {
}

[Injectio.Attributes.RegisterSingleton]
public class TestBWWorkerWithQueueHandler {
    public List<TestBWWorkerWithQueueQueueItem> List = new();
    public TestBWWorkerWithQueueHandler() {
    }

    [Handler]
    public static async Task HandleAsync(
        TestBWWorkerWithQueueQueueItem message,
        TestBWWorkerWithQueueHandler handler,
        CancellationToken cancellationToken
        ) {
        if (20 < message.Value) {
            if (0 == message.Value%10) {
                await Task.Delay(1, cancellationToken);
            }
        }
        handler.List.Add(message);
    }
}


