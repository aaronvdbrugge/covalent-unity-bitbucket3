
using System.Runtime.InteropServices;
using Plugins;
using UnityEngine;

//Include based on platform
#if PLATFORM_ANDROID
using Plugins.Android;

#elif UNITY_IOS
using Plugins.iOS;

#endif

namespace Covalent.Scripts.Util
{
    public static  class NativeProxy
    {
        //Instantiate messageProxy to an instance of INativeProxy based on the platform
#if PLATFORM_ANDROID
        private static INativeProxy messageProxy = new AndroidNativeProxy();
#else
        private static INativeProxy messageProxy = new IOSNativeProxy();
#endif
        /**
         * <summary>Notifies about the number of users currently in a photon room</summary>
         * <param name="unityJsonList"> List of current players serialized to strings </param>
         * <param name="count"> Number of users </param>
         */
        public static void _updatePlayersInRoom(string[] unityJsonList, int count)
        {
            messageProxy._updatePlayersInRoom(unityJsonList, count);
        }
        /**
         * <summary>To be called when a player mutes their mic</summary>
         * <param name="player_id">The Id of the player that muted the microphone</param>
         */
        public static void _playerDidMute(uint player_id)
        {
            messageProxy._playerDidMute(player_id);
        }
        /**
         * <summary>To be called when a player unmutes their mic</summary>
         * <param name="player_id">The Id of the player that unmuted the microphone</param>
         */
        public static void _playerDidUnmute(uint player_id)
        {
            messageProxy._playerDidUnmute(player_id);
        }
        /**
         * <summary>Notifies when a player begins talking</summary>
         * <param name="player_id">Th Id of the player that started talking</param>
         */
        public static void _playerStartedTalking(uint player_id)
        {
            messageProxy._playerStartedTalking(player_id);
        }
        /**
         * <summary>Notifies when a player stops talking</summary>
         * <param name="player_id">Th Id of the player that stopped talking</param>
         */
        public static void _playerEndedTalking(uint player_id)
        {
            messageProxy._playerEndedTalking(player_id);
        }
        /**
         * <summary>Notifies when the user leaves the game due to network connection issues</summary>
         */
        public static void _playerDidLeaveGame()
        {
            messageProxy._playerDidLeaveGame();
        }
        /**
         * <summary>Failure to connect to photon servers</summary>
         * <param name="error">Error message for the failure to connect</param>
         */
        public static void _failureToConnect(string error)
        {
            messageProxy._failureToConnect(error);
        }
        /**
         * <summary>Failure to join a photon room</summary>
         * <param name="error">Error message</param>
         */
        public static void _failureToJoinRoom(string error)
        {
            messageProxy._failureToJoinRoom(error);
        }
        /**
         * <summary>Failure to connect to Agora servers</summary>
         * <param name="error">Error message</param>
         */
        public static void _failureToConnectAgora(string error)
        {
            messageProxy._failureToConnectAgora(error);
        }
        /**
         * <summary>Notifies that the device has not granted the app Microphone permissions</summary>
         */
        public static void _missingMicPermission()
        {
            messageProxy._missingMicPermission();
        }
    }
}