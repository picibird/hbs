// SearchResultList.cs
// Date Created: 28.01.2016
// 
// Copyright (c) 2016, picibird GmbH 
// All rights reserved.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;
using System.Threading;
using picibits.core.collection;
using picibits.core.util;

[assembly: ComVisible(false)]

namespace picibird.hbs.ldu
{
    public class ItemList<Hit> : PiciObservableCollectionWithCancellationToken<Hit>
    {
    }

    public class FilterList<FilterCategory> : PiciObservableCollectionWithCancellationToken<FilterCategory>
    {
    }

    public class PiciObservableCollectionWithCancellationToken<T> : PiciObservableCollection<T>
    {
        /// <summary>The synchronization context captured upon construction.  This will never be null.</summary>
        private readonly SynchronizationContext m_synchronizationContext;

        public CancellationToken? CancellationToken { get; private set; }

        public event SimpleEventHandler<PiciObservableCollectionWithCancellationToken<T>> ListUpdated;

        /// <summary>A cached delegate used to post invocation to the synchronization context.</summary>
        private readonly SendOrPostCallback m_StatusHandler;

        public PiciObservableCollectionWithCancellationToken()
        {
            // Capture the current synchronization context.  "current" is determined by CurrentNoFlow,
            // which doesn't consider the [....] ctx flown with an ExecutionContext, avoiding
            // [....] ctx reference identity issues where the [....] ctx for one thread could be Current on another.
            // If there is no current context, we use a default instance targeting the ThreadPool.
            m_synchronizationContext = SynchronizationContext.Current;

            // NOTE: this methot is not implemented in mono and is for debugging purposes only anyway
            //Contract.Assert(m_synchronizationContext != null);

            m_StatusHandler = new SendOrPostCallback(InvokeEvent);
        }

        internal void FireListUpdated()
        {
            SimpleEventHandler<PiciObservableCollectionWithCancellationToken<T>> changedEvent = ListUpdated;
            if (changedEvent != null)
            {
                // Post the processing to the [....] context.
                // (If T is a value type, it will get boxed here.)
                m_synchronizationContext.Post(m_StatusHandler, null);
            }
        }

        /// <summary>Invokes the action and event callbacks.</summary>
        /// <param name="state">The progress value.</param>
        private void InvokeEvent(object state)
        {
            SimpleEventHandler<PiciObservableCollectionWithCancellationToken<T>> changedEvent = ListUpdated;
            if (changedEvent != null) changedEvent(this);
        }
    }
}