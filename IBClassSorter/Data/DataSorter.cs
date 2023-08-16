using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//ADD THE IS ELECTIVE VALIDATION ***************************************************************************************************
//ON the class prefrences page if one class is set to an elective set all the ones in the same course to be an elective
namespace IBClassSorter.Data
{
    public static class DataSorter
    {
        public static List<TeacherModel> allTeachers = new List<TeacherModel>();
        public static List<ClassModel> allClasses = new List<ClassModel>();
        public static List<CourseModel> allCourses = new List<CourseModel>();
        public static List<Data.ClassPreferences> classPreferences = new List<ClassPreferences>();
        public static List<StudentModel> allStudents = new List<StudentModel>();

        public static List<possibleTeacherSchedules> allTeacherSchedules = new List<possibleTeacherSchedules>();


        public static List<List<possibleSchoolSchedules>> allSchedulePossibilities = new List<List<possibleSchoolSchedules>>();
        public static int currentListIndex = 0;
        

       // public static List<StudentSchedule> allStudentSchedules = new List<StudentSchedule>();

        public static List<possibleGroupSchedule> finalSchedules=new List<possibleGroupSchedule>();
        

        public static possibleTeacherSchedules getTeacherSchedulesByTeacherModel(TeacherModel t)
        {
            foreach(possibleTeacherSchedules x in allTeacherSchedules)
            {
                if (x.teacher == t)
                {
                    return x;
                }
            }
            return null;
        }

        public static TeacherSchedule getTeacherScheduleBySchoolSchedule(TeacherModel t, possibleSchoolSchedules s)
        {
            foreach(TeacherSchedule x in s.allTeacherSchedules)
            {
                if (x.teacher.id == t.id)
                {
                    return x;
                }
            }
            return null;
        }

        public static Data.ClassPreferences getClassPreferences(ClassModel c)
        {
            foreach(Data.ClassPreferences x in classPreferences)
            {
                if (x.classModel.id == c.id)
                {
                    return x;
                }
            }
            return null;
        }

