using System;
using System.ComponentModel;

namespace elbsms_core.Video
{
    [Flags]
    enum VDPStatusFlags : byte
    {
        [Description("Sprite Collision")]
        COL = 0x20,
        [Description("Sprite Overflow")]
        OVR = 0x40,
        [Description("Pending Frame Interrupt")]
        INT = 0x20
    }
}
