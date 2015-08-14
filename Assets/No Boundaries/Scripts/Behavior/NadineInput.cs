using UnityEngine;
using System.Collections;
using Rewired;

public class NadineInput : MonoBehaviour
{

    [SerializeField]
    private int playerId;

    [SerializeField]
    private string horizontalAxisName;

    [SerializeField]
    private string verticalAxisName;

    [SerializeField]
    private string burstActionName;

    [SerializeField]
    private Vector2 deadZone;

    private Player player;
    protected Player Player
    {
        get
        {
            player = player ?? Rewired.ReInput.players.GetPlayer(playerId);
            return player;
        }
        
    }

    public void SetPlayerId(int id)
    {
        playerId = id;
        player = Rewired.ReInput.players.GetPlayer(playerId);
    }

    public Vector2 GetMovementAxes()
    {
        float hAxis = Player.GetAxisRaw(horizontalAxisName);
        float vAxis = Player.GetAxisRaw(verticalAxisName);

        if (Mathf.Abs(hAxis) < Mathf.Abs(deadZone.x)) hAxis = 0;
        if (Mathf.Abs(vAxis) < Mathf.Abs(deadZone.y)) vAxis = 0;
        return new Vector2(hAxis, vAxis);
    }

    public bool GetBurstInput()
    {
        return Player.GetButtonDown(burstActionName);
    }
}
