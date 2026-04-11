namespace Brimborium.Worker;

public interface IBWMessageWithResultScope<TResult, TScope>
    : IBWMessageWithResult<TResult>
    , IBWMessageWithScope<TScope> { }

public sealed class BWMessageWithResultScope<TResult, TScope>
    : BWMessageBase
    , IBWMessageWithResult<TResult>
    , IBWMessageWithScope<TScope>
    , IBWMessageWithResultScope<TResult, TScope> {

    public BWMessageWithResultScope(
        IBWMessageResult<TResult> messageResult,
        TScope messageScope
        ) {
        this.MessageResult = messageResult;
        this.MessageScope = messageScope;
    }

    public IBWMessageResult<TResult> MessageResult { get; }

    public TScope MessageScope { get; }

    //public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
    //    return new BWMessageWithValueResultScope<TNextValue, TResult, TScope>(nextValue, this.MessageResult, this.MessageScope);
    //}

    //public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
    //    return new BWMessageWithResultScope<TNextResult, TScope>(nextResult, this.MessageScope);
    //}

    //public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
    //    return new BWMessageWithResultScope<TResult, TNextScope>(this.MessageResult, nextScope);
    //}
}
