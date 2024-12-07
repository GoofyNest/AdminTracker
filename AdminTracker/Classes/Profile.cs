using System.Collections.Generic;
using AdminTracker.Functions;
using Newtonsoft.Json;

namespace AdminTracker
{
    public class RootObject
    {
        [JsonProperty("profile")]
        public Profile profile { get; set; }
    }

    public class Profile
    {
        public string steamID64 { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string steamID { get; set; }

        public string onlineState { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string stateMessage { get; set; }

        public string privacyState { get; set; }
        public int visibilityState { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string avatarIcon { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string avatarMedium { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string avatarFull { get; set; }

        public int vacBanned { get; set; }
        public string tradeBanState { get; set; }
        public int isLimitedAccount { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string customURL { get; set; }

        public string memberSince { get; set; }
        public string steamRating { get; set; }
        public string hoursPlayed2Wk { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string headline { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string location { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string realname { get; set; }

        [JsonConverter(typeof(CDataJsonConverter))] 
        public string summary { get; set; }

        [JsonProperty("mostPlayedGames")]
        public MostPlayedGames MostPlayedGames { get; set; }
    }

    public class MostPlayedGame
    {
        [JsonProperty("gameName")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string GameName { get; set; }

        [JsonProperty("gameLink")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string GameLink { get; set; }

        [JsonProperty("gameIcon")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string GameIcon { get; set; }

        [JsonProperty("gameLogo")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string GameLogo { get; set; }

        [JsonProperty("gameLogoSmall")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string GameLogoSmall { get; set; }

        [JsonProperty("gameJoinLink")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string GameJoinLink { get; set; }

        [JsonProperty("hoursPlayed")]
        public double HoursPlayed { get; set; }

        [JsonProperty("hoursOnRecord")]
        public double HoursOnRecord { get; set; }

        [JsonProperty("statsName")]
        [JsonConverter(typeof(CDataJsonConverter))]
        public string StatsName { get; set; }
    }

    public class MostPlayedGames
    {
        [JsonProperty("mostPlayedGame")]
        [JsonConverter(typeof(MostPlayedGameConverter))]
        public List<MostPlayedGame> MostPlayedGameList { get; set; }
    }
}
