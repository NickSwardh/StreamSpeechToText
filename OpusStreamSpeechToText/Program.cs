using OpusStream.Models;
using OpusStreamSpeechToText.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpusStream
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var opusFile = "opusaudio.opus";
            var containerName = "your_blob_container";
            var languagesToAutoDetect = new string[] { "sv-SE", "en-US", "pt-BR" };

            try
            {
                var speechToText = new SpeechToText(languagesToAutoDetect);
                var textResult = await speechToText.RunRecognitionAsync(opusFile, containerName);

                PrintSpeechToTextResult(textResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.ReadKey();
        }

        public static void PrintSpeechToTextResult(List<Speech> result)
        {
            GetWordDetails(result);
            PrintCompleteText(result);
        }

        public static void PrintCompleteText(List<Speech> result)
        {
            var completeText = string.Join(" ", result.Select(x => x.DisplayText).ToList());

            Console.WriteLine("\r\nText Result:");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(completeText + "\r\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void GetWordDetails(List<Speech> textResult)
        {
            Console.WriteLine($"\r\n\tDetailed Information:");
            Console.WriteLine($"\t{ new string('=', 80)}");

            foreach (var result in textResult)
            {
                // Sort by confidence
                var text = result.NBest?.OrderByDescending(x => x.Confidence).FirstOrDefault();

                if (text?.Words != null)
                {
                    foreach (var word in text.Words)
                    {
                        var timestamp = SpeechToText.TicksToTime(word.Offset);
                        var duration = SpeechToText.TicksToTime(word.Duration);
                        PrintWordDetails(timestamp, duration, word.Word);
                    }
                }
            }
        }

        public static void PrintWordDetails(string timeStamp, string duration, string word)
        {
            Console.Write($"\tTimestamp: ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{timeStamp}\t\t");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write($"Word Duration: ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{duration}\t");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write($"Word: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{word}\r\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}