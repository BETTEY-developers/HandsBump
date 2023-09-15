using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using System.IO.Pipes;

namespace HandsBump;

internal enum ClassicPlayMode
{
    Twice,
    ThreeTime,
    FourTime,
    Custom
}

internal struct ConsoleSize
{
    public int Width;
    public int Height;
}

internal partial class Program
{
    static Game? CurrentGame;

    static ConsoleSize StartupConsoleSize = new ConsoleSize();

    static Thread? ResizeThread;

    static List<Preset> Presets = new List<Preset>();

    static bool IsPipeOpened = false;

    static bool canresize = false;

    

    static void WriteLogo()
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
            int select = Window.Menu.WriteMenu(
                new Dictionary<string, string>
                {
                { "经典", "传统的碰手指玩法，支持二人、三人或四人。\n默认每人两个hand，初始1点，5点赢" },
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

    private static void CustomStartMenu()
    {
        Console.Clear();
        WriteLogo();

        List<Type> allowtypes = new();

        Dictionary<string, string> items = new();
        List<StringBuilder> descriptionlist = new();

        List<FileInfo> presetfiles = new();

        while (true) 
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
                        fed = true;
                    }
                );
                if(fed)
                    items.Add("返回上一级菜单", "返回上一个菜单");
                else
                    items.Add("返回上一级菜单", "未找到任何预设文件");

                items.Add("直接设置","从头设置游戏选项");
                break;
            }
            catch 
            {
                items.Add("返回上一级菜单","未找到任何预设文件");
            }
        List<Preset> presets = new();

