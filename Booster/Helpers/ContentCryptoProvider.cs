using System.Collections.Generic;

namespace Booster.Helpers
{
    /// <summary>
    /// Byte content Encryptor/Decrypor
    /// </summary>
    internal class ContentCryptoProvider
    {
        #region Mask
        List<byte> _maskTable = new List<byte>();
        long generator;

        byte NextMask()
        {
            byte next = (byte)(generator % 255);
            byte mask = 0;
            for (short p = 0; p < 8; p++)
            {
                mask |= (byte)(next & (2 << p));
            }
            generator = (a * generator + next) % m;
            return mask;
        }

        private const int a = 42;
        private const int m = 197;
        #endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="contentLength">Content legth</param>
        internal ContentCryptoProvider(long contentLength)
        {
            generator = contentLength;
        }
        /// <summary>
        /// Symmetric encrypt/decrypt data
        /// </summary>
        /// <param name="data"></param>
        internal void ProcessData(ref byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                int i = 0;
                if (data.Length > 0)
                {
                    data[0] = InversionBit(data[0], 3, 5, 6);
                    data[1] = InversionBit(data[1], 0, 1, 2, 4, 7);
                    data[2] = InversionBit(data[2], 1, 2, 3, 5);
                    data[3] = InversionBit(data[3], 0, 1, 2, 4, 7);
                    i = 4;
                }
                for (; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ NextMask());
                }
            }
        }

        internal byte[] ProcessData(byte[] data)
        {
            byte[] result = new byte[data.Length];
            if (data != null && data.Length > 0)
            {
                int i = 0;
                if (data.Length > 0)
                {
                    result[0] = InversionBit(data[0], 3, 5, 6);
                    result[1] = InversionBit(data[1], 0, 1, 2, 4, 7);
                    result[2] = InversionBit(data[2], 1, 2, 3, 5);
                    result[3] = InversionBit(data[3], 0, 1, 2, 4, 7);
                    i = 4;
                }
                for (; i < data.Length; i++)
                {
                    result[i] = (byte)(data[i] ^ NextMask());
                }
            }
            return result;
        }

        internal static byte InversionBit(byte original, params int[] positions)
        {
            byte result = original;
            foreach (int position in positions)
            {
                result ^= (byte)(1 << position);
            }
            return result;
        }
    }
}
