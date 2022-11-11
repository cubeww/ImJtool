using ImGuiNET;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace ImJtool
{
    /// <summary>
    /// ImJtool program class. Controls the main logic of the program.
    /// </summary>
    public class Jtool : Game
    {
        public static string Caption { get; } = "ImJtool";
        public string[] Args { get; set; }
        public GraphicsDeviceManager Graphics { get; set; }
        public ImGuiRenderer ImGuiRender { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public RenderTarget2D MapRenderTarget { get; set; }

        ImFontPtr defaultGuiFont;

        private static readonly Lazy<Jtool> lazy = new(() => new Jtool());

        /// <summary>
        /// Singleton of Jtool.
        /// You can get the instance through "Jtool.Instance" in any part of the program!
        /// </summary>
        public static Jtool Instance => lazy.Value;

        public ResourceManager ResourceManager { get; private set; }
        public MapObjectManager MapObjectManager { get; private set; }
        public Gui Gui { get; private set; }
        public Editor Editor { get; private set; }
        public InputManager InputManager { get; private set; }
        public PlayerManager PlayerManager { get; private set; }
        public SkinManager SkinManager { get; private set; }
        public MapManager MapManager { get; private set; }
        public ConfigManager ConfigManager { get; private set; }

        public Jtool()
        {
            Graphics = new GraphicsDeviceManager(this);

            // Resolution setting. Can be adjusted at any time, independent of the map window
            Graphics.PreferredBackBufferWidth = 1920;
            Graphics.PreferredBackBufferHeight = 1200;
            Graphics.PreferMultiSampling = true;

            // Game FPS (50)
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 50.0f);

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }
        /// <summary>
        /// Initialize and create managers
        /// </summary>
        protected override void Initialize()
        {
            Directory.SetCurrentDirectory(Application.StartupPath);

            ImGuiRender = new ImGuiRenderer(this);

            // Make imgui font bigger
            var io = ImGui.GetIO();
            defaultGuiFont = io.Fonts.AddFontFromFileTTF("arial.ttf", 30.0f);
            ImGuiRender.RebuildFontAtlas();
            ImGui.GetStyle().ScaleAllSizes(1.0f);

            MapRenderTarget = new RenderTarget2D(GraphicsDevice, 800, 608);

            ResourceManager = new ResourceManager();
            MapObjectManager = new MapObjectManager();
            InputManager = new InputManager();
            PlayerManager = new PlayerManager();
            Gui = new Gui();
            Editor = new Editor();
            SkinManager = new SkinManager();
            MapManager = new MapManager();
            ConfigManager = new ConfigManager();

            base.Initialize();
        }
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Load game resources from files
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Gui.MapTexture = ImGuiRender.BindTexture(MapRenderTarget);

            ResourceManager.LoadTextures();
            SkinManager.LoadConfig();

            ConfigManager.Load();

            Gui.GeneratePaletteIcons();
            Gui.SetTheme("Dark");
            Editor.GridTexture = new RenderTarget2D(GraphicsDevice, 800, 608);
            Editor.RedrawGrid();

            MapObjectManager.Instance.CreateObject(0, 0, typeof(Bg));
            MapObjectManager.Instance.CreateObject(0, 0, typeof(Grid));
            MapManager.NewMap();

            // Set as default program for jmap
            if (IsAdministrator())
            {
                string ext = ".jmap";
                RegistryKey key = Registry.ClassesRoot.CreateSubKey(ext);
                key.SetValue("", "ImJtool");
                key.Close();

                key = Registry.ClassesRoot.CreateSubKey(ext + "\\Shell\\Open\\command");

                key.SetValue("", "\"" + Application.ExecutablePath + "\" \"%L\"");
                key.Close();

                key = Registry.ClassesRoot.CreateSubKey(ext + "\\DefaultIcon");
                key.SetValue("", Application.StartupPath + "\\jmap.ico");
                key.Close();
            }
            // Load exe argument map
            if (Args.Length != 0)
                MapManager.LoadJMap(Args[0]);

            base.LoadContent();
        }
        /// <summary>
        /// Called once per game frame. 
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();
            PlayerManager.Update();

            MapObjectManager.DoStep();

            GraphicsDevice.SetRenderTarget(MapRenderTarget);
            GraphicsDevice.Clear(Color.White);

            SpriteBatch.Begin();

            MapObjectManager.DoDraw();

            if (Editor.NeedDrawPreview)
            {
                Editor.PreviewSprite.Draw(0, Editor.PreviewPosition.X, Editor.PreviewPosition.Y, 1, 1, 0, Color.White * 0.5f);
            }
            
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            ImGuiRender.BeforeLayout(gameTime);

            ImGui.PushFont(defaultGuiFont);
            Gui.Update();
            ImGui.PopFont();

            GraphicsDevice.Clear(Gui.BgColor);
            ImGuiRender.AfterLayout();

            InputManager.AfterUpdate();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            ConfigManager.Save();
            base.OnExiting(sender, args);
        }
    }
}