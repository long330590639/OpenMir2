﻿using GameSvr.CommandSystem;
using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 师徒传送，师父可以将徒弟传送到自己身边，徒弟必须允许传送。
    /// </summary>
    [GameCommand("MasterRecall", "师徒传送，师父可以将徒弟传送到自己身边，徒弟必须允许传送。", 0)]
    public class MasterRecallCommand : BaseCommond
    {
        [DefaultCommand]
        public void MasterRecall(TPlayObject PlayObject)
        {
            TPlayObject MasterHuman;
            if (!PlayObject.m_boMaster)
            {
                PlayObject.SysMsg("只能师父才能使用此功能!!!", MsgColor.Red, MsgType.Hint);
                return;
            }
            if (PlayObject.m_MasterList.Count == 0)
            {
                PlayObject.SysMsg("你的徒弟一个都不在线!!!", MsgColor.Red, MsgType.Hint);
                return;
            }
            //if (PlayObject.m_PEnvir.m_boNOMASTERRECALL)
            //{
            //    PlayObject.SysMsg("本地图禁止师徒传送!!!", TMsgColor.c_Red, TMsgType.t_Hint);
            //    return;
            //}
            if ((HUtil32.GetTickCount() - PlayObject.m_dwMasterRecallTick) < 10000)
            {
                PlayObject.SysMsg("稍等一会才能再次使用此功能!!!", MsgColor.Red, MsgType.Hint);
                return;
            }
            for (var i = 0; i < PlayObject.m_MasterList.Count; i++)
            {
                MasterHuman = PlayObject.m_MasterList[i];
                if (MasterHuman.m_boCanMasterRecall)
                {
                    PlayObject.RecallHuman(MasterHuman.m_sCharName);
                }
                else
                {
                    PlayObject.SysMsg(MasterHuman.m_sCharName + " 不允许传送!!!", MsgColor.Red, MsgType.Hint);
                }
            }
        }
    }
}