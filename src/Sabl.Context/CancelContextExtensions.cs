// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;

/// <summary>Extension methods to IContext for working with cancelable contexts</summary>
public static class CancelContextExtensions
{

    /// <summary>
    /// Shorthand check to see if cancellation token associated with the context is 
    /// cancelable (<see cref="CancellationToken.CanBeCanceled"/>) and is in 
    /// fact canceled (<see cref="CancellationToken.IsCancellationRequested"/>)
    /// </summary>
    public static bool IsCanceled(this IContext context)
    {
        var token = context.CancellationToken;
        return token.CanBeCanceled && token.IsCancellationRequested;
    }

    /// <summary>Create a cancelable child context</summary>
    public static ICancelContext WithCancel(this IContext context)
    {
        if (IsCanceled(context)) {
            // Parent is already canceled
            return new CanceledContext(context);
        }

        return new CancelContext(context);
    }

    private readonly static ContextKey<DateTime> ctxKeyDeadline = new("deadline");

    /// <summary>
    /// Get the scheduled deadline when the context will timeout, if any
    /// </summary> 
    public static DateTime? GetDeadline(this IContext context) => context.Value(ctxKeyDeadline) as DateTime?;

    /// <summary>
    /// Create a new child cancelable context which will automatically cancel at <paramref name="deadline"/>
    /// </summary>
    public static ICancelContext WithDeadline(this IContext context, DateTime deadline)
    {
        if (IsCanceled(context)) {
            // Parent is already canceled
            return new CanceledContext(context);
        }

        var now = DateTime.UtcNow;
        if (deadline < now) {
            // Already past deadline 
            return new CanceledContext(context);
        }

        var existing = GetDeadline(context);
        if (existing != null && existing.Value < deadline) {
            // Existing deadline is already earlier.
            return new CancelContext(context);
        }

        var cctx = new CancelContext(context.WithValue(ctxKeyDeadline, deadline));
        cctx.TokenSource.CancelAfter(deadline - now);
        return cctx;
    }

    /// <summary>
    /// Create a new child cancelable context which will automatically cancel after <paramref name="timeout"/>
    /// </summary>
    public static ICancelContext WithTimeout(this IContext context, TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero) {
            return new CanceledContext(context);
        }

        var deadline = DateTime.UtcNow + timeout;
        return WithDeadline(context, deadline);
    }

}
