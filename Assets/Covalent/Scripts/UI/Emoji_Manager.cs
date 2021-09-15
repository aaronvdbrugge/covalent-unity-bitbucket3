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
    public Image[] emojiButtonImages;
    public Sprite[] emojiUI_Icons;
    private Image mySprite;
    public Button[] emojiButtons;
    public bool showEmotes;
    public Player_Emotes playerEmotes;
    public int emotes = 0;
    // Start is called before the first frame update
    void Start()
    {
        mySprite = GetComponent<Image>();
        emojiButtonImages = GetComponentsInChildren<Image>();
        emojiButtons = GetComponentsInChildren<Button>();
        showEmotes = false;
    }

    public void toggleMenu()
    {
        if (!showEmotes)
        {
            for (int i = 1; i < emojiButtons.Length; i++)
            {
                emojiButtonImages[i].enabled = true;
                emojiButtons[i].interactable = true;
            }
            showEmotes = true;
            mySprite.sprite = emojiUI_Icons[1];
        }
        else
        {
            for (int i = 1; i < emojiButtons.Length; i++)
            {
                emojiButtonImages[i].enabled = false;
                emojiButtons[i].interactable = false;
            }
            showEmotes = false;
            mySprite.sprite = emojiUI_Icons[0];
        }
    }

    public void showEmote(int slot)
    {
        toggleMenu();
        playerEmotes.showEmote(slot);
    }

}
