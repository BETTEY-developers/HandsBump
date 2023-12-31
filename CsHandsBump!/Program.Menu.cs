﻿using EUtility.ConsoleEx.Message;
using EUtility.StringEx.StringExtension;
using System.Reflection;

namespace HandsBump;
using MessageUnitOrigin = KeyValuePair<string, string>;

internal partial class Program
{
    public class Message
    {
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        public Dictionary<string, MessageUnitOrigin> MessageUnit { get; set; } = new();

        public void AddMessageUnit(string idkey, MessageUnitOrigin messageUnit)
        {
            MessageUnit.Add(idkey, messageUnit);
        }

        public void SetMessageUnit(string idkey, MessageUnitOrigin messageUnit)
        {
            MessageUnit[idkey] = messageUnit;
        }

        public void RemoveMessageUnit(string idkey)
        {
            MessageUnit.Remove(idkey);
        }

        public void ResetMessageUnit()
        {
            MessageUnit.Clear();
        }

        public void InsertMessageUnit(int index,string idkey, MessageUnitOrigin messageUnit)
        {
            var temp = MessageUnit.ToList();
            temp.Insert(index, new KeyValuePair<string, MessageUnitOrigin>(idkey, messageUnit));
            MessageUnit = temp.ToDictionary((x)=> x.Key, x => x.Value);
        }

        public void SetValue(string key, string value)
        {
            MessageUnit[key] = new MessageUnitOrigin(MessageUnit[key].Key, value);
        }

        public virtual void OutputMessage(int curt = 0)
        {
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
            int currentTop = curt==0?Console.CursorTop:curt;
            int currentLeft = Console.CursorLeft;
            Console.CursorTop = Console.WindowHeight-1;
            Console.CursorLeft = 0;
            List<string> message = new List<string>();
            foreach (var unit in MessageUnit)
            {
                message.Add($"{unit.Value.Key} {unit.Value.Value}");
            }
            string output = string.Join("    ", message);
            Console.Write(output + new string(' ',Console.WindowWidth - output.GetStringInConsoleGridWidth() - 0));
            Console.CursorTop = currentTop;
            Console.CursorLeft= currentLeft;
        }
    }

    public static class Menu
    {
        static MessageOutputer menuguide = new()
        {
            new MessageUnit()
            {
                Title = "↑",
                Description = "上一个选项"
            },
            new MessageUnit()
            {
                Title = "↓",
                Description = "下一个选项"
            },
            new MessageUnit()
            {
                Title = "Enter",
                Description = "确认选项"
            }
        };
        static Menu()
        {

        }
        public static int WriteMenu(Dictionary<string, string> menuitem, int curstartindex = 2)
        {
            int select = 0;
            Console.CursorTop = curstartindex;
            for(int i = 0; i < Console.WindowHeight - curstartindex - 1; i++)
            {
                Console.WriteLine(new string(' ', Console.WindowWidth));
            }
            while (true)
            {
                menuguide.Write();
                Console.CursorTop = curstartindex;
                int index = 0;
                foreach (var item in menuitem)
                {
                    if (index == select)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine("   -->" + item.Key + new string(' ', Console.WindowWidth - (item.Key + "   -->").GetStringInConsoleGridWidth()));
                        Console.ResetColor();

                        var ct = Console.CursorTop;
                        Console.CursorTop = menuitem.Count + 2 + curstartindex;
                        Console.WriteLine("说明：");
                        var dct = Console.CursorTop;
                        for (int i = 0; i < Console.WindowHeight - Console.CursorTop - 1; i++)
                        {
                            Console.WriteLine(new string(' ', Console.WindowWidth));
                        }
                        Console.CursorTop = dct;
                        Console.WriteLine(item.Value);
                        Console.CursorTop = ct;
                    }
                    else
                    {
                        Console.WriteLine(item.Key + new string(' ', Console.WindowWidth - item.Key.GetStringInConsoleGridWidth()));
                    }
                    index++;
                }
                Console.CursorVisible = false;
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                {
                    select--;
                    if (select < 0)
                    {
                        select = menuitem.Count - 1;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    select++;
                    select %= menuitem.Count;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return select;
                }
            }
        }

