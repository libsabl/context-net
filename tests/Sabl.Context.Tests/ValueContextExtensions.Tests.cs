// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
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
        var expectedMessage = "Provided value of type System.DateTime cannot be assigned to type System.Int32 associated with key Int32";
        Assert.Equal(expectedMessage, ex.Message);
    }
}

public class ValueOfT
{
    [Fact]
    public void ReturnsValue()
    {
        ContextKey<Encoding> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, Encoding.UTF8);
        var v = ValueContextExtensions.Value(vctx, key);
        Assert.Same(Encoding.UTF8, v);
    }

    [Fact]
    public void ReturnsNull()
    {
        ContextKey<Encoding> key = new();
        var ctx = Context.Background;
        var v = ValueContextExtensions.Value(ctx, key);
        Assert.Null(v);
    }

    [Fact]
    public void ThrowsInvalidCast()
    {
        ContextKey<Encoding> key = new();
        var ctx = new ValueContext(Context.Background, key, "boom");
        var ex = Assert.Throws<InvalidCastException>(() => ValueContextExtensions.Value(ctx, key));
        Assert.Equal("Invalid value associated with key Encoding", ex.Message);
    }
}

public class TryOfNullableT
{
    [Fact]
    public void ReturnsValue()
    {
        ContextKey<int?> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, 22);
        var v = vctx.Try(key);
        Assert.Equal(22, v);
    }

    [Fact]
    public void ReturnsNull()
    {
        ContextKey<int?> key = new();
        var ctx = Context.Background;
        var v = ctx.Try(key);
        Assert.Null(v);
    }

    [Fact]
    public void ThrowsInvalidCast()
    {
        ContextKey<int?> key = new();
        var ctx = (IContext)new ValueContext(Context.Background, key, "boom");
        var ex = Assert.Throws<InvalidCastException>(() => ctx.Try(key));
        Assert.Equal("Invalid value associated with key Nullable<Int32>", ex.Message);
    }
}

public class TryOfT
{
    [Fact]
    public void ReturnsValue()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var vctx = ctx.WithValue(key, 22);
        var v = vctx.Try(key);
        Assert.Equal(22, v);
    }

    [Fact]
    public void ReturnsNull()
    {
        ContextKey<int> key = new();
        var ctx = Context.Background;
        var v = ctx.Try(key);
        Assert.Null(v);
    }

    [Fact]
    public void ThrowsInvalidCast()
    {
        ContextKey<int> key = new();
        var ctx = (IContext)new ValueContext(Context.Background, key, "boom");
        var ex = Assert.Throws<InvalidCastException>(() => ctx.Try(key));
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
        Assert.Equal("Invalid value of type System.String associated with key Encoding for type System.Text.Encoding", ex.Message);
    }
}

public class AllValues
{
    private static readonly ContextKey<string> strKey1 = new("item 1");
    private static readonly ContextKey<string> strKey2 = new("item 2");
    private static readonly ContextKey<int> numKey = new("number");

    private static readonly IContext ctx = Context.Background
        .WithValue(strKey1, "Hello")
        .WithValue(strKey2, "Hi")
        .WithCancel()
        .WithValue(numKey, 22)
        .WithValue(strKey2, "Another value");

    [Fact]
    public void ReturnsAllKeyValuePairs()
    {

        var pairs = ctx.AllValues().ToList();
        var expected = new List<KeyValuePair<object, object?>> {
            new(strKey2, "Another value"),
            new(numKey, 22),
            new(strKey2, "Hi"),
            new(strKey1, "Hello"),
        };

        Assert.Equal(expected, pairs);
    }

    [Fact]
    public void ReturnsDistinctKeyValuePairs()
    {
        var pairs = ctx.AllValues(true).ToList();
        var expected = new List<KeyValuePair<object, object?>> {
            new(strKey2, "Another value"),
            new(numKey, 22),
            new(strKey1, "Hello"),
        };

        Assert.Equal(expected, pairs);
    }

    private class SillyContext : IContext
    {
        public IContext? MyParent { get; }

        public SillyContext(IContext? myParent)
        {
            this.MyParent = myParent;
        }

        public CancellationToken CancellationToken
            => this.MyParent?.CancellationToken ?? CancellationToken.None;

        public object? Value(object key)
            => this.MyParent?.Value(key);
    }

    [Fact]
    public void ReturnsEmptyForNonChildContext()
    {
        var sillyCtx = new SillyContext(ctx);

        // Yes, it implements .Value and will return items by key
        Assert.Equal("Hello", sillyCtx.Value(strKey1));
        Assert.Equal("Another value", sillyCtx.Value(strKey2));
        Assert.Equal(22, sillyCtx.Value(numKey));

        // But because it does not implement IChildContext, AllValues returns empty
        var pairs = sillyCtx.AllValues(true);
        Assert.Empty(pairs);
    }
}
