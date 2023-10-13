// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;

namespace Sabl.ContextValueStruct;

public class Ctor
{
    [Fact]
    public void SetsKeyAndValue()
    {
        Symbol key = "number";
        var val = 22;

        var item = new ContextValue<int>(key, val);
        Assert.Equal(val, item.Value);
    }

    [Fact]
    public void SetsKeyAndNullValue()
    {
        Symbol key = "number";

        var item = new ContextValue<int>(key, null);
        Assert.False(item.Value.HasValue);
    }
}

public class Require
{
    [Fact]
    public void ReturnsValue()
    {
        Symbol key = "number";
        var val = 22;

        var item = new ContextValue<int>(key, val);
        var unwrapped = item.Require();
        Assert.Equal(val, unwrapped);
    }

    [Fact]
    public void ThrowsIfNull()
    {
        Symbol key = "number";
        var item = new ContextValue<int>(key, null);

        var ex = Assert.Throws<KeyNotFoundException>(() => item.Require());
        Assert.Equal("Context item for key Symbol(number) was null or undefined", ex.Message);
    }
}

public class IsNull
{
    [Fact]
    public void ReturnsFalseForItem()
    {
        Symbol key = "number";
        var val = 22;

        var item = new ContextValue<int>(key, val);
        Assert.False(item.IsNull);
    }

    [Fact]
    public void ReturnsTrueForNull()
    {
        Symbol key = "number";
        var item = new ContextValue<int>(key, null);
        Assert.True(item.IsNull);
    }
}

public class ImplicitToT
{
    [Fact]
    public void ReturnsValue()
    {
        Symbol key = "number";
        var val = 22;

        var item = new ContextValue<int>(key, val);
        int unwrapped = item;
        Assert.Equal(val, unwrapped);
    }

    [Fact]
    public void ThrowsIfNull()
    {
        Symbol key = "number";
        var item = new ContextValue<int>(key, null);

        var ex = Assert.Throws<KeyNotFoundException>(() => { int unwrapped = item; });
        Assert.Equal("Context item for key Symbol(number) was null or undefined", ex.Message);
    }
}
