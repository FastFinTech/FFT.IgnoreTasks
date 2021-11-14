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
    private static readonly Action<Task> _observeException = t => { _ = t.Exception; };

    /// <summary>
    /// Prevent unobserved exceptions and other performance degradations on a
    /// task that you wish to fire-and-forget.
    /// <see href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
    /// </summary>
    /// <param name="task">The task to be ignored.</param>
    public static void Ignore(this Task task)
    {
      if (task.IsCompleted)
      {
        _ = task.Exception;
      }
      else
      {
        task.ContinueWith(
            _observeException,
            CancellationToken.None,
            OnlyOnFaulted | ExecuteSynchronously,
            TaskScheduler.Default);
      }
    }

    /// <summary>
    /// Prevent unobserved exceptions and other performance degradations caused
    /// by an unobserved result on a ValueTask that you wish to fire-and-forget.
    /// <see href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
    /// </summary>
    /// <param name="task">The task to be ignored.</param>
    public static void Ignore(this ValueTask task)
    {
      if (task.IsCompleted)
      {
        try
        {
          task.GetAwaiter().GetResult();
        }
        catch { }
      }
      else
      {
        task.GetAwaiter().OnCompleted(() =>
        {
          try
          {
            task.GetAwaiter().GetResult();
          }
          catch { }
        });
      }
    }

    /// <summary>
    /// Prevent unobserved exceptions and other performance degradations caused
    /// by an unobserved result on a ValueTask that you wish to fire-and-forget.
    /// <see href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
    /// </summary>
    /// <param name="task">The task to be ignored.</param>
    public static void Ignore<T>(this ValueTask<T> task)
    {
      if (task.IsCompleted)
      {
        try
        {
          _ = task.GetAwaiter().GetResult();
        }
        catch { }
      }
      else
      {
        task.GetAwaiter().OnCompleted(() =>
        {
          try
          {
            _ = task.GetAwaiter().GetResult();
          }
          catch { }
        });
      }
    }

    /// <summary>
    /// Prevents performance degradation by observing the result/exception of a
    /// <see cref="ValueTask{TResult}"/> while converting it to a <see
    /// cref="ValueTask"/>. <see
    /// href="https://github.com/FastFinTech/FFT.IgnoreTasks"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing completion of the <paramref
    /// name="task"/>.
    /// </returns>
    public static ValueTask WithoutResult<T>(this ValueTask<T> task)
    {
      if (task.IsCompletedSuccessfully)
      {
        _ = task.GetAwaiter().GetResult();
        return default;
      }

      return ToValueTaskAsync(task);

      static async ValueTask ToValueTaskAsync(ValueTask<T> task)
      {
        _ = await task;
      }
    }
  }
}
