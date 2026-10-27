namespace backend.ValueObjects;

public readonly struct SimilarityScore : IComparable<SimilarityScore>, IEquatable<SimilarityScore>
{
    public static SimilarityScore Zero => new(0);

    public double Value { get; }

    public SimilarityScore(double value)
    {
        if (double.IsNaN(value) || value < 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Similarity score must be between 0 and 1.");
        }

        Value = value;
    }

    public int CompareTo(SimilarityScore other) => Value.CompareTo(other.Value);

    public bool Equals(SimilarityScore other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is SimilarityScore other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(SimilarityScore left, SimilarityScore right) => left.Equals(right);

    public static bool operator !=(SimilarityScore left, SimilarityScore right) => !left.Equals(right);

    public static bool operator <(SimilarityScore left, SimilarityScore right) => left.CompareTo(right) < 0;

    public static bool operator >(SimilarityScore left, SimilarityScore right) => left.CompareTo(right) > 0;

    public static bool operator <=(SimilarityScore left, SimilarityScore right) => left.CompareTo(right) <= 0;

    public static bool operator >=(SimilarityScore left, SimilarityScore right) => left.CompareTo(right) >= 0;
}
