
using System.Text.RegularExpressions;

namespace BlogA.DAL
{
    public class Helper
    {
        private string[] splitCon { get; set; }
        private Dictionary<string, string> _wordToCon { get; set; } = new();
        private Dictionary<string, string> _conToWord { get; set; } = new();

        private string[] splitConReader { get; set; }
        private Dictionary<string, string> _wordToConReader { get; set; } = new();
        private Dictionary<string, string> _conToWordReader { get; set; } = new();

        private string _versionA = "A";
        private string _versionB = "B";

        public Helper(string words,string convertors, string wordsReader, string convertorsReader)
        {
            //Admin
            string[] splitWords = words.Split(',');
            splitCon = convertors.Split(',');
            for (int i = 0; i < splitCon.Length; i++)
            {
                _wordToCon[splitWords[i]] = splitCon[i];
                _conToWord[splitCon[i]] = splitWords[i];
            }

            //Client/Reader
            string[] splitWordsReader = wordsReader.Split(',');
            splitConReader = convertorsReader.Split(',');
            for (int i = 0; i < splitConReader.Length; i++)
            {
                _wordToConReader[splitWordsReader[i]] = splitConReader[i];
                _conToWordReader[splitConReader[i]] = splitWordsReader[i];
            }
        }

        public string VA => _versionA;
        public string VB => _versionB;

        public string ConvertWordToCon(string wording)
        {

            string[] allChar = wording.Select(c => c.ToString()).ToArray();
            string result = "start";

            for (int i = 0; i < allChar.Length; i++)
            {
                if (_wordToCon.ContainsKey(allChar[i]))
                {
                    string conNum = _wordToCon[allChar[i]];
                    allChar[i] = conNum;
                }
            }
            result = string.Join("", allChar);

            return result;
        }

        public string ConvertConToWord(string wording)
        {
            string process = wording;

            foreach (var con in splitCon)
            {
                if (wording.Contains(con))
                {
                    string word = _conToWord[con];
                    process = process.Replace(con, word);
                }
            }

            return process;
        }

        public string ConvertWordToConReader(string wording)
        {
            string[] allChar = wording.Select(c => c.ToString()).ToArray();
            string result = "start";

            for (int i = 0; i < allChar.Length; i++)
            {
                if (_wordToConReader.ContainsKey(allChar[i]))
                {
                    string conNum = _wordToConReader[allChar[i]];
                    allChar[i] = conNum;
                }
            }
            result = string.Join("", allChar);

            return result;
        }

        public string ConvertConToWordReader(string wording)
        {
            string process = wording;

            foreach (var con in splitConReader)
            {
                if (wording.Contains(con))
                {
                    string word = _conToWordReader[con];
                    process = process.Replace(con, word);
                }
            }

            return process;
        }

        public static int CalculateWordCount(List<string> lineList)
        {
            int wordCount = 0;
            foreach (string line in lineList)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string cleanLine = line.Trim();
                    string[] words = Regex.Split(cleanLine, @"\s+");
                    wordCount += words.Length;
                }
            }

            return wordCount;
        }

        public static int CalculateIndividualWordFromList(List<string> lineList)
        {
            int wordCount = 0;

            foreach (string line in lineList)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] words = Regex.Split(line.Trim(), @"\s+");

                    foreach (var word in words)
                    {
                        wordCount = word.Length + wordCount;
                    }
                }
            }

            return wordCount;
        }

        public static List<string> RemoveTailingLines(List<string> lineList)
        {
            int lastLineIndex = lineList.Count - 1;

            while (string.IsNullOrWhiteSpace(lineList[lastLineIndex]))
            {
                lineList.RemoveAt(lastLineIndex);
                lastLineIndex--;
            };

            return lineList;
        }



    }
}
