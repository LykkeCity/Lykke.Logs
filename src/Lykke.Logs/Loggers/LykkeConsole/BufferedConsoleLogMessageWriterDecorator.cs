using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Microsoft.Extensions.Logging.Console.Internal;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal class BufferedConsoleLogMessageWriterDecorator : IConsoleLogMessageWriter
    {
        private const int MaxQueuedMessages = 1024 * 4;
        
        private readonly IConsoleLogMessageWriter _inner;
        private readonly bool _disposeInner;

        private readonly BlockingCollection<LogMessageEntry> _messageQueue;
        private readonly Task _outputTask;
        
        public BufferedConsoleLogMessageWriterDecorator(IConsoleLogMessageWriter inner, bool disposeInner = true)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _disposeInner = disposeInner;

            _messageQueue = new BlockingCollection<LogMessageEntry>(MaxQueuedMessages);
            _outputTask = Task.Factory.StartNew(ProcessLogQueue, this, TaskCreationOptions.LongRunning);
        }

        public void Write(LogMessageEntry entry)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _inner.Write(entry);
                    return;
                }
                catch (InvalidOperationException)
                {
                }
            }

            _inner.Write(entry);
        }

        private void ProcessLogQueue()
        {
            do
            {
                try
                {
                    foreach (var entry in _messageQueue.GetConsumingEnumerable())
                    {
                        _inner.Write(entry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToAsyncString());
                }
            } while (!_messageQueue.IsAddingCompleted && _messageQueue.Count == 0);
        }

        private static void ProcessLogQueue(object state)
        {
            ((BufferedConsoleLogMessageWriterDecorator) state).ProcessLogQueue();
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(TimeSpan.FromSeconds(6));
            }
            catch (TaskCanceledException)
            {
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {
            }

            if (_disposeInner)
            {
                _inner.Dispose();
            }
        }
    }
}