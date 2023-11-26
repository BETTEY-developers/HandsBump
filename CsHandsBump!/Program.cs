using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.IO.Pipes;
using HandsBump.Options;

namespace HandsBump;

internal partial class Program
{
    static Game? CurrentGame;

    static ConsoleSize StartupConsoleSize = new ConsoleSize();

    static Thread? ResizeThread;

    static List<Preset> Presets { get { return _presets; } set { ischanged = true; _presets = value; } }
    static List<Preset> _presets = new List<Preset>();

    static bool ischanged = false;
    static bool IsPipeOpened = false;

    static bool canresize = false;

    static Message AppMessageBar = new Message();



    public static void WriteLogo()
    {
        Console.WriteLine("Hands Bump!  (Copyright) Amlight-Elipese 2023");
        Console.WriteLine();
    }

    static void ModeMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteLogo();
            int select = Menu.WriteMenu(
                new Dictionary<string, string>
                {
                { "经典", "传统的碰手指玩法，支持二人、三人或四人。\n默认每人两个hand，初始1点，每hand5点收hand" },
                { "自定义", "支持自定义初始点数、结束点数、人数及hand数" },
                { "返回上一级菜单", "返回上个菜单" }
                });

            if (select == 0)
            {
                ClassicStartMenu();
            }
            else if (select == 1)
            {
                CustomStartMenu();
            }
            else
            {
                return;
            }
        }
    }

    private static List<FileInfo> GetRawPresetFiles()
    {
        List<FileInfo> presetfiles = new();
        try
        {
            if (!Directory.Exists("Presets"))
                Directory.CreateDirectory("Presets");

            bool fed = false;
            Directory.EnumerateFiles("Presets")
                .ToList()
                .ForEach(
                (p) =>
                {
                    presetfiles.Add(new FileInfo(p));
                }
            );
        }
        catch
        {
            return null;
        }

        return presetfiles;
    }

    private static List<Preset> GetPresetsFromFiles(List<FileInfo> presetfiles)
    {
        List<Type> allowtypes = new();
        List<Preset> result = new();
        foreach (var preset in presetfiles)
        {
            var pb = preset.OpenRead();
            byte[] data = new byte[preset.Length];
            pb.Read(data);
            List<byte> j = new();
            try
            {
                Encoding.Default.GetString(data).Split("#").ToList().ForEach((s) => j.Add(byte.Parse(s)));
            }
            catch (FormatException)
            {
                if (!allowtypes.Contains(typeof(FormatException)))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("发生了一些错误。");
                    Console.WriteLine(preset.Name + "文件格式不正确。");
                    if (Menu.WriteMenu(new() { { "继续", "继续并忽略此条消息" }, { "继续，但忽略以后此类消息", "继续，但忽略以后此类消息" } }) == 1)
                        allowtypes.Add(typeof(FormatException));
                }
            }
            catch (OverflowException)
            {
                if (!allowtypes.Contains(typeof(OverflowException)))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("发生了一些错误。");
                    Console.WriteLine(preset.Name + "段落数值太大或太小。");
                    if (Menu.WriteMenu(new() { { "继续", "继续并忽略此条消息" }, { "继续，但忽略以后此类消息", "继续，但忽略以后此类消息" } }) == 1)
                        allowtypes.Add(typeof(OverflowException));
                }
            }

            string s = Encoding.Default.GetString(j.ToArray());

            try
            {
                var item = JsonSerializer.Deserialize<Preset>(s);
                if (!result.Contains(item))
                    result.Add(item);
            }
            catch (JsonException)
            {
                if (!allowtypes.Contains(typeof(JsonException)))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("发生了一些错误。");
                    Console.WriteLine(preset.Name + "无法反序列化为对象。");
                    if (Menu.WriteMenu(new() { { "继续", "继续并忽略此条消息" }, { "继续，但忽略以后此类消息", "继续，但忽略以后此类消息" } }) == 1)
                        allowtypes.Add(typeof(JsonException));
                }
            }
        }
        return result;
    }

    private static void CustomStartMenu()
    {
        List<Preset> normalNewPresets = new();
        List<(Preset item, int index)> changeOriginNewPresets = new(); 
Startup:
        Console.Clear();
        WriteLogo();

        List<FileInfo> presetfiles = GetRawPresetFiles();
        List<Preset> presets = GetPresetsFromFiles(presetfiles);
        
        List<StringBuilder> descriptionlist = new();

        Presets = presets.Concat(normalNewPresets).ToList();

        changeOriginNewPresets.ForEach(item =>
        {
            Presets[item.index] = item.item;
        });

        Dictionary<string, string> items = new()
        {
            { "返回上一级菜单", Presets.Count != 0? "返回上一个菜单" : "未找到任何预设" },
            { "创建新预设", "创建一个新预设" },
            { "刷新预设", "刷新已经找到的预设" }
        };

        int select = 0;
        
        foreach(var preset in Presets)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{preset.PresetName} 预设 {$"玩家数量 {preset.PlayerCount}",6}");
            sb.AppendLine();
            sb.AppendLine($"{"TargetPlayer",-15}|{"Target",-10}|{"Hand Count",-10}|{"Startup",-10}|");
            foreach(var player in preset.PlayerOption??new())
            {
                sb.AppendLine($"{$"Player{player.TargetPlayerId}",-15}|{player.Target,-10}|{player.HandCount,-10}|{player.StartupNumber,-10}");
            }
            try
            {
                items.Add(preset.PresetName, sb.ToString());
            }
            catch { }
            descriptionlist.Add(sb);
        }

        CreatePipe(normalNewPresets.Add);
        
        select = Menu.WriteLargerMenu(items, Math.Min(8, items.Count));

        switch (select)
        {
            case 0:
                return;
            case 1:
                normalNewPresets.Add(GamePresetCreater(new Preset()));
                goto Startup;
            case 2:
                goto Startup;
            default:
                {
                    (Preset preset, InfoPageStatus status) = PresetInfoPage(Presets[select - 3]);
                    if(status == InfoPageStatus.NoChanges)
                        goto Startup;
                    else if(status == InfoPageStatus.CloneToNew)
                    {
                        preset.PresetName += " Clone";
                        normalNewPresets.Add(preset);
                        goto Startup;
                    }
                    else if (status == InfoPageStatus.ChangeToOrigin)
                    {
                        changeOriginNewPresets.Add((preset, select - 3));
                        goto Startup;
                    }
                    else if (status == InfoPageStatus.SelectToPlay)
                    {
                        StartNewGame(preset);
                        break;
                    }
                    break;
                }
        }
    }

    private static (Preset? preset, InfoPageStatus flag) PresetInfoPage(Preset preset)
    {
        while (true)
        {
            Console.Clear();
            WriteLogo();
            int select = Menu.WriteMenu(new()
            {
                { "查看预设信息", "查看该预设的所有信息" },
                { "使用该预设", "通过此预设开始游戏" },
                { "修改该预设", "修改这个预设的所有选项" },
                { "退出", "不对此预设做任何操作并退回到上一级菜单" }
            });
            switch (select)
            {
                case 0:
                    {
                        StringBuilder sb = new();
                        sb.AppendLine($"{preset.PresetName} 预设 {$"玩家数量 {preset.PlayerCount}",6}");
                        sb.AppendLine();
                        sb.AppendLine($"{"TargetPlayer",-15}|{"Target",-10}|{"Hand Count",-10}|{"Startup",-10}|");
                        foreach (var player in preset.PlayerOption)
                        {
                            sb.AppendLine($"{$"Player{player.TargetPlayerId}",-15}|{player.Target,-10}|{player.HandCount,-10}|{player.StartupNumber,-10}");
                        }
                        Menu.LargerContentBoard(sb.ToString().Split(Environment.NewLine));
                        break;
                    }

                case 1:
                    return (preset, InfoPageStatus.SelectToPlay);
                case 3:
                    return (preset, InfoPageStatus.NoChanges);
                default:
                    {
                        var pre = GamePresetCreater(preset);

                        int saveoptselect = Menu.WriteMenu(new()
                        {
                            { "添加", "当作副本添加一个预设(推荐)" },
                            { "直接修改", "直接修改这个预设\n注意：将会覆盖这个预设的所有内容" },
                            { "不保存", "你确定你不保存？" }
                        });

                        return (pre, (InfoPageStatus)(saveoptselect + 1));
                    }
            }
        }
    }

    private static async void CreatePipe(Action<Preset> callback)
    {
        if(IsPipeOpened) { return; }
        NamedPipeServerStream namedPipe = new NamedPipeServerStream("HandsBump");
        IsPipeOpened = true;
        while(true)
        {
            await namedPipe.WaitForConnectionAsync();
            string content = new StreamReader(namedPipe).ReadToEnd();

            Preset preset = JsonSerializer.Deserialize<Preset>(content);
            callback(preset);
        }
    }

    private static Preset GamePresetCreater(Preset preset = default)
    {
        var pre = Setting((prop, type) =>
        {
            var list = (List<Preset.PlayerPreset>)Convert.ChangeType(prop, type) ?? new List<Preset.PlayerPreset>();
            while (true)
            {
                Console.Clear();
                WriteLogo();
                Dictionary<string, string> items = new();
                int index = 0;
                foreach (var item in list)
                {
                    items.Add($"第{index}位玩家设置", $"第{index}位玩家设置");
                    index++;
                }
                items.Add("添加", "添加一个玩家设置");
                items.Add("退出", "退出");
                int select = Menu.WriteLargerMenu(items);
                if (select == items.Count - 1)
                {
                    return list;
                }
                else if (select == items.Count - 1 - 1)
                {
                    list.Add(new Preset.PlayerPreset());
                    list[select] = Setting((prop1, _) => prop1, Default: list[select]);
                }
                else
                {
                    list[select] = Setting((prop1, _) => prop1, Default: list[select]);
                }
            }
        }, OtherTypeToString: (obj, type) =>
        {
            List<Preset.PlayerPreset> player = obj as List<Preset.PlayerPreset> ?? new();
            StringBuilder sb = new();
            for (int i = 0; i < Math.Min(5, player.Count); i++)
            {
                sb.Append($"{player[i].Target,-3}");
            }
            return sb.ToString();
        }, Default: preset??new());
        return pre;
    }

    static void ClassicStartMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteLogo();
            int select = Menu.WriteMenu(
                new Dictionary<string, string>
                {
                    { "双人模式", "两个玩家" },
                    { "三人模式", "三个玩家" },
                    { "四人模式", "四个玩家" },
                    { "返回上一级菜单", "返回上个菜单" }
                }
            );
            if (select == 3)
            {
                return;
            }
            else
            {
                StartNewGame(GameArgumentCreater((ClassicPlayMode)select));
            }
        }
    }

