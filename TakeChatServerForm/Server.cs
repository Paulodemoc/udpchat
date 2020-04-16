using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TakeChat;

namespace TakeChatServerForm
{
    public partial class Server : Form
    {
        TakeChatServer mTakeChatServer;

        public Server()
        {
            mTakeChatServer = new TakeChatServer();
            mTakeChatServer.RaisePrintStringEvent += ChatServer_PrintString;
            InitializeComponent();
        }

        private void ChatServer_PrintString(object sender, PrintStringEventArgs e)
        {
            Action<string> print = PrintToConsole;
            txtConsole.Invoke(print, new string[] { e.MessageToPrint });
        }

        private void PrintToConsole(string message)
        {
            txtConsole.AppendText(message);
            txtConsole.AppendText(Environment.NewLine);
        }

        private void Server_Load(object sender, EventArgs e)
        {
            mTakeChatServer.StartReceivingData();
        }
    }
}
