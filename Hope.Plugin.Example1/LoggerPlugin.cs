using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HOPEless.Bancho;
using HOPEless.Bancho.Objects;
using HOPEless.Extensions;
using HOPEless.osu;
using osu_HOPE.Plugin;

namespace Hope.Plugin.Example1
{
    public class LoggerPlugin : IHopePlugin
    {
        private readonly List<PacketType> _blacklist = new List<PacketType> { PacketType.ServerPing, PacketType.ClientPong };

        public PluginMetadata GetMetadata()
        {
            return new PluginMetadata {
                Name = "Example1",
                Author = "HoLLy",
                ShortDescription = "Logs all bancho packets",
                Version = "1.0"
            };
        }

        public void Load()
        {
            //we don't do anything here
        }

        public void OnBanchoRequest(ref List<BanchoPacket> plist)
        {
            //if there is any packet that is not in the blacklist
            if (plist.Any(packet => !_blacklist.Contains(packet.Type)))
                Console.WriteLine("osu!->bancho:");

            //show all packet names that are not in blacklist
            foreach (BanchoPacket packet in plist.Where(packet => !_blacklist.Contains(packet.Type)))
                PrintPacketDescription(packet);
        }

        public void OnBanchoResponse(ref List<BanchoPacket> plist)
        {
            //if there is any packet that is not in the blacklist
            if (plist.Any(a => !_blacklist.Contains(a.Type)))
                Console.WriteLine("bancho->osu!:");

            //show all packet names that are not in blacklist
            foreach (BanchoPacket packet in plist.Where(a => !_blacklist.Contains(a.Type)))
                PrintPacketDescription(packet);
        }

