using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using ImGuiNET;
using static System.Net.Mime.MediaTypeNames;

namespace ImJtool
{
    /// <summary>
    /// The sprite of the map object. 
    /// Can have multiple sprite items for easy animation playback
    /// </summary>
    public class Sprite
    {
        public int XOrigin, YOrigin;

        List<SpriteItem> items = new();

        public Sprite(int xo = 0, int yo = 0)
        {
            XOrigin = xo;
            YOrigin = yo;
        }
        /// <summary>
        /// Add a sprite sheet for the sprite and automatically crop the rectangular area.
        /// </summary>
        public void AddSheet(Texture2D texture, int col, int row)
        {
            int w = texture.Width / col;
            int h = texture.Height / row;
            for (var y = 0; y < row; y++)
            {
                for (var x = 0; x < col; x++)
                {
                    var item = new SpriteItem(texture, x * w, y * h, w, h);
                    item.GetBoundingBox();
                    items.Add(item);
                }
            }
        }
        /// <summary>
        /// Draws the sprite to the map with the specified transform.
        /// </summary>
        public void Draw(float index, float x, float y, float xscale, float yscale, float rotation, Color color)
        {
            var xx = (int)Math.Round(x);
            var yy = (int)Math.Round(y);

            var item = GetItem(index);

            var xo = XOrigin;
            var yo = YOrigin;

            var effects = SpriteEffects.None;

            if (xscale < 0)
            {
                effects |= SpriteEffects.FlipHorizontally;
                xo = item.W - xo;
            }

            if (yscale < 0)
            {
                effects |= SpriteEffects.FlipVertically;
                yo = item.H - yo;
            }

            Jtool.Instance.SpriteBatch.Draw(item.Texture, new Rectangle(xx, yy, (int)(item.W * MathF.Abs(xscale)), (int)(item.H * MathF.Abs(yscale))), new Rectangle(item.X, item.Y, item.W, item.H), color, -rotation, new Vector2(xo, yo), effects, 0);
        }

        public SpriteItem GetItem(float index)
        {
            return items[(int)Math.Round(index) % items.Count];
        }
    }
    /// <summary>
    /// Contains a texture reference and a rectangular area. 
    /// You can get the bounding box information of this area, 
    /// which is convenient for collision detection.
    /// </summary>
    public class SpriteItem
    {
        public Texture2D Texture { get; set; }
        public IntPtr ImGuiTexture { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }

        // Bounding box
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
        public bool[] Data { get; set; }

        public SpriteItem(Texture2D texture, int x, int y, int w, int h)
        {
            this.Texture = texture;
            this.X = x;
            this.Y = y;
            this.W = w;
            this.H = h;

            ImGuiTexture = Jtool.Instance.ImGuiRender.BindTexture(texture);
        }
        ~SpriteItem()
        {
            Jtool.Instance.ImGuiRender.UnbindTexture(ImGuiTexture);
        }
        /// <summary>
        /// Get the uv0 and uv1 of the sprite item for use in ImGui.
        /// </summary>
        public (System.Numerics.Vector2, System.Numerics.Vector2) GetUV()
        {
            return (new System.Numerics.Vector2(X / Texture.Width, Y / Texture.Height), new System.Numerics.Vector2((X + W) / (float)Texture.Width, (Y + H) / (float)Texture.Height));
        }

        /// <summary>
        /// Get bounding box and collision data for the sprite. Usually you only need to do it once.
        /// </summary>
        public void GetBoundingBox()
        {
            Data = new bool[W * H];
            Left = W - 1;
            Top = H - 1;
            Right = 0;
            Bottom = 0;
            var colors = new Color[Texture.Width * Texture.Height];
            Texture.GetData(colors);

            for (int yy = Y, i = 0; yy < Y + H; yy++)
            {
                for (int xx = X; xx < X + W; xx++, i++)
                {
                    if (colors[xx + yy * Texture.Width].A != 0)
                    {
                        Data[i] = true;
                        Left = Math.Min(Left, xx - X);
                        Top = Math.Min(Top, yy - Y);
                        Right = Math.Max(Right, xx - X);
                        Bottom = Math.Max(Bottom, yy - Y);
                    }
                    else
                    {
                        Data[i] = false;
                    }
                }
            }
        }
    }
}
