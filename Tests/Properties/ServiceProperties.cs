using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Online_Course_Enrollment_System_Properties.Domain;
using Online_Course_Enrollment_System_Properties.Tests.Arbitraries;

namespace Online_Course_Enrollment_System_Properties.Tests.Properties
{
    public class ServiceProperties
    {
        //Тест виняток
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void CreateCourse_DuplicateId_ThrowsException(Course course)
        {
            var repository = new EnrollmentRepository();
            var service = new EnrollmentService(repository);

            service.CreateCourse(course.Id, course.Capacity);

            Action act = () => service.CreateCourse(course.Id, course.Capacity);

            act.Should().Throw<InvalidOperationException>();
        }

        //P5. Оракул
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void P5_Oracle_UnenrollService_Equals_DirectUnenroll( Course course, Student student)
        {
            course.Enroll(student);

            var viaService = Copy(course);
            var direct = Copy(course);

            var repository = new EnrollmentRepository();
            repository.Save(viaService);

            var service = new EnrollmentService(repository);

            direct.Unenroll(student.Id);
            service.Unenroll(viaService.Id, student.Id);

            direct.Students.Count.Should().Be(viaService.Students.Count);
            direct.FreeSeats.Should().Be(viaService.FreeSeats);
            direct.IsEnrolled(student.Id).Should().Be(viaService.IsEnrolled(student.Id));
        }

        //Тест виняток
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Unenroll_NonExistingCourse_ThrowsKeyNotFoundException(Course course, Student student)
        {
            var repository = new EnrollmentRepository();
            var service = new EnrollmentService(repository);

            Action act = () => service.Unenroll(course.Id, student.Id);

            act.Should().Throw<KeyNotFoundException>();
        }

        private static Course Copy(Course course)
        {
            var copy = new Course(course.Id, course.Capacity);

            foreach (var student in course.Students)
            {
                copy.Enroll(new Student(student.Id));
            }

            return copy;
        }
    }
}
