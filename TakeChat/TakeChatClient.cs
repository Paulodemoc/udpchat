using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TakeChat
{
    public class TakeChatClient : TakeChat
    {
        Socket mBroadcastSender;
        IPEndPoint mIPEPBroadcast;
        IPEndPoint mIPEPLocal;
        private EndPoint mChatServerEP;
        string nickname = "";

        public TakeChatClient(int _localPort, int _remotePort)
        {
            mIPEPBroadcast = new IPEndPoint(IPAddress.Broadcast, _remotePort);
            mIPEPLocal = new IPEndPoint(IPAddress.Any, _localPort);

            mBroadcastSender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mBroadcastSender.EnableBroadcast = true;
            mBroadcastSender.Bind(mIPEPLocal);
        }

        public void SendBroadcast(string message, PACKET_TYPE type)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            try
            {
                ChatPacket packet = new ChatPacket();
                packet.Message = message;
                packet.PacketType = type;
                packet.Nickname = mIPEPLocal.ToString();

                string jsonPacket = JsonConvert.SerializeObject(packet);

                var dataBytes = Encoding.UTF8.GetBytes(jsonPacket);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(dataBytes, 0, dataBytes.Length);
                args.RemoteEndPoint = mIPEPBroadcast;
                args.Completed += SendCompletedCallback;
                args.UserToken = packet;
                mBroadcastSender.SendToAsync(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void SendCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            if ((e.UserToken as ChatPacket).PacketType == PACKET_TYPE.DISCOVERY)
            {
                ReceiveTextFromServer(IPEPReceiverLocal: mIPEPLocal);
            }
        }

        private void ReceiveTextFromServer(IPEndPoint IPEPReceiverLocal)
        {
            if (IPEPReceiverLocal == null)
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"No endpoint specified"));
                return;
            }

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[1024], 0, 1024);
            args.RemoteEndPoint = IPEPReceiverLocal;
            args.Completed += ReceiveCompletedCallback;
            mBroadcastSender.ReceiveFromAsync(args);

        }

        private void ReceiveCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Socket Error: {e.SocketError}"));
                return;
            }
            var receivedText = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);

            ChatPacket messagePacket = JsonConvert.DeserializeObject<ChatPacket>(receivedText);

            if (messagePacket.PacketType == PACKET_TYPE.CONFIRMATION)
            {
                mChatServerEP = e.RemoteEndPoint;
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Server: {messagePacket.Message}"));
                ReceiveTextFromServer(mChatServerEP as IPEndPoint);
            }
            else if (messagePacket.PacketType == PACKET_TYPE.SETUP_ERROR)
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Server: {messagePacket.Message}"));
                ReceiveTextFromServer(mChatServerEP as IPEndPoint);
            }
            else if (messagePacket.PacketType == PACKET_TYPE.SETUP_CONFIRMATION)
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Server: {messagePacket.Message}"));
                nickname = messagePacket.Nickname;
                ReceiveTextFromServer(mChatServerEP as IPEndPoint);
            }
            else if (messagePacket.PacketType == PACKET_TYPE.TEXT && !string.IsNullOrEmpty(messagePacket.Message))
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"{messagePacket.Nickname}: {messagePacket.Message}"));
                ReceiveTextFromServer(mChatServerEP as IPEndPoint);
            }
        }

        public void SendMessageToKnownServer(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return;
                }

                ChatPacket packet = new ChatPacket();
                packet.Message = message;
                if (string.IsNullOrEmpty(nickname))
                {
                    packet.PacketType = PACKET_TYPE.SETUP;
                    packet.Nickname = string.Empty;
                }
                else
                {
                    packet.PacketType = PACKET_TYPE.TEXT;
                    packet.Nickname = nickname;
                }

                string jsonPacket = JsonConvert.SerializeObject(packet);

                var bytesToSend = Encoding.UTF8.GetBytes(jsonPacket);
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bytesToSend, 0, bytesToSend.Length);
                args.RemoteEndPoint = mChatServerEP;
                args.UserToken = packet;
                args.Completed += SendMessageToKnownServerCompleted;
                mBroadcastSender.SendToAsync(args);
            }
            catch (Exception e)
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Error: {e.ToString()}"));
                throw;
            }
        }

        private void SendMessageToKnownServerCompleted(object sender, SocketAsyncEventArgs e)
        {

        }
    }
}
