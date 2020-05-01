using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using static System.String;

namespace LeaveAppPendApprovalService
{
    public class Db
    {
        private static SqlConnection _connection;
        private static SqlDataReader _sqlDataReader;
        public const string ReportTitle = "OPTiiM İzinApp Onay Bekleyen İzinler Raporu";
        private static string _reportMessage = Empty;

        public static readonly string LeaveDbConString = ConfigurationManager.AppSettings["DB_CON_STRING"];
        public static readonly string LeaveAppBaseUrl = ConfigurationManager.AppSettings["APP_BASE_URL"];

        public string GetHtmlData()
        {
            var flag = false;
            const int colNumber = 12;

              _reportMessage =
                "<table border='1' style='width: 100%; border: 1px solid black;'>" +
                "   <thead>" +
                "       <tr>" +
                "           <td colspan='"+colNumber+"' style='text-align: center;'>" +
                "               <b> " + ReportTitle + " </b>" +
                "           </td>" +
                "       </tr>" +
                "       <tr>" +
                "           <th>İzin No</th>" +
                "           <th>Kaynak Adı</th>" +
                "           <th>Rol</th>" +
                "           <th>Takım</th>" +
                "           <th>Oluşturma Tarihi</th>" +
                "           <th>İzin Başlangıç Tarihi</th>" +
                "           <th>İzin Bitiş Tarihi</th>" +
                "           <th>İzin Süresi</th>" +
                "           <th>İzin Tipi</th>" +
                "           <th>Onay Seviyesi</th>" +
                "           <th>Onay Beklenen Kişi</th>" +
                "           <th style='width:24%;'>İzin Açıklaması</th>" +
                "       </tr>" +
                "   </thead>" +
                "<tbody style='text-align: center;'>";

            _connection = new SqlConnection(LeaveDbConString);
            _connection.Open();
            _sqlDataReader = new SqlCommand("select pa.LeaveId, pa.ResourceName, pa.ResourceRole, pa.Team, pa.CreationDate, pa.StartDate, pa.EndDate, pa.LeaveDuration, pa.LeaveType, pa.ApprovalLevel, mg.FullName WaitingApprover, pa.Description from ( select dr.ID LeaveId, u.FullName ResourceName, u.Role ResourceRole, t.TeamName Team, dr.CreatedOn CreationDate, dr.StartDate, dr.EndDate, dr.Day LeaveDuration, lt.LeaveTypeTr LeaveType, case dr.Status when 0 then '1' when 5 then '2' else '' end ApprovalLevel, case dr.Status when 0 then dr.ManagerId when 5 then u.SecondManager else '' end WaitingApprover, dr.Description from Users u join LeaveRequests dr on u.UserId= dr.UserId join LeaveTypes lt on lt.LeaveTypeId = dr.LeaveTypeId left join Teams t on t.TeamId = u.TeamId where (dr.Status=0 or dr.Status=5) ) pa JOIN Users mg On mg.UserId = pa.WaitingApprover where 1=1 order by 1", _connection).ExecuteReader();
            while (_sqlDataReader.Read())
            {
                flag = true;

                var leaveId = _sqlDataReader["LeaveId"].ToString();
                var resourceName = _sqlDataReader["ResourceName"].ToString();
                var resourceRole = _sqlDataReader["ResourceRole"].ToString();
                var team = _sqlDataReader["Team"].ToString();
                var creationDate = DateTime.Parse(_sqlDataReader["CreationDate"].ToString()).Date;
                var startDate = DateTime.Parse(_sqlDataReader["StartDate"].ToString()).Date;
                var endDate = DateTime.Parse(_sqlDataReader["EndDate"].ToString()).Date;
                var leaveDuration = _sqlDataReader["LeaveDuration"].ToString();
                var leaveType = _sqlDataReader["LeaveType"].ToString();
                var approvalLevel = _sqlDataReader["ApprovalLevel"].ToString();
                var waitingApprover = _sqlDataReader["WaitingApprover"].ToString();
                var description = _sqlDataReader["Description"].ToString();

                _reportMessage += 
                "<tr>" +
                    "<td><a href='"+ LeaveAppBaseUrl + "/LeaveDetails?id=" + leaveId +"'>#" + leaveId + "</a></td>" +
                    "<td>" + resourceName + "</td>" +
                    "<td>" + resourceRole + "</td>" +
                    "<td>" + team + "</td>" +
                    "<td>" + creationDate.ToString(CultureInfo.DefaultThreadCurrentCulture).Replace(" 12:00:00 AM", "").Replace("00:00:00", "") + "</td>" +
                    "<td>" + startDate.ToString(CultureInfo.DefaultThreadCurrentCulture).Replace(" 12:00:00 AM", "").Replace("00:00:00", "") + "</td>" +
                    "<td>" + endDate.ToString(CultureInfo.DefaultThreadCurrentCulture).Replace(" 12:00:00 AM", "").Replace("00:00:00", "") + "</td>" +
                    "<td>" + leaveDuration + "</td>" +
                    "<td>" + leaveType + "</td>" +
                    "<td>" + approvalLevel + "</td>" +
                    "<td>" + waitingApprover + "</td>" +
                    "<td>" + description + "</td>" +
                "</tr>";
            }
            _sqlDataReader.Close();
            _connection.Close();

            if (!flag)
                _reportMessage += "<tr><td colspan='" + colNumber + "'>Onay bekleyen izin bulunmamaktadır.</td></tr>";

            return _reportMessage + "</tbody></table>";
        }
    }
}