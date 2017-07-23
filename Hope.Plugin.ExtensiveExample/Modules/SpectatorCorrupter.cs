using System;
using System.Linq;
using HOPEless.Bancho.Objects;
using HOPEless.osu;

namespace Hope.Plugin.ExtensiveExample.Modules
{
    internal class SpectatorCorrupter
    {
        private static readonly Random Rand = new Random();

        public static void ModifySpectatePacket(ref BanchoReplayFrameBundle b)
        {
            b.CurrentScoreState.TotalScore = b.ReplayFrames.FirstOrDefault()?.Time ?? 0;
            b.CurrentScoreState.CurrentHp = b.ReplayFrames.FirstOrDefault()?.Time % 254 ?? 0;
            
            b.Action = ReplayAction.SongSelect;
            b.ReplayFrames.ForEach(
                a => {
                    a.MouseX = a.Time % 512f;
                    a.MouseY = a.Time % 386f;
                    a.ButtonState = (ButtonState)Rand.Next((int)ButtonState.RightKeyboard * 2 - 1); //fill bits below
                });
        }
    }
}
