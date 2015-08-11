using UnityEngine;
using System.Collections.Generic;

using SpriteTile;

[System.Serializable]
public struct TriggerReplacement
{
    public int TriggerValue;
    public Transform ReplacementPrefab;
}

[System.Serializable]
public struct LevelTriggerReplacer
{
    
    public int Layer;
    public List<TriggerReplacement> Replacements;
}

public class LevelTestLoader : MonoBehaviour {

}
