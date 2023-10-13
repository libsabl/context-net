namespace Sabl;

public abstract class ContextKey
{
    public readonly string Label;
    private readonly int _hash;

    public ContextKey(string label)
    {
        Label = label;
        _hash = label.GetHashCode();
    }

    internal abstract Type ValueType { get; }

    internal abstract bool Nullable { get; } 

    /// <inheritdoc />
    public override int GetHashCode() => _hash;

    /// <inheritdoc />
    public override bool Equals(object? obj) => object.ReferenceEquals(this, obj);

    /// <inheritdoc />
    public override string ToString() => "Key(" + Label + ")";

    /// <inheritdoc />
    public static bool operator ==(ContextKey a, ContextKey b) => object.ReferenceEquals(a, b);

    /// <inheritdoc />
    public static bool operator !=(ContextKey a, ContextKey b) => !object.ReferenceEquals(a, b);
}

public sealed class ContextKey<T> : ContextKey
{
    private readonly Type _type;
    private readonly bool _nullable;

    public ContextKey() : this(typeof(T).Name) { }

    public ContextKey(string label) : base(label)
    {
        _type = typeof(T);
        _nullable = IsTypeNullable(_type);
    }

    private static bool IsTypeNullable(Type t)
    {
        if (!t.IsValueType) return true;
        if (!t.IsGenericType) return false; 
        var tdef = t.GetGenericTypeDefinition(); 
        return (tdef == typeof(Nullable<>));
    }

    internal override Type ValueType => _type;

    internal override bool Nullable => _nullable;
}
