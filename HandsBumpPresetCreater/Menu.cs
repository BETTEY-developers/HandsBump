using EUtility.StringEx.StringExtension;
using EUtility.ConsoleEx.Message;

namespace HandsBumpPresetCreater
{
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
            for (int i = 0; i < Console.WindowHeight - curstartindex - 1; i++)
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
                        Console.WriteLine("   -->" + item.Key + new string(' ', Console.WindowWidth - (item.Key + "   -->").GetStringInConsoleGridWidth()));
                        Console.ResetColor();

                        var ct = Console.CursorTop;
                        Console.CursorTop = maxitems + 2 + curstartindex;
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
    }
}
