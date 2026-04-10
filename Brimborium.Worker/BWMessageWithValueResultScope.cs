namespace Brimborium.Worker;

public interface IBWMessageWithValueResultScope<TValue, TResult, TScope>
    : IBWMessageWithValue<TValue>
    , IBWMessageWithResult<TResult>
    , IBWMessageWithScope<TScope> {
}

public sealed class BWMessageWithValueResultScope<TValue, TResult, TScope>
    : BWMessageBase
    , IBWMessageWithValue<TValue>
    , IBWMessageWithResult<TResult>
    , IBWMessageWithScope<TScope> {

    public BWMessageWithValueResultScope(
        TValue value,
        IBWMessageResult<TResult> messageResult,
        TScope messageScope
        ) {
        this.Value = value;
        this.MessageResult = messageResult;
        this.MessageScope = messageScope;
    }

    public TValue Value { get; }

    public IBWMessageResult<TResult> MessageResult { get; }

    public TScope MessageScope { get; }

    public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
        return new BWMessageWithValueResultScope<TNextValue, TResult, TScope>(nextValue, this.MessageResult, this.MessageScope);
    }

    public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
        return new BWMessageWithValueResultScope<TValue, TNextResult, TScope>(this.Value, nextResult, this.MessageScope);
    }

    public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
        return new BWMessageWithValueResultScope<TValue, TResult, TNextScope>(this.Value, this.MessageResult, nextScope);
    }
}
