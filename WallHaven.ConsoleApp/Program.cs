﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WallHaven.ConsoleApp
{
    class Program
    {
        private const string regexUrl = @"(?:(?:https?:\/\/))[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b(?:[-a-zA-Z0-9@:%_\+.~#?&\/=]*(\.jpg|\.png|\.jpeg))";
        private const string requestUrl = "https://alpha.wallhaven.cc/search?q=&search_image=&categories=001&purity=001&ratios=16x9&sorting=date_added&order=desc&page={0}";
        private const string requestUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
        private const string requestContentType = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private const string requestCookie_0 = "d120f21787983998fb1524ae8d97398841500506629";
        private const string requestCookie_1 = "eyJpdiI6Ik9nZjNGRUp6U1RDM25YaG1abFRGc0NERWxBTVM2RW9OT2VZU1hOd1Fwcnc9IiwidmFsdWUiOiJmTENnRkZXbkpqRlMxd01HbGk4TVBGWVdPbE1KQ3VhaXhtQTBPZ1FPQ3JrTjBYXC9BckgyUzJ2VWRGV2lGemxRUGZkbCtmOVRhNHl6M1FuYmdzRkpPM1h3bEg5QXpTT1hwMlpQRXJadWJ1c2tuaUxmcVdIK3dkTHNKcmNwZXhBV28iLCJtYWMiOiJhNzMwNWE3YTVmMzg4ZWY5ZDVmYWFlZWJhZTAwMGE5NTQ1MGZjNjEwYWNhNmVmZGRhNDViZjY0NjY5NDlkZWE0In0%3D";
        private const string requestCookie_2 = "1";
        private const string requestCookie_3 = "GA1.2.544989659.1500506632";
        private const string requestCookie_4 = "GA1.2.1281294653.1500506632";

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

                    while (!tokenSource.Token.IsCancellationRequested)
                    {
                        var request = WebRequest.CreateDefault(new Uri(string.Format(requestUrl, page)));
                        var httpRequest = request as HttpWebRequest;

                        request.Credentials = CredentialCache.DefaultCredentials;
                        request.ContentType = requestContentType;

                        httpRequest.UserAgent = requestUserAgent;
                        httpRequest.CookieContainer = new CookieContainer();
                        httpRequest.CookieContainer.Add(uri, cookieCollection);

                        using (var response = await request.GetResponseAsync())
                        {
                            var httpResponse = response as HttpWebResponse;
                            cookieCollection["wallhaven_session"].Value = httpResponse.Cookies["wallhaven_session"].Value;

                            using (var responseStream = response.GetResponseStream())
                            using (var responseReader = new StreamReader(responseStream))
                            {
                                var matches = Regex.Matches(await responseReader.ReadToEndAsync(), regexUrl);
                                for (var m = 0; m < matches.Count; m++)
                                {
                                    var match = matches[m].Value.Substring(52);
                                    var url = string.Format("https://wallpapers.wallhaven.cc/wallpapers/full/wallhaven-{0}", match);

                                    //We have our image URL, now to download all of them async and move to the next page.
                                }
                            }
                        }
                    }

                    page++;

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
