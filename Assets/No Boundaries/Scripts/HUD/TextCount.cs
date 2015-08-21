using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextCount : CountDisplay
{
    public Text Text;
    public int MinDigits;
    public int DisplayMax;

    protected override void UpdateDisplay(int current, int max)
    {

        int value = Mathf.Min(current, DisplayMax);
        Text.text = value.ToString("D" + MinDigits);

    }
}
