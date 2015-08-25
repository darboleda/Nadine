using UnityEngine;
using System;
using System.Collections;

public class PlayerStateRelay : StateRelay.GenericStateRelay<PlayerState>
{
    public string PlayerIdPrefix;
    public int PlayerId;

    public override string StateId
    {
        get
        {
            return string.Format("{0}{1}", PlayerIdPrefix, PlayerId);
        }
    }

    public void SetPlayerId(int id)
    {
        PlayerId = id;
        RequestNewState(StateId, true);
    }
}
