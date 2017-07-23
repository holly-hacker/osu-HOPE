using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HOPEless.Bancho;
using osu_HOPE.Plugin;
using osu_HOPE.PluginManagement;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace osu_HOPE
{
    internal class Program
    {
        private static readonly PluginManager Manager = new PluginManager();

        private static void Main(string[] args)
        {
#if DEBUG
            Debug.Listeners.Add(new ConsoleTraceListener());
#endif

            PrintLogo();

#if !NO_PLUGINS
            Console.WriteLine("Loading plugins");
            Manager.LoadPlugins();
            foreach (IHopePlugin plugin in Manager.Plugins) plugin.Load();
            Console.WriteLine($"Loaded {Manager.Plugins.Count} Plugin{(Manager.Plugins.Count == 1 ? "" : "s")}!");
#else
#warning Plugin support disabled!
            Console.WriteLine("Warning: This build does not support plugins.");
#endif

            Console.WriteLine("Preparing proxy server...");
            var proxyServer = new ProxyServer();
            proxyServer.TrustRootCertificate = true;
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;

            var endpoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);

            proxyServer.AddEndPoint(endpoint);

            Console.WriteLine("Starting proxy server...");
            proxyServer.Start();

            Console.WriteLine("Setting as system proxy...");
            proxyServer.SetAsSystemHttpProxy(endpoint);
            proxyServer.SetAsSystemHttpsProxy(endpoint);

            Console.WriteLine("Waiting...");
            Console.ReadLine();
        }

        private static void PrintLogo()
        {
            Console.Write("osu!");
            Console.WriteLine("H o P E");
            Console.WriteLine(
                "    o s a d\n" +
                "    L u c i\n" +
                "    L ! k t\n" +
                "    y   e o\n" +
                "    '   t r\n" +
                "    s\n");
        }

        private static async Task OnRequest(object sender, SessionEventArgs e)
        {
            //check if the request is to something that interests us
            string url = e.WebSession.Request.Url;
            if (!url.Contains("ppy.sh")) return;

            if (url == "https://c.ppy.sh/") {
                if (e.WebSession.Request.GetAllHeaders().Any(a => a.Name == "osu-token")) {
                    byte[] bodyOriginal = await e.GetRequestBody();
                    List<BanchoPacket> plist = BanchoSerializer.DeserializePackets(bodyOriginal).ToList();

                    foreach (IHopePlugin plugin in Manager.Plugins) {
                        try {
                            plugin.OnBanchoRequest(ref plist);
                        }
                        catch (Exception exception) {
                            Debug.WriteLine($"Exception occured in plugin {plugin.GetMetadata().Name} OnBanchoRequest: " + exception);
                        }
                    }

                    byte[] bodySerialized = BanchoSerializer.Serialize(plist);
                    await e.SetRequestBody(bodySerialized);

#if DEBUG && NO_PLUGINS
                    bool equal = true;
                    if (bodyOriginal.Length == bodySerialized.Length) {
                        if (bodyOriginal.Where((t, i) => t != bodySerialized[i]).Any())
                            equal = false;
                    }
                    else equal = false;
                    if (!equal) {
                        File.WriteAllBytes("orig", bodyOriginal);
                        File.WriteAllBytes("gen", bodySerialized);
                        Debugger.Break();
                    }
#endif
                }
                else {
                    //login request, don't modify this for now
                    Debug.WriteLine("Not parsing login request.");
                }
            }
        }

        private static async Task OnResponse(object sender, SessionEventArgs e)
        {
            string url = e.WebSession.Request.Url;
            if (!url.Contains("ppy.sh")) return;

            if (url == "https://c.ppy.sh/") {
                //normal request
                byte[] bodyOriginal = await e.GetResponseBody();
                List<BanchoPacket> plist = BanchoSerializer.DeserializePackets(bodyOriginal).ToList();

                foreach (IHopePlugin plugin in Manager.Plugins) {
                    try {
                        plugin.OnBanchoResponse(ref plist);
                    }
                    catch (Exception exception) {
                        Debug.WriteLine($"Exception occured in plugin {plugin.GetMetadata().Name} OnBanchoRequest: " + exception);
                    }
                }

                /*foreach (BanchoPacket packet in plist) {
                    if (packet.Type == PacketType.ServerMultiMatchNew) {
                        var m = new BanchoMultiplayerMatch();
                        m.Populate(packet.Data);
                        Console.WriteLine($"Bancho match info: name=\"{m.GameName}\", pass=\"{m.GamePassword}\"");
                    }
                }*/

                byte[] bodySerialized = BanchoSerializer.Serialize(plist);
                await e.SetResponseBody(bodySerialized);

#if DEBUG && NO_PLUGINS
                bool equal = true;
                if (bodyOriginal.Length == bodySerialized.Length)
                {
                    if (bodyOriginal.Where((t, i) => t != bodySerialized[i]).Any())
                        equal = false;
                }
                else equal = false;
                if (!equal)
                {
                    File.WriteAllBytes("orig", bodyOriginal);
                    File.WriteAllBytes("gen", bodySerialized);
                    Debugger.Break();
                }
#endif
            }
        }
    }
}