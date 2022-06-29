//
//  Created by uncaha on 2021/11/30.
//

#ifndef HabbySDKAPI_h
#define HabbySDKAPI_h
@interface HabbySDKAPI : NSObject
+ (GKLocalPlayer *) LocalPlayer;
+ (void)LoginGameCenter:(int) seqId;
+ (void)GetIdentityVerificationSignature:(int) seqId;
+ (BOOL)IsAuthenticate;
+ (const char*)GetPlayerIdDictionary;
+ (void)SetAuthenticate:(bool) isCalled;
@end

#endif /* GameCenterTest_h */
