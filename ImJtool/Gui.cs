using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using Vector2 = System.Numerics.Vector2;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Win32;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using System.Windows.Forms;

namespace ImJtool
{
    /// <summary>
    /// Define all GUI elements
    /// </summary>
    public class Gui
    {
        // Show window fields
        bool showMapWindow = true;
        bool showSnapWindow = false;
        bool showShiftWindow = false;
        bool showGrid = false;
        bool showGridWindow = false;
        bool showMouseCoord = false;
        bool showSkinWindow = false;
        bool showPaletteWindow = true;
        bool showLogWindow = true;
        bool showAnalysisWindow = false;
        bool showAboutWindow = false;
        bool showConfirmOpenMap = false;

        /// <summary>
        /// Due to some limitations of ImGui.Image, 
        /// it is now necessary to customize the texture for each Palette Object.
        /// </summary>
        Dictionary<Type, SpriteItem> paletteIcons = new();

        List<(string, string)> logText = new();
        bool scrollToBottom = true;

        string skinSearchString = "";
        List<int> skinSearchList = new();
        int skinSelect = 0;

        /// <summary>
        /// The scale of the map window.
        /// On high resolution screens (140ppi+), I recommend setting this larger.
        /// </summary>
        public float MapWindowScale { get; set; } = 1.5f;
        public static float TitleBarHeight => ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;

        public IntPtr MapTexture { get; set; }
        /// <summary>
        /// Because the objects sprites of the palette need to have a fixed size, 
        /// we need to generate textures for them separately.
        /// </summary>
        public void GeneratePaletteIcons()
        {
            foreach (var type in MapObject.PaletteObjects)
            {
                var item = Jtool.Instance.SkinManager.GetCurrentSpriteOfType(type).GetItem(0);
                var tex = item.Texture;
                var gd = Jtool.Instance.GraphicsDevice;
                var sb = Jtool.Instance.SpriteBatch;
                var rt = new RenderTarget2D(gd, 32, 32);

                gd.SetRenderTarget(rt);
                gd.Clear(Color.Transparent);
                sb.Begin();
                sb.Draw(tex, new Rectangle(0, 0, item.W, item.H), new Rectangle(item.X, item.Y, item.W, item.H), Color.White, 0, new Vector2(-16 + item.W / 2, -16 + item.H / 2), SpriteEffects.None, 0);
                sb.End();
                gd.SetRenderTarget(null);

                paletteIcons[type] = new SpriteItem(rt, 0, 0, 32, 32);
            }
        }

        public void Update()
        {
            // Shortcut keys
            if (Jtool.Instance.InputManager.IsKeyHold(Keys.LeftControl) && Jtool.Instance.InputManager.IsKeyPress(Keys.Z))
                Jtool.Instance.Editor.Undo();

            if (Jtool.Instance.InputManager.IsKeyHold(Keys.LeftControl) && Jtool.Instance.InputManager.IsKeyPress(Keys.Y))
                Jtool.Instance.Editor.Redo();

            void SaveMap()
            {
                if (Jtool.Instance.MapManager.CurrentMapFile != null)
                    Jtool.Instance.MapManager.SaveJMap(Jtool.Instance.MapManager.CurrentMapFile);
                else SaveMapAs();
            }

            void SaveMapAs()
            {
                var d = new SaveFileDialog();
                d.Filter = "jtool map file|*.jmap";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    Jtool.Instance.MapManager.SaveJMap(d.FileName);
                }
            }

            void OpenMap()
            {
                var d = new OpenFileDialog();
                d.Filter = "jtool map file|*.jmap";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    Jtool.Instance.MapManager.LoadJMap(d.FileName);
                }
            }

