using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Configuration;
using OCTWEB_NET45.Models;
using System.Net.Mime;

namespace OCTWEB_NET45.Controllers
{
    public class SendMailCenterController : Controller
    {
        //
        // GET: /SendMailCenter/
        public ActionResult Index()
        {
            return View();
        }


        //
        //SendMailCenter/SendMail
        public static void SendMail(SendMailCenterModel model)
        {
            MailMessage msg = new MailMessage() ;
            msg.IsBodyHtml = true; //add
            msg.From = new MailAddress(ConfigurationManager.AppSettings["fromEmail"]);
            //To
            foreach (var to_address in model.To)
            {
                msg.To.Add(new MailAddress(to_address));
            }
            // To cc
            foreach (var to_cc_address in model.Tocc) 
            {
                msg.CC.Add(new MailAddress(to_cc_address));
            }
            msg.Subject = model.Subject;
            //string Body = model.Body;
            //msg.Body = Body;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(model.Body, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient(
                    ConfigurationManager.AppSettings["mailHost"],
                    Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]));

            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(
                                                                           ConfigurationManager.AppSettings["mailAccount"],
                                                                           ConfigurationManager.AppSettings["mailPassword"]);
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = false;
             
            try
            {
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateCopyMessage(): {0}",
                     ex.ToString());
            }

        }  
	}
}