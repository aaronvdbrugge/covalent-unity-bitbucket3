#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"
@implementation FrameworkLibAPI
id<NativeCallsProtocol> api = NULL;
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}
@end
extern "C" {

void showHostMainWindow(const char* color) { return [api showHostMainWindow:[NSString stringWithUTF8String:color]]; }
void _updatePlayersInRoom(char *arr[], int count) {
    NSLog(@"FROM UNITY: updatePlayersInRoom(%i)", count);
    
    char* cStringCopy(const char* string);
    NSMutableArray *items = [NSMutableArray array];
    for(int i = 0; i < count; i++)
    {
        NSString *str = [[NSString alloc] initWithCString:arr[i] encoding:NSUTF8StringEncoding];
        [items addObject:str];
    }
    [api updatePlayersInRoom:items];
}
void _playerDidMute(unsigned int player_id) {
    // NOTE! player_id is an Agora assigned ID.
    NSLog(@"FROM UNITY: playerDidMute(%i)", player_id);
}
void _playerDidUnmute(unsigned int player_id) {
    NSLog(@"FROM UNITY: playerDidUnmute(%i)", player_id);
}
void _playerStartedTalking(unsigned int player_id) {
    NSLog(@"FROM UNITY: playerStartedTalking(%i)", player_id);
}
void _playerEndedTalking(unsigned int player_id) {
    NSLog(@"FROM UNITY: playerEndedTalking(%i)", player_id);
}
void _playerDidLeaveGame() {
    // NOTE! You should get this call if you disable the device's internet
    // while you are playing, forcing a disconnect.
    // However, if you tap the "Leave" button, I think that is currently a button overlaid
    // from the native interface (not in Unity) so you won't get this call from
    // Unity in that case.
    NSLog(@"FROM UNITY: _playerDidLeaveGame");
}
void _failureToConnect(const char* error) {
    NSLog(@"FROM UNITY: failureToConnect(%s)", error);
}
void _failureToJoinRoom(const char* error) {
    NSLog(@"FROM UNITY: failureToJoinRoom(%s)", error);
}
void _failureToConnectAgora(const char* error) {
    NSLog(@"FROM UNITY: failureToConnectAgora(%s)", error);
}


}
