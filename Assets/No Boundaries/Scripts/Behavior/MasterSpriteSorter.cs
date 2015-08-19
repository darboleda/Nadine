using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MasterSpriteSorter : MonoBehaviour {

    private static HashSet<AutoSpriteSorter> spriteSorters = new HashSet<AutoSpriteSorter>();
    private static MasterSpriteSorter activeMaster;

	public static void RegisterSpriteSorter(AutoSpriteSorter sorter)
    {
        if (activeMaster != null) 
        {
            activeMaster.InternalRegisterSpriteSorter(sorter);
        }
        else
        {
            spriteSorters.Add(sorter);
        }
    }

    public static void UnregisterSpriteSorter(AutoSpriteSorter sorter)
    {
        if (activeMaster != null)
        {
            activeMaster.InternalUnregisterSpriteSorter(sorter);
        }
        else
        {
            spriteSorters.Remove(sorter);
        }
    }

    public int SortResolution;
    public List<AutoSpriteSorter> SpriteSorters;

    private void InternalRegisterSpriteSorter(AutoSpriteSorter sorter)
    {
        if (!SpriteSorters.Contains(sorter))
        {
            SpriteSorters.Add(sorter);
            activeMaster.UpdateSpriteSorters();
        }
    }

    private void InternalUnregisterSpriteSorter(AutoSpriteSorter sorter)
    {
        SpriteSorters.Remove(sorter);
    }

    private void OnEnable()
    {
        activeMaster = this;
        SpriteSorters.Clear();
        SpriteSorters.AddRange(spriteSorters);
        UpdateSpriteSorters();
    }

    private void OnDisable()
    {
        if (activeMaster == this)
        {
            activeMaster = null;
            foreach (AutoSpriteSorter sorter in SpriteSorters)
            {
                if (sorter == null) continue;
                spriteSorters.Add(sorter);
            }
        }
        SpriteSorters.Clear();
    }

    public void UpdateSpriteSorters()
    {
        foreach(AutoSpriteSorter sorter in spriteSorters)
        {
            if (sorter == null) continue;
            sorter.SortResolution = SortResolution;
            sorter.UpdateSortOrder();
        }
    }
}
