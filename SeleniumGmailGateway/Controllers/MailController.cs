using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using S22.Imap;

namespace SeleniumGmailGateway.Controllers
{
    public class MailModel // ha ha
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Mode { get; set; }
    }
    public class MailController : Controller
    {
        //
        // GET: /Mail/

        public ActionResult Index()
        {
            return View(new MailModel());
        }

        [HttpPost]
        public ActionResult Headers(MailModel model)
        {
            using (var imapClient = new ImapClient(@"imap.gmail.com", 993, model.UserName, model.Password, AuthMethod.Auto, true))
            {
                var inbox = imapClient.GetMailboxInfo(imapClient.DefaultMailbox);

                var condition = SearchCondition.SentOn(DateTime.Today);
                if (!string.IsNullOrWhiteSpace(model.From))
                {
                    condition = condition.And(SearchCondition.From(model.From));
                }
                if (!string.IsNullOrWhiteSpace(model.To))
                {
                    condition = condition.And(SearchCondition.To(model.To));
                }
                var messageUids = imapClient.Search(condition, imapClient.DefaultMailbox);
                var message = imapClient.GetMessage(messageUids.Max(), FetchOptions.HeadersOnly);
                
                return View(message);
            }

        }

        [HttpPost]
        public ActionResult Body(MailModel model)
        {
            using (var imapClient = new ImapClient(@"imap.gmail.com", 993, model.UserName, model.Password, AuthMethod.Auto, true))
            {
                var inbox = imapClient.GetMailboxInfo(imapClient.DefaultMailbox);

                var condition = SearchCondition.SentOn(DateTime.Today);
                if (!string.IsNullOrWhiteSpace(model.From))
                {
                    condition = condition.And(SearchCondition.From(model.From));
                }
                if (!string.IsNullOrWhiteSpace(model.To))
                {
                    condition = condition.And(SearchCondition.To(model.To));
                }
                var messageUids = imapClient.Search(condition, imapClient.DefaultMailbox);
                var message = imapClient.GetMessage(messageUids.Max());

                var alternateView = message.AlternateViews.FirstOrDefault();
                if (alternateView != null)
                {
                    using (var streamReader = new StreamReader(alternateView.ContentStream))
                    {
                        return Content(streamReader.ReadToEnd(), alternateView.ContentType.MediaType, Encoding.UTF8);
                    }
                    

                }

                return Content(message.Body, message.IsBodyHtml ? @"text/html" : "text/plain", message.BodyEncoding);
            }

        }


    }
}
