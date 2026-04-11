using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Worker;

public interface IBWBehaviour { }

public struct BWMessageBehaviour {
    private List<IBWBehaviour>? _ListBWBehaviour;

    public List<IBWBehaviour>? ListBWBehaviour => this._ListBWBehaviour;

    public static bool TryAddBehaviour<T>(ref BWMessageBehaviour that, T behaviour) where T : IBWBehaviour {
        var listBWBehaviour = (that._ListBWBehaviour ??= new());
        foreach (var itemBehaviour in listBWBehaviour) {
            if (itemBehaviour is T) {
                return false;
            }
        }
        listBWBehaviour.Add(behaviour);
        return true;
    }

    public bool TryGetBehaviour<T>([MaybeNullWhen(false)] out T behaviour) where T : IBWBehaviour {
        {
            if (this._ListBWBehaviour is { } listBWBehaviour) {
                foreach (var itemBehaviour in listBWBehaviour) {
                    if (itemBehaviour is T itemBehaviourT) {
                        behaviour = itemBehaviourT;
                        return true;
                    }
                }
            }
        }
        {
            behaviour = default;
            return false;
        }
    }
}


