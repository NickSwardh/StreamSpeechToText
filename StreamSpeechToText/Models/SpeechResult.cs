using System.Collections.Generic;

namespace StreamSpeechToText.Models
{
    public class SpeechResult
    {
        public double Confidence { get; set; }
        public string Display { get; set; }
        public string ITN { get; set; }
        public string Lexical { get; set; }
        public string MaskedITN { get; set; }
        public List<SpokenWord> Words { get; set; }
    }
}
