namespace Association2;

public class Association
{
    public Type SourceType { get; }

    public Type TargetType { get; }


    public int? Cardinality { get; init; } = null;

    public Type? QualifierType { get; init; } = null;

    public IReadOnlySet<Association> Xor => _xor;

    public IReadOnlyList<IAssociationConstraint> Constraints => _constraints;


    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;


    private readonly HashSet<Association> _xor = new();

    private readonly List<IAssociationConstraint> _constraints = new();


    public Association(
        Type sourceType,
        Type targetType,
        Association? xorSource = null,
        IEnumerable<IAssociationConstraint>? constraints = null)
    {
        SourceType = sourceType;
        TargetType = targetType;

        if (xorSource is not null)
        {
            _xor.Add(xorSource);
            xorSource._xor.Add(this);
        }

        if (constraints is not null)
        {
            _constraints.AddRange(constraints);
        }
    }
}