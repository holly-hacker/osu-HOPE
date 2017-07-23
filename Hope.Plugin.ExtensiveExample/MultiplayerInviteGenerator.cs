using System;
using System.Collections.Generic;
using System.Diagnostics;
using HOPEless.Bancho.Objects;

namespace Hope.Plugin.ExtensiveExample
{
    /// <summary>
    /// Abuses an oversight in ripple, where it sends passwords for multiplayer
    /// matches even the player is not in them.
    /// </summary>
    internal class MultiplayerInviteGenerator
    {
        /// <summary> Stores matchid-password combo </summary>
        public Dictionary<int, string> StoredPasswords = new Dictionary<int, string>();

        public void HandleNewMultiplayerMatch(BanchoMultiplayerMatch m)
        {
            //check if this game is password-protected and it is sent to our client
            if (string.IsNullOrEmpty(m.GamePassword)) return;
            Debug.WriteLine("nonempty pass");

            //we caught a password, let's see if it's not already in our list
            if (StoredPasswords.ContainsKey(m.MatchId) && (!StoredPasswords.ContainsKey(m.MatchId) || StoredPasswords[m.MatchId] == m.GamePassword)) return;
            Debug.WriteLine("new pass");

            //w00t we got a new one, let's add it to the list, so we don't have dupes later
            StoredPasswords[m.MatchId] = m.GamePassword;
                    
            //send a message so we know about it
            PluginMain.SendMessage("New password-protected invite: " + GenerateInvite(m.MatchId, m.GamePassword, m.GameName));
        }

        private static string GenerateInvite(int id, string password = "", string message = "") => $"[osump://{id}/{password} {message}]";
    }
}
