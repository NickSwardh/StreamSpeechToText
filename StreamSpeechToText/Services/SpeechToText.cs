using Azure.Storage.Blobs;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using StreamSpeechToText.Config;
using StreamSpeechToText.Models;
using StreamSpeechToText.Services.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StreamSpeechToText.Services
{
    public class SpeechToText
    {
        private SpeechConfig _speechConfig;
        private AutoDetectSourceLanguageConfig _languagesToDetect;
        private TaskCompletionSource<int> _stopRecognition;
        private string _currentAudioFile;

        private List<Speech> SpeechResult { get; set; }

        public SpeechToText(string[] languages)
        {
            _speechConfig = SpeechConfig.FromSubscription(Settings.AzureSpeech.KEY, Settings.AzureSpeech.REGION);
            _speechConfig.RequestWordLevelTimestamps();
            _languagesToDetect = AutoMaticLanguageDetection(languages);
            _stopRecognition = new TaskCompletionSource<int>();
        }

        /// <summary>
        /// Stream selected audio from Azure storage and return as "speech to text"
        /// </summary>
        /// <param name="audioFile">Name of audio-file</param>
        /// <param name="container">Azure storage container name</param>
        /// <returns>List<Speech></returns>
        public async Task<List<Speech>> RunRecognitionAsync(string audioFile, string container)
        {
            _currentAudioFile = audioFile;
            SpeechResult = new List<Speech>();

            var blobService = new BlobService();
            var blobClient = await blobService.GetBlobFromContainerAsync(audioFile, container);

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
            using (audioInputStream)
            {
                using (var stream = await blobStream.OpenReadAsync())
                {
                    ReadAudioStream(stream, audioInputStream);
                }
            }
        }

        private void ReadAudioStream(Stream stream, PushAudioInputStream inputStream)
        {
            switch (GetAudioFormat())
            {
                case AudioFormat.Mp3:
                    inputStream.Mp3(stream);
                    break;
                case AudioFormat.Opus:
                    inputStream.Opus(stream);
                    break;
                case AudioFormat.Wav:
                    inputStream.Wav(stream);
                    break;
            }
        }

        private AudioFormat GetAudioFormat()
        {
            var file = Path.GetExtension(_currentAudioFile).Trim('.');
            var extension = char.ToUpper(file[0]) + file.Substring(1).ToLower();

            var isValidFormat = Enum.TryParse(extension, out AudioFormat format);

            if (!isValidFormat)
            {
                throw new FileFormatException($"{extension} is an unsupported audio format.");
            }

            return format;
        }

        private void SessionStarted(object sender, SessionEventArgs e)
        {
            Console.WriteLine($"Speech session {e.SessionId} started\r\n");
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

        private void Recognizing(object sender, SpeechRecognitionEventArgs e)
        {
            Console.WriteLine($"Recognizing text: {e.Result.Text}");
        }

        private void Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            Console.WriteLine();
            var speech = GetResponse(e.Result.Properties);
            SpeechResult.Add(speech);
        }

        private Speech GetResponse(PropertyCollection properties)
        {
            string property = properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
            return JsonConvert.DeserializeObject<Speech>(property);
        }

        private AutoDetectSourceLanguageConfig AutoMaticLanguageDetection(string[] languagesToDetect)
            => AutoDetectSourceLanguageConfig.FromLanguages(languagesToDetect);

        /// <summary>
        /// Convert ticks to hh:mm:ss
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns>TimeSpan</returns>
        public static string TicksToTime(long ticks)
            => TimeSpan.FromTicks(ticks).ToString(@"hh\:mm\:ss\.fff");
    }
}