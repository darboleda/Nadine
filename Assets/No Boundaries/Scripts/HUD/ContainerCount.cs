using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class ContainerCount : CountDisplay
{
    public Sprite[] Sprites;
    public Image[] Images;

    protected override void UpdateDisplay(int current, int max)
    {
        if (Sprites.Length < 1) return;

        for (int i = 0; i < Images.Length; i++)
        {
            int x = current - i * Sprites.Length;
            if (x > Sprites.Length)
            {
                Images[i].overrideSprite = Sprites[Sprites.Length - 1];
            }
            else if (x <= 0)
            {
                Images[i].overrideSprite = null;
            }
            else
            {
                Images[i].overrideSprite = Sprites[x - 1];
            }


            int y = max - i * Sprites.Length;
            Images[i].enabled = (y > 0);
        }
    }
}
