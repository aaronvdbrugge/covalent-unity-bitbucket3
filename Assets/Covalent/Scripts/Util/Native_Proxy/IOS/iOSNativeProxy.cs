using System.Runtime.InteropServices;
using Covalent.Scripts.Util.Native_Proxy;

namespace Covalent.Scripts.Util.Native_Proxy.IOS
{
#if UNITY_IOS
    public class IOSNativeProxy : INativeProxy
    {
        /**
         * Wrapper class for the iOS native functions that will trigger events when running on iOS.
         * Ideally not to be called directly by anything other than IOSNativeProxy as we want to provide wrapper methods
         * that are defined in INativeProxy interface to keep consistency between Android and iOS.
         */
        private static class iOSInternal
        {

            [DllImport("__Internal")]
            public static extern void _updatePlayersInRoom(string[] unityJSONList, int count);

            [DllImport("__Internal")]
            public static extern void _playerDidLeaveGame();

            [DllImport("__Internal")]
            public static extern void _failureToConnect(string error);

            [DllImport("__Internal")]
            public static extern void _failureToJoinRoom(string error);
            
            [DllImport("__Internal")]
            public static extern void _failureToConnectAgora(string error);
            
            [DllImport("__Internal")]
            public static extern void _playerDidMute(uint player_id);
            
            [DllImport("__Internal")]
            public static extern void _playerDidUnmute(uint player_id);
            
            [DllImport("__Internal")]
            public static extern void _playerStartedTalking(uint player_id);
            
            [DllImport("__Internal")]
            public static extern void _playerEndedTalking(uint player_id);
            
        }
        
        public void UpdatePlayersInRoom(string[] unityJsonList, int count)
        {
            iOSInternal._updatePlayersInRoom(unityJsonList, count);
        }
        
        public void PlayerDidMute(uint player_id)
        {
            iOSInternal._playerDidMute(player_id);
        }
        
        public void PlayerDidUnmute(uint player_id) {
            iOSInternal._playerDidUnmute(player_id);
        }
        
        public void PlayerStartedTalking(uint player_id) {
            iOSInternal._playerStartedTalking(player_id);
        }
        
        public void PlayerEndedTalking(uint player_id) {
            iOSInternal._playerEndedTalking(player_id);
        }
        
        public void PlayerDidLeaveGame() {
            iOSInternal._playerDidLeaveGame();
        }
        
        public void FailureToConnect(string error) {
            iOSInternal._failureToConnect(error);
        }
        
        public void FailureToJoinRoom(string error) {
            iOSInternal._failureToJoinRoom(error);
        }
        
        public void FailureToConnectAgora(string error)
        {
           iOSInternal._failureToConnectAgora(error);
        }
        
        public void MissingMicPermission()
        {
        }
    }
#endif
}