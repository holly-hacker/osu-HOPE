using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
