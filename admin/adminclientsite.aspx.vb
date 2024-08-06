Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Xml

Public Class adminclientsite
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If


        '' Populate the DropDownList with Clients
        If Not IsPostBack Then

            Dim validate As String = "SELECT client_id, clientname FROM client_tbl "
            Dim cmd = New SqlCommand(validate, conn)

            Dim myTable As New DataTable
            Dim adapter As New SqlDataAdapter(cmd)
            adapter.Fill(myTable)

            For Each Row As DataRow In myTable.Rows
                Dim newListItem As ListItem
                newListItem = New ListItem(Row.Item(1), Row.Item(0))
                client_ddl.Items.Add(newListItem)
            Next

            If client_ddl.Items.Count > 0 Then tb_client_id.Text = client_ddl.Text
            If client_ddl.Items.Count > 0 Then clientnameTB.Text = client_ddl.SelectedItem.Text

        End If

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        'Log_Change(Session(user), Log Out, Logged Out, )

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub

    Protected Sub client_ddl_SelectedIndexChanged(sender As Object, e As EventArgs)
        tb_client_id.Text = client_ddl.Text
        clientnameTB.Text = client_ddl.SelectedItem.Text
    End Sub
    Protected Sub ClientSite_OnRowUpdating(sender As Object, e As GridViewUpdateEventArgs)

        #Region "NewValues cannot be blank"
        If (e.NewValues("name") = Nothing) Then
            Response.Write("<script> alert(""Site Name cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("company") = Nothing) Then
            Response.Write("<script> alert(""Company cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("status") = Nothing) Then
            Response.Write("<script> alert(""Status cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("payrate") = Nothing) Then
            Response.Write("<script> alert(""Pay Rate cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("manager_email") = Nothing) Then
            Response.Write("<script> alert(""Manager Email cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("cc_emails_semicolon_separated") = Nothing) Then
            Response.Write("<script> alert(""CC Emails cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("onduty_mealperiods") = Nothing) Then
            Response.Write("<script> alert(""On Duty Meals cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("timesheetdata_oninvoice") = Nothing) Then
            Response.Write("<script> alert(""Timesheet On Invoice cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("supervisor_sheets") = Nothing) Then
            Response.Write("<script> alert(""Supervisor Sheets cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("streetaddress") = Nothing) Then
            e.NewValues("streetaddress") = " "
            'Response.Write("<script> alert(""Street Address cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        If (e.NewValues("city") = Nothing) Then
            e.NewValues("city") = " "
            'Response.Write("<script> alert(""City cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        If (e.NewValues("zip") = Nothing) Then
            e.NewValues("zip") = " "
            'Response.Write("<script> alert(""Zip Code cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        If (e.NewValues("notes") = Nothing) Then
            e.NewValues("notes") = " "
            'Response.Write("<script> alert(""Notes cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        #End Region

        #Region "Input Validation"
        If (e.NewValues("name").ToString.Length > 49) Then
            Response.Write("<script> alert(""Site Name must be less than 50 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("status").ToString <> "Active" And e.NewValues("status").ToString <> "Inactive") Then
            Response.Write("<script> alert(""Status must be 'Active' or 'Inactive'"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("company").ToString <> "Excalibur" and e.NewValues("company").ToString <> "Matt's Staffing") Then
            Response.Write("<script> alert(""Company must be 'Excalibur' or 'Matt's Staffing'"")</script>")
            e.cancel = true
            return
        End If
        Try
            dim rate As Double = Double.Parse(e.NewValues("payrate"))
            If (rate > 1000.00 or rate < 5.00 ) Then
                Response.Write("<script> alert(""Pay Rate must be less then $1000/hr and greater than $5.00/hr"")</script>")
                e.cancel = true
                return
            End If
        Catch ex As Exception
            Response.Write("<script> alert(""Base Pay Rate must be in $$.$$ format"")</script>")
            e.cancel = true
            return
        End Try
        If (e.NewValues("manager_email").ToString.Length > 999) Then
            Response.Write("<script> alert(""Manager Email must be less than 1000 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("cc_emails_semicolon_separated").ToString.Length > 1999) Then
            Response.Write("<script> alert(""CC Emails must be less than 2000 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("supervisor_sheets").ToString <> "True" And e.NewValues("supervisor_sheets").ToString <> "False") Then
            Response.Write("<script> alert(""Supervisor Sheets must be 'True' or 'False'"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("onduty_mealperiods").ToString <> "True" And e.NewValues("onduty_mealperiods").ToString <> "False") Then
            Response.Write("<script> alert(""On Duty Meals must be 'True' or 'False'"")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("timesheetdata_oninvoice").ToString <> "True" And e.NewValues("timesheetdata_oninvoice").ToString <> "False") Then
            Response.Write("<script> alert(""TimeSheet on Invoices must be 'True' or 'False'"")</script>")
            e.cancel = true
            return
        End If
        'If (Regex.IsMatch("^([0-1]\d|2[0-3]):([0-5]\d)(:([0-5]\d))?$", e.NewValues("startofweektime").ToString)) Then
        '    Response.Write("<script> alert(""Start of Week Time must be in 00:00:00 format"")</script>")
        '    e.cancel = true
        'End If
        'Dim s as string = e.NewValues("startofweekday").ToString
        'If (s<>"Monday" And s<>"Tuesday" And s<>"Wednesday" And s<>"Thursday" And s<>"Friday" And s<>"Saturday" And s<>"Sunday") Then
        '    Response.Write("<script> alert(""Start of Week Day must be Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, or Sunday."")</script>")
        '    e.cancel = true
        'End If
        If (e.NewValues("streetaddress") <> Nothing) Then
            If (e.NewValues("streetaddress").ToString.Length > 99) Then
                Response.Write("<script> alert(""Street Address must be less than 100 characters."")</script>")
                e.cancel = true
                return
            End If
        End If
        If (e.NewValues("city") <> Nothing) Then
            If (e.NewValues("city").ToString.Length > 49) Then
                Response.Write("<script> alert(""City must be less than 50 characters."")</script>")
                e.cancel = true
                return
            End If
        End If
        If (e.NewValues("zip") <> Nothing) Then
            If (e.NewValues("zip").ToString.Length > 11) Then
                Response.Write("<script> alert(""Zip must be less than 12 characters."")</script>")
                e.cancel = true
                return
            End If
        End If
        #End Region

        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("name") = Nothing) Then e.OldValues("name") = " "
        If (e.OldValues("status") = Nothing) Then e.OldValues("status") = " "
        If (e.OldValues("payrate") = Nothing) Then e.OldValues("payrate") = " "
        If (e.OldValues("company") = Nothing) Then e.OldValues("company") = " "
        If (e.OldValues("manager_email") = Nothing) Then e.OldValues("manager_email") = " " 
        If (e.OldValues("cc_emails_semicolon_separated") = Nothing) Then e.OldValues("cc_emails_semicolon_separated") = " "
        If (e.OldValues("onduty_mealperiods") = Nothing) Then e.OldValues("onduty_mealperiods") = " "
        If (e.OldValues("timesheetdata_oninvoice") = Nothing) Then e.OldValues("timesheetdata_oninvoice") = " "
        If (e.OldValues("supervisor_sheets") = Nothing) Then e.OldValues("supervisor_sheets") = " "
        If (e.OldValues("streetaddress") = Nothing) Then e.OldValues("streetaddress") = " "
        If (e.OldValues("city") = Nothing) Then e.OldValues("city") = " "
        If (e.OldValues("zip") = Nothing) Then e.OldValues("zip") = " "
        If (e.OldValues("notes") = Nothing) Then e.OldValues("notes") = " "

        If (e.NewValues("name") = Nothing) Then e.NewValues("name") = " "
        If (e.NewValues("status") = Nothing) Then e.NewValues("status") = " "
        If (e.NewValues("payrate") = Nothing) Then e.NewValues("payrate") = " "
        If (e.NewValues("company") = Nothing) Then e.NewValues("company") = " "
        If (e.NewValues("manager_email") = Nothing) Then e.NewValues("manager_email") = " "
        If (e.NewValues("cc_emails_semicolon_separated") = Nothing) Then e.NewValues("cc_emails_semicolon_separated") = " "
        If (e.NewValues("onduty_mealperiods") = Nothing) Then e.NewValues("onduty_mealperiods") = " "
        If (e.NewValues("timesheetdata_oninvoice") = Nothing) Then e.NewValues("timesheetdata_oninvoice") = " "
        If (e.NewValues("supervisor_sheets") = Nothing) Then e.NewValues("supervisor_sheets") = " "
        If (e.NewValues("streetaddress") = Nothing Or e.NewValues("streetaddress") = "") Then e.NewValues("streetaddress") = " "
        If (e.NewValues("city") = Nothing Or e.NewValues("city") = "") Then e.NewValues("city") = " "
        If (e.NewValues("zip") = Nothing Or e.NewValues("zip") = "") Then e.NewValues("zip") = " "
        If (e.NewValues("notes") = Nothing Or e.NewValues("notes") = "") Then e.NewValues("notes") = " "
        #End Region

        '' Add Change To Log
        Dim Changes_Made As string = ""
        if (e.OldValues("name") <> e.NewValues("name")) Then Changes_Made += Session("user") + " changed Name of "+e.OldValues("name").ToString()+" from '"+e.OldValues("name").ToString()+"' to '"+e.NewValues("name").ToString()+"' <br />"
        if (e.OldValues("company") <> e.NewValues("company")) Then Changes_Made += Session("user") + " changed Company of "+e.OldValues("name").ToString()+" from '"+e.OldValues("company").ToString()+"' to '"+e.NewValues("company").ToString()+"' <br />"
        if (e.OldValues("status") <> e.NewValues("status")) Then Changes_Made += Session("user") + " changed Status of "+e.OldValues("name").ToString()+" from '"+e.OldValues("status").ToString()+"' to '"+e.NewValues("status").ToString()+"' <br />"
        if (e.OldValues("payrate") <> e.NewValues("payrate")) Then Changes_Made += Session("user") + " changed Billable Rate of "+e.OldValues("name").ToString()+" from '"+e.OldValues("payrate").ToString()+"' to '"+e.NewValues("payrate").ToString()+"' <br />"
        if (e.OldValues("manager_email") <> e.NewValues("manager_email")) Then Changes_Made += Session("user") + " changed Manager Email of "+e.OldValues("name").ToString()+" from '"+e.OldValues("manager_email").ToString()+"' to '"+e.NewValues("manager_email").ToString()+"' <br />"
        if (e.OldValues("cc_emails_semicolon_separated") <> e.NewValues("cc_emails_semicolon_separated")) Then Changes_Made += Session("user") + " changed CC Emails of "+e.OldValues("name").ToString()+" from '"+e.OldValues("cc_emails_semicolon_separated").ToString()+"' to '"+e.NewValues("cc_emails_semicolon_separated").ToString()+"' <br />"
        if (e.OldValues("onduty_mealperiods") <> e.NewValues("onduty_mealperiods")) Then Changes_Made += Session("user") + " changed onduty_mealperiods of "+e.OldValues("name").ToString()+" from '"+e.OldValues("onduty_mealperiods").ToString()+"' to '"+e.NewValues("onduty_mealperiods").ToString()+"' <br />"
        if (e.OldValues("timesheetdata_oninvoice") <> e.NewValues("timesheetdata_oninvoice")) Then Changes_Made += Session("user") + " changed timesheetdata_oninvoice? of "+e.OldValues("name").ToString()+" from '"+e.OldValues("timesheetdata_oninvoice").ToString()+"' to '"+e.NewValues("timesheetdata_oninvoice").ToString()+"' <br />"
        if (e.OldValues("supervisor_sheets") <> e.NewValues("supervisor_sheets")) Then Changes_Made += Session("user") + " changed supervisor_sheets? of "+e.OldValues("name").ToString()+" from '"+e.OldValues("supervisor_sheets").ToString()+"' to '"+e.NewValues("supervisor_sheets").ToString()+"' <br />"
        if (e.OldValues("streetaddress") <> e.NewValues("streetaddress")) Then Changes_Made += Session("user") + " changed Street Address of "+e.OldValues("name").ToString()+" from '"+e.OldValues("streetaddress").ToString()+"' to '"+e.NewValues("streetaddress").ToString()+"' <br />"
        if (e.OldValues("city") <> e.NewValues("city")) Then Changes_Made += Session("user") + " changed City of "+e.OldValues("name").ToString()+" from '"+e.OldValues("city").ToString()+"' to '"+e.NewValues("city").ToString()+"' <br />"
        if (e.OldValues("zip") <> e.NewValues("zip")) Then Changes_Made += Session("user") + " changed Zip Code of "+e.OldValues("name").ToString()+" from '"+e.OldValues("zip").ToString()+"' to '"+e.NewValues("zip").ToString()+"' <br />"
        if (e.OldValues("notes") <> e.NewValues("notes")) Then Changes_Made += Session("user") + " changed Notes of "+e.OldValues("name").ToString()+" from '"+e.OldValues("notes").ToString()+"' to '"+e.NewValues("notes").ToString()+"' <br />"

        If (Changes_Made <> "") Then Log_Change(Session("user"), "ClientSite", Changes_Made, "Update")
    End Sub
    Protected Sub ClientSite_OnRowDeleting(sender As Object, e As GridViewDeleteEventArgs)
        Dim Changes_Made As string = Session("user") + " deleted row with client_id#"+e.Keys.Values(0).ToString()+" which had values: name="+e.Values("name")+", status="+e.Values("status")+", payrate='"+e.Values("payrate")+"', company="+e.Values("company")+", manager_email="+e.Values("manager_email")+", cc_emails="+e.Values("cc_emails_semicolon_separated")+", onduty_mealperiods="+e.Values("onduty_mealperiods")+", timesheetdata_oninvoice="+e.Values("timesheetdata_oninvoice")+
            +", supervisor_sheets="+e.Values("supervisor_sheets")+", latitude="+e.Values("latitude")+", longitude="+e.Values("longitude")+", notes="+e.Values("notes")+"."
        Log_Change(Session("user"), "ClientSite", Changes_Made, "Delete")
    End Sub
    Protected Sub createSite(sender As Object, e As EventArgs)
        Dim clientnames As String = clientnameTB.Text
        Dim client_ids As String = tb_client_id.Text
        Dim names As String = nameTB.Text
        Dim payrates As String = payrateTB.Text
        Dim statuss As String = "Active"
        Dim streetaddresss As String = streetaddressTB.Text
        Dim streetaddress2s As String = streetaddress2TB.Text
        Dim citys As String = cityTB.Text
        Dim zips As String = zipTB.Text
        Dim states As String = stateTB.Text

        '' Check for name already exists
        Dim validate As String = "SELECT name FROM client_site_tbl WHERE name ='" & names & "'"
        Dim cmd = New SqlCommand(validate, conn)

        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""Site Name already exists"")</script>")
        Else

            Dim addquery As String = "INSERT INTO client_site_tbl (client_id, name, status, payrate, company, manager_email, CC_emails_semicolon_separated, onduty_mealperiods, timesheetdata_oninvoice, supervisor_sheets, streetaddress, streetaddress2, city, zip, state) VALUES (@client_id, @name, @status, @payrate, @company, @manager_email, @cc_emails_semicolon_separated, @onduty_mealperiods, @timesheetdata_oninvoice, @supervisor_sheets, @streetaddress, @streetaddress2, @city, @zip, @state)"
            Dim com = New SqlCommand(addquery, conn)

            com.Parameters.AddWithValue("@client_id", client_ids)
            com.Parameters.AddWithValue("@name", names)
            com.Parameters.AddWithValue("@status", statuss)
            com.Parameters.AddWithValue("@payrate", payrates)
            com.Parameters.AddWithValue("@company", companyDDL.Text)
            com.Parameters.AddWithValue("@manager_email", manager_emailTB.Text)
            com.Parameters.AddWithValue("@cc_emails_semicolon_separated", cc_emails_semicolon_separatedTB.Text)
            com.Parameters.AddWithValue("@onduty_mealperiods", onduty_mealperiodsDDL.Text)
            com.Parameters.AddWithValue("@timesheetdata_oninvoice", timesheetdata_oninvoiceDDL.Text)
            com.Parameters.AddWithValue("@supervisor_sheets", supervisor_sheetsDDL.Text)
            com.Parameters.AddWithValue("@streetaddress", streetaddresss)
            com.Parameters.AddWithValue("@streetaddress2", streetaddress2s)
            com.Parameters.AddWithValue("@city", citys)
            com.Parameters.AddWithValue("@zip", zips)
            com.Parameters.AddWithValue("@state", states)

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
                    Log_Change(Session("user"), "Client Site", Session("user") + " created a new Client Site: Client_ID='"+client_ids+"', SiteName='"+names+"'", "Insert")
                    Response.Write("<script> alert(""New Client Site Created successfully"")</script>")
                Case 0
                    Response.Write("<script> alert(""Issue connecting to server or executing the Create Client query!"")</script>")
            End Select
        End If

        ''InsertCommand=INSERT INTO attendancelogs (employee_id, username, action, datetime, date, time) VALUES (@employee_id, @username, @action, @datetime, @date, @time)
        nameTB.Text = ""
        payrateTB.Text = ""
        companyDDL.Text = ""
        manager_emailTB.Text = ""
        cc_emails_semicolon_separatedTB.Text = ""
        onduty_mealperiodsDDL.Text = ""
        timesheetdata_oninvoiceDDL.Text = ""
        supervisor_sheetsDDL.Text = ""
        streetaddressTB.Text = ""
        streetaddress2TB.Text = ""
        cityTB.Text = ""
        zipTB.Text = ""
        stateTB.Text = ""

        '' Refresh
        GridView1.DataBind()

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
