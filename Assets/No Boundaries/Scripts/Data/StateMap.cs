using UnityEngine;
using System;
using System.Collections;

public class StateMap : ScriptableObject {

    [Serializable]
    public struct GameStatePairing
    {
        public string Id;
        public RequestableState State;
    }

    public GameStatePairing[] SupportedGameStates;

    public T GetRequestableState<T>(string id) where T : RequestableState
    {
        for (int i = 0; i < SupportedGameStates.Length; i++)
        {
            if (SupportedGameStates[i].Id != id) continue;
            RequestableState state = SupportedGameStates[i].State;
            if (state is T) return (T)state;
        }
        
        return default(T);
    }
}
