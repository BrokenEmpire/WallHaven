using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WallHaven.ConsoleApp
{
    class Program
    {
        private const string regexUrl = "(?:(?:https?:\\/\\/)(?:alpha\\.wallhaven\\.cc\\/wallpaper\\/))([0-9]+(?=\"))";
        private const string regexImage = "(?:src=\"\\/\\/)(wallpapers\\.wallhaven\\.cc\\/wallpapers\\/full\\/wallhaven-{0}+\\.[a-zA-Z]+)";
        private const string requestUrl = "https://alpha.wallhaven.cc/search?q=&search_image=&categories=001&purity=001&ratios=16x9&sorting=date_added&order=desc&page={0}";
        private const string requestUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
        private const string requestContentType = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private const string requestCookie_0 = "d120f21787983998fb1524ae8d97398841500506629";
        private const string requestCookie_1 = "eyJpdiI6Ik9nZjNGRUp6U1RDM25YaG1abFRGc0NERWxBTVM2RW9OT2VZU1hOd1Fwcnc9IiwidmFsdWUiOiJmTENnRkZXbkpqRlMxd01HbGk4TVBGWVdPbE1KQ3VhaXhtQTBPZ1FPQ3JrTjBYXC9BckgyUzJ2VWRGV2lGemxRUGZkbCtmOVRhNHl6M1FuYmdzRkpPM1h3bEg5QXpTT1hwMlpQRXJadWJ1c2tuaUxmcVdIK3dkTHNKcmNwZXhBV28iLCJtYWMiOiJhNzMwNWE3YTVmMzg4ZWY5ZDVmYWFlZWJhZTAwMGE5NTQ1MGZjNjEwYWNhNmVmZGRhNDViZjY0NjY5NDlkZWE0In0%3D";
        private const string requestCookie_2 = "1";
        private const string requestCookie_3 = "GA1.2.544989659.1500506632";
        private const string requestCookie_4 = "GA1.2.1281294653.1500506632";
        private const string outputFolder = "C:\\Users\\DHR\\Desktop\\Images\\";

        static void Main(string[] args)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                ConsoleCancelEventHandler cancellationHandler;
                Console.CancelKeyPress += cancellationHandler = (s, e) =>
                {
                    e.Cancel = true;
                    tokenSource.Cancel();
                    Console.WriteLine("Cancelling Operation");
                };

                Task.Run(async () => 
                {
                    var page = 1;
                    var uri = new Uri("https://alpha.wallhaven.cc");
                    var cookieCollection = new CookieCollection()
                    {
                        new Cookie("__cfduid", requestCookie_0),
                        new Cookie("remember_82e5d2c56bdd0811318f0cf078b78bfc", requestCookie_1),
                        new Cookie("_gat", requestCookie_2),
                        new Cookie("_ga", requestCookie_3),
                        new Cookie("_gid", requestCookie_4),
                        new Cookie("wallhaven_session", string.Empty)
                    };

                    if (!Directory.Exists(outputFolder))
                        Directory.CreateDirectory(outputFolder);

                    while (!tokenSource.Token.IsCancellationRequested)
                    {
                        var imageTask = new List<Task>();
                        var request = WebRequest.CreateDefault(new Uri(string.Format(requestUrl, page)));
                        var httpRequest = request as HttpWebRequest;

                        request.Credentials = CredentialCache.DefaultCredentials;
                        request.ContentType = requestContentType;

                        httpRequest.UserAgent = requestUserAgent;
                        httpRequest.CookieContainer = new CookieContainer();
                        httpRequest.CookieContainer.Add(uri, cookieCollection);

                        tokenSource.Token.ThrowIfCancellationRequested();
                        
                        using (var response = await request.GetResponseAsync())
                        {
                            var httpResponse = response as HttpWebResponse;
                            cookieCollection["wallhaven_session"].Value = httpResponse.Cookies["wallhaven_session"].Value;

                            using (var responseStream = response.GetResponseStream())
                            using (var responseReader = new StreamReader(responseStream))
                            {
                                tokenSource.Token.ThrowIfCancellationRequested();
                                var matches = Regex.Matches(await responseReader.ReadToEndAsync(), regexUrl);

                                foreach (var match in matches)
                                {
                                    imageTask.Add(Task.Factory.StartNew(async (i) =>
                                    {
                                        var matchUrl = i as Match;
                                        var imageID = matchUrl.Groups[1].Value;
                                        var imagePageRequest = WebRequest.CreateDefault(new Uri(matchUrl.Value));

                                        imagePageRequest.Credentials = CredentialCache.DefaultCredentials;
                                        imagePageRequest.ContentType = requestContentType;
                                        (imagePageRequest as HttpWebRequest).UserAgent = requestUserAgent;
                                        (imagePageRequest as HttpWebRequest).CookieContainer = new CookieContainer();
                                        (imagePageRequest as HttpWebRequest).CookieContainer.Add(uri, cookieCollection);

                                        using (var imagePageResponse = await imagePageRequest.GetResponseAsync())
                                        using (var imagePageResponseStream = imagePageResponse.GetResponseStream())
                                        using (var imagePageResponseReader = new StreamReader(imagePageResponseStream))
                                        {
                                            var imageMatches = Regex.Matches(await imagePageResponseReader.ReadToEndAsync(), string.Format(regexImage, imageID));
                                            var imageRequest = WebRequest.CreateDefault(new Uri(string.Format("https://{0}", imageMatches[0].Groups[1].Value)));

                                            Console.WriteLine("Requesting {0}", imageRequest.RequestUri);

                                            using (var imageResponse = await imageRequest.GetResponseAsync())
                                            using (var imageResponseStream = imageResponse.GetResponseStream())
                                            using (var imageFileStream = new FileStream(string.Format("{0}{1}", outputFolder, imageRequest.RequestUri.Segments[3]), FileMode.Create, FileAccess.ReadWrite))
                                                await imageResponseStream.CopyToAsync(imageFileStream);
                                        }
                                    }, match, tokenSource.Token));
                                }
                            }
                        }

                        while (imageTask.Count > 0)
                        {
                            var task = await Task.WhenAny(imageTask);
                            imageTask.Remove(task);
                            tokenSource.Token.ThrowIfCancellationRequested();

                            await task;
                        }

                        page++;
                        await Task.Delay(5000);
                    }
                }, tokenSource.Token).ContinueWith((i) =>
                {
                    i.Exception?.Handle(ex =>
                    {
                        Console.WriteLine("Exception: {0}", ex.Message);
                        return true;
                    });

                    if (i.Status == TaskStatus.Canceled)
                    {
                        Console.WriteLine("Operation Cancelled");
                        return;
                    }

                    Console.WriteLine("Operation Successful");

                }).Wait();

                Console.CancelKeyPress -= cancellationHandler;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}
