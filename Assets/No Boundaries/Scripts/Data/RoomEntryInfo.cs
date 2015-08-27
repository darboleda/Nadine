using UnityEngine;
using System.Collections;

public class RoomEntryInfo : ScriptableObject
{
    [SerializeField]
    [SceneAttribute]
    private Object scene;
    public string[] Entrances;

    public string Scene { get { return (scene != null ? scene.name : string.Empty); } }
}
