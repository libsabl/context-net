// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Collections.Generic;

namespace Sabl;

/// <summary>Extension methods to IContext for working with context values</summary>
public static class ValueContextExtensions
{
    /// <summary>Create a child context with the provided value set</summary>
    public static IContext WithValue(this IContext context, object key, object? value)
    {
        if (key is ContextKey ckey) {
            if (value == null) {
                if (!ckey.Nullable) {
                    throw new ArgumentNullException(nameof(value), $"Cannot use null value for value of non-nullable type {ckey.ValueType}");
                }
            } else {
                if (!value.GetType().IsAssignableTo(ckey.ValueType)) {
                    throw new InvalidCastException($"Provided value cannot be assigned to type {ckey.ValueType} associated with key {ckey.Label}");
                }
            }
        }

        return new ValueContext(context, key, value);
    }

    /// <summary>Create a child context with the provided definitely type value set</summary>
    public static IContext WithValue<T>(this IContext context, ContextKey<T> key, T value)
        => new ValueContext(context, key, value);

    /// <summary>Get a definitely typed class instance from the context, returning null if the item is not defined</summary> 
    public static T? Get<T>(this IContext context, ContextKey<T> key) where T : class?
    {
        var raw = context.Value(key);
        if (raw is null) return null;
        if (raw is T value) return value; 
        throw new InvalidCastException($"Invalid value associated with key {key.Label}");
    }

    /// <summary>Get a definitely typed struct value from the context, returning null if the item is not defined</summary> 
    public static T? GetValue<T>(this IContext context, ContextKey<T> key) where T : struct
    {
        var raw = context.Value(key);
        if (raw is null) return null;
        if (raw is T value) return value; 
        throw new InvalidCastException($"Invalid value associated with key {key.Label}");
    }

    /// <summary>Get a definitely typed value from the context, throwing an exception if the item is null or undefined</summary> 
    public static T Require<T>(this IContext context, ContextKey<T> key)
    {
        var raw = context.Value(key) ?? throw new KeyNotFoundException($"Context item for key {key.Label} was null or undefined");
        if (raw is T value) return value;
        throw new InvalidCastException($"Invalid value associated with key {key.Label}");
    }

}
