using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AdminTracker.Functions.Threads
{
    public class ProfileCrawler
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string CacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", "steam"); // Better handling of directory path

        // This method is used to initialize your profiles from existing cache files
        public static void Initialize(List<Profile> profiles)
        {
            var _cfg = Program._config;
            try
            {
                // Check if the cache directory exists
                if (Directory.Exists(CacheDirectory))
                {
                    // Get all XML files from the cache directory
                    string[] xmlFiles = Directory.GetFiles(CacheDirectory, "*.xml");

                    // Loop through each file and load it into the profiles list
                    foreach (var filePath in xmlFiles)
                    {
                        string steamId64 = Path.GetFileNameWithoutExtension(filePath);

                        // Convert the XML content to a Profile object
                        Profile profile = ConvertXmlToProfile(filePath);

                        if (profile != null)
                        {
                            profiles.Add(profile); // Add the profile to the list

                            //WarnForCheater(profile);
                        }
                    }

                    File.WriteAllText(Path.Combine("config", "profiles.json"), JsonConvert.SerializeObject(profiles, Newtonsoft.Json.Formatting.Indented));
                }
                else
                {
                    Custom.WriteLine("Cache directory not found. No profiles were loaded.", ConsoleColor.DarkMagenta);
                }
            }
            catch (Exception ex)
            {
                Custom.WriteLine($"Error initializing profiles from cache: {ex.Message}", ConsoleColor.DarkMagenta);
            }
        }

        // Method to fetch XML from URL and update Program._profiles list
        public static async Task<Profile> FetchXmlAsync(string url, string steamId64)
        {
            try
            {
                string filePath = Path.Combine(CacheDirectory, $"{steamId64}.xml");

                // Check if the file exists and if it's older than a week
                if (File.Exists(filePath))
                {
                    DateTime lastModified = File.GetLastWriteTime(filePath);

                    if ((DateTime.Now - lastModified).Days <= 7)
                        return ConvertXmlToProfile(filePath); // Load profile from file if it's within a week
                }

                // If the file doesn't exist or is outdated, make the web request
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string xmlContent = await response.Content.ReadAsStringAsync();

                // Save the XML to disk
                SaveXmlToDisk(filePath, xmlContent);

                // Parse the XML and create/update the Profile
                Profile profile = ConvertXmlToProfile(filePath);

                // Update or add the profile to the list
                UpdateOrAddProfile(profile);

                return profile;
            }
            catch (Exception ex)
            {
                Custom.WriteLine($"Error fetching XML from {url}: {ex.Message}", ConsoleColor.DarkMagenta);
                return null;
            }
        }

        // Method to save XML content to disk asynchronously
        private static void SaveXmlToDisk(string filePath, string xmlContent)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));  // Ensure directory exists
                File.WriteAllText(filePath, xmlContent, Encoding.UTF8); // Asynchronous file writing
            }
            catch (Exception ex)
            {
                Custom.WriteLine($"Error saving XML to {filePath}: {ex.Message}", ConsoleColor.DarkMagenta);
            }
        }

        // Method to fetch multiple profiles with concurrency
        public static async Task FetchMultipleWithConcurrencyAsync(List<string> urls, int maxConcurrentRequests)
        {
            using SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrentRequests);

            List<Task> tasks = new List<Task>();

            foreach (var url in urls)
            {
                await concurrencySemaphore.WaitAsync(); // Limit concurrency

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string steamId64 = ExtractSteamId64FromUrl(url);
                        if (steamId64 != null)
                        {
                            Profile result = await FetchXmlAsync(url, steamId64);
                        }
                    }
                    finally
                    {
                        concurrencySemaphore.Release(); // Release the semaphore
                    }
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }

        // Extract the SteamID64 from the URL using regex
        private static string ExtractSteamId64FromUrl(string url)
        {
            var match = Regex.Match(url, @"https:\/\/steamcommunity\.com\/profiles\/(\d+)\?xml=1");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static void WarnForCheater(Profile profile)
        {
            var _cfg = Program._config;
            
            if (profile.isLimitedAccount == 1)
            {
                Custom.WriteLine($"{profile.steamID}({profile.steamID64}) is a G2G.com account", ConsoleColor.Yellow);
            }
            else
            {
                if (profile.MostPlayedGames == null)
                    return;

                if (profile.MostPlayedGames.MostPlayedGameList == null)
                    return;

                var gameIndex = profile.MostPlayedGames.MostPlayedGameList.FindIndex(m => m.GameName == "Rust");

                if (gameIndex > -1)
                {
                    var game = profile.MostPlayedGames.MostPlayedGameList[gameIndex];

                    if(game.HoursOnRecord < _cfg.cheaterHours)
                        Custom.WriteLine($"{profile.steamID}({profile.steamID64}) low hours {game.HoursOnRecord}", ConsoleColor.Yellow);
                }
            }
        }

        // Method to update or add the profile to Program._profiles
        private static void UpdateOrAddProfile(Profile profile)
        {
            var existingProfile = Program._profiles.FirstOrDefault(p => p.steamID64 == profile.steamID64);
            if (existingProfile != null)
            {
                // Update the existing profile with new data
                existingProfile = profile; // Or selectively update fields

                WarnForCheater(profile);

            }
            else
            {
                // Add the new profile
                Program._profiles.Add(profile);

                WarnForCheater(profile);
            }
        }

        // Helper method to convert XML content to a Profile object using JSON
        private static Profile ConvertXmlToProfile(string filePath)
        {
            var xmlNode = new XmlDocument();
            try
            {
                // Load XML content into XmlDocument
                xmlNode.Load(filePath);  // Alternatively, use xmlNode.Load(filePath) for file-based loading

                // Extract the steamID64 value from the XML
                XmlNode steamID64Node = xmlNode.SelectSingleNode("//steamID64");

                var steamid = ""; // Debugging purposes

                if (steamID64Node != null)
                {
                    steamid = steamID64Node.InnerText;
                }

                // Convert XML node to JSON string
                var json = JsonConvert.SerializeXmlNode(xmlNode);

                try
                {
                    // Deserialize the root object first
                    var jsonProfile = JsonConvert.DeserializeObject<RootObject>(json);

                    // Extract the profile part from the JSON
                    var profile = jsonProfile.profile;

                    return profile;
                }
                catch(Exception ex)
                {
                    File.WriteAllText($"{steamid}.json", json);

                    Custom.WriteLine($"Error converting XML to Profile: {filePath} {ex.Message}", ConsoleColor.DarkMagenta);
                    return null;
                }

            }
            catch (Exception ex)
            {
                // Log or handle error
                Custom.WriteLine($"Unknown XML error {ex.Message}", ConsoleColor.DarkMagenta);
                return null;
            }
        }
    }
}
