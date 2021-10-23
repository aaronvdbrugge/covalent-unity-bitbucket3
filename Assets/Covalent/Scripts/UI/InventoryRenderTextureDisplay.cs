using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// We just need to size the render texture properly for the RawImage we're displaying it on.
/// </summary>
public class InventoryRenderTextureDisplay : MonoBehaviour
{
	public RenderTexture renderTexture;

	private void Start()
	{
		RectTransform rt = GetComponent<RectTransform>();
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		float rect_width = corners[2].x - corners[0].x;
		float rect_height = corners[2].y - corners[0].y;



		renderTexture.Release();
		renderTexture.width = Screen.width;
		renderTexture.height = (int)(Screen.width * rect_height / rect_width);
		renderTexture.Create();
	}
}
