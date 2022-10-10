using Microsoft.VisualBasic.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ImJtool
{
    public class Player : MapObject
    {
        // Player fields
        public float jump = 8.5f;
        public float jump2 = 7f;
        public float maxHspeed = 3f;
        public float maxVspeed = 9f;
        public bool onPlatform = false;
        public bool djump = true;
        public override void Create()
        {
            Gravity = 0.4f;
            Depth = -10;

            SetSprite("player_idle");
            SetMask("player_mask");

            PlayerManager.Instance.Player = this;
        }

        public override void Step()
        {
            if (PlayerManager.Instance.Dotkid)
            {
                SetMask("dotkid");
            }
            else
            {
                if (PlayerManager.Instance.Grav == 1)
                {
                    SetMask("player_mask");
                }
                else
                {
                    SetMask("player_mask_flip");
                }
            }

            var L = InputManager.Instance.IsKeyHold(Keys.Left);
            var R = InputManager.Instance.IsKeyHold(Keys.Right);

            float h = 0;
            if (R) h = 1;
            else if (L) h = -1;

            if (h != 0)
            {
                PlayerManager.Instance.Face = h;
                SetSprite("player_running");
                ImageSpeed = 0.5f;
            }
            else
            {
                SetSprite("player_idle");
                ImageSpeed = 0.2f;
            }
            HSpeed = maxHspeed * h;
            if (!onPlatform)
            {
                if (VSpeed * PlayerManager.Instance.Grav < -0.05f)
                {
                    SetSprite("player_jump");
                }
                else if (VSpeed * PlayerManager.Instance.Grav > 0.05f)
                {
                    SetSprite("player_fall");
                }
            }
            if (!PlaceMeeting(X, Y + 4 * PlayerManager.Instance.Grav, typeof(Platform)))
            {
                onPlatform = false;
            }

            if (MathF.Abs(VSpeed) > maxVspeed)
            {
                VSpeed = MathF.Sign(VSpeed) * maxVspeed;
            }

            var water = PlaceMeeting(X, Y, typeof(Water));
            var water2 = PlaceMeeting(X, Y, typeof(Water2));
            var water3 = PlaceMeeting(X, Y, typeof(Water3));

            if (water || water2 || water3)
            {
                VSpeed = MathF.Min(VSpeed * PlayerManager.Instance.Grav, 2.0f) * PlayerManager.Instance.Grav;

                if (!water2)
                {
                    djump = true;
                }
            }

            if (InputManager.Instance.IsKeyPress(Keys.Z))
            {
                if (MapObjectManager.Instance.GetCount(typeof(PlayerBullet)) < 4)
                {
                    var by = Y;
                    if (PlayerManager.Instance.Dotkid)
                    {
                        by = Y + 6;
                    }
                    var b = MapObjectManager.Instance.CreateObject(X, by, typeof(PlayerBullet));
                    b.HSpeed = PlayerManager.Instance.Face * 16;
                }
            }
            // Jump press
            if (InputManager.Instance.IsKeyPress(Keys.LeftShift) || InputManager.Instance.IsKeyPress(Keys.RightShift))
            {
                if (PlaceMeeting(X, Y + 1 * PlayerManager.Instance.Grav, typeof(Block)) || PlaceMeeting(X, Y + 1 * PlayerManager.Instance.Grav, typeof(Platform)) || onPlatform || water)
                {
                    VSpeed = -jump * PlayerManager.Instance.Grav;
                    djump = true;
                }
                else if (djump || water2 || PlayerManager.Instance.Infjump)
                {
                    SetSprite("player_jump");
                    VSpeed = -jump2 * PlayerManager.Instance.Grav;
                    if (!water3)
                        djump = false;
                    else djump = true;
                }
            }
            // Jump release
            if (InputManager.Instance.IsKeyRelease(Keys.LeftShift) || InputManager.Instance.IsKeyRelease(Keys.RightShift))
            {
                if (VSpeed * PlayerManager.Instance.Grav < 0)
                {
                    VSpeed *= 0.45f;
                }
            }

            // vine
            var notOnBlock = !PlaceMeeting(X, Y + 1 * PlayerManager.Instance.Grav, typeof(Block));
            var onVineL = PlaceMeeting(X - 1, Y, typeof(WalljumpL)) && notOnBlock;
            var onVineR = PlaceMeeting(X + 1, Y, typeof(WalljumpR)) && notOnBlock;
            if (onVineL || onVineR)
            {
                if (onVineL)
                {
                    PlayerManager.Instance.Face = 1;
                }
                else
                {
                    PlayerManager.Instance.Face = -1;
                }

                VSpeed = 2 * PlayerManager.Instance.Grav;

                SetSprite("player_sliding");
                ImageSpeed = 0.5f;

                if ((onVineL && InputManager.Instance.IsKeyPress(Keys.Right)) || (onVineR &&
                    InputManager.Instance.IsKeyPress(Keys.Right)))
                {
                    if (InputManager.Instance.IsKeyHold(Keys.LeftShift) || InputManager.Instance.IsKeyHold(Keys.RightShift))
                    {
                        if (onVineR) HSpeed = -15;
                        else HSpeed = 15;
                        VSpeed = -9 * PlayerManager.Instance.Grav;
                        SetSprite("player_jump");
                    }
                    else
                    {
                        if (onVineR) HSpeed = -3;
                        else HSpeed = 3;
                        SetSprite("player_fall");
                    }
                }
            }

            // A / D adjust
            if (PlaceMeeting(X, Y + PlayerManager.Instance.Grav, typeof(Block)))
            {
                if (InputManager.Instance.IsKeyPress(Keys.A))
                {
                    HSpeed = -1;
                }
                if (InputManager.Instance.IsKeyPress(Keys.D))
                {
                    HSpeed = 1;
                }
            }
        }

        public override void AfterMovement()
        {
            // Block
            var block = InstancePlace(X, Y, typeof(Block));
            if (block != null)
            {
                X = XPrevious;
                Y = YPrevious;

                if (PlaceMeeting(X + HSpeed, Y, typeof(Block)))
                {
                    if (PlayerManager.Instance.Grav == 1)
                    {
                        if (HSpeed <= 0) MoveContact(180, MathF.Abs(HSpeed), typeof(Block));
                        if (HSpeed > 0) MoveContact(0, MathF.Abs(HSpeed), typeof(Block));
                    }
                    else
                    {
                        if (HSpeed < 0) MoveContact(180, MathF.Abs(HSpeed), typeof(Block));
                        if (HSpeed >= 0) MoveContact(0, MathF.Abs(HSpeed), typeof(Block));
                    }
                    HSpeed = 0;
                }
                if (PlaceMeeting(X, Y + VSpeed, typeof(Block)))
                {
                    if (PlayerManager.Instance.Grav == 1)
                    {
                        if (VSpeed <= 0)
                        {
                            MoveContact(90, MathF.Abs(VSpeed), typeof(Block));
                        }
                        if (VSpeed > 0)
                        {
                            MoveContact(270, MathF.Abs(VSpeed), typeof(Block));
                            djump = true;
                        }
                    }
                    else
                    {
                        if (VSpeed <= 0)
                        {
                            MoveContact(90, MathF.Abs(VSpeed), typeof(Block));
                            djump = true;
                        }
                        if (VSpeed > 0)
                        {
                            MoveContact(270, MathF.Abs(VSpeed), typeof(Block));
                        }
                    }
                    VSpeed = 0;
                }
                if (PlaceMeeting(X + HSpeed, Y + VSpeed, typeof(Block)))
                {
                    HSpeed = 0;
                }

                X += HSpeed;
                Y += VSpeed;
            }

            // Platform
            var pf = InstancePlace(X, Y, typeof(Platform));
            if (pf != null)
            {
                if (PlayerManager.Instance.Grav == 1)
                {
                    if (Y - VSpeed / 2 <= pf.Y)
                    {
                        Y = pf.Y - 9;
                        VSpeed = 0;

                        onPlatform = true;
                        djump = true;
                    }
                }
                else
                {
                    if (Y - VSpeed / 2 >= pf.Y + pf.Sprite.GetItem(0).H - 1)
                    {
                        Y = pf.Y + pf.Sprite.GetItem(0).H + 8;
                        VSpeed = 0;

                        onPlatform = true;
                        djump = true;
                    }
                }
            }

            // Jump refresher
            var jr = (JumpRefresher)InstancePlace(X, Y, typeof(JumpRefresher));
            if (jr != null)
            {
                djump = true;
                jr.Refresh();
            }

            // Gravity arrow
            if (PlayerManager.Instance.Grav == 1 && PlaceMeeting(X, Y, typeof(GravityArrowUp)))
            {
                FlipGrav();
            }

            if (PlayerManager.Instance.Grav == -1 && PlaceMeeting(X, Y, typeof(GravityArrowDown)))
            {
                FlipGrav();
            }

            // Killer
            var killer = (Killer)InstancePlace(X, Y, typeof(Killer));
            if (killer != null)
            {
                killer.SetHighlight();
                if (PlayerManager.Instance.DeathEnable)
                {
                    Kill();
                    return;
                }
            }

            // Border
            if (X < 0 || X > 800 || Y < 0 || Y > 608)
            {
                Kill();
                return;
            }
        }
        public void Kill()
        {
            for (int i = 0; i < 200; i++)
            {
                MapObjectManager.Instance.CreateObject(X, Y, typeof(Blood));
            }
            Destroy();
        }
        public override void Draw()
        {
            if (!PlayerManager.Instance.Dotkid)
            {
                if (PlayerManager.Instance.ShowMask == ShowMask.OnlyPlayer)
                {
                    Sprite.Draw(ImageIndex, X, Y, PlayerManager.Instance.Face, PlayerManager.Instance.Grav, Rotation, Color);
                }
                else if (PlayerManager.Instance.ShowMask == ShowMask.OnlyMask)
                {
                    MaskSprite.Draw(ImageIndex, X, Y, 1, 1, 0, Color);
                }
                else
                {
                    var col = Color.White * 0.5f;
                    Sprite.Draw(ImageIndex, X, Y, PlayerManager.Instance.Face, PlayerManager.Instance.Grav, Rotation, col);
                    MaskSprite.Draw(ImageIndex, X, Y, 1, 1, 0, col);
                }
            }
            else
            {
                MaskSprite.Draw(ImageIndex, X, Y, 1, 1, 0, Color);
                if (PlayerManager.Instance.DotkidOutline)
                {
                    ResourceManager.Instance.GetSprite("dotkid_outline").Draw(0, X, Y + 8, 1, 1, 0, Color.White);
                }
            }
        }

        public void FlipGrav()
        {
            PlayerManager.Instance.Grav *= -1;
            Gravity = 0.4f * PlayerManager.Instance.Grav;
            djump = true;
            VSpeed = 0;

            if (!PlayerManager.Instance.Dotkid)
            {
                if (PlayerManager.Instance.Grav == 1)
                {
                    SetMask("player_mask");
                }
                else
                {
                    SetMask("player_mask_flip");
                }
            }
            Y += 4 * PlayerManager.Instance.Grav;
        }
    }
    public class Apple : Killer
    {
        public override void Create()
        {
            Depth = 0;
            ImageSpeed = 1 / 15f;
        }
    }
    public class Block : MapObject
    {
        public override void Create()
        {
            Depth = 1;
        }
        public override void Step()
        {

        }
    }
    public class Platform : MapObject
    {
        public override void Create()
        {
            Depth = 10;
        }
        public override void Step()
        {
        }
    }
    public class Killer : MapObject
    {
        bool highlight = false;
        int highlightTimer = 0;
        public void SetHighlight()
        {
            highlight = true;
            highlightTimer = 10;
        }
        public override void Create()
        {
        }
        public override void Step()
        {
            if (highlight)
            {
                Color = new Color(255, 204, 204);
                if (highlightTimer-- == 0)
                {
                    Color = Color.White;
                    highlight = false;
                }
            }
        }
    }
    public class Save : MapObject
    {
        int timer = 0;
        int timer2 = 0;
        bool canSave = true;
        public override void Create()
        {
            ImageSpeed = 0;
            Depth = 1;
        }
        public override void Step()
        {
            if (--timer == 0)
                canSave = true;

            if (--timer2 == 0)
                ImageIndex = 0;

            void SavePlayer()
            {
                if (canSave)
                {
                    timer = 30;
                    timer2 = 59;
                    ImageIndex = 1;
                    canSave = false;
                    PlayerManager.Instance.Save();
                }
            }

            var press = InputManager.Instance.IsKeyPress(Keys.Z);
            var enter = PlaceMeeting(X, Y, typeof(Player));
            if (PlayerManager.Instance.SaveType == SaveType.OnlyShoot)
            {
                if (enter && press)
                {
                    SavePlayer();
                }
            }
            else
            {
                if ((enter && press) || PlaceMeeting(X, Y, typeof(PlayerBullet)))
                {
                    SavePlayer();
                }
            }
        }
    }
    public class Water : MapObject
    {
        public override void Create()
        {
            Depth = -50;
        }
        public override void Step()
        {
        }
    }
    public class Water2 : MapObject
    {
        public override void Create()
        {
            Depth = -50;
        }
        public override void Step()
        {
        }
    }
    public class Water3 : MapObject
    {
        public override void Create()
        {
            Depth = -50;
        }
        public override void Step()
        {
        }
    }
    public class WalljumpL : MapObject
    {
        public override void Create()
        {
            Depth = -1;
        }
        public override void Step()
        {
        }
    }
    public class WalljumpR : MapObject
    {
        public override void Create()
        {
            Depth = -1;
        }
        public override void Step()
        {
        }
    }
    public class GravityArrowUp : MapObject
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
        }
    }
    public class GravityArrowDown : MapObject
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
        }
    }
    public class JumpRefresher : MapObject
    {
        int count = 0;
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            if (count > 0)
                count--;

            if (count == 0)
            {
                Color = Color.White;
            }
            else
            {
                Color = Color.White * 0.1f;
            }
        }
        public void Refresh()
        {
            count = 100;
        }
    }
    public class Warp : MapObject
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
        }
    }
    public class Blood : MapObject
    {
        Random random = new();
        public override void Create()
        {
            Depth = -5;

            ImageIndex = (float)random.NextDouble() * 2f;
            ImageSpeed = 0;
            Gravity = (0.1f + (float)random.NextDouble() * 0.2f) * PlayerManager.Instance.Grav;

            Direction = (float)random.NextDouble() * 360f;
            Speed = (float)random.NextDouble() * 6f;

            XScale = 1.5f;
            YScale = XScale;
        }

        public override void AfterMovement()
        {
            if (PlaceMeeting(X, Y, typeof(Block)))
            {
                X = XPrevious;
                Y = YPrevious;

                MoveContact(Direction, Speed, typeof(Block));

                HSpeed = VSpeed = Gravity = 0;
            }
        }
    }
    public class PlayerBullet : MapObject
    {
        int timer = 40;
        public override void Create()
        {
            Depth = -1;
            ImageSpeed = 1;
        }
        public override void Step()
        {
            if (--timer == 0)
            {
                Destroy();
                return;
            }
            if (X < 0 || X > 800)
            {
                Destroy();
                return;
            }
            if (PlaceMeeting(X, Y, typeof(Block)) || PlaceMeeting(X, Y, typeof(BulletBlocker)))
            {
                Destroy();
                return;
            }
        }
    }
    public class BulletBlocker : MapObject
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
        }
    }
    public class SpikeUp : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class SpikeDown : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class SpikeLeft : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class SpikeRight : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class MiniSpikeUp : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class MiniSpikeDown : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class MiniSpikeLeft : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class MiniSpikeRight : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class KillerBlock : Killer
    {
        public override void Create()
        {
            Depth = 0;
        }
        public override void Step()
        {
            base.Step();
        }
    }
    public class PlayerStart : MapObject
    {
        public override void Create()
        {
            Depth = 0;

            foreach (var o in MapObjectManager.Instance.Objects)
            {
                if (o.GetType() == GetType() && o != this)
                {
                    o.Destroy();
                }

            }
            MapObjectManager.Instance.DestroyByType(typeof(Player));
            MapObjectManager.Instance.DestroyByType(typeof(Blood));
            MapObjectManager.Instance.CreateObject(X + 17, Y + 23, typeof(Player));
            PlayerManager.Instance.Save();
            Depth = 0;
        }
        public override void Step()
        {
        }
    }
    public class MiniBlock : MapObject
    {
        public override void Create()
        {
            Depth = 1;
        }
        public override void Step()
        {
        }
    }
    public class Bg : MapObject
    {
        public override void Create()
        {
            Depth = 100;
        }
        public override void Step()
        {
            HSpeed = SkinManager.Instance.CurrentSkin.HSpeed;
            VSpeed = SkinManager.Instance.CurrentSkin.VSpeed;
        }
        public override void Draw()
        {
            var sb = Jtool.Instance.SpriteBatch;
            sb.End();

            if (SkinManager.Instance.CurrentSkin.BgType == BgType.Tile)
            {
                sb.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap);
                var item = Sprite.GetItem(0);

                while (X < 0)
                    X += item.W;

                while (X > item.W)
                    X -= item.W;

                while (Y < 0)
                    Y += item.H;

                while (Y > item.H)
                    Y -= item.H;

                var w = item.W + 800 + item.W;
                var h = item.H + 800 + item.H;
                sb.Draw(item.Texture, new Vector2(-item.W + X, -item.H + Y), new Rectangle(0, 0, w, h), Color.White);
                sb.End();
            }
            else
            {
                sb.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
                var item = Sprite.GetItem(0);

                while (X < 0)
                    X += 800;

                while (X > 800)
                    X -= 800;

                while (Y < 0)
                    Y += 608;

                while (Y > 608)
                    Y -= 608;

                var xx = (int)X;
                var yy = (int)Y;

                sb.Draw(item.Texture, new Rectangle(xx - 800, yy - 608, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx + 0, yy - 608, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx + 800, yy - 608, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx - 800, yy + 0, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx + 0, yy + 0, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx + 800, yy + 0, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx - 800, yy + 608, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx + 0, yy + 608, 800, 608), null, Color.White);
                sb.Draw(item.Texture, new Rectangle(xx + 800, yy + 608, 800, 608), null, Color.White);
                sb.End();
            }

            sb.Begin();
        }
        //int CycleNum(int v, int vmin, int vmax)
        //{
        //    return (v - vmin + (1 + (int)MathF.Abs(v)) * (vmax - vmin)) % (vmax - vmin) + vmin;
        //}
    }
    public class Grid : MapObject
    {
        public override void Create()
        {
            Depth = 99;
        }
        public override void Step()
        {
        }
    }
    public class BorderBlock : Block
    {
        public override void Create()
        {
            Depth = 0;
            SetMask("block");
        }
        public override void Step()
        {
        }
    }
    public class SaveEffect : MapObject
    {
        public override void Create()
        {
            Depth = -1;
        }
        public override void Step()
        {
        }
    }
}
