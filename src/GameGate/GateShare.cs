using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using SystemModule;
using SystemModule.Common;

namespace GameGate
{
    public class GateShare
    {
        public static object CS_MainLog = null;
        public static object CS_FilterMsg = null;
        public static IList<string> MainLogMsgList = null;
        public static int nShowLogLevel = 0;
        public static int ServerCount = 1;
        public static string GateClass = "GameGate";
        public static string GateAddr = "*";
        public static int GatePort = 7200;
        /// <summary>
        /// 显示B 或 KB
        /// </summary>
        public static bool boShowBite = true;
        public static bool boServiceStart = false;
        /// <summary>
        ///  网关游戏服务器之间检测超时时间长度
        /// </summary>
        public static long dwCheckServerTimeOutTime = 3 * 60 * 1000;
        public static IList<string> AbuseList = null;
        /// <summary>
        /// 是否显示SOCKET接收的信息
        /// </summary>
        public static bool boShowSckData = true;
        public static string sReplaceWord = "*";
        /// <summary>
        /// 转发封包（数据引擎-》网关）
        /// </summary>
        public static Channel<ForwardMessage> ForwardMsgList = null;
        public static int nCurrConnCount = 0;
        public static bool boSendHoldTimeOut = false;
        public static long dwSendHoldTick = 0;
        public static long dwCheckRecviceTick = 0;
        public static long dwCheckRecviceMin = 0;
        public static long dwCheckRecviceMax = 0;
        public static long dwCheckServerTick = 0;
        public static long dwCheckServerTimeMin = 0;
        public static long dwCheckServerTimeMax = 0;
        /// <summary>
        /// 累计接受数据大小
        /// </summary>
        public static int NReviceMsgSize;
        public static bool boDecodeMsgLock = false;
        public static long dwProcessReviceMsgTimeLimit = 0;
        public static long dwProcessSendMsgTimeLimit = 0;
        /// <summary>
        /// 禁止连接IP列表
        /// </summary>
        public static IList<string> BlockIPList = null;
        /// <summary>
        /// 临时禁止连接IP列表
        /// </summary>
        public static IList<string> TempBlockIPList = null;
        public static int nMaxConnOfIPaddr = 50;
        public static int nMaxClientPacketSize = 7000;
        public static int nNomClientPacketSize = 150;
        public static int dwClientCheckTimeOut = 50;
        public static int nMaxOverNomSizeCount = 2;
        public static int nMaxClientMsgCount = 15;
        public static TBlockIPMethod BlockMethod = TBlockIPMethod.mDisconnect;
        public static bool bokickOverPacketSize = true;
        /// <summary>
        /// 发送给客户端数据包大小限制
        /// </summary>
        public static int nClientSendBlockSize = 1000;
        /// <summary>
        /// 客户端连接会话超时(指定时间内未有数据传输)
        /// </summary>
        public static long dwClientTimeOutTime = 5000;
        public static IniFile Conf = null;
        private static string sConfigFileName = "config.conf";
        /// <summary>
        /// 会话超时时间
        /// </summary>
        public static long dwSessionTimeOutTime = 15 * 24 * 60 * 60 * 1000;
        public static bool[] Magic_Attack_Array;
        private static ConcurrentDictionary<int, int> MagicDelayTimeMap;
        public static IList<ClientThread> ServerGateList;
        public static Dictionary<string, ClientSession> PunishList;

        public static void AddMainLogMsg(string Msg, int nLevel)
        {
            string tMsg;
            try
            {
                HUtil32.EnterCriticalSection(CS_MainLog);
                tMsg = "[" + DateTime.Now + "] " + Msg;
                MainLogMsgList.Add(tMsg);
            }
            finally
            {
                HUtil32.LeaveCriticalSection(CS_MainLog);
            }
        }

        public static void LoadAbuseFile()
        {
            AddMainLogMsg("正在加载文字过滤配置信息...", 4);
            var sFileName = ".\\WordFilter.txt";
            if (File.Exists(sFileName))
            {
                try
                {
                    HUtil32.EnterCriticalSection(CS_FilterMsg);
                    //AbuseList.LoadFromFile(sFileName);
                }
                finally
                {
                    HUtil32.LeaveCriticalSection(CS_FilterMsg);
                }
            }
            AddMainLogMsg("文字过滤信息加载完成...", 4);
        }

        public static void LoadBlockIPFile()
        {
            AddMainLogMsg("正在加载IP过滤配置信息...", 4);
            var sFileName = ".\\BlockIPList.txt";
            if (File.Exists(sFileName))
            {
                //BlockIPList.LoadFromFile(sFileName);
            }
            AddMainLogMsg("IP过滤配置信息加载完成...", 4);
        }

