using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECP
{
    public enum ECPPacketType
    {
        HANDSHAKE,
        HREPLY,
        HSUCCESS,
        HFAIL,
        SHUTDOWN
    }
}
