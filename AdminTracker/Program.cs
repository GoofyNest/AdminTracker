using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdminTracker
{
    internal class Program
    {
        public static string facepunchManifest = "https://api.facepunch.com/api/public/manifest/?public_key=j0VF6sNnzn9rwt9qTZtI02zTYK8PRdN1";

        public static Root adminRoot = new Root();
        public static List<Administrators> _admins = new List<Administrators>();

        public static Config _config = new Config();

        public static bool killProgram = false;

        public static bool waitingRust = false;
        public static bool getSteamID = false;

        public static string playerID = "";
        public static string playerName = "";

        public static DateTimeOffset lastWriteTime = new DateTimeOffset();

        public static long lastAdminThing = -1231;

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
            Console.Title = "AdminTracker | Bomb Facepunch";

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

                var playerList = Rust.PlayerList();

                if(playerList != null)
                    AdminCheck(playerList);
            }
        }

        private static void AdminCheck(List<Decrypt> players)
        {
            foreach(var player in players)
            {
                var adminIndex = _admins.FindIndex(m => m.steamID == player.steamid);

                if(adminIndex > -1)
                {
                    var _admin = _admins[adminIndex];

                    Custom.WriteLine($"Admin found: [{_admin.staticName}], ({_admin.steamName}), {_admin.steamID}", ConsoleColor.DarkYellow);

                    // Initialize a new instance of the SpeechSynthesizer.
                    using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    {
                        synth.SetOutputToDefaultAudioDevice();

                        synth.Speak($"Admin found: {_admin.staticName}");
                    }
                }
            }
        }

        static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            return processes.Length > 0;
        }
    }
}
