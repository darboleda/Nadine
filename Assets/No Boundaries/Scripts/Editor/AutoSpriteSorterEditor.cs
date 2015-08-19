using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AutoSpriteSorter))]
public class AutoSpriteSorterEditor : Editor {

	public override void OnInspectorGUI()
    {
        AutoSpriteSorter sorter = target as AutoSpriteSorter;

        if (GUILayout.Button("Add Sort Orders to Renderers"))
        {
            foreach (Renderer renderer in sorter.GetComponentsInChildren<Renderer>(true))
            {
                AutoSpriteSortOrder sprite = renderer.GetComponent<AutoSpriteSortOrder>() ?? renderer.gameObject.AddComponent<AutoSpriteSortOrder>();
            }
        }

        if (GUILayout.Button("Find Sort Orders In Children"))
        {
            if (sorter.SpritesToSort == null)
            {
                sorter.SpritesToSort = new List<AutoSpriteSortOrder>();
            }
            else
            {
                sorter.SpritesToSort.Clear();
            }

            sorter.SpritesToSort.AddRange(sorter.GetComponentsInChildren<AutoSpriteSortOrder>(true));
        }

        if (GUILayout.Button("Reset Sort Orders"))
        {
            sorter.SetSortOrder(0);
        }

        base.OnInspectorGUI();
    }
}
