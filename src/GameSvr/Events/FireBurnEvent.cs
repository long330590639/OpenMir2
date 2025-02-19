﻿using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 火墙
    /// </summary>
    public class FireBurnEvent : Event
    {
        private int m_fireRunTick = 0;

        public FireBurnEvent(TBaseObject Creat, int nX, int nY, int nType, int nTime, int nDamage) : base(Creat.m_PEnvir, nX, nY, nType, nTime, true)
        {
            m_nDamage = nDamage;
            m_OwnBaseObject = Creat;
        }

        public override void Run()
        {
            TBaseObject TargeTBaseObject;
            if ((HUtil32.GetTickCount() - m_fireRunTick) > 3000)
            {
                m_fireRunTick = HUtil32.GetTickCount();
                IList<TBaseObject> BaseObjectList = new List<TBaseObject>();
                if (m_Envir != null)
                {
                    m_Envir.GetBaseObjects(m_nX, m_nY, true, BaseObjectList);
                    for (var i = 0; i < BaseObjectList.Count; i++)
                    {
                        TargeTBaseObject = BaseObjectList[i];
                        if (TargeTBaseObject != null && m_OwnBaseObject != null && m_OwnBaseObject.IsProperTarget(TargeTBaseObject))
                        {
                            TargeTBaseObject.SendMsg(m_OwnBaseObject, Grobal2.RM_MAGSTRUCK_MINE, 0, m_nDamage, 0, 0, "");
                        }
                    }
                }
                BaseObjectList.Clear();
                BaseObjectList = null;
            }
            base.Run();
        }
    }
}

