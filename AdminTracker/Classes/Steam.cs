using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdminTracker
{
    public class Steam
    {
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public int commentpermission { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public string avatarhash { get; set; }
        public int lastlogoff { get; set; }
        public int personastate { get; set; }
        public string realname { get; set; }
        public string primaryclanid { get; set; }
        public int timecreated { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
        public string locstatecode { get; set; }
        public int loccityid { get; set; }

        // Game details
        public ulong gameid { get; set; } = 0;
        public string gameserverip { get; set; } = "";
        public string gameextrainfo { get; set; } = "";

        public static string GetPlayerSummaries(string steamids)
        {
            try
            {
                using (var client = new WebClient())
                {
                    //client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
                    return client.DownloadString("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=0A4234951FB3626FD452EEC83C3B92BC&steamids=" + steamids);
                }
            }
            catch (Exception ex)
            {
                Custom.WriteLine("GetPlayerSummaries error", ConsoleColor.Red);
                Custom.WriteLine(ex.Message, ConsoleColor.Red);
                return "error";
            }
        }

        public static string GetPlayerBans(string steamids)
        {
            try
            {
                using (var client = new WebClient())
                {
                    //client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
                    return client.DownloadString("https://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key=0A4234951FB3626FD452EEC83C3B92BC&steamids=" + steamids);
                }
            }
            catch (Exception ex)
            {
                Custom.WriteLine("GetPlayerBans error", ConsoleColor.Red);
                Custom.WriteLine(ex.Message, ConsoleColor.Red);
                return "error";
            }
        }
    }
}
