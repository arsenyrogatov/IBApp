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
            Successful
        }

        internal static OperationStatus TryLogin(char[] login, char[] pwd)
        {
            try
            {
                using (DBConnection connection = new DBConnection())
                {
                    System.Data.SqlClient.SqlCommand GetUserSalt = new System.Data.SqlClient.SqlCommand("GetUserSalt", DBConnection.connection);
                    GetUserSalt.CommandType = System.Data.CommandType.StoredProcedure;

                    GetUserSalt.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                    GetUserSalt.Parameters["@login"].Value = login;

                    GetUserSalt.Parameters.Add("@usalt", System.Data.SqlDbType.VarBinary, 256);
                    GetUserSalt.Parameters["@usalt"].Direction = System.Data.ParameterDirection.Output;

                    GetUserSalt.ExecuteNonQuery();

                    if (GetUserSalt.Parameters["@usalt"].Value == System.DBNull.Value)
                        return OperationStatus.LoginError;
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

                        CheckUserPwd.ExecuteNonQuery();

                        if (Convert.ToInt32(CheckUserPwd.Parameters["@response"].Value) == 0)
                        {
                            return OperationStatus.PwdError;
                        }
                        else
                        {
                            System.Data.SqlClient.SqlCommand GetUserInfo = new System.Data.SqlClient.SqlCommand("GetUserInfo", DBConnection.connection);
                            GetUserInfo.CommandType = System.Data.CommandType.StoredProcedure;

                            GetUserInfo.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                            GetUserInfo.Parameters["@login"].Value = login;

                            GetUserInfo.Parameters.Add("@role", System.Data.SqlDbType.Int);
                            GetUserInfo.Parameters["@role"].Direction = System.Data.ParameterDirection.Output;

                            GetUserInfo.ExecuteNonQuery();

                            Login = new string(login);
                            UserRole = Convert.ToInt32(GetUserInfo.Parameters["@role"].Value) == 0 ? Role.Regular : Role.Admin;
                            return OperationStatus.Successful;
                        }
                    }
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
                using (DBConnection connection = new DBConnection())
                {
                    System.Data.SqlClient.SqlCommand CheckNewLogin = new System.Data.SqlClient.SqlCommand("CheckNewLogin", DBConnection.connection);
                    CheckNewLogin.CommandType = System.Data.CommandType.StoredProcedure;

                    CheckNewLogin.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                    CheckNewLogin.Parameters["@login"].Value = login;

                    CheckNewLogin.Parameters.Add("@response", System.Data.SqlDbType.Int);
                    CheckNewLogin.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                    CheckNewLogin.ExecuteNonQuery();

                    if (Convert.ToInt32(CheckNewLogin.Parameters["@response"].Value) > 1)
                        return OperationStatus.LoginError;
                    else
                    {
                        var salt = CryptoClass.GetSalt();
                        var hash = CryptoClass.HashSome(salt.Concat(Encoding.UTF8.GetBytes(pwd)).ToArray());

                        System.Data.SqlClient.SqlCommand AddUser = new System.Data.SqlClient.SqlCommand("AddUser", DBConnection.connection);
                        AddUser.CommandType = System.Data.CommandType.StoredProcedure;

                        AddUser.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                        AddUser.Parameters["@login"].Value = login;

                        AddUser.Parameters.Add("@salt", System.Data.SqlDbType.VarBinary, 256);
                        AddUser.Parameters["@salt"].Value = salt;

                        AddUser.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary, hash.Length);
                        AddUser.Parameters["@hash"].Value = hash;

                        AddUser.ExecuteNonQuery();

                        Login = login.ToString();
                        UserRole = Role.Regular;

                        return OperationStatus.Successful;
                    }
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
                using (DBConnection connection = new DBConnection())
                {

                    System.Data.SqlClient.SqlCommand GetUserSalt = new System.Data.SqlClient.SqlCommand("GetUserSalt", DBConnection.connection);
                    GetUserSalt.CommandType = System.Data.CommandType.StoredProcedure;

                    GetUserSalt.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                    GetUserSalt.Parameters["@login"].Value = Login;

                    GetUserSalt.Parameters.Add("@usalt", System.Data.SqlDbType.VarBinary, 256);
                    GetUserSalt.Parameters["@usalt"].Direction = System.Data.ParameterDirection.Output;

                    GetUserSalt.ExecuteNonQuery();

                    if (GetUserSalt.Parameters["@usalt"].Value == System.DBNull.Value)
                        return OperationStatus.LoginError;
                    else
                    {
                        var salt = (byte[])GetUserSalt.Parameters["@usalt"].Value;
                        var hash = CryptoClass.HashSome(salt.Concat(Encoding.UTF8.GetBytes(oldPwd)).ToArray());

                        var newsalt = CryptoClass.GetSalt();
                        var newhash = CryptoClass.HashSome(newsalt.Concat(Encoding.UTF8.GetBytes(newPwd)).ToArray());

                        System.Data.SqlClient.SqlCommand ChangePwd = new System.Data.SqlClient.SqlCommand("ChangePwd", DBConnection.connection);
                        ChangePwd.CommandType = System.Data.CommandType.StoredProcedure;

                        ChangePwd.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                        ChangePwd.Parameters["@login"].Value = Login;

                        ChangePwd.Parameters.Add("@oldP", System.Data.SqlDbType.VarBinary, hash.Length);
                        ChangePwd.Parameters["@oldP"].Value = hash;

                        ChangePwd.Parameters.Add("@newP", System.Data.SqlDbType.VarBinary, newhash.Length);
                        ChangePwd.Parameters["@newP"].Value = newhash;

                        ChangePwd.Parameters.Add("@newSalt", System.Data.SqlDbType.VarBinary, 256);
                        ChangePwd.Parameters["@newSalt"].Value = newsalt;

                        ChangePwd.Parameters.Add("@response", System.Data.SqlDbType.Int);
                        ChangePwd.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                        ChangePwd.ExecuteNonQuery();

                        switch (Convert.ToInt32(ChangePwd.Parameters["@response"].Value))
                        {
                            case -1: return OperationStatus.PwdError;
                            case 1: return OperationStatus.Successful;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pwd change error {ex.Message}");
            }
            return OperationStatus.DBError;
        }

        internal static OperationStatus ChangeLogin(char[] newlogin)
        {
            try
            {
                using (DBConnection connection = new DBConnection())
                {
                    System.Data.SqlClient.SqlCommand CheckNewLogin = new System.Data.SqlClient.SqlCommand("CheckNewLogin", DBConnection.connection);
                    CheckNewLogin.CommandType = System.Data.CommandType.StoredProcedure;

                    CheckNewLogin.Parameters.Add("@login", System.Data.SqlDbType.NVarChar, 50);
                    CheckNewLogin.Parameters["@login"].Value = newlogin;

                    CheckNewLogin.Parameters.Add("@response", System.Data.SqlDbType.Int);
                    CheckNewLogin.Parameters["@response"].Direction = System.Data.ParameterDirection.Output;

                    CheckNewLogin.ExecuteNonQuery();

                    if (Convert.ToInt32(CheckNewLogin.Parameters["@response"].Value) > 1)
                        return OperationStatus.LoginError;
                    else
                    {
                        
                        System.Data.SqlClient.SqlCommand ChangeLogin = new System.Data.SqlClient.SqlCommand("ChangeLogin", DBConnection.connection);
                        ChangeLogin.CommandType = System.Data.CommandType.StoredProcedure;

                        ChangeLogin.Parameters.Add("@newlogin", System.Data.SqlDbType.NVarChar, 50);
                        ChangeLogin.Parameters["@newlogin"].Value = newlogin;

                        ChangeLogin.Parameters.Add("@oldlogin", System.Data.SqlDbType.NVarChar, 50);
                        ChangeLogin.Parameters["@oldlogin"].Value = Login;

                        ChangeLogin.ExecuteNonQuery();

                        Login = newlogin.ToString();

                        return OperationStatus.Successful;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login change error {ex.Message}");
            }
            return OperationStatus.DBError;
        }

        internal static void LogOut ()
        {
            Login = null;
            UserRole = null;
        }
    }
}
