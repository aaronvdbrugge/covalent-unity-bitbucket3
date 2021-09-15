using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Spine_Player_Controller : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    public SkeletonMecanim skeletonMechanim;
    public MeshRenderer mesh;
    Skin characterSkin;
    public int characterSkinSlot = -1;
    [SpineSkin] public string baseSkin = "Bareskin";
    private bool skinInit = false;
    private Vector3 char_position;
    Dictionary<int, string> full_skins;
    int fullSkinCount = 0;
    Dictionary<int, string> bottoms_skins;
    int bottomsSkinCount = 0;
    Dictionary<int, string> tops_skins;
    int topsSkinCount = 0;
    Dictionary<int, string> shoes_skins;
    int shoesSkinCount = 0;
    Dictionary<int, string> hair_skins;
    int hairSkinCount = 0;

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
        full_skins = new Dictionary<int, string>();
        hair_skins = new Dictionary<int, string>();
        tops_skins = new Dictionary<int, string>();
        bottoms_skins = new Dictionary<int, string>();
        shoes_skins = new Dictionary<int, string>();
        EventManager.StartListening("init_char_creator", Character_Creator_Startup);
        EventManager.StartListening("end_char_creator", Character_Creator_End);
        skeletonMechanim = GetComponent<SkeletonMecanim>();

        if( GameObject.Find("Color_Slider") != null && GameObject.Find("Color_Slider").GetComponent<Character_Creator_Controller>() != null )
            GameObject.Find("Color_Slider").GetComponent<Character_Creator_Controller>().skeletonMechanim = skeletonMechanim;

        mesh = GetComponent<MeshRenderer>();

        //mesh.enabled = false;   // don't know why this was here but it gave me problems so I commented it out.  --seb

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
        //Debug.Log("Full skin count: " + fullSkinCount);
    }

    // Update is called once per frame
    void Update()
    {
        if (characterSkinSlot != -1 && skeletonMechanim != null && !skinInit)
        {
            skinInit = true;
            SetFullSkin(characterSkinSlot);
            //SetFullSkin(characterSkinSlot);
            //this.photonView.RPC("sendSkin", Photon.Pun.RpcTarget.All, characterSkinSlot);
        }
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;

        Debug.Log("Instantiating Spine Player Controller with skin " + (int)instantiationData[0]);
        characterSkinSlot = (int)instantiationData[0];
    }

    public void Character_Creator_Startup()
    {
        if (photonView.IsMine)
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
        
    }
    public void Character_Creator_End()
    {
        if (photonView.IsMine)
        {
            GetComponent<BoxCollider2D>().enabled = true;
            transform.localScale = new Vector3(1, 1, 1);
            transform.position = char_position;
            GetComponent<Renderer>().sortingLayerName = "Default";
        }
        
    }
    /*
    [PunRPC]
    public void sendSkin(int slot)
    {
        //SetFullSkin(skinNum);
        characterSkinSlot = slot;
        skeletonMechanim.skeleton.SetSkin(full_skins[slot]);
        skeletonMechanim.skeleton.SetToSetupPose();
        setFullSkinCalled = true;
        mesh.enabled = true;
    }
    */
    public void SetFullSkin(int slot)
    {
        Debug.Log("Inside setskin");
        if (skeletonMechanim != null)
        {
            characterSkinSlot = slot;
            skeletonMechanim.skeleton.SetSkin(full_skins[slot]);
            skeletonMechanim.skeleton.SetToSetupPose();
            //setFullSkinCalled = true;   //due to warning, assigned but never used. -seb
            mesh.enabled = true;
        }
        
    }

    public void ChangeColor(int r, int g, int b)
    {
        Color32 c2 = new Color32((byte)r, (byte)g, (byte)b, 255);
        skeletonMechanim.skeleton.FindSlot("head_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("body_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("leftarm_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("lefthand_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("leftfoot_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("leftleg_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("rightarm_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("righthand_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("rightfoot_skin").SetColor(c2);
        skeletonMechanim.skeleton.FindSlot("rightleg_skin").SetColor(c2);
    }
    public void ChangeSkin()
    {
        var skeleton = skeletonMechanim.skeleton;
        var skeletonData = skeleton.Data;

        characterSkin = new Skin("character-base");
        characterSkin.AddSkin(skeletonData.FindSkin(baseSkin));

        int randomHair = Random.Range(0, hairSkinCount);
        characterSkin.AddSkin(skeletonData.FindSkin(hair_skins[0]));

        int randomTop = Random.Range(0, topsSkinCount);
        characterSkin.AddSkin(skeletonData.FindSkin(tops_skins[0]));

        int randomBottom = Random.Range(0, bottomsSkinCount);
        characterSkin.AddSkin(skeletonData.FindSkin(bottoms_skins[0]));

        int randomShoe = Random.Range(0, shoesSkinCount);
        characterSkin.AddSkin(skeletonData.FindSkin(shoes_skins[0]));

        skeleton.SetSkin(characterSkin);
        skeleton.SetSlotsToSetupPose();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
