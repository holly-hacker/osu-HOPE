using System;
using System.Collections.Generic;
using System.Diagnostics;
using HOPEless.Bancho;
using HOPEless.Bancho.Objects;
using HOPEless.Extensions;
using osu_HOPE.Plugin;

namespace Hope.Plugin.Example2
{
    public class ModificationPlugin : IHopePlugin
    {
        private bool _injectedChannel = false;

        public PluginMetadata GetMetadata()
        {
            return new PluginMetadata
            {
                Name = "Example1",
                Author = "HoLLy",
                ShortDescription = "Modifies any ServerMainMenuNews packets and injects a custom channel.",
                Version = "1.0"
            };
        }

        public void Load() {}

        public void OnBanchoRequest(ref List<BanchoPacket> plist) {}

        public void OnBanchoResponse(ref List<BanchoPacket> plist)
        {
            foreach (BanchoPacket packet in plist)
            {
                switch (packet.Type)
                {
                    case PacketType.ServerMainMenuNews:
                        BanchoString bs = new BanchoString();
                        bs.Populate(packet.Data);
                        Debug.WriteLine("Bancho Title Update: " + bs.Value);
                        bs.Value = "http://i.imgur.com/IC1ApNK.png|http://JustM3.net";
                        packet.Data = bs.Serialize();
                        break;
                }
            }

            if (!_injectedChannel) {
                plist.Add(new BanchoPacket(PacketType.ServerChatChannelAvailableAutojoin,
                    new BanchoChatChannel {
                        Name = "#admin",
                        Topic = "Raple is cute",
                        UserCount = 1337
                    }));
                _injectedChannel = true;
                Debug.WriteLine("Added custom channel.");
            }
        }
    }
}
