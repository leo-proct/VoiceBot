using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.API;
using Discord.Commands;
using Discord.Audio;
using System.Diagnostics;
using System.IO;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace ConsoleApplication1
{
    class Program
    {
        private string[] files = Directory.GetFiles(".../Sounds");
        private static DiscordClient client;
        private IAudioClient audio;


        private static readonly string AppKey = "MTc1OTQ0NTg0NTE2MzM3NjY1.CgY0qg.Y2VMmtQpGCR0DrnYlLvFQLqSDUQ";
        static void Main(string[] args) => new Program().Start(args);
        public void Start(string[] args)
        {
            client = new DiscordClient()
                .UsingAudio(audio =>
                {
                    audio.Mode = AudioMode.Outgoing;
                })
                .UsingModules();
            client.MessageReceived += async (source, e) =>
            {
                if (e.Message.Text.Contains("Hello Discord Bot"))
                {
                    await e.Channel.SendMessage("Hello");
                }

            };

            client.UsingCommands(input =>
            {
                input.PrefixChar = '$';
                input.AllowMentionPrefix = true;

            });
            var commands = client.GetService<CommandService>();
            commands.CreateCommand("voice")
                .Parameter("selected", ParameterType.Required)
                .Do(async (e) =>
                {
                    var dave = e.GetArg("selected");
                    int position = 0;
                    try
                    {
                        position = Convert.ToInt32(dave);
                    }
                    catch
                    {
                        position = 0;
                    }

                    var voiceChannel = e.Server.VoiceChannels.FirstOrDefault();
                    
                    if (voiceChannel != null)
                    {
                        audio = await voiceChannel.JoinAudio();
                        switch(audio.State)
                        {
                            case ConnectionState.Connected:
                                break;
                        }
                        var _vClient = await client.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
         .Join(voiceChannel);
                        SendAudio(_vClient, client, files[position]);
                        
                        await voiceChannel.LeaveAudio();
                    }
        });
            commands.CreateCommand("list").Do(async (e) =>
            {
                string output = "";
                int counter = 0;
                foreach (string files in files)
                {
                    output += "(" + counter + ") : " + files + "\n";
                    counter++;
                }
                await e.Channel.SendMessage("```" + output + "```");


            });
                client.ServerAvailable += (s, e) =>
            {
                Console.WriteLine($"Server \"{e.Server.Name}\" Is Online");
            };
            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MjcxMzgwOTYxMzY5Nzg0MzIx.C2Fm9g.k8qMJwKkjqaqSrjI - t7hG1P9WwA", TokenType.Bot);
                await client.WaitForServer().ConfigureAwait(false);
                //client.AddModule<Modules.Links>();  // Use for later tutorials
            });
        }
        public void SendAudio(Discord.Audio.IAudioClient _vClient, Discord.DiscordClient _client, string filePath)
        {
            try
            {
                var channelCount = _client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        audio.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }
                    audio.Wait();
                }
            }
            catch
            {

            }
        }
    }
    public static class DiscordClientExtensions
    {
        public static async Task WaitForServer(this DiscordClient client)
        {
            var delay = 3000;
            Console.WriteLine($"Waiting{delay / 1000} for servers");
            await Task.Delay(delay);
            if (client.Servers == null || client.Servers.Count() == 0) throw new TimeoutException("No Servers Found");
        }
    }
}
