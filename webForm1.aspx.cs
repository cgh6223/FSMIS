using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.IO;
namespace FSMIS
{
    public partial class webForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string msg = "";
                if (Request.QueryString.Count>0)
                {                   
                    foreach (var item in Request.QueryString.Keys)
                    {
                        msg += string.Format("{0}={1}&", item.ToString(), Request.QueryString[item.ToString()].ToString());
                    }
                   msg = msg.Trim('&');
                }
               
                Response.Write(Request.QueryString["echostr"].ToString());
                Response.End();
            }
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
        private string ToUrl(List<KeyValuePair<string, string>> pair)
        {
            string buff = "";
            foreach (KeyValuePair<string, string> item in pair)
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
    }
}