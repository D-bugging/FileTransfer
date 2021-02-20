using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileTransferServer
{
    public partial class Form1 : Form
    {
        public delegate void FileReceivedEventHandler(object font, string fileName);
        public event FileReceivedEventHandler FileReceived;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FileReceived += new FileReceivedEventHandler(Form1_FileReceived);
        }
        private void Form1_FileReceived(object font, string fileName)
        {
            this.BeginInvoke(
                new Action(
                delegate ()
                {
                    MessageBox.Show("New file received\n" + fileName);
                    System.Diagnostics.Process.Start("explorer", @"C:\");
                }));
        }

        private void btnConnection_Click(object sender, EventArgs e)
        {
            string ipAddress = txtAddress.Text;
            int port = int.Parse(txtPort.Text);
            try
            {
                Task.Factory.StartNew(() => TreatFileReceived(ipAddress, port));
                MessageBox.Show("Listening port..." + port);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void TreatFileReceived(string ipAddress, int port)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipAddress);
                TcpListener tcpListener = new TcpListener(ip, port);
                tcpListener.Start();

                while (true)
                {
                    Socket socket = tcpListener.AcceptSocket();
                    if (socket.Connected)
                    {
                        string fileName = string.Empty;
                        NetworkStream networkStream = new NetworkStream(socket);
                        int thisRead = 0;
                        int blockSize = 1024;
                        Byte[] dataByte = new Byte[blockSize];

                        lock (this)
                        {
                            string pathDirectory = @"C:\Users\D-bugging\Desktop\TEMP\";
                            socket.Receive(dataByte);
                            int fileNameLenght = BitConverter.ToInt32(dataByte, 0);
                            fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLenght);

                            Stream fileStream = File.OpenWrite(pathDirectory + fileName);
                            fileStream.Write(dataByte, 4 + fileNameLenght, (1024 - (4 + fileNameLenght)));

                            while (true)
                            {
                                thisRead = networkStream.Read(dataByte, 0, blockSize);
                                fileStream.Write(dataByte, 0, thisRead);
                                if (thisRead == 0)
                                    break;
                            }

                            fileStream.Close();
                        }

                        FileReceived?.Invoke(this, fileName);
                        socket = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