        public static int WriteLargerMenu(Dictionary<string, string> menuitem, int maxitems = 8, int curstartindex = 2)
        {
            int select = 0, startitem = 0, enditem = maxitems;
            void WriteItems()
            {
                int index = startitem;
                foreach (var item in menuitem.Skip(startitem).SkipLast(menuitem.Count - startitem - enditem))
                {
                    if (index == select)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine("   -->" + item.Key + new string(' ',Console.WindowWidth-(item.Key + "   -->").GetStringInConsoleGridWidth()));
                        Console.ResetColor();

                        var ct = Console.CursorTop;
                        Console.CursorTop = maxitems + 2 + curstartindex;
                        Console.WriteLine("说明：");
                        var dct = Console.CursorTop;
                        for (int i = 0; i < Console.WindowHeight - Console.CursorTop - 1; i++)
                        {
                            Console.WriteLine(new string(' ',Console.WindowWidth));
                        }
                        Console.CursorTop = dct;
                        Console.WriteLine(item.Value);
                        Console.CursorTop = ct;
                    }
                    else
                    {
                        Console.WriteLine(item.Key + new string(' ',Console.WindowWidth - item.Key.GetStringInConsoleGridWidth()));
                    }
                    index++;
                }
            }


            while (true)
            {
                menuguide.Write();
                Console.CursorTop = curstartindex;
                WriteItems(); 

                Console.CursorVisible = false;
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                {
                    select--;
                    if (select < 0)
                        select = 0;
                    if (select < startitem)
                    {
                        startitem = select;
                        enditem--;
                        if (startitem < 0)
                        {
                            startitem = menuitem.Count - enditem;
                            enditem = menuitem.Count - 1;
                            select = enditem;
                        }

                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    select++;
                    if (select > menuitem.Count - 1)
                        select = menuitem.Count - 1;
                    if (select > enditem)
                    {
                        enditem = select;
                        startitem++;
                        if (enditem > menuitem.Count)
                        {
                            enditem = maxitems;
                            select = 0;
                            startitem = 0;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return select;
                }
            }
        }

        public static void LargerContentBoard(string[] lines)
        {
            List<string> wordbreak = new();
            AppMessageBar.ResetMessageUnit();
            AppMessageBar.AddMessageUnit("Pre", new("↑", "往上显示一行"));
            AppMessageBar.AddMessageUnit("Next", new("↓", "往下显示一行"));
            AppMessageBar.AddMessageUnit("OK", new("Enter", "退出内容版"));
            AppMessageBar.AddMessageUnit("Process", new("当前", ""));
            foreach (string line in lines)
            {
                if (line.GetStringInConsoleGridWidth() <= Console.WindowWidth)
                {
                    wordbreak.Add(line);
                    continue;
                }

                int length = 0, index = 0, lastindex = 0;
                foreach (char c in line)
                {
                    length += NullLib.ConsoleEx.ConsoleText.CalcCharLength(c);
                    if (length > Console.WindowWidth)
                    {
                        wordbreak.Add(line[lastindex..(index - 1)]);
                        lastindex = index;
                        length = 0;
                    }
                    else if (length == Console.WindowWidth)
                    {
                        wordbreak.Add(line[lastindex..index]);
                        lastindex = index;
                        length = 0;
                    }
                    index++;
                }
            }
            int startline = 0, endline = Math.Min(wordbreak.Count, Console.WindowHeight - 1);
            while (true)
            {
                Console.Clear();
                for (int i = startline; i < endline; i++)
                {
                    Console.WriteLine(wordbreak[i]);
                }
                Console.SetCursorPosition(0, Console.WindowHeight-1);
                AppMessageBar.SetMessageUnit("Process", new("当前", $"{startline}-{endline}/{wordbreak.Count}"));
                AppMessageBar.OutputMessage(Console.WindowHeight - 1);
                Console.CursorVisible = true;
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.UpArrow && startline > 0)
                    {
                        startline--;
                        endline--;
                        break;
                    }
                    else if (key.Key == ConsoleKey.DownArrow && endline < wordbreak.Count)
                    {
                        endline++;
                        startline++;
                        break;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        return;
                    }
                }

            }
        }
    }
}
