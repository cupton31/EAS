<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="yourprofile.aspx.vb" Inherits="EAS.yourprofile" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Profile</title>
     <style type="text/css">
        body {
            /*background:#eee;*/
            font-family:Arial;
            margin:0 auto;
        }
        #container {

            margin: 0 auto;
            width:800px;
            height: 0 auto;
            padding:10px;
            border-top-left-radius: 5px;
            border-top-right-radius: 5px;
            border-bottom-left-radius: 5px;
            border-bottom-right-radius: 5px;
            border:1px solid #eee;
            background:#fff;
        }
        #divadd{
            border-bottom: 1px solid #eee;
        }
        #table td{
            padding:5px;
        }

    </style>
</head>
<body>
    <form id="form1" runat="server" autocomplete="off">
        <div id="wrapper">
        <!-- #include file="navfixedprof.html"-->
        <br />
        <br />
       <div id="page-wrapper">
           <div class="col-lg-12">
			<h3 class="page-header">Profile Information</h3>
		</div>
    <div class="col-lg-12">
        <table width="100%" id="table">
            <!--<tr>
                <td width="5%">CellPhone:</td>
                <td><asp:TextBox ID="cellphone" Cssclass="form-control" runat="server"></asp:TextBox></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" validationGroup="valAdd" ControlToValidate="cellphone" ErrorMessage="Cellphone is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
		     </tr>
            <tr>
               <td width="5%">Email:
                <td><asp:TextBox ID="email" Cssclass="form-control" runat="server"></asp:TextBox></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" validationGroup="valAdd" ControlToValidate="email" ErrorMessage="Email is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
		     </tr>
            <tr>
                <td width="5%">MailingAddress:</td>
                <td><asp:TextBox ID="streetaddress" CssClass="form-control" runat="server"></asp:TextBox></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" validationGroup="valAdd" ControlToValidate="streetaddress" ErrorMessage="Mailing Address is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
		    </tr>
            <tr>
                <td width="5%">MailAddress(2):</td>
                  <td><asp:TextBox ID="streetaddress2" Cssclass="form-control" runat="server"></asp:TextBox></td>
                 <td><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" validationGroup="valAdd" ControlToValidate="streetaddress" ErrorMessage="Mailing Address is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
            </tr>
            <tr>
            <td width="5%">City:</td>
                <td><asp:TextBox ID="city" Cssclass="form-control" runat="server"></asp:TextBox></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" validationGroup="valAdd" ControlToValidate="city" ErrorMessage="City is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
            </tr>
            <td width="5%">State:</td>
                <td><asp:TextBox ID="state" Cssclass="form-control" runat="server"></asp:TextBox></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" validationGroup="valAdd" ControlToValidate="city" ErrorMessage="State is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
            </tr>-->
            <tr>
            <td width="5%">Nickname:</td>
                <td><asp:TextBox ID="nickname" Cssclass="form-control" runat="server"></asp:TextBox></td>
                <td><asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" validationGroup="valAdd" ControlToValidate="nickname" ErrorMessage="Nickname is Required" ForeColor="Red"></asp:RequiredFieldValidator></td>
            <td width="5%">Favorite Shift:</td>
                <td><asp:DropDownList ID="favoriteshift" runat="server" OnSelectedIndexChanged="favoriteshift_SelectedIndexChanged" AutoPostBack="true">
                    <asp:ListItem></asp:ListItem>
                    <asp:ListItem>Day</asp:ListItem>
                    <asp:ListItem>Swing</asp:ListItem>
                    <asp:ListItem>Grave</asp:ListItem>
                                   </asp:DropDownList></td>
            <td width="5%">2nd Fav Shift:</td>
                <td><asp:DropDownList ID="secondfavoriteshift" runat="server" OnSelectedIndexChanged="secondfavoriteshift_SelectedIndexChanged" AutoPostBack="true">
                    <asp:ListItem></asp:ListItem>
                    <asp:ListItem>Day</asp:ListItem>
                    <asp:ListItem>Swing</asp:ListItem>
                    <asp:ListItem>Grave</asp:ListItem>
                                </asp:DropDownList></td>
            <td width="5%">3rd Fav Shift:</td>
                <td><asp:DropDownList ID="thirdfavoriteshift" runat="server" OnSelectedIndexChanged="thirdfavoriteshift_SelectedIndexChanged" AutoPostBack="true">
                    <asp:ListItem></asp:ListItem>
                    <asp:ListItem>Day</asp:ListItem>
                    <asp:ListItem>Swing</asp:ListItem>
                    <asp:ListItem>Grave</asp:ListItem>
                                   </asp:DropDownList></td>
            </tr>
            <tr>
                <td width="5%"></td>
                <td><asp:Button ID="btnAddVehicle" Cssclass="btn btn-default" runat="server" Text="Submit Changes" ValidationGroup="valAdd"/></td>
                <td></td>
            </tr>
         </table>

         <asp:HiddenField ID="employee_id" runat="server" />
         <asp:GridView ID="GridView1" runat="server" class="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%" Font-Names="Arial" Font-Size="Small" PageSize = "10" >
            <Columns>
                <asp:BoundField DataField="firstname" HeaderText="First Name"/>
                <asp:BoundField DataField="lastname" HeaderText="Last Name"/>
                <asp:BoundField DataField="nickname" HeaderText="Nick Name"/>
                <asp:BoundField DataField="position" HeaderText="Position" />
                <asp:BoundField DataField="favoriteshift" HeaderText="Favorite Shift" />
                <asp:BoundField DataField="secondfavoriteshift" HeaderText="2nd Fav Shift" />
                <asp:BoundField DataField="thirdfavoriteshift" HeaderText="3rd Fav Shift" />
                <asp:BoundField DataField="datehired" HeaderText="Date Hired" />
                <asp:BoundField DataField="cellphone" HeaderText="Cell Phone"/>
                <asp:BoundField DataField="email" HeaderText="Email"/>
                <asp:templateField>
                <ItemTemplate>
                   <asp:LinkButton runat ="server"  class="btn btn-default" OnClick="dataShow" CommandArgument='<%# Eval("employee_id")%>'>LOAD TEXT</asp:LinkButton>
                </ItemTemplate>
            </asp:templateField>
                </Columns>
        </asp:GridView>



        <table width="100%" id="table">
                <tr>
                    <td width="5%">Need to change something? Contact an administrator or scheduler. </td>

                </tr>
        </table>



    </div>
</div><asp:TextBox ID="firstname" Cssclass="form-control" runat="server"></asp:TextBox>
    </form>
</body>
</html>
