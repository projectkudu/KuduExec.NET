using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace KuduExec
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: {0} [kudu service url]", typeof(Program).Assembly.GetName().Name);
            }
            else
            {
                string uriString = args[0];
                var uri = new Uri(uriString);
                string userName = uri.UserInfo;

                string siteName = uri.Host.Split(new char[] { '.' })[0];
                if (!uriString.EndsWith("/"))
                {
                    uriString = uriString + "/";
                }

                uriString = uriString + "command";

                var handler = new HttpClientHandler();
                if (!String.IsNullOrEmpty(userName))
                {
                    Console.Write("Enter password (WARNING: will echo!): ");
                    // TODO: don't echo password! :)
                    string password = Console.ReadLine();
                    handler.Credentials = new NetworkCredential(userName, password);
                }

                HttpClient client = new HttpClient(handler);
                Console.Write("{0}>> ", siteName);
                string content = null;

                while ((content = Console.ReadLine()) != null)
                {
                    JObject payload = new JObject(new JProperty("command", content));
                    try
                    {
                        JObject result = client.PostAsJsonAsync<JObject>(uriString, payload).Result.Content.ReadAsAsync<JObject>().Result;
                        string output = result.Value<string>("Output");
                        string error = result.Value<string>("Error");
                        int exitCode = result.Value<int>("ExitCode");

                        if (string.IsNullOrEmpty(output))
                        {
                            if (!string.IsNullOrEmpty(error))
                            {
                                Console.WriteLine(error);
                            }
                        }
                        else
                        {
                            Console.WriteLine(output);
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                    finally
                    {
                        Console.Write("{0}>> ", siteName);
                    }
                }
            }
        }
    }
}
