<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ForgotPassword.aspx.vb" Inherits="EAS.ForgotPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Login Form</title>
    <link rel="stylesheet" href="css/LoginFormStyle.css" />
    <link rel="stylesheet" href="css/bootstrap.min.css" />
    <script type="text/javascript">
        function switchMe(login, register) {
            document.getElementById(login).style.display = block;
            document.getElementById(register).style.display = none;
        }
    </script>

    <style type="text/css">
        body {
            margin:0 auto;
            background-image: url('images/bg.jpg');
            background-repeat:no-repeat, repeat;
            background-size:cover;
            animation:animatedbackground 10s linear infiniten alternate
        }
        @keyframes animatedbackground {
            from {
                background-position: 0 0;
            }
            to {
                background-position: 100%;
            }
        }
        #register {
	        border:1px solid #ccc;
	        border-radius:4px;
            padding:20px;
            background:#fff;
            width:450px;
        }
        #login {
            margin:auto;
            width: 450px;
	        border:1px solid #ccc;
	        border-radius:4px;
            /*background: rgba(0, 0, 0, 0.8);*/
            /*color:#fff;*/
            padding:20px;
            background: #fff;

        }
        #container {
            width:450px;
            margin:auto;
        }
        #gender, #username, #password, #retypepassword, #logusername, #logpassword, #DateHired, #fname, #lname, #age, #contact, #position, #DateHired {
            padding: 10px;
            display: inline-block;
            box-sizing: border-box;
            border: 1px solid #ccc;
            border-radius: 4px;
        }
        #tablelogin td{
            padding:5px;
        }
         #tableregister td{
            padding:5px;
        }
        #user_level {
            cursor:none;
        }
        #h2 {
           color:#000;
           text-align:center;
           padding:5px;
           border:1px solid #ccc;
	        border-radius:4px;
            background:#fff;
        }
    </style>
   
</head>
<body>
    <form id="form1" runat="server">
        <br /><br /><br /><br /><br /><br /><br /><br />
        <div id="login">
        <h1 style="border-bottom:1px solid #ccc"">Account Recovery</h1>
            <br />
        <table id="tablelogin">
            <tr>
            <td><asp:Label ID="Label4" runat="server" Font-Bold="true" Text="Email Address: "></asp:Label></td>
            <td><asp:TextBox ID="EmailTB" runat="server"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" foreColor="red" ErrorMessage="Required"  ValidationGroup="valLog" ControlToValidate="EmailTB"></asp:RequiredFieldValidator></td>
           </tr>
           <tr>
            <td><asp:Button ID="SendButton" CssClass="btn btn-primary" OnClick="Send_Click" Text="Send" runat="server" style="position:relative; float:right; margin-right:20px;" /></td>
           </tr>
         </table>
        </div>
    </form>
</body>
</html>
<script src="js/dataTables.bootstrap.min.js"></script>
