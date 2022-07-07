//
//  Created by uncaha on 2021/11/30.
//

#import <Foundation/Foundation.h>
#import <GameKit/GameKit.h>
#import "HabbyAccountAPI.h"
#import "HabbyKeychainItemWrapper.h"


void _getIdentityVerificationSignature(int seqId)
{
    [HabbySDKAPI GetIdentityVerificationSignature: seqId];
}

void _loginGameCenter(int seqId)
{
    [HabbySDKAPI LoginGameCenter: seqId];
}

bool _IsAuthenticate()
{
    return [HabbySDKAPI IsAuthenticate];
}

char* _GetLocalPlayerData()
{
    const char* tstr = [HabbySDKAPI GetPlayerIdDictionary];
    if(tstr == NULL) return NULL;
    char* res = (char*)malloc(strlen(tstr)+1);
    strcpy(res, tstr);
    return res;
}

char * _GetUUID(const char * unityuuid)
{
    HabbyKeychainItemWrapper *keychainItem = [[HabbyKeychainItemWrapper alloc]
      initWithIdentifier:@"UUID"
      accessGroup:nil];

    NSString* strUUID = [keychainItem objectForKey:(id)CFBridgingRelease(kSecAttrAccount)];
    
    if (strUUID == nil || [strUUID length] == 0)
    {
        strUUID = [NSString stringWithUTF8String: unityuuid];
        [keychainItem setObject: strUUID forKey:(id)CFBridgingRelease(kSecAttrAccount)];
    }
    else
    {
        strUUID = [keychainItem objectForKey:(id)CFBridgingRelease(kSecAttrAccount)];
    }
    const char* a = [strUUID UTF8String];
    char* pRet = (char*)malloc(strlen(a) + 1);
    strcpy(pRet, a);
    return pRet;
}

void _setAuthenticate(bool called)
{
    [HabbySDKAPI SetAuthenticate: called];
}


@implementation HabbySDKAPI

static bool isCalledAuthenticate = false;

+ (void)SetAuthenticate:(bool) isCalled
{
    isCalledAuthenticate = isCalled;
}

+(GKLocalPlayer *) LocalPlayer
{
    return [GKLocalPlayer localPlayer];
}
+(BOOL)IsAuthenticate
{
    GKLocalPlayer * localPlayer =  [HabbySDKAPI LocalPlayer];
    if(localPlayer == nil || localPlayer == NULL) return false;
    
    if (@available(iOS 12.4, *)) {
        return localPlayer.playerID != nil && localPlayer.playerID != NULL;
        
    } else {
        return localPlayer.playerID != nil && localPlayer.playerID != NULL;
    }
    
}

+(const char*)GetPlayerIdDictionary
{
    if(![HabbySDKAPI IsAuthenticate]) return NULL;
    
    NSMutableDictionary *dictionary = [[NSMutableDictionary alloc] init];
    
    [dictionary setValue:[HabbySDKAPI LocalPlayer].playerID forKey:@"playerID"];
    
    if (@available(iOS 12.4, *)) {
        [dictionary setValue:[HabbySDKAPI LocalPlayer].teamPlayerID forKey:@"teamPlayerID"];
        [dictionary setValue:[HabbySDKAPI LocalPlayer].gamePlayerID forKey:@"gamePlayerID"];
    } else {
        // Fallback on earlier versions
    }
    
    [dictionary setValue:[HabbySDKAPI LocalPlayer].displayName forKey:@"displayName"];

    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary
                                                options:NSJSONWritingPrettyPrinted
                                                  error:&error];
    if(error != nil)
    {
        NSLog(@"GetPlayerIdDictionary failed");
    }
    
    NSString *jsonString  = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return [jsonString UTF8String];
}


+ (void)LoginGameCenter:(int) seqId
{
    int tseqid = seqId;
    
    GKLocalPlayer* tplayer = [HabbySDKAPI LocalPlayer];
    
    
    if([HabbySDKAPI IsAuthenticate])
    {
        NSLog(@"gamecenter，Login is successed. : %d", seqId);
        
        [HabbySDKAPI OnLoginGameCenterCallBack:tseqid forError:nil forCode:0];
    }
    else
    {

        if(isCalledAuthenticate)
        {
            bool tisauthent = tplayer.isAuthenticated;
            NSLog(@"gamecenter，isAuthenticated = %@ .but playerId or teamPlayerId = null.: %d",[NSString stringWithFormat:@"%d",tisauthent], seqId);
            [HabbySDKAPI OnLoginGameCenterCallBack:tseqid forError:@"You must login to GameCenter first." forCode:6];
            return;
        }
        
        
        NSLog(@"gamecenter，startLogin. : %d", seqId);
        
        
        isCalledAuthenticate = true;
        tplayer.authenticateHandler =^(UIViewController * _Nullable viewController, NSError * _Nullable error)
        {
            
            int code = 0;
            NSString * errorStr = nil;
            if(error != nil)
            {
                code = (int)error.code;
                errorStr =[error localizedDescription];
            }

            [HabbySDKAPI OnLoginGameCenterCallBack:tseqid forError:errorStr forCode:code];
        };

        //[[HabbySDKAPI LocalPlayer] setAuthenticateHandler:^(UIViewController * _Nullable viewController, NSError * _Nullable error)
        //{

           // [HabbySDKAPI OnLoginGameCenterCallBack:tseqid forError:error];
       // }];
    }
    
}

