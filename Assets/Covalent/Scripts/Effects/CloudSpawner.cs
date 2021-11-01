using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
	[Tooltip("Width/height of spawn area, our transform is lower left corner")]
    public Vector2 spawnArea;



	[Tooltip("Rather than sprite-sort each cloud (potentially slow and hard to get right), we'll just nestle it behind the rainbow.")]
	public SpriteRenderer copyOrderInLayerFrom;

	[Tooltip("We'll pool them just by disabling them.")]
	public SpawnedCloud cloudPrefab;

	[Tooltip("All clouds will go one direction")]
	public float minCloudSpeed=0.05f;
	public float maxCloudSpeed=0.1f;

	public float minCloudSize=0.5f;
	public float maxCloudSize=1.0f;

	public float minCloudLifetime=10.0f;
	public float maxCloudLifetime=30.0f;

	public float cloudSpawnInterval = 5.0f;    // spawn clouds at regular intervals.


	[Tooltip("Scales in/out. Percentage of lifetime.")]
	public float cloudScaleTime = 0.2f;

	public EasingFunction.Ease cloudScaleEase = EasingFunction.Ease.EaseOutQuad;


	List<SpawnedCloud> clouds = new List<SpawnedCloud>();   // clouds we've spawned. Includes disabled pooled ones
	float _spawnTimer = 0;
	public Vector2 GetWorldCoordFromSpawnAreaNormalized( Vector2 normalized )
	{
		// Gets a world point in the spawn area from normalized coordinate...
		Vector2 local = normalized * spawnArea;
		return transform.localToWorldMatrix.MultiplyPoint( local );
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;

		Vector2[] corners = new Vector2[]{
			GetWorldCoordFromSpawnAreaNormalized( new Vector2(0,0) ),
			GetWorldCoordFromSpawnAreaNormalized( new Vector2(1,0) ),
			GetWorldCoordFromSpawnAreaNormalized( new Vector2(1,1) ),
			GetWorldCoordFromSpawnAreaNormalized( new Vector2(0,1) )};

		Gizmos.DrawLine( corners[0], corners[1] );
		Gizmos.DrawLine( corners[1], corners[2] );
		Gizmos.DrawLine( corners[2], corners[3] );
		Gizmos.DrawLine( corners[3], corners[0] );
	}

	/// <summary>
	/// Spawns or un-pools a cloud
	/// </summary>
	SpawnedCloud SpawnCloud()
	{
		foreach( SpawnedCloud sc in clouds )
			if( !sc.gameObject.activeInHierarchy )   // found an inactive, pooled one
			{
				sc.gameObject.SetActive(true);
				return sc;
			}

		// No pooled ones, need to make a new one
		SpawnedCloud cloud = Instantiate( cloudPrefab, transform ).GetComponent<SpawnedCloud>();
		clouds.Add(cloud);
		return cloud;
	}

	
	private void Update()
	{
		_spawnTimer -= Time.deltaTime;
		if( _spawnTimer <= 0 )
		{
			_spawnTimer = cloudSpawnInterval;
			
			SpawnedCloud cloud = SpawnCloud();
			cloud.transform.position = GetWorldCoordFromSpawnAreaNormalized( new Vector2( Random.value, Random.value ) );  // pick random point in our spawn range
			cloud.speed = Random.Range( minCloudSpeed, maxCloudSpeed );
			cloud.lifetime = Random.Range( minCloudLifetime, maxCloudLifetime );
			cloud.targetSize = Random.Range( minCloudSize, maxCloudSize );
			cloud.cloudScaleEase = cloudScaleEase;
			cloud.scaleTimeNormalized = cloudScaleTime;
			cloud.copyOrderInLayerFrom = copyOrderInLayerFrom;
			cloud.timeAlive = 0;   // in case it was un-pooled
		}
	}


}
