using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Worker;

/* public interface IBWBehaviour { } */

public struct BWMessageBehaviour {
    private List<object>? _ListBWBehaviour;

    public List<object>? ListBWBehaviour => this._ListBWBehaviour;


    public static void SetBehaviour<T>(ref BWMessageBehaviour that, int index, T behaviour) where T : class {
        var listBWBehaviour = (that._ListBWBehaviour ??= new());

        if (0 <= index) {
            for (var idx = 0; idx < listBWBehaviour.Count; idx++) {
                var itemBehaviour = listBWBehaviour[idx];
                if (itemBehaviour is T itemBehaviourT) {
                    if (index == 0) {
                        listBWBehaviour[idx] = behaviour;
                    } else {
                        index--;
                    }
                }
            }
            {
                listBWBehaviour.Add(behaviour);
            }
        } else {
            for (var idx = listBWBehaviour.Count - 1; 0 <= idx; idx--) {
                var itemBehaviour = listBWBehaviour[idx];
                if (itemBehaviour is T itemBehaviourT) {
                    if (index == 0) {
                        listBWBehaviour[idx] = behaviour;
                    } else {
                        index++;
                    }
                }
            }
            {
                listBWBehaviour.Insert(0, behaviour);
            }
        }
    }

    public readonly bool TryGetBehaviour<T>(int index, [MaybeNullWhen(false)] out T behaviour) {
        if (this._ListBWBehaviour is { } listBWBehaviour) {
            if (0 <= index) {
                for (var idx = 0; idx < listBWBehaviour.Count; idx++) {
                    var itemBehaviour = listBWBehaviour[idx];
                    if (itemBehaviour is T itemBehaviourT) {
                        if (index == 0) {
                            behaviour = itemBehaviourT;
                            return true;
                        } else {
                            index--;
                        }
                    }
                }
                {
                    behaviour = default;
                    return false;
                }
            } else {
                for (var idx = listBWBehaviour.Count - 1; 0 <= idx; idx--) {
                    var itemBehaviour = listBWBehaviour[idx];
                    if (itemBehaviour is T itemBehaviourT) {
                        if (index == 0) {
                            behaviour = itemBehaviourT;
                            return true;
                        } else {
                            index++;
                        }
                    }
                }
                {
                    behaviour = default;
                    return false;
                }
            }
        } else {
            behaviour = default;
            return false;
        }
    }

    public void RemoveBehaviour<T>(T behaviour) where T : class {
        if (this._ListBWBehaviour is { } listBWBehaviour) {
            for (var idx = listBWBehaviour.Count - 1; 0 <= idx; idx--) {
                var itemBehaviour = listBWBehaviour[idx];
                if (ReferenceEquals(itemBehaviour, behaviour)) {
                    listBWBehaviour.RemoveAt(idx);
                }
            }
        }
    }
}

