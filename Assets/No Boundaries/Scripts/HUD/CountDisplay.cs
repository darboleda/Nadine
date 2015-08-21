using UnityEngine;

public abstract class CountDisplay : MonoBehaviour
{
    private int max;
    private int current;

    public void SetCurrent(int value)
    {
        current = value;
        UpdateDisplay(current, max);
    }

    public void SetMax(int value)
    {
        max = value;
        UpdateDisplay(current, max);
    }

    public void Set(int current, int max)
    {
        this.current = current;
        this.max = max;
        UpdateDisplay(current, max);
    }

    protected abstract void UpdateDisplay(int current, int max);
}
