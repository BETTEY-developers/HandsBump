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
    public struct InitGameData
    {

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

    public Game()
    {
        StartTime = DateTime.Now;
        id = Guid.NewGuid();
    }

    public void InitPlayers(int playercount, Dictionary<int,List<int>>? option)
    {
        this.playercount = playercount;

        int i = 0;
        while(i++ < playercount)
        {
            if (option.ContainsKey(i) || option == null ? false : true)
            {
                Player player = new Player
                {
                    HandCount = option[i][0],
                    Target = option[i][1],
                    StartupNumber = (option[i].Count == 3) ? option[i][2] : 0
                };
                players.Add(player);
            }
            else
                players.Add(new());
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
