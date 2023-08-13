using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public class ClassModel
    {
        public int id { get; set; }
        public CourseModel course { get; set; }
        public TeacherModel teacher { get; set; }
        public int numTimesTeaching { get; set; }
        public bool zeroAvail { get; set; }
        public bool sevenAvail { get; set; }

        public static ClassModel duplicate(ClassModel x)
        {
            return new ClassModel
            {
                id = x.id,
                course = x.course,
                teacher = x.teacher,
                numTimesTeaching = x.numTimesTeaching,
                zeroAvail = x.zeroAvail,
                sevenAvail = x.sevenAvail,
            };

        }

    }
}
