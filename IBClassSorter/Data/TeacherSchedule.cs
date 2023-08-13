using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public class TeacherSchedule
    {
        public TeacherModel teacher { get; set; }

        public ClassModel[] periods { get; set; }

        public int preferencePower { get; set; }

        public static TeacherSchedule duplicate(TeacherSchedule t)
        {
            ClassModel[] copy = new ClassModel[8];
            for(int i = 0; i < t.periods.Length; i++)
            {
                copy[i] = t.periods[i];
            }
            
            return new TeacherSchedule
            {
                teacher = t.teacher,
                periods = copy,
                preferencePower = t.preferencePower
            };
        }

    }
}
