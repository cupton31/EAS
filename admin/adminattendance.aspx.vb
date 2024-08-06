Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO
Imports System.Web.UI.HtmlTextWriter
Imports System.IO.StringWriter
Imports System.Windows.Forms
Imports System.Runtime.CompilerServices
Imports System.Drawing
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock
Imports System.Runtime.InteropServices.ComTypes
Public Class adminattendance
    Inherits System.Web.UI.Page
    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)

    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If

        If (Not IsPostBack) Then
            '' Date Set
            Dim startdate = DateTime.Now.AddDays(-7)
            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            tbDateFrom.Text = startdate.Date.ToString("yyyy-MM-dd")
            tbDateTo.Text = DateTime.Now.Date.ToString("yyyy-MM-dd")

            '' Populate the Employee DropDownLists
            myCmd = conn.CreateCommand
            myCmd.CommandText = "SELECT username, employee_id FROM employee_tbl WHERE status='Active' ORDER BY employee_tbl.username ASC"
            conn.Open()
            myReader = myCmd.ExecuteReader()
            employee_ddl.Items.Clear()
            employee_ddl.Items.Add(" ")
            employee_ddl.Items.FindByText(" ").Value = 0
            Do While myReader.Read()
                results = myReader.GetString(0)
                employee_ddl.Items.Add(results)
                employee_ddl.Items.FindByText(results).Value = myReader.GetInt32(1)
            Loop
            myReader.Close()
            conn.Close()
        End If

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        'Log_Change(Session("user"), Log Out, "Logged Out", "")

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub
    Protected Sub employee_ddl_textChanged(sender As Object, e As EventArgs)
        SqlDataSource1.FilterExpression = "employee_id='"+employee_ddl.SelectedValue+"' AND datetime >='"+DateTime.Parse(tbDateFrom.Text)+"' AND datetime <='"+DateTime.Parse(tbDateTo.Text)+"'"
        GridView1.DataBind()
    End Sub
    Protected Sub fromDate_TextChanged(sender As Object, e As EventArgs)
        SqlDataSource1.FilterExpression = "employee_id='"+employee_ddl.SelectedValue+"' AND datetime >='"+DateTime.Parse(tbDateFrom.Text)+"' AND datetime <='"+DateTime.Parse(tbDateTo.Text)+"'"
        GridView1.DataBind()
    End Sub
    Protected Sub toDate_TextChanged(sender As Object, e As EventArgs)
        SqlDataSource1.FilterExpression = "employee_id='"+employee_ddl.SelectedValue+"' AND datetime >='"+DateTime.Parse(tbDateFrom.Text)+"' AND datetime <='"+DateTime.Parse(tbDateTo.Text)+"'"
        GridView1.DataBind()
    End Sub
    Protected Sub Insert(sender As Object, e As EventArgs)
        Dim employee_ids As String = txtemployee_id.Text
        Dim usernames As String = txtusername.Text
        Dim actions As String = DropDownList1.Text
        Dim strings As String() = txttimeinout.Text.Split("-")
        Dim datetimes As DateTime = New DateTime(strings(0), strings(1), strings(2)) + " " + txtinout.Text
        Dim dates As String = datetimes.Date.ToShortDateString
        Dim times As String = txtinout.Text

        '' Check if Username / ID combination exists in database before adding TimeIn/Out to attendancelogs
        Dim validate As String = "SELECT TOP 1 * FROM employee_tbl WHERE username = '" & usernames & "' AND employee_id = '" & employee_ids & "' ORDER BY employee_id DESC"
        Dim cmd = New SqlCommand(validate, conn)
        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)
        If myTable.Rows.Count = 0 Then
            Response.Write("<script> alert(""Username / ID combination does not exist in the database!"")</script>")
            return
        End If

        Dim addquery As String = " INSERT INTO attendancelogs (employee_id, username, action, datetime, date, time) VALUES (@employee_id, @username, @action, @datetime, @date, @time) " 
        Dim com = New SqlCommand(addquery, conn)

        com.Parameters.AddWithValue("@employee_id", employee_ids)
        com.Parameters.AddWithValue("@username", usernames)
        com.Parameters.AddWithValue("@action", actions)
        com.Parameters.AddWithValue("@datetime", datetimes)
        com.Parameters.AddWithValue("@date", dates)
        com.Parameters.AddWithValue("@time", times)

        Dim x As Integer = 0
        Try
            conn.close()
            conn.open()
            x = com.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            com.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                Response.Write("<script> alert(""Attendance log successfully added to the database!"")</script>")
                Log_Change(Session("user"), "Attendance", Session("user") + " added a "+actions+" for "+usernames+" for "+datetimes+"", "")
            Case 0
                Response.Write("<script> alert(""Issue connecting to server or executing the Create Employee query!"")</script>")
        End Select

        ''InsertCommand="INSERT INTO attendancelogs (employee_id, username, action, datetime, date, time) VALUES (@employee_id, @username, @action, @datetime, @date, @time)"
        txttimeinout.Text = ""
        txtinout.Text = ""
        DropDownList1.Text = ""
        txtemployee_id.Text = ""

        '' Refresh
        GridView1.DataBind()
    End Sub
    Protected Sub Attendance_OnRowUpdating(sender As Object, e As GridViewUpdateEventArgs)
        '' Validate Input
        #Region "NewValues cannot be blank"
        If (e.NewValues("action") = Nothing) Then
            Response.Write("<script> alert(""Action cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("datetime") = Nothing) Then
            Response.Write("<script> alert(""DateTime cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        #End Region

        Try
            DateTime.Parse(e.NewValues("datetime"))
        Catch ex As Exception
            Response.Write("<script> alert(""DateTime is in an incompatable format."")</script>")
            e.Cancel = true
            return
        End Try
        If (e.NewValues("action").ToString <> "TimeIn" And e.NewValues("action").ToString <> "TimeOut") Then
            Response.Write("<script> alert(""Action must be TimeIn or TimeOut."")</script>")
            e.cancel = true
            return
        End If

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("datetime") = Nothing) Then e.OldValues("datetime") = " "
        If (e.OldValues("action") = Nothing) Then e.OldValues("action") = " "

        If (e.NewValues("datetime") = Nothing) Then e.NewValues("datetime") = " "
        If (e.NewValues("action") = Nothing) Then e.NewValues("action") = " "
        #End Region

        '' Add Change To Log
        Dim Changes_Made As string = ""
        if (e.OldValues("datetime") <> e.NewValues("datetime")) Then Changes_Made += Session("user") + " changed TimeIn/Out of "+e.OldValues("username").ToString()+"'s Log #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("datetime").ToString()+"' to '"+e.NewValues("datetime").ToString()+"' <br />"
        If (e.OldValues("action") <> e.NewValues("action")) Then Changes_Made += Session("user") + " changed Action of "+e.OldValues("username").ToString()+"'s Log #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("action").ToString()+"' to '"+e.NewValues("action").ToString()+"' <br />"
        If (Changes_Made <> "") Then Log_Change(Session("user"), "Attandance", Changes_Made, "")
    End Sub
    Protected Function GetColumnIndexByHeaderText(grid As GridView, findHeader As String) As Integer
        Dim i As Integer = 0
        For i = 0 To grid.Columns.Count - 1
            If grid.Columns(i).HeaderText.ToLower().Trim() = findHeader.ToLower().Trim() Then
                Return i
            End If
        Next

        Return -1
    End Function


    Public Sub Log_Change(admin_username As string, table_changed As string, actions_performed As string, query_performed As string)

        ' Get the admin_id of the user
        Dim myReader As SqlDataReader
        Dim admin_id As String = ""
        Dim myCmd As SqlCommand = conn.CreateCommand
        myCmd.CommandText = "SELECT admin_id FROM admin_tbl WHERE username='"+admin_username+"'"
        conn.Close()
        conn.Open()
        myReader = myCmd.ExecuteReader()
        Do While myReader.Read()
            admin_id = myReader.Item(0).ToString
        Loop
        myReader.Close()
        conn.Close()

        Dim addquery As String =  "INSERT INTO change_log_tbl (admin_id, admin_username, datetime, table_changed, actions_performed, query_performed) VALUES (@admin_id, @admin_username, @datetime, @table_changed, @actions_performed, @query_performed)" 
        Dim com = New SqlCommand(addquery, conn)
        com.Parameters.AddWithValue("@admin_id", admin_id)
        com.Parameters.AddWithValue("@admin_username", admin_username)
        com.Parameters.AddWithValue("@datetime", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToString, TimeZoneInfo.Local.Id, "Pacific Standard Time"))
        com.Parameters.AddWithValue("@table_changed", table_changed)
        com.Parameters.AddWithValue("@actions_performed", actions_performed)
        com.Parameters.AddWithValue("@query_performed", query_performed)

        Dim x As Integer = 0
        Try
            conn.close()
            conn.open()
            x = com.ExecuteNonQuery()
        Catch ex As Exception
            'Response.Write("<script> alert("" & ex.Message & "")</script>")
        Finally
            conn.close()
            com.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                'Response.Write("<script> alert(""Attendance log successfully added to the database!"")</script>")
            Case 0
                'Response.Write("<script> alert(""Issue connecting to server or executing the Create Employee query!"")</script>")
        End Select
    End Sub

End Class