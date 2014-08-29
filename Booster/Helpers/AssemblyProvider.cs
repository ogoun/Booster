using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Booster.Helpers
{
    internal sealed class ImageSet
    {
        private readonly Dictionary<string, long> _imagesVolume = new Dictionary<string, long>();
        private readonly List<string> _images = new List<string>();
        int index = 0;

        internal void Append(string imageFile)
        {
            using (Image img = Image.FromFile(imageFile))
            {
                _imagesVolume.Add(imageFile, img.Width * img.Height - 4);
                _images.Add(imageFile);
            }
        }

        internal string FindImage(long size)
        {
            for (int i = 0; i < _images.Count; i++)
            {
                if (index >= _images.Count)
                {
                    index = 0;
                }
                if (_imagesVolume[_images[index]] >= size)
                {
                    return _images[index++];
                }
                index++;
            }
            return null;
        }
    }

    internal static class AssemblyProvider
    {
        internal static void PackAssemblies(string imagesFolder, string outputImagesFolder, string dllsFolder)
        {
            ImageSet images = new ImageSet();
            if (!Directory.Exists(outputImagesFolder))
            {
                Directory.CreateDirectory(outputImagesFolder);
            }

            Dictionary<string, long> imagesVolume = new Dictionary<string, long>();
            var imageFiles = Directory
                                .GetFiles(imagesFolder, "*.*")
                                .Where(file => file.ToLower().EndsWith("bmp") || file.ToLower().EndsWith("png") || file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("jpeg"))
                                .ToList();
            foreach (string file in imageFiles)
            {
                images.Append(file);
            }

            foreach (string file in Directory.GetFiles(dllsFolder, "*.dll"))
            {
                byte[] packed = AssemblyPacker.Pack(file);
                string imageFile = images.FindImage(packed.Length);
                if (imageFile == null)
                {
                    Console.WriteLine("Not found image to write dll " + file + " (need " + packed.Length + " bytes)");
                }
                else
                {
                    string outImagefile = Path.Combine(outputImagesFolder, Path.GetFileNameWithoutExtension(file) + "_" + Path.GetFileNameWithoutExtension(imageFile) + ".png");
                    Bitmap bmp = SteganographyProvider.Injection(File.ReadAllBytes(imageFile), packed);
                    bmp.Save(outImagefile, ImageFormat.Png);
                }
            }
        }

        internal static AssembliesSet LoadAssemblies(string imagesFolder)
        {
            AssembliesSet set = new AssembliesSet();
            foreach (string file in Directory.GetFiles(imagesFolder, "*.png"))
            {
                byte[] data = null;
                data = SteganographyProvider.Extraction(File.ReadAllBytes(file));
                data = AssemblyPacker.UnPack(data);
                set.TryAppendAssembly(data);
            }
            return set;
        }

        #region Helpers
        static ImageCodecInfo FindEncoder(Guid id)
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo[] array = imageEncoders;
            for (int i = 0; i < array.Length; i++)
            {
                ImageCodecInfo imageCodecInfo = array[i];
                if (imageCodecInfo.FormatID.Equals(id))
                {
                    return imageCodecInfo;
                }
            }
            return null;
        }


        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern int GdipSaveImageToFile(HandleRef image, string filename, ref Guid classId, HandleRef encoderParams);
        #endregion
    }
}
