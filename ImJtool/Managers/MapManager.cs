using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ImJtool.Managers;

namespace ImJtool
{
    /// <summary>
    /// Manage map switching, saving, exporting, etc.
    /// </summary>
    public static class MapManager
    {
        public static string MapName { get; set; } = "Untitled.jmap";
        static bool modified = false;

        /// <summary>
        /// Whether the map has been modified.
        /// Modifying this property will automatically update the window title.
        /// </summary>
        public static bool Modified
        {
            get => modified;
            set
            {
                modified = value;
                UpdateWindowTitle();
            }
        }
        public static string CurrentMapFile { get; set; } = null;
        public static Dictionary<Type, int> TypeToJMap { set; get; } = new()
        {
            [typeof(Block)] = 1,
            [typeof(MiniBlock)] = 2,
            [typeof(SpikeUp)] = 3,
            [typeof(SpikeRight)] = 4,
            [typeof(SpikeLeft)] = 5,
            [typeof(SpikeDown)] = 6,
            [typeof(MiniSpikeUp)] = 7,
            [typeof(MiniSpikeRight)] = 8,
            [typeof(MiniSpikeLeft)] = 9,
            [typeof(MiniSpikeDown)] = 10,
            [typeof(Apple)] = 11,
            [typeof(Save)] = 12,
            [typeof(Platform)] = 13,
            [typeof(Water)] = 14,
            [typeof(Water2)] = 15,
            [typeof(WalljumpL)] = 16,
            [typeof(WalljumpR)] = 17,
            [typeof(KillerBlock)] = 18,
            [typeof(BulletBlocker)] = 19,
            [typeof(PlayerStart)] = 20,
            [typeof(Warp)] = 21,
            [typeof(JumpRefresher)] = 22,
            [typeof(Water3)] = 23,
            [typeof(GravityArrowUp)] = 24,
            [typeof(GravityArrowDown)] = 25,
        };
        public static Dictionary<int, Type> JMapToType { set; get; }
        public static Dictionary<Type, int> TypeToRMJ { set; get; } = new()
        {
            [typeof(Block)] = 2,
            [typeof(SpikeUp)] = 12,
            [typeof(SpikeRight)] = 11,
            [typeof(SpikeLeft)] = 10,
            [typeof(SpikeDown)] = 9,
            [typeof(MiniSpikeUp)] = 19,
            [typeof(MiniSpikeRight)] = 18,
            [typeof(MiniSpikeLeft)] = 17,
            [typeof(MiniSpikeDown)] = 16,
            [typeof(Apple)] = 20,
            [typeof(Save)] = 32,
            [typeof(Platform)] = 31,
            [typeof(Water)] = 23,
            [typeof(Water2)] = 30,
            [typeof(WalljumpL)] = 29,
            [typeof(WalljumpR)] = 28,
            [typeof(KillerBlock)] = 27,
            [typeof(PlayerStart)] = 3,
            [typeof(Warp)] = 21,
        };
        public static Dictionary<int, Type> RMJToJMap { set; get; }

        public static Dictionary<int, Type> MakeReverse(Dictionary<Type, int> dict)
        {
            var result = new Dictionary<int, Type>();
            foreach (var (k, v) in dict)
            {
                result[v] = k;
            }
            return result;
        }

