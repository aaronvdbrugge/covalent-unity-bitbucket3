using System.Runtime.InteropServices;

namespace Plugins.iOS
{
#if UNITY_IOS
    public class iOSNativeProxy : INativeProxy
    {
        
        private static class iOSInternal
        {
            // External calls (out to native app)

            [DllImport("__Internal")]
            public static extern void _updatePlayersInRoom(string[] unityJSONList, int count);

            [DllImport("__Internal")]
            public static extern void _playerDidLeaveGame();

            [DllImport("__Internal")]
            public static extern void _failureToConnect(string error);

            [DllImport("__Internal")]
            public static extern void _failureToJoinRoom(string error);
            
            // External functions (to native app)
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
        
        public void _updatePlayersInRoom(string[] unityJsonList, int count)
        {
            iOSInternal._updatePlayersInRoom(unityJsonList, count);
        }
        
        public void _playerDidMute(uint player_id)
        {
            iOSInternal._playerDidMute(player_id);
        }
        
        public void _playerDidUnmute(uint player_id) {
            iOSInternal._playerDidUnmute(player_id);
        }
        
        public void _playerStartedTalking(uint player_id) {
            iOSInternal._playerStartedTalking(player_id);
        }
        
        public void _playerEndedTalking(uint player_id) {
            iOSInternal._playerEndedTalking(player_id);
        }
        
        public void _playerDidLeaveGame() {
            iOSInternal._playerDidLeaveGame();
        }
        
        public void _failureToConnect(string error) {
            iOSInternal._failureToConnect(error);
        }
        
        public void _failureToJoinRoom(string error) {
            iOSInternal._failureToJoinRoom(error);
        }
        
        public void _failureToConnectAgora(string error)
        {
           iOSInternal._failureToConnectAgora(error);
        }
        
        public void _missingMicPermission()
        {
        }
    }
#endif
}