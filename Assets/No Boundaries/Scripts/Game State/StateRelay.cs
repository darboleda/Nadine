using UnityEngine;
using System.Collections;

public abstract class StateRelay : MonoBehaviour {

    public abstract class GenericStateRelay<T> : StateRelay where T : RequestableState
    {
        private T internalState;
        public T State
        {
            get
            {
                if (internalState == null)
                {
                    GameState game = GameState.FindGame();
                    if (game != null)
                    {
                        internalState = game.RequestState<T>(StateId);
                    }
                }
                return internalState;
            }
        }

        protected override RequestableState InternalState {get { return State; } }

        public abstract string StateId { get; }

        protected T RequestNewState(string stateId, bool destroyOldState)
        {
            GameState game = GameState.FindGame();
            if (game != null)
            {
                if (internalState != null && destroyOldState)
                {
                    game.DestroyState(internalState);
                }
                internalState = game.RequestState<T>(StateId);
            }
            else
            {
                internalState = null;
            }
            return internalState;
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
        if (InternalState != null)
        {
            InternalState.UpdateWith(updateId);
        }
    }
}
