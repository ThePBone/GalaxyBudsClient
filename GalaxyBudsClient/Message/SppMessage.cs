﻿using System;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Sentry;
using Serilog;

namespace GalaxyBudsClient.Message;

public partial class SppMessage(SppMessage.MessageIds id, SppMessage.MsgType type, byte[] payload)
{
    private const int CrcSize = 2;
    private const int MsgIdSize = 1;
    private const int SomSize = 1;
    private const int EomSize = 1;
    private const int TypeSize = 1;
    private const int BytesSize = 1;

    public MsgType Type { set; get; } = type;
    public MessageIds Id { set; get; } = id;
    public int Size => MsgIdSize + Payload.Length + CrcSize;
    public int TotalPacketSize => SomSize + TypeSize + BytesSize + MsgIdSize + Payload.Length + CrcSize + EomSize;
    public byte[] Payload { set; get; } = payload;
    public int Crc16 { private set; get; }
        
    /* No Buds support at the moment */
    public bool IsFragment { set; get; }

    public SppMessage() : this(MessageIds.UNKNOWN_0, MsgType.Request, Array.Empty<byte>())
    {
    }

    public BaseMessageParser? BuildParser()
    {
        return SppMessageParserFactory.BuildParser(this);
    }

    public byte[] EncodeMessage()
    {
        var msg = new byte[TotalPacketSize];
            
        if (BluetoothService.ActiveModel != Models.Buds)
        {
            msg[0] = (byte)Constants.SOMPlus;
                
            /* Generate header */
            var header = BitConverter.GetBytes((short)Size);
            if (IsFragment) {
                header[1] = (byte) (header[1] | 32);
            }
            if (Type == MsgType.Response) {
                header[1] = (byte) (header[1] | 16);
            }

            msg[1] = header[0];
            msg[2] = header[1];
        }
        else
        {
            msg[0] = (byte)Constants.SOM;
            msg[1] = (byte)Type;
            msg[2] = (byte)Size;
        }

        msg[3] = (byte)Id;

        Array.Copy(Payload, 0, msg, 4, Payload.Length);

        var crcData = new byte[Size - 2];
        crcData[0] = msg[3];
        Array.Copy(Payload, 0, crcData, 1, Payload.Length);
        var crc16 = Utils.Crc16.crc16_ccitt(crcData);
        msg[4 + Payload.Length] = (byte)(crc16 & 255);
        msg[4 + Payload.Length + 1] = (byte)((crc16 >> 8) & 255);

        if (BluetoothService.ActiveModel != Models.Buds)
        {
            msg[TotalPacketSize - 1] = (byte)Constants.EOMPlus;
        }
        else
        {
            msg[TotalPacketSize - 1] = (byte)Constants.EOM;
        }

        return msg;
    }

    /**
      * Static "constructors"
      */
    public static SppMessage DecodeMessage(byte[] raw)
    {
        try
        {
            var draft = new SppMessage();

            if (raw.Length < 6)
            {
                SentrySdk.AddBreadcrumb($"Message too small (Length: {raw.Length})", "spp",
                    level: BreadcrumbLevel.Warning);
                Log.Error("Message too small (Length: {Length})", raw.Length);
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.TooSmall,Loc.Resolve("sppmsg_too_small"));
            }
                
            if ((raw[0] != (byte) Constants.SOM &&
                 BluetoothService.ActiveModel == Models.Buds) ||
                (raw[0] != (byte) Constants.SOMPlus &&
                 BluetoothService.ActiveModel != Models.Buds))
            {
                SentrySdk.AddBreadcrumb($"Invalid SOM (Received: {raw[0]})", "spp",
                    level: BreadcrumbLevel.Warning);
                Log.Error("Invalid SOM (Received: {SomByte})", raw[0]);
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.SOM,Loc.Resolve("sppmsg_invalid_som"));
            }

            draft.Id = (MessageIds) Convert.ToInt32(raw[3]);
            int size;

