using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdminTracker
{
    public class Decrypt
    {
        public static List<Decrypt> _coplay = new List<Decrypt>();

        public string steamid { get; set; }
        public long gameid { get; set; }
        public long playtime { get; set; }
        public int currentgame { get; set; } = 0;

        public static string GenerateClass(string path, string steamid = "", string accuracy = "3")
        {
            _coplay = new List<Decrypt>();

            var steamids = "";

            if (!File.Exists(path))
                return string.Empty;

            var hex = ReadFileAsHex(path);

            var index = 0;

            foreach (var steamId in Decrypt.GetUlongs(hex, "SteamID"))
            {
                var alreadyExist = _coplay.FindIndex(m => m.steamid == steamId.ToString());

                if (alreadyExist > -1)
                    continue;

                if (string.IsNullOrEmpty(steamid) == false && accuracy != "3")
                {
                    if (steamId.ToString() == steamid)
                        _coplay.Add(new Decrypt() { steamid = steamId.ToString(), currentgame = 1 });
                    else
                        _coplay.Add(new Decrypt() { steamid = steamId.ToString(), currentgame = 0 });
                }
                else
                    _coplay.Add(new Decrypt() { steamid = steamId.ToString() });

                steamids += steamId + ",";
            }

            var count = _coplay.Count;
            var _temp = _coplay.FindLast(m => m.steamid == steamid);

            foreach (var playtime in Decrypt.GetUints(hex, "Playtime"))
            {
                if (index > count)
                    break;

                if (string.IsNullOrEmpty(steamid) == false && _temp != null && accuracy != "3")
                {
                    if (playtime == _temp.playtime)
                        _coplay[index].currentgame = 1;
                }

                _coplay[index].playtime = playtime;
                index++;
            }

            index = 0;
            foreach (var gameid in Decrypt.GetUints(hex, "gameid"))
            {
                if (index > count)
                    break;

                _coplay[index].gameid = gameid;
                index++;
            }
            return steamids;
        }


        public static List<UInt32> GetUints(string input, string field = "Playtime", bool useFilePath = false)
        {
            List<string> tempList = new List<string>();

            if (useFilePath)
                tempList = FindStringInHex(ReadFileAsHex(input), field, 4);
            else
                tempList = FindStringInHex(input, field, 4);

            List<UInt32> dataOut = new List<UInt32>();

            foreach (var item in tempList)
                dataOut.Add(HexToUint(item, true));

            return dataOut;
        }

        public static List<ulong> GetUlongs(string input, string field = "SteamID", bool useFilePath = false)
        {
            List<string> tempList = new List<string>();

            if (useFilePath)
                tempList = FindStringInHex(ReadFileAsHex(input), field, 8);
            else
                tempList = FindStringInHex(input, field, 8);

            List<ulong> dataOut = new List<ulong>();

            foreach (var item in tempList)
                dataOut.Add(HexToUlong(item, true));

            return dataOut;
        }

        public static List<string> FindStringInHex(string input, string searchString, int bytesToRead)
        {
            List<string> endResultHex = new List<string>();
            try
            {
                //List<ulong> endResult = new List<ulong>();
                searchString = StringToHex(searchString);
                int searchStringLength = searchString.Length;
                int indexOfSearchString = input.IndexOf(searchString);
                while (indexOfSearchString != -1)
                {
                    string outputHex = input.Substring(indexOfSearchString + searchStringLength + 2, bytesToRead * 2);
                    //Console.WriteLine(Utils.Utils.HexToUlong(outputHex, true));
                    //endResult.Add(Utils.Utils.HexToUlong(outputHex, true));

                    //Console.WriteLine(outputHex);
                    endResultHex.Add(outputHex);
                    indexOfSearchString = input.IndexOf(searchString, indexOfSearchString + searchStringLength + 2 + bytesToRead * 2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception at FindStringInHex() - {ex}");
            }

            return endResultHex;
        }

        public static string ReadFileAsHex(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    byte[] fileBytes = binaryReader.ReadBytes((int)fileStream.Length);

                    return BitConverter.ToString(fileBytes).Replace("-", "");
                }
            }
        }

        public static string SplitInParts(string input, uint length)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                output += input[i];
                if (i % length == 1)
                {
                    output += " ";
                }
            }
            return output;
        }

        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }


        public static string StringToHex(string input, bool splitBytes = false)
        {
            byte[] bytes = Encoding.Default.GetBytes(input);

            string hexString = ByteArrayToHexString(bytes);

            if (splitBytes)
            {
                return SplitInParts(hexString, 2);
            }

            return hexString;
        }

        public static ulong HexToUlong(string input, bool reverseEndianness = false)
        {
            ulong output = Convert.ToUInt64(input, 16);
            if (reverseEndianness)
            {
                return BinaryPrimitives.ReverseEndianness(output);
            }

            return output;
        }

        public static uint HexToUint(string input, bool reverseEndianness = false)
        {
            uint output = Convert.ToUInt32(input, 16);
            if (reverseEndianness)
            {
                return BinaryPrimitives.ReverseEndianness(output);
            }

            return output;
        }
    }
}
