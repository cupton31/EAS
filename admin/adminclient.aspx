<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="adminclient.aspx.vb" Inherits="EAS.adminclient" EnableEventValidation="True" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Client</title>
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
        <!-- #include file="adminnavfixedclient.html"-->
        <br />
       <div id="page-wrapper">
          <div class="col-lg-12">
			<h3 class="page-header"><b>Clients</b></h3>
               <asp:GridView ID="GridView1" runat="server" OnRowDeleting="Client_OnRowDeleting" OnRowUpdating="Client_OnRowUpdating" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="client_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource1" PagerSettings-Mode="Numeric" EmptyDataText="No records has been added." PageSize="10">
                <Columns>
                 <asp:BoundField DataField="client_id" HeaderText="Client ID #" InsertVisible="False" ReadOnly="True" SortExpression="client_id" Visible="True" />  
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="False" ShowInsertButton="False" HeaderText="Tools" />
                <asp:BoundField DataField="clientname" HeaderText="Client Name" InsertVisible="True" />
                <asp:BoundField DataField="phone" HeaderText="Phone" InsertVisible="True" />
                <asp:BoundField DataField="email" HeaderText="Email" SortExpression="email" InsertVisible="True"/>
                <asp:BoundField DataField="website" HeaderText="Web Site URL" InsertVisible="True"/>
                <asp:BoundField DataField="daterelationshipstarted" HeaderText="Date Relationship Started" InsertVisible="True"/>
                <asp:BoundField DataField="description" HeaderText="Description" InsertVisible="True"/>
              </Columns>
            <PagerStyle Width="15px" />
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" DeleteCommand="DELETE FROM client_tbl WHERE client_id=@client_id" UpdateCommand="UPDATE client_tbl SET clientname=@clientname, phone=@phone, email=@email, website=@website, daterelationshipstarted=@daterelationshipstarted, description=@description WHERE client_id=@client_id" SelectCommand="SELECT * FROM [client_tbl]"></asp:SqlDataSource>

        
                <h3 class="page-header">Add New Client</h3>
                <table runat="server" id="tableregister">
                    <tr>
                        <td><asp:Label ID="Label6" runat="server" Text="Client Name*:"></asp:Label></td>
                        <td><asp:TextBox ID="clientname" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" foreColor="red" ErrorMessage="*ClientName" ValidationGroup="valAdd" ControlToValidate="clientname"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label8" runat="server" Text="Phone*:"></asp:Label></td>
                        <td ><asp:TextBox ID="phone" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" foreColor="red" ErrorMessage="*Phone" ValidationGroup="valAdd" ControlToValidate="phone"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label13" runat="server" Text="Email*:"></asp:Label></td>
                        <td ><asp:TextBox ID="email" runat="server"> </asp:TextBox></td>
                        <td><asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" foreColor="red" ErrorMessage="*Email" ValidationGroup="valAdd" ControlToValidate="email"></asp:RequiredFieldValidator></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label9" runat="server" Text="Street Address:"></asp:Label></td>
                            <td ><asp:TextBox ID="streetaddress" runat="server"> </asp:TextBox></td>
                    </tr>
                        <tr>
                        <td><asp:Label ID="Label5" runat="server" Text="Street Address(2):"></asp:Label></td>
                        <td ><asp:TextBox ID="streetaddress2" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label12" runat="server" Text="City:"></asp:Label></td>
                        <td ><asp:TextBox ID="city" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label14" runat="server" Text="Zip:"></asp:Label></td>
                        <td ><asp:TextBox ID="zip" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label29" runat="server" Text="State:"></asp:Label></td>
                        <td ><asp:TextBox ID="state" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label2" runat="server" Text="Website:"></asp:Label></td>
                        <td ><asp:TextBox ID="website" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label11" runat="server" Text="Date Relationship Started:"></asp:Label></td>
                        <td ><asp:TextBox ID="daterelationshipstarted" runat="server" TextMode="Date"> </asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="Label1" runat="server" TextMode="MultiLine" Rows="10" Text="Description (About):"></asp:Label></td>
                        <td ><asp:TextBox ID="description" runat="server"> </asp:TextBox></td>
                    </tr>
                    <tr> <td><asp:Label ID="Label36" runat="server" Text="."> </asp:Label> </td></tr>
                    <tr>
                    <td></td>
                    <td><asp:Button ID="btnCreate" CssClass="btn btn-primary" runat="server" onclick="createClient" Text="Add Client" ValidationGroup="valAdd"/>
                    </td>
                    </tr>
            </table>

        <br />
        <br />
        <br />
        

    </div>
    </div>
    </div>
    </form>
</body>
</html>