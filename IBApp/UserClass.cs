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

        private static OperationStatus Authorization(char[] login, char[] pwd)
        {
            try
            {
                System.Data.SqlClient.SqlCommand GetUserSalt = new System.Data.SqlClient.SqlCommand("GetUserSalt", DBConnection.connection);
                GetUserSalt.CommandType = System.Data.CommandType.StoredProcedure;

                GetUserSalt.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                GetUserSalt.Parameters["@login"].Value = login;

                GetUserSalt.Parameters.Add("@usalt", System.Data.SqlDbType.VarBinary, 256);
                GetUserSalt.Parameters["@usalt"].Direction = System.Data.ParameterDirection.Output;

                GetUserSalt.Parameters.Add("@response", System.Data.SqlDbType.Int);
                GetUserSalt.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    GetUserSalt.ExecuteNonQuery();
                }

                if (GetUserSalt.Parameters["@usalt"].Value == System.DBNull.Value)
                {
                    if (Convert.ToInt32(GetUserSalt.Parameters["@response"].Value) == 0)
                        return OperationStatus.BlockedUser; //пользователь заблокирован
                    else
                        return OperationStatus.LoginError; //неправильный логин
                }
                else
                {
                    var salt = (byte[])GetUserSalt.Parameters["@usalt"].Value;
                    var hash = CryptoClass.HashSome(salt.Concat(Encoding.UTF8.GetBytes(pwd)).ToArray());

                    System.Data.SqlClient.SqlCommand CheckUserPwd = new System.Data.SqlClient.SqlCommand("CheckUserPwd", DBConnection.connection);
                    CheckUserPwd.CommandType = System.Data.CommandType.StoredProcedure;

                    CheckUserPwd.Parameters.Add("@data", System.Data.SqlDbType.VarBinary, hash.Length);
                    CheckUserPwd.Parameters["@data"].Value = hash;
                    CheckUserPwd.Parameters.Add("@response", System.Data.SqlDbType.Int);
                    CheckUserPwd.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                    using (DBConnection connection = new DBConnection())
                    {
                        CheckUserPwd.ExecuteNonQuery();
                    }

                    if (Convert.ToInt32(CheckUserPwd.Parameters["@response"].Value) == 0)
                    {
                        return OperationStatus.PwdError; //неправильный пароль
                    }
                    else
                    {
                        return OperationStatus.Successful; //успешно
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authorization Error: {ex.Message.ToString()}");
            }
            return OperationStatus.DBError; //ошибка при подключении к бд
        }

        internal static OperationStatus TryLogin(char[] login, char[] pwd)
        {
            try
            {
                var authorizationStatus = Authorization(login, pwd);
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

        internal static OperationStatus TryRegister(char[] login, char[] pwd)
        {
            try
            {

                System.Data.SqlClient.SqlCommand CheckNewLogin = new System.Data.SqlClient.SqlCommand("CheckNewLogin", DBConnection.connection);
                CheckNewLogin.CommandType = System.Data.CommandType.StoredProcedure;

                CheckNewLogin.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                CheckNewLogin.Parameters["@login"].Value = login;

                CheckNewLogin.Parameters.Add("@response", System.Data.SqlDbType.Int);
                CheckNewLogin.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    CheckNewLogin.ExecuteNonQuery();
                }

                if (Convert.ToInt32(CheckNewLogin.Parameters["@response"].Value) == 1)
                    return OperationStatus.LoginError;
                else
                {
                    bool newHashUnique = false;
                    byte[] newsalt;
                    do
                    {
                        newsalt = CryptoClass.GetSalt();
                        var newhash = CryptoClass.HashSome(newsalt.Concat(Encoding.UTF8.GetBytes(pwd)).ToArray());

                        System.Data.SqlClient.SqlCommand AddPwdHash = new System.Data.SqlClient.SqlCommand("AddPwdHash", DBConnection.connection);
                        AddPwdHash.CommandType = System.Data.CommandType.StoredProcedure;

                        AddPwdHash.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary, newhash.Length);
                        AddPwdHash.Parameters["@hash"].Value = newhash;

                        AddPwdHash.Parameters.Add("@response", System.Data.SqlDbType.Int);
                        AddPwdHash.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                        using (DBConnection connection = new DBConnection())
                        {
                            AddPwdHash.ExecuteNonQuery();
                        }

                        newHashUnique = Convert.ToInt32(AddPwdHash.Parameters["@response"].Value) == 1;
                    }
                    while (!newHashUnique);

                    System.Data.SqlClient.SqlCommand UpdateUserSalt = new System.Data.SqlClient.SqlCommand("UpdateUserSalt", DBConnection.connection);
                    UpdateUserSalt.CommandType = System.Data.CommandType.StoredProcedure;

                    UpdateUserSalt.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                    UpdateUserSalt.Parameters["@login"].Value = login;

                    UpdateUserSalt.Parameters.Add("@salt", System.Data.SqlDbType.VarBinary, 256);
                    UpdateUserSalt.Parameters["@salt"].Value = newsalt;

                    using (DBConnection connection = new DBConnection())
                    {
                        UpdateUserSalt.ExecuteNonQuery();
                    }

                    Login = new string(login);
                    UserRole = Role.Regular;

                    return OperationStatus.Successful;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration Error: {ex.Message.ToString()}");
            }
            return OperationStatus.DBError;
        }

        internal static OperationStatus ChangePwd(char[] oldPwd, char[] newPwd)
        {
            try
            {
                System.Data.SqlClient.SqlCommand GetUserSalt = new System.Data.SqlClient.SqlCommand("GetUserSalt", DBConnection.connection);
                GetUserSalt.CommandType = System.Data.CommandType.StoredProcedure;

                GetUserSalt.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                GetUserSalt.Parameters["@login"].Value = Login;

                GetUserSalt.Parameters.Add("@usalt", System.Data.SqlDbType.VarBinary, 256);
                GetUserSalt.Parameters["@usalt"].Direction = System.Data.ParameterDirection.Output;

                GetUserSalt.Parameters.Add("@response", System.Data.SqlDbType.Int);
                GetUserSalt.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                using (DBConnection connection = new DBConnection())
                {
                    GetUserSalt.ExecuteNonQuery();
                }

                if (GetUserSalt.Parameters["@usalt"].Value == System.DBNull.Value)
                {
                    if (Convert.ToInt32(GetUserSalt.Parameters["@response"].Value) == 0)
                        return OperationStatus.BlockedUser;
                    else
                        return OperationStatus.LoginError;
                }
                else
                {
                    var salt = (byte[])GetUserSalt.Parameters["@usalt"].Value;
                    var hash = CryptoClass.HashSome(salt.Concat(Encoding.UTF8.GetBytes(oldPwd)).ToArray());

                    System.Data.SqlClient.SqlCommand RemovePwdHash = new System.Data.SqlClient.SqlCommand("RemovePwdHash", DBConnection.connection);
                    RemovePwdHash.CommandType = System.Data.CommandType.StoredProcedure;

                    RemovePwdHash.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary, hash.Length);
                    RemovePwdHash.Parameters["@hash"].Value = hash;

                    RemovePwdHash.Parameters.Add("@response", System.Data.SqlDbType.Int);
                    RemovePwdHash.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                    using (DBConnection connection = new DBConnection())
                    {
                        RemovePwdHash.ExecuteNonQuery();
                    }

                    if (Convert.ToInt32(RemovePwdHash.Parameters["@response"].Value) == -1)
                        return OperationStatus.PwdError;
                    else
                    {
                        bool newHashUnique = false;
                        byte[] newsalt;
                        do
                        {
                            newsalt = CryptoClass.GetSalt();
                            var newhash = CryptoClass.HashSome(newsalt.Concat(Encoding.UTF8.GetBytes(newPwd)).ToArray());

                            System.Data.SqlClient.SqlCommand AddPwdHash = new System.Data.SqlClient.SqlCommand("AddPwdHash", DBConnection.connection);
                            AddPwdHash.CommandType = System.Data.CommandType.StoredProcedure;

                            AddPwdHash.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary, newhash.Length);
                            AddPwdHash.Parameters["@hash"].Value = newhash;

                            AddPwdHash.Parameters.Add("@response", System.Data.SqlDbType.Int);
                            AddPwdHash.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                            using (DBConnection connection = new DBConnection())
                            {
                                AddPwdHash.ExecuteNonQuery();
                            }

                            newHashUnique = Convert.ToInt32(AddPwdHash.Parameters["@response"].Value) == 1;
                        }
                        while (!newHashUnique);

                        System.Data.SqlClient.SqlCommand UpdateUserSalt = new System.Data.SqlClient.SqlCommand("UpdateUserSalt", DBConnection.connection);
                        UpdateUserSalt.CommandType = System.Data.CommandType.StoredProcedure;

                        UpdateUserSalt.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                        UpdateUserSalt.Parameters["@login"].Value = Login;

                        UpdateUserSalt.Parameters.Add("@salt", System.Data.SqlDbType.VarBinary, 256);
                        UpdateUserSalt.Parameters["@salt"].Value = newsalt;

                        using (DBConnection connection = new DBConnection())
                        {
                            UpdateUserSalt.ExecuteNonQuery();
                        }
                        return OperationStatus.Successful;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pwd change error {ex.Message}");
            }
            return OperationStatus.DBError;
        }

        internal static OperationStatus ChangeLogin(char[] newlogin, char[] pwd)
        {
            try
            {
                var authorizationStatus = Authorization(Login.ToCharArray(), pwd);
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
