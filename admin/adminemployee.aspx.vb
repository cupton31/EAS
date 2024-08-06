Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO
Imports System.Drawing
Imports System.ComponentModel.DataAnnotations
Imports System.Security.Cryptography
Public Class adminemployee
    Inherits System.Web.UI.Page
    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If

        If Not IsPostBack Then

        End If

        passwordTB.Text = GeneratePassword()
        retypepasswordTB.Text = passwordTB.Text

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        'Log_Change(Session(user), Log Out, Logged Out, )

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub

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
    Protected Sub updateUserName(sender As Object, e As EventArgs)
        Dim _firstname As String = Regex.Replace(firstnameTB.Text, "[^0-9a-zA-Z]+", "")
        Dim _lastname As String = Regex.Replace(lastnameTB.Text, "[^0-9a-zA-Z]+", "")
        usernameTB.Text = _firstname + _lastname

    End Sub
    Protected Sub createEmployee(sender As Object, e As EventArgs)
        '' Drop Down Lists break nav bar

        Dim user_levels As String = "Employee"
        Dim usernames As String = usernameTB.Text
        Dim passwords As String = passwordTB.Text
        Dim statuss As String = "Active"
        Dim firstnames As String = firstnameTB.Text
        Dim middlenames As String = middlenameTB.Text
        Dim lastnames As String = lastnameTB.Text
        Dim cellphones As String = cellphoneTB.Text
        Dim emails As String = emailTB.Text
        Dim streetaddresss As String = ""
        Dim streetaddress2s As String = ""
        Dim citys As String = ""
        Dim states As String = ""
        Dim zips As String = ""
        Dim guardcardnumbers As String = ""
        Dim firearmpermitnumbers As String = ""
        Dim ssns As String = ""
        Dim driverlicensenumbers As String = ""
        Dim driverlicensestates As String = ""
        Dim bankaccountnumbers As String = ""
        Dim bankroutingnumbers As String = ""
        Dim birthdates As String = ""
        Dim companys As String = companyTB.Text
        Dim ms_ids As String = ms_idTB.Text
        Dim excalibur_ids As String = excalibur_idTB.Text
        Dim positions As String = positionTB.Text
        Dim basepayrates As String = basepayrateTB.Text
        Dim datehireds As String = ""
        Dim genders As String = ""
        Dim ethnicitys As String = ""
        Dim citizenstatuss As String = ""
        Dim veteranstatuss As String = ""
        Dim disabilitystatuss As String = ""

        #Region "Input Error Checking "
        If ms_idTB.Text.Length = 0 And excalibur_idTB.Text.Length = 0 Then
            Response.Write("<script> alert(""Both Excalibur ID and MS ID are empty! "")</script>")
            Return
        End If
        If ms_idTB.Text.Length > 0 Then
            If Not Integer.TryParse(ms_idTB.Text, 0) Then
                Response.Write("<script> alert(""MS ID must be an Integer! "")</script>")
                Return
            End If
        End If
        If excalibur_idTB.Text.Length > 0 Then
            If Not Integer.TryParse(excalibur_idTB.Text, 0) Then
                Response.Write("<script> alert(""Excalibur ID must be an Integer! "")</script>")
                Return
            End If
        End If
        If companys <> "Excalibur" And companys <> "Matt's Staffing" Then
            Response.Write("<script> alert(""Company must be Excalibur or Matt's Staffing! "")</script>")
            Return
        End If
        #End Region
        #Region "Check if admint_tbl.username already exists"
        Dim validate As String = "SELECT username FROM admin_tbl WHERE username ='" & usernames & "'"
        Dim cmd = New SqlCommand(validate, conn)

        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""Admin username already exists"")</script>")
            Return
        End If
        #End Region
        #Region "Check if employee_tbl.username already exists"
        validate = "SELECT username FROM employee_tbl WHERE username ='" & usernames & "'"
        cmd = New SqlCommand(validate, conn)
        myTable = New DataTable
        adapter = New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""Employee username already exists"")</script>")
            Return
        End If
        #End Region
        #Region "Check if ms_id already exists"
        If ms_ids.Length > 0 Then
            validate = "SELECT ms_id FROM employee_tbl WHERE ms_id='" & ms_ids & "'"
            cmd = New SqlCommand(validate, conn)
            myTable.Clear()
            adapter = New SqlDataAdapter(cmd)
            adapter.Fill(myTable)
            If myTable.Rows.Count > 0 Then
                Response.Write("<script> alert(""ms_id already exists in database"")</script>")
                Return
            End If
        End If
        #End Region
        #Region "Check if excalibur_id already exists"
        If excalibur_ids.Length > 0 Then
            validate = "SELECT excalibur_id FROM employee_tbl WHERE _id='" & excalibur_ids & "'"
            cmd = New SqlCommand(validate, conn)
            myTable.Clear()
            adapter = New SqlDataAdapter(cmd)
            adapter.Fill(myTable)
            If myTable.Rows.Count > 0 Then
                Response.Write("<script> alert(""excalibur_id already exists in database"")</script>")
                Return
            End If
        End If
        #End Region

        Dim addquery As String = "INSERT INTO employee_tbl ( username, password, status, user_level, firstname, middlename, lastname, guardcardnumber, firearmpermitnumber, ssn, driverlicensenumber, driverlicensestate, bankaccountnumber, bankroutingnumber, cellphone, company, ms_id, excalibur_id, position, basepayrate, birthdate, datehired, email, streetaddress, streetaddress2, city, zip, state, gender, ethnicity, citizenstatus, veteranstatus, disabilitystatus ) VALUES ( @username, @password, @status, @user_level, @firstname, @middlename, @lastname, @guardcardnumber, @firearmpermitnumber, @ssn, @driverlicensenumber, @driverlicensestate, @bankaccountnumber, @bankroutingnumber, @cellphone, @company, @ms_id, @excalibur_id, @position, @basepayrate, @birthdate, @datehired, @email, @streetaddress, @streetaddress2, @city, @zip, @state, @gender, @ethnicity, @citizenstatus, @veteranstatus, @disabilitystatus )"
        Dim com = New SqlCommand(addquery, conn)

        com.Parameters.AddWithValue("@username", usernames)
        com.Parameters.AddWithValue("@password", passwords)
        com.Parameters.AddWithValue("@status", statuss)
        com.Parameters.AddWithValue("@user_level", user_levels)
        com.Parameters.AddWithValue("@firstname", firstnames)
        com.Parameters.AddWithValue("@middlename", middlenames)
        com.Parameters.AddWithValue("@lastname", lastnames)
        com.Parameters.AddWithValue("@guardcardnumber", guardcardnumbers)
        com.Parameters.AddWithValue("@firearmpermitnumber", firearmpermitnumbers)
        com.Parameters.AddWithValue("@ssn", ssns)
        com.Parameters.AddWithValue("@driverlicensenumber", driverlicensenumbers)
        com.Parameters.AddWithValue("@driverlicensestate", driverlicensestates)
        com.Parameters.AddWithValue("@bankaccountnumber", bankaccountnumbers)
        com.Parameters.AddWithValue("@bankroutingnumber", bankroutingnumbers)
        com.Parameters.AddWithValue("@cellphone", cellphones)
        com.Parameters.AddWithValue("@company", companys)
        com.Parameters.AddWithValue("@ms_id", ms_ids)
        com.Parameters.AddWithValue("@excalibur_id", excalibur_ids)
        com.Parameters.AddWithValue("@position", positions)
        com.Parameters.AddWithValue("@basepayrate", basepayrates)
        com.Parameters.AddWithValue("@birthdate", birthdates)
        com.Parameters.AddWithValue("@datehired", datehireds)
        com.Parameters.AddWithValue("@email", emails)
        com.Parameters.AddWithValue("@streetaddress", streetaddresss)
        com.Parameters.AddWithValue("@streetaddress2", streetaddress2s)
        com.Parameters.AddWithValue("@city", citys)
        com.Parameters.AddWithValue("@zip", zips)
        com.Parameters.AddWithValue("@state", states)
        com.Parameters.AddWithValue("@gender", genders)
        com.Parameters.AddWithValue("@ethnicity", ethnicitys)
        com.Parameters.AddWithValue("@citizenstatus", citizenstatuss)
        com.Parameters.AddWithValue("@veteranstatus", veteranstatuss)
        com.Parameters.AddWithValue("@disabilitystatus", disabilitystatuss)

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
                usernameTB.Text = ""
                passwordTB.Text = GeneratePassword()
                retypepasswordTB.Text = passwordTB.Text
                firstnameTB.Text = ""
                middlenameTB.Text = ""
                lastnameTB.Text = ""
                cellphoneTB.Text = ""
                emailTB.Text = ""
                companyTB.Text = ""
                positionTB.Text = ""
                ms_idTB.Text = ""
                excalibur_idTB.Text = ""
                Log_Change(Session("user"), "Employee", Session("user") + " created a New Employee: UserName='"+usernames+"'", "")
                Response.Write("<script> alert(""New Employee Created successfully"")</script>")
            Case 0
                Response.Write("<script> alert(""Issue connecting to server or executing the Create Employee query!"")</script>")
        End Select
    End Sub
    Protected Sub GridView1_RowUpdating(sender As Object, e As GridViewUpdateEventArgs)

        #Region "NewValues cannot be blank"
        If (e.NewValues("company") = Nothing) Then
            Response.Write("<script> alert(""Company cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("ms_id") = Nothing) Then
            e.NewValues("ms_id") = 0
            'Response.Write("<script> alert(""MS ID cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        If (e.NewValues("excalibur_id") = Nothing) Then
            e.NewValues("excalibur_id") = 0
            'Response.Write("<script> alert(""Excalibur ID cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        If (e.NewValues("status") = Nothing) Then
            Response.Write("<script> alert(""Status cannot be blank."")</script>")
            e.cancel = true
            return
        End If
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
        If (e.NewValues("position") = Nothing) Then
            Response.Write("<script> alert(""Position cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("basepayrate") = Nothing) Then
            Response.Write("<script> alert(""Base Pay Rate cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("cellphone") = Nothing) Then
            Response.Write("<script> alert(""Cell Phone cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("email") = Nothing) Then
            Response.Write("<script> alert(""Email cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("record") = Nothing) Then
            e.NewValues("record") = " "
            'Response.Write("<script> alert(""Record cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        #End Region

        #Region "Input Validation"
        If (e.NewValues("company").ToString <> "Excalibur" and e.NewValues("company").ToString <> "Matt's Staffing") Then
            Response.Write("<script> alert(""Company must be Excalibur or Matt's Staffing!"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("ms_id").ToString.Length > 49) Then
            Response.Write("<script> alert(""MD ID must be less than 50 characters."")</script>")
            e.cancel = true
            return
        Else
            If Not Integer.TryParse(e.NewValues("ms_id").ToString, 0) Then
                Response.Write("<script> alert(""MS ID must be an Integer!"")</script>")
                e.cancel = true
                return
            End If
        End If
        If (e.NewValues("excalibur_id").ToString.Length > 49) Then
            Response.Write("<script> alert(""Excalibur ID must be less than 50 characters."")</script>")
            e.cancel = true
            return
        Else
            If Not Integer.TryParse(e.NewValues("excalibur_id").ToString, 0) Then
                Response.Write("<script> alert(""Excalibur ID must be an Integer!"")</script>")
                e.cancel = true
                return
            End If
        End If
        If e.NewValues("ms_id").ToString = 0 And e.NewValues("excalibur_id") = 0 Then
            Response.Write("<script> alert(""Both Excalibur ID and MS ID are empty!"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("status").ToString <> "Active" And e.NewValues("status").ToString <> "Inactive") Then
            Response.Write("<script> alert(""Status must be 'Active' or 'Inactive'"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("firstname").ToString.Length >= 20) Then
            Response.Write("<script> alert(""First Name must be less than 20 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("lastname").ToString.Length >= 20) Then
            Response.Write("<script> alert(""Last Name must be less than 20 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("position").ToString.Length >= 50) Then
            Response.Write("<script> alert(""Position must be less than 50 characters."")</script>")
            e.cancel = true
            return
        End If
        Try
            dim rate As Double = Double.Parse(e.NewValues("basepayrate"))
            If (rate > 1000.00 or rate < 5.00 ) Then
                Response.Write("<script> alert(""Base Pay Rate must be less then $1000/hr and greater than $5.00/hr"")</script>")
                e.cancel = true
                return
            End If
        Catch ex As Exception
            Response.Write("<script> alert(""Base Pay Rate must be in $$.$$ format"")</script>")
            e.cancel = true
            return
        End Try
        If (e.NewValues("cellphone").ToString.Length >= 20) Then
            Response.Write("<script> alert(""Cell Phone must be less than 20 characters"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("email").ToString.Length >= 50) Then
            Response.Write("<script> alert(""Email must be less than 50 characters"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("record") <> Nothing)
            If (e.NewValues("record").ToString.Length > 10000) Then
                Response.Write("<script> alert(""Record must be less than 10,000 characters"")</script>")
                e.cancel = true
                return
            End If
        End If
        #End Region

        #Region "Check if ms_id already exists"
        If (e.OldValues("ms_id").ToString <> e.NewValues("ms_id").ToString) Then
            Dim validate As string = "SELECT ms_id FROM employee_tbl WHERE ms_id='" & e.NewValues("ms_id") & "'"
            Dim cmd = New SqlCommand(validate, conn)
            Dim myTable As New DataTable
            Dim adapter As New SqlDataAdapter(cmd)
            adapter.Fill(myTable)
            If myTable.Rows.Count > 0 Then
                Response.Write("<script> alert(""ms_id already exists in database"")</script>")
                Return
            End If
        End If
        #End Region
        #Region "Check if excalibur_id already exists"
        If (e.OldValues("excalibur_id").ToString <> e.NewValues("excalibur_id").ToString) Then
            Dim validate As string = "SELECT excalibur_id FROM employee_tbl WHERE excalibur_id='" & e.NewValues("excalibur_id") & "'"
            Dim cmd = New SqlCommand(validate, conn)
            Dim myTable As New DataTable
            Dim adapter = New SqlDataAdapter(cmd)
            adapter.Fill(myTable)
            If myTable.Rows.Count > 0 Then
                Response.Write("<script> alert(""excalibur_id already exists in database"")</script>")
                Return
            End If
        End If
        #End Region

        Dim username As String = ""
        'Check if basepayrate was updated
        If (e.OldValues("basepayrate").ToString <> e.NewValues("basepayrate").ToString) Then

            Dim row As GridViewRow = GridView1.Rows(e.RowIndex)
            Dim control As TextBox = row.Cells(GetColumnIndexByHeaderText(GridView1, "Base Pay Rate")).Controls(0)
            Dim payrate As String = control.Text
            Dim employee_id As String = row.Cells(GetColumnIndexByHeaderText(GridView1, "Employee ID")).Text

            Dim dt1 As New DataTable()
            Dim sqlCmd1 As New SqlCommand("SELECT basepayrate, username FROM employee_tbl WHERE employee_id='"+employee_id+"'", conn)
            Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
            conn.close()
            conn.open()
            sqlDa1.Fill(dt1)
            conn.close()
            username = dt1.Rows(0).ItemArray(1).ToString()

            If dt1.Rows(0).ItemArray(0).ToString() <> payrate Then
                ' Update basepayrate for this employee for all future shift instances
                dt1 = New DataTable()
                sqlCmd1 = New SqlCommand("UPDATE client_site_shift_instance_tbl SET payrate='"+payrate+"' WHERE employee_id='"+employee_id+"' AND startdatetime>='"+DateTime.Now.ToString()+"'", conn)
                sqlDa1 = New SqlDataAdapter(sqlCmd1)
                conn.close()
                conn.open()
                sqlDa1.Fill(dt1)
                conn.close()

                ' Update client_site_shift_tbl payrate
                dt1 = New DataTable()
                sqlCmd1 = New SqlCommand("UPDATE client_site_shift_tbl SET payrate='"+payrate+"' WHERE employee_id='"+employee_id+"'", conn)
                sqlDa1 = New SqlDataAdapter(sqlCmd1)
                conn.close()
                conn.open()
                sqlDa1.Fill(dt1)
                conn.close()
            End If
        End If

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("company") = Nothing) Then e.OldValues("company") = " "
        If (e.OldValues("ms_id") = Nothing) Then e.OldValues("ms_id") = " "
        If (e.OldValues("excalibur_id") = Nothing) Then e.OldValues("excalibur_id") = " "
        If (e.OldValues("status") = Nothing) Then e.OldValues("status") = " "
        If (e.OldValues("firstname") = Nothing) Then e.OldValues("firstname") = " "
        If (e.OldValues("lastname") = Nothing) Then e.OldValues("lastname") = " "
        If (e.OldValues("position") = Nothing) Then e.OldValues("position") = " "
        If (e.OldValues("basepayrate") = Nothing) Then e.OldValues("basepayrate") = " "
        If (e.OldValues("cellphone") = Nothing) Then e.OldValues("cellphone") = " "
        If (e.OldValues("email") = Nothing) Then e.OldValues("email") = " "
        If (e.OldValues("record") = Nothing) Then e.OldValues("record") = " "

        If (e.NewValues("company") = Nothing) Then e.NewValues("company") = " "
        If (e.NewValues("ms_id") = Nothing) Then e.NewValues("ms_id") = " "
        If (e.NewValues("excalibur_id") = Nothing) Then e.NewValues("excalibur_id") = " "
        If (e.NewValues("status") = Nothing) Then e.NewValues("status") = " "
        If (e.NewValues("firstname") = Nothing) Then e.NewValues("firstname") = " "
        If (e.NewValues("lastname") = Nothing) Then e.NewValues("lastname") = " "
        If (e.NewValues("position") = Nothing) Then e.NewValues("position") = " "
        If (e.NewValues("basepayrate") = Nothing) Then e.NewValues("basepayrate") = " " 
        If (e.NewValues("cellphone") = Nothing) Then e.NewValues("cellphone") = " "
        If (e.NewValues("email") = Nothing) Then e.NewValues("email") = " "
        If (e.NewValues("record") = Nothing Or e.NewValues("record") = " ") Then e.NewValues("record") = " "
        #End Region

        '' Add Change To Log
        Dim Changes_Made As string = ""
        if (e.OldValues("company") <> e.NewValues("company")) Then Changes_Made += Session("user") + " changed Company of "+username+" from '"+e.OldValues("company").ToString()+"' to '"+e.NewValues("company").ToString()+"' <br />"
        if (e.OldValues("ms_id") <> e.NewValues("ms_id")) Then Changes_Made += Session("user") + " changed MS ID of "+username+" from '"+e.OldValues("ms_id").ToString()+"' to '"+e.NewValues("ms_id").ToString()+"' <br />"
        if (e.OldValues("excalibur_id") <> e.NewValues("excalibur_id")) Then Changes_Made += Session("user") + " changed Excalibur ID of "+username+" from '"+e.OldValues("excalibur_id").ToString()+"' to '"+e.NewValues("excalibur_id").ToString()+"' <br />"
        if (e.OldValues("status") <> e.NewValues("status")) Then Changes_Made += Session("user") + " changed Status of "+username+" from '"+e.OldValues("status").ToString()+"' to '"+e.NewValues("status").ToString()+"' <br />"
        if (e.OldValues("firstname") <> e.NewValues("firstname")) Then Changes_Made += Session("user") + " changed First Name of "+username+" from '"+e.OldValues("firstname").ToString()+"' to '"+e.NewValues("firstname").ToString()+"' <br />"
        if (e.OldValues("lastname") <> e.NewValues("lastname")) Then Changes_Made += Session("user") + " changed Last Name of "+username+" from '"+e.OldValues("lastname").ToString()+"' to '"+e.NewValues("lastname").ToString()+"' <br />"
        if (e.OldValues("position") <> e.NewValues("position")) Then Changes_Made += Session("user") + " changed Position of "+username+" from '"+e.OldValues("position").ToString()+"' to '"+e.NewValues("position").ToString()+"' <br />"
        if (e.OldValues("basepayrate") <> e.NewValues("basepayrate")) Then Changes_Made += Session("user") + " changed Base Pay Rate of "+username+" from '"+e.OldValues("basepayrate").ToString()+"' to '"+e.NewValues("basepayrate").ToString()+"' <br />"
        if (e.OldValues("cellphone") <> e.NewValues("cellphone")) Then Changes_Made += Session("user") + " changed Cell Phone of "+username+" from '"+e.OldValues("cellphone").ToString()+"' to '"+e.NewValues("cellphone").ToString()+"' <br />"
        if (e.OldValues("email") <> e.NewValues("email")) Then Changes_Made += Session("user") + " changed Email of "+username+" from '"+e.OldValues("email").ToString()+"' to '"+e.NewValues("email").ToString()+"' <br />"
        if (e.OldValues("record") <> e.NewValues("record")) Then Changes_Made += Session("user") + " changed Record of "+username+" from '"+e.OldValues("record").ToString()+"' to '"+e.NewValues("record").ToString()+"' <br />"

        If (Changes_Made <> "") Then Log_Change(Session("user"), "Employee", Changes_Made, "Update")


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