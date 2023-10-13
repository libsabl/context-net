// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl.CanceledContextClass;

public class Ctor
{
    [Fact]
    public void IsAlreadyCanceled()
    {
        var ctx = new CanceledContext(null);
        Assert.True(ctx.CancellationToken.CanBeCanceled);
        Assert.True(ctx.CancellationToken.IsCancellationRequested);
    }
}

public class CancelDispose
{

    [Fact]
    public void CannotCancelAfterDispose()
    {
        ICancelContext cctx = new CanceledContext(null);
        var token = cctx.CancellationToken;

        cctx.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cctx.Cancel());
    }

    [Fact]
    public void CanCancelManyTimes()
    {
        ICancelContext cctx = new CanceledContext(null);
        var token = cctx.CancellationToken;

        // Already starts canceled
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
        var cctx = new CanceledContext(null);

        cctx.Dispose();

        // Should not throw
        cctx.Dispose();
        cctx.Dispose();
        cctx.Dispose();
    }
}

