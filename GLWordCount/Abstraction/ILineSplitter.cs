using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLWordCount.Abstraction
{
    internal interface ILineSplitter
    {
		IEnumerable<string> SplitLine(string line);
	}
}
