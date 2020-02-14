//
//  BluetoothInterface.m
//
//  Created by Taavet Maask on 08.01.2020.
//  Copyright © 2020 Taavet Maask. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

@interface BluetoothInterface : NSObject <CBCentralManagerDelegate, CBPeripheralDelegate>
    @property (strong, nonatomic) CBCentralManager *centralManager;
    @property (strong, nonatomic) NSMutableArray *peripheralsList;
    @property (strong, nonatomic) CBPeripheral *connectedPeripheral;
@end

@implementation BluetoothInterface
    static BluetoothInterface *instance;

    + (BluetoothInterface*)shared
    {
        return instance;
    }

    + (void)initialize
    {
        instance = [BluetoothInterface alloc];
        [instance initializeBluetooth];
    }

    - (void)startScanning
    {
        [_peripheralsList removeAllObjects];
        NSDictionary *options = [NSDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithBool:YES], CBCentralManagerScanOptionAllowDuplicatesKey, nil];
        [_centralManager scanForPeripheralsWithServices:nil options:options];
    }

    - (void)stopScanning
    {
        [_centralManager stopScan];
    }

    - (void)connect:(NSString *)uid
    {
        NSLog(@"Searching for connection %@", uid);
        for (CBPeripheral* peri in _peripheralsList)
        {
            if ([uid isEqualToString:[[peri identifier] UUIDString]])
            {
                [_centralManager connectPeripheral:peri options:nil];
                NSLog(@"Connecting to %@", uid);
                NSString *data = [NSString stringWithFormat:@"%@", uid];
                [self callUnityObject:"iOS Bluetooth Listener" Method:"ConnectingStarted" Parameter:[data  UTF8String]];
                return;
            }
        }
        
        NSString *data = [NSString stringWithFormat:@"%@", uid];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"PeripheralNotFound" Parameter:[data  UTF8String]];
    }

    - (void)cancelConnection:(NSString *)uid
    {
        for (CBPeripheral* peri in _peripheralsList)
        {
            if ([uid isEqualToString:[[peri identifier] UUIDString]])
            {
                [_centralManager cancelPeripheralConnection:peri];
                NSLog(@"Canceling connecting with %@", uid);
                return;
            }
        }
    }

    - (void)discoverCharacteristics:(NSString *)service
    {
        for (CBService* serv in _connectedPeripheral.services)
        {
            if ([serv.UUID.description isEqualToString:service])
            {
                [_connectedPeripheral discoverCharacteristics:nil forService:serv];
                return;
            }
        }
    }

    - (void)getServices
    {
        [_connectedPeripheral discoverServices:nil];
    }

    - (void)subscribeToCharacteristic:(NSString *)service :(NSString *)characteristic
    {
        for (CBService* serv in _connectedPeripheral.services)
        {
            if ([serv.UUID.description isEqualToString:service])
            {
                for (CBCharacteristic* charac in serv.characteristics)
                {
                    if ([charac.UUID.description isEqualToString:characteristic])
                    {
                        [_connectedPeripheral setNotifyValue:true forCharacteristic:charac];
                    }
                }
            }
        }
    }

    - (void)writeToCharacteristic:(NSString *)service :(NSString *)characteristic :(NSData *)data
    {
        for (CBService* serv in _connectedPeripheral.services)
        {
            if ([serv.UUID.description isEqualToString:service])
            {
                for (CBCharacteristic* charac in serv.characteristics)
                {
                    if ([charac.UUID.description isEqualToString:characteristic])
                    {
                        [_connectedPeripheral writeValue:data forCharacteristic:charac type:CBCharacteristicWriteWithResponse];
                    }
                }
            }
        }
    }

    - (void)initializeBluetooth
    {
        _centralManager = [[CBCentralManager alloc] initWithDelegate:self queue:nil];
        _peripheralsList = [[NSMutableArray alloc] init];
        NSLog(@"BluetoothInterface initialized.");
    }

    - (void)centralManagerDidUpdateState:(CBCentralManager *)central
    {
        if (central.state != CBManagerStatePoweredOn)
        {
            NSLog(@"Bluetooth is not ready!");
        }
        
        if (central.state == CBManagerStatePoweredOn)
        {
            NSLog(@"Ready to scan!");
        }
    }

    - (void)centralManager:(CBCentralManager *)central didDiscoverPeripheral:(CBPeripheral *)peripheral advertisementData:(NSDictionary<NSString *,id> *)advertisementData RSSI:(NSNumber *)RSSI
    {
        bool exists = false;
        for (CBPeripheral* peri in _peripheralsList)
        {
            if ([peripheral identifier] == [peri identifier])
            {
                exists = true;
            }
        }
        
        if (!exists)
        {
            NSLog(@"Discovered new peripheral %@ (%@) at %@", peripheral.name, [peripheral identifier], RSSI);
            [_peripheralsList addObject:peripheral];
        }
        
        NSString *data = [NSString stringWithFormat:@"%@|%@|%@", [peripheral identifier], peripheral.name, RSSI];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"PeripheralScanned" Parameter:[data  UTF8String]];
    }

    - (void)centralManager:(CBCentralManager *)central didConnectPeripheral:(CBPeripheral *)peripheral {
        NSLog(@"Connecting to %@ was successful!", [peripheral identifier]);
        NSString *data = [NSString stringWithFormat:@"%@", [peripheral identifier]];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"ConnectionSuccessful" Parameter:[data  UTF8String]];
        
        _connectedPeripheral = peripheral;
        _connectedPeripheral.delegate = self;
        
        if (@available(iOS 11.0, *))
        {
            [_connectedPeripheral canSendWriteWithoutResponse];
        }
    }

    - (void)centralManager:(CBCentralManager *)central didFailToConnectPeripheral:(CBPeripheral *)peripheral error:(NSError *)error {
        NSLog(@"Connecting to %@ was failed: %@", [peripheral identifier], error);
        NSString *data = [NSString stringWithFormat:@"%@|%@", [peripheral identifier], error];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"ConnectionFailed" Parameter:[data  UTF8String]];
    }

    - (void)centralManager:(CBCentralManager *)central didDisconnectPeripheral:(CBPeripheral *)peripheral error:(NSError *)error
    {
        NSLog(@"Disconnecting from %@ : %@", [peripheral identifier], error);
        NSString *data = [NSString stringWithFormat:@"%@|%@", [peripheral identifier], error];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"Disconnect" Parameter:[data  UTF8String]];
    }

    - (void)peripheral:(CBPeripheral *)peripheral didDiscoverServices:(NSError *)error
    {
        NSLog(@"Services found %@ : %@", [peripheral services], error);
        
        NSMutableArray *services = [[NSMutableArray alloc] init];
        for (CBService* service in peripheral.services)
        {
            [services addObject:service.UUID];
        }
        
        NSString *data = [NSString stringWithFormat:@"%@|%@|%@", [peripheral identifier], [services componentsJoinedByString:@"#"], error];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"GetServices" Parameter:[data  UTF8String]];
    }

    - (void)peripheral:(CBPeripheral *)peripheral didDiscoverCharacteristicsForService:(CBService *)service error:(NSError *)error
    {
        NSMutableArray *chars = [[NSMutableArray alloc] init];
        for (CBCharacteristic* characteristic in service.characteristics)
        {
            [chars addObject:characteristic.UUID.description];
        }
        NSString *data = [NSString stringWithFormat:@"%@|%@|%@|%@", [peripheral identifier], service.UUID.description, [chars componentsJoinedByString:@"#"], error];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"GetCharacteristics" Parameter:[data  UTF8String]];
    }

    - (void)peripheral:(CBPeripheral *)peripheral didUpdateNotificationStateForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
    {
        [peripheral readValueForCharacteristic:characteristic];
    }

    - (void)peripheral:(CBPeripheral *)peripheral didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
    {
        NSString *data = [NSString stringWithFormat:@"%@|%@|%@|%@|%@", [peripheral identifier], characteristic.service.UUID.description, characteristic.UUID.description, error, [characteristic.value base64EncodedStringWithOptions:NSUTF8StringEncoding]];
        [self callUnityObject:"iOS Bluetooth Listener" Method:"UpdateCharacteristic" Parameter:[data  UTF8String]];
    }

    - (void)callUnityObject:(const char*)object Method:(const char*)method Parameter:(const char*)parameter
    {
        UnitySendMessage(object, method, parameter);
    }
