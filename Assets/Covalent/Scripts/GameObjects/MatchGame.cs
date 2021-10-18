using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the game where you try to match two cards.
/// </summary>
public class MatchGame : MonoBehaviour
{
	[Tooltip("We'll create these on Start")]
	public GameObject cardPrefab;

	public int cardsWide = 6;
	public int cardsHigh = 6;

	[Tooltip("Isometric offset for a card that adds 1 to x index")]
	public Vector2 cardOffsetX;
	[Tooltip("Isometric offset for a card that adds 1 to y index")]
	public Vector2 cardOffsetY;


	/// <summary>
	/// The sprite sorting script needs "movable" to be on for a frame to work properly
	/// </summary>
	public void Start()
	{
		for( int x=0; x<cardsWide; x++)
			for( int y=0; y<cardsHigh; y++)
			{
				GameObject go = Instantiate( cardPrefab, transform );
				go.transform.localPosition = cardOffsetX * x + cardOffsetY * y;
				go.GetComponent<IsoSpriteSorting>().isMovable= true;    // fixed sprite sorting issue
			}
	}


}
