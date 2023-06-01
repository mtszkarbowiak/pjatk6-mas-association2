namespace Association2.Tests;

[TestClass]
public class SchoolTests
{
    public Student John { get; } = new();
    public Student Mary { get; } = new();

    public Course Math { get; } = new();
    public Course Physics { get; } = new();

    public Teacher Smith { get; } = new();
    public Teacher Brown { get; } = new();
    public Teacher White { get; } = new();
    public Teacher Stone { get; } = new();

    public Group TheGroup { get; } = new();


    [TestMethod]
    public void TypeConstraint()
    {
        John.AddLink(School.Enrollment, Math);

        Assert.ThrowsException<ArgumentException>(
            () => { Physics.AddLink(School.Enrollment, John); }
        );
    }

    [TestMethod]
    public void CardinalityConstraint()
    {
        Math.AddLink(School.HeadOfCourse, Smith);
        Math.AddLink(School.TeacherOfCourse, Brown);
        Math.AddLink(School.HeadOfCourse, White);

        Assert.ThrowsException<ArgumentException>(
            () => { Math.AddLink(School.HeadOfCourse, Stone); }
        );
    }

    [TestMethod]
    public void XorConstraint()
    {
        Math.AddLink(School.HeadOfCourse, Smith);

        Assert.ThrowsException<ArgumentException>(
            () => { Math.AddLink(School.TeacherOfCourse, Smith); }
        );
    }

    [TestMethod]
    public void QualifierConstraint()
    {
        TheGroup.AddLink(School.ScheduledLesson, Math, DayOfWeek.Monday);
        TheGroup.AddLink(School.ScheduledLesson, Physics, DayOfWeek.Tuesday);

        Assert.ThrowsException<ArgumentException>(
            () => { TheGroup.AddLink(School.ScheduledLesson, Physics, DayOfWeek.Monday); }
        );
    }


    [TestMethod]
    public void CustomConstraint()
    {
        Assert.ThrowsException<ArgumentException>(
            () => { TheGroup.AddLink(School.ScheduledLesson, Physics, DayOfWeek.Saturday); }
        );
    }


    [TestMethod]
    public void RemovingByTarget()
    {
        Mary.AddLink(School.Enrollment, Math);
        Mary.AddLink(School.Enrollment, Physics);

        Mary.RemoveLinkByTarget(School.Enrollment, Math);

        Assert.IsFalse(
            Mary.GetLinks(School.Enrollment)
                .Select(keyValue => (Course)keyValue.Key)
                .Contains(Math)
        );
        Assert.IsTrue(
            Mary.GetLinks(School.Enrollment)
                .Select(keyValue => (Course)keyValue.Key)
                .Contains(Physics)
        );

        Assert.AreEqual(
            Mary,
            Physics.GetLinks(School.Enrollment)
                .Select(pair => pair.Value)
                .Cast<Student>()
                .Single()
        );

        Assert.IsFalse(
            Math.GetLinks(School.Enrollment)
                .Select(pair => pair.Value)
                .Cast<Student>()
                .Any()
        );
    }

    [TestMethod]
    public void RemovingByQualifier()
    {
        TheGroup.AddLink(School.ScheduledLesson, Math, DayOfWeek.Friday);

        Assert.IsTrue(
            TheGroup.GetLinks(School.ScheduledLesson)
                .Select(pair => ((DayOfWeek)pair.Key, (Course)pair.Value))
                .Any(tuple => tuple.Item1 == DayOfWeek.Friday)
        );

        TheGroup.RemoveLinkByQualifier(School.ScheduledLesson, DayOfWeek.Friday);

        Assert.IsFalse(
            TheGroup.GetLinks(School.ScheduledLesson)
                .Select(pair => ((DayOfWeek)pair.Key, (Course)pair.Value))
                .Any(tuple => tuple.Item1 == DayOfWeek.Friday)
        );

        Assert.ThrowsException<ArgumentException>(
            () => { TheGroup.AddLink(School.ScheduledLesson, Physics, DayOfWeek.Sunday); }
        );
    }
}