            // Define main menu
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Map", "F2"))
                    {
                        showMapWindow = true;
                    }
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Open Map", "CTRL+O"))
                    {
                        if (Jtool.Instance.MapManager.Modified)
                        {
                            showConfirmOpenMap = true;
                        }
                        else OpenMap();
                    }
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Save Map", "CTRL+S"))
                    {
                        SaveMap();
                    }
                    if (ImGui.MenuItem("Save As..."))
                    {
                        SaveMapAs();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit", "ALT+F4"))
                    {
                        Jtool.Instance.Exit();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit", showMapWindow))
                {
                    if (ImGui.MenuItem("Undo", "CTRL+Z"))
                    {
                        Jtool.Instance.Editor.Undo();
                    }
                    if (ImGui.MenuItem("Redo", "CTRL+Y"))
                    {
                        Jtool.Instance.Editor.Redo();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Snap", "G"))
                    {
                        showSnapWindow = true;
                    }
                    if (ImGui.MenuItem("Shift"))
                    {
                        showShiftWindow = true;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View", showMapWindow))
                {
                    if (ImGui.BeginMenu("Grid"))
                    {
                        if (ImGui.MenuItem("Show Grid", null, showGrid))
                        {
                            showGrid = !showGrid;
                            if (showGrid)
                            {

                            }
                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Set Size"))
                        {
                            showGridWindow = !showGridWindow;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Show Mouse Coord", null, showMouseCoord))
                    {
                        showMouseCoord = !showMouseCoord;
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Skin"))
                    {
                        if (ImGui.MenuItem("Next"))
                        {

                        }
                        if (ImGui.MenuItem("Previous"))
                        {

                        }
                        if (ImGui.MenuItem("Random"))
                        {

                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Choose", "F8"))
                        {
                            showSkinWindow = true;
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("GUI Theme"))
                    {
                        if (ImGui.MenuItem("Default"))
                        {

                        }
                        if (ImGui.MenuItem("Dark"))
                        {

                        }
                        if (ImGui.MenuItem("Light"))
                        {

                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Player", showMapWindow))
                {
                    if (ImGui.MenuItem("Dot Kid", null, Jtool.Instance.PlayerManager.Dotkid))
                    {
                        Jtool.Instance.PlayerManager.Dotkid = !Jtool.Instance.PlayerManager.Dotkid;
                    }
                    if (ImGui.MenuItem("Outline", null, Jtool.Instance.PlayerManager.DotkidOutline))
                    {
                        Jtool.Instance.PlayerManager.DotkidOutline = !Jtool.Instance.PlayerManager.DotkidOutline;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Enable Death", null, Jtool.Instance.PlayerManager.DeathEnable))
                    {
                        Jtool.Instance.PlayerManager.DeathEnable = !Jtool.Instance.PlayerManager.DeathEnable;
                    }
                    if (ImGui.MenuItem("Inf Jump", null, Jtool.Instance.PlayerManager.Infjump))
                    {
                        Jtool.Instance.PlayerManager.Infjump = !Jtool.Instance.PlayerManager.Infjump;
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Save Type"))
                    {
                        if (ImGui.MenuItem("Only Shoot", null, Jtool.Instance.PlayerManager.SaveType == SaveType.OnlyShoot))
                        {
                            Jtool.Instance.PlayerManager.SaveType = SaveType.OnlyShoot;
                        }
                        if (ImGui.MenuItem("Shoot Or Bullet", null, Jtool.Instance.PlayerManager.SaveType == SaveType.ShootOrBullet))
                        {
                            Jtool.Instance.PlayerManager.SaveType = SaveType.ShootOrBullet;
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Map Border Type"))
                    {
                        if (ImGui.MenuItem("Killer", null, Jtool.Instance.PlayerManager.DeathBorder == DeathBorder.Killer))
                        {
                            Jtool.Instance.PlayerManager.DeathBorder = DeathBorder.Killer;
                        }
                        if (ImGui.MenuItem("Solid", null, Jtool.Instance.PlayerManager.DeathBorder == DeathBorder.Solid))
                        {
                            Jtool.Instance.PlayerManager.DeathBorder = DeathBorder.Solid;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Mask (Hitbox)"))
                    {
                        if (ImGui.MenuItem("Only Player", null, Jtool.Instance.PlayerManager.ShowMask == ShowMask.OnlyPlayer))
                        {
                            Jtool.Instance.PlayerManager.ShowMask = ShowMask.OnlyPlayer;
                        }
                        if (ImGui.MenuItem("Only Mask", null, Jtool.Instance.PlayerManager.ShowMask == ShowMask.OnlyMask))
                        {
                            Jtool.Instance.PlayerManager.ShowMask = ShowMask.OnlyMask;
                        }
                        if (ImGui.MenuItem("Player And Mask", null, Jtool.Instance.PlayerManager.ShowMask == ShowMask.PlayerAndMask))
                        {
                            Jtool.Instance.PlayerManager.ShowMask = ShowMask.PlayerAndMask;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.MenuItem("Map Window", null, showMapWindow))
                    {
                        showMapWindow = !showMapWindow;
                    }
                    if (ImGui.MenuItem("Palette", null, showPaletteWindow))
                    {
                        showPaletteWindow = !showPaletteWindow;
                    }
                    if (ImGui.MenuItem("Log Window", null, showLogWindow))
                    {
                        showLogWindow = !showLogWindow;
                    }
                    if (ImGui.MenuItem("Shift", null, showShiftWindow))
                    {
                        showShiftWindow = !showShiftWindow;
                    }
                    if (ImGui.MenuItem("Player Analysis", null, showAnalysisWindow))
                    {
                        showAnalysisWindow = !showAnalysisWindow;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem("Github"))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "https://github.com/cubeww/imjtool",
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                    if (ImGui.MenuItem("Delicious Fruit"))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "https://delicious-fruit.com/",
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("About"))
                    {
                        showAboutWindow = true;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }

            if (showMapWindow)
            {
                ImGui.SetNextWindowPos(new Vector2(510, 100), ImGuiCond.Once);
                ImGui.SetNextWindowSize(new Vector2(800, 608) * MapWindowScale, ImGuiCond.Once);

                var flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize;
                if (!Jtool.Instance.Editor.MouseInTitle)
                    flags |= ImGuiWindowFlags.NoMove;


                if (ImGui.Begin("Map Window", ref showMapWindow, flags))
                {
                    Jtool.Instance.Editor.Update();

                    // Draw map window
                    var startPos = ImGui.GetWindowPos() + new Vector2(0, TitleBarHeight);

                    if (ImGui.IsWindowFocused())
                    {
                        ImGui.GetForegroundDrawList().AddImage(MapTexture, startPos, startPos + new Vector2(800, 608) * MapWindowScale);
                    }
                    else
                    {
                        ImGui.GetBackgroundDrawList().AddImage(MapTexture, startPos, startPos + new Vector2(800, 608) * MapWindowScale);
                    }
                }
                ImGui.End();
            }

            if (showPaletteWindow)
            {
                // Define palette
                ImGui.SetNextWindowPos(new Vector2(2, 100), ImGuiCond.Once);
                ImGui.SetNextWindowSize(new Vector2(500, 300), ImGuiCond.Once);
                if (ImGui.Begin("Palette", ref showPaletteWindow))
                {
                    void AddObject(Type type)
                    {
                        if (ImGui.ImageButton(paletteIcons[type].ImGuiTexture, new Vector2(32, 32)))
                        {
                            Jtool.Instance.Editor.SetSelectType(type);
                        }
                    }

                    if (ImGui.CollapsingHeader("Player"))
                    {
                        AddObject(typeof(PlayerStart));
                        ImGui.SameLine();
                        AddObject(typeof(Save));
                        ImGui.SameLine();
                        AddObject(typeof(Warp));
                    }
                    if (ImGui.CollapsingHeader("Killer"))
                    {
                        AddObject(typeof(SpikeUp));
                        ImGui.SameLine();
                        AddObject(typeof(SpikeDown));
                        ImGui.SameLine();
                        AddObject(typeof(SpikeLeft));
                        ImGui.SameLine();
                        AddObject(typeof(SpikeRight));

                        AddObject(typeof(MiniSpikeUp));
                        ImGui.SameLine();
                        AddObject(typeof(MiniSpikeDown));
                        ImGui.SameLine();
                        AddObject(typeof(MiniSpikeLeft));
                        ImGui.SameLine();
                        AddObject(typeof(MiniSpikeRight));

                        AddObject(typeof(Apple));
                        ImGui.SameLine();
                        AddObject(typeof(KillerBlock));
                    }
                    if (ImGui.CollapsingHeader("Block & Platform"))
                    {
                        AddObject(typeof(Block));
                        ImGui.SameLine();
                        AddObject(typeof(MiniBlock));
                        ImGui.SameLine();
                        AddObject(typeof(Platform));
                        ImGui.SameLine();
                        AddObject(typeof(BulletBlocker));
                    }
                    if (ImGui.CollapsingHeader("Vine & Water"))
                    {
                        AddObject(typeof(WalljumpR));
                        ImGui.SameLine();
                        AddObject(typeof(WalljumpL));

                        AddObject(typeof(Water));
                        ImGui.SameLine();
                        AddObject(typeof(Water2));
                        ImGui.SameLine();
                        AddObject(typeof(Water3));
                    }
                    if (ImGui.CollapsingHeader("Misc"))
                    {
                        AddObject(typeof(GravityArrowUp));
                        ImGui.SameLine();
                        AddObject(typeof(GravityArrowDown));
                        ImGui.SameLine();
                        AddObject(typeof(JumpRefresher));
                    }
                }
                ImGui.End();
            }

            if (showLogWindow)
            {
                ImGui.SetNextWindowPos(new Vector2(2, 402), ImGuiCond.Once);
                ImGui.SetNextWindowSize(new Vector2(500, 600), ImGuiCond.Once);

                if (ImGui.Begin("Log Window", ref showLogWindow))
                {
                    var needClear = false;
                    if (ImGui.Button("Clear"))
                    {
                        needClear = true;
                    }

                    ImGui.Separator();

                    if (ImGui.BeginChild("Output"))
                    {
                        var maxSize = 100;
                        for (int i = Math.Max(logText.Count - 1 - maxSize, 0); i < logText.Count; i++)
                        {
                            var (sender, text) = logText[i];
                            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 255, 128, 255));
                            ImGui.TextWrapped($"[{sender}] {text}");
                            ImGui.PopStyleColor();
                            ImGui.Separator();
                        }
                        if (scrollToBottom)
                        {
                            ImGui.SetScrollHereY(1.0f);
                            scrollToBottom = false;
                        }
                        ImGui.EndChild();
                    }

                    if (needClear)
                        logText.Clear();

                    ImGui.End();
                }
            }

            if (showSkinWindow)
            {
                ImGui.OpenPopup("Skin");
            }
            ImGui.SetNextWindowSize(new Vector2(430, 400), ImGuiCond.Once);
            if (ImGui.BeginPopupModal("Skin", ref showSkinWindow))
            {
                ImGui.Columns(2);
                ImGui.Text("Search");
                ImGui.SameLine();
                var names = Jtool.Instance.SkinManager.SkinNames;
                if (ImGui.InputText("##SearchSkin", ref skinSearchString, 256))
                {
                    skinSearchList.Clear();
                    int index = 0;
                    foreach (var name in names)
                    {
                        if (ContainsWord(name.ToLower(), skinSearchString.ToLower()))
                        {
                            skinSearchList.Add(index);
                        }
                        index++;
                    }
                }
                if (ImGui.BeginListBox("##Skins", new Vector2(200, 250)))
                {
                    if (Jtool.Instance.SkinManager.PreviewSkin == null)
                    {
                        skinSelect = 0;
                        Jtool.Instance.SkinManager.PreviewSkin = new();
                    }
                    var count = skinSearchString.Length == 0 ? names.Count : skinSearchList.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var idx = skinSearchList.Count == 0 ? i : skinSearchList[i];
                        var name = names[idx];
                        if (ImGui.Selectable(name, skinSelect == idx))
                        {
                            skinSelect = idx;
                            Jtool.Instance.SkinManager.PreviewSkin = new(name);
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.NextColumn();
                ImGui.Text("Preview:");

                var startPos = ImGui.GetCursorPos();
                void DrawPreview(Type type, int xx, int yy)
                {
                    ImGui.SetCursorPos(new Vector2(startPos.X + xx, startPos.Y + yy));

                    var spr = Jtool.Instance.SkinManager.GetPreviewSpriteOfType(type);
                    var item = spr.GetItem(0);
                    var uv = item.GetUV();
                    ImGui.Image(item.ImGuiTexture, new Vector2(item.W, item.H), uv.Item1, uv.Item2);

                };
                var d = 32;
                DrawPreview(typeof(Warp), 0, 0);
                DrawPreview(typeof(SpikeUp), d, 0);
                DrawPreview(typeof(JumpRefresher), d * 2, 0);
                DrawPreview(typeof(Block), d * 3, 0);
                DrawPreview(typeof(WalljumpR), d * 3, 0);
                DrawPreview(typeof(Block), d * 4, 0);
                DrawPreview(typeof(WalljumpL), d * 4, 0);

                DrawPreview(typeof(SpikeLeft), 0, d);
                DrawPreview(typeof(Block), d, d);
                DrawPreview(typeof(SpikeRight), d * 2, d);
                DrawPreview(typeof(Platform), d * 3, d);
                DrawPreview(typeof(Apple), d * 4, d);

                DrawPreview(typeof(PlayerStart), 0, d * 2);
                DrawPreview(typeof(SpikeDown), d, d * 2);
                DrawPreview(typeof(MiniSpikeUp), d * 2, d * 2);
                DrawPreview(typeof(MiniSpikeRight), d * 2 + 16, d * 2);
                DrawPreview(typeof(MiniSpikeLeft), d * 2, d * 2 + 16);
                DrawPreview(typeof(MiniSpikeDown), d * 2 + 16, d * 2 + 16);
                DrawPreview(typeof(Water2), d * 3, d * 2);
                DrawPreview(typeof(Save), d * 4, d * 2);

                ImGui.Columns();
                if (ImGui.Button("Apply"))
                {
                    Jtool.Instance.SkinManager.ApplySkin(Jtool.Instance.SkinManager.PreviewSkin.Name);
                    GeneratePaletteIcons();

                    showSkinWindow = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    showSkinWindow = false;
                }
                ImGui.EndPopup();
            }

            if (showConfirmOpenMap)
            {
                ImGui.OpenPopup("Confirm Open Map");
            }
            if (ImGui.BeginPopupModal("Confirm Open Map", ref showConfirmOpenMap))
            {
                ImGui.Text("Map has been changed. Save Changes?");
                if (ImGui.Button("Yes"))
                {
                    SaveMap();
                    showConfirmOpenMap = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("No"))
                {
                    OpenMap();
                    showConfirmOpenMap = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel")) 
                { 
                    showConfirmOpenMap = false;
                }
                
                ImGui.EndPopup();
            }
        }
        /// <summary>
        /// Displays a log in the log window. Maybe for debug, maybe just for cool...
        /// </summary>
        public static void Log(string sender, string text)
        {
            if (sender == "MapObjectManager" && text.Contains("Blood"))
                return;

            Jtool.Instance.Gui.logText.Add((sender, text));
            Jtool.Instance.Gui.scrollToBottom = true;
        }

        public static bool ContainsWord(string word, string otherword)
        {
            int currentIndex = 0;

            foreach (var character in otherword)
            {
                if ((currentIndex = word.IndexOf(character, currentIndex)) == -1)
                    return false;
            }

            return true;
        }
    }
}
