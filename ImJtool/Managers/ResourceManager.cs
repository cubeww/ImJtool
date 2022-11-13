using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace ImJtool.Managers
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
            return texture;
        }

        public static Sprite CreateSprite(string name, int xo, int yo)
        {
            var sprite = new Sprite(xo, yo);
            sprites[name] = sprite;
            return sprite;
        }

        public static void LoadTextures()
        {
            // Load all images according to define.json
            var defineJson = File.ReadAllText("configs/sprites.json");
            var define = JsonNode.Parse(defineJson).AsObject();
            foreach ((string name, JsonNode val) in define)
            {
                string filename = (string)val["file"];
                int x = (int)(val["x"] ?? 1);
                int y = (int)(val["y"] ?? 1);
                int xo = (int)(val["xo"] ?? 1);
                int yo = (int)(val["yo"] ?? 1);

                var tex = CreateTexture(name, filename);
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
