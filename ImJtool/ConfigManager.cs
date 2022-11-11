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
    public class ConfigManager
    {
        public ConfigManager Instance => Jtool.Instance.ConfigManager;
        public void Save()
        {
            var cfg = new JsonObject();
            cfg["skin"] = SkinManager.Instance.CurrentSkin.Name;
            cfg["grid"] = Editor.Instance.GridSize;
            cfg["snap"] = Editor.Instance.Snap;
            cfg["coord"] = Gui.Instance.ShowMouseCoord;

            var str = JsonSerializer.Serialize(cfg);
            File.WriteAllText("config.json", str);
        }
        public void Load()
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

            ParseString("skin", (string v) => SkinManager.Instance.ApplySkin(v));
            ParseInt("grid", (int v) => Editor.Instance.GridSize = v);
            ParseInt("snap", (int v) => Editor.Instance.Snap = v);
            ParseBool("coord", (bool v) => Gui.Instance.ShowMouseCoord = v);
        }
    }
}
