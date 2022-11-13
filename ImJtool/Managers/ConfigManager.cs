using System.IO;
using System.Text.Json;

namespace ImJtool.Managers
{
    public class ConfigManager
    {
        private const string ConfigFile = "configs/config.json";
        public static ConfigManager Current { get; set; }
        public string SkinName
        {
            get => SkinManager.CurrentSkin.Name;
            set
            {
                SkinManager.CurrentSkin.Name = value;
                SkinManager.ApplyToCurrent(value);
            }
        }
        public int Grid
        {
            get => Editor.GridSize;
            set => Editor.GridSize = value;
        }
        public int Snap
        {
            get => Editor.Snap;
            set => Editor.Snap = value;
        }
        public bool ShowMouseCoord
        {
            get => Gui.ShowMouseCoord;
            set => Gui.ShowMouseCoord = value;
        }
        public static void Initialize()
        {
            Current = new ConfigManager();
        }
        public static void Save()
        {
            var str = JsonSerializer.Serialize(Current, new JsonSerializerOptions() { WriteIndented = true }); ;
            File.WriteAllText(ConfigFile, str);
        }
        public static void Load()
        {
            if (File.Exists(ConfigFile))
            {
                var str = File.ReadAllText(ConfigFile);
                Current = JsonSerializer.Deserialize<ConfigManager>(str);
            }
        }
    }
}
