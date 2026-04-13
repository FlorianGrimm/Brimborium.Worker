namespace Brimborium.Worker;

public interface IBWBehaviourCompletion {
    Task GetCompletionAsync(CancellationToken cancellationToken);
    Task SetCompletionAsync(CancellationToken cancellationToken);
}

public sealed class BWBehaviourCompletion 
    : IBWBehaviourCompletion {
    private readonly TaskCompletionSource _CompletionSource = new();

    public BWBehaviourCompletion() {
    }

    public Task SetCompletionAsync(CancellationToken cancellationToken) {
        this._CompletionSource.TrySetResult();
        return Task.CompletedTask;
    }

    public Task GetCompletionAsync(CancellationToken cancellationToken) {
        return this._CompletionSource.Task;
    }
}

public static class BWBehaviourCompletionExtension {
    extension<TBWMessage>(TBWMessage that)
        where TBWMessage : IBWMessage {
        public Task SetCompletionAsync(CancellationToken cancellationToken) {
            if (that.TryGetBehaviour<IBWBehaviourCompletion>(0, out var completion)) {
                return completion.SetCompletionAsync(cancellationToken);
            } else {
                return Task.CompletedTask;
            }
        }

        public Task GetCompletionAsync(CancellationToken cancellationToken) {
            if (that.TryGetBehaviour<IBWBehaviourCompletion>(0, out var completion)) {
                return completion.GetCompletionAsync(cancellationToken);
            } else {
                return Task.CompletedTask;
            }
        }
    }
}