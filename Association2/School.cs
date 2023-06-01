namespace Association2;

public static class School
{
    public static Association Enrollment { get; } = new(
        typeof(Student),
        typeof(Course)
    )
    {
        Name = nameof(Enrollment),
        Description = "A student is enrolled in a course.",
    };

    public static Association HeadOfCourse { get; } = new(
        typeof(Course),
        typeof(Teacher)
    )
    {
        Name = nameof(HeadOfCourse),
        Description = "Selects scope of the course.",
        Cardinality = 2,
    };

    public static Association TeacherOfCourse { get; } = new(
        typeof(Course),
        typeof(Teacher),
        xorSource: HeadOfCourse
    )
    {
        Name = nameof(TeacherOfCourse),
        Description = "Teaching the course.",
    };

    public static Association ScheduledLesson { get; } = new(
        typeof(Group),
        typeof(Course),
        constraints: new[] { new WorkingDayOfWeekConstraint() }
    )
    {
        Name = nameof(ScheduledLesson),
        Description = "Group has lessons in this day of a week.",
        QualifierType = typeof(DayOfWeek),
    };
}

public class Student : AssociableBase
{
}

public class Course : AssociableBase
{
}

public class Teacher : AssociableBase
{
}

public class Group : AssociableBase
{
}

public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday,
}

public class WorkingDayOfWeekConstraint : IAssociationConstraint
{
    public bool IsSatisfied(Association association, AssociableBase source, AssociableBase target, object? qualifier)
    {
        if (qualifier is not DayOfWeek dayOfWeek)
        {
            throw new ArgumentException(
                $"The qualifier must be of type {typeof(DayOfWeek)}.",
                nameof(qualifier)
            );
        }

        return dayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
    }

    public string Name => nameof(WorkingDayOfWeekConstraint);
}