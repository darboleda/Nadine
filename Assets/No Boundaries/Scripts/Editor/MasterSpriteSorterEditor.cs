using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MasterSpriteSorter))]
public class MasterSpriteSorterEditor : Editor {

	public override void OnInspectorGUI()
    {
        MasterSpriteSorter sorter = target as MasterSpriteSorter;
        if (GUILayout.Button("Update sort orders"))
        {
            sorter.UpdateSpriteSorters();
        }

        base.OnInspectorGUI();
    }
}
