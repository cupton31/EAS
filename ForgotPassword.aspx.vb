Imports System.Data.SqlClient
Imports System.Net
Imports System.Net.Mail
Imports System.Net.Mime

Public Class ForgotPassword
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub Send_Click(sender As Object, e As EventArgs)
        Dim destination As string = EmailTB.Text
        Dim adminusername As string = ""
        Dim adminpassword As string = ""
        Dim employeeusername As string = ""
        Dim employeepassword As string = ""

        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("SELECT username, password from admin_tbl where email='"+EmailTB.Text+"'", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()
        Dim dt2 As New DataTable()
        Dim sqlCmd2 As New SqlCommand("SELECT username, password from employee_tbl where email='"+EmailTB.Text+"'", conn)
        Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
        conn.close()
        conn.open()
        sqlDa2.Fill(dt2)
        conn.close()


        Dim html As String = ""
        If (dt1.Rows.Count > 0) Then
            adminusername = dt1.Rows(0).ItemArray(0)
            adminpassword = dt1.Rows(0).ItemArray(1)
            html += "Admin Username: " + adminusername + " <br />"
            html += "Admin Password: " + adminpassword + " <br /><br />"
        End If
        If (dt2.Rows.Count > 0) Then
            employeeusername = dt2.Rows(0).ItemArray(0)
            employeepassword = dt2.Rows(0).ItemArray(1)
            html += "Employee Username: " + employeeusername + " <br />"
            html += "Employee Password: " + employeepassword + " <br /><br />"
        End If
        Dim text As String = String.Format("Someone requested a password for this email address.")

        ' Handle email doe not exist use case
        If (dt1.Rows.Count < 1 And dt2.Rows.Count < 1) Then
            Response.Write("<script> alert(""E-mail address selected does not exist in admin or employee tables!"")</script>")
            Return
        End if

        Dim msg As MailMessage = New MailMessage()
        msg.From = New MailAddress("no-reply@excaliburems.net")
        msg.To.Add(New MailAddress(destination))
        msg.Subject = "Password Recovery Request"
        msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, Encoding.Default, MediaTypeNames.Text.Plain))
        msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, Encoding.Default, MediaTypeNames.Text.Html))

        Dim smtpClient As SmtpClient = New SmtpClient("smtp.ionos.com", Convert.ToInt32(587))
        Dim credentials As System.Net.NetworkCredential = New System.Net.NetworkCredential("no-reply@excaliburems.net", "Fu7F-M(!bgG@Fcy!7oDvW@")
        smtpClient.Credentials = credentials
        smtpClient.EnableSsl = True
        smtpClient.Send(msg)

        Response.Redirect("~/LoginForm.aspx")

    End Sub

End Class