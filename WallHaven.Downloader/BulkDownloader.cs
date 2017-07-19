using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Web;

namespace WallHaven.Downloader
{
    public class BulkDownloader : IDisposable
    {
        private const string requestUrl = "https://alpha.wallhaven.cc/search?q=&search_image=&categories=001&purity=001&ratios=16x9&sorting=date_added&order=desc&page={0}";
        private const string requestHeader = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        private Uri GetRequestURL(int page) => new Uri(string.Format(requestUrl, page));

        public async Task GetRequest(IProgress<string> progress)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(requestHeader);
                var result = await client.DownloadStringTaskAsync(GetRequestURL(1));

                System.Diagnostics.Debug.Write(result);
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BulkDownload() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
