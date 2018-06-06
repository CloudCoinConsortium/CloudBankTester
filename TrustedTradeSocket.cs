﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;

namespace CloudBankTester
{

    class TrustedTradeSocket
    {
        public enum Status {NONE, STATUS_CONNECTED, STATUS_ERROR, STATUS_DISCONNECTED, STATUS_SENDING, STATUS_DONE, STATUS_REQUEST_RECIPIENT, STATUS_WAITING_RECIPIENT }

        public enum PacketType {NONE, PACKET_TYPE_INIT, PACKET_TYPE_WORD, PACKET_TYPE_COINS, PACKET_TYPE_PROGRESS, PACKET_TYPE_DONE, PACKET_TYPE_REQUEST_RECIPIENT, PACKET_TYPE_OK, PACKET_TYPE_HASH, PACKET_TYPE_RECIPIENT_REPLY }

        Status status = Status.STATUS_DISCONNECTED;
        string errorMsg = "";

        ClientWebSocket ws;
        CancellationTokenSource cts;
        string _url;
        int _timeout;
        Func<string, bool> _onWord;
        Func<bool> _onStatusChange;
        Func<string, bool> _onReceive;
        Func<string, bool> _onProgress;
        public string secretWord;

        public string Url { get => _url; set => _url = value; }
        public int Timeout { get => _timeout; set => _timeout = value; }
        public Func<string, bool> OnWord { get => _onWord; set => _onWord = value; }
        public Func<bool> OnStatusChange { get => _onStatusChange; set => _onStatusChange = value; }
        public Func<string, bool> OnReceive { get => _onReceive; set => _onReceive = value; }
        public Func<string, bool> OnProgress { get => _onProgress; set => _onProgress = value; }



        public TrustedTradeSocket(string url, int timeout = 10, Func<string, bool> onWord = null, Func<bool> onStatusChange = null, Func<string, bool> onReceive = null, Func<string, bool> onProgress = null)
        {
            Url = url;
            Timeout = timeout;
            OnWord = onWord;
            OnStatusChange = onStatusChange;
            OnReceive = onReceive;
            OnProgress = onProgress;
            
        }

        public async Task Connect()
        {
            
            try
            {
                ws = new ClientWebSocket();
                cts = new CancellationTokenSource();
                await ws.ConnectAsync(new Uri(Url), cts.Token);

            }catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            await Send("{\"type\":1}");

            await Task.Factory.StartNew(
            async () =>
            {
                var rcvBytes = new byte[128];
                var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                while (true)
                {
                    WebSocketReceiveResult rcvResult =
                    await ws.ReceiveAsync(rcvBuffer, cts.Token);
                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                    string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                    OnMessage(rcvMsg);
                    //Console.WriteLine("Received: {0}", rcvMsg);
                }
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        void OnMessage(string message)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            if(data["result"] != "success")
            {
                SetError(data["message"]);
                return;
            }
            PacketType packet = (PacketType)int.Parse(data["type"]);
            switch (packet)
            {
                case PacketType.PACKET_TYPE_WORD:
                    OnWord?.Invoke(data["data"]);
                    break;
                case PacketType.PACKET_TYPE_PROGRESS:
                    OnProgress?.Invoke(data["data"]);
                    break;
                case PacketType.PACKET_TYPE_DONE:
                    SetStatus(Status.STATUS_DONE);
                    break;
                case PacketType.PACKET_TYPE_OK:
                    if(status == Status.STATUS_REQUEST_RECIPIENT)
                        SetStatus(Status.STATUS_WAITING_RECIPIENT);
                    break;
                case PacketType.PACKET_TYPE_RECIPIENT_REPLY:
                    if(status != Status.STATUS_WAITING_RECIPIENT)
                    {
                        SetError("Protocol Error");
                        return;
                    }
                    Console.WriteLine("recipient replied:" + data["data"]);
                    break;
                case PacketType.PACKET_TYPE_HASH:
                    Console.WriteLine("Received CloudCoins");
                    Console.WriteLine("h=" + data["data"]);
                    OnReceive?.Invoke(data["data"]);
                    break;
                default:
                    SetError("Invalid packet " + data["type"]);
                    break;
            }
        }

        public string GetError()
        {
            return errorMsg;
        }

        public string GetStatus()
        {
            Dictionary<Status, string> r = new Dictionary<Status, string>
            {
                [Status.STATUS_DISCONNECTED] = "Disconnected",
                [Status.STATUS_ERROR] = "Error",
                [Status.STATUS_CONNECTED] = "Connected",
                [Status.STATUS_SENDING] = "Sending Coins",
                [Status.STATUS_DONE] = "Coins sent",
                [Status.STATUS_REQUEST_RECIPIENT] = "Waiting for recipient",
                [Status.STATUS_WAITING_RECIPIENT] = "Waiting for recipient",
            };

            return r[status];
        }

        public void SetStatus(Status newStatus)
        {
            status = newStatus;
            OnStatusChange?.Invoke();
        }

        public void SetError(string msg)
        {
            errorMsg = msg;
            SetStatus(Status.STATUS_ERROR);
        }

        public async Task SendCoins(string sh, string stack)
        {
            Dictionary<string, string> json = new Dictionary<string, string> { ["type"] = "3", ["word"] = sh, ["stack"] = stack};
            string message = JsonConvert.SerializeObject(json);
            await Send(message);
            SetStatus(Status.STATUS_REQUEST_RECIPIENT);//Status.STATUS_SENDING
        }

        public async Task Send(string message)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts.Token);
        }
    }
}
