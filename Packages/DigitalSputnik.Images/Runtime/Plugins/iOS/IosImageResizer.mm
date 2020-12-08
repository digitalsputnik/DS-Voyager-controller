//  IosImageResizer.m
//  Created by Taavet Maask on 07.12.2020.

#import <Foundation/Foundation.h>
#import "UIImage+ResizeMagick.h"

@interface ImageResizer : NSObject @end

@implementation ImageResizer
    + (void)resize:(NSString*) input :(NSString *)output :(NSNumber *)width :(NSNumber *)height
    {
        NSURL* inputURL = [NSURL fileURLWithPath:input];
        NSData* imageData = [NSData dataWithContentsOfURL:inputURL];
        UIImage* image = [UIImage imageWithData:imageData];
        NSString* command = [NSString stringWithFormat:@"%@x%@", width, height];
        UIImage* resizedImage = [image resizedImageByMagick: command];
        
        [UIImagePNGRepresentation(resizedImage) writeToFile:output atomically:YES];
        
        NSString *data = [NSString stringWithFormat:@"%@", output];
        [self callUnityObject:"iOS Image Resize Listener" Method:"ResizeDone" Parameter:[data  UTF8String]];
    }

    + (void)callUnityObject:(const char*)object Method:(const char*)method Parameter:(const char*)parameter
    {
        UnitySendMessage(object, method, parameter);
    }
@end

extern "C" {
    void _iOS_ImageResizer_ResizeImageBetween(const char* path, const char* output, const int width, const int height)
    {
        NSString* inputStr = [[NSString alloc] initWithUTF8String:path];
        NSString* outputStr = [[NSString alloc] initWithUTF8String:output];
        NSNumber* widthNum = [NSNumber numberWithInt:width];
        NSNumber* heightNum = [NSNumber numberWithInt:height];
        
        [ImageResizer resize:inputStr :outputStr :widthNum :heightNum];
    }
}
