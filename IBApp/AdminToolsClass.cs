using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IBApp
{
    internal static class AdminToolsClass
    {

        internal static UserClass.OperationStatus ChangeUserRole(char[] userlogin, UserClass.Role newrole)
        {
            try
            {

                System.Data.SqlClient.SqlCommand SetUserRole = new System.Data.SqlClient.SqlCommand("SetUserRole", DBConnection.connection);
                SetUserRole.CommandType = System.Data.CommandType.StoredProcedure;

                SetUserRole.Parameters.Add("@adminlogin", System.Data.SqlDbType.NVarChar, 50);
                SetUserRole.Parameters["@adminlogin"].Value = UserClass.Login;

                SetUserRole.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                SetUserRole.Parameters["@login"].Value = userlogin;

                SetUserRole.Parameters.Add("@role", System.Data.SqlDbType.Int);
                SetUserRole.Parameters["@role"].Value = newrole == UserClass.Role.Admin ? 1 : 0;

                SetUserRole.Parameters.Add("@response", System.Data.SqlDbType.Int);
                SetUserRole.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    SetUserRole.ExecuteNonQuery();
                }

                switch (Convert.ToInt32(SetUserRole.Parameters["@response"].Value))
                {
                    case -1: return UserClass.OperationStatus.NotAdmin;
                    case 0: return UserClass.OperationStatus.LoginError;
                    case 1: return UserClass.OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminChangeUserRole Error: {ex.Message.ToString()}");
            }
            return UserClass.OperationStatus.DBError;
        }

        internal static UserClass.OperationStatus ChangeUserLogin(char[] olduserlogin, char[] newuserlogin)
        {
            try
            {

                System.Data.SqlClient.SqlCommand SetUserLogin = new System.Data.SqlClient.SqlCommand("SetUserLogin", DBConnection.connection);
                SetUserLogin.CommandType = System.Data.CommandType.StoredProcedure;

                SetUserLogin.Parameters.Add("@adminlogin", System.Data.SqlDbType.NVarChar, 50);
                SetUserLogin.Parameters["@adminlogin"].Value = UserClass.Login;

                SetUserLogin.Parameters.Add("@oldlogin", System.Data.SqlDbType.NVarChar, 50);
                SetUserLogin.Parameters["@oldlogin"].Value = olduserlogin;

                SetUserLogin.Parameters.Add("@newLogin", System.Data.SqlDbType.NVarChar, 50);
                SetUserLogin.Parameters["@newLogin"].Value = newuserlogin;

                SetUserLogin.Parameters.Add("@response", System.Data.SqlDbType.Int);
                SetUserLogin.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;
                using (DBConnection connection = new DBConnection())
                {
                    SetUserLogin.ExecuteNonQuery();
                }
                switch (Convert.ToInt32(SetUserLogin.Parameters["@response"].Value))
                {
                    case -1: return UserClass.OperationStatus.NotAdmin;
                    case 0: return UserClass.OperationStatus.LoginError;
                    case 1: return UserClass.OperationStatus.LoginExists;
                    case 2: return UserClass.OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminChangeUserLogin Error: {ex.Message.ToString()}");
            }
            return UserClass.OperationStatus.DBError;
        }

        internal static UserClass.OperationStatus BlockUser(char[] login, int state)
        {
            try
            {

                System.Data.SqlClient.SqlCommand SetUserBlock = new System.Data.SqlClient.SqlCommand("SetUserBlock", DBConnection.connection);
                SetUserBlock.CommandType = System.Data.CommandType.StoredProcedure;

                SetUserBlock.Parameters.Add("@adminlogin", System.Data.SqlDbType.NVarChar, 50);
                SetUserBlock.Parameters["@adminlogin"].Value = UserClass.Login;

                SetUserBlock.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                SetUserBlock.Parameters["@login"].Value = login;

                SetUserBlock.Parameters.Add("@block", System.Data.SqlDbType.Int);
                SetUserBlock.Parameters["@block"].Value = state;

                SetUserBlock.Parameters.Add("@response", System.Data.SqlDbType.Int);
                SetUserBlock.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    SetUserBlock.ExecuteNonQuery();
                }

                switch (Convert.ToInt32(SetUserBlock.Parameters["@response"].Value))
                {
                    case -1: return UserClass.OperationStatus.NotAdmin;
                    case 0: return UserClass.OperationStatus.LoginError;
                    case 1: return UserClass.OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminBlockUser Error: {ex.Message.ToString()}");
            }
            return UserClass.OperationStatus.DBError;
        }

        internal static List<UserModel> GetAllUsers()
        {
            List<UserModel> userModels = new List<UserModel>();
            try
            {

                var resultTable = new DataTable();
                System.Data.SqlClient.SqlCommand AdminGetAllUsers = new System.Data.SqlClient.SqlCommand("SELECT * FROM AdminGetAllUsers(@adminLogin)", DBConnection.connection);
                AdminGetAllUsers.CommandType = System.Data.CommandType.Text;

                AdminGetAllUsers.Parameters.Add("@adminLogin", System.Data.SqlDbType.NVarChar, 50);
                AdminGetAllUsers.Parameters["@adminLogin"].Value = UserClass.Login;

                using (DBConnection connection = new DBConnection())
                {
                    resultTable.Load(AdminGetAllUsers.ExecuteReader());
                }


                foreach (DataRow dataRow in resultTable.Rows)
                {
                    userModels.Add(new UserModel(Convert.ToString(dataRow["ulogin"]), Convert.ToInt32(dataRow["urole"]), Convert.ToInt32(dataRow["blocked"]) == 1));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminGetAllUsers Error: {ex.Message.ToString()}");
            }
            return userModels;
        }
    }

    public class UserModel : INotifyPropertyChanged
    {
        private string login_;
        private string role_;
        private bool blocked_;

        public string Login { set { login_ = value; OnPropertyChanged("Login"); } get { return login_; } }
        public string Role { set { role_ = value; OnPropertyChanged("Role"); } get { return role_; } }
        public bool Blocked { set { blocked_ = value; OnPropertyChanged("Blocked"); } get { return blocked_; } }

        public UserModel(string login, int role, bool blocked)
        {
            Login = login;
            Role = role == 1 ? "Администратор" : "Пользователь";
            Blocked = blocked;
        }

        internal void BlockUser(bool state)
        {
            if (Login != UserClass.Login)
            {
                if (Blocked != state)
                {
                    var opResult = AdminToolsClass.BlockUser(Login.ToCharArray(), state ? 1 : 0);

                    if (opResult != UserClass.OperationStatus.Successful)
                    {
                        MessageBox.Show(UserClass.OperationStatusToString(opResult));
                    }
                    else
                    {
                        Blocked = state;
                    }

                }
            }
            else
            {
                MessageBox.Show("Вы не можете заблокировать себя!");
            }
        }

        internal void ChangeRole(string role)
        {
            if (Login != UserClass.Login)
            {
                if (Role != role)
                {
                    var opResult = AdminToolsClass.ChangeUserRole(Login.ToCharArray(), role == "Пользователь" ? UserClass.Role.Regular : UserClass.Role.Admin);

                    if (opResult != UserClass.OperationStatus.Successful)
                    {
                        MessageBox.Show(UserClass.OperationStatusToString(opResult));
                    }
                    else
                    {
                        Role = role;
                    }

                }
            }
            else
            {
                MessageBox.Show("Вы не можете изменить свою роль!");
            }
        }

        internal void ChangeLogin(string newlogin)
        {
            if (Login != newlogin)
            {
                var msg = "";
                if (InputsCheckClass.loginCheck(newlogin, out msg))
                {
                    var opResult = AdminToolsClass.ChangeUserLogin(Login.ToCharArray(), newlogin.ToCharArray());

                    if (opResult != UserClass.OperationStatus.Successful)
                    {
                        MessageBox.Show(UserClass.OperationStatusToString(opResult));
                    }
                    else
                    {
                        Login = new string(newlogin);
                    }
                }
                else
                {
                    MessageBox.Show(msg);
                }

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
