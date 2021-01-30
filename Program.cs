using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VIAutocomplete
{
    class Program
    {
        public static Dictionary<string, Dictionary<string, int>> WordOccurence = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, string> Predictions = new Dictionary<string, string>();
        public static int nGram = 3;

        static void Main(string[] args)
        {
            using (FileStream fs = File.Open("input.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                string previous = "";
                string leftover = "";
                while ((line = sr.ReadLine()) != null)
                {
                    int dotPosition = line.IndexOf('.');
                    if (dotPosition == -1)
                    {
                        previous += " " + line + " ";
                    }
                    else
                    {
                        ProcessSentence(previous + line.Substring(0, dotPosition));
                        leftover = line.Substring(dotPosition, line.Length - dotPosition);
                        int leftoverDotPosition = leftover.IndexOf('.');

                        while (leftoverDotPosition != -1)
                        {
                            string sentence = leftover.Substring(0, leftoverDotPosition);
                            if (sentence.Length > 0)
                                ProcessSentence(sentence);
                            leftover = leftover.Substring(leftoverDotPosition + 1, leftover.Length - leftoverDotPosition - 1);
                            leftoverDotPosition = leftover.IndexOf('.');
                        }

                        if (leftover.Length > 0)
                        {
                            previous = leftover + " ";
                        }
                        else
                        {
                            previous = "";
                        }
                    }
                }
            }

            InitPredictions();
            StartPredicting();
        }

        public static void StartPredicting()
        {
            while(true)
            {
                Console.WriteLine("Start sentence:");
                string start = Console.ReadLine().ToLower();
                var words = new List<string>(("\\s \\s " + start.Trim()).Split(" "));
                words.RemoveAll(x => x == "");
                // string prediction = null;
                // Predictions.TryGetValue(words[words.Count - 2] + " " + words[words.Count - 1], out prediction);


                string word1 = words[words.Count - 2];
                string word2 = words[words.Count - 1];
                string prediction = " ";
                for (int i = 0; i < 20; i++)
                {
                    string predicted = null;
                    Predictions.TryGetValue(word1 + " " + word2, out predicted);
                    if (predicted == null || predicted == "\\e")
                    {
                        break;
                    }
                    else
                    {
                        prediction += predicted + " ";
                        Console.WriteLine(start + prediction);
                        word1 = word2;
                        word2 = predicted;
                    }
                }
            }
        }

        public static void InitPredictions()
        {
            foreach(var start in WordOccurence)
            {
                string prediction = "";
                int maximum = 0;
                foreach (var end in start.Value)
                {
                    if (end.Value > maximum)
                    {
                        prediction = end.Key;
                        maximum = end.Value;
                    }
                }
                Predictions[start.Key] = prediction;
            }
        }

        public static void ProcessSentence(string sentence)
        {
            var words = new List<string>(("\\s \\s " + sentence + " \\e").Split(" "));
            words.RemoveAll(x => x == "");

            for (int i = 2; i < words.Count - 1; i++)
            {
                Regex rgx = new Regex("^[^a-zA-Z0-9 -]+");
                words[i] = rgx.Replace(words[i], "");
                rgx = new Regex("[^a-zA-Z0-9 -]+$");
                words[i] = rgx.Replace(words[i], "");
                words[i] = words[i].ToLower();
            }

            for(int i = 0; i < words.Count - nGram + 1; i++)
            {
                Dictionary<string, int> value;
                bool startExists = WordOccurence.TryGetValue(words[i] + " " + words[i + 1], out value);
                if (!startExists)
                {
                    value = WordOccurence[words[i] + " " + words[i + 1]] = new Dictionary<string, int>();
                }
                int occurences;
                bool endExists = value.TryGetValue(words[i + 2], out occurences);
                if (endExists)
                {
                    value[words[i + 2]] = occurences + 1;
                }
                else
                {
                    value[words[i + 2]] = 1;
                }
            }
        }
    }
}
