using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FSMIS.DAL
{
    public class OracleFactory : IFactory
    {
        private static OracleFactory _instance;
        public static OracleFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OracleFactory();
                }
                return _instance;
            }
        }
        public int ExecuteNonQuery(string sql)
        {
            throw new NotImplementedException();
        }

        public DataTable Fill(string sql)
        {
            throw new NotImplementedException();
        }
        public DataTable GetByPage(string tableName,string orderBy, string condition, int pagesize, int pageIndex)
        {
            return null;
        }
    }
}