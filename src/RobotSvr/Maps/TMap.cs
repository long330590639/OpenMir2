﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using SystemModule;

namespace RobotSvr
{
    public class TMap : TPathMap
    {
        private readonly RobotClient robotClient;
        public Point[] Path
        {
            get
            {
                return FPath;
            }
            set
            {
                FPath = value;
            }
        }
        private Point[] FPath;
        public TMapInfo[,] m_MArr;
        public bool m_boChange = false;
        public Rectangle m_ClientRect;
        public Rectangle m_OClientRect;
        public Rectangle m_OldClientRect;
        public int m_nBlockLeft = 0;
        public int m_nBlockTop = 0;
        public int m_nOldLeft = 0;
        public int m_nOldTop = 0;
        public string m_sOldMap = String.Empty;
        public int m_nCurUnitX = 0;
        public int m_nCurUnitY = 0;
        public string m_sCurrentMap = String.Empty;
        public int m_nCurrentMap;
        public int m_nSegXCount = 0;
        public int m_nSegYCount = 0;

        public TMap(RobotClient robotClient) : base()
        {
            this.robotClient = robotClient;
            m_ClientRect = new Rectangle(0, 0, 0, 0);
            m_boChange = false;
            m_sCurrentMap = "";
            m_nCurrentMap = 0;
            m_nSegXCount = 0;
            m_nSegYCount = 0;
            m_nCurUnitX = -1;
            m_nCurUnitY = -1;
            m_nBlockLeft = -1;
            m_nBlockTop = -1;
            m_sOldMap = "";
        }

        public void LoadMapData(bool bFirst = false)
        {
            int X;
            int Y;
            int n;
            int nMapSize;
            TMapInfo_Old[] TempMapInfoArr;
            TMapInfo_2[] TempMapInfoArr2;
            bool canMove;
            if (m_nCurrentMap != 0)
            {
                if (this.m_MapBuf == null)
                {
                    //nMapSize = this.m_MapHeader.wWidth * sizeof(TMapInfo) * this.m_MapHeader.wHeight;
                    //this.m_MapBuf = AllocMem(nMapSize);
                    //FileSeek(m_nCurrentMap); switch ((byte)this.m_MapHeader.Reserved[0])
                    //{
                    //    case 6:
                    //        FileRead(m_nCurrentMap, this.m_MapBuf, nMapSize);
                    //        break;
                    //    case 2:
                    //        n = this.m_MapHeader.wWidth * sizeof(TMapInfo_2) * this.m_MapHeader.wHeight;
                    //        TempMapInfoArr2 = AllocMem(n);
                    //        FileRead(m_nCurrentMap, TempMapInfoArr2, n);
                    //        for (X = 0; X < this.m_MapHeader.wWidth * this.m_MapHeader.wHeight; X++)
                    //        {
                    //            Move(TempMapInfoArr2[X], this.m_MapBuf[X]);
                    //        }
                    //        FreeMem(TempMapInfoArr2);
                    //        break;
                    //    default:
                    //        n = this.m_MapHeader.wWidth * sizeof(TMapInfo_Old) * this.m_MapHeader.wHeight;
                    //        TempMapInfoArr = AllocMem(n);
                    //        FileRead(m_nCurrentMap, TempMapInfoArr, n);
                    //        for (X = 0; X < this.m_MapHeader.wWidth * this.m_MapHeader.wHeight; X++)
                    //        {
                    //            Move(TempMapInfoArr[X], this.m_MapBuf[X]);
                    //        }
                    //        FreeMem(TempMapInfoArr);
                    //        break;
                    //}
                }
                if (this.m_MapBuf != null)
                {
                    if (this.m_MapData.Length <= 0)
                    {
                        this.m_MapData = new TCellParams[this.m_MapHeader.wWidth];
                    }
                    for (X = 0; X < this.m_MapHeader.wWidth; X++)
                    {
                        n = X * this.m_MapHeader.wHeight;
                        for (Y = 0; Y < this.m_MapHeader.wHeight; Y++)
                        {
                            this.m_MapData[X, Y].TCellActor = false;
                            canMove = ((this.m_MapBuf[n + Y].wBkImg & 0x8000) + (this.m_MapBuf[n + Y].wFrImg & 0x8000)) == 0;
                            if (canMove)
                            {
                                this.m_MapData[X, Y].TerrainType = false;
                            }
                            else
                            {
                                this.m_MapData[X, Y].TerrainType = true;
                            }
                        }
                    }
                    ReLoadMapData(false);
                }
            }
        }

