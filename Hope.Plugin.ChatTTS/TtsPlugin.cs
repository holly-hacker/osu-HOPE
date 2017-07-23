using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using HOPEless.Bancho;
using HOPEless.Bancho.Objects;
using HOPEless.Extensions;
using osu_HOPE.Plugin;

namespace Hope.Plugin.ChatTTS
{
    public class TtsPlugin : IHopePlugin
    {
        private SpeechSynthesizer _synth;

        public PluginMetadata GetMetadata()
        {
            return new PluginMetadata {
                Author = "HoLLy",
                Name = "osu!TTS",
                ShortDescription = "Says PMs out loud",
                Version = "1.0"
            };
        }

        public void Load()
        {
            _synth = new SpeechSynthesizer();
            Console.WriteLine("osu!TTS loaded!");
        }

        public void OnBanchoRequest(ref List<BanchoPacket> plist) { } //not doing anything here

        public void OnBanchoResponse(ref List<BanchoPacket> plist)
        {
            //pick out all chat packets
            foreach (BanchoPacket packet in plist) {
                if (packet.Type == PacketType.ServerChatMessage) {
                    BanchoChatMessage msg = new BanchoChatMessage();
                    msg.Populate(packet.Data);

                    //Console.WriteLine($"CHAT: {msg.Channel} {msg.Sender}: {msg.Message}");

                    if (!msg.Channel.StartsWith("#")) {  //PM
                        _synth.SpeakAsync($"Message from {msg.Sender}: {msg.Message}");
                    }
                }
            }
        }
    }
}
