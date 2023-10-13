// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;

/// <summary>
/// A base implementation of <see cref="IContext"/> that 
/// provides a name and an inner context. The inner context is neither
/// required nor guaranteed to be non-null
/// </summary>
public abstract class Context : IContext
{
    private readonly CancellationToken _parentToken;

    /// <summary>Create a new Context</summary>
    public Context(IContext? parent, string? name = null)
    {
        this.Parent = parent;
        this.Name = name;
        _parentToken = parent?.CancellationToken ?? CancellationToken.None;
    }

    /// <summary>The parent context. May be null</summary>
    public IContext? Parent { get; }

    /// <summary>A display name for the context or context type</summary>
    public string? Name { get; }

    /// <inheritdoc />
    public override string ToString() => this.Name ?? this.GetType().Name;

    /// <inheritdoc />
    public abstract object? Value(object key);

    /// <inheritdoc />
    public virtual CancellationToken CancellationToken => _parentToken;

    /// <summary>An empty root background context</summary>
    public static IContext Background { get; } = new EmptyContext(null, "Background");

    /// <summary>An empty context that indicates incomplete implementation</summary>
    public static IContext TODO { get; } = new EmptyContext(null, "TODO");

    /// <summary>Create a new empty root context</summary>
    public static IContext Empty(string? name) => new EmptyContext(null, name);

    /// <summary>Create a cancelable context along with its associated <see cref="CancellationTokenSource"/>, based on <see cref="Background"/></summary>
    public static ICancelContext Cancel() => Background.WithCancel();

    /// <summary>Create a new context with a value, based on <see cref="Context.Background"/></summary>
    public static IContext Value(object key, object? value) => Context.Background.WithValue(key, value);

}
