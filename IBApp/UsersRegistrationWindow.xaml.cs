using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IBApp
{
    /// <summary>
    /// Логика взаимодействия для UsersRegistrationWindow.xaml
    /// </summary>
    public partial class UsersRegistrationWindow : Window
    {
        public UsersRegistrationWindow()
        {
            InitializeComponent();
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (pwdbox1.Visibility == Visibility.Hidden)
            {
                pwdbox1.Password = pwdbox1s.Text;
                pwdbox2.Password = pwdbox2s.Text;
            }
            statuslabel.Text = "";
            if (checkInputs())
            {
                var regStatus = UserClass.TryRegister(loginbox.Text.ToCharArray(), pwdbox1.Password.ToCharArray());
                if (regStatus == UserClass.OperationStatus.LoginError)
                    statuslabel.Text = "Пользователь с таким логином уже есть!";
                if (regStatus == UserClass.OperationStatus.DBError)
                    statuslabel.Text = "Ошибка при обращении к серверу";
                if (regStatus == UserClass.OperationStatus.Successful)
                {
                    MessageBox.Show("Вы успешно зарегистрированы!");
                }
            }

        }

        private bool checkInputs ()
        {
            if (loginbox.Text.Length < 3)
            {
                statuslabel.Text = "Логин должен быть больше 3 символов!";
                return false;
            }
            if (loginbox.Text.Length > 20)
            {
                statuslabel.Text = "Логин должен быть меньше 20 символов!";
                return false;
            }
            if (pwdbox1.Password != pwdbox2.Password)
            {
                statuslabel.Text = "Пароли не совпадают!";
                return false;
            }
            if (pwdbox1.Password.Length < 8)
            {
                statuslabel.Text = "Пароль должен быть больше 8 символов!";
                return false;
            }
            if (pwdbox1.Password.Length > 50)
            {
                statuslabel.Text = "Пароль должен быть меньше 50 символов!";
                return false;
            }

            if (pwdbox1.Password == loginbox.Text || pwdbox1.Password == loginbox.Text.Reverse().ToString())
            {
                statuslabel.Text = "Пароль не должен совпадать с логином!";
                return false;
            }

            var forbiddenSymbols = new string[] { ";", "\'", "--", "/*", "*/", "xp_" };
            foreach (var forbiddenSymbol in forbiddenSymbols)
            {
                if (loginbox.Text.Contains(forbiddenSymbol))
                {
                    statuslabel.Text = $"Логин содержит запрещенный символ {forbiddenSymbol}";
                    return false;
                }
            }

            var engCapsRegex = new Regex(@"([A-Z])");
            var rusCapsRegex = new Regex(@"([А-Я])");
            if (!engCapsRegex.IsMatch(pwdbox1.Password) && !rusCapsRegex.IsMatch(pwdbox1.Password))
            {
                statuslabel.Text = "Пароль должен содержать прописные буквы!";
                return false;
            }

            var digitsRegex = new Regex(@"([0-9])");
            if (!digitsRegex.IsMatch(pwdbox1.Password))
            {
                statuslabel.Text = "Пароль должен содержать цифры!";
                return false;
            }

            var symbolsRegex = new Regex(@"([!,@,#,$,%,^,&,*,?,_,~])");
            if (!symbolsRegex.IsMatch(pwdbox1.Password))
            {
                statuslabel.Text = "Пароль должен содержать спец символы!";
                return false;
            }

            return true;
        }

        private void ShowPasswords(object sender, RoutedEventArgs e)
        {
                pwdbox1s.Text = pwdbox1.Password;
                pwdbox2s.Text = pwdbox2.Password;
                pwdbox1.Visibility = Visibility.Hidden;
                pwdbox2.Visibility = Visibility.Hidden;
                pwdbox1s.Visibility = Visibility.Visible;
                pwdbox2s.Visibility = Visibility.Visible;
        }

        private void HidePasswords(object sender, RoutedEventArgs e)
        {
                pwdbox1.Password = pwdbox1s.Text;
                pwdbox2.Password = pwdbox2s.Text;
                pwdbox1s.Visibility = Visibility.Hidden;
                pwdbox2s.Visibility = Visibility.Hidden;
                pwdbox1.Visibility = Visibility.Visible;
                pwdbox2.Visibility = Visibility.Visible;
        }
    }
}