#if DEBUG
    
    static void Test()
    {
        Console.WriteLine(Process.GetCurrentProcess().ProcessName);
        Console.ReadLine();
    }
#endif

    static void MainMenu()
    {
        while(true)
        {
            Console.Title = "Hands Bump!  (Copyright) Amlight-Elipese 2023";
            Console.Clear();
            WriteLogo();
            int select = Menu.WriteMenu(
                new Dictionary<string, string>
                {
                { "开始游戏", "开始HandBump!"},
                { "查看游戏记录", "查看您的游戏记录"},
                { "设置", "设置游戏选项"},
                { "退出", "退出HandsBump!"},
#if DEBUG
                    { "test","test" }
#endif
                });

            switch (select)
            {
                case 0:
                    ModeMenu();
                    break;
                case 2:
                    SettingMenu();
                    break;
#if DEBUG
                case 4:
                    Test();
                    break;
#endif
                default:
                    return;
            }
        }
        
    }

    private static void SettingMenu()
    {
        while (true)
        {
            Console.Clear();
            int select = Menu.WriteMenu(
                new Dictionary<string, string>
                {
                    {"调整控制台大小", "当选择内容不正确时才可使用该选项" },
                    {"返回上一级菜单", "GTR0（"}
                }
            );
            if (select == 0)
            {
                Console.Clear();
                WriteLogo();
                canresize = false;
                Console.WriteLine("现在可以安全的调整大小了");
                Console.WriteLine("按E键结束调整");
                Console.WriteLine();
                while (Console.ReadKey(true).Key == ConsoleKey.E)
                {
                    InitConsoleSizeStructure(true);
                    canresize = true;
                    return;
                }
            }
            else if(select == 1)
            {
                return;
            }
        }
    }

    static Preset GameArgumentCreater(ClassicPlayMode classicPlayMode)
    {
        switch (classicPlayMode)
        {
            case ClassicPlayMode.Twice:
                return new()
                {
                    PlayerCount = 2,
                    PresetName = "Two Players",
                };
            case ClassicPlayMode.ThreeTime:
                return new()
                {
                    PlayerCount = 3,
                    PresetName = "Three Players"
                };
            case ClassicPlayMode.FourTime:
                return new()
                {
                    PlayerCount = 4,
                    PresetName = "Four Players"
                };
        }
        return new();
    }

    static void Main(string[] args)
    {
        Console.WriteLine("加载中...");
        if (LoadGame())
            return;

        MainMenu();
    }
}