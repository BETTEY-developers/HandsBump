namespace HandsBump;

internal class Preset
{
    public class Player
    {
        [MemberId(10, typeof(int))]
        public int Target { get; set; }
        [MemberId(11, typeof(int))]
        public int HandCount { get; set; }
        [MemberId(12, typeof(int))]
        public int StartupNumber { get; set; }
        public int TargetPlayerId { get; set; }
    }

    [MemberId(0, typeof(string))]
    public string PresetName { get; set; }

    [MemberId(1, typeof(int))]
    public int PlayerCount { get; set; }

    [MemberId(2, typeof(List<Player>))]
    public List<Player> PlayerOption { get; set; }
} 
