//  IosVideoResizer.mm
//  Created by Taavet Maask on 16.07.2020.
//  Copyright © 2020 Taavet Maask. All rights reserved.

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import "SDAVAssetExportSession.h"

@interface VideoResizer : NSObject @end

@implementation VideoResizer
    + (void)resize:(NSString*) input :(NSString *)output :(NSNumber *)width :(NSNumber *)height
    {
        NSURL* inputURL = [NSURL fileURLWithPath:input]; //[NSURL URLWithString:input];
        NSURL* outputURL = [NSURL fileURLWithPath:output];
        
        AVAsset* asset = [AVAsset assetWithURL:inputURL];
        
        SDAVAssetExportSession *encoder = [SDAVAssetExportSession.alloc initWithAsset:asset];
        encoder.outputFileType = AVFileTypeMPEG4;
        encoder.outputURL = outputURL;
        encoder.videoSettings = @
        {
            AVVideoCodecKey: AVVideoCodecTypeH264,
            AVVideoWidthKey: width,
            AVVideoHeightKey: height,
            AVVideoCompressionPropertiesKey: @
            {
                AVVideoAverageBitRateKey: @6000000,
                AVVideoProfileLevelKey: AVVideoProfileLevelH264High40,
            },
        };
        encoder.audioSettings = @
        {
            AVFormatIDKey: @(kAudioFormatMPEG4AAC),
            AVNumberOfChannelsKey: @2,
            AVSampleRateKey: @44100,
            AVEncoderBitRateKey: @128000,
        };

        [encoder exportAsynchronouslyWithCompletionHandler:^
        {
            if (encoder.status == AVAssetExportSessionStatusCompleted)
            {
                NSString *data = [NSString stringWithFormat:@"%@", outputURL.absoluteString];
                [self callUnityObject:"iOS Video Resize Listener" Method:"ResizeDone" Parameter:[data  UTF8String]];
            }
            else if (encoder.status == AVAssetExportSessionStatusCancelled)
            {
                NSString *data = [NSString stringWithFormat:@"%@", encoder.error];
                [self callUnityObject:"iOS Video Resize Listener" Method:"ResizeError" Parameter:[data  UTF8String]];
            }
            else
            {
                NSString *data = [NSString stringWithFormat:@"%@", encoder.error];
                [self callUnityObject:"iOS Video Resize Listener" Method:"ResizeError" Parameter:[data  UTF8String]];
            }
        }];
    }

    + (void)callUnityObject:(const char*)object Method:(const char*)method Parameter:(const char*)parameter
    {
        UnitySendMessage(object, method, parameter);
    }
@end

extern "C" {
    void _iOS_VideoResizer_ResizeVideo(const char* path, const char* output, const int width, const int height)
    {
        NSString* inputStr = [[NSString alloc] initWithUTF8String:path];
        NSString* outputStr = [[NSString alloc] initWithUTF8String:output];
        NSNumber* widthNum = [NSNumber numberWithInt:width];
        NSNumber* heightNum = [NSNumber numberWithInt:height];
        [VideoResizer resize:inputStr :outputStr :widthNum :heightNum];
    }
}
