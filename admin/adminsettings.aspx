<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="adminsettings.aspx.vb" Inherits="EAS.adminsettings" EnableEventValidation="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Settings</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="wrapper">
        <!-- #include file="adminnavfixedsettings.html"-->
        
	<div id="page-wrapper">
           <div class="col-lg-12">
               <h3 class="page-header"></h3>
		<asp:Label ID="ManageAdminLabel" runat="server" Font-Bold="true" Font-Size="X-Large" Text="Manage Admins"></asp:Label><hr />
         <asp:GridView ID="SuperAdmin_GridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="admin_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource1" PagerSettings-Mode="Numeric" OnRowDeleting="SuperAdmin_GridView_RowDeleting" OnRowUpdating="SuperAdmin_GridView_RowUpdating" >
            <Columns>
                 <asp:CommandField ShowEditButton="True" ShowDeleteButton="false" ShowInsertButton="false" HeaderText="Tools" />
                 <asp:BoundField DataField="firstname" HeaderText="First Name" SortExpression="firstname" />
                 <asp:BoundField DataField="lastname" HeaderText="Last Name" SortExpression="lastname" />
                 <asp:BoundField DataField="phone" HeaderText="Phone" SortExpression="phone" />
                 <asp:BoundField DataField="email" HeaderText="Email" SortExpression="email" />
                 <asp:BoundField DataField="user_level" HeaderText="User Level" SortExpression="user_level" />
				 <asp:BoundField DataField="username" HeaderText="Username" ReadOnly="true" SortExpression="username" />
                 <asp:BoundField DataField="password" HeaderText="Password" SortExpression="password"  />
             </Columns>
         </asp:GridView>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT * FROM [admin_tbl] ORDER BY admin_id ASC" DeleteCommand="DELETE FROM admin_tbl WHERE admin_id = @admin_id" UpdateCommandType="Text" UpdateCommand="UPDATE admin_tbl SET firstname = @firstname, lastname = @lastname, phone = @phone, email = @email, password = @password WHERE admin_id = @admin_id" >
                <SelectParameters>
                    <asp:SessionParameter DefaultValue="" Name="username" SessionField="user" Type="String" />
                </SelectParameters>
            </asp:SqlDataSource>



               <asp:GridView ID="Admin_GridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="admin_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource2" PagerSettings-Mode="Numeric" OnRowUpdating="Admin_GridView_RowUpdating" >
                   <Columns>
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="false" ShowInsertButton="false" HeaderText="Tools" />
                        <asp:BoundField DataField="firstname" HeaderText="First Name" SortExpression="firstname" />
                        <asp:BoundField DataField="lastname" HeaderText="Last Name" SortExpression="lastname" />
                        <asp:BoundField DataField="phone" HeaderText="Phone" SortExpression="phone" />
                        <asp:BoundField DataField="email" HeaderText="Email" SortExpression="email" />
                        <asp:BoundField DataField="user_level" HeaderText="User Level" ReadOnly="True" SortExpression="user_level" />
	                    <asp:BoundField DataField="username" HeaderText="Username" ReadOnly="true" SortExpression="username" />
                        <asp:BoundField DataField="password" HeaderText="Password" SortExpression="password"  />
                    </Columns>
                </asp:GridView>
                   <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT * FROM [admin_tbl] ORDER BY admin_id ASC" UpdateCommandType="Text" UpdateCommand="UPDATE admin_tbl SET firstname = @firstname, lastname = @lastname, phone = @phone, email = @email, username = @username, password = @password WHERE admin_id = @admin_id" >
                       <SelectParameters>
                           <asp:SessionParameter DefaultValue="" Name="username" SessionField="user" Type="String" />
                       </SelectParameters>
                   </asp:SqlDataSource>











         <br /><br /><asp:Label ID="CreateAdminLabel" runat="server" Font-Bold="true" Font-Size="X-Large" Text="Create Admin"></asp:Label><hr />
         <asp:ScriptManager ID="ScriptManager1" runat="server"/>
         <asp:table runat="server" id="CreateAdminTable" >
                 <asp:tablerow>
                         <asp:tablecell><asp:Label ID="Label24" runat="server" Text="Username*:"></asp:Label></asp:tablecell>
                         <asp:tablecell><asp:TextBox ID="UserNameTB" runat="server" MaxLength="20"></asp:TextBox></asp:tablecell>
                         <asp:tablecell><asp:RegularExpressionValidator runat="server" ID="RegularExpressionValidator2" ValidationExpression="^[^\s]{7,}$" foreColor="red" ErrorMessage="*7+ Characters" ValidationGroup="valAdd" ControlToValidate="UserNameTB"></asp:RegularExpressionValidator></asp:tablecell>
                 </asp:tablerow>
                 <asp:tablerow>
                         <asp:tablecell><asp:Label ID="Label65" runat="server" Text="Password*:"></asp:Label></asp:tablecell>
                         <asp:tablecell><asp:TextBox ID="PassWordTB" MaxLength="20" runat="server"></asp:TextBox></asp:tablecell>
                         <asp:tablecell><asp:RegularExpressionValidator runat="server" ID="RegularExpressionValidator3" ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*:_~|/.,;()@'%?%&]).{12,}$" foreColor="red" ErrorMessage="1 lower, 1 upper, 1 number, 1 special, 12+ chars" ValidationGroup="valAdd" ControlToValidate="PassWordTB"></asp:RegularExpressionValidator></asp:tablecell>
                 </asp:tablerow>
                <asp:TableRow>
                      <asp:tablecell><asp:Label ID="Label1" runat="server" Text="User Level*:"></asp:Label></asp:tablecell>
                      <asp:tablecell ><asp:DropDownList ID="UserLevelDDL" runat="server" > <asp:ListItem Selected="True">admin</asp:ListItem><asp:ListItem>superadmin</asp:ListItem></asp:DropDownList></asp:tablecell>
                </asp:TableRow>
                 <asp:tablerow>
                     <asp:tablecell><asp:Label ID="Label6" runat="server" Text="First Name*:"></asp:Label></asp:tablecell>
                      <asp:tablecell ><asp:TextBox ID="FirstNameTB" runat="server" MaxLength="20"> </asp:TextBox></asp:tablecell>
                     <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" foreColor="red" ErrorMessage="*Firstname" ValidationGroup="valAdd" ControlToValidate="firstnameTB"></asp:RequiredFieldValidator></asp:tablecell>
                 </asp:tablerow>
                  <asp:tablerow>
                     <asp:tablecell><asp:Label ID="Label7" runat="server" Text="Last Name*:"></asp:Label></asp:tablecell>
                      <asp:tablecell ><asp:TextBox ID="LastNameTB" runat="server" MaxLength="20"> </asp:TextBox></asp:tablecell>
                      <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" foreColor="red" ErrorMessage="*Lastname" ValidationGroup="valAdd" ControlToValidate="lastnameTB"></asp:RequiredFieldValidator></asp:tablecell>
                 </asp:tablerow>
                  <asp:tablerow>
                     <asp:tablecell><asp:Label ID="Label8" runat="server" Text="Cell Phone*:"></asp:Label></asp:tablecell>
                      <asp:tablecell ><asp:TextBox ID="PhoneTB" runat="server" MaxLength="20"> </asp:TextBox></asp:tablecell>
                      <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" foreColor="red" ErrorMessage="*Phone" ValidationGroup="valAdd" ControlToValidate="PhoneTB"></asp:RequiredFieldValidator></asp:tablecell>
                 </asp:tablerow>
                   <asp:tablerow>
                     <asp:tablecell><asp:Label ID="Label13" runat="server" Text="Email*:"></asp:Label></asp:tablecell>
                     <asp:tablecell ><asp:TextBox ID="EmailTB" runat="server" MaxLength="50"> </asp:TextBox></asp:tablecell>
                     <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" foreColor="red" ErrorMessage="*Email" ValidationGroup="valAdd" ControlToValidate="EmailTB"></asp:RequiredFieldValidator></asp:tablecell>
                 </asp:tablerow>

             <asp:tablerow> <asp:tablecell><asp:Label ID="Label36" runat="server" Text="."> </asp:Label> </asp:tablecell></asp:tablerow>
               <asp:tablerow>
                  <asp:tablecell colspan="1"></asp:tablecell>
                 <asp:tablecell><asp:Button runat="server" ID="InsertAdmin_Button" CssClass="btn btn-primary runat=server" onclick="InsertAdmin_Click" Text="Create" ValidationGroup="valAdd" />
                 </asp:tablecell>
               </asp:tablerow>
         </asp:table>








            		
                    <br /><asp:Label ID="ChangeLogs_Label" runat="server" Font-Bold="true" Font-Size="X-Large" Text="Logs"></asp:Label><hr />  
                      <asp:GridView ID="ChangeLogs_GridView" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="log_id" AllowSorting="True" AllowPaging="True" DataSourceID="SqlDataSource3" PagerSettings-Mode="Numeric" EmptyDataText="No records has been added." PageSize="50" >
                        <Columns>
                            <asp:BoundField DataField="log_id" HeaderText="#" InsertVisible="False" ReadOnly="True" SortExpression="log_id" Visible="True" /> 
                            <asp:BoundField DataField="admin_id" HeaderText="Admin ID" ReadOnly="True" SortExpression="admin_id" />
                            <asp:BoundField DataField="admin_username" HeaderText="User Name" InsertVisible="False" ReadOnly="True" SortExpression="admin_username" />
                            <asp:BoundField DataField="datetime" HeaderText="DateTime" SortExpression="datetime"  />
                            <asp:BoundField DataField="table_changed" HeaderText="Table Changed" SortExpression="table_changed" />
                            <asp:BoundField DataField="actions_performed" HeaderText="Action Performed" SortExpression="actions_performed" HtmlEncode="False" HtmlEncodeFormatString="False" />
                            <asp:BoundField DataField="query_performed" HeaderText="Query Performed" Visible="false" SortExpression="query_performed" />
                          </Columns>
                        <PagerStyle Width="15px" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT * FROM [change_log_tbl] ORDER BY datetime DESC" >

                    </asp:SqlDataSource>


           
    </div>
    </form>
</body>
</html>


