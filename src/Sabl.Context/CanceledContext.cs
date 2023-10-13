// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;

/// <summary>
/// Internal implementation of a cancelable context that is already canceled
/// </summary>
internal class CanceledContext : EmptyContext, ICancelContext
{
    private readonly CancellationToken _cancelToken = new(true);
    private bool _explicitDisposed = false;

    public CanceledContext(IContext? parent) : base(parent, "Cancel") { }

    public override CancellationToken CancellationToken => _cancelToken;

    public void Dispose()
    {
        _explicitDisposed = true;
    }

    void ICancelContext.Cancel()
    {
        // If attempting to cancel after explicitly disposing, throw an exception
        if (_explicitDisposed) {
            throw new ObjectDisposedException("CancelContext");
        }
    }
}
