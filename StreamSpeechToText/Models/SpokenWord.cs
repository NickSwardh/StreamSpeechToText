namespace StreamSpeechToText.Models
{
    public class SpokenWord
    {
        public long Duration { get; set; }
        public long Offset { get; set; }
        public string Word { get; set; }
    }
}
