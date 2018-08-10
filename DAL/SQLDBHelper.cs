using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
namespace FSMIS.DAL
{
    public class SQLFactory : IFactory
    {
        private SQLFactory()
        { }
        private static SQLFactory _instance;
        public static SQLFactory Instance {
            get {
                if (_instance == null) {
                    _instance = new SQLFactory();                    
                }
                return _instance;
            }
        }
        private static readonly string _conStr = ConfigurationManager.AppSettings["conStr"].ToString();
        public int ExecuteNonQuery(string sql)
        {
            int result = 0;
            SqlConnection con = new SqlConnection(_conStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {


            }
            return result;
           

        }

        public DataTable Fill(string sql)
        {
            DataTable dt = null; 
            try
            {
                SqlConnection con = new SqlConnection(_conStr);
                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {

                Mylog.Instance.WriteLog("SQLDBhelper", "执行SQL出错：SQL：" + sql);
                return null;
            }
        }
        public DataTable GetByPage(string tableName,string orderBy,string condition, int pagesize, int pageIndex)
        {
            if (string.IsNullOrEmpty(condition))
            {
                condition = "1=1";
            }
            string sql = string.Format("SELECT w2.n, w1.* FROM {0} w1, (SELECT TOP {3} row_number() OVER (ORDER BY {2}) n, keyid FROM {0} as ff  where {1} ) w2 WHERE w1.keyid = w2.keyid AND w2.n > {4}  ORDER BY w2.n ASC ", tableName, condition, orderBy, pagesize * pageIndex, (pageIndex - 1) * pagesize);
           return Fill(sql);
        }
    }
}