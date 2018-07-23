#region Imports

using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using ECP.API.Objects;
using ECP.API.Management;
using ECP.API.Security;

#endregion
namespace ECP.API.Networking
{
    public class ECPServer
    {
        #region Variables

        public static Hashtable Clients = new Hashtable();
        public event ServerConnectHandler OnServerConnect;
        public event ServerDisconnectHandler OnServerDisconnect;
        public event ServerDataReceivedHandler OnDataReceived;
        public event LogOutputHandler OnLogOutput;

        #endregion
        #region Initialization

        public ECPServer()
        {
            string assembly = "ECP.SecurityDriven.Inferno.dll";
            ECPAssemblyResolve.Load(assembly, "SecurityDriven.Inferno.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        #endregion
        #region Methods

        #region Core

        public void Start(int port)
        {
            // Create our TCP objects.
            TcpListener Listener = new TcpListener(IPAddress.Any, port);
            //TcpClient Client = default(TcpClient);

            // Start the socket.
            Listener.Start();

            // Tell the user that our server has connected.
            ServerConnected();
            //LogOutput("ECPServer has been started...", EntryType.General);

            // Loop our socket to await data.
            while (true)
            {
                // Accept any incoming clients.
                ECPUser user = new ECPUser(Listener.AcceptTcpClient());
                //Client = user.Client;

                // Create some data buffers.
                byte[] buffer = new byte[1024];
                //string data = null;

                // Create a stream to read data from.
                NetworkStream stream = user.Client.GetStream();
                stream.Read(buffer, 0, buffer.Length);

                // Deserialize our data packet and get our client's info.
                ECPDataPacket packet = ECPDataPacket.Deserialize(buffer);
                user.Username = packet.User.Username;
                user.Role = packet.User.Role;

                //data = data.Substring(0, data.IndexOf("$ECP"));


                // Check if our connecting client's name is null or not.
                if (user.Username == "" || user.Username == " " || user.Username == null)
                {
                    // Create a new message packet and send it.
                    

                    // Since we errored out, our client obviously already exists.
                    packet = new ECPDataPacket(user, stream);
                    packet.Send("You must identify yourself to the server!", false, PacketType.Message);
                    //buffer = ("You must identify yourself to the server!").ToBytes();
                    //Client.SendBufferSize = buffer.Length;
                    //stream.Write(buffer, 0, buffer.Length);
                    //stream.Flush();

                    // Write a shutdown command to the client.
                    packet = new ECPDataPacket(user, stream);
                    packet.Send("{SHUTDOWN}", false, PacketType.Message);
                    //buffer = ("{SHUTDOWN}").ToBytes();
                    //Client.SendBufferSize = buffer.Length;
                    //stream.Write(buffer, 0, buffer.Length);
                    //stream.Flush();

                    // Close our stream and client socket. 
                    stream.Close();
                    user.Client.Close();
                    user.Client.Client.Close();
                }
                else
                {
                    // Check if the connecting client already exists.
                    try
                    {
                        // Add them to our hashtable
                        Clients.Add(user.Username, user);
                        
                        // Log that our client connected.
                        LogOutput(user.Username + " has connected.", EntryType.Notice);
                        // Broadcast that our client connected.
                        //byte[] received = Encoding.ASCII.GetBytes(data);
                        //DataReceived(received);
                        //Broadcast(data + " Joined ", data, false);
                        //Console.WriteLine(Timestamp() + "[+] {0} has joined.", data);

                        // Handle any other incoming data from the client.
                        ECPServerHandler handler = new ECPServerHandler(user);
                        handler.Handle(this, user);
                    }
                    catch
                    {

                        // Since we errored out, our client obviously already exists.
                        packet = new ECPDataPacket(user, stream);
                        packet.Send("Client already exists in the table.", false, PacketType.Message);
                        //buffer = ("Client already exists in the table.").ToBytes();
                        //Client.SendBufferSize = buffer.Length;
                        //stream.Write(buffer, 0, buffer.Length);
                        //stream.Flush();

                        // Write a shutdown command to the client.
                        packet = new ECPDataPacket(user, stream);
                        packet.Send("{SHUTDOWN}", false, PacketType.Message);
                        //buffer = ("{SHUTDOWN}").ToBytes();
                        //Client.SendBufferSize = buffer.Length;
                        //stream.Write(buffer, 0, buffer.Length);
                        //stream.Flush();

                        // Close our stream and client socket. 
                        stream.Close();
                        user.Client.Close();
                        user.Client.Client.Close();
                    }
                }
            }
        }

        public void Broadcast(string command, ECPUser user, bool encrypt = false)
        {
            if (user.Client != null)
            {
                NetworkStream stream = user.Client.GetStream();
                ECPDataPacket packet = new ECPDataPacket(user, stream);
                packet.Send(command, encrypt, PacketType.Message);
            }
        }

        /// <summary>
        /// Broadcasts a message from a client to all other clients.
        /// </summary>
        /// <param name="command">The message to broadcast to each client.</param>
        /// <param name="uName">The client id that is trying to broadcast.</param>
        public void BroadcastAll(string command, string name)
        {
            // Send a message to all clients in our table.
            foreach (DictionaryEntry Item in Clients)
            {
                if (Item.Key.ToString() != name)
                {
                    string message = (name + ": " + command);
                    ECPUser user = (ECPUser)Item.Value;
                    NetworkStream stream = user.Client.GetStream();
                    ECPDataPacket packet = new ECPDataPacket(user, stream);
                    packet.Send(message, true, PacketType.Message);
                }
            }
        }

        #endregion
        #region Utils
        
        private void ParseData(string data)
        {
            byte[] converted = Convert.FromBase64String(data);
            
            
        }

        internal void UpdateClient(ECPUser user)
        {
            try
            {
                if (Clients.ContainsKey(user.Username))
                {
                    Clients.Remove(user.Username);
                    Clients.Add(user.Username, user);
                }
            }
            catch { }
        }

        internal void RemoveClient(string id)
        {
            try
            {
                if (Clients.ContainsKey(id))
                    Clients.Remove(id);
            }
            catch { LogOutput("The client could not be removed from the table.", EntryType.Error); }
        }

        private string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }
        
        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ECPAssemblyResolve.Get(args.Name);
        }

