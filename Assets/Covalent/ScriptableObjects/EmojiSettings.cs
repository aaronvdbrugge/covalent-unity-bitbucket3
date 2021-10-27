using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Emoji Settings")]
public class EmojiSettings: ScriptableObject
{
	[Serializable]
	public class EmojiEntry
	{
		[Tooltip("Icon for the emoji")]
		public Sprite sprite;

		[Tooltip("Animation that gets played from this emoji.")]
		public string playerAnim;

		[Tooltip("Sound cue will be this. If left blank, we'll just do \"emoji_\"+playerAnim.ToLower()")]
		public string soundCue;
	}

	public EmojiEntry[] emojis; 
}