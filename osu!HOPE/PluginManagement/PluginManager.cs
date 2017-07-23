using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using osu_HOPE.Plugin;

namespace osu_HOPE.PluginManagement
{
    internal class PluginManager
    {
        public List<IHopePlugin> Plugins = new List<IHopePlugin>();

        public void LoadPlugins()
        {
            //clear previously loaded plugins
            Plugins.Clear();

            //find all plugins DLL's matching pattern
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "Hope.Plugin.*.dll")) {
                //load the assembly
                Assembly ass = Assembly.LoadFile(file);

                //find all types that inherit IHopePlugin, add an instance to the list
                foreach (Type type in ass.GetTypes().Where(a => a.GetInterfaces().Contains(typeof(IHopePlugin))))
                    Plugins.Add((IHopePlugin) Activator.CreateInstance(type));
            }
        }
    }
}
