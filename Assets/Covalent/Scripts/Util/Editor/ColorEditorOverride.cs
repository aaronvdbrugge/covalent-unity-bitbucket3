using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Color))]
public class ColorEditorOverride : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Debug.Log($"Hallo");
		EditorGUI.BeginProperty(position, label, property);
		position.width -= 80f;
		EditorGUI.PropertyField(position, property, label);
		Color color = property.colorValue;
		var newP = new Rect(position);
		newP.x += newP.width;
		newP.width = 80f;
		string hex = EditorGUI.TextField(newP, ColorUtility.ToHtmlStringRGBA(color));
		if (ColorUtility.TryParseHtmlString("#" + hex, out Color newColor))
		{
			if (color != newColor)
			{
				property.colorValue = newColor;
			}
		}

		EditorGUI.EndProperty();
	}
}