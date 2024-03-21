﻿using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class NoiseReductionModeUpdateParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.NOISE_REDUCTION_MODE_UPDATE;
    public bool Enabled { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Enabled = Convert.ToBoolean(msg.Payload[0]);
    }
}