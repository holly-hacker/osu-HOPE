using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HOPEless.Bancho;
using HOPEless.Bancho.Objects;
using HOPEless.Extensions;
using HOPEless.osu;
using osu_HOPE.Plugin;

namespace Hope.Plugin.ExtensiveExample
{
    public class PluginMain : IHopePlugin
    {
        private const string CommandPrefix = "!hope" + " ";
        private static Queue<BanchoPacket> _packetQueueReceive;
        private static Queue<BanchoPacket> _packetQueueSend;

        private readonly MultiplayerInviteGenerator _mig = new MultiplayerInviteGenerator();
        private readonly MultiplayerScoreSpam _mss = new MultiplayerScoreSpam();

        //bad way to do it, but I had to add
        private bool _mpInviteGenerator = false;
        private bool _mpScoreSpam = false;
        private bool _spectateCorrupt = false;
        private bool _customAction = false;
        private bool _peppy = false;
        private bool _peopleAreWeird = false;

        public PluginMetadata GetMetadata()
        {
            return new PluginMetadata
            {
                Author = "HoLLy",
                Name = "Extensive example",
                ShortDescription = "Does more stuff!",
                Version = "indev"
            };
        }

        public void Load()
        {
            Console.WriteLine("Loaded the more extensive plugin!");
            _packetQueueReceive = new Queue<BanchoPacket>();
            _packetQueueSend = new Queue<BanchoPacket>();
        }

        public void OnBanchoRequest(ref List<BanchoPacket> plist)
        {
            for (var i = 0; i < plist.Count; i++) {
                BanchoPacket packet = plist[i];
                switch (packet.Type) {
                    case PacketType.ClientChatMessagePublic:
                    case PacketType.ClientChatMessagePrivate:
                        BanchoChatMessage msg = new BanchoChatMessage();
                        msg.Populate(packet.Data);
                        if (msg.Message.StartsWith(CommandPrefix)) {    //check if this message is a command for us
                            plist.RemoveAt(i);  //remove this packet from packet list
                            HandleCommand(msg); //do stuff w/ it
                        }
                        break;
                    case PacketType.ClientMultiScoreUpdate:
                        if (_mpScoreSpam) {
                            var score = new BanchoScoreFrame();
                            score.Populate(packet.Data);
                            _mss.ModifyScorePacket(ref score);
                            plist[i].Data = score.Serialize();
                        }
                        break;
                    case PacketType.ClientUserStatus:
                        if (_customAction) {
                            var status = new BanchoUserStatus();
                            status.Populate(packet.Data);
                            status.Action = BanchoAction.Submitting;
                            status.ActionText = "totally legit scores";
                            plist[i].Data = status.Serialize();
                        }
                        break;
                    case PacketType.ClientSpectateData:
                        if (_spectateCorrupt) {
                            var frames = new BanchoReplayFrameBundle();
                            frames.Populate(packet.Data);
                            SpectatorCorrupter.ModifySpectatePacket(ref frames);
                            plist[i].Data = frames.Serialize();
                        }
                        break;
                }
            }
            
            while (_packetQueueSend.Count != 0)
                plist.Add(_packetQueueSend.Dequeue());
        }