@end

extern "C" {
    void _iOSInitialize()
    {
        [BluetoothInterface initialize];
    }

    void _iOSStartScanning()
    {
        [[BluetoothInterface shared] startScanning];
    }

    void _iOSStopScanning()
    {
        [[BluetoothInterface shared] stopScanning];
    }

    void _iOSConnect( const char* uid )
    {
        NSString* uidConv = [[NSString alloc] initWithUTF8String:uid];
        [[BluetoothInterface shared] connect:uidConv];
    }

    void _iOSCancelConnection( const char* uid )
    {
        NSString* uidConv = [[NSString alloc] initWithUTF8String:uid];
        [[BluetoothInterface shared] cancelConnection:uidConv];
    }

    void _iOSGetServices()
    {
        [[BluetoothInterface shared] getServices];
    }

    void _iOSGetCharacteristics( const char* service )
    {
        NSString* serviceConv = [[NSString alloc] initWithUTF8String:service];
        [[BluetoothInterface shared] discoverCharacteristics:serviceConv];
    }

    void _iOSSubscribeToCharacteristic( const char* service, const char* characteristic )
    {
        NSString* serviceConv = [[NSString alloc] initWithUTF8String:service];
        NSString* charConv = [[NSString alloc] initWithUTF8String:characteristic];
        [[BluetoothInterface shared] subscribeToCharacteristic:serviceConv :charConv];
    }

    void _iOSWriteToCharacteristic( const char* service, const char* characteristic, const Byte* data, const int length )
    {
        NSString* serviceConv = [[NSString alloc] initWithUTF8String:service];
        NSString* charConv = [[NSString alloc] initWithUTF8String:characteristic];
        NSData* dataConv = [[NSData alloc] initWithBytes:data length:length];
        [[BluetoothInterface shared] writeToCharacteristic:serviceConv :charConv :dataConv];
    }
}
