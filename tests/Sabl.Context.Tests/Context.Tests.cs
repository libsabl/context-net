// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl.ContextClass;

public class Ctor
{

    [Fact]
    public void AcceptsNameAndParent()
    {
        var ex = new ExampleContext(null);
        Assert.Null(ex.Parent);
        Assert.Null(ex.Name);

        var ex2 = new ExampleContext(Context.Background, "SuperContext");
        Assert.Same(Context.Background, ex2.Parent);
        Assert.Equal("SuperContext", ex2.Name);
    }
}

public class CancellationTokenTest
{
    [Fact]
    public void ReturnsNoneCancellationToken()
    {
        var ex = new ExampleContext(null);
        Assert.Equal(CancellationToken.None, ex.CancellationToken);
    }

    [Fact]
    public void ReturnsParentCancellationToken()
    {
        var cctx = new CancelContext(null);
        var ex = new ExampleContext(cctx);
        Assert.Equal(cctx.CancellationToken, ex.CancellationToken);
    }
}

public class ToStringTests
{
    [Fact]
    public void ReturnsExplicitName()
    {
        var ex = new ExampleContext(null, "SuperContext");
        Assert.Equal("SuperContext", ex.ToString());
    }

    [Fact]
    public void FallsBackToTypeName()
    {
        var ex = new ExampleContext(null);
        Assert.Equal("ExampleContext", ex.ToString());
    }
}

public class Background
{
    [Fact]
    public void ReturnsSingleton()
    {
        var bgctx = Context.Background;
        var bgctx2 = Context.Background;
        Assert.Same(bgctx, bgctx2);
    }

    [Fact]
    public void CalledBackground()
    {
        Assert.Equal("Background", Context.Background.ToString());
    }

    [Fact]
    public void NoParent()
    {
        var bgctx = Context.Background as Context;
        Assert.Null(bgctx!.Parent);
    }
}

public class TODO
{
    [Fact]
    public void ReturnsSingleton()
    {
        var tdctx = Context.TODO;
        var tdctx2 = Context.TODO;
        Assert.Same(tdctx, tdctx2);
    }

    [Fact]
    public void CalledTODO()
    {
        Assert.Equal("TODO", Context.TODO.ToString());
    }

    [Fact]
    public void NoParent()
    {
        var tdctx = Context.TODO as Context;
        Assert.Null(tdctx!.Parent);
    }
}

public class Empty
{
    [Fact]
    public void ReturnsNewEmptyContext()
    {
        var ectx = Context.Empty("Foo");
        var ectx2 = Context.Empty("Foo");
        Assert.NotSame(ectx, ectx2);
    }

    [Fact]
    public void UsesProvidedName()
    {
        var ectx = Context.Empty("Foo");
        Assert.Equal("Foo", ectx.ToString());
    }

    [Fact]
    public void NoParent()
    {
        var ectx = Context.Empty("Foo") as Context;
        Assert.Null(ectx!.Parent);
    }

    [Fact]
    public void ValueReturnsNull()
    {
        var ectx = Context.Empty("Foo");
        foreach (object? key in new object?[] { null, 22, "hello", DateTime.Now }) {
            Assert.Null(ectx.Value(key!));
        }
    }

    [Fact]
    public void ValueReturnsAncestor()
    {
        var ctxBase = Context.Background.WithValue("a", 1);
        var ectx = new EmptyContext(ctxBase, "Foo");
        Assert.Equal(1, ectx.Value("a"));
    }
}
