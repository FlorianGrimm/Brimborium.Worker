namespace Brimborium.Worker;

public sealed class BWMessageBehaviourTree {
    public IBWMessage? Parent { get; set; }
    public List<IBWMessage>? ListChild { get; set; }
    public List<IBWMessage> GetListChild() {
        return this.ListChild ??= new();
    }

}

public static class BWMessageBehaviourTreeExtension {
    extension<TMessage>(TMessage message)
        where TMessage : IBWMessage {
        public BWMessageBehaviourTree GetMessageBehaviourTree() {
            if (message.TryGetBehaviour<BWMessageBehaviourTree>(0, out var result)) {
                return result;
            } else {
                result = new BWMessageBehaviourTree();
                message.SetBehaviour<BWMessageBehaviourTree>(0, result);
                return result;
            }
        }


        public void SetParent(IBWMessage messageParent) {
            var messageBehaviourTreeParent = messageParent.GetMessageBehaviourTree();
            var messageBehaviourTreeChild = message.GetMessageBehaviourTree();
            if (ReferenceEquals(messageBehaviourTreeChild.Parent, messageParent)) {
                return;
            } else if (messageBehaviourTreeChild.Parent is { }) {
                throw new ArgumentException("has already another parent", nameof(messageParent));
            } else { 
                messageBehaviourTreeParent.GetListChild().Add(message);
                messageBehaviourTreeChild.Parent = messageParent;
            }
        }
    }
}

