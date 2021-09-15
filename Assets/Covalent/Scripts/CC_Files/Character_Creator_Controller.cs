using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
/*
 * Alrighty this is our main Character Creator Class. Given a SkeletonMechanim it is able to change the color of all pertanent
 * slots and attachments there are to our Spine character. This will also be extended later to include a way to handle full skin changes in the future.
 * 
 * 
 * 
 * 
 */
public class Character_Creator_Controller : MonoBehaviour
{
    //All the colors that we want the player to use. For our case we need both the Color and the "Base" Color.
    //The base color is the right end of the gradient while the Color is the left end of the gradient.
    #region Colors
    Color32 dark_brown_color = new Color32(67, 46, 38, 255);
    Color32 base_dark_brown_color = new Color32(219, 198, 189, 255);

    Color32 dark_brown_color_2 = new Color32(101, 64, 49, 255);
    Color32 base_dark_brown_color_2 = new Color32(251, 214, 199, 255);

    Color32 mid_brown_color = new Color32(125, 85, 59, 255);
    Color32 base_mid_brown_color = new Color32(255, 226, 200, 255);

    Color32 mid_brown_color_2 = new Color32(149, 100, 69, 255);
    Color32 base_mid_brown_color_2 = new Color32(255, 255, 253, 255);

    Color32 light_brown_color = new Color32(184, 129, 95, 255);
    Color32 base_light_brown_color = new Color32(255, 255, 228, 255);

    Color32 light_brown_color_2 = new Color32(222, 165, 129, 255);
    Color32 base_light_brown_color_2 = new Color32(255, 255, 255, 255);

    Color32 tan_color = new Color32(240, 185, 150, 255);
    Color32 base_tan_color = new Color32(255, 255, 255, 255);

    Color32 tan_color_2 = new Color32(248, 202, 171, 255);
    Color32 base_tan_color_2 = new Color32(255, 255, 255, 255);

    Color32 tan_color_3 = new Color32(251, 211, 185, 255);
    Color32 base_tan_color_3 = new Color32(255, 255, 255, 255);

    Color32 grey_color = new Color32(177, 174, 174, 255);
    Color32 base_grey_color = new Color32(255, 255, 255, 255);

    Color32 red_color = new Color32(214, 50, 22, 255);
    Color32 base_red_color = new Color32(255, 231, 203, 255);

    Color32 orange_color = new Color32(243, 131, 2, 255);
    Color32 base_orange_color = new Color32(255, 255, 184, 255);

    Color32 yellow_color = new Color32(255, 208, 59, 255);
    Color32 base_yellow_color = new Color32(255, 255, 255, 255);

    Color32 green_color = new Color32(115, 187, 37, 255);
    Color32 base_green_color = new Color32(255, 255, 255, 255);

    Color32 blue_color = new Color32(86, 151, 255, 255);
    Color32 base_blue_color = new Color32(255, 255, 255, 255);

    Color32 purple_color = new Color32(150, 115, 242, 255);
    Color32 base_purple_color = new Color32(255, 255, 255, 255);

    Color32 pink_color = new Color32(237, 108, 166, 255);
    Color32 base_pink_color = new Color32(255, 255, 255, 255);
    #endregion

    //All our Skin related variables. Keeps track of all our skins and where we are in the array so we can swap out easily.
    #region Skin Variables
    Skin characterSkin;
    [SpineSkin] string baseSkin = "Bareskin";

    Dictionary<int, string> full_skins;
    int fullSkinCount = 0;
    public int currentFullSkinIndex = 0;    //this had a compiler warning. just made it public to avoid that. -seb
    Color32 currentSkinColor = new Color32(255, 255, 255, 255);

    Dictionary<int, string> bottoms_skins;
    int bottomsSkinCount = 0;
    int currentBottomIndex = 0;
    Color32 currentBottomColor = new Color32(255, 255, 255, 255);

    Dictionary<int, string> tops_skins;
    int topsSkinCount = 0;
    int currentTopIndex = 0;
    Color32 currentTopColor = new Color32(255, 255, 255, 255);

