using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SystemModule;
using SystemModule.Packet;

namespace LoginSvr
{
    public class AccountDB
    {
        private IDbConnection ADOConnection = null;
        private IList<AccountQuick> m_QuickList = null;
        private int nRecordCount = 0;
        private IDbConnection _dbConnection;

        public AccountDB()
        {
            m_QuickList = new List<AccountQuick>();
            nRecordCount = -1;
            LSShare.g_boDataDBReady = false;
        }

        public void Initialization()
        {
            LSShare.MainOutMessage("0) 正在连接SQL服务器...");
            ADOConnection = new MySqlConnection(LSShare.DBConnection);
            try
            {
                ADOConnection.Open();
                LSShare.MainOutMessage("1) 连接SQL服务器成功...");
                LoadQuickList();
            }
            catch (Exception E)
            {
                LSShare.MainOutMessage("[警告] SQL 连接失败!请检查SQL设置...");
                LSShare.MainOutMessage(LSShare.DBConnection);
                LSShare.MainOutMessage(E.Message);
            }
        }

        public bool Open()
        {
            bool result = false;
            if (_dbConnection == null)
            {
                _dbConnection = new MySqlConnection(LSShare.DBConnection);
            }
            switch (_dbConnection.State)
            {
                case ConnectionState.Open:
                    return true;
                case ConnectionState.Closed:
                    try
                    {
                        _dbConnection.Open();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        LSShare.MainOutMessage("打开数据库[MySql]失败.");
                        result = false;
                    }
                    break;
            }
            return result;
        }

        public void Close()
        {
            if (_dbConnection != null)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
        }

        private void LoadQuickList()
        {
            int nIndex = 0;
            bool boDeleted;
            string sAccount;
            const string sSQL = "SELECT Id,FLD_DELETED，FLD_LOGINID FROM TBL_ACCOUNT";
            nRecordCount = -1;
            m_QuickList.Clear();
            try
            {
                if (!Open())
                {
                    return;
                }
                var command = new MySqlCommand();
                command.CommandText = sSQL;
                command.Connection = (MySqlConnection)_dbConnection;
                using  var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    nIndex = dr.GetInt32("Id");
                    boDeleted = dr.GetBoolean("FLD_DELETED");
                    sAccount = dr.GetString("FLD_LOGINID");
                    if (!boDeleted && (!string.IsNullOrEmpty(sAccount)))
                    {
                        m_QuickList.Add(new AccountQuick(sAccount, nIndex));
                    }
                }
                nRecordCount = m_QuickList.Count;
                dr.Close();
                dr.Dispose();
            }
            finally
            {
                Close();
            }
            //m_QuickList.SortString(0, m_QuickList.Count - 1);
            LSShare.g_boDataDBReady = true;
        }

        public int FindByName(string sName, ref IList<AccountQuick> List)
        {
            int result;
            for (var i = 0; i < m_QuickList.Count; i++)
            {
                if (HUtil32.CompareLStr(m_QuickList[i].sAccount, sName, sName.Length))
                {
                    List.Add(new AccountQuick(m_QuickList[i].sAccount, m_QuickList[i].nIndex));
                }
            }
            result = List.Count;
            return result;
        }

        public bool GetBy(int nIndex, ref TAccountDBRecord DBRecord)
        {
            bool result;
            if ((nIndex >= 0) && (m_QuickList.Count > nIndex))
            {
                result = GetRecord(nIndex, ref DBRecord);
            }
            else
            {
                result = false;
            }
            return result;
        }

