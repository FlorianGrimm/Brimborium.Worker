namespace Brimborium.Worker;

public interface IBWMessageResult<TResult> {
    Task<TResult> GetResultAsync();
    void SetResult(TResult result);
    void SetException(Exception exception);
}

public class BWMessageResult<TResult> : IBWMessageResult<TResult> {
    public BWMessageResult(TaskCompletionSource<TResult>? completion) {
        this._Completion = completion;
    }

    private TaskCompletionSource<TResult>? _Completion;

    public Task<TResult> GetResultAsync() {
        if (this._Completion is null) {
            System.Threading.Interlocked.CompareExchange(
                ref this._Completion,
                new(),
                null);
        }
        return this._Completion.Task;
    }

    public void SetResult(TResult result) {
        if (this._Completion is null) {
            System.Threading.Interlocked.CompareExchange(
                ref this._Completion,
                new(),
                null);
        }
        this._Completion.SetResult(result);
    }
    public void SetException(Exception exception) {
        if (this._Completion is null) {
            System.Threading.Interlocked.CompareExchange(
                ref this._Completion,
                new(),
                null);
        }
        this._Completion.SetException(exception);
    }
}


public interface IBWMessageWithResult<TResult> {
    IBWMessageResult<TResult> MessageResult { get; }
}

public sealed class BWMessageWithResult<TResult>
    : BWMessageBase
    , IBWMessageWithResult<TResult> {

    public BWMessageWithResult(
            IBWMessageResult<TResult> messageResult
        ) {
        this.MessageResult = messageResult;
    }

    public IBWMessageResult<TResult> MessageResult { get; }


    //public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
    //    throw new NotImplementedException();
    //}

    //public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
    //    throw new NotImplementedException();
    //}

    //public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
    //    throw new NotImplementedException();
    //}
}
