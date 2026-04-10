namespace Brimborium.Worker;

public interface IBWMessageWithValueScope<TValue, TScope>
    : IBWMessageWithValue<TValue>
    , IBWMessageWithScope<TScope> {
}

public sealed class BWMessageWithValueScope<TValue, TScope>
    : BWMessageBase
    , IBWMessageWithValue<TValue>
    , IBWMessageWithScope<TScope> {

    public BWMessageWithValueScope(
        TValue value,
        TScope messageScope
        )  {
        this.Value = value;
        this.MessageScope = messageScope;
    }

    public TValue Value { get; }

    public TScope MessageScope { get; }

    public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
        return new BWMessageWithValueScope<TNextValue, TScope>(nextValue, this.MessageScope);
    }
    public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
        return new BWMessageWithValueResultScope<TValue, TNextResult, TScope>(this.Value, nextResult, this.MessageScope);
    }
    public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
        return new BWMessageWithValueScope<TValue, TNextScope>(this.Value, nextScope);
    }
}
