using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Exo.Collections;
using mshtml;
using System.Windows.Forms;
using System.IO;

namespace DistribuJob.Client.Processors
{
    class MshtmlExtractor:Processor
    {
        private IHTMLDocument2 doc = new HTMLDocumentClass();

        public MshtmlExtractor(IQueue<Job> queue)
            : base(queue)
        {
            ((IOleObject)doc).SetClientSite(new MshtmlFeatureFilters());
        }

        public override void Process(Job job)
        {
            IHTMLDocument3 doc3 = (IHTMLDocument3)doc;

            using (StreamReader sr = new StreamReader(job.FilePath))
            {
                doc.write(sr.ReadToEnd());
            }

            doc.close();

            Console.WriteLine("objects: " + doc3.getElementsByTagName("object").length);
            Console.WriteLine("embeds: " + doc3.getElementsByTagName("embed").length);

            //Console.WriteLine("object elements: " + ((IHTMLElement)doc3.getElementsByTagName("embed").item(null, 0)).GetAttribute("src", 0));
            //Console.WriteLine(doc.body.innerHTML);

            Console.WriteLine(doc.body.innerHTML);

            doc.clear();
        }

        public override void QueueControl(Job job)
        {
            //throw new Exception("The method or operation is not implemented.");
        }
    }

    [ComVisible(true)]
    public class MshtmlFeatureFilters : IOleClientSite
    {
        public enum BrowserOptions : int
        {
            /// <summary>
            /// No flags are set.
            /// </summary>
            None = 0,
            /// <summary>
            /// The browser will operate in offline mode. Equivalent to DLCTL_FORCEOFFLINE.
            /// </summary>
            AlwaysOffline = 0x10000000,
            /// <summary>
            /// The browser will play background sounds. Equivalent to DLCTL_BGSOUNDS.
            /// </summary>
            BackgroundSounds = 0x00000040,
            /// <summary>
            /// Specifies that the browser will not run Active-X controls. Use this setting
            /// to disable Flash movies. Equivalent to DLCTL_NO_RUNACTIVEXCTLS.
            /// </summary>
            DontRunActiveX = 0x00000200,

            DownloadOnly = 0x00000800,
            /// <summary>
            ///  Specifies that the browser should fetch the content from the server. If the server's
            ///  content is the same as the cache, the cache is used.Equivalent to DLCTL_RESYNCHRONIZE.
            /// </summary>
            IgnoreCache = 0x00002000,
            /// <summary>
            ///  The browser will force the request from the server, and ignore the proxy, even if the
            ///  proxy indicates the content is up to date.Equivalent to DLCTL_PRAGMA_NO_CACHE.
            /// </summary>
            IgnoreProxy = 0x00004000,
            /// <summary>
            ///  Specifies that the browser should download and display images. This is set by default.
            ///  Equivalent to DLCTL_DLIMAGES.
            /// </summary>
            Images = 0x00000010,
            /// <summary>
            ///  Disables downloading and installing of Active-X controls.Equivalent to DLCTL_NO_DLACTIVEXCTLS.
            /// </summary>
            NoActiveXDownload = 0x00000400,
            /// <summary>
            ///  Disables web behaviours.Equivalent to DLCTL_NO_BEHAVIORS.
            /// </summary>
            NoBehaviours = 0x00008000,
            /// <summary>
            ///  The browser suppresses any HTML charset specified.Equivalent to DLCTL_NO_METACHARSET.
            /// </summary>
            NoCharSets = 0x00010000,
            /// <summary>
            ///  Indicates the browser will ignore client pulls.Equivalent to DLCTL_NO_CLIENTPULL.
            /// </summary>
            NoClientPull = 0x20000000,
            /// <summary>
            ///  The browser will not download or display Java applets.Equivalent to DLCTL_NO_JAVA.
            /// </summary>
            NoJava = 0x00000100,
            /// <summary>
            ///  The browser will download framesets and parse them, but will not download the frames
            ///  contained inside those framesets.Equivalent to DLCTL_NO_FRAMEDOWNLOAD.
            /// </summary>
            NoFrameDownload = 0x00080000,
            /// <summary>
            ///  The browser will not execute any scripts.Equivalent to DLCTL_NO_SCRIPTS.
            /// </summary>
            NoScripts = 0x00000080,
            /// <summary>
            ///  If the browser cannot detect any internet connection, this causes it to default to
            ///  offline mode.Equivalent to DLCTL_OFFLINEIFNOTCONNECTED.
            /// </summary>
            //OfflineIfNotConnected = (int)0x80000000,
            /// <summary>
            ///  Specifies that UTF8 should be used.Equivalent to DLCTL_URL_ENCODING_ENABLE_UTF8.
            /// </summary>
            UTF8 = 0x00040000,
            /// <summary>
            ///  The browser will download and display Video ArtificialMedia.Equivalent to DLCTL_VIDEOS.
            /// </summary>
            Videos = 0x00000020,

            Silent = 0x40000000
        }

        [DispId(-5512)]
        public int IDispatch_Invoke_Handler()
        {
            return
                (int)(
                BrowserOptions.DontRunActiveX |
                BrowserOptions.BackgroundSounds |
                BrowserOptions.NoActiveXDownload |
                BrowserOptions.NoBehaviours |
                BrowserOptions.NoFrameDownload |
                BrowserOptions.NoJava |
                BrowserOptions.UTF8 |
                BrowserOptions.Silent
                );
        }

        #region IOleClientSite Members

        public void SaveObject()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetMoniker(uint dwAssign, uint dwWhichMoniker, ref object ppmk)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetContainer(ref object ppContainer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ShowObject()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnShowWindow(bool fShow)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RequestNewObjectLayout()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    [ComImport]
    [Guid("00000112-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleObject
    {
        void SetClientSite(IOleClientSite pClientSite);
        void GetClientSite(ref IOleClientSite ppClientSite);
        void SetHostNames(object szContainerApp, object szContainerObj);
        void Close(uint dwSaveOption);
        void SetMoniker(uint dwWhichMoniker, object pmk);
        void GetMoniker(uint dwAssign, uint dwWhichMoniker, object ppmk);
        void InitFromData(IDataObject pDataObject, bool fCreation, uint dwReserved);
        void GetClipboardData(uint dwReserved, ref IDataObject ppDataObject);
        void DoVerb(uint iVerb, uint lpmsg, object pActiveSite, uint lindex, uint hwndParent, uint lprcPosRect);
        void EnumVerbs(ref object ppEnumOleVerb);
        void Update();
        void IsUpToDate();
        void GetUserClassID(uint pClsid);
        void GetUserType(uint dwFormOfType, uint pszUserType);
        void SetExtent(uint dwDrawAspect, uint psizel);
        void GetExtent(uint dwDrawAspect, uint psizel);
        void Advise(object pAdvSink, uint pdwConnection);
        void Unadvise(uint dwConnection);
        void EnumAdvise(ref object ppenumAdvise);
        void GetMiscStatus(uint dwAspect, uint pdwStatus);
        void SetColorScheme(object pLogpal);
    };

    [ComImport]
    [Guid("00000118-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleClientSite
    {
        void SaveObject();
        void GetMoniker(uint dwAssign, uint dwWhichMoniker, ref object ppmk);
        void GetContainer(ref object ppContainer);
        void ShowObject();
        void OnShowWindow(bool fShow);
        void RequestNewObjectLayout();
    }
}
