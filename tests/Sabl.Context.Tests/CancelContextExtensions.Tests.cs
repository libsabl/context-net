using System.Threading.Tasks;

namespace Sabl.CancelContextExtensionsClass;

public class WithCancel
{
    [Fact]
    public void ReturnsCancelContext()
    {
        var ctx = Context.Background;
        var cctx = ctx.WithCancel();

        Assert.False(cctx.CancellationToken.IsCancellationRequested);

        cctx.Cancel();

        Assert.True(cctx.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void ReturnsAlreadyCanceledContext()
    {
        var parentCctx = Context.Cancel();
        parentCctx.Cancel();

        var cctx = parentCctx.WithCancel();

        // Already canceled
        Assert.True(cctx.CancellationToken.IsCancellationRequested);
    }
}

public class GetDeadline
{
    [Fact]
    public void ReturnsNull()
    {
        var dl = Context.Background.GetDeadline();
        Assert.Null(dl);
    }

    [Fact]
    public void ReturnsDeadline()
    {
        var dlInput = DateTime.UtcNow.AddDays(1);
        using var ctx = Context.Background.WithDeadline(dlInput);

        var dl = ctx.GetDeadline();
        Assert.Equal(dlInput, dl);
    }

    [Fact]
    public void ReturnsSoonestDeadline()
    {
        var dlInput1 = DateTime.UtcNow.AddMinutes(30);
        var dlInput2 = dlInput1.AddSeconds(10);
        var dlInput3 = dlInput2.AddSeconds(10);

        var cases = new DateTime[][] {
            new [] { dlInput1, dlInput2, dlInput3 },
            new [] { dlInput1, dlInput3, dlInput2 },
            new [] { dlInput2, dlInput1, dlInput3 },
            new [] { dlInput2, dlInput3, dlInput1 },
            new [] { dlInput3, dlInput1, dlInput2 },
            new [] { dlInput3, dlInput2, dlInput1 },
        };

        // No matter what order the deadlines are added, the 
        // earliest on (dlInput1) is always returned as the effective
        // deadline
        foreach (var @case in cases) {
            using var ctx1 = Context.Background.WithDeadline(@case[0]);
            using var ctx2 = ctx1.WithDeadline(@case[1]);
            using var ctx3 = ctx2.WithDeadline(@case[2]);

            var dl = ctx3.GetDeadline();
            Assert.Equal(dlInput1, dl);
        }
    }
}

public class WithDeadline
{
    [Fact]
    public async void AutomaticallyCancelsAfterDeadline()
    {
        var dl = DateTime.UtcNow.AddMilliseconds(100);
        using var dctx = Context.Background.WithDeadline(dl);

        Assert.False(dctx.IsCanceled());

        await Task.Delay(110);

        Assert.True(dctx.IsCanceled());
    }

    [Fact]
    public void ReturnsCanceledIfParentCanceled()
    {
        using var pctx = Context.Cancel();
        pctx.Cancel();

        var dlInput = DateTime.UtcNow.AddDays(1);
        using var dctx = pctx.WithDeadline(dlInput);
        Assert.IsType<CanceledContext>(dctx);
        Assert.True(dctx.IsCanceled());
    }

    [Fact]
    public void ReturnsCanceledIfDeadlineIsPast()
    {
        var dlInput = DateTime.UtcNow.AddSeconds(-1);
        using var dctx = Context.Background.WithDeadline(dlInput);
        Assert.IsType<CanceledContext>(dctx);
        Assert.True(dctx.IsCanceled());
    }

    [Fact]
    public void DefersToEarlierDeadline()
    {
        var dlInput1 = DateTime.UtcNow.AddMinutes(10);
        var dlInput2 = dlInput1.AddSeconds(10);

        using var dctxParent = Context.Background.
            WithDeadline(dlInput1);

        using var dctx = dctxParent.
            WithDeadline(dlInput2);

        Assert.False(dctx.IsCanceled());

        // Deadline is still first deadline, because
        // second deadline was later and thus ignored
        var dl = dctx.GetDeadline();

        Assert.Equal(dlInput1, dl);

        // BUT, it's a still a separate child context
        // that can be proactively canceled independent of parent
        Assert.NotSame(dctxParent, dctx);
    }

    [Fact]
    public void OverridesEarlierDeadline()
    {
        var dlInput1 = DateTime.UtcNow.AddMinutes(10);
        var dlInput2 = dlInput1.AddSeconds(-10);

        using var dctxParent = Context.Background.
            WithDeadline(dlInput1);

        using var dctx = dctxParent.
            WithDeadline(dlInput2);

        Assert.False(dctx.IsCanceled());

        // Deadline is second deadline, because it
        // is sooner than the existing deadline
        var dl = dctx.GetDeadline();

        Assert.Equal(dlInput2, dl);
    }
}

public class WithTimeout
{
    [Fact]
    public async void AutomaticallyCancelsAfterTimeout()
    {
        var tout = TimeSpan.FromMilliseconds(100);
        using var dctx = Context.Background.WithTimeout(tout);

        Assert.False(dctx.IsCanceled());

        await Task.Delay(10 + (int)tout.TotalMilliseconds);

        Assert.True(dctx.IsCanceled());
    }

    [Fact]
    public void SetsDeadline()
    {
        var tout = TimeSpan.FromSeconds(30);
        var dlExpected = DateTime.UtcNow.Add(tout);

        using var dctx = Context.Background.WithTimeout(tout);

        DateTime dl = dctx.GetDeadline()!.Value;
        Assert.Equal(dlExpected, dl, TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(-5)]
    [InlineData(0)]
    public void ReturnsCanceledIfNonPositiveTimeout(int tout)
    {
        using var dctx = Context.Background.WithTimeout(TimeSpan.FromMilliseconds(tout));
        Assert.IsType<CanceledContext>(dctx);
        Assert.True(dctx.IsCanceled());
    }
}
