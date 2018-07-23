#region Imports

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

#endregion
namespace ECP.API.Objects
{
    /// <summary>
    /// Determines how the structure of a packet will be created.
    /// </summary>
    internal enum PacketType { Message, File, Other }
    /// <summary>
    /// Determines what information to send with the packet.
    /// </summary>
    internal enum SenderType { Client, Server, Other }
    internal class ECPDataPacket
    {
        /// <summary>
        /// The type of packet that will be created and sent.
        /// </summary>
        internal PacketType Type { get; set; }
        /// <summary>
        /// The user object which contains information about the connected client.
        /// </summary>
        internal ECPUser User { get; set; }
        /// <summary>
        /// The underlying stream that data packets will be written to.
        /// </summary>
        private NetworkStream Stream { get; set; }
        /// <summary>
        /// The date and time when the packet was crafted.
        /// </summary>
        internal string Timestamp { get; set; }
        /// <summary>
        /// The actual data of the packet encoded in Base64.
        /// </summary>
        internal string Contents { get; set; }
        /// <summary>
        /// Blank constructor allowing for a more customized approach to object construction.
        /// </summary>
        internal ECPDataPacket() { }
        /// <summary>
        /// Default constructor which allows the construction of an encrypted data packet.
        /// </summary>
        /// <param name="user">Object which holds information such as the client name, id, and encryption key.</param>
        /// <param name="stream">The data stream which is to be used for writing a packet to.</param>
        internal ECPDataPacket(ECPUser user, NetworkStream stream)
        {
            User = user;
            Stream = stream;
        }
        /// <summary>
        /// Send a packet of data to another ECP client or server using the provided data stream.
        /// </summary>
        /// <param name="data">A message or filepath to be used when sending the pacekt.</param>
        /// <param name="encrypt">A flag determining whether the packet should be encrypted or not.</param>
        /// <param name="type">Packets can be of three types: Message, File, or an Unstructured packet.</param>
        internal void Send(string data, bool encrypt, PacketType type)
        {
            // Create a blank packet.
            byte[] packet = null;

            // Set our packet type.
            Type = type;

            // Create a stream from the provided client info.
            switch (type)
            {
                case PacketType.Message:
                    // Create a serialized message object.
                    ECPMessage message = new ECPMessage(data);
                    Contents = Convert.ToBase64String(message.Serialize());

                    // Generate a timestamp for the packet.
                    Timestamp = GetTimestamp();

                    // Serialize the data packet.
                    if (encrypt) { packet = SerializeEncrypted(); }
                    else { packet = Serialize(); }

                    // Create a data stream and write our packet.
                    Stream.Write(packet, 0, packet.Length);
                    Stream.Flush();
                    break;
                case PacketType.File:
                    SendFile(data, encrypt);
                    break;
                case PacketType.Other:
                    break;
            }
        }

        private void SendFile(string path, bool encrypt)
        {
            int count = 0;
            byte[] packet = null;
            ECPFile file = new ECPFile(path);
                NetworkStream stream = User.Client.GetStream();
            
            while (file.Read != file.Total)
            {
                count++;
                Contents = Convert.ToBase64String(file.Serialize());
                Timestamp = GetTimestamp();
                if (encrypt) { packet = SerializeEncrypted(); }
                else { packet = Serialize(); }
                stream.Write(packet, 0, packet.Length);
                stream.Flush();

                // [DEBUG]
                Console.WriteLine("Read: {0}", file.Read.ToString());
                Console.WriteLine("Packet {0} has been sent!", count.ToString());
                //Thread.Sleep(1000);
            }
            count = 0;
        }

        internal byte[] SerializeEncrypted()
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(output))
                    writer.Write(Convert.ToBase64String(Serialize().Encrypt(User.Key)));
                return output.ToArray();
            }
        }

        internal byte[] Serialize()
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(output))
                {
                    writer.Write(Convert.ToByte(Type));
                    writer.Write(User.Username);
                    writer.Write(Convert.ToByte(User.Role));
                    writer.Write(Timestamp);
                    writer.Write(Contents);
                }
                return output.ToArray();
            }
        }

        internal static ECPDataPacket Deserialize(byte[] data)
        {
            ECPDataPacket packet = new ECPDataPacket();
            using (MemoryStream input = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(input))
                {
                    ECPUser user = new ECPUser();
                    packet.Type = (PacketType)reader.ReadByte();
                    user.Username = reader.ReadString();
                    user.Role = (UserType)reader.ReadByte();
                    packet.User = user;
                    packet.Timestamp = reader.ReadString();
                    packet.Contents = reader.ReadString();
                }
            }
            return packet;
        }

        private string GetTimestamp()
        {
            return (DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ");
        }
    }
    internal class ECPMessage
    {
        internal string Message { get; private set; }
        internal ECPMessage(string message) { Message = message; }
        internal byte[] Serialize()
        {
            return Message.ToBytes();
        }
    }
    internal class ECPFile
    {
        internal enum FileAction { Create, Append, Close }
        internal string Path { get; set; }
        internal int Read = 0;
        internal int Remaining  = 0;
        internal int Total = 0;
        internal int Buffer = 106496;
        internal FileAction Action { get; set; } = FileAction.Create;
        internal byte[] Data { get; set; }      
        internal ECPFile() { }
        internal ECPFile(string path)
        {
            Path = path;
            Total = (int)new FileInfo(path).Length;
            Console.WriteLine("Original Buffer: {0}", Buffer.ToString());
        }
        internal byte[] Serialize()
        {
            // Create a buffer for our file chunks.
            Remaining = (Total - Read);
            if (Remaining < Buffer) { Buffer = Remaining; }
            Data = new byte[Buffer];

            // [DEBUG]
            Console.WriteLine("Remaining: {0}", Remaining.ToString());
            Console.WriteLine("Data Buffer: {0}", Data.Length.ToString());

            // Read a chunk of our file and reset our buffer.
            if (Read != 0) { Action = FileAction.Append; }
            using (FileStream fs = new FileStream(Path, FileMode.Open))
                Read += fs.Read(Data, 0, Data.Length);
            if (Read == Total || Read > Total) { Action = FileAction.Close; }

            // Serialize the data and return it.
            using (MemoryStream output = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(output))
                {
                    writer.Write(Path);
                    writer.Write(Read);
                    writer.Write(Remaining);
                    writer.Write(Total);
                    writer.Write(Buffer);
                    writer.Write(Convert.ToByte(Action));
                    writer.Write(Data);
                }
                return output.ToArray();
            }
        }

        internal static ECPFile Deserialize(byte[] data)
        {
            // Create a blank file object.
            ECPFile file = new ECPFile();

            // Fill our file object with the deserialized data.
            using (MemoryStream input = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(input))
                {
                    file.Path = reader.ReadString();
                    file.Read = reader.ReadInt32();
                    file.Remaining = reader.ReadInt32();
                    file.Total = reader.ReadInt32();
                    file.Buffer = reader.ReadInt32();
                    file.Action = (FileAction)reader.ReadByte();
                    file.Data = reader.ReadBytes(file.Buffer);
                }
            }
            return file;
        }
    }
}
