using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class TestSamplesGenerator
    {
        public static List<Character> ReferenceCharacters;

        public static Dictionary<char, int> GenerateRandomTestSamples(int size)
        {
            Dictionary<char, int> sampleDict = new Dictionary<char, int>();
            Random seed = new Random();

            while (sampleDict.Count < size)
            {
                int index = seed.Next(0, 1000);
                Character sample = ReferenceCharacters[index];
                if (sampleDict.ContainsKey(sample.Font))
                {
                    continue;
                }
                else
                {
                    sampleDict.Add(sample.Font, sample.StrokeCount());
                }
            }

            var sortedDict = Algorithms.SortDictionaryAscendingByValue<char>(sampleDict);
            Dictionary<char, int> newList = new Dictionary<char, int>();
            foreach (KeyValuePair<char, int> item in sortedDict)
            {
                newList.Add(item.Key, item.Value);
            }
            return newList;
        }
    }
}
