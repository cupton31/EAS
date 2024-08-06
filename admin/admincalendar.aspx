<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="admincalendar.aspx.vb" Inherits="EAS.admincalendar" EnableEventValidation="true" %>

<%@ Register Assembly="DayPilot" Namespace="DayPilot.Web.Ui" TagPrefix="DayPilot" %>


<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Calendar</title>
    <style>
    #DropDownList1, #username, #password, #retypepassword, #logusername, #logpassword, #DateHired, #fname, #lname, #age, #contact, #position, #DateHired {
        padding: 4px;
        display: inline-block;
        box-sizing: border-box;
        border: 1px solid #ccc;
    }
</style>
</head>

<body>
    <form id="form1" runat="server">
    <div id="wrapper"><!-- #include file="adminnavfixedcalendar.html"-->
	<div id="page-wrapper">
           <div class="col-lg-12">  
               Calendar
            </div>


    <div>
        <br /><br /><br />
            <asp:label runat="server" ID="DisplayingLabel" Font-Size="X-Large" Font-Bold="true" Text="Displaying: Calendar_View_State"></asp:label><hr />

            <asp:Button ID="AllButton" runat="server" Font-Size="Larger" Font-Bold="true" Text="All" OnClick="AllButton_OnClick" CssClass="btn btn-sm btn-primary"/>

            &nbsp &nbsp <asp:Button ID="OpenButton" Font-Size="Larger" Font-Bold="true" runat="server" Text="Open" OnClick="OpenButton_OnClick" CssClass="btn btn-sm btn-primary"/>

            &nbsp &nbsp <asp:label ID="ClientSiteLabel" runat="server" Font-Size="Larger" Font-Bold="true">Client Site:</asp:label>
            <asp:DropDownList ID="ClientSiteDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ClientSiteDDL_SelectedIndexChanged">
            </asp:DropDownList>

            &nbsp &nbsp <asp:label ID="EmployeeLabel" runat="server" Font-Size="Larger" Font-Bold="true">Employee:</asp:label>
            <asp:DropDownList ID="EmployeeDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="EmployeeDDL_SelectedIndexChanged">
            </asp:DropDownList>

            &nbsp &nbsp <asp:label ID="StartDateLabel" runat="server" Font-Size="Larger" Font-Bold="true">Start Date:</asp:label> <asp:TextBox ID="StartDateTB" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="true" TextMode="Date" ViewState="enabled" OnTextChanged="StartDateTB_OnTextChanged"/>

            &nbsp &nbsp <asp:label ID="DaysLabel" runat="server" Font-Size="Larger" Font-Bold="true">Days:</asp:label>
            <asp:DropDownList ID="DaysDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="DaysDDL_SelectedIndexChanged">
                <asp:ListItem>1</asp:ListItem>
                <asp:ListItem>2</asp:ListItem>
                <asp:ListItem>3</asp:ListItem>
                <asp:ListItem>4</asp:ListItem>
                <asp:ListItem>5</asp:ListItem>
                <asp:ListItem>6</asp:ListItem>
                <asp:ListItem Selected=True>7</asp:ListItem>
                <asp:ListItem>8</asp:ListItem>
            </asp:DropDownList>

            &nbsp &nbsp <asp:label ID="CellHeightLabel" runat="server" Font-Size="Larger" Font-Bold="true">Cell Height:</asp:label>
            <asp:DropDownList ID="CellHeightDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="CellHeightDDL_SelectedIndexChanged">
                <asp:ListItem>10</asp:ListItem>
                <asp:ListItem Selected=True>15</asp:ListItem>
                <asp:ListItem>20</asp:ListItem>
                <asp:ListItem>30</asp:ListItem>
                <asp:ListItem>40</asp:ListItem>
                <asp:ListItem>50</asp:ListItem>
                <asp:ListItem>60</asp:ListItem>
                <asp:ListItem>70</asp:ListItem>
                <asp:ListItem>80</asp:ListItem>
            </asp:DropDownList>
        
            &nbsp&nbsp&nbsp &nbsp&nbsp &nbsp&nbsp &nbsp <asp:Button ID="CreateShift_Button" runat="server" Font-Size="Larger" Font-Bold="true" Text="Create Shift" OnClick="CreateButton_OnClick" CssClass="btn btn-sm btn-danger"/>
        
        <br /><br />
    </div> 
     <div>
         <DayPilot:DayPilotCalendar ID="DayPilotCalendar"
             DataTextField="name"
             DataValueField="id"
             TimeFormat="Clock12Hours"
             DataStartField="Start"
             DataEndField="End"
             NonBusinessHours="Show"
             CellHeight="30"
             Days="7"
             OnBeforeEventRender="DayPilotCalendar_BeforeEventRender"
             EventClickHandling="PostBack"
             EventClickJavaScript="alert(e.id());"
             OnEventClick="DayPilotCalendar_OnEventClick"
             runat="server" HeightSpec="Full" ShowEventStartEnd="False" BusinessBeginsHour="0" BusinessEndsHour="24" EventBackColor="White" ShowHours="True" EnableTheming="False" BorderWidth="3" BorderColor="Black" BackColor="White" EventBorderColor="Black" DurationBarColor="#33CCFF" BorderStyle="Solid" CssOnly="False"  />

       </div>
       <br /><br /><br /><br /><br />
      </div>
     </div>

        <div class="container" >
            <div class="modal fade" id="myshiftmodal" data-backdrop="false" role="dialog">
                <div class="modal-dialog modal-dailog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="modal-title"></h4>
                            <asp:Label ID="ShiftModalLabel" Font-Size="X-Large" Font-Bold="true" Text="Site Name" runat="server" />
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                        </div>
                         <div class="modal-body">
                             <ul class="nav nav-tabs" id="tabContent">
                                 <li class="active"><a href="#shift" data-toggle="tab" onclick="shiftClick">Shift Instance</a></li>
                                 <li><a href="#shiftTemplate" data-toggle="tab" onclick="shiftTemplateClick">Shift Template</a></li>
                             </ul>
                             <div class="tab-content">
                                 <div class="tab-pane active" id="shift">
                                     <asp:TextBox id="ShiftInstance_ID" runat="server" visible="false" Text="" />
                                     <br />
                                     <label>Shift Name</label>
                                     <asp:TextBox ID="ShiftInstance_Name" CssClass="form-control" runat="server" />
                                     <br />
                                     <label>Employee Assigned</label>
                                     <asp:DropDownList ID="ShiftInstance_EmployeeDDL" CssClass="form-control" runat="server">
                                     </asp:DropDownList>
                                     <br />
                                     <label>Shift Notes</label>
                                     <asp:TextBox ID="ShiftInstance_ShiftNotes" CssClass="form-control" TextMode="MultiLine" Rows="1" runat="server" />
                                     <br />
                                     <label>Pay Rate: </label>
                                     <asp:TextBox ID="ShiftInstance_PayRate" enabled="false" runat="server" />
                                     &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                     <label>On Duty Meals: </label>
                                     <asp:CheckBox ID="ShiftInstance_OnDutyMealPeriods" runat="server" />
                                     <br /><br /><br />
                                     <label>Start Date: </label>
                                     <asp:TextBox ID="ShiftInstance_StartDate" runat="server" TextMode="Date"></asp:TextBox>
                                     &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                     <label>Start Time: </label>
                                     <asp:TextBox ID="ShiftInstance_StartTime" runat="server" TextMode="Time" format="HH:mm"></asp:TextBox>
                                     <br /><br />
                                     <label>End Date:&nbsp</label>
                                     <asp:TextBox ID="ShiftInstance_EndDate" runat="server" TextMode="Date"></asp:TextBox>
                                     &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                     <label>End Time:&nbsp</label>
                                     <asp:TextBox ID="ShiftInstance_EndTime" runat="server" TextMode="Time" format="HH:mm"></asp:TextBox>
                                     <br /><br />
                                     <asp:Button ID="DeleteShift_Button" CssClass="btn btn-danger" OnClick="DeleteShift_Click" Text="Delete Shift" runat="server" style="position:relative; float:left; margin-right:20px;"/>
                                     <asp:Button ID="UpdateShift_Button" CssClass="btn btn-primary" OnClick="UpdateShift_Click" Text="Update Shift" runat="server" style="position:relative; float:right; margin-right:20px;"/>
                                     <button type="button" class="btn btn-danger" data-dismiss="modal" style="position:relative; float:right; margin-right:20px;">Close</button>
                                 </div> 
                                 <div class="modal-footer">

                                </div>
                                <div class="tab-pane" id="shiftTemplate">
                                    <asp:TextBox id="Shift_ID" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Client_ID_" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="ClientSite_ID" runat="server" visible="false" Text="" />

                                    <asp:TextBox id="Pre_SI_Name" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_SI_Notes" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_SI_startdatetime" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_SI_enddatetime" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_SI_username" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_Name" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_Description" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_employee_id" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_startdatetime" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_enddatetime" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_ondutymeals" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_startday" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_endday" runat="server" visible="false" Text="" />
                                    <asp:TextBox id="Pre_ST_username" runat="server" visible="false" Text="" />

                                    <label>Shift Name</label>
                                    <asp:TextBox ID="ShiftTemplate_Name" CssClass="form-control" runat="server" />
                                    <br />
                                    <label>Employee Assigned</label>
                                    <asp:DropDownList ID="ShiftTemplate_EmployeeDDL" CssClass="form-control" runat="server">
                                    </asp:DropDownList>
                                    <br />
                                    <label>Shift Description</label>
                                    <asp:TextBox ID="ShiftTemplate_ShiftDescription" CssClass="form-control" TextMode="MultiLine" Rows="4" runat="server" />
                                    <br />
                                    <label>Pay Rate: </label>
                                    <asp:TextBox ID="ShiftTemplate_PayRate" enabled="false" runat="server" />
                                    &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                    <label>On Duty Meals: </label>
                                    <asp:CheckBox ID="ShiftTemplate_OnDutyMealPeriods" runat="server" />
                                    <br /><br /><br />
                                    <asp:Table runat="server">
                                        <asp:TableRow>
                                            <asp:TableCell><label>Start Date:&nbsp&nbsp</label><asp:TextBox ID="ShiftTemplate_StartDate" runat="server" TextMode="Date"></asp:TextBox></asp:TableCell>
                                            <asp:TableCell><label>&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp</label></asp:TableCell>
                                            <asp:TableCell><label>End Date:&nbsp&nbsp</label><asp:TextBox ID="ShiftTemplate_EndDate" runat="server" TextMode="Date"></asp:TextBox></asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow><asp:TableCell><label>&nbsp</label></asp:TableCell></asp:TableRow>
                                        <asp:TableRow><asp:TableCell><label>&nbsp</label></asp:TableCell></asp:TableRow>
                                        <asp:TableRow><asp:TableCell Font-Bold="true" Font-Italic="true"><label>(Weekly Recurring Data)</label></asp:TableCell></asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell><label>&nbsp&nbsp&nbsp&nbsp Start Time: &nbsp&nbsp</label><asp:TextBox ID="ShiftTemplate_StartTime" runat="server" TextMode="Time" ></asp:TextBox></asp:TableCell>
                                            <asp:TableCell><label>&nbsp&nbsp</label></asp:TableCell>
                                            <asp:TableCell><label>&nbsp&nbsp End Time: </label>&nbsp&nbsp<asp:TextBox ID="ShiftTemplate_EndTime" runat="server" TextMode="Time"></asp:TextBox></asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell><label>&nbsp&nbsp&nbsp&nbsp Start Day: &nbsp&nbsp</label><asp:DropDownList ID="StartDay_DDL" runat="server">
                                                    <asp:ListItem>Sunday</asp:ListItem>
                                                    <asp:ListItem>Monday</asp:ListItem>
                                                    <asp:ListItem>Tuesday</asp:ListItem>
                                                    <asp:ListItem>Wednesday</asp:ListItem>
                                                    <asp:ListItem>Thursday</asp:ListItem>
                                                    <asp:ListItem>Friday</asp:ListItem>
                                                    <asp:ListItem>Saturday</asp:ListItem>
                                                </asp:DropDownList></asp:TableCell>
                                            <asp:TableCell><label>&nbsp&nbsp</label></asp:TableCell>
                                            <asp:TableCell><label>&nbsp&nbsp End Day: </label>&nbsp&nbsp<asp:DropDownList ID="EndDay_DDL" runat="server">
                                                    <asp:ListItem>Sunday</asp:ListItem>
                                                    <asp:ListItem>Monday</asp:ListItem>
                                                    <asp:ListItem>Tuesday</asp:ListItem>
                                                    <asp:ListItem>Wednesday</asp:ListItem>
                                                    <asp:ListItem>Thursday</asp:ListItem>
                                                    <asp:ListItem>Friday</asp:ListItem>
                                                    <asp:ListItem>Saturday</asp:ListItem>
                                                </asp:DropDownList></asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                    <br /> <asp:Label runat="server" Font-Size="Smaller">* Template changes will update template and all shifts with StartTime > DateTime.Now </asp:Label>
                                    <br /><br />
                                    <asp:Button ID="UpdateShiftTemplate_Button" CssClass="btn btn-primary" OnClick="UpdateShiftTemplate_Click" Text="Update Template" runat="server" style="position:relative; float:right; margin-right:20px;" />
                                    <button type="button" class="btn btn-danger" data-dismiss="modal" style="position:relative; float:right; margin-right:20px;">Close</button>
                                </div>
                                <div class="modal-footer"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="container" >
            <div class="modal fade" id="createmodal" data-backdrop="false" role="dialog">
                <div class="modal-dialog modal-dailog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="modal-title"></h4>
                            <asp:Label ID="CreateShift_SiteNameLabel" Font-Size="X-Large" Font-Bold="true" Text="Client Site: " runat="server" />
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                        </div>
                        <div class="modal-body">
                            <div class="tab-pane active" id="create">
                                <asp:TextBox id="CreateShift_ClientSite_ID" runat="server" visible="false" Text="" />
                                <asp:TextBox id="CreateShift_ClientSite_Name" runat="server" visible="false" Text="" />
                                <br />
                                <label>Shift Name</label>
                                <asp:TextBox ID="CreateShift_Name" CssClass="form-control" runat="server" />
                                <br />
                                <label>Employee Assigned</label>
                                <asp:DropDownList ID="CreateShift_EmployeeDDL" CssClass="form-control" runat="server">
                                </asp:DropDownList>
                                <br />
                                <label>Shift Description</label>
                                <asp:TextBox ID="CreateShift_Description" CssClass="form-control" TextMode="MultiLine" Rows="4" runat="server" />
                                <br />
                                <label> </label>
                                <asp:TextBox ID="CreateShift_PayRate" enabled="false" Visible="false" runat="server" />
                                &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                <label>On Duty Meals: </label>
                                <asp:CheckBox ID="CreateShift_OnDutyMeals" runat="server" />
                                <br /><br /><br />
                                <asp:Table runat="server">
                                    <asp:TableRow>
                                        <asp:TableCell><label>Start Date:&nbsp&nbsp</label><asp:TextBox ID="CreateShift_StartDate" runat="server" TextMode="Date"></asp:TextBox></asp:TableCell>
                                        <asp:TableCell><label>&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp</label></asp:TableCell>
                                        <asp:TableCell><label>End Date:&nbsp&nbsp</label><asp:TextBox ID="CreateShift_EndDate" runat="server" TextMode="Date"></asp:TextBox></asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow><asp:TableCell><label>&nbsp</label></asp:TableCell></asp:TableRow>
                                    <asp:TableRow><asp:TableCell><label>&nbsp</label></asp:TableCell></asp:TableRow>
                                    <asp:TableRow><asp:TableCell Font-Bold="true" Font-Italic="true"><label>(Weekly Recurring Data)</label></asp:TableCell></asp:TableRow>
                                    <asp:TableRow>
                                        <asp:TableCell><label>&nbsp&nbsp&nbsp&nbsp Start Time: &nbsp&nbsp</label><asp:TextBox ID="CreateShift_StartTime" runat="server" TextMode="Time" ></asp:TextBox></asp:TableCell>
                                        <asp:TableCell><label>&nbsp&nbsp</label></asp:TableCell>
                                        <asp:TableCell><label>&nbsp&nbsp End Time: </label>&nbsp&nbsp<asp:TextBox ID="CreateShift_EndTime" runat="server" TextMode="Time"></asp:TextBox></asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow>
                                        <asp:TableCell><label>&nbsp&nbsp&nbsp&nbsp Start Day: &nbsp&nbsp</label><asp:DropDownList ID="CreateShift_StartDay" runat="server">
                                                <asp:ListItem>Sunday</asp:ListItem>
                                                <asp:ListItem>Monday</asp:ListItem>
                                                <asp:ListItem>Tuesday</asp:ListItem>
                                                <asp:ListItem>Wednesday</asp:ListItem>
                                                <asp:ListItem>Thursday</asp:ListItem>
                                                <asp:ListItem>Friday</asp:ListItem>
                                                <asp:ListItem>Saturday</asp:ListItem>
                                            </asp:DropDownList></asp:TableCell>
                                        <asp:TableCell><label>&nbsp&nbsp</label></asp:TableCell>
                                        <asp:TableCell><label>&nbsp&nbsp End Day: </label>&nbsp&nbsp<asp:DropDownList ID="CreateShift_EndDay" runat="server">
                                                <asp:ListItem>Sunday</asp:ListItem>
                                                <asp:ListItem>Monday</asp:ListItem>
                                                <asp:ListItem>Tuesday</asp:ListItem>
                                                <asp:ListItem>Wednesday</asp:ListItem>
                                                <asp:ListItem>Thursday</asp:ListItem>
                                                <asp:ListItem>Friday</asp:ListItem>
                                                <asp:ListItem>Saturday</asp:ListItem>
                                            </asp:DropDownList></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                                <br /> <asp:Label runat="server" Font-Size="Smaller">* Create Shift button creates template and all shift instances from StartDate to EndDate.</asp:Label>
                                <asp:HiddenField ID="HiddenField2" runat="server"/>
                                <br /><br />
                                <asp:Button ID="CreateShift_ModalButton" CssClass="btn btn-primary" OnClick="CreateShift_Click" Text="Create Shift" runat="server" style="position:relative; float:right; margin-right:20px;" />
                                <button type="button" class="btn btn-danger" data-dismiss="modal" style="position:relative; float:right; margin-right:20px;">Close</button>
                            </div> 
                            <div class="modal-footer">

                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>





    </form>

   
</body>

<script>
    $('#tabContent a').click(function (e) {
        e.preventDefault()
        $(this).tab('show')
    })
</script>
<script src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js"></script>
<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/js/bootstrap.min.js"></script>

</html>