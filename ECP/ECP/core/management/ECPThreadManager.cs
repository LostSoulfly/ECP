#region Imports

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using ECP.API.Objects;
using ECP.API.Security;

#endregion
namespace ECP.API.Management
{
    internal class ECPThreadManager
    {
        #region Variables

        private static ECPThreadManager instance;
        internal List<ECPThread> threads = new List<ECPThread>();

        #endregion
        #region Initialization

        private ECPThreadManager() { } // Nada por ahora.

        #endregion
        #region Methods

        public static ECPThreadManager Instance { get { return instance ?? (instance = new ECPThreadManager()); } }

        internal void StartThread(Thread thread)
        {
            string name = thread.Name;
            string id = ECPStringGenerator.GenerateString(8);
            ECPThread t = new ECPThread(name, id, thread);
            threads.Add(t);
            t.Method.Start();
        }

        internal void StopThread(ECPThread thread)
        {
            threads[threads.IndexOf(thread)].Method.Abort();
        }

        #endregion
    }
}
