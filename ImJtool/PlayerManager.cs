namespace ImJtool
{
    public enum ShowMask
    {
        OnlyPlayer,
        OnlyMask,
        PlayerAndMask,
    };

    public enum SaveType
    {
        OnlyShoot,
        ShootOrBullet,
    };

    public enum DeathBorder
    {
        Killer,
        Solid,
    };

    public enum JumpState
    {
        FirstHold,
        FirstWait,
        SecondHold,
        SecondWait,
        Done,
    };
    public class PlayerManager
    {
        public static PlayerManager Instance => Jtool.Instance.PlayerManager;
        public PlayerSave CurrentSave { get; set; } = new();
        public Player Player { get; set; }

        public float Face { get; set; } = 1;
        public float Grav { get; set; } = 1;
        public bool Dotkid { get; set; } = false;
        public bool DotkidOutline { get; set; } = false;
        public bool DeathEnable { get; set; } = true;
        public bool Infjump { get; set; } = false;

        public ShowMask ShowMask { get; set; } = ShowMask.OnlyPlayer;
        public SaveType SaveType { get; set; } = SaveType.OnlyShoot;

        public DeathBorder deathBorder = DeathBorder.Killer;
        public DeathBorder DeathBorder 
        {
            get => deathBorder;
            set
            {
                deathBorder = value;
                MapObjectManager.Instance.DestroyByType(typeof(BorderBlock));
                if (value == DeathBorder.Solid)
                {
                    var obj = MapObjectManager.Instance.CreateObject(-32, 0, typeof(BorderBlock));
                    obj.YScale = 19;
                    obj = MapObjectManager.Instance.CreateObject(800, 0, typeof(BorderBlock));
                    obj.YScale = 19;
                    obj = MapObjectManager.Instance.CreateObject(0, -32, typeof(BorderBlock));
                    obj.XScale = 25;
                    obj = MapObjectManager.Instance.CreateObject(0, 608, typeof(BorderBlock));
                    obj.XScale = 25;
                }
            }
        }

        public void Update()
        {
            if (InputManager.Instance.IsKeyPress(Microsoft.Xna.Framework.Input.Keys.R))
            {
                // Press R
                PlayerManager.Instance.Load();
            }
        }

        public void Save()
        {
            if (Player != null)
            {
                CurrentSave.X = Player.X;
                CurrentSave.Y = Player.Y;
                CurrentSave.Face = Face;
                CurrentSave.Grav = Grav;
                Gui.Log("PlayerManager", $"Player saved: {{ X: {CurrentSave.X}, Y: {CurrentSave.Y} }}");
            }
        }
        public void Load()
        {
            MapObjectManager.Instance.DestroyByType(typeof(Player));
            MapObjectManager.Instance.DestroyByType(typeof(Blood));

            MapObjectManager.Instance.CreateObject(CurrentSave.X, CurrentSave.Y, typeof(Player));
            Grav = CurrentSave.Grav;
            Face = CurrentSave.Face;
            Gui.Log("PlayerManager", $"Player loaded: {{ X: {CurrentSave.X}, Y: {CurrentSave.Y} }}");
        }
    }

    //public class PlayerState
    //{
    //    public float x = -1;
    //    public float y = -1;
    //    public float face = 1;
    //    public float grav = 1;
    //    public float hspeed = 0;
    //    public float vspeed = 0;
    //    public float gravity = 0;
    //    public float imageSpeed = 0;
    //    public float imageIndex = 0;
    //    public bool djump = false;
    //}
    public class PlayerSave
    {
        public float X { get; set; } = -1;
        public float Y { get; set; } = -1;
        public float Face { get; set; } = 1;
        public float Grav { get; set; } = 1;
    }
}
