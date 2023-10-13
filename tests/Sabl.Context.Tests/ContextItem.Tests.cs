// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;

namespace Sabl.ContextItemStruct;

public class Ctor
{
    [Fact]
    public void SetsKeyAndValue()
    {
        Symbol key = "encoding";
        var val = Encoding.UTF8;

        var item = new ContextItem<Encoding>(key, val);
        Assert.Same(val, item.Value);
    }

    [Fact]
    public void SetsKeyAndNullValue()
    {
        Symbol key = "encoding";

        var item = new ContextItem<Encoding>(key, null);
        Assert.Null(item.Value);
    }
}

public class Require
{
    [Fact]
    public void ReturnsValue()
    {
        Symbol key = "encoding";
        var val = Encoding.UTF8;

        var item = new ContextItem<Encoding>(key, val);
        var unwrapped = item.Require();
        Assert.Same(val, unwrapped);
    }

    [Fact]
    public void ThrowsIfNull()
    {
        Symbol key = "encoding";
        var item = new ContextItem<Encoding>(key, null);

        var ex = Assert.Throws<KeyNotFoundException>(() => item.Require());
        Assert.Equal("Context item for key Symbol(encoding) was null or undefined", ex.Message);
    }
}

public class IsNull
{
    [Fact]
    public void ReturnsFalseForItem()
    {
        Symbol key = "encoding";
        var val = Encoding.UTF8;

        var item = new ContextItem<Encoding>(key, val);
        Assert.False(item.IsNull);
    }

    [Fact]
    public void ReturnsTrueForNull()
    {
        Symbol key = "encoding";
        var item = new ContextItem<Encoding>(key, null);
        Assert.True(item.IsNull);
    }
}

public class ImplicitToT
{
    [Fact]
    public void ReturnsValue()
    {
        Symbol key = "encoding";
        var val = Encoding.UTF8;

        var item = new ContextItem<Encoding>(key, val);
        Encoding unwrapped = item;
        Assert.Same(val, unwrapped);
    }

    [Fact]
    public void ThrowsIfNull()
    {
        Symbol key = "encoding";
        var item = new ContextItem<Encoding>(key, null);

        var ex = Assert.Throws<KeyNotFoundException>(() => { Encoding unwrapped = item; });
        Assert.Equal("Context item for key Symbol(encoding) was null or undefined", ex.Message);
    }
}
