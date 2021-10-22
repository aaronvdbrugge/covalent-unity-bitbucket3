﻿using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Spine_Player_Controller : MonoBehaviour
{
    public SkeletonMecanim skeletonMecanim;
    public MeshRenderer mesh;
    Skin characterSkin;
    public int characterSkinSlot = -1;
    [SpineSkin] public string baseSkin = "Bareskin";
    private bool skinInit = false;
    private Vector3 char_position;

    public static int PrecisionValue
    {
        get
        {
            return -10;
        }
    }


	// Start is called before the first frame update
	void Start()
    {
        PlayerSkinManager.Init(skeletonMecanim);

        EventManager.StartListening("init_char_creator", Character_Creator_Startup);
        EventManager.StartListening("end_char_creator", Character_Creator_End);
        skeletonMecanim = GetComponent<SkeletonMecanim>();

        if( GameObject.Find("Color_Slider") != null && GameObject.Find("Color_Slider").GetComponent<Character_Creator_Controller>() != null )
            GameObject.Find("Color_Slider").GetComponent<Character_Creator_Controller>().skeletonMechanim = skeletonMecanim;

        mesh = GetComponent<MeshRenderer>();

        //mesh.enabled = false;   // don't know why this was here but it gave me problems so I commented it out.  --seb
        //Debug.Log("Full skin count: " + fullSkinCount);
    }

    // Update is called once per frame
    void Update()
    {
        if (characterSkinSlot != -1 && skeletonMecanim != null && !skinInit)
        {
            skinInit = true;
            SetFullSkin(characterSkinSlot);
            //SetFullSkin(characterSkinSlot);
            //this.photonView.RPC("sendSkin", Photon.Pun.RpcTarget.All, characterSkinSlot);
        }


        //Test
        if( Input.GetKeyDown( KeyCode.P ) )
            SetFullSkin( (characterSkinSlot + 1) % PlayerSkinManager.fullSkins.Count );
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;

        Debug.Log("Instantiating Spine Player Controller with skin " + (int)instantiationData[0]);
        characterSkinSlot = (int)instantiationData[0];
    }

    public void Character_Creator_Startup()
    {

        GetComponent<BoxCollider2D>().enabled = false;
        transform.localScale = new Vector3(2, 2, 2);
        char_position = new Vector3(0, 0, 0);
        char_position.x = transform.position.x;
        char_position.y = transform.position.y;
        char_position.z = transform.position.z;
        //transform.position = new Vector3(0, 3.5f, 0);
        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.65f, 1));
        GetComponent<Renderer>().sortingLayerName = "CC";        
    }
    public void Character_Creator_End()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        transform.localScale = new Vector3(1, 1, 1);
        transform.position = char_position;
        GetComponent<Renderer>().sortingLayerName = "Default";   
    }
    

    public void SetFullSkin(int slot)
    {
        Debug.Log("Inside setskin");
        if (skeletonMecanim != null)
        {
            characterSkinSlot = slot;
            skeletonMecanim.skeleton.SetSkin(PlayerSkinManager.fullSkins[slot]);
            skeletonMecanim.skeleton.SetToSetupPose();
            //setFullSkinCalled = true;   //due to warning, assigned but never used. -seb
            mesh.enabled = true;
        }
        
    }

    public void ChangeColor(int r, int g, int b)
    {
        Color32 c2 = new Color32((byte)r, (byte)g, (byte)b, 255);
        skeletonMecanim.skeleton.FindSlot("head_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("body_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("leftarm_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("lefthand_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("leftfoot_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("leftleg_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("rightarm_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("righthand_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("rightfoot_skin").SetColor(c2);
        skeletonMecanim.skeleton.FindSlot("rightleg_skin").SetColor(c2);
    }
    public void RandomizeSkin()
    {
        var skeleton = skeletonMecanim.skeleton;
        var skeletonData = skeleton.Data;

        characterSkin = new Skin("character-base");
        characterSkin.AddSkin(skeletonData.FindSkin(baseSkin));

        int randomHair = Random.Range(0, PlayerSkinManager.hairSkins.Count);
        characterSkin.AddSkin(skeletonData.FindSkin(PlayerSkinManager.hairSkins[randomHair]));

        int randomTop = Random.Range(0, PlayerSkinManager.topsSkins.Count);
        characterSkin.AddSkin(skeletonData.FindSkin(PlayerSkinManager.topsSkins[randomTop]));

        int randomBottom = Random.Range(0, PlayerSkinManager.bottomsSkins.Count);
        characterSkin.AddSkin(skeletonData.FindSkin(PlayerSkinManager.bottomsSkins[randomBottom]));

        int randomShoe = Random.Range(0, PlayerSkinManager.shoesSkins.Count);
        characterSkin.AddSkin(skeletonData.FindSkin(PlayerSkinManager.shoesSkins[randomShoe]));

        skeleton.SetSkin(characterSkin);
        skeleton.SetSlotsToSetupPose();
    }



}
