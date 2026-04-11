namespace Brimborium.Worker;

public interface IBWMessageScope<TScope> {
}

public interface IBWMessageWithScope<TScope>:IBWMessage {
    TScope MessageScope { get; }
}

public sealed class BWMessageWithScope<TScope>
    : BWMessageBase
    , IBWMessageWithScope<TScope> {

    public BWMessageWithScope(
        TScope messageScope
        ) {
        this.MessageScope = messageScope;
    }

    public TScope MessageScope { get; }

    //public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
    //    return new BWMessageWithValueScope<TNextValue, TScope>(nextValue, this.MessageScope);
    //}

    //public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
    //    return new BWMessageWithResultScope<TNextResult, TScope>(nextResult, this.MessageScope);
    //}

    //public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
    //    return new BWMessageWithScope<TNextScope>(nextScope);
    //}
}
