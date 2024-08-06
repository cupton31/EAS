Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO

Public Class yourprofile
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        getAllData()
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If
        If Session("user") = Nothing Then
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
        If usernames = Nothing Then
            Response.Redirect("~/LoginForm.aspx")
        End If
        Dim getData As String = "SELECT * FROM employee_tbl WHERE username = @username"
        Dim cmd = New SqlCommand(getData, conn)

        cmd.Parameters.AddWithValue("@username", usernames)

        conn.open()

        GridView1.DataSource = cmd.ExecuteReader()
        GridView1.DataBind()
        conn.close()
    End Sub

    Sub dataShow(x As Object, e As EventArgs)
        Dim id As Integer = Convert.ToInt32(CType(x, LinkButton).CommandArgument())

        Dim getquery As String = "SELECT * FROM employee_tbl WHERE employee_id =@id"
        Dim cmd = New SqlCommand(getquery, conn)

        cmd.Parameters.AddWithValue("@id", id)


        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)

        Try
            conn.open()
            adapter.Fill(myTable)
            nickname.Text = myTable.Rows(0)("nickname").ToString()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            cmd.Parameters.Clear()
        End Try
    End Sub

    Protected Sub btnAddVehicle_Click(sender As Object, e As EventArgs) Handles btnAddVehicle.Click
        Dim nicknames As String = nickname.Text
        Dim usernames As String = Session("user")

        Dim ad As String = "UPDATE employee_tbl SET nickname = @nickname WHERE username = @id"

        Dim cmd = New SqlCommand(ad, conn)
        cmd.Parameters.AddWithValue("@nickname", nicknames)
        cmd.Parameters.AddWithValue("@id", usernames)

        Dim x As Integer = 0
        Try
            conn.open()
            x = cmd.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            cmd.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                getAllData()
                nickname.Text = ""
                Response.Write("<script> alert(""Information has been updated successfully"")</script>")
            Case 0
                Response.Write("<script> alert(""Opps!! hinay hinay lang!!"")</script>")
        End Select
    End Sub

    Protected Sub favoriteshift_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim usernames As String = Session("user")
        Dim favoriteshifts As String = favoriteshift.Text

        Dim ad As String = "UPDATE employee_tbl SET favoriteshift = @favoriteshift WHERE username = @id"

        Dim cmd = New SqlCommand(ad, conn)
        cmd.Parameters.AddWithValue("@id", usernames)
        cmd.Parameters.AddWithValue("@favoriteshift", favoriteshifts)

        Dim x As Integer = 0
        Try
            conn.open()
            x = cmd.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            cmd.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                getAllData()
                Response.Write("<script> alert(""Information has been updated successfully"")</script>")
            Case 0
                Response.Write("<script> alert(""Opps!! hinay hinay lang!!"")</script>")
        End Select
    End Sub

    Protected Sub secondfavoriteshift_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim usernames As String = Session("user")
        Dim secondfavoriteshifts As String = secondfavoriteshift.Text

        Dim ad As String = "UPDATE employee_tbl SET secondfavoriteshift = @secondfavoriteshift WHERE username = @id"

        Dim cmd = New SqlCommand(ad, conn)
        cmd.Parameters.AddWithValue("@id", usernames)
        cmd.Parameters.AddWithValue("@secondfavoriteshift", secondfavoriteshifts)

        Dim x As Integer = 0
        Try
            conn.open()
            x = cmd.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            cmd.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                getAllData()
                Response.Write("<script> alert(""Information has been updated successfully"")</script>")
            Case 0
                Response.Write("<script> alert(""Opps!! hinay hinay lang!!"")</script>")
        End Select
    End Sub

    Protected Sub thirdfavoriteshift_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim usernames As String = Session("user")
        Dim thirdfavoriteshifts As String = thirdfavoriteshift.Text

        Dim ad As String = "UPDATE employee_tbl SET thirdfavoriteshift = @thirdfavoriteshift WHERE username = @id"

        Dim cmd = New SqlCommand(ad, conn)
        cmd.Parameters.AddWithValue("@id", usernames)
        cmd.Parameters.AddWithValue("@thirdfavoriteshift", thirdfavoriteshifts)

        Dim x As Integer = 0
        Try
            conn.open()
            x = cmd.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            cmd.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                getAllData()
                Response.Write("<script> alert(""Information has been updated successfully"")</script>")
            Case 0
                Response.Write("<script> alert(""Opps!! hinay hinay lang!!"")</script>")
        End Select
    End Sub
End Class