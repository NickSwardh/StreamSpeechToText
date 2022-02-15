using System.Collections.Generic;

namespace StreamSpeechToText.Models
{
    public class Speech
    {
        public string Id { get; set; }
        public string DisplayText { get; set; }
        public long Duration { get; set; }
        public string RecognitionStatus { get; set; }
        public List<SpeechResult> NBest { get; set; }
    }
}
