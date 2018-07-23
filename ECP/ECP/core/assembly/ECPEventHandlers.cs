#region Imports

using System;
using ECP.API.Management;

#endregion
namespace ECP.API
{
    #region Delegates

    public delegate void ServerConnectHandler(object sender, ServerConnectEventArgs args);
    public delegate void ServerDisconnectHandler(object sender, ServerDisconnectEventArgs args);
    public delegate void ServerDataReceivedHandler(object sender, ServerDataReceivedEventArgs args);
    public delegate void ClientConnectHandler(object sender, ClientConnectEventArgs args);
    public delegate void ClientDisconnectHandler(object sender, ClientDisconnectEventArgs args);
    public delegate void ClientDataReceivedHandler(object sender, ClientDataReceivedEventArgs args);
    public delegate void LogOutputHandler(object sender, LogOutputEventArgs args);

    #endregion
    #region EventArgs

    public class ServerConnectEventArgs : EventArgs
    {
        public ServerConnectEventArgs() { }
    }

    public class ServerDisconnectEventArgs : EventArgs
    {
        public ServerDisconnectEventArgs() { }
    }

    public class ServerDataReceivedEventArgs : EventArgs
    {
        public string User { get; private set; }
        public byte[] Data { get; private set; }
        public ServerDataReceivedEventArgs(string user, byte[] data)
        {
            User = user;
            Data = data;
        }
    }

    public class ClientConnectEventArgs : EventArgs
    {
        public string Server { get; private set; }
        public ClientConnectEventArgs(string server)
        {
            Server = server;
        }
    }

    public class ClientDisconnectEventArgs : EventArgs
    {
        public ClientDisconnectEventArgs()
        {
        }
    }

    public class ClientDataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }
        public ClientDataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public class LogOutputEventArgs : EventArgs
    {
        public string Output { get; private set; }
        public EntryType Type { get; private set; }
        public Exception Error { get; private set; }
        public LogOutputEventArgs(string output, EntryType type, Exception error = null)
        {
            Output = output;
            Type = type;
            Error = error;
        }
    }

    #endregion
}
