using System;
using System.Collections.Generic;
using System.Text;

namespace ReadMemory.Common {
    public static class Offsets {
        public const int BA = 0xE444A4; // base address
        public const int GA = 0xE44C4C; // game address
        public const int D_GA = 0x1C; // offset to GA
        public const int PacketCall = 0x819A40;

        public const int PERS_STRUCT = 0x34;

        public const int MY_MAX_HP = 0x520;
        public const int MY_CUR_HP = 0x4CC;
        public const int MY_LEVEL = 0x4F8;
        public const int TARGET_WID = 0x5A4;
        public const int LocX = 0x3C;
        public const int LocY = 0x44;
        public const int LocZ = 0x40;



        // Offsets to the mob structure
        public const int M_D1 = 0x1C; // first offset
        public const int M_D2 = 0x20; // second offset

        // Mob structure BA + D_GA + M_D1 + M_D2 +
        public const int M_COUNT = 0x60; // +
        public const int M_STRUCT = 0x5C; // +

        // Mob parameters M_STRUCT+
        public const int MOB_X = 0x03C; // +
        public const int MOB_Z = 0x040; // +
        public const int MOB_Y = 0x044; // +
        public const int MOB_TYPE = 0x0B4; // +
        public const int MOB_WID = 0x114; // +
        public const int MOB_LEVEL = 0x120; // +
        public const int MOB_CUR_HP = 0x128; // +
        public const int MOB_DIST_3D = 0x280; // +
        public const int MOB_DIST_2D = 0x284; // +
    }

}
