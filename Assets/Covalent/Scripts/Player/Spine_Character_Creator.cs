using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class Spine_Character_Creator : MonoBehaviour
{
    public SkeletonMecanim skeletonMechanim;

    Skin characterSkin;
    Color hairColor;

    [SpineSkin] public string baseSkin = "Bareskin";

    [SpineSkin] public string[] hairSkins = new string[27];
    [SpineSkin] public string[] hair_back_Skins = new string[8];
    public int activeHairIndex = 0;


    void Awake()
    {
        skeletonMechanim = GetComponent<SkeletonMecanim>();
        for (int i = 1; i < 28; i++)
        {
            hairSkins[i - 1] = "Hair/Hair" + i;
        }
        //2,3,7,9,11,14,15,25
        hair_back_Skins[0] = "hair_back/backhair2";
        hair_back_Skins[1] = "hair_back/backhair3";
        hair_back_Skins[2] = "hair_back/backhair7";
        hair_back_Skins[3] = "hair_back/backhair9";
        hair_back_Skins[4] = "hair_back/backhair11";
        hair_back_Skins[5] = "hair_back/backhair14";
        hair_back_Skins[6] = "hair_back/backhair15";
        hair_back_Skins[7] = "hair_back/backhair25";

    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterSkin();
        UpdateColor(Color.red);
    }

    void UpdateCharacterSkin()
    {
        var skeleton = skeletonMechanim.skeleton;
        var skeletonData = skeleton.Data;

        characterSkin = new Skin("character-base");
        characterSkin.AddSkin(skeletonData.FindSkin(baseSkin));

        Skin hair = skeletonData.FindSkin(hairSkins[activeHairIndex]);
        characterSkin.AddSkin(hair);

        skeleton.SetSkin(characterSkin);
        skeleton.SetSlotsToSetupPose();
    }

    public void UpdateColor(Color c)
    {
        skeletonMechanim.skeleton.FindSlot("hair").SetColor(c);
        if (backHair(activeHairIndex))
        {
            skeletonMechanim.skeleton.FindSlot("hair_back").SetColor(c);
        }
        hairColor = c;
    }

    void UpdateCombinedSkin()
    {

    }

    public void NextHairSkin()
    {
        activeHairIndex += 1;
        Debug.Log(activeHairIndex);
        if (activeHairIndex == 26)
        {
            activeHairIndex = 0;
        }
        UpdateCharacterSkin();
        UpdateColor(hairColor);
    }

    public void PrevHairSkin()
    {
        activeHairIndex -= 1;
        Debug.Log(activeHairIndex);
        if (activeHairIndex == -1)
        {
            activeHairIndex = 26;
        }
        UpdateCharacterSkin();
        UpdateColor(hairColor);
    }

    public bool backHair(int hairIndex)
    {
        if (hairIndex == 1 || hairIndex == 2 || hairIndex == 6 || hairIndex == 8 || hairIndex == 10
            || hairIndex == 13 || hairIndex == 14 || hairIndex == 24)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
