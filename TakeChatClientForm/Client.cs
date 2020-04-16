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

namespace TakeChatClientForm
{
    public partial class Client : Form
    {
        TakeChatClient mChatClient;
        public Client()
        {
            mChatClient = new TakeChatClient(new Random().Next(23000, 29999), 30000);
            mChatClient.RaisePrintStringEvent += ChatClient_PrintString;
            mChatClient.SendBroadcast("<DISCOVER>", PACKET_TYPE.DISCOVERY);
            InitializeComponent();
        }

        private void ChatClient_PrintString(object sender, PrintStringEventArgs e)
        {
            Action<string> print = PrintToConsole;
            txtConsole.Invoke(print, new string[] { e.MessageToPrint });
        }

        private void PrintToConsole(string message)
        {
            txtConsole.AppendText(message);
            txtConsole.AppendText(Environment.NewLine);
        }

        private void txtMessageField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtMessageField.Lines.Length >= 1 && !string.IsNullOrEmpty(txtMessageField.Text))
                {
                    mChatClient.SendMessageToKnownServer(txtMessageField.Text);
                    txtMessageField.Lines = null;
                    txtMessageField.Text = string.Empty;
                }
            }
        }
    }
}
