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
    [Tooltip("Tint the icon when the menu is open.")]
    public Color menuOpenColor;

    public bool showEmotes;
    public Player_Emotes playerEmotes;

    public Image iconSprite;

    public Button[] _emojiButtons;


    Color _iconColorOriginal;

    // Start is called before the first frame update
    void Start()
    {
        _iconColorOriginal = iconSprite.color;

        _emojiButtons = this.GetComponentsInChildrenOnly<Button>();  // ignores bg and icon

        showEmotes = true;
        toggleMenu();  // will toggle it back to false, and setup UI
    }

    public void toggleMenu()
    {
        if (!showEmotes)   // change to shown
        {
            for (int i = 0; i < _emojiButtons.Length; i++)
                _emojiButtons[i].gameObject.SetActive(true);
            showEmotes = true;
            iconSprite.color = menuOpenColor;
        }
        else   // change to not shown
        {
            for (int i = 0; i < _emojiButtons.Length; i++)
                _emojiButtons[i].gameObject.SetActive(false);
            showEmotes = false;
            iconSprite.color = _iconColorOriginal;
        }
    }
     
    public void showEmote(int slot)  // called from buttons
    {
        toggleMenu();
        playerEmotes.showEmote(slot);
    }

}