        public static void Initialization()
        {
            Conf = new IniFile(Path.Combine(AppContext.BaseDirectory, sConfigFileName));
            Conf.Load();
            nShowLogLevel = Conf.ReadInteger(GateClass, "ShowLogLevel", nShowLogLevel);
            CS_MainLog = new object();
            CS_FilterMsg = new object();
            MainLogMsgList = new List<string>();
            AbuseList = new List<string>();
            ForwardMsgList = Channel.CreateUnbounded<ForwardMessage>();
            boShowSckData = false;
            BlockIPList = new List<string>();
            TempBlockIPList = new List<string>();
            MagicDelayTimeMap = new ConcurrentDictionary<int, int>();
            ServerGateList = new List<ClientThread>();
            InitMagicAttackMap();
            PunishList = new Dictionary<string, ClientSession>();
        }

        public static void InitMagicAttackMap()
        {
            Magic_Attack_Array = new bool[128]
            {
                false, true, false, false, false, true, true, false, false, true, true, true, false, true, false, false,
                false, false, false, false, false, false, true, true, true, false, false,
                false, false, false, false, false, true, true, false, false, true, true, true, true, false, true, false,
                false, true, true, false, true, false, false, false, true, true, true, true, false, false, true, true,
                false, false, false, false, false, false, false, false, false, false, false, false, true, true, true,
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
                true, true, true
            };
        }

        public static void InitMagicDelayTimeMap()
        {
            MagicDelayTimeMap[1] = 1000; //火球术
            MagicDelayTimeMap[2] = 1000; //治愈术
            MagicDelayTimeMap[5] = 1000; //大火球
            MagicDelayTimeMap[6] = 1000; //施毒术
            MagicDelayTimeMap[8] = 960; //抗拒火环
            MagicDelayTimeMap[9] = 1000; //地狱火
            MagicDelayTimeMap[10] = 1000; //疾光电影
            MagicDelayTimeMap[11] = 1000; //雷电术
            MagicDelayTimeMap[13] = 1000; //灵魂火符
            MagicDelayTimeMap[14] = 1000; //幽灵盾
            MagicDelayTimeMap[15] = 1000; //神圣战甲术
            MagicDelayTimeMap[16] = 1000; //困魔咒
            MagicDelayTimeMap[Grobal2.SKILL_SKELLETON] = 1000; //召唤骷髅
            MagicDelayTimeMap[18] = 1000; //隐身术
            MagicDelayTimeMap[19] = 1000; //集体隐身术
            MagicDelayTimeMap[20] = 1000; //诱惑之光
            MagicDelayTimeMap[21] = 1000; //瞬息移动
            MagicDelayTimeMap[22] = 1000; //火墙
            MagicDelayTimeMap[23] = 1000; //爆裂火焰
            MagicDelayTimeMap[24] = 1000; //地狱雷光
            MagicDelayTimeMap[28] = 1000; //心灵启示
            MagicDelayTimeMap[29] = 1000; //群体治疗术
            MagicDelayTimeMap[Grobal2.SKILL_SINSU] = 1000; //召唤神兽
            MagicDelayTimeMap[32] = 1000; //圣言术
            MagicDelayTimeMap[33] = 950; //冰咆哮
            MagicDelayTimeMap[34] = 1000; //解毒术
            MagicDelayTimeMap[35] = 1000; //狮吼功
            MagicDelayTimeMap[36] = 1000; //火焰冰
            MagicDelayTimeMap[37] = 1000; //群体雷电术
            MagicDelayTimeMap[38] = 1000; //群体施毒术
            MagicDelayTimeMap[39] = 960; //彻地钉
            MagicDelayTimeMap[41] = 960; //狮子吼
            MagicDelayTimeMap[44] = 1000; //寒冰掌
            MagicDelayTimeMap[45] = 1000; //灭天火
            MagicDelayTimeMap[46] = 1000; //分身术
            MagicDelayTimeMap[47] = 1000; //火龙焰
        }
    }

    public class THardwareHeader : Packets
    {
        public uint dwMagicCode;
        public byte[] xMd5Digest;

        public THardwareHeader(byte[] buffer)
        {
            var binaryReader = new BinaryReader(new MemoryStream(buffer));
            dwMagicCode = binaryReader.ReadUInt32();
            xMd5Digest = binaryReader.ReadBytes(16);
        }
    }
}