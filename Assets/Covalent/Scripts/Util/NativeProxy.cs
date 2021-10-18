
using System.Runtime.InteropServices;
using Plugins;
using UnityEngine;
//Include based on platform
#if PLATFORM_ANDROID
using Plugins.Android;

#elif UNITY_IOS
using Plugins.iOS;

#endif

namespace Covalent.Scripts.Util.Editor
{
    public static  class NativeProxy
    {
#if PLATFORM_ANDROID
        private static INativeProxy messageProxy = new AndroidNativeProxy();
#else
        private static INativeProxy messageProxy = new iOSNativeProxy();
#endif
        public static void _updatePlayersInRoom(string[] unityJsonList, int count)
        {
            messageProxy._updatePlayersInRoom(unityJsonList, count);
        }

        public static void _playerDidMute(uint player_id)
        {
            messageProxy._playerDidMute(player_id);
        }

        public static void _playerDidUnmute(uint player_id)
        {
            messageProxy._playerDidUnmute(player_id);
        }

        public static void _playerStartedTalking(uint player_id)
        {
            messageProxy._playerStartedTalking(player_id);
        }

        public static void _playerEndedTalking(uint player_id)
        {
            messageProxy._playerEndedTalking(player_id);
        }

        public static void _playerDidLeaveGame()
        {
            messageProxy._playerDidLeaveGame();
        }

        public static void _failureToConnect(string error)
        {
            messageProxy._failureToConnect(error);
        }

        public static void _failureToJoinRoom(string error)
        {
            messageProxy._failureToJoinRoom(error);
        }

        public static void _failureToConnectAgora(string error)
        {
            messageProxy._failureToConnectAgora(error);
        }

        public static void _missingMicPermission()
        {
            messageProxy._missingMicPermission();
        }
    }
}