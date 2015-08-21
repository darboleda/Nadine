using UnityEngine;
using System;

public class RequestableState : MonoBehaviour
{
    [Serializable]
    public struct UpdateHandlerPair
    {
        public string Id;
        public UpdateHandler Handler;
    }
    
    public UpdateHandlerPair[] HandlerMap;
    public UpdateMap Map;

    public void UpdateWith(string valueId)
    {
        UpdateInfo update = Map.GetMappedUpdate(valueId);
        foreach (UpdateHandlerPair handler in HandlerMap)
        {
            if (handler.Id != update.HandlerId) continue;
            handler.Handler.Handle(update);
        }
    }
}
