using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using AdminTracker.Classes;
using AdminTracker.Functions.Threads;

namespace AdminTracker
{
    internal class Program
    {
        public static string facepunchManifest = "https://api.facepunch.com/api/public/manifest/?public_key=j0VF6sNnzn9rwt9qTZtI02zTYK8PRdN1";

        public static Root adminRoot = new Root();
        public static List<Administrators> _admins = new List<Administrators>();
        public static List<AdminCache> _adminCache = new List<AdminCache>();
        public static List<Profile> _profiles = new List<Profile>();

        public static Config _config = new Config();

        public static bool killProgram = false;

        public static bool waitingRust = false;
        public static bool getSteamID = false;

        public static string playerID = "";
        public static string playerName = "";

        public static long lastWriteTime = 0;


        private static void ConsolePoolThread()
        {
            while(true)
            {
                Thread.Sleep(1);

                var line = Console.ReadLine();

                Console.WriteLine(line);
            }
        }

        private static void ResetThread()
        {
            while(true)
            {
                Thread.Sleep(60000);

                waitingRust = false;
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "AdminTracker | Bomb Facepunch";

            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    synth.SetOutputToDefaultAudioDevice();
                    synth.Speak($"Admintracker starting");
                }
            } 
            catch (Exception ex)
            {
                Custom.WriteLine($"WARNING!!!!!!!", ConsoleColor.Red);
                Custom.WriteLine($"SpeechSynthesizer is not working for you", ConsoleColor.Red);
                Custom.WriteLine($"You will not hear TTS when admin joins your server", ConsoleColor.Red);
                Custom.WriteLine($"{ex.ToString()}", ConsoleColor.Red);

            }

            new Thread(ConsolePoolThread).Start();

            Config.Initialize(_config);

            if(killProgram)
            {
                Custom.WriteLine($"Program stopped", ConsoleColor.Red);
                return;
            }

            Administrators.Initialize();

            if (killProgram)
            {
                Custom.WriteLine($"Program stopped", ConsoleColor.Red);
                return;
            }

            if(_config.profileCrawler)
                ProfileCrawler.Initialize(_profiles);

            new Thread(ResetThread).Start();
            
            while(true)
            {
                Thread.Sleep(100);

                if (!IsProcessRunning("RustClient"))
                {
                    if(!waitingRust)
                    {
                        Custom.WriteLine($"Waiting for Rust", ConsoleColor.Cyan);
                        waitingRust = true;
                    }
                    continue;
                }

                if (!getSteamID)
                {
                    Custom.WriteLine($"Getting steamid", ConsoleColor.Cyan);
                    
                    Rust.ReadLogFile();

                    if (Program.playerID != null)
                        getSteamID = true;

                    continue;
                }

                AdminCheck(Rust.PlayerList());


            }
        }

        private static async void AdminCheck(List<Decrypt> players)
        {
            if (players == null)
                return;

            if (players.Count <= 0)
                return;

            var profileUrls = new List<string>();

            Custom.WriteLine($"PlayerList v1 count {players.Count}", ConsoleColor.DarkMagenta);
            Custom.WriteLine($"AdminList count {_admins.Count}", ConsoleColor.DarkMagenta);

            for(var i = 0; i<players.Count; i++)
            {
                var player = players[i];

                profileUrls.Add($"https://steamcommunity.com/profiles/{player.SteamId}?xml=1");

                //Console.WriteLine(player);

                var adminIndex = _admins.FindIndex(m => m.steamID == player.SteamId);

                // No admins found
                if (adminIndex == -1)
                    continue;

                var _admin = _admins[adminIndex];

                if (player.Playtime > 0 && _config.useAdminCache)
                {
                    var adminCacheIndex = _adminCache.FindIndex(m => m.steamID == _admin.steamID);

                    if (adminCacheIndex > -1)
                    {
                        var _adminFromCache = _adminCache[adminCacheIndex];

                        var lastSeen = _adminFromCache.lastSeen;

                        if (lastSeen != player.Playtime)
                        {
                            using (SpeechSynthesizer synth = new SpeechSynthesizer())
                            {
                                synth.SetOutputToDefaultAudioDevice();

                                Custom.WriteLine($"Admin re-connected: [{_admin.staticName}], ({_admin.steamName}), {_admin.steamID}, {player.Playtime}", ConsoleColor.DarkYellow);
                                synth.Speak($"Admin re-connected: {_admin.staticName}");
                            }

                            _adminFromCache.lastSeen = player.Playtime;
                        }
                    }
                    else
                    {
                        _adminCache.Add(new AdminCache() { steamID = player.SteamId, lastSeen = player.Playtime });

                        Custom.WriteLine($"Admin found: [{_admin.staticName}], ({_admin.steamName}), {_admin.steamID}, {player.Playtime}", ConsoleColor.DarkYellow);

                        // Initialize a new instance of the SpeechSynthesizer.
                        using (SpeechSynthesizer synth = new SpeechSynthesizer())
                        {
                            synth.SetOutputToDefaultAudioDevice();

                            synth.Speak($"Admin found: {_admin.staticName}");
                        }
                    }
                }
                else
                {
                    Custom.WriteLine($"Admin found: [{_admin.staticName}], ({_admin.steamName}), {_admin.steamID}, {player.Playtime}", ConsoleColor.DarkYellow);

                    // Initialize a new instance of the SpeechSynthesizer.
                    using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    {
                        synth.SetOutputToDefaultAudioDevice();

                        synth.Speak($"Admin found: {_admin.staticName}");
                    }
                }
            }

            // New admin tracker version
            if(File.Exists("exported-cookies.json") && _config.useNewAdminTracker)
            {
                await Task.Run(async () =>
                {
                    await RecentPlayedWith.FindAdminsV2();
                });
            }

            // Use Task.Run to start the FetchMultipleWithConcurrencyAsync method asynchronously
            if (_config.profileCrawler)
            {
                await Task.Run(async () =>
                {
                    await ProfileCrawler.FetchMultipleWithConcurrencyAsync(profileUrls, 5);
                });
            }
        }

        static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            return processes.Length > 0;
        }
    }
}
