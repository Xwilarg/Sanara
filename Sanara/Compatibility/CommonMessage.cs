﻿using Discord;

namespace Sanara.Compatibility;

public class CommonMessage
{
    public CommonMessage(Discord.IMessage msg)
    {
        CreatedAt = msg.CreatedAt;
    }

    public CommonMessage(RevoltSharp.Message msg)
    {
        CreatedAt = msg.CreatedAt;
    }

    public DateTimeOffset CreatedAt { private set; get; }
}
