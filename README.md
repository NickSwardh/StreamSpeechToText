### Stream Mp3 and Opus audio to Azure's "Speech to Text" without GStreamer
Here's a C# demo on how to stream compressed audio like Mp3 and Opus from Azure File Storage to raw text format using Azure's Speech-To-Text service and get detailed information like timestamps and duration of each spoken word extracted from the audio file.

Azure Speech to Text only works with wav files and if you want to stream compressed audio like Mp3, Opus etc Microsoft recomments to install and use [GStreamer](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/how-to-use-codec-compressed-audio-input-streams?tabs=debian&pivots=programming-language-csharp). That's all nice and dandy if you're building a desktop application, but if you want to publish your app online as a webservice, then things get way trickier. 

By using Nuget packages, [Concentus](https://www.nuget.org/packages/Concentus/), [Concentus.OggFile](https://www.nuget.org/packages/Concentus.OggFile/) and [NAudio](https://github.com/naudio/NAudio) we can decode the compressed audio as wav and then stream it to Azure Speech to get the audio as text without the need of Gstreamer. Yay!

### Using the demo
1. Open *settings.cs* in the config folder and add your *connectionstring* for Azure File Storage and *key* + *region* for your Azure Speech service.
![Azure settings](https://nswardh.com/github/opusstream/settings.jpg)
3. Finally select an audio file, a blobcontainer and add languages you want to be extracted to text.
![Setup](https://nswardh.com/github/opusstream/blobcontainer.jpg)

### Demo Screenshot

![screenshot](https://nswardh.com/github/opusstream/screenshot.jpg)
