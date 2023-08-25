using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

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
    static bool canresize = false;

    [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
    static extern IntPtr GetStdHandle(uint nStdHandle);

    [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
    static extern int SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
    static extern int GetLastError();

    [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
    static extern int GetConsoleMode(IntPtr hConsoleHandle, ref uint lpMode);

    static bool SetConsoleOutMode()
    {
        int _errorcode = 0;
        try
        {
            IntPtr ConsoleHandle = GetStdHandle(0xFFFFFFF5);
            uint mode = 0;
            int getstatus = GetConsoleMode(ConsoleHandle, ref mode);
            if (getstatus != 0)
            {
                _errorcode = GetLastError();
                throw new Win32Exception(_errorcode);
            }
            int status = SetConsoleMode(ConsoleHandle, mode|0x0004);
            if(status != 0)
            {
                _errorcode = GetLastError();
                throw new Win32Exception(_errorcode);
            }
            return false;
        }
        catch(Win32Exception e)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("发生了一些错误。");
            Console.WriteLine("错误码：" + _errorcode);
            Console.WriteLine("Win32Error: " + e.Message);
            //Console.WriteLine("使用 Err 工具获取错误信息");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return true;
        }
        catch(Exception e)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("发生了一些错误。");
            Console.WriteLine(e.Message);
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return true;
        }
    }

    static bool InitConsoleSizeStruct()
    {
        try
        {
            StartupConsoleSize.Width = Console.WindowWidth;
            StartupConsoleSize.Height = Console.WindowHeight;
            return false;
        }
        catch
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("发生了一些错误。");
            Console.WriteLine("该应用不支持此系统");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return true;
        }
    }

    static void ResetConsoleSize()
    {
        try
        {
            Console.WindowWidth = StartupConsoleSize.Width; //设置窗体宽度
            Console.BufferWidth = StartupConsoleSize.Width; //设置缓存宽度
            Console.WindowHeight = StartupConsoleSize.Height;//设置窗体高度
            Console.BufferHeight = StartupConsoleSize.Height;//设置缓存高度
            Console.WindowWidth = StartupConsoleSize.Width; //重新设置窗体宽度
        }
        catch { }
    }

    static bool OpenResizeConsoleThread()
    {
        try
        {
            canresize = true;
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    if(canresize)
                        ResetConsoleSize();
                    Thread.Sleep(1000);
                }
            });
            thread.Name = "ResizeConsole";
            thread.IsBackground = true;
            thread.Start();
            ResizeThread = thread;
            return false;
        }
        catch
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("发生了一些错误。");
            Console.WriteLine("在启动新线程时发生了错误。");
            Console.WriteLine("电脑运行内存不足。");
            Console.WriteLine("建议：退出一些应用并重新启动该应用。");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return true;
        }
    }

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
        while (true) try
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
            Span<byte> bytes = new();
            pb.Read(bytes);
            List<byte> j = new();
            try
            {
                Encoding.Default.GetString(bytes).Split("#").ToList().ForEach((s) => j.Add(byte.Parse(s)));
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
            sb.AppendLine($"{"玩家",-10}|{"需赢点数",-10}|{"hand数",-10}|{"起始点数",-10}");
            int currentplayerindex = 0;
            foreach(var player in preset.PlayerOption)
            {
                sb.AppendLine($"{$"玩家{currentplayerindex}",-10}|{player.Target,-10}|{player.HandCount,-10}|{player.StartupNumber,-10}");
            }
            items.Add(preset.PresetName, sb.ToString());
            descriptionlist.Add(sb);
        }

        
        select = Window.Menu.WriteLargerMenu(items, Math.Min(8, items.Count));

        if(select == 0)
        {
            return;
        }
        else if(select == 1)
        {
            GameOptionCreater(new Preset());
        }
    }

    private static void GameOptionCreater(Preset preset)
    {
        /*
        int GetInputIntValue(int min, int max)
        {
            Console.Clear();
            WriteLogo();

            Console.WriteLine();
            Console.Write("请输入值：");
            
        }
        Preset pc = preset;
        PropSeter(pc.PlayerOption);*/
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

    static bool LoadGame()
    {
        bool LoadGameRecord()
        {
            try
            {
                if (!File.Exists("GameRecord.cod"))
                    return false;

                List<byte> bytes = new List<byte>();
                File.ReadAllText("GameRecord.cod").Split("#").ToList().ForEach(v => bytes.Add(byte.Parse(v)));

                string json = Encoding.UTF8.GetString(bytes.ToArray());
                Game.FormatGameRecords(json);
                return false;
            }
            catch (Exception e) when (e is JsonException or FormatException)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("发生了一些错误。");
                Console.WriteLine("GameRecord.cod 文件发生了错误。");
                Console.WriteLine("该文件疑似被篡改，不应篡改这些文件。");
                Console.WriteLine("建议：备份后删除文件并重启应用。");
                Console.WriteLine("注意：不备份你将损失你的数据！");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return true;
            }
        }

        

        return LoadGameRecord() || InitConsoleSizeStruct() || OpenResizeConsoleThread() ;
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
            Console.WriteLine("建议：备份后使用 JSON解析器 辅助更改文件并保存文件。");
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