        public void OnBanchoResponse(ref List<BanchoPacket> plist)
        {
            for (var i = 0; i < plist.Count; i++) {
                BanchoPacket packet = plist[i];
                switch (packet.Type)
                {
                    case PacketType.ServerMultiMatchNew:
                    case PacketType.ServerMultiMatchUpdate:
                        if (_mpInviteGenerator) {
                            BanchoMultiplayerMatch match = new BanchoMultiplayerMatch();
                            match.Populate(packet.Data);
                            _mig.HandleNewMultiplayerMatch(match);
                        }
                        break;
                    case PacketType.ServerChatMessage:
                        if (_peppy) {
                            var msg = new BanchoChatMessage();
                            msg.Populate(packet.Data);
                            var rand = new Random((int)DateTime.Now.Ticks);
                            msg.Message = string.Join(" ", msg.Message.Split(' ').Select(a => new[] { "dick", "wang", "fuck a duck", "fuck a donkey", "cocks" }[rand.Next(5)]));
                            //msg.Message = "No quote found. Wow, talk more, losers. (Syntax: .quote [#|nick|add|remove] [\"quote\"|#] [- nick])";
                            packet.Data = msg.Serialize();
                        }
                        break;
                    case PacketType.ServerUserPresence:
                        if (_peopleAreWeird) {
                            var pr = new BanchoUserPresence();
                            pr.Populate(packet.Data);
                            if (pr.Username != "JustM3") pr.Username = "wanker";    //bad hardcode, sorry
                            pr.Permissions |= UserPermissions.Peppy;
                            pr.Rank = Int32.MinValue;
                            packet.Data = pr.Serialize();
                        }
                        break;
                    case PacketType.ServerUserData:
                        if (_peopleAreWeird) {
                            var u = new BanchoUserData();
                            u.Populate(packet.Data);
                            u.Accuracy = float.NegativeInfinity;
                            u.Performance = short.MaxValue;
                            u.RankedScore = 1;
                            u.TotalScore = 1;
                            u.Rank = -1;
                            u.Playcount = Int32.MaxValue;
                            u.Status.Action = BanchoAction.Testing;
                            u.Status.ActionText = "osu!HOPE";
                            u.Status.BeatmapId = 0;
                            u.Status.BeatmapChecksum = "";
                            packet.Data = u.Serialize();
                        }
                        break;
                    case PacketType.ServerSpectateData:
                        if (_spectateCorrupt) {
                            var frames = new BanchoReplayFrameBundle();
                            frames.Populate(packet.Data);
                            SpectatorCorrupter.ModifySpectatePacket(ref frames);
                            plist[i].Data = frames.Serialize();
                        }
                        break;
                }
            }

            while (_packetQueueReceive.Count != 0)
                plist.Add(_packetQueueReceive.Dequeue());
        }

        public void HandleCommand(BanchoChatMessage msg)
        {
            //extract command
            string fullCommand = msg.Message.Substring(CommandPrefix.Length);
            Debug.WriteLine($"command detected: {fullCommand} (channel: {msg.Channel})");

            //handle command
            string command, arguments;
            if (fullCommand.Contains(' ')) {
                command = fullCommand.Substring(0, fullCommand.IndexOf(' '));
                arguments = fullCommand.Substring(fullCommand.IndexOf(' ') + 1);
            } else {
                command = fullCommand;
                arguments = string.Empty;
            }

            switch (command) {
                case "test":
                    SendMessage("test!", msg.Channel);
                    Console.WriteLine("test command T R I G G E R E D (sorry)");
                    break;
                case "rtx":
                    _packetQueueReceive.Enqueue(new BanchoPacket(PacketType.ServerRtx, new BanchoString() { Value = arguments }));
                    break;
                case "notification":
                    _packetQueueReceive.Enqueue(new BanchoPacket(PacketType.ServerNotification, new BanchoString() { Value = arguments }));
                    break;
                case "toggle":
                    if (!string.IsNullOrEmpty(arguments)) {
                        switch (arguments)
                        {
                            case "invite":
                                _mpInviteGenerator = !_mpInviteGenerator;
                                break;
                            case "mpscore":
                                _mpScoreSpam = !_mpScoreSpam;
                                break;
                            case "spectate":
                                _spectateCorrupt = !_spectateCorrupt;
                                break;
                            case "action":
                                _customAction = !_customAction;
                                break;
                            case "peppy":
                                _peppy = !_peppy;
                                break;
                            case "wtf":
                                _peopleAreWeird = !_peopleAreWeird;
                                break;
                            default:
                                SendMessage("blah, unknown", msg.Channel);
                                return;
                        }

                        SendMessage("Toggled.", msg.Channel);
                    }
                    else
                        SendMessage("too lazy to give a help list. Use !toggle [what]");
                    break;
            }
        }

        public static void SendMessage(string content, string channel = "#osu!HOPE", string username = "osu!HOPE", int userid = Int32.MaxValue)
        {
            _packetQueueReceive.Enqueue(new BanchoPacket(PacketType.ServerChatMessage, new BanchoChatMessage
            {
                Message = content,
                Sender = username,
                SenderId = userid,
                Channel = channel
            }));
        }
    }
}
