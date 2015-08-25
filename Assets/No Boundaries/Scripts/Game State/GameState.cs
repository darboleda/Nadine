using UnityEngine;
using System;
using System.Collections;

public class GameState : MonoBehaviour
{
    public static GameState FindGame()
    {
        return FindObjectOfType<GameState>();
    }

    public StateMap StateMap;

    public T RequestState<T>(string id) where T : RequestableState
    {
        T prefab = StateMap.GetRequestableState<T>(id);
        if (prefab == default(T)) return default(T);

        T instance = GameObject.Instantiate(prefab);
        instance.transform.SetParent(transform);

        return instance;
    }

    public void DestroyState(RequestableState state)
    {
        GameObject.Destroy(state.gameObject);
    }
}
