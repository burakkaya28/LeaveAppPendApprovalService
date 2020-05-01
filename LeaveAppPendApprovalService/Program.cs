using System;
using System.Configuration;
using System.Threading;
using static System.String;

namespace LeaveAppPendApprovalService
{
    public class P
    {
        private readonly string _senderMail = ConfigurationManager.AppSettings["SENDER_MAIL"];
        private readonly string _senderMailPass = ConfigurationManager.AppSettings["SENDER_MAIL_PASSWORD"];
        private readonly string _senderHost = ConfigurationManager.AppSettings["SENDER_HOST"];
        private readonly bool _senderSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SENDER_SSL"]);
        private readonly int _senderPort = Convert.ToInt32(ConfigurationManager.AppSettings["SENDER_PORT"]);
        private string _tableHtml = Empty;

        private static void Main()
        {
            Console.WriteLine("Starting LeaveApp Pending Approvals Report Service...");

            var p = new P();
            var db = new Db();
            try
            {
                Console.WriteLine("Getting SQL Data from LeaveApp Database...");
                p._tableHtml = db.GetHtmlData();
                Console.WriteLine("Getting SQL Data from LeaveApp Database has completed succesfullly.");

                Console.WriteLine("Sending HTML Data to E-Mail System...");
                new Environment().SendAnEmail(true, p._tableHtml, p._senderMail, p._senderMailPass, p._senderHost,
                    p._senderSsl, p._senderPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("LeaveApp Pending Approvals Report Service has completed succesfullly.");
                Thread.Sleep(2500);
            }
        }
    }
}
