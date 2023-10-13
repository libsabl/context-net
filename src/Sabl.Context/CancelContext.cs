// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;
 
/// <summary>Internal implementation of a cancelable context</summary>
/// <remarks>
/// <see cref="Cancel"/> and <see cref="Dispose"/> will both dispose the underlying 
/// <see cref="CancellationTokenSource"/>. However, calling <see cref="Dispose"/> will
/// not cancel the context, and will prevent any subsequent calls to <see cref="Cancel"/>.
/// </remarks>
internal class CancelContext : EmptyContext, ICancelContext
{
    private readonly CancellationTokenSource _tokenSource;
    private readonly CancellationToken _cancelToken;
    private bool _unregisterStarted = false;
    private bool _explicitDisposed = false;

    public CancelContext(IContext? parent) : base(parent, "Cancel")
    {
        if (parent == null || !parent.CancellationToken.CanBeCanceled) {
            _tokenSource = new CancellationTokenSource();
        } else {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(parent.CancellationToken);
        }

        _cancelToken = _tokenSource.Token;
    }

    override public CancellationToken CancellationToken => _cancelToken;

    internal CancellationTokenSource TokenSource => _tokenSource;

    new public void Cancel()
    {
        // If attempting to cancel after explicitly disposing, throw an exception
        if (_explicitDisposed) {
            throw new ObjectDisposedException("CancelContext");
        }

        // If internally unregistered, just silently return
        if (_unregisterStarted) return;

        _tokenSource.Cancel();
        this.Unregister();
    }

    public void Dispose()
    {
        _explicitDisposed = true;
        this.Unregister();
    }

    private void Unregister()
    {
        if (_unregisterStarted) return;
        _unregisterStarted = true;

        // A linked CancellationTokenSource is unregistered by disposing it
        _tokenSource.Dispose();
    }
}
