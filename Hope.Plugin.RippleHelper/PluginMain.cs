using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_HOPE;
using osu_HOPE.Bancho;
using osu_HOPE.Bancho.Objects;
using osu_HOPE.Plugin;

namespace Hope.Plugin.RippleHelper
{
    public class PluginMain : IHopePlugin
    {
        private const string CommandPrefix = "!faq ";
        private Queue<BanchoPacket> _packetsToAdd;

        public PluginMetadata GetMetadata()
        {
            return new PluginMetadata {
                Author = "HoLLy",
                Name = "Ripple Helper",
                ShortDescription = "Does stuff to ripple",
                Version = "indev"
            };
        }

        public void Load()
        {
            _packetsToAdd = new Queue<BanchoPacket>();
        }

        public void OnBanchoRequest(ref List<BanchoPacket> plist)
        {
            for (var i = 0; i < plist.Count; i++) {
                BanchoPacket packet = plist[i];
                switch (packet.Type) {
                    case PacketType.OsuSendIrcMessage:
                    case PacketType.OsuSendIrcMessagePrivate:
                        //check if this message is a command for us
                        BanchoChatMessage msg = new BanchoChatMessage();
                        msg.Populate(packet.Data);
                        if (msg.Message.StartsWith(CommandPrefix)) {
                            string command = msg.Message.Substring(CommandPrefix.Length);
                            Console.WriteLine("aaaaaaaa: " + command);

                            //remove from packet list
                            plist.RemoveAt(i);
                        }
                        break;
                }
            }
        }

        public void OnBanchoResponse(ref List<BanchoPacket> plist)
        {
        }
    }
}
