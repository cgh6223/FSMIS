using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
namespace FSMIS.DAL
{
    public interface IFactory
    {
         int ExecuteNonQuery(string sql);
        DataTable Fill(string sql);
        DataTable GetByPage(string tableName,string orderBy, string condition, int pagesize, int pageIndex);
    }
}