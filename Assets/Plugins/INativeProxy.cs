namespace Plugins
{
    /**
     * Defines the native functions accessible to Unity to communicate with the native code.
     * Implemented by both Android and iOS messaging proxies.
     */
    public interface INativeProxy
    {
        public void _updatePlayersInRoom(string[] unityJsonList, int count);
        
        public void _playerDidMute(uint player_id);

        public void _playerDidUnmute(uint player_id);

        public void _playerStartedTalking(uint player_id);

        public void _playerEndedTalking(uint player_id);

        public void _playerDidLeaveGame();

        public void _failureToConnect(string error);

        public void _failureToJoinRoom(string error);

        public void _failureToConnectAgora(string error);

        public void _missingMicPermission();

    }
}