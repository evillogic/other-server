using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;

namespace ConsoleApplication1
{
    class Program
    {
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;
            while ((true))
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                string dataFromClient = null;
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                if (clientsList.Contains(dataFromClient))
                {
                    Console.WriteLine(dataFromClient + " Tried to join chat room, but " + dataFromClient + " is already in use");
                    broadcast("A doppleganger of " + dataFromClient + " has attempted to join!", dataFromClient, false);
                }
                else {
                    clientsList.Add(dataFromClient, clientSocket);
                    broadcast(dataFromClient + " Joined ", dataFromClient, false);
                    Console.WriteLine(dataFromClient + " Joined chat room ");
                    handleClinet client = new handleClinet();
                    client.startClient(clientSocket, dataFromClient, clientsList);
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();

        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream;
                try
                {
                    broadcastStream = broadcastSocket.GetStream();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    clientsList.Remove(uName);
                    broadcastSocket.Close();
                    return;
                }
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }  //end broadcast function
    }//end Main class

    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.IsBackground = true;
            ctThread.Start();
        }



        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while (true)
            {
                try
                {
                    requestCount = requestCount + 1;
                    //
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                    rCount = Convert.ToString(requestCount);

                    string[] splitString = dataFromClient.Split();
                    if (splitString[0] == "/me")
                    {
                        String action = "";
                        for (int i = 1; i < splitString.Length; i++)
                            action += splitString[i] + " ";
                        Program.broadcast(clNo + " " + action, clNo, false);
                    }
                    else
                    {
                        Program.broadcast(dataFromClient, clNo, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    //Theyve probably disconnected
                    //clientsList.Remove(dataFromClient);
                }
            }//end while
        }//end doChat
    } //end class handleClinet
}//end namespace

