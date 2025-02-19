﻿using GameSvr.CommandSystem;
using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 重新加载怪物爆率文件
    /// </summary>
    [GameCommand("ReloadMonItems", "重新加载怪物爆率文件", 10)]
    public class ReloadMonItemsCommand : BaseCommond
    {
        [DefaultCommand]
        public void ReloadMonItems(TPlayObject PlayObject)
        {
            TMonInfo Monster;
            try
            {
                for (var i = 0; i < M2Share.UserEngine.MonsterList.Count; i++)
                {
                    Monster = M2Share.UserEngine.MonsterList[i];
                    M2Share.LocalDB.LoadMonitems(Monster.sName, ref Monster.ItemList);
                }
                PlayObject.SysMsg("怪物爆物品列表重加载完成...", MsgColor.Green, MsgType.Hint);
            }
            catch
            {
                PlayObject.SysMsg("怪物爆物品列表重加载失败!!!", MsgColor.Green, MsgType.Hint);
            }
        }
    }
}