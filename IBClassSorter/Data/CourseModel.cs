using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public class CourseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<TeacherModel> teachers { get; set; }

        public static CourseModel duplicate(CourseModel x)
        {
            return new CourseModel
            {
                id = x.id,
                name = x.name,
                teachers = x.teachers
            };
        }
    }
}
