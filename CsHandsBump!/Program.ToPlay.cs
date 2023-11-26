using EUtility.ConsoleEx.Message;
using EUtility.StringEx.StringExtension;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandsBump;

internal partial class Program
{
    static void SetAccent()
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
    }

    private class SecondaryMessage : Message
    {
        public override void OutputMessage(int curt = 0)
        {
            int currentTop = curt == 0 ? Console.CursorTop : curt;
            int currentLeft = Console.CursorLeft;
            Console.CursorTop = Console.WindowHeight - 2;
            Console.CursorLeft = 0;
            List<string> message = new List<string>();
            foreach (var unit in MessageUnit)
            {
                message.Add($"{unit.Value.Key} {unit.Value.Value}");
            }
            string output = string.Join("    ", message);
            Console.Write(output + new string(' ', Console.WindowWidth - output.GetStringInConsoleGridWidth() - 2));
            Console.CursorTop = currentTop;
            Console.CursorLeft = currentLeft;
        }
    }

    static SecondaryMessage AppSecondaryMessageBar = new();
    static Message AppGameMessageBar = new();

    static int DisplayItems(List<string> menuitem, int select, int start)
    {
        int currentlength = 0;
        int displaylength = 0;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.CursorTop = 0;
        Console.CursorLeft = 0;
        for (int i = start; i < menuitem.Count; i++)
        {
            var item = menuitem[i];
            currentlength += (item.Length + 1);
            if (currentlength > Console.WindowWidth)
                break;
            if (select == i)
            {
                SetAccent();
            }
            Console.Write(item);
            Console.ResetColor();
            Console.Write(' ');
            displaylength++;
        }

        return displaylength;
    }

    static void PrintTable(Player player, int linelimit, int select = -1, int column = 2, int left = 25, int top = 2)
    {
        Console.CursorLeft = left;
        Console.CursorTop = top;
        for (int i = 0; i < linelimit; i++)
        {
            Console.Write(new string(' ', Console.WindowWidth - 25));
            Console.CursorLeft = left;
            Console.CursorTop++;
        }

        Console.CursorLeft = left;
        Console.CursorTop = top;

        Console.Write("hands列表");

        Console.CursorLeft = left;
        Console.CursorTop++;
        Console.Write(string.Join("", Enumerable.Repeat($"|{"hand顺序",-8}|{"点数",-8}", Math.Min(column, player.Hands.Count)).ToArray()) + "|");

        Console.CursorLeft = left;
        Console.CursorTop = top+2;
        int index = 1;
        foreach (var hand in player.Hands)
        {
            if (Console.CursorTop == top + linelimit)
            {
                break;
            }
            if(index-1 == select)
            {
                Console.Write('|');
                SetAccent();
                Console.Write($"{index,-10}");
                Console.ResetColor();
                Console.Write($"|{hand,-10}");
            }
            else
            {
                Console.Write($"|{index,-10}|{hand,-10}");
            }
            
            if ((index % column) == 0)
            {
                Console.Write("|");
                Console.CursorLeft = left;
                Console.CursorTop++;
            }
            index++;
        }
    }

    public static void StartNewGame(Preset preset)
    {
        #region Define Const
        const string PLAYER_NAME_ITEM = "Current Player Name";
        const string HANDCOUNT_ITEM = "Hand Count";
        const string TARGET_ITEM = "Target";
        const string STEPCOUNT_ITEM = "Step Count";
        const string SCORE_ITEM = "Clac. Score";

        const string SELECT_PREVIOUS_ITEM = "Select Previous";
        const string SELECT_NEXT_ITEM = "Select Next";
        const string SELECT_PREVIOUS_LINE_ITEM = "Select Previous Line";
        const string SELECT_NEXT_LINE_ITEM = "Select Next Line";
        const string SELECT_ITEM = "Select";
        const string EXIT_ITEM = "Exit";
        #endregion

        CurrentGame = new Game(preset);
        var game = CurrentGame;
        Dictionary<Player, int> Score = new();

        var players = game.GetPlayers();
        players.ForEach(x => Score.Add(x, 0));

        List<string> playernames = new();
        players.ForEach(x => playernames.Add(x.Name));

        SelectPlayer:
        #region SelectPlayer
        Console.Clear();

        AppSecondaryMessageBar.ResetMessageUnit();
        AppGameMessageBar.ResetMessageUnit();

        AppSecondaryMessageBar.AddMessageUnit(PLAYER_NAME_ITEM, new("当前玩家", ""));
        AppSecondaryMessageBar.AddMessageUnit(HANDCOUNT_ITEM, new("剩余hand数", ""));
        AppSecondaryMessageBar.AddMessageUnit(TARGET_ITEM, new("目标点数", ""));
        AppSecondaryMessageBar.AddMessageUnit(STEPCOUNT_ITEM, new("步数", ""));
        AppSecondaryMessageBar.AddMessageUnit(SCORE_ITEM, new("将获得的分数", ""));
        AppSecondaryMessageBar.OutputMessage();

        SetAccent();
        AppGameMessageBar.AddMessageUnit(SELECT_PREVIOUS_ITEM, new("←", "上一个玩家"));
        AppGameMessageBar.AddMessageUnit(SELECT_NEXT_ITEM, new("→", "下一个玩家"));
        AppGameMessageBar.AddMessageUnit(SELECT_ITEM, new("Enter", "选择玩家"));
        AppGameMessageBar.AddMessageUnit(EXIT_ITEM, new("ESC", "退出游戏"));
        AppGameMessageBar.OutputMessage();
        Console.ResetColor();

        while (!game.JudgeIsGameOver())
        {
            Console.ResetColor();
            var player = game.GetCurrentPlayer();
            Score[player] = player.GetSum();

            AppSecondaryMessageBar.SetValue(PLAYER_NAME_ITEM, player.Name);
            AppSecondaryMessageBar.SetValue(HANDCOUNT_ITEM, player.Hands.Count.ToString());
            AppSecondaryMessageBar.SetValue(TARGET_ITEM, player.Target.ToString());
            AppSecondaryMessageBar.SetValue(STEPCOUNT_ITEM, player.StepCount.ToString());
            AppSecondaryMessageBar.SetValue(SCORE_ITEM, player.StepCount == 0 ? 0.ToString() : CalcScore(Score, player).ToString());
            AppSecondaryMessageBar.OutputMessage();

            Console.ResetColor();
            int select = 0;
            int start = 0;
            while (true)
            {
                Console.SetCursorPosition(0, 0);
                List<string> noselfnames = playernames.FindAll(x => x != player.Name);
                int displaylength = DisplayItems(noselfnames, select, start);

                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;

                int linelimit = Console.WindowHeight - 6;
                int currentTop = 3;

                Console.CursorTop = 2;
                Console.CursorLeft = 0;
                Console.WriteLine($"{$"点数排行(前{linelimit - 1})",-9}");
                var tempscorelist = Score.ToList();
                tempscorelist.Sort((item, item2) => item.Value.CompareTo(item2.Value));
                tempscorelist.Reverse();
                Score = tempscorelist.ToDictionary(k => k.Key, v => v.Value);
                foreach (var playerscore in Score)
                {
                    Console.CursorLeft = 0;
                    if (currentTop - 3 >= linelimit)
                    {
                        break;
                    }
                    Console.WriteLine($"{(playerscore.Key.Name.Length > 19 ? playerscore.Key.Name[..19] : playerscore.Key.Name),-19} {playerscore.Value,3}");
                    currentTop++;
                }

                Console.ResetColor();

                PrintTable(players[select], linelimit);

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        select--;
                        if (select == -1)
                        {
                            select = 0;
                            continue;
                        }
                        if (select < start)
                        {
                            start--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        select++;
                        if (select == playernames.Count)
                        {
                            select = playernames.Count - 1;
                            continue;
                        }
                        if (displaylength <= select)
                        {
                            start++;
                        }
                        break;
                    case ConsoleKey.Enter:
                        goto SelectOtherPlayerHand;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        Console.WriteLine("确定？(Y/N)");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                            return;
                        goto SelectPlayer;
                }
            }

        SelectOtherPlayerHand:
            #region SelectOtherPlayerHand
            bool inserted = false;
            Player selectPlayer = players[select];
            int column = (int)Math.Floor((double)((Console.WindowWidth - 1) / 22));
            int handsSelect = 0;
            Console.Clear();
            if (!inserted)
            {
                AppGameMessageBar.InsertMessageUnit(2, SELECT_PREVIOUS_LINE_ITEM, new("↑", "上一行hand"));
                AppGameMessageBar.InsertMessageUnit(3, SELECT_NEXT_LINE_ITEM, new("↓", "下一行hand"));
                inserted = true;
            }

            AppGameMessageBar.SetValue(SELECT_NEXT_ITEM, "下一个hand");
            AppGameMessageBar.SetValue(SELECT_PREVIOUS_ITEM, "上一个hand");
            AppGameMessageBar.SetValue(SELECT_ITEM, "选择hand");
            AppGameMessageBar.SetValue(EXIT_ITEM, "返回到选择玩家");
            AppGameMessageBar.OutputMessage();
            while (true)
            {
                PrintTable(selectPlayer,
                     Console.WindowHeight - 1,
                       handsSelect,
                       column,
                       0, 0);
                var handkey = Console.ReadKey();
                switch (handkey.Key)
                {
                    case ConsoleKey.LeftArrow:
                        handsSelect--;
                        if (handsSelect == -1)
                        {
                            handsSelect = 0;
                            continue;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        handsSelect++;
                        if (handsSelect >= selectPlayer.Hands.Count)
                        {
                            handsSelect = playernames.Count - 1;
                            continue;
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        handsSelect -= column;
                        if (0 > handsSelect)
                        {
                            handsSelect = 0;
                            continue;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        handsSelect += column;
                        if (handsSelect >= selectPlayer.Hands.Count)
                        {
                            handsSelect = selectPlayer.Hands.Count - 1;
                            continue;
                        }
                        break;
                    case ConsoleKey.Escape:
                        goto SelectPlayer;
                    case ConsoleKey.Enter:
                        goto SelectOwnerHand;
                }
            }
        #endregion
        SelectOwnerHand:
            #region SelectOwnerHand
            int ownerhandsSelect = 0;
            AppGameMessageBar.SetValue(EXIT_ITEM, "返回到选择对方hand");
            AppGameMessageBar.OutputMessage();
            while (true)
            {
                PrintTable(player,
                     Console.WindowHeight - 1,
                       ownerhandsSelect,
                       column,
                       0, 0);
                var ownerhandkey = Console.ReadKey();
                switch (ownerhandkey.Key)
                {
                    case ConsoleKey.LeftArrow:
                        ownerhandsSelect--;
                        if (ownerhandsSelect == -1)
                        {
                            ownerhandsSelect = 0;
                            continue;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        ownerhandsSelect++;
                        if (ownerhandsSelect >= player.Hands.Count)
                        {
                            ownerhandsSelect = playernames.Count - 1;
                            continue;
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        ownerhandsSelect -= column;
                        if (0 > ownerhandsSelect)
                        {
                            ownerhandsSelect = 0;
                            continue;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        ownerhandsSelect += column;
                        if (ownerhandsSelect >= player.Hands.Count)
                        {
                            ownerhandsSelect = player.Hands.Count - 1;
                            continue;
                        }
                        break;
                    case ConsoleKey.Escape:
                        goto SelectOtherPlayerHand;
                    case ConsoleKey.Enter:
                        goto ToNextPlayer;
                }
            }
        #endregion
        ToNextPlayer:
            player.Bump(selectPlayer, handsSelect, ownerhandsSelect);
            game.NextPlayer();
            goto SelectPlayer;
        }
        #endregion

        var winplayer = game.GetWinPlayer();

        Console.Clear();
        WriteLogo();

        Console.WriteLine("游戏已结束！");
        Console.WriteLine("本次游戏优胜者是！(按点数排行)");
        SetAccent();
        Console.WriteLine("......" + winplayer.Name + new string(' ', 32) + winplayer.GetSum() + "点！！！");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("以下是各玩家点数排行！");
        players.Sort((x, nx) => nx.GetSum().CompareTo(x.GetSum()));
        foreach(var item in players)
        {
            Console.WriteLine(item.Name + new string(' ', 32) + item.GetSum());
        }
        AppMessageBar.ResetMessageUnit();
        AppMessageBar.AddMessageUnit(EXIT_ITEM, new("Enter", "继续"));
        AppMessageBar.OutputMessage();
        Console.ReadLine();

        Console.Clear();
        WriteLogo();

        Console.WriteLine("游戏已结束！");
        Console.WriteLine("本次游戏优胜者是！(按计算分数排行)");
        SetAccent();
        Console.WriteLine("......" + winplayer.Name + new string(' ', 32) + CalcScore(Score, winplayer) + "分！！！");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("以下是各玩家分数排行！");
        players.Sort((x, nx) => CalcScore(Score, nx).CompareTo(CalcScore(Score, x)));
        foreach (var item in players)
        {
            Console.WriteLine(item.Name + new string(' ', 32) + CalcScore(Score, item));
        }
        AppMessageBar.ResetMessageUnit();
        AppMessageBar.AddMessageUnit(EXIT_ITEM, new("Enter", "继续"));
        AppMessageBar.OutputMessage();
        Console.ReadLine();

        Console.Clear();
        WriteLogo();

        Console.WriteLine("请输入本局游戏的名称");
        string name = Console.ReadLine();
        game.GameEnd(name);
    }

    private static int CalcScore(Dictionary<Player, int> Score, Player? player)
    {
        return (Score[player] / (player.Target * player.HandCount) * 60 + (player.Target * player.HandCount) / (player.Target * player.StepCount) * 40 + player.HandCount);
    }
}
