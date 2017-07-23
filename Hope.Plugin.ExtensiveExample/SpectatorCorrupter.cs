using System;
using HOPEless.Bancho.Objects;
using HOPEless.osu;

namespace Hope.Plugin.ExtensiveExample
{
    internal class SpectatorCorrupter
    {
        private static readonly Random Rand = new Random();

        public static void ModifySpectatePacket(ref BanchoReplayFrameBundle b)
        {
            b.CurrentScoreState.TotalScore = Rand.Next(int.MaxValue);
            b.CurrentScoreState.CurrentHp = Rand.Next(220);
            
            b.Action = ReplayAction.SongSelect;
            b.ReplayFrames.ForEach(
                a => {
                    a.MouseX = (float)Rand.NextDouble()*512f;
                    a.MouseY = (float)Rand.NextDouble()*386f;
                    a.ButtonState = (ButtonState)Rand.Next((int)ButtonState.Smoke * 2 - 1);
                });
        }
    }
}