            if (BluetoothService.ActiveModel != Models.Buds)
            {
                var p1 = raw[2] << 8;
                var p2 = raw[1] & 255;
                var header = p1 + p2;
                draft.IsFragment = (header & 8192) != 0;
                draft.Type = (header & 4096) != 0 ? MsgType.Request : MsgType.Response;
                size = header & 1023;
            }
            else
            {
                draft.Type = (MsgType) Convert.ToInt32(raw[1]);
                size = Convert.ToInt32(raw[2]);
            }

            //Subtract Id and CRC from size
            var rawPayloadSize = size - 3;
            if (rawPayloadSize < 0)
            {
                rawPayloadSize = 0;
                size = 3;
            }
            var payload = new byte[rawPayloadSize];

            var crcData = new byte[size];
            crcData[0] = raw[3]; //Msg ID

            for (var i = 0; i < rawPayloadSize; i++)
            {
                //Start to read at byte 4
                payload[i] = raw[i + 4];
                crcData[i + 1] = raw[i + 4];
            }

            var crc1 = raw[4 + rawPayloadSize];
            var crc2 = raw[4 + rawPayloadSize + 1];
            crcData[^2] = crc2;
            crcData[^1] = crc1;

            draft.Payload = payload;
            draft.Crc16 = Utils.Crc16.crc16_ccitt(crcData);

            if (size != draft.Size)
            {
                SentrySdk.AddBreadcrumb($"Invalid size (Reported: {size}, Calculated: {draft.Size})", "spp",
                    level: BreadcrumbLevel.Warning);
                Log.Error("Invalid size (Reported: {Size}, Calculated: {DraftSize})", size, draft.Size);
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.SizeMismatch,Loc.Resolve("sppmsg_size_mismatch"));
            }

            if (draft.Crc16 != 0)
            {
                SentrySdk.AddBreadcrumb($"CRC checksum failed (ID: {draft.Id}, Size: {draft.Size})", "spp",
                    level: BreadcrumbLevel.Warning);
                Log.Error("CRC checksum failed (ID: {Id}, Size: {Size})", draft.Id, draft.Size);
                //throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Checksum,Loc.Resolve("sppmsg_crc_fail"), draft);
            }

            if ((raw[draft.TotalPacketSize - 1] != (byte) Constants.EOM && BluetoothService.ActiveModel == Models.Buds) ||
                (raw[draft.TotalPacketSize - 1] != (byte) Constants.EOMPlus && BluetoothService.ActiveModel != Models.Buds))
            {
                SentrySdk.AddBreadcrumb($"Invalid EOM (Received: {raw[4 + rawPayloadSize + 2]})", "spp",
                    level: BreadcrumbLevel.Warning);
                Log.Error("Invalid EOM (Received: {EomByte}", raw[4 + rawPayloadSize + 2]);
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.EOM,Loc.Resolve("sppmsg_invalid_eom"), draft);
            }

            return draft;
        }
        catch (IndexOutOfRangeException)
        {
            SentrySdk.AddBreadcrumb("IndexOutOfRangeException");
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("raw-data-available", "true");
                scope.SetExtra("raw-data", HexUtils.Dump(raw, 512, false, false, false));
            });
            throw new InvalidPacketException(InvalidPacketException.ErrorCodes.OutOfRange,"OutOfRange. Update your firmware!");
        }
        catch (OverflowException ex)
        {
            SentrySdk.AddBreadcrumb("OverflowException");
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("raw-data-available", "true");
                scope.SetExtra("raw-data", HexUtils.Dump(raw, 512, false, false, false));
            });
            SentrySdk.CaptureException(ex);

            throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Overflow,"Overflow. Update your firmware!");
        }
    }

    public override string ToString()
    {
        return $"SPPMessage[MessageID={Id},PayloadSize={Size},Type={(IsFragment ? "Fragment/" : string.Empty) + Type},CRC16={Crc16}," +
               $"Payload={{{BitConverter.ToString(Payload).Replace("-", " ")}}}]";
    }

}