﻿using GameSvr.CommandSystem;
using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 调整指定玩家属性点
    /// </summary>
    [GameCommand("BonuPoint", "调整指定玩家属性点", "人物名称 属性点数(不输入为查看点数)", 10)]
    public class BonuPointCommand : BaseCommond
    {
        [DefaultCommand]
        public void BonuPoint(string[] @Params, TPlayObject PlayObject)
        {
            if (@Params == null)
            {
                return;
            }
            var sHumName = @Params.Length > 0 ? @Params[0] : "";
            var nCount = @Params.Length > 1 ? int.Parse(@Params[1]) : 0;
            string sMsg;
            if (string.IsNullOrEmpty(sHumName))
            {
                PlayObject.SysMsg(GameCommand.ShowHelp, MsgColor.Red, MsgType.Hint);
                return;
            }
            var m_PlayObject = M2Share.UserEngine.GetPlayObject(sHumName);
            if (m_PlayObject == null)
            {
                PlayObject.SysMsg(string.Format(M2Share.g_sNowNotOnLineOrOnOtherServer, sHumName), MsgColor.Red, MsgType.Hint);
                return;
            }
            if (nCount > 0)
            {
                m_PlayObject.m_nBonusPoint = nCount;
                m_PlayObject.SendMsg(PlayObject, Grobal2.RM_ADJUST_BONUS, 0, 0, 0, 0, "");
                return;
            }
            sMsg = string.Format("未分配点数:{0} 已分配点数:(DC:{1} MC:{2} SC:{3} AC:{4} MAC:{5} HP:{6} MP:{7} HIT:{8} SPEED:{9})", m_PlayObject.m_nBonusPoint,
                m_PlayObject.m_BonusAbil.DC, m_PlayObject.m_BonusAbil.MC, m_PlayObject.m_BonusAbil.SC, m_PlayObject.m_BonusAbil.AC,
                m_PlayObject.m_BonusAbil.MAC, m_PlayObject.m_BonusAbil.HP, m_PlayObject.m_BonusAbil.MP, m_PlayObject.m_BonusAbil.Hit, m_PlayObject.m_BonusAbil.Speed);
            PlayObject.SysMsg(string.Format("{0}的属性点数为:{1}", sHumName, sMsg), MsgColor.Red, MsgType.Hint);
        }
    }
}