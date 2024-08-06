Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebControl
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO
Imports System
Imports System.Drawing
Imports DayPilot.Web.Ui.Events
Imports DayPilot.Web.Ui
Imports System.Data.SqlTypes
Public Class admincalendar
    Inherits System.Web.UI.Page
    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)

    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Enum Calendar_View
        all
        open
        employee
        clientsite  
    End Enum
    public calendar_view_state As Calendar_View

    Enum DurationBarColor
        Crimson 
        MediumVioletRed 
        OrangeRed
        Indigo
        DarkGreen
        Blue
        DeepSkyBlue
        SaddleBrown
        Black
    End Enum
    Enum BackgroundColor
        LightSalmon
        LightPink
        Khaki
        Thistle
        PaleGreen
        PowderBlue
        LightCyan
        Wheat
        Gainsboro
    End Enum
    public site_color As DurationBarColor
    public site_back_color As BackgroundColor

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If

        '' View State
        If (Me.ViewState("calendarViewState") IsNot Nothing) Then
            calendar_view_state = CType(Me.ViewState("calendarViewState"), Calendar_View)
        End If


        If Not IsPostBack Then

            #Region "Fill the Employee And ClientSite DDLs"
            '' EmployeeDDL
            myCmd = conn.CreateCommand
            myCmd.CommandText = "SELECT username, employee_id FROM employee_tbl ORDER BY employee_tbl.username ASC"
            conn.Open()
            myReader = myCmd.ExecuteReader()
            EmployeeDDL.Items.Clear()
            EmployeeDDL.Items.Add(" ")
            EmployeeDDL.Items.FindByText(" ").Value = 0
            Do While myReader.Read()
                results = myReader.GetString(0)
                EmployeeDDL.Items.Add(results)
                EmployeeDDL.Items.FindByText(results).Value = myReader.GetInt32(1)
            Loop
            myReader.Close()
            conn.Close()
            
            '' ClientSiteDDL
            myCmd = conn.CreateCommand
            myCmd.CommandText = "SELECT name, client_site_id FROM client_site_tbl ORDER BY client_site_tbl.name ASC"
            conn.Open()
            myReader = myCmd.ExecuteReader()
            ClientSiteDDL.Items.Clear()
            ClientSiteDDL.Items.Add(" ")
            ClientSiteDDL.Items.FindByText(" ").Value = 0
            Do While myReader.Read()
                results = myReader.GetString(0)
                ClientSiteDDL.Items.Add(results)
                ClientSiteDDL.Items.FindByText(results).Value = myReader.GetInt32(1)
            Loop
            myReader.Close()
            conn.Close()
            #End Region

            '' View All Shifts for Today
            calendar_view_state = Calendar_View.all
            DayPilotCalendar.CellHeight = 15
            DayPilotCalendar.StartDate = DateTime.Now 'firstDayOfWeek(DateTime.Now, DayOfWeek.Sunday)
            StartDateTB.Text = DateTime.Now.Date.ToString("yyyy-MM-dd")
            DayPilotCalendar.Days = 1
        End If

        If (calendar_view_state = Calendar_View.clientsite) Then
            CreateShift_Button.Visible = true
            CreateShift_Button.Enabled = true
        Else 
            CreateShift_Button.Visible = false
            CreateShift_Button.Enabled = false
        End If

        DataBind()
        Load_Shift_Instances()
        DataBind()
 
    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        'Log_Change(Session(user), Log Out, Logged Out, )

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub



    Protected Sub AllButton_OnClick(sender As Object, e As EventArgs)
        calendar_view_state = Calendar_View.all
        Me.ViewState.Add("calendarViewState", calendar_view_state)
        Load_Shift_Instances()
    End Sub
    Protected Sub OpenButton_OnClick(sender As Object, e As EventArgs)
        calendar_view_state = Calendar_View.open
        Me.ViewState.Add("calendarViewState", calendar_view_state)
        Load_Shift_Instances()
    End Sub
    Protected Sub ClientSiteDDL_SelectedIndexChanged(sender As Object, e As EventArgs)
        calendar_view_state = Calendar_View.clientsite
        Me.ViewState.Add("calendarViewState", calendar_view_state) 
        Load_Shift_Instances()
    End Sub
    Protected Sub EmployeeDDL_SelectedIndexChanged(sender As Object, e As EventArgs)
        calendar_view_state = Calendar_View.employee
        Me.ViewState.Add("calendarViewState", calendar_view_state)  
        Load_Shift_Instances()
    End Sub



    Protected Sub Load_Shift_Instances()
        If (calendar_view_state = Calendar_View.clientsite) Then
            CreateShift_Button.Visible = true
            CreateShift_Button.Enabled = true
        Else 
            CreateShift_Button.Visible = false
            CreateShift_Button.Enabled = false
        End If

        Select calendar_view_state
            case Calendar_View.all:
                DisplayingLabel.Text = "All Shifts"
                DayPilotCalendar.Days = 1
                LoadShiftInstances_All()
                return
            case Calendar_View.open:
                DisplayingLabel.Text = "Open Shifts"
                DayPilotCalendar.Days = DaysDDL.SelectedValue
                LoadShiftInstances_UnFilled()
                return
            case Calendar_View.employee:
                DisplayingLabel.Text = "" + EmployeeDDL.SelectedItem.Text
                DayPilotCalendar.Days = DaysDDL.SelectedValue
                LoadShiftInstances_Employee()
                return
            case Calendar_View.clientsite:
                DisplayingLabel.Text = "" + ClientSiteDDL.SelectedItem.Text
                DayPilotCalendar.Days = DaysDDL.SelectedValue
                LoadShiftInstances_ClientSite()
                return
            case Else:
                return
        End Select
    End Sub
    Protected Sub LoadShiftInstances_ClientSite()
   
        Dim queryString1 As string = "SELECT startdatetime, enddatetime, shift_name, client_site_shift_instance_id, employee_id, client_site_id FROM client_site_shift_instance_tbl WHERE client_site_id = '" + ClientSiteDDL.SelectedValue.ToString() + "' AND startdatetime >= '" + DayPilotCalendar.StartDate.AddDays(-1).ToString() + "' AND enddatetime <= '" + DayPilotCalendar.EndDate.AddDays(2).ToString() + "'"
        Dim command As SqlCommand = new SqlCommand(queryString1, conn)
        Dim datatable = new DataTable()
        try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            datatable.Load(reader)
            reader.Close()
            Finally 
            conn.Close() 
        End Try
        Dim dt = new DataTable()
        dt.Columns.Add("start")
        dt.Columns.Add("end")
        dt.Columns.Add("name")
        dt.Columns.Add("id")
        dt.Columns.Add("sitename")
        dt.Columns.Add("employeename")
        dt.Columns.Add("color")
        Dim dr As DataRow
        For Each r in datatable.Rows
            ' Get the Client_Site Info
            Dim queryString2 As string = "SELECT name from client_site_tbl WHERE client_site_id = '"+r(5).ToString()+"'"
            Dim command2 As SqlCommand = new SqlCommand(queryString2, conn)
            Dim siteName As string
            Dim dt2 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command2.ExecuteReader()
                dt2.Load(reader)
                reader.Close()
                Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(5)) < 1) Then 
                siteName = "<< !! NONE >> ^#$&^#^"
            Else
                siteName = dt2.Rows(0).ItemArray(0).ToString()
            End If

            ' Get the Employee_Info
            Dim queryString3 As string = "SELECT firstname, lastname from employee_tbl WHERE employee_id = '"+r(4).ToString()+"'"
            Dim command3 As SqlCommand = new SqlCommand(queryString3, conn)
            Dim employeeName As string
            Dim dt3 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command3.ExecuteReader()
                dt3.Load(reader)
                reader.Close()
                Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(4)) < 1) Then 
                employeeName = "<<  OPEN  >>"
            Else
                employeeName = dt3.Rows(0).ItemArray(0).ToString() + " " + dt3.Rows(0).ItemArray(1).ToString()
            End If

            dr = dt.NewRow()
            dr("id") = r(3)
            dr("start") = r(0)
            dr("end") = r(1)
            dr("name") = siteName + " - " + employeeName
            dr("sitename") = siteName
            dr("employeename") = employeeName
            site_color = (Int32.Parse(r(5)) Mod 8)
            dr("color") = site_color.ToString()
            dt.Rows.Add(dr)
        Next

        ' _items.Clear()
        dt.PrimaryKey = new DataColumn() { dt.Columns("id") }
        DayPilotCalendar.DataSource = dt
        DataBind()

    End Sub
    Protected Sub LoadShiftInstances_Employee()
    
        Dim queryString1 As String = "SELECT startdatetime, enddatetime, shift_name, client_site_shift_instance_id, employee_id, client_site_id FROM client_site_shift_instance_tbl WHERE employee_id = '" + EmployeeDDL.SelectedValue.ToString() + "' AND startdatetime >= '" + DayPilotCalendar.StartDate.AddDays(-1).ToString() + "' AND enddatetime <= '" + DayPilotCalendar.EndDate.AddDays(2).ToString() + "'"
        Dim command As SqlCommand = new SqlCommand(queryString1, conn)
        Dim datatable = new DataTable()
        try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            datatable.Load(reader)
            reader.Close()
            Finally 
            conn.Close() 
        End Try
        Dim dt = new DataTable()
        dt.Columns.Add("start")
        dt.Columns.Add("end")
        dt.Columns.Add("name")
        dt.Columns.Add("id")
        dt.Columns.Add("sitename")
        dt.Columns.Add("employeename")
        dt.Columns.Add("color")
        Dim dr As DataRow
        For Each r in datatable.Rows
            ' Get the Client_Site Info
            Dim queryString2 As string = "SELECT name from client_site_tbl WHERE client_site_id = '"+r(5).ToString()+"'"
            Dim command2 As SqlCommand = new SqlCommand(queryString2, conn)
            Dim siteName As string
            Dim dt2 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command2.ExecuteReader()
                dt2.Load(reader)
                reader.Close()
                Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(5)) < 1) Then 
                siteName = "<< !! NONE >> ^#$&^#^"
            Else
                siteName = dt2.Rows(0).ItemArray(0).ToString()
            End If

            ' Get the Employee_Info
            Dim queryString3 As string = "SELECT firstname, lastname from employee_tbl WHERE employee_id = '"+r(4).ToString()+"'"
            Dim command3 As SqlCommand = new SqlCommand(queryString3, conn)
            Dim employeeName As string
            Dim dt3 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command3.ExecuteReader()
                dt3.Load(reader)
                reader.Close()
                Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(4)) < 1) Then 
                employeeName = "<<  OPEN  >>"
            Else
                employeeName = dt3.Rows(0).ItemArray(0).ToString() + " " + dt3.Rows(0).ItemArray(1).ToString()
            End If

            dr = dt.NewRow()
            dr("id") = r(3)
            dr("start") = r(0)
            dr("end") = r(1)
            dr("name") = siteName + " - " + employeeName
            dr("sitename") = siteName
            dr("employeename") = employeeName
            site_color = (Int32.Parse(r(5)) Mod 12)
            dr("color") = site_color.ToString()
            dt.Rows.Add(dr)
        Next


        ' _items.Clear()
        dt.PrimaryKey = new DataColumn() { dt.Columns("id") }
        DayPilotCalendar.DataSource = dt
        DataBind()
    End Sub
    Protected Sub LoadShiftInstances_UnFilled()
    
        Dim queryString1 As String = "SELECT startdatetime, enddatetime, shift_name, client_site_shift_instance_id, employee_id, client_site_id FROM client_site_shift_instance_tbl WHERE (employee_id IS NULL OR employee_id = '') AND (startdatetime >= '" + DayPilotCalendar.StartDate.AddDays(-1).ToString() + "' AND enddatetime <= '" + DayPilotCalendar.EndDate.AddDays(2).ToString() + "')"
        Dim command As SqlCommand = new SqlCommand(queryString1, conn)
        Dim datatable = new DataTable()
        try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            datatable.Load(reader)
            reader.Close()
            Finally 
            conn.Close() 
        End Try
        Dim dt = new DataTable()
        dt.Columns.Add("start")
        dt.Columns.Add("end")
        dt.Columns.Add("name")
        dt.Columns.Add("id")
        dt.Columns.Add("sitename")
        dt.Columns.Add("employeename")
        dt.Columns.Add("color")
        Dim dr As DataRow
        For Each r in datatable.Rows
            ' Get the Client_Site Info
            Dim queryString2 As string = "SELECT name from client_site_tbl WHERE client_site_id = '"+r(5).ToString()+"'"
            Dim command2 As SqlCommand = new SqlCommand(queryString2, conn)
            Dim siteName As string
            Dim dt2 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command2.ExecuteReader()
                dt2.Load(reader)
                reader.Close()
                Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(5)) < 1) Then 
                siteName = "<< !! NONE >> ^#$&^#^"
            Else
                siteName = dt2.Rows(0).ItemArray(0).ToString()
            End If

            ' Get the Employee_Info
            Dim queryString3 As string = "SELECT firstname, lastname from employee_tbl WHERE employee_id = '"+r(4).ToString()+"'"
            Dim command3 As SqlCommand = new SqlCommand(queryString3, conn)
            Dim employeeName As string
            Dim dt3 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command3.ExecuteReader()
                dt3.Load(reader)
                reader.Close()
                Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(4)) < 1) Then 
                employeeName = "<<  OPEN  >>"
            Else
                employeeName = dt3.Rows(0).ItemArray(0).ToString() + " " + dt3.Rows(0).ItemArray(1).ToString()
            End If

            dr = dt.NewRow()
            dr("id") = r(3)
            dr("start") = r(0)
            dr("end") = r(1)
            dr("name") = siteName + " - " + employeeName
            dr("sitename") = siteName
            dr("employeename") = employeeName
            site_color = (Int32.Parse(r(5)) Mod 12)
            dr("color") = site_color.ToString()
            dt.Rows.Add(dr)
        Next

        ' _items.Clear()
        dt.PrimaryKey = new DataColumn() { dt.Columns("id") }
        DayPilotCalendar.DataSource = dt  
        DataBind()
    End Sub
    Protected Sub LoadShiftInstances_All()
    
        Dim queryString1 As string = "SELECT startdatetime, enddatetime, shift_name, client_site_shift_instance_id, employee_id, client_site_id FROM client_site_shift_instance_tbl WHERE (startdatetime >= '" + DayPilotCalendar.StartDate.AddDays(-1).ToString() + "' AND enddatetime <= '" + DayPilotCalendar.EndDate.AddDays(2).ToString() + "')"
        Dim command As SqlCommand = new SqlCommand(queryString1, conn)
        Dim datatable = new DataTable()
        try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            datatable.Load(reader)
            reader.Close()
            Finally 
                conn.Close() 
        End Try
        Dim dt = new DataTable()
        dt.Columns.Add("start")
        dt.Columns.Add("end")
        dt.Columns.Add("name")
        dt.Columns.Add("id")
        dt.Columns.Add("sitename")
        dt.Columns.Add("employeename")
        dt.Columns.Add("color")
        Dim dr As DataRow
        For Each r in datatable.Rows
            ' Get the Client_Site Info
            Dim queryString2 As string = "SELECT name from client_site_tbl WHERE client_site_id = '"+r(5).ToString()+"'"
            Dim command2 As SqlCommand = new SqlCommand(queryString2, conn)
            Dim siteName As string
            Dim dt2 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command2.ExecuteReader()
                dt2.Load(reader)
                reader.Close()
            Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(5)) < 1) Then 
                siteName = "<< !! NONE >> ^#$&^#^"
            Else
                siteName = dt2.Rows(0).ItemArray(0).ToString()
            End If

            ' Get the Employee_Info
            Dim queryString3 As string = "SELECT firstname, lastname from employee_tbl WHERE employee_id = '"+r(4).ToString()+"'"
            Dim command3 As SqlCommand = new SqlCommand(queryString3, conn)
            Dim employeeName As string
            Dim dt3 as DataTable = new DataTable()
            try 
                conn.Close()
                conn.Open()
                Dim reader As SqlDataReader = command3.ExecuteReader()
                dt3.Load(reader)
                reader.Close()
            Finally 
                conn.Close() 
            End Try
            If (Int32.Parse(r(4)) < 1) Then 
                employeeName = "<<  OPEN  >>"
            Else
                employeeName = dt3.Rows(0).ItemArray(0).ToString() + " " + dt3.Rows(0).ItemArray(1).ToString()
            End If

            dr = dt.NewRow()
            dr("id") = r(3)
            dr("start") = r(0)
            dr("end") = r(1)
            dr("name") = siteName + " - " + employeeName
            dr("sitename") = siteName
            dr("employeename") = employeeName
            site_color = (Int32.Parse(r(5)) Mod 12)
            dr("color") = site_color.ToString()
            dt.Rows.Add(dr)
        Next

        ' _items.Clear()
        dt.PrimaryKey = new DataColumn() { dt.Columns("id") }
        DayPilotCalendar.DataSource = dt
        DataBind()
    End Sub



    Protected Sub StartDateTB_OnTextChanged(sender As Object, e As EventArgs)
        DayPilotCalendar.StartDate = DateTime.Parse(StartDateTB.Text)
        Me.ViewState.Add("calendarViewState", calendar_view_state)  
        Load_Shift_Instances()
    End Sub
    Protected Sub DaysDDL_SelectedIndexChanged(sender As Object, e As EventArgs)
        DayPilotCalendar.Days = Convert.ToInt32(DaysDDL.SelectedValue) 
        Me.ViewState.Add("calendarViewState", calendar_view_state)
        Load_Shift_Instances()
    End Sub
    Protected Sub CellHeightDDL_SelectedIndexChanged(sender As Object, e As EventArgs)
        DayPilotCalendar.CellHeight = Convert.ToInt32(CellHeightDDL.SelectedValue)
        Me.ViewState.Add("calendarViewState", calendar_view_state)
        Load_Shift_Instances()
    End Sub


    Function FirstDayOfWeek(day As DateTime, weekStarts As DayOfWeek)
        Dim d As DateTime = day
        While d.DayOfWeek <> weekStarts
            d = d.AddDays(-1)
        End While
        Return d
    End Function

    Protected Sub DayPilotCalendar_BeforeEventRender(sender As Object, e As DayPilot.Web.Ui.Events.Calendar.BeforeEventRenderEventArgs)
        Try
            Dim color As String = e.DataItem("color")
            If Not String.IsNullOrEmpty(color) Then
                e.DurationBarColor = color
            End If
            Finally

        End Try
        
    End Sub
    Protected Sub DayPilotCalendar_OnEventMove(sender As Object, e As EventMoveEventArgs)
        '' Commented because we don't watch click and drag... its not fine enough and too sketchy
        'Dim dr As DataRow = sender.Items(sender.Items.IndexOf(e))
        'If dr.IsNull(0) <> true Then
        '    dr("start") = e.NewStart
        '    dr("end") = e.NewEnd
        '    sender.table.AcceptChanges()
        'End If

        'DayPilotCalendar.DataBind()
        'DayPilotCalendar.Update()
    End Sub
    Protected Sub DayPilotCalendar_OnEventResize(sender As Object, e As EventResizeEventArgs)
        '' Commented because we don't watch click and drag... its not fine enough and too sketchy
        'Dim dr As DataRow = sender.Items(sender.Items.IndexOf(e))
        'If dr.IsNull(0) <> true Then
        '   dr("start") = e.NewStart
        '   dr("end") = e.NewEnd
        '   sender.table.AcceptChanges()
        'End If

        'DayPilotCalendar.DataBind()
        'DayPilotCalendar.Update()
    End Sub
    Protected Sub DayPilotCalendar_OnTimeRangeSelected(sender As Object, e As TimeRangeSelectedEventArgs)
        '' Commented because we don't watch click and drag... its not fine enough and too sketchy
        'Dim dr As DataRow = sender.Items(sender.Items.IndexOf(e))
        'If dr.IsNull(0) <> true Then
        '   dr("start") = e.NewStart
        '   dr("end") = e.NewEnd
        '   sender.table.AcceptChanges()
        'End If

        'DayPilotCalendar.DataBind()
        'DayPilotCalendar.Update()

    End Sub

    Protected Sub CreateButton_OnClick(sender As Object, e As EventArgs)   
        Dim script As string = "$('#createmodal').modal('show');"
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp",script,true)


        ' Get the ClientSite_ID and ClientSite_Name
        CreateShift_ClientSite_ID.Text = ClientSiteDDL.SelectedValue.ToString()
        CreateShift_ClientSite_Name.Text = ClientSiteDDL.SelectedItem.Text
        CreateShift_SiteNameLabel.Text = "Client Site: " + ClientSiteDDL.SelectedItem.Text


        ' Populate the AssignedEmployee ComboBox
        Dim dataTable = new DataTable()
        Dim queryString as string = "SELECT username FROM employee_tbl WHERE status='Active' ORDER BY employee_tbl.username ASC"
        Dim command = new SqlCommand(queryString, conn)
        Try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            dataTable.Load(reader)
            reader.Close()
        finally 
            conn.Close()
        End Try

        CreateShift_EmployeeDDL.Items.Add("<--     OPEN     -->")
        For Each r in dataTable.Rows 
            CreateShift_EmployeeDDL.Items.Add(r.ItemArray(0))
        Next

    End Sub
    Protected Sub DayPilotCalendar_OnEventClick(sender As Object, e As EventClickEventArgs)
        Dim script As string = "$('#myshiftmodal').modal('show');"
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp",script,true)

        '' Load Modal with ShiftInstance clicked
        ShiftModalLabel.Text = "Client Site: " + e.Text.Split("-")(0)
        ShiftInstance_ID.Text = e.Id

        '' Load ShiftInstance div
        'Load this form with client_site_shift_instance data
        Dim queryString As string = "SELECT shift_name, shift_notes, startdatetime, enddatetime, payrate, onduty_mealperiods, employee_id FROM client_site_shift_instance_tbl WHERE client_site_shift_instance_id = '" + e.Id.ToString() + "'"
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

                Pre_SI_Name.Text = ShiftInstance_Name.Text 
                Pre_SI_Notes.Text = ShiftInstance_ShiftNotes.Text
                Pre_SI_startdatetime.Text = this_startdatetime
                Pre_SI_enddatetime.Text = this_enddatetime

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
                Pre_SI_username.Text = reader(0).ToString()
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

        '' Load ShiftTemplate div
        'Get the client_site_id, client_id and client_shift_instance_id
        queryString = "SELECT client_site_id, client_site_shift_id, client_id FROM client_site_shift_instance_tbl WHERE client_site_shift_instance_id = '" + e.Id + "'"
        Dim client_site_id As string = ""
        Dim client_site_shift_id As string = ""
        Dim client_id As string = ""
        command = new SqlCommand(queryString, conn)
        try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read()) 
                client_site_id = reader(0).ToString()
                client_site_shift_id = reader(1).ToString()
                client_id = reader(2).ToString()
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try
        Shift_ID.Text = client_site_shift_id
        ClientSite_ID.Text = client_site_id
        Client_ID_.Text = client_id

        ' Load this form with client_site_shift data
        queryString = "SELECT shift_name, shift_description, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods FROM client_site_shift_tbl WHERE client_site_shift_id = '" + client_site_shift_id.ToString() + "'"
        command = new SqlCommand(queryString, conn)
        Dim employee_id As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read())
                ShiftTemplate_Name.Text = reader(0).ToString()
                ShiftTemplate_ShiftDescription.Text = reader(1).ToString()
                employee_id = reader(2).ToString()
                ShiftTemplate_StartDate.Text = DateTime.Parse(reader(3).ToString()).ToString("yyyy-MM-dd")
                ShiftTemplate_EndDate.Text = DateTime.Parse(reader(4).ToString()).ToString("yyyy-MM-dd")
                ShiftTemplate_StartTime.Text = DateTime.Parse(reader(3).ToString()).ToString(“HH:mm”)
                ShiftTemplate_EndTime.Text = DateTime.Parse(reader(4).ToString()).ToString(“HH:mm”)
                ShiftTemplate_PayRate.Text = reader(5).ToString()
                ShiftTemplate_OnDutyMealPeriods.Checked = boolean.Parse(reader(6).ToString())

                Pre_ST_Name.Text = reader(0).ToString()
                Pre_ST_Description.Text = reader(1).ToString()
                Pre_ST_employee_id.Text = reader(2).ToString()
                Pre_ST_startdatetime.Text = reader(3).ToString()
                Pre_ST_enddatetime.Text = reader(4).ToString()
                Pre_ST_ondutymeals.Text = reader(6).ToString()

            End While
            reader.Close()
        Finally 
            conn.Close()
        End Try

        'If StartDate is past, then StartDate_Calendar.Enabled = false
        if (DateTime.Parse(ShiftTemplate_StartDate.Text)  < DateTime.Now) Then ShiftTemplate_StartDate.Enabled = false

        'EndDate cannot be set before current date
        'ShiftTemplate_EndDate. = DateTime.Now.Date.AddDays(1); ' cannot be implemented here

        'Load Day of Week ComboBoxes
        queryString = "SELECT weeklystartday, weeklyendday FROM client_site_shift_tbl WHERE client_site_shift_id = '" + client_site_shift_id + "'"
        command = new SqlCommand(queryString, conn)
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
                StartDay_DDL.SelectedValue = reader(0).ToString()
                EndDay_DDL.SelectedValue = reader(1).ToString()
                Pre_ST_startday.Text = reader(0).ToString()
                Pre_ST_endday.Text = reader(1).ToString()
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try

        'Load Assigned Employee ComboBoxes
        queryString = "SELECT username, cellphone FROM employee_tbl WHERE employee_id = '" + employee_id + "'"
        command = new SqlCommand(queryString, conn)
        username = ""
        cellphone = ""
        try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
                username = reader(0).ToString()
                cellphone = reader(1).ToString()
                Pre_ST_username.Text = reader(0).ToString()
            End While
            reader.Close()
        Finally 
            conn.Close()
        End Try

        'Populate the AssignedEmployee ComboBox
        dataTable = new DataTable()
        queryString = "SELECT username, cellphone FROM employee_tbl WHERE status='Active' OR username='" + username + "' ORDER BY employee_tbl.username ASC"
        command = new SqlCommand(queryString, conn)
        try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            dataTable.Load(reader)
            reader.Close()
        Finally 
            conn.Close()
        End Try

        ShiftTemplate_EmployeeDDL.Items.Add("<--     OPEN     -->")
        For Each r As DataRow in dataTable.Rows
            ShiftTemplate_EmployeeDDL.Items.Add(r.ItemArray(0) + ", " + r.ItemArray(1))
        Next
        If username = "" Then
            ShiftTemplate_EmployeeDDL.SelectedValue = "<--     OPEN     -->"
        Else 
            ShiftTemplate_EmployeeDDL.SelectedValue = ShiftTemplate_EmployeeDDL.Items.FindByText(username + ", " + cellphone).ToString()
        End If

    End Sub


    '' Modal
    Protected Sub CreateShift_Click(sender As Object, e As EventArgs)
        #Region "Validate Form Input"
        ' StartDay is MTWTFSS 
        if ((CreateShift_StartDay.Text <> "Monday") and (CreateShift_StartDay.Text <> "Tuesday") and (CreateShift_StartDay.Text <> "Wednesday") and (CreateShift_StartDay.Text <> "Thursday") and (CreateShift_StartDay.Text <> "Friday") and (CreateShift_StartDay.Text <> "Saturday") and (CreateShift_StartDay.Text <> "Sunday")) Then
            Response.Write("<script> alert(""Start Day must be Monday, Tuesday, Wednesday, Thursday, Friday, Saturday or Sunday!"")</script>") 
            return
        End If
        ' EndDay Is MTWTFSS 
        if ((CreateShift_EndDay.Text <> "Monday") and (CreateShift_EndDay.Text <> "Tuesday") and (CreateShift_EndDay.Text <> "Wednesday") and (CreateShift_EndDay.Text <> "Thursday") and (CreateShift_EndDay.Text <> "Friday") and (CreateShift_EndDay.Text <> "Saturday") and (CreateShift_EndDay.Text <> "Sunday")) Then 
            Response.Write("<script> alert(""End Day must be Monday, Tuesday, Wednesday, Thursday, Friday, Saturday or Sunday!"")</script>") 
            return
        End If
        ' PayRate Is $$.$$
        'Dim paternWithDot As Regex = New Regex($"\d+(\.\d+)+")
        'if (Not paternWithDot.IsMatch(CreateShift_PayRate.Text)) Then
        '    Response.Write("<script> alert(""Pay Rate must be in $$.$$ format!"")</script>") 
        '    return
        'End If
        'End Date >= Start Date
        if (DateTime.Parse(CreateShift_EndDate.Text) < DateTime.Parse(CreateShift_StartDate.Text)) Then
            Response.Write("<script> alert(""End Date must be after Start Date!"")</script>") 
            return
        End If
        'StartTime < EndTime if (StartDate == EndDate)
        If (CreateShift_StartDay.Text = CreateShift_EndDay.Text) Then 
            If (DateTime.Parse(CreateShift_StartTime.Text) >= DateTime.Parse(CreateShift_EndTime.Text).AddMinutes(-15)) Then
                Response.Write("<script> alert(""End Time must be at least 15 minutes after Start Time!"")</script>") 
                return
            End If
        End If
        ' EndDay Is same Or +1 of StartDay
        If (CreateShift_StartDay.Text = "Monday") Then
            If ((CreateShift_EndDay.Text <> "Monday") and (CreateShift_EndDay.Text <> "Tuesday")) Then
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        if (CreateShift_StartDay.Text = "Tuesday") Then
            if ((CreateShift_EndDay.Text <> "Tuesday") and (CreateShift_EndDay.Text <> "Wednesday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        if (CreateShift_StartDay.Text = "Wednesday") Then
            if ((CreateShift_EndDay.Text <> "Wednesday") and (CreateShift_EndDay.Text <> "Thursday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        if (CreateShift_StartDay.Text = "Thursday") Then
            if ((CreateShift_EndDay.Text <> "Thursday") and (CreateShift_EndDay.Text <> "Friday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        if (CreateShift_StartDay.Text = "Friday") Then
            if ((CreateShift_EndDay.Text <> "Friday") and (CreateShift_EndDay.Text <> "Saturday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        if (CreateShift_StartDay.Text = "Saturday") Then
            if ((CreateShift_EndDay.Text <> "Saturday") and (CreateShift_EndDay.Text <> "Sunday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        if (CreateShift_StartDay.Text = "Sunday") Then
            if ((CreateShift_EndDay.Text <> "Sunday") and (CreateShift_EndDay.Text <> "Monday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>") 
                return
            End If
        End If
        #End Region

        ' Get the ClientSite_ID given the ClientSite_DDL
        Dim queryString As string = "SELECT client_id FROM client_site_tbl WHERE client_site_id = '" + ClientSiteDDL.SelectedValue.ToString() + "'"
        Dim command As SqlCommand = new SqlCommand(queryString, conn)
        Dim client_id As string = ""
        Dim client_site_id As string = ClientSiteDDL.SelectedValue.ToString()
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read()) 
                client_id = reader(0).ToString()
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try
        ' Get the ClientSite_ID and ClientSite_Name
        CreateShift_ClientSite_ID.Text = ClientSiteDDL.SelectedValue.ToString()
        CreateShift_ClientSite_Name.Text = ClientSiteDDL.SelectedItem.Text
        CreateShift_SiteNameLabel.Text = "Client Site: " + ClientSiteDDL.SelectedItem.Text


        ' Get the Employee_id of the AssignedEmployee
        queryString = "SELECT employee_id, basepayrate FROM employee_tbl WHERE username = '" + CreateShift_EmployeeDDL.Text.Split(",").First() + "'"
        command = new SqlCommand(queryString, conn)
        Dim employee_id_temp As string = ""
        Dim basepayrate As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read()) 
                employee_id_temp = reader(0).ToString() 
                basepayrate = reader(1).ToString()
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try

        ' Check if any shift instances will be created
        Dim number_of_instances_created as Integer = 0
        Dim date1 As DateTime = DateTime.Parse(CreateShift_StartDate.Text).Date
        Dim enddate1 As DateTime = DateTime.Parse(CreateShift_EndDate.Text).Date
        While (date1 <= enddate1)
            If (date1.DayOfWeek.ToString() = CreateShift_StartDay.Text)
                number_of_instances_created += 1
            End If
            date1 = date1.AddDays(1)
        End While

        ' If no Instances to be created Then do not create this shift_template
        if (number_of_instances_created <= 0) Then
            Response.Write("<script> alert(""The proposed changes would result in 0 shift instances of this shift."")</script>")
            return
        End If

        Dim queryString2 As string = "INSERT INTO client_site_shift_tbl (client_site_id, client_id, shift_name, shift_description, employee_id, startdate, enddate, startdatetime, enddatetime, dailystarttime, dailyendtime, weeklystartday, weeklyendday, payrate, onduty_mealperiods) OUTPUT Inserted.client_site_shift_id VALUES (@client_site_id, @client_id, @shift_name, @shift_description, @employee_id, @startdate, @enddate, @startdatetime, @enddatetime, @dailystarttime, @dailyendtime, @weeklystartday, @weeklyendday, @payrate, @onduty_mealperiods)"
        Dim client_site_shift_id As string = ""
        command = new SqlCommand(queryString2, conn)
        command.Parameters.AddWithValue("@client_site_id", client_site_id)
        command.Parameters.AddWithValue("@client_id", client_id)
        command.Parameters.AddWithValue("@shift_name", CreateShift_Name.Text)
        command.Parameters.AddWithValue("@shift_description", CreateShift_Description.Text)
        command.Parameters.AddWithValue("@employee_id", employee_id_temp)
        command.Parameters.AddWithValue("@startdate", CreateShift_StartDate.Text.Split(" ").First())
        command.Parameters.AddWithValue("@enddate", CreateShift_EndDate.Text.Split(" ").First())
        command.Parameters.AddWithValue("@startdatetime", CreateShift_StartDate.Text.Split(" ").First() + " " + CreateShift_StartTime.Text.ToString())
        command.Parameters.AddWithValue("@enddatetime", CreateShift_EndDate.Text.Split(" ").First() + " " + CreateShift_EndTime.Text.ToString())
        command.Parameters.AddWithValue("@dailystarttime", CreateShift_StartTime.Text)
        command.Parameters.AddWithValue("@dailyendtime", CreateShift_EndTime.Text)
        command.Parameters.AddWithValue("@weeklystartday", CreateShift_StartDay.Text)
        command.Parameters.AddWithValue("@weeklyendday", CreateShift_EndDay.Text)
        command.Parameters.AddWithValue("@payrate", basepayrate)
        command.Parameters.AddWithValue("@onduty_mealperiods", CreateShift_OnDutyMeals.Checked.ToString())
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
                client_site_shift_id = reader(0).ToString()
            End While
            reader.Close()
            Log_Change(Session("user"), "Client_Site_Shift", Session("user") + " created a new shift for "+CreateShift_SiteNameLabel.Text+" that recurs every "+CreateShift_StartDay.Text+" starting "+CreateShift_StartDate.Text.Split( ).First() + " " + CreateShift_StartTime.Text.ToString()+" and ending "+CreateShift_EndDate.Text.Split( ).First() + " " + CreateShift_EndTime.Text.ToString()+" and assigned it to "+EmployeeDDL.SelectedItem.Text+".", "")
        Finally 
            conn.Close()
        End Try

        ' Create all shift instances for this shift
        date1 = DateTime.Parse(CreateShift_StartDate.Text)
        enddate1 = DateTime.Parse(CreateShift_EndDate.Text)
        While (date1 <= enddate1)
            If (date1.DayOfWeek.ToString() = CreateShift_StartDay.Text) Then
                ' Create a Shift Instance
                queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_description, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, @secondmeal_startdatetime, @secondmeal_enddatetime, @thirdmeal_startdatetime, @thirdmeal_enddatetime)"
                command = new SqlCommand(queryString, conn)
                command.Parameters.AddWithValue("@client_site_shift_id", client_site_shift_id)
                command.Parameters.AddWithValue("@client_site_id", client_site_id)
                command.Parameters.AddWithValue("@client_id", client_id)
                command.Parameters.AddWithValue("@shift_name", CreateShift_Name.Text)
                command.Parameters.AddWithValue("@shift_notes", " ")
                command.Parameters.AddWithValue("@employee_id", employee_id_temp)
                command.Parameters.AddWithValue("@startdatetime", date1.Date.ToString().Split(" ").First() + " " + CreateShift_StartTime.Text)
                Dim date2 As DateTime = date1
                If (CreateShift_StartDay.Text = CreateShift_EndDay.Text) Then
                    command.Parameters.AddWithValue("@enddatetime", date1.Date.ToString().Split(" ").First() + " " + CreateShift_EndTime.Text)
                else
                    date1 = date1.Date.AddDays(1)
                    command.Parameters.AddWithValue("@enddatetime", date2.Date.ToString().Split(" ").First() + " " + CreateShift_EndTime.Text)
                End If
                command.Parameters.AddWithValue("@payrate", basepayrate)
                command.Parameters.AddWithValue("@onduty_mealperiods", CreateShift_OnDutyMeals.Checked.ToString())
                Dim starttime = DateTime.Parse(date1.ToString().Split(" ").First() + " " + CreateShift_StartTime.Text)
                Dim endtime = DateTime.Parse(date2.ToString().Split(" ").First() + " " + CreateShift_EndTime.Text)
                Dim span As long = endtime.Ticks - starttime.Ticks
                Dim fivehours As long = 10000000L * 60 * 60 * 5
                Dim tenhours As long = 10000000L * 60 * 60 * 10
                Dim sixteenhours As long = 10000000L * 60 * 60 * 16
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
                If (thirdmeal_startdatetime <> "") Then
                    queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, @secondmeal_startdatetime, @secondmeal_enddatetime, @thirdmeal_startdatetime, @thirdmeal_enddatetime)"
                Else If (secondmeal_enddatetime <> "") Then 
                    queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, @secondmeal_startdatetime, @secondmeal_enddatetime, NULL, NULL)"
                Else 
                    queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, NULL, NULL, NULL, NULL)"
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
            End If

            date1 = date1.AddDays(1)
        End While

        'Close this form
        Load_Shift_Instances()
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp","$('#createmodal').modal('hide');",true)

    End Sub
    Protected Sub UpdateShift_Click(sender As Object, e As EventArgs)
        '' Validate Form Input
        ''PayRAte is $$.$$  
        Dim paternWithDot As Regex = new Regex($"\d+(\.\d+)+")
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
            if (starttime1.AddMinutes(15) >= endtime1) Then
                Response.Write("<script> alert(""End DateTime must be at least 15 minutes after Start DateTime!"")</script>")
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
        Dim span As Long = endtime.Ticks - starttime.Ticks
        Dim fivehours As Long = 10000000L * 60 * 60 * 5
        Dim tenhours As Long = 10000000L * 60 * 60 * 10
        Dim sixteenhours As Long = 10000000L * 60 * 60 * 16
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
        Dim Changes_Made As string = ""
        If (starttime <> Pre_SI_startdatetime.Text) Then Changes_Made += Session("user")+" changed Start Time from '"+Pre_SI_startdatetime.Text+"' to '"+starttime+"' <br />"
        If (endtime <> Pre_SI_enddatetime.Text) Then Changes_Made += Session("user")+" changed End Time from '"+Pre_SI_enddatetime.Text+"' to '"+endtime+"' <br />"
        If (ShiftInstance_EmployeeDDL.Text.Split(",").First() <> Pre_SI_username.Text) Then Changes_Made += Session("user")+" changed Employee from '"+Pre_SI_username.Text+"' to '"+ShiftInstance_EmployeeDDL.Text.Split(",").First()+"' <br />"
        If (ShiftInstance_Name.Text <> Pre_SI_Name.Text) Then Changes_Made += Session("user")+" changed Shift Instance Name from '"+Pre_SI_Name.Text+"' to '"+ShiftInstance_Name.Text+"' <br />"
        Try 
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            While (reader.Read()) 
            End While
            reader.Close()
            Log_Change(Session("user"), "Shift Instance", Session("user") + " updated Shift Instance "+ShiftInstance_ID.Text+": <br />"+Changes_Made, "")
        Finally 
            conn.Close()
        End Try
        
        'Close this form
        Load_Shift_Instances()
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp","$('#myshiftmodal').modal('hide');",true)

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
            Log_Change(Session("user"), "Shift Instance", Session("user") + " deleted Shift Instance "+ShiftInstance_ID.Text+" which had: <br />StartTime="+ShiftInstance_StartDate.Text.ToString().Split( ).First() + " " + ShiftInstance_StartTime.Text.ToString()+ "<br />EndTime="+ShiftInstance_EndDate.Text.ToString().Split(" ").First() + " " + ShiftInstance_EndTime.Text.ToString()+" <br />Employee="+ShiftInstance_EmployeeDDL.Text.Split(",").First()+" <br />ShiftName="+ShiftInstance_Name.Text+"", "")
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
                Log_Change(Session("user"), "Shift Template", Session("user") + " deleted Shift Instance "+ShiftInstance_ID.Text+" which was the last shift_instance of: <br />Shift="+ShiftTemplate_Name.Text+" <br />StartTime="+ShiftTemplate_StartTime.Text+" <br />StartDay="+StartDay_DDL.SelectedValue+" <br />Employee="+ShiftTemplate_EmployeeDDL.SelectedValue+"", "")
            Finally 
                conn.Close()
            End Try

            ' Message User that there are no more shift instances and the shift template was permanently deleted
            Response.Write("<script> alert(""You deleted the last shift instance for this shift. The shift template was also deleted."")</script>") 

        End If

        'Close this form
        Load_Shift_Instances()
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp","$('#myshiftmodal').modal('hide');",true)

    End Sub
    Protected Sub UpdateShiftTemplate_Click(sender As Object, e As EventArgs)
        
        #Region "Validate Form Input"
        ' StartDay is MTWTFSS 
        if ((StartDay_DDL.Text <> "Monday") And (StartDay_DDL.Text <> "Tuesday") And (StartDay_DDL.Text <> "Wednesday") And (StartDay_DDL.Text <> "Thursday") And (StartDay_DDL.Text <> "Friday") And (StartDay_DDL.Text <> "Saturday") And (StartDay_DDL.Text <> "Sunday")) Then
            Response.Write("<script> alert(""Start Day must be Monday, Tuesday, Wednesday, Thursday, Friday, Saturday or Sunday!"")</script>")
            return
        End If
        ' EndDay Is MTWTFSS  
        if ((EndDay_DDL.Text <> "Monday") And (EndDay_DDL.Text <> "Tuesday") And (EndDay_DDL.Text <> "Wednesday") And (EndDay_DDL.Text <> "Thursday") And (EndDay_DDL.Text <> "Friday") And (EndDay_DDL.Text <> "Saturday") And (EndDay_DDL.Text <> "Sunday")) THen 
            Response.Write("<script> alert(""End Day must be Monday, Tuesday, Wednesday, Thursday, Friday, Saturday or Sunday!"")</script>")
            return
        End If
        ' PayRAte Is $$.$$
        Dim paternWithDot As Regex = New Regex($"\d+(\.\d+)+")
        if (Not paternWithDot.IsMatch(ShiftTemplate_PayRate.Text)) Then
            Response.Write("<script> alert(""Pay Rate must be in $$.$$ format!"")</script>")
            return
        End If
        ' End Date >= Start Date 
        if (DateTime.Parse(ShiftTemplate_EndDate.Text) < DateTime.Parse(ShiftTemplate_StartDate.Text)) Then
            Response.Write("<script> alert(""End Date must be after Start Date!"")</script>")
            return
        End If
        ' StartTime < EndTime if (StartDate == EndDate)  
        if (StartDay_DDL.Text = EndDay_DDL.Text) Then 
            if (DateTime.Parse(ShiftTemplate_StartTime.Text) >= DateTime.Parse(ShiftTemplate_EndTime.Text).AddMinutes(-15)) Then
                Response.Write("<script> alert(""End Time must be at least 15 minutes after Start Time!"")</script>")
                return
            End If
        End If
        ' EndDay Is same Or +1 of StartDay 
        if (StartDay_DDL.Text = "Monday") Then
            if ((EndDay_DDL.Text <> "Monday") and (EndDay_DDL.Text <> "Tuesday")) Then 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End If
        if (StartDay_DDL.Text = "Tuesday") Then
            if ((EndDay_DDL.Text <> "Tuesday") and (EndDay_DDL.Text <> "Wednesday")) Then
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End If
        if (StartDay_DDL.Text = "Wednesday") Then
            if ((EndDay_DDL.Text <> "Wednesday") And (EndDay_DDL.Text <> "Thursday")) Then
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End If
        if (StartDay_DDL.Text = "Thursday") 
            if ((EndDay_DDL.Text <> "Thursday") And (EndDay_DDL.Text <> "Friday")) 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End If
        if (StartDay_DDL.Text = "Friday") 
            if ((EndDay_DDL.Text <> "Friday") And (EndDay_DDL.Text <> "Saturday")) 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End If
        if (StartDay_DDL.Text = "Saturday") 
            if ((EndDay_DDL.Text <> "Saturday") And (EndDay_DDL.Text <> "Sunday")) 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End IF
        if (StartDay_DDL.Text = "Sunday") 
            if ((EndDay_DDL.Text <> "Sunday") And (EndDay_DDL.Text <> "Monday")) 
                Response.Write("<script> alert(""EndDay must be same or +1 day from StartDay!"")</script>")
                return
            End If
        End If
        #End Region

        ' Get the Employee_id of the AssignedEmployee
        Dim queryString As string = "SELECT employee_id, basepayrate FROM employee_tbl WHERE username = '" + ShiftTemplate_EmployeeDDL.Text.Split(",").First() + "'"
        Dim command As SqlCommand = new SqlCommand(queryString, conn)
        DIm employee_id As string = ""
        Dim basepayrate As string = ""
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read()) 
                employee_id = reader(0).ToString() 
                basepayrate = reader(1).ToString()
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try

        ' Check if any shift instances will be created
        Dim number_of_instances_created As Integer = 0
        Dim date1 As DateTime = DateTime.Parse(ShiftInstance_StartDate.Text).Date
        Dim enddate1 As DateTime = DateTime.Parse(ShiftInstance_EndDate.Text).Date
        While (date1 <= enddate1)
            If (date1.DayOfWeek.ToString() = StartDay_DDL.Text)
                number_of_instances_created += 1
            End If
            date1 = date1.AddDays(1)
        End While

        ' If no Instances to be created Then do not create this shift_template
        if (number_of_instances_created <= 0) Then
            Response.Write("<script> alert(""The proposed changes would result in 0 shift instances of this shift."")</script>")
            return
        End If

        ' Update client_site_shift_tbl using form data
        queryString = "UPDATE client_site_shift_tbl SET shift_name=@shift_name, shift_description=@shift_description, employee_id=@employee_id, payrate=@payrate, onduty_mealperiods=@onduty_mealperiods, startdate=@startdate, enddate=@enddate, startdatetime=@startdatetime, enddatetime=@enddatetime, weeklystartday=@weeklystartday, weeklyendday=@weeklyendday, dailystarttime=@dailystarttime, dailyendtime=@dailyendtime WHERE client_site_shift_id = '" + Shift_ID.Text + "'"
        command = new SqlCommand(queryString, conn)
        command.Parameters.AddWithValue("@shift_name", ShiftTemplate_Name.Text)
        command.Parameters.AddWithValue("@shift_description", ShiftTemplate_ShiftDescription.Text)
        command.Parameters.AddWithValue("@employee_id", employee_id)
        command.Parameters.AddWithValue("@payrate", basepayrate)
        command.Parameters.AddWithValue("@onduty_mealperiods", ShiftTemplate_OnDutyMealPeriods.Checked.ToString())
        command.Parameters.AddWithValue("@startdate", ShiftTemplate_StartDate.Text.Split(" ").First())
        command.Parameters.AddWithValue("@enddate", ShiftTemplate_EndDate.Text.Split(" ").First())
        command.Parameters.AddWithValue("@startdatetime", ShiftTemplate_StartDate.Text.Split(" ").First() + " " + ShiftTemplate_StartTime.Text)
        command.Parameters.AddWithValue("@enddatetime", ShiftTemplate_EndDate.Text.Split(" ").First() + " " + ShiftTemplate_EndTime.Text)
        command.Parameters.AddWithValue("@weeklystartday", StartDay_DDL.Text)
        command.Parameters.AddWithValue("@weeklyendday", EndDay_DDL.Text)
        command.Parameters.AddWithValue("@dailystarttime", ShiftTemplate_StartTime.Text.ToString())
        command.Parameters.AddWithValue("@dailyendtime", ShiftTemplate_EndTime.Text.ToString())
        Dim Changes_Made As string = ""
        If (ShiftTemplate_StartDate.Text.Split(" ").First() + " " + ShiftTemplate_StartTime.Text <> Pre_ST_startdatetime.Text) Then Changes_Made += Session("user")+" changed Start Time from '"+Pre_ST_startdatetime.Text+"' to '"+ShiftTemplate_StartDate.Text.Split( ).First() + " " + ShiftTemplate_StartTime.Text+"' <br />"
        If (ShiftTemplate_EndDate.Text.Split(" ").First() + " " + ShiftTemplate_EndTime.Text <> Pre_ST_enddatetime.Text) Then Changes_Made += Session("user")+" changed End Time from '"+Pre_ST_enddatetime.Text+"' to '"+ShiftTemplate_EndDate.Text.Split( ).First() + " " + ShiftTemplate_EndTime.Text+"' <br />"
        If (ShiftTemplate_EmployeeDDL.Text.Split(",").First() <> Pre_ST_username.Text) Then Changes_Made += Session("user")+" changed Employee from '"+Pre_ST_username.Text+"' to '"+ShiftTemplate_EmployeeDDL.Text.Split(",").First()+"' <br />"
        If (ShiftTemplate_Name.Text <> Pre_ST_Name.Text) Then Changes_Made += Session("user")+" changed Shift Template Name from '"+Pre_ST_Name.Text+"' to '"+ShiftTemplate_Name.Text+"' <br />"
        If (StartDay_DDL.Text <> Pre_ST_startday.Text) Then Changes_Made += Session("user")+" changed Shift Template Name from '"+Pre_ST_Name.Text+"' to '"+ShiftTemplate_Name.Text+"' <br />"
        If (ShiftTemplate_OnDutyMealPeriods.Checked.ToString() <> Pre_ST_ondutymeals.Text) Then Changes_Made += Session("user")+" changed Shift Template Name from '"+Pre_ST_Name.Text+"' to '"+ShiftTemplate_Name.Text+"' <br />"

        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read())
            End While
            reader.Close()
            Log_Change(Session("user"), "Shift Template", Session("user") + " updated Shift "+Shift_ID.Text +": <br />"+Changes_Made+"<br /><br /> Which modified all future shift_instances for this shift, accordingly.", "")
        Finally 
            conn.Close()
        End Try


        ' Delete all future shift_instances
        queryString = "DELETE FROM client_site_shift_instance_tbl WHERE client_site_shift_id = '" + Shift_ID.Text + "' AND startdatetime >= '" + DateTime.Now.Date + "'"
        command = new SqlCommand(queryString, conn)
        Try
            conn.Close()
            conn.Open()
            Dim reader As SqlDataReader = command.ExecuteReader()
            while (reader.Read()) 
            End While
            reader.Close()
        finally 
            conn.Close()
        End Try

        ' Re-Create all future shift instances using the input data ... From tomorrow until the EndDate 
        date1 = DateTime.Now.Date
        enddate1 = DateTime.Parse(ShiftTemplate_EndDate.Text)
        While (date1 < enddate1)
            If (date1.DayOfWeek.ToString() = StartDay_DDL.Text) Then
            
                ' Create a Shift Instance
                queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, @secondmeal_startdatetime, @secondmeal_enddatetime, @thirdmeal_startdatetime, @thirdmeal_enddatetime)"
                command = new SqlCommand(queryString, conn)
                command.Parameters.AddWithValue("@client_site_shift_id", Shift_ID.Text)
                command.Parameters.AddWithValue("@client_site_id", ClientSite_ID.Text)
                command.Parameters.AddWithValue("@client_id", Client_ID_.Text)
                command.Parameters.AddWithValue("@shift_name", ShiftTemplate_Name.Text)
                command.Parameters.AddWithValue("@shift_notes", ShiftTemplate_ShiftDescription.Text)
                command.Parameters.AddWithValue("@employee_id", employee_id)
                command.Parameters.AddWithValue("@startdatetime", date1.Date.ToString().Split(" ").First() + " " + ShiftTemplate_StartTime.Text.ToString())
                Dim date2 As DateTime = date1
                if (StartDay_DDL.Text = EndDay_DDL.Text) Then
                    command.Parameters.AddWithValue("@enddatetime", date1.Date.ToString().Split(" ").First() + " " + ShiftTemplate_EndTime.Text.ToString())
                Else 
                    date2 = date2.Date.AddDays(1)
                    command.Parameters.AddWithValue("@enddatetime", date2.Date.ToString().Split(" ").First() + " " + ShiftTemplate_EndTime.Text.ToString())
                End If
                command.Parameters.AddWithValue("@payrate", basepayrate)
                command.Parameters.AddWithValue("@onduty_mealperiods", ShiftTemplate_OnDutyMealPeriods.Checked.ToString())
                Dim starttime As DateTime = DateTime.Parse(date1.ToString().Split(" ").First() + " " + ShiftTemplate_StartTime.Text.ToString())
                Dim endtime As DateTime = DateTime.Parse(date1.ToString().Split(" ").First() + " " + ShiftTemplate_EndTime.Text.ToString())
                Dim span As Long = endtime.Ticks - starttime.Ticks
                Dim fivehours As Long = 10000000L * 60 * 60 * 5
                Dim tenhours As long = 10000000L * 60 * 60 * 10
                Dim sixteenhours As long = 10000000L * 60 * 60 * 16
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
                if (thirdmeal_startdatetime <> "") Then
                    queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, @secondmeal_startdatetime, @secondmeal_enddatetime, @thirdmeal_startdatetime, @thirdmeal_enddatetime)"
                Else if (secondmeal_enddatetime <> "") Then 
                    queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, @secondmeal_startdatetime, @secondmeal_enddatetime, NULL, NULL)"
                Else 
                    queryString = "INSERT INTO client_site_shift_instance_tbl (client_site_shift_id, client_site_id, client_id, shift_name, shift_notes, employee_id, startdatetime, enddatetime, payrate, onduty_mealperiods, firstmeal_startdatetime, firstmeal_enddatetime, secondmeal_startdatetime, secondmeal_enddatetime, thirdmeal_startdatetime, thirdmeal_enddatetime) VALUES (@client_site_shift_id, @client_site_id, @client_id, @shift_name, @shift_notes, @employee_id, @startdatetime, @enddatetime, @payrate, @onduty_mealperiods, @firstmeal_startdatetime, @firstmeal_enddatetime, NULL, NULL, NULL, NULL)"
                End If
                command.CommandText = queryString
                Try
                    conn.Close()
                    conn.Open()
                    Dim reader As SqlDataReader = command.ExecuteReader()
                    while (reader.Read())

                    End While
                    reader.Close()
                    number_of_instances_created += 1
                Finally 
                    conn.Close()
                End Try
                
            End If

            date1 = date1.AddDays(1)
        End While

        'Close this form
        Load_Shift_Instances()
        ClientScript.RegisterStartupScript(Page.GetType,"PopUp","$('#myshiftmodal').modal('hide');",true)
    End Sub


    '' Log Changes
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