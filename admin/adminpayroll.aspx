<%@ Page Language="vb" AutoEventWireup="false" MaintainScrollPositionOnPostback="true" CodeBehind="adminpayroll.aspx.vb" Inherits="EAS.adminpayroll" EnableEventValidation="True" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Payroll</title>
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
        <!-- #include file="adminnavfixedpayroll.html"-->
        <br />
       <div id="page-wrapper">
           <div class="col-lg-12">
			<h1 class=page-header><b>Payroll</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:label id="payroll_date_range_start" ForeColor="#8B0000" font-bold="false" Font-Size="Smaller" runat="server" />&nbsp;<asp:Label runat="server" id="hyphen" Text="-" Visible="false" />&nbsp;<asp:label id="payroll_date_range_end" ForeColor="#8B0000" font-bold="false" Font-Size="Smaller" runat="server" />               &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="createMattsFlexFileButton" runat="server" CssClass="btn-sm btn btn-danger" font-bold="true" Text="Create Matt's Flex File" OnClick="CreateMattsFlexFile" Visible="false"/>&nbsp;&nbsp;<asp:Button ID="createExcaliburFlexFileButton" runat="server" CssClass="btn-sm btn btn-danger" font-bold="true" Text="Create Excalibur Flex File" OnClick="CreateExcaliburFlexFile" Visible="false" /> &nbsp;&nbsp;<asp:Button ID="PTOButton" runat="server" font-bold="true" CssClass="btn-sm btn btn-danger" Text="Create PTO FIle" OnClick="CreatePTOFile" Visible="false" /> </h1> 
        <asp:Label runat="server" Font-Size="Larger" Font-Bold="true">Week Of:</asp:Label>
        <asp:TextBox ID="weekOfDate" runat="server" TextMode="Date" Font-Size="Larger" Font-Bold="true" onkeydown="return false" AutoPostBack="true" OnTextChanged="weekOfChanged" Visible="true"></asp:TextBox>
            <br />



               <!--<br /><h3><b> &nbsp;</b><asp:ImageButton runat="server" ID="CalculateTotals_Button" OnClick="CalculateCompanyTotals_ButtonClick" Visible="false" ImageUrl="../images/refresh.png" Width="18" Height="18"></asp:ImageButton></h3>-->
                 <asp:GridView ID="ExcaliburTotalsGridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False"  PagerSettings-Mode="Numeric" EmptyDataText="Select Employee and Week." PageSize="3" Visible="false">
                    <Columns>
                        <asp:BoundField DataField="rowtitle" HeaderText="Excalibur" ReadOnly="True" ItemStyle-Font-Bold="True"/>
                        <asp:BoundField DataField="mondaytime" HeaderText="M" ReadOnly="True"/>
                        <asp:BoundField DataField="tuesdaytime" HeaderText="Tu"  ReadOnly="True" />
                        <asp:BoundField DataField="wednesdaytime" HeaderText="W" ReadOnly="True" />
                        <asp:BoundField DataField="thursdaytime" HeaderText="Th" ReadOnly="True"  />
                        <asp:BoundField DataField="fridaytime" HeaderText="F" ReadOnly="True" />
                        <asp:BoundField DataField="saturdaytime" HeaderText="Sa" ReadOnly="True" />
                        <asp:BoundField DataField="sundaytime" HeaderText="Su" ReadOnly="True"  />
                        <asp:BoundField DataField="all_total" HeaderText="Total" ReadOnly="True" />
                        <asp:BoundField DataField="rt_total" HeaderText="RT" ReadOnly="True"  />
                        <asp:BoundField DataField="ht_total" HeaderText="HT" ReadOnly="True"  />
                        <asp:BoundField DataField="ot_total" HeaderText="OT" ReadOnly="True"  />
                        <asp:BoundField DataField="dt_total" HeaderText="DT" ReadOnly="True"  />
                        <asp:BoundField DataField="totaldollars" HeaderText="$ Total" ReadOnly="True"  />
                      </Columns>
                    <PagerStyle Width="15px" />
                </asp:GridView>
                       <asp:GridView ID="MattsTotalsGridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" AllowSorting="False" AllowPaging="False"  PagerSettings-Mode="Numeric" EmptyDataText="Select Employee and Week." PageSize="3" Visible="false">
                        <Columns>
                            <asp:BoundField DataField="rowtitle" HeaderText="Matt's Staffing" ReadOnly="True" ItemStyle-Font-Bold="True"/>
                            <asp:BoundField DataField="mondaytime" HeaderText="M" ReadOnly="True"/>
                            <asp:BoundField DataField="tuesdaytime" HeaderText="Tu"  ReadOnly="True" />
                            <asp:BoundField DataField="wednesdaytime" HeaderText="W" ReadOnly="True" />
                            <asp:BoundField DataField="thursdaytime" HeaderText="Th" ReadOnly="True"  />
                            <asp:BoundField DataField="fridaytime" HeaderText="F" ReadOnly="True" />
                            <asp:BoundField DataField="saturdaytime" HeaderText="Sa" ReadOnly="True" />
                            <asp:BoundField DataField="sundaytime" HeaderText="Su" ReadOnly="True"  />
                            <asp:BoundField DataField="all_total" HeaderText="Total" ReadOnly="True" />
                            <asp:BoundField DataField="rt_total" HeaderText="RT" ReadOnly="True"  />
                            <asp:BoundField DataField="ht_total" HeaderText="HT" ReadOnly="True"  />
                            <asp:BoundField DataField="ot_total" HeaderText="OT" ReadOnly="True"  />
                            <asp:BoundField DataField="dt_total" HeaderText="DT" ReadOnly="True"  />
                            <asp:BoundField DataField="totaldollars" HeaderText="$ Total" ReadOnly="True"  />
                          </Columns>
                        <PagerStyle Width="15px" />
                    </asp:GridView>
                            <br />





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




                     <br />





               <asp:Label runat="server" Font-Size="XX-Large" Font-Bold="true">Shift Instances</asp:Label>
                <asp:Label runat="server" Font-Size="Larger" Font-Bold="true">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Filter By:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Employee:</asp:Label>
                    <asp:DropDownList ID="employee_ddl" runat="server" Font-Size="Larger" Font-Bold="true" OnTextChanged="employee_ddl_textChanged" AutoPostBack="true" ></asp:DropDownList>
                <asp:Label runat="server" Font-Size="Larger" Font-Bold="true">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Client Site:</asp:Label>
                    <asp:DropDownList ID="client_ddl" runat="server" Font-Size="Larger" Font-Bold="true" OnTextChanged="client_ddl_textChanged" AutoPostBack="true" ></asp:DropDownList><hr />
               <asp:GridView ID="ShiftInstanceGridView" EnableViewState="true" HeaderStyle-BackColor="#d9ffff" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="client_site_shift_instance_id" AllowSorting="True" OnSorting="ShiftInstanceGridView_Sorting" AllowPaging="False" PagerSettings-Mode="Numeric" EmptyDataText="Select Employee and Week." PageSize="20" Visible="True" >
                   <Columns>
                        <asp:TemplateField ShowHeader="False" Visible="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="ShiftInstanceGrid_UpdateLabel" runat="server" CausesValidation="True" CommandName="Cancel" Text="Update" OnClick="ButtonUpdate_Click" Visible="True"/>
                            </ItemTemplate>
                            <ItemTemplate>
                                <asp:LinkButton ID="ShiftInstanceGrid_ShowModal" runat="server" CausesValidation="True" CommandName="Cancel" Text="Edit" OnClick="ButtonEdit_Click" Visible="True"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="client_site_id" HeaderText="client_site_id"  ReadOnly="true" Visible="false" />
                        <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True" />
                        <asp:BoundField DataField="name" HeaderText="Location" ReadOnly="True" ItemStyle-Font-Bold="true" />
                        <asp:TemplateField HeaderText="Employees" HeaderStyle-ForeColor="#cc0099" ItemStyle-Font-Bold="true" ItemStyle-BackColor="#ffeeff" >
                            <ItemTemplate>
                                <asp:DropDownList ID="Username_DDL" runat="server" Font-Bold="true" BackColor="#fedefd" ReadOnly="false" OnSelectedIndexChanged="Username_DLL_SelectedIndexChanged" AutoPostBack="True" ></asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Phone" HeaderStyle-ForeColor="#cc0099" ItemStyle-BackColor="#ffeeff" >
                            <ItemTemplate>
                                <asp:TextBox ID="Phone_TextBox" runat="server"  BorderStyle="None" BackColor="#fedefd" Width="90" MaxLength="19" ReadOnly="false" OnTextChanged="Phone_OnTextChanged" AutoPostBack="True" Text='<%# Bind("cellphone") %>' ></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Ends On" Visible="False" >
                             <ItemTemplate>
                                 <asp:Label ID="EndsOn_Label" runat="server"  Font-Bold="true" BackColor="" ReadOnly="true" Text='<%# Bind("enddatetime", "{0:ddd}") %>'></asp:Label>
                             </ItemTemplate>
                        </asp:TemplateField>
                         <asp:TemplateField HeaderText="Starts On" >
                             <ItemTemplate>
                                 <asp:Label ID="StartsOn_Label" runat="server"  Font-Bold="true" BackColor="" ReadOnly="true" Text='<%# Bind("startdatetime", "{0:ddd}") %>'></asp:Label>
                             </ItemTemplate>
                         </asp:TemplateField>
                         <asp:TemplateField HeaderText="Start Time" ItemStyle-BackColor="#ccffcc">
                             <ItemTemplate>
                                 <asp:TextBox ID="StartTime_TextBox" runat="server"  BorderStyle="None" Font-Bold="true" BackColor="#ccffcc" TextMode="DateTimeLocal" ReadOnly="false" OnKeyDown="return false" OnPaste="return false" OnTextChanged="startdatetime_OnTextChanged" AutoPostBack="True" Text='<%# Bind("startdatetime", "{0:yyyy-MM-ddTHH:mm}") %>'></asp:TextBox>
                             </ItemTemplate>
                         </asp:TemplateField>
                         <asp:TemplateField HeaderText="End Time"  ItemStyle-BackColor="#ffb3b3">
                             <ItemTemplate>
                                 <asp:TextBox ID="EndTime_TextBox" runat="server"  BorderStyle="None" Font-Bold="true" BackColor="#ffb3b3" TextMode="DateTimeLocal" ReadOnly="false" OnKeyDown="return false" OnPaste="return false" OnTextChanged="enddatetime_OnTextChanged" AutoPostBack="True" Text='<%# Bind("enddatetime", "{0:yyyy-MM-ddTHH:mm}") %>'></asp:TextBox>
                             </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Hours in Pay Period" >
                             <ItemTemplate>
                                 <asp:Label ID="Hours_Label" runat="server" Font-Bold="true" BackColor="" ReadOnly="true" Text=""></asp:Label>
                             </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Notes" >
                            <ItemTemplate>
                                <asp:TextBox ID="Notes_TextBox" runat="server" BorderStyle="None" Font-Bold="true" BackColor="#ffffb5" MaxLength="1999" ReadOnly="false" OnTextChanged="notes_OnTextChanged" AutoPostBack="True" Text='<%# Bind("shift_notes") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="MS_ID" HeaderStyle-ForeColor="#cc0099" ItemStyle-BackColor="#ffeeff">
                            <ItemTemplate>
                                <asp:TextBox ID="ms_id_TextBox" runat="server" BorderStyle="None" BackColor="#fedefd" Width="50" MaxLength="10" ReadOnly="false" OnTextChanged="ms_id_OnTextChanged" AutoPostBack="True" Text='<%# Bind("ms_id") %>' ></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="EX_ID" HeaderStyle-ForeColor="#cc0099" ItemStyle-BackColor="#ffeeff">
                            <ItemTemplate>
                                <asp:TextBox ID="ex_id_TextBox" runat="server" BorderStyle="None" BackColor="#fedefd" Width="50" MaxLength="10" ReadOnly="false" OnTextChanged="ex_id_OnTextChanged" AutoPostBack="True" Text='<%# Bind("excalibur_id") %>' ></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="payrate" DataFormatString="{0:N}" HeaderText="Rate" ReadOnly="true" HeaderStyle-ForeColor="#cc0099" ItemStyle-BackColor="#ffeeff" />
                        <asp:BoundField DataField="username" HeaderText="Employee" ReadOnly="True" HeaderStyle-ForeColor="#cc0099" ItemStyle-BackColor="#ffeeff" />
                        <asp:BoundField DataField="onduty_mealperiods" ReadOnly="True" HeaderText="On Duty Meals" ItemStyle-Font-Bold="true" />
                        <asp:TemplateField HeaderText="1st Meal Start" >
                            <ItemTemplate>
                                <asp:TextBox ID="FirstMealStartTime_TextBox" runat="server" BorderStyle="None" TextMode="DateTimeLocal" ReadOnly="false" AutoPostBack="True" OnKeyDown="return false" OnPaste="return false" OnTextChanged="firstmeal_startdatetime_OnTextChanged" Text='<%# Bind("firstmeal_startdatetime", "{0:yyyy-MM-ddTHH:mm}") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="1st Meal End" >
                            <ItemTemplate>
                                <asp:TextBox ID="FirstMealEndTime_TextBox" runat="server" BorderStyle="None" TextMode="DateTimeLocal" ReadOnly="false" AutoPostBack="True" OnKeyDown="return false" OnPaste="return false" OnTextChanged="firstmeal_enddatetime_OnTextChanged" Text='<%# Bind("firstmeal_enddatetime", "{0:yyyy-MM-ddTHH:mm}") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="2nd Meal Start" >
                            <ItemTemplate>
                                <asp:TextBox ID="SecondMealStartTime_TextBox" runat="server" BorderStyle="None" TextMode="DateTimeLocal" ReadOnly="false" AutoPostBack="True" OnKeyDown="return false" OnPaste="return false" OnTextChanged="secondmeal_startdatetime_OnTextChanged" Text='<%# Bind("secondmeal_startdatetime", "{0:yyyy-MM-ddTHH:mm}") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="2nd Meal End" >
                            <ItemTemplate>
                                <asp:TextBox ID="SecondMealEndTime_TextBox" runat="server" BorderStyle="None" TextMode="DateTimeLocal" ReadOnly="false" AutoPostBack="True" OnKeyDown="return false" OnPaste="return false" OnTextChanged="secondmeal_enddatetime_OnTextChanged" Text='<%# Bind("secondmeal_enddatetime", "{0:yyyy-MM-ddTHH:mm}") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="client_site_shift_instance_id" HeaderText="Shift #" ReadOnly="true" />
                        <asp:BoundField DataField="employee_id" HeaderText="Employee ID" ReadOnly="true" HeaderStyle-ForeColor="#cc0099" ItemStyle-BackColor="#ffeeff"/>
                      </Columns>
                    <PagerStyle Width="15px" />
                </asp:GridView>
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









               <div class="container" >
                    <div class="modal fade" id="createmodal" data-backdrop="false" role="dialog">
                        <div class="modal-dialog modal-dailog-centered">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h4 class="modal-title"></h4>
                                    <asp:Label ID="ShiftModalLabel" Font-Size="X-Large" Font-Bold="true" Text="Site Name" runat="server" />
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                </div>
                                <div class="modal-body">
                                    <ul class="nav nav-tabs" id="tabContent">
                                        <li class="active"><a href="#shift" data-toggle="tab" onclick="">Shift</a></li>
                                    </ul>
                                    <div class="tab-content">
                                        <div class="tab-pane active" id="shift">
                                            <asp:TextBox id="ShiftInstance_ID" runat="server" visible="false" Text="" />
                                            <asp:TextBox id="Shift_ID" runat="server" visible="false" Text="" />
                                            <br />
                                            <label>Shift Name</label>
                                            <asp:TextBox ID="ShiftInstance_Name" CssClass="form-control" runat="server" />
                                            <br />
                                            <label>Employee Assigned</label>
                                            <asp:DropDownList ID="ShiftInstance_EmployeeDDL" CssClass="form-control" runat="server">
                                            </asp:DropDownList>
                                            <br />
                                            <label>Shift Notes</label>
                                            <asp:TextBox ID="ShiftInstance_ShiftNotes" CssClass="form-control" TextMode="MultiLine" Rows="4" runat="server" />
                                            <br />
                                            <label>Pay Rate: </label>
                                            <asp:TextBox ID="ShiftInstance_PayRate" enabled="false" runat="server" />
                                            &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                            <label>On Duty Meals: </label>
                                            <asp:CheckBox ID="ShiftInstance_OnDutyMealPeriods" enabled="false" runat="server" />
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
                                            <asp:Button ID="UpdateShift_Button" CssClass="btn btn-primary" OnClick="UpdateShift_Click" Text="Update Shift" runat="server" style="position:relative; float:right; margin-right:20px;"/>
                                            <button type="button" class="btn btn-danger" data-dismiss="modal" style="position:relative; float:right; margin-right:20px;">Close</button>
                                        </div> 
                                        <div class="modal-footer">

                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>









       </div>
    </form>
</body>



</html>