+ (void)OnLoginGameCenterCallBack:(int) seqId forError:(NSString * _Nullable) error forCode:(int) code
{
    NSLog(@"gamecenter，playerID : %@ , displayName : %@", [HabbySDKAPI LocalPlayer].playerID, [HabbySDKAPI LocalPlayer].displayName);
    
    NSMutableDictionary *dictionary = [[NSMutableDictionary alloc] init];

    if(error != nil && error != NULL)
    {
        [dictionary setValue:error forKey:@"error"];
    }
    
    [HabbySDKAPI AddPlayerIdToDictionary:dictionary forSeq:seqId forEvent:@"OnLogin" forCode:code];
    
    [HabbySDKAPI SendJsonToUnity:dictionary forFun:"OnRecAccountMessage"];
}

+ (void)GetIdentityVerificationSignature:(int) seqId
{
    int tseqid = seqId;
    if (@available(iOS 13.5, *)) {
        [[HabbySDKAPI LocalPlayer] fetchItemsForIdentityVerificationSignature:^(NSURL * _Nullable publicKeyURL, NSData * _Nullable signature, NSData * _Nullable salt, uint64_t timestamp, NSError * _Nullable error) {
            [HabbySDKAPI OnIdentitySignatureComplete:tseqid forUrl:publicKeyURL forSign:signature forSalt:salt forTimetamp:timestamp forMethodTag:@"1" forError:error];
        }];
    } else {
        [[HabbySDKAPI LocalPlayer] generateIdentityVerificationSignatureWithCompletionHandler:^(NSURL * _Nullable publicKeyUrl, NSData * _Nullable signature, NSData * _Nullable salt, uint64_t timestamp, NSError * _Nullable error) {
            [HabbySDKAPI OnIdentitySignatureComplete:tseqid forUrl:publicKeyUrl forSign:signature forSalt:salt forTimetamp:timestamp forMethodTag:@"2" forError:error];
        }];
    }
}

+(void)OnIdentitySignatureComplete:(int)seqId forUrl:(NSURL*) publicKeyUrl forSign:(NSData*)signature forSalt:(NSData*)salt forTimetamp:(uint64_t)timestamp    forMethodTag:(NSString*)methodTag forError:(NSError*)error
{
    
    
    NSMutableDictionary *dictionary = [[NSMutableDictionary alloc] init];
    
    int code = 0;
    if(error == nil)
    {
        [dictionary setValue:methodTag forKey:@"methodTag"];
        [dictionary setValue:[publicKeyUrl absoluteString] forKey:@"publicKeyURL"];
        [dictionary setValue:[salt base64EncodedStringWithOptions:0] forKey:@"salt"];
        [dictionary setValue:[NSString stringWithFormat:@"%llu",timestamp] forKey:@"timestamp"];
        [dictionary setValue:[signature base64EncodedStringWithOptions:0] forKey:@"signature"];
        
    }
    else
    {
        code = (int)error.code;
        [dictionary setValue:[error localizedDescription] forKey:@"error"];
        NSLog(@"IdentityVerificationSignature failed:%@", [error localizedDescription]);
    }
    
    [HabbySDKAPI AddPlayerIdToDictionary:dictionary forSeq:seqId forEvent:@"OnIdentityVerificationSignature" forCode:code];
    
    [HabbySDKAPI SendJsonToUnity:dictionary forFun:"OnRecAccountMessage"];
}

+(void)AddPlayerIdToDictionary:(NSMutableDictionary*) dictionary forSeq:(int)seqid forEvent:(NSString *)eventName forCode:(int)code
{
    
    if(dictionary == nil) return;
    GKLocalPlayer* tplayer = [HabbySDKAPI LocalPlayer];
    
    [dictionary setValue:[NSString stringWithFormat:@"%d",code] forKey:@"code"];
    [dictionary setValue:[NSString stringWithFormat:@"%d",seqid] forKey:@"seqId"];
    [dictionary setValue:eventName forKey:@"eventName"];

    if(code == 0)
    {
        bool tisauthent = tplayer.isAuthenticated;

        [dictionary setValue:tplayer.playerID forKey:@"playerID"];
        
        if(tisauthent)
        {
            if (@available(iOS 12.4, *)) {
                [dictionary setValue:tplayer.teamPlayerID forKey:@"teamPlayerID"];
                [dictionary setValue:tplayer.gamePlayerID forKey:@"gamePlayerID"];
            } else {
                // Fallback on earlier versions
            }
        }
        else
        {
            NSLog(@"gamecenter，AddPlayerIdToDictionary not login .isAuthenticated = %@",[NSString stringWithFormat:@"%d",tisauthent]);
        }

    }

    [dictionary setValue:tplayer.displayName forKey:@"displayName"];
}

+(void)SendJsonToUnity:(NSMutableDictionary*) dictionary forFun:(const char *)funName
{
    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary
                                                options:NSJSONWritingPrettyPrinted
                                                  error:&error];
    
    if(error != nil)
    {
        NSLog(@"SendJsonToUnity %@ failed. %@",[NSString stringWithUTF8String:funName],[error localizedDescription]);
    }
    
    NSString *jsonString  = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    // NSLog(@"SendJsonToUnity json = %@",jsonString);
    
    UnitySendMessage("HabbySDKLauncher", funName, [jsonString UTF8String]);
}
@end

