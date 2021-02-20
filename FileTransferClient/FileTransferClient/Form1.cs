using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileTransferClient
{
    public partial class Form1 : Form
    {
        private static string fileName = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "File Sending";
            fileDialog.ShowDialog();
            txtFile.Text = fileDialog.FileName;
            fileName = fileDialog.SafeFileName;
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAddress.Text) &&
                string.IsNullOrEmpty(txtPort.Text) &&
                string.IsNullOrEmpty(txtFile.Text))
            {
                MessageBox.Show("Invalid data in fields");
                return;
            }

            string ipAddress = txtAddress.Text;
            int port = int.Parse(txtPort.Text);
            string filePath = txtFile.Text;

            try
            {
                Task.Factory.StartNew(() => sendFile(ipAddress, port, filePath, fileName));
                MessageBox.Show("File sent!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void sendFile(string ipAddress, int port, string filePatch, string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
                    byte[] fileData = File.ReadAllBytes(filePatch);
                    byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                    byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);

                    fileNameLen.CopyTo(clientData, 0);
                    fileNameByte.CopyTo(clientData, 4);
                    fileData.CopyTo(clientData, 4 + fileNameByte.Length);

                    TcpClient clientSocket = new TcpClient(ipAddress, port);
                    NetworkStream networkStream = clientSocket.GetStream();

                    networkStream.Write(clientData, 0, clientData.GetLength(0));
                    networkStream.Close();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
