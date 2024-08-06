<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="attendancelogs.aspx.vb" Inherits="EAS.attendancelogs" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Attendance</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="wrapper">
        <!-- #include file="navfixedattend.html"-->
        <br />
        <br />
       <div id="page-wrapper">
           <div class="col-lg-12">
			<h3 class="page-header">Attendance Summary</h3>
		    
    
        <asp:Label>Enter Date Range From: </asp:Label> 
        <asp:TextBox ID="tbDateFrom" runat="server" TextMode="Date"></asp:TextBox>
        <asp:Label>To: </asp:Label> 
        <asp:TextBox ID="tbDateTo" runat="server" TextMode="Date"></asp:TextBox>
         <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-sm btn-default" Text="Filter" />
            <!--<asp:Label><i>Example: From: 1/11/2019 To: 31/11/2019</i></asp:Label> -->
        <br />
         <br />
          <asp:GridView ID="GridView1" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" PageSize = "10" DataKeyNames="log_id" ReadOnly="True" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource1" AllowCustomPaging="False" PagerSettings-Mode="Numeric">
            <Columns>
                <asp:BoundField DataField="log_id" HeaderText="Log #" InsertVisible="False" ReadOnly="True" SortExpression="log_id" />
                <asp:BoundField DataField="username" HeaderText="Username" ReadOnly="True" SortExpression="username" />
                <asp:BoundField DataField="action" HeaderText="Action" ReadOnly="True" SortExpression="action"  />
                <asp:BoundField DataField="datetime" ItemStyle-CssClass="hiddencol"  HeaderStyle-CssClass="hiddencol" HeaderText="DateTime" ReadOnly="True" SortExpression="datetime"  />
                <asp:BoundField DataField="date" HeaderText="Date" ReadOnly="True" SortExpression="date"  />
                <asp:BoundField DataField="time" HeaderText="Time" ReadOnly="True" SortExpression="time" />
                <asp:BoundField DataField="editedaction" HeaderText="Edit Action" ReadOnly="True" SortExpression="editedaction" />
                <asp:BoundField DataField="editeddate" HeaderText="Edit Date" ReadOnly="True" SortExpression="editeddate" />
                <asp:BoundField DataField="editedtime" HeaderText="Edit Time" ReadOnly="True" SortExpression="editedtime" />
                <asp:BoundField DataField="editedreason" HeaderText="Edit Reason" SortExpression="editedreason" />
            </Columns>
           </asp:GridView>
           <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" UpdateCommand="UPDATE attendancelogs SET editedreason=@editedreason WHERE log_id=@log_id" SelectCommand="SELECT employee_tbl.username, attendancelogs.log_id, attendancelogs.action, attendancelogs.datetime, attendancelogs.date, attendancelogs.time, attendancelogs.editedaction, attendancelogs.editedtime, attendancelogs.editeddate, attendancelogs.editedreason FROM [employee_tbl] INNER JOIN [attendancelogs] ON employee_tbl.username=attendancelogs.username WHERE attendancelogs.username=@username ORDER BY log_id ASC " FilterExpression="datetime >='{0}' AND datetime <='{1}'">
            <SelectParameters>
            <asp:SessionParameter DefaultValue="" Name="username" SessionField="user" Type="String" />
            </SelectParameters>
            <FilterParameters>
            <asp:ControlParameter Name="datetime" ControlID="tbDateFrom"/>
            <asp:ControlParameter Name="datetime" ControlID="tbDateTo"/>
        </FilterParameters>
        </asp:SqlDataSource>

        <hr />
        <table class="table table-striped table-bordered table-hover"><asp:Label ID="Label1" runat="server" Text="Label"><b>Made a mistake clocking in? ... Submit a change request:</b></asp:Label>
        <tr>
            <td>Log #:<asp:TextBox ID="txtlog_id" runat="server" /></td>
            <td>Reason for Change:<asp:TextBox ID="txtreason" runat="server" MaxLength="200" TextMode="SingleLine" /></td>
            <td>Edit Action:<asp:DropDownList ID="DropDownList1" runat="server">
                    <asp:ListItem></asp:ListItem>
                    <asp:ListItem>TimeIn</asp:ListItem>
                    <asp:ListItem>TimeOut</asp:ListItem>
                    <asp:ListItem>DELETE</asp:ListItem>
                       </asp:DropDownList></td>
            <td>Edit Date:<asp:TextBox ID="txtDate" runat="server" TextMode="Date"/></td>
            <td>Edit Time:<asp:TextBox ID="txtTime" runat="server"  TextMode="Time" /></td>
            <td rowspan="1"><asp:Button ID="btnChange" runat="server" Text="Submit Change Request" OnClick="makeLogChange" CssClass="btn btn-sm btn-primary" ValidationGroup="btnChange"/></td>
        </tr>
        <tr>
            <td><asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" foreColor="red" ErrorMessage="*Log ID" ValidationGroup="btnChange" ControlToValidate="txtlog_id"></asp:RequiredFieldValidator>
                <asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Integer" ControlToValidate="txtlog_id" foreColor="red" ErrorMessage="*Number" /></td>
            <td><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" foreColor="red" ErrorMessage="*Reason" ValidationGroup="btnChange" ControlToValidate="txtreason"></asp:RequiredFieldValidator></td>
            <td><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" foreColor="red" ErrorMessage="*Action" ValidationGroup="btnChange" ControlToValidate="DropDownList1"></asp:RequiredFieldValidator></td>
            <td><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" foreColor="red" ErrorMessage="*Date" ValidationGroup="btnChange" ControlToValidate="txtDate"></asp:RequiredFieldValidator>
                <asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Date" ControlToValidate="txtDate" foreColor="red" ErrorMessage="*Date" /></td>
            <td><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" foreColor="red" ErrorMessage="*Time" ValidationGroup="btnChange" ControlToValidate="txtTime"></asp:RequiredFieldValidator>
                <asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="String" ControlToValidate="txtTime" foreColor="red" ErrorMessage="*Time" /></td>
        </tr>
        </table>

    </div>
    </div>
    </div>
    </form>
</body>
</html>
