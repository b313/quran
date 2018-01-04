using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quran.Util
{
    internal class SingleInstance : IDisposable
    {
        private readonly bool ownsMutex;
        private Mutex mutex;
        private Guid identifier;

        internal SingleInstance(Guid id)
        {
            this.identifier = id;
            mutex = new Mutex(true, identifier.ToString(), out ownsMutex);
        }

        internal bool IsFirstInstance
        {
            get
            {
                return ownsMutex;
            }
        }

        public void Dispose()
        {
            if (mutex != null && ownsMutex)
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
        }

    }
}

