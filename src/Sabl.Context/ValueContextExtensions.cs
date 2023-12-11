// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Collections.Generic;

namespace Sabl;

/// <summary>Extension methods to IContext for working with context values</summary>
public static class ValueContextExtensions
{
    /// <summary>Create a child context with the provided value set</summary>
    /// <remarks>
    /// If <paramref name="key"/> is a <see cref="ContextKey"/>, the
    /// runtime type of <paramref name="value"/> will be enforced
    /// </remarks>
    public static IContext WithValue(this IContext context, object key, object? value)
    {
        if (key is ContextKey ckey && !ContextKey.IsValueAllowed(ckey, value, out var ex)) {
            throw ex;
        }
        return new ValueContext(context, key, value);
    }

    /// <summary>Get a definitely typed class instance from the context, returning null if the item is not defined</summary> 
    public static T? Value<T>(this IContext context, ContextKey<T> key) where T : class
    {
        var raw = context.Value(key);
        if (raw is null) return null;
        if (raw is T value) return value;
        throw new InvalidCastException($"Invalid value associated with key {key.Label}");
    }

    /// <summary>Get a definitely typed struct value from the context, returning null if the item is not defined</summary> 
    /// <remarks>This overload supports keys where the the key type is a <see cref="Nullable{T}"/></remarks>
    public static T? Try<T>(this IContext context, ContextKey<T?> key) where T : struct
    {
        var raw = context.Value((object)key);
        if (raw is null) return null;
        if (raw is T value) return value;
        throw new InvalidCastException($"Invalid value associated with key {key.Label}");
    }

    /// <summary>Get a definitely typed struct value from the context, returning null if the item is not defined</summary> 
    /// <remarks>This overload supports keys where the the key type is a non-nullable value type</remarks>
    public static T? Try<T>(this IContext context, ContextKey<T> key) where T : struct
    {
        var raw = context.Value((object)key);
        if (raw is null) return null;
        if (raw is T value) return value;
        throw new InvalidCastException($"Invalid value associated with key {key.Label}");
    }

    /// <summary>Get a definitely typed value from the context, throwing an exception if the item is null or undefined</summary> 
    public static T Require<T>(this IContext context, ContextKey<T> key)
    {
        var raw = context.Value((object)key) ?? throw new KeyNotFoundException($"Context item for key {key.Label} was null or undefined");
        if (raw is T value) return value;
        throw new InvalidCastException($"Invalid value of type {raw.GetType().FullName} associated with key {key.Label} for type {key.ValueType.FullName}");
    }

    /// <summary>
    /// Iterate through all key-value pairs reachable from the context
    /// </summary>
    /// <param name="context">The starting context</param>
    /// <param name="distinct">Whether to return only one key-value pair per unique key value</param>
    /// <remarks>
    /// Any intermediate contexts that to do not implement <see cref="IChildContext"/> will terminate
    /// the iteration.
    /// </remarks>
    public static IEnumerable<KeyValuePair<object, object?>> AllValues(this IContext context, bool distinct = false)
        => distinct ? AllDistinctValues(context) : AllValuesNonDistinct(context);

    private static IEnumerable<KeyValuePair<object, object?>> AllValuesNonDistinct(IContext context)
    {
        var cctx = context as IChildContext;
        while (cctx != null) {
            if (cctx is ValueContext vctx) {
                yield return new KeyValuePair<object, object?>(vctx.Key, vctx.OwnValue);
            }
            cctx = cctx.Parent as IChildContext;
        }
    }

    private static IEnumerable<KeyValuePair<object, object?>> AllDistinctValues(IContext context)
    {
        var cctx = context as IChildContext;
        if (cctx == null)
            yield break;

        var keys = new HashSet<object>();
        while (cctx != null) {
            if (cctx is ValueContext vctx && keys.Add(vctx.Key)) {
                yield return new KeyValuePair<object, object?>(vctx.Key, vctx.OwnValue);
            }
            cctx = cctx.Parent as IChildContext;
        }
    }
}
