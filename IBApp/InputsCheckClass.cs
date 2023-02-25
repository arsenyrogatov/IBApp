using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IBApp
{
    internal static class InputsCheckClass
    {
        internal static bool loginCheck(string login, out string message ,int minLen = 3, int maxLen = 20)
        {
            if (login.Length < minLen)
            {
                message = $"Логин должен быть больше {minLen} символов!";
                return false;
            }
            if (login.Length > maxLen)
            {
                message = $"Логин должен быть меньше {maxLen} символов!";
                return false;
            }

            var forbiddenSymbols = new string[] { ";", "\'", "--", "/*", "*/", "xp_" };
            foreach (var forbiddenSymbol in forbiddenSymbols)
            {
                if (login.Contains(forbiddenSymbol))
                {
                    message = $"Логин содержит запрещенный символ {forbiddenSymbol}";
                    return false;
                }
            }

            message = "";
            return true;
        }

        internal static bool pwdCheck(string pwd, out string message, int minLen = 8, int maxLen = 50)
        {
            if (pwd.Length < 8)
            {
                message = $"Пароль должен быть больше {minLen} символов!";
                return false;
            }
            if (pwd.Length > 50)
            {
                message = $"Пароль должен быть меньше {maxLen} символов!";
                return false;
            }

            var engCapsRegex = new Regex(@"([A-Z])");
            var rusCapsRegex = new Regex(@"([А-Я])");
            if (!engCapsRegex.IsMatch(pwd) && !rusCapsRegex.IsMatch(pwd))
            {
                message = "Пароль должен содержать прописные буквы!";
                return false;
            }

            var digitsRegex = new Regex(@"([0-9])");
            if (!digitsRegex.IsMatch(pwd))
            {
                message = "Пароль должен содержать цифры!";
                return false;
            }

            var symbolsRegex = new Regex(@"([!,@,#,$,%,^,&,*,?,_,~])");
            if (!symbolsRegex.IsMatch(pwd))
            {
                message = "Пароль должен содержать спец символы!";
                return false;
            }

            message = "";
            return true;
        }
    }
}
