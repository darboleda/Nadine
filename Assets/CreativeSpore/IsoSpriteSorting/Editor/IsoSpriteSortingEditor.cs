using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CreativeSpore.SpriteSorting
{
    [CustomEditor(typeof(IsoSpriteSorting))]
    public class IsoSpriteSortingEditor : Editor
    {
        private static bool m_showStatistics = false;

        public override void OnInspectorGUI()
        {
            IsoSpriteSorting myTarget = (IsoSpriteSorting)target;

            EditorGUILayout.Space();

            m_showStatistics = EditorGUILayout.Foldout(m_showStatistics, "Show Statistics");
            if( m_showStatistics )
            {
                GUIStyle style = new GUIStyle();
                style.richText = true;
                EditorGUILayout.TextArea(myTarget.GetStatistics(), style);
                Repaint();
            }

            int iVal = PlayerPrefs.GetInt(IsoSpriteSorting.k_IntParam_ExecuteInEditMode, 1);
            bool bVal = EditorGUILayout.ToggleLeft("Execute in Edit Mode", iVal != 0, EditorStyles.boldLabel);
            PlayerPrefs.SetInt(IsoSpriteSorting.k_IntParam_ExecuteInEditMode, bVal ? 1 : 0);

            EditorGUILayout.HelpBox("Axis used for sorting.", MessageType.Info);
            myTarget.SorterAxis = (IsoSpriteSorting.eSortingAxis)EditorGUILayout.EnumPopup("Sorter Axis", myTarget.SorterAxis);

            EditorGUILayout.HelpBox("Position used for sorting, relative to gameobject position. Change it in the editor dragging the white dot square.", MessageType.Info);
            myTarget.SorterPositionOffset = EditorGUILayout.Vector3Field("Sorter Position Offset", myTarget.SorterPositionOffset);

            EditorGUILayout.HelpBox("If invalidated and IncludeInactiveRenderer is true, inactive renderers will be taking into account.", MessageType.Info);
            myTarget.IncludeInactiveRenderer = EditorGUILayout.ToggleLeft("Include Inactive Renderers", myTarget.IncludeInactiveRenderer );

            EditorGUILayout.HelpBox("Invalidate when adding or removing any renderer.\nInvalidateAll invalidates all objects.", MessageType.Info);
            if (GUILayout.Button("Invalidate")) myTarget.Invalidate();
            if (GUILayout.Button("InvalidateAll")) myTarget.InvalidateAll();

            if (GUI.changed)
            {
                SceneView.RepaintAll();
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI()
        {
            IsoSpriteSorting myTarget = (IsoSpriteSorting)target;
            myTarget.SorterPositionOffset = Handles.FreeMoveHandle(myTarget.transform.position + myTarget.SorterPositionOffset, Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotCap) - myTarget.transform.position;
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}