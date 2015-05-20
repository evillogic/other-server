using System;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;
using System.Threading;



namespace WindowsFormsApplication2
{

    public partial class Form1 : Form
    {
        public delegate void newDelegate();
        public newDelegate myDelegate;
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newMsg();
        }

        private void newMsg()
        {
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox2.Text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            textBox2.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            readData = "Connecting to Chat Server ...";
            msg();
            clientSocket.Connect(txtIP.Text, int.Parse(txtPort.Text));
            serverStream = clientSocket.GetStream();

            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(txtName.Text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            myDelegate = new newDelegate(disconnect);
            Thread ctThread = new Thread(getMessage);
            ctThread.IsBackground = true;
            ctThread.Start();
            button2.Enabled = false;
        }

        private void getMessage()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                buffSize = clientSocket.ReceiveBufferSize;
                try{
                    serverStream.Read(inStream, 0, buffSize);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    readData = "" + returndata;
                    msg();
                }catch{
                    Invoke(myDelegate);
                    return;
                }
            }
        }

        private void disconnect()
        {
            button2.Enabled = true;
        }

        private void msg()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
                textBox1.AppendText(Environment.NewLine + " >> " + readData);
                //textBox1.Text = textBox1.Text + Environment.NewLine + " >> " + readData;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                newMsg();
            }
        }

        private void cmdHost_Click(object sender, EventArgs e)
        {
            Server serv = new Server(txtLog);
        }



    }

}

