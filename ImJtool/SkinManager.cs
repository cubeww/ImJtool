using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;

namespace ImJtool
{
    public class SkinManager
    {
        public static SkinManager Instance => Jtool.Instance.SkinManager;
        /// <summary>
        /// Skin Package used by current map objects.
        /// </summary>
        public SkinPackage CurrentSkin { get; set; }
        /// <summary>
        /// Skin Package used by preview of Skin selector.
        /// </summary>
        public SkinPackage PreviewSkin { get; set; }
        /// <summary>
        /// All names obtained from skins.ini
        /// </summary>
        public List<string> SkinNames { get; set; } = new();
        /// <summary>
        /// Load skins.ini and get the skin names.
        /// </summary>
        public void LoadConfig()
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile("skins/skins.ini");

            SkinNames = data["skins"]["names"].Split(',').ToList();
            SkinNames.Insert(0, "<default>");
            Gui.Log("SkinManager", $"Loaded skins.ini, got {SkinNames.Count} names");

            CurrentSkin = new SkinPackage();
        }
        /// <summary>
        /// Apply the skin package named "name".
        /// It iterates over all map objects that support skins and switches their sprites.
        /// </summary>
        public void ApplySkin(string name)
        {
            var package = new SkinPackage(name);
            CurrentSkin = package;

            foreach (var obj in MapObjectManager.Instance.Objects)
            {
                if (MapObject.SkinableObjects.Contains(obj.GetType()))
                {
                    obj.ApplySkin();
                }
            }
        }
        /// <summary>
        /// Will get the skin object of the specified type of object in the current skin package.
        /// </summary>
        public SkinObject GetCurrentObjectOfType(Type type)
        {
            return CurrentSkin.SkinObjects.ContainsKey(type) ? CurrentSkin.SkinObjects[type] : null;
        }
        /// <summary>
        /// Will try to get the sprite of the specified type of object in the current skin package.
        /// If the sprite does not exist, the default sprite is returned.
        /// </summary>
        public Sprite GetCurrentSpriteOfType(Type type)
        {
            if (MapObject.SkinableObjects.Contains(type) && CurrentSkin.SkinObjects.ContainsKey(type))
                return CurrentSkin.SkinObjects[type].Sprite;
            else return ResourceManager.Instance.GetSprite(MapObject.SpriteNames[type]);
        }
        /// <summary>
        /// Will get the skin object of the specified type of object in the preview skin package.
        /// </summary>
        public SkinObject GetPreviewObjectOfType(Type type)
        {
            return PreviewSkin.SkinObjects.ContainsKey(type) ? CurrentSkin.SkinObjects[type] : null;
        }
        /// <summary>
        /// Will try to get the sprite of the specified type of object in the preview skin package.
        /// If the sprite does not exist, the default sprite is returned.
        /// </summary>
        public Sprite GetPreviewSpriteOfType(Type type)
        {
            if (PreviewSkin.SkinObjects.ContainsKey(type))
                return PreviewSkin.SkinObjects[type].Sprite;
            else return ResourceManager.Instance.GetSprite(MapObject.SpriteNames[type]);
        }
    }
    public enum BgType
    {
        Tile,
        Stretch,
    };
    /// <summary>
    /// Consists of skin objects.
    /// </summary>
    public class SkinPackage
    {
        public string Name { get; set; }
        public BgType BgType { get; set; }
        public float HSpeed { get; set; }
        public float VSpeed { get; set; }
        public Dictionary<Type, SkinObject> SkinObjects { get; set; } = new();
        /// <summary>
        /// Create a new skin package, and load the skin package from the "textures" folder.
        /// </summary>
        public SkinPackage()
        {
            LoadDefault();
        }
        /// <summary>
        /// Create a new skin package, and load the skin package from the "skins/name" folder.
        /// </summary>
        public SkinPackage(string folder)
        {
            LoadSkinFolder(folder);
        }
        /// <summary>
        /// Add a skin object from the image filename and parameters.
        /// </summary>
        void AddSkinObject(Type type, string filename, int xo, int yo, int col = 1, int row = 1, float? speed = null, bool tile = false)
        {
            if (File.Exists(filename))
            {
                SkinObjects[type] = new SkinObject(filename, xo, yo, col, row, speed, tile);
            }
        }
        /// <summary>
        /// Generate the skin package from the "skins/name" folder.
        /// </summary>
        public void LoadSkinFolder(string folder)
        {
            if (folder == "<default>")
            {
                LoadDefault();
                return;
            }

            Name = folder;
            var path = $"skins/{folder}/";
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(path + "skin_config.ini");

            T ReadConfig<T>(string section, string key, T def)
            {
                if (data.Sections.ContainsSection(section) && data[section].ContainsKey(key))
                {
                    return (T)Convert.ChangeType(data[section][key], typeof(T));
                }
                return def;
            }

            var spikeFrames = ReadConfig("objects", "spike_frames", 1);
            var spikeSpeed = ReadConfig("objects", "spike_animspeed", 1.0f);

            var miniSpikeFrames = ReadConfig("objects", "minispike_frames", 1);
            var miniSpikeSpeed = ReadConfig("objects", "minispike_animspeed", 1.0f);

            var bgtype = ReadConfig("bg", "type", "stretch");

            if (bgtype == "stretch")
            {
                BgType = BgType.Stretch;
            }
            else
            {
                BgType = BgType.Tile;
            }

            HSpeed = ReadConfig("bg", "hspeed", 0);
            VSpeed = ReadConfig("bg", "vspeed", 0);

            AddSkinObject(typeof(SpikeUp), path + "spikeup.png", 0, 0, spikeFrames, 1, spikeSpeed);
            AddSkinObject(typeof(SpikeDown), path + "spikedown.png", 0, 0, spikeFrames, 1, spikeSpeed);
            AddSkinObject(typeof(SpikeLeft), path + "spikeleft.png", 0, 0, spikeFrames, 1, spikeSpeed);
            AddSkinObject(typeof(SpikeRight), path + "spikeright.png", 0, 0, spikeFrames, 1, spikeSpeed);

            AddSkinObject(typeof(MiniSpikeUp), path + "miniup.png", 0, 0, miniSpikeFrames, 1, miniSpikeSpeed);
            AddSkinObject(typeof(MiniSpikeDown), path + "minidown.png", 0, 0, miniSpikeFrames, 1, miniSpikeSpeed);
            AddSkinObject(typeof(MiniSpikeLeft), path + "minileft.png", 0, 0, miniSpikeFrames, 1, miniSpikeSpeed);
            AddSkinObject(typeof(MiniSpikeRight), path + "miniright.png", 0, 0, miniSpikeFrames, 1, miniSpikeSpeed);

            AddSkinObject(typeof(Apple), path + "apple.png", 10, 12, 2, 1);
            AddSkinObject(typeof(KillerBlock), path + "killerblock.png", 0, 0);

            AddSkinObject(typeof(Block), path + "block.png", 0, 0);
            AddSkinObject(typeof(MiniBlock), path + "miniblock.png", 0, 0);
            AddSkinObject(typeof(BulletBlocker), path + "bulletblocker.png", 0, 0);
            AddSkinObject(typeof(Platform), path + "platform.png", 0, 0);

            AddSkinObject(typeof(WalljumpL), path + "walljumpL.png", 0, 0);
            AddSkinObject(typeof(WalljumpR), path + "walljumpR.png", 0, 0);
            AddSkinObject(typeof(Water), path + "water1.png", 0, 0);
            AddSkinObject(typeof(Water2), path + "water2.png", 0, 0);
            AddSkinObject(typeof(Water3), path + "water3.png", 0, 0);

            AddSkinObject(typeof(Warp), path + "warp.png", 0, 0);
            AddSkinObject(typeof(PlayerStart), path + "playerstart.png", 0, 0);
            AddSkinObject(typeof(JumpRefresher), path + "jumprefresher.png", 15, 15);

            AddSkinObject(typeof(Save), path + "save.png", 0, 0, 2, 1);

            AddSkinObject(typeof(Bg), path + "bg.png", 0, 0, 1, 1, 0, BgType == BgType.Tile);
        }
        /// <summary>
        /// Generate the skin package from the "textures" folder.
        /// </summary>
        public void LoadDefault()
        {
            Name = "<default>";
            var path = "textures/";

            BgType = BgType.Stretch;
            HSpeed = 0;
            VSpeed = 0;

            AddSkinObject(typeof(SpikeUp), path + "spike_up.png", 0, 0, 1, 1, 0);
            AddSkinObject(typeof(SpikeDown), path + "spike_down.png", 0, 0, 1, 1, 0);
            AddSkinObject(typeof(SpikeLeft), path + "spike_left.png", 0, 0, 1, 1, 0);
            AddSkinObject(typeof(SpikeRight), path + "spike_right.png", 0, 0, 1, 1, 0);

            AddSkinObject(typeof(MiniSpikeUp), path + "mini_spike_up.png", 0, 0, 1, 1, 0);
            AddSkinObject(typeof(MiniSpikeDown), path + "mini_spike_down.png", 0, 0, 1, 1, 0);
            AddSkinObject(typeof(MiniSpikeLeft), path + "mini_spike_left.png", 0, 0, 1, 1, 0);
            AddSkinObject(typeof(MiniSpikeRight), path + "mini_spike_right.png", 0, 0, 1, 1, 0);

            AddSkinObject(typeof(Apple), path + "apple.png", 10, 12, 2, 1);
            AddSkinObject(typeof(KillerBlock), path + "killer_block.png", 0, 0);

            AddSkinObject(typeof(Block), path + "block.png", 0, 0);
            AddSkinObject(typeof(MiniBlock), path + "mini_block.png", 0, 0);
            AddSkinObject(typeof(BulletBlocker), path + "bullet_blocker.png", 0, 0);
            AddSkinObject(typeof(Platform), path + "platform.png", 0, 0);

            AddSkinObject(typeof(WalljumpL), path + "walljump_l.png", 0, 0);
            AddSkinObject(typeof(WalljumpR), path + "walljump_r.png", 0, 0);
            AddSkinObject(typeof(Water), path + "water.png", 0, 0);
            AddSkinObject(typeof(Water2), path + "water2.png", 0, 0);
            AddSkinObject(typeof(Water3), path + "water3.png", 0, 0);

            AddSkinObject(typeof(Warp), path + "warp.png", 0, 0);
            AddSkinObject(typeof(PlayerStart), path + "player_start.png", 0, 0);
            AddSkinObject(typeof(JumpRefresher), path + "jump_refresher.png", 15, 15);

            AddSkinObject(typeof(Save), path + "save.png", 0, 0, 2, 1);

            AddSkinObject(typeof(Bg), path + "bg.png", 0, 0, 1, 1, 0, false);
        }
    }

    public class SkinObject
    {
        public Texture2D Texture { get; private set; }
        public Sprite Sprite { get; private set; }
        public float? ImageSpeed { get; private set; }
        public SkinObject(string filename, int xo, int yo, int col, int row, float? speed, bool tile)
        {
            Texture = Texture2D.FromFile(Jtool.Instance.GraphicsDevice, filename);
            Sprite = new Sprite(xo, yo);
            Sprite.AddSheet(Texture, col, row);
            ImageSpeed = speed;
        }
    }
}
