using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public class TeacherModel
    {
        public int id { get; set; }
        public string name { get; set; }

        public List<ClassModel> classes { get; set; }
    }
}
