using System;
using HOPEless.Bancho.Objects;

namespace Hope.Plugin.ExtensiveExample
{
    internal class MultiplayerScoreSpam
    {
        private bool high = false;
        public void ModifyScorePacket(ref BanchoScoreFrame s)
        {
            s.Count300 = (ushort)(high ? 10000 : 0);
            s.CountMiss = (ushort)(high ? 0 : 10000);
            s.Count100 = 0;
            s.Count50 = 0;

            s.CurrentHp = high ? 200 : 0;
            s.CurrentCombo = (ushort)(high ? 1337 : 0);
            s.TotalScore = high ? Int32.MaxValue : -1;

            s.CountGeki = ushort.MaxValue;
            s.CountKatu = ushort.MaxValue;

            s.Id++;

            high = !high;
        }
    }
}