        public bool ReLoadMapData(bool IntActor = false)
        {
            int nX;
            int nY;
            TActor Actor;
            bool result = false;
            if ((MShare.g_MySelf != null) && (m_nCurrentMap != 0) && (this.m_MapBuf != null))
            {
                for (nX = MShare.g_MySelf.m_nCurrX - 32; nX <= MShare.g_MySelf.m_nCurrX + 32; nX++)
                {
                    for (nY = MShare.g_MySelf.m_nCurrY - 32; nY <= MShare.g_MySelf.m_nCurrY + 32; nY++)
                    {
                        if ((nX >= 0) && (nX < this.m_MapHeader.wWidth) && (nY >= 0) && (nY < this.m_MapHeader.wHeight))
                        {
                            this.m_MapData[nX, nY].TCellActor = false;
                        }
                    }
                }
                for (var i = 0; i < robotClient.g_PlayScene.m_ActorList.Count; i++)
                {
                    Actor = (TActor)robotClient.g_PlayScene.m_ActorList[i];
                    if (Actor == MShare.g_MySelf)
                    {
                        continue;
                    }
                    if ((Actor.m_nCurrX >= MShare.g_MySelf.m_nCurrX - 32) && (Actor.m_nCurrX <= MShare.g_MySelf.m_nCurrX + 32) && (Actor.m_nCurrY >= MShare.g_MySelf.m_nCurrY - 32) && (Actor.m_nCurrY <= MShare.g_MySelf.m_nCurrY + 32))
                    {
                        if (Actor.m_boVisible && Actor.m_boHoldPlace && (!Actor.m_boDeath))
                        {
                            this.m_MapData[Actor.m_nCurrX, Actor.m_nCurrY].TCellActor = true;
                        }
                    }
                }
            }
            return result;
        }

        private void LoadMapArr(int nCurrX, int nCurrY)
        {
            int i;
            int j;
            int nAline;
            int nLx;
            int nRx;
            int nTy;
            int nBy;
            if (m_nCurrentMap != 0)
            {
                //FillChar(m_MArr); 
                nLx = (nCurrX - 1) * MShare.LOGICALMAPUNIT;
                nRx = (nCurrX + 2) * MShare.LOGICALMAPUNIT;
                nTy = (nCurrY - 1) * MShare.LOGICALMAPUNIT;
                nBy = (nCurrY + 2) * MShare.LOGICALMAPUNIT;
                if (nLx < 0)
                {
                    nLx = 0;
                }
                if (nTy < 0)
                {
                    nTy = 0;
                }
                if (nBy >= this.m_MapHeader.wHeight)
                {
                    nBy = this.m_MapHeader.wHeight;
                }
                //switch ((byte)this.m_MapHeader.Reserved[0])
                //{
                //    case 6:
                //        nAline = sizeof(TMapInfo) * this.m_MapHeader.wHeight;
                //        for (i = nLx; i < nRx; i++)
                //        {
                //            if ((i >= 0) && (i < this.m_MapHeader.wWidth))
                //            {
                //                FileSeek(m_nCurrentMap); 
                //                FileRead(m_nCurrentMap, m_MArr[i - nLx, 0]);
                //            }
                //        }
                //        break;
                //    case 2:
                //        nAline = sizeof(TMapInfo_2) * this.m_MapHeader.wHeight;
                //        for (i = nLx; i < nRx; i++)
                //        {
                //            if ((i >= 0) && (i < this.m_MapHeader.wWidth))
                //            {
                //                FileSeek(m_nCurrentMap); 
                //                for (j = 0; j < nBy - nTy; j++)
                //                {
                //                    FileRead(m_nCurrentMap, m_MArr[i - nLx, j]);
                //                }
                //            }
                //        }
                //        break;
                //    default:
                //        nAline = sizeof(TMapInfo_Old) * this.m_MapHeader.wHeight;
                //        for (i = nLx; i < nRx; i++)
                //        {
                //            if ((i >= 0) && (i < this.m_MapHeader.wWidth))
                //            {
                //                FileSeek(m_nCurrentMap); 
                //                for (j = 0; j < nBy - nTy; j++)
                //                {
                //                    FileRead(m_nCurrentMap, m_MArr[i - nLx, j]);
                //                }
                //            }
                //        }
                //        break;
                //}
            }
        }

