﻿namespace Microsoft.PSharp.ServiceFabric
{
    using Microsoft.ServiceFabric.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IReactiveReliableQueue<T>
    {
        Task EnqueueAsync(ITransaction tx, T item);

        Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx, CancellationToken cancellationToken);

        Task<long> GetCountAsync(ITransaction tx);
    }
}
