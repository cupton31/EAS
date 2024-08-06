<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="adminattendance.aspx.vb" Inherits="EAS.adminattendance" EnableEventValidation="True" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Attendance</title>
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
        <!-- #include file="adminnavfixedattend.html"-->
        <br />
       <div id="page-wrapper">
        <div class="col-lg-12">
		    <h3 class="page-header"><b>Attendance Summary</b></h3>
		    <!--<asp:Label ForeColor=#003366>Example: From: 1/11/2019 To: 31/11/2019</asp:Label>-->
        <br />
        <asp:Label>Enter Date Range From: </asp:Label> 
        <asp:TextBox ID="tbDateFrom" runat="server" text="1/1/1111" TextMode="Date" OnTextChanged="fromDate_TextChanged" AutoPostBack="true"></asp:TextBox>
        <asp:Label>To: </asp:Label> 
        <asp:TextBox ID="tbDateTo" runat="server" Text="9/9/9999" TextMode="Date" OnTextChanged="toDate_TextChanged" AutoPostBack="true"></asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Label>Employee: </asp:Label>
        <asp:DropDownList ID="employee_ddl" runat="server" OnTextChanged="employee_ddl_textChanged" AutoPostBack="true" ></asp:DropDownList>
        <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-sm btn-success" Text="Find" Visible="false"/>
               <br />
               <br />
               <asp:GridView ID="GridView1" runat="server" OnRowUpdating="Attendance_OnRowUpdating" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="log_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource1" PagerSettings-Mode="Numeric" EmptyDataText="No records has been added." PageSize="10">
            <Columns>
                 <asp:BoundField DataField="log_id" HeaderText="#" InsertVisible="False" ReadOnly="False" SortExpression="log_id" Visible="False" /> 
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="False" ShowInsertButton="False" HeaderText="Tools" />
                <asp:BoundField DataField="employee_id" HeaderText="Employee ID" InsertVisible="True" ReadOnly="True" />
                <asp:BoundField DataField="username" HeaderText="User Name" InsertVisible="True" ReadOnly="True" />
                <asp:BoundField DataField="action" HeaderText="Actions" SortExpression="action" />
                <asp:BoundField DataField="datetime" HeaderText="DateTime" SortExpression="datetime"  />
                <asp:BoundField DataField="date" HeaderText="Date" SortExpression="date" visible="false" />
                <asp:BoundField DataField="time" HeaderText="Time In/Out" SortExpression="time" visible="false" />
                <asp:BoundField DataField="editedaction" ReadOnly="True" HeaderText="Edit Action" SortExpression="editedaction" />
                <asp:BoundField DataField="editeddate" ReadOnly="True" HeaderText="Edit Date" SortExpression="editeddate"  />
                <asp:BoundField DataField="editedtime" ReadOnly="True" HeaderText="Edit Time" SortExpression="editedtime" />
                <asp:BoundField DataField="editedreason" ReadOnly="True" HeaderText="Edit Reason" SortExpression="editedreason" />
              </Columns>
            <PagerStyle Width="15px" />
        </asp:GridView>
               <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" DeleteCommand="DELETE FROM attendancelogs WHERE log_id=@log_id" UpdateCommand="UPDATE attendancelogs SET action=@action, datetime=@datetime WHERE log_id=@log_id" SelectCommand="SELECT * FROM [attendancelogs]" FilterExpression="">
                </asp:SqlDataSource>

           <table class="table table-striped table-bordered table-hover"><asp:Label ID="Label1" runat="server" Text="Label">Add New TimeIn/Out: </asp:Label>
            <tr>
                <td>Employee ID:<asp:TextBox ID="txtemployee_id" runat="server" /></td>
                <td>User Name:<asp:TextBox ID="txtusername" runat="server" /></td>
                <td>Action:<asp:DropDownList ID="DropDownList1" runat="server">
                        <asp:ListItem></asp:ListItem>
                        <asp:ListItem>TimeIn</asp:ListItem>
                        <asp:ListItem>TimeOut</asp:ListItem></asp:DropDownList></td>
                <td>Date:<asp:TextBox ID="txttimeinout" runat="server" TextMode="Date"/></td>
                <td>Time:<asp:TextBox ID="txtinout" runat="server" TextMode="Time" /></td>
                <td rowspan=2><asp:Button ID="btnAdd" runat="server" Text="Add" OnClick="Insert" CssClass="btn btn-sm btn-primary" ValidationGroup="btnAddTimeInOut"/></td>
            </tr>
            <tr>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" foreColor="red" ErrorMessage="*Employee Id" ValidationGroup="btnAddTimeInOut" ControlToValidate="txtemployee_id"></asp:RequiredFieldValidator></td>
                    <asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Integer" ControlToValidate="txtemployee_id" foreColor="red" ErrorMessage="*Number" />
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" foreColor="red" ErrorMessage="*Action" ValidationGroup="btnAddTimeInOut" ControlToValidate="DropDownList1"></asp:RequiredFieldValidator></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" foreColor="red" ErrorMessage="*Date TimeInOut" ValidationGroup="btnAddTimeInOut" ControlToValidate="txttimeinout"></asp:RequiredFieldValidator></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" foreColor="red" ErrorMessage="*Time TimeInOut" ValidationGroup="btnAddTimeInOut" ControlToValidate="txtinout"></asp:RequiredFieldValidator></td>
            </tr>
            </table>



    </div>
    </div>
    </div>
    </form>
</body>
</html>