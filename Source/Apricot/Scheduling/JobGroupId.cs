namespace Apricot.Scheduling;

public readonly struct JobGroupId(string name) : IEquatable<JobGroupId>
{
    public string Name { get; } = name;

    public bool Equals(JobGroupId other) => Name == other.Name;
    
    public override bool Equals(object? obj) => obj is JobGroupId other && Equals(other);
    
    public override int GetHashCode() => Name.GetHashCode();
    
    public override string ToString() => Name;

    public static bool operator ==(JobGroupId left, JobGroupId right) => left.Equals(right);

    public static bool operator !=(JobGroupId left, JobGroupId right) => !(left == right);
}
