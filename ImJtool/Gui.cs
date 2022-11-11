using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using Vector4 = System.Numerics.Vector4;
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
    public static class Gui
    {
        // Show window fields
        static bool showMapWindow = true;
        static bool showSnapWindow = false;
        static bool showShiftWindow = false;
        static bool showSkinWindow = false;
        static bool showPaletteWindow = true;
        static bool showLogWindow = false;
        static bool showAnalysisWindow = false;
        static bool showAboutWindow = false;

        static bool showConfirmWindow = false;
        static string confirmText = "";
        static Action confirmAction = null;

        static string currentTheme;

        static int snap = 32;

        /// <summary>
        /// Due to some limitations of ImGui.Image, 
        /// it is now necessary to customize the texture for each Palette Object.
        /// </summary>
        static Dictionary<Type, SpriteItem> paletteIcons = new();

        public static bool ShowMouseCoord { get; set; } = false;
        static List<(string, string)> logText = new();
        static bool scrollToBottom = true;

        static string skinSearchString = "";
        static List<int> skinSearchList = new();
        static int skinSelect = 0;

        /// <summary>
        /// The scale of the map window.
        /// On high resolution screens (140ppi+), I recommend setting this larger.
        /// </summary>
        public static float MapWindowScale { get; set; } = 1.5f;
        public static float TitleBarHeight => ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;

        public static IntPtr MapTexture { get; set; }
        static uint MakeUIntColor(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
        /// <summary>
        /// Because the objects sprites of the palette need to have a fixed size, 
        /// we need to generate textures for them separately.
        /// </summary>
        public static void GeneratePaletteIcons()
        {
            foreach (var type in MapObject.PaletteObjects)
            {
                var item = SkinManager.GetCurrentSpriteOfType(type).GetItem(0);
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
        public static Color BgColor = Color.Black;
        public static void SetTheme(string name)
        {
            currentTheme = name;
            var colors = ImGui.GetStyle().Colors;

            switch (name)
            {
                case "Dark":
                    BgColor = new Color(30, 30, 30);
                    colors[(int)ImGuiCol.Text] = new Vector4(0.82f, 0.82f, 0.82f, 1.00f);
                    colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
                    colors[(int)ImGuiCol.WindowBg] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.ChildBg] = new Vector4(0.17f, 0.18f, 0.20f, 1.00f);
                    colors[(int)ImGuiCol.PopupBg] = new Vector4(0.22f, 0.24f, 0.25f, 1.00f);
                    colors[(int)ImGuiCol.Border] = new Vector4(0.16f, 0.17f, 0.18f, 1.00f);
                    colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.16f, 0.17f, 0.18f, 1.00f);
                    colors[(int)ImGuiCol.FrameBg] = new Vector4(0.14f, 0.15f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.84f, 0.34f, 0.17f, 1.00f);
                    colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.59f, 0.24f, 0.12f, 1.00f);
                    colors[(int)ImGuiCol.TitleBg] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.75f, 0.30f, 0.15f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
                    colors[(int)ImGuiCol.CheckMark] = new Vector4(0.90f, 0.90f, 0.90f, 0.50f);
                    colors[(int)ImGuiCol.SliderGrab] = new Vector4(1.00f, 1.00f, 1.00f, 0.30f);
                    colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
                    colors[(int)ImGuiCol.Button] = new Vector4(0.19f, 0.20f, 0.22f, 1.00f);
                    colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.84f, 0.34f, 0.17f, 1.00f);
                    colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.59f, 0.24f, 0.12f, 1.00f);
                    colors[(int)ImGuiCol.Header] = new Vector4(0.22f, 0.23f, 0.25f, 1.00f);
                    colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.84f, 0.34f, 0.17f, 1.00f);
                    colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.59f, 0.24f, 0.12f, 1.00f);
                    colors[(int)ImGuiCol.Separator] = new Vector4(0.17f, 0.18f, 0.20f, 1.00f);
                    colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.75f, 0.30f, 0.15f, 1.00f);
                    colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.59f, 0.24f, 0.12f, 1.00f);
                    colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.84f, 0.34f, 0.17f, 0.14f);
                    colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.84f, 0.34f, 0.17f, 1.00f);
                    colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.59f, 0.24f, 0.12f, 1.00f);
                    colors[(int)ImGuiCol.Tab] = new Vector4(0.16f, 0.16f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.TabHovered] = new Vector4(0.84f, 0.34f, 0.17f, 1.00f);
                    colors[(int)ImGuiCol.TabActive] = new Vector4(0.68f, 0.28f, 0.14f, 1.00f);
                    colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.13f, 0.14f, 0.16f, 1.00f);
                    colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.17f, 0.18f, 0.20f, 1.00f);
                    //colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.19f, 0.20f, 0.22f, 1.00f);
                    //colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.75f, 0.30f, 0.15f, 1.00f);
                    colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.75f, 0.30f, 0.15f, 1.00f);
                    colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.75f, 0.30f, 0.15f, 1.00f);
                    colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
                    colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
                    colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
                    break;
                case "Light":
                    BgColor = new Color(192, 192, 192);
                    colors[(int)ImGuiCol.Text] = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);
                    colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
                    colors[(int)ImGuiCol.WindowBg] = new Vector4(0.87f, 0.87f, 0.87f, 1.00f);
                    colors[(int)ImGuiCol.ChildBg] = new Vector4(0.87f, 0.87f, 0.87f, 1.00f);
                    colors[(int)ImGuiCol.PopupBg] = new Vector4(0.87f, 0.87f, 0.87f, 1.00f);
                    colors[(int)ImGuiCol.Border] = new Vector4(0.89f, 0.89f, 0.89f, 1.00f);
                    colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
                    colors[(int)ImGuiCol.FrameBg] = new Vector4(0.93f, 0.93f, 0.93f, 1.00f);
                    colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(1.00f, 0.69f, 0.07f, 0.69f);
                    colors[(int)ImGuiCol.FrameBgActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.TitleBg] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.TitleBgActive] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.MenuBarBg] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.87f, 0.87f, 0.87f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(1.00f, 0.69f, 0.07f, 0.69f);
                    colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.CheckMark] = new Vector4(0.01f, 0.01f, 0.01f, 0.63f);
                    colors[(int)ImGuiCol.SliderGrab] = new Vector4(1.00f, 0.69f, 0.07f, 0.69f);
                    colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.Button] = new Vector4(0.83f, 0.83f, 0.83f, 1.00f);
                    colors[(int)ImGuiCol.ButtonHovered] = new Vector4(1.00f, 0.69f, 0.07f, 0.69f);
                    colors[(int)ImGuiCol.ButtonActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.Header] = new Vector4(0.67f, 0.67f, 0.67f, 1.00f);
                    colors[(int)ImGuiCol.HeaderHovered] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.HeaderActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.Separator] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.SeparatorActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.00f, 1.00f, 1.00f, 0.18f);
                    colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    colors[(int)ImGuiCol.Tab] = new Vector4(0.16f, 0.16f, 0.16f, 0.00f);
                    colors[(int)ImGuiCol.TabHovered] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.TabActive] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.TabUnfocused] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.87f, 0.87f, 0.87f, 1.00f);
                    //colors[(int)ImGuiCol.DockingPreview] = new Vector4(1.00f, 0.82f, 0.46f, 0.69f);
                    //colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
                    colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.00f, 0.69f, 0.07f, 1.00f);
                    colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
                    colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.87f, 0.87f, 0.87f, 1.00f);
                    colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
                    break;
            }
        }

        static void SetConfirm(Action action, string text)
        {
            showConfirmWindow = true;
            confirmAction = action;
            confirmText = text;
        }
        public static void Update()
        {
            // Shortcut keys
            if (InputManager.IsKeyHold(Keys.LeftControl) && InputManager.IsKeyPress(Keys.Z))
                Editor.Undo();

            if (InputManager.IsKeyHold(Keys.LeftControl) && InputManager.IsKeyPress(Keys.Y))
                Editor.Redo();

            // Define main menu
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Map", "F2"))
                    {
                        showMapWindow = true;

                        if (MapManager.Modified)
                        {
                            SetConfirm(MapManager.NewMap, "Map has been changed. Save Changes?");
                        }
                        else MapManager.NewMap();
                    }
                    ImGui.Separator();

                    if (ImGui.MenuItem("Open Map", "CTRL+O"))
                    {
                        if (MapManager.Modified)
                        {
                            SetConfirm(MapManager.OpenMap, "Map has been changed. Save Changes?");
                        }
                        else MapManager.OpenMap();
                    }
                    ImGui.Separator();

                    if (ImGui.MenuItem("Save Map", "CTRL+S"))
                    {
                        MapManager.SaveMap();
                    }
                    if (ImGui.MenuItem("Save As..."))
                    {
                        MapManager.SaveMapAs();
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
                        Editor.Undo();
                    }
                    if (ImGui.MenuItem("Redo", "CTRL+Y"))
                    {
                        Editor.Redo();
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
                        if (ImGui.MenuItem("Show Grid", null, Editor.ShowGrid))
                        {
                            Editor.ShowGrid = !Editor.ShowGrid;
                        }
                        ImGui.Separator();
                        if (ImGui.BeginMenu("Grid Size"))
                        {
                            if (ImGui.MenuItem("32x32", null, Editor.GridSize == 32))
                            {
                                Editor.GridSize = 32;
                                Editor.RedrawGrid();
                            }
                            if (ImGui.MenuItem("16x16", null, Editor.GridSize == 16))
                            {
                                Editor.GridSize = 16;
                                Editor.RedrawGrid();
                            }
                            if (ImGui.MenuItem("8x8", null, Editor.GridSize == 8))
                            {
                                Editor.GridSize = 8;
                                Editor.RedrawGrid();
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Show Mouse Coord", null, ShowMouseCoord))
                    {
                        ShowMouseCoord = !ShowMouseCoord;
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
                        ImGui.Separator();
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
                        if (ImGui.MenuItem("Dark", null, currentTheme == "Dark"))
                        {
                            SetTheme("Dark");
                        }
                        if (ImGui.MenuItem("Light", null, currentTheme == "Light"))
                        {
                            SetTheme("Light");
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Player", showMapWindow))
                {
                    if (ImGui.MenuItem("Dot Kid", null, PlayerManager.Dotkid))
                    {
                        PlayerManager.Dotkid = !PlayerManager.Dotkid;
                    }
                    if (ImGui.MenuItem("Outline", null, PlayerManager.DotkidOutline))
                    {
                        PlayerManager.DotkidOutline = !PlayerManager.DotkidOutline;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Enable Death", null, PlayerManager.DeathEnable))
                    {
                        PlayerManager.DeathEnable = !PlayerManager.DeathEnable;
                    }
                    if (ImGui.MenuItem("Inf Jump", null, PlayerManager.Infjump))
                    {
                        PlayerManager.Infjump = !PlayerManager.Infjump;
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Save Type"))
                    {
                        if (ImGui.MenuItem("Only Shoot", null, PlayerManager.SaveType == SaveType.OnlyShoot))
                        {
                            PlayerManager.SaveType = SaveType.OnlyShoot;
                        }
                        if (ImGui.MenuItem("Shoot Or Bullet", null, PlayerManager.SaveType == SaveType.ShootOrBullet))
                        {
                            PlayerManager.SaveType = SaveType.ShootOrBullet;
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Map Border Type"))
                    {
                        if (ImGui.MenuItem("Killer", null, PlayerManager.DeathBorder == DeathBorder.Killer))
                        {
                            PlayerManager.DeathBorder = DeathBorder.Killer;
                        }
                        if (ImGui.MenuItem("Solid", null, PlayerManager.DeathBorder == DeathBorder.Solid))
                        {
                            PlayerManager.DeathBorder = DeathBorder.Solid;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Mask (Hitbox)"))
                    {
                        if (ImGui.MenuItem("Only Player", null, PlayerManager.ShowMask == ShowMask.OnlyPlayer))
                        {
                            PlayerManager.ShowMask = ShowMask.OnlyPlayer;
                        }
                        if (ImGui.MenuItem("Only Mask", null, PlayerManager.ShowMask == ShowMask.OnlyMask))
                        {
                            PlayerManager.ShowMask = ShowMask.OnlyMask;
                        }
                        if (ImGui.MenuItem("Player And Mask", null, PlayerManager.ShowMask == ShowMask.PlayerAndMask))
                        {
                            PlayerManager.ShowMask = ShowMask.PlayerAndMask;
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
                if (!Editor.MouseInTitle)
                    flags |= ImGuiWindowFlags.NoMove;


                if (ImGui.Begin("Map Window", ref showMapWindow, flags))
                {
                    Editor.Update();

                    // Draw map window
                    var windowPos = ImGui.GetWindowPos();
                    var startPos = windowPos + new Vector2(0, TitleBarHeight);

                    ImDrawListPtr drawList;

                    if (ImGui.IsWindowFocused())
                    {
                        drawList = ImGui.GetForegroundDrawList();
                    }
                    else
                    {
                        drawList = ImGui.GetBackgroundDrawList();
                    }
                    drawList.AddImage(MapTexture, startPos, startPos + new Vector2(800, 608) * MapWindowScale);

                    if (ShowMouseCoord)
                    {
                        var pos = (ImGui.GetMousePos() - startPos) / MapWindowScale;

                        if (pos.X >= 0 && pos.X <= 800 && pos.Y >= 0 && pos.Y <= 608)
                        {
                            
                            drawList.AddText(pos * MapWindowScale + windowPos, MakeUIntColor(255, 255, 255, 255), $"({MathF.Floor(pos.X)}, {MathF.Floor(pos.Y)})");
                        }
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
                            Editor.SetSelectType(type);
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
                var names = SkinManager.SkinNames;
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
                    if (SkinManager.PreviewSkin == null)
                    {
                        skinSelect = 0;
                        SkinManager.PreviewSkin = new();
                    }
                    var count = skinSearchString.Length == 0 ? names.Count : skinSearchList.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var idx = skinSearchList.Count == 0 ? i : skinSearchList[i];
                        var name = names[idx];
                        if (ImGui.Selectable(name, skinSelect == idx))
                        {
                            skinSelect = idx;
                            SkinManager.PreviewSkin = new(name);
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

                    var spr = SkinManager.GetPreviewSpriteOfType(type);
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
                    SkinManager.ApplySkin(SkinManager.PreviewSkin.Name);
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

            // Confirm window
            if (showConfirmWindow)
            {
                ImGui.OpenPopup("Confirm");
            }
            if (ImGui.BeginPopupModal("Confirm", ref showConfirmWindow))
            {
                ImGui.Text(confirmText);
                if (ImGui.Button("Yes"))
                {
                    confirmAction();
                    showConfirmWindow = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("No"))
                {
                    confirmAction();
                    showConfirmWindow = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    showConfirmWindow = false;
                }

                ImGui.EndPopup();
            }

            // snap window
            if (showSnapWindow)
            {
                ImGui.OpenPopup("Set Snap");
            }
            ImGui.SetNextWindowSize(new Vector2(300, 200), ImGuiCond.Once);
            if (ImGui.BeginPopupModal("Set Snap", ref showSnapWindow))
            {
                if (ImGui.Button("32"))
                {
                    snap = 32;
                }
                ImGui.SameLine();
                if (ImGui.Button("16"))
                {
                    snap = 16;
                }
                ImGui.SameLine();
                if (ImGui.Button("8"))
                {
                    snap = 8;
                }
                ImGui.SameLine();
                if (ImGui.Button("1"))
                {
                    snap = 1;
                }

                ImGui.InputInt("Snap Size (1~128)", ref snap);
                snap = Math.Clamp(snap, 1, 128);

                if (ImGui.Button("OK"))
                {
                    Editor.Snap = snap;
                    showSnapWindow = false;
                    ImGui.CloseCurrentPopup();
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

            logText.Add((sender, text));
            scrollToBottom = true;
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
