using Newtonsoft.Json;
using System;
using System.IO;

namespace AdminTracker
{
    public class Config
    {
        public string steamPath { get; set; } = "C:\\Program Files (x86)\\Steam";
        public string gamePath { get; set; }

        public string configPath { get; set; }
        public string adminPath { get; set; }

        public bool useAdminCache { get; set; } = true;
        public bool useNewAdminTracker { get; set; } = true;

        public bool profileCrawler { get; set; } = false;

        public int cheaterHours { get; set; } = 1000;

        public static void Initialize(Config cfg)
        {
            Custom.WriteLine($"Starting configuration initization", ConsoleColor.DarkMagenta);

            Directory.CreateDirectory("config");

            cfg.configPath = Path.Combine("config", "config.json");
            cfg.adminPath = Path.Combine("config", "admin.json");

            if (!File.Exists("config/config.json"))
            {
                Custom.WriteLine($"Creating default configuration file", ConsoleColor.Magenta);

                File.WriteAllText(cfg.configPath, JsonConvert.SerializeObject(cfg, Formatting.Indented));

                Custom.WriteLine($"Configuration file created {cfg.configPath}", ConsoleColor.Cyan);
            }
            else
            {
                Custom.WriteLine($"Loading default configuration file", ConsoleColor.Magenta);

                cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(cfg.configPath));

                Custom.WriteLine($"Loaded {cfg.configPath}", ConsoleColor.Cyan);

                File.WriteAllText(cfg.configPath, JsonConvert.SerializeObject(cfg, Formatting.Indented));
            }

            if(!Directory.Exists(cfg.steamPath))
            {
                Custom.WriteLine($"steamPath incorrect, check {cfg.configPath}", ConsoleColor.Cyan);

                Program.killProgram = true;
                return;
            }

            if (!Directory.Exists(cfg.gamePath))
            {
                Custom.WriteLine($"gamePath incorrect, check {cfg.configPath}", ConsoleColor.Cyan);

                Program.killProgram = true;
                return;
            }

            Program._config = cfg;
        }
    }
}
