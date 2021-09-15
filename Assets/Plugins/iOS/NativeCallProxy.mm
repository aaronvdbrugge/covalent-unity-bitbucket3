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
    char* cStringCopy(const char* string);
    NSMutableArray *items = [NSMutableArray array];
    for(int i = 0; i < count; i++)
    {
        NSString *str = [[NSString alloc] initWithCString:arr[i] encoding:NSUTF8StringEncoding];
        [items addObject:str];
    }
    [api updatePlayersInRoom:items];
}
}
