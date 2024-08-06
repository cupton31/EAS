<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="adminemployee.aspx.vb" Inherits="EAS.adminemployee" EnableEventValidation="True" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Employee</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="wrapper">
        <!-- #include file="adminnavfixedemployee.html"-->
        <br />
       <div id="page-wrapper">
           <div class="col-lg-12">
			<h3 class="page-header"><b>Employee List</b></h3>

        <asp:Label>Search employee Last Name:</asp:Label>
        <asp:TextBox ID="lastname" runat="server"></asp:TextBox>
         <asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;</asp:Label>
         <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-sm btn-success" Text="Find"/>

         <br /><br />
                      <asp:GridView ID="GridView1" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" DataKeyNames="employee_id" AllowSorting="True" AllowPaging="True" OnRowUpdating="GridView1_RowUpdating" DataSourceID="SqlDataSource1" PagerSettings-Mode="Numeric" PageSize="10">
                        <Columns>
                           <asp:CommandField ShowEditButton="True" ShowDeleteButton="false" ShowInsertButton="false" HeaderText="Tools" />
                            <asp:BoundField DataField="employee_id" HeaderText="Employee ID" InsertVisible="False" ReadOnly="True" SortExpression="employee_id" Visible="True"/> 
                            <asp:BoundField DataField="company" HeaderText="Company" SortExpression="company" />
                            <asp:BoundField DataField="ms_id" HeaderText="MS ID" SortExpression="ms_id" />
                            <asp:BoundField DataField="excalibur_id" HeaderText="Excalibur ID" SortExpression="excalibur_id" />
                            <asp:BoundField DataField="username" HeaderText="Username" SortExpression="username" ReadOnly="True"/>
                            <asp:BoundField DataField="status" HeaderText="Status (Active/Inactive)" SortExpression="status" />
                            <asp:BoundField DataField="firstname" HeaderText="First Name" SortExpression="firstname" />
                            <asp:BoundField DataField="lastname" HeaderText="Last Name" SortExpression="lastname" />
                            <asp:BoundField DataField="position" HeaderText="Position" SortExpression="position"  />
                            <asp:BoundField DataField="basepayrate" HeaderText="Base Pay Rate" SortExpression="basepayrate" />
                            <asp:BoundField DataField="cellphone"  HeaderText="Cell Phone" SortExpression="cellphone" />
		                    <asp:BoundField DataField="email" HeaderText="Email" SortExpression="email" />
                            <asp:BoundField DataField="record" HeaderText="Permanent Record" SortExpression="record" />
                        </Columns>
                </asp:GridView>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Excalibur %>" SelectCommand="SELECT* FROM [employee_tbl] ORDER BY lastname ASC"  FilterExpression="lastname='{0}'" UpdateCommand="UPDATE employee_tbl SET status=@status, firstname=@firstname, lastname=@lastname, cellphone=@cellphone, email=@email, position=@position, basepayrate=@basepayrate, company=@company, ms_id=@ms_id, excalibur_id=@excalibur_id, record=@record WHERE employee_id=@employee_id" >
            <FilterParameters>
             <asp:ControlParameter Name="lastname" ControlID="lastname"/>
           </FilterParameters>
          </asp:SqlDataSource>
           


               <h3 class="page-header">Create Account</h3>
                        <asp:ScriptManager ID="ScriptManager1" runat="server" />
                        <asp:table runat="server" id="tableregister">
                                <asp:tablerow>
                                        <asp:tablecell><asp:Label ID="Label1" runat="server" Text="Username (FirstLast):"></asp:Label></asp:tablecell>
                                        <asp:tablecell><asp:TextBox ID="usernameTB" runat="server" MaxLength="50" readOnly="true" Enabled="false"> </asp:TextBox></asp:tablecell>
                                        <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" foreColor="red" ErrorMessage="*username" ValidationGroup="valAdd" ControlToValidate="usernameTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                        <asp:tablecell><asp:Label ID="Label2" runat="server" Text="Password*:"></asp:Label></asp:tablecell>
                                        <asp:tablecell><asp:TextBox ID="passwordTB" MaxLength="50" runat="server"></asp:TextBox></asp:tablecell>
                                        <asp:TableCell><asp:RegularExpressionValidator runat="server" ID="RequiredExpressionValidator1" ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*:_~|/.,;()@'%?%&]).{12,}$" foreColor="red" ErrorMessage="1 lower, 1 upper, 1 number, 1 special, 12+ chars" ValidationGroup="valAdd" ControlToValidate="passwordTB"></asp:RegularExpressionValidator></asp:TableCell>
                                </asp:tablerow>
                                 <asp:tablerow>
                                         <asp:tablecell><asp:Label ID="Label3" runat="server" Text="Re-Type Password*:"></asp:Label></asp:tablecell>
                                         <asp:tablecell><asp:TextBox ID="retypepasswordTB" runat="server" TextMode="Password"></asp:TextBox></asp:tablecell>
                                          <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" foreColor="red" ErrorMessage="RequiredField" ValidationGroup="valAdd" ControlToValidate="retypepasswordTB"></asp:RequiredFieldValidator><br />
                                                 <asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="Password Mismatch" ControlToCompare="passwordTB" ControlToValidate="retypepasswordTB" ForeColor="Red" ValidationGroup="valAdd"></asp:CompareValidator>
                                             </asp:tablecell>
                                </asp:tablerow> 
                                <asp:tablerow>
                                    <asp:tablecell colspan="3">
                                        <h3 style="border-bottom:1px solid #ccc""></h3>
                                    </asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label6" runat="server" Text="First Name*:"></asp:Label></asp:tablecell>
                                     <asp:tablecell ><asp:TextBox ID="firstnameTB" MaxLength="20" runat="server" OnTextChanged="updateUserName" AutoPostBack="true"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" foreColor="red" ErrorMessage="*Firstname" ValidationGroup="valAdd" ControlToValidate="firstnameTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label4" runat="server" Text="Middle Name:"></asp:Label></asp:tablecell>
                                    <asp:tablecell ><asp:TextBox ID="middlenameTB" MaxLength="20" runat="server"> </asp:TextBox></asp:tablecell>
                                </asp:tablerow>
                                 <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label7" runat="server" Text="Last Name*:"></asp:Label></asp:tablecell>
                                     <asp:tablecell ><asp:TextBox ID="lastnameTB" MaxLength="20" runat="server" OnTextChanged="updateUserName" AutoPostBack="true"> </asp:TextBox></asp:tablecell>
                                     <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" foreColor="red" ErrorMessage="*Lastname" ValidationGroup="valAdd" ControlToValidate="lastnameTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                 <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label8" runat="server" Text="Cell Phone*:"></asp:Label></asp:tablecell>
                                     <asp:tablecell ><asp:TextBox ID="cellphoneTB" MaxLength="20" runat="server"> </asp:TextBox></asp:tablecell>
                                     <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" foreColor="red" ErrorMessage="*Phone" ValidationGroup="valAdd" ControlToValidate="cellphoneTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                  <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label13" runat="server" Text="Email*:"></asp:Label></asp:tablecell>
                                    <asp:tablecell ><asp:TextBox ID="emailTB" MaxLength="50" runat="server"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" foreColor="red" ErrorMessage="*Email" ValidationGroup="valAdd" ControlToValidate="emailTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label5" runat="server" Text="Company*:"></asp:Label></asp:tablecell>
                                    <asp:tablecell ><asp:TextBox ID="companyTB" MaxLength="50" runat="server"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" foreColor="red" ErrorMessage="*Company" ValidationGroup="valAdd" ControlToValidate="companyTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label31" runat="server" Text="MS ID:"></asp:Label></asp:tablecell>
                                    <asp:tablecell ><asp:TextBox ID="ms_idTB" MaxLength="20" runat="server"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Integer" ControlToValidate="ms_idTB" foreColor="red" ErrorMessage="*Integer" /></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label32" runat="server" Text="Excalibur ID:"></asp:Label></asp:tablecell>
                                    <asp:tablecell ><asp:TextBox ID="excalibur_idTB" MaxLength="20" runat="server"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Integer" ControlToValidate="excalibur_idTB" foreColor="red" ErrorMessage="*Integer" /></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label10" runat="server" Text="Position*:"></asp:Label></asp:tablecell>
                                    <asp:tablecell ><asp:TextBox ID="positionTB" MaxLength="50" runat="server"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" foreColor="red" ErrorMessage="*Position" ValidationGroup="valAdd" ControlToValidate="positionTB"></asp:RequiredFieldValidator></asp:tablecell>
                                </asp:tablerow>
                                <asp:tablerow>
                                    <asp:tablecell><asp:Label ID="Label30" runat="server" Text="Base Pay Rate (hourly)*:"></asp:Label></asp:tablecell>
                                    <asp:tablecell><asp:TextBox ID="basepayrateTB" MaxLength="20" runat="server" Text="18.00"> </asp:TextBox></asp:tablecell>
                                    <asp:tablecell><asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" foreColor="red" ErrorMessage="*Base Pay Rate" ValidationGroup="valAdd" ControlToValidate="basepayrateTB"></asp:RequiredFieldValidator><asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Currency" ControlToValidate="basepayrateTB" foreColor="red" ErrorMessage="$$.$$" /></asp:tablecell>
                                </asp:tablerow>
                            <asp:tablerow> <asp:tablecell><asp:Label ID="Label36" runat="server" Text="."> </asp:Label> </asp:tablecell></asp:tablerow>
                              <asp:tablerow>
                                 <asp:tablecell colspan="1"></asp:tablecell>
                                <asp:tablecell><asp:Button ID="btnCreate" CssClass="btn btn-primary" runat="server" onclick="createEmployee" Text="Create Account" ValidationGroup="valAdd"/>
                                </asp:tablecell>
                              </asp:tablerow>
                        </asp:table>

               <br />
               <br />
               <br />


    </div>
     </div>
    </div>
    </form>
</body>
</html>