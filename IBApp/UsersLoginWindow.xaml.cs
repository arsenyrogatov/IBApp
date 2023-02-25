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
    /// Логика взаимодействия для UsersLoginWindow.xaml
    /// </summary>
    public partial class UsersLoginWindow : Window
    {
        public UsersLoginWindow()
        {
            InitializeComponent();
        }

        /*
        Arsenyrogatov
        Qwerty07!
         
        user1
        p!unXzFse8T

        user2
        E9@N6hwSnvn

        user3
        Un9#wvFWKWx

        user4
        TDyt$CS4ptq

        user5
        s94qtP%fys

        user7
        m#4T5jgUS%
         
         */

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UsersRegistrationWindow usersRegistrationWindow = new UsersRegistrationWindow();
            usersRegistrationWindow.ShowDialog();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (pwdbox.Visibility == Visibility.Hidden)
            {
                pwdbox.Password = pwdboxs.Text;
            }
            statuslabel.Text = "";

            if (CheckInputs())
            {
                var result = UserClass.TryLogin(loginbox.Text.ToCharArray(), pwdbox.Password.ToCharArray());
                if (result == UserClass.OperationStatus.LoginError)
                {
                    statuslabel.Text = "Нет такого пользователя";
                }
                if (result == UserClass.OperationStatus.PwdError)
                {
                    statuslabel.Text = "Неправильный пароль";
                }
                if (result == UserClass.OperationStatus.BlockedUser)
                {
                    statuslabel.Text = "Пользователь заблокирован";
                }
                if (result == UserClass.OperationStatus.Successful)
                {
                    var uaw = new UsersActionsWindow();
                    Close();
                    uaw.Show();
                }
            }
        }

        private bool CheckInputs()
        {
            if (loginbox.Text.Length == 0)
            {
                statuslabel.Text = "Введите логин!";
                return false;
            }
            if (pwdbox.Password.Length == 0)
            {
                statuslabel.Text = "Введите пароль!";
                return false;
            }
            var msg = "";
            if (!InputsCheckClass.loginCheck(loginbox.Text, out msg))
            {
                statuslabel.Text = msg;
                return false;
            }
            return true;
        }

        private void ShowPassword(object sender, RoutedEventArgs e)
        {
            pwdboxs.Text = pwdbox.Password;
            pwdbox.Visibility = Visibility.Hidden;
            pwdboxs.Visibility = Visibility.Visible;
        }

        private void HidePassword(object sender, RoutedEventArgs e)
        {
            pwdbox.Password = pwdboxs.Text;
            pwdboxs.Visibility = Visibility.Hidden;
            pwdbox.Visibility = Visibility.Visible;
        }
    }
}
