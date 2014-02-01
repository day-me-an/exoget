using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public class ImageInfo
    {
        private int originalWidth, originalHeight;
        internal byte[] imageBytes;

        public int OriginalWidth
        {
            get { return originalWidth; }
            set { originalWidth = value; }
        }

        public int OriginalHeight
        {
            get { return originalHeight; }
            set { originalHeight = value; }
        }

        public byte[] ImageBytes
        {
            get { return imageBytes; }
            set { imageBytes = value; }
        }

        public bool HasDimensions
        {
            get { return originalWidth != 0 && originalHeight != 0; }
        }
    }
}
