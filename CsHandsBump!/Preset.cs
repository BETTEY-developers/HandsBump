namespace HandsBump;

internal struct Preset
{
    public struct Player
    {
        [MemberId(10)]
        public int Target { get; set; }
        [MemberId(11)]
        public int HandCount { get; set; }
        [MemberId(12)]
        public int StartupNumber { get; set; }
    }

    [MemberId(0)]
    public string PresetName { get; set; }

    [MemberId(1)]
    public int PlayerCount { get; set; }

    [MemberId(2)]
    public List<Player> PlayerOption { get; set; }
} 
