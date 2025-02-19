﻿using GameSvr.CommandSystem;
using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 调整当前玩家攻击模式
    /// </summary>
    [GameCommand("AttackMode", "调整当前玩家攻击模式", 0)]
    public class ChangeAttackModeCommand : BaseCommond
    {
        [DefaultCommand]
        public void ChangeAttackMode(TPlayObject PlayObject)
        {
            if (PlayObject.m_btAttatckMode >= M2Share.HAM_PKATTACK)
            {
                PlayObject.m_btAttatckMode = 0;
            }
            else
            {
                if (PlayObject.m_btAttatckMode < M2Share.HAM_PKATTACK)
                {
                    PlayObject.m_btAttatckMode++;
                }
                else
                {
                    PlayObject.m_btAttatckMode = M2Share.HAM_ALL;
                }
            }
            if (PlayObject.m_btAttatckMode < M2Share.HAM_PKATTACK)
            {
                PlayObject.m_btAttatckMode++;
            }
            else
            {
                PlayObject.m_btAttatckMode = M2Share.HAM_ALL;
            }
            switch (PlayObject.m_btAttatckMode)
            {
                case M2Share.HAM_ALL:// [攻击模式: 全体攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfAll, MsgColor.Green, MsgType.Hint);
                    break;
                case M2Share.HAM_PEACE: // [攻击模式: 和平攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfPeaceful, MsgColor.Green, MsgType.Hint);
                    break;
                case M2Share.HAM_DEAR:// [攻击模式: 和平攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfDear, MsgColor.Green, MsgType.Hint);
                    break;
                case M2Share.HAM_MASTER:// [攻击模式: 和平攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfMaster, MsgColor.Green, MsgType.Hint);
                    break;
                case M2Share.HAM_GROUP:// [攻击模式: 编组攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfGroup, MsgColor.Green, MsgType.Hint);
                    break;
                case M2Share.HAM_GUILD:// [攻击模式: 行会攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfGuild, MsgColor.Green, MsgType.Hint);
                    break;
                case M2Share.HAM_PKATTACK:// [攻击模式: 红名攻击]
                    PlayObject.SysMsg(M2Share.sAttackModeOfRedWhite, MsgColor.Green, MsgType.Hint);
                    break;
            }
            PlayObject.SendDefMessage(Grobal2.SM_ATTACKMODE, PlayObject.m_btAttatckMode, 0, 0, 0, "");
        }
    }
}