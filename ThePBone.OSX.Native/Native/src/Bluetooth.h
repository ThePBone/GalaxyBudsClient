//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#import <Foundation/Foundation.h>
#import <IOBluetooth/IOBluetooth.h>

enum BT_CONN_RESULT {
    BT_CONN_SUCCESS = 0x00,
    BT_CONN_EBASECONN,
    BT_CONN_ENOTFOUND,
    BT_CONN_ENOTPAIRED,
    BT_CONN_ESDP,
    BT_CONN_ECID,
    BT_CONN_EOPEN,
    BT_CONN_ENULL,
    BT_CONN_EUNKNOWN
};

enum BT_SEND_RESULT {
    BT_SEND_SUCCESS = 0x00,
    BT_SEND_EPARTIAL,
    BT_SEND_EUNKNOWN,
    BT_SEND_ENULL
};

enum BT_ENUM_RESULT {
    BT_ENUM_SUCCESS = 0x00,
    BT_ENUM_EUNKNOWN
};

struct Device {
    char *mac_address;
    char *device_name;
    bool is_connected;
    bool is_paired;
    uint32_t cod;
};

struct EnumerationResult {
    int length;
    struct Device *devices;
};

@class IOBluetoothDevice;
@class IOBluetoothRFCOMMChannel;

typedef void (*Bt_OnChannelData)(void *data, unsigned long size);
typedef void (*Bt_OnChannelClosed)(void);

@interface Bluetooth<IOBluetoothRFCOMMChannelDelegate, IOBluetoothDeviceAsyncCallbacks> : NSObject {
    __strong IOBluetoothRFCOMMChannel *mRFCOMMChannel;
    bool sdpQueryDone;
}

- (id)init;
- (BT_CONN_RESULT)connect:(NSString *)mac uuid:(const UInt8 *)uuid;
- (BT_ENUM_RESULT)enumerate:(EnumerationResult *)result;
- (BOOL)disconnect;
- (BOOL)isConnected;
- (BT_SEND_RESULT)sendData:(char *)buffer length:(UInt32)length;

- (void)setOnChannelData:(Bt_OnChannelData)callback;
- (void)setOnChannelClosed:(Bt_OnChannelClosed)callback;

- (NSString *)currentMac;

+ (BOOL)getDevice:(NSString *)nsId result:(IOBluetoothDevice **)device;

// Implementation of delegate calls (see IOBluetoothRFCOMMChannel.h)
- (void)rfcommChannelData:(IOBluetoothRFCOMMChannel *)rfcommChannel data:(void *)dataPointer length:(size_t)dataLength;
- (void)rfcommChannelClosed:(IOBluetoothRFCOMMChannel *)rfcommChannel;
- (void)sdpQueryComplete:(IOBluetoothDevice *)device status:(IOReturn)status;
@end
