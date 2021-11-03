// FOUND THIS HERE:
//   https://answers.unity.com/questions/1091618/ui-panel-without-image-component-as-raycast-target.html

using UnityEngine;
 using UnityEditor;
 using UnityEditor.UI;
 
 [CanEditMultipleObjects, CustomEditor(typeof(NonDrawingGraphic), false)]
 public class NonDrawingGraphicEditor : GraphicEditor
 {
     public override void OnInspectorGUI ()
     {
         base.serializedObject.Update();
         EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
         // skipping AppearanceControlsGUI
         base.RaycastControlsGUI();
         base.serializedObject.ApplyModifiedProperties();
     }
 }