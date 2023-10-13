// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;

namespace Sabl.ValueContextExtensionsClass;

public class WithValue
{
    [Fact]
    public void ReturnsAValueContext()
    {
        var key = "key";
        var ctx = Context.Background;

        var vctx = ctx.WithValue(key, 42);
        Assert.IsType<ValueContext>(vctx);

        var v = vctx.Value(key);
        Assert.Equal(42, v);
    }

    [Fact]
    public void AllowsNullForNullableCKey()
    {
        var key = new ContextKey<int?>();
        var ctx = Context.Background;
        var vctx = ctx.WithValue((object)key, null);

        Assert.IsType<ValueContext>(vctx);
    }

    [Fact]
    public void AllowsValueForNonNullableCKey()
    {
        var key = new ContextKey<int>();
        var ctx = Context.Background;
        var vctx = ctx.WithValue((object)key, 42);

        Assert.IsType<ValueContext>(vctx);
        var v = vctx.Value(key);
        Assert.Equal(42, v);
    }

    [Fact]
    public void AllowsValueForNullableCKey()
    {
        var key = new ContextKey<int?>();
        var ctx = Context.Background;
        var vctx = ctx.WithValue((object)key, 42);

        Assert.IsType<ValueContext>(vctx);
        var v = vctx.Value(key);
        Assert.Equal(42, v);
    }

    [Fact]
    public void AllowsNullForRefCKey()
    {
        var key = new ContextKey<object>();
        var ctx = Context.Background;
        var vctx = ctx.WithValue((object)key, null);

        Assert.IsType<ValueContext>(vctx);
    }

    [Fact]
    public void RejectsNullForNonNullableCKey()
    {
        var key = new ContextKey<int>();
        var ctx = Context.Background;

        var ex = Assert.Throws<ArgumentNullException>(() => ctx.WithValue((object)key, null));
        var expectedMessage = "Cannot use null value for value of non-nullable type System.Int32 (Parameter 'value')";
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void RejectsWrongValueType()
    {
        var key = new ContextKey<int>();
        var ctx = Context.Background;

        var ex = Assert.Throws<InvalidCastException>(() => ctx.WithValue((object)key, DateTime.Now));
        var expectedMessage = "Provided value cannot be assigned to type System.Int32 associated with key Int32";
        Assert.Equal(expectedMessage, ex.Message);
    }
}

public class WithValueOfT
{
    [Fact]
    public void AssignsValue()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, 42);
        var v = vctx.Value(key);
        Assert.Equal(42, v);
    }
}

public class GetOfT
{
    [Fact]
    public void ReturnsValue()
    {
        ContextKey<Encoding> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, Encoding.UTF8);
        var v = vctx.Get(key);
        Assert.Same(Encoding.UTF8, v);
    }

    [Fact]
    public void ReturnsNull()
    {
        ContextKey<Encoding> key = new();
        var ctx = Context.Background;
        var v = ctx.Get(key);
        Assert.Null(v);
    }

    [Fact]
    public void ThrowsInvalidCast()
    {
        ContextKey<Encoding> key = new();
        var ctx = new ValueContext(Context.Background, key, "boom");
        var ex = Assert.Throws<InvalidCastException>(() => ctx.Get(key));
        Assert.Equal("Invalid value associated with key Encoding", ex.Message);
    }
}


public class GetValueOfT
{
    [Fact]
    public void ReturnsValue()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, 22);
        var v = vctx.GetValue(key);
        Assert.Equal(22, v);
    }

    [Fact]
    public void ReturnsNull()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var v = ctx.GetValue(key);
        Assert.Null(v);
    }

    [Fact]
    public void ThrowsInvalidCast()
    {
        ContextKey<int> key = new();
        var ctx = new ValueContext(Context.Background, key, "boom");
        var ex = Assert.Throws<InvalidCastException>(() => ctx.GetValue(key));
        Assert.Equal("Invalid value associated with key Int32", ex.Message);
    }
}

public class RequireOfT
{

    [Fact]
    public void ReturnsStructValue()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, 22);
        var v = vctx.Require(key);
        Assert.Equal(22, v);
    }

    [Fact]
    public void ReturnsRefValue()
    {
        ContextKey<Encoding> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, Encoding.UTF8);
        var v = vctx.Require(key);
        Assert.Same(Encoding.UTF8, v);
    }

    [Fact]
    public void ThrowsForNullStruct()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var ex = Assert.Throws<KeyNotFoundException>(() => ctx.Require(key));
        Assert.Equal("Context item for key Int32 was null or undefined", ex.Message);
    }

    [Fact]
    public void ThrowsForNullRef()
    {
        ContextKey<Encoding> key = new();
        var ctx = Context.Background;
        var ex = Assert.Throws<KeyNotFoundException>(() => ctx.Require(key));
        Assert.Equal("Context item for key Encoding was null or undefined", ex.Message);
    }

    [Fact]
    public void ThrowsInvalidCast()
    {
        ContextKey<Encoding> key = new();
        var ctx = new ValueContext(Context.Background, key, "boom");
        var ex = Assert.Throws<InvalidCastException>(() => ctx.Require(key));
        Assert.Equal("Invalid value associated with key Encoding", ex.Message);
    }
}
