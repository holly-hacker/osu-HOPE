using System;
using System.Collections.Generic;
using HOPEless.Bancho;

namespace osu_HOPE.Plugin
{
    public interface IHopePlugin
    {
        PluginMetadata GetMetadata();
        void Load();
        void OnBanchoRequest(ref List<BanchoPacket> plist);
        void OnBanchoResponse(ref List<BanchoPacket> plist);
    }
}
