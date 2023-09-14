using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public static class DataHolder
    {
        public static List<TeacherModel> allTeachers { get; set; }

        public static List<CourseModel> allCourses { get; set; }

        public static List<ClassModel> allClasses { get; set; }

        public static List<Data.ClassPreferences> allClassesPreferences { get; set; } 

        public static List<StudentModel> allStudents { get; set; }
    }
}
