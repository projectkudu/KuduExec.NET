using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

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
                string command = null;
                string currentFolder = "";
                ShowPrompt(currentFolder);

                while ((command = Console.ReadLine()) != null)
                {
                    command = command.Trim();

                    try
                    {
                        // Ignore empty lines
                        if (String.IsNullOrEmpty(command))
                        {
                            continue;
                        }

                        // Add a 'cd' at the end so we can get the working folder on the way out
                        command = command + " & cd";

                        JObject payload = new JObject(new JProperty("command", command), new JProperty("dir", currentFolder));
                        JObject result = client.PostAsJsonAsync<JObject>(uriString, payload).Result.Content.ReadAsAsync<JObject>().Result;
                        string output = result.Value<string>("Output");
                        string error = result.Value<string>("Error");
                        int exitCode = result.Value<int>("ExitCode");

                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine(error);
                            continue;
                        }

                        output = output.TrimEnd();

                        // The last like should be the working folder so parse it out
                        int lastLineIndex = output.LastIndexOf("\r\n");

                        if (lastLineIndex < 0)
                        {
                            currentFolder = output;
                        }
                        else
                        {
                            currentFolder = output.Substring(lastLineIndex);

                            Console.WriteLine(output.Substring(0, lastLineIndex));
                        }

                        currentFolder = currentFolder.Trim();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                    finally
                    {
                        ShowPrompt(currentFolder);
                    }
                }
            }
        }

        private static void ShowPrompt(string currentFolder)
        {
            Console.Write("[Kudu] {0}> ", currentFolder);
        }
    }
}
