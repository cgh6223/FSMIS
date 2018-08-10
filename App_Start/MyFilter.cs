using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.IO;
namespace FSMIS
{
    public class MyFilter:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["user"]==null)
            {
                filterContext.Result = new RedirectResult("/index.html");
            }
          //  base.OnActionExecuting(filterContext);
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
        }
    }
    public class loginUser {
       
        public string ID { get; set; }
        public string UserName { get; set; }
        public string DeptID { get; set; }
        public string LoginName { get; set; }
        public string Pwd { set; get; }
       
    }
    public class handleF : IHttpHandler
    {
        public bool IsReusable {
            get {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {            
           
           string upath = context.Request.PhysicalPath;
            using (Bitmap bit = new Bitmap(upath))
            {
                Graphics g = Graphics.FromImage(bit);
                Font f = new Font("宋体", 12);
                Brush b = new SolidBrush(Color.Red);
                g.DrawString("测试", f, b, 0, 0);
                MemoryStream ms = new MemoryStream();
                bit.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                context.Response.BinaryWrite(ms.GetBuffer());
                //context.Response.BinaryWrite(ms.GetBuffer());
                ms.Close();
                //context.Response.WriteFile()
            }
           
            //context.Response.Write()
        }
    }

}