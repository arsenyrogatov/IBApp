using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для ChangePwdWindow.xaml
    /// </summary>
    public partial class ChangePwdWindow : Window
    {
        public ChangePwdWindow()
        {
            InitializeComponent();
            loginbox.Text = UserClass.Login;
        }

        private void ShowPasswords(object sender, RoutedEventArgs e)
        {
            pwdbox0s.Text = pwdbox0.Password;
            pwdbox1s.Text = pwdbox1.Password;
            pwdbox2s.Text = pwdbox2.Password;
            pwdbox0.Visibility = Visibility.Hidden;
            pwdbox1.Visibility = Visibility.Hidden;
            pwdbox2.Visibility = Visibility.Hidden;
            pwdbox0s.Visibility = Visibility.Visible;
            pwdbox1s.Visibility = Visibility.Visible;
            pwdbox2s.Visibility = Visibility.Visible;
        }

        private void HidePasswords(object sender, RoutedEventArgs e)
        {
            pwdbox1.Password = pwdbox1s.Text;
            pwdbox2.Password = pwdbox2s.Text;
            pwdbox0.Password = pwdbox0s.Text;
            pwdbox0s.Visibility = Visibility.Hidden;
            pwdbox1s.Visibility = Visibility.Hidden;
            pwdbox2s.Visibility = Visibility.Hidden;
            pwdbox0.Visibility = Visibility.Visible;
            pwdbox1.Visibility = Visibility.Visible;
            pwdbox2.Visibility = Visibility.Visible;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (pwdbox1.Visibility == Visibility.Hidden)
            {
                pwdbox0.Password = pwdbox0s.Text;
                pwdbox1.Password = pwdbox1s.Text;
                pwdbox2.Password = pwdbox2s.Text;
            }
            statuslabel.Text = "";
            if (checkInputs())
            {
                var regStatus = UserClass.ChangePwd(pwdbox0.Password.ToCharArray(), pwdbox1.Password.ToCharArray());
                if (regStatus == UserClass.OperationStatus.PwdError)
                    statuslabel.Text = "Неправильный старый пароль!";
                if (regStatus == UserClass.OperationStatus.DBError)
                    statuslabel.Text = "Ошибка при обращении к серверу";
                if (regStatus == UserClass.OperationStatus.BlockedUser)
                    statuslabel.Text = "Пользователь заблокирован";
                if (regStatus == UserClass.OperationStatus.Successful)
                {
                    MessageBox.Show("Пароль успешно изменен!");
                    Close();
                }
            }
        }

        private bool checkInputs()
        {
            if (pwdbox0.Password.Length == 0)
            {
                statuslabel.Text = "Введите старый пароль!";
                return false;
            }

            if (pwdbox1.Password != pwdbox2.Password)
            {
                statuslabel.Text = "Новые пароли не совпадают!";
                return false;
            }

            if (pwdbox0.Password == pwdbox1.Password)
            {
                statuslabel.Text = "Старый пароль не должен совпадать с новым!";
            }

            if (pwdbox1.Password == loginbox.Text || pwdbox1.Password == loginbox.Text.Reverse().ToString())
            {
                statuslabel.Text = "Пароль не должен совпадать с логином!";
                return false;
            }

            string message;
            if (!InputsCheckClass.pwdCheck(pwdbox1.Password, out message))
            {
                statuslabel.Text = message;
                return false;
            }

            return true;
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
