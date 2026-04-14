namespace Brimborium.Worker;

public static partial class BWMiddlewareBuilderExtension {
    extension<TMessage>(BWMiddlewareBuilder<TMessage> that)
        where TMessage : IBWMessage {
        public BWMiddlewareBuilder<TMessage> AddTrackQueueIncomingMiddlewareBuilder(
                IBWTrackQueue<TMessage> trackQueue
            ) {
            var builder = new BWTrackQueueIncomingMiddlewareBuilder<TMessage>(trackQueue);
            return that.Add(builder);
        }
        public BWMiddlewareBuilder<TMessage> AddTrackQueueProcessMiddlewareBuilder() {
            var builder = new BWTrackQueueProcessMiddlewareBuilder<TMessage>();
            return that.Add(builder);
        }

    }
}

public sealed class BWTrackQueueIncomingMiddlewareBuilder<TMessage>(
        IBWTrackQueue<TMessage> trackQueue
        )
    : IBWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage {
    private readonly IBWTrackQueue<TMessage> _TrackQueue = trackQueue;

    public IBWMiddleware<TMessage> CreateMiddleware(
            IBWMiddleware<TMessage> caller,
            IBWMiddleware<TMessage> next,
            IBWMonitor monitor
        ) => new BWTrackQueueIncomingMiddleware<TMessage>(this._TrackQueue, next);
}

public sealed class BWTrackQueueProcessMiddlewareBuilder<TMessage>
    : IBWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage {

    public IBWMiddleware<TMessage> CreateMiddleware(
            IBWMiddleware<TMessage> caller,
            IBWMiddleware<TMessage> next,
            IBWMonitor monitor
        ) => new BWTrackQueueProcessMiddleware<TMessage>(next);
}

public class BWTrackQueueIncomingMiddleware<TMessage>(
        IBWTrackQueue<TMessage> trackQueue,
        IBWMiddleware<TMessage> next
        )
    : IBWMiddleware<TMessage>
    where TMessage : IBWMessage {
    private readonly IBWTrackQueue<TMessage> _TrackQueue = trackQueue;
    private readonly IBWMiddleware<TMessage> _Next = next;

    public async Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken) {
        message.SetBehaviour<IBWTrackQueue<TMessage>>(0, this._TrackQueue);
        await this._TrackQueue.AddIncomingAsync(message, cancellationToken);
        await this._Next.ProcessMessageAsync(message, cancellationToken);
    }
}

public class BWTrackQueueProcessMiddleware<TMessage>(
        IBWMiddleware<TMessage> next
        )
    : IBWMiddleware<TMessage>
    where TMessage : IBWMessage {
    private readonly IBWMiddleware<TMessage> _Next = next;

    public async Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken) {
        if (!message.TryGetBehaviour<IBWTrackQueue<TMessage>>(0, out var trackQueue)) {
            throw new ArgumentException("need a Behaviour of IBWTrackQueue", nameof(message));
        }

        try {
            await trackQueue.SetProcessAsync(message, cancellationToken);
            await this._Next.ProcessMessageAsync(message, cancellationToken);
            await trackQueue.SetDoneAsync(message, cancellationToken);
        } catch (System.Exception error) {
            await trackQueue.SetFaultedAsync(message, error, cancellationToken);
        }
    }
}


public interface IBWTrackQueue<TMessage>
    where TMessage : IBWMessage {
    Task AddIncomingAsync(TMessage message, CancellationToken cancellationToken);
    Task SetProcessAsync(TMessage message, CancellationToken cancellationToken);
    Task SetFaultedAsync(TMessage message, Exception error, CancellationToken cancellationToken);
    Task SetDoneAsync(TMessage message, CancellationToken cancellationToken);
}

public class BWTrackQueue<TMessage>
    : IBWTrackQueue<TMessage>
    where TMessage : IBWMessage {
    protected ConcurrentDictionary<TMessage, TMessage> ListWaiting = new();
    protected ConcurrentDictionary<TMessage, TMessage> ListProcessing = new();
    protected List<TMessage> ListDone = new();
    protected SemaphoreSlim SemaphoreDone = new(1, 1);
    protected readonly int MaxCountDone;

    public BWTrackQueue(
        int maxDone
        ) {
        this.MaxCountDone = maxDone;
    }

    public async Task AddIncomingAsync(TMessage message, CancellationToken cancellationToken) {
        bool addedToWaiting = this.ListWaiting.TryAdd(message, message);
        await this.AddedWaiting(addedToWaiting, message, cancellationToken);
    }

    protected virtual Task AddedWaiting(bool added, TMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public async Task SetProcessAsync(TMessage message, CancellationToken cancellationToken) {
        bool removedFromWaiting = this.ListWaiting.TryRemove(message, out _);
        bool addedToProcessing = this.ListProcessing.TryAdd(message, message);
        await this.AddedProcessing(removedFromWaiting, addedToProcessing, message, cancellationToken);
    }

    protected virtual Task AddedProcessing(bool removedFromWaiting, bool addedToProcessing, TMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public async Task SetFaultedAsync(TMessage message, Exception error, CancellationToken cancellationToken) {
        bool removedFromProcessing = this.ListProcessing.TryRemove(message, out _);
        if (removedFromProcessing) {
            await this.SemaphoreDone.WaitAsync(cancellationToken);
            try {
                this.LimitListDone();
                this.ListDone.Add(message);
                await this.AddedFaulted(message, error, cancellationToken);
            } finally {
                this.SemaphoreDone.Release();
            }
        }
    }

    protected virtual Task AddedFaulted(
        TMessage message,
        Exception error,
        CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }


    public async Task SetDoneAsync(TMessage message, CancellationToken cancellationToken) {
        bool removedFromProcessing = this.ListProcessing.TryRemove(message, out _);
        if (removedFromProcessing) {
            await this.SemaphoreDone.WaitAsync(cancellationToken);
            try {
                this.LimitListDone();
                this.ListDone.Add(message);
                await this.AddedDone(message, cancellationToken);
            } finally {
                this.SemaphoreDone.Release();
            }
        }
    }

    protected virtual void LimitListDone() {
        if (this.MaxCountDone < this.ListDone.Count) {
            this.ListDone.RemoveRange(0, this.ListDone.Count / 2);
        }
    }

    protected virtual Task AddedDone(
        TMessage message,
        CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public async Task<SnapshotQueueItem<TMessage>> GetSnapshotQueueItemAsync(CancellationToken cancellationToken) {
        await this.SemaphoreDone.WaitAsync(cancellationToken);
        try {
            List<TMessage> listWaiting = new List<TMessage>();
            foreach (var itemWaiting in this.ListWaiting.Keys) {
                listWaiting.Add(itemWaiting);
            }

            List<TMessage> listProcessing = new List<TMessage>();
            foreach (var itemProcessing in this.ListProcessing.Keys) {
                listProcessing.Add(itemProcessing);
            }

            List<TMessage> listDone = new List<TMessage>(this.ListDone.Count);
            listDone.AddRange(this.ListDone);
            return new SnapshotQueueItem<TMessage>(
                listWaiting,
                listProcessing,
                listDone);
        } finally {
            this.SemaphoreDone.Release();
        }
    }
}

public sealed record class SnapshotQueueItem<TMessage>(
    List<TMessage> ListWaiting,
    List<TMessage> ListProcessing,
    List<TMessage> ListDone
    )
    where TMessage : IBWMessage;