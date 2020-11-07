using System;

namespace SanaraV3.Help
{
    [Flags]
    public enum Restriction
    {
        None = 0,
        Nsfw = 1,
        AdminOnly = 2,
        OwnerOnly = 4,
        PremiumOnly = 8
    }
}
