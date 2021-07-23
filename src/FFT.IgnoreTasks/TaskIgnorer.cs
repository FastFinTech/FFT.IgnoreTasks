// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1501 // Statement should not be on a single line
#pragma warning disable SA1618 // Generic type parameters should be documented

namespace FFT.IgnoreTasks
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using static System.Threading.Tasks.TaskContinuationOptions;

  /// <summary>
  /// Provides methods for ignoring <see cref="Task"/> and <see
  /// cref="ValueTask"/> objects as they complete in the background, observing
  /// but not throwing exceptions, and preventing memory-leak performance
  /// degradation.
  /// <see href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
  /// </summary>
  public static class TaskIgnorer
  {
    private static readonly Action<Task> _observeExeption = t => { _ = t.Exception; };

    /// <summary>
    /// Prevent unobserved exceptions and other performance degradations on a task that you wish to fire-and-forget.
    /// <param name="task">The task to be ignored.</param>
    /// </summary>
    public static void Ignore(this Task task)
    {
      // TODO: Benchmark see if it's worthwhile reading the task.Status
      // property to find a way to gain a little performance.
      if (task.IsCompleted)
      {
        _ = task.Exception;
      }
      else
      {
        task.ContinueWith(
            _observeExeption,
            CancellationToken.None,
            OnlyOnFaulted | ExecuteSynchronously,
            TaskScheduler.Default);
      }
    }

    /// <summary>
    /// Prevent unobserved exceptions and other performance degradations on a task that you wish to fire-and-forget.
    /// <param name="task">The task to be ignored.</param>
    /// <see href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
    /// </summary>
    public static void Ignore(this ValueTask task)
    {
      if (task.IsCompleted)
      {
        // TODO: Benchmark see if a success completed check eliding the
        // try/catch is a worthwhile performance gain.
        try { task.GetAwaiter().GetResult(); } catch { }
      }
      else
      {
        task.AsTask().ContinueWith(
            _observeExeption,
            CancellationToken.None,
            OnlyOnFaulted | ExecuteSynchronously,
            TaskScheduler.Default);
      }
    }

    /// <summary>
    /// Prevent unobserved exceptions and other performance degradations on a task that you wish to fire-and-forget.
    /// <param name="task">The task to be ignored.</param>
    /// <see href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
    /// </summary>
    public static void Ignore<T>(this ValueTask<T> task)
    {
      if (task.IsCompleted)
      {
        // TODO: Benchmark see if a success completed check eliding the
        // try/catch is a worthwhile performance gain.
        try { task.GetAwaiter().GetResult(); } catch { }
      }
      else
      {
        task.AsTask().ContinueWith(
            _observeExeption,
            CancellationToken.None,
            OnlyOnFaulted | ExecuteSynchronously,
            TaskScheduler.Default);
      }
    }
  }
}