        public static int getPeriodOfClassByTeacherSchedule(TeacherSchedule t, ClassModel c)
        {
            for(int i=0;i<t.periods.Length;i++)
            {
                if (t.periods[i]!=null && t.periods[i].id == c.id)
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool doesStudentHaveCourses(CourseModel x, CourseModel y)
        {
            foreach(StudentModel s in allStudents)
            {
                List<int> courseIds = new List<int>();
                foreach(CourseModel c in s.courses)
                {
                    courseIds.Add(c.id);
                }

                if(courseIds.Contains(x.id) && courseIds.Contains(y.id))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool isSchoolSchedulePossibleByLoaders(possibleSchoolSchedules s)
        {
            List<ClassModel> loadBearers = findFlexibleLoadBearingClasses();

            foreach(ClassModel x in loadBearers)
            {
                TeacherModel tempX = x.teacher;
                TeacherSchedule tempScheduleX = getTeacherScheduleBySchoolSchedule(tempX, s);
                int periodX = getPeriodOfClassByTeacherSchedule(tempScheduleX, x);
                bool isXElective = getClassPreferences(x).isElective;

                foreach (ClassModel y in loadBearers)
                {
                    TeacherModel tempY = y.teacher;
                    TeacherSchedule tempScheduleY = getTeacherScheduleBySchoolSchedule(tempY, s);
                    int periodY = getPeriodOfClassByTeacherSchedule(tempScheduleY, y);
                    bool isYElective = getClassPreferences(y).isElective;

                    if (!isXElective &&!isYElective && x!=y && periodX!=-1 && periodY!=-1 && periodX == periodY && doesStudentHaveCourses(x.course, y.course))//the 2 loadbearing classes share a period
                    {
                        //check if 1 student has both of these classes
                        return false;
                    }
                }
            }

            return true;
        }

        public static List<ClassModel> findFlexibleLoadBearingClasses()
        {
            List<ClassModel> loadBearingClasses = new List<ClassModel>();
            //the classes where there is only one spot (ex:ASB)
            foreach (ClassModel x in allClasses)
            {
                if (x.course.teachers.Count == 1 && x.numTimesTeaching == 1)
                {
                    loadBearingClasses.Add(x);
                }
            }

            return loadBearingClasses;
        }

        public static List<ClassModel> findLoadBearingClasses()
        {
            List<ClassModel> loadBearingClasses = new List<ClassModel>();
            //the classes where there is only one spot (ex:ASB)
            foreach (ClassModel x in allClasses)
            {
                ClassPreferences temp = getClassPreferences(x);
                if (x.course.teachers.Count==1 && x.numTimesTeaching==1 && !temp.isElective && temp.preferencePower == 1)
                {
                    loadBearingClasses.Add(x);
                }
            }

            return loadBearingClasses;
        }

        public static ClassModel getClassModelByCourseAndTeacher(CourseModel c, TeacherModel t)
        {
            foreach(ClassModel x in t.classes)
            {
                if (x.course == c)
                {
                    return x;
                }
            }
            return null;
        }


        public static List<classModelPlacement> getPeriodPlacements(CourseModel c, possibleSchoolSchedules s)
        {
            List<classModelPlacement> placements = new List<classModelPlacement>();
            List<TeacherModel> allCourseTeachers = c.teachers;
            foreach(TeacherModel x in allCourseTeachers)
            {
                TeacherSchedule currentTeachSched = getTeacherScheduleBySchoolSchedule(x,s);
                for(int i=0;i<currentTeachSched.periods.Length; i++)
                {
                    if (currentTeachSched.periods[i]!=null && currentTeachSched.periods[i].course.id == c.id)
                    {
                        placements.Add(new classModelPlacement
                        {
                            classType = currentTeachSched.periods[i],
                            period=i
                        });
                    }
                }
            }


            return placements;
        }

        

        public static StudentSchedule getRequiredStudentSchedule(StudentModel s, possibleSchoolSchedules p)
        {
            StudentSchedule schedule = new StudentSchedule
            {
                periods = new ClassModel[8],
                student=s,
            };

            foreach(CourseModel x in s.courses)
            {
                List<classModelPlacement> possiblePlacements=getPeriodPlacements(x,p);
                if (possiblePlacements.Count == 1 && !getClassPreferences(x.teachers[0].classes[0]).isElective)//required placement since only 1 class for the course
                {
                    if (schedule.periods[possiblePlacements[0].period] == null)
                    {
                        schedule.periods[possiblePlacements[0].period] = possiblePlacements[0].classType;
                    }
                    else
                    {
                        return null;//failure
                    }
                    
                }
            }


            return schedule;
        }

        /**
         * @return true if current schedule works, false if schedule fails
         */
        public static bool setAllStudentSchedulesBySchoolSchedule(List<studentScheduleOptions> schedules, possibleSchoolSchedules schedule)
        {
            
            foreach(StudentModel x in allStudents)//creates and sets all students with their required class placements
            {
                List<CourseModel> remainingCourses = new List<CourseModel>();
                foreach (CourseModel course in x.courses)
                {
                    remainingCourses.Add(CourseModel.duplicate(course));
                }
                
                
                
                StudentSchedule temp = getRequiredStudentSchedule(x, schedule);
                if (temp == null)
                {
                    return false;//FAILURE SCHEDULE
                }
                List<int> completedIDs = new List<int>();
                List<int> courseIDs = new List<int>();
                foreach (CourseModel course in remainingCourses)
                {
                    courseIDs.Add(course.id);
                }
                foreach (ClassModel c in temp.periods)
                {
                    
                    if (c!=null && courseIDs.Contains(c.course.id))
                    {
                        completedIDs.Add(c.course.id);
                    }
                }
                List<CourseModel> completedCourses = new List<CourseModel>();
                foreach(int c in completedIDs)
                {
                    foreach(CourseModel course in remainingCourses)
                    {
                        if (course.id == c)
                        {
                            completedCourses.Add(course);
                        }
                    }
                }
                foreach(CourseModel course in completedCourses)
                {
                    remainingCourses.Remove(course);
                }

                List<StudentSchedule> tempFilledIn = new List<StudentSchedule>(); 
                fillInRemainingStudentSchedule(tempFilledIn,temp, schedule, remainingCourses,0);

                if(tempFilledIn.Count > 0)
                {
                    
                        schedules.Add(new studentScheduleOptions
                        {
                            possibleSchedules=tempFilledIn,
                            student=x
                        });
                    
                    
                }
                else
                {
                    return false;//no possible schedules were created for this student-failure
                }
                
            }
            return true;
        }

        /**
         *
         */
        public static void fillInRemainingStudentSchedule(List<StudentSchedule> returnList, StudentSchedule _s, possibleSchoolSchedules schedule, List<CourseModel> rC, int numFailedElectives)
        {
            StudentSchedule s = StudentSchedule.duplicate(_s);

            List<CourseModel> reminingCourses = new List<CourseModel>();
            foreach(CourseModel x in rC)//**************************************?????????????????????????????????????????????
            {
                reminingCourses.Add(CourseModel.duplicate(x));
            }

            if (reminingCourses.Count != 0)
            {
                CourseModel c = reminingCourses[0];//get the first course

                List<classModelPlacement> coursePossiblePlacements = getPeriodPlacements(c, schedule);
                foreach (classModelPlacement x in coursePossiblePlacements)
                {
                    //assuming that all classpreferences for the course are equal
                    if (s.periods[x.period] != null && !getClassPreferences(c.teachers[0].classes[0]).isElective)
                    {
                        fillInRemainingStudentSchedule(returnList, s, schedule, new List<CourseModel>(),0);//break early causes failed
                    }
                    else if (s.periods[x.period] != null && getClassPreferences(c.teachers[0].classes[0]).isElective)//is an elective that doesn't work
                    {
                        int temp = numFailedElectives + 1;
                        reminingCourses.Remove(c);
                        fillInRemainingStudentSchedule(returnList, s, schedule, reminingCourses, temp);
                        reminingCourses.Add(c);
                    }
                    else
                    {
                        s.periods[x.period] = x.classType;
                        reminingCourses.Remove(c);
                        fillInRemainingStudentSchedule(returnList, s, schedule, reminingCourses,numFailedElectives);
                        reminingCourses.Add(c);
                        s.periods[x.period] = null;
                    }
                    

                }
            }
            else
            {
                if (studentScheduleComplete(s))
                {
                    s.numElectivesFailed = numFailedElectives;
                    returnList.Add(s);
                }
            }
            
        }

        public static bool studentScheduleComplete(StudentSchedule s)
        {
            List<CourseModel> requiredCourses = s.student.courses;
            int numRequired = 0;
            foreach(CourseModel x in requiredCourses)
            {
                if (!getClassPreferences(x.teachers[0].classes[0]).isElective)
                {
                    numRequired += 1;
                }
            }

            foreach(ClassModel x in s.periods)
            {
                if (x!=null && requiredCourses.Contains(x.course) && !getClassPreferences(x).isElective)
                {
                    numRequired-=1;
                }
            }

            return numRequired == 0;
        }

        public static void removeConflicts()
        {
            List<ClassModel> loadBearers = findLoadBearingClasses();
            foreach(ClassModel x in loadBearers)
            {
                int limitingPeriod = getClassPreferences(x).preferedPeriods[0];
                //find students with loadbearing class
                List<StudentModel> loadStudents = new List<StudentModel>();
                foreach(StudentModel y in allStudents)
                {
                    if (y.courses.Contains(x.course))
                    {
                        loadStudents.Add(y);
                    }
                }

                //find singular classes for loadstudents
                foreach(StudentModel y in loadStudents)
                {
                    
                    foreach(CourseModel z in y.courses)
                    {
                        
                        if (z.teachers.Count==1 && getClassModelByCourseAndTeacher(z, z.teachers[0]) != x && getClassModelByCourseAndTeacher(z, z.teachers[0]).numTimesTeaching == 1)
                        {//current class is a singular class
                            possibleTeacherSchedules teachSched = getTeacherSchedulesByTeacherModel(z.teachers[0]);
                            List<TeacherSchedule> overlappingSchedules=new List<TeacherSchedule>();
                            foreach (TeacherSchedule S in teachSched.allSchedules)
                            {
                                
                                //the singular class is placed in the limiting period
                                if (S.periods[limitingPeriod]!=null && !getClassPreferences(S.periods[limitingPeriod]).isElective && getClassModelByCourseAndTeacher(z, z.teachers[0])!=null && (S.periods[limitingPeriod].id== getClassModelByCourseAndTeacher(z, z.teachers[0]).id))
                                {
                                    overlappingSchedules.Add(S);
                                }
                            }

                            foreach(TeacherSchedule teacherSchedule in overlappingSchedules)
                            {
                                teachSched.allSchedules.Remove(teacherSchedule);
                            }
                            
                        }
                    }
                }
                
            }
        }

        public static void setAllPossibleIndividualTeacherSchedules()
        {
            allSchedulePossibilities.Add(new List<possibleSchoolSchedules>());


            for (int i = 0; i < allTeachers.Count; i++)
            {
                TeacherSchedule requiredClasses = setRequiredClasses(allTeachers[i]);
                List<ClassModel> tempRemainingClasses = new List<ClassModel>();

                foreach (ClassModel y in allTeachers[i].classes)
                {
                    tempRemainingClasses.Add(ClassModel.duplicate(y));
                }

                foreach (ClassModel x in requiredClasses.periods)
                {
                    

                    for(int j=0;j<tempRemainingClasses.Count;j++)
                    {
                        if (x!=null && tempRemainingClasses[j]!=null && tempRemainingClasses[j].id == x.id)//current class in tempReminingClasses is covered in the requiredClassPlacement
                        {
                            if (tempRemainingClasses[j].numTimesTeaching > 1)//if class still has another time teaching
                            {
                                tempRemainingClasses[j].numTimesTeaching--;
                            }else
                            if (tempRemainingClasses[j].numTimesTeaching == 1)//if class is finished with all placements
                            {
                                tempRemainingClasses[j].numTimesTeaching--;
                                tempRemainingClasses.Remove(tempRemainingClasses[j]);
                            }
                        }
                    }
                    
                }
                possibleTeacherSchedules newSet = new possibleTeacherSchedules
                {
                    teacher = allTeachers[i],
                    allSchedules = new List<TeacherSchedule>()
                };

                allTeacherSchedules.Add(newSet);

                setAllIndividualTeacherSchedules(requiredClasses, tempRemainingClasses, 0, newSet);

            }

            
            removeConflicts();
            setPossibleSchedules(new object[2] { 0, new List<TeacherSchedule>() });

            runStudentThreads(7);

            finalSchedules.Sort(new possibleGroupSchedule.Sorter());
            

        }

        public static int threadCounter = 0;

        public static List<possibleGroupSchedule>[] tempThreadHolder;
        
        public static void runStudentThreads(int totalThreads)//FIX THE THREAdS SOME 1495 some 1499 the ends change with diff num of threads
        {
            tempThreadHolder=new List<possibleGroupSchedule>[totalThreads];
            
            for (int i=0; i<totalThreads; i++)
            {
                tempThreadHolder[i]=new List<possibleGroupSchedule>();
                
            }

            Thread[] threads=new Thread[totalThreads];
            
            for (int i = 0; i < totalThreads; i++)
            {

                Thread t = new Thread(new ParameterizedThreadStart(runIndividualStudentThread));
                threads[i] = t;

                t.Start(new object[2] { i, totalThreads });

            }

         //   for (int i = 0; i < totalThreads; i++)
         //   {
          //      threads[i].Start(new object[2] { i, totalThreads });
         //   }


            while (true)
            {
                bool allDone = true;
                for(int i=0; i < threads.Length; i++)
                {
                    if (allDone && threads[i].ThreadState == ThreadState.Running)
                    {
                        allDone = false;
                    }
                }

                if (allDone)
                {
                    foreach (List<possibleGroupSchedule> x in tempThreadHolder)
                    {
                        finalSchedules.AddRange(x);
                    }
                    
                    //combine all the lists since 1 list isnt thread safe
                    return;
                }
            }
        }
       // public static List<int> tmepIndexes = new List<int>();
        public static void runIndividualStudentThread(object args)
        {
            Array argArray = new object[2];
            argArray = (Array)args;
            int startIndex = (int)argArray.GetValue(0);
            int numThreads = (int)argArray.GetValue(1);

            
            for(int i = 0; i <= currentListIndex; i++)
            {
                for (int j = startIndex; j < allSchedulePossibilities[i].Count; j+=numThreads)
                {
                    // if (isSchoolSchedulePossibleByLoaders(allSchedulePossibilities[j]))
                    // {
                    List<studentScheduleOptions> tempPossibleSchedules = new List<studentScheduleOptions>();
                    if (setAllStudentSchedulesBySchoolSchedule(tempPossibleSchedules, allSchedulePossibilities[i][j]))
                    {
                        possibleGroupSchedule tempPossibleGroupSchedule = new possibleGroupSchedule
                        {
                            possibleSchoolSchedule = allSchedulePossibilities[i][j],
                            studentSchedules = tempPossibleSchedules
                        };

                       
                        tempThreadHolder[startIndex].Add(tempPossibleGroupSchedule);
                        
                        

                        //Console.WriteLine("Student Schedule: " + finalSchedules.Count);
                        //   tmepIndexes.Add(j);

                        //worked-add the possibility
                    }
                    else
                    {
                        //failed school schedule didn't work
                    }
                    //   }


                }
            }
            
            threadCounter+=1;
        }

        public static bool threadsEnabled = false;

        public static int numThreads;

        public static void setPossibleSchedules(object args)
        {//add multithREADING-takes way too long for more teachers NAH

            Array argArray = new object[2];
            argArray = (Array)args;
            int currentIndex = (int)argArray.GetValue(0);
            List<TeacherSchedule> temp = (List<TeacherSchedule>)argArray.GetValue(1);

            List<TeacherSchedule> currentSchedules = new List<TeacherSchedule>();
            foreach(TeacherSchedule schedule in temp)
            {
                currentSchedules.Add(TeacherSchedule.duplicate(schedule));
            }
            if (allTeacherSchedules.Count > currentIndex)
            {
         //       bool started = false;
                foreach (TeacherSchedule x in allTeacherSchedules[currentIndex].allSchedules)
                {
                    
                    currentSchedules.Add(x);
                    //if currentIndex==0 then put paramaterized start thread else regular--DOesn't work if the first teacher is ASB-only 1 thread
                    //bool threadsEnabled=false, if (!threadsEnabled && allTeacherSchedules[currentIndex].allSchedles.Count>5) then start threads and threadsEmabled=true
             //       if(!threadsEnabled && allTeacherSchedules[currentIndex].allSchedules.Count > 5)
             //       {
             //           numThreads = allTeacherSchedules[currentIndex].allSchedules.Count;
             //           Thread t = new Thread(new ParameterizedThreadStart(setPossibleSchedules));
             //           t.Name = "T";
             //           t.Start(new object[2] { currentIndex += 1, currentSchedules });

            //            started = true;
           //         }
           //         else
           //         {
                        setPossibleSchedules(new object[2] { currentIndex += 1, currentSchedules });
            //        }

                    currentSchedules.Remove(x);
                    currentIndex -= 1;

                }
             //   if (started)
             //   {
            //        threadsEnabled = true;
             //   }
            }
            else
            {
                int totalPOWER = 0;
                foreach(TeacherSchedule x in currentSchedules)
                {
                    totalPOWER += x.preferencePower;
                }
                possibleSchoolSchedules tempSchedyule = new possibleSchoolSchedules
                {
                    id = possibleSchoolSchedules.idCounter++,
                    allTeacherSchedules = currentSchedules,
                    totalSchedulePoints = totalPOWER
                };

                if (allSchedulePossibilities[currentListIndex].Count > 1000000)
                {
                    allSchedulePossibilities.Add(new List<possibleSchoolSchedules>());
                    currentListIndex += 1;
                }

                if (isSchoolSchedulePossibleByLoaders(tempSchedyule))
                {
                    allSchedulePossibilities[currentListIndex].Add(tempSchedyule);
                  //  Console.WriteLine("School Schedule: " + allSchedulePossibilities[currentListIndex].Count);
                }
                

                
            }

            /*
            if (Thread.CurrentThread.Name == "T")
            {
                numThreads -= 1;
            }
            else
            {
                while (true)
                {
                    if (numThreads == 0)
                    {
                        return;
                    }
                }
            }
            */

            
        }
        public static TeacherSchedule setRequiredClasses(TeacherModel t)
        {
            TeacherSchedule allRequiredClasses = new TeacherSchedule
            {
                periods = new ClassModel[8],
                teacher=t
            };
            foreach (ClassPreferences x in classPreferences)
            {
                if (x.classModel.teacher == t && x.preferencePower == 1)
                {
                    foreach (int y in x.preferedPeriods)
                    {
                        allRequiredClasses.periods[y] = x.classModel;
                    }
                }
            }

            return allRequiredClasses;
        }

        public static bool scheduleComplete(TeacherSchedule t)
        {
            TeacherModel currentTeach = t.teacher;

            int totalClasses = 0;
            foreach(ClassModel x in currentTeach.classes)
            {
                totalClasses += x.numTimesTeaching;
            }

            foreach(ClassModel y in t.periods)
            {
                if (y != null)
                {
                    totalClasses -= 1;
                }
            }

            return totalClasses == 0;
            
        }

        public static void setAllIndividualTeacherSchedules(TeacherSchedule temp, List<ClassModel> remainingclasses, int currentPeriod, possibleTeacherSchedules l)
        {
            TeacherSchedule t = TeacherSchedule.duplicate(temp);
            List<ClassModel> remainingClasses = new List<ClassModel>();
            foreach(ClassModel x in remainingclasses)
            {
                remainingClasses.Add(ClassModel.duplicate(x));
            }

            if (currentPeriod == 0)
            {
                for (int i=0;(i<remainingClasses.Count && t.periods[0]==null); i++)
                {
                    if (remainingClasses[i].zeroAvail)
                    {
                        ClassPreferences classPreferences = getClassPreferences(remainingClasses[i]);
                        if (classPreferences.preferedPeriods != null && classPreferences.preferedPeriods.Contains(0) && classPreferences.preferencePower == 2)
                        {
                            int currentPower = t.preferencePower;
                            t.preferencePower = currentPower + 1;
                        }

                        t.periods[0] = remainingClasses[i];
                        remainingClasses.Remove(remainingClasses[i]);
                        
                    }
                }
                //check if has a class in RemainingClasses that has a 0 placement, if so place it in 0 and remove from remaining classes

                setAllIndividualTeacherSchedules(t, remainingClasses, 1,l);
            }
            else if (currentPeriod<7)
            {
                if (t.periods[currentPeriod] != null)//already placed there from required placements
                {
                    setAllIndividualTeacherSchedules(t, remainingClasses, currentPeriod += 1, l);
                    currentPeriod -= 1;
                }
                else
                {
                    //set possibility for empty space
                    setAllIndividualTeacherSchedules(t, remainingClasses, currentPeriod += 1, l);
                    currentPeriod -= 1;

                    List<ClassModel> prevCopy = new List<ClassModel>();
                    foreach (ClassModel x in remainingclasses)
                    {
                        prevCopy.Add(ClassModel.duplicate(x));
                    }

                    for (int i = 0; (i < remainingClasses.Count); i++)//for (int i=remainingClasses.Count-1;i>=0;i--)
                    {


                        ClassPreferences classPreferences = getClassPreferences(remainingClasses[i]);
                        if (classPreferences.preferedPeriods != null && classPreferences.preferedPeriods.Contains(currentPeriod) && classPreferences.preferencePower == 2)
                        {
                            int currentPower = t.preferencePower;
                            t.preferencePower = currentPower + 1;
                        }

                        t.periods[currentPeriod] = remainingClasses[i];
                        if (remainingClasses[i].numTimesTeaching > 0)
                        {
                           remainingClasses[i].numTimesTeaching--;
                        }

                        remainingClasses=removeEmptys(remainingClasses);

                        setAllIndividualTeacherSchedules(t, remainingClasses, currentPeriod += 1, l);
                        currentPeriod -= 1;

                        //reverse changes for next run
                        t.periods[currentPeriod] = null;
                        remainingClasses = prevCopy;

                    }
                    //find how many classes the teacher has left to teach and if its less than the remaining periods left have option for null placement
                }
            }
            else//7
            {
                for (int i = 0; (i < remainingClasses.Count && t.periods[7] == null); i++)
                {
                    if (remainingClasses[i].sevenAvail)
                    {
                        ClassPreferences classPreferences = getClassPreferences(remainingClasses[i]);
                        if (classPreferences.preferedPeriods!=null && classPreferences.preferedPeriods.Contains(7) && classPreferences.preferencePower==2)
                        {
                            int currentPower = t.preferencePower;
                            t.preferencePower = currentPower + 1;
                        }
                        t.periods[7] = remainingClasses[i];
                        remainingClasses.Remove(remainingClasses[i]);

                    }
                }
                if (scheduleComplete(t))
                {
                    l.allSchedules.Add(t);
                }
                
                //check if has class in 7 placement and place it in

            }
        }

        /*
        public static int totalRemainingClassSpots(TeacherSchedule t)
        {
            int totalSpacesLeft = 0;
            foreach(ClassModel x in t.periods)
            {
                if (x == null)
                {
                    totalSpacesLeft++;
                }
            }

            return totalSpacesLeft-2;
        }
        */

        public static List<ClassModel> removeEmptys(List<ClassModel> input)
        {
            List<ClassModel> remainingClasses = new List<ClassModel>();
            foreach(ClassModel x in input)
            {
                if (x.numTimesTeaching != 0)
                {
                    remainingClasses.Add(x);
                }
            }
            return remainingClasses;
        }
        /*
        public static int totalRemainingClassesToTeach(List<ClassModel> c)
        {
            int total = 0;
            foreach(ClassModel x in c)
            {
                total += x.numTimesTeaching;
            }
            return total;
        }
        */
        
        /*
        public static bool groupScheduleOverlap(possibleGroupSchedule p)
        {
            foreach(possibleGroupSchedule x in finalSchedules)
            {
                if (x.studentSchedules.Equals(p.studentSchedules))
                {
                    return true;
                }
            }
            return false;
        }
        */

        /*
        public static bool groupScheduleListsEqual(List<studentScheduleOptions> x, List<studentScheduleOptions> y)
        {
            if (x.Count != y.Count)
            {
                return false;
            }

            for(int i = 0; i < x.Count; i++)
            {
                if (!x[i].equals(y[i]))
                {
                    return false;
                }
            }

            return true;
        }
        */
        public class possibleTeacherSchedules{
            public TeacherModel teacher { get; set; }
            public List<TeacherSchedule> allSchedules { get; set; }
        }

        public class possibleSchoolSchedules
        {
            public static int idCounter { get; set; }
            public int id { get; set; }
            public List<TeacherSchedule> allTeacherSchedules { get; set; }
            public int totalSchedulePoints { get; set; }
        }

        public class classModelPlacement
        {
            public ClassModel classType { get; set; }
            public int period { get; set; }
        }

        public class possibleGroupSchedule
        {
            public List<studentScheduleOptions> studentSchedules { get; set; }
            public possibleSchoolSchedules possibleSchoolSchedule { get; set; }

            public class Sorter : IComparer<possibleGroupSchedule>
            {
                public int Compare(possibleGroupSchedule x, possibleGroupSchedule y)
                {
                    return y.possibleSchoolSchedule.totalSchedulePoints.CompareTo(x.possibleSchoolSchedule.totalSchedulePoints);
                }
            }

        }

        public class studentScheduleOptions
        {
            public StudentModel student { get; set; }
            public List<StudentSchedule> possibleSchedules { get; set; }

            public bool equals(studentScheduleOptions s)
            {
                if(s.student.id==student.id)
                {
                    if (possibleSchedules.Count != s.possibleSchedules.Count)
                    {
                        return false;
                    }
                    for(int i = 0; i < possibleSchedules.Count; i++)
                    {
                        if (!possibleSchedules[i].equals(s.possibleSchedules[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }

    }
}
