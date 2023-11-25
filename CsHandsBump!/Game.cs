using System.Text.Json;
using System.Text.Json.Serialization;

namespace HandsBump;

internal class Game
{
    public struct GameData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public Status Status { get; set; }
        public List<Player> Players { get; set; }

        public int PlayerCount { get; set; }

        public bool IsNullStruct { get; set; }

    }

    public class Status
    {
        public int CurrentPlayer = 0;
        public int? WinPlayer;
    }

    private static List<GameData> games = new List<GameData>();

    private Status status = new Status();

    private DateTime StartTime;

    private Guid id;

    private int playercount = 0;

    private List<Player> players = new List<Player>();

    public Game(Preset preset)
    {
        StartTime = DateTime.Now;
        id = Guid.NewGuid();

        int playerindex = 0;

        this.playercount = preset.PlayerCount;
        foreach(var playeroption in preset.PlayerOption)
        {
            string name = "";

            Console.Clear();
            Program.WriteLogo();
            Console.WriteLine("请输入玩家 " + (playerindex + 1) + " 昵称：");
            name = Console.ReadLine();
            
            if (playeroption.Target - 1 == playerindex)
            {
                Player player = playeroption;
                player.Name = name;
                players.Add(player);
            }
            else
            {
                players.Add(new()
                {
                    HandCount = 2,
                    Name = name,
                    Target = 10,
                    StartupNumber = 0
                });
            }
        }

        status.CurrentPlayer = 0;
        status.WinPlayer = -1;
    }

    public Player GetCurrentPlayer() => players[status.CurrentPlayer];

    public Player GetPlayerFromIndex(int index) => players[index];

    public void NextPlayer() => status.CurrentPlayer++;

    public bool JudgeIsGameOver()
    {
        int index = 0;
        foreach(Player player in players)
        {
            if (player.Hands.Count == 0)
            {
                status.WinPlayer = index;
                return true;
            }
            index++;
        }
        
        return false;
    }

    public void GameEnd(string name)
    {
        GameData gd = new()
        {
            StartTime = StartTime,
            EndTime = DateTime.Now,
            Id = id,
            Name = name,
            Status = status,
            PlayerCount = playercount,
            Players = players,
            IsNullStruct = false
        };

        games.Add(gd);
    }

    public static GameData GetGame(Func<GameData, bool> func)
    {
        foreach(var gd in games)
        {
            bool flag = func(gd);
            if(flag)
                return gd;
        }
        return new GameData() { IsNullStruct = true };
    }

    public static void FormatGameRecords(string json)
    {
        games = JsonSerializer.Deserialize<List<GameData>>(json);
    }
}
