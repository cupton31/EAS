Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO

Public Class Dashboard
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        getAllData()
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub
    Sub getAllData()
        Dim usernames As String = Session("user")
        Dim getData As String = "SELECT * FROM employee_tbl WHERE username = @username"
        Dim cmd = New SqlCommand(getData, conn)

        cmd.Parameters.AddWithValue("@username", usernames)

    End Sub
    Protected Sub btnTimeinIN_Click(sender As Object, e As EventArgs) Handles btnTimeinIN.Click
        Dim usernames As String = (Session("user"))
        Dim actions As String = fortimein.Text
        Dim datetimes As DateTime = DateTime.Now
        Dim dates As String = datetimes.Date.ToShortDateString
        Dim times As String = datetimes.TimeOfDay.Hours.ToString + ":" + datetimes.TimeOfDay.Minutes.ToString + ":" + datetimes.TimeOfDay.Seconds.ToString

        '' Set actions base on last action TimeOut or ReTimeOut
        Dim validate As String = "SELECT TOP 1 * FROM attendancelogs WHERE username = '" & usernames & "' ORDER BY log_id DESC"
        Dim cmd = New SqlCommand(validate, conn)
        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)
        If myTable.Rows.Count > 0 Then
            If myTable.Rows.Item(0).ItemArray.Contains(fortimein.Text) Or myTable.Rows.Item(0).ItemArray.Contains("ReTimeIn") Then
                actions = "ReTimeIn"
            End If
        End If

        '' Get Employee ID of current user
        Dim validate1 As String = "SELECT TOP 1 employee_id FROM employee_tbl WHERE username = '" & usernames & "'"
        Dim cmd1 = New SqlCommand(validate1, conn)
        Dim myTable1 As New DataTable
        Dim adapter1 As New SqlDataAdapter(cmd1)
        adapter1.Fill(myTable1)
        Dim employee_ids As String = myTable1.Rows.Item(0).ItemArray(0).ToString

        '' Insert action into attendance log
        Dim addquery As String = "INSERT INTO attendancelogs (employee_id, username, action, datetime, date, time) VALUES (@employee_id, @username, @action, @datetime, @date, @time)"
        Dim com = New SqlCommand(addquery, conn)
        com.Parameters.AddWithValue("@employee_id", employee_ids)
        com.Parameters.AddWithValue("@username", usernames)
        com.Parameters.AddWithValue("@action", actions)
        com.Parameters.AddWithValue("@datetime", datetimes)
        com.Parameters.AddWithValue("@date", dates)
        com.Parameters.AddWithValue("@time", times)

        Dim x As Integer = 0
        Try
            conn.open()
            x = com.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            cmd.Parameters.Clear()
        End Try
        Select Case x
            Case 1
                Response.Write("<script> alert(""TimeIn successful!"")</script>")
            Case 0
                Response.Write("<script> alert(""Issue with database connectivity or TimeIn query. Please try again. If problem persists, please contact IT technician."")</script>")
        End Select
    End Sub

    Protected Sub btnTimeOut_Click(sender As Object, e As EventArgs) Handles btnTimeOut.Click
        Dim usernames As String = (Session("user"))
        Dim actions As String = fortimeout.Text
        Dim datetimes As DateTime = DateTime.Now
        Dim dates As String = datetimes.Date.ToShortDateString
        Dim times As String = datetimes.TimeOfDay.Hours.ToString + ":" + datetimes.TimeOfDay.Minutes.ToString + ":" + datetimes.TimeOfDay.Seconds.ToString

        '' Set actions base on last action TimeOut or ReTimeOut
        Dim validate As String = "SELECT  TOP 1 * FROM attendancelogs WHERE username = '" & usernames & "' ORDER BY log_id DESC"
        Dim cmd = New SqlCommand(validate, conn)

        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            If myTable.Rows.Item(0).ItemArray.Contains(fortimeout.Text) Or myTable.Rows.Item(0).ItemArray.Contains("ReTimeOut") Then
                actions = "ReTimeOut"
            End If
        End If

        '' Get Employee ID of current user
        Dim validate1 As String = "SELECT TOP 1 employee_id FROM employee_tbl WHERE username = '" & usernames & "'"
        Dim cmd1 = New SqlCommand(validate1, conn)
        Dim myTable1 As New DataTable
        Dim adapter1 As New SqlDataAdapter(cmd1)
        adapter1.Fill(myTable1)
        Dim employee_ids As String = myTable1.Rows.Item(0).ItemArray(0).ToString

        '' Insert action into attendance log
        Dim addquery As String = "INSERT INTO attendancelogs (employee_id, username, action, datetime, date, time) VALUES (@employee_id, @username, @action, @datetime, @date, @time)"
        Dim com = New SqlCommand(addquery, conn)
        com.Parameters.AddWithValue("@employee_id", employee_ids)
        com.Parameters.AddWithValue("@username", usernames)
        com.Parameters.AddWithValue("@action", actions)
        com.Parameters.AddWithValue("@datetime", datetimes)
        com.Parameters.AddWithValue("@date", dates)
        com.Parameters.AddWithValue("@time", times)

        Dim x As Integer = 0
            Try
                conn.open()
                x = com.ExecuteNonQuery()
            Catch ex As Exception
                Response.Write("<script> alert(" & ex.Message & ")</script>")
            Finally
                conn.close()
                cmd.Parameters.Clear()
            End Try
        Select Case x
            Case 1
                Response.Write("<script> alert(""TimeOut successful"")</script>")
            Case 0
                Response.Write("<script> alert(""Issue with database connectivity or TimeOut query. Please try again. If problem persists, please contact IT technician."")</script>")
        End Select
    End Sub


End Class

