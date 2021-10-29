// [!] important set UnityFramework in Target Membership for this file
// [!]           and set Public header visibility
#import <Foundation/Foundation.h>
// NativeCallsProtocol defines protocol with methods you want to be called from managed
@protocol NativeCallsProtocol
@required
- (void) showHostMainWindow:(NSString*)color;
- (void) updatePlayersInRoom:(NSMutableArray*)players;
- (void) playerDidMute:(int)playerId;
- (void) playerDidUnmute:(int)playerId;
- (void) playerStartedTalking:(int)playerId;
- (void) playerEndedTalking:(int)playerId;
- (void) playerDidLeaveGame;
// other methods
@end
__attribute__ ((visibility("default")))
@interface FrameworkLibAPI : NSObject
// call it any time after UnityFrameworkLoad to set object implementing NativeCallsProtocol methods
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi;
@end