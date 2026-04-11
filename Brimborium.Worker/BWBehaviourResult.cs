namespace Brimborium.Worker;

public interface IBWBehaviourResult<T> : IBWBehaviour {
    Task<T> GetResultAsync(CancellationToken cancellationToken);
    Task SetResultAsync(T value, CancellationToken cancellationToken);
}

public class BWBehaviourResult<T> : IBWBehaviourResult<T> {
    private TaskCompletionSource<T>? _Completion;

    public BWBehaviourResult(
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
