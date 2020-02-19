using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Compressor
{
    public class Pipeline
    {
        private readonly Config _config;
        private readonly BlockingCollection<Block> _readBuffer;
        private readonly BlockingCollection<Block> _writeBuffer;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly CancellationToken _cancellationToken;
        private Exception _exception;

        public Pipeline(Config config, CancellationToken cancellationToken)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationToken = _cancellationSource.Token;
            _readBuffer = new BlockingCollection<Block>(_config.BuffersCapacity);
            _writeBuffer = new BlockingCollection<Block>(_config.BuffersCapacity);
        }

        public void Run(IEnumerable<Block> blockSource, Func<Block, Block> transformAlg, Action<Block> writeAlg) 
        {
            var readWork = new WorkGroup(
                work: () => ReadSourceItems(
                    source: blockSource,
                    target: _readBuffer,
                    _cancellationToken),
                workersCount: 1,
                onComplete: _readBuffer.CompleteAdding,
                onError: StopPipeline);

            var transformWork = new WorkGroup(
                work: () => TransformItems(
                    source: _readBuffer,
                    target: _writeBuffer,
                    action: transformAlg,
                    _cancellationToken),
                workersCount: _config.DegreeOfParallelism,
                onComplete: _writeBuffer.CompleteAdding,
                onError: StopPipeline);

            var writeWork = new WorkGroup(
                work: () => WriteItems(
                    source: _writeBuffer,
                    action: writeAlg,
                    _cancellationToken),
                workersCount: 1,
                onError: StopPipeline);

            readWork.Start();
            transformWork.Start();
            writeWork.Start();

            writeWork.Join();

            if (_exception != null) throw _exception;
        }

        private void StopPipeline(Exception ex)
        {
            _exception = ex;
            _cancellationSource.Cancel();
        }

        private void ReadSourceItems<T>(IEnumerable<T> source, BlockingCollection<T> target,
            CancellationToken cancellationToken)
        {
            foreach (var item in source)
            {
                if (cancellationToken.IsCancellationRequested) break;
                target.Add(item, cancellationToken);
            }
        }

        private void TransformItems<T>(BlockingCollection<T> source, BlockingCollection<T> target, Func<T, T> action,
            CancellationToken cancellationToken)
        {
            foreach (var item in source.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested) break;
                target.Add(action(item), cancellationToken);
            }
        }

        private void WriteItems<T>(BlockingCollection<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            foreach (var item in source.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested) break;
                action(item);
            }
        }
    }
}