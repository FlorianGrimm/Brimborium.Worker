namespace Brimborium.Worker;

public interface IBWMessageWithValueResult<TValue, TResult>
    : IBWMessageWithValue<TValue>
    , IBWMessageWithResult<TResult> { 
}

public sealed class BWMessageWithValueResult<TValue, TResult>
    : BWMessageBase
    , IBWMessageWithValueResult<TValue, TResult>
    , IBWMessageWithValue<TValue>
    , IBWMessageWithResult<TResult> {

    public BWMessageWithValueResult(
            TValue value,
            IBWMessageResult<TResult> messageResult
        ) {
        this.Value = value;
        this.MessageResult = messageResult;
    }

    public TValue Value { get; }

    public IBWMessageResult<TResult> MessageResult { get; }

    //public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
    //    return new BWMessageWithValueResult<TNextValue, TResult>(nextValue, this.MessageResult);
    //}

    //public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
    //    return new BWMessageWithValueResult<TValue, TNextResult>(this.Value, nextResult);
    //}

    //public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
    //    return new BWMessageWithValueResultScope<TValue, TResult, TNextScope>(this.Value, this.MessageResult, nextScope);
    //}
}
