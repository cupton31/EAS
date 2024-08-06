Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Public Class adminsettings
    Inherits System.Web.UI.Page
    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If
        Dim queryString As string = "SELECT user_level FROM admin_tbl WHERE username = '" + Session("user") + "'"
        Dim command As SqlCommand = new SqlCommand(queryString, conn)
        Dim user_level As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read())
                user_level = reader(0).ToString()
        End While
            reader.Close()
        finally 
            conn.Close()
        End Try


        SqlDataSource2.FilterExpression = "username = '"+Session("user")+"'"
        SqlDataSource1.FilterExpression = ""
        If user_level = "superadmin" Then
            Admin_GridView.Visible = false
            Admin_GridView.Enabled = false
            SuperAdmin_GridView.Visible = true
            SuperAdmin_GridView.Enabled = true
            CreateAdminTable.Visible = true
            CreateAdminTable.Enabled = true
            CreateAdminLabel.Visible = true
            ManageAdminLabel.Text = "Manage Admins"
        End If
        If user_level = "admin" Then
            Admin_GridView.Visible = true
            Admin_GridView.Enabled = true
            SuperAdmin_GridView.Visible = false
            SuperAdmin_GridView.Enabled = false
            CreateAdminTable.Visible = false
            CreateAdminTable.Enabled = false
            CreateAdminLabel.Visible = false
            ManageAdminLabel.Text = "My Admin Account"
        End If


        If (Not IsPostBack) Then
            UserNameTB.Text = "Admin"
            PassWordTB.Text = GeneratePassword()
        End If
        ChangeLogs_GridView.Columns(GetColumnIndexByHeaderText(ChangeLogs_GridView, "Action Performed")).ItemStyle.Wrap = true

        SqlDataSource3.FilterExpression = ""

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        'Log_Change(Session("user"), "Log Out", "Logged Out", "")

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
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
    Protected Function GeneratePassword() As string
        Dim requireNonLetterOrDigit As boolean = true
        Dim requireDigit As boolean = true
        Dim requireLowercase As boolean = true
        Dim requireUppercase As boolean = true

        Dim randomPassword As string = ""

        Dim passwordLength As Int32 = 17

        Dim rando As Random = new Random()
        While (randomPassword.Length <> passwordLength)
            Dim randomNumber as Int32 = rando.Next(48, 122)
            If (randomNumber=91 Or randomNumber=92 Or randomNumber=93 Or randomNumber=94 Or randomNumber=95 Or randomNumber=96 Or randomNumber=60 Or randomNumber=62) Then
                Continue While
            End If

            Dim c As char = Convert.ToChar(randomNumber)

            if (requireDigit) Then
                if (char.IsDigit(c)) Then
                    requireDigit = false
                End if
            End if
            if (requireLowercase) Then
                if (char.IsLower(c)) Then
                    requireLowercase = false
                End if
            End if

            if (requireUppercase) Then
                if (char.IsUpper(c)) Then
                    requireUppercase = false
                    End if
                End if
            if (requireNonLetterOrDigit) Then
                if (Not char.IsLetterOrDigit(c)) Then
                    requireNonLetterOrDigit = false
                End if
            End if

            randomPassword += c
        End While

        if (requireDigit) Then randomPassword += Convert.ToChar(rando.Next(48, 58))
        if (requireLowercase) Then randomPassword += Convert.ToChar(rando.Next(97, 123))
        if (requireUppercase) Then randomPassword += Convert.ToChar(rando.Next(65, 91))
        if (requireNonLetterOrDigit) Then randomPassword += Convert.ToChar(rando.Next(33, 46))
        return randomPassword

    End Function
    Protected Sub InsertAdmin_Click(sender As object, e As EventArgs)

        '' Check if username already exists
        #Region "Check if admint_tbl.username already exists"
        Dim validate As String = "SELECT username FROM admin_tbl WHERE username ='"+UserNameTB.Text+"'"
        Dim cmd = New SqlCommand(validate, conn)
        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""Username already exists in Admin Table"")</script>")
            Return
        End If
        #End Region
        #Region "Check if employee_tbl.username already exists"
        Dim validate1 As String = "SELECT username FROM employee_tbl WHERE username='"+UserNameTB.Text+"'"
        Dim cmd1 = New SqlCommand(validate1, conn)
        Dim myTable1 As New DataTable
        Dim adapter1 As New SqlDataAdapter(cmd1)
        adapter1.Fill(myTable1)

        If myTable1.Rows.Count > 0 Then
            Response.Write("<script> alert(""Username already exists in Employee Table"")</script>")
            Return
        End If

        #End Region

        If (PhoneTB.Text.Length < 1) Then PhoneTB.Text = "### ### ####"
        If (EmailTB.Text.Length < 1) Then PhoneTB.Text = "email@site.com"
        If (FirstNameTB.Text.Length < 1) Then FirstNameTB.Text = "FirstName"
        If (LastNameTB.Text.Length < 1) Then LastNameTB.Text = "LastName"

        Dim usernames As String = usernameTB.Text
        Dim passwords As String = passwordTB.Text
        Dim userlevels As String = UserLevelDDL.SelectedValue
        Dim firstnames As String = firstnameTB.Text
        Dim lastnames As String = lastnameTB.Text
        Dim phones As String = phoneTB.Text
        Dim emails As String = emailTB.Text
        Dim addquery As String = "INSERT INTO admin_tbl ( username, password, user_level, firstname, lastname, phone, email ) VALUES ( @username, @password, @user_level, @firstname, @lastname, @phone, @email )"
        Dim com = New SqlCommand(addquery, conn)
        com.Parameters.AddWithValue("@username", usernames)
        com.Parameters.AddWithValue("@password", passwords)
        com.Parameters.AddWithValue("@user_level", userlevels)
        com.Parameters.AddWithValue("@firstname", firstnames)
        com.Parameters.AddWithValue("@lastname", lastnames)
        com.Parameters.AddWithValue("@phone", phones)
        com.Parameters.AddWithValue("@email", emails)

        Dim x As Integer = 0
        Try
            conn.close()
            conn.open()
            x = com.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert("" & ex.Message & "")</script>")
        Finally
            conn.close()
            com.Parameters.Clear()
        End Try

        Log_Change(Session("user"), "Admin", Session("user") + " created a new admin: username="+usernames+", userlevel="+userlevels+", firstname="+firstnames+", lastname="+lastnames+", phone="+phones+", email="+emails+".", "")

        SuperAdmin_GridView.DataBind()

        UserNameTB.Text = "Admin"
        PassWordTB.Text = GeneratePassword()
        FirstNameTB.Text = ""
        LastNameTB.Text = ""
        PhoneTB.Text = ""
        EmailTB.Text = ""

    End Sub
    Protected Sub SuperAdmin_GridView_RowDeleting(sender As object, e As GridViewDeleteEventArgs)

        'MsgBox does not work with server... To implement Yes/No Dialog, use a modal?
        'Dim confirmed As Integer = MsgBox(Are you sure that you want to delete this admin account?, MsgBoxStyle.YesNo + MsgBoxStyle.MsgBoxSetForeground, Confirm Delete)
        'If Not confirmed = MsgBoxResult.Yes Then
        'e.Cancel = True 
        'End If

        Dim Changes_Made As string = Session("user") + " deleted row with Admin_Id="+e.Keys.Values(0).ToString()+" which had values: firstname="+e.Values("firstname")+", lastname="+e.Values("lastname")+", phone="+e.Values("phone")+", email="+e.Values("email")+", user_level="+e.Values("user_level")+", username="+e.Values("username")+"."
        Log_Change(Session("user"), "Admin", Changes_Made, "Delete")

    End Sub
    Protected Sub Admin_GridView_RowUpdating(sender As object, e As GridViewUpdateEventArgs)
        '' Input Validation
        #Region "NewValues cannot be blank"
        If (e.NewValues("firstname") = Nothing) Then
            Response.Write("<script> alert(""First Name cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("lastname") = Nothing) Then
            Response.Write("<script> alert(""Last Name cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("phone") = Nothing) Then
            Response.Write("<script> alert(""Phone cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("email") = Nothing) Then
            Response.Write("<script> alert(""Email cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("password") = Nothing) Then
            Response.Write("<script> alert(""Password cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        #End Region

        Dim password as String = e.NewValues("password")
        If (Not Regex.Match(password , "^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$").Success Or password.Length <8) Then
            Response.Write("<script> alert(""Password must have 1 lower, 1 upper, 1 number, 1 special, 8+ chars"")</script>")
            e.Cancel = true
            return
        End If

        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("SELECT username FROM admin_tbl WHERE admin_id='"+e.Keys.Values(0).ToString()+"'", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()
        Dim username As string = dt1.Rows(0).ItemArray(0).ToString()

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("firstname") = Nothing) Then e.OldValues("firstname") = " "
        If (e.OldValues("lastname") = Nothing) Then e.OldValues("lastname") = " "
        If (e.OldValues("phone") = Nothing) Then e.OldValues("phone") = " "
        If (e.OldValues("email") = Nothing) Then e.OldValues("email") = " "
        If (e.OldValues("password") = Nothing) Then e.OldValues("password") = " "
        If (e.NewValues("firstname") = Nothing) Then e.NewValues("firstname") = " "
        If (e.NewValues("lastname") = Nothing) Then e.NewValues("lastname") = " "
        If (e.NewValues("phone") = Nothing) Then e.NewValues("phone") = " "
        If (e.NewValues("email") = Nothing) Then e.NewValues("email") = " "
        If (e.NewValues("password") = Nothing) Then e.NewValues("password") = " "
        #End Region

        '' Add Change To Log
        Dim Changes_Made As string = ""
        if (e.OldValues("firstname") <> e.NewValues("firstname")) Then Changes_Made += Session("user") + " changed First Name of +username+ from '"+e.OldValues("firstname").ToString()+"' to '"+e.NewValues("firstname").ToString()+"' <br />"
        if (e.OldValues("lastname") <> e.NewValues("lastname")) Then Changes_Made += Session("user") + " changed Last Name of +username+ from '"+e.OldValues("lastname").ToString()+"' to '"+e.NewValues("lastname").ToString()+"' <br />"
        if (e.OldValues("phone") <> e.NewValues("phone")) Then Changes_Made += Session("user") + " changed Phone of +username+ from '"+e.OldValues("phone").ToString()+"' to '"+e.NewValues("phone").ToString()+"' <br />"
        if (e.OldValues("email") <> e.NewValues("email")) Then Changes_Made += Session("user") + " changed Email of +username+ from '"+e.OldValues("email").ToString()+"' to '"+e.NewValues("email").ToString()+"' <br />"
        if (e.OldValues("password") <> e.NewValues("password")) Then Changes_Made += Session("user") + " changed Password of "+username+" <br />"
        If (Changes_Made <> "") Then Log_Change(Session("user"), "Admin", Changes_Made, "Update")

    End Sub
    Protected Sub SuperAdmin_GridView_RowUpdating(sender As object, e As GridViewUpdateEventArgs)
        '' Input Validation
        #Region "NewValues cannot be blank"
        If (e.NewValues("firstname") = Nothing) Then
            Response.Write("<script> alert(""First Name cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("lastname") = Nothing) Then
            Response.Write("<script> alert(""Last Name cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("phone") = Nothing) Then
            Response.Write("<script> alert(""Phone cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("email") = Nothing) Then
            Response.Write("<script> alert(""Email cannot be blank."")</script>")
            e.cancel = true
            return
            End If
        If (e.NewValues("user_level") = Nothing) Then
            Response.Write("<script> alert(""User Level cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("password") = Nothing) Then
            Response.Write("<script> alert(""Password cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        #End Region

        Dim password as String = e.NewValues("password")
        If (Not Regex.Match(password , "^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$").Success Or password.Length <8) Then
            Response.Write("<script> alert(""Password must have 1 lower, 1 upper, 1 number, 1 special, 8+ chars"")</script>")
            e.Cancel = true
            return
        End If

        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("SELECT username FROM admin_tbl WHERE admin_id='"+e.Keys.Values(0).ToString()+"'", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()
        Dim username As string = dt1.Rows(0).ItemArray(0).ToString()

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("firstname") = Nothing) Then e.OldValues("firstname") = " "
        If (e.OldValues("lastname") = Nothing) Then e.OldValues("lastname") = " "
        If (e.OldValues("phone") = Nothing) Then e.OldValues("phone") = " "
        If (e.OldValues("email") = Nothing) Then e.OldValues("email") = " "
        If (e.NewValues("user_level") = Nothing) Then e.NewValues("user_level") = " "
        If (e.OldValues("password") = Nothing) Then e.OldValues("password") = " "

        If (e.NewValues("firstname") = Nothing) Then e.NewValues("firstname") = " "
        If (e.NewValues("lastname") = Nothing) Then e.NewValues("lastname") = " "
        If (e.NewValues("phone") = Nothing) Then e.NewValues("phone") = " "
        If (e.NewValues("email") = Nothing) Then e.NewValues("email") = " "
        If (e.NewValues("user_level") = Nothing) Then e.NewValues("user_level") = " "
        If (e.NewValues("password") = Nothing) Then e.NewValues("password") = " "
        #End Region

        '' Add Change To Log
        Dim Changes_Made As string = ""
        if (e.OldValues("firstname") <> e.NewValues("firstname")) Then Changes_Made += Session("user") + " changed First Name of "+username+" from '"+e.OldValues("firstname").ToString()+"' to '"+e.NewValues("firstname").ToString()+"' <br />"
        if (e.OldValues("lastname") <> e.NewValues("lastname")) Then Changes_Made += Session("user") + " changed Last Name of "+username+" from '"+e.OldValues("lastname").ToString()+"' to '"+e.NewValues("lastname").ToString()+"' <br />"
        if (e.OldValues("phone") <> e.NewValues("phone")) Then Changes_Made += Session("user") + " changed Phone of "+username+" from '"+e.OldValues("phone").ToString()+"' to '"+e.NewValues("phone").ToString()+"' <br />"
        if (e.OldValues("email") <> e.NewValues("email")) Then Changes_Made += Session("user") + " changed Email of "+username+" from '"+e.OldValues("email").ToString()+"' to '"+e.NewValues("email").ToString()+"' <br />"
        if (e.OldValues("user_level") <> e.NewValues("user_level")) Then Changes_Made += Session("user") + " changed User Level of "+username+" from '"+e.OldValues("user_level").ToString()+"' to '"+e.NewValues("user_level").ToString()+"' <br />"
        if (e.OldValues("password") <> e.NewValues("password")) Then Changes_Made += Session("user") + " changed Password of Admin "+username+" <br />"
        If (Changes_Made <> "") Then Log_Change(Session("user"), "Admin", Changes_Made, "Update")

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