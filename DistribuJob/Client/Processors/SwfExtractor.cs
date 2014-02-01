using System;
using System.Collections.Generic;
using System.Text;
//using SWFToImage;
using Exo.Media;
using System.Drawing;
using Exo.Media.image;
using DistribuJob.Client;
using Exo.Collections;

namespace DistribuJob.Client.Processors
{
    class SwfExtractor : Processor
    {
        public SwfExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            /*this.FlashPlayerControl1.FlashProperty_AlignMode = 0;
            this.FlashPlayerControl1.FlashProperty_AllowScriptAccess = "always";
            this.FlashPlayerControl1.FlashProperty_BackgroundColor = 0x00ffffff;
            this.FlashPlayerControl1.FlashProperty_Base = "";
            this.FlashPlayerControl1.FlashProperty_BGColor = "FFFFFF";
            this.FlashPlayerControl1.FlashProperty_DeviceFont = ((short)(0));
            this.FlashPlayerControl1.FlashProperty_EmbedMovie = ((short)(0));
            this.FlashPlayerControl1.FlashProperty_FlashVars = "";
            this.FlashPlayerControl1.FlashProperty_FrameNum = -1;
            this.FlashPlayerControl1.FlashProperty_Loop = ((short)(-1));
            this.FlashPlayerControl1.FlashProperty_Menu = ((short)(-1));
            this.FlashPlayerControl1.FlashProperty_Movie = "";
            this.FlashPlayerControl1.FlashProperty_MovieData = "";
            this.FlashPlayerControl1.FlashProperty_Playing = ((short)(-1));
            this.FlashPlayerControl1.FlashProperty_Quality = 1;
            this.FlashPlayerControl1.FlashProperty_Quality2 = "High";
            this.FlashPlayerControl1.FlashProperty_SAlign = "";
            this.FlashPlayerControl1.FlashProperty_Scale = "ShowAll";
            this.FlashPlayerControl1.FlashProperty_ScaleMode = 0;
            this.FlashPlayerControl1.FlashProperty_Stacking = "";
            this.FlashPlayerControl1.FlashProperty_SWRemote = "";
            this.FlashPlayerControl1.FlashProperty_WMode = "Transparent";
            this.FlashPlayerControl1.Location = new System.Drawing.Point(8, 8);
            this.FlashPlayerControl1.Name = "FlashPlayerControl1";
            this.FlashPlayerControl1.Size = new System.Drawing.Size(616, 360);
            this.FlashPlayerControl1.StandardMenu = false;
            this.FlashPlayerControl1.TabIndex = 4;
            this.FlashPlayerControl1.Text = "FlashPlayerControl1";

            /*SWFToImageObject swfToImage = new SWFToImageObject();
            swfToImage.InitLibrary("SingleDeveloperLicense", "C8P7-6973-B7PQ-G99U");

            swfToImage.InputSWFFileName = "E:\\Projects\\Visual Studio\\videoPreviewCreator\\videoPreviewCreator\\bin\\Debug\\ArtificialMedia\\" + job.id + ".swf";
            swfToImage.ImageOutputType = TImageOutputType.iotJPG;
            //swfToImage.FrameIndex = 5;

            swfToImage.JPEGQuality = 80;
            swfToImage.ImageWidth = 120;
            swfToImage.ImageHeight = 96;

            swfToImage.Execute();
            swfToImage.SaveToFile("videoPreviews\\" + job.id + ".jpg"); // not using anymore

            /*Quicktime quicktime = new Quicktime();

            quicktime.Open("E:\\Projects\\Visual Studio\\videoPreviewCreator\\videoPreviewCreator\\bin\\Debug\\ArtificialMedia\\" + job.id + ".swf");

            job.bitrate = quicktime.GetBitrate();
            job.duration = quicktime.GetDuration();

            job.height = (int)quicktime.GetWidth();
            job.height = (int)quicktime.GetHeight();

            Image preview = quicktime.GetFrameAtPosition(0.5, 120, 96);

            ImageCompressor.compressPng(preview, 80L, "videoPreviews\\" + job.id + ".png"); // not using anymore
             */
        }

        public override void QueueControl(Job job)
        {
            Dj.Queues.exports.Enqueue(job);
        }
    }
}