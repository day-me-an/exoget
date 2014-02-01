using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public class ImageExtract : Extract
    {
        private ImageInfo image;

        public override void AddIndexProperties()
        {
            return;
        }

        public ImageInfo Image
        {
            get { return image; }
            set { image = value; }
        }
    }
}
