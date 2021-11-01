using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawned by CloudSpawner
/// </summary>
public class SpawnedCloud : MonoBehaviour
{
	[Header("References")]
	public SpriteRenderer spriteRenderer;

	[Header("Settings")]
	[Tooltip("Applies to copyOrderInLayerFrom")]
    public int orderInLayerOffset = -1;

	[Header("Runtime")]
	public float speed=0.05f;
	public float lifetime=10.0f;
	public float targetSize = 1.0f;
	public float scaleTimeNormalized=0.1f;    // percentage of lifetime
	public EasingFunction.Ease cloudScaleEase = EasingFunction.Ease.EaseOutQuad;

	[Tooltip("Every frame, we just copy order in layer form this object.")]
    public SpriteRenderer copyOrderInLayerFrom;


	public float timeAlive = 0;  // counts up to lifetime. reset this when you un-pool it


	private void LateUpdate()
	{
		transform.localPosition = (Vector2)transform.localPosition + new Vector2(speed * Time.deltaTime, 0);
		timeAlive += Time.deltaTime;

		float lerp = timeAlive / lifetime;
		if( lerp < scaleTimeNormalized )  // scale up animation
		{
			float eased = EasingFunction.GetEasingFunction(cloudScaleEase)(0, 1, lerp / scaleTimeNormalized);
			transform.localScale = Vector3.one * eased * targetSize;
		}
		else if( lerp > 1 - scaleTimeNormalized )  // scale down animation
		{
			float eased = EasingFunction.GetEasingFunction(cloudScaleEase)(0, 1, (lerp - (1 - scaleTimeNormalized)) / scaleTimeNormalized);
			transform.localScale = Vector3.one * targetSize * (1 - eased);
		}
		else  // not scaling
			transform.localScale = Vector3.one * targetSize;


		if( lerp >= 1 )   // animation finished!	
			gameObject.SetActive(false);   // allows CloudSpawner to pool us

		// Copy sorting from another object
		spriteRenderer.sortingOrder = copyOrderInLayerFrom.sortingOrder + orderInLayerOffset;
	}
}