    Dictionary<int, string> shoes_skins;
    int shoesSkinCount = 0;
    int currentShoeIndex = 0;
    Color32 currentShoeColor = new Color32(255, 255, 255, 255);

    Dictionary<int, string> hair_skins;
    int hairSkinCount = 0;
    int currentHairIndex = 0;
    Color32 currentHairColor = new Color32(255, 255, 255, 255);
    #endregion

    //0 = Skin  1 = Hair  2 = Top  3 = Bottom  4 = Shoes
    //Used to know what part of the Character to change the color of
    int currentBodyIndex = 0;

    //Variables
    Slider slider;
    public SkeletonMecanim skeletonMechanim;
    public Material material;
    public Color32 currentColor, baseColor; 

    

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        full_skins = new Dictionary<int, string>();
        hair_skins = new Dictionary<int, string>();
        tops_skins = new Dictionary<int, string>();
        bottoms_skins = new Dictionary<int, string>();
        shoes_skins = new Dictionary<int, string>();
        //EventManager.StartListening("init_char_creator", Init_Character_Creator);
        Init_Character_Creator();

    }
    public void Init_Character_Creator()
    {
        foreach (Skin s in skeletonMechanim.skeleton.Data.Skins)
        {
            if (s.Name.Contains("Full Skins"))
            {
                full_skins.Add(fullSkinCount, s.Name);
                fullSkinCount++;
            }
            else if (s.Name.Contains("Bottoms"))
            {
                bottoms_skins.Add(bottomsSkinCount, s.Name);
                bottomsSkinCount++;
            }
            else if (s.Name.Contains("Tops"))
            {
                tops_skins.Add(topsSkinCount, s.Name);
                topsSkinCount++;
            }
            else if (s.Name.Contains("Shoes"))
            {
                shoes_skins.Add(shoesSkinCount, s.Name);
                shoesSkinCount++;
            }
            else if (s.Name.Contains("Hair"))
            {
                hair_skins.Add(hairSkinCount, s.Name);
                hairSkinCount++;
            }
        }
        ChangeSkin();
    }
    public void updateGradientColor(int color)
    {
        if (color == 0)
        {
            material.SetColor("_GradientColor", dark_brown_color);
            currentColor = dark_brown_color;
            baseColor = base_dark_brown_color;
        }
        else if (color == 1)
        {
            material.SetColor("_GradientColor", dark_brown_color_2);
            currentColor = dark_brown_color_2;
            baseColor = base_dark_brown_color_2;
        }
        else if (color == 2)
        {
            material.SetColor("_GradientColor", mid_brown_color);
            currentColor = mid_brown_color;
            baseColor = base_mid_brown_color;
        }
        else if (color == 3)
        {
            material.SetColor("_GradientColor", mid_brown_color_2);
            currentColor = mid_brown_color_2;
            baseColor = base_mid_brown_color_2;
        }
        else if (color == 4)
        {
            material.SetColor("_GradientColor", light_brown_color);
            currentColor = light_brown_color;
            baseColor = base_light_brown_color;
        }
        else if (color == 5)
        {
            material.SetColor("_GradientColor", light_brown_color_2);
            currentColor = light_brown_color_2;
            baseColor = base_light_brown_color_2;
        }
        else if (color == 6)
        {
            material.SetColor("_GradientColor", tan_color);
            currentColor = tan_color;
            baseColor = base_tan_color;
        }
        else if (color == 7)
        {
            material.SetColor("_GradientColor", tan_color_2);
            currentColor = tan_color_2;
            baseColor = base_tan_color_2;
        }
        else if (color == 8)
        {
            material.SetColor("_GradientColor", tan_color_3);
            currentColor = tan_color_3;
            baseColor = base_tan_color_3;
        }
        else if (color == 9)
        {
            material.SetColor("_GradientColor", grey_color);
            currentColor = grey_color;
            baseColor = base_grey_color;
        }
        else if (color == 10)
        {
            material.SetColor("_GradientColor", red_color);
            currentColor = red_color;
            baseColor = base_red_color;
        }
        else if (color == 11)
        {
            material.SetColor("_GradientColor", orange_color);
            currentColor = orange_color;
            baseColor = base_orange_color;
        }
        else if (color == 12)
        {
            material.SetColor("_GradientColor", yellow_color);
            currentColor = yellow_color;
            baseColor = base_yellow_color;
        }
        else if (color == 13)
        {
            material.SetColor("_GradientColor", green_color);
            currentColor = green_color;
            baseColor = base_green_color;
        }
        else if (color == 14)
        {
            material.SetColor("_GradientColor", blue_color);
            currentColor = blue_color;
            baseColor = base_blue_color;
        }
        else if (color == 15)
        {
            material.SetColor("_GradientColor", purple_color);
            currentColor = purple_color;
            baseColor = base_purple_color;
        }
        else if (color == 16)
        {
            material.SetColor("_GradientColor", pink_color);
            currentColor = pink_color;
            baseColor = base_pink_color;
        }

        updateColor();
    }

    public void updateColor()
    {
        if (currentBodyIndex == 0) { updateSkinColor(); }
        else if (currentBodyIndex == 1) { updateHairColor(); }
        else if (currentBodyIndex == 2) { updateTopColor(); }
        else if (currentBodyIndex == 3) { updateBottomColor(); }
        else if (currentBodyIndex == 4) { updateShoeColor(); }
    }

    public void updateSkinColor()
    {
        Color32 newColor = Color32.Lerp(currentColor, baseColor, slider.normalizedValue);
        skeletonMechanim.skeleton.FindSlot("head_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("body_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftarm_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("lefthand_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftfoot_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftleg_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("rightarm_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("righthand_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("rightfoot_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("rightleg_skin").SetColor(newColor);

        currentSkinColor = newColor;
    }
    public void updateSkinColor(Color32 newColor)
    {
        skeletonMechanim.skeleton.FindSlot("head_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("body_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftarm_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("lefthand_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftfoot_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftleg_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("rightarm_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("righthand_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("rightfoot_skin").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("rightleg_skin").SetColor(newColor);
    }

    public void updateHairColor()
    {
        Color32 newColor = Color32.Lerp(currentColor, baseColor, slider.normalizedValue);
        Slot hair = skeletonMechanim.skeleton.FindSlot("hair");
        Slot back_hair = skeletonMechanim.skeleton.FindSlot("hair_back");
        Slot ponytail = skeletonMechanim.skeleton.FindSlot("ponytail");
        Slot ponytail2 = skeletonMechanim.skeleton.FindSlot("ponytail2");
        if (hair != null)
        {
            hair.SetColor(newColor);
        }
        if (back_hair != null)
        {
            back_hair.SetColor(newColor);
        }
        if (ponytail != null)
        {
            ponytail.SetColor(newColor);
        }
        if (ponytail2 != null)
        {
            ponytail2.SetColor(newColor);
        }
        currentHairColor = newColor;
    }
    public void updateHairColor(Color32 newColor)
    {
        Slot hair = skeletonMechanim.skeleton.FindSlot("hair");
        Slot back_hair = skeletonMechanim.skeleton.FindSlot("hair_back");
        Slot ponytail = skeletonMechanim.skeleton.FindSlot("ponytail");
        Slot ponytail2 = skeletonMechanim.skeleton.FindSlot("ponytail2");
        if (hair != null)
        {
            hair.SetColor(newColor);
        }
        if (back_hair != null)
        {
            back_hair.SetColor(newColor);
        }
        if (ponytail != null)
        {
            ponytail.SetColor(newColor);
        }
        if (ponytail2 != null)
        {
            ponytail2.SetColor(newColor);
        }
    }
    public void updateTopColor()
    {
        Color32 newColor = Color32.Lerp(currentColor, baseColor, slider.normalizedValue);
        Slot body = skeletonMechanim.skeleton.FindSlot("bodyShirt");
        Slot rightSleeve = skeletonMechanim.skeleton.FindSlot("rightarmSleeve");
        Slot leftSleeve = skeletonMechanim.skeleton.FindSlot("leftarmSleeve");
        Slot rightSleeveShort = skeletonMechanim.skeleton.FindSlot("rightarmSleeveShort");
        Slot leftSleeveShort = skeletonMechanim.skeleton.FindSlot("leftarmSleeveShort");
        if (body != null)
        {
            body.SetColor(newColor);
        }
        if (rightSleeve != null)
        {
            rightSleeve.SetColor(newColor);
        }
        if (leftSleeve != null)
        {
            leftSleeve.SetColor(newColor);
        }
        if (rightSleeveShort != null)
        {
            rightSleeveShort.SetColor(newColor);
        }
        if (leftSleeveShort != null)
        {
            leftSleeveShort.SetColor(newColor);
        }
        currentTopColor = newColor;
    }
    public void updateTopColor(Color32 newColor)
    {
        Slot body = skeletonMechanim.skeleton.FindSlot("bodyShirt");
        Slot rightSleeve = skeletonMechanim.skeleton.FindSlot("rightarmSleeve");
        Slot leftSleeve = skeletonMechanim.skeleton.FindSlot("leftarmSleeve");
        Slot rightSleeveShort = skeletonMechanim.skeleton.FindSlot("rightarmSleeveShort");
        Slot leftSleeveShort = skeletonMechanim.skeleton.FindSlot("leftarmSleeveShort");
        if (body != null)
        {
            body.SetColor(newColor);
        }
        if (rightSleeve != null)
        {
            rightSleeve.SetColor(newColor);
        }
        if (leftSleeve != null)
        {
            leftSleeve.SetColor(newColor);
        }
        if (rightSleeveShort != null)
        {
            rightSleeveShort.SetColor(newColor);
        }
        if (leftSleeveShort != null)
        {
            leftSleeveShort.SetColor(newColor);
        }
    }
    public void updateBottomColor()
    {
        Color32 newColor = Color32.Lerp(currentColor, baseColor, slider.normalizedValue);
        Slot hips = skeletonMechanim.skeleton.FindSlot("hips");
        Slot rightPant = skeletonMechanim.skeleton.FindSlot("rightlegPant");
        Slot leftPant = skeletonMechanim.skeleton.FindSlot("leftlegPant");
        Slot rightShort = skeletonMechanim.skeleton.FindSlot("rightlegShort");
        Slot leftShort = skeletonMechanim.skeleton.FindSlot("leftlegShort");
        if (hips != null)
        {
            hips.SetColor(newColor);
        }
        if (rightPant != null)
        {
            rightPant.SetColor(newColor);
        }
        if (leftPant != null)
        {
            leftPant.SetColor(newColor);
        }
        if (rightShort != null)
        {
            rightShort.SetColor(newColor);
        }
        if (leftShort != null)
        {
            leftShort.SetColor(newColor);
        }

        currentBottomColor = newColor;
    }
    public void updateBottomColor(Color32 newColor)
    {
        Slot hips = skeletonMechanim.skeleton.FindSlot("hips");
        Slot rightPant = skeletonMechanim.skeleton.FindSlot("rightlegPant");
        Slot leftPant = skeletonMechanim.skeleton.FindSlot("leftlegPant");
        Slot rightShort = skeletonMechanim.skeleton.FindSlot("rightlegShort");
        Slot leftShort = skeletonMechanim.skeleton.FindSlot("leftlegShort");
        if (hips != null)
        {
            hips.SetColor(newColor);
        }
        if (rightPant != null)
        {
            rightPant.SetColor(newColor);
        }
        if (leftPant != null)
        {
            leftPant.SetColor(newColor);
        }
        if (rightShort != null)
        {
            rightShort.SetColor(newColor);
        }
        if (leftShort != null)
        {
            leftShort.SetColor(newColor);
        }
    }
    public void updateShoeColor()
    {
        Color32 newColor = Color32.Lerp(currentColor, baseColor, slider.normalizedValue);
        skeletonMechanim.skeleton.FindSlot("rightShoe").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftShoe").SetColor(newColor);
        currentShoeColor = newColor;
    }
    public void updateShoeColor(Color32 newColor)
    {
        skeletonMechanim.skeleton.FindSlot("rightShoe").SetColor(newColor);
        skeletonMechanim.skeleton.FindSlot("leftShoe").SetColor(newColor);
    }

    public void ChangeSkin()
    {
        var skeleton = skeletonMechanim.skeleton;
        var skeletonData = skeleton.Data;

        characterSkin = new Skin("character-base");
        characterSkin.AddSkin(skeletonData.FindSkin(baseSkin));
        characterSkin.AddSkin(skeletonData.FindSkin(hair_skins[currentHairIndex]));
        characterSkin.AddSkin(skeletonData.FindSkin(tops_skins[currentTopIndex]));
        characterSkin.AddSkin(skeletonData.FindSkin(bottoms_skins[currentBottomIndex]));
        characterSkin.AddSkin(skeletonData.FindSkin(shoes_skins[currentShoeIndex]));

        skeleton.SetSkin(characterSkin);
        skeleton.SetSlotsToSetupPose();

        updateSkinColor(currentSkinColor);
        updateHairColor(currentHairColor);
        updateTopColor(currentTopColor);
        updateBottomColor(currentBottomColor);
        updateShoeColor(currentShoeColor);
    }

    public void SetCurrentBodyIndex(int index)
    {
        currentBodyIndex = index;
    }

    public void Next()
    {
        if (currentBodyIndex == 0) { }
        else if (currentBodyIndex == 1) { NextHair(); }
        else if (currentBodyIndex == 2) { NextTop(); }
        else if (currentBodyIndex == 3) { NextBottom(); }
        else if (currentBodyIndex == 4) { NextShoe(); }
    }
    public void Previous()
    {
        if (currentBodyIndex == 0) { }
        else if (currentBodyIndex == 1) { PreviousHair(); }
        else if (currentBodyIndex == 2) { PreviousTop(); }
        else if (currentBodyIndex == 3) { PreviousBottom(); }
        else if (currentBodyIndex == 4) { PreviousShoe(); }
    }

    private void NextHair()
    {
        if (currentHairIndex == hairSkinCount)
        {
            currentHairIndex = 0;
        }
        else
        {
            currentHairIndex++;
        }
        ChangeSkin();
    }
    private void PreviousHair()
    {
        if (currentHairIndex == 0)
        {
            currentHairIndex = hairSkinCount - 1;
        }
        else
        {
            currentHairIndex--;
        }
        ChangeSkin();
    }
    private void NextTop()
    {
        if (currentTopIndex == topsSkinCount)
        {
            currentTopIndex = 0;
        }
        else
        {
            currentTopIndex++;
        }
        ChangeSkin();
    }
    private void PreviousTop()
    {
        if (currentTopIndex == 0)
        {
            currentTopIndex = topsSkinCount - 1;
        }
        else
        {
            currentTopIndex--;
        }
        ChangeSkin();
    }
    private void NextBottom()
    {
        if (currentBottomIndex == bottomsSkinCount)
        {
            currentBottomIndex = 0;
        }
        else
        {
            currentBottomIndex++;
        }
        ChangeSkin();
    }
    private void PreviousBottom()
    {
        if (currentBottomIndex == 0)
        {
            currentBottomIndex = bottomsSkinCount - 1;
        }
        else
        {
            currentBottomIndex--;
        }
        ChangeSkin();
    }
    private void NextShoe()
    {
        if (currentShoeIndex == shoesSkinCount)
        {
            currentShoeIndex = 0;
        }
        else
        {
            currentShoeIndex++;
        }
        ChangeSkin();
    }
    private void PreviousShoe()
    {
        if (currentShoeIndex == 0)
        {
            currentShoeIndex = shoesSkinCount - 1;
        }
        else
        {
            currentShoeIndex--;
        }
        ChangeSkin();
    }

}
