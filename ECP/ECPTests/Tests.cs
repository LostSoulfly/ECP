#region Imports

using System;
using ECP.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion
namespace ECPTests
{
    [TestClass]
    public class Tests
    {
        #region Methods
        
        #region Tests

        /// <summary>
        /// Create a client and server pair and then send a series of short messages.
        /// </summary>
        [TestMethod]
        public void SendMessages_Short_Client()
        {
            // Create a server and start listening on a port.
            //ECPServer server = new ECPServer();
            //server.Start(80);
        }

        /// <summary>
        /// Create a client and server pair and then send a series of long messages.
        /// </summary>
        [TestMethod]
        public void SendMessage_Long_Client()
        {

        }

        /// <summary>
        /// Create multiple clients and a server which will broadcast a short message to all clients.
        /// </summary>
        [TestMethod]
        public void SendMessage_Short_MultipleClients()
        {

        }

        /// <summary>
        /// Create multiple clients and a server which will broadcast a long message to all clients.
        /// </summary>
        [TestMethod]
        public void SendMessage_Long_MultipleClients()
        {

        }

        /// <summary>
        /// Create a client and server pair and then send a small file from client to server.
        /// </summary>
        [TestMethod]
        public void UploadFile_Small_Client()
        {
        }

        /// <summary>
        /// Create a client and server pair and then send a large file from client to server.
        /// </summary>
        [TestMethod]
        public void UploadFile_Large_Client()
        {
        }

        /// <summary>
        /// Create a client and server pair and sends multiple files of various sizes from client to server.
        /// </summary>
        [TestMethod]
        public void UploadFile_Multiple_Client()
        {
        }

        /// <summary>
        /// Create multiple clients and a server which will upload a small file to all clients.
        /// </summary>
        [TestMethod]
        public void UploadFile_Small_MultipleClients()
        {

        }

        /// <summary>
        /// Create multiple clients and a server which will upload a large file to all clients.
        /// </summary>
        [TestMethod]
        public void UploadFile_Large_MultipleClients()
        {

        }

        /// <summary>
        /// Create multiple clients and a server which will upload multiple files of various sizes to all clients.
        /// </summary>
        [TestMethod]
        public void UploadFile_Multiple_MultipleClients()
        {

        }

        #endregion
        #region Events

        private void OnClientConnect(object sender, ClientConnectEventArgs args)
        {
            
        }

        private void OnClientDisconnect(object sender, ClientDisconnectEventArgs args)
        {

        }

        private void OnClientDataReceived(object sender, ClientDataReceivedEventArgs args)
        {

        }

        private void OnServerConnect(object sender, ServerConnectEventArgs args)
        {

        }
        
        private void OnServerDisconnect(object sender, ServerDisconnectEventArgs args)
        {

        }

        private void OnServerDataReceived(object sender, ServerDataReceivedEventArgs args)
        {

        }

        private void OnLogOutput(object sender, LogOutputEventArgs args)
        {

        }

        #endregion

        #endregion
    }
}
