<%@ Page Language="vb" AutoEventWireup="false" MaintainScrollPositionOnPostback="true" CodeBehind="adminscheduler.aspx.vb" Inherits="EAS.adminscheduler" EnableEventValidation="True" %>
<%@ Register Assembly="DayPilot" Namespace="DayPilot.Web.Ui" TagPrefix="DayPilot" %>


<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Scheduler</title>
    <style>
        #DropDownList1, #username, #password, #retypepassword, #logusername, #logpassword, #DateHired, #fname, #lname, #age, #contact, #position, #DateHired {
            padding: 4px;
            display: inline-block;
            box-sizing: border-box;
            border: 1px solid #ccc;
        }
    </style>
    <style>
     th {
        position: sticky!important;
        top: -1px;
        }
    th, td {

        }
    div.scroll {
            margin: 0px, 0px;
            padding: 0px;
            width: auto;
            height: auto;
            max-height: 70%;
            overflow-x: auto;
            overflow-y: scroll;
            margin-top: 2px;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server" autocomplete="off">
    <div id="wrapper">
        <!-- #include file="adminnavfixedscheduler.html"-->
       <div id="page-wrapper">
           <div class="col-lg-12">  
               Scheduler
            </div>


    <div>
        <br /><br /><br />
            
            <asp:label runat="server" ID="DisplayingLabel" Font-Size="X-Large" Font-Bold="true" Text="Displaying: Location" Visible="false"></asp:label>
            <asp:DropDownList ID="ResourceDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ResourceDDL_SelectedIndexChanged">
                <asp:ListItem>Employee</asp:ListItem>
                <asp:ListItem Selected=True>Location</asp:ListItem>
            </asp:DropDownList>

            &nbsp &nbsp <asp:label ID="StartDateLabel" runat="server" Font-Size="Larger" Font-Bold="true">Start Date:</asp:label> <asp:TextBox ID="StartDateTB" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="true" TextMode="Date" ViewState="enabled" OnTextChanged="StartDateTB_OnTextChanged"/>

            &nbsp &nbsp <asp:label ID="DaysLabel" runat="server" Font-Size="Larger" Font-Bold="true" Visible="true" >Days:</asp:label>
            <asp:DropDownList ID="DaysDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="DaysDDL_SelectedIndexChanged" Visible="true">
                <asp:ListItem>1</asp:ListItem>
                <asp:ListItem>2</asp:ListItem>
                <asp:ListItem>3</asp:ListItem>
                <asp:ListItem>4</asp:ListItem>
                <asp:ListItem>5</asp:ListItem>
                <asp:ListItem>6</asp:ListItem>
                <asp:ListItem Selected=True>7</asp:ListItem>
                <asp:ListItem>8</asp:ListItem>
                <asp:ListItem>9</asp:ListItem>
            </asp:DropDownList>

            &nbsp &nbsp <asp:label ID="CellDurationLabel" runat="server" Font-Size="Larger" Font-Bold="true" Visible="false">Cell Duration (Minutes):</asp:label>
            <asp:DropDownList ID="CellDurationDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="CellDurationDDL_SelectedIndexChanged" Visible="false">
                <asp:ListItem Selected=True>1440</asp:ListItem>
            </asp:DropDownList>

            &nbsp &nbsp <asp:label ID="CellWidthLabel" runat="server" Font-Size="Larger" Font-Bold="true" >Cell Width (Pixels):</asp:label>
            <asp:DropDownList ID="CellWidthDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="CellWidthDDL_SelectedIndexChanged" >
                <asp:ListItem>100</asp:ListItem>
                <asp:ListItem>150</asp:ListItem>
                <asp:ListItem>200</asp:ListItem>
                <asp:ListItem Selected=True>250</asp:ListItem>
                <asp:ListItem>300</asp:ListItem>
                <asp:ListItem>350</asp:ListItem>
                <asp:ListItem>400</asp:ListItem>
                <asp:ListItem>500</asp:ListItem>
                <asp:ListItem>600</asp:ListItem>
                <asp:ListItem>700</asp:ListItem>
                <asp:ListItem>800</asp:ListItem>
                <asp:ListItem>900</asp:ListItem>
                <asp:ListItem>1000</asp:ListItem>
                <asp:ListItem>1100</asp:ListItem>
                <asp:ListItem>1200</asp:ListItem>

            </asp:DropDownList>

             &nbsp &nbsp <asp:label ID="EventHeightLabel" runat="server" Font-Size="Larger" Font-Bold="true" Visible="False">Event Height (Pixels):</asp:label>
             <asp:DropDownList ID="EventHeightDDL" Font-Size="Larger" Font-Bold="true" runat="server" AutoPostBack="True" OnSelectedIndexChanged="EventHeightDDL_SelectedIndexChanged" Visible="false">
                 <asp:ListItem Selected=True>20</asp:ListItem>
             </asp:DropDownList>

            
            &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp <asp:label ID="ShowTimesLabel" runat="server" Font-Size="Larger" Font-Bold="true" Text="Show Times: "  Visible="true"></asp:label><asp:ImageButton runat="server" ID="ShowTimes_ImageButton" OnClick="ShowTimesEyeBall_Click" Visible="true" AlternateText="Show Times" ToolTip="Show Times" ImageUrl="../images/eyeball.png" Width="18" Height="18"></asp:ImageButton>
            &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp <asp:label ID="EventWidthLabel" runat="server" Font-Size="Larger" Font-Bold="true" Text="Event Width: "  Visible="true"></asp:label><asp:ImageButton runat="server" ID="EventWidthMatters_ImageButton" OnClick="EventWidthMatters_RedGreen_Click" Visible="true" AlternateText="Full Cell Width" ToolTip="Full Cell Width" ImageUrl="../images/green.png" Width="15" Height="15"></asp:ImageButton>

        <br /><br />
    </div>

                <DayPilot:DayPilotScheduler ID="DayPilotScheduler"
                       DataTextField="employeename"
                       DataIdField="id"
                       DataStartField="start"
                       DataEndField="end"
                       NonBusinessHours="Show"
                       DataResourceField="siteid"
                       EventFontSize="11pt"
                       CellDuration="1440" 
                       CellWidth="250"
                       Days="7" 
                       EventHeight="25"
                       OnBeforeEventRender="DayPilotScheduler_BeforeEventRender"
                       EventClickHandling="PostBack"
                       OnEventClick="DayPilotScheduler_EventClick"
                       EventClickJavaScript="alert('{0}');"
                       runat="server" HeightSpec="Auto" ShowEventStartEnd="True" BusinessBeginsHour="0" BusinessEndsHour="24" EventBackColor="White" ShowHours="True" EnableTheming="True" BorderWidth="1" BorderColor="Black" BackColor="White" EventBorderColor="Black" DurationBarColor="#33CCFF" BorderStyle="Solid" CssOnly="False" DataValueField="sitename" Height="20" TimeFormat="Clock12Hours" RowHeaderWidth="0" RowHeaderColumnWidths="250" HourFontSize="14pt" HourBorderColor="Black" Font-Bold="True" Font-Size="Larger" EventFontFamily="Veranda" HeaderFontFamily="Veranda" HeaderFontSize=1pt HeaderHeight="25" HourFontFamily="Veranda" CssClassPrefix="scheduler_traditional" TimeRangeSelectedHandling="JavaScript" CssClass="scheduler_traditional" Theme="scheduler_traditional" HoverColor="White" />



        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
       <br />





            <hr />
            	<h1 class=page-header><b>Payroll</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:label id="payroll_date_range_start" ForeColor="#8B0000" font-bold="false" Font-Size="Smaller" runat="server" />&nbsp;<asp:Label runat="server" id="hyphen" Text="-" Visible="false" />&nbsp;<asp:label id="payroll_date_range_end" ForeColor="#8B0000" font-bold="false" Font-Size="Smaller" runat="server" />               &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="createMattsFlexFileButton" runat="server" CssClass="btn-sm btn btn-danger" font-bold="true" Text="Create Matt's Flex File" OnClick="CreateMattsFlexFile" Visible="false"/>&nbsp;&nbsp;<asp:Button ID="createExcaliburFlexFileButton" runat="server" CssClass="btn-sm btn btn-danger" font-bold="true" Text="Create Excalibur Flex File" OnClick="CreateExcaliburFlexFile" Visible="false" /> &nbsp;&nbsp;<asp:Button ID="PTOButton" runat="server" font-bold="true" CssClass="btn-sm btn btn-danger" Text="Create PTO FIle" OnClick="CreatePTOFile" Visible="false" /> </h1> 
                    <asp:Label runat="server" Font-Size="Larger" Font-Bold="true">Week Of:</asp:Label>
                    <asp:TextBox ID="weekOfDate" runat="server" TextMode="Date" Font-Size="Larger" Font-Bold="true" onkeydown="return false" AutoPostBack="true" OnTextChanged="weekOfChanged" Visible="true"></asp:TextBox>



                <br /><h3><b>Client Totals Table &nbsp;</b><asp:ImageButton runat="server" ID="RefreshClients_ImageButton" OnClick="CalculateClientTotals_ButtonClick" Visible="true" ImageUrl="../images/refresh.png" Width="18" Height="18"></asp:ImageButton>&nbsp;&nbsp;&nbsp;<asp:ImageButton runat="server" ID="ClientEyeBall_ImageButton" OnClick="ClientEyeBall_Click" Visible="true" ImageUrl="../images/eyeball.png" Width="18" Height="18"></asp:ImageButton></h3><hr />
                     <div class="scroll"><asp:GridView ID="ClientTotalsGridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="80%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False" PagerSettings-Mode="Numeric" EmptyDataText="Click Refresh." PageSize="3" Visible="true">
                         <Columns>
                           <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True" ItemStyle-Wrap="false" ItemStyle-BackColor="#ffeeff" HeaderStyle-BackColor="#fedefd" />
                           <asp:BoundField DataField="clientsite" HeaderText="Client Site" ReadOnly="True" ItemStyle-BackColor="#ffeeff" ItemStyle-Wrap="false" HeaderStyle-BackColor="#fedefd" ItemStyle-Font-Bold="True" SortExpression="employee" />
                           <asp:BoundField DataField="rate" visible="false" HeaderText="Rate" ReadOnly="True" ItemStyle-BackColor="#ffeeff" HeaderStyle-BackColor="#fedefd" />
                           <asp:BoundField DataField="client_cost" visible="false" HeaderText="Rev. (Hrs)" ReadOnly="True" ItemStyle-BackColor="#dfffe2" HeaderStyle-BackColor="#c4ffc7" ItemStyle-Font-Bold="True" ItemStyle-HorizontalAlign="Right"  />
                           <asp:BoundField DataField="rt_total" HeaderText="RT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="ot_total" HeaderText="OT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="dt_total" HeaderText="DT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="all_total" HeaderText="Total (Hrs)" ReadOnly="True" ItemStyle-Font-Bold="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="mondaytime" HeaderText="Mon (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="tuesdaytime" HeaderText="Tue (Hrs)"  ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="wednesdaytime" HeaderText="Wed (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="thursdaytime" HeaderText="Thur (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="fridaytime" HeaderText="Fri (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="saturdaytime" HeaderText="Sat (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                           <asp:BoundField DataField="sundaytime" HeaderText="Sun (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                         </Columns>
                       <PagerStyle Width="15px" />
                   </asp:GridView></div>
                     <br /><h3><b>Employee Totals Table &nbsp;</b><asp:ImageButton runat="server" ID="RefreshEmployees_ImageButton" OnClick="CalculateEmployeeTotals_ButtonClick" Visible="true" ImageUrl="../images/refresh.png" Width="18" Height="18"></asp:ImageButton>&nbsp;&nbsp;&nbsp;<asp:ImageButton runat="server" ID="EmployeeEyeBall_ImageButton" OnClick="EmployeeEyeBall_Click" Visible="true" ImageUrl="../images/eyeball.png" Width="18" Height="18"></asp:ImageButton></h3><hr />
                     <div class="scroll"><asp:GridView ID="EmployeeTotalsGridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False"  PagerSettings-Mode="Numeric" EmptyDataText="Click Refresh." PageSize="3" Visible="true">
                         <Columns>
                             <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True" ItemStyle-Wrap="false" ItemStyle-BackColor="#ffeeff" HeaderStyle-BackColor="#fedefd" />
                             <asp:BoundField DataField="ex_ms_id" HeaderText="ID"  ReadOnly="True" ItemStyle-BackColor="#ffeeff" HeaderStyle-BackColor="#fedefd" />
                             <asp:BoundField DataField="employee" HeaderText="Employee" ReadOnly="True" ItemStyle-BackColor="#ffeeff" ItemStyle-Wrap="false" HeaderStyle-BackColor="#fedefd" ItemStyle-Font-Bold="True" SortExpression="employee" />
                             <asp:BoundField DataField="phone" HeaderText="Phone" ReadOnly="True" ItemStyle-BackColor="#ffeeff" HeaderStyle-BackColor="#fedefd" ItemStyle-Wrap="false" />
                             <asp:BoundField DataField="rate" HeaderText="Pay Rate" ReadOnly="True"  ItemStyle-BackColor="#ffeeff" HeaderStyle-BackColor="#fedefd" ItemStyle-Font-Bold="True" />
                             <asp:BoundField DataField="paycheck_amount" HeaderText="PayCheck Total" ReadOnly="True" ItemStyle-BackColor="#dfffe2" ItemStyle-Font-Bold="True" HeaderStyle-BackColor="#c4ffc7" SortExpression="paycheck_amount" ItemStyle-HorizontalAlign="Right" />
                             <asp:BoundField DataField="mondaytime" HeaderText="Mon (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="tuesdaytime" HeaderText="Tue (Hrs)"  ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="wednesdaytime" HeaderText="Wed (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="thursdaytime" HeaderText="Thu (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="fridaytime" HeaderText="Fri (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="saturdaytime" HeaderText="Sat (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="sundaytime" HeaderText="Sun (Hrs)" ReadOnly="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="all_total" HeaderText="Total (Hrs)" ReadOnly="True" ItemStyle-Font-Bold="True" HeaderStyle-BackColor="#ccffff" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="rt_total" HeaderText="RT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="ht_total" HeaderText="HT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="ot_total" HeaderText="OT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="dt_total" HeaderText="DT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="st_total" HeaderText="ST" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="reimb_rt_total" HeaderText="Net Reimb RT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="reimb_ht_total" HeaderText="Net Reimb HT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="reimb_ot_total" HeaderText="Net Reimb OT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="reimb_dt_total" HeaderText="Net Reimb DT" ReadOnly="True" HeaderStyle-BackColor="#ffff80" ItemStyle-HorizontalAlign="Center" />
                             <asp:BoundField DataField="gross_add_ddctn" HeaderText="Gross Add/Ddcn" ReadOnly="True" ItemStyle-BackColor="#dfffe2" ItemStyle-Font-Bold="True" HeaderStyle-BackColor="#c4ffc7" ItemStyle-HorizontalAlign="Right" />
                             <asp:BoundField DataField="net_add_ddctn" HeaderText="Net Add/Ddcn" ReadOnly="True" ItemStyle-BackColor="#dfffe2" ItemStyle-Font-Bold="True" HeaderStyle-BackColor="#c4ffc7" ItemStyle-HorizontalAlign="Right" />
                             </Columns>
                         <PagerStyle Width="15px" />
                     </asp:GridView></div>
                     <br />





               <h3>Reimbursements (+) / Deductions (-)</h3>
               <h6>Phone and Internet Use Automatically Included in Flex File</h6>
                 <asp:GridView ID="ReimbursementsGridView" runat="server" OnRowDeleting="Reimbursements_OnRowDeleting" OnRowUpdating="Reimbursements_OnRowUpdating" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="reimbursement_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource1" PagerSettings-Mode="Numeric" EmptyDataText="" PageSize="30" Visible="true" >
                     <Columns>
                         <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" ShowInsertButton="False" HeaderText="Tools" />
                         <asp:BoundField DataField="username" HeaderText="Employee" ReadOnly="True" />
                         <asp:BoundField DataField="datetime" HeaderText="Date"  />
                         <asp:BoundField DataField="description" HeaderText="Description"  />
                         <asp:BoundField DataField="amount" HeaderText="Amount (+/-)" />
                         <asp:BoundField DataField="is_gross_taxablewage" HeaderText="Is Gross Taxable Wage (HR compliance messaging, bonus/hour)"   />
                       </Columns>
                     <PagerStyle Width="15px" />
                 </asp:GridView>
                    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT reimbursement_id, username, amount, description, datetime, is_gross_taxablewage FROM reimbursement_tbl" DeleteCommand="DELETE FROM reimbursement_tbl WHERE reimbursement_id=@reimbursement_id" UpdateCommand="UPDATE reimbursement_tbl SET amount=@amount, description=@description, datetime=@datetime, is_gross_taxablewage=@is_gross_taxablewage WHERE reimbursement_id=@reimbursement_id" FilterExpression="datetime >='{0}' AND datetime <='{1}'" >
                    <FilterParameters>
                        <asp:ControlParameter Name="datetime" ControlID="payroll_date_range_start" />
                        <asp:ControlParameter Name="datetime" ControlID="payroll_date_range_end" />
                    </FilterParameters>
                    </asp:SqlDataSource>
                         <asp:table ID="Reimbursement_AddTable" runat="server" class="table table-striped table-bordered table-hover" Font-Size="Smaller" Font-Bold="False" Visible="true" >
                                <asp:tablerow>
                                    <asp:tablecell><asp:LinkButton ID="linkButton" runat="server" Text="Add New Row" OnClick="Insert_Reimbursement" Font-Size="Larger" CssClass="" ValidationGroup="btnAddReimbursement"/></asp:tablecell>
                                    <asp:tablecell>Employee: <asp:DropDownList ID="Reimburse_EmployeeDDL" runat="server" ></asp:DropDownList></asp:tablecell>
                                    <asp:tablecell>Date: <asp:TextBox ID="Reimburse_DateTB" runat="server" TextMode="Date" /></asp:tablecell>
                                    <asp:tablecell>Description: <asp:TextBox ID="Reimburse_DescriptionTB" runat="server" /></asp:tablecell>
                                    <asp:tablecell>Amount (+/-): <asp:TextBox ID="Reimburse_AmountTB" runat="server" /></asp:tablecell>
                                    <asp:tablecell>Is Gross Taxable Wage?  <asp:CheckBox ID="Reimburse_GrossCB"  runat="server" /></asp:tablecell>
                                    <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" foreColor="red" ErrorMessage="*Employee" ValidationGroup="btnAddReimbursement" ControlToValidate="Reimburse_EmployeeDDL"></asp:RequiredFieldValidator> <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" foreColor="red" ErrorMessage="*Date" ValidationGroup="btnAddReimbursement" ControlToValidate="Reimburse_DateTB"></asp:RequiredFieldValidator><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" foreColor="red" ErrorMessage="*Description" ValidationGroup="btnAddReimbursement" ControlToValidate="Reimburse_DescriptionTB"></asp:RequiredFieldValidator><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" foreColor="red" ErrorMessage="*Amount" ValidationGroup="btnAddReimbursement" ControlToValidate="Reimburse_AmountTB"></asp:RequiredFieldValidator><asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Currency" ControlToValidate="Reimburse_AmountTB" foreColor="red" ErrorMessage="$$.$$" /></asp:tablecell>
                                </asp:tablerow>
                          </asp:table> 
                        <br />


                <h3>Sick Pay Requests</h3>
               <asp:GridView ID="SickPayGridView" runat="server" OnRowDeleting="SickPay_OnRowDeleting" OnRowUpdating="SickPay_OnRowUpdating" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="sickpay_request_id" PagerSettings-Mode="Numeric" PageSize="30" DataSourceID="SqlDataSource2" Visible="true">
                   <Columns>
                          <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" ShowInsertButton="False" HeaderText="Tools" />
                          <asp:BoundField DataField="username" HeaderText="Employee" ReadOnly="True" />
                          <asp:BoundField DataField="datetime" HeaderText="Description"  />
                          <asp:BoundField DataField="description" HeaderText="Description"   />
                          <asp:BoundField DataField="minutes_requested" HeaderText="# of Minutes Requested"  />
                        </Columns>
                      <PagerStyle Width="15px" />
                  </asp:GridView>
                  <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT sickpay_request_id, username, minutes_requested, description, datetime FROM sickpay_request_tbl" DeleteCommand="DELETE FROM sickpay_request_tbl WHERE sickpay_request_id=@sickpay_request_id" UpdateCommand="UPDATE sickpay_request_tbl SET datetime=@datetime, minutes_requested=@minutes_requested, description=@description WHERE sickpay_request_id=@sickpay_request_id" FilterExpression="datetime >='{0}' AND datetime <='{1}'">
                    <FilterParameters>
                        <asp:ControlParameter Name="datetime" ControlID="payroll_date_range_start" />
                        <asp:ControlParameter Name="datetime" ControlID="payroll_date_range_end" />
                    </FilterParameters>
                    </asp:SqlDataSource>
                          <asp:table ID="SickPay_AddTable" runat="server" class="table table-striped table-bordered table-hover" Font-Size="Smaller" Font-Bold="False" Visible="true" >
                               <asp:tablerow>
                                   <asp:tablecell><asp:LinkButton ID="linkButton1" runat="server" Text="Add New Row" OnClick="Insert_SickRequest" Font-Size="Larger" CssClass="" ValidationGroup="btnAddSickPay"/></asp:tablecell>
                                   <asp:tablecell>Employee: <asp:DropDownList ID="Sick_EmployeeDDL" runat="server" ></asp:DropDownList></asp:tablecell>
                                   <asp:tablecell>Date: <asp:TextBox ID="Sick_DateTB" runat="server" TextMode="Date" /></asp:tablecell>
                                   <asp:tablecell>Reason: <asp:TextBox ID="Sick_DescriptionTB" runat="server" /></asp:tablecell>
                                   <asp:tablecell>Minutes Requested (8hr = 480min): <asp:TextBox ID="Sick_MinutesTB" runat="server" /></asp:tablecell>
                                   <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" foreColor="red" ErrorMessage="*Employee" ValidationGroup="btnAddSickPay" ControlToValidate="Sick_EmployeeDDL"></asp:RequiredFieldValidator> <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" foreColor="red" ErrorMessage="*Date" ValidationGroup="btnAddSickPay" ControlToValidate="Sick_DateTB"></asp:RequiredFieldValidator><asp:RequiredFieldValidator ID=RequiredFieldValidator8 runat="server" foreColor="red" ErrorMessage="*Reason" ValidationGroup="btnAddSickPay" ControlToValidate="Sick_DescriptionTB"></asp:RequiredFieldValidator><asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" foreColor="red" ErrorMessage="*Minutes" ValidationGroup="btnAddSickPay" ControlToValidate="Sick_MinutesTB"></asp:RequiredFieldValidator><asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Integer" ControlToValidate="Sick_MinutesTB" foreColor="red" ErrorMessage="*WholeNumber" /></asp:tablecell>
                               </asp:tablerow>
                         </asp:table> 
                                <br />




 













                      <asp:GridView ID="ExcaliburFlexGridView" runat="server" visible="false" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False"  PagerSettings-Mode="Numeric" EmptyDataText="Select Employee and Week." PageSize="3" >
                         <Columns>
                             <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True"/>
                             <asp:BoundField DataField="workerid" HeaderText="Worker ID" ReadOnly="True" />
                             <asp:BoundField DataField="_org" HeaderText="Org" ReadOnly="True"/>
                             <asp:BoundField DataField="_jobnumber" HeaderText="Job Number"  ReadOnly="True" />
                             <asp:BoundField DataField="paycomponent" HeaderText="Pay Component"  ReadOnly="True" />
                             <asp:BoundField DataField="rate" HeaderText="Rate"  ReadOnly="True" />
                             <asp:BoundField DataField="_ratenumber" HeaderText="Rate Number"  ReadOnly="True" />
                             <asp:BoundField DataField="hours" HeaderText="Hours"  ReadOnly="True" />
                             <asp:BoundField DataField="_units" HeaderText="Units"  ReadOnly="True" />
                             <asp:BoundField DataField="_linedate" HeaderText="Line Date"  ReadOnly="True" />
                             <asp:BoundField DataField="amount" HeaderText="Amount"  ReadOnly="True" />
                             <asp:BoundField DataField="_checkseqnumber" HeaderText="Check Seq Number"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridestate" HeaderText="Override State"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridelocal" HeaderText="Override Local"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridelocaljurisdiction" HeaderText="Override Local Jurisdiction"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridelabor" HeaderText="Labor Override"  ReadOnly="True" />
                           </Columns>
                         <PagerStyle Width="15px" />
                     </asp:GridView>
                            <asp:GridView ID="MattsFlexGridView" runat="server" visible="false" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False"  PagerSettings-Mode="Numeric" EmptyDataText="Select Employee and Week." PageSize="3" >
                             <Columns>
                                 <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True"/>
                                 <asp:BoundField DataField="workerid" HeaderText="Worker ID" ReadOnly="True" />
                                 <asp:BoundField DataField="_org" HeaderText="Org" ReadOnly="True" />
                                 <asp:BoundField DataField="_jobnumber" HeaderText="Job Number"  ReadOnly="True" />
                                 <asp:BoundField DataField="paycomponent" HeaderText="Pay Component"  ReadOnly="True" />
                                 <asp:BoundField DataField="rate" HeaderText="Rate"  ReadOnly="True" />
                                 <asp:BoundField DataField="_ratenumber" HeaderText="Rate Number"  ReadOnly="True" />
                                 <asp:BoundField DataField="hours" HeaderText="Hours"  ReadOnly="True" />
                                 <asp:BoundField DataField="_units" HeaderText="Units"  ReadOnly="True" />
                                 <asp:BoundField DataField="_linedate" HeaderText="Line Date"  ReadOnly="True" />
                                 <asp:BoundField DataField="amount" HeaderText="Amount"  ReadOnly="True" />
                                 <asp:BoundField DataField="_checkseqnumber" HeaderText="Check Seq Number"  ReadOnly="True" />
                                 <asp:BoundField DataField="_overridestate" HeaderText="Override State"  ReadOnly="True" />
                                 <asp:BoundField DataField="_overridelocal" HeaderText="Override Local"  ReadOnly="True" />
                                 <asp:BoundField DataField="_overridelocaljurisdiction" HeaderText="Override Local Jurisdiction"  ReadOnly="True" />
                                 <asp:BoundField DataField="_overridelabor" HeaderText="Labor Override"  ReadOnly="True" />
                               </Columns>
                             <PagerStyle Width="15px" />
                         </asp:GridView>
                       <asp:GridView ID="PTOGridView" runat="server" visible="false" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False" PagerSettings-Mode="Numeric" EmptyDataText="Select Employee and Week." PageSize="3">
                         <Columns>
                             <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True"/>
                             <asp:BoundField DataField="workerid" HeaderText="Worker ID" ReadOnly="True" />
                             <asp:BoundField DataField="_org" HeaderText="Org" ReadOnly="True"/>
                             <asp:BoundField DataField="_jobnumber" HeaderText="Job Number"  ReadOnly="True" />
                             <asp:BoundField DataField="paycomponent" HeaderText="Pay Component"  ReadOnly="True" />
                             <asp:BoundField DataField="rate" HeaderText="Rate"  ReadOnly="True" />
                             <asp:BoundField DataField="_ratenumber" HeaderText="Rate Number"  ReadOnly="True" />
                             <asp:BoundField DataField="hours" HeaderText="Hours"  ReadOnly="True" />
                             <asp:BoundField DataField="_units" HeaderText="Units"  ReadOnly="True" />
                             <asp:BoundField DataField="_linedate" HeaderText="Line Date"  ReadOnly="True" />
                             <asp:BoundField DataField="amount" HeaderText="Amount"  ReadOnly="True" />
                             <asp:BoundField DataField="_checkseqnumber" HeaderText="Check Seq Number" ReadOnly="True" />
                             <asp:BoundField DataField="_overridestate" HeaderText="Override State"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridelocal" HeaderText="Override Local"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridelocaljurisdiction" HeaderText="Override Local Jurisdiction"  ReadOnly="True" />
                             <asp:BoundField DataField="_overridelabor" HeaderText="Labor Override"  ReadOnly="True" />
                           </Columns>
                         <PagerStyle Width="15px" />
                     </asp:GridView>
                                <br />

























      </div>
     </div>

        <div class="container" >
            <div class="modal fade" id="myshiftmodal" data-backdrop="false" role="dialog">
                <div class=" modal-dialog modal-dailog-centered">
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
                                    <asp:TextBox ID="ShiftTemplate_ShiftDescription" CssClass="form-control" TextMode="MultiLine" Rows="1" runat="server" />
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