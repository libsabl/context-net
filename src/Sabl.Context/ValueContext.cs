// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;

/// <summary>Internal implementation of a context with a single key and value</summary>
internal class ValueContext : Context
{
    public ValueContext(IContext? parent, object key, object? value) : base(parent)
    {
        this.key = key ?? throw new ArgumentNullException(nameof(key));
        this.value = value;
    }

    internal object Key => key;

    internal object? OwnValue => value;

    private readonly object key;
    private readonly object? value;

    public override object? Value(object key)
    {
        if (this.key.Equals(key)) return value;
        return this.Parent?.Value(key);
    }
}
