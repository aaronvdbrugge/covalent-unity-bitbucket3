using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Handles the UI for Emoji and sends input to the player
/// </summary>
public class Emoji_Manager : MonoBehaviour
{
    public static Emoji_Manager inst;   // easy access, so we can read emojiSettings from Player_Emotes


    [Header("References")]
    [Tooltip("Gives us every emoji sprite and its corresponding player animation.")]
    public EmojiSettings emojiSettings;

    [Tooltip("We'll tint this when opened")]
    public Image iconSprite;

    [Tooltip("We'll enable / disable this when button pushed")]
    public GameObject emojiView;

    [Tooltip("we'll instantiate emoji prefabs under this")]
    public Transform contentView;

    [Tooltip("Should have an Image and Button component")]
    public GameObject emojiPrefab;


    [Header("Settings")]
    [Tooltip("We'll multiply pixel size of the sprites to set actual size.")]
    public float sizeRatio = 0.24242424242424242424242424242424f;

    [Tooltip("Tint the icon when the menu is open.")]
    public Color menuOpenColor;

    [Header("Runtime")]
    public bool showEmotes;



    Color _iconColorOriginal;  // iconSprite.color in Start

	private void Awake()
	{
		inst = this;
	}

	// Start is called before the first frame update
	void Start()
    {
        _iconColorOriginal = iconSprite.color;

        showEmotes = true;
        toggleMenu();  // will toggle it back to false, and setup UI

        // Create a prefab for each emoji.
        foreach( var setting in emojiSettings.emojis )
        {
            GameObject go = Instantiate( emojiPrefab, contentView );
            go.GetComponent<Image>().sprite = setting.sprite;
            go.GetComponent<Button>().onClick.AddListener( delegate{ showEmote(setting.sprite); } );    // will call back showEmote with its corresponding index.
            go.GetComponent<RectTransform>().sizeDelta = new Vector2( setting.sprite.rect.width * sizeRatio, setting.sprite.rect.height * sizeRatio );
        }


    }

    public void toggleMenu()
    {
        showEmotes = !showEmotes;

        emojiView.SetActive(showEmotes);

        iconSprite.color = showEmotes ? menuOpenColor : _iconColorOriginal;
    }
     
    public void showEmote(Sprite sprite)  // called from buttons
    {
        toggleMenu();

        // We can just use sprite to find the slot...
        int i;
        for(i=0; i<emojiSettings.emojis.Length; i++)
            if( sprite == emojiSettings.emojis[i].sprite )
                break;

        Player_Controller_Mobile.mine.playerEmotes.showEmote(i);
    }

}
