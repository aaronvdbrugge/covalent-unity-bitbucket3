using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Network_Object : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Variables
    
    #endregion

    //Should store spine sprite and other variables locally then only send on change.


    #region Private Variables
    Rigidbody2D r;
    Vector2 latestPos;
    Quaternion latestRot;
    Vector2 velocity = Vector2.zero;
    bool valuesReceived = false;
    #endregion




    void Awake()
    {
        r = GetComponent<Rigidbody2D>();
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (r != null)
            {
                if (gameObject.tag.Equals("soccerball"))
                {
                    stream.SendNext(r.position);
                    stream.SendNext(transform.rotation);
                    stream.SendNext(r.velocity);
                }
                else if (gameObject.tag.Equals("Player"))
                {
                    stream.SendNext(r.position);
                    stream.SendNext(transform.rotation);
                    stream.SendNext(r.velocity);
                    
                }
            }
        }
        else
        {
            if (r != null)
            {
                if (gameObject.tag.Equals("soccerball"))
                {
                    latestPos = (Vector2)stream.ReceiveNext();
                    latestRot = (Quaternion)stream.ReceiveNext();
                    r.velocity = (Vector2)stream.ReceiveNext();

                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                    latestPos += velocity * lag;

                    valuesReceived = true;
                    
                }
                else if (gameObject.tag.Equals("Player"))
                {
                    
                    latestPos = (Vector2)stream.ReceiveNext();
                    latestRot = (Quaternion)stream.ReceiveNext();
                    r.velocity = (Vector2)stream.ReceiveNext();

                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                    latestPos += r.velocity * lag;

                    valuesReceived = true;
                }
                
            }
           
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine && valuesReceived)
        {
            //r.position = Vector2.Lerp(r.position, latestPos, Time.fixedDeltaTime);
            r.position = Vector2.MoveTowards(r.position, latestPos, Time.fixedDeltaTime);
            transform.rotation = latestRot;
            if (gameObject.tag.Equals("Player"))
            {
                foreach (TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>())
                {
                    t.transform.rotation = Quaternion.Euler(0, transform.rotation.y * -1.0f, 0);
                }
            }
            
        }
    }
}
