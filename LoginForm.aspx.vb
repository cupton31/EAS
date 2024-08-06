Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Drawing
Imports System.IO
Imports System.Net.Mail
Imports System.Security.Policy
Imports System.Threading.Tasks
Imports Microsoft.IdentityFramework
Imports Microsoft.AspNet.Identity
Imports System.Net.Mime
Imports System.Windows.Forms.LinkLabel

Public Class LoginForm
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim username As String = logusername.Text
        Dim password As String = logpassword.Text

        Dim userquery As String = "SELECT username, password, user_level FROM employee_tbl WHERE username=@username AND password=@password UNION SELECT username, password, user_level FROM admin_tbl WHERE username=@username AND password=@password"
        Dim cmd = New SqlCommand(userquery, conn)

        cmd.Parameters.AddWithValue("@username", username)
        cmd.Parameters.AddWithValue("@password", password)

        Dim adapter As New SqlDataAdapter(cmd)
        Dim myTable As New DataTable

        conn.close()
        conn.open()
        adapter.Fill(myTable)
        Session("user") = username
        If myTable.Rows.Count > 0 Then

            Log_Change(Session("user"), "Log In", Session("user")+ " logged in", "")

            If myTable.Rows(0)("user_level").ToString.Contains("admin") Then
                Response.Redirect("~/admin/admindashboard.aspx")
            ElseIf myTable.Rows(0)("user_level") = "Employee" Then
                Response.Redirect("~/employee/Dashboard.aspx")
            End If
        Else
            Response.Write("<script> alert(""incorrect username or password!"")</script>")
        End If

    End Sub

    Protected Sub ForgotPassword_Click(sender As Object, e As EventArgs)
        Response.Redirect("~/ForgotPassword.aspx")
    End Sub


    Protected Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        Dim user As String = username.Text
        Dim pass As String = password.Text
        Dim user_levels As String = "Employee"
        Dim firstnames As String = fname.Text
        ''Dim middlenames As String = "xxx"
        Dim lastnames As String = lname.Text
        ''Dim guardcardnumbers As String = "xxx"
        ''Dim ssns As String = "xxx"
        ''Dim driverlicensenumbers As String = "xxx"
        ''Dim driverlicensestates As String = "xxx"
        Dim phones As String = phone.Text
        ''Dim cellphones As String = "xxx"
        ''Dim companys As String = Excalibur
        ''Dim divisions As String = California
        ''Dim groupss As String = "xxx"
        Dim positions As String = position.Text
        Dim datehireds As String = DateHired.Text
        ''Dim datedismisseds As String = "xxx"
        Dim emails As String = email.Text
        Dim streetaddresss As String = address.Text
        ''Dim streetaddress2s As String = "xxx"
        ''Dim citys As String = "xxx"
        ''Dim zips As String = "xxx"
        ''Dim states As String = "xxx"
        ''Dim records As String = "xxx"
        ''Dim pictures As Bitmap = Bitmap.FromFile(C:\Users\Owner\Desktop\Excalibur\OpenSource Free Projects\EAS-main\EAS-main\images\employee.png)
        ''Dim pictures As String = "xxx"
        ''Dim genders As String = "xxx"
        ''Dim ethnicitys As String = "xxx"
        ''Dim citizenstatuss As String = "xxx"
        ''Dim veteranstatuss As String = "xxx"
        ''Dim disabilitystatuss As String = "xxx"

        Dim validate As String = "SELECT username FROM employee_tbl WHERE username ='" & user & "'"
        Dim cmd = New SqlCommand(validate, conn)

        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""Username is already existed"")</script>")
        Else
            Dim addquery As String = "INSERT INTO employee_tbl (username, password, user_level, firstname, lastname, phone, position, datehired, email, streetaddress) VALUES (@username, @password, @user_level, @firstname, @lastname, @phone, @position, @datehired, @email, @streetaddress)"
            Dim com = New SqlCommand(addquery, conn)

            com.Parameters.AddWithValue("@username", user)
            com.Parameters.AddWithValue("@password", pass)
            com.Parameters.AddWithValue("@user_level", user_levels)
            com.Parameters.AddWithValue("@firstname", firstnames)
            'com.Parameters.AddWithValue("@middlename", middlenames)
            com.Parameters.AddWithValue("@lastname", lastnames)
            'com.Parameters.AddWithValue("@guardcardnumber", guardcardnumbers)
            'com.Parameters.AddWithValue("@ssn", ssns)
            'com.Parameters.AddWithValue("@driverlicensenumber", driverlicensenumbers)
            'com.Parameters.AddWithValue("@driverlicensestate", driverlicensestates)
            com.Parameters.AddWithValue("@phone", phones)
            'com.Parameters.AddWithValue("@cellphone", cellphones)
            'com.Parameters.AddWithValue("@company", companys)
            'com.Parameters.AddWithValue("@division", divisions)
            'com.Parameters.AddWithValue("@groups", groupss)
            com.Parameters.AddWithValue("@position", positions)
            com.Parameters.AddWithValue("@datehired", datehireds)
            'com.Parameters.AddWithValue("@datedismissed", datedismisseds)
            com.Parameters.AddWithValue("@email", emails)
            com.Parameters.AddWithValue("@streetaddress", streetaddresss)
            'com.Parameters.AddWithValue("@streetaddress2", streetaddress2s)
            'com.Parameters.AddWithValue("@city", citys)
            'com.Parameters.AddWithValue("@zip", zips)
            'com.Parameters.AddWithValue("@state", states)
            'com.Parameters.AddWithValue("@record", records)
            'com.Parameters.AddWithValue("@picture", pictures)
            'com.Parameters.AddWithValue("@gender", genders)
            'com.Parameters.AddWithValue("@ethnicity", ethnicitys)
            'com.Parameters.AddWithValue("@citizenstatus", citizenstatuss)
            'com.Parameters.AddWithValue("@veretanstatus", veteranstatuss)
            'com.Parameters.AddWithValue("@disabilitystatus", disabilitystatuss)


            Dim x As Integer = 0
            Try
                conn.open()
                x = com.ExecuteNonQuery()
            Catch ex As Exception
                Response.Write("<script> alert("" & ex.Message & "")</script>")
            Finally
                conn.close()
                cmd.Parameters.Clear()
            End Try

            Select Case x
                Case 1
                    Response.Write("<script> alert(""Registered successfully"")</script>")
                Case 0
                    Response.Write("<script> alert(""Issue connecting to server or executing the Create Account query!"")</script>")
            End Select
        End If
    End Sub

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

        Dim addquery As String =  "INSERT INTO change_log_tbl (admin_id, admin_username, datetime, table_changed, actions_performed, query_performed) VALUES (@admin_id, @admin_username, @datetime, @table_changed, @actions_performed, @query_performed) "
        Dim com = New SqlCommand(addquery, conn)
        com.Parameters.AddWithValue("@admin_id", admin_id)
        com.Parameters.AddWithValue("@admin_username", admin_username)
        com.Parameters.AddWithValue("@datetime", DateTime.Now.ToString)
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