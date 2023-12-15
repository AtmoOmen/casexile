using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;

namespace XivCommon.Functions.FriendList {
    /// <summary>
    /// An entry in a player's friend list.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    public unsafe struct FriendListEntry {
        internal const int Size = 104;

        /// <summary>
        /// The content ID of the friend.
        /// </summary>
        [FieldOffset(0x00)]
        public readonly ulong ContentId;

        /// <summary>
        /// The current world of the friend.
        /// </summary>
        [FieldOffset(0x1E)]
        public readonly ushort CurrentWorld;

        /// <summary>
        /// The home world of the friend.
        /// </summary>
        [FieldOffset(0x20)]
        public readonly ushort HomeWorld;

        /// <summary>
        /// The job the friend is currently on.
        /// </summary>
        [FieldOffset(0x29)]
        public readonly byte Job;

        /// <summary>
        /// The friend's raw SeString name. See <see cref="Name"/>.
        /// </summary>
        [FieldOffset(0x2A)]
        public fixed byte RawName[32];

        /// <summary>
        /// The friend's raw SeString free company tag. See <see cref="FreeCompany"/>.
        /// </summary>
        [FieldOffset(0x4A)]
        public fixed byte RawFreeCompany[5];

        /// <summary>
        /// The friend's name.
        /// </summary>
        public SeString Name {
            get {
                fixed (byte* ptr = this.RawName) {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr) ptr);
                }
            }
        }

        /// <summary>
        /// The friend's free company tag.
        /// </summary>
        public SeString FreeCompany {
            get {
                fixed (byte* ptr = this.RawFreeCompany) {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr) ptr);
                }
            }
        }
    }
}
