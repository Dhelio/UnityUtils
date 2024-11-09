using System;
using System.Collections.Concurrent;

namespace Castrimaris.Core {

    /// <summary>
    /// Simple dispatcher to execute code on the main thread; useful when using Unity methods in Tasks or Threads.
    /// </summary>
    public class ThreadDispatcher : SingletonMonoBehaviour<ThreadDispatcher> {

        private ConcurrentQueue<Action> mainThreadQ = new ConcurrentQueue<Action>();

        /// <summary>
        /// Enqueues this code to be executed in the first frame available.
        /// </summary>
        public void DispatchOnMainThread(Action action) => mainThreadQ.Enqueue(action);

        private void Update() {
            if (!mainThreadQ.TryDequeue(out var action))
                return;

            action.Invoke();
        }

    }

}