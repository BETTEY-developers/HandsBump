namespace HandsBump;

internal struct Preset
{
    public struct Player
    {
        public int Target { get; set; }
        public int HandCount { get; set; }
        public int StartupNumber { get; set; }
    }

    public string PresetName { get; set; }

    public int PlayerCount { get; set; }
    public List<Player> PlayerOption { get; set; }
} 