        #endregion
        #region Events

        internal void ServerConnected()
        {
            if (OnServerConnect == null) return;
            ServerConnectEventArgs args = new ServerConnectEventArgs();
            OnServerConnect(this, args);
        }

        internal void ServerDisconnected()
        {
            if (OnServerDisconnect == null) return;
            ServerDisconnectEventArgs args = new ServerDisconnectEventArgs();
            OnServerDisconnect(this, args);
        }

        internal void DataReceived(string user, byte[] data)
        {
            if (OnDataReceived == null) return;
            ServerDataReceivedEventArgs args = new ServerDataReceivedEventArgs(user, data);
            OnDataReceived(this, args);
        }

        internal void LogOutput(string output, EntryType type, Exception error = null, bool silent = false)
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

    internal class ECPServerHandler
    {
        #region Variables

        private string[] commands = { "{HANDSHAKE}", "{HREPLY}", "{SHUTDOWN}" };
        private ECPServer Server;
        //private TcpClient Client;
        //private string ClientID;
        private ECPHandshake Exchange;
        private ECPUser User = null;
        //private string Key = null;

        #endregion
        #region Initialization

            internal ECPServerHandler(ECPUser user) { User = user; }

        #endregion
        #region Methods

        #region Core

        internal void Handle(ECPServer server, ECPUser user)
        {
            Server = server;
            User = user;
            Thread t = new Thread(PullData);
            t.Start();
        }

        ///// <summary>
        ///// Starts a new thread to handle client data.
        ///// </summary>
        ///// <param name="client">The client to await incoming data.</param>
        ///// <param name="id">The name of the client.</param>
        //internal void Handle(ECPServer server, TcpClient client, string id)
        //{
        //    // Set our client and its id.
        //    Server = server;
        //    Client = client;
        //    ClientID = id;

        //    // Create a new thread to pull data.
        //    Thread t = new Thread(PullData);
        //    t.Start();
        //}

        /// <summary>
        /// Read incoming data from the client stream.
        /// </summary>
        internal void PullData()
        {
            // Create our buffers.
            byte[] buffer = new byte[1024];
            //string data = null;

            // Start reading incoming data.
            while (true)
            {
                try
                {
                    // Create a new buffer and read the incoming bytes.
                    buffer = new byte[User.Client.ReceiveBufferSize];
                    NetworkStream networkStream = User.Client.GetStream();
                    networkStream.Read(buffer, 0, buffer.Length);

                    // Deserialize the packet and parse the contents.
                    ECPDataPacket packet = ECPDataPacket.Deserialize(buffer.DecryptBytes(User.Key));
                    ParseData(packet);
                    //ParseData(Convert.FromBase64String(packet.Contents));
                    
                    //data = Encoding.ASCII.GetString(buffer.TrimArray());
                    //data = data.Substring(0, data.IndexOf("$ECP"));

                    //// Create an array with the processed data..
                    //byte[] received = Encoding.ASCII.GetBytes(data);
                    //ParseData(received);
                }
                catch (ObjectDisposedException ex)
                {
                    // Log that our user has disconnected and break from our loop.
                    Server.LogOutput(User.Username + " has disconnected.", EntryType.Warning, ex);
                    Server.RemoveClient(User.Username);
                    User.Client.Close();
                    break;
                }
                catch (Exception ex)
                {
                    // Check if our client is still connected.
                    if (ClientConnected())
                        Server.LogOutput(User.Username + ": " + ex.ToString(), EntryType.Error);
                    else
                    {
                        // Log that our user has disconnected and break from our loop.
                        Server.LogOutput(User.Username + " has disconnected.", EntryType.Warning);
                        Server.RemoveClient(User.Username);
                        User.Client.Close();
                        break;
                    }
                }

                // Check if we're still connected to our client before looping.
                if (!ClientConnected())
                {
                    // Log that our user has disconnected and break from our loop.
                    Server.LogOutput(User.Username + " has disconnected.", EntryType.Warning);
                    Server.RemoveClient(User.Username);
                    User.Client.Close();
                    break;
                }
            }
        }

