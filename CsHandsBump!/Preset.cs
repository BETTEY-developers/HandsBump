namespace HandsBump;

internal class Preset
{
    public class PlayerPreset
    {
        [MemberId(10, typeof(int))]
        public int Target { get; set; }
        [MemberId(11, typeof(int))]
        public int HandCount { get; set; }
        [MemberId(12, typeof(int))]
        public int StartupNumber { get; set; }
        [MemberId(13, typeof(int))]
        public int TargetPlayerId { get; set; }

        public static implicit operator Player(PlayerPreset preset) 
            => new()
            {
                HandCount = preset.HandCount,
                StartupNumber = preset.StartupNumber,
                Target = preset.Target
            };
    }

    [MemberId(0, typeof(string))]
    public string PresetName { get; set; }

    [MemberId(1, typeof(int))]
    public int PlayerCount { get; set; }

    [MemberId(2, typeof(List<PlayerPreset>))]
    public List<PlayerPreset> PlayerOption { get; set; }
} 
