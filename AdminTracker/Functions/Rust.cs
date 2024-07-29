using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace AdminTracker
{
    public class Rust
    {
        public static List<Decrypt> oldPlayerList = new List<Decrypt>();

        public static List<Decrypt> PlayerList()
        {
            var cfg = Program._config;

            var coplayFile = Path.Combine(cfg.steamPath, "config", $"coplay_{Program.playerID}.vdf");

            if (!File.Exists(coplayFile))
            {
                Custom.WriteLine($"coplayFile not found coplay_{Program.playerID}.vdf", ConsoleColor.Red);
                return null;
            }

            DateTimeOffset LastWriteTime = File.GetLastWriteTimeUtc(coplayFile);

            // First program launch
            if(Program.lastWriteTime == 0)
            {
                Program.lastWriteTime = LastWriteTime.ToUnixTimeSeconds();
                return DumpVDF(coplayFile);
            }
            // Changes detected
            else if(Program.lastWriteTime != LastWriteTime.ToUnixTimeSeconds())
            {
                Program.lastWriteTime = LastWriteTime.ToUnixTimeSeconds();
                return DumpVDF(coplayFile);
            }
            return null;
        }

        public static List<Decrypt> DumpVDF(string coplayFile)
        {
            var steamIds = Decrypt.GenerateClass(coplayFile);

            var _coplay = Decrypt._coplay;

            oldPlayerList = _coplay;

            return _coplay;
        }

        public static void ReadLogFile()
        {
            var cfg = Program._config;

            var outputLog = Path.Combine(cfg.gamePath, "output_log.txt");

            if (!File.Exists(outputLog))
            {
                Custom.WriteLine($"outputLog not found {outputLog}", ConsoleColor.Red);
                return;
            }

            try
            {
                // Open the file with shared read access
                using (FileStream fileStream = new FileStream(outputLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("SteamID:"))
                        {
                            var parts = line.Split(new string[] { "SteamID:" }, StringSplitOptions.None);
                            if (parts.Length > 1)
                            {
                                var relevantPart = parts[1].Trim();
                                Custom.WriteLine(relevantPart);

                                var steamid = relevantPart.Split(' ')[0].Trim();
                                var name = relevantPart.Split('(')[1].Trim().Replace(")", "");

                                Program.playerID = steamid;
                                Program.playerName = name;

                                Console.Title = $"AdminTracker | {name} ({steamid})";

                                Custom.WriteLine($"Current account: {steamid} {name}", ConsoleColor.Magenta);
                                break;
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Custom.WriteLine("An error occurred while reading the file:", ConsoleColor.Red);
                Custom.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
}
