namespace Brimborium.Worker;

public sealed class BWBehaviourQueueScope {
    public BWBehaviourQueueScope(IBWMonitorScope scope) {
        this.Scope = scope;
    }

    public IBWMonitorScope Scope { get; }
}

public class BWWorkerWithQueue<TMessage>
    : BWWorker<TMessage>
    , IBWWorkerWithQueue
    where TMessage : IBWMessage {
    private readonly IBWQueue<TMessage> _Queue;
    private readonly IBWInvoker<TMessage> _InvokerNext;

    public BWWorkerWithQueue(
            IBWQueue<TMessage> queue,
            IBWInvoker<TMessage> invokerNext,
            BWMiddlewareBuilder<TMessage>? middlewareBuilder,
            BWIdentifier identifier, IBWMonitor monitor
        ) : base(
            middlewareBuilder,
            identifier, monitor
        ) {
        this._Queue = queue;
        this._InvokerNext = this.RegisterNext(invokerNext, this.Identifier.CreateChild("Next"));
        this._CompletionSource = new();
    }

    public override async Task ExecuteMessageAsync(TMessage message, CancellationToken cancellationToken) {
        var scope = await this.Monitor.ReportBlockStart(this, message, "Queue", cancellationToken);
        BWBehaviourQueueScope queueScope = new(scope);
        message.SetBehaviour(0, queueScope);
        await base.ExecuteMessageAsync(message, cancellationToken);
    }

    protected override async Task ExecuteLogic(TMessage message, CancellationToken cancellationToken) {
        await this._Queue.Enqueue(message, cancellationToken);
    }

    private SemaphoreSlim? _SemaphoreExecution;
    private Task? _TaskExecute;
    private TaskCompletionSource _CompletionSource;

    public async Task StartAsync(CancellationToken cancellationToken) {
        if (_TaskExecute is null) {
            System.Threading.Interlocked.CompareExchange(
                ref this._SemaphoreExecution,
                new(1, 1),
                null
            );
            await this._SemaphoreExecution.WaitAsync(cancellationToken);
            try {
                if (_TaskExecute is null) {
                    var monitorScope = await this.Monitor.ReportBlockStart(this, BWMessageVoid.Instance, "Execute", cancellationToken);
                    this._TaskExecute = this.ExecuteAsync(monitorScope, cancellationToken);
                }
            } finally {
                this._SemaphoreExecution.Release();
            }
        }
    }

    private async Task ExecuteAsync(IBWMonitorScope monitorScope, CancellationToken cancellationToken) {
        try {
            while (await this._Queue.WaitToReadAsync(cancellationToken)) {
                while (this._Queue.TryRead(out var message)) {
                    if (message.TryGetBehaviour<BWBehaviourQueueScope>(0, out var queueScope)) {
                        queueScope.Scope.Dispose();                        
                    }
                    await this._InvokerNext.ExecuteAsync(message, cancellationToken);
                    if (message.TryGetBehaviour<BWBehaviourCompletion>(0, out var completion)) {
                        await completion.SetCompletionAsync(cancellationToken);
                    }
                }
            }
            await monitorScope.ReportSuccess(cancellationToken);

            this._CompletionSource.TrySetResult();
        } catch (Exception error) {
            await monitorScope.ReportError(error, cancellationToken);

            this._CompletionSource.TrySetException(error);
        } finally {
            monitorScope.Dispose();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        await this._Queue.StopAsync(cancellationToken);
    }

    public Task CompletionAsync(CancellationToken cancellationToken) {
        return this._CompletionSource.Task;
    }
}
