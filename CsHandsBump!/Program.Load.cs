using HandsBump.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HandsBump
{
    internal partial class Program
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr GetStdHandle(nuint nStdHandle);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int SetConsoleMode(IntPtr hConsoleHandle, nuint dwMode);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetConsoleMode(IntPtr hConsoleHandle, ref nuint lpMode);

        static bool SetConsoleOutMode()
        {
            int _errorcode = 0;
            try
            {
                IntPtr ConsoleHandle = GetStdHandle(0xFFFFFFF5);
                nuint mode = 0;
                int getstatus = GetConsoleMode(ConsoleHandle, ref mode);
                if (getstatus != 0)
                {
                    _errorcode = GetLastError();
                    throw new Win32Exception(_errorcode);
                }
                int status = SetConsoleMode(ConsoleHandle, mode | 0x0200);
                if (status != 0)
                {
                    _errorcode = GetLastError();
                    throw new Win32Exception(_errorcode);
                }
                return false;
            }
            catch (Win32Exception e)
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
            catch (Exception e)
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

        static bool InitConsoleSizeStructure(bool overwrite = false)
        {
            try
            {
                if(!File.Exists("Setting.txt") || overwrite)
                {
                    var stream = File.CreateText("Setting.txt");
                    StartupConsoleSize.Width = Console.WindowWidth;
                    StartupConsoleSize.Height = Console.WindowHeight;
                    JsonSerializer.Serialize(stream.BaseStream, StartupConsoleSize);
                    stream.Close();
                }
                else
                {
                    string setting = File.ReadAllText("Setting.txt");
                    StartupConsoleSize = JsonSerializer.Deserialize<ConsoleSize>(setting);
                }
                return false;
            }
            catch
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
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
                        if (canresize)
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("发生了一些错误。");
                Console.WriteLine("在启动新线程时发生了错误。");
                Console.WriteLine("电脑运行内存不足。");
                Console.WriteLine("建议：退出一些应用并重新启动该应用。");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return true;
            }
        }

        static bool LoadGame()
        {
            /*foreach(var pro in Process.GetProcesses())
            {
                if(pro. == Process.GetCurrentProcess().MainModule)
                {
                    Console.Clear();
                    Console.WriteLine("计算机上已经运行了一个handsbump！");
                    Console.ReadKey();
                    return true;
                }
            }*/
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



            return LoadGameRecord() || InitConsoleSizeStructure() || OpenResizeConsoleThread();
        }
    }
}
