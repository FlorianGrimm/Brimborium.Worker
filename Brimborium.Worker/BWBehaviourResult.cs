namespace Brimborium.Worker;

public interface IBWBehaviourResult<T> {
    Task<T> GetResultAsync(CancellationToken cancellationToken);
    Task SetResultAsync(T value, CancellationToken cancellationToken);
}

public class BWBehaviourResultTaskCompletionSource<T> : IBWBehaviourResult<T> {
    private TaskCompletionSource<T>? _Completion;

    public BWBehaviourResultTaskCompletionSource(
        TaskCompletionSource<T>? completion
        ) {
        this._Completion = completion;
    }

    public Task SetResultAsync(T value, CancellationToken cancellationToken) {
        if (this._Completion is not { } completion) {
            System.Threading.Interlocked.CompareExchange(
                ref this._Completion,
                new(),
                null
                );
            completion = this._Completion ?? throw new Exception();
        }
        completion.SetResult(value);
        return Task.CompletedTask;
    }

    public Task<T> GetResultAsync(CancellationToken cancellationToken){
        if (this._Completion is not { } completion) {
            System.Threading.Interlocked.CompareExchange(
                ref this._Completion,
                new(),
                null
                );
            completion = this._Completion ?? throw new Exception();
        }
        Task<T> task = completion.Task.WaitAsync(cancellationToken);
        return task;
    }
}

public static class BWBehaviourResultExtension {
    extension (IBWMessage that) {
        public Task? SetResultAsync<T>(T value, CancellationToken cancellationToken) {
            if (that.TryGetBehaviour<IBWBehaviourResult<T>>(0, out var behaviourResult)) {
                return behaviourResult.SetResultAsync(value, cancellationToken);
            } else {
                return default;
            }
        }
        public Task<T>? GetResultAsync<T>(CancellationToken cancellationToken) {
            if (that.TryGetBehaviour<IBWBehaviourResult<T>>(0, out var behaviourResult)) {
                return GetBehaviour(behaviourResult, cancellationToken);
            } else {
                return default;
            }

            static async Task<T> GetBehaviour(IBWBehaviourResult<T> behaviourResult, CancellationToken cancellationToken) {
                var result = await behaviourResult.GetResultAsync(cancellationToken);
                return result;
            }
        }
    }
}