        public void ReadyReload()
        {
            m_nCurUnitX = -1;
            m_nCurUnitY = -1;
        }

        public void UpdateMapSquare(int cx, int cy)
        {
            if ((cx != m_nCurUnitX) || (cy != m_nCurUnitY))
            {
                LoadMapArr(cx, cy);
                m_nCurUnitX = cx;
                m_nCurUnitY = cy;
            }
        }

        public void UpdateMapPos_Unmark(int xx, int yy, ref int cx, ref int cy)
        {
            int ax;
            int ay;
            if ((cx == xx / MShare.LOGICALMAPUNIT) && (cy == yy / MShare.LOGICALMAPUNIT))
            {
                ax = xx - m_nBlockLeft;
                ay = yy - m_nBlockTop;
                m_MArr[ax, ay].wFrImg = (ushort)(m_MArr[ax, ay].wFrImg & 0x7FFF);
                m_MArr[ax, ay].wBkImg = (ushort)(m_MArr[ax, ay].wBkImg & 0x7FFF);
            }
        }

        public void UpdateMapPos(int mx, int my)
        {
            int cx = mx / MShare.LOGICALMAPUNIT;
            int cy = my / MShare.LOGICALMAPUNIT;
            m_nBlockLeft = HUtil32._MAX(0, (cx - 1) * MShare.LOGICALMAPUNIT);
            m_nBlockTop = HUtil32._MAX(0, (cy - 1) * MShare.LOGICALMAPUNIT);
            UpdateMapSquare(cx, cy);
            if ((m_nOldLeft != m_nBlockLeft) || (m_nOldTop != m_nBlockTop) || (m_sOldMap != m_sCurrentMap))
            {
                if (m_sCurrentMap == "3")
                {
                    UpdateMapPos_Unmark(624, 278, ref cx, ref cy);
                    UpdateMapPos_Unmark(627, 278, ref cx, ref cy);
                    UpdateMapPos_Unmark(634, 271, ref cx, ref cy);
                    UpdateMapPos_Unmark(564, 287, ref cx, ref cy);
                    UpdateMapPos_Unmark(564, 286, ref cx, ref cy);
                    UpdateMapPos_Unmark(661, 277, ref cx, ref cy);
                    UpdateMapPos_Unmark(578, 296, ref cx, ref cy);
                }
            }
            m_nOldLeft = m_nBlockLeft;
            m_nOldTop = m_nBlockTop;
        }

        public void LoadMap(string sMapName, int nMx, int nMy)
        {
            m_nCurUnitX = -1;
            m_nCurUnitY = -1;
            m_sCurrentMap = sMapName;
            if (m_nCurrentMap != 0)
            {
                m_nCurrentMap.Close();
                m_nCurrentMap = 0;
            }
            string sFileName = string.Format("{0}{1}{2}", MAP_BASEPATH, m_sCurrentMap, ".map");
            if (File.Exists(sFileName))
            {
                //m_nCurrentMap = File.Open(sFileName, (FileMode)FileAccess.Read | FileShare.ReadWrite);
                //if (m_nCurrentMap != 0)
                //{
                //    if (FileRead(m_nCurrentMap, this.m_MapHeader));
                //    {
                //        m_nCurrentMap.Close();
                //        m_nCurrentMap = 0;
                //    }
                //}
                UpdateMapPos(nMx, nMy);
            }
            m_sOldMap = m_sCurrentMap;
        }

        public void MarkCanWalk(int mx, int my, bool bowalk)
        {
            int cx = mx - m_nBlockLeft;
            int cy = my - m_nBlockTop;
            if ((cx < 0) || (cy < 0))
            {
                return;
            }
            if (bowalk)
            {
                robotClient.Map.m_MArr[cx, cy].wFrImg = (ushort)(robotClient.Map.m_MArr[cx, cy].wFrImg & 0x7FFF);
            }
            else
            {
                robotClient.Map.m_MArr[cx, cy].wFrImg = (ushort)(robotClient.Map.m_MArr[cx, cy].wFrImg | 0x8000);
            }
        }

