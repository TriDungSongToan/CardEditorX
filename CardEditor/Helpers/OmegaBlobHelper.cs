using System;
using System.Collections.Generic;

namespace CardEditor.Helpers
{
    public static class OmegaBlobHelper
    {
        public static ulong BlobToUInt64(byte[]? blob)
        {
            if (blob == null || blob.Length == 0)
                return 0;

            if (blob.Length > sizeof(ulong))
                throw new InvalidOperationException($"BLOB length ({blob.Length} bytes) exceeds the maximum supported UInt64 size of 8 bytes.");

            ulong value = 0;

            for (var i = 0; i < blob.Length; i++)
            {
                value |= (ulong)blob[i] << (i * 8);
            }

            return value;
        }
        public static byte[]? UInt64ToBlob(ulong value)
        {
            // Đồng bộ DataEditorX: 0 được ghi thành NULL.
            if (value == 0) return null;

            var bytes = new List<byte>();

            while (value > 0)
            {
                bytes.Add((byte)(value & 0xFF));
                value >>= 8;
            }

            return bytes.ToArray();
        }
    }
}