        #endregion
        #region Utils
        int count = 0;
        private void ParseData(ECPDataPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.Message:
                    // Parse any commands from our received data.
                    byte[] data = Convert.FromBase64String(packet.Contents);
                    string command = data.GetString();
                    try
                    {
                        if (command.Contains(commands[0]))
                        {
                            // Generate a new handshake request for the client.
                            if (command.Substring(0, commands[0].Length) == commands[0])
                            {
                                try
                                {
                                    // Log that we've received a handshake request.
                                    Server.LogOutput(User.Username + " has requested a handshake.", EntryType.Notice);

                                    // Jit our diffie-hellman before sending the actual request packet.
                                    ECPHandshake Jitter = new ECPHandshake(32).GenerateRequest();

                                    // Create a new request packet.
                                    Exchange = new ECPHandshake(256).GenerateRequest();

                                    // Send our packet to the client and log it.
                                    string message = "{HANDSHAKE}" + Exchange.ToString();
                                    Server.Broadcast(message, User);
                                    Server.LogOutput("A handshake has been sent to " + User.Username + ".", EntryType.Notice);
                                }
                                catch //catch(Exception ex)
                                {
                                    //Server.LogOutput(ex.StackTrace, EntryType.Error);
                                    Server.LogOutput("A handshake could not be sent to " + User.Username + ".", EntryType.Error);
                                }
                            }
                        }
                        else if (command.Contains(commands[1]))
                        {
                            // Generate a new encryption key using our handshake response.
                            if (command.Substring(0, commands[1].Length) == commands[1])
                            {
                                try
                                {
                                    // Parse our response to get our handshake reply.
                                    string response = command.Replace(commands[1], null);

                                    // Generate a new session key from our response.
                                    Exchange.HandleResponse(response);
                                    User.Key = (Convert.ToBase64String(Exchange.Key));
                                    Server.UpdateClient(User);
                                    Server.LogOutput("A new session key has been generated!", EntryType.Success);
                                }
                                catch { Server.LogOutput("The handshake response could not be processed.", EntryType.Error); }

                                ////// [DEBUG BLOCK]
                                //if (User.Key != null)
                                //{
                                //    Server.LogOutput(User.Key, EntryType.General);
                                //    Server.Broadcast("Hello World!", User, true);
                                //    Server.Broadcast("Mi nombre es Jason.", User, true);
                                //}
                            }
                        }
                        else if (command.Contains(commands[2]))
                        {
                            // Log that our user has disconnected and break from our loop.
                            //Server.LogOutput(ClientID + " has disconnected.", EntryType.Warning);
                            Server.RemoveClient(User.Username);
                            User.Client.Close();
                        }
                        else
                        {
                            // Pass our data to our event.
                            Server.DataReceived(User.Username, data);
                        }
                    }
                    catch { }
                    break;
                case PacketType.File:
                    count++;
                    Server.LogOutput("Packet " + count.ToString() + " has arrived.", EntryType.Notice);
                    ECPFile file = ECPFile.Deserialize(Convert.FromBase64String(packet.Contents));
                    string destination = @"C:\Users\X\Desktop\uploads\" + Path.GetFileName(file.Path); // Temporary obviously...
                    string temp = destination + ".tmp";
                    switch (file.Action)
                    {
                        case ECPFile.FileAction.Create:
                            // Create a temp file and wait for more bytes.
                            using (FileStream fs = new FileStream(temp, FileMode.Create))
                            {
                                fs.Write(file.Data, 0, file.Data.Length);
                                fs.Flush();
                            }
                            Server.LogOutput(temp + " has been created.", EntryType.Notice);
                            break;
                        case ECPFile.FileAction.Append:
                            // Append the received bytes to the temp file.
                            using (FileStream fs = new FileStream(temp, FileMode.Append))
                            {
                                fs.Write(file.Data, 0, file.Data.Length);
                                fs.Flush();
                            }
                            Server.LogOutput("Data has been appended to " + temp, EntryType.Notice);
                            break;
                        case ECPFile.FileAction.Close:
                            // Append the bytes to the temp file and rename it to original name.
                            using (FileStream fs = new FileStream(temp, FileMode.Append))
                            {
                                fs.Write(file.Data, 0, file.Data.Length);
                                fs.Flush();
                            }
                            Server.LogOutput(Path.GetFileName(temp) + " has finished uploading!", EntryType.Success);
                            File.Move(temp, destination);
                            break;
                    }
                    break;
            }
        }

        private bool ClientConnected()
        {
            try
            {
                if (User.Key != null) { Server.Broadcast("{KEEPALIVE}", User, true); }
                else { Server.Broadcast("{KEEPALIVE}", User); }
                return true;
            }
            catch { return false; }
        }

        private string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }

        #endregion

        #endregion
    }
}
