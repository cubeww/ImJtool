using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ImJtool
{
    /// <summary>
    /// The parent class of all map objects (block, player, warp, etc.)
    /// See IWannaObjects.cs for internal map objects
    /// </summary>
    public class MapObject
    {
        // Object fields

        float hspeed = 0;
        float vspeed = 0;
        float speed = 0;
        float direction = 0;

        // Object properties
        public bool IsInPalette => PaletteObjects.Contains(GetType());
        public bool IsSkinable => SkinableObjects.Contains(GetType());
        public bool Visible { get; set; } = true;
        public int Depth { get; set; } = 0;
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float XPrevious { get; set; } = 0;
        public float YPrevious { get; set; } = 0;
        public float XScale { get; set; } = 1;
        public float YScale { get; set; } = 1;
        /// <summary>
        /// Changes in HSpeed also affect speed and direction.
        /// </summary>
        public float HSpeed
        {
            get
            {
                return hspeed;
            }
            set
            {
                hspeed = value;
                speed = MathF.Sqrt(MathF.Pow(hspeed, 2) + MathF.Pow(vspeed, 2));
                direction = MathF.Atan2(-vspeed, hspeed) / MathF.PI * 180f;
            }
        }
        /// <summary>
        /// Changes in VSpeed also affect speed and direction.
        /// </summary>
        public float VSpeed
        {
            get
            {
                return vspeed;
            }
            set
            {
                vspeed = value;
                speed = MathF.Sqrt(MathF.Pow(hspeed, 2) + MathF.Pow(vspeed, 2));
                direction = MathF.Atan2(-vspeed, hspeed) / MathF.PI * 180f;
            }
        }
        /// <summary>
        /// Changes in Speed also affect HSpeed and VSpeed.
        /// </summary>
        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                hspeed = speed * MathF.Cos(direction * MathF.PI / 180f);
                vspeed = -speed * MathF.Sin(direction * MathF.PI / 180f);
            }
        }
        /// <summary>
        /// The direction is 0~360, and counterclockwise is positive.
        /// Changes in Direction also affect HSpeed and VSpeed.
        /// </summary>
        public float Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                hspeed = speed * MathF.Cos(direction * MathF.PI / 180f);
                vspeed = -speed * MathF.Sin(direction * MathF.PI / 180f);
            }
        }
        /// <summary>
        /// The rotation is 0~360, and counterclockwise is positive.
        /// Only affects Sprite rendering and collision detection.
        /// </summary>
        public float Rotation { get; set; } = 0;
        public float Gravity { get; set; } = 0;
        public float GravityDirection { get; set; } = 270;
        public Color Color { get; set; } = Color.White;
        public Sprite Sprite { get; private set; }
        public Sprite MaskSprite { get; private set; }
        /// <summary>
        /// Although ImageIndex is a float, it will be rounded up when rendering, and % the image count.
        /// </summary>
        public float ImageIndex { get; set; } = 0;
        /// <summary>
        /// ImageIndex will automatically add ImageSpeed after the Step method.
        /// </summary>
        public float ImageSpeed { get; set; } = 1;
        /// <summary>
        /// Try not to modify it manually. Call Destroy() method.
        /// </summary>
        public bool NeedDestroy { get; private set; } = false;

        // Call GetBoundingBox() method to get BBox_ properties.
        public int BBoxLeft { get; private set; }
        public int BBoxRight { get; private set; }
        public int BBoxTop { get; private set; }
        public int BBoxBottom { get; private set; }

        public MapObjectManager MapObjectManager
        {
            get => Jtool.Instance.MapObjectManager;
        }

        public InputManager InputManager
        {
            get => Jtool.Instance.InputManager;
        }

        public PlayerManager PlayerManager
        {
            get => Jtool.Instance.PlayerManager;
        }

        public static Dictionary<Type, string> SpriteNames = new()
        {
            [typeof(SpikeUp)] = "spike_up",
            [typeof(SpikeDown)] = "spike_down",
            [typeof(SpikeLeft)] = "spike_left",
            [typeof(SpikeRight)] = "spike_right",
            [typeof(MiniSpikeUp)] = "mini_spike_up",
            [typeof(MiniSpikeDown)] = "mini_spike_down",
            [typeof(MiniSpikeLeft)] = "mini_spike_left",
            [typeof(MiniSpikeRight)] = "mini_spike_right",
            [typeof(Block)] = "block",
            [typeof(MiniBlock)] = "mini_block",
            [typeof(Apple)] = "apple",
            [typeof(Save)] = "save",
            [typeof(Platform)] = "platform",
            [typeof(KillerBlock)] = "killer_block",
            [typeof(Water)] = "water",
            [typeof(Water2)] = "water2",
            [typeof(Water3)] = "water3",
            [typeof(WalljumpL)] = "walljump_l",
            [typeof(WalljumpR)] = "walljump_r",
            [typeof(PlayerStart)] = "player_start",
            [typeof(Warp)] = "warp",
            [typeof(JumpRefresher)] = "jump_refresher",
            [typeof(GravityArrowUp)] = "gravity_up",
            [typeof(GravityArrowDown)] = "gravity_down",
            [typeof(BulletBlocker)] = "bullet_blocker",
            [typeof(Bg)] = "bg",
            [typeof(SaveEffect)] = "save",
            [typeof(Blood)] = "blood",
            [typeof(PlayerBullet)] = "bullet",
        };
        public static List<Type> PaletteObjects = new()
        {
            typeof(SpikeUp),
            typeof(SpikeDown),
            typeof(SpikeLeft),
            typeof(SpikeRight),
            typeof(MiniSpikeUp),
            typeof(MiniSpikeDown),
            typeof(MiniSpikeLeft),
            typeof(MiniSpikeRight),
            typeof(Block),
            typeof(MiniBlock),
            typeof(Apple),
            typeof(Save),
            typeof(Platform),
            typeof(KillerBlock),
            typeof(Water),
            typeof(Water2),
            typeof(Water3),
            typeof(WalljumpL),
            typeof(WalljumpR),
            typeof(PlayerStart),
            typeof(Warp),
            typeof(JumpRefresher),
            typeof(GravityArrowUp),
            typeof(GravityArrowDown),
            typeof(BulletBlocker),
        };
        public static List<Type> SkinableObjects = new()
        {
            typeof(SpikeUp),
            typeof(SpikeDown),
            typeof(SpikeLeft),
            typeof(SpikeRight),
            typeof(MiniSpikeUp),
            typeof(MiniSpikeDown),
            typeof(MiniSpikeLeft),
            typeof(MiniSpikeRight),
            typeof(Block),
            typeof(MiniBlock),
            typeof(Apple),
            typeof(Save),
            typeof(Platform),
            typeof(KillerBlock),
            typeof(Water),
            typeof(Water2),
            typeof(Water3),
            typeof(WalljumpL),
            typeof(WalljumpR),
            typeof(PlayerStart),
            typeof(Warp),
            typeof(JumpRefresher),
            typeof(BulletBlocker),
            typeof(Bg),
            typeof(SaveEffect),
        };
        public static Dictionary<Type, List<Type>> ChildTypes { get; set; } = new();
        public void SetSprite(string spriteName)
        {
            Sprite = Jtool.Instance.ResourceManager.GetSprite(spriteName);
        }
        public void SetMask(string spriteName)
        {
            MaskSprite = Jtool.Instance.ResourceManager.GetSprite(spriteName);
        }

        /// <summary>
        /// Automatically find and set the right sprite
        /// </summary>
        public MapObject()
        {
            if (IsSkinable)
            {
                ApplySkin();
            }
            else if (SpriteNames.ContainsKey(GetType()))
            {
                Sprite = Jtool.Instance.ResourceManager.GetSprite(SpriteNames[GetType()]);
            }

            MaskSprite = Sprite;
        }
        public virtual void Create() { }

        public void BeforeStep()
        {
            XPrevious = X;
            YPrevious = Y;
        }
        public virtual void Step() { }
        public void HandleImageIndex()
        {
            ImageIndex += ImageSpeed;
        }
        public virtual void Draw()
        {
            if (Visible)
                DrawSelf();
        }

        public void HandleMovement()
        {
            HSpeed += Gravity * MathF.Cos(GravityDirection * MathF.PI / 180f);
            VSpeed -= Gravity * MathF.Sin(GravityDirection * MathF.PI / 180f);

            X += HSpeed;
            Y += VSpeed;
        }
        /// <summary>
        /// Recommended to override this method to do some collision detection.
        /// </summary>
        public virtual void AfterMovement() { }
        /// <summary>
        /// The only way to destroy object.
        /// </summary>
        public void Destroy()
        {
            Gui.Log("MapObjectManager", $"Destroyed object \"{GetType()}\" at ({X}, {Y})");
            NeedDestroy = true;
        }
        /// <summary>
        /// Draw object using the object's transform
        /// </summary>
        public void DrawSelf()
        {
            if (Sprite != null)
                Sprite.Draw(ImageIndex, X, Y, XScale, YScale, Rotation * MathF.PI / 180f, Color);
        }
        
        /// <summary>
        /// Returns whether the object placed at (x,y) meets an object of the specified type.
        /// </summary>
        public bool PlaceMeeting(float x, float y, Type type)
        {
            return InstancePlace(x, y, type) != null;
        }
        /// <summary>
        /// Returns whether the object placed at (x,y) meets an object of the specified type.
        /// </summary>
        public MapObject InstancePlace(float x, float y, Type type)
        {
            MapObject result = null;

            var pool = Jtool.Instance.MapObjectManager.GetTypeObjectsWithChildren(type);
            if (pool.Count == 0)
                return result;

            var oldX = this.X;
            var oldY = this.Y;

            this.X = x;
            this.Y = y;
            GetBoundingBox();

            foreach (var o in pool)
            {
                if (o == this || o.NeedDestroy || o.MaskSprite == null)
                    continue;

                o.GetBoundingBox();

                if (PreciseCollision(o))
                {
                    result = o;
                    break;
                }
            }

            this.X = oldX;
            this.Y = oldY;

            return result;
        }
        /// <summary>
        /// Moves the object in the direction 
        /// until just before a collision occurs
        /// </summary>
        public void MoveContact(float dir, float maxdist, Type type)
        {
            var steps = MathF.Round(maxdist);

            var dx = MathF.Cos(dir * MathF.PI / 180f);
            var dy = -MathF.Sin(dir * MathF.PI / 180f);

            if (PlaceMeeting(X, Y, type))
                return;

            for (int i = 1; i <= steps; i++)
            {
                if (!PlaceMeeting(X + dx, Y + dy, type))
                {
                    X += dx;
                    Y += dy;
                }
                else return;
            }
        }

        /// <summary>
        /// Check if this object collides with another object
        /// </summary>
        public bool PreciseCollision(MapObject obj)
        {
            var item1 = MaskSprite.GetItem(ImageIndex);
            var item2 = obj.MaskSprite.GetItem(obj.ImageIndex);

            var l = MathF.Max(BBoxLeft, obj.BBoxLeft);
            var r = MathF.Min(BBoxRight, obj.BBoxRight);
            var t = MathF.Max(BBoxTop, obj.BBoxTop);
            var b = MathF.Min(BBoxBottom, obj.BBoxBottom);

            if (XScale == 0 && YScale == 0 && Rotation == 0 && obj.XScale == 0 && obj.YScale == 0 && obj.Rotation == 0)
            {
                // Case 1: NOT Scaled, NOT Rotated
                for (var j = t; j <= b; j++)
                {
                    for (var i = l; i <= r; i++)
                    {
                        if (!PointCollision(item1, i, j, X, Y, 1, 1, 0, 1, MaskSprite.XOrigin, MaskSprite.YOrigin))
                            continue;

                        if (!PointCollision(item2, i, j, obj.X, obj.Y, 1, 1, 0, 1, obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                            continue;

                        return true;
                    }
                }
                return false;
            }
            else
            {
                var s1x = 1.0f / XScale;
                var s1y = 1.0f / YScale;
                var s2x = 1.0f / obj.XScale;
                var s2y = 1.0f / obj.YScale;

                if (Rotation == 0)
                {
                    // Case 2: Scaled, NOT Rotated
                    for (var j = t; j <= b; j++)
                    {
                        for (var i = l; i <= r; i++)
                        {
                            if (!PointCollision(item1, i, j, X, Y, s1x, s1y, 0, 1, MaskSprite.XOrigin, MaskSprite.YOrigin))
                                continue;

                            if (!PointCollision(item2, i, j, obj.X, obj.Y, s2x, s2y, 0, 1, obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                                continue;

                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    // Case 3: Scaled, Rotated
                    var ss1 = MathF.Sin(Rotation * MathF.PI / 180f);
                    var cc1 = MathF.Cos(Rotation * MathF.PI / 180f);
                    var ss2 = MathF.Sin(obj.Rotation * MathF.PI / 180f);
                    var cc2 = MathF.Cos(obj.Rotation * MathF.PI / 180f);

                    for (var j = t; j <= b; j++)
                    {
                        for (var i = l; i <= r; i++)
                        {
                            if (!PointCollision(item1, i, j, X, Y, s1x, s1y, ss1, cc1, MaskSprite.XOrigin, MaskSprite.YOrigin))
                                continue;

                            if (!PointCollision(item2, i, j, obj.X, obj.Y, s2x, s2y, ss2, cc2, obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                                continue;

                            return true;
                        }
                    }
                    return false;
                }
            }

        }

        /// <summary>
        /// Check if a point collides with a transformed sprite item. 
        /// Scaling takes the reciprocal because multiplication is more efficient.
        /// </summary>
        /// <param name="item">Sprite item to detect</param>
        /// <param name="px">X coordinate to detect</param>
        /// <param name="py">Y coordinate to detect</param>
        /// <param name="x">X coordinate of the object</param>
        /// <param name="y">Y coordinate of the object</param>
        /// <param name="rxs">Reciprocal of X scaling</param>
        /// <param name="rys">Reciprocal of Y scaling</param>
        /// <param name="ss">Sine of the object's rotation</param>
        /// <param name="cc">Cosine of the object's rotation</param>
        /// <param name="xo">Sprite's X origin</param>
        /// <param name="yo">Sprite's Y origin</param>
        /// <returns></returns>
        public static bool PointCollision(SpriteItem item, float px, float py, float x, float y, float rxs, float rys, float ss, float cc, float xo, float yo)
        {
            int xx = (int)MathF.Floor(((cc * (px - x) - ss * (py - x)) * rxs + xo));
            if ((xx < 0) || (xx >= item.W)) return false;

            int yy = (int)MathF.Floor(((cc * (py - y) + ss * (px - y)) * rys + yo));
            if ((yy < 0) || (yy >= item.H)) return false;

            return item.Data[xx + (yy * item.W)];
        }

        /// <summary>
        /// Get the bounding box of the map object in the map
        /// </summary>
        public void GetBoundingBox()
        {
            var item = MaskSprite.GetItem(ImageIndex);

            if (Rotation == 0)
            {
                // Case 1: Not Rotated
                BBoxLeft = (int)MathF.Round(X + XScale * (item.Left - MaskSprite.XOrigin));
                BBoxRight = (int)MathF.Round(X + XScale * (item.Right - MaskSprite.XOrigin + 1) - 1);

                if (BBoxLeft > BBoxRight) (BBoxLeft, BBoxRight) = (BBoxRight, BBoxLeft);

                BBoxTop = (int)MathF.Round(Y + YScale * (item.Top - MaskSprite.YOrigin));
                BBoxBottom = (int)MathF.Round(Y + YScale * (item.Bottom - MaskSprite.YOrigin + 1) - 1);

                if (BBoxTop > BBoxBottom) (BBoxTop, BBoxBottom) = (BBoxBottom, BBoxTop);
            }
            else
            {
                // Case 2: Rotated
                var xmin = XScale * (item.Left - MaskSprite.XOrigin);
                var xmax = XScale * (item.Right - MaskSprite.XOrigin + 1) - 1;

                var ymin = YScale * (item.Top - MaskSprite.YOrigin);
                var ymax = YScale * (item.Bottom - MaskSprite.YOrigin + 1) - 1;

                var cc = MathF.Cos(Rotation * MathF.PI / 180f);
                var ss = MathF.Sin(Rotation * MathF.PI / 180f);

                var ccxmax = cc * xmax;
                var ccxmin = cc * xmin;
                var ssymax = ss * ymax;
                var ssymin = ss * ymin;

                if (ccxmax < ccxmin) (ccxmax, ccxmin) = (ccxmin, ccxmax);
                if (ssymax < ssymin) (ssymax, ssymin) = (ssymin, ssymax);

                BBoxLeft = (int)MathF.Floor(X + ccxmin + ssymin);
                BBoxRight = (int)MathF.Floor(X + ccxmax + ssymax);

                var ccymax = cc * ymax;
                var ccymin = cc * ymin;
                var ssxmax = ss * xmax;
                var ssxmin = ss * xmin;

                if (ccymax < ccymin) (ccymax, ccymin) = (ccymin, ccymax);
                if (ssxmax < ssxmin) (ssxmax, ssxmin) = (ssxmin, ssxmax);

                BBoxTop = (int)MathF.Floor(Y + ccymin - ssxmax);
                BBoxBottom = (int)MathF.Floor(Y + ccymax - ssxmin);
            }

        }
        public void ApplySkin()
        {
            var skin = Jtool.Instance.SkinManager.GetCurrentObjectOfType(GetType());
            if (skin != null)
            {
                Sprite = skin.Sprite;
                ImageSpeed = skin.ImageSpeed ?? ImageSpeed;
            }
            else
            {
                Sprite = Jtool.Instance.ResourceManager.GetSprite(SpriteNames[GetType()]);
            }
        }
    }
}
