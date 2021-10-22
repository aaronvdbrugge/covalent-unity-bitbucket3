
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Spine;
using Spine.Unity;
using UnityEngine;



/// <summary>
/// On start, this will take inventory of all the player skins we have, and organize them where they
/// can be accessible to players.
/// The lists are static so can be accessed from anywhere.
/// Call Init() from Start of the first SkeletonMecanim you find. It's OK to call it multiple times
/// </summary>
public static class PlayerSkinManager 
{
    [SpineSkin] public static string baseSkin = "Bareskin";

    public static List<string> fullSkins = new List<string>();
    public static List<string> bottomsSkins = new List<string>();
    public static List<string> topsSkins = new List<string>();
    public static List<string> shoesSkins = new List<string>();
    public static List<string> hairSkins = new List<string>();



    public static void Init(SkeletonMecanim skeleton_mecanim)
    {
        if( fullSkins.Count > 0 )  // already initialized!
            return; 

        foreach (Skin s in skeleton_mecanim.skeleton.Data.Skins)
        {
            if (s.Name.Contains("Full Skins"))
                fullSkins.Add(s.Name);
            else if (s.Name.Contains("Bottoms"))
                bottomsSkins.Add(s.Name);
            else if (s.Name.Contains("Tops"))
                topsSkins.Add(s.Name);
            else if (s.Name.Contains("Shoes"))
                shoesSkins.Add(s.Name);
            else if (s.Name.Contains("Hair"))
                hairSkins.Add(s.Name);
        }
    }
}
