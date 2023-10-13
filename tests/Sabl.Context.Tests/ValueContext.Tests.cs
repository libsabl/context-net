// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl.ValueContextClass;

public class Ctor
{
    [Fact]
    public void RejectsNullKey()
    {
        Assert.Throws<ArgumentNullException>("key", () => new ValueContext(null, null!, null));
    }

    [Fact]
    public void AcceptsNullValue()
    {
        var vctx = new ValueContext(null, "key", null);
        Assert.Null(vctx.Value("key"));
    }
}

public class Value
{
    [Fact]
    public void ReturnsDefinedValue()
    {
        var vctx = new ValueContext(null, "key", 42);
        Assert.Equal(42, vctx.Value("key"));
    }

    [Fact]
    public void ReturnsAncestorValue()
    {
        var vone = new ValueContext(null, "One", 1);
        var vtwo = vone.WithValue("Two", 2);
        var cctx = vtwo.WithCancel();
        var vthree = cctx.WithValue("Three", 3);

        Assert.Equal(2, vthree.Value("Two"));
        Assert.Equal(2, cctx.Value("Two"));

        Assert.Equal(1, vthree.Value("One"));
        Assert.Equal(1, cctx.Value("One"));
        Assert.Equal(1, vtwo.Value("One"));


        Assert.Null(vthree.Value("foo"));
        Assert.Null(cctx.Value("foo"));
        Assert.Null(vtwo.Value("foo"));
        Assert.Null(vone.Value("foo"));
    }
}

