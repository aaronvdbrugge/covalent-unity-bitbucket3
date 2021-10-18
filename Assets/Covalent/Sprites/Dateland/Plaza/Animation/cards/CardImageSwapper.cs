using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// I just wrote this class because it didn't make sense to create new animation clips,
/// then set up and maintain 36 different animation states in the Animator for cards.
/// Instead, we'll catch the swappable frames in LateUpdate, and change them out for a
/// different frame, depending on whichi frame is selected.
/// 
/// This class is general purpose and could be used for other things besides the cards in the
/// future, if we run into a similar situation. See "_Note on card animations.txt" for more
/// info on why this class was necessary.
/// </summary>
public class CardImageSwapper : MonoBehaviour
{
    [Tooltip("If sprite name matches this, we won't waste time doing any processing...")]
    public string idleFrameName;

	[Tooltip("We'll search for this in the sprite's name...")]
	public string findThis = "balloon";

	[Tooltip("...And replace it with this instead")]
	public string replaceWithThis = "balloon";

	[Tooltip("These frames names will not get replacement done. (NOTE: cached to a HashSet on Awake)")]
	public string[] ignoreTheseFrames;

    [Tooltip("Dump all the sprite subrects in here. We'll sort them out...")]
    public Sprite[] sprites;

	[Tooltip("We'll act on this SpriteRenderer as it's animated.")]
	public SpriteRenderer spriteToSwap;


	/// <summary>
	/// Allows us to retrieve the sprites we need by name, to swap them out with an existing one in the animation.
	/// </summary>
	Dictionary<string, Sprite> _spritesByName = new Dictionary<string, Sprite>();

	/// <summary>
	/// More efficient way of storing ignoreTheseFrames
	/// </summary>
	HashSet<string> _ignoreTheseFramesSet = new HashSet<string>();



	private void Awake()
	{
		foreach( Sprite spr in sprites )
			_spritesByName[spr.name] = spr;
		foreach( string str in ignoreTheseFrames )
			_ignoreTheseFramesSet.Add(str);
	}



	private void LateUpdate()
	{
		string curname = spriteToSwap.sprite.name;
		if( curname != idleFrameName && !_ignoreTheseFramesSet.Contains(curname) )    // It's doing an animation, so search the frame names for our target replacement
		{
			string newname = curname.Replace(findThis, replaceWithThis);
			if( _spritesByName.ContainsKey( newname ) )   // we have a replacement! do it
				spriteToSwap.sprite = _spritesByName[newname];
		}


	}


}
