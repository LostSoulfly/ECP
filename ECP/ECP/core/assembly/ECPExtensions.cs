#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SecurityDriven.Inferno;
using System.Security.Cryptography;
using System.IO;

#endregion
namespace ECP.API
{
    internal static class ECPExtensions
    {
        #region Methods
        //public static byte[] ToPacket(this string command, PacketType type = PacketType.Message)
        //{
        //    try
        //    {
        //        List<byte[]> parts = new List<byte[]>();
        //        byte[] length = command.Length.ToBytes();
        //        parts.Add(length);
        //        //byte[] header = Convert.ToInt32(type).ToBytes();
        //        //parts.Add(header);
        //        byte[] contents = command.ToBytes();
        //        parts.Add(contents);
        //        return CombineArrays(parts);
        //    }
        //    catch { return null; }
        //}

        internal static string TrimCommand(this string input)
        {
            try { return input.Split('}')[1]; }
            catch { return null; }// All commands end with a curly bracket.
        }

        internal static string GetString(this byte[] data)
        {
            try { return Encoding.ASCII.GetString(data); }
            catch { return null; }
        }

        internal static byte[] ToBytes(this string input)
        {
            try { return Encoding.ASCII.GetBytes(input); }
            catch { return null; }
        }

        //public static byte[] GetBytes(this PacketType header)
        //{
        //    try { return BitConverter.GetBytes((int)header); }
        //    catch { return null; }
        //}

        internal static byte[] ToBytes(this int value)
        {
            try { return BitConverter.GetBytes(value); }
            catch { return null; }
        }

        internal static byte[] Concatenate(this byte[] first, byte[] second)
        {
            try
            {
                int index = first.Length;
                Array.Resize(ref first, index + second.Length);
                Array.Copy(second, 0, first, index, second.Length);
                return null;
            }
            catch { return null; }
        }

        internal static byte GetByte(this byte[] array, int index)
        {
            try
            {
                byte header = array[index];

                List<byte> bytes = array.ToList();
                bytes.RemoveAt(index);
                array = bytes.ToArray();

                return header;
            }
            catch { return 0; }
        }

        internal static byte[] TrimArray(this byte[] data)
        {
            try
            {
                int i = data.Length - 1;
                while (data[i] == 0)
                {
                    if ((i - 1) != 0) { i--; }
                }
                byte[] trimmed = new byte[i + 1];
                Array.Copy(data, trimmed, trimmed.Length);
                return trimmed;
            }
            catch { return null; }
        }

        internal static byte[] CombineArrays(this List<byte[]> data)
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

        internal static byte[] PickBytes(this byte[] bytes, int index, int length)
        {
            try
            {
                List<byte> data = bytes.ToList();
                List<byte> header = new List<byte>();
                for (int i = index; i < length; i++)
                {
                    header.Add(bytes[i]);
                    data.RemoveAt(i);
                }
                return header.ToArray();
            }
            catch { return null; }
        }

        internal static byte[] Encrypt(this byte[] data, string key)
        {
            // Create a new random object and memory stream.
            CryptoRandom random = new CryptoRandom();
            MemoryStream ciphertext = new MemoryStream();

            // Transform our plaintext into ciphertext.
            using (MemoryStream plaintext = new MemoryStream(data))
            using (EtM_EncryptTransform transform = new EtM_EncryptTransform(key: key.ToBytes()))
            using (CryptoStream crypto = new CryptoStream(ciphertext, transform, CryptoStreamMode.Write))
            {
                plaintext.CopyTo(crypto);
            }

            // Return our encrypted data.
            return ciphertext.ToArray();
        }

        internal static byte[] Decrypt(this byte[] data, string key)
        {
            // Create a new memory stream.
            MemoryStream plaintext = new MemoryStream();

            // Transform our ciphertext into plaintext.
            using (MemoryStream ciphertext = new MemoryStream(data))
            using (EtM_DecryptTransform transform = new EtM_DecryptTransform(key: key.ToBytes()))
            {
                using (CryptoStream crypto = new CryptoStream(ciphertext, transform, CryptoStreamMode.Read))
                    crypto.CopyTo(plaintext);
                if (!transform.IsComplete) throw new Exception("The data could not be decrypted.");
            }

            // Return our decrypted data.
            return plaintext.ToArray();
        }


        internal static byte[] DecryptBytes(this byte[] data, string key)
        {
            if (key != null)
            {
                //Console.WriteLine(key);
                //x = data.Decrypt(key);
                try { return GetDecodedBytes(data).Decrypt(key); }
                catch { return data; }
            }
            else { return data; }
            //// Try to decrypt our data packet.
            //byte[] x = null;
            //string encoded = data.GetString();
            //string parsed = encoded.Substring(0, encoded.IndexOf("$ECP"));
            //byte[] buffer = Convert.FromBase64String(parsed);

            //if (key != null)
            //{
            //    try { x = buffer.Decrypt(key); }
            //    catch { x = buffer; }
            //}
            //else { x = buffer; }
            //return x;
        }

        private static byte[] GetDecodedBytes(byte[] encoded)
        {
            using (MemoryStream input = new MemoryStream(encoded))
            using (BinaryReader reader = new BinaryReader(input))
                return Convert.FromBase64String(reader.ReadString());
        }

        #endregion
    }
}
