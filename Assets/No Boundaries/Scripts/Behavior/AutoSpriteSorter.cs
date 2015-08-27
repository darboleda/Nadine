using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AutoSpriteSorter : MonoBehaviour
{
    public Vector3 Offset;
    public List<AutoSpriteSortOrder> SpritesToSort;

    [HideInInspector]
    public int SortResolution = 16;

    public void OnEnable()
    {
        MasterSpriteSorter.RegisterSpriteSorter(this);
    }

    public void OnDisable()
    {
        MasterSpriteSorter.UnregisterSpriteSorter(this);
    }

    public void Update()
    {
        UpdateSortOrder();
    }

    public void UpdateSortOrder()
    {
        if (!this.enabled) return;

        int sortOrder = -(int)((transform.position.y + Offset.y) * SortResolution);
        //Debug.LogFormat("{0} {1}", name, sortOrder);
        SetSortOrder(sortOrder);
    }

    public void SetSortOrder(int sortOrder)
    {
        if (SpritesToSort != null)
        {
            SpritesToSort.RemoveAll(x => x == null);
            foreach (AutoSpriteSortOrder sprite in SpritesToSort)
            {
                sprite.SetOrder(sortOrder);
            }
        }
    }
}
