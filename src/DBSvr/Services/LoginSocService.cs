using System;
using System.Collections.Generic;
using SystemModule;
using SystemModule.Common;
using SystemModule.Sockets;

namespace DBSvr
{
    /// <summary>
    /// 账号中心
    /// </summary>
    public class LoginSocService
    {
        private IList<TGlobaSessionInfo> GlobaSessionList = null;
        private string m_sSockMsg = string.Empty;
        private string sIDAddr = string.Empty;
        private int nIDPort = 0;
        private readonly IClientScoket _socket;

        public LoginSocService()
        {
            _socket = new IClientScoket();
            _socket.ReceivedDatagram += IDSocketRead;
            _socket.OnConnected += IDSocketConnected;
            _socket.OnDisconnected += IDSocketDisconnected;
            _socket.OnError += IDSocketError;
            IniFile Conf = new IniFile(DBShare.sConfFileName);
            sIDAddr = Conf.ReadString("Server", "IDSAddr", DBShare.sIDServerAddr);
            nIDPort = Conf.ReadInteger("Server", "IDSPort", DBShare.nIDServerPort);
            Conf = null;
            GlobaSessionList = new List<TGlobaSessionInfo>();
        }

        private void IDSocketError(object sender, DSCClientErrorEventArgs e)
        {
            switch (e.ErrorCode)
            {
                case System.Net.Sockets.SocketError.ConnectionRefused:
                    DBShare.MainOutMessage("账号登陆服务器[" + sIDAddr + ":" + nIDPort + "]拒绝链接...");
                    break;
                case System.Net.Sockets.SocketError.ConnectionReset:
                    DBShare.MainOutMessage("账号登陆服务器[" + sIDAddr + ":" + nIDPort + "]关闭连接...");
                    break;
                case System.Net.Sockets.SocketError.TimedOut:
                    DBShare.MainOutMessage("账号登陆服务器[" + sIDAddr + ":" + nIDPort + "]链接超时...");
                    break;
            }
        }

        private void IDSocketConnected(object sender, DSCClientConnectedEventArgs e)
        {
            DBShare.MainOutMessage($"账号登陆服务器[{e.RemoteAddress}:{e.RemotePort}]链接成功.");
        }

        private void IDSocketDisconnected(object sender, DSCClientConnectedEventArgs e)
        {
            DBShare.MainOutMessage($"账号登录服务器[{e.RemoteAddress}:{e.RemotePort}]断开链接.");
        }

        public void Start()
        {
            _socket.Connect(sIDAddr, nIDPort);
        }

        public void Stop()
        {
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionList[i] = null;
            }
            GlobaSessionList = null;
        }

        public void CheckConnection()
        {
            if (_socket.IsConnected)
            {
                return;
            }
            if (_socket.IsBusy)
            { 
                return;
            }
            _socket.Connect(sIDAddr, nIDPort);
        }

        private void IDSocketRead(object sender, DSCClientDataInEventArgs e)
        {
            m_sSockMsg += e.ReceiveText;
            if (m_sSockMsg.IndexOf(")", StringComparison.Ordinal) > 0)
            {
                ProcessSocketMsg();
            }
        }

        private void ProcessSocketMsg()
        {
            string sData = string.Empty;
            string sCode = string.Empty;
            string sScoketText = m_sSockMsg;
            while ((sScoketText.IndexOf(")", StringComparison.Ordinal) > 0))
            {
                sScoketText = HUtil32.ArrestStringEx(sScoketText, "(", ")", ref sData);
                if (sData == "")
                {
                    break;
                }
                string sBody = HUtil32.GetValidStr3(sData, ref sCode, new string[] { "/" });
                int nIdent = HUtil32.Str_ToInt(sCode, 0);
                switch (nIdent)
                {
                    case Grobal2.SS_OPENSESSION:
                        ProcessAddSession(sBody);
                        break;
                    case Grobal2.SS_CLOSESESSION:
                        ProcessDelSession(sBody);
                        break;
                    case Grobal2.SS_KEEPALIVE:
                        ProcessGetOnlineCount(sBody);
                        break;
                }
            }
            m_sSockMsg = sScoketText;
        }

        public void SendSocketMsg(short wIdent, string sMsg)
        {
            const string sFormatMsg = "({0}/{1})";
            string sSendText = string.Format(sFormatMsg, wIdent, sMsg);
            if (_socket.IsConnected)
            {
                _socket.SendText(sSendText);
            }
        }

