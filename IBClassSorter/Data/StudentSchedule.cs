using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBClassSorter.Data
{
    public class StudentSchedule
    {
        public StudentModel student { get; set; }

        public ClassModel[] periods { get; set; }

        public int numElectivesFailed { get; set; }

        public bool equals(StudentSchedule s)
        {
            if (s.student.id == student.id)
            {
                for(int i=0;i<periods.Length;i++)
                {
                    if (!((periods[i]==null && s.periods[i]==null) || (periods[i]!=null&& s.periods[i]!=null && periods[i].id == s.periods[i].id)))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static StudentSchedule duplicate(StudentSchedule t)
        {
            ClassModel[] copy = new ClassModel[8];
            for (int i = 0; i < t.periods.Length; i++)
            {
                copy[i] = t.periods[i];
            }

            return new StudentSchedule
            {
                student=t.student,
                periods=copy
            };
        }


    }
}