        foreach( var preset in presetfiles )
        {
            var pb = preset.OpenRead();
            byte[] data = new byte[preset.Length];
            pb.Read(data);
            List<byte> j = new();
            try
            {
                Encoding.Default.GetString(data).Split("#").ToList().ForEach((s) => j.Add(byte.Parse(s)));
            }
            catch(FormatException)
            {
                if(!allowtypes.Contains(typeof(FormatException)))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("发生了一些错误。");
                    Console.WriteLine(preset.Name + "文件格式不正确。");
                    if(Window.Menu.WriteMenu(new() { { "继续", "继续并忽略此条消息" }, { "继续，但忽略以后此类消息", "继续，但忽略以后此类消息" } }) == 1)
                        allowtypes.Add(typeof(FormatException));
                }
            }
            catch(OverflowException)
            {
                if (!allowtypes.Contains(typeof(OverflowException)))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("发生了一些错误。");
                    Console.WriteLine(preset.Name + "段落数值太大或太小。");
                    if (Window.Menu.WriteMenu(new() { { "继续", "继续并忽略此条消息" }, { "继续，但忽略以后此类消息", "继续，但忽略以后此类消息" } }) == 1)
                        allowtypes.Add(typeof(OverflowException));
                }
            }


            string s = Encoding.Default.GetString(j.ToArray());

            try
            {
                presets.Add(JsonSerializer.Deserialize<Preset>(s));
            }
            catch(JsonException)
            {
                if (!allowtypes.Contains(typeof(JsonException)))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("发生了一些错误。");
                    Console.WriteLine(preset.Name + "无法反序列化为对象。");
                    if (Window.Menu.WriteMenu(new() { { "继续", "继续并忽略此条消息" }, { "继续，但忽略以后此类消息", "继续，但忽略以后此类消息" } }) == 1)
                        allowtypes.Add(typeof(JsonException));
                }
            }
        }

        int select = 0;
        
        foreach(var preset in presets)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{preset.PresetName} 预设 {$"玩家数量 {preset.PlayerCount}",6}");
            sb.AppendLine();
            sb.AppendLine($"{"TargetPlayer",-15}|{"Target",-10}|{"Hand Count",-10}|{"Startup",-10}|");
            foreach(var player in preset.PlayerOption)
            {
                sb.AppendLine($"{$"Player{player.TargetPlayerId}",-15}|{player.Target,-10}|{player.HandCount,-10}|{player.StartupNumber,-10}");
            }
            items.Add(preset.PresetName, sb.ToString());
            descriptionlist.Add(sb);
        }

        CreatePipe();
        
        select = Window.Menu.WriteLargerMenu(items, Math.Min(8, items.Count));

        if(select == 0)
        {
            return;
        }
        else if(select == 1)
        {
            GameOptionCreater(new Preset());
        }
        else
        {
            PresetInfoPage(presets[select - 2]);
        }
    }

    private static (Preset preset, int flag) PresetInfoPage(Preset preset)
    {
        while (true)
        {
            Console.Clear();
            WriteLogo();
            int select = Window.Menu.WriteMenu(new()
            {
                { "查看预设信息", "查看该预设的所有信息" },
                { "使用该预设", "通过此预设开始游戏" },
                { "修改该预设", "修改这个预设的所有选项" }
            });
            if (select == 0)
            {
                StringBuilder sb = new();
                sb.AppendLine($"{preset.PresetName} 预设 {$"玩家数量 {preset.PlayerCount}",6}");
                sb.AppendLine();
                sb.AppendLine($"{"TargetPlayer",-15}|{"Target",-10}|{"Hand Count",-10}|{"Startup",-10}|");
                foreach (var player in preset.PlayerOption)
                {
                    sb.AppendLine($"{$"Player{player.TargetPlayerId}",-15}|{player.Target,-10}|{player.HandCount,-10}|{player.StartupNumber,-10}");
                }
                Window.Menu.LargerContentBoard(sb.ToString().Split(Environment.NewLine));
            }
            else if(select == 1)
            {
                // TODO: Startup Game
            }
            else
            {
                var pre = GameOptionCreater(preset);

                int saveoptselect = Window.Menu.WriteMenu(new()
                {
                    { "直接修改", "直接修改这个预设\n注意：将会覆盖这个预设的所有内容" },
                    { "添加", "当作副本添加一个预设" }
                });

                if( saveoptselect == 0)
                {
                    return (pre, 0);
                }
                else
                {
                    return (pre, 1);
                }
            }
        }
    }

    private static async void CreatePipe()
    {
        if(IsPipeOpened) { return; }

        await Task.Run(() =>
        {
            NamedPipeServerStream namedPipe = new NamedPipeServerStream("HandsBump");
            IsPipeOpened = true;
            namedPipe.WaitForConnection();

            List<byte> bytes = new();
            int dbyte = -1;
            while (!((dbyte = namedPipe.ReadByte()) == 0)) bytes.Add((byte)dbyte);

            namedPipe.Close();

            IsPipeOpened = false;
            Preset preset = JsonSerializer.Deserialize<Preset>(Encoding.UTF8.GetString(bytes.ToArray()));
            
        });
    }

    private static Preset GameOptionCreater(Preset preset = default)
    {
        var pre = Setting<Preset>((prop, type) =>
        {
            var list = (List<Preset.Player>)Convert.ChangeType(prop, type) ?? new List<Preset.Player>();
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
                int select = Window.Menu.WriteLargerMenu(items);
                if (select == items.Count - 1)
                {
                    return list;
                }
                else if (select == items.Count - 1 - 1)
                {
                    list.Add(new Preset.Player());
                    list[select] = Setting((prop1, _) => prop1, Default: list[select]);
                }
                else
                {
                    list[select] = Setting((prop1, _) => prop1, Default: list[select]);
                }
            }
        }, OtherTypeToString: (obj, type) =>
        {
            List<Preset.Player> player = obj as List<Preset.Player> ?? new();
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
            int select = Window.Menu.WriteMenu(
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
        }
    }

#if DEBUG
    
    static void Test()
    {
        Window.Menu.LargerContentBoard(Enumerable.Repeat("我是测试1145141919810aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaassssssssssssssssssssssssssssssssssaaa", 50).ToArray());
    }
#endif

    static void MainMenu()
    {
        while(true)
        {
            Console.Title = "Hands Bump!  (Copyright) Amlight-Elipese 2023";
            Console.Clear();
            WriteLogo();
            int select = Window.Menu.WriteMenu(
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
            int select = Window.Menu.WriteMenu(
                new Dictionary<string, string>
                {
                {"调整控制台大小", "当选择内容不正确时才可使用该选项" },
                {"返回上一级菜单", "GTR0（"}
                });
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
                    InitConsoleSizeStruct();
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

    

    static (int playercount, Dictionary<int,List<int>>? option) GameArgumentCreater(ClassicPlayMode classicPlayMode)
    {
        switch (classicPlayMode)
        {
            case ClassicPlayMode.Twice:
                return (2, null);
            case ClassicPlayMode.ThreeTime:
                return (3, null);
            case ClassicPlayMode.FourTime:
                return (4, null);
        }
        return (0, null);
    }

    static (int playercount, Dictionary<int, List<int>>? option) FromOptionFileGameArgumentCreater(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<(int playercount, Dictionary<int, List<int>>? option)>(json);
        }
        catch (JsonException)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("发生了一些错误。");
            Console.WriteLine("您选择的配置预设 文件发生了错误。");
            Console.WriteLine("该文件疑似被篡改，不应篡改这些文件。");
            Console.WriteLine("建议：备份后使用 PSON解析器 辅助更改文件并保存文件。");
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            return (0, null);
        }
    }

    static void ToGame(int playercount, Dictionary<int, List<int>>? option)
    {
        Game game = new Game();
        game.InitPlayers(playercount, option);
    }

    static void Main(string[] args)
    {
        Console.WriteLine("加载中...");
        if (LoadGame())
            return;

        MainMenu();
    }
}