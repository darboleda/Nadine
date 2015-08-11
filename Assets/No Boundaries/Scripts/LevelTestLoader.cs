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

    public TextAsset Level;
    public Camera MainCamera;
    public List<int> LayersToTurnOff;
    public List<LevelTriggerReplacer> Replacements;


    public void Awake()
    {
        Tile.SetCamera(MainCamera);
        Tile.LoadLevel(Level);
        
        foreach (int i in LayersToTurnOff)
        {
            Tile.SetLayerActive(i, false);
        }

        foreach (LevelTriggerReplacer replacer in Replacements)
        {
            foreach (TriggerReplacement replacement in replacer.Replacements)
            {
                List<Int2> positions = new List<Int2>();
                Tile.GetTriggerPositions(replacement.TriggerValue, replacer.Layer, ref positions);
                foreach (Int2 position in positions)
                {
                    Vector3 world = Tile.MapToWorldPosition(position);
                    Transform transform = GameObject.Instantiate(replacement.ReplacementPrefab);
                    transform.position = world;
                    Tile.DeleteTile(position, replacer.Layer, true);
                }
            }
        }
    }
}
