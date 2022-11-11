using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ImJtool
{
    public static class ConfigManager
    {
        public static void Save()
        {
            var cfg = new JsonObject();
            cfg["skin"] = SkinManager.CurrentSkin.Name;
            cfg["grid"] = Editor.GridSize;
            cfg["snap"] = Editor.Snap;
            cfg["coord"] = Gui.ShowMouseCoord;

            var str = JsonSerializer.Serialize(cfg);
            File.WriteAllText("config.json", str);
        }
        public static void Load()
        {
            var str = File.ReadAllText("config.json");
            var cfg = JsonSerializer.Deserialize<JsonObject>(str);

            void ParseString(string key, Action<string> action)
            {
                if (cfg.ContainsKey(key))
                    action(cfg[key].ToString());
            }
            void ParseInt(string key, Action<int> action)
            {
                if (cfg.ContainsKey(key))
                    action(int.Parse(cfg[key].ToString()));
            }
            void ParseBool(string key, Action<bool> action)
            {
                if (cfg.ContainsKey(key))
                    action(bool.Parse(cfg[key].ToString()));
            }

            ParseString("skin", (string v) => SkinManager.ApplySkin(v));
            ParseInt("grid", (int v) => Editor.GridSize = v);
            ParseInt("snap", (int v) => Editor.Snap = v);
            ParseBool("coord", (bool v) => Gui.ShowMouseCoord = v);
        }
    }
}
