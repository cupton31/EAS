<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="adminclientsite.aspx.vb" Inherits="EAS.adminclientsite" EnableEventValidation="True" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Client Sites</title>
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
    <form id="form1" runat="server" autocomplete="off">
    <div id="wrapper">
        <!-- #include file="adminnavfixedclientsite.html"-->
        <br />
       <div id="page-wrapper">
           <div class="col-lg-12">
		    <h3 class="page-header">Clients Sites</h3>
            <label>Select Client:  </label>
               <asp:DropDownList ID="client_ddl" runat="server" DataTextField="clientname" DataValueField="client__id" onselectedindexchanged="client_ddl_SelectedIndexChanged" AutoPostBack="true"/>
            <br />
            <br />
               <asp:GridView ID="GridView1" runat="server" OnRowDeleting="ClientSite_OnRowDeleting" OnRowUpdating="ClientSite_OnRowUpdating" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="client_site_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource1" PagerSettings-Mode="Numeric" EmptyDataText="No records has been added." PageSize="10" >
                <Columns>
                        <asp:BoundField DataField="client_site_id" HeaderText="Site ID #" InsertVisible="False" ReadOnly="True" Visible="True" /> 
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="False" ShowInsertButton="False" HeaderText="Tools" />
                        <asp:BoundField DataField="client_id" HeaderText="Client ID" Readonly="true" InsertVisible="False" />
                        <asp:BoundField DataField="name" HeaderText="Site Name" InsertVisible="True" />
                        <asp:BoundField DataField="payrate" HeaderText="Billable Rate" InsertVisible="True" />
                        <asp:BoundField DataField="status" HeaderText="Status (Active/Inactive)" InsertVisible="True" />
                        <asp:BoundField DataField="company" HeaderText="Company" InsertVisible="True" />
                        <asp:BoundField DataField="manager_email" HeaderText="Manager Email" InsertVisible="True" />
                        <asp:BoundField DataField="cc_emails_semicolon_separated" HeaderText="CC Emails (1;2;3)" InsertVisible="True" />
                        <asp:BoundField DataField="onduty_mealperiods" HeaderText="On Duty Meals" InsertVisible="True" />
                        <asp:BoundField DataField="timesheetdata_oninvoice" HeaderText="TimeSheet Data on Invoice" InsertVisible="True" />
                        <asp:BoundField DataField="supervisor_sheets" HeaderText="Supervisor Sheets" InsertVisible="True" />
                        <asp:BoundField DataField="streetaddress" HeaderText="Street Address" InsertVisible="True"/>
                        <asp:BoundField DataField="city" HeaderText="City" InsertVisible="True"/>
                        <asp:BoundField DataField="zip" HeaderText="Zip" InsertVisible="True"/>
                        <asp:BoundField DataField="notes" HeaderText="Notes" InsertVisible="True"/>
                </Columns>
                    <PagerStyle Width="15px" />
            </asp:GridView>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT * FROM [client_site_tbl]" UpdateCommand="UPDATE client_site_tbl SET name=@name, payrate=@payrate, status=@status, company=@company, manager_email=@manager_email, cc_emails_semicolon_separated=@cc_emails_semicolon_separated, onduty_mealperiods=@onduty_mealperiods, timesheetdata_oninvoice=@timesheetdata_oninvoice, supervisor_sheets=@supervisor_sheets, streetaddress=@streetaddress, city=@city, zip=@zip, notes=@notes WHERE client_site_id=@client_site_id" DeleteCommand="DELETE FROM client_site_tbl WHERE client_site_id=@client_site_id" FilterExpression="client_id='{0}'">
               <FilterParameters>
                    <asp:ControlParameter ControlID="client_ddl" Name="client__id" PropertyName="SelectedValue" Type="String"/>
                </FilterParameters>
            </asp:SqlDataSource>

                <h3 class="page-header">Add New Client Site</h3>
                <asp:ScriptManager ID="ScriptManager1" runat="server"/>
                <table runat="server" id="tableregister">
                    <tr>
                        <td><asp:Label ID="Label6" runat="server" Text="Client Name*:"></asp:Label></td>
                        <td><asp:TextBox ID="clientnameTB" runat="server" ReadOnly="true" Enabled="false"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" foreColor="red" ErrorMessage="*Client Name" ValidationGroup="valAdd" ControlToValidate="clientnametb"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label1" runat="server" Text="Client ID*:"></asp:Label></td>
                        <td><asp:TextBox ID="tb_client_id" runat="server" ReadOnly="true" Enabled="false"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" foreColor="red" ErrorMessage="*Client ID" ValidationGroup="valAdd" ControlToValidate="tb_client_id"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label8" runat="server" Text="Site Name*:"></asp:Label></td>
                        <td ><asp:TextBox ID="nameTB" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" foreColor="red" ErrorMessage="*Client Site Name" ValidationGroup="valAdd" ControlToValidate="nametb"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label30" runat="server" Text="Billing Rate (hourly)*:"></asp:Label></td>
                        <td><asp:TextBox ID="payrateTB" MaxLength="20" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" foreColor="red" ErrorMessage="*Billing Rate" ValidationGroup="valAdd" ControlToValidate="payrateTB"></asp:RequiredFieldValidator><asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Currency" ControlToValidate="payrateTB" foreColor="red" ErrorMessage="$$.$$" /></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label4" runat="server" Text="Company*:"></asp:Label></td>
                        <td><asp:DropDownList ID="companyDDL" runat="server"> 
                            <asp:ListItem>Matt's Staffing</asp:ListItem>
                            <asp:ListItem>Excalibur</asp:ListItem>
                        </asp:DropDownList></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" foreColor="red" ErrorMessage="*Company" ValidationGroup="valAdd" ControlToValidate="companyDDL"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label7" runat="server" Text="Manager E-Mail*:"></asp:Label></td>
                        <td ><asp:TextBox ID="manager_emailTB" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" foreColor="red" ErrorMessage="*Manager Email" ValidationGroup="valAdd" ControlToValidate="manager_emailTB"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label10" runat="server" Text="CC Emails (a;b;c)*:"></asp:Label></td>
                        <td ><asp:TextBox ID="cc_emails_semicolon_separatedTB" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" foreColor="red" ErrorMessage="*CC Emails" ValidationGroup="valAdd" ControlToValidate="cc_emails_semicolon_separatedTB"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label11" runat="server" Text="On Duty Meals*:"></asp:Label></td>
                        <td><asp:DropDownList ID="onduty_mealperiodsDDL" runat="server"> 
                            <asp:ListItem>True</asp:ListItem>
                            <asp:ListItem>False</asp:ListItem>
                        </asp:DropDownList></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator14" runat="server" foreColor="red" ErrorMessage="*On Duty Meals" ValidationGroup="valAdd" ControlToValidate="onduty_mealperiodsDDL"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label15" runat="server" Text="Timesheet Data on Invoice*:"></asp:Label></td>
                        <td><asp:DropDownList ID="timesheetdata_oninvoiceDDL" runat="server"> 
                            <asp:ListItem>True</asp:ListItem>
                            <asp:ListItem>False</asp:ListItem>
                        </asp:DropDownList></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator15" runat="server" foreColor="red" ErrorMessage="*Timesheet Data on Invoice" ValidationGroup="valAdd" ControlToValidate="timesheetdata_oninvoiceDDL"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label16" runat="server" Text="Supervisor Sheets*:"></asp:Label></td>
                        <td><asp:DropDownList ID="supervisor_sheetsDDL" runat="server"> 
                            <asp:ListItem>True</asp:ListItem>
                            <asp:ListItem>False</asp:ListItem>
                        </asp:DropDownList></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator16" runat="server" foreColor="red" ErrorMessage="*Supervisor Sheets" ValidationGroup="valAdd" ControlToValidate="supervisor_sheetsDDL"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label9" runat="server" Text="Street Address:"></asp:Label></td>
                        <td ><asp:TextBox ID="streetaddressTB" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label5" runat="server" Text="Street Address(2):"></asp:Label></td>
                        <td ><asp:TextBox ID="streetaddress2TB" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label12" runat="server" Text="City:"></asp:Label></td>
                        <td ><asp:TextBox ID="cityTB" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label14" runat="server" Text="Zip:"></asp:Label></td>
                        <td ><asp:TextBox ID="zipTB" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label29" runat="server" Text="State:"></asp:Label></td>
                        <td ><asp:TextBox ID="stateTB" runat="server"> </asp:TextBox></td>
                    </tr>
                    
                  <tr>
                    <td></td>
                      <td><asp:Button ID="btnCreate" CssClass="btn btn-primary" runat="server" onclick="createSite" Text="Create Site" ValidationGroup="valAdd"/>
                      </td>
                     </tr>
	      </table>

    </div>
    </div>
    </div>
    </form>
</body>
</html>