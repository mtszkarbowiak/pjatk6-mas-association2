namespace Association2;

public interface IAssociationConstraint
{
    bool IsSatisfied(Association association, AssociableBase source, AssociableBase target, object? qualifier);

    string Name { get; }
}