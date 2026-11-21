using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Online_Course_Enrollment_System_Properties.Domain;
using Online_Course_Enrollment_System_Properties.Tests.Arbitraries;

namespace Online_Course_Enrollment_System_Properties.Tests.Properties
{
    public class CourseProperties
    {
        //Тест виняток
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Course_NonPositiveCapacity_ThrowsArgumentException(Course course, NonNegativeInt capaciity)
        {
            Action act = () => new Course(course.Id, -capaciity.Get);

            act.Should().Throw<ArgumentException>();
        }

        //P1. Туди й назад з передумовою When
        [Property(Arbitrary =new[]{typeof(CourseArbitraries)})]
        public Property P1_Invariant_EnrollThenUnenroll_ReturnsCourseToOriginalState(Course course, Student student)
        {
            bool canEnroll=!course.IsFull && !course.IsEnrolled(student.Id);
            bool ok=true;

            if(canEnroll)
            {
                var studentsBefore = course.Students.Count;
                var freeSeatsBefore = course.FreeSeats;

                course.Enroll(student);
                course.Unenroll(student.Id);

                ok=course.Students.Count==studentsBefore && course.FreeSeats==freeSeatsBefore && !course.IsEnrolled(student.Id);
            }

            return ok.When(canEnroll);
        }

        // P2 · Інваріант
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void P2_Invariant_enrollment_keeps_students_within_capacity(Course course, Student[] students)
        {
            foreach(var student in students)
            {
                course.Enroll(student);

                var actual=course.Students.Count<=course.Capacity;

                actual.Should().BeTrue();
                course.FreeSeats.Should().Be(course.Capacity - course.Students.Count);
                course.Students.Select(s => s.Id).Should().OnlyHaveUniqueItems();
            }
        }

        //P3. Ідемпотентність 
        [Property(Arbitrary =new[]{typeof(CourseArbitraries)})]
        public void P3_EnrollSameStudentTwice_DoesNotChangeCourse(Course course, Student[] students, Student student)
        {
            foreach(var s in students)
            {
                if (course.FreeSeats > 1)
                {
                    course.Enroll(s);
                }
            }
            
            var newStudent=new Student(student.Id);
            course.Enroll(newStudent);

            var studentsCountAfterFirst=course.Students.Count;

            var secondAttempt=course.Enroll(new Student(newStudent.Id));

            secondAttempt.Should().BeFalse();
            course.Students.Count.Should().Be(studentsCountAfterFirst);
            course.FreeSeats.Should().Be(course.Capacity - course.Students.Count);
        }

        //P8. Без винятку
        [Property(Arbitrary =new[]{typeof(CourseArbitraries)})]
        public void P8_NoException_Enroll_ValidStudent(Course course, Student[] students, Student student)
        {
            foreach (var s in students)
            {
                if (course.FreeSeats > 1)
                {
                    course.Enroll(s);
                }
            }
             
            var newStudent = new Student(student.Id);

            Action act = () => course.Enroll(newStudent);

            act.Should().NotThrow();
            course.IsEnrolled(newStudent.Id).Should().BeTrue();
        }

        // P8. Без винятку
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void P8_NoException_Unenroll_EnrolledStudent(Course course, Student[] students, Student student)
        {
            foreach (var s in students)
            {
                if (course.FreeSeats > 1)
                {
                    course.Enroll(s);
                }
            }

            var newStudent=new Student(student.Id);
            course.Enroll(newStudent);

            Action act = () => course.Unenroll(newStudent.Id);

            act.Should().NotThrow();
            course.IsEnrolled(newStudent.Id).Should().BeFalse();
        }

        //Тест з передумовою When
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public Property FullCourse_RejectsNewStudent(Course course,Student[] students,Student student)
        {
            foreach (var s in students)
            {
                course.Enroll(s);
            }

            bool applicable = course.IsFull && !course.IsEnrolled(student.Id);

            bool ok = true;

            if (applicable)
            {
                int studentsBefore = course.Students.Count;

                bool enrolled = course.Enroll(student);

                ok = !enrolled&& course.Students.Count == studentsBefore&& course.IsFull;
            }

            return ok.When(applicable);
        }

        //Тест виняток з передумовою
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public Property Unenroll_StudentNotEnrolled_ThrowsException(Course course, Student[] students, Student student)
        {
            foreach(var s in students)
            {
                if (course.FreeSeats > 1)
                {
                    course.Enroll(s);
                }
            }

            bool applicable = !course.IsEnrolled(student.Id);
            bool ok = true;

            if (applicable)
            {
                Action act = () => course.Unenroll(student.Id);

                act.Should().Throw<InvalidOperationException>();
            }

            return ok.When(applicable);
        }
        
    }
}
