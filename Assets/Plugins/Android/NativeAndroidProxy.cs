using UnityEngine;

namespace Plugins.Android
{
    public static class NativeAndroidProxy
    {
        //Primary static clas, which contains all the different static proxy objects
        private static AndroidJavaClass javaClass = new AndroidJavaClass("com.covalent.kippo.unity.UnityDispatcher");
        
        private static AndroidJavaObject messageProxy = javaClass.GetStatic<AndroidJavaObject>("messageProxy");
        
        public static void _updatePlayersInRoom(string[] unityJsonList, int count)
        {
            messageProxy.Call("updatePlayersInRoom", unityJsonList, count);
        }
        
        public static void _playerDidMute(int player_id)
        {
            messageProxy.Call("playerDidMute", player_id);   
        }
        
        public static void _playerDidUnmute(int player_id) {
            messageProxy.Call("playerDidUnmute", player_id); 
        }
        
        public static void _playerStartedTalking(int player_id) {
            messageProxy.Call("playerStartedTalking", player_id);
        }
        
        public static void _playerEndedTalking(int player_id) {
            messageProxy.Call("playerEndedTalking", player_id);
        }
        
        public static void _playerDidLeaveGame() {
            // NOTE! You should get this call if you disable the device's internet
            // while you are playing, forcing a disconnect.
            // However, if you tap the "Leave" button, I think that is currently a button overlaid
            // from the native interface (not in Unity) so you won't get this call from
            // Unity in that case.
            messageProxy.Call("playerDidLeaveGame");
            
        }
        
        public static void _failureToConnect(string error) {
            messageProxy.Call("failureToConnect", error);
        }
        
        public static void _failureToJoinRoom(string error) {
            messageProxy.Call("failureToJoinRoom", error);
        }
        
        public static void _failureToConnectAgora(string error)
        {
            messageProxy.Call("failureToConnectAgora", error);
        }
        
        public static void _missingMicPermission()
        {
            messageProxy.Call("missingMicPermission");
        }

        public static void showHostMainWindow(string color)
        {
            //TODO Create dispatcher for transition event.
        }

    }
}