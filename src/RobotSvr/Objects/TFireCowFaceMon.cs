﻿namespace RobotSvr;

public class TFireCowFaceMon : TGasKuDeGi
{
    public TFireCowFaceMon(RobotClient robotClient) : base(robotClient)
    {
    }

    public override int light()
    {
        int result;
        int L;
        L = m_nChrLight;
        if (L < 2)
            if (m_boUseEffect)
                L = 2;
        result = L;
        return result;
    }
}