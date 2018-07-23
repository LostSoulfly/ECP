#region Imports

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

#endregion
namespace ECP.API.Objects
{
    internal class ECPThread
    {
        #region Variables

        internal string ThreadName { get; private set; }
        internal string ThreadID { get; private set; }
        internal Thread Method { get; private set; }

        #endregion
        #region Initialization

        internal ECPThread(string name, string id, Thread thread)
        {
            ThreadName = name;
            ThreadID = id;
            Method = thread;
        }

        #endregion
    }
}
