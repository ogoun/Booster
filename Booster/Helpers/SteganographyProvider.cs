using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Booster.Helpers
{
    internal static class SteganographyProvider
    {
        #region Helpers
        struct Pix
        {
            public byte blue;
            public byte green;
            public byte red;
            public byte alpha;
        }

        struct StByte
        {
            public byte a1;
            public byte a2;
            public byte a3;
            public byte a4;
        }

        private static StByte ByteDecomposition(byte b, bool alpha)
        {
            StByte result = new StByte();
            while (b > 99)
            {
                result.a1++;
                b -= 100;
            }
            while (b > 9)
            {
                result.a2++;
                b -= 10;
            }
            result.a3 = b;
            if (alpha)
            {
                if (result.a2 >= 5 && result.a3 >= 5)
                {
                    result.a4 = 2;
                    result.a2 -= 5;
                    result.a3 -= 5;
                }
                else if (result.a2 >= 5)
                {
                    result.a4 = 3;
                    result.a2 -= 5;
                }
                else if (result.a3 >= 5)
                {
                    result.a4 = 4;
                    result.a3 -= 5;
                }
                else
                {
                    result.a4 = 5;
                }
            }
            else
            {
                result.a4 = 5;
            }
            return result;
        }

        private static byte ByteComposition(StByte b, bool alpha)
        {
            byte result = 0;
            result += (byte)(b.a1 * 100);
            if (alpha)
            {
                switch (b.a4)
                {
                    case 2:
                        b.a2 += 5;
                        b.a3 += 5;
                        break;
                    case 3:
                        b.a2 += 5;
                        break;
                    case 4:
                        b.a3 += 5;
                        break;
                }
            }
            result += (byte)(b.a2 * 10);
            result += b.a3;
            return result;
        }

        private static Pix MixByteAndPixel(Pix p, StByte b)
        {
            p.blue = (byte)(((byte)(p.blue / 10)) * 10);
            if (p.blue == 250) p.blue = 240;
            p.blue += b.a1;
            p.green = (byte)(((byte)(p.green / 10)) * 10);
            if (p.green == 250) p.green = 240;
            p.green += b.a2;
            p.red = (byte)(((byte)(p.red / 10)) * 10);
            if (p.red == 250) p.red = 240;
            p.red += b.a3;
            p.alpha = (byte)(((byte)(p.alpha / 10)) * 10);
            if (p.alpha == 250) p.alpha = 240;
            p.alpha += b.a4;
            return p;
        }

        private static StByte SecretionByteFromPixel(Pix p)
        {
            StByte result = new StByte();
            result.a1 = (byte)(p.blue - ((byte)(p.blue / 10)) * 10);
            result.a2 = (byte)(p.green - ((byte)(p.green / 10)) * 10);
            result.a3 = (byte)(p.red - ((byte)(p.red / 10)) * 10);
            result.a4 = (byte)(p.alpha - ((byte)(p.alpha / 10)) * 10);
            return result;
        }
        #endregion

        private readonly static Random rnd;

        static SteganographyProvider()
        {
            rnd = new Random((int)Environment.TickCount);
        }

        internal static Bitmap Injection(byte[] original, byte[] content)
        {
            int index = 0;
            byte[] length = BitConverter.GetBytes(content.Length);
            byte[] data = new byte[content.Length + 4];
            Array.Copy(length, data, 4);
            Array.Copy(content, 0, data, 4, content.Length);
            var bmp = (Bitmap)Image.FromStream(new MemoryStream(original));
            {
                unsafe
                {
                    BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int size = bmd.Stride / bmp.Width;
                    bool hasAlpha = size == 4;
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            int offset = (y * bmd.Stride) + (x * size);
                            Pix pix = new Pix
                            {
                                blue = ((byte*)bmd.Scan0)[offset],
                                green = ((byte*)bmd.Scan0)[offset + 1],
                                red = ((byte*)bmd.Scan0)[offset + 2],
                                alpha = ((hasAlpha) ? ((byte*)bmd.Scan0)[offset + 3] : (byte)255)
                            };
                            if (index >= data.Length)
                            {
                                pix = MixByteAndPixel(pix, ByteDecomposition((byte)rnd.Next(0, 255), hasAlpha));
                                ((byte*)bmd.Scan0)[offset] = pix.blue;
                                ((byte*)bmd.Scan0)[offset + 1] = pix.green;
                                ((byte*)bmd.Scan0)[offset + 2] = pix.red;
                                if(hasAlpha)
                                    ((byte*)bmd.Scan0)[offset + 3] = pix.alpha;
                            }
                            else
                            {
                                pix = MixByteAndPixel(pix, ByteDecomposition(data[index], hasAlpha));
                                ((byte*)bmd.Scan0)[offset] = pix.blue;
                                ((byte*)bmd.Scan0)[offset + 1] = pix.green;
                                ((byte*)bmd.Scan0)[offset + 2] = pix.red;
                                if (hasAlpha)
                                    ((byte*)bmd.Scan0)[offset + 3] = pix.alpha;
                            }
                            index++;
                        }
                    }
                    bmp.UnlockBits(bmd);
                }
            }
            return bmp;
        }

        internal static byte[] Extraction(byte[] img)
        {
            byte[] length = new byte[4];
            byte[] data = null;
            using (var bmp = (Bitmap)Image.FromStream(new MemoryStream(img)))
            {
                int index = 0;
                bool end = false;
                unsafe
                {
                    BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                    int size = bmd.Stride / bmp.Width;
                    bool hasAlpha = size == 4;
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            int offset = (y * bmd.Stride) + (x * size);
                            Pix pix = new Pix
                            {
                                blue = ((byte*)bmd.Scan0)[offset],
                                green = ((byte*)bmd.Scan0)[offset + 1],
                                red = ((byte*)bmd.Scan0)[offset + 2],
                                alpha = ((hasAlpha) ? ((byte*)bmd.Scan0)[offset + 3] : (byte)255)
                            };
                            StByte b = SecretionByteFromPixel(pix);
                            byte wb = ByteComposition(b, hasAlpha);
                            if (data == null)
                            {
                                length[index] = wb;
                                index++;
                                if (index == 4)
                                {
                                    data = new byte[BitConverter.ToInt32(length, 0)];
                                    index = 0;
                                }
                            }
                            else
                            {
                                data[index] = wb;
                                index++;
                                if (index >= data.Length)
                                {
                                    end = true;
                                    break;
                                }
                            }
                        }
                        if (end) break;
                    }
                    bmp.UnlockBits(bmd);
                }
            }
            return data;
        }
    }
}
