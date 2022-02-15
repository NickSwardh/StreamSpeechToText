using Concentus.Oggfile;
using Concentus.Structs;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System;
using System.IO;

namespace StreamSpeechToText.Services.Extensions
{
    public static class AudioExtensions
    {
        /// <summary>
        /// Read Wav into Speech-To-Text
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="stream"></param>
        public static void Wav(this PushAudioInputStream inputStream, Stream stream)
        {
            byte[] buffer = new byte[2048];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                inputStream.Write(buffer, bytesRead);
            }
        }

        /// <summary>
        /// Read Mp3 into Speech-To-Text
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="stream"></param>
        public static void Mp3(this PushAudioInputStream inputStream, Stream stream)
        {
            var mp3 = new Mp3FileReader(stream);

            byte[] buffer = new byte[2048];
            int bytesRead;

            while ((bytesRead = mp3.Read(buffer, 0, buffer.Length)) > 0)
            {
                inputStream.Write(buffer, bytesRead);
            }
        }

        /// <summary>
        /// Read Opus into Speech-To-Text
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="stream"></param>
        public static void Opus(this PushAudioInputStream inputStream, Stream stream)
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
                        inputStream.Write(bytes, bytes.Length);
                    }
                }
            }
        }
    }
}
