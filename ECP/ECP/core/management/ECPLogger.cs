#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion
namespace ECP.API.Management
{
    #region Enums

    public enum EntryType { General, Notice, Success, Warning, Error }

    #endregion
    internal static class ECPLogger
    {
        #region Variables

        private static List<string> Log = new List<string>();

        #endregion
        #region Methods

        internal static void AddEntry(string message, EntryType type)
        {
            // Check what type of entry we're adding and create it.
            string entry = null;
            switch (type)
            {
                case EntryType.General:
                    entry += Timestamp() + "[-] " + message;
                    break;
                case EntryType.Notice:
                    entry += Timestamp() + "[*] " + message;
                    break;
                case EntryType.Success:
                    entry += Timestamp() + "[+] " + message;
                    break;
                case EntryType.Warning:
                    entry += Timestamp() + "[!] " + message;
                    break;
                case EntryType.Error:
                    entry += Timestamp() + "[x] " + message;
                    break;
            }
            
            // Add our entry if it's not null, otherwise, add our received message.
            if (entry != null)
                Log.Add(entry);
            else
            {
                if (message != null)
                    Log.Add(entry);
            }
        }

        private static string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }

        #endregion
    }
}