        private void PrintPacketDescription(BanchoPacket packet)
        {
            //write in front of every line
            Console.Write(" * ");

            //show different info depending on the packet type
            //TODO: sort unsorted packets
            //TODO: support more packets
            switch (packet.Type) {
                #region chat
                case PacketType.ClientChatMessagePublic:
                case PacketType.ClientChatMessagePrivate:
                case PacketType.ServerChatMessage:
                    BanchoChatMessage msg = new BanchoChatMessage();
                    msg.Populate(packet.Data);

                    bool isPrivate = !string.IsNullOrEmpty(msg.Channel) && !msg.Channel.StartsWith("#");
                    bool isIncoming = packet.Type == PacketType.ServerChatMessage;

                    Console.WriteLine($"{(isIncoming ? "Incoming" : "Outgoing")} {(isPrivate ? "private" : "public")} chat message. " +
                                      $"[{(isPrivate ? "PM" : msg.Channel)}] {(isIncoming ? msg.Sender : "You")}: {msg.Message}");
                    break;
                case PacketType.ServerChatChannelAvailable:
                case PacketType.ServerChatChannelAvailableAutojoin:
                case PacketType.ServerChatChannelJoinSuccess:
                case PacketType.ServerChatChannelRevoked:
                case PacketType.ClientChatChannelJoin:
                case PacketType.ClientChatChannelLeave:
                    // ReSharper disable once TooWideLocalVariableScope
                    string channelName; //fuck you ReSharper, you can't define this in inner scope
                    // ReSharper disable once TooWideLocalVariableScope
                    string action;      //same here
                    if (packet.Type == PacketType.ServerChatChannelRevoked || packet.Type == PacketType.ServerChatChannelJoinSuccess) {
                        var channel = new BanchoString();
                        channel.Populate(packet.Data);
                        channelName = channel.Value;
                        action = packet.Type == PacketType.ServerChatChannelRevoked ? "revoked" : "successfully joined";
                    } else if (packet.Type == PacketType.ClientChatChannelJoin || packet.Type == PacketType.ClientChatChannelLeave) {
                        var channel = new BanchoString();
                        channel.Populate(packet.Data);
                        channelName = channel.Value;
                        action = packet.Type == PacketType.ClientChatChannelJoin ? "joined" : "left";
                    } else {
                        var channel = new BanchoChatChannel();
                        channel.Populate(packet.Data);
                        channelName = channel.Name;
                        action = packet.Type == PacketType.ServerChatChannelAvailable ? "available" : "available, and autojoin it.";
                    }
                    Console.WriteLine($"Channel {channelName} {action}");
                    break;
                #endregion
                #region users
                case PacketType.ClientUserStatsRequest:
                    var requestedUsers = new BanchoIntList();
                    requestedUsers.Populate(packet.Data);
                    Console.WriteLine($"Asking user info on {requestedUsers.Value.Count} user{(requestedUsers.Value.Count==1 ? "" : "s")}");
                    break;
                case PacketType.ServerUserData:
                    var userData = new BanchoUserData();
                    userData.Populate(packet.Data);
                    Console.WriteLine($"User {userData.UserId} has {userData.Performance:F0}pp is {userData.Status.Action} {userData.Status.ActionText}");
                    break;
                case PacketType.ClientUserStatus:
                    var userStatus = new BanchoUserStatus();
                    userStatus.Populate(packet.Data);
                    Console.WriteLine($"We're {userStatus.Action} {userStatus.ActionText}");
                    break;
                case PacketType.ServerUserPresence:
                    var userPresence = new BanchoUserPresence();
                    userPresence.Populate(packet.Data);
                    Console.WriteLine($"Presence: User with id {userPresence.UserId} is called {userPresence.Username}, has rank #{userPresence.Rank} and lives at long {userPresence.Longitude} and lat {userPresence.Latitude}");
                    break;
                case PacketType.ClientRequestPlayerList:
                    var updateType = new BanchoInt();
                    updateType.Populate(packet.Data);
                    Console.WriteLine("Asking for player list of type " + (PlayerListType)updateType.Value);
                    break;
                case PacketType.ServerUserQuit:
                    var userQuit = new BanchoUserQuit();
                    userQuit.Populate(packet.Data);
                    Console.WriteLine($"User {userQuit.UserId} left, type {userQuit.QuitType}");
                    break;
                #endregion
                #region multiplayer
                case PacketType.ClientLobbyJoin:
                case PacketType.ClientLobbyLeave:
                    Console.WriteLine($"{(packet.Type == PacketType.ClientLobbyJoin ? "Joined" : "Left")} osu! lobby");
                    break;
                case PacketType.ServerMultiMatchNew:
                case PacketType.ServerMultiMatchUpdate:
                    BanchoMultiplayerMatch match = new BanchoMultiplayerMatch();
                    match.Populate(packet.Data);
                    Console.WriteLine($"Multiplayer: {packet.Type.ToString().Substring("BanchoMatch".Length)} match {match.GameName}, pass: {match.GamePassword ?? "(none)"}");
                    break;
                #endregion
                #region initialization
                case PacketType.ServerLockClient:
                    var banLength = new BanchoInt();
                    banLength.Populate(packet.Data);
                    Console.WriteLine(banLength.Value > 0 ? $"This client has been locked for {banLength.Value} more seconds!" : "This client is not locked.");
                    break;
                case PacketType.ServerLoginReply:
                    var userid = new BanchoInt();
                    userid.Populate(packet.Data);
                    switch (userid.Value) {
                        case -1:
                            Console.WriteLine("Login failed!");
                            break;
                        case -2:
                            Console.WriteLine("Your client is very outdated!");
                            break;
                        case -3:
                        case -4:
                            Console.WriteLine("You are banned!");
                            break;
                        case -5:
                            Console.WriteLine("A serverside error occured!");
                            break;
                        case -6:
                            Console.WriteLine("You need supporter to use this client build.");
                            break;
                        case -7:
                            Console.WriteLine("Your account's password has been reset!");
                            break;
                        case -8:
                            Console.WriteLine("You need to log in with 2FA!");
                            break;
                        default:
                            Console.WriteLine("You logged in with userid " + userid.Value);
                            break;
                    }
                    break;
                case PacketType.ServerBanchoVersion:
                    var banchoProtocol = new BanchoInt();
                    banchoProtocol.Populate(packet.Data);
                    Console.WriteLine("Bancho protocol: v" + banchoProtocol.Value);
                    break;
                case PacketType.ServerUserPermissions:
                    var userPermissions = new BanchoInt();
                    userPermissions.Populate(packet.Data);
                    Console.WriteLine("This user's permissions: " + (UserPermissions)userPermissions.Value);
                    break;
                case PacketType.ServerFriendsList:
                    var friendList = new BanchoIntList();
                    friendList.Populate(packet.Data);
                    Console.WriteLine($"You have {friendList.Value.Count} friends.");
                    break;
                case PacketType.ServerMainMenuNews:
                    var mainMenuTitle = new BanchoString();
                    mainMenuTitle.Populate(packet.Data);
                    Console.WriteLine($"Menu bottom advertisement image: {mainMenuTitle.Value}");
                    break;
                case PacketType.ServerChatChannelListingComplete:
                    Console.WriteLine("All channels have been sent to the client.");
#if DEBUG
                    //for some reason, ripple sends data that the client does not parse. Display that data
                    if (packet.Data.Length > 0) {
                        Console.WriteLine("Data s: " + Encoding.UTF7.GetString(packet.Data));
                        Console.WriteLine("Data h: " + string.Join(" ", packet.Data.Select(a => $"0x{a:X2}")));
                    }
#endif
                    break;
                case PacketType.ClientStatusRequestOwn:
                    Console.WriteLine("Asking for own user info.");
                    break;
                #endregion
                #region spectating
                case PacketType.ClientSpectateNoBeatmap:
                    Console.WriteLine("We can't spectate this user.");
                    break;
                case PacketType.ClientSpectateStart:
                case PacketType.ClientSpectateStop:
                    var spectatedUser = new BanchoInt();
                    spectatedUser.Populate(packet.Data);
                    Console.WriteLine((packet.Type == PacketType.ClientSpectateStart ? "Started" : "Stopped") + " spectating playerid " + spectatedUser);
                    break;
                case PacketType.ServerSpectateData:
                    var frameBundle = new BanchoReplayFrameBundle();
                    frameBundle.Populate(packet.Data);
                    Console.WriteLine($"Spectate frames received for action {frameBundle.Action}.");
                    break;
                case PacketType.ServerSpectateOtherSpectatorJoined:
                case PacketType.ServerSpectateOtherSpectatorLeft:
                    var otherSpectator = new BanchoInt();
                    otherSpectator.Populate(packet.Data);
                    Console.WriteLine($"Other spectator with id {otherSpectator.Value} {(packet.Type == PacketType.ServerSpectateOtherSpectatorJoined ? "joined" : "left")}.");
                    break;
                #endregion
                case PacketType.ClientBeatmapInfoRequest:
                    var beatmapInfoRequest = new BanchoBeatmapInfoRequest();
                    beatmapInfoRequest.Populate(packet.Data);
                    Console.WriteLine($"Requesting info on {beatmapInfoRequest.Filenames} beatmap(s) by filename and {beatmapInfoRequest.BeatmapIds} beatmap(s) by beatmap id");
                    break;
                default:
#if DEBUG
                    ConsoleColor orig = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
#endif
                    if (packet.Data.Length > 0) Console.Write("[HAS DATA] ");
                    Console.WriteLine(packet.Type);
#if DEBUG
                    Console.ForegroundColor = orig;
#endif
                    break;
            }
        }
    }
}
