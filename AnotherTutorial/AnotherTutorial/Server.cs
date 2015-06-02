using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace WindowsFormsApplication2
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        public Hashtable clientsList = new Hashtable();
        private System.Windows.Forms.TextBox output;
        private delegate void ObjectDelegate(String text);
        private ObjectDelegate del;

        public Server(System.Windows.Forms.TextBox setOut)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 8888);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.IsBackground = true;
            this.listenThread.Start();
            output = setOut;
            del = new ObjectDelegate(outputTextToServer);
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();
            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                //create a thread to handle communication 
                //with connected client
                addClient(client);
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;
            while (true)
            {
                bytesRead = 0;
                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }
                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }
                //message has successfully been received
                String text = getData(clientStream);
                del.Invoke(text); //Used for Cross Threading & sending text to server output
                //if filter(text)
                sendMessage(tcpClient);
                //System.Diagnostics.Debug.WriteLine(text); //Spit it out in the console
            }

            tcpClient.Close();
        }

        private void outputTextToServer(String text)
        {
            if (output.InvokeRequired)
            {
                // we then create the delegate again
                // if you've made it global then you won't need to do this
                ObjectDelegate method = new ObjectDelegate(outputTextToServer);
                // we then simply invoke it and return
                output.Invoke(method, text);
                return;
            }
            output.AppendText(Environment.NewLine + " >> " + text);
        }

        private String getData(NetworkStream stream)
        {
            int newData;
            byte[] message = new byte[4096];
            ASCIIEncoding encoder = new ASCIIEncoding();
            newData = stream.Read(message, 0, 4096);
            String text = encoder.GetString(message, 0, newData); //Translate it into text
            text = text.Substring(0, text.IndexOf("$")); //Here comes the money
            return text;
        }

        private void addClient(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            String dataFromClient = getData(clientStream);
            if (clientsList.Contains(dataFromClient))
            {
                Console.WriteLine(dataFromClient + " Tried to join chat room, but " + dataFromClient + " is already in use");
                //broadcast("A doppleganger of " + dataFromClient + " has attempted to join!", dataFromClient, false);
            }
            else
            {
                clientsList.Add(dataFromClient, tcpClient);
                //broadcast(dataFromClient + " Joined ", dataFromClient, false);
                del.Invoke(dataFromClient + " Joined chat room ");
                //handleClinet client = new handleClinet();
                //client.startClient(clientSocket, dataFromClient, clientsList);
            }
        }

        private Boolean connectionAlive(NetworkStream stream)
        {
            byte[] message = new byte[4096];
            int bytesRead = 0;
            try
            {
                //blocks until a client sends a message
                bytesRead = stream.Read(message, 0, 4096);
            }
            catch
            {
                //a socket error has occured
                return false;
            }
            if (bytesRead == 0)
            {
                //the client has disconnected from the server
                //clientsList.Remove
                return false;
            }
            return true;
        }

        private void sendMessage(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("Hello Client!");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }
    }
}