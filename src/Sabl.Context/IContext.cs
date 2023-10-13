// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

namespace Sabl;

/// <summary>IContext is a simple interface for immutable context trees</summary>
public interface IContext
{
    /// <summary>Retrieve a named value by its key</summary>
    /// <remarks>Value should simply return null if the key is not defined</remarks>
    object? Value(object key);

    /// <summary>The cancellation token for the context</summary>
    CancellationToken CancellationToken { get; }
}

/// <summary>A cancelable, disposable <see cref="IContext"/></summary>
/// <remarks>
/// Callers must ensure the context is disposed after use, such as with a using block. 
/// It is safe to call <see cref="Cancel"/> many times, but <see cref="Cancel"/> cannot 
/// be called after the context has been disposed.
/// </remarks>
public interface ICancelContext : IContext, IDisposable
{
    /// <summary>
    /// Cancel the context and all descendant contexts
    /// </summary>
    void Cancel();
}
