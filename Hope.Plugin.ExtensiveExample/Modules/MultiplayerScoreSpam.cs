using System;
using HOPEless.Bancho.Objects;

namespace Hope.Plugin.ExtensiveExample.Modules
{
    internal class MultiplayerScoreSpam
    {
        private bool _high = false;
        public void ModifyScorePacket(ref BanchoScoreFrame s)
        {
            s.Count300 = (ushort)(_high ? 10000 : 0);
            s.CountMiss = (ushort)(_high ? 0 : 10000);
            s.Count100 = 0;
            s.Count50 = 0;

            s.CurrentHp = _high ? 200 : 0;
            s.CurrentCombo = (ushort)(_high ? 1337 : 0);
            s.TotalScore = _high ? Int32.MaxValue : -1;

            s.CountGeki = ushort.MaxValue;
            s.CountKatu = ushort.MaxValue;

            s.Id++;

            _high = !_high;
        }
    }
}
