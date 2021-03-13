### Stream Opus audio to Azure's "Speech to Text" without GStreamer

Here's a C# demo on how to stream compressed Opus audio from Azure File Storage to Azure's Speech To Text and get timestamps and duration of each spoken word.

Opus is a totally open, royalty-free, highly versatile audio codec. Opus is unmatched for interactive speech and music transmission over the Internet, but is also intended for storage and streaming applications.

Unfortunately Azure Speech to Text only works with wav files and if you want to stream compressed audio like Opus, [Microsoft asks the programmer to install GStreamer](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/how-to-use-codec-compressed-audio-input-streams?tabs=debian&pivots=programming-language-csharp). That's all nice and dandy if you're building a desktop application, but if you want to publish your app online as a webservice, then things get way trickier. 

By using 2 Nuget packages, [Concentus](https://www.nuget.org/packages/Concentus/) and [Concentus.OggFile](https://www.nuget.org/packages/Concentus.OggFile/) (credit: Logan Stromberg) which can be used to extract or encode Opus packets, we can decode each streamed packet as wav and pass it on to Azure Speech to get the audio as text.

### Using the demo

1. Download and open *settings.cs* in the config folder and add your *connectionstring* and *key* to Azure File Storage and Azure Speech. ![Settings](https://nswardh.com/github/opusstream/settings.jpg)
4. Finally select an opus audio file, a blobcontainer and add languages you want to autodetect in *Main()*. ![Settings](https://nswardh.com/github/opusstream/blobcontainer.jpg)

### Demo Screenshot

![screenshot](https://nswardh.com/github/opusstream/screenshot.jpg)
