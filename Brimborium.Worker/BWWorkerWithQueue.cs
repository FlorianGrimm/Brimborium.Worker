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
        this._InvokerNext = invokerNext;
        this._CompletionSource = new();
    }

    public override async Task ExecuteLogic(TMessage message, CancellationToken cancellationToken) {
        var scope = await this.Monitor.ReportBlockStart(this, message, "Queue", cancellationToken);
        BWBehaviourQueueScope queueScope = new(scope);
        message.SetBehaviour(0, queueScope);
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
/*
public class BWWorkerWithQueue : BWWorker, IBWWorkerWithQueue, IBWMonitored {
    public BWWorkerWithQueue(
            BWIdentifier identifier,
            IBWMonitor monitor
        ) : base(identifier, monitor) {
    }

    protected SemaphoreSlim? _SemaphoreExecution;
    protected Task? _TaskExecute;
    public virtual async Task StartAsync(CancellationToken cancellationToken) {
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

    public virtual async Task ExecuteAsync(IBWMonitorScope monitorScope, CancellationToken cancellationToken) {
        monitorScope.Dispose();
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task CompletionAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}

public class BWWorkerWithQueue<TValue, TMessage>
    : BWWorkerWithQueue
    , IBWWorker<TMessage>
    where TMessage : IBWMessage {
    private readonly IBWQueue<TMessage> _Queue;

    public BWWorkerWithQueue(
            IBWQueue<TMessage> queue,
            BWIdentifier identifier,
            IBWMonitor monitor
        ) : base(
            identifier,
            monitor
        ) {
        this._Queue = queue;
    }

    public async Task ExecuteMessage(TMessage message, CancellationToken cancellationToken) {
        await this.Monitor.ReportEvent(this, message, "Queue", "Enqueue", cancellationToken);
        await this._Queue.Enqueue(message, cancellationToken);
    }

    public override async Task ExecuteAsync(IBWMonitorScope monitorScope, CancellationToken cancellationToken) {
        try {
            while (await this._Queue.WaitToReadAsync(cancellationToken)) {
                while (this._Queue.TryRead(out var message)) {
                    var messageScope = await this.Monitor.ReportBlockStart(this, message, BWMonitor.ScopeExecute, cancellationToken);
                    try {
                        await this.ExecuteInner(message, cancellationToken);
                        await messageScope.ReportSuccess(cancellationToken);
                    } catch (Exception error) {
                        await messageScope.ReportError(error,cancellationToken);
                    } finally {
                        messageScope.Dispose();
                    }
                }
            }
            await monitorScope.ReportSuccess(cancellationToken);
        } catch (Exception error) {
            await monitorScope.ReportError(error, cancellationToken);
        } finally {
            monitorScope.Dispose();
        }
    }

    protected virtual Task ExecuteInner(TMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
*/