using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueFileCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            FileDB db = new FileDB();
            LoadFileList.Load(@"", db);
        }
    }
}