        public bool CanMove(int mx, int my)
        {
            bool result = false;
            int cx = mx - m_nBlockLeft;
            int cy = my - m_nBlockTop;
            if ((cx < 0) || (cy < 0))
            {
                return result;
            }
            result = ((robotClient.Map.m_MArr[cx, cy].wBkImg & 0x8000) + (robotClient.Map.m_MArr[cx, cy].wFrImg & 0x8000)) == 0;
            if (result)
            {
                if ((robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x80) > 0)
                {
                    if ((robotClient.Map.m_MArr[cx, cy].btDoorOffset & 0x80) == 0)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool CanFly(int mx, int my)
        {
            bool result = false;
            int cx = mx - m_nBlockLeft;
            int cy = my - m_nBlockTop;
            if ((cx < 0) || (cy < 0))
            {
                return result;
            }
            result = (robotClient.Map.m_MArr[cx, cy].wFrImg & 0x8000) == 0;
            if (result)
            {
                if ((robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x80) > 0)
                {
                    if ((robotClient.Map.m_MArr[cx, cy].btDoorOffset & 0x80) == 0)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public int GetDoor(int mx, int my)
        {
            int result = 0;
            int cx = mx - m_nBlockLeft;
            int cy = my - m_nBlockTop;
            if ((robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x80) > 0)
            {
                result = robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x7F;
            }
            return result;
        }

        public bool IsDoorOpen(int mx, int my)
        {
            bool result;
            int cx;
            int cy;
            result = false;
            cx = mx - m_nBlockLeft;
            cy = my - m_nBlockTop;
            if ((robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x80) > 0)
            {
                result = (robotClient.Map.m_MArr[cx, cy].btDoorOffset & 0x80) != 0;
            }
            return result;
        }

        public bool OpenDoor(int mx, int my)
        {
            bool result;
            int i;
            int j;
            int cx;
            int cy;
            int idx;
            result = false;
            cx = mx - m_nBlockLeft;
            cy = my - m_nBlockTop;
            if ((cx < 0) || (cy < 0))
            {
                return result;
            }
            if (robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x80 > 0)
            {
                idx = robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x7F;
                for (i = cx - 10; i <= cx + 10; i++)
                {
                    for (j = cy - 10; j <= cy + 10; j++)
                    {
                        if ((i > 0) && (j > 0))
                        {
                            if ((robotClient.Map.m_MArr[i, j].btDoorIndex & 0x7F) == idx)
                            {
                                robotClient.Map.m_MArr[i, j].btDoorOffset = robotClient.Map.m_MArr[i, j].btDoorOffset | 0x80;
                            }
                        }
                    }
                }
            }
            return result;
        }

        public bool CloseDoor(int mx, int my)
        {
            bool result;
            int i;
            int j;
            int cx;
            int cy;
            int idx;
            result = false;
            cx = mx - m_nBlockLeft;
            cy = my - m_nBlockTop;
            if ((cx < 0) || (cy < 0))
            {
                return result;
            }
            if (robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x80 > 0)
            {
                idx = robotClient.Map.m_MArr[cx, cy].btDoorIndex & 0x7F;
                for (i = cx - 8; i <= cx + 10; i++)
                {
                    for (j = cy - 8; j <= cy + 10; j++)
                    {
                        if ((robotClient.Map.m_MArr[i, j].btDoorIndex & 0x7F) == idx)
                        {
                            robotClient.Map.m_MArr[i, j].btDoorOffset = robotClient.Map.m_MArr[i, j].btDoorOffset & 0x7F;
                        }
                    }
                }
            }
            return result;
        }

        public Point[] FindPath(int StartX, int StartY, int StopX, int StopY, int PathSpace)
        {
            Point[] result;
            this.m_nPathWidth = PathSpace;
            this.m_PathMapArray = this.FillPathMap(StartX, StartY, StopX, StopY);
            // 费时
            result = this.FindPathOnMap(StopX, StopY);
            return result;
        }

        public Point[] FindPath(int StopX, int StopY)
        {
            Point[] result;
            result = this.FindPathOnMap(StopX, StopY);
            return result;
        }

        public void SetStartPos(int StartX, int StartY, int PathSpace)
        {
            this.m_nPathWidth = PathSpace;
            this.m_PathMapArray = this.FillPathMap(StartX, StartY, -1, -1);
        }

    }
}

