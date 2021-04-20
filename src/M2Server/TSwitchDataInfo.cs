﻿using System.Collections.Generic;

namespace M2Server
{
    public class TSwitchDataInfo
    {
        public string sMap;
        public short wX;
        public short wY;
        public TAbility Abil;
        public string sChrName;
        public int nCode;
        public bool boC70;
        public bool boBanShout;
        public bool boHearWhisper;
        public bool boBanGuildChat;
        public bool boAdminMode;
        public bool boObMode;
        public IList<string> BlockWhisperArr;
        public TSlaveInfo[] SlaveArr;
        public short[] StatusValue;
        public int[] StatusTimeOut;
        public int dwWaitTime;
    }
}