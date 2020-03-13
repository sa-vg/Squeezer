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

        public void Run(IEnumerable<Block> blockSource, Func<Block, Block> transformBlockAlg, Action<Block> writeBlockAlg) 
        {
            var readWork = new WorkGroup(
                work: () => _readBuffer.Fill(blockSource, _cancellationToken),
                workersCount: 1,
                onComplete: _readBuffer.CompleteAdding,
                onError: StopPipeline);

            var transformWork = new WorkGroup(
                work: () => _readBuffer.TransformItems(action: transformBlockAlg, target: _writeBuffer, _cancellationToken),
                workersCount: _config.DegreeOfParallelism,
                onComplete: _writeBuffer.CompleteAdding,
                onError: StopPipeline);

            var writeWork = new WorkGroup(
                work: () => _writeBuffer.Consume(action: writeBlockAlg, _cancellationToken),
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
    }
}