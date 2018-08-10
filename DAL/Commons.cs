using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections;
using System.Data;
using Ionic.Zip;
namespace FSMIS.DAL
{
    public class Commons
    {
        private static Commons _instance;

        public static Commons Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Commons();
                }
                return _instance;
            }

        }
        public IFactory chooseFactory(string type) {
            IFactory factory = null;
            switch (type)
            {
                case "sql":
                    factory = SQLFactory.Instance;
                    break;             
                case "oracle":
                    factory = OracleFactory.Instance;
                    break;
                default:
                    break;
            }
            return factory;
        }
        public string Md5Crypt(string str)
        {
            string pwd = "";
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < s.Length; i++)
            {
                pwd += s[i].ToString("X");
            }
            return pwd;
        }
        public string GetGuid()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
        private Commons() { }

        private JObject streamToJson()
        {
            StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream);
            string msg = sr.ReadToEnd();
            JObject myarr = null;
            try
            {
                myarr = JsonConvert.DeserializeObject(msg) as JObject;
            }
            catch (Exception ex)
            {
                Mylog.Instance.WriteLog(this.GetType().Name, "获取JSON对象出错!--" + msg);
            }
            return myarr;
        }
        public T JsonToClass<T>()
        {
            Type t = typeof(T);
            PropertyInfo[] pro = t.GetProperties();
            JObject myjson = streamToJson();
            //Mylog.Instance.WriteLog("ffs", myjson.ToString());
            object o = Activator.CreateInstance(t);
            foreach (PropertyInfo pi in pro)
            {

                if (myjson[pi.Name.ToLower()] == null)
                {
                    continue;
                }
                try
                {

                    pi.SetValue(o, Convert.ChangeType(myjson[pi.Name.ToLower()], pi.PropertyType), null);

                }
                catch (Exception ex)
                {
                    Mylog.Instance.WriteLog(this.GetType().Name, "setValue出错!" + pi.Name + ":values:" + myjson[pi.Name.ToLower()]);
                }
            }
            // t.GetProperty("KeyId").SetValue()

            return (T)o;
        }
        public T FormDataToClass<T>()
        {
            Type t = typeof(T);
            PropertyInfo[] prs = t.GetProperties();
            object obj = Activator.CreateInstance(t);
            foreach (PropertyInfo pi in prs)
            {
                var p = HttpContext.Current.Request.Form[pi.Name];
                if (p == null)
                {
                    continue;
                }
                pi.SetValue(obj, Convert.ChangeType(p, pi.PropertyType), null);
            }
            return (T)obj;
        }
        public string ToJson(object o)
        {
            string msg = JsonConvert.SerializeObject(o);
            return msg;
            // HttpContext.Current.Response.Write(msg);
            // HttpContext.Current.Response.End();
        }
        public string ToJson(DataTable dt, bool total)
        {
            ArrayList al = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dic.Add(col.ColumnName.ToLower(), item[col.ColumnName]);
                }
                al.Add(dic);
            }
            object obj;
            if (total)
                obj = new { total = dt.Rows.Count, rows = al };
            else
                obj = al;
            return ToJson(obj);
            // string msg = JsonConvert.SerializeObject(par);
        }
        public string ToJsonByPage(DataTable dt,string TableName,string condition) {
            ArrayList al = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dic.Add(col.ColumnName.ToLower(), item[col.ColumnName]);
                }
                al.Add(dic);
            }
            if (string.IsNullOrEmpty(condition))
            {
                condition = "1=1";
            }
            string sql = string.Format("select count(*) from {0} as a where {1}",TableName,condition);
            var counts = chooseFactory("sql").Fill(sql).Rows[0][0];
            object obj;           
                obj = new { total = counts, rows = al };
           
            return ToJson(obj);
        }
        public string GetUploadFileName(string DirName,string ExtName) {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + DirName;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string filename =string.Format("{0}.{1}",DateTime.Now.ToString("yyyy-MM-ddHHmmssms"),ExtName);
            return DirName + "//" + filename;
        }
        public static void Zip(string[] files,string ZipedFile) {
            ZipFile zip = new ZipFile(ZipedFile, Encoding.UTF8);
            foreach (string item in files)
            {
                zip.AddFile(item);
            }
            zip.Save();
        }
    }
}