using UnityEngine;

namespace Plugins.Android
{
    public static class NativeAndroidProxy
    {
        //Primary static clas, which contains all the different static proxy objects
        private static AndroidJavaClass javaClass = new AndroidJavaClass("com.covalent.kippo.unity.UnityDispatcher");
        
        private static AndroidJavaObject dateLandProxy = javaClass.GetStatic<AndroidJavaObject>("dateLandDispatcher");
        private static AndroidJavaObject homeIslandProxy = javaClass.GetStatic<AndroidJavaObject>("homeIslandDispatcher");
        private static AndroidJavaObject errorProxy = javaClass.GetStatic<AndroidJavaObject>("errorDispatcher");
        
        public static void _updatePlayersInRoom(string[] unityJsonList, int count)
        {
            dateLandProxy.Call("updatePlayersInRoom", unityJsonList, count);
        }
        
        public static void _playerDidMute(int player_id)
        {
            dateLandProxy.Call("playerDidMute", player_id);   
        }
        
        public static void _playerDidUnmute(int player_id) {
            dateLandProxy.Call("playerDidUnmute", player_id); 
        }
        
        public static void _playerStartedTalking(int player_id) {
            dateLandProxy.Call("playerStartedTalking", player_id);
        }
        
        public static void _playerEndedTalking(int player_id) {
            dateLandProxy.Call("playerEndedTalking", player_id);
        }
        
        public static void _playerDidLeaveGame() {
            // NOTE! You should get this call if you disable the device's internet
            // while you are playing, forcing a disconnect.
            // However, if you tap the "Leave" button, I think that is currently a button overlaid
            // from the native interface (not in Unity) so you won't get this call from
            // Unity in that case.
            dateLandProxy.Call("playerDidLeaveGame");
            
        }
        
        public static void _failureToConnect(string error) {
            errorProxy.Call("failureToConnect", error);
        }
        
        public static void _failureToJoinRoom(string error) {
            
            errorProxy.Call("failureToJoinRoom", error);
        }
        
        public static void _failureToConnectAgora(string error)
        {
            errorProxy.Call("failureToConnectAgora", error);
        }
        
        public static void _missingMicPermission()
        {
            errorProxy.Call("missingMicPermission");
        }

        public static void showHostMainWindow(string color)
        {
            //TODO Create dispatcher for transition event.
        }

    }
}