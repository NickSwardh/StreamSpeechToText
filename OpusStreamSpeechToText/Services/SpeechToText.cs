using Azure.Storage.Blobs;
using Concentus.Oggfile;
using Concentus.Structs;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using OpusStream.Models;
using OpusStreamSpeechToText.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpusStreamSpeechToText.Services
{
    public class SpeechToText
    {
        private SpeechConfig _speechConfig;
        private AutoDetectSourceLanguageConfig _languagesToDetect;
        private TaskCompletionSource<int> _stopRecognition;

        public SpeechToText(string[] languages)
        {
            _speechConfig = SpeechConfig.FromSubscription(Settings.AzureSpeech.KEY, Settings.AzureSpeech.REGION);
            _speechConfig.RequestWordLevelTimestamps();
            _languagesToDetect = AutoMaticLanguageDetection(languages);
            _stopRecognition = new TaskCompletionSource<int>();
        }

        /// <summary>
        /// Returns speech to text from selected Opus audiofile streamed from a blobcontainer in Azure Storage.
        /// </summary>
        /// <param name="opusBlob">Name of opus file</param>
        /// <param name="container">Azure blob container name</param>
        /// <returns>List<Speech> container speechresults</returns>
        public async Task<List<Speech>> RunRecognitionAsync(string opusBlob, string container)
        {
            SpeechResult = new List<Speech>();

            var blobService = new BlobService();
            var blobClient = await blobService.GetBlobFromContainerAsync(opusBlob, container);

            using var audioInputStream = AudioInputStream.CreatePushStream();
            using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
            using (var recognizer = new SpeechRecognizer(_speechConfig, _languagesToDetect, audioConfig))
            {
                recognizer.Recognizing += Recognizing;
                recognizer.Recognized += Recognized;
                recognizer.SessionStarted += SessionStarted;
                recognizer.SessionStopped += SessionStopped;
                recognizer.Canceled += SessionCanceled;

                await InjectStreamIntoRecognizerAsync(audioInputStream, blobClient);

                await recognizer.StartContinuousRecognitionAsync();
                Task.WaitAny(new[] { _stopRecognition.Task });
                await recognizer.StopContinuousRecognitionAsync();
            }

            return SpeechResult;
        }

        private async Task InjectStreamIntoRecognizerAsync(PushAudioInputStream audioInputStream, BlobClient blobStream)
        {
            using (var stream = await blobStream.OpenReadAsync())
            {
                var decoder = new OpusDecoder(16000, 1);
                var opus = new OpusOggReadStream(decoder, stream);

                while (opus.HasNextPacket)
                {
                    short[] packet = opus.DecodeNextPacket();
                    if (packet != null)
                    {
                        for (int i = 0; i < packet.Length; i++)
                        {
                            var bytes = BitConverter.GetBytes(packet[i]);
                            audioInputStream.Write(bytes, bytes.Length);
                        }
                    }
                }
            }

            audioInputStream.Close();
        }

        private void SessionStopped(object sender, SessionEventArgs e)
        {
            Console.WriteLine($"\r\nSpeech session {e.SessionId} stopped.\r\n");
            _stopRecognition.TrySetResult(0);
        }

        private void SessionCanceled(object sender, SessionEventArgs e)
        {
            _stopRecognition.TrySetResult(0);
        }

        private void SessionStarted(object sender, SessionEventArgs e)
        {
            Console.WriteLine($"Speech session {e.SessionId} started\r\n");
        }

        private void Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            Console.WriteLine();
            var test = GetResponse(e.Result.Properties);
            SpeechResult.Add(test);
        }

        private void Recognizing(object sender, SpeechRecognitionEventArgs e)
        {
            Console.WriteLine($"Recognizing text: {e.Result.Text}");
        }

        private static Speech GetResponse(PropertyCollection properties)
        {
            string property = properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
            return JsonConvert.DeserializeObject<Speech>(property);
        }

        private AutoDetectSourceLanguageConfig AutoMaticLanguageDetection(string[] languagesToDetect)
            => AutoDetectSourceLanguageConfig.FromLanguages(languagesToDetect);

        private List<Speech> SpeechResult { get; set; }

        public static string TicksToTime(long ticks)
            => TimeSpan.FromTicks(ticks).ToString(@"hh\:mm\:ss\.fff");
    }
}