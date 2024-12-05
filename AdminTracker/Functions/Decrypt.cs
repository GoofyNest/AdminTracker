using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdminTracker
{
    public class Decrypt
    {
        public static List<Decrypt> Coplay { get; set; } = new List<Decrypt>();

        public string SteamId { get; set; } = string.Empty;
        public long GameId { get; set; }
        public long Playtime { get; set; }
        public int CurrentGame { get; set; }

        public override string ToString() =>
            $"SteamId: {SteamId}, GameId: {GameId}, Playtime: {Playtime}, CurrentGame: {CurrentGame}";

        public static string GenerateClass(string path, string targetSteamId = "", string accuracy = "3")
        {
            if (!File.Exists(path))
                return string.Empty;

            string hexContent = ReadFileAsHex(path);

            // Populate Coplay list
            Coplay = ExtractSteamIds(hexContent, targetSteamId, accuracy);
            AssignPlaytimes(hexContent, targetSteamId, accuracy);
            AssignGameIds(hexContent);

            return string.Join(",", Coplay.Select(c => c.SteamId));
        }

        private static List<Decrypt> ExtractSteamIds(string hex, string targetSteamId, string accuracy)
        {
            var ids = GetUlongs(hex, "SteamID");
            var uniqueIds = new HashSet<ulong>(ids);

            return uniqueIds.Select(id => new Decrypt
            {
                SteamId = id.ToString(),
                CurrentGame = (!string.IsNullOrEmpty(targetSteamId) && accuracy != "3" && id.ToString() == targetSteamId) ? 1 : 0
            }).ToList();
        }

        private static void AssignPlaytimes(string hex, string targetSteamId, string accuracy)
        {
            var playtimes = GetUints(hex, "Playtime");
            for (int i = 0; i < playtimes.Count && i < Coplay.Count; i++)
            {
                var coplayEntry = Coplay[i];
                coplayEntry.Playtime = playtimes[i];
                if (!string.IsNullOrEmpty(targetSteamId) && accuracy != "3" && coplayEntry.SteamId == targetSteamId)
                {
                    coplayEntry.CurrentGame = 1;
                }
            }
        }

        private static void AssignGameIds(string hex)
        {
            var gameIds = GetUints(hex, "gameid");
            for (int i = 0; i < gameIds.Count && i < Coplay.Count; i++)
            {
                Coplay[i].GameId = gameIds[i];
            }
        }

        public static List<uint> GetUints(string input, string field) =>
            ExtractHexValues(input, field, 4)
                .Select(hex => HexToUint(hex, true))
                .ToList();

        public static List<ulong> GetUlongs(string input, string field) =>
            ExtractHexValues(input, field, 8)
                .Select(hex => HexToUlong(hex, true))
                .ToList();

        private static List<string> ExtractHexValues(string hex, string searchString, int byteSize)
        {
            var results = new List<string>();
            try
            {
                string searchHex = StringToHex(searchString);
                int index = hex.IndexOf(searchHex, StringComparison.Ordinal);
                while (index != -1)
                {
                    int valueStart = index + searchHex.Length + 2;
                    results.Add(hex.Substring(valueStart, byteSize * 2));
                    index = hex.IndexOf(searchHex, valueStart + byteSize * 2, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExtractHexValues: {ex.Message}");
            }
            return results;
        }

        public static string ReadFileAsHex(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            byte[] fileBytes = binaryReader.ReadBytes((int)fileStream.Length);
            return ByteArrayToHexString(fileBytes);
        }

        public static string ByteArrayToHexString(byte[] bytes) =>
            string.Concat(bytes.Select(b => b.ToString("X2")));

        public static string StringToHex(string input) =>
            string.Concat(Encoding.Default.GetBytes(input).Select(b => b.ToString("X2")));

        public static ulong HexToUlong(string hex, bool reverseEndianness = false)
        {
            ulong value = Convert.ToUInt64(hex, 16);
            return reverseEndianness ? ReverseEndianness(value) : value;
        }

        public static uint HexToUint(string hex, bool reverseEndianness = false)
        {
            uint value = Convert.ToUInt32(hex, 16);
            return reverseEndianness ? ReverseEndianness(value) : value;
        }

        private static ulong ReverseEndianness(ulong value) =>
            BinaryPrimitives.ReverseEndianness(value);

        private static uint ReverseEndianness(uint value) =>
            BinaryPrimitives.ReverseEndianness(value);
    }
}