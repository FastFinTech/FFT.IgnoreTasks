# FFT.IgnoreTasks

[![NuGet package](https://img.shields.io/nuget/v/FFT.IgnoreTasks.svg)](https://nuget.org/packages/FFT.IgnoreTasks)

Provides methods for ignoring `Task` and `ValueTask` objects as they complete in the background, observing but not throwing exceptions, and preventing memory-leak performance degradation.

### Ignoring `Task`

If a Task fails and throws an exception which is never observed, it may be caught by the .NET finalizer thread, potentially crashing your application, depending on framework version and application settings.

Calling `Ignore()` on a fire-and-forget `Task` helps you ensure this can never happen. The method is allocation-free and efficient.

```csharp
using System.IO;
using FFT.TaskIgnorer;
// Kick off the background task and ignore the completion.
// .Ignore() ensures that if the task fails, the exception won't be thrown by the finalizer thread.
File.WriteAllTextAsync("filename", "abc").Ignore();
```

### Ignoring `ValueTask`

These two articles provide very helpful information about correct usage patterns and pitfalls of using `ValueTask`

- [Stephen Toub - Understanding `ValueTask`](https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/#valid-consumption-patterns-for-valuetasks)
- [Stephen Cleary - `ValueTask` restrictions](https://blog.stephencleary.com/2020/03/valuetask.html)

In addition, [Code Analysis Rule CA2012](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2012?view=vs-2019) states that "Ignoring a ValueTask is likely an indication of a functional bug and may degrade performance". In other words, code like this can degrade performance: 

```csharp
// WRONG! Don't do this!
// send the data fire and forget style and don't observe the result
_ = _socket.SendAsync(myBytes); // did not await the `ValueTask`
```

At the time of writing, a `ValueTask` can be either of three kinds:

- Result value wrapper
- Reference `Task` wrapper
- `ValueTaskSource` wrapper

Most of the time, you don't know which of these kinds is used by the method returning the `ValueTask`. Often the kind will vary depending on whether the method was able to complete itself synchronously.

Efficiently-written libraries often use object pooling to reuse `ValueTaskSource` objects. If you do not await or observe the result of the `ValueTask`, the underlying system is unable to recycle those objects back to their pool, and a memory leak occurs.

Therefore, even if YOU don't care about the result of the task you want to fire-and-forget, the underlying system needs you do make sure the `ValueTask` is observed so it can do its job.

Here's how to do it using `FFT.IgnoreTasks`

```csharp
using System.Net.Sockets;
using FFT.IgnoreTasks;
var socket = GetSomeSocket();
// Fire and forget a socket send operation without caring about the result.
// Use the .Ignore() method to make sure everything is cleaned up properly.
socket.SendAsync(new byte[]{1,2,3}).Ignore();
```


