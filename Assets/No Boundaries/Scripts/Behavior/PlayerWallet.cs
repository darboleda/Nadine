using UnityEngine;
using System;

public class PlayerWallet : Collector<Bauble> {

    public NadineCollectionAnimation CollectionAnimation;

    public event Action<int, int> AmountChanged;

    public int Current;
    public int Max;

    protected override void Collect(Bauble bauble)
    {
        Current = Mathf.Min(Max, Current + bauble.Value);
        if (AmountChanged != null)
        {
            AmountChanged(Current, Max);
        }

        if (CollectionAnimation != null)
        {
            CollectionAnimation.Display(bauble);
        }
        else
        {
            base.Collect(bauble);
        }
    }
}
