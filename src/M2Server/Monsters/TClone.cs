using System;

namespace M2Server
{
    public class TClone : TMonster
    {
        public TClone() : base()
        {
            m_dwSearchTime = M2Share.RandomNumber.Random(1500) + 1500;
        }

        public override bool Operate(TProcessMessage ProcessMsg)
        {
            var result = false;
            if (ProcessMsg.wIdent == grobal2.RM_STRUCK || ProcessMsg.wIdent == grobal2.RM_MAGSTRUCK || ProcessMsg.wIdent == grobal2.RM_SPELL)
            {
                if (m_Master != null)
                {
                    if (m_Master.m_WAbil.MP <= 0)
                    {
                        m_WAbil.HP = 0;
                    }
                    // kill slave if your mp is 0
                    if (ProcessMsg.wIdent == grobal2.RM_SPELL)
                    {
                        M2Share.MainOutMessage("rmSpell: " + ProcessMsg.nParam3);
                        m_Master.m_WAbil.MP -= (short)ProcessMsg.nParam3;
                    }
                    else
                    {
                        M2Share.MainOutMessage("rmHit: " + ProcessMsg.wParam);
                        m_Master.m_WAbil.MP -= (short)ProcessMsg.wParam;
                    }
                }
            }
            return result;
        }

        private void LightingAttack(int nDir)
        {
            short nSX = 0;
            short nSY = 0;
            short nTX = 0;
            short nTY = 0;
            int nPwr;
            TAbility WAbil;
            m_btDirection = (byte)nDir;
            SendRefMsg(grobal2.RM_LIGHTING, 1, m_nCurrX, m_nCurrY, m_TargetCret.ObjectId, "");
            if (m_PEnvir.GetNextPosition(m_nCurrX, m_nCurrY, nDir, 1, ref nSX, ref nSY))
            {
                m_PEnvir.GetNextPosition(m_nCurrX, m_nCurrY, nDir, 9, ref nTX, ref nTY);
                WAbil = m_WAbil;
                nPwr = M2Share.RandomNumber.Random(HUtil32.HiWord(WAbil.DC) - HUtil32.LoWord(WAbil.DC) + 1) + HUtil32.LoWord(WAbil.DC);
                MagPassThroughMagic(nSX, nSY, nTX, nTY, nDir, nPwr, true);
            }
            BreakHolySeizeMode();
        }

        public override void Struck(TBaseObject hiter)
        {
            if (hiter == null)
            {
                return;
            }
            // m_btDirection:=hiter.m_btDirection;
            // n550:=Random(4) + (n550 + 4);
            // n550:=_MIN(20,n550);
            // m_PEnvir.GetNextPosition(m_nCurrX,m_nCurrY,m_btDirection,n550,m_nTargetX,m_nTargetY);
        }

        public override void Run()
        {
            int nAttackDir;
            if (!m_boDeath && !bo554 && !m_boGhost && m_wStatusTimeArr[grobal2.POISON_STONE] == 0 && HUtil32.GetTickCount() - m_dwSearchEnemyTick > 8000)
            {

                if (HUtil32.GetTickCount() - m_dwSearchEnemyTick > 1000 && m_TargetCret == null)
                {

                    m_dwSearchEnemyTick = HUtil32.GetTickCount();
                    SearchTarget();
                }
                if (HUtil32.GetTickCount() - m_dwWalkTick > m_nWalkSpeed && m_TargetCret != null && Math.Abs(m_nCurrX - m_TargetCret.m_nCurrX) <= 4 && Math.Abs(m_nCurrY - m_TargetCret.m_nCurrY) <= 4)
                {
                    if (Math.Abs(m_nCurrX - m_TargetCret.m_nCurrX) <= 2 && Math.Abs(m_nCurrY - m_TargetCret.m_nCurrY) <= 2 && M2Share.RandomNumber.Random(3) != 0)
                    {
                        base.Run();
                        return;
                    }
                    GetBackPosition(ref m_nTargetX, ref m_nTargetY);
                }

                if (m_TargetCret != null && Math.Abs(m_nCurrX - m_TargetCret.m_nCurrX) < 6 && Math.Abs(m_nCurrY - m_TargetCret.m_nCurrY) < 6 && HUtil32.GetTickCount() - m_dwHitTick > m_nNextHitTime)
                {

                    m_dwHitTick = HUtil32.GetTickCount();
                    nAttackDir = M2Share.GetNextDirection(m_nCurrX, m_nCurrY, m_TargetCret.m_nCurrX, m_TargetCret.m_nCurrY);
                    LightingAttack(nAttackDir);
                }
            }
            base.Run();
        }
    }
}

