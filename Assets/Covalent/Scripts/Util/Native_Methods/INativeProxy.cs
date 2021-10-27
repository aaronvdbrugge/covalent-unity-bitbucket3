namespace Plugins
{
    /**
     * Defines the native functions accessible to Unity to communicate with the native code.
     * Implemented by both Android and iOS messaging proxies.
     */
    public interface INativeProxy
    {
        public void UpdatePlayersInRoom(string[] unityJsonList, int count);
        
        public void PlayerDidMute(uint player_id);

        public void PlayerDidUnmute(uint player_id);

        public void PlayerStartedTalking(uint player_id);

        public void PlayerEndedTalking(uint player_id);

        public void PlayerDidLeaveGame();

        public void FailureToConnect(string error);

        public void FailureToJoinRoom(string error);

        public void FailureToConnectAgora(string error);

        public void MissingMicPermission();

    }
}