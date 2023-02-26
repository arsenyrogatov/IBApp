using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBApp
{
    internal static class UserClass
    {
        internal static string? Login { private set; get; }
        internal static Role? UserRole { private set; get; }

        public enum Role
        {
            Regular,
            Admin
        }

        public enum OperationStatus
        {
            DBError,
            LoginError,
            PwdError,
            Successful,
            NotAdmin,
            BlockedUser,
            LoginExists
        }

        internal static string OperationStatusToString(OperationStatus status)
        {
            switch (status)
            {
                case OperationStatus.DBError: return "Ошибка при обращении к серверу";
                case OperationStatus.LoginError: return "Неправильный логин";
                case OperationStatus.PwdError: return "Неправильный пароль";
                case OperationStatus.NotAdmin: return "Вы не являетесь администратором";
                case OperationStatus.BlockedUser: return "Пользователь заблокирован";
                case OperationStatus.LoginExists: return "Логин занят другим пользователем";
            }
            return "";
        }

        private static OperationStatus Authorization2(char[] login, char[] pwd)
        {
            try
            {
                System.Data.SqlClient.SqlCommand AuthorizeUser = new System.Data.SqlClient.SqlCommand("AuthorizeUser", DBConnection.connection);
                AuthorizeUser.CommandType = System.Data.CommandType.StoredProcedure;

                AuthorizeUser.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                AuthorizeUser.Parameters["@login"].Value = login;

                AuthorizeUser.Parameters.Add("@data", System.Data.SqlDbType.VarBinary, 256);
                AuthorizeUser.Parameters["@data"].Value = CryptoClass.HashSome(Encoding.UTF8.GetBytes(pwd));

                AuthorizeUser.Parameters.Add("@response", System.Data.SqlDbType.Int);
                AuthorizeUser.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    AuthorizeUser.ExecuteNonQuery();
                }

                switch (Convert.ToInt32(AuthorizeUser.Parameters["@response"].Value))
                {
                    case -1: return OperationStatus.PwdError;
                    case -2: return OperationStatus.LoginError;
                    case -3: return OperationStatus.BlockedUser;
                    case 0: return OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authorization2 Error: {ex.Message.ToString()}");
            }
            return OperationStatus.DBError; //ошибка при подключении к бд
        }

        internal static OperationStatus Register2(char[] login, char[] pwd)
        {
            try
            {
                System.Data.SqlClient.SqlCommand RegisterUser = new System.Data.SqlClient.SqlCommand("RegisterUser", DBConnection.connection);
                RegisterUser.CommandType = System.Data.CommandType.StoredProcedure;

                RegisterUser.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                RegisterUser.Parameters["@login"].Value = login;

                RegisterUser.Parameters.Add("@data", System.Data.SqlDbType.VarBinary, 256);
                RegisterUser.Parameters["@data"].Value = CryptoClass.HashSome(Encoding.UTF8.GetBytes(pwd));

                RegisterUser.Parameters.Add("@response", System.Data.SqlDbType.Int);
                RegisterUser.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    RegisterUser.ExecuteNonQuery();
                }

                switch (Convert.ToInt32(RegisterUser.Parameters["@response"].Value))
                {
                    case -1: return OperationStatus.LoginExists;
                    case 0: return OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authorization2 Error: {ex.Message.ToString()}");
            }
            return OperationStatus.DBError; //ошибка при подключении к бд
        }

        internal static OperationStatus Login2(char[] login, char[] pwd)
        {
            try
            {
                var authorizationStatus = Authorization2(login, pwd);
                if (authorizationStatus != OperationStatus.Successful)
                {
                    return authorizationStatus;
                }
                else
                {
                    System.Data.SqlClient.SqlCommand GetUserInfo = new System.Data.SqlClient.SqlCommand("GetUserInfo", DBConnection.connection);
                    GetUserInfo.CommandType = System.Data.CommandType.StoredProcedure;

                    GetUserInfo.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                    GetUserInfo.Parameters["@login"].Value = login;

                    GetUserInfo.Parameters.Add("@role", System.Data.SqlDbType.Int);
                    GetUserInfo.Parameters["@role"].Direction = System.Data.ParameterDirection.Output;

                    using (DBConnection connection = new DBConnection())
                    {
                        GetUserInfo.ExecuteNonQuery();
                    }

                    Login = new string(login);
                    UserRole = Convert.ToInt32(GetUserInfo.Parameters["@role"].Value) == 0 ? Role.Regular : Role.Admin;
                    return OperationStatus.Successful;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message.ToString()}");
            }
            return OperationStatus.DBError;
        } 

        internal static OperationStatus ChangePwd2(char[] oldPwd, char[] newPwd)
        {
            try
            {
                System.Data.SqlClient.SqlCommand ChangePassword = new System.Data.SqlClient.SqlCommand("ChangePassword", DBConnection.connection);
                ChangePassword.CommandType = System.Data.CommandType.StoredProcedure;

                ChangePassword.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                ChangePassword.Parameters["@login"].Value = Login;

                ChangePassword.Parameters.Add("@olddata", System.Data.SqlDbType.VarBinary, 256);
                ChangePassword.Parameters["@olddata"].Value = CryptoClass.HashSome(Encoding.UTF8.GetBytes(oldPwd));

                ChangePassword.Parameters.Add("@newdata", System.Data.SqlDbType.VarBinary, 256);
                ChangePassword.Parameters["@newdata"].Value = CryptoClass.HashSome(Encoding.UTF8.GetBytes(newPwd));

                ChangePassword.Parameters.Add("@response", System.Data.SqlDbType.Int);
                ChangePassword.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    ChangePassword.ExecuteNonQuery();
                }

                switch (Convert.ToInt32(ChangePassword.Parameters["@response"].Value))
                {
                    case -1: return OperationStatus.PwdError;
                    case -2: return OperationStatus.LoginError;
                    case -3: return OperationStatus.BlockedUser;
                    case 0: return OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangePwd2 Error: {ex.Message.ToString()}");
            }
            return OperationStatus.DBError; //ошибка при подключении к бд
        }

        internal static OperationStatus ChangeLogin2(char[] newlogin, char[] pwd)
        {
            try
            {
                var authorizationStatus = Authorization2(Login.ToCharArray(), pwd);
                if (authorizationStatus != OperationStatus.Successful)
                {
                    return authorizationStatus;
                }
                else
                {
                    System.Data.SqlClient.SqlCommand ChangeLogin = new System.Data.SqlClient.SqlCommand("ChangeLogin", DBConnection.connection);
                    ChangeLogin.CommandType = System.Data.CommandType.StoredProcedure;

                    ChangeLogin.Parameters.Add("@newlogin", System.Data.SqlDbType.NVarChar, 50);
                    ChangeLogin.Parameters["@newlogin"].Value = newlogin;

                    ChangeLogin.Parameters.Add("@oldlogin", System.Data.SqlDbType.NVarChar, 50);
                    ChangeLogin.Parameters["@oldlogin"].Value = Login;

                    ChangeLogin.Parameters.Add("@response", System.Data.SqlDbType.Int);
                    ChangeLogin.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                    using (DBConnection connection = new DBConnection())
                    {
                        ChangeLogin.ExecuteNonQuery();
                    }

                    if (Convert.ToInt32(ChangeLogin.Parameters["@response"].Value) == 0)
                        return OperationStatus.LoginExists; //пользователь заблокирован
                    else
                    {
                        Login = new string(newlogin);
                        return OperationStatus.Successful; //неправильный логин
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login change error {ex.Message}");
            }
            return OperationStatus.DBError;
        }

        internal static void LogOut()
        {
            Login = null;
            UserRole = null;
        }
    }
}
