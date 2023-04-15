using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLWordCount.Model
{
    public class WordOccurance
    {
        public string Word { get; set; }
        public int Occurance { get; set; }
        public WordOccurance(string word, int occurance) 
        { 
            Word = word;
            Occurance = occurance;
        }
    }
}
