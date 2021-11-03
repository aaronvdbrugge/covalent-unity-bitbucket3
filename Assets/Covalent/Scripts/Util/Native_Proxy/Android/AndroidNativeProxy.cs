using Covalent.Scripts.Util.Native_Proxy;
using UnityEngine;

namespace Covalent.Scripts.Util.Native_Proxy.Android
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
        //private static AndroidJavaClass javaClass = new AndroidJavaClass("com.covalent.kippo.unity.UnityDispatcher");
        //private static AndroidJavaObject messageProxy = javaClass.GetStatic<AndroidJavaObject>("messageProxy");
        // See NativeProxy.cs for an explanation of why we can't initialize static members in this way. It's the compiler's fault.
        


        //The static instance of the UnityDispatcher class.
        // Note that it will be null when building in isolation.
        private static AndroidJavaObject messageProxy
        {
            get
            {
                if( _messageProxy == null )
                {
                    AndroidJavaClass javaClass; //Android Java Class that contains the static object which will be used to trigger callbacks on native side
                    try
                    {
                        javaClass = new AndroidJavaClass("com.covalent.kippo.unity.UnityDispatcher"); 
                        _messageProxy = javaClass.GetStatic<AndroidJavaObject>("messageProxy");
                    }
                    catch
                    {
                        return null;   // class not found, probably. This is perfectly normal when building in isolation (not integrated)
                    }
                }
                return _messageProxy;
            }
        }
        private static AndroidJavaObject _messageProxy;


        
        public void UpdatePlayersInRoom(string[] unityJsonList, int count)
        {
            messageProxy?.Call("updatePlayersInRoom", unityJsonList, count);    // note:  "?." syntax ensures this won't happen if messageProxy is null, which can happen when building in isolation
        }
        
        public void PlayerDidMute(uint player_id)
        {
            messageProxy?.Call("playerDidMute", (int) player_id);   
        }
        
        public void PlayerDidUnmute(uint player_id) {
            messageProxy?.Call("playerDidUnmute", (int) player_id); 
        }
        
        public void PlayerStartedTalking(uint player_id) {
            messageProxy?.Call("playerStartedTalking", (int) player_id);
        }
        
        public void PlayerEndedTalking(uint player_id) {
            messageProxy?.Call("playerEndedTalking", (int) player_id);
        }
        
        public void PlayerDidLeaveGame() {
            // NOTE! You should get this call if you disable the device's internet
            // while you are playing, forcing a disconnect.
            // However, if you tap the "Leave" button, I think that is currently a button overlaid
            // from the native interface (not in Unity) so you won't get this call from
            // Unity in that case.
            messageProxy?.Call("playerDidLeaveGame");
            
        }
        
        public void FailureToConnect(string error) {
            messageProxy?.Call("failureToConnect", error);
        }
        
        public void FailureToJoinRoom(string error) {
            messageProxy?.Call("failureToJoinRoom", error);
        }
        
        public void FailureToConnectAgora(string error)
        {
            messageProxy?.Call("failureToConnectAgora", error);
        }
        
        public void MissingMicPermission()
        {
            messageProxy?.Call("missingMicPermission");
        }

        public void ShowHostMainWindow(string color)
        {
            //TODO Create dispatcher for transition event.
        }

    }
#endif
}