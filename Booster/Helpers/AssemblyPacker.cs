using System.IO;
using System.IO.Compression;

namespace Booster.Helpers
{
    internal static class AssemblyPacker
    {
        internal static byte[] Pack(string assemplyPath)
        {
            return Pack(File.ReadAllBytes(assemplyPath));
        }

        internal static byte[] UnPack(string packedAssemblyPath)
        {
            return UnPack(File.ReadAllBytes(packedAssemblyPath));
        }

        internal static byte[] Pack(byte[] rawAsswmbly)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(ms, CompressionMode.Compress))
                {
                    compressionStream.Write(rawAsswmbly, 0, rawAsswmbly.Length);
                }
                data = ms.ToArray();
            }
            ContentCryptoProvider ccp = new ContentCryptoProvider(data.Length);
            ccp.ProcessData(ref data);
            return data;
        }

        internal static byte[] UnPack(byte[] packedAsswmbly)
        {
            ContentCryptoProvider ccp = new ContentCryptoProvider(packedAsswmbly.Length);
            byte[] data = ccp.ProcessData(packedAsswmbly);
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (GZipStream decompressionStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressed = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int count;
                        while ((count = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            decompressed.Write(buffer, 0, count);
                        }
                        data = decompressed.ToArray();
                    }
                }
            }
            return data;
        }
    }
}
