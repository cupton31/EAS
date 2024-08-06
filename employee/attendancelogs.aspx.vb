Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO

Public Class attendancelogs
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
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

    Protected Sub makeLogChange(sender As Object, e As EventArgs)
        Dim usernames As String = Session("user")
        Dim log_ids As String = txtlog_id.Text
        Dim editedactions As String = DropDownList1.Text
        Dim editeddates As String = txtDate.Text
        Dim editedtimes As String = txtTime.Text
        Dim editedreasons As String = txtreason.Text

        '' Check if log_id / Username exists in database before adding TimeIn/Out to attendancelogs
        Dim validate As String = "SELECT TOP 1 * FROM attendancelogs WHERE username = '" & usernames & "' AND log_id = '" & log_ids & "' ORDER BY log_id DESC"
        Dim cmd = New SqlCommand(validate, conn)
        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)
        If myTable.Rows.Count = 0 Then
            Response.Write("<script> alert(""Username / Log ID combination does not exist in the database!"")</script>")
        End If

        Dim addquery As String = " UPDATE attendancelogs SET editedaction = @editedaction, editedtime = @editedtime , editeddate = @editeddate, editedreason = @editedreason WHERE log_id = @log_id "
        Dim com = New SqlCommand(addquery, conn)

        com.Parameters.AddWithValue("@editedaction", editedactions)
        com.Parameters.AddWithValue("@editedtime", editedtimes)
        com.Parameters.AddWithValue("@editeddate", editeddates)
        com.Parameters.AddWithValue("@editedreason", editedreasons)
        com.Parameters.AddWithValue("@username", usernames)
        com.Parameters.AddWithValue("@log_id", log_ids)

        Dim x As Integer = 0
        Try
            conn.close()
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
                Response.Write("<script> alert(""Change request successfully added to the database!"")</script>")
            Case 0
                Response.Write("<script> alert(""Issue connecting to server or executing the Create Employee query!"")</script>")
        End Select

        '' Clear textboxes
        txtlog_id.Text = ""
        DropDownList1.Text = ""
        txtDate.Text = ""
        txtTime.Text = ""
        txtreason.Text = ""

        '' Reload the page because of GridView needs to update to show new attendance log entry
        ''Response.Redirect(Request.RawUrl)
        GridView1.DataBind()

    End Sub

    'Protected Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
    '    Dim usernum As String = Session("user")
    '    Dim fromdate As String = tbDateFrom.Text
    '    Dim enddates As String = tbDateTo.Text
    '    'Dim searchquery As String = "SELECT * FROM attendancelogs WHERE timeinout LIKE '%" & fromdate & "%' AND timeinout LIKE '%" & enddate & "%' AND user_id LIKE '%" & usernum & "%'"
    '    'Dim searchquery As String = "SELECT user_id, status, timeinout, inout FROM attendancelogs WHERE timeinout = '" & fromdate & "' AND timeinout ='" & enddates & "' AND user_id = '" & usernum & "'"
    '    Dim searchquery As String = "SELECT * FROM attendancelogs WHERE timeinout >= '" & fromdate & "' AND timeinout <'" & enddates & "' AND user_id ='" & usernum & "'"

    '    Dim cmd = New SqlCommand(searchquery, conn)

    '    'conn.open()
    '    'Dim ds As New DataSet
    '    'Dim adapter As New SqlDataAdapter(cmd)
    '    'adapter.Fill(ds)
    '    'GridView1.DataSource = ds
    '    'GridView1.DataBind()
    '    'conn.close()

    '    'conn.open()
    '    'GridView1.DataSource = cmd.ExecuteReader()
    '    'GridView1.DataBind()
    '    'conn.close()
    'End Sub
End Class