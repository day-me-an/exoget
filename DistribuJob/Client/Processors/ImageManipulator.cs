using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DistribuJob.Client.Extracts;
using Exo.Collections;
using Exo.Media.image;

namespace DistribuJob.Client.Processors
{
    public class ImageManipulator:Processor
    {
        public static readonly Size DefaultDimensions = new Size(135, 101);

        public ImageManipulator(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            ImageInfo image;

            if (CreateImage(job.FilePath, out image))
                job.ImageExtract.Image = image;
        }

        public static bool CreateImage(Image image, out ImageInfo imageInfo)
        {
            using (image)
            using (Image thumbnail = image.GetThumbnailImage(DefaultDimensions.Width, DefaultDimensions.Height, delegate() { return true; }, IntPtr.Zero))
            {
                imageInfo = new ImageInfo();

                if (!image.PhysicalDimension.IsEmpty)
                {
                    imageInfo.OriginalWidth = image.Width;
                    imageInfo.OriginalHeight = image.Height;
                }

                Stream thumbnailStream = new MemoryStream();

                ImageCompressor.CompressImage(thumbnail, ImageFormat.Jpeg, 80L, ref thumbnailStream);

                imageInfo.ImageBytes = ((MemoryStream)thumbnailStream).ToArray();

                thumbnailStream.Dispose();
            }

            return true;
        }

        public static bool CreateImage(Stream stream, out ImageInfo imageInfo)
        {
            Image image;

            try
            {
                image = Image.FromStream(stream);
            }
            catch
            {
                imageInfo = null;
                return false;
            }

            bool success = false;

            using (image)
            {
                success = CreateImage(image, out imageInfo);
            }

            return success;
        }

        public static bool CreateImage(byte[] imageBytes, out ImageInfo image)
        {
            bool success = false;

            using (Stream stream = new MemoryStream(imageBytes))
            {
                success = CreateImage(stream, out image);
            }

            return success;
        }

        public static bool CreateImage(string filepath, out ImageInfo image)
        {
            bool success = false;

            using (Stream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                success = CreateImage(stream, out image);
            }

            return success;
        }

        public override void QueueControl(Job job)
        {
            Dj.Queues.exports.Enqueue(job);
        }
    }
}
