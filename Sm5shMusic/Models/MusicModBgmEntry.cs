namespace Sm5shMusic.Models
{
    public class MusicModBgmEntry
    {
        public string NameId { get; set; }
        public Song Song { get; set; }
        public string AudioFilePath { get; set; }
        public string InternalToneName { get; set; }
        public MusicModAudioCuePoints CuePoints { get; set; }
    }
}
