using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public class ClassPreferences
    {
        public ClassModel classModel { get; set; }
        public List<int> preferedPeriods { get; set; }
        public int preferencePower { get; set; }

        public bool isElective { get; set; }

    }
}
