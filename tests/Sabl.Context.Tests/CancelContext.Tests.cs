// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl.CancelContextClass;

public class Ctor
{
    [Fact]
    public void CalledCancel()
    {
        using var cctx = new CancelContext(null);
        Assert.Equal("Cancel", cctx.Name);
        cctx.TokenSource.Dispose();
    }
}

public class CancelDispose
{
    [Fact]
    public void DisposeDoesNotCancel()
    {
        using var cctx = new CancelContext(null);
        var token = cctx.CancellationToken;

        Assert.False(token.IsCancellationRequested);

        cctx.Dispose();

        Assert.False(token.IsCancellationRequested);
    }

    [Fact]
    public void CannotCancelAfterDispose()
    {
        using var cctx = new CancelContext(null);
        var token = cctx.CancellationToken;

        Assert.False(token.IsCancellationRequested);

        cctx.Dispose();

        Assert.False(token.IsCancellationRequested);

        Assert.Throws<ObjectDisposedException>(() => cctx.Cancel());
    }

    [Fact]
    public void CanCancelManyTimes()
    {
        using var cctx = new CancelContext(null);
        var token = cctx.CancellationToken;

        Assert.False(token.IsCancellationRequested);

        cctx.Cancel();

        Assert.True(token.IsCancellationRequested);

        // Should not throw
        cctx.Cancel();
        cctx.Cancel();
        cctx.Cancel();

        Assert.True(token.IsCancellationRequested);

        cctx.Dispose();
    }

    [Fact]
    public void CanDisposeManyTimes()
    {
        using var cctx = new CancelContext(null);

        cctx.Dispose();

        // Should not throw
        cctx.Dispose();
        cctx.Dispose();
        cctx.Dispose();
    }
}

public class Tree
{
    [Fact]
    public void ParentCancelsChild()
    {
        using var ctxParent = Context.Background.WithCancel();
        using var ctxChild = ctxParent.WithValue("One", 1).WithValue("Two", 2).WithCancel();

        Assert.False(ctxParent.CancellationToken.IsCancellationRequested);
        Assert.False(ctxChild.CancellationToken.IsCancellationRequested);

        ctxParent.Cancel();

        Assert.True(ctxChild.CancellationToken.IsCancellationRequested);
        Assert.True(ctxParent.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void DisposeAutomaticallyUnregisters()
    {
        using var ctxParent = new CancelContext(null);
        using var ctxChild = ctxParent.WithValue("One", 1).WithValue("Two", 2).WithCancel();

        var regCnt = ctxParent.TokenSource.GetRegistrationCount();
        Assert.Equal(1, regCnt);

        ctxChild.Dispose();

        regCnt = ctxParent.TokenSource.GetRegistrationCount();
        Assert.Equal(0, regCnt);
    }

    [Fact]
    public void CancelAutomaticallyUnregisters()
    {
        using var ctxParent = new CancelContext(null);
        using var ctxChild = ctxParent.WithValue("One", 1).WithValue("Two", 2).WithCancel();

        var regCnt = ctxParent.TokenSource.GetRegistrationCount();
        Assert.Equal(1, regCnt);

        ctxChild.Cancel();

        regCnt = ctxParent.TokenSource.GetRegistrationCount();
        Assert.Equal(0, regCnt);
    }

    [Fact]
    public void ChildDoesNotCancelParent()
    {
        using var ctxParent = Context.Cancel();
        using var ctxChild = ctxParent.WithValue("One", 1).WithValue("Two", 2).WithCancel();

        Assert.False(ctxParent.CancellationToken.IsCancellationRequested);
        Assert.False(ctxChild.CancellationToken.IsCancellationRequested);

        ctxChild.Cancel();

        Assert.True(ctxChild.CancellationToken.IsCancellationRequested);
        Assert.False(ctxParent.CancellationToken.IsCancellationRequested);
    }
}
