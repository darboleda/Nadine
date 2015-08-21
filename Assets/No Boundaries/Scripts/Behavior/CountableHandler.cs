using UnityEngine;
using System;

public class CountableHandler : UpdateHandler
{
    public string DisplayId;

    public int Max;
    public int Current;

    public void OnEnable()
    {
        UpdateDisplay();
    }

    public override void Handle(UpdateInfo update)
    {
        CountableInfo countable = update as CountableInfo;
        if (countable != null)
        {
            Current = Mathf.Min(Max, countable.Value + Current);
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        foreach (CountDisplay display in Hud.FindHud().RequestDisplay<CountDisplay>(DisplayId))
        {
            display.SetCurrent(Current);
        }
    }
}
