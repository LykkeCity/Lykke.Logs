using System;
using System.Collections.Concurrent;
using System.Threading;
using AsyncFriendlyStackTrace;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal class BufferedConsoleLogMessageWriterDecorator : IConsoleLogMessageWriter
    {
        private const int MaxQueuedMessages = 1024 * 4;
        
        private readonly IConsoleLogMessageWriter _inner;
        private readonly bool _disposeInner;

        private readonly BlockingCollection<LogMessageEntry> _messageQueue;
        private readonly Thread _outputThread;
        
        public BufferedConsoleLogMessageWriterDecorator(IConsoleLogMessageWriter inner, bool disposeInner = true)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _disposeInner = disposeInner;

            _messageQueue = new BlockingCollection<LogMessageEntry>(MaxQueuedMessages);
            
            _outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "Console logger queue processing thread"
            };
            _outputThread.Start();
        }

        public void Write(LogMessageEntry entry)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(entry);
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
            try
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

                        _messageQueue.CompleteAdding();
                    }
                } while (!_messageQueue.IsAddingCompleted && _messageQueue.Count == 0);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToAsyncString());

                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch(Exception ex1)
                {
                    Console.WriteLine(ex1.ToAsyncString());
                }
            }

        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputThread.Join(TimeSpan.FromSeconds(6));
            }
            catch (ThreadStateException)
            {
            }

            if (_disposeInner)
            {
                _inner.Dispose();
            }
        }
    }
}