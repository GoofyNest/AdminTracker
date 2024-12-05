using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AdminTracker.Functions
{
    public class MostPlayedGameConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // The converter applies to lists of MostPlayedGame
            return objectType == typeof(List<MostPlayedGame>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Handle the single object case
            if (reader.TokenType == JsonToken.StartObject)
            {
                var singleGame = serializer.Deserialize<MostPlayedGame>(reader);
                return new List<MostPlayedGame> { singleGame };
            }
            // Handle the array case
            else if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<List<MostPlayedGame>>(reader);
            }
            // If neither, return null
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is List<MostPlayedGame> games)
            {
                if (games.Count == 1)
                {
                    // Serialize as a single object if there's only one game
                    serializer.Serialize(writer, games[0]);
                }
                else
                {
                    // Serialize as an array if there are multiple games
                    serializer.Serialize(writer, games);
                }
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
