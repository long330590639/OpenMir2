﻿using GameSvr.CommandSystem;
using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 退出行会
    /// </summary>
    [GameCommand("EndGuild", "退出行会", 0)]
    public class EndGuildCommand : BaseCommond
    {
        [DefaultCommand]
        public void EndGuild(TPlayObject PlayObject)
        {
            if (PlayObject.m_MyGuild != null)
            {
                if (PlayObject.m_nGuildRankNo > 1)
                {
                    if (PlayObject.m_MyGuild.IsMember(PlayObject.m_sCharName) && PlayObject.m_MyGuild.DelMember(PlayObject.m_sCharName))
                    {
                        M2Share.UserEngine.SendServerGroupMsg(Grobal2.SS_207, M2Share.nServerIndex, PlayObject.m_MyGuild.sGuildName);
                        PlayObject.m_MyGuild = null;
                        PlayObject.RefRankInfo(0, "");
                        PlayObject.RefShowName();
                        PlayObject.SysMsg("你已经退出行会。", MsgColor.Green, MsgType.Hint);
                    }
                }
                else
                {
                    PlayObject.SysMsg("行会掌门人不能这样退出行会!!!", MsgColor.Red, MsgType.Hint);
                }
            }
            else
            {
                PlayObject.SysMsg("你都没加入行会!!!", MsgColor.Red, MsgType.Hint);
            }
        }
    }
}