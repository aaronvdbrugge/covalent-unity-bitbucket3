using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// When players join a room, they reserve a slot for themselves, and a slot for their partner
/// player (see: TeamRoomJoin.cs)
/// However, if they both disconnect, their slots are still reserved. We need the Master Client
/// to take responsibility for this, and periodically check reserved slots, clearing out any that
/// appear to be unused by any players in the server, or their partner players.
/// 
/// There is a chance we might get reserved slots several seconds before either player joins,
/// so time out each slot before you actually clear it.
/// </summary>
public class CullReservedSlots : MonoBehaviour
{
    [Tooltip("Iterates through PhotonNetwork.room.ExpectedUsers every this amount of seconds, if master client.")]
    public float checkInterval = 10.0f;

    [Tooltip("Slots are given this long to reconnect, before their reservation is removed.")]
    public float slotTimeout = 60.0f;


    /// <summary>
    /// Time.time when we first noticed that the slot with string key ID should
    /// probably be removed. If it's still 
    /// </summary>
    Dictionary<string, float> _firstTimeWentMissing = new Dictionary<string, float>();

    float _checkCooldown = 0;

    bool _didBecameMasterLog = false;   // just used for a debug log

    List<string> _oldExpectedUsers = new List<string>();   // this is just so we can make debug logs when slots are reserved / unreserved

    void FixedUpdate()
    {
        if( PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null )    //note that we could become the master client at any time, if the original master client leaves
        {
            _checkCooldown -= Time.fixedDeltaTime;
            if( _checkCooldown <= 0 )
            {
                _checkCooldown = checkInterval;


                if( !_didBecameMasterLog )
                {
                    _didBecameMasterLog = true;
                    Debug.Log("Became master client. Handling reservation slot culling.");
                }

                // For debugging: log reservations
                List<string> expected_users = PhotonNetwork.CurrentRoom.ExpectedUsers != null ? new List<string>(PhotonNetwork.CurrentRoom.ExpectedUsers) : new List<string>();  //empty list if null
            
                foreach( string str in expected_users )
                    if( !_oldExpectedUsers.Contains( str ) )   // new reserved slot!
                        Debug.Log("New reserved slot: " + str);
                foreach( string str in _oldExpectedUsers )
                    if( !expected_users.Contains(str) )   // slot was newly removed!
                        Debug.Log("Removed reserved slot: " + str );

                _oldExpectedUsers = new List<string>(expected_users);  // for next time



                // Now, start timing out reservations that don't have anyone holding them.
                List<string> chopping_block = new List<string>();   // slots that we would like to get rid of, but may need to wait for cooldown

                // Look for reserved slots we can start timing out, by putting them in _firstTimeWentMissing...
                foreach( string slot in expected_users)
                {
                    bool found = false;
                    foreach( var kvp in Player_Controller_Mobile.playersByKippoId )
                        if( kvp.Value.kippoUserId.ToString() == slot || kvp.Value.partnerPlayerId.ToString() == slot )  // This slot is for an existing player, or a partner of one
                        {
                            found = true;
                            break;
                        }

                    if( !found )   // We probably don't need this reservation slot... start cooling it down
                        chopping_block.Add( slot );
                }


                // Make the entries in _firstTimeWentMissing match chopping_block.
                // If any are removed from _firstTimeWentMissing, they've been "saved" because the user connected.
                List<string> saved = new List<string>();
                foreach( var kvp in _firstTimeWentMissing )
                    if( !chopping_block.Contains( kvp.Key ) )   // we're saved; not in chopping block anymore
                        saved.Add( kvp.Key );

                foreach( string save in saved )
                {
                    Debug.Log("Slot reservation for " + save + " didn't time out. The player or their partner reconnected.");
                    _firstTimeWentMissing.Remove( save );
                }


                // Now add entries TO _firstTimeWentMissing
                foreach( string chop in chopping_block )
                    if( !_firstTimeWentMissing.ContainsKey( chop ) )
                        _firstTimeWentMissing[chop] = Time.time;   // timestamp so we know when they've been gone too long



                // Finally, do actual removal when an entry has been inside _firstTimeWentMissing for too long.
                List<string> removed = new List<string>();
                List<string> new_expected = new List<string>(expected_users);
                foreach( var kvp in _firstTimeWentMissing )
                    if( Time.time - kvp.Value >= slotTimeout )  // it's been in this list too long. We should remove it now
                    {
                        Debug.Log("Slot reservation for " + kvp.Key + " is being removed. Player seems to have disconnected.");
                        if( new_expected.Contains( kvp.Key ) )
                            new_expected.Remove( kvp.Key );

                        removed.Add( kvp.Key );
                    }

                if( removed.Count > 0 )  //Need to update expected users!
                    PhotonNetwork.CurrentRoom.SetExpectedUsers( new_expected.ToArray() );

                foreach( string key in removed )   // clean up _firstTimeWentMissing
                    _firstTimeWentMissing.Remove( key );
            }
        }
        else
        {
            // If we are not the master client, just make sure everything's clean in case we become them at some point.
            _firstTimeWentMissing.Clear();
            _checkCooldown = 0;
            _didBecameMasterLog = false;
        }
    }
}
