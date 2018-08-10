using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Text;
using System.ServiceModel;
using System.Web.Security;
namespace FSMIS.Controllers
{
    public class HomeController : Controller
    {
        [MyFilter]
        public ActionResult Index()
        {
            loginUser u = Session["user"] as loginUser;
            ViewBag.Name = u.UserName;
            return View();
        }
        public void Test() {
           bool result = checksign();
            DAL.Mylog.Instance.WriteLog("是否有效:", result.ToString());
        }

        public bool checksign()
        {
            string sign = Request.QueryString["signature"].ToString();
            List<KeyValuePair<string, string>> pair = new List<KeyValuePair<string, string>>();
            pair.Add(new KeyValuePair<string, string>("nonce", Request.QueryString["nonce"].ToString()));
            pair.Add(new KeyValuePair<string, string>("timestamp", Request.QueryString["timestamp"].ToString()));
            pair.Add(new KeyValuePair<string, string>("token", "weixin"));

          
            string para = ToUrl(pair);          
            string signdata = FormsAuthentication.HashPasswordForStoringInConfigFile(para, "SHA1");            
            return sign == para;
        }
        private string ToUrl(List<KeyValuePair<string, string>> pair) {
            string buff = "";
            foreach (KeyValuePair<string,string> item in pair)
            {
                if (item.Value == null)
                {
                    DAL.Mylog.Instance.WriteLog(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    //Log.Error(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    //throw new WxPayException("WxPayData内部含有值为null的字段!");
                }

                if (item.Key != "sign" && item.Value.ToString() != "")//key为sign值为空的不加入到当中
                {
                    buff += item.Key + "=" + item.Value + "&";
                }
            }
            return buff.Trim('&');
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        /// <summary>
        /// 通过登录用户信息获取树目录
        /// </summary>
        public void getTree() {
            loginUser lu = Session["user"] as loginUser;
            string sql = string.Format("exec getDirByUserID @id={0}",lu.ID);
           System.Data.DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
           string msg = DAL.Commons.Instance.ToJson(dt, true);
            Response.Write(msg);
        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        public void login(string name, string pwd) {
            string sql = string.Format("select keyid,deptid,username from userinfo u where u.loginname='{0}' and password='{1}'", name, pwd);
           // DAL.Mylog.Instance.WriteLog("login", "this is sql:" + sql);
            System.Data.DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
            if (dt.Rows.Count > 0)
            {
                loginUser u = new loginUser() {  ID = dt.Rows[0]["keyid"].ToString(), DeptID = dt.Rows[0]["DeptID"].ToString(), UserName= dt.Rows[0]["UserName"].ToString(), LoginName=name,Pwd=pwd };
                Session["user"] = u;
                sql = string.Format("update userinfo set logintime='{0}' where keyid='{1}'", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), u.ID);
                DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
            }
            var par = new { sucess = dt.Rows.Count > 0 ? true : false };
            Response.Write(DAL.Commons.Instance.ToJson(par));
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult DirectoryInfo(string dirid) {
            ViewBag.id = dirid;
            return View();
        }
        /// <summary>
        /// 通过目录ID获取对应的文件
        /// </summary>
        /// <param name="dirid">目录ID</param>
        public void GetFileByDir(string dirid) {
            //string sql = string.Format("select * from fileinfo f where f.dirid='{0}'", ID);
            if (Request.Form["page"] != null)
            {
                int pagesize = int.Parse(Request.Form["rows"].ToString());
                int pageIndex = int.Parse(Request.Form["page"].ToString());
                string condition = "dirid="+dirid;
                if (Request.Form["condition"] != null)
                {
                    condition += " and " + Request.Form["condition"].ToString();
                }
                System.Data.DataTable dt = DAL.Commons.Instance.chooseFactory("sql").GetByPage("fileinfo","keyid",condition, pagesize, pageIndex);
                string msg = "error";
                if (dt!=null)
                {
                    msg = DAL.Commons.Instance.ToJsonByPage(dt, "fileinfo",condition);
                }
                
                Response.Write(msg);
            }
        }
        /// <summary>
        /// 文件上传
        /// </summary>
        public void UploadFiles() {
            bool result = false;
            if (Request.Form["option"].ToString() == "update")
            {
                string fn = Request.Form["filename"].ToString();
                string keyword = Request.Form["keyword"].ToString(); 
                string status = Request.Form["ispublic"].ToString() == "checked" ? "1" : "0";
                string id= Request.Form["id"].ToString();
                string sql = string.Format("update fileinfo set filename='{0}',keyword='{1}',status='{2}' where keyid={3}", fn, keyword, status, id);
                int i = DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
                result = i > 0 ? true : false;
            }
            else
            {
                if (Request.Files.Count > 0)
                {
                    loginUser lu = Session["user"] as loginUser;
                    string dir = string.Format("{0}//{1}//{2}", "upload", DateTime.Now.ToString("yyyy-MM-dd"), lu.LoginName);

                    string extName = Path.GetExtension(Request.Files[0].FileName);//.Split('.')[1].ToLower();
                    extName = extName.Substring(1);
                    string filename = DAL.Commons.Instance.GetUploadFileName(dir, extName);
                    Request.Files[0].SaveAs(AppDomain.CurrentDomain.BaseDirectory + filename);
                    string fn = Request.Form["filename"].ToString();
                    string keyword = Request.Form["keyword"].ToString();
                    string dirid = Request.Form["dirid"].ToString();
                    string userid = lu.ID;
                    string status = Request.Form["ispublic"].ToString() == "checked" ? "1" : "0";
                    string f1 = "doc,docx";
                    string f2 = "xls,xlsx";
                    string f3 = "txt,jpeg,png";
                    string f4 = "pdf";
                    int viewmode = 3;
                    string viewdir = "";
                    if (f1.IndexOf(extName) >= 0)
                    {
                        viewmode = 1;
                        Spire.Doc.Document doc = new Spire.Doc.Document();
                        doc.LoadFromFile(AppDomain.CurrentDomain.BaseDirectory + filename);
                        string newname = DAL.Commons.Instance.GetUploadFileName(dir, "pdf");
                        doc.SaveToFile(AppDomain.CurrentDomain.BaseDirectory +newname, Spire.Doc.FileFormat.PDF);
                        viewdir = newname;
                        doc.Close();
                    }
                    else if (f2.IndexOf(extName) >= 0)
                    {
                        viewmode = 1;
                        Spire.Xls.Workbook book = new Spire.Xls.Workbook();
                        book.LoadFromFile(AppDomain.CurrentDomain.BaseDirectory + filename);
                        string newname = DAL.Commons.Instance.GetUploadFileName(dir, "pdf");
                        book.ConverterSetting.SheetFitToPage = true;
                        book.SaveToFile(AppDomain.CurrentDomain.BaseDirectory +newname, Spire.Xls.FileFormat.PDF);
                        viewdir = newname;
                    }
                    else if (f3.IndexOf(extName) >= 0)
                    {
                        viewmode = 2;
                        viewdir = filename;
                    }
                    else if (f4.IndexOf(extName) >= 0)
                    {
                        viewmode = 1;
                        viewdir = filename;
                    }
                    Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
                   
                   
                    string sql = string.Format("insert into fileinfo (filename,keyword,dirid,userid,status,localdir,viewdir,viewmode,extname) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", fn, keyword, dirid, userid, status, filename,viewdir,viewmode,extName);
                    int i = DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
                    result = i > 0 ? true : false;
                }
            }
            var par = new { sucess = result };
            Response.Write(DAL.Commons.Instance.ToJson(par));
        }
        /// <summary>
        /// 根据文件ID下载
        /// </summary>
        /// <param name="fileid">文件ID</param>
        /// <returns></returns>
        public FileStreamResult DownFile(string fileid) {
            List<string> fs = new List<string>();
            string sql = string.Format("select localdir from fileinfo where keyid in ({0})", fileid);
            DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
            foreach (DataRow  item in dt.Rows)
            {
                fs.Add(AppDomain.CurrentDomain.BaseDirectory+item["localdir"].ToString());
            }
           
            //string filePath = AppDomain.CurrentDomain.BaseDirectory + path;//路径 
            loginUser lu = Session["user"] as loginUser;
            string fileName = lu.LoginName + ".zip";
            string filePath = AppDomain.CurrentDomain.BaseDirectory +"//downLoad//"+ fileName;
            DAL.Commons.Zip(fs.ToArray(), filePath);
            FileStreamResult fr = File(new FileStream(filePath,FileMode.Open), "text/plain",fileName);
            return fr;
        }
        /// <summary>
        ///  树控件操作
        /// </summary>
        public void TreeOP() {
            string filename = Request.Form["filename"].ToString();
            string sql = "";
            loginUser lu = Session["user"] as loginUser;
            if (Request.Form["option"].ToString() == "add")
            {
                string pid = Request.Form["pid"].ToString();
                sql = string.Format("insert into directoryinfo(dirname,parentid,userid) values ('{0}','{1}','{2}')", filename, pid, lu.ID);
            }
            else {
                string id = Request.Form["pid"].ToString();
                sql = string.Format("update directoryinfo set dirname='{0}' where keyid='{1}'", filename, id);
            }
            int i = DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
            var par = new { sucess = i > 0 };
            string msg = DAL.Commons.Instance.ToJson(par);
            Response.Write(msg);
        }
        /// <summary>
        /// 密码修改
        /// </summary>
        public void ModifyPwd() {
            loginUser lu = Session["user"] as loginUser;
            string pwd = Request.Form["pwd"].ToString();
            string npwd = Request.Form["npwd"].ToString();
            string nrpwd = Request.Form["nrpwd"].ToString();
            bool result = true;
            if (lu.Pwd!=pwd)
            {
                result = false;
            }
            string sql = string.Format("update userinfo set password='{0}' where keyid='{1}'",npwd,lu.ID);
            DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
            var par = new { sucess = result };
            Response.Write(DAL.Commons.Instance.ToJson(par));
        }

        public ActionResult FindFile() {
            return View();
        }
        /// <summary>
        /// 文件查询过滤
        /// </summary>
        /// <param name="condition"></param>
        public void GetFile(string condition) {
            if (string.IsNullOrEmpty(condition))
            {
                return;
            }
            if (Request.Form["page"] != null)
            {
                int pagesize = int.Parse(Request.Form["rows"].ToString());
                int pageIndex = int.Parse(Request.Form["page"].ToString());

                //if (Request.Form["condition"] != null)
                //{
                //    condition += " and " + Request.Form["condition"].ToString();
                //}
                string tableName = "(select f.keyid,f.filename,f.keyword,f.localdir,f.status,f.createtime,u.username,f.viewdir,f.viewmode,f.extname from fileinfo f,userinfo u where f.userid=u.keyid)";
                System.Data.DataTable dt = DAL.Commons.Instance.chooseFactory("sql").GetByPage(tableName, "keyid", condition, pagesize, pageIndex);
                string msg = "error";
                if (dt != null)
                {
                    msg = DAL.Commons.Instance.ToJsonByPage(dt, tableName,condition);
                }

                Response.Write(msg);
            }
                                                                                                                                                                                                                                                                                                                               
        }

        public ActionResult ShowList(string deptid) {
            ViewBag.deptid = deptid;
           // ViewBag.orderid = orderid;
         
            return View();
        }
        public ActionResult ShowList2(string deptid)
        {
            ViewBag.deptid = deptid;
            return View();
        }
        public ActionResult ShowList3(string deptid)
        {
            ViewBag.deptid = deptid;
            return View();
        }
        public ActionResult ShowList4(string deptid)
        {
            ViewBag.deptid = deptid;
            return View();
        }
        /// <summary>
        /// 前端PORTAL生成
        /// </summary>
        /// <param name="deptid"></param>
        public void GetShowList(string deptid) {
            string sql = string.Format("select top 10 fi.*,u.username from fileinfo fi,userinfo u where fi.userid=u.keyid and u.deptid='{0}'", deptid);
            DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
            Response.Write(DAL.Commons.Instance.ToJson(dt, true));

        }       
        /// <summary>
        ///pdf文件在线预览     
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public ActionResult pdfobject(string fileid) {
            string sql = string.Format("select viewdir,viewmode from fileinfo where keyid='{0}'", fileid);
            DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
            if (dt.Rows.Count>0)
            {
                ViewBag.fileid = "/"+dt.Rows[0][0].ToString();
                ViewBag.mode= dt.Rows[0][1].ToString();
            }
            
            loginUser lu = Session["user"] as loginUser;
            sql = string.Format("exec UpdateView @uid={0},@fid={1}", lu.ID, fileid);
            DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
            //if (dt.Rows[0][1].ToString()=="2")
            //{
            //    //Response.ContentEncoding = Encoding.GetEncoding("gb2312");
            //   // Response.Redirect()
            //}
            return View();
        }
        /// <summary>
        /// 根据文件ID拿到对应的浏览记录
        /// </summary>
        /// <param name="fid"></param>
        public void GetView(string fid) {
            string sql = string.Format("select * from userinfo u,viewinfo v where u.keyid=v.userid and v.fileid='{0}'", fid);
            DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
            Response.Write(DAL.Commons.Instance.ToJson(dt, true));
        }

        public void mytest() {
          
        }
        public void gos(string id) {
           
        }


        public void DelFile(string ID)
        {
            string sql = string.Format("select localdir from fileinfo where keyid={0}", ID);
            DataTable dt = DAL.Commons.Instance.chooseFactory("sql").Fill(sql);
            string localpath = AppDomain.CurrentDomain.BaseDirectory + dt.Rows[0][0].ToString();
            System.IO.File.Delete(localpath);
            sql = string.Format("delete from fileinfo where keyid={0}", ID);
            int i = DAL.Commons.Instance.chooseFactory("sql").ExecuteNonQuery(sql);
            var par = new { sucess = i > 0 };
            Response.Write(DAL.Commons.Instance.ToJson(par));
        }
    }
    
}