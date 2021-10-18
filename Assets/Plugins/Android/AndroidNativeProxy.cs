using UnityEngine;

namespace Plugins.Android
{
#if PLATFORM_ANDROID
    /**
     * The dispatcher/proxy class to trigger callback/listeners when running on the Android Platform.
     * Android exposes a static instance of Java class UnityDispatcher which can be utilized through AndroidJavaObject.
     * Through this instance we can call the functions defined in <see cref="INativeProxy"/> and it will trigger events
     * that will be handled on the native side.
     */
    public class AndroidNativeProxy : INativeProxy
    {
        //Android Java Class that contains the static object which will be used to trigger callbacks on native side
        private static AndroidJavaClass javaClass = new AndroidJavaClass("com.covalent.kippo.unity.UnityDispatcher");
        //The static instance of the UnityDispatcher class.
        private static AndroidJavaObject messageProxy = javaClass.GetStatic<AndroidJavaObject>("messageProxy");
        
        public void _updatePlayersInRoom(string[] unityJsonList, int count)
        {
            messageProxy.Call("updatePlayersInRoom", unityJsonList, count);
        }
        
        public void _playerDidMute(uint player_id)
        {
            messageProxy.Call("playerDidMute", player_id);   
        }
        
        public void _playerDidUnmute(uint player_id) {
            messageProxy.Call("playerDidUnmute", player_id); 
        }
        
        public void _playerStartedTalking(uint player_id) {
            messageProxy.Call("playerStartedTalking", player_id);
        }
        
        public void _playerEndedTalking(uint player_id) {
            messageProxy.Call("playerEndedTalking", player_id);
        }
        
        public void _playerDidLeaveGame() {
            // NOTE! You should get this call if you disable the device's internet
            // while you are playing, forcing a disconnect.
            // However, if you tap the "Leave" button, I think that is currently a button overlaid
            // from the native interface (not in Unity) so you won't get this call from
            // Unity in that case.
            messageProxy.Call("playerDidLeaveGame");
            
        }
        
        public void _failureToConnect(string error) {
            messageProxy.Call("failureToConnect", error);
        }
        
        public void _failureToJoinRoom(string error) {
            messageProxy.Call("failureToJoinRoom", error);
        }
        
        public void _failureToConnectAgora(string error)
        {
            messageProxy.Call("failureToConnectAgora", error);
        }
        
        public void _missingMicPermission()
        {
            messageProxy.Call("missingMicPermission");
        }

        public void showHostMainWindow(string color)
        {
            //TODO Create dispatcher for transition event.
        }

    }
#endif
}