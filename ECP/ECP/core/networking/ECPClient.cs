#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Reflection;
using System.IO;
using ECP.API.Objects;
using ECP.API.Management;
using ECP.API.Security;

#endregion
namespace ECP.API.Networking
{
    public class ECPClient
    {
        #region Variables

        public bool Connected = false;
        public ECPUser User { get; private set; }
        private string[] commands = { "{HANDSHAKE}", "{HREPLY}", "{SHUTDOWN}" };
        private string Key = null;
        private bool Handshake = false;
        private TcpClient Client = new TcpClient();
        private NetworkStream Stream = default(NetworkStream);
        private ECPHandshake Exchange = new ECPHandshake();
        public event ClientConnectHandler OnClientConnect;
        public event ClientDisconnectHandler OnClientDisconnect;
        public event ClientDataReceivedHandler OnDataReceived;
        public event LogOutputHandler OnLogOutput;

        #endregion
        #region Initialization

        public ECPClient()
        {
            string assembly = "ECP.SecurityDriven.Inferno.dll";
            ECPAssemblyResolve.Load(assembly, "SecurityDriven.Inferno.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
            User = new ECPUser(Client);
        }

        public ECPClient(string username)
        {
            string assembly = "ECP.SecurityDriven.Inferno.dll";
            ECPAssemblyResolve.Load(assembly, "SecurityDriven.Inferno.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
            User = new ECPUser(username, UserType.Guest, Client);
        }

        #endregion
        #region Methods

        #region Core

        public void Connect(string ip, int port)
        {
            try
            {
                Client.Connect(ip, port);
                Stream = Client.GetStream();
                ClientConnected(ip + ":" + port);

                ECPDataPacket packet = new ECPDataPacket(User, Stream);
                packet.Send("{LOGIN}", false, PacketType.Message);

                Thread t = new Thread(PullData);
                t.Start();
            }
            catch (SocketException ex) { LogOutput("Connection to the server could not be established.", EntryType.Error, ex); }
            catch (Exception ex) { LogOutput(ex.Message, EntryType.Error, ex); }
        }

        public void Disconnect()
        {
            try
            {
                Send("{SHUTDOWN}");
                Stream.Close();
                Client.Close();
                Client.Client.Close();
            }
            catch (ObjectDisposedException ex) { LogOutput("The client has already been disconnected from the server.", EntryType.Error, ex); }
            catch (InvalidOperationException ex) { LogOutput("The client has already been disconnected.", EntryType.Error, ex); }
            catch (Exception ex) { LogOutput(ex.Message, EntryType.Error, ex); }
        }

        public void Send(string message)
        {
            try
            {
                // Create a message object which will hold our message's info.
                ECPDataPacket packet = new ECPDataPacket(User, Stream);

                // If we're trying to close the socket then call our disconnect method.
                if (message == (commands[2]))
                {
                    if (User.Key != null) { packet.Send(message, true, PacketType.Message); }
                    else { packet.Send(message, false, PacketType.Message); }
                    Stream.Close();
                    Client.Close();
                    Client.Client.Close();
                }
                else
                {
                    // If we're trying to send a handshake or reply then let it through.
                    if (message.Contains(commands[0]) || message.Contains(commands[1]))
                        Handshake = true;

                    // Check if we're still waiting for a handshake.
                    if (Handshake)
                    {
                        // If we're trying to send a handshake or reply then let it through.
                        if (message.Contains(commands[0]) || message.Contains(commands[1]))
                            packet.Send(message, false, PacketType.Message);
                        else
                            LogOutput("A session key could not be established.", EntryType.Error);
                    }
                    else
                    {
                        // Only send messages if we've established a key.
                        if (User.Key != null)
                            packet.Send(message, true, PacketType.Message);
                        else
                            LogOutput("A session key has not been established.", EntryType.Warning);
                    }
                }
            }
            catch(Exception ex) { LogOutput("The client is not connected to the server.", EntryType.Error, ex); }
        }

        public void Upload(string path)
        {
            // Create a message object which will hold our message's info.
            ECPDataPacket packet = new ECPDataPacket(User, Stream);

            // Send an encrypted packet if we have a key.
            if (User.Key != null)
                packet.Send(path, true, PacketType.File);
            else
                LogOutput("A session key has not been established.", EntryType.Warning);
            //try
            //{
            //    // Create a message object which will hold our message's info.
            //    ECPDataPacket packet = new ECPDataPacket(User, Stream);

            //    // Send an encrypted packet if we have a key.
            //    if (User.Key != null)
            //        packet.Send(path, true, PacketType.File);
            //    else
            //        LogOutput("A session key has not been established.", EntryType.Warning);
            //}
            //catch (Exception ex) { LogOutput("The client is not connected to the server.", EntryType.Error, ex); }
        }

        private void PullData()
        {
            // Send a handshake request before entering our loop.
            try
            {
                Handshake = true;
                ECPDataPacket packet = new ECPDataPacket(User, Stream);
                packet.Send("{HANDSHAKE}", false, PacketType.Message);
                //Send("{HANDSHAKE}");
            }
            catch { }

            // Start listening for incoming data.
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[Client.ReceiveBufferSize];
                    Stream = Client.GetStream();
                    Stream.Read(buffer, 0, buffer.Length);
                    
                    ECPDataPacket packet = ECPDataPacket.Deserialize(buffer.DecryptBytes(User.Key));
                    ParseData(Convert.FromBase64String(packet.Contents));
                }
                catch(Exception ex)
                {
                    LogOutput(ex.StackTrace, EntryType.Error);
                    //ClientDisconnected();
                    //break;
                }
            }
        }

