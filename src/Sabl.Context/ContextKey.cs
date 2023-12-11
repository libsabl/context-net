// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Diagnostics.CodeAnalysis;

namespace Sabl;

/// <summary>A reference-equality key with both a string label and a target type</summary>
/// <remarks>
/// ContextKeys are always compared with reference equality. Two key instances
/// with the same label and value type will not be considered equal.
/// </remarks>
public abstract class ContextKey
{
    /// <summary>A descriptive label for the key</summary>
    public string Label { get; }

    private readonly int _hash;

    internal ContextKey(string label)
    {
        ArgumentNullException.ThrowIfNull(label, nameof(label));
        this.Label = label;
        _hash = label.GetHashCode();
    }

    /// <summary>The runtime type of values to be associated with this key</summary>
    internal abstract Type ValueType { get; }

    /// <summary>Indicates whether <see cref="ValueType"/> is a <see cref="Nullable{T}"/></summary>
    internal abstract bool Nullable { get; }

    /// <inheritdoc />
    public override int GetHashCode() => _hash;

    /// <inheritdoc />
    public override bool Equals(object? obj) => object.ReferenceEquals(this, obj);

    /// <inheritdoc />
    public override string ToString() => "Key(" + this.Label + ")";

    /// <inheritdoc />
    public static bool operator ==(ContextKey a, ContextKey b) => object.ReferenceEquals(a, b);

    /// <inheritdoc />
    public static bool operator !=(ContextKey a, ContextKey b) => !object.ReferenceEquals(a, b);

    /// <summary>Check if <paramref name="value"/> can be associated with <paramref name="key"/>.
    /// If not, assigns an appropriate exception to <see langword="out"/> <paramref name="ex"/></summary>
    public static bool IsValueAllowed(ContextKey key, object? value, [NotNullWhen(false)] out Exception? ex)
    {
        if (value == null) {
            if (!key.Nullable) {
                ex = new ArgumentNullException(nameof(value), $"Cannot use null value for value of non-nullable type {key.ValueType}");
                return false;
            }
        } else {
            if (!value.GetType().IsAssignableTo(key.ValueType)) {
                ex = new InvalidCastException($"Provided value of type {value.GetType()} cannot be assigned to type {key.ValueType} associated with key {key.Label}");
                return false;
            }
        }
        ex = null;
        return true;
    }
}

/// <summary>A <see cref="ContextKey"/> associated with a specific value type <typeparamref name="T"/></summary>
public sealed class ContextKey<T> : ContextKey
{
    private readonly Type _type;
    private readonly bool _nullable;

    /// <summary>Create a new context key with the default label of typeof(<typeparamref name="T"/>.Name</summary>
    public ContextKey() : this(TypeName(typeof(T))) { }

    /// <summary>Create a new context key with the provided label</summary>
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

    private static string TypeName(Type type)
    {
        if (type.IsGenericType) {
            var tdef = type.GetGenericTypeDefinition();
            if (tdef == typeof(Nullable<>)) {
                return "Nullable<" + type.GetGenericArguments()[0].Name + ">";
            }
        }
        return type.Name;
    }
}
