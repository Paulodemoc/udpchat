using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TakeChat
{
    public class TakeChatServer : TakeChat
    {
        Socket mBroadcastReceiver;
        IPEndPoint mIPEPLocal;
        private int retryCount;
        Dictionary<string, EndPoint> mListOfClients;

        public TakeChatServer()
        {
            mBroadcastReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mBroadcastReceiver.EnableBroadcast = true;
            mIPEPLocal = new IPEndPoint(IPAddress.Any, 30000);
            mListOfClients = new Dictionary<string, EndPoint>();
        }

        public void StartReceivingData()
        {
            try
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(new byte[1024], 0, 1024);
                args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                if (!mBroadcastReceiver.IsBound)
                {
                    mBroadcastReceiver.Bind(mIPEPLocal);
                }

                args.Completed += ReceiveCompletedCallback;

                if (!mBroadcastReceiver.ReceiveFromAsync(args))
                {
                    OnRaisePrintStringEvent(new PrintStringEventArgs($"Failed to receive data: {args.SocketError}"));
                    if (retryCount++ >= 10)
                    {
                        return;
                    }
                    else
                    {
                        StartReceivingData();
                    }
                }
            }
            catch (Exception e)
            {
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Error: {e.ToString()}"));
                throw;
            }
        }

        private void ReceiveCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            string textReceived = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);

            ChatPacket packet = JsonConvert.DeserializeObject<ChatPacket>(textReceived);
            packet.Message = packet.Message.Trim();

            if (packet.PacketType == PACKET_TYPE.DISCOVERY && packet.Message.Equals("<DISCOVER>"))
            {
                if (!mListOfClients.ContainsValue(e.RemoteEndPoint))
                {
                    mListOfClients.Add(e.RemoteEndPoint.ToString(), e.RemoteEndPoint);

                    OnRaisePrintStringEvent(new PrintStringEventArgs($"New client connected: {e.RemoteEndPoint.ToString()}"));

                    ChatPacket confirmationPacket = new ChatPacket();
                    confirmationPacket.Message = "Inform your nickname";
                    confirmationPacket.PacketType = PACKET_TYPE.CONFIRMATION;
                    confirmationPacket.Nickname = "Server";

                    SendTextToEndPoint(JsonConvert.SerializeObject(confirmationPacket), e.RemoteEndPoint);
                }
            }
            else if (packet.PacketType == PACKET_TYPE.SETUP)
            {
                string choosenNickname = packet.Message;
                if (mListOfClients.ContainsKey(choosenNickname))
                {
                    packet.Message = "The choosen nickname already exists. Please choose another one";
                    packet.PacketType = PACKET_TYPE.SETUP_ERROR;
                    SendTextToEndPoint(JsonConvert.SerializeObject(packet), e.RemoteEndPoint);
                }
                else
                {
                    mListOfClients.Remove(e.RemoteEndPoint.ToString());
                    mListOfClients.Add(choosenNickname, e.RemoteEndPoint);
                    packet.Message = "Welcome to the chat";
                    packet.PacketType = PACKET_TYPE.SETUP_CONFIRMATION;
                    packet.Nickname = choosenNickname;
                    SendTextToEndPoint(JsonConvert.SerializeObject(packet), e.RemoteEndPoint);
                }
            }
            else if (packet.PacketType == PACKET_TYPE.TEXT)
            {
                if (packet.Message.StartsWith("/help"))
                {
                    packet.Nickname = "Server";
                    packet.Message = $"To send a message to someone start the message with a @ followed by the nickname (e.g.: @paulo Oi){Environment.NewLine}To send a private message to someone start the message with a # followed by the nickname (e.g.: #paulo essa mensagem é só pra você){Environment.NewLine}";
                    SendTextToEndPoint(JsonConvert.SerializeObject(packet), e.RemoteEndPoint);
                }
                else if (packet.Message.StartsWith("@"))
                {
                    string targetNick = packet.Message.Substring(0, packet.Message.IndexOf(' ')).Replace("@", "");
                    if (mListOfClients.ContainsKey(targetNick))
                    {
                        packet.Nickname += $" says to {targetNick}";
                        packet.Message = packet.Message.Remove(0, packet.Message.IndexOf(' '));
                        SendToAll(JsonConvert.SerializeObject(packet));
                    } else
                    {
                        packet.Nickname = "Server";
                        packet.Message = "User not found on this room";
                        SendTextToEndPoint(JsonConvert.SerializeObject(packet), e.RemoteEndPoint);
                    }
                }
                else if (packet.Message.Trim().StartsWith("#"))
                {
                    string targetNick = packet.Message.Substring(0, packet.Message.IndexOf(' ')).Replace("#", "");
                    if (mListOfClients.ContainsKey(targetNick))
                    {
                        packet.Nickname += $" privately says to {targetNick}";
                        packet.Message = packet.Message.Remove(0, packet.Message.IndexOf(' '));
                        SendTextToEndPoint(JsonConvert.SerializeObject(packet), e.RemoteEndPoint);
                        SendTextToEndPoint(JsonConvert.SerializeObject(packet), mListOfClients[targetNick]);
                    }
                    else
                    {
                        packet.Nickname = "Server";
                        packet.Message = "User not found on this room";
                        SendTextToEndPoint(JsonConvert.SerializeObject(packet), e.RemoteEndPoint);
                    }
                }
                else
                {
                    SendToAll(textReceived);
                }
            }
            StartReceivingData();
        }

        private void SendToAll(string textReceived)
        {
            foreach (KeyValuePair<string, EndPoint> kvp in mListOfClients)
            {
                SendTextToEndPoint(textReceived, kvp.Value);
            }
        }

        private void SendTextToEndPoint(string textToSend, EndPoint remoteEndPoint)
        {
            if (string.IsNullOrEmpty(textToSend) || remoteEndPoint == null)
            {
                return;
            }

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = remoteEndPoint;

            var bytesToSend = Encoding.UTF8.GetBytes(textToSend);

            args.SetBuffer(bytesToSend, 0, bytesToSend.Length);
            args.Completed += SendTextToEndpointComplete;

            mBroadcastReceiver.SendToAsync(args);
        }

        private void SendTextToEndpointComplete(object sender, SocketAsyncEventArgs e)
        {

        }
    }
}
