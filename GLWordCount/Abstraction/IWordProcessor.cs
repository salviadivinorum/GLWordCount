using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLWordCount.Abstraction
{
    public interface IWordProcessor
    {
		IEnumerable<string> GetWords();
	}
}
