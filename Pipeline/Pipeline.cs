using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Channels;

namespace Pipelines
{
    public class Pipeline
    {
        public Config Config { get; }
        private readonly WorkPlan _algs;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Pipeline(Config config, WorkPlan workAlgs)
        {
            _algs = workAlgs ?? throw new ArgumentNullException(nameof(workAlgs));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Process(string inputFile, string outputFile)
        {
            if (string.IsNullOrWhiteSpace(inputFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(inputFile));
            if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(outputFile));
            
            try
            {
                using var inputFs = new FileStream(inputFile, FileMode.Open);
                using var outputFs = new FileStream(outputFile, FileMode.Create);
                using var reader = new BinaryReader(inputFs);
                using var writer = new BinaryWriter(outputFs);
                
                SetPipeline(blockSource: _algs.ReadBlocks(reader), transformAlg: _algs.TransformBlock(), writeAlg: _algs.WriteBlock(writer));
            }
            catch (InvalidDataException de)
            {
                Console.WriteLine("File corrupted or not supported format");
            }
            catch (IOException io)
            {
                Console.WriteLine($"Wrong file name \n {io}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error \n {ex}");
            }
        }

        private void SetPipeline(IEnumerable<Block> blockSource, Func<Block, Block> transformAlg, Action<Block> writeAlg) 
        {
            var readBuffer = new BlockingCollection<Block>(Config.BuffersCapacity);
            var writeBuffer = new BlockingCollection<Block>(Config.BuffersCapacity);
            var cancellationToken = _cancellationTokenSource.Token;

            var readWork = new Work(
                work: () => ReadSourceItems(
                    source: blockSource,
                    target: readBuffer,
                    cancellationToken),
                workersCount: 1,
                onComplete: readBuffer.CompleteAdding,
                onError: StopPipeline);

            var transformWork = new Work(
                work: () => TransformItems(
                    source: readBuffer,
                    target: writeBuffer,
                    action: transformAlg,
                    cancellationToken),
                workersCount: Config.DegreeOfParallelism,
                onComplete: writeBuffer.CompleteAdding,
                onError: StopPipeline);

            var writeWork = new Work(
                work: () => WriteItems(
                    source: writeBuffer.GetConsumingEnumerable(),
                    action: writeAlg,
                    cancellationToken),
                workersCount: 1,
                onError: StopPipeline);

            readWork.Start();
            transformWork.Start();
            writeWork.Start();

            writeWork.Join();
        }

        private void StopPipeline(Exception ex)
        {
            Console.WriteLine(ex);
            _cancellationTokenSource.Cancel();
        }

        public void ReadSourceItems<T>(IEnumerable<T> source, BlockingCollection<T> target,
            CancellationToken cancellationToken)
        {
            foreach (var item in source)
            {
                if (cancellationToken.IsCancellationRequested) break;
                target.Add(item, cancellationToken);
            }
        }

        public void TransformItems<T>(BlockingCollection<T> source, BlockingCollection<T> target, Func<T, T> action,
            CancellationToken cancellationToken)
        {
            foreach (var item in source.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested) break;
                target.Add(action(item), cancellationToken);
            }
        }

        public void WriteItems<T>(IEnumerable<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            foreach (var item in source)
            {
                if (cancellationToken.IsCancellationRequested) break;
                action(item);
            }
        }
    }
}