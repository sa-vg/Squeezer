using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Compressor
{
    internal static class ProducerConsumerExtensions
    {
        public static void Fill<T>(this BlockingCollection<T> buffer, IEnumerable<T> source, 
            CancellationToken cancellationToken)
        {
            foreach (var item in source)
            {
                if (cancellationToken.IsCancellationRequested) break;
                buffer.Add(item, cancellationToken);
            }
        }

        public static void TransformItems<T>(this BlockingCollection<T> source, Func<T, T> action,
            BlockingCollection<T> target,
            CancellationToken cancellationToken)
        {
            foreach (var item in source.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested) break;
                target.Add(action(item), cancellationToken);
            }
        }

        public static void Consume<T>(this BlockingCollection<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            foreach (var item in source.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested) break;
                action(item);
            }
        }
    }
}