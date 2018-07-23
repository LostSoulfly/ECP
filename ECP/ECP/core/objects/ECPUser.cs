#region Imports

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ECP.API;
using System.Net.Sockets;
using ECP.API.Security;

#endregion
namespace ECP.API.Objects
{
    #region Enums

    // 0 = Lowest priority; 1 = Medium Priority; 2 = Highest Priority
    public enum UserType { Guest, Client, Admin }

    #endregion
    public class ECPUser
    {
        #region Variables

        internal string Username { get; set; } = ECPStringGenerator.GenerateString(25);
        internal string ID { get; set; } = ECPStringGenerator.GenerateString(15);
        internal string Key { get; set; }
        internal UserType Role { get; set; } = UserType.Guest;
        internal TcpClient Client { get; set; }

        #endregion
        #region Initialization

        internal ECPUser() { }
        internal ECPUser(TcpClient client)
        {
            Client = client;
        }

        internal ECPUser(string username, UserType role, TcpClient client)
        {
            Username = username;
            Role = role;
            Client = client;
        }

        //internal void SetUsername(string name) { Username = name; }
        //internal void SetID(string id) { ID = id; }
        //internal void SetKey(string key) { Key = key; }
        //internal void SetUserRole(UserType role) { Role = role; }
        internal byte[] Serialize()
        {
            // Return the username, id, and role of the user.
            return null;
        }

        #endregion
    }
}
