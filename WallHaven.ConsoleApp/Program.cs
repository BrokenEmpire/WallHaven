using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WallHaven.ConsoleApp
{
    class Program
    {
        private const string requestUrl = "https://alpha.wallhaven.cc/search?q=&search_image=&categories=001&purity=001&ratios=16x9&sorting=date_added&order=desc&page={0}";
        private const string requestHeader = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

        static void Main(string[] args)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                var progress = new Progress<string>(i => Console.WriteLine(i));
                var cancellationEventHandler = default(ConsoleCancelEventHandler);

                cancellationEventHandler = (s, e) =>
                {
                    tokenSource.Cancel();
                    e.Cancel = true;

                    Console.CancelKeyPress -= cancellationEventHandler;
                    Console.WriteLine("Cancelling Operation");
                };

                Console.CancelKeyPress += cancellationEventHandler;

                Task.Run(async () =>
                {
                    var page = 1;
                    WebRequest request;
                    
                    while (!tokenSource.IsCancellationRequested)
                    {
                        request = WebRequest.CreateHttp(new Uri(string.Format(requestUrl, page)));
                        request.Headers.Add(requestHeader);
                        request.Credentials = CredentialCache.DefaultCredentials;

                        var response = await request.GetResponseAsync();
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
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}
