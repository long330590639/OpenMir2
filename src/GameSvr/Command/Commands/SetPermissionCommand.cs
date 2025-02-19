﻿using GameSvr.CommandSystem;
using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 调整指定玩家权限
    /// </summary>
    [GameCommand("SetPermission", "调整指定玩家权限", "人物名称 权限等级(0 - 10)", 10)]
    public class SetPermissionCommand : BaseCommond
    {
        [DefaultCommand]
        public void SetPermission(string[] @Params, TPlayObject PlayObject)
        {
            if (@Params == null)
            {
                return;
            }
            var sHumanName = @Params.Length > 0 ? @Params[0] : "";
            var sPermission = @Params.Length > 1 ? @Params[1] : "";
            var nPerission = HUtil32.Str_ToInt(sPermission, 0);
            const string sOutFormatMsg = "[权限调整] {0} [{1} {2} -> {3}]";
            if (string.IsNullOrEmpty(sHumanName) || !(nPerission >= 0 && nPerission <= 10))
            {
                PlayObject.SysMsg(GameCommand.ShowHelp, MsgColor.Red, MsgType.Hint);
                return;
            }
            var m_PlayObject = M2Share.UserEngine.GetPlayObject(sHumanName);
            if (m_PlayObject == null)
            {
                PlayObject.SysMsg(string.Format(M2Share.g_sNowNotOnLineOrOnOtherServer, sHumanName), MsgColor.Red, MsgType.Hint);
                return;
            }
            if (M2Share.g_Config.boShowMakeItemMsg)
            {
                M2Share.MainOutMessage(string.Format(sOutFormatMsg, PlayObject.m_sCharName, m_PlayObject.m_sCharName, m_PlayObject.m_btPermission, nPerission));
            }
            m_PlayObject.m_btPermission = (byte)nPerission;
            PlayObject.SysMsg(sHumanName + " 当前权限为: " + m_PlayObject.m_btPermission, MsgColor.Red, MsgType.Hint);
        }
    }
}