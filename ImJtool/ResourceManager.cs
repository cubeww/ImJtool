using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;

namespace ImJtool
{
    /// <summary>
    /// Store all resources (texture, sprite, sound, etc.)
    /// </summary>
    public static class ResourceManager
    {
        static Dictionary<string, Sprite> sprites = new();
        static Dictionary<string, Texture2D> textures = new();

        public static Texture2D CreateTexture(string name, string filename)
        {
            var texture = Texture2D.FromFile(Jtool.Instance.GraphicsDevice, filename);
            textures[name] = texture;
            Gui.Log("ResourceManager", $"Created texture: {{ Name: {name}, File: {filename} }}");
            return texture;
        }

        public static Sprite CreateSprite(string name, int xo, int yo)
        {
            var sprite = new Sprite(xo, yo);
            sprites[name] = sprite;
            Gui.Log("ResourceManager", $"Created sprite: {{ Name: {name}, Origin: ({xo}, {yo}) }}");
            return sprite;
        }

        public static void LoadTextures()
        {
            // Load all images according to define.json
            var defineJson = File.ReadAllText("textures/define.json");
            var define = (JsonArray)JsonNode.Parse(defineJson);
            foreach (JsonNode i in define)
            {
                string filename = (string)i["file"];
                int x = i["x"] == null ? 1 : (int)i["x"];
                int y = i["y"] == null ? 1 : (int)i["y"];
                int xo = i["xo"] == null ? 0 : (int)i["xo"];
                int yo = i["yo"] == null ? 0 : (int)i["yo"];

                string name = Path.GetFileNameWithoutExtension(filename);
                var tex = CreateTexture(name, $"textures/{filename}");
                CreateSprite(name, xo, yo).AddSheet(tex, x, y);
            }
        }

        public static Sprite GetSprite(string name)
        {
            return sprites[name];
        }

        public static Texture2D GetTexture(string name)
        {
            return textures[name];
        }
    }
}
