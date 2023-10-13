// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;

/// <summary>An empty context with a name</summary>
public class EmptyContext : Context
{
    /// <summary>Create a new EmptyContext</summary>
    public EmptyContext(IContext? parent, string? name = null) : base(parent, name) { }

    /// <inheritdoc />
    override public object? Value(object key) => this.Parent?.Value(key);
}
