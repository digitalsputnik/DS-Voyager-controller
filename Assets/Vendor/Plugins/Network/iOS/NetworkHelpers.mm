//
//  NetworkHelpers.mm
//
//  Created by Taavet Maask on 04.05.2020.
//  Copyright © 2020 Taavet Maask. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <SystemConfiguration/CaptiveNetwork.h>
#import <CoreLocation/CoreLocation.h>

@interface NetworkHelper : NSObject

@end

@implementation NetworkHelper
+ (NSString *)GetCurrentWifiHotSpotName {
    NSString *wifiName = nil;
    NSArray *interfaceNames = CFBridgingRelease(CNCopySupportedInterfaces());
    NSLog(@"%s: Supported interfaces: %@", __func__, interfaceNames);

    NSDictionary *SSIDInfo;
    for (NSString *interfaceName in interfaceNames) {
        SSIDInfo = CFBridgingRelease(CNCopyCurrentNetworkInfo((__bridge CFStringRef)interfaceName));
        NSLog(@"%s: %@ => %@", __func__, interfaceName, SSIDInfo);
        BOOL isNotEmpty = (SSIDInfo.count > 0);
        if (isNotEmpty) {
            wifiName = SSIDInfo[@"SSID"];
            break;
        }
    }
    
    if (wifiName == nil) {
        return @"unknown";
    } else {
        return wifiName;
    }
}
@end

char* convertNSStringToCString(const NSString* nsString)
{
    if (nsString == NULL)
        return NULL;
    const char* nsStringUtf8 = [nsString UTF8String];
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);
    return cString;
}

extern "C" {
    char* _iOSGetSsidName()
    {
        return convertNSStringToCString([NetworkHelper GetCurrentWifiHotSpotName]);
    }
}