        public bool CheckSession(string sAccount, string sIPaddr, int nSessionID)
        {
            TGlobaSessionInfo GlobaSessionInfo;
            bool result = false;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.sAccount == sAccount) && (GlobaSessionInfo.nSessionID == nSessionID))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public int CheckSessionLoadRcd(string sAccount, string sIPaddr, int nSessionID, ref bool boFoundSession)
        {
            TGlobaSessionInfo GlobaSessionInfo;
            int result = -1;
            boFoundSession = false;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.sAccount == sAccount) && (GlobaSessionInfo.nSessionID == nSessionID))
                    {
                        boFoundSession = true;
                        if (!GlobaSessionInfo.boLoadRcd)
                        {
                            GlobaSessionInfo.boLoadRcd = true;
                            result = 1;
                        }
                        break;
                    }
                }
            }
            return result;
        }

        public bool SetSessionSaveRcd(string sAccount)
        {
            bool result = false;
            TGlobaSessionInfo GlobaSessionInfo;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.sAccount == sAccount))
                    {
                        GlobaSessionInfo.boLoadRcd = false;
                        result = true;
                    }
                }
            }
            return result;
        }

        public void SetGlobaSessionNoPlay(int nSessionID)
        {
            TGlobaSessionInfo GlobaSessionInfo;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.nSessionID == nSessionID))
                    {
                        GlobaSessionInfo.boStartPlay = false;
                        break;
                    }
                }
            }
        }

        public void SetGlobaSessionPlay(int nSessionID)
        {
            TGlobaSessionInfo GlobaSessionInfo;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.nSessionID == nSessionID))
                    {
                        GlobaSessionInfo.boStartPlay = true;
                        break;
                    }
                }
            }
        }

        public bool GetGlobaSessionStatus(int nSessionID)
        {
            bool result = false;
            TGlobaSessionInfo GlobaSessionInfo;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.nSessionID == nSessionID))
                    {
                        result = GlobaSessionInfo.boStartPlay;
                        break;
                    }
                }
            }
            return result;
        }

        public void CloseSession(string sAccount, int nSessionID)
        {
            TGlobaSessionInfo GlobaSessionInfo;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.nSessionID == nSessionID))
                    {
                        if (GlobaSessionInfo.sAccount == sAccount)
                        {
                            GlobaSessionInfo = null;
                            GlobaSessionList.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        private void ProcessAddSession(string sData)
        {
            string sAccount = string.Empty;
            string s10 = string.Empty;
            string s14 = string.Empty;
            string s18 = string.Empty;
            string sIPaddr = string.Empty;
            sData = HUtil32.GetValidStr3(sData, ref sAccount, new string[] { "/" });
            sData = HUtil32.GetValidStr3(sData, ref s10, new string[] { "/" });
            sData = HUtil32.GetValidStr3(sData, ref s14, new string[] { "/" });
            sData = HUtil32.GetValidStr3(sData, ref s18, new string[] { "/" });
            sData = HUtil32.GetValidStr3(sData, ref sIPaddr, new string[] { "/" });
            TGlobaSessionInfo GlobaSessionInfo = new TGlobaSessionInfo();
            GlobaSessionInfo.sAccount = sAccount;
            GlobaSessionInfo.sIPaddr = sIPaddr;
            GlobaSessionInfo.nSessionID = HUtil32.Str_ToInt(s10, 0);
            //GlobaSessionInfo.n24 = HUtil32.Str_ToInt(s14, 0);
            GlobaSessionInfo.boStartPlay = false;
            GlobaSessionInfo.boLoadRcd = false;
            GlobaSessionInfo.dwAddTick = HUtil32.GetTickCount();
            GlobaSessionInfo.dAddDate = DateTime.Now;
            GlobaSessionList.Add(GlobaSessionInfo);
        }

        private void ProcessDelSession(string sData)
        {
            string sAccount = string.Empty;
            TGlobaSessionInfo GlobaSessionInfo;
            sData = HUtil32.GetValidStr3(sData, ref sAccount, new string[] { "/" });
            int nSessionID = HUtil32.Str_ToInt(sData, 0);
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.nSessionID == nSessionID) && (GlobaSessionInfo.sAccount == sAccount))
                    {
                        GlobaSessionInfo = null;
                        GlobaSessionList.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public bool GetSession(string sAccount, string sIPaddr)
        {
            bool result = false;
            TGlobaSessionInfo GlobaSessionInfo;
            for (var i = 0; i < GlobaSessionList.Count; i++)
            {
                GlobaSessionInfo = GlobaSessionList[i];
                if (GlobaSessionInfo != null)
                {
                    if ((GlobaSessionInfo.sAccount == sAccount) && (GlobaSessionInfo.sIPaddr == sIPaddr))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        private void ProcessGetOnlineCount(string sData)
        {

        }

        public void SendKeepAlivePacket(int userCount)
        {
            if (_socket.IsConnected)
            {
                _socket.SendText("(" + Grobal2.SS_SERVERINFO + "/" + DBShare.sServerName + "/" + "99" + "/" + userCount + ")");
            }
        }
    }
}