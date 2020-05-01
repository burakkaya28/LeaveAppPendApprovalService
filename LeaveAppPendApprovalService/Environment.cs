using System;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveAppPendApprovalService
{
    public class Environment
    {
        //Environment of Service
        public static string IsProduction = ConfigurationManager.AppSettings["IS_PRODUCTION"];

        /* PRODUCTION MAIL INFORMATION */
        public static string Receiver1 = ConfigurationManager.AppSettings["RECEIVER_CC_1"];
        public static string Receiver2 = ConfigurationManager.AppSettings["RECEIVER_CC_2"];
        public static string Receiver3 = ConfigurationManager.AppSettings["RECEIVER_CC_3"];
        public static string Receiver4 = ConfigurationManager.AppSettings["RECEIVER_CC_4"];
        public static string Receiver5 = ConfigurationManager.AppSettings["RECEIVER_CC_5"];
        public static string Receiver6 = ConfigurationManager.AppSettings["RECEIVER_CC_6"];
        public static string Receiver7 = ConfigurationManager.AppSettings["RECEIVER_CC_7"];
        
        public static string Bcc1 = ConfigurationManager.AppSettings["RECEIVER_BCC_1"];
        public static string Bcc2 = ConfigurationManager.AppSettings["RECEIVER_BCC_2"];
        public static string Bcc3 = ConfigurationManager.AppSettings["RECEIVER_BCC_3"];
        public static string Bcc4 = ConfigurationManager.AppSettings["RECEIVER_BCC_4"];
        public static string Bcc5 = ConfigurationManager.AppSettings["RECEIVER_BCC_5"];

        public static bool LeaveIsExist;

        /* DEVELOPMENT MAIL INFORMATION */
        public static string DevTo1 = "burak.kaya@optiim.com";
        
        /*SQL Connection String*/
        public static readonly string LeaveDbConString = ConfigurationManager.AppSettings["DB_CON_STRING"];

        //SQL Connection Variables
        private static SqlConnection _connection;
        private static SqlDataReader _sqlDataReader;

        public void SendAnEmail(bool isHtml, string body, string sender, string senderPass, string host, bool enableSsl, int port)
        {
            try
            {
                Console.WriteLine("Sending E-Mail process is starting...");
                var message = new MailMessage()
                {
                    From = new MailAddress(sender)
                };

                if (IsProduction == "1") //If env is PRODUCTION
                {
                    //Getting E-mail addresses of Waiting Approvers
                    _connection = new SqlConnection(LeaveDbConString);
                    _connection.Open();
                    _sqlDataReader = new SqlCommand("select distinct mg.Email from ( select case dr.Status when 0 then dr.ManagerId when 5 then u.SecondManager else '' end WaitingApprover from Users u join LeaveRequests dr on u.UserId= dr.UserId join LeaveTypes lt on lt.LeaveTypeId = dr.LeaveTypeId where (dr.Status=0 or dr.Status=5) ) pa JOIN Users mg On mg.UserId = pa.WaitingApprover", _connection).ExecuteReader();
                    while (_sqlDataReader.Read())
                    {
                        LeaveIsExist = true;
                        var managerEmail = _sqlDataReader["Email"].ToString();

                        //Adding to mail list as TO
                        if (managerEmail != "") message.To.Add(new MailAddress(managerEmail));
                    }
                    _sqlDataReader.Close();
                    _connection.Close();

                    //If there is at least one pending approval for leave
                    if(LeaveIsExist)
                    {
                        //Adding to mail list as CC
                        if (Receiver1 != "") message.CC.Add(new MailAddress(Receiver1));
                        if (Receiver2 != "") message.CC.Add(new MailAddress(Receiver2));
                        if (Receiver3 != "") message.CC.Add(new MailAddress(Receiver3));
                        if (Receiver4 != "") message.CC.Add(new MailAddress(Receiver4));
                        if (Receiver5 != "") message.CC.Add(new MailAddress(Receiver5));
                        if (Receiver6 != "") message.CC.Add(new MailAddress(Receiver6));
                        if (Receiver7 != "") message.CC.Add(new MailAddress(Receiver7));
                    }

                    //Adding to mail list as BCC
                    if (Bcc1 != "") message.Bcc.Add(new MailAddress(Bcc1));
                    if (Bcc2 != "") message.Bcc.Add(new MailAddress(Bcc2));
                    if (Bcc3 != "") message.Bcc.Add(new MailAddress(Bcc3));
                    if (Bcc4 != "") message.Bcc.Add(new MailAddress(Bcc4));
                    if (Bcc5 != "") message.Bcc.Add(new MailAddress(Bcc5));
                }
                else //If env is Development
                {
                    LeaveIsExist = true;
                    if (DevTo1 != "") //If developer mail is not null
                    {
                        message.To.Add(new MailAddress(DevTo1));
                    }
                }

                if (LeaveIsExist)
                {
                    //Mail Content
                    message.Subject = "İzinApp Onay Bekleyen İzinler Raporu " + DateTime.Now.ToString("dd.MM.yyy");
                    message.Body = body;
                    message.IsBodyHtml = isHtml;

                    var smtpClient = new SmtpClient()
                    {
                        Credentials = new NetworkCredential(sender, senderPass),
                        Host = host,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        EnableSsl = enableSsl,
                        Port = port
                    };

                    try
                    {
                        smtpClient.SendAsync(message, message);
                    }
                    catch (SmtpException ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        Console.WriteLine("Sending E-Mail process has completed succesfullly.");
                    }
                }
                else
                {
                    Console.WriteLine("E-Mail was not sent because there is no any leave.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
                Console.ReadLine();
            }
        }
    }
}