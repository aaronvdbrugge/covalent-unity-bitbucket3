using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User
{
    //Make ints into strings
    public int id;
    public string name;
    public string userName;
    public string age;
    public string distance;
    public Address address;
    public string[] photos;
    public bool direct_message_available;
    public CardModules cardModules;
    public Game[] games;
    public bool isPublicFigure;
    public bool isPremium;
    public bool isPaidPremium;
    public bool isBorderOn;
    public bool isBadgeOn;
    public string chatColor;
    public bool onlyFriends;
}
