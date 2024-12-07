using System;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdminTracker.Classes;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace AdminTracker.Functions.Threads
{
    public class RecentPlayedWith
    {
        public static bool doThisOnce = false;

        public static async Task FindAdminsV2()
        {
            // Load the cookies from the JSON file
            var cookies = LoadCookies("exported-cookies.json");

            // Create an HttpClientHandler to add cookies to the request
            var handler = new HttpClientHandler();
            handler.CookieContainer.Add(new Uri("https://steamcommunity.com"), cookies);

            // Create an HttpClient using the handler with cookies
            using (var client = new HttpClient(handler))
            {
                try
                {
                    // Set up the User-Agent header to mimic a real browser
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    // Make the request to the Steam Coplay page
                    var response = await client.GetAsync("https://steamcommunity.com/profiles/"+ Program.playerID + "/friends/coplay");

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();

                        // Parse the HTML content to extract SteamIDs
                        ParseSteamIDs(content);
                    }
                    else
                    {
                        Custom.WriteLine("Request failed with status: " + response.StatusCode, ConsoleColor.DarkMagenta);
                    }
                }
                catch (Exception ex)
                {
                    Custom.WriteLine("Error: " + ex.Message, ConsoleColor.DarkMagenta);
                }

                await Task.Delay(0);
            }
        }

        static System.Net.CookieCollection LoadCookies(string filePath)
        {
            var cookieCollection = new System.Net.CookieCollection();

            // Read the JSON file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON into a list of cookies using Newtonsoft.Json
            var cookies = JsonConvert.DeserializeObject<CookieItem[]>(json);

            foreach (var cookie in cookies)
            {
                try
                {
                    string name = cookie.Name;
                    string value = cookie.Value;
                    string domain = cookie.Domain;
                    string path = cookie.Path;
                    bool secure = cookie.Secure;
                    bool httpOnly = cookie.HttpOnly;

                    // Skip cookies with invalid values (e.g., timezoneOffset with '3600,0')
                    if (string.IsNullOrEmpty(value) || value.Contains(","))
                    {
                        //Custom.WriteLine($"Skipping invalid cookie: {name} = {value}", ConsoleColor.DarkMagenta);
                        continue;
                    }

                    var cookieObj = new System.Net.Cookie(name, value, path, domain)
                    {
                        Secure = secure,
                        HttpOnly = httpOnly
                    };

                    cookieCollection.Add(cookieObj);
                }
                catch (Exception ex)
                {
                    Custom.WriteLine($"Error adding cookie {cookie.Name}: {ex.Message}", ConsoleColor.DarkMagenta);
                }
            }

            return cookieCollection;
        }

        static void ParseSteamIDs(string htmlContent)
        {
            // Load the HTML content using HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Select the first div with class "coplayGroup"
            var coplayGroupNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'coplayGroup')]");

            if (coplayGroupNode != null)
            {
                // Get the inner HTML of the first coplayGroup
                string coplayGroupHtml = coplayGroupNode.InnerHtml;

                // Regex pattern to match data-steamid values
                string pattern = @"data-steamid=""(765611\d+)""";

                // Use Regex to find all Steam IDs in the inner HTML
                var matches = Regex.Matches(coplayGroupHtml, pattern);

                //Console.WriteLine("Steam IDs found:");

                Custom.WriteLine($"PlayerList v2 count {matches.Count}", ConsoleColor.DarkMagenta);

                // Loop through all matches and print the Steam IDs
                foreach (Match match in matches)
                {
                    string steamID = match.Groups[1].Value;

                    var adminIndex = Program._admins.FindIndex(m => m.steamID == steamID);

                    if(adminIndex > -1)
                    {
                        var _admin = Program._admins[adminIndex];

                        using (SpeechSynthesizer synth = new SpeechSynthesizer())
                        {
                            synth.SetOutputToDefaultAudioDevice();

                            Custom.WriteLine($"Admin: [{_admin.staticName}], ({_admin.steamName}), {_admin.steamID}", ConsoleColor.DarkYellow);
                            synth.Speak($"Admin found: {_admin.staticName}");
                        }
                    }

                    //Console.WriteLine(steamID);
                }
            }
            else
            {
                Console.WriteLine("No coplayGroup div found in the HTML.");
            }
        }
    }
}
