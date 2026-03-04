using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Minh2.Services
{
    // Simple queued TTS service. Can be extended to play audio files or remote TTS.
    public class TtsService : IDisposable
    {
        private readonly BlockingCollection<string> _queue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;

        public TtsService()
        {
            _worker = Task.Run(ProcessQueueAsync);
        }

        public void Enqueue(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _queue.Add(text);
        }

        public void StopCurrent()
        {
            // stop currently speaking by cancelling and restarting worker
            _cts.Cancel();
        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                foreach (var text in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        var request = new SpeechOptions { Volume = 1.0f, Pitch = 1.0f };
                        await TextToSpeech.Default.SpeakAsync(text, request);
                        // short gap between messages
                        await Task.Delay(300, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // swallow TTS errors in POC, log in real app
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _cts.Cancel();
        }
    }
}