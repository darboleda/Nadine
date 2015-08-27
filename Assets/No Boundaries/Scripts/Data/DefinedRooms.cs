using UnityEngine;
using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field)]
public class SceneAttribute : PropertyAttribute
{

}

public class DefinedRooms : ScriptableObject
{
    [HideInInspector]
    [Scene]
    public List<UnityEngine.Object> Scenes;
}
