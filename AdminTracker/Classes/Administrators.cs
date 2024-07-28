using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace AdminTracker
{
    public class Administrators
    {
        public string steamName { get; set; }
        public string steamID { get; set; }
        public string staticName { get; set; }
        public long lastSeen { get; set; } = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        public static void Initialize()
        {
            try
            {
                Custom.WriteLine("Attempting to query Facepunch manifest", ConsoleColor.Cyan);

                using (var _request = new WebClient())
                {
                    var jsonResponse = _request.DownloadString(Program.facepunchManifest);

                    Program.adminRoot = JsonConvert.DeserializeObject<Root>(jsonResponse);

                    Custom.WriteLine("Download success, attempting to BuildList", ConsoleColor.Green);

                    BuildList();
                }
            }
            catch(Exception ex) 
            {
                Custom.WriteLine("Error quering Facepunch manifest data", ConsoleColor.Red);
                Custom.WriteLine(ex.ToString(), ConsoleColor.Red);
                Custom.WriteLine("Recommend closing program or using local backup", ConsoleColor.Red);

                Program.killProgram = true;
            }
        }

        public static void BuildList()
        {
            var _config = Program._config;

            if (File.Exists(_config.adminPath))
                Program._admins = JsonConvert.DeserializeObject<List<Administrators>>(File.ReadAllText(_config.adminPath));

            List<string> steamIds = new List<string>();

            if(Program._admins.Count == 0)
            {
                steamIds = Program.adminRoot.Admins.Select(admin => admin.UserId).ToList();
            }
            else
            {
                var existingUserIds = new HashSet<string>(Program._admins.Select(admin => admin.steamID));
                steamIds = Program.adminRoot.Admins
                    .Select(admin => admin.UserId)
                    .Where(userId => !existingUserIds.Contains(userId))
                    .ToList();
            }

            int chunkSize = 100;
            for (int i = 0; i < steamIds.Count; i += chunkSize)
            {
                var chunk = steamIds.Skip(i).Take(chunkSize);
                string joinedSteamIds = string.Join(",", chunk);

                var playerSummaries = Steam.GetPlayerSummaries(joinedSteamIds);

                if (playerSummaries == "error")
                {
                    Custom.WriteLine("Error parsing admin summaries", ConsoleColor.Red);
                    Program.killProgram = true;
                    return;
                }

                JObject o = JObject.Parse(playerSummaries);

                var _summaries = new List<Steam>();
                try
                {
                    _summaries = JsonConvert.DeserializeObject<List<Steam>>(o.SelectToken("response").SelectToken("players").ToString());
                }
                catch (Exception ex)
                {
                    Custom.WriteLine("Error parsing admin summaries", ConsoleColor.Red);
                    Custom.WriteLine($"{ex.ToString()}", ConsoleColor.Red);
                    return;
                }

                foreach (var admin in _summaries)
                {
                    Administrators tempAdmin = new Administrators();

                    tempAdmin.steamID = admin.steamid;
                    tempAdmin.steamName = admin.personaname;
                    tempAdmin.staticName = admin.personaname;

                    Program._admins.Add(tempAdmin);
                }
            }

            File.WriteAllText(_config.adminPath, JsonConvert.SerializeObject(Program._admins, Formatting.Indented));
        }
    }

    public class Admins
    {
        public string UserId { get; set; }
        public string Level { get; set; }
    }

    public class Root
    {
        [JsonProperty("Administrators")]
        public List<Admins> Admins { get; set; }
    }

}
