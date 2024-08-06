Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Diagnostics.Eventing
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Linq.Expressions
Imports System.Web.DynamicData
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles

Public Class adminpayroll
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)

    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Private startdate As DateTime
    Private enddate As DateTime

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If

        If Not IsPostBack Then

            '' If SuperAdmin Then Show $$ Columns
            myCmd = conn.CreateCommand
            myCmd.CommandText = "SELECT user_level FROM admin_tbl WHERE username='"+Session("user")+"'"
            conn.Open()
            myReader = myCmd.ExecuteReader()
            Do While myReader.Read()
                results = myReader.GetString(0)
            Loop
            myReader.Close()
            conn.Close()
            If (results = "superadmin") Then
                ClientTotalsGridView.Columns(GetColumnIndexByHeaderText(ClientTotalsGridView, "Rate")).Visible = True
                ClientTotalsGridView.Columns(GetColumnIndexByHeaderText(ClientTotalsGridView, "Rev. (Hrs)")).Visible = True
            End If

            '' Populate the Employee DropDownLists
            myCmd = conn.CreateCommand
            myCmd.CommandText = "SELECT username, employee_id FROM employee_tbl WHERE status='Active' ORDER BY employee_tbl.username ASC"
            conn.Open()
            myReader = myCmd.ExecuteReader()
            employee_ddl.Items.Clear()
            employee_ddl.Items.Add(" ")
            employee_ddl.Items.FindByText(" ").Value = 0
            Reimburse_EmployeeDDL.Items.Clear()
            Reimburse_EmployeeDDL.Items.Add(" ")
            Reimburse_EmployeeDDL.Items.FindByText(" ").Value = 0
            Sick_EmployeeDDL.Items.Clear()
            Sick_EmployeeDDL.Items.Add(" ")
            Sick_EmployeeDDL.Items.FindByText(" ").Value = 0
            Do While myReader.Read()
                results = myReader.GetString(0)
                employee_ddl.Items.Add(results)
                Reimburse_EmployeeDDL.Items.Add(results)
                Sick_EmployeeDDL.Items.Add(results)
                employee_ddl.Items.FindByText(results).Value = myReader.GetInt32(1)
                Reimburse_EmployeeDDL.Items.FindByText(results).Value = myReader.GetInt32(1)
                Sick_EmployeeDDL.Items.FindByText(results).Value = myReader.GetInt32(1)
            Loop
            myReader.Close()
            conn.Close()

            '' Populate the Client DropDownList
            myCmd = conn.CreateCommand
            myCmd.CommandText = "SELECT name, client_site_id FROM client_site_tbl ORDER BY name ASC"
            conn.Open()
            myReader = myCmd.ExecuteReader()
            client_ddl.Items.Clear()
            client_ddl.Items.Add(" ")
            client_ddl.Items.FindByText(" ").Value = 0
            Do While myReader.Read()
                results = myReader.GetString(0)
                client_ddl.Items.Add(results)
                client_ddl.Items.FindByText(results).Value = myReader.GetInt32(1)
            Loop
            myReader.Close()
            conn.Close()

            '' Set Week Of and load it
            weekOfDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd")
            weekOfChanged(vbNull, e)

            updateShiftTable() '' Must be inside If (Not PostBack) Statement or else Shift Instance Grid View methods will not work

        End If

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        'Log_Change(Session(user), Log Out, Logged Out, )

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub


    #Region "ShiftInstance GridView Methods"
    Protected Sub ShiftInstanceGridView_Sorting(sender As object, e As GridViewSortEventArgs)
      
        'Retrieve the table from the session object.
        Dim dt As DataTable = Session("ShiftInstanceGridView")

        ShiftInstanceGridView.DataSource = dt
        ShiftInstanceGridView.DataBind()

        ' Sort the data.
        If (e.SortExpression = "name") Then 
            dt.DefaultView.Sort = e.SortExpression + " ASC, username ASC, startdatetime ASC"
        Else If (e.SortExpression = "username") Then
            dt.DefaultView.Sort = e.SortExpression + " ASC, startdatetime ASC"
        End If
        ShiftInstanceGridView.DataSource = Session("ShiftInstanceGridView")
        ShiftInstanceGridView.DataBind()

        ' Populate the Employee_DDL
        'Dim dt2 As New DataTable()
        'Dim sqlCmd2 As New SqlCommand("SELECT username, employee_id username FROM employee_tbl WHERE status='Active' ORDER BY employee_tbl.username ASC", conn)
        'Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
        'conn.close()
        'conn.open()
        'sqlDa2.Fill(dt2)
        'conn.close()
        'Dim r As Int32 = 0
        'For Each row in ShiftInstanceGridView.Rows
        '    Dim ddl As DropDownList = row.FindControl(Username_DDL)
        '    ddl.DataSource = dt2
        '    ddl.DataTextField = dt2.Columns(0).ToString()
        '    ddl.DataValueField = dt2.Columns(1).ToString()
        '    ddl.DataBind()
        '    ddl.Items.Insert(0, new ListItem("<--     OPEN     -->", ""))
        '    r += 1
        'Next

        updateShiftTable()

    End Sub
    Protected Sub ButtonEdit_Click(sender As Object, e As EventArgs)
        
        Dim script As string = "$('#mymodal').modal('show');"
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp",script,true)

        ShiftInstance_ID.Text = sender.DataItemContainer.Cells(0).Text

        '' Load Modal with ShiftInstance clicked
        ShiftModalLabel.Text = "Client Site: " + sender.DataItemContainer.Cells(3).Text

        '' Load ShiftInstance div
        'Load this form with client_site_shift_instance data
        Dim queryString As string = "SELECT shift_name, shift_notes, startdatetime, enddatetime, payrate, onduty_mealperiods, employee_id, client_site_shift_id FROM client_site_shift_instance_tbl WHERE client_site_shift_instance_id='" + ShiftInstance_ID.Text + ""'"
        Dim command As SqlCommand = new SqlCommand(queryString, conn)
        Dim this_startdatetime As string = ""
        Dim this_enddatetime As string = ""
        Dim this_employee_id As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
                ShiftInstance_Name.Text = reader(0).ToString()
                ShiftInstance_ShiftNotes.Text = reader(1).ToString()
                ShiftInstance_StartDate.Text = DateTime.Parse(reader(2).ToString()).ToString("yyyy-MM-dd")
                ShiftInstance_EndDate.Text = DateTime.Parse(reader(3).ToString()).ToString("yyyy-MM-dd")
                this_startdatetime = reader(2).ToString()
                this_enddatetime = reader(3).ToString()
                ShiftInstance_StartTime.Text = DateTime.Parse(reader(2).ToString()).ToString(“HH:mm”)
                ShiftInstance_EndTime.Text = DateTime.Parse(reader(3).ToString()).ToString(“HH:mm”)
                ShiftInstance_PayRate.Text = reader(4).ToString()
                ShiftInstance_OnDutyMealPeriods.Checked = boolean.Parse(reader(5).ToString())
                this_employee_id = reader(6).ToString()
                Shift_Id.Text = reader(7).ToString()
            End While
            reader.Close()
            finally  
                conn.Close() 
        End Try
            
        'Load Assigned Employee ComboBoxes
        queryString = "SELECT username, cellphone FROM employee_tbl WHERE employee_id = '" + this_employee_id + "'"
        command = new SqlCommand(queryString, conn)
        Dim username As string = ""
        Dim cellphone As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
                username = reader(0).ToString()
                cellphone = reader(1).ToString()
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try

        'Load Assigned Employee ComboBox with all active employees
        Dim dataTable = new DataTable()
        queryString = "SELECT username, cellphone FROM employee_tbl WHERE status='Active' OR employee_id='" + this_employee_id + "' ORDER BY employee_tbl.username ASC"
        command = new SqlCommand(queryString, conn)
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            dataTable.Load(reader)
            reader.Close()
        finally 
            conn.Close()
        End Try

        ShiftInstance_EmployeeDDL.Items.Add("<--     OPEN     -->")
        For Each r As DataRow in dataTable.Rows
            ShiftInstance_EmployeeDDL.Items.Add(r.ItemArray(0) + ", " + r.ItemArray(1))
        Next
        If username = "" Then
            ShiftInstance_EmployeeDDL.SelectedValue = "<--     OPEN     -->"
        Else 
            ShiftInstance_EmployeeDDL.SelectedValue = ShiftInstance_EmployeeDDL.Items.FindByText(username + ", " + cellphone).ToString()
        End If

        updateShiftTable()

    End Sub
    Protected Sub UpdateShift_Click(sender As Object, e As EventArgs)
        '' Validate Form Input
        ''PayRAte is $$.$$  
        Dim paternWithDot As Regex = new Regex("$\d+(\.\d+)+")
            if ( Not paternWithDot.IsMatch(ShiftInstance_PayRate.Text)) Then
                Response.Write("<script> alert(""Pay Rate must be in $$.$$ format!"")</script>") 
                return
            End If
        ''End Date >= Start Date 
            if (DateTime.Parse(ShiftInstance_EndDate.Text) < DateTime.Parse(ShiftInstance_StartDate.Text)) Then
                Response.Write("<script> alert(""End Date must be after Start Date!"")</script>")
                return
            End If
            Dim starttime1 As DateTime = DateTime.Parse(ShiftInstance_StartDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_StartTime.Text.ToString())
            Dim endtime1 As DateTime = DateTime.Parse(ShiftInstance_EndDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_EndTime.Text.ToString())
            if (starttime1.AddMinutes(1) > endtime1) Then
                Response.Write("<script> alert(""End Time must be at least 1 minute after Start Time!"")</script>")
                return
            End If


        '' Get the Employee_id of the AssignedEmployee
        Dim queryString As string = "SELECT employee_id, basepayrate FROM employee_tbl WHERE username = '" + ShiftInstance_EmployeeDDL.Text.Split(",").First() + "'"
        Dim command As SqlCommand = New SqlCommand(queryString, conn)
        Dim employee_id As string = ""
        Dim basepayrate As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
                employee_id = reader(0).ToString()
                basepayrate = reader(1).ToString()
            End While
            reader.Close()
        Finally 
            conn.Close()
        End Try

        '' Update client_site_shift_instance_tbl using form data
        queryString = "UPDATE client_site_shift_instance_tbl SET shift_name=@shift_name, shift_notes=@shift_notes, employee_id=@employee_id, payrate=@payrate, onduty_mealperiods=@onduty_mealperiods, startdatetime=@startdatetime, enddatetime=@enddatetime, firstmeal_startdatetime=@firstmeal_startdatetime, firstmeal_enddatetime=@firstmeal_enddatetime, secondmeal_startdatetime=@secondmeal_startdatetime, secondmeal_enddatetime=@secondmeal_enddatetime, thirdmeal_startdatetime=@thirdmeal_startdatetime, thirdmeal_enddatetime=@thirdmeal_enddatetime WHERE client_site_shift_instance_id = '" + ShiftInstance_ID.Text + "'"
        command = new SqlCommand(queryString, conn)
        Dim starttime As DateTime = DateTime.Parse(ShiftInstance_StartDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_StartTime.Text.ToString())
        Dim endtime As DateTime = DateTime.Parse(ShiftInstance_EndDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_EndTime.Text.ToString())
        Dim span As Double = endtime.Ticks - starttime.Ticks
        Dim fivehours As Double = 10000000L * 60 * 60 * 5
        Dim tenhours As Double = 10000000L * 60 * 60 * 10
        Dim sixteenhours As Double = 10000000L * 60 * 60 * 16
        Dim firstmeal_startdatetime As string = ""
        Dim firstmeal_enddatetime As string = ""
        Dim secondmeal_startdatetime As string = ""
        Dim secondmeal_enddatetime As string = ""
        Dim thirdmeal_startdatetime As string = ""
        Dim thirdmeal_enddatetime As string = ""
        if (span > fivehours) Then
            firstmeal_startdatetime = starttime.AddHours(4).ToString()
            firstmeal_enddatetime = starttime.AddHours(4.5).ToString()
        End If
        if (span > tenhours) Then
            secondmeal_startdatetime = starttime.AddHours(8.5).ToString()
            secondmeal_enddatetime = starttime.AddHours(9).ToString()
        End If
        if (span > sixteenhours) Then
            thirdmeal_startdatetime = starttime.AddHours(13).ToString()
            thirdmeal_enddatetime = starttime.AddHours(13.5).ToString()
        End If
        command.Parameters.AddWithValue("@firstmeal_startdatetime", firstmeal_startdatetime)
        command.Parameters.AddWithValue("@firstmeal_enddatetime", firstmeal_enddatetime)
        command.Parameters.AddWithValue("@secondmeal_startdatetime", secondmeal_startdatetime)
        command.Parameters.AddWithValue("@secondmeal_enddatetime", secondmeal_enddatetime)
        command.Parameters.AddWithValue("@thirdmeal_startdatetime", thirdmeal_startdatetime)
        command.Parameters.AddWithValue("@thirdmeal_enddatetime", thirdmeal_enddatetime)
        command.Parameters.AddWithValue("@shift_name", ShiftInstance_Name.Text)
        command.Parameters.AddWithValue("@shift_notes", ShiftInstance_ShiftNotes.Text)
        command.Parameters.AddWithValue("@employee_id", employee_id)
        command.Parameters.AddWithValue("@payrate", basepayrate)
        command.Parameters.AddWithValue("@onduty_mealperiods", ShiftInstance_OnDutyMealPeriods.Checked.ToString())
        command.Parameters.AddWithValue("@startdatetime", ShiftInstance_StartDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_StartTime.Text.ToString())
        command.Parameters.AddWithValue("@enddatetime", ShiftInstance_EndDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_EndTime.Text.ToString())
        if (thirdmeal_startdatetime <> "") Then
            queryString = "UPDATE client_site_shift_instance_tbl SET shift_name=@shift_name, shift_notes=@shift_notes, employee_id=@employee_id, payrate=@payrate, onduty_mealperiods=@onduty_mealperiods, startdatetime=@startdatetime, enddatetime=@enddatetime, firstmeal_startdatetime=@firstmeal_startdatetime, firstmeal_enddatetime=@firstmeal_enddatetime, secondmeal_startdatetime=@secondmeal_startdatetime, secondmeal_enddatetime=@secondmeal_enddatetime, thirdmeal_startdatetime=@thirdmeal_startdatetime, thirdmeal_enddatetime=@thirdmeal_enddatetime WHERE client_site_shift_instance_id = '" + ShiftInstance_ID.Text + "'" 
        Else if (secondmeal_enddatetime <> "") Then 
            queryString = "UPDATE client_site_shift_instance_tbl SET shift_name=@shift_name, shift_notes=@shift_notes, employee_id=@employee_id, payrate=@payrate, onduty_mealperiods=@onduty_mealperiods, startdatetime=@startdatetime, enddatetime=@enddatetime, firstmeal_startdatetime=@firstmeal_startdatetime, firstmeal_enddatetime=@firstmeal_enddatetime, secondmeal_startdatetime=@secondmeal_startdatetime, secondmeal_enddatetime=@secondmeal_enddatetime, thirdmeal_startdatetime=NULL, thirdmeal_enddatetime=NULL WHERE client_site_shift_instance_id = '" + ShiftInstance_ID.Text + "'"
        Else  
            queryString = "UPDATE client_site_shift_instance_tbl SET shift_name=@shift_name, shift_notes=@shift_notes, employee_id=@employee_id, payrate=@payrate, onduty_mealperiods=@onduty_mealperiods, startdatetime=@startdatetime, enddatetime=@enddatetime, firstmeal_startdatetime=@firstmeal_startdatetime, firstmeal_enddatetime=@firstmeal_enddatetime, secondmeal_startdatetime=NULL, secondmeal_enddatetime=NULL, thirdmeal_startdatetime=NULL, thirdmeal_enddatetime=NULL WHERE client_site_shift_instance_id = '" + ShiftInstance_ID.Text + "'"
        End If
        command.CommandText = queryString
        Try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
            End While
            reader.Close()
        Finally 
            conn.Close()
        End Try
        
        '' Reload Shift Table
        updateShiftTable()

        'Hide the modal
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp","$('#mymodal').modal('hide');",true)

    End Sub
    Protected Sub DeleteShift_Click(sender As Object, e As EventArgs)

        ' Delete the shift
        Dim queryString As string = "DELETE FROM client_site_shift_instance_tbl WHERE client_site_shift_instance_id = '" + ShiftInstance_ID.Text + "'"
        Dim command = new SqlCommand(queryString, conn)
        Try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read()) 
            End While
            reader.Close()
        Finally 
            conn.Close()
        End Try

        ' If the number of shift instances for this shift is 0, then delete the shift template
        Dim dataTable = new DataTable()
        queryString = "SELECT client_site_shift_id FROM client_site_shift_instance_tbl WHERE client_site_shift_id = '" + Shift_ID.Text + "'"
        command = new SqlCommand(queryString, conn)
        Try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            dataTable.Load(reader)
            reader.Close()
        Finally 
            conn.Close()
        End Try

        if (dataTable.Rows.Count = 0) Then
        
            queryString = "DELETE FROM client_site_shift_tbl WHERE client_site_shift_id = '" + Shift_ID.Text + "'"
            command = new SqlCommand(queryString, conn)
            Try
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command.ExecuteReader()
                While (reader.Read())
                End While
                reader.Close()
            Finally 
                conn.Close()
            End Try

            ' Message User that there are no more shift instances and the shift template was permanently deleted
            Response.Write("<script> alert(""You deleted the last shift instance for this shift. The shift template was also deleted."")</script>") 

        End If

        '' Reload Shift Table
        updateShiftTable()

        'Hide the modal
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp","$('#mymodal').modal('hide');",true)

    End Sub

    #End Region



    Protected Sub Insert_Reimbursement(sender As Object, e As EventArgs)
        '' Input Checking
        If weekOfDate.Text.Length > 0 Then
            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)
        Else
            Response.Write("<script> alert(""Week Of must be selected!"")</script>")
            Return
        End If
        If (DateTime.Parse(Reimburse_DateTB.Text) < DateTime.Parse(startdate)) Or (DateTime.Parse(Reimburse_DateTB.Text) > DateTime.Parse(enddate)) Then
            Response.Write("<script> alert(""Date must be within selected Payroll Week!"")</script>")
            Return
        End If


        Dim employee_ids As String = Reimburse_EmployeeDDL.SelectedValue
        Dim amounts As String = Reimburse_AmountTB.Text
        Dim usernames As String = Reimburse_EmployeeDDL.SelectedItem.Text
        Dim descriptions As String = Reimburse_DescriptionTB.Text
        Dim datetimes As DateTime = DateTime.Parse(Reimburse_DateTB.Text)
        Dim is_taxable As Boolean = Reimburse_GrossCB.Checked
        Dim is_not_taxable As Boolean = Not Reimburse_GrossCB.Checked

        '' Fill The reimbursements_Table
        Dim sqlCmd2 As New SqlCommand("INSERT INTO reimbursement_tbl (employee_id, username, amount, description, datetime, is_gross_taxablewage) VALUES (@employee_id, @username, @amount, @description, @datetime, @is_gross_taxablewage)", conn)
        sqlCmd2.Parameters.AddWithValue("@employee_id", employee_ids)
        sqlCmd2.Parameters.AddWithValue("@username", usernames)
        sqlCmd2.Parameters.AddWithValue("@amount", amounts)
        sqlCmd2.Parameters.AddWithValue("@description", descriptions)
        sqlCmd2.Parameters.AddWithValue("@datetime", datetimes)
        sqlCmd2.Parameters.AddWithValue("@is_gross_taxablewage", is_taxable)

        Dim x As Integer = 0
        Try
            conn.close()
            conn.open()
            x = sqlCmd2.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            sqlCmd2.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                'Response.Write("<script> alert(""Reimbursement successfully added To the database!"")</script>")
                Log_Change(Session("user"), "Reimbursments", Session("user") + " added Reimbursment for " + usernames + ":  Amount=" + amounts + ", Date="+datetimes.ToShortDateString+", Description="+descriptions+", Is Taxable Wage="+is_taxable.ToString+".", "")
            Case 0
                Response.Write("<script> alert(""Issue connecting To server Or executing the INSERT INTO reimbursement_tbl query!"")</script>")
        End Select

        ReimbursementsGridView.DataBind()

    End Sub
    Protected Sub Insert_SickRequest(sender As Object, e As EventArgs)
        '' Input Checking
        If weekOfDate.Text.Length > 0 Then
            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)
        Else
            Response.Write("<script> alert(""Week Of must be selected!"")</script>")
            Return
        End If
        If (DateTime.Parse(Sick_DateTB.Text) < DateTime.Parse(startdate)) Or (DateTime.Parse(Sick_DateTB.Text) > DateTime.Parse(enddate)) Then
            Response.Write("<script> alert(""Date must be within selected Payroll Week!"")</script>")
            Return
        End If

        Dim employee_ids As String = Sick_EmployeeDDL.SelectedValue
        Dim usernames As String = Sick_EmployeeDDL.SelectedItem.Text
        Dim minutes As String = Sick_MinutesTB.Text
        Dim descriptions As String = Sick_DescriptionTB.Text
        Dim datetimes As DateTime = DateTime.Parse(Sick_DateTB.Text)

        '' Fill The Sick Pay Table
        Dim sqlCmd2 As New SqlCommand("INSERT INTO sickpay_request_tbl (employee_id, username, datetime, minutes_requested, description) VALUES (@employee_id, @username, @datetime, @minutes_requested, @description)", conn)
        sqlCmd2.Parameters.AddWithValue("@employee_id", employee_ids)
        sqlCmd2.Parameters.AddWithValue("@username", usernames)
        sqlCmd2.Parameters.AddWithValue("@datetime", datetimes)
        sqlCmd2.Parameters.AddWithValue("@minutes_requested", minutes)
        sqlCmd2.Parameters.AddWithValue("@description", descriptions)

        Dim x As Integer = 0
        Try
            conn.close()
            conn.open()
            x = sqlCmd2.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write("<script> alert(" & ex.Message & ")</script>")
        Finally
            conn.close()
            sqlCmd2.Parameters.Clear()
        End Try

        Select Case x
            Case 1
                'Response.Write("<script> alert(""Sickpay Request successfully added To the database!"")</script>")
                Log_Change(Session("user"), "Sick Pay Request", Session("user") + " added Sick Pay Request for " + usernames + ":  Minutes=" + minutes + ", Date="+datetimes.ToShortDateString+", Description='"+descriptions+"'.", "")
            Case 0
                Response.Write("<script> alert(""Issue connecting To server Or executing the INSERT INTO sickpay_request_tbl query!"")</script>")
        End Select

        SickPayGridView.DataBind()

    End Sub
    Protected Sub Reimbursements_OnRowUpdating(sender As Object, e As GridViewUpdateEventArgs)

        #Region "NewValues cannot be blank"
        If (e.NewValues("datetime") = Nothing) Then
            Response.Write("<script> alert(""Date Time cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("description") = Nothing) Then
            Response.Write("<script> alert(""Description cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("amount") = Nothing) Then
            Response.Write("<script> alert(""Amount cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("is_gross_taxablewage") = Nothing) Then
            Response.Write("<script> alert(""Is Taxable Wage cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        #End Region

        #Region "Input Validation"
        Try 
            DateTime.Parse(e.NewValues("datetime"))
        Catch ex As Exception
            Response.Write("<script> alert(""DateTime is in an incompatable format"")</script>")
            e.Cancel = true
            return
        End Try
        If (e.NewValues("description").ToString.Length > 1999) Then
            Response.Write("<script> alert(""Description must be less than 2000 characters."")</script>")
            e.cancel = true
            return
        End If
        Try
            dim amount As Double = Double.Parse(e.NewValues("amount"))
            If (amount > 10000.00 or amount < -10000.00 ) Then
                Response.Write("<script> alert(""Amount must be less then $10000.00 and greater than -$10000.00"")</script>")
                e.cancel = true
                return
            End If
        Catch ex As Exception
            Response.Write("<script> alert(""Amount must be in $$.$$ format"")</script>")
            e.cancel = true
            return
        End Try
        If (e.NewValues("is_gross_taxablewage").ToString <> "True" And e.NewValues("is_gross_taxablewage").ToString <> "False") Then
            Response.Write("<script> alert(""Is Gross Taxable Wage must be 'True' or 'False'"")</script>")
            e.cancel = true
            return
        End If
        #End Region

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("datetime") = Nothing) Then e.OldValues("datetime") = " "
        If (e.OldValues("description") = Nothing) Then e.OldValues("description") = " " 
        If (e.OldValues("amount") = Nothing) Then e.OldValues("amount") = " "
        If (e.OldValues("is_gross_taxablewage") = Nothing) Then e.OldValues("is_gross_taxablewage") = " "

        If (e.NewValues("datetime") = Nothing) Then e.NewValues("datetime") = " "
        If (e.NewValues("description") = Nothing) Then e.NewValues("description") = " "
        If (e.NewValues("amount") = Nothing) Then e.NewValues("amount") = " "
        If (e.NewValues("is_gross_taxablewage") = Nothing) Then e.NewValues("is_gross_taxablewage") = " "
        #End Region

        '' Add Change To Log
        Dim username As string = e.OldValues("username")
        Dim Changes_Made As string = ""
        if (e.OldValues("datetime") <> e.NewValues("datetime")) Then Changes_Made += Session("user") + " changed Date/Time of "+username+"'s Reimbursement #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("datetime").ToString()+"' to '"+e.NewValues("datetime").ToString()+"' <br />"
        if (e.OldValues("description") <> e.NewValues("description")) Then Changes_Made += Session("user") + " changed Description of "+username+"'s Reimbursement #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("description").ToString()+"' to '"+e.NewValues("description").ToString()+"' <br />"
        if (e.OldValues("amount") <> e.NewValues("amount")) Then Changes_Made += Session("user") +"  changed Amount of "+username+"'s Reimbursement #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("amount").ToString()+"' to '"+e.NewValues("amount").ToString()+"' <br />"
        if (e.OldValues("is_gross_taxablewage") <> e.NewValues("is_gross_taxablewage")) Then Changes_Made += Session("user") + " changed IsTaxableWage of "+username+"'s Reimbursement #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("is_gross_taxablewage").ToString()+"' to '"+e.NewValues("is_gross_taxablewage").ToString()+"' <br />"
        
        If (Changes_Made <> "") Then Log_Change(Session("user"), "Reimbursements", Changes_Made, "Update")
    End Sub
    Protected Sub Reimbursements_OnRowDeleting(sender As Object, e As GridViewDeleteEventArgs)
        Dim Changes_Made As string = Session("user") + " deleted "+e.Values("username")+"'s Reimbursement #"+e.Keys.Values(0).ToString()+" which had values: datetime="+e.Values("datetime")+", description="+e.Values("description")+", amount='"+e.Values("amount")+"', is_gross_taxablewage="+e.Values("is_gross_taxablewage")+"."
        Log_Change(Session("user"), "Reimbursements", Changes_Made, "Delete")
    End Sub
    Protected Sub SickPay_OnRowUpdating(sender As Object, e As GridViewUpdateEventArgs)

        #Region "NewValues cannot be blank"
        If (e.NewValues("datetime") = Nothing) Then
            Response.Write("<script> alert(""Date Time cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("description") = Nothing) Then
            Response.Write("<script> alert(""Description cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("minutes_requested") = Nothing) Then
            Response.Write("<script> alert(""Minutes Requested cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        #End Region

        #Region "Input Validation"
        Try 
            DateTime.Parse(e.NewValues("datetime"))
        Catch ex As Exception
            Response.Write("<script> alert(""DateTime is in an incompatable format."")</script>")
            e.Cancel = true
            return
        End Try
        If (e.NewValues("description").ToString.Length > 1999) Then
            Response.Write("<script> alert(""Description must be less than 2000 characters."")</script>")
            e.cancel = true
            return
        End If
        Try
            dim minutes_requested As Integer = Integer.Parse(e.NewValues("minutes_requested"))
            If (minutes_requested >= 10000 or minutes_requested < 0 ) Then
                Response.Write("<script> alert(""Minutes Requested must be less then 10000 and greater than 0"")</script>")
                e.cancel = true
                return
            End If
        Catch ex As Exception
            Response.Write("<script> alert(""Minutes Requested must be an integer."")</script>")
            e.cancel = true
            return
        End Try
        #End Region

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("datetime") = Nothing) Then e.OldValues("datetime") = " "
        If (e.OldValues("description") = Nothing) Then e.OldValues("description") = " "
        If (e.OldValues("minutes_requested") = Nothing) Then e.OldValues("minutes_requested") = " "

        If (e.NewValues("datetime") = Nothing) Then e.NewValues("datetime") = " "
        If (e.NewValues("description") = Nothing) Then e.NewValues("description") = " "
        If (e.NewValues("minutes_requested") = Nothing) Then e.NewValues("minutes_requested") = " "
        #End Region

        '' Add Change To Log
        Dim username As string = e.OldValues("username")
        Dim Changes_Made As string = ""
        if (e.OldValues("datetime") <> e.NewValues("datetime")) Then Changes_Made += Session("user") + " changed Date/Time of "+username+"'s Sick Pay Request #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("datetime").ToString()+"' to '"+e.NewValues("datetime").ToString()+"' <br />"
        if (e.OldValues("description") <> e.NewValues("description")) Then Changes_Made += Session("user") + " changed Description of "+username+"'s Sick Pay Request #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("description").ToString()+"' to '"+e.NewValues("description").ToString()+"' <br />"
        if (e.OldValues("minutes_requested") <> e.NewValues("minutes_requested")) Then Changes_Made += Session("user") + " changed Minutes Requested of "+username+"'s Sick Pay Request #"+e.Keys.Values(0).ToString()+" from '"+e.OldValues("minutes_requested").ToString()+"' to '"+e.NewValues("minutes_requested").ToString()+"' <br />"
        
        If (Changes_Made <> "") Then Log_Change(Session("user"), "Sick Pay Request", Changes_Made, "Update")
    End Sub
    Protected Sub SickPay_OnRowDeleting(sender As Object, e As GridViewDeleteEventArgs)
        Dim Changes_Made As string = Session("user") +"  deleted "+e.Values("username")+"'s Sick Pay Request #"+e.Keys.Values(0).ToString()+" which had values: datetime="+e.Values("datetime")+", description="+e.Values("description")+", minutes_requested='"+e.Values("minutes_requested")+"."
        Log_Change(Session("user"), "Sick Pay", Changes_Made, "Delete")
    End Sub
    Protected Sub employee_ddl_textChanged(sender As Object, e As EventArgs)

        If weekOfDate.Text.Length > 0 Then

            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)

            payroll_date_range_start.Text = startdate.ToShortDateString()
            payroll_date_range_end.Text = enddate.ToShortDateString()

            client_ddl.SelectedIndex = 0

            If employee_ddl.SelectedIndex <> 0 Then
                updateTables("Excalibur", false)
                updateTables("Matt's Staffing", false)
            Else
                ExcaliburTotalsGridView.Visible = False
                MattsTotalsGridView.Visible = False
                updateCompanyTotalsTable_AllEmployeesView("Excalibur", false)
                updateCompanyTotalsTable_AllEmployeesView("Matt's Staffing", false)
            End If

        End If

    End Sub
    Protected Sub client_ddl_textChanged(sender As Object, e As EventArgs)
        If weekOfDate.Text.Length > 0 Then

            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)

            payroll_date_range_start.Text = startdate.ToShortDateString()
            payroll_date_range_end.Text = enddate.ToShortDateString()

            employee_ddl.SelectedIndex = 0

            If client_ddl.SelectedIndex <> 0 Then
                updateTables_byClient("Excalibur", false)
                updateTables_byClient("Matt's Staffing", false)
            Else
                updateCompanyTotalsTable_AllEmployeesView("Excalibur", false)
                updateCompanyTotalsTable_AllEmployeesView("Matt's Staffing", false)
            End If

        End If

    End Sub
    Protected Sub weekOfChanged(sender As Object, e As EventArgs)
        If weekOfDate.Text.Length > 0 Then
            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)

            payroll_date_range_start.Text = startdate.ToShortDateString()
            payroll_date_range_end.Text = enddate.ToShortDateString()

            hyphen.Visible = True
            createMattsFlexFileButton.Visible = True
            createExcaliburFlexFileButton.Visible = True
            PTOButton.Visible = True

            CalculateTotals_Button.Visible = True

            employee_ddl.SelectedIndex = 0
            client_ddl.SelectedIndex = 0

            ExcaliburTotalsGridView.Visible = False
            MattsTotalsGridView.Visible = False
            updateCompanyTotalsTable_AllEmployeesView("Matt's Staffing", false)
            updateCompanyTotalsTable_AllEmployeesView("Excalibur", false)

        End If


    End Sub
    Protected Sub EmployeeEyeBall_Click(sender As Object, e As EventArgs)
        if (EmployeeTotalsGridView.Visible) Then
            EmployeeTotalsGridView.Visible = false
            EmployeeEyeBall_ImageButton.ImageUrl = "../images/noeyeball.png"
        Else
            EmployeeTotalsGridView.Visible = true
            EmployeeEyeBall_ImageButton.ImageUrl = "../images/eyeball.png"
        End If
    End Sub
    Protected Sub ClientEyeBall_Click(sender As Object, e As EventArgs)
        if (ClientTotalsGridView.Visible) Then
            ClientTotalsGridView.Visible = false
            ClientEyeBall_ImageButton.ImageUrl = "../images/noeyeball.png"
        Else
            ClientTotalsGridView.Visible = true
            ClientEyeBall_ImageButton.ImageUrl = "../images/eyeball.png"
        End If
    End Sub
    Protected Sub CalculateCompanyTotals_ButtonClick(sender As Object, e As EventArgs)
        If weekOfDate.Text.Length = 0 Then
            weekOfDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd")
        End If
        startdate = DateTime.Parse(weekOfDate.Text)
        enddate = startdate

        While startdate.DayOfWeek <> DayOfWeek.Monday
            startdate = startdate.AddDays(-1)
        End While
        While enddate.DayOfWeek <> DayOfWeek.Sunday
            enddate = enddate.AddDays(1)
        End While
        enddate = enddate.AddHours(23)
        enddate = enddate.AddMinutes(59)

        payroll_date_range_start.Text = startdate.ToShortDateString()
        payroll_date_range_end.Text = enddate.ToShortDateString()

        hyphen.Visible = True
        createMattsFlexFileButton.Visible = True
        createExcaliburFlexFileButton.Visible = True
        PTOButton.Visible = True

        ExcaliburTotalsGridView.Visible = True
        MattsTotalsGridView.Visible = True
        ShiftInstanceGridView.Visible = True
        ReimbursementsGridView.Visible = True
        Reimbursement_AddTable.Visible = True
        SickPayGridView.Visible = True
        SickPay_AddTable.Visible = True
        CalculateTotals_Button.Visible = True

        If employee_ddl.SelectedIndex <> 0 Then
            updateTables("Excalibur", true)
            updateTables("Matt's Staffing", true)
        Else If client_ddl.SelectedIndex <> 0 Then
            updateTables_ByClient("Excalibur", true)
            updateTables_ByClient("Matt's Staffing", true)
        Else
            updateCompanyTotalsTable_AllEmployeesView("Matt's Staffing", true)
            updateCompanyTotalsTable_AllEmployeesView("Excalibur", true)
        End If
        
    End Sub
    Protected Sub CalculateClientTotals_ButtonClick(sender As Object, e As EventArgs)
        If weekOfDate.Text.Length = 0 Then
            weekOfDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd")
        End If
        startdate = DateTime.Parse(weekOfDate.Text)
        enddate = startdate

        While startdate.DayOfWeek <> DayOfWeek.Monday
            startdate = startdate.AddDays(-1)
        End While
        While enddate.DayOfWeek <> DayOfWeek.Sunday
            enddate = enddate.AddDays(1)
        End While
        enddate = enddate.AddHours(23)
        enddate = enddate.AddMinutes(59)

        payroll_date_range_start.Text = startdate.ToShortDateString()
        payroll_date_range_end.Text = enddate.ToShortDateString()

        ClientTotalsGridView.Visible = True
        updateClientTotalsTable_AllEmployeesView()

        ClientTotalsGridView.Visible = true
        ClientEyeBall_ImageButton.ImageUrl = "../images/eyeball.png"
        
    End Sub
    Protected Sub CalculateEmployeeTotals_ButtonClick(sender As Object, e As EventArgs)
        If weekOfDate.Text.Length = 0 Then
            weekOfDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd")
        End If
        startdate = DateTime.Parse(weekOfDate.Text)
        enddate = startdate

        While startdate.DayOfWeek <> DayOfWeek.Monday
            startdate = startdate.AddDays(-1)
        End While
        While enddate.DayOfWeek <> DayOfWeek.Sunday
            enddate = enddate.AddDays(1)
        End While
        enddate = enddate.AddHours(23)
        enddate = enddate.AddMinutes(59)

        payroll_date_range_start.Text = startdate.ToShortDateString()
        payroll_date_range_end.Text = enddate.ToShortDateString()

        EmployeeTotalsGridView.Visible = True
        updateEmployeeTotalsTable_AllEmployeesView()

        EmployeeTotalsGridView.Visible = true
        EmployeeEyeBall_ImageButton.ImageUrl = "../images/eyeball.png"

    End Sub
    Protected Function dateIsHoliday(_date As DateTime) As Boolean

        Dim holidays = New DateTime() {New DateTime, New DateTime, New DateTime, New DateTime, New DateTime, New DateTime}

        '' New Years
        Dim newYearsDate As DateTime = New DateTime(_date.Year, 1, 1)
        holidays(0) = newYearsDate

        '' Memorial Day -- last monday in May 
        Dim memorialDay = New DateTime(_date.Year, 5, 31)
        Dim DayOfWeek = memorialDay.DayOfWeek
        While (DayOfWeek <> DayOfWeek.Monday)
            memorialDay = memorialDay.AddDays(-1)
            DayOfWeek = memorialDay.DayOfWeek
        End While
        holidays(1) = memorialDay

        '' Independence Day
        Dim independenceDay = New DateTime(_date.Year, 7, 4)
        holidays(2) = independenceDay

        '' Labor Day -- 1st Monday in September 
        Dim laborDay = New DateTime(_date.Year, 9, 1)
        DayOfWeek = laborDay.DayOfWeek
        While DayOfWeek <> DayOfWeek.Monday
            laborDay = laborDay.AddDays(1)
            DayOfWeek = laborDay.DayOfWeek
        End While
        holidays(3) = laborDay

        '' Thanksgiving Day -- 4th Thursday in November 
        Dim thanksgiving = (From day In Enumerable.Range(1, 30)
                            Where New DateTime(_date.Year, 11, day).DayOfWeek = DayOfWeek.Thursday
                            Select day).ElementAt(3)
        Dim thanksgivingDay = New DateTime(_date.Year, 11, thanksgiving)
        holidays(4) = thanksgivingDay

        '' Christmas Day 
        Dim christmasDay = New DateTime(_date.Year, 12, 25)
        holidays(5) = christmasDay

        Return holidays.Contains(_date)

    End Function
    Protected Function GetColumnIndexByHeaderText(grid As GridView, findHeader As String) As Integer
        Dim i As Integer = 0
        For i = 0 To grid.Columns.Count - 1
            If grid.Columns(i).HeaderText.ToLower().Trim() = findHeader.ToLower().Trim() Then
                Return i
            End If
        Next

        Return -1
    End Function



    #Region "Cell Value Changed"

    Protected Sub Update_ShiftInstanceTable_ToDB(sender As Object, e As EventArgs)
        Dim shift_id as string = sender.DataItemContainer.Cells(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Shift #")).Text '' TIME::.Text=2024-06-30T22:01,.DataItemContainer.Cells(0).Text=243567,.ID=StartTime_TextBox
        Dim y as string = sender.ToString '' Employee_DDL::.Text=80,SelectedValue=80,SelectedItem.Text=DarnellWilliams,.DataItemContainer.Cells(0).Text=243567,.ID=Username_DDL
        Dim z as string = sender.ToString '' TIME::.Text=Cats are intruders!,.DataItemContainer.Cells(0).Text=243567,.ID=Notes_TextBox

        '' Get the Shift_Instance data of selected row
        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        Dim Change_Made as string = ""
        Dim dt3 As New DataTable()
        Dim sqlCmd3 As New SqlCommand("SELECT startdatetime, enddatetime, shift_notes, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, employee_tbl.username, employee_tbl.cellphone, employee_tbl.employee_id, employee_tbl.ms_id, employee_tbl.excalibur_id FROM client_site_shift_instance_tbl LEFT JOIN employee_tbl ON employee_tbl.employee_id = client_site_shift_instance_tbl.employee_id WHERE client_site_shift_instance_id='"+shift_id+"'", conn)
        Dim sqlDa3 As New SqlDataAdapter(sqlCmd3)
        conn.close()
        conn.open()
        sqlDa3.Fill(dt3)
        conn.close()
        Dim old_startdatetime As string = dt3.Rows(0)("startdatetime")
        Dim old_enddatetime As string = dt3.Rows(0)("enddatetime")
        Dim old_shift_notes As string = dt3.Rows(0)("shift_notes")
        Dim old_firstmeal_startdatetime As string = ""
        Dim old_firstmeal_enddatetime As string = ""
        Dim old_secondmeal_startdatetime As string = ""
        Dim old_secondmeal_enddatetime As string = ""
        Dim old_username As string = "<-- OPEN -->"
        Dim old_employee_id As string = ""
        Dim old_cellphone As string = ""
        Dim old_ms_id As string = ""
        Dim old_excalibur_id As string = ""
        Try
            old_firstmeal_startdatetime = dt3.Rows(0)("firstmeal_startdatetime")
        Catch ex As Exception
        End Try
        Try
            old_firstmeal_enddatetime = dt3.Rows(0)("firstmeal_enddatetime")
        Catch ex As Exception
        End Try
        Try
            old_secondmeal_startdatetime = dt3.Rows(0)("secondmeal_startdatetime")
        Catch ex As Exception
        End Try
        Try
            old_secondmeal_enddatetime = dt3.Rows(0)("secondmeal_enddatetime") 
        Catch ex As Exception 
        End Try
        Try
            old_username = dt3.Rows(0)("username") 
        Catch ex As Exception 
        End Try
        Try
            old_employee_id = dt3.Rows(0)("employee_id")
        Catch ex As Exception 
        End Try
        Try
            old_cellphone = dt3.Rows(0)("cellphone")
        Catch ex As Exception 
        End Try
        Try
            old_ms_id = dt3.Rows(0)("ms_id")
        Catch ex As Exception 
        End Try
        Try
            old_excalibur_id = dt3.Rows(0)("excalibur_id")
        Catch ex As Exception 
        End Try

        '' Variable End Date and Start Date 
        Dim starttime1 As DateTime
        Dim endtime1 As DateTime
        Try
            starttime1 = DateTime.Parse(old_startdatetime)
        Catch ex As Exception 
            Response.Write("<script> alert(""Start Time must be in DateTime format."")</script>")
            updateShiftTable()
            return
        End Try
        Try
            endtime1 = DateTime.Parse(old_enddatetime)
        Catch ex As Exception 
            Response.Write("<script> alert(""End Time must be in DateTime format."")</script>")
            updateShiftTable()
            return
        End Try

        '' Construct Query of user request
        Dim date_time as String = "NULL"
        Select Case sender.ID
            Case "Username_DDL"
                Dim basepayrate as string = ""
                Dim username as string = "<-- OPEN -->"
                if (sender.SelectedValue <> "") Then
                    Dim dt2 As New DataTable()
                    Dim sqlCmd2 As New SqlCommand("SELECT basepayrate, username from employee_tbl WHERE employee_id='"+sender.SelectedValue+"'", conn)
                    Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
                    conn.close()
                    conn.open()
                    sqlDa2.Fill(dt2)
                    conn.close()
                    basepayrate = dt2.Rows(0).ItemArray(0)
                    username = dt2.Rows(0).ItemArray(1)
                Else
                    basepayrate = 0.00
                End If
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET employee_id='"+sender.SelectedValue+"', payrate='"+basepayrate+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s Employee from '"+old_username+"' to '"+username+"'"
            Case "StartTime_TextBox"
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET startdatetime='"+DateTime.Parse(sender.Text).ToString+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s Start Time from '"+old_startdatetime+"' to '"+DateTime.Parse(sender.Text).ToString+"'"
            Case "EndTime_TextBox"
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET enddatetime='"+DateTime.Parse(sender.Text).ToString+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s End Time from '"+old_enddatetime+"' to '"+DateTime.Parse(sender.Text).ToString+"'"
            Case "Notes_TextBox"
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET shift_notes='"+sender.Text+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s Notes from '"+old_shift_notes+"' to '"+sender.Text+"'"
            Case "Phone_TextBox"
                sqlCmd1.CommandText = "UPDATE employee_tbl SET cellphone='"+sender.Text+"' WHERE employee_id='"+old_employee_id+"'"
                Change_Made = Session("user") + " changed Employee "+old_username+"s Phone from '"+old_cellphone+"' to '"+sender.Text+"'"
            Case "ms_id_TextBox"
                sqlCmd1.CommandText = "UPDATE employee_tbl SET ms_id='"+sender.Text+"' WHERE employee_id='"+old_employee_id+"'"
                Change_Made = Session("user") + " changed Employee "+old_username+"s ms_id from '"+old_ms_id+"' to '"+sender.Text+"'"
            Case "ex_id_TextBox"
                sqlCmd1.CommandText = "UPDATE employee_tbl SET excalibur_id='"+sender.Text+"' WHERE employee_id='"+old_employee_id+"'"
                Change_Made = Session("user") + " changed Employee "+old_username+"s excalibur_id from '"+old_excalibur_id+"' to '"+sender.Text+"'"
            Case "FirstMealStartTime_TextBox"
                Try
                    date_time = DateTime.Parse(sender.Text).ToString
                Catch ex As Exception
                End Try
                If (date_time <> "NULL") Then
                    If (date_time <= starttime1 Or date_time >= endtime1) Then 
                        Response.Write("<script> alert(""The following must be true: endtime >= First Meal Start Time >= starttime"")</script>")
                        updateShiftTable()
                        return
                    End If
                End If
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+date_time+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                sqlCmd1.CommandText = sqlCmd1.CommandText.Replace("'NULL'", "NULL")
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s 1st Meal Start Time from '"+old_firstmeal_startdatetime+"' to '"+date_time+"'"
            Case "FirstMealEndTime_TextBox"
                Try
                    date_time = DateTime.Parse(sender.Text).ToString
                Catch ex As Exception
                End Try
                If (date_time <> "NULL") Then
                    If (date_time <= starttime1 Or date_time >= endtime1) Then 
                        Response.Write("<script> alert(""The following must be true: endtime >= First Meal End Time >= starttime"")</script>")
                        updateShiftTable()
                        return
                    End If
                End If
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET firstmeal_enddatetime='"+date_time+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                sqlCmd1.CommandText = sqlCmd1.CommandText.Replace("'NULL'", "NULL")
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s 1st Meal End Time from '"+old_firstmeal_enddatetime+"' to '"+date_time+"'"
            Case "SecondMealStartTime_TextBox"
                Try
                    date_time = DateTime.Parse(sender.Text).ToString
                Catch ex As Exception
                End Try
                If (date_time <> "NULL") Then
                    If (date_time <= starttime1 Or date_time >= endtime1) Then
                        Response.Write("<script> alert(""The following must be true: endtime >= Second Meal Start Time >= starttime"")</script>")
                        updateShiftTable()
                        return
                    End If
                End If
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET secondmeal_startdatetime='"+date_time+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                sqlCmd1.CommandText = sqlCmd1.CommandText.Replace("'NULL'", "NULL")
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s 2nd Meal Start Time from '"+old_secondmeal_startdatetime+"' to '"+date_time+"'"
            Case "SecondMealEndTime_TextBox"
                Try
                    date_time = DateTime.Parse(sender.Text).ToString
                Catch ex As Exception 
                End Try
                If (date_time <> "NULL") Then
                    If (date_time <= starttime1 Or date_time >= endtime1) Then 
                        Response.Write("<script> alert(""The following must be true: endtime >= Second Meal End Time >= starttime"")</script>")
                        updateShiftTable()
                        return
                    End If
                End If
                sqlCmd1.CommandText = "UPDATE client_site_shift_instance_tbl SET secondmeal_enddatetime='"+date_time+"' WHERE client_site_shift_instance_id='"+shift_id+"'"
                sqlCmd1.CommandText = sqlCmd1.CommandText.Replace("'NULL'", "NULL")
                Change_Made = Session("user") + " changed Shift Instance "+shift_id+"s 2nd Meal End Time from '"+old_secondmeal_enddatetime+"' to '"+date_time+"'"
            Case Else
                return
        End Select

        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()
        Log_Change(Session("user"), "Shift Instance", Change_Made, "")

        'Updates ShiftTable
        updateShiftTable()

        'For Each cell as TableCell in ShiftInstanceGridView.Rows(sender.DataItemContainer.RowIndex).Cells
        '    cell.BackCOlor = ColorTranslator.FromHtml(#d9ffff)
        'Next
        'ShiftInstanceGridView.Rows(sender.DataItemContainer.RowIndex).BackColor = ColorTranslator.FromHtml(#d9ffff)
        ShiftInstanceGridView.Rows(sender.DataItemContainer.RowIndex).BorderStyle = BorderStyle.Fixed3D
        ShiftInstanceGridView.Rows(sender.DataItemContainer.RowIndex).BorderWidth = Unit.Pixel(2)
        ShiftInstanceGridView.Rows(sender.DataItemContainer.RowIndex).BorderColor = Color.Black
        ShiftInstanceGridView.SelectedIndex = sender.DataItemContainer.RowIndex

    End Sub
    Protected Sub startdatetime_OnTextChanged(sender As Object, e As EventArgs)
        Dim shift_id as string = sender.DataItemContainer.Cells(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Shift #")).Text

        Dim dt2 As New DataTable()
        Dim sqlCmd2 As New SqlCommand("SELECT enddatetime from client_site_shift_instance_tbl WHERE client_site_shift_instance_id='"+shift_id+"'", conn)
        Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
        conn.close()
        conn.open()
        sqlDa2.Fill(dt2)
        conn.close()
        Dim enddatetime As string = dt2.Rows(0).ItemArray(0)

        '' End Date >= Start Date 
        Dim starttime1 As DateTime
        Dim endtime1 As DateTime
        Try
            starttime1 = DateTime.Parse(sender.Text)
        Catch ex As Exception 
            Response.Write("<script> alert(""Start Time must be chosen."")</script>")
            updateShiftTable()
            return
        End Try
        Try
            endtime1 = DateTime.Parse(enddatetime)
        Catch ex As Exception 
            Response.Write("<script> alert(""End Time must be in DateTime format."")</script>")
            updateShiftTable()
            return
        End Try
        if (starttime1.AddMinutes(1) > endtime1) Then
            Response.Write("<script> alert(""End Time must be at least 1 minute after Start Time!"")</script>")
            updateShiftTable()
            return
        End If

        '' Set Meal Times
        Dim span As Double = endtime1.Ticks - starttime1.Ticks
        Dim fivehours As Double = 10000000L * 60 * 60 * 5
        Dim tenhours As Double = 10000000L * 60 * 60 * 10
        Dim sixteenhours As Double = 10000000L * 60 * 60 * 16
        Dim firstmeal_startdatetime As string = ""
        Dim firstmeal_enddatetime As string = ""
        Dim secondmeal_startdatetime As string = ""
        Dim secondmeal_enddatetime As string = ""
        Dim thirdmeal_startdatetime As string = ""
        Dim thirdmeal_enddatetime As string = ""
        if (span > fivehours) Then
            firstmeal_startdatetime = starttime1.AddHours(4).ToString()
            firstmeal_enddatetime = starttime1.AddHours(4.5).ToString()
        End If
        if (span > tenhours) Then
            secondmeal_startdatetime = starttime1.AddHours(8.5).ToString()
            secondmeal_enddatetime = starttime1.AddHours(9).ToString()
        End If
        if (span > sixteenhours) Then
            thirdmeal_startdatetime = starttime1.AddHours(13).ToString()
            thirdmeal_enddatetime = starttime1.AddHours(13.5).ToString()
        End If
        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("", conn)
        Dim queryString As string = ""
        if (thirdmeal_startdatetime <> "") Then
            queryString = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+firstmeal_startdatetime+"', firstmeal_enddatetime='"+firstmeal_enddatetime+"', secondmeal_startdatetime='"+secondmeal_startdatetime+"', secondmeal_enddatetime='"+secondmeal_enddatetime+"', thirdmeal_startdatetime='"+thirdmeal_startdatetime+"', thirdmeal_enddatetime='"+thirdmeal_enddatetime+"' WHERE client_site_shift_instance_id = '" + shift_id + "'"
        Else if (secondmeal_enddatetime <> "") Then 
            queryString = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+firstmeal_startdatetime+"', firstmeal_enddatetime='"+firstmeal_enddatetime+"', secondmeal_startdatetime='"+secondmeal_startdatetime+"', secondmeal_enddatetime='"+secondmeal_enddatetime+"', thirdmeal_startdatetime=NULL, thirdmeal_enddatetime=NULL WHERE client_site_shift_instance_id = '" + shift_id + "'"
        Else  
            queryString = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+firstmeal_startdatetime+"', firstmeal_enddatetime='"+firstmeal_enddatetime+"', secondmeal_startdatetime=NULL, secondmeal_enddatetime=NULL, thirdmeal_startdatetime=NULL, thirdmeal_enddatetime=NULL WHERE client_site_shift_instance_id = '" + shift_id + "'"
        End If
        sqlCmd1.CommandText = queryString
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        Update_ShiftInstanceTable_ToDB(sender, e)

    End Sub
    Protected Sub enddatetime_OnTextChanged(sender As Object, e As EventArgs)
        Dim shift_id as string = sender.DataItemContainer.Cells(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Shift #")).Text

        Dim dt2 As New DataTable()
        Dim sqlCmd2 As New SqlCommand("SELECT startdatetime from client_site_shift_instance_tbl WHERE client_site_shift_instance_id='"+shift_id+"'", conn)
        Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
        conn.close()
        conn.open()
        sqlDa2.Fill(dt2)
        conn.close()
        Dim startdatetime As string = dt2.Rows(0).ItemArray(0)

        ''End Date >= Start Date 
        Dim starttime1 As DateTime
        Dim endtime1 As DateTime
        Try
            endtime1 = DateTime.Parse(sender.Text)
        Catch ex As Exception 
            Response.Write("<script> alert(""End Time must be chosen."")</script>")
            updateShiftTable()
            return
        End Try
        Try
            starttime1 = DateTime.Parse(startdatetime)
        Catch ex As Exception 
            Response.Write("<script> alert(""Start Time must be in DateTime format."")</script>")
            updateShiftTable()
            return
        End Try
        if (starttime1.AddMinutes(1) > endtime1) Then
            Response.Write("<script> alert(""End Time must be at least 1 minute after Start Time!"")</script>")
            updateShiftTable()
            return
        End If

        '' Set Meal Times
        Dim span As Double = endtime1.Ticks - starttime1.Ticks
        Dim fivehours As Double = 10000000L * 60 * 60 * 5
        Dim tenhours As Double = 10000000L * 60 * 60 * 10
        Dim sixteenhours As Double = 10000000L * 60 * 60 * 16
        Dim firstmeal_startdatetime As string = ""
        Dim firstmeal_enddatetime As string = ""
        Dim secondmeal_startdatetime As string = ""
        Dim secondmeal_enddatetime As string = ""
        Dim thirdmeal_startdatetime As string = ""
        Dim thirdmeal_enddatetime As string = ""
        if (span > fivehours) Then
            firstmeal_startdatetime = starttime1.AddHours(4).ToString()
            firstmeal_enddatetime = starttime1.AddHours(4.5).ToString()
        End If
        if (span > tenhours) Then
            secondmeal_startdatetime = starttime1.AddHours(8.5).ToString()
            secondmeal_enddatetime = starttime1.AddHours(9).ToString()
        End If
        if (span > sixteenhours) Then
            thirdmeal_startdatetime = starttime1.AddHours(13).ToString()
            thirdmeal_enddatetime = starttime1.AddHours(13.5).ToString()
        End If
        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("", conn)
        Dim queryString As string = ""
        if (thirdmeal_startdatetime <> "") Then
            queryString = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+firstmeal_startdatetime+"', firstmeal_enddatetime='"+firstmeal_enddatetime+"', secondmeal_startdatetime='"+secondmeal_startdatetime+"', secondmeal_enddatetime='"+secondmeal_enddatetime+"', thirdmeal_startdatetime='"+thirdmeal_startdatetime+"', thirdmeal_enddatetime='"+thirdmeal_enddatetime+"' WHERE client_site_shift_instance_id='"+shift_id+"'" 
        Else if (secondmeal_enddatetime <> "") Then 
            queryString = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+firstmeal_startdatetime+"', firstmeal_enddatetime='"+firstmeal_enddatetime+"', secondmeal_startdatetime='"+secondmeal_startdatetime+"', secondmeal_enddatetime='"+secondmeal_enddatetime+"', thirdmeal_startdatetime=NULL, thirdmeal_enddatetime=NULL WHERE client_site_shift_instance_id ='"+shift_id+"'"
        Else  
            queryString = "UPDATE client_site_shift_instance_tbl SET firstmeal_startdatetime='"+firstmeal_startdatetime+"', firstmeal_enddatetime='"+firstmeal_enddatetime+"', secondmeal_startdatetime=NULL, secondmeal_enddatetime=NULL, thirdmeal_startdatetime=NULL, thirdmeal_enddatetime=NULL WHERE client_site_shift_instance_id='"+shift_id+"'"
        End If
        sqlCmd1.CommandText = queryString
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub Phone_OnTextChanged(sender As Object, e As EventArgs)
        If (sender.Text.Length >= 20) Then
            Response.Write("<script> alert(""Phone must be less than 20 characters"")</script>")
            updateShiftTable()
            return
        End If

        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub notes_OnTextChanged(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub ms_id_OnTextChanged(sender As Object, e As EventArgs)
        '' Check that ms_id is integer and does not already exist
        If Not Integer.TryParse(sender.Text, 0) Then
            Response.Write("<script> alert(""MS ID must be an Integer!"")</script>")
            UpdateShiftTable()
            return
        End If

        Dim validate As string = "SELECT ms_id FROM employee_tbl WHERE ms_id='" & sender.Text & "'"
        Dim cmd = New SqlCommand(validate, conn)
        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)
        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""That ms_id already exists in database"")</script>")
            UpdateShiftTable()
            Return
        End If

        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub ex_id_OnTextChanged(sender As Object, e As EventArgs)
        '' Check that ms_id is integer and does not already exist
        If Not Integer.TryParse(sender.Text, 0) Then
            Response.Write("<script> alert(""Excalibur ID must be an Integer!"")</script>")
            UpdateShiftTable()
            return
        End If

        Dim validate As string = "SELECT excalibur_id FROM employee_tbl WHERE excalibur_id='" & sender.Text & "'"
        Dim cmd = New SqlCommand(validate, conn)
        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)
        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""That excalibur_id already exists in database"")</script>")
            UpdateShiftTable()
            Return
        End If

        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub firstmeal_startdatetime_OnTextChanged(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub firstmeal_enddatetime_OnTextChanged(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub secondmeal_startdatetime_OnTextChanged(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub secondmeal_enddatetime_OnTextChanged(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub ButtonUpdate_Click(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub SaveButton_Click(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub Username_DLL_SelectedIndexChanged(sender As Object, e As EventArgs)
        Update_ShiftInstanceTable_ToDB(sender, e)
    End Sub
    Protected Sub ms_ex_id_OnTextChanged(sender As Object, e As EventArgs)

    End Sub
    #End Region


    Protected Sub updateShiftTable()
        'Updates ShiftTable
        startdate = DateTime.Parse(weekOfDate.Text)
        enddate = startdate

        While startdate.DayOfWeek <> DayOfWeek.Monday
            startdate = startdate.AddDays(-1)
        End While
        While enddate.DayOfWeek <> DayOfWeek.Sunday
            enddate = enddate.AddDays(1)
        End While
        enddate = enddate.AddHours(23)
        enddate = enddate.AddMinutes(59)

        payroll_date_range_start.Text = startdate.ToShortDateString()
        payroll_date_range_end.Text = enddate.ToShortDateString()

        If employee_ddl.SelectedIndex <> 0 Then
            updateTables("Excalibur", false)
            updateTables("Matt's Staffing", false)
        Else If client_ddl.SelectedIndex <> 0 Then
            updateTables_ByClient("Excalibur", false)
            updateTables_ByClient("Matt's Staffing", false)
        Else
            updateCompanyTotalsTable_AllEmployeesView("Excalibur", false)
            updateCompanyTotalsTable_AllEmployeesView("Matt's Staffing", false)
        End If
    End Sub
    Protected Sub updateTables(company As String, include_totals As boolean)

        '' Fill The shift_instance_Table
        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("SELECT client_site_tbl.client_site_id, client_site_tbl.name, client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_tbl.company, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.payrate, employee_tbl.username, employee_tbl.ms_id, employee_tbl.excalibur_id, employee_tbl.cellphone, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id LEFT JOIN employee_tbl ON client_site_shift_instance_tbl.employee_id = employee_tbl.employee_id WHERE client_site_shift_instance_tbl.employee_id = '" + employee_ddl.SelectedValue + "' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "')) ORDER BY client_site_shift_instance_tbl.startdatetime ASC", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        ShiftInstanceGridView.DataSource = dt1
        ShiftInstanceGridView.DataBind()
        ShiftInstanceGridView.Columns(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Employees")).Visible = false

        ' Calculate Hours in PayPeriod
        Dim r as Int32 = 0
        Dim _startdate = DateTime.Parse(weekOfDate.Text)
        Dim _enddate = startdate
        While _startdate.DayOfWeek <> DayOfWeek.Monday
            _startdate = _startdate.AddDays(-1)
        End While
        While _enddate.DayOfWeek <> DayOfWeek.Sunday
            _enddate = _enddate.AddDays(1)
        End While
        _enddate = _enddate.AddHours(24)
        For Each row As DataRow in dt1.Rows
            Dim startdatetime As DateTime = DateTime.Parse(row.ItemArray(11).ToString) ' Don't Change Query of dt1
            If (startdatetime < _startdate) Then startdatetime = _startdate
            Dim enddatetime As DateTime = DateTime.Parse(row.ItemArray(12).ToString) ' Don't Change Query of dt1
            If (enddatetime > _enddate) Then enddatetime = _enddate
            Dim ticks As Double = enddatetime.Ticks - startdatetime.Ticks
            ShiftInstanceGridView.Rows(r).Cells(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Hours in Pay Period")).Text = TimeSpan.FromTicks(ticks).TotalHours.ToString("N2")
            r += 1
        Next

        ' 1 line per row
        Dim i as Int32 = 0
        while i < ShiftInstanceGridView.Columns.Count 
            ShiftInstanceGridView.Columns(i).ItemStyle.Wrap = false
            i += 1
        End While

        Session("ShiftInstanceGridView") = dt1

        If (Not include_totals) Then return

        '' Fill the totals table
        #Region "Variable Declaration"
        Dim MondayTotalHoursWorked As Double
        Dim TuesdayTotalHoursWorked As Double
        Dim WednesdayTotalHoursWorked As Double
        Dim ThursdayTotalHoursWorked As Double
        Dim FridayTotalHoursWorked As Double
        Dim SaturdayTotalHoursWorked As Double
        Dim SundayTotalHoursWorked As Double
        Dim WeeklyTotalHoursWorked As Double

        Dim MondayTotalRT As Double
        Dim TuesdayTotalRT As Double
        Dim WednesdayTotalRT As Double
        Dim ThursdayTotalRT As Double
        Dim FridayTotalRT As Double
        Dim SaturdayTotalRT As Double
        Dim SundayTotalRT As Double
        Dim MondayTotalOT As Double
        Dim TuesdayTotalOT As Double
        Dim WednesdayTotalOT As Double
        Dim ThursdayTotalOT As Double
        Dim FridayTotalOT As Double
        Dim SaturdayTotalOT As Double
        Dim SundayTotalOT As Double
        Dim MondayTotalDT As Double
        Dim TuesdayTotalDT As Double
        Dim WednesdayTotalDT As Double
        Dim ThursdayTotalDT As Double
        Dim FridayTotalDT As Double
        Dim SaturdayTotalDT As Double
        Dim SundayTotalDT As Double

        Dim MondayTotalOffDutyMealTimeRT As Double
        Dim TuesdayTotalOffDutyMealTimeRT As Double
        Dim WednesdayTotalOffDutyMealTimeRT As Double
        Dim ThursdayTotalOffDutyMealTimeRT As Double
        Dim FridayTotalOffDutyMealTimeRT As Double
        Dim SaturdayTotalOffDutyMealTimeRT As Double
        Dim SundayTotalOffDutyMealTimeRT As Double
        Dim WeeklyTotalOffDutyMealTimeRT As Double
        Dim MondayTotalOffDutyMealTimeOT As Double
        Dim TuesdayTotalOffDutyMealTimeOT As Double
        Dim WednesdayTotalOffDutyMealTimeOT As Double
        Dim ThursdayTotalOffDutyMealTimeOT As Double
        Dim FridayTotalOffDutyMealTimeOT As Double
        Dim SaturdayTotalOffDutyMealTimeOT As Double
        Dim SundayTotalOffDutyMealTimeOT As Double
        Dim WeeklyTotalOffDutyMealTimeOT As Double
        Dim MondayTotalOffDutyMealTimeDT As Double
        Dim TuesdayTotalOffDutyMealTimeDT As Double
        Dim WednesdayTotalOffDutyMealTimeDT As Double
        Dim ThursdayTotalOffDutyMealTimeDT As Double
        Dim FridayTotalOffDutyMealTimeDT As Double
        Dim SaturdayTotalOffDutyMealTimeDT As Double
        Dim SundayTotalOffDutyMealTimeDT As Double
        Dim WeeklyTotalOffDutyMealTimeDT As Double
        Dim MondayTotalOffDutyMealTime As Double
        Dim TuesdayTotalOffDutyMealTime As Double
        Dim WednesdayTotalOffDutyMealTime As Double
        Dim ThursdayTotalOffDutyMealTime As Double
        Dim FridayTotalOffDutyMealTime As Double
        Dim SaturdayTotalOffDutyMealTime As Double
        Dim SundayTotalOffDutyMealTime As Double
        Dim WeeklyTotalOffDutyMealTime As Double
        #End Region

        Dim dt As New DataTable()
        Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE company='"+company.Replace("'","''")+"' AND employee_id = '"+employee_ddl.SelectedValue+"' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
        Dim sqlDa As New SqlDataAdapter(sqlCmd)
        conn.close()
        conn.open()
        sqlDa.Fill(dt)
        conn.close()

        Dim _date_ As DateTime
        If weekOfDate.Text.Length > 0 Then
            _date_ = DateTime.Parse(weekOfDate.Text)
            While _date_.DayOfWeek <> DayOfWeek.Monday
                _date_ = _date_.AddDays(-1)
            End While
        End If

        #Region "For Each Employee"
        For Each row In dt.Rows
            Dim wholespan As Double
            Dim firstspan As Double
            Dim secondspan As Double
            Dim startdatetime = DateTime.Parse(row(4))
            Dim enddatetime = DateTime.Parse(row(5))

            '' Figure total hours worked by day of week
            If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                Select Case startdatetime.DayOfWeek
                    Case DayOfWeek.Sunday
                        If enddatetime.Date = _date_.Date Then
                            MondayTotalHoursWorked += secondspan
                        Else
                            SundayTotalHoursWorked += firstspan
                        End If
                    Case DayOfWeek.Monday
                        MondayTotalHoursWorked += firstspan
                        TuesdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Tuesday
                        TuesdayTotalHoursWorked += firstspan
                        WednesdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Wednesday
                        WednesdayTotalHoursWorked += firstspan
                        ThursdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Thursday
                        ThursdayTotalHoursWorked += firstspan
                        FridayTotalHoursWorked += secondspan
                    Case DayOfWeek.Friday
                        FridayTotalHoursWorked += firstspan
                        SaturdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Saturday
                        SaturdayTotalHoursWorked += firstspan
                        SundayTotalHoursWorked += secondspan
                End Select
            Else
                wholespan = enddatetime.Ticks - startdatetime.Ticks
                Select Case startdatetime.DayOfWeek
                    Case DayOfWeek.Sunday
                        SundayTotalHoursWorked += wholespan
                    Case DayOfWeek.Monday
                        MondayTotalHoursWorked += wholespan
                    Case DayOfWeek.Tuesday
                        TuesdayTotalHoursWorked += wholespan
                    Case DayOfWeek.Wednesday
                        WednesdayTotalHoursWorked += wholespan
                    Case DayOfWeek.Thursday
                        ThursdayTotalHoursWorked += wholespan
                    Case DayOfWeek.Friday
                        FridayTotalHoursWorked += wholespan
                    Case DayOfWeek.Saturday
                        SaturdayTotalHoursWorked += wholespan
                End Select
            End If

            '' Figure total off duty lunch time by day of week
            If row(7) = False Then
                Dim firstmeal_startdatetime As DateTime
                Dim firstmeal_enddatetime As DateTime
                Dim secondmeal_startdatetime As DateTime
                Dim secondmeal_enddatetime As DateTime
                Dim thirdmeal_startdatetime As DateTime
                Dim thirdmeal_enddatetime As DateTime

                Try
                    firstmeal_startdatetime = DateTime.Parse(row(8))
                Catch ex As Exception
                    firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    firstmeal_enddatetime = DateTime.Parse(row(9))
                Catch ex As Exception
                    firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    secondmeal_startdatetime = DateTime.Parse(row(10))
                Catch ex As Exception
                    secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    secondmeal_enddatetime = DateTime.Parse(row(11))
                Catch ex As Exception
                    secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    thirdmeal_startdatetime = DateTime.Parse(row(12))
                Catch ex As Exception
                    thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    thirdmeal_enddatetime = DateTime.Parse(row(13))
                Catch ex As Exception
                    thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try

                If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                    firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                    secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                    Select Case firstmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If enddatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeRT += secondspan
                            Else
                                SundayTotalOffDutyMealTimeRT += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalOffDutyMealTimeRT += firstspan
                            TuesdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeRT += firstspan
                            WednesdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeRT += firstspan
                            ThursdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeRT += firstspan
                            FridayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeRT += firstspan
                            SaturdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeRT += firstspan
                            SundayTotalOffDutyMealTimeRT += secondspan
                    End Select
                Else
                    wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                    Select Case firstmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If firstmeal_enddatetime.Date > _date_.Date Then
                                SundayTotalOffDutyMealTimeRT += wholespan
                            End If
                        Case DayOfWeek.Monday
                            If firstmeal_startdatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeRT += wholespan
                            End If
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeRT += wholespan
                    End Select
                End If
                If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                    firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                    secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                    Select Case secondmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If secondmeal_enddatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeOT += secondspan
                            Else
                                SundayTotalOffDutyMealTimeOT += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalOffDutyMealTimeOT += firstspan
                            TuesdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeOT += firstspan
                            WednesdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeOT += firstspan
                            ThursdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeOT += firstspan
                            FridayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeOT += firstspan
                            SaturdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeOT += firstspan
                            SundayTotalOffDutyMealTimeOT += secondspan
                    End Select
                Else
                    wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                    Select Case secondmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If secondmeal_enddatetime.Date > _date_.Date Then
                                SundayTotalOffDutyMealTimeOT += wholespan
                            End If
                        Case DayOfWeek.Monday
                            If secondmeal_startdatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeOT += wholespan
                            End If
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeOT += wholespan
                    End Select
                End If
                If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                    firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                    secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                    Select Case thirdmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If thirdmeal_enddatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeDT += secondspan
                            Else
                                SundayTotalOffDutyMealTimeDT += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalOffDutyMealTimeDT += firstspan
                            TuesdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeDT += firstspan
                            WednesdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeDT += firstspan
                            ThursdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeDT += firstspan
                            FridayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeDT += firstspan
                            SaturdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeDT += firstspan
                            SundayTotalOffDutyMealTimeDT += secondspan
                    End Select
                Else
                    wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                    Select Case thirdmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If thirdmeal_enddatetime.Date > _date_.Date Then
                                SundayTotalOffDutyMealTimeDT += wholespan
                            End If
                        Case DayOfWeek.Monday
                            If thirdmeal_startdatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeDT += wholespan
                            End If
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeDT += wholespan
                    End Select
                End If
            End If
        Next
        #End Region

        WeeklyTotalHoursWorked = MondayTotalHoursWorked + TuesdayTotalHoursWorked + WednesdayTotalHoursWorked + ThursdayTotalHoursWorked + FridayTotalHoursWorked + SaturdayTotalHoursWorked + SundayTotalHoursWorked

        MondayTotalOffDutyMealTime = MondayTotalOffDutyMealTimeRT + MondayTotalOffDutyMealTimeOT + MondayTotalOffDutyMealTimeDT
        TuesdayTotalOffDutyMealTime = TuesdayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeDT
        WednesdayTotalOffDutyMealTime = WednesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeDT
        ThursdayTotalOffDutyMealTime = ThursdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeDT
        FridayTotalOffDutyMealTime = FridayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeDT
        SaturdayTotalOffDutyMealTime = SaturdayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeDT
        SundayTotalOffDutyMealTime = SundayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeDT

        Dim halfhour = 10000000L * 60 * 60 * 0.5
        Dim onehour = 10000000L * 60 * 60 * 1
        Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5

        If MondayTotalOffDutyMealTime <= halfhour Then
            MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTime
            MondayTotalOffDutyMealTimeOT = 0
            MondayTotalOffDutyMealTimeDT = 0
        ElseIf MondayTotalOffDutyMealTime > halfhour And MondayTotalOffDutyMealTime <= onehour Then
            MondayTotalOffDutyMealTimeRT = halfhour
            MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTime - halfhour
            MondayTotalOffDutyMealTimeDT = 0
        ElseIf MondayTotalOffDutyMealTime > onehour And MondayTotalOffDutyMealTime <= oneandhalfhour Then
            MondayTotalOffDutyMealTimeRT = halfhour
            MondayTotalOffDutyMealTimeOT = halfhour
            MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTime - onehour
        ElseIf MondayTotalOffDutyMealTime > oneandhalfhour Then
            MondayTotalOffDutyMealTimeRT = halfhour
            MondayTotalOffDutyMealTimeOT = halfhour
            MondayTotalOffDutyMealTimeDT = halfhour
        End If
        If TuesdayTotalOffDutyMealTime <= halfhour Then
            TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTime
            TuesdayTotalOffDutyMealTimeOT = 0
            TuesdayTotalOffDutyMealTimeDT = 0
        ElseIf TuesdayTotalOffDutyMealTime > halfhour And TuesdayTotalOffDutyMealTime <= onehour Then
            TuesdayTotalOffDutyMealTimeRT = halfhour
            TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTime - halfhour
            TuesdayTotalOffDutyMealTimeDT = 0
        ElseIf TuesdayTotalOffDutyMealTime > onehour And TuesdayTotalOffDutyMealTime <= oneandhalfhour Then
            TuesdayTotalOffDutyMealTimeRT = halfhour
            TuesdayTotalOffDutyMealTimeOT = halfhour
            TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTime - onehour
        ElseIf TuesdayTotalOffDutyMealTime > oneandhalfhour Then
            TuesdayTotalOffDutyMealTimeRT = halfhour
            TuesdayTotalOffDutyMealTimeOT = halfhour
            TuesdayTotalOffDutyMealTimeDT = halfhour
        End If
        If WednesdayTotalOffDutyMealTime <= halfhour Then
            WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTime
            WednesdayTotalOffDutyMealTimeOT = 0
            WednesdayTotalOffDutyMealTimeDT = 0
        ElseIf WednesdayTotalOffDutyMealTime > halfhour And WednesdayTotalOffDutyMealTime <= onehour Then
            WednesdayTotalOffDutyMealTimeRT = halfhour
            WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTime - halfhour
            WednesdayTotalOffDutyMealTimeDT = 0
        ElseIf WednesdayTotalOffDutyMealTime > onehour And WednesdayTotalOffDutyMealTime <= oneandhalfhour Then
            WednesdayTotalOffDutyMealTimeRT = halfhour
            WednesdayTotalOffDutyMealTimeOT = halfhour
            WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTime - onehour
        ElseIf WednesdayTotalOffDutyMealTime > oneandhalfhour Then
            WednesdayTotalOffDutyMealTimeRT = halfhour
            WednesdayTotalOffDutyMealTimeOT = halfhour
            WednesdayTotalOffDutyMealTimeDT = halfhour
        End If
        If ThursdayTotalOffDutyMealTime <= halfhour Then
            ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTime
            ThursdayTotalOffDutyMealTimeOT = 0
            ThursdayTotalOffDutyMealTimeDT = 0
        ElseIf WednesdayTotalOffDutyMealTime > halfhour And ThursdayTotalOffDutyMealTime <= onehour Then
            ThursdayTotalOffDutyMealTimeRT = halfhour
            ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTime - halfhour
            ThursdayTotalOffDutyMealTimeDT = 0
        ElseIf ThursdayTotalOffDutyMealTime > onehour And ThursdayTotalOffDutyMealTime <= oneandhalfhour Then
            ThursdayTotalOffDutyMealTimeRT = halfhour
            ThursdayTotalOffDutyMealTimeOT = halfhour
            ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTime - onehour
        ElseIf ThursdayTotalOffDutyMealTime > oneandhalfhour Then
            ThursdayTotalOffDutyMealTimeRT = halfhour
            ThursdayTotalOffDutyMealTimeOT = halfhour
            ThursdayTotalOffDutyMealTimeDT = halfhour
        End If
        If FridayTotalOffDutyMealTime <= halfhour Then
            FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTime
            FridayTotalOffDutyMealTimeOT = 0
            FridayTotalOffDutyMealTimeDT = 0
        ElseIf FridayTotalOffDutyMealTime > halfhour And FridayTotalOffDutyMealTime <= onehour Then
            FridayTotalOffDutyMealTimeRT = halfhour
            FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTime - halfhour
            FridayTotalOffDutyMealTimeDT = 0
        ElseIf FridayTotalOffDutyMealTime > onehour And FridayTotalOffDutyMealTime <= oneandhalfhour Then
            FridayTotalOffDutyMealTimeRT = halfhour
            FridayTotalOffDutyMealTimeOT = halfhour
            FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTime - onehour
        ElseIf FridayTotalOffDutyMealTime > oneandhalfhour Then
            FridayTotalOffDutyMealTimeRT = halfhour
            FridayTotalOffDutyMealTimeOT = halfhour
            FridayTotalOffDutyMealTimeDT = halfhour
        End If
        If SaturdayTotalOffDutyMealTime <= halfhour Then
            SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTime
            SaturdayTotalOffDutyMealTimeOT = 0
            SaturdayTotalOffDutyMealTimeDT = 0
        ElseIf SaturdayTotalOffDutyMealTime > halfhour And SaturdayTotalOffDutyMealTime <= onehour Then
            SaturdayTotalOffDutyMealTimeRT = halfhour
            SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTime - halfhour
            SaturdayTotalOffDutyMealTimeDT = 0
        ElseIf SaturdayTotalOffDutyMealTime > onehour And SaturdayTotalOffDutyMealTime <= oneandhalfhour Then
            SaturdayTotalOffDutyMealTimeRT = halfhour
            SaturdayTotalOffDutyMealTimeOT = halfhour
            SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTime - onehour
        ElseIf SaturdayTotalOffDutyMealTime > oneandhalfhour Then
            SaturdayTotalOffDutyMealTimeRT = halfhour
            SaturdayTotalOffDutyMealTimeOT = halfhour
            SaturdayTotalOffDutyMealTimeDT = halfhour
        End If
        If SundayTotalOffDutyMealTime <= halfhour Then
            SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTime
            SundayTotalOffDutyMealTimeOT = 0
            SundayTotalOffDutyMealTimeDT = 0
        ElseIf SundayTotalOffDutyMealTime > halfhour And SundayTotalOffDutyMealTime <= onehour Then
            SundayTotalOffDutyMealTimeRT = halfhour
            SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTime - halfhour
            SundayTotalOffDutyMealTimeDT = 0
        ElseIf SundayTotalOffDutyMealTime > onehour And SundayTotalOffDutyMealTime <= oneandhalfhour Then
            SundayTotalOffDutyMealTimeRT = halfhour
            SundayTotalOffDutyMealTimeOT = halfhour
            SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTime - onehour
        ElseIf SundayTotalOffDutyMealTime > oneandhalfhour Then
            SundayTotalOffDutyMealTimeRT = halfhour
            SundayTotalOffDutyMealTimeOT = halfhour
            SundayTotalOffDutyMealTimeDT = halfhour
        End If

        WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
        WeeklyTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeOT
        WeeklyTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + TuesdayTotalOffDutyMealTimeDT + WednesdayTotalOffDutyMealTimeDT + ThursdayTotalOffDutyMealTimeDT + FridayTotalOffDutyMealTimeDT + SaturdayTotalOffDutyMealTimeDT + SundayTotalOffDutyMealTimeDT

        Dim fourhours = 10000000L * 60 * 60 * 4
        Dim eighthours = 10000000L * 60 * 60 * 8
        Dim twelvehours = 10000000L * 60 * 60 * 12
        Dim fourtyhours = 10000000L * 60 * 60 * 40
        Dim WeeklyTotalRegularTime = 0L
        Dim WeeklyTotalOverTime = 0L
        Dim WeeklyTotalDoubleTime = 0L

        If MondayTotalHoursWorked >= twelvehours Then
            MondayTotalDT = MondayTotalHoursWorked - twelvehours
            MondayTotalOT = fourhours
            MondayTotalRT = eighthours
        ElseIf MondayTotalHoursWorked >= eighthours Then
            MondayTotalOT = MondayTotalHoursWorked - eighthours
            MondayTotalRT = eighthours
        Else
            MondayTotalRT = MondayTotalHoursWorked
        End If
        If TuesdayTotalHoursWorked >= twelvehours Then
            TuesdayTotalDT = TuesdayTotalHoursWorked - twelvehours
            TuesdayTotalOT = fourhours
            TuesdayTotalRT = eighthours
        ElseIf TuesdayTotalHoursWorked >= eighthours Then
            TuesdayTotalOT = TuesdayTotalHoursWorked - eighthours
            TuesdayTotalRT = eighthours
        Else
            TuesdayTotalRT = TuesdayTotalHoursWorked
        End If
        If WednesdayTotalHoursWorked >= twelvehours Then
            WednesdayTotalDT = WednesdayTotalHoursWorked - twelvehours
            WednesdayTotalOT = fourhours
            WednesdayTotalRT = eighthours
        ElseIf WednesdayTotalHoursWorked >= eighthours Then
            WednesdayTotalOT = WednesdayTotalHoursWorked - eighthours
            WednesdayTotalRT = eighthours
        Else
            WednesdayTotalRT = WednesdayTotalHoursWorked
        End If
        If ThursdayTotalHoursWorked >= twelvehours Then
            ThursdayTotalDT = ThursdayTotalHoursWorked - twelvehours
            ThursdayTotalOT = fourhours
            ThursdayTotalRT = eighthours
        ElseIf ThursdayTotalHoursWorked >= eighthours Then
            ThursdayTotalOT = ThursdayTotalHoursWorked - eighthours
            ThursdayTotalRT = eighthours
        Else
            ThursdayTotalRT = ThursdayTotalHoursWorked
        End If
        If FridayTotalHoursWorked >= twelvehours Then
            FridayTotalDT = FridayTotalHoursWorked - twelvehours
            FridayTotalOT = fourhours
            FridayTotalRT = eighthours
        ElseIf FridayTotalHoursWorked >= eighthours Then
            FridayTotalOT = FridayTotalHoursWorked - eighthours
            FridayTotalRT = eighthours
        Else
            FridayTotalRT = FridayTotalHoursWorked
        End If
        If SaturdayTotalHoursWorked >= twelvehours Then
            SaturdayTotalDT = SaturdayTotalHoursWorked - twelvehours
            SaturdayTotalOT = fourhours
            SaturdayTotalRT = eighthours
        ElseIf SaturdayTotalHoursWorked >= eighthours Then
            SaturdayTotalOT = SaturdayTotalHoursWorked - eighthours
            SaturdayTotalRT = eighthours
        Else
            SaturdayTotalRT = SaturdayTotalHoursWorked
        End If
        If SundayTotalHoursWorked >= twelvehours Then
            SundayTotalDT = SundayTotalHoursWorked - twelvehours
            SundayTotalOT = fourhours
            SundayTotalRT = eighthours
        ElseIf SundayTotalHoursWorked >= eighthours Then
            SundayTotalOT = SundayTotalHoursWorked - eighthours
            SundayTotalRT = eighthours
        Else
            SundayTotalRT = SundayTotalHoursWorked
        End If

        WeeklyTotalRegularTime = MondayTotalRT + TuesdayTotalRT + WednesdayTotalRT + ThursdayTotalRT + FridayTotalRT + SaturdayTotalRT + SundayTotalRT
        WeeklyTotalOverTime = MondayTotalOT + TuesdayTotalOT + WednesdayTotalOT + ThursdayTotalOT + FridayTotalOT + SaturdayTotalOT + SundayTotalOT
        WeeklyTotalDoubleTime = MondayTotalDT + TuesdayTotalDT + WednesdayTotalDT + ThursdayTotalDT + FridayTotalDT + SaturdayTotalDT + SundayTotalDT

        If WeeklyTotalHoursWorked - fourtyhours > WeeklyTotalOverTime Then
            WeeklyTotalOverTime = WeeklyTotalHoursWorked - fourtyhours - WeeklyTotalDoubleTime
            WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalOverTime
        End If

        '' Calculate HT and HT_OffDutyMeal for each day of week
        Dim MondayTotalHT As Double
        Dim TuesdayTotalHT As Double
        Dim WednesdayTotalHT As Double
        Dim ThursdayTotalHT As Double
        Dim FridayTotalHT As Double
        Dim SaturdayTotalHT As Double
        Dim SundayTotalHT As Double
        Dim WeeklyTotalHolidayTime As Double
        Dim MondayTotalOffDutyMealTimeHT As Double
        Dim TuesdayTotalOffDutyMealTimeHT As Double
        Dim WednesdayTotalOffDutyMealTimeHT As Double
        Dim ThursdayTotalOffDutyMealTimeHT As Double
        Dim FridayTotalOffDutyMealTimeHT As Double
        Dim SaturdayTotalOffDutyMealTimeHT As Double
        Dim SundayTotalOffDutyMealTimeHT As Double
        Dim WeeklyTotalOffDutyMealTimeHT As Double
        Dim _date As DateTime
        If weekOfDate.Text.Length > 0 Then
            _date = DateTime.Parse(weekOfDate.Text)
            While _date.DayOfWeek <> DayOfWeek.Monday
                _date = _date.AddDays(-1)
            End While
        End If
        If dateIsHoliday(_date) Then
            MondayTotalHT = MondayTotalRT
            MondayTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeRT
            MondayTotalRT = 0
            MondayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            TuesdayTotalHT = TuesdayTotalRT
            TuesdayTotalOffDutyMealTimeHT = TuesdayTotalOffDutyMealTimeRT
            TuesdayTotalRT = 0
            TuesdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            WednesdayTotalHT = WednesdayTotalRT
            WednesdayTotalOffDutyMealTimeHT = WednesdayTotalOffDutyMealTimeRT
            WednesdayTotalRT = 0
            WednesdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            ThursdayTotalHT = ThursdayTotalRT
            ThursdayTotalOffDutyMealTimeHT = ThursdayTotalOffDutyMealTimeRT
            ThursdayTotalRT = 0
            ThursdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            FridayTotalHT = FridayTotalRT
            FridayTotalOffDutyMealTimeHT = FridayTotalOffDutyMealTimeRT
            FridayTotalRT = 0
            FridayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            SaturdayTotalHT = SaturdayTotalRT
            SaturdayTotalOffDutyMealTimeHT = SaturdayTotalOffDutyMealTimeRT
            SaturdayTotalRT = 0
            SaturdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            SundayTotalHT = SundayTotalRT
            SundayTotalOffDutyMealTimeHT = SundayTotalOffDutyMealTimeRT
            SundayTotalRT = 0
            SundayTotalOffDutyMealTimeRT = 0
        End If
        WeeklyTotalHolidayTime = MondayTotalHT + TuesdayTotalHT + WednesdayTotalHT + ThursdayTotalHT + FridayTotalHT + SaturdayTotalHT + SundayTotalHT
        WeeklyTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeHT + TuesdayTotalOffDutyMealTimeHT + WednesdayTotalOffDutyMealTimeHT + ThursdayTotalOffDutyMealTimeHT + FridayTotalOffDutyMealTimeHT + SaturdayTotalOffDutyMealTimeHT + SundayTotalOffDutyMealTimeHT


        Dim MondayTotalHoursWorkedMinusLunches As Double = MondayTotalHoursWorked - MondayTotalOffDutyMealTime
        Dim TuesdayTotalHoursWorkedMinusLunches As Double = TuesdayTotalHoursWorked - TuesdayTotalOffDutyMealTime
        Dim WednesdayTotalHoursWorkedMinusLunches As Double = WednesdayTotalHoursWorked - WednesdayTotalOffDutyMealTime
        Dim ThursdayTotalHoursWorkedMinusLunches As Double = ThursdayTotalHoursWorked - ThursdayTotalOffDutyMealTime
        Dim FridayTotalHoursWorkedMinusLunches As Double = FridayTotalHoursWorked - FridayTotalOffDutyMealTime
        Dim SaturdayTotalHoursWorkedMinusLunches As Double = SaturdayTotalHoursWorked - SaturdayTotalOffDutyMealTime
        Dim SundayTotalHoursWorkedMinusLunches As Double = SundayTotalHoursWorked - SundayTotalOffDutyMealTime

        '' Subtract HT from RT
        WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalHolidayTime
        '' Recalculate Total MealTimes RT 
        WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
        '' Calculate Total MealTime
        WeeklyTotalOffDutyMealTime = WeeklyTotalOffDutyMealTimeRT + WeeklyTotalOffDutyMealTimeOT + WeeklyTotalOffDutyMealTimeDT + WeeklyTotalOffDutyMealTimeHT
        '' Calculate WeeklyTotalHoursWorked Minus Lunches
        Dim WeeklyTotalHoursWorkedMinusLunches As Double = WeeklyTotalHoursWorked - WeeklyTotalOffDutyMealTime
        Dim WeeklyTotalRegularTimeMinusLunches As Double = WeeklyTotalRegularTime - WeeklyTotalOffDutyMealTimeRT
        Dim WeeklyTotalHolidayTimeMinusLunches As Double = WeeklyTotalHolidayTime - WeeklyTotalOffDutyMealTimeHT
        Dim WeeklyTotalOverTimeMinusLunches As Double = WeeklyTotalOverTime - WeeklyTotalOffDutyMealTimeOT
        Dim WeeklyTotalDoubleTimeMinusLunches As Double = WeeklyTotalDoubleTime - WeeklyTotalOffDutyMealTimeDT

        '' Get the PayRate
        '' Fill The shift_instance_Table
        dt1 = New DataTable()
        sqlCmd1 = New SqlCommand("SELECT basepayrate FROM employee_tbl WHERE employee_id='"+employee_ddl.SelectedValue+"'", conn)
        sqlDa1 = New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        ' Calculate Dollars to be paid
        Dim basepayrate As Double = Convert.ToInt64(Decimal.Parse(dt1.Rows(0).Item(0).ToString()))
        Dim WeeklyTotalDollars As Double = (TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours*2.0) * (basepayrate)
        Dim WeeklyTotalDollarsMinusLunches As Double = (TimeSpan.FromTicks(WeeklyTotalRegularTimeMinuslunches).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinuslunches).TotalHours*2.0) * (basepayrate)
        Dim WeeklyTotalDollarsOffDutyMealTime As Double = (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours*2.0) * (basepayrate)

        'Create datatable and columns
        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("rowtitle"))
        dtable.Columns.Add(New DataColumn("mondaytime"))
        dtable.Columns.Add(New DataColumn("tuesdaytime"))
        dtable.Columns.Add(New DataColumn("wednesdaytime"))
        dtable.Columns.Add(New DataColumn("thursdaytime"))
        dtable.Columns.Add(New DataColumn("fridaytime"))
        dtable.Columns.Add(New DataColumn("saturdaytime"))
        dtable.Columns.Add(New DataColumn("sundaytime"))
        dtable.Columns.Add(New DataColumn("all_total"))
        dtable.Columns.Add(New DataColumn("rt_total"))
        dtable.Columns.Add(New DataColumn("ht_total"))
        dtable.Columns.Add(New DataColumn("ot_total"))
        dtable.Columns.Add(New DataColumn("dt_total"))
        dtable.Columns.Add(New DataColumn("totaldollars"))
        'ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, RT)).Visible = True
        'ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, HT)).Visible = True
        'ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, OT)).Visible = True
        'ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, DT)).Visible = True
        'ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, $ Total)).Visible = True
        'MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, RT)).Visible = True
        'MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, HT)).Visible = True
        'MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, OT)).Visible = True
        'MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, DT)).Visible = True
        'MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, $ Total)).Visible = True

        'If WeeklyTotalHoursWorked = 0 Then
        '    If company = Excalibur Then
        '        ExcaliburTotalsGridView.Visible = False
        '    Else 
        '        MattsTotalsGridView.Visible = False
        '    End If
        'Else
        '    If company = Excalibur Then
        '        ExcaliburTotalsGridView.Visible = True
        '    Else 
        '        MattsTotalsGridView.Visible = True
        '    End If
        'End If

        'Add Rows
        Dim RowValues As Object() = {"","","","","","","","","","","","","",""}
        RowValues(0) = "Total Hours"
        RowValues(1) = If((MondayTotalHoursWorked) = 0L, "0", TimeSpan.FromTicks(MondayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(FridayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(SundayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(9) = If(WeeklyTotalRegularTime = 0L, "0", TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours.ToString("N2"))
        RowValues(10) = If(WeeklyTotalHolidayTime = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours.ToString("N2"))
        RowValues(11) = If(WeeklyTotalOverTime = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours.ToString("N2"))
        RowValues(12) = If(WeeklyTotalDoubleTime = 0L, "0", TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours.ToString("N2"))
        RowValues(13) = If(WeeklyTotalDollars = 0L, "0", "$ "+WeeklyTotalDollars.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()
        RowValues(0) = "On Duty Hours Worked"
        RowValues(1) = If((MondayTotalHoursWorkedMinusLunches) = 0L, "0", TimeSpan.FromTicks(MondayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(FridayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(SundayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(9) = If(WeeklyTotalRegularTimeMinusLunches = 0L, "0", TimeSpan.FromTicks(WeeklyTotalRegularTimeMinusLunches).TotalHours.ToString("N2"))
        RowValues(10) = If(WeeklyTotalHolidayTimeMinusLunches = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinusLunches).TotalHours.ToString("N2"))
        RowValues(11) = If(WeeklyTotalOverTimeMinusLunches = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOverTimeMinusLunches).TotalHours.ToString("N2"))
        RowValues(12) = If(WeeklyTotalDoubleTimeMinusLunches = 0L, "0", TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinusLunches).TotalHours.ToString("N2"))
        RowValues(13) = If(WeeklyTotalDollarsMinusLunches = 0L, "0", "$ "+WeeklyTotalDollarsMinuslunches.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()
        RowValues(0) = "Off Duty Meal Time"
        RowValues(1) = If(MondayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(MondayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(TuesdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(WednesdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(ThursdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(FridayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(SaturdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(SundayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(9) = If(WeeklyTotalOffDutyMealTimeRT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours.ToString("N2"))
        RowValues(10) = If(WeeklyTotalOffDutyMealTimeHT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours.ToString("N2"))
        RowValues(11) = If(WeeklyTotalOffDutyMealTimeOT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours.ToString("N2"))
        RowValues(12) = If(WeeklyTotalOffDutyMealTimeDT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours.ToString("N2"))
        RowValues(13) = If(WeeklyTotalDollarsOffDutyMealTime = 0L, "0", "$ "+WeeklyTotalDollarsOffDutyMealTime.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()

        'now bind datatable to gridview... 
        'If company = Excalibur Then
        '    ExcaliburTotalsGridView.DataSource = dtable
        '    ExcaliburTotalsGridView.DataBind()
        'Else
        '    MattsTotalsGridView.DataSource = dtable
        '    MattsTotalsGridView.DataBind()
        'End If

    End Sub
    Protected Sub updateTables_byClient(company As String, include_totals As boolean)

        '' Fill The shift_instance_Table
        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("SELECT client_site_tbl.client_site_id, client_site_tbl.name, client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_tbl.company, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.payrate, employee_tbl.username, employee_tbl.ms_id, employee_tbl.excalibur_id, employee_tbl.cellphone, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id LEFT JOIN employee_tbl ON client_site_shift_instance_tbl.employee_id = employee_tbl.employee_id WHERE client_site_shift_instance_tbl.client_site_id = '" + client_ddl.SelectedValue + "' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "')) ORDER BY employee_tbl.username ASC, client_site_shift_instance_tbl.startdatetime ASC", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        ShiftInstanceGridView.DataSource = dt1
        ShiftInstanceGridView.DataBind()
        ShiftInstanceGridView.Columns(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Employees")).Visible = true

        ' Calculate Hours in PayPeriod
        Dim r as Int32 = 0
        Dim _startdate = DateTime.Parse(weekOfDate.Text)
        Dim _enddate = startdate
        While _startdate.DayOfWeek <> DayOfWeek.Monday
            _startdate = _startdate.AddDays(-1)
        End While
        While _enddate.DayOfWeek <> DayOfWeek.Sunday
            _enddate = _enddate.AddDays(1)
        End While
        _enddate = _enddate.AddHours(24)
        For Each row As DataRow in dt1.Rows
            Dim startdatetime As DateTime = DateTime.Parse(row.ItemArray(11).ToString) ' Don't Change Query of dt1
            If (startdatetime < _startdate) Then startdatetime = _startdate
            Dim enddatetime As DateTime = DateTime.Parse(row.ItemArray(12).ToString) ' Don't Change Query of dt1
            If (enddatetime > _enddate) Then enddatetime = _enddate
            Dim ticks As Double = enddatetime.Ticks - startdatetime.Ticks
            ShiftInstanceGridView.Rows(r).Cells(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Hours in Pay Period")).Text = TimeSpan.FromTicks(ticks).TotalHours.ToString("N2")
            r += 1
        Next

        ' Populate the Employee_DDL
        Dim dt2 As New DataTable()
        Dim sqlCmd2 As New SqlCommand("SELECT username, employee_id username FROM employee_tbl WHERE status='Active' ORDER BY employee_tbl.username ASC", conn)
        Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
        conn.close()
        conn.open()
        sqlDa2.Fill(dt2)
        conn.close()
        r = 0
        For Each row in ShiftInstanceGridView.Rows
            Dim ddl As DropDownList = row.FindControl("Username_DDL")
            ddl.DataSource = dt2
            ddl.DataTextField = dt2.Columns(0).ToString()
            ddl.DataValueField = dt2.Columns(1).ToString()
            ddl.DataBind()
            ddl.Items.Insert(0, new ListItem("<--     OPEN     -->", ""))
            ddl.SelectedValue = dt1.Rows(r).ItemArray(4).ToString
            r += 1
        Next

        ' 1 line per row
        Dim i as Int32 = 0
        while i < ShiftInstanceGridView.Columns.Count 
            ShiftInstanceGridView.Columns(i).ItemStyle.Wrap = false
            i += 1
        End While

        Session("ShiftInstanceGridView") = dt1

        If (Not include_totals) Then return

        '' Fill the totals table
        #Region "Variable Declaration"
        Dim MondayTotalHoursWorked As Double
        Dim TuesdayTotalHoursWorked As Double
        Dim WednesdayTotalHoursWorked As Double
        Dim ThursdayTotalHoursWorked As Double
        Dim FridayTotalHoursWorked As Double
        Dim SaturdayTotalHoursWorked As Double
        Dim SundayTotalHoursWorked As Double
        Dim WeeklyTotalHoursWorked As Double

        Dim MondayTotalRT As Double
        Dim TuesdayTotalRT As Double
        Dim WednesdayTotalRT As Double
        Dim ThursdayTotalRT As Double
        Dim FridayTotalRT As Double
        Dim SaturdayTotalRT As Double
        Dim SundayTotalRT As Double
        Dim MondayTotalOT As Double
        Dim TuesdayTotalOT As Double
        Dim WednesdayTotalOT As Double
        Dim ThursdayTotalOT As Double
        Dim FridayTotalOT As Double
        Dim SaturdayTotalOT As Double
        Dim SundayTotalOT As Double
        Dim MondayTotalDT As Double
        Dim TuesdayTotalDT As Double
        Dim WednesdayTotalDT As Double
        Dim ThursdayTotalDT As Double
        Dim FridayTotalDT As Double
        Dim SaturdayTotalDT As Double
        Dim SundayTotalDT As Double

        Dim MondayTotalOffDutyMealTimeRT As Double
        Dim TuesdayTotalOffDutyMealTimeRT As Double
        Dim WednesdayTotalOffDutyMealTimeRT As Double
        Dim ThursdayTotalOffDutyMealTimeRT As Double
        Dim FridayTotalOffDutyMealTimeRT As Double
        Dim SaturdayTotalOffDutyMealTimeRT As Double
        Dim SundayTotalOffDutyMealTimeRT As Double
        Dim WeeklyTotalOffDutyMealTimeRT As Double
        Dim MondayTotalOffDutyMealTimeOT As Double
        Dim TuesdayTotalOffDutyMealTimeOT As Double
        Dim WednesdayTotalOffDutyMealTimeOT As Double
        Dim ThursdayTotalOffDutyMealTimeOT As Double
        Dim FridayTotalOffDutyMealTimeOT As Double
        Dim SaturdayTotalOffDutyMealTimeOT As Double
        Dim SundayTotalOffDutyMealTimeOT As Double
        Dim WeeklyTotalOffDutyMealTimeOT As Double
        Dim MondayTotalOffDutyMealTimeDT As Double
        Dim TuesdayTotalOffDutyMealTimeDT As Double
        Dim WednesdayTotalOffDutyMealTimeDT As Double
        Dim ThursdayTotalOffDutyMealTimeDT As Double
        Dim FridayTotalOffDutyMealTimeDT As Double
        Dim SaturdayTotalOffDutyMealTimeDT As Double
        Dim SundayTotalOffDutyMealTimeDT As Double
        Dim WeeklyTotalOffDutyMealTimeDT As Double
        Dim MondayTotalOffDutyMealTime As Double
        Dim TuesdayTotalOffDutyMealTime As Double
        Dim WednesdayTotalOffDutyMealTime As Double
        Dim ThursdayTotalOffDutyMealTime As Double
        Dim FridayTotalOffDutyMealTime As Double
        Dim SaturdayTotalOffDutyMealTime As Double
        Dim SundayTotalOffDutyMealTime As Double
        Dim WeeklyTotalOffDutyMealTime As Double
        #End Region

        Dim dt As New DataTable()
        Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE client_site_tbl.company='"+company.Replace("'","''")+"' AND client_site_shift_instance_tbl.client_site_id = '" + client_ddl.SelectedValue + "' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
        Dim sqlDa As New SqlDataAdapter(sqlCmd)
        conn.close()
        conn.open()
        sqlDa.Fill(dt)
        conn.close()

        Dim _date_ As DateTime
        If weekOfDate.Text.Length > 0 Then
            _date_ = DateTime.Parse(weekOfDate.Text)
            While _date_.DayOfWeek <> DayOfWeek.Monday
                _date_ = _date_.AddDays(-1)
            End While
        End If

        For Each row In dt.Rows
            Dim wholespan As Double
            Dim firstspan As Double
            Dim secondspan As Double
            Dim startdatetime = DateTime.Parse(row(4))
            Dim enddatetime = DateTime.Parse(row(5))

            #Region "Divide this shift and Meals into buckets"
            '' Figure total hours worked by day of week
            If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                Select Case startdatetime.DayOfWeek
                    Case DayOfWeek.Sunday
                        If enddatetime.Date = _date_.Date Then
                            MondayTotalHoursWorked += secondspan
                        Else
                            SundayTotalHoursWorked += firstspan
                        End If
                    Case DayOfWeek.Monday
                        MondayTotalHoursWorked += firstspan
                        TuesdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Tuesday
                        TuesdayTotalHoursWorked += firstspan
                        WednesdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Wednesday
                        WednesdayTotalHoursWorked += firstspan
                        ThursdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Thursday
                        ThursdayTotalHoursWorked += firstspan
                        FridayTotalHoursWorked += secondspan
                    Case DayOfWeek.Friday
                        FridayTotalHoursWorked += firstspan
                        SaturdayTotalHoursWorked += secondspan
                    Case DayOfWeek.Saturday
                        SaturdayTotalHoursWorked += firstspan
                        SundayTotalHoursWorked += secondspan
                End Select
            Else
                wholespan = enddatetime.Ticks - startdatetime.Ticks
                Select Case startdatetime.DayOfWeek
                    Case DayOfWeek.Sunday
                        SundayTotalHoursWorked += wholespan
                    Case DayOfWeek.Monday
                        MondayTotalHoursWorked += wholespan
                    Case DayOfWeek.Tuesday
                        TuesdayTotalHoursWorked += wholespan
                    Case DayOfWeek.Wednesday
                        WednesdayTotalHoursWorked += wholespan
                    Case DayOfWeek.Thursday
                        ThursdayTotalHoursWorked += wholespan
                    Case DayOfWeek.Friday
                        FridayTotalHoursWorked += wholespan
                    Case DayOfWeek.Saturday
                        SaturdayTotalHoursWorked += wholespan
                End Select
            End If

            '' Figure total off duty lunch time by day of week
            If row(7) = False Then
                Dim firstmeal_startdatetime As DateTime
                Dim firstmeal_enddatetime As DateTime
                Dim secondmeal_startdatetime As DateTime
                Dim secondmeal_enddatetime As DateTime
                Dim thirdmeal_startdatetime As DateTime
                Dim thirdmeal_enddatetime As DateTime

                Try
                    firstmeal_startdatetime = DateTime.Parse(row(8))
                Catch ex As Exception
                    firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    firstmeal_enddatetime = DateTime.Parse(row(9))
                Catch ex As Exception
                    firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    secondmeal_startdatetime = DateTime.Parse(row(10))
                Catch ex As Exception
                    secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    secondmeal_enddatetime = DateTime.Parse(row(11))
                Catch ex As Exception
                    secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    thirdmeal_startdatetime = DateTime.Parse(row(12))
                Catch ex As Exception
                    thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try
                Try
                    thirdmeal_enddatetime = DateTime.Parse(row(13))
                Catch ex As Exception
                    thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                Finally
                End Try

                If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                    firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                    secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                    Select Case firstmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If enddatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeRT += secondspan
                            Else
                                SundayTotalOffDutyMealTimeRT += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalOffDutyMealTimeRT += firstspan
                            TuesdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeRT += firstspan
                            WednesdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeRT += firstspan
                            ThursdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeRT += firstspan
                            FridayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeRT += firstspan
                            SaturdayTotalOffDutyMealTimeRT += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeRT += firstspan
                            SundayTotalOffDutyMealTimeRT += secondspan
                    End Select
                Else
                    wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                    Select Case firstmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If firstmeal_enddatetime.Date > _date_.Date Then
                                SundayTotalOffDutyMealTimeRT += wholespan
                            End If
                        Case DayOfWeek.Monday
                            If firstmeal_startdatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeRT += wholespan
                            End If
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeRT += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeRT += wholespan
                    End Select
                End If
                If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                    firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                    secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                    Select Case secondmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If secondmeal_enddatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeOT += secondspan
                            Else
                                SundayTotalOffDutyMealTimeOT += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalOffDutyMealTimeOT += firstspan
                            TuesdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeOT += firstspan
                            WednesdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeOT += firstspan
                            ThursdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeOT += firstspan
                            FridayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeOT += firstspan
                            SaturdayTotalOffDutyMealTimeOT += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeOT += firstspan
                            SundayTotalOffDutyMealTimeOT += secondspan
                    End Select
                Else
                    wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                    Select Case secondmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If secondmeal_enddatetime.Date > _date_.Date Then
                                SundayTotalOffDutyMealTimeOT += wholespan
                            End If
                        Case DayOfWeek.Monday
                            If secondmeal_startdatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeOT += wholespan
                            End If
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeOT += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeOT += wholespan
                    End Select
                End If
                If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                    firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                    secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                    Select Case thirdmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If thirdmeal_enddatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeDT += secondspan
                            Else
                                SundayTotalOffDutyMealTimeDT += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalOffDutyMealTimeDT += firstspan
                            TuesdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeDT += firstspan
                            WednesdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeDT += firstspan
                            ThursdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeDT += firstspan
                            FridayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeDT += firstspan
                            SaturdayTotalOffDutyMealTimeDT += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeDT += firstspan
                            SundayTotalOffDutyMealTimeDT += secondspan
                    End Select
                Else
                    wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                    Select Case thirdmeal_startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If thirdmeal_enddatetime.Date > _date_.Date Then
                                SundayTotalOffDutyMealTimeDT += wholespan
                            End If
                        Case DayOfWeek.Monday
                            If thirdmeal_startdatetime.Date = _date_.Date Then
                                MondayTotalOffDutyMealTimeDT += wholespan
                            End If
                        Case DayOfWeek.Tuesday
                            TuesdayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalOffDutyMealTimeDT += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalOffDutyMealTimeDT += wholespan
                    End Select
                End If
            End If

            #End Region

        Next

        WeeklyTotalHoursWorked = MondayTotalHoursWorked + TuesdayTotalHoursWorked + WednesdayTotalHoursWorked + ThursdayTotalHoursWorked + FridayTotalHoursWorked + SaturdayTotalHoursWorked + SundayTotalHoursWorked

        MondayTotalOffDutyMealTime = MondayTotalOffDutyMealTimeRT + MondayTotalOffDutyMealTimeOT + MondayTotalOffDutyMealTimeDT
        TuesdayTotalOffDutyMealTime = TuesdayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeDT
        WednesdayTotalOffDutyMealTime = WednesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeDT
        ThursdayTotalOffDutyMealTime = ThursdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeDT
        FridayTotalOffDutyMealTime = FridayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeDT
        SaturdayTotalOffDutyMealTime = SaturdayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeDT
        SundayTotalOffDutyMealTime = SundayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeDT

        Dim halfhour = 10000000L * 60 * 60 * 0.5
        Dim onehour = 10000000L * 60 * 60 * 1
        Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5

        #Region "Meal Times into buckets RT OT DT by DayOfWeek"
        If MondayTotalOffDutyMealTime <= halfhour Then
            MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTime
            MondayTotalOffDutyMealTimeOT = 0
            MondayTotalOffDutyMealTimeDT = 0
        ElseIf MondayTotalOffDutyMealTime > halfhour And MondayTotalOffDutyMealTime <= onehour Then
            MondayTotalOffDutyMealTimeRT = halfhour
            MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTime - halfhour
            MondayTotalOffDutyMealTimeDT = 0
        ElseIf MondayTotalOffDutyMealTime > onehour And MondayTotalOffDutyMealTime <= oneandhalfhour Then
            MondayTotalOffDutyMealTimeRT = halfhour
            MondayTotalOffDutyMealTimeOT = halfhour
            MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTime - onehour
        ElseIf MondayTotalOffDutyMealTime > oneandhalfhour Then
            MondayTotalOffDutyMealTimeRT = halfhour
            MondayTotalOffDutyMealTimeOT = halfhour
            MondayTotalOffDutyMealTimeDT = halfhour
        End If
        If TuesdayTotalOffDutyMealTime <= halfhour Then
            TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTime
            TuesdayTotalOffDutyMealTimeOT = 0
            TuesdayTotalOffDutyMealTimeDT = 0
        ElseIf TuesdayTotalOffDutyMealTime > halfhour And TuesdayTotalOffDutyMealTime <= onehour Then
            TuesdayTotalOffDutyMealTimeRT = halfhour
            TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTime - halfhour
            TuesdayTotalOffDutyMealTimeDT = 0
        ElseIf TuesdayTotalOffDutyMealTime > onehour And TuesdayTotalOffDutyMealTime <= oneandhalfhour Then
            TuesdayTotalOffDutyMealTimeRT = halfhour
            TuesdayTotalOffDutyMealTimeOT = halfhour
            TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTime - onehour
        ElseIf TuesdayTotalOffDutyMealTime > oneandhalfhour Then
            TuesdayTotalOffDutyMealTimeRT = halfhour
            TuesdayTotalOffDutyMealTimeOT = halfhour
            TuesdayTotalOffDutyMealTimeDT = halfhour
        End If
        If WednesdayTotalOffDutyMealTime <= halfhour Then
            WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTime
            WednesdayTotalOffDutyMealTimeOT = 0
            WednesdayTotalOffDutyMealTimeDT = 0
        ElseIf WednesdayTotalOffDutyMealTime > halfhour And WednesdayTotalOffDutyMealTime <= onehour Then
            WednesdayTotalOffDutyMealTimeRT = halfhour
            WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTime - halfhour
            WednesdayTotalOffDutyMealTimeDT = 0
        ElseIf WednesdayTotalOffDutyMealTime > onehour And WednesdayTotalOffDutyMealTime <= oneandhalfhour Then
            WednesdayTotalOffDutyMealTimeRT = halfhour
            WednesdayTotalOffDutyMealTimeOT = halfhour
            WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTime - onehour
        ElseIf WednesdayTotalOffDutyMealTime > oneandhalfhour Then
            WednesdayTotalOffDutyMealTimeRT = halfhour
            WednesdayTotalOffDutyMealTimeOT = halfhour
            WednesdayTotalOffDutyMealTimeDT = halfhour
        End If
        If ThursdayTotalOffDutyMealTime <= halfhour Then
            ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTime
            ThursdayTotalOffDutyMealTimeOT = 0
            ThursdayTotalOffDutyMealTimeDT = 0
        ElseIf WednesdayTotalOffDutyMealTime > halfhour And ThursdayTotalOffDutyMealTime <= onehour Then
            ThursdayTotalOffDutyMealTimeRT = halfhour
            ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTime - halfhour
            ThursdayTotalOffDutyMealTimeDT = 0
        ElseIf ThursdayTotalOffDutyMealTime > onehour And ThursdayTotalOffDutyMealTime <= oneandhalfhour Then
            ThursdayTotalOffDutyMealTimeRT = halfhour
            ThursdayTotalOffDutyMealTimeOT = halfhour
            ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTime - onehour
        ElseIf ThursdayTotalOffDutyMealTime > oneandhalfhour Then
            ThursdayTotalOffDutyMealTimeRT = halfhour
            ThursdayTotalOffDutyMealTimeOT = halfhour
            ThursdayTotalOffDutyMealTimeDT = halfhour
        End If
        If FridayTotalOffDutyMealTime <= halfhour Then
            FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTime
            FridayTotalOffDutyMealTimeOT = 0
            FridayTotalOffDutyMealTimeDT = 0
        ElseIf FridayTotalOffDutyMealTime > halfhour And FridayTotalOffDutyMealTime <= onehour Then
            FridayTotalOffDutyMealTimeRT = halfhour
            FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTime - halfhour
            FridayTotalOffDutyMealTimeDT = 0
        ElseIf FridayTotalOffDutyMealTime > onehour And FridayTotalOffDutyMealTime <= oneandhalfhour Then
            FridayTotalOffDutyMealTimeRT = halfhour
            FridayTotalOffDutyMealTimeOT = halfhour
            FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTime - onehour
        ElseIf FridayTotalOffDutyMealTime > oneandhalfhour Then
            FridayTotalOffDutyMealTimeRT = halfhour
            FridayTotalOffDutyMealTimeOT = halfhour
            FridayTotalOffDutyMealTimeDT = halfhour
        End If
        If SaturdayTotalOffDutyMealTime <= halfhour Then
            SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTime
            SaturdayTotalOffDutyMealTimeOT = 0
            SaturdayTotalOffDutyMealTimeDT = 0
        ElseIf SaturdayTotalOffDutyMealTime > halfhour And SaturdayTotalOffDutyMealTime <= onehour Then
            SaturdayTotalOffDutyMealTimeRT = halfhour
            SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTime - halfhour
            SaturdayTotalOffDutyMealTimeDT = 0
        ElseIf SaturdayTotalOffDutyMealTime > onehour And SaturdayTotalOffDutyMealTime <= oneandhalfhour Then
            SaturdayTotalOffDutyMealTimeRT = halfhour
            SaturdayTotalOffDutyMealTimeOT = halfhour
            SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTime - onehour
        ElseIf SaturdayTotalOffDutyMealTime > oneandhalfhour Then
            SaturdayTotalOffDutyMealTimeRT = halfhour
            SaturdayTotalOffDutyMealTimeOT = halfhour
            SaturdayTotalOffDutyMealTimeDT = halfhour
        End If
        If SundayTotalOffDutyMealTime <= halfhour Then
            SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTime
            SundayTotalOffDutyMealTimeOT = 0
            SundayTotalOffDutyMealTimeDT = 0
        ElseIf SundayTotalOffDutyMealTime > halfhour And SundayTotalOffDutyMealTime <= onehour Then
            SundayTotalOffDutyMealTimeRT = halfhour
            SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTime - halfhour
            SundayTotalOffDutyMealTimeDT = 0
        ElseIf SundayTotalOffDutyMealTime > onehour And SundayTotalOffDutyMealTime <= oneandhalfhour Then
            SundayTotalOffDutyMealTimeRT = halfhour
            SundayTotalOffDutyMealTimeOT = halfhour
            SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTime - onehour
        ElseIf SundayTotalOffDutyMealTime > oneandhalfhour Then
            SundayTotalOffDutyMealTimeRT = halfhour
            SundayTotalOffDutyMealTimeOT = halfhour
            SundayTotalOffDutyMealTimeDT = halfhour
        End If
        #End Region

        WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
        WeeklyTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeOT
        WeeklyTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + TuesdayTotalOffDutyMealTimeDT + WednesdayTotalOffDutyMealTimeDT + ThursdayTotalOffDutyMealTimeDT + FridayTotalOffDutyMealTimeDT + SaturdayTotalOffDutyMealTimeDT + SundayTotalOffDutyMealTimeDT

        Dim fourhours = 10000000L * 60 * 60 * 4
        Dim eighthours = 10000000L * 60 * 60 * 8
        Dim twelvehours = 10000000L * 60 * 60 * 12
        Dim fourtyhours = 10000000L * 60 * 60 * 40
        Dim WeeklyTotalRegularTime = 0L
        Dim WeeklyTotalOverTime = 0L
        Dim WeeklyTotalDoubleTime = 0L

        #Region "Total RTOTDT by DayOfWeek"
        If MondayTotalHoursWorked >= twelvehours Then
            MondayTotalDT = MondayTotalHoursWorked - twelvehours
            MondayTotalOT = fourhours
            MondayTotalRT = eighthours
        ElseIf MondayTotalHoursWorked >= eighthours Then
            MondayTotalOT = MondayTotalHoursWorked - eighthours
            MondayTotalRT = eighthours
        Else
            MondayTotalRT = MondayTotalHoursWorked
        End If
        If TuesdayTotalHoursWorked >= twelvehours Then
            TuesdayTotalDT = TuesdayTotalHoursWorked - twelvehours
            TuesdayTotalOT = fourhours
            TuesdayTotalRT = eighthours
        ElseIf TuesdayTotalHoursWorked >= eighthours Then
            TuesdayTotalOT = TuesdayTotalHoursWorked - eighthours
            TuesdayTotalRT = eighthours
        Else
            TuesdayTotalRT = TuesdayTotalHoursWorked
        End If
        If WednesdayTotalHoursWorked >= twelvehours Then
            WednesdayTotalDT = WednesdayTotalHoursWorked - twelvehours
            WednesdayTotalOT = fourhours
            WednesdayTotalRT = eighthours
        ElseIf WednesdayTotalHoursWorked >= eighthours Then
            WednesdayTotalOT = WednesdayTotalHoursWorked - eighthours
            WednesdayTotalRT = eighthours
        Else
            WednesdayTotalRT = WednesdayTotalHoursWorked
        End If
        If ThursdayTotalHoursWorked >= twelvehours Then
            ThursdayTotalDT = ThursdayTotalHoursWorked - twelvehours
            ThursdayTotalOT = fourhours
            ThursdayTotalRT = eighthours
        ElseIf ThursdayTotalHoursWorked >= eighthours Then
            ThursdayTotalOT = ThursdayTotalHoursWorked - eighthours
            ThursdayTotalRT = eighthours
        Else
            ThursdayTotalRT = ThursdayTotalHoursWorked
        End If
        If FridayTotalHoursWorked >= twelvehours Then
            FridayTotalDT = FridayTotalHoursWorked - twelvehours
            FridayTotalOT = fourhours
            FridayTotalRT = eighthours
        ElseIf FridayTotalHoursWorked >= eighthours Then
            FridayTotalOT = FridayTotalHoursWorked - eighthours
            FridayTotalRT = eighthours
        Else
            FridayTotalRT = FridayTotalHoursWorked
        End If
        If SaturdayTotalHoursWorked >= twelvehours Then
            SaturdayTotalDT = SaturdayTotalHoursWorked - twelvehours
            SaturdayTotalOT = fourhours
            SaturdayTotalRT = eighthours
        ElseIf SaturdayTotalHoursWorked >= eighthours Then
            SaturdayTotalOT = SaturdayTotalHoursWorked - eighthours
            SaturdayTotalRT = eighthours
        Else
            SaturdayTotalRT = SaturdayTotalHoursWorked
        End If
        If SundayTotalHoursWorked >= twelvehours Then
            SundayTotalDT = SundayTotalHoursWorked - twelvehours
            SundayTotalOT = fourhours
            SundayTotalRT = eighthours
        ElseIf SundayTotalHoursWorked >= eighthours Then
            SundayTotalOT = SundayTotalHoursWorked - eighthours
            SundayTotalRT = eighthours
        Else
            SundayTotalRT = SundayTotalHoursWorked
        End If
        #End Region

        WeeklyTotalRegularTime = MondayTotalRT + TuesdayTotalRT + WednesdayTotalRT + ThursdayTotalRT + FridayTotalRT + SaturdayTotalRT + SundayTotalRT
        WeeklyTotalOverTime = MondayTotalOT + TuesdayTotalOT + WednesdayTotalOT + ThursdayTotalOT + FridayTotalOT + SaturdayTotalOT + SundayTotalOT
        WeeklyTotalDoubleTime = MondayTotalDT + TuesdayTotalDT + WednesdayTotalDT + ThursdayTotalDT + FridayTotalDT + SaturdayTotalDT + SundayTotalDT

        If WeeklyTotalHoursWorked - fourtyhours > WeeklyTotalOverTime Then
            WeeklyTotalOverTime = WeeklyTotalHoursWorked - fourtyhours - WeeklyTotalDoubleTime
            WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalOverTime
        End If

        #Region "Calculate HT and HT_OffDutyMeal for each day of week"
        Dim MondayTotalHT As Double
        Dim TuesdayTotalHT As Double
        Dim WednesdayTotalHT As Double
        Dim ThursdayTotalHT As Double
        Dim FridayTotalHT As Double
        Dim SaturdayTotalHT As Double
        Dim SundayTotalHT As Double
        Dim WeeklyTotalHolidayTime As Double
        Dim MondayTotalOffDutyMealTimeHT As Double
        Dim TuesdayTotalOffDutyMealTimeHT As Double
        Dim WednesdayTotalOffDutyMealTimeHT As Double
        Dim ThursdayTotalOffDutyMealTimeHT As Double
        Dim FridayTotalOffDutyMealTimeHT As Double
        Dim SaturdayTotalOffDutyMealTimeHT As Double
        Dim SundayTotalOffDutyMealTimeHT As Double
        Dim WeeklyTotalOffDutyMealTimeHT As Double
        Dim _date As DateTime
        If weekOfDate.Text.Length > 0 Then
            _date = DateTime.Parse(weekOfDate.Text)
            While _date.DayOfWeek <> DayOfWeek.Monday
                _date = _date.AddDays(-1)
            End While
        End If
        If dateIsHoliday(_date) Then
            MondayTotalHT = MondayTotalRT
            MondayTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeRT
            MondayTotalRT = 0
            MondayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            TuesdayTotalHT = TuesdayTotalRT
            TuesdayTotalOffDutyMealTimeHT = TuesdayTotalOffDutyMealTimeRT
            TuesdayTotalRT = 0
            TuesdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            WednesdayTotalHT = WednesdayTotalRT
            WednesdayTotalOffDutyMealTimeHT = WednesdayTotalOffDutyMealTimeRT
            WednesdayTotalRT = 0
            WednesdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            ThursdayTotalHT = ThursdayTotalRT
            ThursdayTotalOffDutyMealTimeHT = ThursdayTotalOffDutyMealTimeRT
            ThursdayTotalRT = 0
            ThursdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            FridayTotalHT = FridayTotalRT
            FridayTotalOffDutyMealTimeHT = FridayTotalOffDutyMealTimeRT
            FridayTotalRT = 0
            FridayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            SaturdayTotalHT = SaturdayTotalRT
            SaturdayTotalOffDutyMealTimeHT = SaturdayTotalOffDutyMealTimeRT
            SaturdayTotalRT = 0
            SaturdayTotalOffDutyMealTimeRT = 0
        End If
        _date = _date.AddDays(1)
        If dateIsHoliday(_date) Then
            SundayTotalHT = SundayTotalRT
            SundayTotalOffDutyMealTimeHT = SundayTotalOffDutyMealTimeRT
            SundayTotalRT = 0
            SundayTotalOffDutyMealTimeRT = 0
        End If
        WeeklyTotalHolidayTime = MondayTotalHT + TuesdayTotalHT + WednesdayTotalHT + ThursdayTotalHT + FridayTotalHT + SaturdayTotalHT + SundayTotalHT
        WeeklyTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeHT + TuesdayTotalOffDutyMealTimeHT + WednesdayTotalOffDutyMealTimeHT + ThursdayTotalOffDutyMealTimeHT + FridayTotalOffDutyMealTimeHT + SaturdayTotalOffDutyMealTimeHT + SundayTotalOffDutyMealTimeHT
        #End Region

        Dim MondayTotalHoursWorkedMinusLunches As Double = MondayTotalHoursWorked - MondayTotalOffDutyMealTime
        Dim TuesdayTotalHoursWorkedMinusLunches As Double = TuesdayTotalHoursWorked - TuesdayTotalOffDutyMealTime
        Dim WednesdayTotalHoursWorkedMinusLunches As Double = WednesdayTotalHoursWorked - WednesdayTotalOffDutyMealTime
        Dim ThursdayTotalHoursWorkedMinusLunches As Double = ThursdayTotalHoursWorked - ThursdayTotalOffDutyMealTime
        Dim FridayTotalHoursWorkedMinusLunches As Double = FridayTotalHoursWorked - FridayTotalOffDutyMealTime
        Dim SaturdayTotalHoursWorkedMinusLunches As Double = SaturdayTotalHoursWorked - SaturdayTotalOffDutyMealTime
        Dim SundayTotalHoursWorkedMinusLunches As Double = SundayTotalHoursWorked - SundayTotalOffDutyMealTime

        '' Subtract HT from RT
        WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalHolidayTime
        '' Recalculate Total MealTimes RT 
        WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
        '' Calculate Total MealTime
        WeeklyTotalOffDutyMealTime = WeeklyTotalOffDutyMealTimeRT + WeeklyTotalOffDutyMealTimeOT + WeeklyTotalOffDutyMealTimeDT + WeeklyTotalOffDutyMealTimeHT
        '' Calculate WeeklyTotalHoursWorked Minus Lunches
        Dim WeeklyTotalHoursWorkedMinusLunches As Double = WeeklyTotalHoursWorked - WeeklyTotalOffDutyMealTime
        Dim WeeklyTotalRegularTimeMinusLunches As Double = WeeklyTotalRegularTime - WeeklyTotalOffDutyMealTimeRT
        Dim WeeklyTotalHolidayTimeMinusLunches As Double = WeeklyTotalHolidayTime - WeeklyTotalOffDutyMealTimeHT
        Dim WeeklyTotalOverTimeMinusLunches As Double = WeeklyTotalOverTime - WeeklyTotalOffDutyMealTimeOT
        Dim WeeklyTotalDoubleTimeMinusLunches As Double = WeeklyTotalDoubleTime - WeeklyTotalOffDutyMealTimeDT

        '' Get the PayRate
        '' Fill The shift_instance_Table
        dt1 = New DataTable()
        sqlCmd1 = New SqlCommand("SELECT basepayrate FROM employee_tbl WHERE employee_id = '" + employee_ddl.SelectedValue + "'", conn)
        sqlDa1 = New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        ' Calculate Dollars to be paid
        Dim basepayrate As Double = 0.00'Convert.ToInt64(Decimal.Parse(dt1.Rows(0).Item(0).ToString()))
        Dim WeeklyTotalDollars As Double = (TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours*2.0) * (basepayrate)
        Dim WeeklyTotalDollarsMinusLunches As Double = (TimeSpan.FromTicks(WeeklyTotalRegularTimeMinuslunches).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinuslunches).TotalHours*2.0) * (basepayrate)
        Dim WeeklyTotalDollarsOffDutyMealTime As Double = (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours*2.0) * (basepayrate)

        'Create datatable and columns
        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("rowtitle"))
        dtable.Columns.Add(New DataColumn("mondaytime"))
        dtable.Columns.Add(New DataColumn("tuesdaytime"))
        dtable.Columns.Add(New DataColumn("wednesdaytime"))
        dtable.Columns.Add(New DataColumn("thursdaytime"))
        dtable.Columns.Add(New DataColumn("fridaytime"))
        dtable.Columns.Add(New DataColumn("saturdaytime"))
        dtable.Columns.Add(New DataColumn("sundaytime"))
        dtable.Columns.Add(New DataColumn("all_total"))
        dtable.Columns.Add(New DataColumn("rt_total"))
        dtable.Columns.Add(New DataColumn("ht_total"))
        dtable.Columns.Add(New DataColumn("ot_total"))
        dtable.Columns.Add(New DataColumn("dt_total"))
        dtable.Columns.Add(New DataColumn("totaldollars"))
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "RT")).Visible = False
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "HT")).Visible = False
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "OT")).Visible = False
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "DT")).Visible = False
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "$ Total")).Visible = False
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "RT")).Visible = False
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "HT")).Visible = False
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "OT")).Visible = False
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "DT")).Visible = False
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "$ Total")).Visible = False

        If WeeklyTotalHoursWorked = 0 Then
            If company = "Excalibur" Then
                ExcaliburTotalsGridView.Visible = False
            Else 
                MattsTotalsGridView.Visible = False
            End If
        Else
            If company = "Excalibur" Then
                ExcaliburTotalsGridView.Visible = True
            Else 
                MattsTotalsGridView.Visible = True
            End If
        End If

        'Add Rows
        Dim RowValues As Object() = {"", "", "", "", "", "", "", "", ""}
        RowValues(0) = "Total Hours"
        RowValues(1) = If((MondayTotalHoursWorked) = 0L, "0", TimeSpan.FromTicks(MondayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(FridayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(SundayTotalHoursWorked).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalHoursWorked = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHoursWorked).TotalHours.ToString("N2"))
        'RowValues(9) = If(WeeklyTotalRegularTime = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours.ToString("N2"))
        'RowValues(10) = If(WeeklyTotalHolidayTime = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours.ToString("N2"))
        'RowValues(11) = If(WeeklyTotalOverTime = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours.ToString("N2"))
        'RowValues(12) = If(WeeklyTotalDoubleTime = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours.ToString("N2"))
        'RowValues(13) = If(WeeklyTotalDollars = 0L, "0", -)'$ +WeeklyTotalDollars.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()
        RowValues(0) = "On Duty Hours Worked"
        RowValues(1) = If((MondayTotalHoursWorkedMinusLunches) = 0L, "0", TimeSpan.FromTicks(MondayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(FridayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(SundayTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalHoursWorkedMinusLunches = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHoursWorkedMinusLunches).TotalHours.ToString("N2"))
        'RowValues(9) = If(WeeklyTotalRegularTimeMinusLunches = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalRegularTimeMinusLunches).TotalHours.ToString("N2"))
        'RowValues(10) = If(WeeklyTotalHolidayTimeMinusLunches = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinusLunches).TotalHours.ToString("N2"))
        'RowValues(11) = If(WeeklyTotalOverTimeMinusLunches = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalOverTimeMinusLunches).TotalHours.ToString("N2"))
        'RowValues(12) = If(WeeklyTotalDoubleTimeMinusLunches = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinusLunches).TotalHours.ToString("N2"))
        'RowValues(13) = If(WeeklyTotalDollarsMinusLunches = 0L, "0", -)'$ +WeeklyTotalDollarsMinuslunches.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()
        RowValues(0) = "Off Duty Meal Time"
        RowValues(1) = If(MondayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(MondayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(TuesdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(WednesdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(ThursdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(FridayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(SaturdayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(SundayTotalOffDutyMealTime).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalOffDutyMealTime = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTime).TotalHours.ToString("N2"))
        'RowValues(9) = If(WeeklyTotalOffDutyMealTimeRT = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours.ToString("N2"))
        'RowValues(10) = If(WeeklyTotalOffDutyMealTimeHT = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours.ToString("N2"))
        'RowValues(11) = If(WeeklyTotalOffDutyMealTimeOT = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours.ToString("N2"))
        'RowValues(12) = If(WeeklyTotalOffDutyMealTimeDT = 0L, "0", -)'TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours.ToString("N2"))
        'RowValues(13) = If(WeeklyTotalDollarsOffDutyMealTime = 0L, "0", -)'$ +WeeklyTotalDollarsOffDutyMealTime.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()

        'now bind datatable to gridview... 
        If company = "Excalibur" Then
            ExcaliburTotalsGridView.DataSource = dtable
            ExcaliburTotalsGridView.DataBind()
        Else
            MattsTotalsGridView.DataSource = dtable
            MattsTotalsGridView.DataBind()
        End If

    End Sub
    Protected Sub updateCompanyTotalsTable_AllEmployeesView(company As String, include_totals As boolean)

        '' Fill The shift_instance_Table
        Dim dt1 As New DataTable()
        Dim sqlCmd1 As New SqlCommand("SELECT client_site_tbl.client_site_id, client_site_tbl.name, client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_tbl.company, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.payrate, employee_tbl.username, employee_tbl.ms_id, employee_tbl.excalibur_id, employee_tbl.cellphone, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id LEFT JOIN employee_tbl ON client_site_shift_instance_tbl.employee_id = employee_tbl.employee_id WHERE ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "')) ORDER BY client_site_tbl.name ASC, employee_tbl.username ASC, client_site_shift_instance_tbl.startdatetime ASC", conn)
        Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
        conn.close()
        conn.open()
        sqlDa1.Fill(dt1)
        conn.close()

        ShiftInstanceGridView.DataSource = dt1
        ShiftInstanceGridView.DataBind()
        ShiftInstanceGridView.Columns(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Employees")).Visible = true

        ' Calculate Hours in PayPeriod
        Dim r as Int32 = 0
        Dim _startdate = DateTime.Parse(weekOfDate.Text)
        Dim _enddate = startdate
        While _startdate.DayOfWeek <> DayOfWeek.Monday
            _startdate = _startdate.AddDays(-1)
        End While
        While _enddate.DayOfWeek <> DayOfWeek.Sunday
            _enddate = _enddate.AddDays(1)
        End While
        _enddate = _enddate.AddHours(24)
        For Each row As DataRow in dt1.Rows
            Dim startdatetime As DateTime = DateTime.Parse(row.ItemArray(11).ToString) ' Don't Change Query of dt1
            If (startdatetime < _startdate) Then startdatetime = _startdate
            Dim enddatetime As DateTime = DateTime.Parse(row.ItemArray(12).ToString) ' Don't Change Query of dt1
            If (enddatetime > _enddate) Then enddatetime = _enddate
            Dim ticks As Double = enddatetime.Ticks - startdatetime.Ticks
            ShiftInstanceGridView.Rows(r).Cells(GetColumnIndexByHeaderText(ShiftInstanceGridView, "Hours in Pay Period")).Text = TimeSpan.FromTicks(ticks).TotalHours.ToString("N2")
            r += 1
        Next

        ' Populate the Employee_DDL
        Dim dt2 As New DataTable()
        Dim sqlCmd2 As New SqlCommand("SELECT username, employee_id FROM employee_tbl WHERE status='Active' ORDER BY employee_tbl.username ASC", conn)
        Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
        conn.close()
        conn.open()
        sqlDa2.Fill(dt2)
        conn.close()
        r = 0
        For Each row in ShiftInstanceGridView.Rows
            Dim ddl As DropDownList = row.FindControl("Username_DDL")
            ddl.DataSource = dt2
            ddl.DataTextField = dt2.Columns(0).ToString()
            ddl.DataValueField = dt2.Columns(1).ToString()
            ddl.DataBind()
            ddl.Items.Insert(0, new ListItem("<--     OPEN     -->", ""))
            ddl.SelectedValue = dt1.Rows(r).ItemArray(4).ToString
            r += 1
        Next

        ' 1 line per row
        Dim i as Int32 = 0
        while i < ShiftInstanceGridView.Columns.Count 
            ShiftInstanceGridView.Columns(i).ItemStyle.Wrap = false
            i += 1
        End While

        Session("ShiftInstanceGridView") = dt1

        If (Not include_totals) Then return

        '' Fill The shift_instance_Table
        Dim dt5 As New DataTable()
        Dim sqlCmd5 As New SqlCommand("SELECT employee_id, basepayrate FROM employee_tbl WHERE company='"+company.Replace("'", "''")+"'", conn)
        Dim sqlDa5 As New SqlDataAdapter(sqlCmd5)
        conn.close()
        conn.open()
        sqlDa5.Fill(dt5)
        conn.close()
    
        #Region "Totals Variables"
        Dim MondayTotalHours As Double
        Dim MondayTotalOnDutyHours As Double
        Dim MondayTotalOffDutyHours As Double
        Dim TuesdayTotalHours As Double
        Dim TuesdayTotalOnDutyHours As Double
        Dim TuesdayTotalOffDutyHours As Double
        Dim WednesdayTotalHours As Double
        Dim WednesdayTotalOnDutyHours As Double
        Dim WednesdayTotalOffDutyHours As Double
        Dim ThursdayTotalHours As Double
        Dim ThursdayTotalOnDutyHours As Double
        Dim ThursdayTotalOffDutyHours As Double
        Dim FridayTotalHours As Double
        Dim FridayTotalOnDutyHours As Double
        Dim FridayTotalOffDutyHours As Double
        Dim SaturdayTotalHours As Double
        Dim SaturdayTotalOnDutyHours As Double
        Dim SaturdayTotalOffDutyHours As Double
        Dim SundayTotalHours As Double
        Dim SundayTotalOnDutyHours As Double
        Dim SundayTotalOffDutyHours As Double
        Dim WeeklyTotalHours As Double
        Dim WeeklyTotalOnDutyHours As Double
        Dim WeeklyTotalOffDutyHours As Double
        Dim WeeklyTotalRTHours As Double
        Dim WeeklyTotalRTOnDutyHours As Double
        Dim WeeklyTotalRTOffDutyHours As Double
        Dim WeeklyTotalHTHours As Double
        Dim WeeklyTotalHTOnDutyHours As Double
        Dim WeeklyTotalHTOffDutyHours As Double
        Dim WeeklyTotalOTHours As Double
        Dim WeeklyTotalOTOnDutyHours As Double
        Dim WeeklyTotalOTOffDutyHours As Double
        Dim WeeklyTotalDTHours As Double
        Dim WeeklyTotalDTOnDutyHours As Double
        Dim WeeklyTotalDTOffDutyHours As Double
        Dim WeeklyTotalDollars As Double
        Dim WeeklyTotalOnDutyDollars As Double
        Dim WeeklyTotalOffDutyDollars As Double
        #End Region

        For Each employee in dt5.Rows

            '' Fill the totals table
            #Region "Declare Variables"
            Dim MondayTotalHoursWorked As Double = 0.0
            Dim TuesdayTotalHoursWorked As Double = 0.0
            Dim WednesdayTotalHoursWorked As Double = 0.0
            Dim ThursdayTotalHoursWorked As Double = 0.0
            Dim FridayTotalHoursWorked As Double = 0.0
            Dim SaturdayTotalHoursWorked As Double = 0.0
            Dim SundayTotalHoursWorked As Double = 0.0
            Dim WeeklyTotalHoursWorked As Double = 0.0

            Dim MondayTotalRT As Double = 0.0
            Dim TuesdayTotalRT As Double = 0.0
            Dim WednesdayTotalRT As Double = 0.0
            Dim ThursdayTotalRT As Double = 0.0
            Dim FridayTotalRT As Double = 0.0
            Dim SaturdayTotalRT As Double = 0.0
            Dim SundayTotalRT As Double = 0.0
            Dim MondayTotalOT As Double = 0.0
            Dim TuesdayTotalOT As Double = 0.0
            Dim WednesdayTotalOT As Double = 0.0
            Dim ThursdayTotalOT As Double = 0.0
            Dim FridayTotalOT As Double = 0.0
            Dim SaturdayTotalOT As Double = 0.0
            Dim SundayTotalOT As Double = 0.0
            Dim MondayTotalDT As Double = 0.0
            Dim TuesdayTotalDT As Double = 0.0
            Dim WednesdayTotalDT As Double = 0.0
            Dim ThursdayTotalDT As Double = 0.0
            Dim FridayTotalDT As Double = 0.0
            Dim SaturdayTotalDT As Double = 0.0
            Dim SundayTotalDT As Double = 0.0

            Dim MondayTotalOffDutyMealTimeRT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeRT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeRT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeRT As Double = 0.0
            Dim MondayTotalOffDutyMealTimeOT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeOT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeOT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeOT As Double = 0.0
            Dim MondayTotalOffDutyMealTimeDT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeDT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeDT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeDT As Double = 0.0
            Dim MondayTotalOffDutyMealTime As Double = 0.0
            Dim TuesdayTotalOffDutyMealTime As Double = 0.0
            Dim WednesdayTotalOffDutyMealTime As Double = 0.0
            Dim ThursdayTotalOffDutyMealTime As Double = 0.0
            Dim FridayTotalOffDutyMealTime As Double = 0.0
            Dim SaturdayTotalOffDutyMealTime As Double = 0.0
            Dim SundayTotalOffDutyMealTime As Double = 0.0
            Dim WeeklyTotalOffDutyMealTime As Double = 0.0
            #End Region

            Dim dt As New DataTable()
            Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE client_site_shift_instance_tbl.employee_id='" + employee.ItemArray(0).ToString() + "' AND company='"+company.Replace("'", "''")+"' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
            Dim sqlDa As New SqlDataAdapter(sqlCmd)
            conn.close()
            conn.open()
            sqlDa.Fill(dt)
            conn.close()

            Dim _date_ As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date_ = DateTime.Parse(weekOfDate.Text)
                While _date_.DayOfWeek <> DayOfWeek.Monday
                    _date_ = _date_.AddDays(-1)
                End While
            End If

            For Each row In dt.Rows
                Dim wholespan As Double = 0.0
                Dim firstspan As Double = 0.0
                Dim secondspan As Double = 0.0
                Dim startdatetime = DateTime.Parse(row(4))
                Dim enddatetime = DateTime.Parse(row(5))

                '' Figure total hours worked by day of week
                If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                    firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                    secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                    Select Case startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If enddatetime.Date = _date_.Date Then
                                MondayTotalHoursWorked += secondspan
                            Else
                                SundayTotalHoursWorked += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalHoursWorked = MondayTotalHoursWorked + firstspan
                            TuesdayTotalHoursWorked = TuesdayTotalHoursWorked + secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalHoursWorked = TuesdayTotalHoursWorked + firstspan
                            WednesdayTotalHoursWorked = WednesdayTotalHoursWorked + secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalHoursWorked = WednesdayTotalHoursWorked + firstspan
                            ThursdayTotalHoursWorked = ThursdayTotalHoursWorked + secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalHoursWorked = ThursdayTotalHoursWorked + firstspan
                            FridayTotalHoursWorked = FridayTotalHoursWorked + secondspan
                        Case DayOfWeek.Friday
                            FridayTotalHoursWorked = FridayTotalHoursWorked + firstspan
                            SaturdayTotalHoursWorked = SaturdayTotalHoursWorked + secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalHoursWorked = SaturdayTotalHoursWorked + firstspan
                            SundayTotalHoursWorked = SundayTotalHoursWorked + secondspan
                    End Select
                Else
                    wholespan = enddatetime.Ticks - startdatetime.Ticks
                    Select Case startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            SundayTotalHoursWorked = SundayTotalHoursWorked + wholespan
                        Case DayOfWeek.Monday
                            MondayTotalHoursWorked = MondayTotalHoursWorked + wholespan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalHoursWorked = TuesdayTotalHoursWorked + wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalHoursWorked = WednesdayTotalHoursWorked + wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalHoursWorked = ThursdayTotalHoursWorked + wholespan
                        Case DayOfWeek.Friday
                            FridayTotalHoursWorked = FridayTotalHoursWorked + wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalHoursWorked = SaturdayTotalHoursWorked + wholespan
                    End Select
                End If

                '' Figure total off duty lunch time by day of week
                If row(7) = False Then
                    Dim firstmeal_startdatetime As DateTime
                    Dim firstmeal_enddatetime As DateTime
                    Dim secondmeal_startdatetime As DateTime
                    Dim secondmeal_enddatetime As DateTime
                    Dim thirdmeal_startdatetime As DateTime
                    Dim thirdmeal_enddatetime As DateTime

                    Try
                        firstmeal_startdatetime = DateTime.Parse(row(8))
                    Catch ex As Exception
                        firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        firstmeal_enddatetime = DateTime.Parse(row(9))
                    Catch ex As Exception
                        firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        secondmeal_startdatetime = DateTime.Parse(row(10))
                    Catch ex As Exception
                        secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        secondmeal_enddatetime = DateTime.Parse(row(11))
                    Catch ex As Exception
                        secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        thirdmeal_startdatetime = DateTime.Parse(row(12))
                    Catch ex As Exception
                        thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        thirdmeal_enddatetime = DateTime.Parse(row(13))
                    Catch ex As Exception
                        thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try

                    If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                        firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                        secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                        Select Case firstmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + secondspan
                                Else
                                    SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTimeRT + firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + firstspan
                                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTimeRT + firstspan
                                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTimeRT + firstspan
                                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTimeRT + firstspan
                                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTimeRT + firstspan
                                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTimeRT + firstspan
                                SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTimeRT + secondspan
                        End Select
                    Else
                        wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                        Select Case firstmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If firstmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTimeRT + wholespan
                                End If
                            Case DayOfWeek.Monday
                                If firstmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTimeRT + wholespan
                        End Select
                    End If
                    If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                        firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                        secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                        Select Case secondmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If secondmeal_enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + secondspan
                                Else
                                    SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTimeOT + firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + firstspan
                                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTimeOT + firstspan
                                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTimeOT + firstspan
                                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTimeOT + firstspan
                                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTimeOT + firstspan
                                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTimeOT + firstspan
                                SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTimeOT + secondspan
                        End Select
                    Else
                        wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                        Select Case secondmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If secondmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTimeOT + wholespan
                                End If
                            Case DayOfWeek.Monday
                                If secondmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTimeOT + wholespan
                        End Select
                    End If
                    If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                        firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                        secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                        Select Case thirdmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If thirdmeal_enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + secondspan
                                Else
                                    SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTimeDT + firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + firstspan
                                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTimeDT + firstspan
                                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTimeDT + firstspan
                                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTimeDT + firstspan
                                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTimeDT + firstspan
                                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTimeDT + firstspan
                                SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTimeDT + secondspan
                        End Select
                    Else
                        wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                        Select Case thirdmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If thirdmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTimeDT + wholespan
                                End If
                            Case DayOfWeek.Monday
                                If thirdmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTimeDT + wholespan
                        End Select
                    End If
                End If
            Next

            WeeklyTotalHoursWorked = MondayTotalHoursWorked + TuesdayTotalHoursWorked + WednesdayTotalHoursWorked + ThursdayTotalHoursWorked + FridayTotalHoursWorked + SaturdayTotalHoursWorked + SundayTotalHoursWorked

            MondayTotalOffDutyMealTime = MondayTotalOffDutyMealTimeRT + MondayTotalOffDutyMealTimeOT + MondayTotalOffDutyMealTimeDT
            TuesdayTotalOffDutyMealTime = TuesdayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeDT
            WednesdayTotalOffDutyMealTime = WednesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeDT
            ThursdayTotalOffDutyMealTime = ThursdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeDT
            FridayTotalOffDutyMealTime = FridayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeDT
            SaturdayTotalOffDutyMealTime = SaturdayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeDT
            SundayTotalOffDutyMealTime = SundayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeDT

            Dim halfhour = 10000000L * 60 * 60 * 0.5
            Dim onehour = 10000000L * 60 * 60 * 1
            Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5

            If MondayTotalOffDutyMealTime <= halfhour Then
                MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTime
                MondayTotalOffDutyMealTimeOT = 0
                MondayTotalOffDutyMealTimeDT = 0
            ElseIf MondayTotalOffDutyMealTime > halfhour And MondayTotalOffDutyMealTime <= onehour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTime - halfhour
                MondayTotalOffDutyMealTimeDT = 0
            ElseIf MondayTotalOffDutyMealTime > onehour And MondayTotalOffDutyMealTime <= oneandhalfhour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = halfhour
                MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTime - onehour
            ElseIf MondayTotalOffDutyMealTime > oneandhalfhour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = halfhour
                MondayTotalOffDutyMealTimeDT = halfhour
            End If
            If TuesdayTotalOffDutyMealTime <= halfhour Then
                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTime
                TuesdayTotalOffDutyMealTimeOT = 0
                TuesdayTotalOffDutyMealTimeDT = 0
            ElseIf TuesdayTotalOffDutyMealTime > halfhour And TuesdayTotalOffDutyMealTime <= onehour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTime - halfhour
                TuesdayTotalOffDutyMealTimeDT = 0
            ElseIf TuesdayTotalOffDutyMealTime > onehour And TuesdayTotalOffDutyMealTime <= oneandhalfhour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = halfhour
                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTime - onehour
            ElseIf TuesdayTotalOffDutyMealTime > oneandhalfhour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = halfhour
                TuesdayTotalOffDutyMealTimeDT = halfhour
            End If
            If WednesdayTotalOffDutyMealTime <= halfhour Then
                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTime
                WednesdayTotalOffDutyMealTimeOT = 0
                WednesdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > halfhour And WednesdayTotalOffDutyMealTime <= onehour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTime - halfhour
                WednesdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > onehour And WednesdayTotalOffDutyMealTime <= oneandhalfhour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = halfhour
                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTime - onehour
            ElseIf WednesdayTotalOffDutyMealTime > oneandhalfhour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = halfhour
                WednesdayTotalOffDutyMealTimeDT = halfhour
            End If
            If ThursdayTotalOffDutyMealTime <= halfhour Then
                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTime
                ThursdayTotalOffDutyMealTimeOT = 0
                ThursdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > halfhour And ThursdayTotalOffDutyMealTime <= onehour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTime - halfhour
                ThursdayTotalOffDutyMealTimeDT = 0
            ElseIf ThursdayTotalOffDutyMealTime > onehour And ThursdayTotalOffDutyMealTime <= oneandhalfhour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = halfhour
                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTime - onehour
            ElseIf ThursdayTotalOffDutyMealTime > oneandhalfhour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = halfhour
                ThursdayTotalOffDutyMealTimeDT = halfhour
            End If
            If FridayTotalOffDutyMealTime <= halfhour Then
                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTime
                FridayTotalOffDutyMealTimeOT = 0
                FridayTotalOffDutyMealTimeDT = 0
            ElseIf FridayTotalOffDutyMealTime > halfhour And FridayTotalOffDutyMealTime <= onehour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTime - halfhour
                FridayTotalOffDutyMealTimeDT = 0
            ElseIf FridayTotalOffDutyMealTime > onehour And FridayTotalOffDutyMealTime <= oneandhalfhour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = halfhour
                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTime - onehour
            ElseIf FridayTotalOffDutyMealTime > oneandhalfhour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = halfhour
                FridayTotalOffDutyMealTimeDT = halfhour
            End If
            If SaturdayTotalOffDutyMealTime <= halfhour Then
                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTime
                SaturdayTotalOffDutyMealTimeOT = 0
                SaturdayTotalOffDutyMealTimeDT = 0
            ElseIf SaturdayTotalOffDutyMealTime > halfhour And SaturdayTotalOffDutyMealTime <= onehour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTime - halfhour
                SaturdayTotalOffDutyMealTimeDT = 0
            ElseIf SaturdayTotalOffDutyMealTime > onehour And SaturdayTotalOffDutyMealTime <= oneandhalfhour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = halfhour
                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTime - onehour
            ElseIf SaturdayTotalOffDutyMealTime > oneandhalfhour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = halfhour
                SaturdayTotalOffDutyMealTimeDT = halfhour
            End If
            If SundayTotalOffDutyMealTime <= halfhour Then
                SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTime
                SundayTotalOffDutyMealTimeOT = 0
                SundayTotalOffDutyMealTimeDT = 0
            ElseIf SundayTotalOffDutyMealTime > halfhour And SundayTotalOffDutyMealTime <= onehour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTime - halfhour
                SundayTotalOffDutyMealTimeDT = 0
            ElseIf SundayTotalOffDutyMealTime > onehour And SundayTotalOffDutyMealTime <= oneandhalfhour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = halfhour
                SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTime - onehour
            ElseIf SundayTotalOffDutyMealTime > oneandhalfhour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = halfhour
                SundayTotalOffDutyMealTimeDT = halfhour
            End If

            WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
            WeeklyTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeOT
            WeeklyTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + TuesdayTotalOffDutyMealTimeDT + WednesdayTotalOffDutyMealTimeDT + ThursdayTotalOffDutyMealTimeDT + FridayTotalOffDutyMealTimeDT + SaturdayTotalOffDutyMealTimeDT + SundayTotalOffDutyMealTimeDT

            Dim fourhours = 10000000L * 60 * 60 * 4
            Dim eighthours = 10000000L * 60 * 60 * 8
            Dim twelvehours = 10000000L * 60 * 60 * 12
            Dim fourtyhours = 10000000L * 60 * 60 * 40
            Dim WeeklyTotalRegularTime = 0L
            Dim WeeklyTotalOverTime = 0L
            Dim WeeklyTotalDoubleTime = 0L

            If MondayTotalHoursWorked >= twelvehours Then
                MondayTotalDT = MondayTotalHoursWorked - twelvehours
                MondayTotalOT = fourhours
                MondayTotalRT = eighthours
            ElseIf MondayTotalHoursWorked >= eighthours Then
                MondayTotalOT = MondayTotalHoursWorked - eighthours
                MondayTotalRT = eighthours
            Else
                MondayTotalRT = MondayTotalHoursWorked
            End If
            If TuesdayTotalHoursWorked >= twelvehours Then
                TuesdayTotalDT = TuesdayTotalHoursWorked - twelvehours
                TuesdayTotalOT = fourhours
                TuesdayTotalRT = eighthours
            ElseIf TuesdayTotalHoursWorked >= eighthours Then
                TuesdayTotalOT = TuesdayTotalHoursWorked - eighthours
                TuesdayTotalRT = eighthours
            Else
                TuesdayTotalRT = TuesdayTotalHoursWorked
            End If
            If WednesdayTotalHoursWorked >= twelvehours Then
                WednesdayTotalDT = WednesdayTotalHoursWorked - twelvehours
                WednesdayTotalOT = fourhours
                WednesdayTotalRT = eighthours
            ElseIf WednesdayTotalHoursWorked >= eighthours Then
                WednesdayTotalOT = WednesdayTotalHoursWorked - eighthours
                WednesdayTotalRT = eighthours
            Else
                WednesdayTotalRT = WednesdayTotalHoursWorked
            End If
            If ThursdayTotalHoursWorked >= twelvehours Then
                ThursdayTotalDT = ThursdayTotalHoursWorked - twelvehours
                ThursdayTotalOT = fourhours
                ThursdayTotalRT = eighthours
            ElseIf ThursdayTotalHoursWorked >= eighthours Then
                ThursdayTotalOT = ThursdayTotalHoursWorked - eighthours
                ThursdayTotalRT = eighthours
            Else
                ThursdayTotalRT = ThursdayTotalHoursWorked
            End If
            If FridayTotalHoursWorked >= twelvehours Then
                FridayTotalDT = FridayTotalHoursWorked - twelvehours
                FridayTotalOT = fourhours
                FridayTotalRT = eighthours
            ElseIf FridayTotalHoursWorked >= eighthours Then
                FridayTotalOT = FridayTotalHoursWorked - eighthours
                FridayTotalRT = eighthours
            Else
                FridayTotalRT = FridayTotalHoursWorked
            End If
            If SaturdayTotalHoursWorked >= twelvehours Then
                SaturdayTotalDT = SaturdayTotalHoursWorked - twelvehours
                SaturdayTotalOT = fourhours
                SaturdayTotalRT = eighthours
            ElseIf SaturdayTotalHoursWorked >= eighthours Then
                SaturdayTotalOT = SaturdayTotalHoursWorked - eighthours
                SaturdayTotalRT = eighthours
            Else
                SaturdayTotalRT = SaturdayTotalHoursWorked
            End If
            If SundayTotalHoursWorked >= twelvehours Then
                SundayTotalDT = SundayTotalHoursWorked - twelvehours
                SundayTotalOT = fourhours
                SundayTotalRT = eighthours
            ElseIf SundayTotalHoursWorked >= eighthours Then
                SundayTotalOT = SundayTotalHoursWorked - eighthours
                SundayTotalRT = eighthours
            Else
                SundayTotalRT = SundayTotalHoursWorked
            End If

            WeeklyTotalRegularTime = MondayTotalRT + TuesdayTotalRT + WednesdayTotalRT + ThursdayTotalRT + FridayTotalRT + SaturdayTotalRT + SundayTotalRT
            WeeklyTotalOverTime = MondayTotalOT + TuesdayTotalOT + WednesdayTotalOT + ThursdayTotalOT + FridayTotalOT + SaturdayTotalOT + SundayTotalOT
            WeeklyTotalDoubleTime = MondayTotalDT + TuesdayTotalDT + WednesdayTotalDT + ThursdayTotalDT + FridayTotalDT + SaturdayTotalDT + SundayTotalDT

            If WeeklyTotalHoursWorked - fourtyhours > WeeklyTotalOverTime Then
                WeeklyTotalOverTime = WeeklyTotalHoursWorked - fourtyhours - WeeklyTotalDoubleTime
                WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalOverTime
            End If

            '' Calculate HT and HT_OffDutyMeal for each day of week
            Dim MondayTotalHT As Double = 0.0
            Dim TuesdayTotalHT As Double = 0.0
            Dim WednesdayTotalHT As Double = 0.0
            Dim ThursdayTotalHT As Double = 0.0
            Dim FridayTotalHT As Double = 0.0
            Dim SaturdayTotalHT As Double = 0.0
            Dim SundayTotalHT As Double = 0.0
            Dim WeeklyTotalHolidayTime As Double = 0.0
            Dim MondayTotalOffDutyMealTimeHT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeHT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeHT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeHT As Double = 0.0
            Dim _date As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date = DateTime.Parse(weekOfDate.Text)
                While _date.DayOfWeek <> DayOfWeek.Monday
                    _date = _date.AddDays(-1)
                End While
            End If
            If dateIsHoliday(_date) Then
                MondayTotalHT = MondayTotalRT
                MondayTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeRT
                MondayTotalRT = 0
                MondayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                TuesdayTotalHT = TuesdayTotalRT
                TuesdayTotalOffDutyMealTimeHT = TuesdayTotalOffDutyMealTimeRT
                TuesdayTotalRT = 0
                TuesdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                WednesdayTotalHT = WednesdayTotalRT
                WednesdayTotalOffDutyMealTimeHT = WednesdayTotalOffDutyMealTimeRT
                WednesdayTotalRT = 0
                WednesdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                ThursdayTotalHT = ThursdayTotalRT
                ThursdayTotalOffDutyMealTimeHT = ThursdayTotalOffDutyMealTimeRT
                ThursdayTotalRT = 0
                ThursdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                FridayTotalHT = FridayTotalRT
                FridayTotalOffDutyMealTimeHT = FridayTotalOffDutyMealTimeRT
                FridayTotalRT = 0
                FridayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                SaturdayTotalHT = SaturdayTotalRT
                SaturdayTotalOffDutyMealTimeHT = SaturdayTotalOffDutyMealTimeRT
                SaturdayTotalRT = 0
                SaturdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                SundayTotalHT = SundayTotalRT
                SundayTotalOffDutyMealTimeHT = SundayTotalOffDutyMealTimeRT
                SundayTotalRT = 0
                SundayTotalOffDutyMealTimeRT = 0
            End If
            WeeklyTotalHolidayTime = MondayTotalHT + TuesdayTotalHT + WednesdayTotalHT + ThursdayTotalHT + FridayTotalHT + SaturdayTotalHT + SundayTotalHT
            WeeklyTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeHT + TuesdayTotalOffDutyMealTimeHT + WednesdayTotalOffDutyMealTimeHT + ThursdayTotalOffDutyMealTimeHT + FridayTotalOffDutyMealTimeHT + SaturdayTotalOffDutyMealTimeHT + SundayTotalOffDutyMealTimeHT


            Dim MondayTotalHoursWorkedMinusLunches As Double = MondayTotalHoursWorked - MondayTotalOffDutyMealTime
            Dim TuesdayTotalHoursWorkedMinusLunches As Double = TuesdayTotalHoursWorked - TuesdayTotalOffDutyMealTime
            Dim WednesdayTotalHoursWorkedMinusLunches As Double = WednesdayTotalHoursWorked - WednesdayTotalOffDutyMealTime
            Dim ThursdayTotalHoursWorkedMinusLunches As Double = ThursdayTotalHoursWorked - ThursdayTotalOffDutyMealTime
            Dim FridayTotalHoursWorkedMinusLunches As Double = FridayTotalHoursWorked - FridayTotalOffDutyMealTime
            Dim SaturdayTotalHoursWorkedMinusLunches As Double = SaturdayTotalHoursWorked - SaturdayTotalOffDutyMealTime
            Dim SundayTotalHoursWorkedMinusLunches As Double = SundayTotalHoursWorked - SundayTotalOffDutyMealTime

            '' Subtract HT from RT
            WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalHolidayTime
            '' Recalculate Total MealTimes RT 
            WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
            '' Calculate Total MealTime
            WeeklyTotalOffDutyMealTime = WeeklyTotalOffDutyMealTimeRT + WeeklyTotalOffDutyMealTimeOT + WeeklyTotalOffDutyMealTimeDT + WeeklyTotalOffDutyMealTimeHT
            '' Calculate WeeklyTotalHoursWorked Minus Lunches
            Dim WeeklyTotalHoursWorkedMinusLunches As Double = WeeklyTotalHoursWorked - WeeklyTotalOffDutyMealTime
            Dim WeeklyTotalRegularTimeMinusLunches As Double = WeeklyTotalRegularTime - WeeklyTotalOffDutyMealTimeRT
            Dim WeeklyTotalHolidayTimeMinusLunches As Double = WeeklyTotalHolidayTime - WeeklyTotalOffDutyMealTimeHT
            Dim WeeklyTotalOverTimeMinusLunches As Double = WeeklyTotalOverTime - WeeklyTotalOffDutyMealTimeOT
            Dim WeeklyTotalDoubleTimeMinusLunches As Double = WeeklyTotalDoubleTime - WeeklyTotalOffDutyMealTimeDT

            '' Running Totals +=     Do not reset to 0.0 every iteration
            MondayTotalHours += MondayTotalHoursWorked
            MondayTotalOnDutyHours += MondayTotalHoursWorkedMinusLunches
            MondayTotalOffDutyHours += MondayTotalOffDutyMealTime
            TuesdayTotalHours += TuesdayTotalHoursWorked
            TuesdayTotalOnDutyHours += TuesdayTotalHoursWorkedMinusLunches
            TuesdayTotalOffDutyHours += TuesdayTotalOffDutyMealTime
            WednesdayTotalHours += WednesdayTotalHoursWorked
            WednesdayTotalOnDutyHours += WednesdayTotalHoursWorkedMinusLunches
            WednesdayTotalOffDutyHours += WednesdayTotalOffDutyMealTime
            ThursdayTotalHours += ThursdayTotalHoursWorked
            ThursdayTotalOnDutyHours += ThursdayTotalHoursWorkedMinusLunches
            ThursdayTotalOffDutyHours += ThursdayTotalOffDutyMealTime
            FridayTotalHours += FridayTotalHoursWorked
            FridayTotalOnDutyHours += FridayTotalHoursWorkedMinusLunches
            FridayTotalOffDutyHours += FridayTotalOffDutyMealTime
            SaturdayTotalHours += SaturdayTotalHoursWorked
            SaturdayTotalOnDutyHours += SaturdayTotalHoursWorkedMinusLunches
            SaturdayTotalOffDutyHours += SaturdayTotalOffDutyMealTime
            SundayTotalHours += SundayTotalHoursWorked
            SundayTotalOnDutyHours += SundayTotalHoursWorkedMinusLunches
            SundayTotalOffDutyHours += SundayTotalOffDutyMealTime
            WeeklyTotalHours += WeeklyTotalHoursWorked
            WeeklyTotalOnDutyHours += WeeklyTotalHoursWorkedMinusLunches
            WeeklyTotalOffDutyHours += WeeklyTotalOffDutyMealTime
            WeeklyTotalRTHours += WeeklyTotalRegularTime
            WeeklyTotalRTOnDutyHours += WeeklyTotalRegularTimeMinusLunches
            WeeklyTotalRTOffDutyHours += WeeklyTotalOffDutyMealTimeRT
            WeeklyTotalHTHours += WeeklyTotalHolidayTime
            WeeklyTotalHTOnDutyHours += WeeklyTotalHolidayTimeMinusLunches
            WeeklyTotalHTOffDutyHours += WeeklyTotalOffDutyMealTimeHT
            WeeklyTotalOTHours += WeeklyTotalOverTime
            WeeklyTotalOTOnDutyHours += WeeklyTotalOverTimeMinusLunches
            WeeklyTotalOTOffDutyHours += WeeklyTotalOffDutyMealTimeOT
            WeeklyTotalDTHours += WeeklyTotalDoubleTime
            WeeklyTotalDTOnDutyHours += WeeklyTotalDoubleTimeMinusLunches
            WeeklyTotalDTOffDutyHours += WeeklyTotalOffDutyMealTimeDT
            Dim basepayrate As Double = Convert.ToInt64(Decimal.Parse(employee.ItemArray(1)))
            WeeklyTotalDollars += (TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours*2.0) * (basepayrate)
            WeeklyTotalOnDutyDollars += (TimeSpan.FromTicks(WeeklyTotalRegularTimeMinuslunches).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinuslunches).TotalHours*2.0) * (basepayrate)
            WeeklyTotalOffDutyDollars += (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours*2.0) * (basepayrate)

        Next

        'Create datatable and columns
        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("rowtitle"))
        dtable.Columns.Add(New DataColumn("mondaytime"))
        dtable.Columns.Add(New DataColumn("tuesdaytime"))
        dtable.Columns.Add(New DataColumn("wednesdaytime"))
        dtable.Columns.Add(New DataColumn("thursdaytime"))
        dtable.Columns.Add(New DataColumn("fridaytime"))
        dtable.Columns.Add(New DataColumn("saturdaytime"))
        dtable.Columns.Add(New DataColumn("sundaytime"))
        dtable.Columns.Add(New DataColumn("all_total"))
        dtable.Columns.Add(New DataColumn("rt_total"))
        dtable.Columns.Add(New DataColumn("ht_total"))
        dtable.Columns.Add(New DataColumn("ot_total"))
        dtable.Columns.Add(New DataColumn("dt_total"))
        dtable.Columns.Add(New DataColumn("totaldollars"))
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "RT")).Visible = True
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "HT")).Visible = True
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "OT")).Visible = True
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "DT")).Visible = True
        ExcaliburTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "$ Total")).Visible = True
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "RT")).Visible = True
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "HT")).Visible = True
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "OT")).Visible = True
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "DT")).Visible = True
        MattsTotalsGridView.Columns(GetColumnIndexByHeaderText(ExcaliburTotalsGridView, "$ Total")).Visible = True

        If WeeklyTotalHours = 0 Then
            If company = "Excalibur" Then
                ExcaliburTotalsGridView.Visible = False
            Else 
                MattsTotalsGridView.Visible = False
            End If
        Else
            If company = "Excalibur" Then
                ExcaliburTotalsGridView.Visible = True
            Else 
                MattsTotalsGridView.Visible = True
            End If
        End If

        ' Add Rows
        Dim RowValues As Object() = {"", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        RowValues(0) = "Total Hours"
        RowValues(1) = If((MondayTotalHours) = 0L, "0", TimeSpan.FromTicks(MondayTotalHours).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHours).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHours).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHours).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalHours = 0L, "0", TimeSpan.FromTicks(FridayTotalHours).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHours).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalHours = 0L, "0", TimeSpan.FromTicks(SundayTotalHours).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHours).TotalHours.ToString("N2"))
        RowValues(9) = If(WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalRTHours).TotalHours.ToString("N2"))
        RowValues(10) = If(WeeklyTotalHTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHTHours).TotalHours.ToString("N2"))
        RowValues(11) = If(WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOTHours).TotalHours.ToString("N2"))
        RowValues(12) = If(WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalDTHours).TotalHours.ToString("N2"))
        RowValues(13) = If(WeeklyTotalDollars = 0L, "0", "$ "+WeeklyTotalDollars.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()
        RowValues(0) = "On Duty Hours Worked"
        RowValues(1) = If((MondayTotalOnDutyHours) = 0L, "0", TimeSpan.FromTicks(MondayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(TuesdayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(WednesdayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(ThursdayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(FridayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(SaturdayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(SundayTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalOnDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOnDutyHours).TotalHours.ToString("N2"))
        RowValues(9) = If(WeeklyTotalRTOnDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalRTOnDutyHours).TotalHours.ToString("N2"))
        RowValues(10) = If(WeeklyTotalHTOnDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHTOnDutyHours).TotalHours.ToString("N2"))
        RowValues(11) = If(WeeklyTotalOTOnDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOTOnDutyHours).TotalHours.ToString("N2"))
        RowValues(12) = If(WeeklyTotalDTOnDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalDTOnDutyHours).TotalHours.ToString("N2"))
        RowValues(13) = If(WeeklyTotalOnDutyDollars = 0L, "0", "$ "+WeeklyTotalOnDutyDollars.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()
        RowValues(0) = "Off Duty Meal Time"
        RowValues(1) = If(MondayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(MondayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(2) = If(TuesdayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(TuesdayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(3) = If(WednesdayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(WednesdayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(4) = If(ThursdayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(ThursdayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(5) = If(FridayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(FridayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(6) = If(SaturdayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(SaturdayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(7) = If(SundayTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(SundayTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(8) = If(WeeklyTotalOffDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyHours).TotalHours.ToString("N2"))
        RowValues(9) = If(WeeklyTotalRTOffDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalRTOffDutyHours).TotalHours.ToString("N2"))
        RowValues(10) = If(WeeklyTotalHTOffDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHTOffDutyHours).TotalHours.ToString("N2"))
        RowValues(11) = If(WeeklyTotalOTOffDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOTOffDutyHours).TotalHours.ToString("N2"))
        RowValues(12) = If(WeeklyTotalDTOffDutyHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalDTOffDutyHours).TotalHours.ToString("N2"))
        RowValues(13) = If(WeeklyTotalOffDutyDollars = 0L, "0", "$ "+WeeklyTotalOffDutyDollars.ToString("N2"))
        dtable.Rows.Add(RowValues)
        dtable.AcceptChanges()

        'now bind datatable to gridview... 
        if company = "Excalibur" Then
            ExcaliburTotalsGridView.DataSource = dtable
            ExcaliburTotalsGridView.DataBind()
        Else 
            MattsTotalsGridView.DataSource = dtable
            MattsTotalsGridView.DataBind()
        End If

    End Sub
    Protected Sub updateClientTotalsTable_AllEmployeesView()
        
        'Create datatable and columns
        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("company"))
        dtable.Columns.Add(New DataColumn("clientsite"))
        dtable.Columns.Add(New DataColumn("rate"))
        dtable.Columns.Add(New DataColumn("client_cost"))
        dtable.Columns.Add(New DataColumn("rt_total"))
        dtable.Columns.Add(New DataColumn("ot_total"))
        dtable.Columns.Add(New DataColumn("dt_total"))
        dtable.Columns.Add(New DataColumn("all_total"))
        dtable.Columns.Add(New DataColumn("mondaytime"))
        dtable.Columns.Add(New DataColumn("tuesdaytime"))
        dtable.Columns.Add(New DataColumn("wednesdaytime"))
        dtable.Columns.Add(New DataColumn("thursdaytime"))
        dtable.Columns.Add(New DataColumn("fridaytime"))
        dtable.Columns.Add(New DataColumn("saturdaytime"))
        dtable.Columns.Add(New DataColumn("sundaytime"))

        #Region "Running Totals Per Company"
        Dim ex_WeeklyTotalDollars as Double = 0.0
        Dim ex_WeeklyTotalRTHours as Double = 0.0
        Dim ex_WeeklyTotalOTHours as Double = 0.0
        Dim ex_WeeklyTotalDTHours as Double = 0.0
        Dim ex_WeeklyTotalHours as Double = 0.0
        Dim ex_MondayTotalHours as Double = 0.0
        Dim ex_TuesdayTotalHours as Double = 0.0
        Dim ex_WednesdayTotalHours as Double = 0.0
        Dim ex_ThursdayTotalHours as Double = 0.0
        Dim ex_FridayTotalHours as Double = 0.0
        Dim ex_SaturdayTotalHours as Double = 0.0
        Dim ex_SundayTotalHours as Double = 0.0
        Dim ms_WeeklyTotalDollars as Double = 0.0
        Dim ms_WeeklyTotalRTHours as Double = 0.0
        Dim ms_WeeklyTotalOTHours as Double = 0.0
        Dim ms_WeeklyTotalDTHours as Double = 0.0
        Dim ms_WeeklyTotalHours as Double = 0.0
        Dim ms_MondayTotalHours as Double = 0.0
        Dim ms_TuesdayTotalHours as Double = 0.0
        Dim ms_WednesdayTotalHours as Double = 0.0
        Dim ms_ThursdayTotalHours as Double = 0.0
        Dim ms_FridayTotalHours as Double = 0.0
        Dim ms_SaturdayTotalHours as Double = 0.0
        Dim ms_SundayTotalHours as Double = 0.0
        #End Region

        '' Get Table of Clients dt5
        Dim dt5 As New DataTable()
        Dim sqlCmd5 As New SqlCommand("SELECT client_site_id, payrate, company, name FROM client_site_tbl WHERE status='Active' ORDER BY name ASC", conn)
        Dim sqlDa5 As New SqlDataAdapter(sqlCmd5)
        conn.close()
        conn.open()
        sqlDa5.Fill(dt5)
        conn.close()

        For Each clientsite in dt5.Rows

            Dim clientsite_id as string = clientsite.ItemArray(0).ToString()
            Dim clientsite_payrate as Double = Double.Parse(clientsite.ItemArray(1))
            Dim clientsite_company as string = clientsite.ItemArray(2).ToString()
            Dim clientsite_name as string = clientsite.ItemArray(3).ToString()

            #Region "Totals Variables"
            Dim MondayTotalHours As Double
            Dim MondayTotalOnDutyHours As Double
            Dim MondayTotalOffDutyHours As Double
            Dim TuesdayTotalHours As Double
            Dim TuesdayTotalOnDutyHours As Double
            Dim TuesdayTotalOffDutyHours As Double
            Dim WednesdayTotalHours As Double
            Dim WednesdayTotalOnDutyHours As Double
            Dim WednesdayTotalOffDutyHours As Double
            Dim ThursdayTotalHours As Double
            Dim ThursdayTotalOnDutyHours As Double
            Dim ThursdayTotalOffDutyHours As Double
            Dim FridayTotalHours As Double
            Dim FridayTotalOnDutyHours As Double
            Dim FridayTotalOffDutyHours As Double
            Dim SaturdayTotalHours As Double
            Dim SaturdayTotalOnDutyHours As Double
            Dim SaturdayTotalOffDutyHours As Double
            Dim SundayTotalHours As Double
            Dim SundayTotalOnDutyHours As Double
            Dim SundayTotalOffDutyHours As Double
            Dim WeeklyTotalHours As Double
            Dim WeeklyTotalOnDutyHours As Double
            Dim WeeklyTotalOffDutyHours As Double
            Dim WeeklyTotalRTHours As Double
            Dim WeeklyTotalRTOnDutyHours As Double
            Dim WeeklyTotalRTOffDutyHours As Double
            Dim WeeklyTotalHTHours As Double
            Dim WeeklyTotalHTOnDutyHours As Double
            Dim WeeklyTotalHTOffDutyHours As Double
            Dim WeeklyTotalOTHours As Double
            Dim WeeklyTotalOTOnDutyHours As Double
            Dim WeeklyTotalOTOffDutyHours As Double
            Dim WeeklyTotalDTHours As Double
            Dim WeeklyTotalDTOnDutyHours As Double
            Dim WeeklyTotalDTOffDutyHours As Double
            Dim WeeklyTotalDollars As Double
            Dim WeeklyTotalOnDutyDollars As Double
            Dim WeeklyTotalOffDutyDollars As Double

            Dim rt_total as double = 0.0
            Dim ot_total as double = 0.0
            Dim dt_total as double = 0.0

            Dim halfhour = 10000000L * 60 * 60 * 0.5
            Dim onehour = 10000000L * 60 * 60 * 1
            Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5
            Dim fourhours = 10000000L * 60 * 60 * 4
            Dim eighthours = 10000000L * 60 * 60 * 8
            Dim twelvehours = 10000000L * 60 * 60 * 12
            Dim fourtyhours = 10000000L * 60 * 60 * 40

            #End Region

            '' Fill the totals table
            #Region "Declare Variables"
            Dim MondayTotalHoursWorked As Double = 0.0
            Dim TuesdayTotalHoursWorked As Double = 0.0
            Dim WednesdayTotalHoursWorked As Double = 0.0
            Dim ThursdayTotalHoursWorked As Double = 0.0
            Dim FridayTotalHoursWorked As Double = 0.0
            Dim SaturdayTotalHoursWorked As Double = 0.0
            Dim SundayTotalHoursWorked As Double = 0.0
            Dim WeeklyTotalHoursWorked As Double = 0.0

            Dim MondayTotalRT As Double = 0.0
            Dim TuesdayTotalRT As Double = 0.0
            Dim WednesdayTotalRT As Double = 0.0
            Dim ThursdayTotalRT As Double = 0.0
            Dim FridayTotalRT As Double = 0.0
            Dim SaturdayTotalRT As Double = 0.0
            Dim SundayTotalRT As Double = 0.0
            Dim MondayTotalOT As Double = 0.0
            Dim TuesdayTotalOT As Double = 0.0
            Dim WednesdayTotalOT As Double = 0.0
            Dim ThursdayTotalOT As Double = 0.0
            Dim FridayTotalOT As Double = 0.0
            Dim SaturdayTotalOT As Double = 0.0
            Dim SundayTotalOT As Double = 0.0
            Dim MondayTotalDT As Double = 0.0
            Dim TuesdayTotalDT As Double = 0.0
            Dim WednesdayTotalDT As Double = 0.0
            Dim ThursdayTotalDT As Double = 0.0
            Dim FridayTotalDT As Double = 0.0
            Dim SaturdayTotalDT As Double = 0.0
            Dim SundayTotalDT As Double = 0.0

            Dim MondayTotalOffDutyMealTimeRT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeRT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeRT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeRT As Double = 0.0
            Dim MondayTotalOffDutyMealTimeOT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeOT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeOT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeOT As Double = 0.0
            Dim MondayTotalOffDutyMealTimeDT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeDT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeDT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeDT As Double = 0.0
            Dim MondayTotalOffDutyMealTime As Double = 0.0
            Dim TuesdayTotalOffDutyMealTime As Double = 0.0
            Dim WednesdayTotalOffDutyMealTime As Double = 0.0
            Dim ThursdayTotalOffDutyMealTime As Double = 0.0
            Dim FridayTotalOffDutyMealTime As Double = 0.0
            Dim SaturdayTotalOffDutyMealTime As Double = 0.0
            Dim SundayTotalOffDutyMealTime As Double = 0.0
            Dim WeeklyTotalOffDutyMealTime As Double = 0.0
            #End Region

            Dim dt As New DataTable()
            Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE client_site_shift_instance_tbl.client_site_id='" + clientsite_id + "' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
            Dim sqlDa As New SqlDataAdapter(sqlCmd)
            conn.close()
            conn.open()
            sqlDa.Fill(dt)
            conn.close()

            Dim _date_ As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date_ = DateTime.Parse(weekOfDate.Text)
                While _date_.DayOfWeek <> DayOfWeek.Monday
                    _date_ = _date_.AddDays(-1)
                End While
            End If

            #Region "For Each Shift"
            Dim shifts_worked As Integer = dt.Rows.Count
            For Each row In dt.Rows
                Dim wholespan As Double = 0.0
                Dim firstspan As Double = 0.0
                Dim secondspan As Double = 0.0
                Dim holidayspan As Double = 0.0
                Dim startdatetime = DateTime.Parse(row(4))
                Dim enddatetime = DateTime.Parse(row(5))
                wholespan = enddatetime.Ticks - startdatetime.Ticks

                '' Figure total hours worked by day of week
                If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                    firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                    secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                    If (dateIsHoliday(enddatetime)) Then holidayspan = secondspan
                    If (dateIsHoliday(startdatetime)) Then holidayspan = firstspan
                    Select Case startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If enddatetime.Date = _date_.Date Then
                                MondayTotalHoursWorked += secondspan
                            Else
                                SundayTotalHoursWorked += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalHoursWorked += firstspan
                            TuesdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalHoursWorked += firstspan
                            WednesdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalHoursWorked += firstspan
                            ThursdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalHoursWorked += firstspan
                            FridayTotalHoursWorked += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalHoursWorked += firstspan
                            SaturdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalHoursWorked += firstspan
                            SundayTotalHoursWorked += secondspan
                    End Select
                Else
                    If (dateIsHoliday(enddatetime)) Then holidayspan = wholespan
                    Select Case startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            SundayTotalHoursWorked += wholespan
                        Case DayOfWeek.Monday
                            MondayTotalHoursWorked += wholespan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalHoursWorked += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalHoursWorked += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalHoursWorked += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalHoursWorked += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalHoursWorked += wholespan
                    End Select
                End If

                '' Client Billable Hours are by shift... Not by Sun12:00-Sun12:00
                If (Not ((startdatetime.DayOfWeek = DayOfWeek.Sunday) And (enddatetime.Date = _date_.Date))) Then
                    If wholespan >= twelvehours Then
                        dt_total += wholespan - twelvehours
                        ot_total += fourhours
                        rt_total += eighthours
                    ElseIf wholespan >= eighthours Then
                        ot_total += wholespan - eighthours
                        rt_total += eighthours
                    Else
                        rt_total += wholespan
                    End If

                    '' How many hours of this shift land on a holiday? holidayspan    <-- Subtract that from RT, add to OT
                    if (holidayspan >= eighthours) then
                        ot_total += eighthours
                        rt_total = 0.0
                    else
                        rt_total -= holidayspan
                        ot_total += holidayspan
                    End If
                End If

                '' Figure total off duty lunch time by day of week
                If row(7) = False Then
                    Dim firstmeal_startdatetime As DateTime
                    Dim firstmeal_enddatetime As DateTime
                    Dim secondmeal_startdatetime As DateTime
                    Dim secondmeal_enddatetime As DateTime
                    Dim thirdmeal_startdatetime As DateTime
                    Dim thirdmeal_enddatetime As DateTime

                    Try
                        firstmeal_startdatetime = DateTime.Parse(row(8))
                    Catch ex As Exception
                        firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        firstmeal_enddatetime = DateTime.Parse(row(9))
                    Catch ex As Exception
                        firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        secondmeal_startdatetime = DateTime.Parse(row(10))
                    Catch ex As Exception
                        secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        secondmeal_enddatetime = DateTime.Parse(row(11))
                    Catch ex As Exception
                        secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        thirdmeal_startdatetime = DateTime.Parse(row(12))
                    Catch ex As Exception
                        thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        thirdmeal_enddatetime = DateTime.Parse(row(13))
                    Catch ex As Exception
                        thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try

                    If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                        firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                        secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                        Select Case firstmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + secondspan
                                Else
                                    SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTimeRT + firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + firstspan
                                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTimeRT + firstspan
                                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTimeRT + firstspan
                                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTimeRT + firstspan
                                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTimeRT + firstspan
                                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTimeRT + secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTimeRT + firstspan
                                SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTimeRT + secondspan
                        End Select
                    Else
                        wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                        Select Case firstmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If firstmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTimeRT + wholespan
                                End If
                            Case DayOfWeek.Monday
                                If firstmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTimeRT + wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTimeRT + wholespan
                        End Select
                    End If
                    If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                        firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                        secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                        Select Case secondmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If secondmeal_enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + secondspan
                                Else
                                    SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTimeOT + firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + firstspan
                                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTimeOT + firstspan
                                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTimeOT + firstspan
                                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTimeOT + firstspan
                                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTimeOT + firstspan
                                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTimeOT + secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTimeOT + firstspan
                                SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTimeOT + secondspan
                        End Select
                    Else
                        wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                        Select Case secondmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If secondmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTimeOT + wholespan
                                End If
                            Case DayOfWeek.Monday
                                If secondmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTimeOT + wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTimeOT + wholespan
                        End Select
                    End If
                    If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                        firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                        secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                        Select Case thirdmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If thirdmeal_enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + secondspan
                                Else
                                    SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTimeDT + firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + firstspan
                                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTimeDT + firstspan
                                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTimeDT + firstspan
                                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTimeDT + firstspan
                                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTimeDT + firstspan
                                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTimeDT + secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTimeDT + firstspan
                                SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTimeDT + secondspan
                        End Select
                    Else
                        wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                        Select Case thirdmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If thirdmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTimeDT + wholespan
                                End If
                            Case DayOfWeek.Monday
                                If thirdmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTimeDT + wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTimeDT + wholespan
                        End Select
                    End If
                End If

            Next
            #End Region

            #Region "Calculate Totals"
            WeeklyTotalHoursWorked = MondayTotalHoursWorked + TuesdayTotalHoursWorked + WednesdayTotalHoursWorked + ThursdayTotalHoursWorked + FridayTotalHoursWorked + SaturdayTotalHoursWorked + SundayTotalHoursWorked

            MondayTotalOffDutyMealTime = MondayTotalOffDutyMealTimeRT + MondayTotalOffDutyMealTimeOT + MondayTotalOffDutyMealTimeDT
            TuesdayTotalOffDutyMealTime = TuesdayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeDT
            WednesdayTotalOffDutyMealTime = WednesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeDT
            ThursdayTotalOffDutyMealTime = ThursdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeDT
            FridayTotalOffDutyMealTime = FridayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeDT
            SaturdayTotalOffDutyMealTime = SaturdayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeDT
            SundayTotalOffDutyMealTime = SundayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeDT

            #Region "Off Duty Meal Times by RT/OT/DT"
            If MondayTotalOffDutyMealTime <= halfhour Then
                MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTime
                MondayTotalOffDutyMealTimeOT = 0
                MondayTotalOffDutyMealTimeDT = 0
            ElseIf MondayTotalOffDutyMealTime > halfhour And MondayTotalOffDutyMealTime <= onehour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTime - halfhour
                MondayTotalOffDutyMealTimeDT = 0
            ElseIf MondayTotalOffDutyMealTime > onehour And MondayTotalOffDutyMealTime <= oneandhalfhour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = halfhour
                MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTime - onehour
            ElseIf MondayTotalOffDutyMealTime > oneandhalfhour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = halfhour
                MondayTotalOffDutyMealTimeDT = halfhour
            End If
            If TuesdayTotalOffDutyMealTime <= halfhour Then
                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTime
                TuesdayTotalOffDutyMealTimeOT = 0
                TuesdayTotalOffDutyMealTimeDT = 0
            ElseIf TuesdayTotalOffDutyMealTime > halfhour And TuesdayTotalOffDutyMealTime <= onehour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTime - halfhour
                TuesdayTotalOffDutyMealTimeDT = 0
            ElseIf TuesdayTotalOffDutyMealTime > onehour And TuesdayTotalOffDutyMealTime <= oneandhalfhour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = halfhour
                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTime - onehour
            ElseIf TuesdayTotalOffDutyMealTime > oneandhalfhour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = halfhour
                TuesdayTotalOffDutyMealTimeDT = halfhour
            End If
            If WednesdayTotalOffDutyMealTime <= halfhour Then
                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTime
                WednesdayTotalOffDutyMealTimeOT = 0
                WednesdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > halfhour And WednesdayTotalOffDutyMealTime <= onehour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTime - halfhour
                WednesdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > onehour And WednesdayTotalOffDutyMealTime <= oneandhalfhour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = halfhour
                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTime - onehour
            ElseIf WednesdayTotalOffDutyMealTime > oneandhalfhour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = halfhour
                WednesdayTotalOffDutyMealTimeDT = halfhour
            End If
            If ThursdayTotalOffDutyMealTime <= halfhour Then
                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTime
                ThursdayTotalOffDutyMealTimeOT = 0
                ThursdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > halfhour And ThursdayTotalOffDutyMealTime <= onehour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTime - halfhour
                ThursdayTotalOffDutyMealTimeDT = 0
            ElseIf ThursdayTotalOffDutyMealTime > onehour And ThursdayTotalOffDutyMealTime <= oneandhalfhour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = halfhour
                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTime - onehour
            ElseIf ThursdayTotalOffDutyMealTime > oneandhalfhour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = halfhour
                ThursdayTotalOffDutyMealTimeDT = halfhour
            End If
            If FridayTotalOffDutyMealTime <= halfhour Then
                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTime
                FridayTotalOffDutyMealTimeOT = 0
                FridayTotalOffDutyMealTimeDT = 0
            ElseIf FridayTotalOffDutyMealTime > halfhour And FridayTotalOffDutyMealTime <= onehour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTime - halfhour
                FridayTotalOffDutyMealTimeDT = 0
            ElseIf FridayTotalOffDutyMealTime > onehour And FridayTotalOffDutyMealTime <= oneandhalfhour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = halfhour
                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTime - onehour
            ElseIf FridayTotalOffDutyMealTime > oneandhalfhour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = halfhour
                FridayTotalOffDutyMealTimeDT = halfhour
            End If
            If SaturdayTotalOffDutyMealTime <= halfhour Then
                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTime
                SaturdayTotalOffDutyMealTimeOT = 0
                SaturdayTotalOffDutyMealTimeDT = 0
            ElseIf SaturdayTotalOffDutyMealTime > halfhour And SaturdayTotalOffDutyMealTime <= onehour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTime - halfhour
                SaturdayTotalOffDutyMealTimeDT = 0
            ElseIf SaturdayTotalOffDutyMealTime > onehour And SaturdayTotalOffDutyMealTime <= oneandhalfhour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = halfhour
                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTime - onehour
            ElseIf SaturdayTotalOffDutyMealTime > oneandhalfhour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = halfhour
                SaturdayTotalOffDutyMealTimeDT = halfhour
            End If
            If SundayTotalOffDutyMealTime <= halfhour Then
                SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTime
                SundayTotalOffDutyMealTimeOT = 0
                SundayTotalOffDutyMealTimeDT = 0
            ElseIf SundayTotalOffDutyMealTime > halfhour And SundayTotalOffDutyMealTime <= onehour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTime - halfhour
                SundayTotalOffDutyMealTimeDT = 0
            ElseIf SundayTotalOffDutyMealTime > onehour And SundayTotalOffDutyMealTime <= oneandhalfhour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = halfhour
                SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTime - onehour
            ElseIf SundayTotalOffDutyMealTime > oneandhalfhour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = halfhour
                SundayTotalOffDutyMealTimeDT = halfhour
            End If
            #End Region

            WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
            WeeklyTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeOT
            WeeklyTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + TuesdayTotalOffDutyMealTimeDT + WednesdayTotalOffDutyMealTimeDT + ThursdayTotalOffDutyMealTimeDT + FridayTotalOffDutyMealTimeDT + SaturdayTotalOffDutyMealTimeDT + SundayTotalOffDutyMealTimeDT


            Dim WeeklyTotalRegularTime = 0L
            Dim WeeklyTotalOverTime = 0L
            Dim WeeklyTotalDoubleTime = 0L

            If MondayTotalHoursWorked >= twelvehours Then
                MondayTotalDT = MondayTotalHoursWorked - twelvehours
                MondayTotalOT = fourhours
                MondayTotalRT = eighthours
            ElseIf MondayTotalHoursWorked >= eighthours Then
                MondayTotalOT = MondayTotalHoursWorked - eighthours
                MondayTotalRT = eighthours
            Else
                MondayTotalRT = MondayTotalHoursWorked
            End If
            If TuesdayTotalHoursWorked >= twelvehours Then
                TuesdayTotalDT = TuesdayTotalHoursWorked - twelvehours
                TuesdayTotalOT = fourhours
                TuesdayTotalRT = eighthours
            ElseIf TuesdayTotalHoursWorked >= eighthours Then
                TuesdayTotalOT = TuesdayTotalHoursWorked - eighthours
                TuesdayTotalRT = eighthours
            Else
                TuesdayTotalRT = TuesdayTotalHoursWorked
            End If
            If WednesdayTotalHoursWorked >= twelvehours Then
                WednesdayTotalDT = WednesdayTotalHoursWorked - twelvehours
                WednesdayTotalOT = fourhours
                WednesdayTotalRT = eighthours
            ElseIf WednesdayTotalHoursWorked >= eighthours Then
                WednesdayTotalOT = WednesdayTotalHoursWorked - eighthours
                WednesdayTotalRT = eighthours
            Else
                WednesdayTotalRT = WednesdayTotalHoursWorked
            End If
            If ThursdayTotalHoursWorked >= twelvehours Then
                ThursdayTotalDT = ThursdayTotalHoursWorked - twelvehours
                ThursdayTotalOT = fourhours
                ThursdayTotalRT = eighthours
            ElseIf ThursdayTotalHoursWorked >= eighthours Then
                ThursdayTotalOT = ThursdayTotalHoursWorked - eighthours
                ThursdayTotalRT = eighthours
            Else
                ThursdayTotalRT = ThursdayTotalHoursWorked
            End If
            If FridayTotalHoursWorked >= twelvehours Then
                FridayTotalDT = FridayTotalHoursWorked - twelvehours
                FridayTotalOT = fourhours
                FridayTotalRT = eighthours
            ElseIf FridayTotalHoursWorked >= eighthours Then
                FridayTotalOT = FridayTotalHoursWorked - eighthours
                FridayTotalRT = eighthours
            Else
                FridayTotalRT = FridayTotalHoursWorked
            End If
            If SaturdayTotalHoursWorked >= twelvehours Then
                SaturdayTotalDT = SaturdayTotalHoursWorked - twelvehours
                SaturdayTotalOT = fourhours
                SaturdayTotalRT = eighthours
            ElseIf SaturdayTotalHoursWorked >= eighthours Then
                SaturdayTotalOT = SaturdayTotalHoursWorked - eighthours
                SaturdayTotalRT = eighthours
            Else
                SaturdayTotalRT = SaturdayTotalHoursWorked
            End If
            If SundayTotalHoursWorked >= twelvehours Then
                SundayTotalDT = SundayTotalHoursWorked - twelvehours
                SundayTotalOT = fourhours
                SundayTotalRT = eighthours
            ElseIf SundayTotalHoursWorked >= eighthours Then
                SundayTotalOT = SundayTotalHoursWorked - eighthours
                SundayTotalRT = eighthours
            Else
                SundayTotalRT = SundayTotalHoursWorked
            End If

            WeeklyTotalRegularTime = MondayTotalRT + TuesdayTotalRT + WednesdayTotalRT + ThursdayTotalRT + FridayTotalRT + SaturdayTotalRT + SundayTotalRT
            WeeklyTotalOverTime = MondayTotalOT + TuesdayTotalOT + WednesdayTotalOT + ThursdayTotalOT + FridayTotalOT + SaturdayTotalOT + SundayTotalOT
            WeeklyTotalDoubleTime = MondayTotalDT + TuesdayTotalDT + WednesdayTotalDT + ThursdayTotalDT + FridayTotalDT + SaturdayTotalDT + SundayTotalDT

            If WeeklyTotalHoursWorked - fourtyhours > WeeklyTotalOverTime Then
                WeeklyTotalOverTime = WeeklyTotalHoursWorked - fourtyhours - WeeklyTotalDoubleTime
                WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalOverTime
            End If

            '' Calculate HT and HT_OffDutyMeal for each day of week
            Dim MondayTotalHT As Double = 0.0
            Dim TuesdayTotalHT As Double = 0.0
            Dim WednesdayTotalHT As Double = 0.0
            Dim ThursdayTotalHT As Double = 0.0
            Dim FridayTotalHT As Double = 0.0
            Dim SaturdayTotalHT As Double = 0.0
            Dim SundayTotalHT As Double = 0.0
            Dim WeeklyTotalHolidayTime As Double = 0.0
            Dim MondayTotalOffDutyMealTimeHT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeHT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeHT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeHT As Double = 0.0
            Dim _date As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date = DateTime.Parse(weekOfDate.Text)
                While _date.DayOfWeek <> DayOfWeek.Monday
                    _date = _date.AddDays(-1)
                End While
            End If
            If dateIsHoliday(_date) Then
                MondayTotalHT = MondayTotalRT
                MondayTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeRT
                MondayTotalRT = 0
                MondayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                TuesdayTotalHT = TuesdayTotalRT
                TuesdayTotalOffDutyMealTimeHT = TuesdayTotalOffDutyMealTimeRT
                TuesdayTotalRT = 0
                TuesdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                WednesdayTotalHT = WednesdayTotalRT
                WednesdayTotalOffDutyMealTimeHT = WednesdayTotalOffDutyMealTimeRT
                WednesdayTotalRT = 0
                WednesdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                ThursdayTotalHT = ThursdayTotalRT
                ThursdayTotalOffDutyMealTimeHT = ThursdayTotalOffDutyMealTimeRT
                ThursdayTotalRT = 0
                ThursdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                FridayTotalHT = FridayTotalRT
                FridayTotalOffDutyMealTimeHT = FridayTotalOffDutyMealTimeRT
                FridayTotalRT = 0
                FridayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                SaturdayTotalHT = SaturdayTotalRT
                SaturdayTotalOffDutyMealTimeHT = SaturdayTotalOffDutyMealTimeRT
                SaturdayTotalRT = 0
                SaturdayTotalOffDutyMealTimeRT = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                SundayTotalHT = SundayTotalRT
                SundayTotalOffDutyMealTimeHT = SundayTotalOffDutyMealTimeRT
                SundayTotalRT = 0
                SundayTotalOffDutyMealTimeRT = 0
            End If
            WeeklyTotalHolidayTime = MondayTotalHT + TuesdayTotalHT + WednesdayTotalHT + ThursdayTotalHT + FridayTotalHT + SaturdayTotalHT + SundayTotalHT
            WeeklyTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeHT + TuesdayTotalOffDutyMealTimeHT + WednesdayTotalOffDutyMealTimeHT + ThursdayTotalOffDutyMealTimeHT + FridayTotalOffDutyMealTimeHT + SaturdayTotalOffDutyMealTimeHT + SundayTotalOffDutyMealTimeHT


            Dim MondayTotalHoursWorkedMinusLunches As Double = MondayTotalHoursWorked - MondayTotalOffDutyMealTime
            Dim TuesdayTotalHoursWorkedMinusLunches As Double = TuesdayTotalHoursWorked - TuesdayTotalOffDutyMealTime
            Dim WednesdayTotalHoursWorkedMinusLunches As Double = WednesdayTotalHoursWorked - WednesdayTotalOffDutyMealTime
            Dim ThursdayTotalHoursWorkedMinusLunches As Double = ThursdayTotalHoursWorked - ThursdayTotalOffDutyMealTime
            Dim FridayTotalHoursWorkedMinusLunches As Double = FridayTotalHoursWorked - FridayTotalOffDutyMealTime
            Dim SaturdayTotalHoursWorkedMinusLunches As Double = SaturdayTotalHoursWorked - SaturdayTotalOffDutyMealTime
            Dim SundayTotalHoursWorkedMinusLunches As Double = SundayTotalHoursWorked - SundayTotalOffDutyMealTime

            '' Subtract HT from RT
            WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalHolidayTime
            '' Recalculate Total MealTimes RT 
            WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
            '' Calculate Total MealTime
            WeeklyTotalOffDutyMealTime = WeeklyTotalOffDutyMealTimeRT + WeeklyTotalOffDutyMealTimeOT + WeeklyTotalOffDutyMealTimeDT + WeeklyTotalOffDutyMealTimeHT
            '' Calculate WeeklyTotalHoursWorked Minus Lunches
            Dim WeeklyTotalHoursWorkedMinusLunches As Double = WeeklyTotalHoursWorked - WeeklyTotalOffDutyMealTime
            Dim WeeklyTotalRegularTimeMinusLunches As Double = WeeklyTotalRegularTime - WeeklyTotalOffDutyMealTimeRT
            Dim WeeklyTotalHolidayTimeMinusLunches As Double = WeeklyTotalHolidayTime - WeeklyTotalOffDutyMealTimeHT
            Dim WeeklyTotalOverTimeMinusLunches As Double = WeeklyTotalOverTime - WeeklyTotalOffDutyMealTimeOT
            Dim WeeklyTotalDoubleTimeMinusLunches As Double = WeeklyTotalDoubleTime - WeeklyTotalOffDutyMealTimeDT

            '' Reset to 0.0 every iteration
            MondayTotalHours += MondayTotalHoursWorked
            MondayTotalOnDutyHours += MondayTotalHoursWorkedMinusLunches
            MondayTotalOffDutyHours += MondayTotalOffDutyMealTime
            TuesdayTotalHours += TuesdayTotalHoursWorked
            TuesdayTotalOnDutyHours += TuesdayTotalHoursWorkedMinusLunches
            TuesdayTotalOffDutyHours += TuesdayTotalOffDutyMealTime
            WednesdayTotalHours += WednesdayTotalHoursWorked
            WednesdayTotalOnDutyHours += WednesdayTotalHoursWorkedMinusLunches
            WednesdayTotalOffDutyHours += WednesdayTotalOffDutyMealTime
            ThursdayTotalHours += ThursdayTotalHoursWorked
            ThursdayTotalOnDutyHours += ThursdayTotalHoursWorkedMinusLunches
            ThursdayTotalOffDutyHours += ThursdayTotalOffDutyMealTime
            FridayTotalHours += FridayTotalHoursWorked
            FridayTotalOnDutyHours += FridayTotalHoursWorkedMinusLunches
            FridayTotalOffDutyHours += FridayTotalOffDutyMealTime
            SaturdayTotalHours += SaturdayTotalHoursWorked
            SaturdayTotalOnDutyHours += SaturdayTotalHoursWorkedMinusLunches
            SaturdayTotalOffDutyHours += SaturdayTotalOffDutyMealTime
            SundayTotalHours += SundayTotalHoursWorked
            SundayTotalOnDutyHours += SundayTotalHoursWorkedMinusLunches
            SundayTotalOffDutyHours += SundayTotalOffDutyMealTime
            WeeklyTotalHours += WeeklyTotalHoursWorked
            WeeklyTotalOnDutyHours += WeeklyTotalHoursWorkedMinusLunches
            WeeklyTotalOffDutyHours += WeeklyTotalOffDutyMealTime
            WeeklyTotalRTHours += WeeklyTotalRegularTime
            WeeklyTotalRTOnDutyHours += WeeklyTotalRegularTimeMinusLunches
            WeeklyTotalRTOffDutyHours += WeeklyTotalOffDutyMealTimeRT
            WeeklyTotalHTHours += WeeklyTotalHolidayTime
            WeeklyTotalHTOnDutyHours += WeeklyTotalHolidayTimeMinusLunches
            WeeklyTotalHTOffDutyHours += WeeklyTotalOffDutyMealTimeHT
            WeeklyTotalOTHours += WeeklyTotalOverTime
            WeeklyTotalOTOnDutyHours += WeeklyTotalOverTimeMinusLunches
            WeeklyTotalOTOffDutyHours += WeeklyTotalOffDutyMealTimeOT
            WeeklyTotalDTHours += WeeklyTotalDoubleTime
            WeeklyTotalDTOnDutyHours += WeeklyTotalDoubleTimeMinusLunches
            WeeklyTotalDTOffDutyHours += WeeklyTotalOffDutyMealTimeDT
            WeeklyTotalDollars += (TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours*2.0) * (clientsite_payrate)
            WeeklyTotalOnDutyDollars += (TimeSpan.FromTicks(WeeklyTotalRegularTimeMinuslunches).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinuslunches).TotalHours*2.0) * (clientsite_payrate)
            WeeklyTotalOffDutyDollars += (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours*2.0) * (clientsite_payrate)
            #End Region
            
            'Add Rows
            Dim RowValues As Object() = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" } ' 15 rows
            RowValues(0) = clientsite_company
            RowValues(1) = clientsite_name
            RowValues(2) = clientsite_payrate.ToString("N2")
            RowValues(3) = WeeklyTotalDollars.ToString("C2")
            RowValues(4) = If(WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(rt_total).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(5) = If(WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(ot_total).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(6) = If(WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(dt_total).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(7) = If(WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(8) = If(MondayTotalHours = 0L, "0", TimeSpan.FromTicks(MondayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(9) = If(TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(10) = If(WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(11) = If(ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(12) = If(FridayTotalHours = 0L, "0", TimeSpan.FromTicks(FridayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(13) = If(SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
            RowValues(14) = If(SundayTotalHours = 0L, "0", TimeSpan.FromTicks(SundayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))

            dtable.Rows.Add(RowValues)
            dtable.AcceptChanges()

            #Region "Calcualte Running Totals (+=)"
            if (clientsite_company = "Excalibur") Then
                ex_WeeklyTotalDollars += WeeklyTotalDollars
                ex_WeeklyTotalRTHours += rt_total
                ex_WeeklyTotalOTHours += ot_total
                ex_WeeklyTotalDTHours += dt_total
                ex_WeeklyTotalHours += WeeklyTotalHours
                ex_MondayTotalHours += MondayTotalHours
                ex_TuesdayTotalHours += TuesdayTotalHours
                ex_WednesdayTotalHours += WednesdayTotalHours
                ex_ThursdayTotalHours += ThursdayTotalHours
                ex_FridayTotalHours += FridayTotalHours
                ex_SaturdayTotalHours += SaturdayTotalHours
                ex_SundayTotalHours += SundayTotalHours
            Else
                ms_WeeklyTotalDollars += WeeklyTotalDollars
                ms_WeeklyTotalRTHours += rt_total
                ms_WeeklyTotalOTHours += ot_total
                ms_WeeklyTotalDTHours += dt_total
                ms_WeeklyTotalHours += WeeklyTotalHours
                ms_MondayTotalHours += MondayTotalHours
                ms_TuesdayTotalHours += TuesdayTotalHours
                ms_WednesdayTotalHours += WednesdayTotalHours
                ms_ThursdayTotalHours += ThursdayTotalHours
                ms_FridayTotalHours += FridayTotalHours
                ms_SaturdayTotalHours += SaturdayTotalHours
                ms_SundayTotalHours += SundayTotalHours
            End If
            #End Region

            #Region "Reset to 0.0 every iteration"
            MondayTotalHours = 0
            MondayTotalOnDutyHours = 0
            MondayTotalOffDutyHours = 0
            TuesdayTotalHours = 0
            TuesdayTotalOnDutyHours = 0
            TuesdayTotalOffDutyHours = 0
            WednesdayTotalHours = 0
            WednesdayTotalOnDutyHours = 0
            WednesdayTotalOffDutyHours = 0
            ThursdayTotalHours = 0
            ThursdayTotalOnDutyHours = 0
            ThursdayTotalOffDutyHours = 0
            FridayTotalHours = 0
            FridayTotalOnDutyHours = 0
            FridayTotalOffDutyHours = 0
            SaturdayTotalHours = 0
            SaturdayTotalOnDutyHours = 0
            SaturdayTotalOffDutyHours = 0
            SundayTotalHours = 0
            SundayTotalOnDutyHours = 0
            SundayTotalOffDutyHours = 0
            WeeklyTotalHours = 0
            WeeklyTotalOnDutyHours = 0
            WeeklyTotalOffDutyHours = 0
            WeeklyTotalRTHours = 0
            WeeklyTotalRTOnDutyHours = 0
            WeeklyTotalRTOffDutyHours = 0
            WeeklyTotalHTHours = 0
            WeeklyTotalHTOnDutyHours = 0
            WeeklyTotalHTOffDutyHours = 0
            WeeklyTotalOTHours = 0
            WeeklyTotalOTOnDutyHours = 0
            WeeklyTotalOTOffDutyHours = 0
            WeeklyTotalDTHours = 0
            WeeklyTotalDTOnDutyHours = 0
            WeeklyTotalDTOffDutyHours = 0
            WeeklyTotalDollars = 0
            WeeklyTotalOnDutyDollars = 0
            WeeklyTotalOffDutyDollars = 0
            #End Region

        Next
        
        #Region "Add a Bottom Rows of Totals"
        Dim RowValue As Object() = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" } ' 15 rows
        RowValue(0) = "Excalibur"
        RowValue(1) = "z Total Excalibur"
        RowValue(2) = ""
        RowValue(3) = ex_WeeklyTotalDollars.ToString("C2")
        RowValue(4) = If(ex_WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalRTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(5) = If(ex_WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalOTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(6) = If(ex_WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalDTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(7) = If(ex_WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(8) = If(ex_MondayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_MondayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(9) = If(ex_TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_TuesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(10) = If(ex_WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_WednesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(11) = If(ex_ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_ThursdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(12) = If(ex_FridayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_FridayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(13) = If(ex_SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_SaturdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(14) = If(ex_SundayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_SundayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        dtable.Rows.Add(RowValue)
        dtable.AcceptChanges()
        RowValue = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" } ' 15 rows
        RowValue(0) = "Matt's Staffing"
        RowValue(1) = "z Total Matt's Staffing"
        RowValue(2) = ""
        RowValue(3) = ms_WeeklyTotalDollars.ToString("C2")
        RowValue(4) = If(ms_WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalRTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(5) = If(ms_WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalOTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(6) = If(ms_WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalDTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(7) = If(ms_WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(8) = If(ms_MondayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_MondayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(9) = If(ms_TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_TuesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(10) = If(ms_WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_WednesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(11) = If(ms_ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_ThursdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(12) = If(ms_FridayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_FridayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(13) = If(ms_SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_SaturdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(14) = If(ms_SundayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_SundayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        dtable.Rows.Add(RowValue)
        dtable.AcceptChanges()
        #End Region

        'now bind datatable to gridview... 
        dtable.DefaultView.Sort = "clientsite ASC"
        ClientTotalsGridView.DataSource = dtable
        ClientTotalsGridView.DataBind()

    End Sub
    Protected Sub updateEmployeeTotalsTable_AllEmployeesView()

        #Region "Create datatable and columns"
        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("company"))
        dtable.Columns.Add(New DataColumn("ex_ms_id"))
        dtable.Columns.Add(New DataColumn("employee"))
        dtable.Columns.Add(New DataColumn("phone"))
        dtable.Columns.Add(New DataColumn("rate"))
        dtable.Columns.Add(New DataColumn("paycheck_amount"))
        dtable.Columns.Add(New DataColumn("mondaytime"))
        dtable.Columns.Add(New DataColumn("tuesdaytime"))
        dtable.Columns.Add(New DataColumn("wednesdaytime"))
        dtable.Columns.Add(New DataColumn("thursdaytime"))
        dtable.Columns.Add(New DataColumn("fridaytime"))
        dtable.Columns.Add(New DataColumn("saturdaytime"))
        dtable.Columns.Add(New DataColumn("sundaytime"))
        dtable.Columns.Add(New DataColumn("all_total"))
        dtable.Columns.Add(New DataColumn("rt_total"))
        dtable.Columns.Add(New DataColumn("ht_total"))
        dtable.Columns.Add(New DataColumn("ot_total"))
        dtable.Columns.Add(New DataColumn("dt_total"))
        dtable.Columns.Add(New DataColumn("st_total"))
        dtable.Columns.Add(New DataColumn("reimb_rt_total"))
        dtable.Columns.Add(New DataColumn("reimb_ht_total"))
        dtable.Columns.Add(New DataColumn("reimb_ot_total"))
        dtable.Columns.Add(New DataColumn("reimb_dt_total"))
        dtable.Columns.Add(New DataColumn("gross_add_ddctn"))
        dtable.Columns.Add(New DataColumn("net_add_ddctn"))
        #End Region

        #Region "Running Totals Per Company"
        Dim ex_paycheck as Double = 0.0
        Dim ex_MondayTotalHours as Double = 0.0
        Dim ex_TuesdayTotalHours as Double = 0.0
        Dim ex_WednesdayTotalHours as Double = 0.0
        Dim ex_ThursdayTotalHours as Double = 0.0
        Dim ex_FridayTotalHours as Double = 0.0
        Dim ex_SaturdayTotalHours as Double = 0.0
        Dim ex_SundayTotalHours as Double = 0.0
        Dim ex_WeeklyTotalHours as Double = 0.0
        Dim ex_WeeklyTotalRTHours as Double = 0.0
        Dim ex_WeeklyTotalHTHours as Double = 0.0
        Dim ex_WeeklyTotalOTHours as Double = 0.0
        Dim ex_WeeklyTotalDTHours as Double = 0.0
        Dim ex_hours_requested as Double = 0.0
        Dim ex_WeeklyTotalOffDutyMealTimeRT as Double = 0.0
        Dim ex_WeeklyTotalOffDutyMealTimeHT as Double = 0.0
        Dim ex_WeeklyTotalOffDutyMealTimeOT as Double = 0.0
        Dim ex_WeeklyTotalOffDutyMealTimeDT as Double = 0.0
        Dim ex_gross_add_ddctn as Double = 0.0
        Dim ex_net_add_ddctn as Double = 0.0
        Dim ms_paycheck as Double = 0.0
        Dim ms_MondayTotalHours as Double = 0.0
        Dim ms_TuesdayTotalHours as Double = 0.0
        Dim ms_WednesdayTotalHours as Double = 0.0
        Dim ms_ThursdayTotalHours as Double = 0.0
        Dim ms_FridayTotalHours as Double = 0.0
        Dim ms_SaturdayTotalHours as Double = 0.0
        Dim ms_SundayTotalHours as Double = 0.0
        Dim ms_WeeklyTotalHours as Double = 0.0
        Dim ms_WeeklyTotalRTHours as Double = 0.0
        Dim ms_WeeklyTotalHTHours as Double = 0.0
        Dim ms_WeeklyTotalOTHours as Double = 0.0
        Dim ms_WeeklyTotalDTHours as Double = 0.0
        Dim ms_hours_requested as Double = 0.0
        Dim ms_WeeklyTotalOffDutyMealTimeRT as Double = 0.0
        Dim ms_WeeklyTotalOffDutyMealTimeHT as Double = 0.0
        Dim ms_WeeklyTotalOffDutyMealTimeOT as Double = 0.0
        Dim ms_WeeklyTotalOffDutyMealTimeDT as Double = 0.0
        Dim ms_gross_add_ddctn as Double = 0.0
        Dim ms_net_add_ddctn as Double = 0.0
        #End Region

        Dim companies as string() = { "Excalibur", "Matt''s Staffing" }

        For Each employee_company in companies

            '' Get Table of Employees dt5
            Dim dt5 As New DataTable()
            Dim sqlCmd5 As New SqlCommand("SELECT employee_id, basepayrate, cellphone, username, firstname, lastname, excalibur_id, ms_id, company FROM employee_tbl WHERE status='Active' ORDER BY username ASC", conn)
            Dim sqlDa5 As New SqlDataAdapter(sqlCmd5)
            conn.close()
            conn.open()
            sqlDa5.Fill(dt5)
            conn.close()

            '' Excalibur
            For Each employee in dt5.Rows

                Dim employee_employee_id as string = employee.ItemArray(0).ToString()
                Dim employee_basepayrate as string = employee.ItemArray(1).ToString()
                Dim employee_cellphone as string = employee.ItemArray(2).ToString()
                Dim employee_username as string = employee.ItemArray(3).ToString()
                Dim employee_firstname as string = employee.ItemArray(4).ToString()
                Dim employee_lastname as string = employee.ItemArray(5).ToString()
                Dim employee_excalibur_id as string = employee.ItemArray(6).ToString()
                Dim employee_ms_id as string = employee.ItemArray(7).ToString()
                Dim employee_employee_company as string = employee.ItemArray(8).ToString()

                #Region "Totals Variables"
                Dim MondayTotalHours As Double
                Dim MondayTotalOnDutyHours As Double
                Dim MondayTotalOffDutyHours As Double
                Dim TuesdayTotalHours As Double
                Dim TuesdayTotalOnDutyHours As Double
                Dim TuesdayTotalOffDutyHours As Double
                Dim WednesdayTotalHours As Double
                Dim WednesdayTotalOnDutyHours As Double
                Dim WednesdayTotalOffDutyHours As Double
                Dim ThursdayTotalHours As Double
                Dim ThursdayTotalOnDutyHours As Double
                Dim ThursdayTotalOffDutyHours As Double
                Dim FridayTotalHours As Double
                Dim FridayTotalOnDutyHours As Double
                Dim FridayTotalOffDutyHours As Double
                Dim SaturdayTotalHours As Double
                Dim SaturdayTotalOnDutyHours As Double
                Dim SaturdayTotalOffDutyHours As Double
                Dim SundayTotalHours As Double
                Dim SundayTotalOnDutyHours As Double
                Dim SundayTotalOffDutyHours As Double
                Dim WeeklyTotalHours As Double
                Dim WeeklyTotalOnDutyHours As Double
                Dim WeeklyTotalOffDutyHours As Double
                Dim WeeklyTotalRTHours As Double
                Dim WeeklyTotalRTOnDutyHours As Double
                Dim WeeklyTotalRTOffDutyHours As Double
                Dim WeeklyTotalHTHours As Double
                Dim WeeklyTotalHTOnDutyHours As Double
                Dim WeeklyTotalHTOffDutyHours As Double
                Dim WeeklyTotalOTHours As Double
                Dim WeeklyTotalOTOnDutyHours As Double
                Dim WeeklyTotalOTOffDutyHours As Double
                Dim WeeklyTotalDTHours As Double
                Dim WeeklyTotalDTOnDutyHours As Double
                Dim WeeklyTotalDTOffDutyHours As Double
                Dim WeeklyTotalDollars As Double
                Dim WeeklyTotalOnDutyDollars As Double
                Dim WeeklyTotalOffDutyDollars As Double
                #End Region

                '' Fill the totals table
                #Region "Declare Variables"
                Dim MondayTotalHoursWorked As Double = 0.0
                Dim TuesdayTotalHoursWorked As Double = 0.0
                Dim WednesdayTotalHoursWorked As Double = 0.0
                Dim ThursdayTotalHoursWorked As Double = 0.0
                Dim FridayTotalHoursWorked As Double = 0.0
                Dim SaturdayTotalHoursWorked As Double = 0.0
                Dim SundayTotalHoursWorked As Double = 0.0
                Dim WeeklyTotalHoursWorked As Double = 0.0

                Dim MondayTotalRT As Double = 0.0
                Dim TuesdayTotalRT As Double = 0.0
                Dim WednesdayTotalRT As Double = 0.0
                Dim ThursdayTotalRT As Double = 0.0
                Dim FridayTotalRT As Double = 0.0
                Dim SaturdayTotalRT As Double = 0.0
                Dim SundayTotalRT As Double = 0.0
                Dim MondayTotalOT As Double = 0.0
                Dim TuesdayTotalOT As Double = 0.0
                Dim WednesdayTotalOT As Double = 0.0
                Dim ThursdayTotalOT As Double = 0.0
                Dim FridayTotalOT As Double = 0.0
                Dim SaturdayTotalOT As Double = 0.0
                Dim SundayTotalOT As Double = 0.0
                Dim MondayTotalDT As Double = 0.0
                Dim TuesdayTotalDT As Double = 0.0
                Dim WednesdayTotalDT As Double = 0.0
                Dim ThursdayTotalDT As Double = 0.0
                Dim FridayTotalDT As Double = 0.0
                Dim SaturdayTotalDT As Double = 0.0
                Dim SundayTotalDT As Double = 0.0

                Dim MondayTotalOffDutyMealTimeRT As Double = 0.0
                Dim TuesdayTotalOffDutyMealTimeRT As Double = 0.0
                Dim WednesdayTotalOffDutyMealTimeRT As Double = 0.0
                Dim ThursdayTotalOffDutyMealTimeRT As Double = 0.0
                Dim FridayTotalOffDutyMealTimeRT As Double = 0.0
                Dim SaturdayTotalOffDutyMealTimeRT As Double = 0.0
                Dim SundayTotalOffDutyMealTimeRT As Double = 0.0
                Dim WeeklyTotalOffDutyMealTimeRT As Double = 0.0
                Dim MondayTotalOffDutyMealTimeOT As Double = 0.0
                Dim TuesdayTotalOffDutyMealTimeOT As Double = 0.0
                Dim WednesdayTotalOffDutyMealTimeOT As Double = 0.0
                Dim ThursdayTotalOffDutyMealTimeOT As Double = 0.0
                Dim FridayTotalOffDutyMealTimeOT As Double = 0.0
                Dim SaturdayTotalOffDutyMealTimeOT As Double = 0.0
                Dim SundayTotalOffDutyMealTimeOT As Double = 0.0
                Dim WeeklyTotalOffDutyMealTimeOT As Double = 0.0
                Dim MondayTotalOffDutyMealTimeDT As Double = 0.0
                Dim TuesdayTotalOffDutyMealTimeDT As Double = 0.0
                Dim WednesdayTotalOffDutyMealTimeDT As Double = 0.0
                Dim ThursdayTotalOffDutyMealTimeDT As Double = 0.0
                Dim FridayTotalOffDutyMealTimeDT As Double = 0.0
                Dim SaturdayTotalOffDutyMealTimeDT As Double = 0.0
                Dim SundayTotalOffDutyMealTimeDT As Double = 0.0
                Dim WeeklyTotalOffDutyMealTimeDT As Double = 0.0
                Dim MondayTotalOffDutyMealTime As Double = 0.0
                Dim TuesdayTotalOffDutyMealTime As Double = 0.0
                Dim WednesdayTotalOffDutyMealTime As Double = 0.0
                Dim ThursdayTotalOffDutyMealTime As Double = 0.0
                Dim FridayTotalOffDutyMealTime As Double = 0.0
                Dim SaturdayTotalOffDutyMealTime As Double = 0.0
                Dim SundayTotalOffDutyMealTime As Double = 0.0
                Dim WeeklyTotalOffDutyMealTime As Double = 0.0
                #End Region

                Dim dt As New DataTable()
                Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE client_site_shift_instance_tbl.employee_id='" + employee_employee_id + "' AND client_site_tbl.company='"+employee_company+"' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
                Dim sqlDa As New SqlDataAdapter(sqlCmd)
                conn.close()
                conn.open()
                sqlDa.Fill(dt)
                conn.close()

                Dim _date_ As DateTime
                If weekOfDate.Text.Length > 0 Then
                    _date_ = DateTime.Parse(weekOfDate.Text)
                    While _date_.DayOfWeek <> DayOfWeek.Monday
                        _date_ = _date_.AddDays(-1)
                    End While
                End If

                #Region "For Each Shift"
                Dim shifts_worked As Integer = dt.Rows.Count
                For Each row In dt.Rows
                    Dim wholespan As Double = 0.0
                    Dim firstspan As Double = 0.0
                    Dim secondspan As Double = 0.0
                    Dim startdatetime = DateTime.Parse(row(4))
                    Dim enddatetime = DateTime.Parse(row(5))

                    '' Figure total hours worked by day of week
                    If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                        firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                        secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                        Select Case startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If enddatetime.Date = _date_.Date Then
                                    MondayTotalHoursWorked += secondspan
                                Else
                                    SundayTotalHoursWorked += firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalHoursWorked += firstspan
                                TuesdayTotalHoursWorked += secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalHoursWorked += firstspan
                                WednesdayTotalHoursWorked += secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalHoursWorked += firstspan
                                ThursdayTotalHoursWorked += secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalHoursWorked += firstspan
                                FridayTotalHoursWorked += secondspan
                            Case DayOfWeek.Friday
                                FridayTotalHoursWorked += firstspan
                                SaturdayTotalHoursWorked += secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalHoursWorked += firstspan
                                SundayTotalHoursWorked += secondspan
                        End Select
                    Else
                        wholespan = enddatetime.Ticks - startdatetime.Ticks
                        Select Case startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                SundayTotalHoursWorked += wholespan
                            Case DayOfWeek.Monday
                                MondayTotalHoursWorked += wholespan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalHoursWorked += wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalHoursWorked += wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalHoursWorked += wholespan
                            Case DayOfWeek.Friday
                                FridayTotalHoursWorked += wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalHoursWorked += wholespan
                        End Select
                    End If

                    '' Figure total off duty lunch time by day of week
                    If row(7) = False Then
                        Dim firstmeal_startdatetime As DateTime
                        Dim firstmeal_enddatetime As DateTime
                        Dim secondmeal_startdatetime As DateTime
                        Dim secondmeal_enddatetime As DateTime
                        Dim thirdmeal_startdatetime As DateTime
                        Dim thirdmeal_enddatetime As DateTime

                        Try
                            firstmeal_startdatetime = DateTime.Parse(row(8))
                        Catch ex As Exception
                            firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            firstmeal_enddatetime = DateTime.Parse(row(9))
                        Catch ex As Exception
                            firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            secondmeal_startdatetime = DateTime.Parse(row(10))
                        Catch ex As Exception
                            secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            secondmeal_enddatetime = DateTime.Parse(row(11))
                        Catch ex As Exception
                            secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            thirdmeal_startdatetime = DateTime.Parse(row(12))
                        Catch ex As Exception
                            thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            thirdmeal_enddatetime = DateTime.Parse(row(13))
                        Catch ex As Exception
                            thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try

                        If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                            firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                            secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                            Select Case firstmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeRT += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeRT += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeRT += firstspan
                                    TuesdayTotalOffDutyMealTimeRT += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeRT += firstspan
                                    WednesdayTotalOffDutyMealTimeRT += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeRT += firstspan
                                    ThursdayTotalOffDutyMealTimeRT += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeRT += firstspan
                                    FridayTotalOffDutyMealTimeRT += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeRT += firstspan
                                    SaturdayTotalOffDutyMealTimeRT += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeRT += firstspan
                                    SundayTotalOffDutyMealTimeRT += secondspan
                            End Select
                        Else
                            wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                            Select Case firstmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If firstmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeRT += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If firstmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeRT += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeRT += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeRT += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeRT += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeRT += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeRT += wholespan
                            End Select
                        End If
                        If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                            firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                            secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                            Select Case secondmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If secondmeal_enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeOT += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeOT += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeOT += firstspan
                                    TuesdayTotalOffDutyMealTimeOT += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeOT += firstspan
                                    WednesdayTotalOffDutyMealTimeOT += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeOT += firstspan
                                    ThursdayTotalOffDutyMealTimeOT += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeOT += firstspan
                                    FridayTotalOffDutyMealTimeOT += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeOT += firstspan
                                    SaturdayTotalOffDutyMealTimeOT += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeOT += firstspan
                                    SundayTotalOffDutyMealTimeOT += secondspan
                            End Select
                        Else
                            wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                            Select Case secondmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If secondmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeOT += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If secondmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeOT += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeOT += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeOT += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeOT += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeOT += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeOT += wholespan
                            End Select
                        End If
                        If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                            firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                            secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                            Select Case thirdmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If thirdmeal_enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeDT += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeDT += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeDT += firstspan
                                    TuesdayTotalOffDutyMealTimeDT += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeDT += firstspan
                                    WednesdayTotalOffDutyMealTimeDT += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeDT += firstspan
                                    ThursdayTotalOffDutyMealTimeDT += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeDT += firstspan
                                    FridayTotalOffDutyMealTimeDT += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeDT += firstspan
                                    SaturdayTotalOffDutyMealTimeDT += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeDT += firstspan
                                    SundayTotalOffDutyMealTimeDT += secondspan
                            End Select
                        Else
                            wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                            Select Case thirdmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If thirdmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeDT += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If thirdmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeDT += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeDT += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeDT += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeDT += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeDT += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeDT += wholespan
                            End Select
                        End If
                    End If

                Next
                #End Region

                #Region "Calculate Totals"
                WeeklyTotalHoursWorked = MondayTotalHoursWorked + TuesdayTotalHoursWorked + WednesdayTotalHoursWorked + ThursdayTotalHoursWorked + FridayTotalHoursWorked + SaturdayTotalHoursWorked + SundayTotalHoursWorked

                MondayTotalOffDutyMealTime = MondayTotalOffDutyMealTimeRT + MondayTotalOffDutyMealTimeOT + MondayTotalOffDutyMealTimeDT
                TuesdayTotalOffDutyMealTime = TuesdayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeDT
                WednesdayTotalOffDutyMealTime = WednesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeDT
                ThursdayTotalOffDutyMealTime = ThursdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeDT
                FridayTotalOffDutyMealTime = FridayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeDT
                SaturdayTotalOffDutyMealTime = SaturdayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeDT
                SundayTotalOffDutyMealTime = SundayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeDT

                Dim halfhour = 10000000L * 60 * 60 * 0.5
                Dim onehour = 10000000L * 60 * 60 * 1
                Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5

                #Region "Off Duty Meal Times by RT/OT/DT"
                If MondayTotalOffDutyMealTime <= halfhour Then
                    MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTime
                    MondayTotalOffDutyMealTimeOT = 0
                    MondayTotalOffDutyMealTimeDT = 0
                ElseIf MondayTotalOffDutyMealTime > halfhour And MondayTotalOffDutyMealTime <= onehour Then
                    MondayTotalOffDutyMealTimeRT = halfhour
                    MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTime - halfhour
                    MondayTotalOffDutyMealTimeDT = 0
                ElseIf MondayTotalOffDutyMealTime > onehour And MondayTotalOffDutyMealTime <= oneandhalfhour Then
                    MondayTotalOffDutyMealTimeRT = halfhour
                    MondayTotalOffDutyMealTimeOT = halfhour
                    MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTime - onehour
                ElseIf MondayTotalOffDutyMealTime > oneandhalfhour Then
                    MondayTotalOffDutyMealTimeRT = halfhour
                    MondayTotalOffDutyMealTimeOT = halfhour
                    MondayTotalOffDutyMealTimeDT = halfhour
                End If
                If TuesdayTotalOffDutyMealTime <= halfhour Then
                    TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTime
                    TuesdayTotalOffDutyMealTimeOT = 0
                    TuesdayTotalOffDutyMealTimeDT = 0
                ElseIf TuesdayTotalOffDutyMealTime > halfhour And TuesdayTotalOffDutyMealTime <= onehour Then
                    TuesdayTotalOffDutyMealTimeRT = halfhour
                    TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTime - halfhour
                    TuesdayTotalOffDutyMealTimeDT = 0
                ElseIf TuesdayTotalOffDutyMealTime > onehour And TuesdayTotalOffDutyMealTime <= oneandhalfhour Then
                    TuesdayTotalOffDutyMealTimeRT = halfhour
                    TuesdayTotalOffDutyMealTimeOT = halfhour
                    TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTime - onehour
                ElseIf TuesdayTotalOffDutyMealTime > oneandhalfhour Then
                    TuesdayTotalOffDutyMealTimeRT = halfhour
                    TuesdayTotalOffDutyMealTimeOT = halfhour
                    TuesdayTotalOffDutyMealTimeDT = halfhour
                End If
                If WednesdayTotalOffDutyMealTime <= halfhour Then
                    WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTime
                    WednesdayTotalOffDutyMealTimeOT = 0
                    WednesdayTotalOffDutyMealTimeDT = 0
                ElseIf WednesdayTotalOffDutyMealTime > halfhour And WednesdayTotalOffDutyMealTime <= onehour Then
                    WednesdayTotalOffDutyMealTimeRT = halfhour
                    WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTime - halfhour
                    WednesdayTotalOffDutyMealTimeDT = 0
                ElseIf WednesdayTotalOffDutyMealTime > onehour And WednesdayTotalOffDutyMealTime <= oneandhalfhour Then
                    WednesdayTotalOffDutyMealTimeRT = halfhour
                    WednesdayTotalOffDutyMealTimeOT = halfhour
                    WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTime - onehour
                ElseIf WednesdayTotalOffDutyMealTime > oneandhalfhour Then
                    WednesdayTotalOffDutyMealTimeRT = halfhour
                    WednesdayTotalOffDutyMealTimeOT = halfhour
                    WednesdayTotalOffDutyMealTimeDT = halfhour
                End If
                If ThursdayTotalOffDutyMealTime <= halfhour Then
                    ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTime
                    ThursdayTotalOffDutyMealTimeOT = 0
                    ThursdayTotalOffDutyMealTimeDT = 0
                ElseIf WednesdayTotalOffDutyMealTime > halfhour And ThursdayTotalOffDutyMealTime <= onehour Then
                    ThursdayTotalOffDutyMealTimeRT = halfhour
                    ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTime - halfhour
                    ThursdayTotalOffDutyMealTimeDT = 0
                ElseIf ThursdayTotalOffDutyMealTime > onehour And ThursdayTotalOffDutyMealTime <= oneandhalfhour Then
                    ThursdayTotalOffDutyMealTimeRT = halfhour
                    ThursdayTotalOffDutyMealTimeOT = halfhour
                    ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTime - onehour
                ElseIf ThursdayTotalOffDutyMealTime > oneandhalfhour Then
                    ThursdayTotalOffDutyMealTimeRT = halfhour
                    ThursdayTotalOffDutyMealTimeOT = halfhour
                    ThursdayTotalOffDutyMealTimeDT = halfhour
                End If
                If FridayTotalOffDutyMealTime <= halfhour Then
                    FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTime
                    FridayTotalOffDutyMealTimeOT = 0
                    FridayTotalOffDutyMealTimeDT = 0
                ElseIf FridayTotalOffDutyMealTime > halfhour And FridayTotalOffDutyMealTime <= onehour Then
                    FridayTotalOffDutyMealTimeRT = halfhour
                    FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTime - halfhour
                    FridayTotalOffDutyMealTimeDT = 0
                ElseIf FridayTotalOffDutyMealTime > onehour And FridayTotalOffDutyMealTime <= oneandhalfhour Then
                    FridayTotalOffDutyMealTimeRT = halfhour
                    FridayTotalOffDutyMealTimeOT = halfhour
                    FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTime - onehour
                ElseIf FridayTotalOffDutyMealTime > oneandhalfhour Then
                    FridayTotalOffDutyMealTimeRT = halfhour
                    FridayTotalOffDutyMealTimeOT = halfhour
                    FridayTotalOffDutyMealTimeDT = halfhour
                End If
                If SaturdayTotalOffDutyMealTime <= halfhour Then
                    SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTime
                    SaturdayTotalOffDutyMealTimeOT = 0
                    SaturdayTotalOffDutyMealTimeDT = 0
                ElseIf SaturdayTotalOffDutyMealTime > halfhour And SaturdayTotalOffDutyMealTime <= onehour Then
                    SaturdayTotalOffDutyMealTimeRT = halfhour
                    SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTime - halfhour
                    SaturdayTotalOffDutyMealTimeDT = 0
                ElseIf SaturdayTotalOffDutyMealTime > onehour And SaturdayTotalOffDutyMealTime <= oneandhalfhour Then
                    SaturdayTotalOffDutyMealTimeRT = halfhour
                    SaturdayTotalOffDutyMealTimeOT = halfhour
                    SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTime - onehour
                ElseIf SaturdayTotalOffDutyMealTime > oneandhalfhour Then
                    SaturdayTotalOffDutyMealTimeRT = halfhour
                    SaturdayTotalOffDutyMealTimeOT = halfhour
                    SaturdayTotalOffDutyMealTimeDT = halfhour
                End If
                If SundayTotalOffDutyMealTime <= halfhour Then
                    SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTime
                    SundayTotalOffDutyMealTimeOT = 0
                    SundayTotalOffDutyMealTimeDT = 0
                ElseIf SundayTotalOffDutyMealTime > halfhour And SundayTotalOffDutyMealTime <= onehour Then
                    SundayTotalOffDutyMealTimeRT = halfhour
                    SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTime - halfhour
                    SundayTotalOffDutyMealTimeDT = 0
                ElseIf SundayTotalOffDutyMealTime > onehour And SundayTotalOffDutyMealTime <= oneandhalfhour Then
                    SundayTotalOffDutyMealTimeRT = halfhour
                    SundayTotalOffDutyMealTimeOT = halfhour
                    SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTime - onehour
                ElseIf SundayTotalOffDutyMealTime > oneandhalfhour Then
                    SundayTotalOffDutyMealTimeRT = halfhour
                    SundayTotalOffDutyMealTimeOT = halfhour
                    SundayTotalOffDutyMealTimeDT = halfhour
                End If
                #End Region

                WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
                WeeklyTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeOT
                WeeklyTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + TuesdayTotalOffDutyMealTimeDT + WednesdayTotalOffDutyMealTimeDT + ThursdayTotalOffDutyMealTimeDT + FridayTotalOffDutyMealTimeDT + SaturdayTotalOffDutyMealTimeDT + SundayTotalOffDutyMealTimeDT

                Dim fourhours = 10000000L * 60 * 60 * 4
                Dim eighthours = 10000000L * 60 * 60 * 8
                Dim twelvehours = 10000000L * 60 * 60 * 12
                Dim fourtyhours = 10000000L * 60 * 60 * 40
                Dim WeeklyTotalRegularTime = 0L
                Dim WeeklyTotalOverTime = 0L
                Dim WeeklyTotalDoubleTime = 0L

                If MondayTotalHoursWorked >= twelvehours Then
                    MondayTotalDT = MondayTotalHoursWorked - twelvehours
                    MondayTotalOT = fourhours
                    MondayTotalRT = eighthours
                ElseIf MondayTotalHoursWorked >= eighthours Then
                    MondayTotalOT = MondayTotalHoursWorked - eighthours
                    MondayTotalRT = eighthours
                Else
                    MondayTotalRT = MondayTotalHoursWorked
                End If
                If TuesdayTotalHoursWorked >= twelvehours Then
                    TuesdayTotalDT = TuesdayTotalHoursWorked - twelvehours
                    TuesdayTotalOT = fourhours
                    TuesdayTotalRT = eighthours
                ElseIf TuesdayTotalHoursWorked >= eighthours Then
                    TuesdayTotalOT = TuesdayTotalHoursWorked - eighthours
                    TuesdayTotalRT = eighthours
                Else
                    TuesdayTotalRT = TuesdayTotalHoursWorked
                End If
                If WednesdayTotalHoursWorked >= twelvehours Then
                    WednesdayTotalDT = WednesdayTotalHoursWorked - twelvehours
                    WednesdayTotalOT = fourhours
                    WednesdayTotalRT = eighthours
                ElseIf WednesdayTotalHoursWorked >= eighthours Then
                    WednesdayTotalOT = WednesdayTotalHoursWorked - eighthours
                    WednesdayTotalRT = eighthours
                Else
                    WednesdayTotalRT = WednesdayTotalHoursWorked
                End If
                If ThursdayTotalHoursWorked >= twelvehours Then
                    ThursdayTotalDT = ThursdayTotalHoursWorked - twelvehours
                    ThursdayTotalOT = fourhours
                    ThursdayTotalRT = eighthours
                ElseIf ThursdayTotalHoursWorked >= eighthours Then
                    ThursdayTotalOT = ThursdayTotalHoursWorked - eighthours
                    ThursdayTotalRT = eighthours
                Else
                    ThursdayTotalRT = ThursdayTotalHoursWorked
                End If
                If FridayTotalHoursWorked >= twelvehours Then
                    FridayTotalDT = FridayTotalHoursWorked - twelvehours
                    FridayTotalOT = fourhours
                    FridayTotalRT = eighthours
                ElseIf FridayTotalHoursWorked >= eighthours Then
                    FridayTotalOT = FridayTotalHoursWorked - eighthours
                    FridayTotalRT = eighthours
                Else
                    FridayTotalRT = FridayTotalHoursWorked
                End If
                If SaturdayTotalHoursWorked >= twelvehours Then
                    SaturdayTotalDT = SaturdayTotalHoursWorked - twelvehours
                    SaturdayTotalOT = fourhours
                    SaturdayTotalRT = eighthours
                ElseIf SaturdayTotalHoursWorked >= eighthours Then
                    SaturdayTotalOT = SaturdayTotalHoursWorked - eighthours
                    SaturdayTotalRT = eighthours
                Else
                    SaturdayTotalRT = SaturdayTotalHoursWorked
                End If
                If SundayTotalHoursWorked >= twelvehours Then
                    SundayTotalDT = SundayTotalHoursWorked - twelvehours
                    SundayTotalOT = fourhours
                    SundayTotalRT = eighthours
                ElseIf SundayTotalHoursWorked >= eighthours Then
                    SundayTotalOT = SundayTotalHoursWorked - eighthours
                    SundayTotalRT = eighthours
                Else
                    SundayTotalRT = SundayTotalHoursWorked
                End If

                WeeklyTotalRegularTime = MondayTotalRT + TuesdayTotalRT + WednesdayTotalRT + ThursdayTotalRT + FridayTotalRT + SaturdayTotalRT + SundayTotalRT
                WeeklyTotalOverTime = MondayTotalOT + TuesdayTotalOT + WednesdayTotalOT + ThursdayTotalOT + FridayTotalOT + SaturdayTotalOT + SundayTotalOT
                WeeklyTotalDoubleTime = MondayTotalDT + TuesdayTotalDT + WednesdayTotalDT + ThursdayTotalDT + FridayTotalDT + SaturdayTotalDT + SundayTotalDT

                If WeeklyTotalHoursWorked - fourtyhours > WeeklyTotalOverTime Then
                    WeeklyTotalOverTime = WeeklyTotalHoursWorked - fourtyhours - WeeklyTotalDoubleTime
                    WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalOverTime
                End If

                '' Calculate HT and HT_OffDutyMeal for each day of week
                Dim MondayTotalHT As Double = 0.0
                Dim TuesdayTotalHT As Double = 0.0
                Dim WednesdayTotalHT As Double = 0.0
                Dim ThursdayTotalHT As Double = 0.0
                Dim FridayTotalHT As Double = 0.0
                Dim SaturdayTotalHT As Double = 0.0
                Dim SundayTotalHT As Double = 0.0
                Dim WeeklyTotalHolidayTime As Double = 0.0
                Dim MondayTotalOffDutyMealTimeHT As Double = 0.0
                Dim TuesdayTotalOffDutyMealTimeHT As Double = 0.0
                Dim WednesdayTotalOffDutyMealTimeHT As Double = 0.0
                Dim ThursdayTotalOffDutyMealTimeHT As Double = 0.0
                Dim FridayTotalOffDutyMealTimeHT As Double = 0.0
                Dim SaturdayTotalOffDutyMealTimeHT As Double = 0.0
                Dim SundayTotalOffDutyMealTimeHT As Double = 0.0
                Dim WeeklyTotalOffDutyMealTimeHT As Double = 0.0
                Dim _date As DateTime
                If weekOfDate.Text.Length > 0 Then
                    _date = DateTime.Parse(weekOfDate.Text)
                    While _date.DayOfWeek <> DayOfWeek.Monday
                        _date = _date.AddDays(-1)
                    End While
                End If
                If dateIsHoliday(_date) Then
                    MondayTotalHT = MondayTotalRT
                    MondayTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeRT
                    MondayTotalRT = 0
                    MondayTotalOffDutyMealTimeRT = 0
                End If
                _date = _date.AddDays(1)
                If dateIsHoliday(_date) Then
                    TuesdayTotalHT = TuesdayTotalRT
                    TuesdayTotalOffDutyMealTimeHT = TuesdayTotalOffDutyMealTimeRT
                    TuesdayTotalRT = 0
                    TuesdayTotalOffDutyMealTimeRT = 0
                End If
                _date = _date.AddDays(1)
                If dateIsHoliday(_date) Then
                    WednesdayTotalHT = WednesdayTotalRT
                    WednesdayTotalOffDutyMealTimeHT = WednesdayTotalOffDutyMealTimeRT
                    WednesdayTotalRT = 0
                    WednesdayTotalOffDutyMealTimeRT = 0
                End If
                _date = _date.AddDays(1)
                If dateIsHoliday(_date) Then
                    ThursdayTotalHT = ThursdayTotalRT
                    ThursdayTotalOffDutyMealTimeHT = ThursdayTotalOffDutyMealTimeRT
                    ThursdayTotalRT = 0
                    ThursdayTotalOffDutyMealTimeRT = 0
                End If
                _date = _date.AddDays(1)
                If dateIsHoliday(_date) Then
                    FridayTotalHT = FridayTotalRT
                    FridayTotalOffDutyMealTimeHT = FridayTotalOffDutyMealTimeRT
                    FridayTotalRT = 0
                    FridayTotalOffDutyMealTimeRT = 0
                End If
                _date = _date.AddDays(1)
                If dateIsHoliday(_date) Then
                    SaturdayTotalHT = SaturdayTotalRT
                    SaturdayTotalOffDutyMealTimeHT = SaturdayTotalOffDutyMealTimeRT
                    SaturdayTotalRT = 0
                    SaturdayTotalOffDutyMealTimeRT = 0
                End If
                _date = _date.AddDays(1)
                If dateIsHoliday(_date) Then
                    SundayTotalHT = SundayTotalRT
                    SundayTotalOffDutyMealTimeHT = SundayTotalOffDutyMealTimeRT
                    SundayTotalRT = 0
                    SundayTotalOffDutyMealTimeRT = 0
                End If
                WeeklyTotalHolidayTime = MondayTotalHT + TuesdayTotalHT + WednesdayTotalHT + ThursdayTotalHT + FridayTotalHT + SaturdayTotalHT + SundayTotalHT
                WeeklyTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeHT + TuesdayTotalOffDutyMealTimeHT + WednesdayTotalOffDutyMealTimeHT + ThursdayTotalOffDutyMealTimeHT + FridayTotalOffDutyMealTimeHT + SaturdayTotalOffDutyMealTimeHT + SundayTotalOffDutyMealTimeHT


                Dim MondayTotalHoursWorkedMinusLunches As Double = MondayTotalHoursWorked - MondayTotalOffDutyMealTime
                Dim TuesdayTotalHoursWorkedMinusLunches As Double = TuesdayTotalHoursWorked - TuesdayTotalOffDutyMealTime
                Dim WednesdayTotalHoursWorkedMinusLunches As Double = WednesdayTotalHoursWorked - WednesdayTotalOffDutyMealTime
                Dim ThursdayTotalHoursWorkedMinusLunches As Double = ThursdayTotalHoursWorked - ThursdayTotalOffDutyMealTime
                Dim FridayTotalHoursWorkedMinusLunches As Double = FridayTotalHoursWorked - FridayTotalOffDutyMealTime
                Dim SaturdayTotalHoursWorkedMinusLunches As Double = SaturdayTotalHoursWorked - SaturdayTotalOffDutyMealTime
                Dim SundayTotalHoursWorkedMinusLunches As Double = SundayTotalHoursWorked - SundayTotalOffDutyMealTime

                '' Subtract HT from RT
                WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalHolidayTime
                '' Recalculate Total MealTimes RT 
                WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
                '' Calculate Total MealTime
                WeeklyTotalOffDutyMealTime = WeeklyTotalOffDutyMealTimeRT + WeeklyTotalOffDutyMealTimeOT + WeeklyTotalOffDutyMealTimeDT + WeeklyTotalOffDutyMealTimeHT
                '' Calculate WeeklyTotalHoursWorked Minus Lunches
                Dim WeeklyTotalHoursWorkedMinusLunches As Double = WeeklyTotalHoursWorked - WeeklyTotalOffDutyMealTime
                Dim WeeklyTotalRegularTimeMinusLunches As Double = WeeklyTotalRegularTime - WeeklyTotalOffDutyMealTimeRT
                Dim WeeklyTotalHolidayTimeMinusLunches As Double = WeeklyTotalHolidayTime - WeeklyTotalOffDutyMealTimeHT
                Dim WeeklyTotalOverTimeMinusLunches As Double = WeeklyTotalOverTime - WeeklyTotalOffDutyMealTimeOT
                Dim WeeklyTotalDoubleTimeMinusLunches As Double = WeeklyTotalDoubleTime - WeeklyTotalOffDutyMealTimeDT

                '' Reset to 0.0 every iteration
                MondayTotalHours += MondayTotalHoursWorked
                MondayTotalOnDutyHours += MondayTotalHoursWorkedMinusLunches
                MondayTotalOffDutyHours += MondayTotalOffDutyMealTime
                TuesdayTotalHours += TuesdayTotalHoursWorked
                TuesdayTotalOnDutyHours += TuesdayTotalHoursWorkedMinusLunches
                TuesdayTotalOffDutyHours += TuesdayTotalOffDutyMealTime
                WednesdayTotalHours += WednesdayTotalHoursWorked
                WednesdayTotalOnDutyHours += WednesdayTotalHoursWorkedMinusLunches
                WednesdayTotalOffDutyHours += WednesdayTotalOffDutyMealTime
                ThursdayTotalHours += ThursdayTotalHoursWorked
                ThursdayTotalOnDutyHours += ThursdayTotalHoursWorkedMinusLunches
                ThursdayTotalOffDutyHours += ThursdayTotalOffDutyMealTime
                FridayTotalHours += FridayTotalHoursWorked
                FridayTotalOnDutyHours += FridayTotalHoursWorkedMinusLunches
                FridayTotalOffDutyHours += FridayTotalOffDutyMealTime
                SaturdayTotalHours += SaturdayTotalHoursWorked
                SaturdayTotalOnDutyHours += SaturdayTotalHoursWorkedMinusLunches
                SaturdayTotalOffDutyHours += SaturdayTotalOffDutyMealTime
                SundayTotalHours += SundayTotalHoursWorked
                SundayTotalOnDutyHours += SundayTotalHoursWorkedMinusLunches
                SundayTotalOffDutyHours += SundayTotalOffDutyMealTime
                WeeklyTotalHours += WeeklyTotalHoursWorked
                WeeklyTotalOnDutyHours += WeeklyTotalHoursWorkedMinusLunches
                WeeklyTotalOffDutyHours += WeeklyTotalOffDutyMealTime
                WeeklyTotalRTHours += WeeklyTotalRegularTime
                WeeklyTotalRTOnDutyHours += WeeklyTotalRegularTimeMinusLunches
                WeeklyTotalRTOffDutyHours += WeeklyTotalOffDutyMealTimeRT
                WeeklyTotalHTHours += WeeklyTotalHolidayTime
                WeeklyTotalHTOnDutyHours += WeeklyTotalHolidayTimeMinusLunches
                WeeklyTotalHTOffDutyHours += WeeklyTotalOffDutyMealTimeHT
                WeeklyTotalOTHours += WeeklyTotalOverTime
                WeeklyTotalOTOnDutyHours += WeeklyTotalOverTimeMinusLunches
                WeeklyTotalOTOffDutyHours += WeeklyTotalOffDutyMealTimeOT
                WeeklyTotalDTHours += WeeklyTotalDoubleTime
                WeeklyTotalDTOnDutyHours += WeeklyTotalDoubleTimeMinusLunches
                WeeklyTotalDTOffDutyHours += WeeklyTotalOffDutyMealTimeDT
                Dim basepayrate As Double = Double.Parse(employee.ItemArray(1))
                WeeklyTotalDollars += (TimeSpan.FromTicks(WeeklyTotalRegularTime).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTime).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTime).TotalHours*2.0) * (basepayrate)
                WeeklyTotalOnDutyDollars += (TimeSpan.FromTicks(WeeklyTotalRegularTimeMinuslunches).TotalHours + TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOverTimeMinuslunches).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinuslunches).TotalHours*2.0) * (basepayrate)
                WeeklyTotalOffDutyDollars += (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours*1.5 + TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours*2.0) * (basepayrate)
                #End Region

                #Region "Sick Pay, Reimbursements, Phone/Internet, Messaging"
                '' Total Sick Time, Reimbursements Total, and 25C Phone/Internet
                'Add Each Item from Reimbursement Table for this employee if Excalibur is their main company
                Dim dt2 As New DataTable()
                Dim sqlCmd2 As New SqlCommand("SELECT reimbursement_tbl.amount, reimbursement_tbl.is_gross_taxablewage FROM reimbursement_tbl LEFT JOIN employee_tbl ON reimbursement_tbl.employee_id = employee_tbl.employee_id WHERE reimbursement_tbl.employee_id = '" + employee_employee_id + "' AND employee_tbl.company='"+employee_company+"' AND (datetime >= '" + DateTime.Parse(startdate).ToString() + "' AND datetime <= '" + DateTime.Parse(enddate).ToString() + "')", conn)
                Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
                conn.close()
                conn.open()
                sqlDa2.Fill(dt2)
                conn.close()

                Dim number_of_reimbursements As Integer = dt2.Rows.Count
                Dim gross_add_ddctn As Double = 0.0
                Dim net_add_ddctn As Double = 0.0
                For Each row In dt2.Rows
                    Dim amount As Double = row(0).ToString()
                    Dim is_gross_taxablewage As Boolean = row(1).ToString()
                    If amount > 0 Then
                        If is_gross_taxablewage Then
                            gross_add_ddctn += amount
                        Else
                            net_add_ddctn += amount
                        End If
                    Else If amount < 0 Then
                        If is_gross_taxablewage Then
                            gross_add_ddctn += amount
                        Else
                            net_add_ddctn += amount
                        End If
                    End If
                Next

                'Add Each Item from SickPay Request Table for this employee if Excalibur is their main company
                Dim dt3 As New DataTable()
                Dim sqlCmd3 As New SqlCommand("SELECT sickpay_request_tbl.minutes_requested FROM sickpay_request_tbl LEFT JOIN employee_tbl ON sickpay_request_tbl.employee_id = employee_tbl.employee_id WHERE employee_tbl.company='"+employee_company+"' AND sickpay_request_tbl.employee_id = '" + employee.ItemArray(0).ToString() + "' AND (datetime >= '" + DateTime.Parse(startdate).ToString() + "' AND datetime <= '" + DateTime.Parse(enddate).ToString() + "')", conn)
                Dim sqlDa3 As New SqlDataAdapter(sqlCmd3)
                conn.close()
                conn.open()
                sqlDa3.Fill(dt3)
                conn.close()
                Dim hours_requested As Double = 0.0
                For Each row In dt3.Rows
                    If row.ItemArray(0) > 0 Then
                        hours_requested += (Double.Parse(row.ItemArray(0))/60.000000).ToString("N6") 'hours
                    End If
                Next            

                'Add Other Reimbursement (Phone Use 2min or 5min, OT rate if 40+hrs)
                '5min if 1st Week of the month, else 2min
                Dim messaging_pay As Double = 0.0
                Dim phone_and_internet_pay As Double = 0.0
                If (true) Then
                    If shifts_worked > 0 Or number_of_reimbursements > 0  Then
                        If enddate.Day <= 7 Then
                            messaging_pay += (basepayrate * 1.5 * (5.000000/60.000000)) 'amount
                        Else 
                            messaging_pay += (basepayrate * 1.5 * (2.000000/60.000000)) 'amount
                        End If
                    End If
            
                    ' Add 25 cents for phone and internet usage
                    If shifts_worked > 0 Or number_of_reimbursements > 0  Then
                        phone_and_internet_pay += 0.25
                    End If
                End If
                #End Region

                '' Make Totals
                gross_add_ddctn += messaging_pay
                net_add_ddctn += phone_and_internet_pay
                If (WeeklyTotalOffDutyMealTimeRT <> 0L) Then net_add_ddctn -= messaging_pay
                Dim paycheck As Double = WeeklyTotalDollars + hours_requested*basepayrate + gross_add_ddctn + net_add_ddctn
            
                'Add Rows
                Dim RowValues As Object() = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""} ' 25 rows
                RowValues(0) = employee_company
                If (employee_company = "Excalibur") Then RowValues(1) = employee_excalibur_id
                If (employee_company = "Matt''s Staffing") Then RowValues(1) = employee_ms_id
                RowValues(2) = employee_firstname + " " + employee_lastname
                RowValues(3) = employee_cellphone
                RowValues(4) = basepayrate.ToString("C2")
                RowValues(5) = paycheck.ToString("C2")
                RowValues(6) = If(MondayTotalHours = 0L, "0", TimeSpan.FromTicks(MondayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(7) = If(TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(TuesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(8) = If(WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(WednesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(9) = If(ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ThursdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(10) = If(FridayTotalHours = 0L, "0", TimeSpan.FromTicks(FridayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(11) = If(SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(SaturdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(12) = If(SundayTotalHours = 0L, "0", TimeSpan.FromTicks(SundayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(13) = If(WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(14) = If(WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalRTOnDutyHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(15) = If(WeeklyTotalHTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalHTOnDutyHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(16) = If(WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOTOnDutyHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(17) = If(WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(WeeklyTotalDTOnDutyHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(18) = hours_requested.ToString("N2").TrimEnd("0").TrimEnd(".") 'ST hours
                RowValues(19) = If(WeeklyTotalOffDutyMealTimeRT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(20) = If(WeeklyTotalOffDutyMealTimeHT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(21) = If(WeeklyTotalOffDutyMealTimeOT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(22) = If(WeeklyTotalOffDutyMealTimeDT = 0L, "0", TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
                RowValues(23) = gross_add_ddctn.ToString("C2")
                RowValues(24) = net_add_ddctn.ToString("C2")

                If (paycheck > 0.0) Then
                    dtable.Rows.Add(RowValues)
                    dtable.AcceptChanges()
                End If

                #Region "Calcualte Running Totals (+=)"
                if (employee_company = "Excalibur") Then
                    ex_paycheck += paycheck
                    ex_MondayTotalHours += MondayTotalHours
                    ex_TuesdayTotalHours += TuesdayTotalHours
                    ex_WednesdayTotalHours += WednesdayTotalHours
                    ex_ThursdayTotalHours += ThursdayTotalHours
                    ex_FridayTotalHours += FridayTotalHours
                    ex_SaturdayTotalHours += SaturdayTotalHours
                    ex_SundayTotalHours += SundayTotalHours
                    ex_WeeklyTotalHours += WeeklyTotalHours
                    ex_WeeklyTotalRTHours += WeeklyTotalRTHours
                    ex_WeeklyTotalHTHours += WeeklyTotalHTHours
                    ex_WeeklyTotalOTHours += WeeklyTotalOTHours
                    ex_WeeklyTotalDTHours += WeeklyTotalDTHours
                    ex_hours_requested += hours_requested
                    ex_WeeklyTotalOffDutyMealTimeRT += WeeklyTotalOffDutyMealTimeRT
                    ex_WeeklyTotalOffDutyMealTimeHT += WeeklyTotalOffDutyMealTimeHT
                    ex_WeeklyTotalOffDutyMealTimeOT += WeeklyTotalOffDutyMealTimeOT
                    ex_WeeklyTotalOffDutyMealTimeDT += WeeklyTotalOffDutyMealTimeDT
                    ex_gross_add_ddctn += gross_add_ddctn
                    ex_net_add_ddctn += net_add_ddctn
                Else
                    ms_paycheck += paycheck
                    ms_MondayTotalHours += MondayTotalHours
                    ms_TuesdayTotalHours += TuesdayTotalHours
                    ms_WednesdayTotalHours += WednesdayTotalHours
                    ms_ThursdayTotalHours += ThursdayTotalHours
                    ms_FridayTotalHours += FridayTotalHours
                    ms_SaturdayTotalHours += SaturdayTotalHours
                    ms_SundayTotalHours += SundayTotalHours
                    ms_WeeklyTotalHours += WeeklyTotalHours
                    ms_WeeklyTotalRTHours += WeeklyTotalRTHours
                    ms_WeeklyTotalHTHours += WeeklyTotalHTHours
                    ms_WeeklyTotalOTHours += WeeklyTotalOTHours
                    ms_WeeklyTotalDTHours += WeeklyTotalDTHours
                    ms_hours_requested += hours_requested
                    ms_WeeklyTotalOffDutyMealTimeRT += WeeklyTotalOffDutyMealTimeRT
                    ms_WeeklyTotalOffDutyMealTimeHT += WeeklyTotalOffDutyMealTimeHT
                    ms_WeeklyTotalOffDutyMealTimeOT += WeeklyTotalOffDutyMealTimeOT
                    ms_WeeklyTotalOffDutyMealTimeDT += WeeklyTotalOffDutyMealTimeDT
                    ms_gross_add_ddctn += gross_add_ddctn
                    ms_net_add_ddctn += net_add_ddctn
                End If

                #End Region

                #Region "Reset to 0.0 every iteration"
                MondayTotalHours = 0
                MondayTotalOnDutyHours = 0
                MondayTotalOffDutyHours = 0
                TuesdayTotalHours = 0
                TuesdayTotalOnDutyHours = 0
                TuesdayTotalOffDutyHours = 0
                WednesdayTotalHours = 0
                WednesdayTotalOnDutyHours = 0
                WednesdayTotalOffDutyHours = 0
                ThursdayTotalHours = 0
                ThursdayTotalOnDutyHours = 0
                ThursdayTotalOffDutyHours = 0
                FridayTotalHours = 0
                FridayTotalOnDutyHours = 0
                FridayTotalOffDutyHours = 0
                SaturdayTotalHours = 0
                SaturdayTotalOnDutyHours = 0
                SaturdayTotalOffDutyHours = 0
                SundayTotalHours = 0
                SundayTotalOnDutyHours = 0
                SundayTotalOffDutyHours = 0
                WeeklyTotalHours = 0
                WeeklyTotalOnDutyHours = 0
                WeeklyTotalOffDutyHours = 0
                WeeklyTotalRTHours = 0
                WeeklyTotalRTOnDutyHours = 0
                WeeklyTotalRTOffDutyHours = 0
                WeeklyTotalHTHours = 0
                WeeklyTotalHTOnDutyHours = 0
                WeeklyTotalHTOffDutyHours = 0
                WeeklyTotalOTHours = 0
                WeeklyTotalOTOnDutyHours = 0
                WeeklyTotalOTOffDutyHours = 0
                WeeklyTotalDTHours = 0
                WeeklyTotalDTOnDutyHours = 0
                WeeklyTotalDTOffDutyHours = 0
                WeeklyTotalDollars = 0
                WeeklyTotalOnDutyDollars = 0
                WeeklyTotalOffDutyDollars = 0
                #End Region

            Next
        Next

        #Region "Add a Bottom Rows of Totals"
        Dim RowValue As Object() = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""} ' 25 rows
        RowValue(0) = "Excalibur"
        RowValue(1) = ""
        RowValue(2) = "z Total Excalibur"
        RowValue(3) = ""
        RowValue(4) = ""
        RowValue(5) = ex_paycheck.ToString("C2")
        RowValue(6) = If(ex_MondayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_MondayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(7) = If(ex_TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_TuesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(8) = If(ex_WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_WednesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(9) = If(ex_ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_ThursdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(10) = If(ex_FridayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_FridayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(11) = If(ex_SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_SaturdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(12) = If(ex_SundayTotalHours = 0L, "0", TimeSpan.FromTicks(ex_SundayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(13) = If(ex_WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(14) = If(ex_WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalRTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(15) = If(ex_WeeklyTotalHTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalHTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(16) = If(ex_WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalOTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(17) = If(ex_WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalDTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(18) = ex_hours_requested.ToString("N2").TrimEnd("0").TrimEnd(".") 'ST hours
        RowValue(19) = If(ex_WeeklyTotalOffDutyMealTimeRT = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalOffDutyMealTimeRT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(20) = If(ex_WeeklyTotalOffDutyMealTimeHT = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalOffDutyMealTimeHT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(21) = If(ex_WeeklyTotalOffDutyMealTimeOT = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalOffDutyMealTimeOT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(22) = If(ex_WeeklyTotalOffDutyMealTimeDT = 0L, "0", TimeSpan.FromTicks(ex_WeeklyTotalOffDutyMealTimeDT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(23) = ex_gross_add_ddctn.ToString("C2")
        RowValue(24) = ex_net_add_ddctn.ToString("C2")
        dtable.Rows.Add(RowValue)
        dtable.AcceptChanges()

        RowValue = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""} ' 25 rows
        RowValue(0) = "Matt's Staffing"
        RowValue(1) = ""
        RowValue(2) = "z Total Matt's Staffing"
        RowValue(3) = ""
        RowValue(4) = ""
        RowValue(5) = ms_paycheck.ToString("C2")
        RowValue(6) = If(ms_MondayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_MondayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(7) = If(ms_TuesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_TuesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(8) = If(ms_WednesdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_WednesdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(9) = If(ms_ThursdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_ThursdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(10) = If(ms_FridayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_FridayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(11) = If(ms_SaturdayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_SaturdayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(12) = If(ms_SundayTotalHours = 0L, "0", TimeSpan.FromTicks(ms_SundayTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(13) = If(ms_WeeklyTotalHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(14) = If(ms_WeeklyTotalRTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalRTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(15) = If(ms_WeeklyTotalHTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalHTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(16) = If(ms_WeeklyTotalOTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalOTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(17) = If(ms_WeeklyTotalDTHours = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalDTHours).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(18) = ms_hours_requested.ToString("N2").TrimEnd("0").TrimEnd(".") 'ST hours
        RowValue(19) = If(ms_WeeklyTotalOffDutyMealTimeRT = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalOffDutyMealTimeRT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(20) = If(ms_WeeklyTotalOffDutyMealTimeHT = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalOffDutyMealTimeHT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(21) = If(ms_WeeklyTotalOffDutyMealTimeOT = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalOffDutyMealTimeOT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(22) = If(ms_WeeklyTotalOffDutyMealTimeDT = 0L, "0", TimeSpan.FromTicks(ms_WeeklyTotalOffDutyMealTimeDT).TotalHours.ToString("N2").TrimEnd("0").TrimEnd("."))
        RowValue(23) = ms_gross_add_ddctn.ToString("C2")
        RowValue(24) = ms_net_add_ddctn.ToString("C2")
        dtable.Rows.Add(RowValue)
        dtable.AcceptChanges()
        #End Region

        'now bind datatable to gridview... 
        dtable.DefaultView.Sort = "employee ASC"
        EmployeeTotalsGridView.DataSource = dtable
        EmployeeTotalsGridView.DataBind()

    End Sub

    Protected Sub ExportGridToExcel(company As String)

        If weekOfDate.Text.Length > 0 Then
            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)
        End If

        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("company"))
        dtable.Columns.Add(New DataColumn("workerid"))
        dtable.Columns.Add(New DataColumn("_org"))
        dtable.Columns.Add(New DataColumn("_jobnumber"))
        dtable.Columns.Add(New DataColumn("paycomponent"))
        dtable.Columns.Add(New DataColumn("rate"))
        dtable.Columns.Add(New DataColumn("_ratenumber"))
        dtable.Columns.Add(New DataColumn("hours"))
        dtable.Columns.Add(New DataColumn("_units"))
        dtable.Columns.Add(New DataColumn("_linedate"))
        dtable.Columns.Add(New DataColumn("amount"))
        dtable.Columns.Add(New DataColumn("_checkseqnumber"))
        dtable.Columns.Add(New DataColumn("_overridestate"))
        dtable.Columns.Add(New DataColumn("_overridelocal"))
        dtable.Columns.Add(New DataColumn("_overridelocaljurisdiction"))
        dtable.Columns.Add(New DataColumn("_overridelabor"))

        Dim dt4 As New DataTable()
        Dim sqlCmd4 As New SqlCommand("SELECT employee_id FROM employee_tbl WHERE status='Active'", conn)
        Dim sqlDa4 As New SqlDataAdapter(sqlCmd4)
        conn.close()
        conn.open()
        sqlDa4.Fill(dt4)
        conn.close()

        For Each employee As DataRow In dt4.Rows

            Dim dt As New DataTable()
            Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company, client_site_tbl.name FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE company='"+company.Replace("'", "''")+"' AND employee_id = '" + employee.ItemArray(0).ToString() + "' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
            Dim sqlDa As New SqlDataAdapter(sqlCmd)
            conn.close()
            conn.open()
            sqlDa.Fill(dt)
            conn.close()
            
            #Region "Variable Declaration"
            Dim MondayTotalHoursWorked As Double = 0.0
            Dim TuesdayTotalHoursWorked As Double = 0.0
            Dim WednesdayTotalHoursWorked As Double = 0.0
            Dim ThursdayTotalHoursWorked As Double = 0.0
            Dim FridayTotalHoursWorked As Double = 0.0
            Dim SaturdayTotalHoursWorked As Double = 0.0
            Dim SundayTotalHoursWorked As Double = 0.0
            Dim WeeklyTotalHoursWorked As Double = 0.0

            Dim MondayTotalHoursWorkedWH As Double = 0.0
            Dim TuesdayTotalHoursWorkedWH As Double = 0.0
            Dim WednesdayTotalHoursWorkedWH As Double = 0.0
            Dim ThursdayTotalHoursWorkedWH As Double = 0.0
            Dim FridayTotalHoursWorkedWH As Double = 0.0
            Dim SaturdayTotalHoursWorkedWH As Double = 0.0
            Dim SundayTotalHoursWorkedWH As Double = 0.0
            Dim WeeklyTotalHoursWorkedWH As Double = 0.0

            Dim MondayTotalRT As Double = 0.0
            Dim TuesdayTotalRT As Double = 0.0
            Dim WednesdayTotalRT As Double = 0.0
            Dim ThursdayTotalRT As Double = 0.0
            Dim FridayTotalRT As Double = 0.0
            Dim SaturdayTotalRT As Double = 0.0
            Dim SundayTotalRT As Double = 0.0
            Dim MondayTotalOT As Double = 0.0
            Dim TuesdayTotalOT As Double = 0.0
            Dim WednesdayTotalOT As Double = 0.0
            Dim ThursdayTotalOT As Double = 0.0
            Dim FridayTotalOT As Double = 0.0
            Dim SaturdayTotalOT As Double = 0.0
            Dim SundayTotalOT As Double = 0.0
            Dim MondayTotalDT As Double = 0.0
            Dim TuesdayTotalDT As Double = 0.0
            Dim WednesdayTotalDT As Double = 0.0
            Dim ThursdayTotalDT As Double = 0.0
            Dim FridayTotalDT As Double = 0.0
            Dim SaturdayTotalDT As Double = 0.0
            Dim SundayTotalDT As Double = 0.0

            ' WH stands for West Hollywood (Maxfield LA and Chrome Hearts LA) gets special paycode
            Dim MondayTotalRTWH As Double = 0.0
            Dim TuesdayTotalRTWH As Double = 0.0
            Dim WednesdayTotalRTWH As Double = 0.0
            Dim ThursdayTotalRTWH As Double = 0.0
            Dim FridayTotalRTWH As Double = 0.0
            Dim SaturdayTotalRTWH As Double = 0.0
            Dim SundayTotalRTWH As Double = 0.0
            Dim MondayTotalOTWH As Double = 0.0
            Dim TuesdayTotalOTWH As Double = 0.0
            Dim WednesdayTotalOTWH As Double = 0.0
            Dim ThursdayTotalOTWH As Double = 0.0
            Dim FridayTotalOTWH As Double = 0.0
            Dim SaturdayTotalOTWH As Double = 0.0
            Dim SundayTotalOTWH As Double = 0.0
            Dim MondayTotalDTWH As Double = 0.0
            Dim TuesdayTotalDTWH As Double = 0.0
            Dim WednesdayTotalDTWH As Double = 0.0
            Dim ThursdayTotalDTWH As Double = 0.0
            Dim FridayTotalDTWH As Double = 0.0
            Dim SaturdayTotalDTWH As Double = 0.0
            Dim SundayTotalDTWH As Double = 0.0

            Dim MondayTotalHTWH As Double = 0.0
            Dim TuesdayTotalHTWH As Double = 0.0
            Dim WednesdayTotalHTWH As Double = 0.0
            Dim ThursdayTotalHTWH As Double = 0.0
            Dim FridayTotalHTWH As Double = 0.0
            Dim SaturdayTotalHTWH As Double = 0.0
            Dim SundayTotalHTWH As Double = 0.0
            Dim WeeklyTotalHolidayTimeWH As Double = 0.0

            Dim MondayTotalOffDutyMealTimeRT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeRT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeRT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeRT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeRT As Double = 0.0
            Dim MondayTotalOffDutyMealTimeOT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeOT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeOT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeOT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeOT As Double = 0.0
            Dim MondayTotalOffDutyMealTimeDT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeDT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeDT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeDT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeDT As Double = 0.0
            Dim MondayTotalOffDutyMealTime As Double = 0.0
            Dim TuesdayTotalOffDutyMealTime As Double = 0.0
            Dim WednesdayTotalOffDutyMealTime As Double = 0.0
            Dim ThursdayTotalOffDutyMealTime As Double = 0.0
            Dim FridayTotalOffDutyMealTime As Double = 0.0
            Dim SaturdayTotalOffDutyMealTime As Double = 0.0
            Dim SundayTotalOffDutyMealTime As Double = 0.0
            Dim WeeklyTotalOffDutyMealTime As Double = 0.0

            Dim MondayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeWH As Double = 0.0
   
            #End Region

            Dim _date_ As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date_ = DateTime.Parse(weekOfDate.Text)
                While _date_.DayOfWeek <> DayOfWeek.Monday
                    _date_ = _date_.AddDays(-1)
                End While
            End If

            Dim shifts_worked = dt.Rows.Count

            For Each row In dt.Rows
                Dim wholespan As Double
                Dim firstspan As Double
                Dim secondspan As Double
                Dim startdatetime = DateTime.Parse(row(4))
                Dim enddatetime = DateTime.Parse(row(5))
                Dim client_name As String = row(15).ToString()
                Dim isWHsite As Boolean = False
                If (client_name = "Maxfield LA") Then isWHsite = True
                If (client_name = "Chrome Hearts LA") Then isWHsite = True 

                '' Figure total hours worked by day of week
                If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                    firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                    secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                    Select Case startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            If enddatetime.Date = _date_.Date Then
                                MondayTotalHoursWorked += secondspan
                            Else
                                SundayTotalHoursWorked += firstspan
                            End If
                        Case DayOfWeek.Monday
                            MondayTotalHoursWorked += firstspan
                            TuesdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalHoursWorked += firstspan
                            WednesdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalHoursWorked += firstspan
                            ThursdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Thursday
                            ThursdayTotalHoursWorked += firstspan
                            FridayTotalHoursWorked += secondspan
                        Case DayOfWeek.Friday
                            FridayTotalHoursWorked += firstspan
                            SaturdayTotalHoursWorked += secondspan
                        Case DayOfWeek.Saturday
                            SaturdayTotalHoursWorked += firstspan
                            SundayTotalHoursWorked += secondspan
                    End Select
                Else
                    wholespan = enddatetime.Ticks - startdatetime.Ticks
                    Select Case startdatetime.DayOfWeek
                        Case DayOfWeek.Sunday
                            SundayTotalHoursWorked += wholespan
                        Case DayOfWeek.Monday
                            MondayTotalHoursWorked += wholespan
                        Case DayOfWeek.Tuesday
                            TuesdayTotalHoursWorked += wholespan
                        Case DayOfWeek.Wednesday
                            WednesdayTotalHoursWorked += wholespan
                        Case DayOfWeek.Thursday
                            ThursdayTotalHoursWorked += wholespan
                        Case DayOfWeek.Friday
                            FridayTotalHoursWorked += wholespan
                        Case DayOfWeek.Saturday
                            SaturdayTotalHoursWorked += wholespan
                    End Select
                End If
                
                '' Figure WH total hours worked by day of week
                If isWHsite Then
                    If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                        firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                        secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                        Select Case startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If enddatetime.Date = _date_.Date Then
                                    MondayTotalHoursWorkedWH += secondspan
                                Else
                                    SundayTotalHoursWorkedWH += firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalHoursWorkedWH += firstspan
                                TuesdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalHoursWorkedWH += firstspan
                                WednesdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalHoursWorkedWH += firstspan
                                ThursdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalHoursWorkedWH += firstspan
                                FridayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Friday
                                FridayTotalHoursWorkedWH += firstspan
                                SaturdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalHoursWorkedWH += firstspan
                                SundayTotalHoursWorkedWH += secondspan
                        End Select
                    Else
                        wholespan = enddatetime.Ticks - startdatetime.Ticks
                        Select Case startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                SundayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Monday
                                MondayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Friday
                                FridayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalHoursWorkedWH += wholespan
                        End Select
                    End If
                End If

                #Region "Figure Off Duty Lunch Time"
                '' Figure total off duty lunch time by day of week
                If row(7) = False Then
                    Dim firstmeal_startdatetime As DateTime
                    Dim firstmeal_enddatetime As DateTime
                    Dim secondmeal_startdatetime As DateTime
                    Dim secondmeal_enddatetime As DateTime
                    Dim thirdmeal_startdatetime As DateTime
                    Dim thirdmeal_enddatetime As DateTime

                    Try
                        firstmeal_startdatetime = DateTime.Parse(row(8))
                    Catch ex As Exception
                        firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        firstmeal_enddatetime = DateTime.Parse(row(9))
                    Catch ex As Exception
                        firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        secondmeal_startdatetime = DateTime.Parse(row(10))
                    Catch ex As Exception
                        secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        secondmeal_enddatetime = DateTime.Parse(row(11))
                    Catch ex As Exception
                        secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        thirdmeal_startdatetime = DateTime.Parse(row(12))
                    Catch ex As Exception
                        thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try
                    Try
                        thirdmeal_enddatetime = DateTime.Parse(row(13))
                    Catch ex As Exception
                        thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                    Finally
                    End Try

                    #Region "Total Off Duty Meal Times"
                    If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                        firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                        secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                        Select Case firstmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeRT += secondspan
                                Else
                                    SundayTotalOffDutyMealTimeRT += firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeRT += firstspan
                                TuesdayTotalOffDutyMealTimeRT += secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeRT += firstspan
                                WednesdayTotalOffDutyMealTimeRT += secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeRT += firstspan
                                ThursdayTotalOffDutyMealTimeRT += secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeRT += firstspan
                                FridayTotalOffDutyMealTimeRT += secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeRT += firstspan
                                SaturdayTotalOffDutyMealTimeRT += secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeRT += firstspan
                                SundayTotalOffDutyMealTimeRT += secondspan
                        End Select
                    Else
                        wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                        Select Case firstmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If firstmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeRT += wholespan
                                End If
                            Case DayOfWeek.Monday
                                If firstmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeRT += wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeRT += wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeRT += wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeRT += wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeRT += wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeRT += wholespan
                        End Select
                    End If
                    If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                        firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                        secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                        Select Case secondmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If secondmeal_enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeOT += secondspan
                                Else
                                    SundayTotalOffDutyMealTimeOT += firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeOT += firstspan
                                TuesdayTotalOffDutyMealTimeOT += secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeOT += firstspan
                                WednesdayTotalOffDutyMealTimeOT += secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeOT += firstspan
                                ThursdayTotalOffDutyMealTimeOT += secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeOT += firstspan
                                FridayTotalOffDutyMealTimeOT += secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeOT += firstspan
                                SaturdayTotalOffDutyMealTimeOT += secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeOT += firstspan
                                SundayTotalOffDutyMealTimeOT += secondspan
                        End Select
                    Else
                        wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                        Select Case secondmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If secondmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeOT += wholespan
                                End If
                            Case DayOfWeek.Monday
                                If secondmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeOT += wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeOT += wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeOT += wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeOT += wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeOT += wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeOT += wholespan
                        End Select
                    End If
                    If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                        firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                        secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                        Select Case thirdmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If thirdmeal_enddatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeDT += secondspan
                                Else
                                    SundayTotalOffDutyMealTimeDT += firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalOffDutyMealTimeDT += firstspan
                                TuesdayTotalOffDutyMealTimeDT += secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeDT += firstspan
                                WednesdayTotalOffDutyMealTimeDT += secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeDT += firstspan
                                ThursdayTotalOffDutyMealTimeDT += secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeDT += firstspan
                                FridayTotalOffDutyMealTimeDT += secondspan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeDT += firstspan
                                SaturdayTotalOffDutyMealTimeDT += secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeDT += firstspan
                                SundayTotalOffDutyMealTimeDT += secondspan
                        End Select
                    Else
                        wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                        Select Case thirdmeal_startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If thirdmeal_enddatetime.Date > _date_.Date Then
                                    SundayTotalOffDutyMealTimeDT += wholespan
                                End If
                            Case DayOfWeek.Monday
                                If thirdmeal_startdatetime.Date = _date_.Date Then
                                    MondayTotalOffDutyMealTimeDT += wholespan
                                End If
                            Case DayOfWeek.Tuesday
                                TuesdayTotalOffDutyMealTimeDT += wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalOffDutyMealTimeDT += wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalOffDutyMealTimeDT += wholespan
                            Case DayOfWeek.Friday
                                FridayTotalOffDutyMealTimeDT += wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalOffDutyMealTimeDT += wholespan
                        End Select
                    End If
                    #End Region
                    
                    If isWHsite Then
                        #Region "Total Off Duty Meal Times - WH"
                        If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                            firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                            secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                            Select Case firstmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeRTWH += secondspan
                                        'If dateIsHoliday(enddatetime) Then MondayTotalOffDutyMealTimeHTWH += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeRTWH += firstspan
                                        'If dateIsHoliday(enddatetime) Then SundayTotalOffDutyMealTimeHTWH += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeRTWH += firstspan
                                    TuesdayTotalOffDutyMealTimeRTWH += secondspan
                                    'If dateIsHoliday(startdatetime.Date) Then MondayTotalOffDutyMealTimeHTWH += firstspan
                                    'If dateIsHoliday(enddatetime.Date) Then TuesdayTotalOffDutyMealTimeHTWH += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeRTWH += firstspan
                                    WednesdayTotalOffDutyMealTimeRTWH += secondspan
                                    'If dateIsHoliday(startdatetime.Date) Then TuesdayTotalOffDutyMealTimeHTWH += firstspan
                                    'If dateIsHoliday(enddatetime.Date) Then WednesdayTotalOffDutyMealTimeHTWH += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeRTWH += firstspan
                                    ThursdayTotalOffDutyMealTimeRTWH += secondspan
                                    'If dateIsHoliday(startdatetime.Date) Then WednesdayTotalOffDutyMealTimeHTWH += firstspan
                                    'If dateIsHoliday(enddatetime.Date) Then ThursdayTotalOffDutyMealTimeHTWH += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeRTWH += firstspan
                                    FridayTotalOffDutyMealTimeRTWH += secondspan
                                    'If dateIsHoliday(startdatetime.Date) Then ThursdayTotalOffDutyMealTimeHTWH += firstspan
                                    'If dateIsHoliday(enddatetime.Date) Then FridayTotalOffDutyMealTimeHTWH += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeRTWH += firstspan
                                    SaturdayTotalOffDutyMealTimeRTWH += secondspan
                                    'If dateIsHoliday(startdatetime.Date) Then FridayTotalOffDutyMealTimeHTWH += firstspan
                                    'If dateIsHoliday(enddatetime.Date) Then SaturdayTotalOffDutyMealTimeHTWH += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeRTWH += firstspan
                                    SundayTotalOffDutyMealTimeRTWH += secondspan
                                    'If dateIsHoliday(startdatetime.Date) Then SaturdayTotalOffDutyMealTimeHTWH += firstspan
                                    'If dateIsHoliday(enddatetime.Date) Then SundayTotalOffDutyMealTimeHTWH += secondspan
                            End Select
                        Else
                            wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                            Select Case firstmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If firstmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeRTWH += wholespan
                                        'If dateIsHoliday(enddatetime.Date) Then SundayTotalOffDutyMealTimeHTWH += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If firstmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeRTWH += wholespan
                                        'If dateIsHoliday(startdatetime.Date) Then MondayTotalOffDutyMealTimeHTWH += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeRTWH += wholespan
                                    'If dateIsHoliday(startdatetime.Date) Then TuesdayTotalOffDutyMealTimeHTWH += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeRTWH += wholespan
                                    'If dateIsHoliday(startdatetime.Date) Then WednesdayTotalOffDutyMealTimeHTWH += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeRTWH += wholespan
                                    'If dateIsHoliday(startdatetime.Date) Then ThursdayTotalOffDutyMealTimeHTWH += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeRTWH += wholespan
                                    'If dateIsHoliday(startdatetime.Date) Then FridayTotalOffDutyMealTimeHTWH += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeRTWH += wholespan
                                    'If dateIsHoliday(startdatetime.Date) Then SundayTotalOffDutyMealTimeHTWH += wholespan
                            End Select
                        End If
                        If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                            firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                            secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                            Select Case secondmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If secondmeal_enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeOTWH += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeOTWH += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeOTWH += firstspan
                                    TuesdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeOTWH += firstspan
                                    WednesdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeOTWH += firstspan
                                    ThursdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeOTWH += firstspan
                                    FridayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeOTWH += firstspan
                                    SaturdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeOTWH += firstspan
                                    SundayTotalOffDutyMealTimeOTWH += secondspan
                            End Select
                        Else
                            wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                            Select Case secondmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If secondmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeOTWH += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If secondmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeOTWH += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeOTWH += wholespan
                            End Select
                        End If
                        If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                            firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                            secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                            Select Case thirdmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If thirdmeal_enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeDTWH += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeDTWH += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeDTWH += firstspan
                                    TuesdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeDTWH += firstspan
                                    WednesdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeDTWH += firstspan
                                    ThursdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeDTWH += firstspan
                                    FridayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeDTWH += firstspan
                                    SaturdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeDTWH += firstspan
                                    SundayTotalOffDutyMealTimeDTWH += secondspan
                            End Select
                        Else
                            wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                            Select Case thirdmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If thirdmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeDTWH += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If thirdmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeDTWH += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeDTWH += wholespan
                            End Select
                        End If
                        #End Region
                    End If

                End If
                #End Region
                
            Next

            WeeklyTotalHoursWorked = MondayTotalHoursWorked + TuesdayTotalHoursWorked + WednesdayTotalHoursWorked + ThursdayTotalHoursWorked + FridayTotalHoursWorked + SaturdayTotalHoursWorked + SundayTotalHoursWorked
            WeeklyTotalHolidayTimeWH = MondayTotalHTWH + TuesdayTotalHTWH + WednesdayTotalHTWH + ThursdayTotalHTWH + FridayTotalHTWH + SaturdayTotalHTWH + SundayTotalHTWH

            MondayTotalOffDutyMealTime = MondayTotalOffDutyMealTimeRT + MondayTotalOffDutyMealTimeOT + MondayTotalOffDutyMealTimeDT
            TuesdayTotalOffDutyMealTime = TuesdayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeDT
            WednesdayTotalOffDutyMealTime = WednesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeDT
            ThursdayTotalOffDutyMealTime = ThursdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeDT
            FridayTotalOffDutyMealTime = FridayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeDT
            SaturdayTotalOffDutyMealTime = SaturdayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeDT
            SundayTotalOffDutyMealTime = SundayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeDT
            MondayTotalOffDutyMealTimeWH = MondayTotalOffDutyMealTimeRTWH + MondayTotalOffDutyMealTimeOTWH + MondayTotalOffDutyMealTimeDTWH
            TuesdayTotalOffDutyMealTimeWH = TuesdayTotalOffDutyMealTimeRTWH + TuesdayTotalOffDutyMealTimeOTWH + TuesdayTotalOffDutyMealTimeDTWH
            WednesdayTotalOffDutyMealTimeWH = WednesdayTotalOffDutyMealTimeRTWH + WednesdayTotalOffDutyMealTimeOTWH + WednesdayTotalOffDutyMealTimeDTWH
            ThursdayTotalOffDutyMealTimeWH = ThursdayTotalOffDutyMealTimeRTWH + ThursdayTotalOffDutyMealTimeOTWH + ThursdayTotalOffDutyMealTimeDTWH
            FridayTotalOffDutyMealTimeWH = FridayTotalOffDutyMealTimeRTWH + FridayTotalOffDutyMealTimeOTWH + FridayTotalOffDutyMealTimeDTWH
            SaturdayTotalOffDutyMealTimeWH = SaturdayTotalOffDutyMealTimeRTWH + SaturdayTotalOffDutyMealTimeOTWH + SaturdayTotalOffDutyMealTimeDTWH
            SundayTotalOffDutyMealTimeWH = SundayTotalOffDutyMealTimeRTWH + SundayTotalOffDutyMealTimeOTWH + SundayTotalOffDutyMealTimeDTWH

            Dim halfhour = 10000000L * 60 * 60 * 0.5
            Dim onehour = 10000000L * 60 * 60 * 1
            Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5

            #Region "Set Meal RT OT DT"
            If MondayTotalOffDutyMealTime <= halfhour Then
                MondayTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTime
                MondayTotalOffDutyMealTimeOT = 0
                MondayTotalOffDutyMealTimeDT = 0
            ElseIf MondayTotalOffDutyMealTime > halfhour And MondayTotalOffDutyMealTime <= onehour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTime - halfhour
                MondayTotalOffDutyMealTimeDT = 0
            ElseIf MondayTotalOffDutyMealTime > onehour And MondayTotalOffDutyMealTime <= oneandhalfhour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = halfhour
                MondayTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTime - onehour
            ElseIf MondayTotalOffDutyMealTime > oneandhalfhour Then
                MondayTotalOffDutyMealTimeRT = halfhour
                MondayTotalOffDutyMealTimeOT = halfhour
                MondayTotalOffDutyMealTimeDT = halfhour
            End If
            If TuesdayTotalOffDutyMealTime <= halfhour Then
                TuesdayTotalOffDutyMealTimeRT = TuesdayTotalOffDutyMealTime
                TuesdayTotalOffDutyMealTimeOT = 0
                TuesdayTotalOffDutyMealTimeDT = 0
            ElseIf TuesdayTotalOffDutyMealTime > halfhour And TuesdayTotalOffDutyMealTime <= onehour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = TuesdayTotalOffDutyMealTime - halfhour
                TuesdayTotalOffDutyMealTimeDT = 0
            ElseIf TuesdayTotalOffDutyMealTime > onehour And TuesdayTotalOffDutyMealTime <= oneandhalfhour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = halfhour
                TuesdayTotalOffDutyMealTimeDT = TuesdayTotalOffDutyMealTime - onehour
            ElseIf TuesdayTotalOffDutyMealTime > oneandhalfhour Then
                TuesdayTotalOffDutyMealTimeRT = halfhour
                TuesdayTotalOffDutyMealTimeOT = halfhour
                TuesdayTotalOffDutyMealTimeDT = halfhour
            End If
            If WednesdayTotalOffDutyMealTime <= halfhour Then
                WednesdayTotalOffDutyMealTimeRT = WednesdayTotalOffDutyMealTime
                WednesdayTotalOffDutyMealTimeOT = 0
                WednesdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > halfhour And WednesdayTotalOffDutyMealTime <= onehour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = WednesdayTotalOffDutyMealTime - halfhour
                WednesdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > onehour And WednesdayTotalOffDutyMealTime <= oneandhalfhour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = halfhour
                WednesdayTotalOffDutyMealTimeDT = WednesdayTotalOffDutyMealTime - onehour
            ElseIf WednesdayTotalOffDutyMealTime > oneandhalfhour Then
                WednesdayTotalOffDutyMealTimeRT = halfhour
                WednesdayTotalOffDutyMealTimeOT = halfhour
                WednesdayTotalOffDutyMealTimeDT = halfhour
            End If
            If ThursdayTotalOffDutyMealTime <= halfhour Then
                ThursdayTotalOffDutyMealTimeRT = ThursdayTotalOffDutyMealTime
                ThursdayTotalOffDutyMealTimeOT = 0
                ThursdayTotalOffDutyMealTimeDT = 0
            ElseIf WednesdayTotalOffDutyMealTime > halfhour And ThursdayTotalOffDutyMealTime <= onehour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = ThursdayTotalOffDutyMealTime - halfhour
                ThursdayTotalOffDutyMealTimeDT = 0
            ElseIf ThursdayTotalOffDutyMealTime > onehour And ThursdayTotalOffDutyMealTime <= oneandhalfhour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = halfhour
                ThursdayTotalOffDutyMealTimeDT = ThursdayTotalOffDutyMealTime - onehour
            ElseIf ThursdayTotalOffDutyMealTime > oneandhalfhour Then
                ThursdayTotalOffDutyMealTimeRT = halfhour
                ThursdayTotalOffDutyMealTimeOT = halfhour
                ThursdayTotalOffDutyMealTimeDT = halfhour
            End If
            If FridayTotalOffDutyMealTime <= halfhour Then
                FridayTotalOffDutyMealTimeRT = FridayTotalOffDutyMealTime
                FridayTotalOffDutyMealTimeOT = 0
                FridayTotalOffDutyMealTimeDT = 0
            ElseIf FridayTotalOffDutyMealTime > halfhour And FridayTotalOffDutyMealTime <= onehour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = FridayTotalOffDutyMealTime - halfhour
                FridayTotalOffDutyMealTimeDT = 0
            ElseIf FridayTotalOffDutyMealTime > onehour And FridayTotalOffDutyMealTime <= oneandhalfhour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = halfhour
                FridayTotalOffDutyMealTimeDT = FridayTotalOffDutyMealTime - onehour
            ElseIf FridayTotalOffDutyMealTime > oneandhalfhour Then
                FridayTotalOffDutyMealTimeRT = halfhour
                FridayTotalOffDutyMealTimeOT = halfhour
                FridayTotalOffDutyMealTimeDT = halfhour
            End If
            If SaturdayTotalOffDutyMealTime <= halfhour Then
                SaturdayTotalOffDutyMealTimeRT = SaturdayTotalOffDutyMealTime
                SaturdayTotalOffDutyMealTimeOT = 0
                SaturdayTotalOffDutyMealTimeDT = 0
            ElseIf SaturdayTotalOffDutyMealTime > halfhour And SaturdayTotalOffDutyMealTime <= onehour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = SaturdayTotalOffDutyMealTime - halfhour
                SaturdayTotalOffDutyMealTimeDT = 0
            ElseIf SaturdayTotalOffDutyMealTime > onehour And SaturdayTotalOffDutyMealTime <= oneandhalfhour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = halfhour
                SaturdayTotalOffDutyMealTimeDT = SaturdayTotalOffDutyMealTime - onehour
            ElseIf SaturdayTotalOffDutyMealTime > oneandhalfhour Then
                SaturdayTotalOffDutyMealTimeRT = halfhour
                SaturdayTotalOffDutyMealTimeOT = halfhour
                SaturdayTotalOffDutyMealTimeDT = halfhour
            End If
            If SundayTotalOffDutyMealTime <= halfhour Then
                SundayTotalOffDutyMealTimeRT = SundayTotalOffDutyMealTime
                SundayTotalOffDutyMealTimeOT = 0
                SundayTotalOffDutyMealTimeDT = 0
            ElseIf SundayTotalOffDutyMealTime > halfhour And SundayTotalOffDutyMealTime <= onehour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = SundayTotalOffDutyMealTime - halfhour
                SundayTotalOffDutyMealTimeDT = 0
            ElseIf SundayTotalOffDutyMealTime > onehour And SundayTotalOffDutyMealTime <= oneandhalfhour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = halfhour
                SundayTotalOffDutyMealTimeDT = SundayTotalOffDutyMealTime - onehour
            ElseIf SundayTotalOffDutyMealTime > oneandhalfhour Then
                SundayTotalOffDutyMealTimeRT = halfhour
                SundayTotalOffDutyMealTimeOT = halfhour
                SundayTotalOffDutyMealTimeDT = halfhour
            End If
            #End Region

            WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
            WeeklyTotalOffDutyMealTimeOT = MondayTotalOffDutyMealTimeOT + TuesdayTotalOffDutyMealTimeOT + WednesdayTotalOffDutyMealTimeOT + ThursdayTotalOffDutyMealTimeOT + FridayTotalOffDutyMealTimeOT + SaturdayTotalOffDutyMealTimeOT + SundayTotalOffDutyMealTimeOT
            WeeklyTotalOffDutyMealTimeDT = MondayTotalOffDutyMealTimeDT + TuesdayTotalOffDutyMealTimeDT + WednesdayTotalOffDutyMealTimeDT + ThursdayTotalOffDutyMealTimeDT + FridayTotalOffDutyMealTimeDT + SaturdayTotalOffDutyMealTimeDT + SundayTotalOffDutyMealTimeDT

            WeeklyTotalOffDutyMealTimeRTWH = MondayTotalOffDutyMealTimeRTWH + TuesdayTotalOffDutyMealTimeRTWH + WednesdayTotalOffDutyMealTimeRTWH + ThursdayTotalOffDutyMealTimeRTWH + FridayTotalOffDutyMealTimeRTWH + SaturdayTotalOffDutyMealTimeRTWH + SundayTotalOffDutyMealTimeRTWH
            WeeklyTotalOffDutyMealTimeOTWH = MondayTotalOffDutyMealTimeOTWH + TuesdayTotalOffDutyMealTimeOTWH + WednesdayTotalOffDutyMealTimeOTWH + ThursdayTotalOffDutyMealTimeOTWH + FridayTotalOffDutyMealTimeOTWH + SaturdayTotalOffDutyMealTimeOTWH + SundayTotalOffDutyMealTimeOTWH
            WeeklyTotalOffDutyMealTimeDTWH = MondayTotalOffDutyMealTimeDTWH + TuesdayTotalOffDutyMealTimeDTWH + WednesdayTotalOffDutyMealTimeDTWH + ThursdayTotalOffDutyMealTimeDTWH + FridayTotalOffDutyMealTimeDTWH + SaturdayTotalOffDutyMealTimeDTWH + SundayTotalOffDutyMealTimeDTWH
            WeeklyTotalOffDutyMealTimeWH = WeeklyTotalOffDutyMealTimeRTWH + WeeklyTotalOffDutyMealTimeOTWH + WeeklyTotalOffDutyMealTimeDTWH

            Dim fourhours = 10000000L * 60 * 60 * 4
            Dim eighthours = 10000000L * 60 * 60 * 8
            Dim twelvehours = 10000000L * 60 * 60 * 12
            Dim fourtyhours = 10000000L * 60 * 60 * 40
            Dim WeeklyTotalRegularTime = 0L
            Dim WeeklyTotalOverTime = 0L
            Dim WeeklyTotalDoubleTime = 0L
            Dim WeeklyTotalRegularTimeWH = 0L
            Dim WeeklyTotalOverTimeWH = 0L
            Dim WeeklyTotalDoubleTimeWH = 0L

            #Region "Calculate Total RT OT DT"
            If MondayTotalHoursWorked >= twelvehours Then
                MondayTotalDT = MondayTotalHoursWorked - twelvehours
                MondayTotalOT = fourhours
                MondayTotalRT = eighthours
            ElseIf MondayTotalHoursWorked >= eighthours Then
                MondayTotalOT = MondayTotalHoursWorked - eighthours
                MondayTotalRT = eighthours
            Else
                MondayTotalRT = MondayTotalHoursWorked
            End If
            If TuesdayTotalHoursWorked >= twelvehours Then
                TuesdayTotalDT = TuesdayTotalHoursWorked - twelvehours
                TuesdayTotalOT = fourhours
                TuesdayTotalRT = eighthours
            ElseIf TuesdayTotalHoursWorked >= eighthours Then
                TuesdayTotalOT = TuesdayTotalHoursWorked - eighthours
                TuesdayTotalRT = eighthours
            Else
                TuesdayTotalRT = TuesdayTotalHoursWorked
            End If
            If WednesdayTotalHoursWorked >= twelvehours Then
                WednesdayTotalDT = WednesdayTotalHoursWorked - twelvehours
                WednesdayTotalOT = fourhours
                WednesdayTotalRT = eighthours
            ElseIf WednesdayTotalHoursWorked >= eighthours Then
                WednesdayTotalOT = WednesdayTotalHoursWorked - eighthours
                WednesdayTotalRT = eighthours
            Else
                WednesdayTotalRT = WednesdayTotalHoursWorked
            End If
            If ThursdayTotalHoursWorked >= twelvehours Then
                ThursdayTotalDT = ThursdayTotalHoursWorked - twelvehours
                ThursdayTotalOT = fourhours
                ThursdayTotalRT = eighthours
            ElseIf ThursdayTotalHoursWorked >= eighthours Then
                ThursdayTotalOT = ThursdayTotalHoursWorked - eighthours
                ThursdayTotalRT = eighthours
            Else
                ThursdayTotalRT = ThursdayTotalHoursWorked
            End If
            If FridayTotalHoursWorked >= twelvehours Then
                FridayTotalDT = FridayTotalHoursWorked - twelvehours
                FridayTotalOT = fourhours
                FridayTotalRT = eighthours
            ElseIf FridayTotalHoursWorked >= eighthours Then
                FridayTotalOT = FridayTotalHoursWorked - eighthours
                FridayTotalRT = eighthours
            Else
                FridayTotalRT = FridayTotalHoursWorked
            End If
            If SaturdayTotalHoursWorked >= twelvehours Then
                SaturdayTotalDT = SaturdayTotalHoursWorked - twelvehours
                SaturdayTotalOT = fourhours
                SaturdayTotalRT = eighthours
            ElseIf SaturdayTotalHoursWorked >= eighthours Then
                SaturdayTotalOT = SaturdayTotalHoursWorked - eighthours
                SaturdayTotalRT = eighthours
            Else
                SaturdayTotalRT = SaturdayTotalHoursWorked
            End If
            If SundayTotalHoursWorked >= twelvehours Then
                SundayTotalDT = SundayTotalHoursWorked - twelvehours
                SundayTotalOT = fourhours
                SundayTotalRT = eighthours
            ElseIf SundayTotalHoursWorked >= eighthours Then
                SundayTotalOT = SundayTotalHoursWorked - eighthours
                SundayTotalRT = eighthours
            Else
                SundayTotalRT = SundayTotalHoursWorked
            End If

            WeeklyTotalRegularTime = MondayTotalRT + TuesdayTotalRT + WednesdayTotalRT + ThursdayTotalRT + FridayTotalRT + SaturdayTotalRT + SundayTotalRT
            WeeklyTotalOverTime = MondayTotalOT + TuesdayTotalOT + WednesdayTotalOT + ThursdayTotalOT + FridayTotalOT + SaturdayTotalOT + SundayTotalOT
            WeeklyTotalDoubleTime = MondayTotalDT + TuesdayTotalDT + WednesdayTotalDT + ThursdayTotalDT + FridayTotalDT + SaturdayTotalDT + SundayTotalDT
            #End Region

            #Region "Calculate Total RT(WH) OT(WH) DT(WH)"
            If MondayTotalHoursWorkedWH >= twelvehours Then
                MondayTotalDTWH = MondayTotalHoursWorkedWH - twelvehours
                MondayTotalOTWH = fourhours
                MondayTotalRTWH = eighthours
            ElseIf MondayTotalHoursWorkedWH >= eighthours Then
                MondayTotalOTWH = MondayTotalHoursWorkedWH - eighthours
                MondayTotalRTWH = eighthours
            Else
                MondayTotalRTWH = MondayTotalHoursWorkedWH
            End If
            If TuesdayTotalHoursWorkedWH >= twelvehours Then
                TuesdayTotalDTWH = TuesdayTotalHoursWorkedWH - twelvehours
                TuesdayTotalOTWH = fourhours
                TuesdayTotalRTWH = eighthours
            ElseIf TuesdayTotalHoursWorkedWH >= eighthours Then
                TuesdayTotalOTWH = TuesdayTotalHoursWorkedWH - eighthours
                TuesdayTotalRTWH = eighthours
            Else
                TuesdayTotalRTWH = TuesdayTotalHoursWorkedWH
            End If
            If WednesdayTotalHoursWorkedWH >= twelvehours Then
                WednesdayTotalDTWH = WednesdayTotalHoursWorkedWH - twelvehours
                WednesdayTotalOTWH = fourhours
                WednesdayTotalRTWH = eighthours
            ElseIf WednesdayTotalHoursWorkedWH >= eighthours Then
                WednesdayTotalOTWH = WednesdayTotalHoursWorkedWH - eighthours
                WednesdayTotalRTWH = eighthours
            Else
                WednesdayTotalRTWH = WednesdayTotalHoursWorkedWH
            End If
            If ThursdayTotalHoursWorkedWH >= twelvehours Then
                ThursdayTotalDTWH = ThursdayTotalHoursWorkedWH - twelvehours
                ThursdayTotalOTWH = fourhours
                ThursdayTotalRTWH = eighthours
            ElseIf ThursdayTotalHoursWorkedWH >= eighthours Then
                ThursdayTotalOTWH = ThursdayTotalHoursWorkedWH - eighthours
                ThursdayTotalRTWH = eighthours
            Else
                ThursdayTotalRTWH = ThursdayTotalHoursWorkedWH
            End If
            If FridayTotalHoursWorkedWH >= twelvehours Then
                FridayTotalDTWH = FridayTotalHoursWorkedWH - twelvehours
                FridayTotalOTWH = fourhours
                FridayTotalRTWH = eighthours
            ElseIf FridayTotalHoursWorkedWH >= eighthours Then
                FridayTotalOTWH = FridayTotalHoursWorkedWH - eighthours
                FridayTotalRTWH = eighthours
            Else
                FridayTotalRTWH = FridayTotalHoursWorkedWH
            End If
            If SaturdayTotalHoursWorkedWH >= twelvehours Then
                SaturdayTotalDTWH = SaturdayTotalHoursWorkedWH - twelvehours
                SaturdayTotalOTWH = fourhours
                SaturdayTotalRTWH = eighthours
            ElseIf SaturdayTotalHoursWorkedWH >= eighthours Then
                SaturdayTotalOTWH = SaturdayTotalHoursWorkedWH - eighthours
                SaturdayTotalRTWH = eighthours
            Else
                SaturdayTotalRTWH = SaturdayTotalHoursWorkedWH
            End If
            If SundayTotalHoursWorkedWH >= twelvehours Then
                SundayTotalDTWH = SundayTotalHoursWorkedWH - twelvehours
                SundayTotalOTWH = fourhours
                SundayTotalRTWH = eighthours
            ElseIf SundayTotalHoursWorkedWH >= eighthours Then
                SundayTotalOTWH = SundayTotalHoursWorkedWH - eighthours
                SundayTotalRTWH = eighthours
            Else
                SundayTotalRTWH = SundayTotalHoursWorkedWH
            End If

            WeeklyTotalRegularTimeWH = MondayTotalRTWH + TuesdayTotalRTWH + WednesdayTotalRTWH + ThursdayTotalRTWH + FridayTotalRTWH + SaturdayTotalRTWH + SundayTotalRTWH
            WeeklyTotalOverTimeWH = MondayTotalOTWH + TuesdayTotalOTWH + WednesdayTotalOTWH + ThursdayTotalOTWH + FridayTotalOTWH + SaturdayTotalOTWH + SundayTotalOTWH
            WeeklyTotalDoubleTimeWH = MondayTotalDTWH + TuesdayTotalDTWH + WednesdayTotalDTWH + ThursdayTotalDTWH + FridayTotalDTWH + SaturdayTotalDTWH + SundayTotalDTWH
            #End Region

            If WeeklyTotalHoursWorked - fourtyhours > WeeklyTotalOverTime Then
                WeeklyTotalOverTime = WeeklyTotalHoursWorked - fourtyhours - WeeklyTotalDoubleTime
                WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalOverTime
            End If
       

            #Region "Calculate HT and HT_OffDutyMeal for each day of week"
            Dim MondayTotalHT As Double = 0.0
            Dim TuesdayTotalHT As Double = 0.0
            Dim WednesdayTotalHT As Double = 0.0
            Dim ThursdayTotalHT As Double = 0.0
            Dim FridayTotalHT As Double = 0.0
            Dim SaturdayTotalHT As Double = 0.0
            Dim SundayTotalHT As Double = 0.0
            Dim WeeklyTotalHolidayTime As Double = 0.0
            Dim MondayTotalOffDutyMealTimeHT As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim FridayTotalOffDutyMealTimeHT As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeHT As Double = 0.0
            Dim SundayTotalOffDutyMealTimeHT As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeHT As Double = 0.0
            Dim _date As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date = DateTime.Parse(weekOfDate.Text)
                While _date.DayOfWeek <> DayOfWeek.Monday
                    _date = _date.AddDays(-1)
                End While
            End If
            If dateIsHoliday(_date) Then
                MondayTotalHT = MondayTotalRT
                MondayTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeRT
                MondayTotalRT = 0
                MondayTotalOffDutyMealTimeRT = 0
                MondayTotalHTWH = MondayTotalRTWH
                MondayTotalOffDutyMealTimeHTWH = MondayTotalOffDutyMealTimeRTWH
                MondayTotalRTWH = 0
                MondayTotalOffDutyMealTimeRTWH = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                TuesdayTotalHT = TuesdayTotalRT
                TuesdayTotalOffDutyMealTimeHT = TuesdayTotalOffDutyMealTimeRT
                TuesdayTotalRT = 0
                TuesdayTotalOffDutyMealTimeRT = 0
                TuesdayTotalHTWH = TuesdayTotalRTWH
                TuesdayTotalOffDutyMealTimeHTWH = TuesdayTotalOffDutyMealTimeRTWH
                TuesdayTotalRTWH = 0
                TuesdayTotalOffDutyMealTimeRTWH = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                WednesdayTotalHT = WednesdayTotalRT
                WednesdayTotalOffDutyMealTimeHT = WednesdayTotalOffDutyMealTimeRT
                WednesdayTotalRT = 0
                WednesdayTotalOffDutyMealTimeRT = 0
                WednesdayTotalHTWH = WednesdayTotalRTWH
                WednesdayTotalOffDutyMealTimeHTWH = WednesdayTotalOffDutyMealTimeRTWH
                WednesdayTotalRTWH = 0
                WednesdayTotalOffDutyMealTimeRTWH = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                ThursdayTotalHT = ThursdayTotalRT
                ThursdayTotalOffDutyMealTimeHT = ThursdayTotalOffDutyMealTimeRT
                ThursdayTotalRT = 0
                ThursdayTotalOffDutyMealTimeRT = 0
                ThursdayTotalHTWH = ThursdayTotalRTWH
                ThursdayTotalOffDutyMealTimeHTWH = ThursdayTotalOffDutyMealTimeRTWH
                ThursdayTotalRTWH = 0
                ThursdayTotalOffDutyMealTimeRTWH = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                FridayTotalHT = FridayTotalRT
                FridayTotalOffDutyMealTimeHT = FridayTotalOffDutyMealTimeRT
                FridayTotalRT = 0
                FridayTotalOffDutyMealTimeRT = 0
                FridayTotalHTWH = FridayTotalRTWH
                FridayTotalOffDutyMealTimeHTWH = FridayTotalOffDutyMealTimeRTWH
                FridayTotalRTWH = 0
                FridayTotalOffDutyMealTimeRTWH = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                SaturdayTotalHT = SaturdayTotalRT
                SaturdayTotalOffDutyMealTimeHT = SaturdayTotalOffDutyMealTimeRT
                SaturdayTotalRT = 0
                SaturdayTotalOffDutyMealTimeRT = 0
                SaturdayTotalHTWH = SaturdayTotalRTWH
                SaturdayTotalOffDutyMealTimeHTWH = SaturdayTotalOffDutyMealTimeRTWH
                SaturdayTotalRTWH = 0
                SaturdayTotalOffDutyMealTimeRTWH = 0
            End If
            _date = _date.AddDays(1)
            If dateIsHoliday(_date) Then
                SundayTotalHT = SundayTotalRT
                SundayTotalOffDutyMealTimeHT = SundayTotalOffDutyMealTimeRT
                SundayTotalRT = 0
                SundayTotalOffDutyMealTimeRT = 0
                SundayTotalHTWH = SundayTotalRTWH
                SundayTotalOffDutyMealTimeHTWH = SundayTotalOffDutyMealTimeRTWH
                SundayTotalRTWH = 0
                SundayTotalOffDutyMealTimeRTWH = 0
            End If
            WeeklyTotalHolidayTime = MondayTotalHT + TuesdayTotalHT + WednesdayTotalHT + ThursdayTotalHT + FridayTotalHT + SaturdayTotalHT + SundayTotalHT
            WeeklyTotalOffDutyMealTimeHT = MondayTotalOffDutyMealTimeHT + TuesdayTotalOffDutyMealTimeHT + WednesdayTotalOffDutyMealTimeHT + ThursdayTotalOffDutyMealTimeHT + FridayTotalOffDutyMealTimeHT + SaturdayTotalOffDutyMealTimeHT + SundayTotalOffDutyMealTimeHT
            WeeklyTotalHolidayTimeWH = MondayTotalHTWH + TuesdayTotalHTWH + WednesdayTotalHTWH + ThursdayTotalHTWH + FridayTotalHTWH + SaturdayTotalHTWH + SundayTotalHTWH
            WeeklyTotalOffDutyMealTimeHTWH = MondayTotalOffDutyMealTimeHTWH + TuesdayTotalOffDutyMealTimeHTWH + WednesdayTotalOffDutyMealTimeHTWH + ThursdayTotalOffDutyMealTimeHTWH + FridayTotalOffDutyMealTimeHTWH + SaturdayTotalOffDutyMealTimeHTWH + SundayTotalOffDutyMealTimeHTWH
            #End Region

            Dim MondayTotalHoursWorkedMinusLunches As Double = MondayTotalHoursWorked - MondayTotalOffDutyMealTime
            Dim TuesdayTotalHoursWorkedMinusLunches As Double = TuesdayTotalHoursWorked - TuesdayTotalOffDutyMealTime
            Dim WednesdayTotalHoursWorkedMinusLunches As Double = WednesdayTotalHoursWorked - WednesdayTotalOffDutyMealTime
            Dim ThursdayTotalHoursWorkedMinusLunches As Double = ThursdayTotalHoursWorked - ThursdayTotalOffDutyMealTime
            Dim FridayTotalHoursWorkedMinusLunches As Double = FridayTotalHoursWorked - FridayTotalOffDutyMealTime
            Dim SaturdayTotalHoursWorkedMinusLunches As Double = SaturdayTotalHoursWorked - SaturdayTotalOffDutyMealTime
            Dim SundayTotalHoursWorkedMinusLunches As Double = SundayTotalHoursWorked - SundayTotalOffDutyMealTime

            Dim MondayTotalHoursWorkedMinusLunchesWH As Double = MondayTotalHoursWorkedWH - MondayTotalOffDutyMealTimeWH
            Dim TuesdayTotalHoursWorkedMinusLunchesWH As Double = TuesdayTotalHoursWorkedWH - TuesdayTotalOffDutyMealTimeWH
            Dim WednesdayTotalHoursWorkedMinusLunchesWH As Double = WednesdayTotalHoursWorkedWH - WednesdayTotalOffDutyMealTimeWH
            Dim ThursdayTotalHoursWorkedMinusLunchesWH As Double = ThursdayTotalHoursWorkedWH - ThursdayTotalOffDutyMealTimeWH
            Dim FridayTotalHoursWorkedMinusLunchesWH As Double = FridayTotalHoursWorkedWH - FridayTotalOffDutyMealTimeWH
            Dim SaturdayTotalHoursWorkedMinusLunchesWH As Double = SaturdayTotalHoursWorkedWH - SaturdayTotalOffDutyMealTimeWH
            Dim SundayTotalHoursWorkedMinusLunchesWH As Double = SundayTotalHoursWorkedWH - SundayTotalOffDutyMealTimeWH

            '' Subtract HT from RT
            WeeklyTotalRegularTime = WeeklyTotalRegularTime - WeeklyTotalHolidayTime
            WeeklyTotalRegularTimeWH = WeeklyTotalRegularTimeWH - WeeklyTotalHolidayTimeWH
            '' Recalculate Total MealTimes RT
            WeeklyTotalOffDutyMealTimeRT = MondayTotalOffDutyMealTimeRT + TuesdayTotalOffDutyMealTimeRT + WednesdayTotalOffDutyMealTimeRT + ThursdayTotalOffDutyMealTimeRT + FridayTotalOffDutyMealTimeRT + SaturdayTotalOffDutyMealTimeRT + SundayTotalOffDutyMealTimeRT
            WeeklyTotalOffDutyMealTimeRTWH = MondayTotalOffDutyMealTimeRTWH + TuesdayTotalOffDutyMealTimeRTWH + WednesdayTotalOffDutyMealTimeRTWH + ThursdayTotalOffDutyMealTimeRTWH + FridayTotalOffDutyMealTimeRTWH + SaturdayTotalOffDutyMealTimeRTWH + SundayTotalOffDutyMealTimeRTWH
            '' Calculate Total MealTime
            WeeklyTotalOffDutyMealTime = WeeklyTotalOffDutyMealTimeRT + WeeklyTotalOffDutyMealTimeOT + WeeklyTotalOffDutyMealTimeDT + WeeklyTotalOffDutyMealTimeHT
            WeeklyTotalOffDutyMealTimeWH = WeeklyTotalOffDutyMealTimeRTWH + WeeklyTotalOffDutyMealTimeOTWH + WeeklyTotalOffDutyMealTimeDTWH + WeeklyTotalOffDutyMealTimeHTWH

            '' Calculate WeeklyTotalHoursWorked Minus Lunches
            Dim WeeklyTotalHoursWorkedMinusLunches As Double = WeeklyTotalHoursWorked - WeeklyTotalOffDutyMealTime
            Dim WeeklyTotalRegularTimeMinusLunches As Double = WeeklyTotalRegularTime - WeeklyTotalOffDutyMealTimeRT
            Dim WeeklyTotalHolidayTimeMinusLunches As Double = WeeklyTotalHolidayTime - WeeklyTotalOffDutyMealTimeHT
            Dim WeeklyTotalOverTimeMinusLunches As Double = WeeklyTotalOverTime - WeeklyTotalOffDutyMealTimeOT 
            Dim WeeklyTotalDoubleTimeMinusLunches As Double = WeeklyTotalDoubleTime - WeeklyTotalOffDutyMealTimeDT  
            Dim WeeklyTotalHoursWorkedMinusLunchesWH As Double = WeeklyTotalHoursWorkedWH - WeeklyTotalOffDutyMealTimeWH
            Dim WeeklyTotalRegularTimeMinusLunchesWH As Double = WeeklyTotalRegularTimeWH - WeeklyTotalOffDutyMealTimeRTWH
            Dim WeeklyTotalHolidayTimeMinusLunchesWH As Double = WeeklyTotalHolidayTimeWH - WeeklyTotalOffDutyMealTimeHTWH
            Dim WeeklyTotalOverTimeMinusLunchesWH As Double = WeeklyTotalOverTimeWH - WeeklyTotalOffDutyMealTimeOTWH 
            Dim WeeklyTotalDoubleTimeMinusLunchesWH As Double = WeeklyTotalDoubleTimeWH - WeeklyTotalOffDutyMealTimeDTWH

            WeeklyTotalHoursWorkedMinusLunches -= WeeklyTotalHoursWorkedMinusLunchesWH
            WeeklyTotalRegularTimeMinusLunches -= WeeklyTotalRegularTimeMinusLunchesWH
            WeeklyTotalHolidayTimeMinusLunches -= WeeklyTotalHolidayTimeMinusLunchesWH
            WeeklyTotalOverTimeMinusLunches -= WeeklyTotalOverTimeMinusLunchesWH
            WeeklyTotalDoubleTimeMinusLunches -= WeeklyTotalDoubleTimeMinusLunchesWH

            Dim dt1 As New DataTable()
            Dim sqlCmd1 As New SqlCommand("SELECT ms_id, excalibur_id, basepayrate, company FROM employee_tbl WHERE employee_id = '" + employee.ItemArray(0).ToString() + "'", conn)
            Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
            conn.close()
            conn.open()
            sqlDa1.Fill(dt1)
            conn.close()
            Dim employees_company As String = dt1.Rows(0).ItemArray(3).ToString()
            Dim worker_id As Int32 = If (employees_company = "Excalibur", dt1.Rows(0).ItemArray(1).ToString(), dt1.Rows(0).ItemArray(0).ToString())
            Dim rate As Double = dt1.Rows(0).ItemArray(2).ToString()
            
            
            #Region "Add PayCodes to GridView"

            'Add Shift Pay for this employee
            Dim RowValues As Object() = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            
            '' Pay Components
            'RT Hours, OT Hours, DT Hours, ST Hours, Gross Additions, GrossDeductions, 
            'RT Net Pay Reimburse, OT Net Pay Reimburse, Other Reimbursements, Net Deductions, 
            'Uniform, RT Hours - WH, OT Hours - WH, DT Hours - WH, ST - WHOL Hrs

            'Add RT
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalRegularTimeMinusLunches > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "RT Hours"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalRegularTimeMinusLunches).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add HT
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalHolidayTimeMinusLunches > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "OT Hours"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinusLunches).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add OT
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalOverTimeMinusLunches > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "OT Hours"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalOverTimeMinusLunches).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add DT
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalDoubleTimeMinusLunches > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "DT Hours"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinusLunches).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add RT WH
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalRegularTimeMinusLunchesWH > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "RT Hours - WH"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalRegularTimeMinusLunchesWH).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add HT WH
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalHolidayTimeMinusLunchesWH > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "OT Hours - WH"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalHolidayTimeMinusLunchesWH).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add OT WH
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalOverTimeMinusLunchesWH > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "OT Hours - WH"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalOverTimeMinusLunchesWH).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add DT WH
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalDoubleTimeMinusLunchesWH > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "DT Hours - WH"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalDoubleTimeMinusLunchesWH).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add RT Lunch Reimburse
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalOffDutyMealTimeRT > 0 Then
                RowValues(0) = 16057793
                RowValues(1) = worker_id
                RowValues(4) = "RT Net Pay Reimburse"
                RowValues(5) = "" 'rate
                RowValues(7) = "" 'hours
                RowValues(10) = (Double.Parse(rate.ToString()) * 1.0) * (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeRT).TotalHours)  'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add HT Lunch Reimburse
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalOffDutyMealTimeHT > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "OT Net Pay Reimburse"
                RowValues(5) = "" 'rate
                RowValues(7) = "" 'hours
                RowValues(10) = (Double.Parse(rate.ToString()) * 1.5) * (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeHT).TotalHours) 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add OT Lunch Reimburse
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalOffDutyMealTimeOT > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "OT Net Pay Reimburse"
                RowValues(5) = "" 'rate
                RowValues(7) = "" 'hours
                RowValues(10) = (Double.Parse(rate.ToString()) * 1.5) * (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeOT).TotalHours) 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            'Add DT Lunch Reimburse
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalOffDutyMealTimeDT > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "DT Net Pay Reimburse"
                RowValues(5) = "" 'rate
                RowValues(7) = "" 'hours
                RowValues(10) = (Double.Parse(rate.ToString()) * 2.0) * (TimeSpan.FromTicks(WeeklyTotalOffDutyMealTimeDT).TotalHours) 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If

            'Add Each Item from Reimbursement Table for this employee if Excalibur is their main company
            Dim dt2 As New DataTable()
            Dim sqlCmd2 As New SqlCommand("SELECT reimbursement_tbl.amount, reimbursement_tbl.is_gross_taxablewage FROM reimbursement_tbl LEFT JOIN employee_tbl ON reimbursement_tbl.employee_id = employee_tbl.employee_id WHERE reimbursement_tbl.employee_id = '" + employee.ItemArray(0).ToString() + "' AND employee_tbl.company = '"+company.Replace("'", "''")+ "' AND (datetime >= '" + DateTime.Parse(startdate).ToString() + "' AND datetime <= '" + DateTime.Parse(enddate).ToString() + "')", conn)
            Dim sqlDa2 As New SqlDataAdapter(sqlCmd2)
            conn.close()
            conn.open()
            sqlDa2.Fill(dt2)
            conn.close()

            Dim number_of_reimbursements As Integer = dt2.Rows.Count

            For Each row In dt2.Rows
                Dim amount As Double = row(0).ToString()
                Dim is_gross_taxablewage As Boolean = row(1).ToString()
                If amount > 0 Then
                    If is_gross_taxablewage Then
                        RowValues(4) = "Gross Additions"
                    Else
                        RowValues(4) = "Other Reimbursements"
                    End If
                Else If amount < 0 Then
                    If is_gross_taxablewage Then
                        RowValues(4) = "Gross Deductions"
                    Else
                        RowValues(4) = "Net Deductions"
                    End If
                End If
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(5) = "" 'rate
                RowValues(7) = "" 'hours
                RowValues(10) = amount.ToString("N6")
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            Next

            'Add Each Item from SickPay Request Table for this employee if Excalibur is their main company
            Dim dt3 As New DataTable()
            Dim sqlCmd3 As New SqlCommand("SELECT sickpay_request_tbl.minutes_requested FROM sickpay_request_tbl LEFT JOIN employee_tbl ON sickpay_request_tbl.employee_id = employee_tbl.employee_id WHERE sickpay_request_tbl.employee_id = '" + employee.ItemArray(0).ToString() + "' AND employee_tbl.company = '"+company.Replace("'", "''")+"' AND (datetime >= '" + DateTime.Parse(startdate).ToString() + "' AND datetime <= '" + DateTime.Parse(enddate).ToString() + "')", conn)
            Dim sqlDa3 As New SqlDataAdapter(sqlCmd3)
            conn.close()
            conn.open()
            sqlDa3.Fill(dt3)
            conn.close()
            For Each row In dt3.Rows
                Dim minutes_requested As Double = row(0).ToString()
                If minutes_requested > 0 Then
                    RowValues(0) = "16057793"
                    RowValues(1) = worker_id
                    RowValues(4) = "ST Hours"
                    RowValues(5) = rate 'rate
                    RowValues(7) = (minutes_requested/60.000000).ToString("N6") 'hours
                    RowValues(10) = "" 'amount
                    dtable.Rows.Add(RowValues)
                    dtable.AcceptChanges()
                End If
            Next            

            'Add Other Reimbursement (Phone Use 2min or 5min, OT rate if 40+hrs)  if Excalibur is their main company
            '5min if 1st Week of the month, else 2min
            'Problem: Total Hours >40   .... WeeklyTotalHoursWorked is only for Matts shifts
            If shifts_worked > 0 Or number_of_reimbursements > 0  Then
                If enddate.Day <= 7 Then
                    RowValues(10) = (rate * 1.5 * (5.000000/60.000000)).ToString("N6") 'amount
                Else 
                    RowValues(10) = (rate * 1.5 * (2.000000/60.000000)).ToString("N6") 'amount
                End If
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "Gross Additions"
                RowValues(5) = ""
                RowValues(7) = ""
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
            
            ' Add 25 cents for phone and internet usage
            If shifts_worked > 0 Or number_of_reimbursements > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "Other Reimbursements"
                RowValues(5) = ""
                RowValues(7) = ""
                RowValues(10) = "0.25"
                If (WeeklyTotalOffDutyMealTimeRT <> 0L) Then
                    If enddate.Day <= 7 Then
                        RowValues(10) = 0.25 - (rate * 1.5 * (5.000000/60.000000)).ToString("N6") 'amount
                    Else 
                        RowValues(10) = 0.25 - (rate * 1.5 * (2.000000/60.000000)).ToString("N6") 'amount
                    End If
                End If
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If

            #End Region

        Next

        'now bind datatable to gridview... 
        If (company="Excalibur") Then
            ExcaliburFlexGridView.DataSource = dtable
            ExcaliburFlexGridView.DataBind()
        Else
            MattsFlexGridView.DataSource = dtable
            MattsFlexGridView.DataBind()
        End If

        Response.Clear()
        Response.Buffer = True
        Response.ClearContent()
        Response.ClearHeaders()
        Response.Charset = ""
        Dim strwritter = New StringWriter()
        Dim htmltextwrtter = New HtmlTextWriter(strwritter)
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        Response.ContentType = "application/vnd.ms-excel"
        If (company = "Excalibur") Then
            Dim FileName As String = "ExcaliburFlex-" + DateTime.Now + ".xls"
            Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName)
            ExcaliburFlexGridView.GridLines = GridLines.Both
            ExcaliburFlexGridView.HeaderStyle.Font.Bold = True
            ExcaliburFlexGridView.RenderBeginTag(htmltextwrtter)
            If Not IsNothing(ExcaliburFlexGridView.HeaderRow) Then
                ExcaliburFlexGridView.HeaderRow.RenderControl(htmltextwrtter)
            End If
            For Each row in ExcaliburFlexGridView.Rows
                row.RenderControl(htmltextwrtter)
            Next
            If Not IsNothing(ExcaliburFlexGridView.HeaderRow) Then
                ExcaliburFlexGridView.FooterRow.RenderControl(htmltextwrtter)
            End If
            ExcaliburFlexGridView.RenderEndTag(htmltextwrtter)
        Else
            Dim FileName As String = "MattsFlex-" + DateTime.Now + ".xls"
            Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName)
            MattsFlexGridView.GridLines = GridLines.Both
            MattsFlexGridView.HeaderStyle.Font.Bold = True
            MattsFlexGridView.RenderBeginTag(htmltextwrtter)
            If Not IsNothing(MattsFlexGridView.HeaderRow) Then
                MattsFlexGridView.HeaderRow.RenderControl(htmltextwrtter)
            End If
            For Each row in MattsFlexGridView.Rows
                row.RenderControl(htmltextwrtter)
            Next
            If Not IsNothing(MattsFlexGridView.HeaderRow) Then
                MattsFlexGridView.FooterRow.RenderControl(htmltextwrtter)
            End If
            MattsFlexGridView.RenderEndTag(htmltextwrtter)
        End IF
        Response.Write(strwritter.ToString())
        Response.End()

    End Sub
    Protected Sub CreatePTOFile(sender As Object, e As EventArgs)

        If weekOfDate.Text.Length > 0 Then
            startdate = DateTime.Parse(weekOfDate.Text)
            enddate = startdate

            While startdate.DayOfWeek <> DayOfWeek.Monday
                startdate = startdate.AddDays(-1)
            End While
            While enddate.DayOfWeek <> DayOfWeek.Sunday
                enddate = enddate.AddDays(1)
            End While
            enddate = enddate.AddHours(23)
            enddate = enddate.AddMinutes(59)
        End If

        Dim dtable As New DataTable
        dtable.Columns.Add(New DataColumn("company"))
        dtable.Columns.Add(New DataColumn("workerid"))
        dtable.Columns.Add(New DataColumn("_org"))
        dtable.Columns.Add(New DataColumn("_jobnumber"))
        dtable.Columns.Add(New DataColumn("paycomponent"))
        dtable.Columns.Add(New DataColumn("rate"))
        dtable.Columns.Add(New DataColumn("_ratenumber"))
        dtable.Columns.Add(New DataColumn("hours"))
        dtable.Columns.Add(New DataColumn("_units"))
        dtable.Columns.Add(New DataColumn("_linedate"))
        dtable.Columns.Add(New DataColumn("amount"))
        dtable.Columns.Add(New DataColumn("_checkseqnumber"))
        dtable.Columns.Add(New DataColumn("_overridestate"))
        dtable.Columns.Add(New DataColumn("_overridelocal"))
        dtable.Columns.Add(New DataColumn("_overridelocaljurisdiction"))
        dtable.Columns.Add(New DataColumn("_overridelabor"))

        Dim dt4 As New DataTable()
        Dim sqlCmd4 As New SqlCommand("SELECT employee_id FROM employee_tbl WHERE status='Active'", conn)
        Dim sqlDa4 As New SqlDataAdapter(sqlCmd4)
        conn.close()
        conn.open()
        sqlDa4.Fill(dt4)
        conn.close()

        For Each employee As DataRow In dt4.Rows

            Dim dt As New DataTable()
            Dim sqlCmd As New SqlCommand("SELECT client_site_shift_instance_tbl.client_site_shift_instance_id, client_site_shift_instance_tbl.shift_name, client_site_shift_instance_tbl.shift_notes, client_site_shift_instance_tbl.employee_id, client_site_shift_instance_tbl.startdatetime, client_site_shift_instance_tbl.enddatetime, client_site_shift_instance_tbl.payrate, client_site_shift_instance_tbl.onduty_mealperiods, client_site_shift_instance_tbl.firstmeal_startdatetime, client_site_shift_instance_tbl.firstmeal_enddatetime, client_site_shift_instance_tbl.secondmeal_startdatetime, client_site_shift_instance_tbl.secondmeal_enddatetime, client_site_shift_instance_tbl.thirdmeal_startdatetime, client_site_shift_instance_tbl.thirdmeal_enddatetime, client_site_tbl.company, client_site_tbl.name FROM client_site_shift_instance_tbl LEFT JOIN client_site_tbl ON client_site_tbl.client_site_id = client_site_shift_instance_tbl.client_site_id WHERE employee_id = '" + employee.ItemArray(0).ToString() + "' AND ((startdatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND startdatetime <= '" + DateTime.Parse(enddate).ToString() + "') OR (enddatetime >= '" + DateTime.Parse(startdate).ToString() + "' AND enddatetime <= '" + DateTime.Parse(enddate).ToString() + "'))", conn)
            Dim sqlDa As New SqlDataAdapter(sqlCmd)
            conn.close()
            conn.open()
            sqlDa.Fill(dt)
            conn.close()
            
            #Region "Variable Declaration"

            Dim MondayTotalHoursWorkedWH As Double = 0.0
            Dim TuesdayTotalHoursWorkedWH As Double = 0.0
            Dim WednesdayTotalHoursWorkedWH As Double = 0.0
            Dim ThursdayTotalHoursWorkedWH As Double = 0.0
            Dim FridayTotalHoursWorkedWH As Double = 0.0
            Dim SaturdayTotalHoursWorkedWH As Double = 0.0
            Dim SundayTotalHoursWorkedWH As Double = 0.0
            Dim WeeklyTotalHoursWorkedWH As Double = 0.0


            ' WH stands for West Hollywood (Maxfield LA and Chrome Hearts LA) gets special paycode
            Dim MondayTotalRTWH As Double = 0.0
            Dim TuesdayTotalRTWH As Double = 0.0
            Dim WednesdayTotalRTWH As Double = 0.0
            Dim ThursdayTotalRTWH As Double = 0.0
            Dim FridayTotalRTWH As Double = 0.0
            Dim SaturdayTotalRTWH As Double = 0.0
            Dim SundayTotalRTWH As Double = 0.0
            Dim MondayTotalOTWH As Double = 0.0
            Dim TuesdayTotalOTWH As Double = 0.0
            Dim WednesdayTotalOTWH As Double = 0.0
            Dim ThursdayTotalOTWH As Double = 0.0
            Dim FridayTotalOTWH As Double = 0.0
            Dim SaturdayTotalOTWH As Double = 0.0
            Dim SundayTotalOTWH As Double = 0.0
            Dim MondayTotalDTWH As Double = 0.0
            Dim TuesdayTotalDTWH As Double = 0.0
            Dim WednesdayTotalDTWH As Double = 0.0
            Dim ThursdayTotalDTWH As Double = 0.0
            Dim FridayTotalDTWH As Double = 0.0
            Dim SaturdayTotalDTWH As Double = 0.0
            Dim SundayTotalDTWH As Double = 0.0

            Dim MondayTotalHTWH As Double = 0.0
            Dim TuesdayTotalHTWH As Double = 0.0
            Dim WednesdayTotalHTWH As Double = 0.0
            Dim ThursdayTotalHTWH As Double = 0.0
            Dim FridayTotalHTWH As Double = 0.0
            Dim SaturdayTotalHTWH As Double = 0.0
            Dim SundayTotalHTWH As Double = 0.0
            Dim WeeklyTotalHolidayTimeWH As Double = 0.0

            Dim MondayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeRTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeHTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeOTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeDTWH As Double = 0.0
            Dim MondayTotalOffDutyMealTimeWH As Double = 0.0
            Dim TuesdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim WednesdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim ThursdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim FridayTotalOffDutyMealTimeWH As Double = 0.0
            Dim SaturdayTotalOffDutyMealTimeWH As Double = 0.0
            Dim SundayTotalOffDutyMealTimeWH As Double = 0.0
            Dim WeeklyTotalOffDutyMealTimeWH As Double = 0.0
   
            #End Region

            Dim _date_ As DateTime
            If weekOfDate.Text.Length > 0 Then
                _date_ = DateTime.Parse(weekOfDate.Text)
                While _date_.DayOfWeek <> DayOfWeek.Monday
                    _date_ = _date_.AddDays(-1)
                End While
            End If

            Dim shifts_worked = dt.Rows.Count

            For Each row In dt.Rows
                Dim wholespan As Double
                Dim firstspan As Double
                Dim secondspan As Double
                Dim startdatetime = DateTime.Parse(row(4))
                Dim enddatetime = DateTime.Parse(row(5))
                Dim client_name As String = row(15).ToString()
                Dim isWHsite As Boolean = False
                If (client_name = "Maxfield LA") Then isWHsite = True
                If (client_name = "Chrome Hearts LA") Then isWHsite = True 
                
                '' Figure WH total hours worked by day of week
                If isWHsite Then
                    If (startdatetime.DayOfWeek <> enddatetime.DayOfWeek) Then
                        firstspan = (enddatetime.Date.Ticks - startdatetime.Ticks)
                        secondspan = (enddatetime.Ticks - enddatetime.Date.Ticks)
                        Select Case startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                If enddatetime.Date = _date_.Date Then
                                    MondayTotalHoursWorkedWH += secondspan
                                Else
                                    SundayTotalHoursWorkedWH += firstspan
                                End If
                            Case DayOfWeek.Monday
                                MondayTotalHoursWorkedWH += firstspan
                                TuesdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalHoursWorkedWH += firstspan
                                WednesdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalHoursWorkedWH += firstspan
                                ThursdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Thursday
                                ThursdayTotalHoursWorkedWH += firstspan
                                FridayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Friday
                                FridayTotalHoursWorkedWH += firstspan
                                SaturdayTotalHoursWorkedWH += secondspan
                            Case DayOfWeek.Saturday
                                SaturdayTotalHoursWorkedWH += firstspan
                                SundayTotalHoursWorkedWH += secondspan
                        End Select
                    Else
                        wholespan = enddatetime.Ticks - startdatetime.Ticks
                        Select Case startdatetime.DayOfWeek
                            Case DayOfWeek.Sunday
                                SundayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Monday
                                MondayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Tuesday
                                TuesdayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Wednesday
                                WednesdayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Thursday
                                ThursdayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Friday
                                FridayTotalHoursWorkedWH += wholespan
                            Case DayOfWeek.Saturday
                                SaturdayTotalHoursWorkedWH += wholespan
                        End Select
                    End If
                

                    #Region "If-Off-Duty-Lunch"
                    '' Figure total off duty lunch time by day of week
                    If row(7) = False Then
                        Dim firstmeal_startdatetime As DateTime
                        Dim firstmeal_enddatetime As DateTime
                        Dim secondmeal_startdatetime As DateTime
                        Dim secondmeal_enddatetime As DateTime
                        Dim thirdmeal_startdatetime As DateTime
                        Dim thirdmeal_enddatetime As DateTime

                        #Region "Get Meal Times"
                        Try
                            firstmeal_startdatetime = DateTime.Parse(row(8))
                        Catch ex As Exception
                            firstmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            firstmeal_enddatetime = DateTime.Parse(row(9))
                        Catch ex As Exception
                            firstmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            secondmeal_startdatetime = DateTime.Parse(row(10))
                        Catch ex As Exception
                            secondmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            secondmeal_enddatetime = DateTime.Parse(row(11))
                        Catch ex As Exception
                            secondmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            thirdmeal_startdatetime = DateTime.Parse(row(12))
                        Catch ex As Exception
                            thirdmeal_startdatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        Try
                            thirdmeal_enddatetime = DateTime.Parse(row(13))
                        Catch ex As Exception
                            thirdmeal_enddatetime = DateTime.Parse("2000-01-01 00:00")
                        Finally
                        End Try
                        #End Region

                        #Region "Total Off Duty Meal Times - WH"
                        If (firstmeal_startdatetime.DayOfWeek <> firstmeal_enddatetime.DayOfWeek) Then
                            firstspan = (firstmeal_enddatetime.Date.Ticks - firstmeal_startdatetime.Ticks)
                            secondspan = (firstmeal_enddatetime.Ticks - firstmeal_enddatetime.Date.Ticks)
                            Select Case firstmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeRTWH += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeRTWH += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeRTWH += firstspan
                                    TuesdayTotalOffDutyMealTimeRTWH += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeRTWH += firstspan
                                    WednesdayTotalOffDutyMealTimeRTWH += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeRTWH += firstspan
                                    ThursdayTotalOffDutyMealTimeRTWH += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeRTWH += firstspan
                                    FridayTotalOffDutyMealTimeRTWH += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeRTWH += firstspan
                                    SaturdayTotalOffDutyMealTimeRTWH += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeRTWH += firstspan
                                    SundayTotalOffDutyMealTimeRTWH += secondspan
                            End Select
                        Else
                            wholespan = firstmeal_enddatetime.Ticks - firstmeal_startdatetime.Ticks
                            Select Case firstmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If firstmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeRTWH += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If firstmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeRTWH += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeRTWH += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeRTWH += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeRTWH += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeRTWH += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeRTWH += wholespan
                            End Select
                        End If
                        If (secondmeal_startdatetime.DayOfWeek <> secondmeal_enddatetime.DayOfWeek) Then
                            firstspan = (secondmeal_enddatetime.Date.Ticks - secondmeal_startdatetime.Ticks)
                            secondspan = (secondmeal_enddatetime.Ticks - secondmeal_enddatetime.Date.Ticks)
                            Select Case secondmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If secondmeal_enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeOTWH += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeOTWH += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeOTWH += firstspan
                                    TuesdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeOTWH += firstspan
                                    WednesdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeOTWH += firstspan
                                    ThursdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeOTWH += firstspan
                                    FridayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeOTWH += firstspan
                                    SaturdayTotalOffDutyMealTimeOTWH += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeOTWH += firstspan
                                    SundayTotalOffDutyMealTimeOTWH += secondspan
                            End Select
                        Else
                            wholespan = secondmeal_enddatetime.Ticks - secondmeal_startdatetime.Ticks
                            Select Case secondmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If secondmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeOTWH += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If secondmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeOTWH += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeOTWH += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeOTWH += wholespan
                            End Select
                        End If
                        If (thirdmeal_startdatetime.DayOfWeek <> thirdmeal_enddatetime.DayOfWeek) Then
                            firstspan = (thirdmeal_enddatetime.Date.Ticks - thirdmeal_startdatetime.Ticks)
                            secondspan = (thirdmeal_enddatetime.Ticks - thirdmeal_enddatetime.Date.Ticks)
                            Select Case thirdmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If thirdmeal_enddatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeDTWH += secondspan
                                    Else
                                        SundayTotalOffDutyMealTimeDTWH += firstspan
                                    End If
                                Case DayOfWeek.Monday
                                    MondayTotalOffDutyMealTimeDTWH += firstspan
                                    TuesdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeDTWH += firstspan
                                    WednesdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeDTWH += firstspan
                                    ThursdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeDTWH += firstspan
                                    FridayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeDTWH += firstspan
                                    SaturdayTotalOffDutyMealTimeDTWH += secondspan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeDTWH += firstspan
                                    SundayTotalOffDutyMealTimeDTWH += secondspan
                            End Select
                        Else
                            wholespan = thirdmeal_enddatetime.Ticks - thirdmeal_startdatetime.Ticks
                            Select Case thirdmeal_startdatetime.DayOfWeek
                                Case DayOfWeek.Sunday
                                    If thirdmeal_enddatetime.Date > _date_.Date Then
                                        SundayTotalOffDutyMealTimeDTWH += wholespan
                                    End If
                                Case DayOfWeek.Monday
                                    If thirdmeal_startdatetime.Date = _date_.Date Then
                                        MondayTotalOffDutyMealTimeDTWH += wholespan
                                    End If
                                Case DayOfWeek.Tuesday
                                    TuesdayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Wednesday
                                    WednesdayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Thursday
                                    ThursdayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Friday
                                    FridayTotalOffDutyMealTimeDTWH += wholespan
                                Case DayOfWeek.Saturday
                                    SaturdayTotalOffDutyMealTimeDTWH += wholespan
                            End Select
                        End If
                        #End Region

                    End If
                    #End Region
                End If               
            Next

            MondayTotalOffDutyMealTimeWH = MondayTotalOffDutyMealTimeRTWH + MondayTotalOffDutyMealTimeOTWH + MondayTotalOffDutyMealTimeDTWH
            TuesdayTotalOffDutyMealTimeWH = TuesdayTotalOffDutyMealTimeRTWH + TuesdayTotalOffDutyMealTimeOTWH + TuesdayTotalOffDutyMealTimeDTWH
            WednesdayTotalOffDutyMealTimeWH = WednesdayTotalOffDutyMealTimeRTWH + WednesdayTotalOffDutyMealTimeOTWH + WednesdayTotalOffDutyMealTimeDTWH
            ThursdayTotalOffDutyMealTimeWH = ThursdayTotalOffDutyMealTimeRTWH + ThursdayTotalOffDutyMealTimeOTWH + ThursdayTotalOffDutyMealTimeDTWH
            FridayTotalOffDutyMealTimeWH = FridayTotalOffDutyMealTimeRTWH + FridayTotalOffDutyMealTimeOTWH + FridayTotalOffDutyMealTimeDTWH
            SaturdayTotalOffDutyMealTimeWH = SaturdayTotalOffDutyMealTimeRTWH + SaturdayTotalOffDutyMealTimeOTWH + SaturdayTotalOffDutyMealTimeDTWH
            SundayTotalOffDutyMealTimeWH = SundayTotalOffDutyMealTimeRTWH + SundayTotalOffDutyMealTimeOTWH + SundayTotalOffDutyMealTimeDTWH

            WeeklyTotalOffDutyMealTimeWH = MondayTotalOffDutyMealTimeWH + TuesdayTotalOffDutyMealTimeWH + WednesdayTotalOffDutyMealTimeWH + ThursdayTotalOffDutyMealTimeWH + FridayTotalOffDutyMealTimeWH + SaturdayTotalOffDutyMealTimeWH + SundayTotalOffDutyMealTimeWH
            
            WeeklyTotalHoursWorkedWH = MondayTotalHoursWorkedWH + TuesdayTotalHoursWorkedWH + WednesdayTotalHoursWorkedWH + ThursdayTotalHoursWorkedWH + FridayTotalHoursWorkedWH + SaturdayTotalHoursWorkedWH + SundayTotalHoursWorkedWH

            Dim WeeklyTotalHoursMinusMeals = WeeklyTotalHoursWorkedWH - WeeklyTotalOffDutyMealTimeWH

            Dim halfhour = 10000000L * 60 * 60 * 0.5
            Dim onehour = 10000000L * 60 * 60 * 1
            Dim oneandhalfhour = 10000000L * 60 * 60 * 1.5

            
            Dim fourhours = 10000000L * 60 * 60 * 4
            Dim eighthours = 10000000L * 60 * 60 * 8
            Dim twelvehours = 10000000L * 60 * 60 * 12
            Dim fourtyhours = 10000000L * 60 * 60 * 40
            Dim WeeklyTotalRegularTimeWH = 0L
            Dim WeeklyTotalOverTimeWH = 0L
            Dim WeeklyTotalDoubleTimeWH = 0L



            Dim dt1 As New DataTable()
            Dim sqlCmd1 As New SqlCommand("SELECT ms_id, basepayrate, company FROM employee_tbl WHERE employee_id = '" + employee.ItemArray(0).ToString() + "'", conn)
            Dim sqlDa1 As New SqlDataAdapter(sqlCmd1)
            conn.close()
            conn.open()
            sqlDa1.Fill(dt1)
            conn.close()
            Dim worker_id As Int32 = dt1.Rows(0).ItemArray(0).ToString()
            Dim rate As Double = dt1.Rows(0).ItemArray(1).ToString()
            Dim company As String = dt1.Rows(0).ItemArray(2).ToString()
        
            'Add PTO to gridview
            'Add Shift Pay for this employee
            Dim RowValues As Object() = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            
            '' Pay Components
            'RT Hours, OT Hours, DT Hours, ST Hours, Gross Additions, GrossDeductions, 
            'RT Net Pay Reimburse, OT Net Pay Reimburse, Other Reimbursements, Net Deductions, 
            'Uniform, RT Hours - WH, OT Hours - WH, DT Hours - WH, ST - WHOL Hrs

            'Add RT
            RowValues = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If WeeklyTotalHoursMinusMeals > 0 Then
                RowValues(0) = "16057793"
                RowValues(1) = worker_id
                RowValues(4) = "PTO"
                RowValues(5) = rate 'rate
                RowValues(7) = TimeSpan.FromTicks(WeeklyTotalHoursMinusMeals * 48/2080).TotalHours.ToString("N6") 'hours
                RowValues(10) = "" 'amount
                dtable.Rows.Add(RowValues)
                dtable.AcceptChanges()
            End If
  
        Next    

        'now bind datatable to gridview... 
        PTOGridView.DataSource = dtable
        PTOGridView.DataBind()

        Response.Clear()
        Response.Buffer = True
        Response.ClearContent()
        Response.ClearHeaders()
        Response.Charset = ""
        Dim FileName As String = "PTO-" + DateTime.Now + ".xls"
        Dim strwritter = New StringWriter()
        Dim htmltextwrtter = New HtmlTextWriter(strwritter)
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        Response.ContentType = "application/vnd.ms-excel"
        Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName)
        PTOGridView.GridLines = GridLines.Both
        PTOGridView.HeaderStyle.Font.Bold = True
        PTOGridView.RenderBeginTag(htmltextwrtter)
        If Not IsNothing(PTOGridView.HeaderRow) Then
            PTOGridView.HeaderRow.RenderControl(htmltextwrtter)
        End If
        For Each row in PTOGridView.Rows
            row.RenderControl(htmltextwrtter)
        Next
        If Not IsNothing(PTOGridView.HeaderRow) Then
            PTOGridView.FooterRow.RenderControl(htmltextwrtter)
        End If
        PTOGridView.RenderEndTag(htmltextwrtter)
        Response.Write(strwritter.ToString())
        Response.End()


    End Sub
    Protected Sub CreateMattsFlexFile(sender As Object, e As EventArgs)
        ExportGridToExcel("Matt's Staffing")
    End Sub
    Protected Sub CreateExcaliburFlexFile(sender As Object, e As EventArgs)
        ExportGridToExcel("Excalibur")
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
