Imports System.Data.SqlClient
Imports System.Drawing

Public Class adminclient
    Inherits System.Web.UI.Page

    Public con As String = ConfigurationManager.ConnectionStrings("Excalibur").ConnectionString
    Public conn = New SqlConnection(con)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("user") = vbNullString Then
            Response.Redirect("~/LoginForm.aspx")
        End If

    End Sub
    Protected Sub logout_click(sender As Object, e As EventArgs)
        Log_Change(Session("user"), "Log Out", "Logged Out", "Log Out")

        Session.Remove("user")
        Session.RemoveAll()
        Session.Abandon()
        Response.Redirect("~/LoginForm.aspx")
    End Sub '
    Protected Sub Insert(sender As Object, e As EventArgs)
        ''InsertCommand="INSERT INTO client_tbl (clientname, phone, email, streetaddress, streetaddress2, city, zip, state, website, daterelationshipstarted, description) VALUES (@clientname, @phone, @email, @streetaddress, @streetaddress2, @city, @zip, @state, @website, @daterelationshipstarted, @description)"
    End Sub
    Protected Sub Client_OnRowUpdating(sender As Object, e As GridViewUpdateEventArgs)
         '' Validate Input
         #Region "NewValues cannot be blank"
        If (e.NewValues("clientname") = Nothing) Then
            Response.Write("<script> alert(""Client Name cannot be blank."")</script>")
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
        If (e.NewValues("website") = Nothing) Then
            e.NewValues("website") = " "
            'Response.Write("<script> alert(""Web Site cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        If (e.NewValues("daterelationshipstarted") = Nothing) Then
            Response.Write("<script> alert(""Date Relationship Started cannot be blank."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("description") = Nothing) Then
            e.NewValues("description") = " "
            'Response.Write("<script> alert(""Description cannot be blank."")</script>")
            'e.cancel = true
            'return
        End If
        #End Region

        Try 
            DateTime.Parse(e.NewValues("daterelationshipstarted"))
        Catch ex As Exception
            Response.Write("<script> alert(""Relationship DateTime is in an incompatable format."")</script>")
            e.Cancel = true
            return
        End Try
        If (e.NewValues("clientname").ToString.Length > 49) Then
            Response.Write("<script> alert(""Client Name must be less than 50 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("phone").ToString.Length > 49) Then
            Response.Write("<script> alert(""Phone must be less than 50 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("email").ToString.Length > 49) Then
            Response.Write("<script> alert(""Email must be less than 50 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("website").ToString.Length > 49) Then
            Response.Write("<script> alert(""Web Site must be less than 200 characters."")</script>")
            e.cancel = true
            return
        End If
        If (e.NewValues("description").ToString.Length > 5000) Then
            Response.Write("<script> alert(""Description must be less than 5000 characters."")</script>")
            e.cancel = true
            return
        End If
        
        #Region "Fill Old and New Dictionaries in case of Nothing values"
        If (e.OldValues("clientname") = Nothing) Then e.OldValues("clientname") = " "
        If (e.OldValues("phone") = Nothing) Then e.OldValues("phone") =  " "
        If (e.OldValues("email") = Nothing) Then e.OldValues("email") =  " "
        If (e.OldValues("website") = Nothing) Then e.OldValues("website") =  " "
        If (e.OldValues("daterelationshipstarted") = Nothing) Then e.OldValues("daterelationshipstarted") =  " "
        If (e.OldValues("description") = Nothing) Then e.OldValues("description") =  " "

        If (e.NewValues("clientname") = Nothing) Then e.NewValues("clientname") = " "
        If (e.NewValues("phone") = Nothing) Then e.NewValues("phone") = " "
        If (e.NewValues("email") = Nothing) Then e.NewValues("email") = " "
        If (e.NewValues("website") = Nothing Or e.NewValues("website") = " ") Then e.NewValues("website") = " "
        If (e.NewValues("daterelationshipstarted") = Nothing) Then e.NewValues("daterelationshipstarted") = " "
        If (e.NewValues("description") = Nothing Or e.NewValues("description") = "") Then e.NewValues("description") = " "
        #End Region

        '' Add Change To Log
        Dim Changes_Made As string = ""
        if (e.OldValues("clientname") <> e.NewValues("clientname")) Then Changes_Made += Session("user") + " changed Client Name of "+e.OldValues("clientname").ToString+" from '"+e.OldValues("clientname").ToString()+"' to '"+e.NewValues("clientname").ToString()+"' <br />"
        if (e.OldValues("phone") <> e.NewValues("phone")) Then Changes_Made += Session("user") + " changed Phone of "+e.OldValues("clientname").ToString+" from '"+e.OldValues("phone").ToString()+"' to '"+e.NewValues("phone").ToString()+"' <br />"
        if (e.OldValues("email") <> e.NewValues("email")) Then Changes_Made += Session("user") + " changed Email of "+e.OldValues("clientname").ToString+" from '"+e.OldValues("email").ToString()+"' to '"+e.NewValues("email").ToString()+"' <br />"
        if (e.OldValues("website") <> e.NewValues("website")) Then Changes_Made += Session("user") + " changed Web Site of "+e.OldValues("clientname").ToString+" from '"+e.OldValues("website").ToString()+"' to '"+e.NewValues("website").ToString()+"' <br />"
        if (e.OldValues("daterelationshipstarted") <> e.NewValues("daterelationshipstarted")) Then Changes_Made += Session("user") + " changed DateRelationshipStarted of "+e.OldValues("clientname").ToString+" from '"+e.OldValues("daterelationshipstarted").ToString()+"' to '"+e.NewValues("daterelationshipstarted").ToString()+"' <br />"
        if (e.OldValues("description") <> e.NewValues("description")) Then Changes_Made += Session("user") + " changed Description of "+e.OldValues("clientname").ToString+" from '"+e.OldValues("description").ToString()+"' to '"+e.NewValues("description").ToString()+"' <br />"

        If (Changes_Made <> "") Then Log_Change(Session("user"), "Client", Changes_Made, "Update")
    End Sub
    Protected Sub Client_OnRowDeleting(sender As Object, e As GridViewDeleteEventArgs)
        Dim Changes_Made As string = Session("user") + " deleted row with client_id#"+e.Keys.Values(0).ToString()+" which had values: clientname="+e.Values("clientname")+", phone="+e.Values("phone")+", email="+e.Values("email")+", website="+e.Values("website")+", daterelationshipstarted="+e.Values("daterelationshipstarted")+", description="+e.Values("description")+"."
        Log_Change(Session("user"), "Client", Changes_Made, "Delete")
    End Sub

    Protected Sub createClient(sender As Object, e As EventArgs)
        Dim clientnames As String = clientname.Text
        Dim phones As String = phone.Text
        Dim emails As String = email.Text
        Dim streetaddresss As String = streetaddress.Text
        Dim streetaddress2s As String = streetaddress2.Text
        Dim citys As String = city.Text
        Dim zips As String = zip.Text
        Dim states As String = state.Text
        Dim websites As String = website.Text
        Dim daterelationshipstarteds As String = daterelationshipstarted.Text
        Dim descriptions As String = description.Text

        Dim validate As String = "SELECT clientname FROM client_tbl WHERE clientname ='" & clientnames & "'"
        Dim cmd = New SqlCommand(validate, conn)

        Dim myTable As New DataTable
        Dim adapter As New SqlDataAdapter(cmd)
        adapter.Fill(myTable)

        If myTable.Rows.Count > 0 Then
            Response.Write("<script> alert(""Username is already existed"")</script>")
        Else

            Dim addquery As String = "INSERT INTO client_tbl (clientname, phone, email, streetaddress, streetaddress2, city, zip, state, website, daterelationshipstarted, description) VALUES (@clientname, @phone, @email, @streetaddress, @streetaddress2, @city, @zip, @state, @website, @daterelationshipstarted, @description)"
            Dim com = New SqlCommand(addquery, conn)

            com.Parameters.AddWithValue("@clientname", clientnames)
            com.Parameters.AddWithValue("@phone", phones)
            com.Parameters.AddWithValue("@email", emails)
            com.Parameters.AddWithValue("@streetaddress", streetaddresss)
            com.Parameters.AddWithValue("@streetaddress2", streetaddress2s)
            com.Parameters.AddWithValue("@city", citys)
            com.Parameters.AddWithValue("@zip", zips)
            com.Parameters.AddWithValue("@state", states)
            com.Parameters.AddWithValue("@website", websites)
            com.Parameters.AddWithValue("@daterelationshipstarted", daterelationshipstarteds)
            com.Parameters.AddWithValue("@description", descriptions)

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
                    Log_Change(Session("user"), "Client", Session("user") + " created a New Client: ClientName='"+clientnames+"'", "Insert")
                    Response.Write("<script> alert(""New Client Created successfully"")</script>")
                Case 0
                    Response.Write("<script> alert(""Issue connecting to server or executing the Create Client query!"")</script>")
            End Select
        End If

        ''InsertCommand="INSERT INTO attendancelogs (employee_id, username, action, datetime, date, time) VALUES (@employee_id, @username, @action, @datetime, @date, @time)"
        clientname.Text = ""
        phone.Text = ""
        email.Text = ""
        streetaddress.Text = ""
        streetaddress2.Text = ""
        city.Text = ""
        zip.Text = ""
        state.Text = ""
        website.Text = ""
        daterelationshipstarted.Text = ""
        description.Text = ""

        '' Refresh
        GridView1.DataBind()

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


