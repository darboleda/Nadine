using UnityEngine;
using System.Collections;

public abstract class StateRelay : MonoBehaviour {

    public abstract class GenericStateRelay<T> : StateRelay where T : RequestableState
    {
        public new T State { get; private set; }

        protected override RequestableState InternalState { get { return State; } }

        public string StateId;
        public void Awake()
        {
            State = GameState.FindGame().RequestState<T>(StateId);
        }

    }

    protected abstract RequestableState InternalState { get; }

    public void OnDestroy()
    {
        if (InternalState != null)
        {
            GameObject.Destroy(InternalState.gameObject);
        }
    }

    public void UpdateState(string updateId)
    {
        InternalState.UpdateWith(updateId);
    }
}