        #endregion
        #region Utils

        private void ParseData(byte[] data)
        {
            if (data.GetString() == "{KEEPALIVE}")
            {
                // This is just a packet to see if we're connected still.
                LogOutput("Keep alive packet received.", EntryType.General, null, true);
            }
            else
            {
                //byte[] x = null;
                //if (Key != null)
                //{
                //    try
                //    {
                //        byte[] buffer = Convert.FromBase64String(data.GetString());
                //        x = buffer.Decrypt(Key);
                //    }
                //    catch { x = data; }
                //}
                //else
                //    x = data;

                // Parse any commands from our received data.
                string command = data.GetString();
                try
                {
                    if (command.Contains(commands[0]))
                    {
                        // Generate a new handshake request for the server.
                        if (command.Substring(0, commands[0].Length) == commands[0])
                        {
                            try
                            {
                                // Create a new response packet.
                                string response = command.Replace(commands[0], null);
                                Exchange = new ECPHandshake(256).GenerateResponse(response);

                                // Generate a new session key from our response.
                                User.Key = Convert.ToBase64String(Exchange.Key);

                                // Send our reponse packet to the server and log it.
                                string message = "{HREPLY}" + Exchange.ToString();
                                Send(message);
                                Handshake = false;
                                LogOutput("A new session key has been generated!", EntryType.Success);
                            }
                            catch(Exception ex) { LogOutput("A handshake could not be sent to the server.", EntryType.Error, ex); }

                            //if (User.Key != null)
                            //    LogOutput(User.Key, EntryType.General);
                        }
                    }
                    else if (command.Contains(commands[2]))
                    {
                        if (command.Substring(0, commands[2].Length) == commands[2])
                        {
                            Stream.Close();
                            Client.Close();
                            ClientDisconnected();
                            LogOutput("The connection has been closed by the server.", EntryType.Notice, null, true);
                        }
                    }
                    else
                    {
                        // Pass our data to our event.
                        DataReceived(data);
                    }
                }
                catch { }
            }
        }

        private byte[] CombineArrays(List<byte[]> data)
        {
            try
            {
                long size = 0;
                int offset = 0;
                foreach (byte[] x in data) { size += x.Length; }
                byte[] combined = new byte[size];
                for (int i = 0; i < data.Count; i++)
                {
                    Buffer.BlockCopy(data[i], 0, combined, offset, data[i].Length);
                    offset += data[i].Length;
                }
                return combined;
            }
            catch { return null; }
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ECPAssemblyResolve.Get(args.Name);
        }

        #endregion
        #region Events

        private void ClientConnected(string server)
        {
            Connected = true;
            if (OnClientConnect == null) return;
            ClientConnectEventArgs args = new ClientConnectEventArgs(server);
            OnClientConnect(this, args);
        }

        private void ClientDisconnected()
        {
            Connected = false;
            if (OnClientDisconnect == null) return;
            ClientDisconnectEventArgs args = new ClientDisconnectEventArgs();
            OnClientDisconnect(this, args);
        }

        private void DataReceived(byte[] data)
        {
            if (OnDataReceived == null) return;
            ClientDataReceivedEventArgs args = new ClientDataReceivedEventArgs(data);
            OnDataReceived(this, args);
        }

        private void LogOutput(string output, EntryType type, Exception error = null, bool silent = false)
        {
            // Add our output to the log and then return or update.
            ECPLogger.AddEntry(output, type);
            if (!silent)
            {
                if (OnLogOutput == null) return;
                LogOutputEventArgs args = new LogOutputEventArgs(output, type, error);
                OnLogOutput(this, args);
            }
        }

        #endregion

        #endregion
    }
}