        public static void Initialize()
        {
            JMapToType = MakeReverse(TypeToJMap);
            RMJToJMap = MakeReverse(TypeToRMJ);

            UpdateWindowTitle();
        }
        public static void UpdateWindowTitle()
        {
            Jtool.Instance.Window.Title = Jtool.Caption + " - " + MapName;

            if (modified)
                Jtool.Instance.Window.Title += '*';
        }
        /// <summary>
        /// Load jtool map file ("|" split, base32 store numbers)
        /// </summary>
        public static void LoadJMap(string filename)
        {
            double Base32ToDouble(string str)
            {
                const string base32string = "0123456789abcdefghijklmnopqrstuv";
                double result = 0;
                var length = str.Length;
                for (var i = 0; i < length; i++)
                {
                    var chr = str[i];
                    var charvalue = base32string.IndexOf(chr);
                    var placevalue = Math.Pow(32, length - 1 - i);
                    result += charvalue * placevalue;
                }
                return result;
            }
            var str = File.ReadAllText(filename);
            int objnum = 0;
            foreach (var chunk in str.Split('|'))
            {
                if (chunk.Contains(':'))
                {
                    var kv = chunk.Split(':');
                    var key = kv[0];
                    var value = kv[1];

                    switch (key)
                    {
                        case "inf":
                            PlayerManager.Infjump = int.Parse(value) switch
                            {
                                0 => false,
                                1 => true,
                                _ => false,
                            };
                            break;
                        case "dot":
                            PlayerManager.Dotkid = int.Parse(value) switch
                            {
                                0 => false,
                                1 => true,
                                _ => false,
                            };
                            break;
                        case "sav":
                            PlayerManager.SaveType = (SaveType)int.Parse(value);
                            break;
                        case "bor":
                            PlayerManager.DeathBorder = (DeathBorder)int.Parse(value);
                            break;
                        case "px":
                            PlayerManager.CurrentSave.X = (float)Base32ToDouble(value);
                            break;
                        case "py":
                            PlayerManager.CurrentSave.Y = (float)Base32ToDouble(value);
                            break;
                        case "ps":
                            PlayerManager.Face = int.Parse(value);
                            break;
                        case "pg":
                            PlayerManager.Grav = int.Parse(value);
                            break;
                        case "objects":
                            ClearMap();
                            var i = 0;
                            var yy = 0f;
                            while (i < value.Length)
                            {
                                if (value[i] == '-')
                                {
                                    yy = (float)Base32ToDouble(value.Substring(i + 1, 2));
                                }
                                else
                                {
                                    var type = JMapToType[(int)Base32ToDouble(value.Substring(i, 1))];
                                    var xx = (float)Base32ToDouble(value.Substring(i + 1, 2));
                                    MapObjectManager.CreateObject(xx - 128, yy - 128, type);
                                    objnum++;
                                }
                                i += 3;
                            }
                            break;
                    }
                }
            }
            PlayerManager.Load();
            Modified = false;
            CurrentMapFile = filename;
        }
        /// <summary>
        /// Save jtool map file ("|" split, base32 store numbers)
        /// </summary>
        public static void SaveJMap(string filename)
        {
            // Shit conversion...
            string IntToBase32(long number)
            {
                const string base32string = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@";
                var result = "";
                while (number > 0)
                {
                    double dec = number / 32.0f;
                    number = (long)dec;
                    var pos = (int)((dec - number) * 32);
                    result = base32string[pos] + result;
                }
                return result;
            };
            string DoubleToBase32(double number)
            {
                return IntToBase32(BitConverter.DoubleToInt64Bits(number));
            };
            string PadStringLeft(string str, int length, string padchar)
            {
                while (str.Length < length)
                {
                    str = padchar + str;
                }
                return str;
            };

            var mapObjectsList = new List<(int, int, int)>();
            foreach (var i in MapObjectManager.Objects)
            {
                if (i.IsInPalette)
                {
                    mapObjectsList.Add(((int)i.X, (int)i.Y, TypeToJMap[i.GetType()]));
                }
            }

            var sorted = mapObjectsList.OrderBy(i => i.Item2);

            int? yy = null;
            string obj = "";
            foreach (var i in sorted)
            {
                if (yy != i.Item2)
                {
                    yy = i.Item2;
                    obj += "-";
                    obj += PadStringLeft(IntToBase32(i.Item2 + 128), 2, "0");
                }
                obj += IntToBase32(i.Item3);
                obj += PadStringLeft(IntToBase32(i.Item1 + 128), 2, "0");
            }
            string str = string.Format("jtool|1.3.0|inf:{0}|dot:{1}|sav:{2}|bor:{3}|px:{4}|py:{5}|ps:{6}|pg:{7}|objects:{8}",
                PlayerManager.Infjump switch
                {
                    true => 1,
                    false => 0,
                },
                PlayerManager.Dotkid switch
                {
                    true => 1,
                    false => 0,
                },
                (int)PlayerManager.SaveType,
                (int)PlayerManager.DeathBorder,
                PadStringLeft(DoubleToBase32(PlayerManager.CurrentSave.X), 13, "0"),
                PadStringLeft(DoubleToBase32(PlayerManager.CurrentSave.Y), 13, "0"),
                (int)PlayerManager.Face,
                (int)PlayerManager.Grav,
                obj);
            File.WriteAllText(filename, str);
            Modified = false;
            CurrentMapFile = filename;
        }

        public static void ClearMap()
        {
            Editor.ClearUndo();
            foreach (var o in MapObjectManager.Objects)
            {
                if (o.IsInPalette)
                    o.Destroy();
            }
        }
        public static void SaveMap()
        {
            if (CurrentMapFile != null)
                SaveJMap(CurrentMapFile);
            else SaveMapAs();
        }

        public static void SaveMapAs()
        {
            var d = new SaveFileDialog();
            d.Filter = "jtool map file|*.jmap";
            if (d.ShowDialog() == DialogResult.OK)
            {
                SaveJMap(d.FileName);
            }
        }

        public static void NewMap()
        {
            ClearMap();

            // Create default objects
            MapObjectManager.CreateObject(352, 416, typeof(Block));
            MapObjectManager.CreateObject(352 + 32, 416, typeof(Block));
            MapObjectManager.CreateObject(352 + 64, 416, typeof(Block));
            MapObjectManager.CreateObject(384, 384, typeof(PlayerStart));
        }

        public static void OpenMap()
        {
            var d = new OpenFileDialog();
            d.Filter = "jtool map file|*.jmap";
            if (d.ShowDialog() == DialogResult.OK)
            {
                LoadJMap(d.FileName);
            }
        }
    }
}