        private bool GetRecord(int nIndex, ref TAccountDBRecord DBRecord)
        {
            const string sSQL = "SELECT * FROM TBL_ACCOUNT WHERE ID={0}";
            var result = true;
            if (!Open())
            {
                return false;
            }
            var command = new MySqlCommand();
            command.CommandText = string.Format(sSQL, nIndex);
            command.Connection = (MySqlConnection)_dbConnection;
            IDataReader dr;
            try
            {
                dr = command.ExecuteReader();
                if (DBRecord == null)
                {
                    DBRecord = new TAccountDBRecord();
                    DBRecord.Header = new TRecordHeader();
                    DBRecord.UserEntry = new TUserEntry();
                    DBRecord.UserEntryAdd = new TUserEntryAdd();
                }
                if (dr.Read())
                {
                    DBRecord.Header.sAccount = dr.GetString("FLD_LOGINID");
                    DBRecord.Header.boDeleted = dr.GetBoolean(dr.GetOrdinal("FLD_DELETED"));
                    DBRecord.Header.CreateDate = dr.GetDateTime("FLD_CREATEDATE");
                    DBRecord.Header.UpdateDate = dr.GetDateTime("FLD_LASTUPDATE");
                    DBRecord.nErrorCount = dr.GetInt32("FLD_ERRORCOUNT");
                    DBRecord.dwActionTick = dr.GetInt32("FLD_ACTIONTICK");
                    DBRecord.UserEntry.sAccount = dr.GetString("FLD_LOGINID");
                    DBRecord.UserEntry.sPassword = dr.GetString("FLD_PASSWORD");
                    DBRecord.UserEntry.sUserName = dr.GetString("FLD_USERNAME");
                    DBRecord.UserEntry.sSSNo = dr.GetString("FLD_SSNO");
                    DBRecord.UserEntry.sPhone = dr.GetString("FLD_PHONE");
                    DBRecord.UserEntry.sQuiz = dr.GetString("FLD_QUIZ1");
                    DBRecord.UserEntry.sAnswer = dr.GetString("FLD_ANSWER1");
                    DBRecord.UserEntry.sEMail = dr.GetString("FLD_EMAIL");
                    DBRecord.UserEntryAdd.sQuiz2 = dr.GetString("FLD_QUIZ2");
                    DBRecord.UserEntryAdd.sAnswer2 = dr.GetString("FLD_ANSWER2");
                    DBRecord.UserEntryAdd.sBirthDay = dr.GetString("FLD_BIRTHDAY");
                    DBRecord.UserEntryAdd.sMobilePhone = dr.GetString("FLD_MOBILEPHONE");
                    DBRecord.UserEntryAdd.sMemo = "";
                    DBRecord.UserEntryAdd.sMemo2 = "";
                }
                var quickAccount = m_QuickList.SingleOrDefault(x => x.nIndex == nIndex);
                if (quickAccount != null)
                {
                    if (DBRecord.Header.sAccount == quickAccount.sAccount)
                    {
                        result = true;
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch
            {
                result = false;
                LSShare.MainOutMessage("[Exception] TFileIDDB.GetRecord (1)");
                return result;
            }
            finally
            {
                Close();
            }
            return result;
        }

        public int Index(string sName)
        {
            var quick = m_QuickList.SingleOrDefault(o => o.sAccount == sName);
            if (quick == null)
            {
                return -1;
            }
            return quick.nIndex;
        }

        public int Get(int nIndex, ref TAccountDBRecord DBRecord)
        {
            int result = -1;
            if (nIndex < 0)
            {
                return result;
            }
            // if (m_QuickList.Count < nIndex)
            // {
            //     return result;
            // }
            if (GetRecord(nIndex, ref DBRecord))
            {
                result = nIndex;
            }
            return result;
        }

        private int UpdateRecord(TAccountDBRecord DBRecord, byte btFlag)
        {
            var result = 0;
            string sdt = "now()";
            const string sUpdateRecord1 = "INSERT INTO TBL_ACCOUNT (FLD_LOGINID, FLD_PASSWORD, FLD_USERNAME, FLD_CREATEDATE, FLD_LASTUPDATE, FLD_DELETED, FLD_ERRORCOUNT, FLD_ACTIONTICK, FLD_SSNO, FLD_BIRTHDAY, FLD_PHONE, FLD_MOBILEPHONE, FLD_EMAIL, FLD_QUIZ1, FLD_ANSWER1, FLD_QUIZ2, FLD_ANSWER2) VALUES('{0}', '{1}', '{2}', {3}, {4}, 0, 0, 0,'{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}');";
            const string sUpdateRecord2 = "UPDATE TBL_ACCOUNT SET FLD_DELETED=1, FLD_CREATEDATE='{0}' WHERE FLD_LOGINID='{1}'";
            const string sUpdateRecord0 = "UPDATE TBL_ACCOUNT SET FLD_PASSWORD='{0}', FLD_USERNAME='{1}',FLD_LASTUPDATE={2}, FLD_ERRORCOUNT={3}, FLD_ACTIONTICK={4},FLD_SSNO='{5}', FLD_BIRTHDAY='{6}', FLD_PHONE='{7}',FLD_MOBILEPHONE='{8}', FLD_EMAIL='{9}', FLD_QUIZ1='{10}', FLD_ANSWER1='{11}', FLD_QUIZ2='{12}',FLD_ANSWER2='{13}' WHERE FLD_LOGINID='{14}'";
            try
            {
                if (!Open())
                {
                    return 0;
                }
                var command = new MySqlCommand();
                command.Connection = (MySqlConnection)_dbConnection;
                switch (btFlag)
                {
                    case 1:
                        command.CommandText = string.Format(sUpdateRecord1, new object[] { DBRecord.UserEntry.sAccount, DBRecord.UserEntry.sPassword, DBRecord.UserEntry.sUserName, sdt, sdt, DBRecord.UserEntry.sSSNo, DBRecord.UserEntryAdd.sBirthDay, DBRecord.UserEntry.sPhone, DBRecord.UserEntryAdd.sMobilePhone, DBRecord.UserEntry.sEMail, DBRecord.UserEntry.sQuiz, DBRecord.UserEntry.sAnswer, DBRecord.UserEntryAdd.sQuiz2, DBRecord.UserEntryAdd.sAnswer2 });
                        try
                        {
                            command.ExecuteNonQuery();
                            result = (int)command.LastInsertedId;
                        }
                        catch (Exception E)
                        {
                            LSShare.MainOutMessage("[Exception] TFileIDDB.UpdateRecord");
                            LSShare.MainOutMessage(E.Message);
                            return 0;
                        }
                        break;
                    case 2:
                        command.CommandText = string.Format(sUpdateRecord2, sdt, DBRecord.UserEntry.sAccount);
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch
                        {
                            result = 0;
                            LSShare.MainOutMessage("[Exception] TFileIDDB.UpdateRecord (3)");
                        }
                        break;
                    default:
                        command.CommandText = string.Format(sUpdateRecord0, new object[] { DBRecord.UserEntry.sPassword, DBRecord.UserEntry.sUserName, sdt, DBRecord.nErrorCount, DBRecord.dwActionTick, DBRecord.UserEntry.sSSNo, DBRecord.UserEntryAdd.sBirthDay, DBRecord.UserEntry.sPhone, DBRecord.UserEntryAdd.sMobilePhone, DBRecord.UserEntry.sEMail, DBRecord.UserEntry.sQuiz, DBRecord.UserEntry.sAnswer, DBRecord.UserEntryAdd.sQuiz2, DBRecord.UserEntryAdd.sAnswer2, DBRecord.UserEntry.sAccount });
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception E)
                        {
                            result = 0;
                            LSShare.MainOutMessage("[Exception] TFileIDDB.UpdateRecord (0)");
                            LSShare.MainOutMessage(E.Message);
                            return result;
                        }
                        break;
                }
            }
            finally
            {
                Close();
            }
            return result;
        }

        public bool Update(int nIndex, ref TAccountDBRecord DBRecord)
        {
            bool result = false;
            if (nIndex < 0)
            {
                return result;
            }
            if (m_QuickList.Count <= nIndex)
            {
                return result;
            }
            if (UpdateRecord(DBRecord, 0) > 0)
            {
                result = true;
            }
            return result;
        }

        public bool Add(ref TAccountDBRecord DBRecord)
        {
            bool result;
            var sAccount = DBRecord.UserEntry.sAccount;
            if (Index(sAccount) > 0)
            {
                result = false;
            }
            else
            {
                var nIndex = UpdateRecord(DBRecord, 1);
                nRecordCount++;
                if (nIndex > 0)
                {
                    m_QuickList.Add(new AccountQuick(sAccount, nIndex));
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool Delete(int nIndex, ref TAccountDBRecord DBRecord)
        {
            bool result = false;
            if (nIndex < 0)
            {
                return result;
            }
            if (m_QuickList.Count <= nIndex)
            {
                return result;
            }
            var up = UpdateRecord(DBRecord, 2);
            if (up > 0)
            {
                m_QuickList.RemoveAt(nIndex);
                result = true;
            }
            return result;
        }

    }
}
