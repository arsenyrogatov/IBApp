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
    /// Логика взаимодействия для ChangeLoginWindow.xaml
    /// </summary>
    public partial class ChangeLoginWindow : Window
    {
        public ChangeLoginWindow()
        {
            InitializeComponent();
            loginbox.Text = UserClass.Login;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (pwdbox1.Visibility == Visibility.Hidden)
            {
                pwdbox1.Password = pwdbox1s.Text;
            }
            statuslabel.Text = "";

            if (CheckInputs())
            {
                var chStatus = UserClass.ChangeLogin(newloginbox.Text.ToCharArray(), pwdbox1.Password.ToCharArray());

                if (chStatus == UserClass.OperationStatus.Successful)
                {
                    MessageBox.Show("Логин успешно изменен!");
                    Close();
                }
                else
                    statuslabel.Text = UserClass.OperationStatusToString(chStatus);
            }
        }

        private bool CheckInputs()
        {
            if (newloginbox.Text.Length == 0)
            {
                statuslabel.Text = "Введите новый логин!";
                return false;
            }
            if (pwdbox1.Password.Length == 0)
            {
                statuslabel.Text = "Введите пароль!";
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
            return true;
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowPasswords(object sender, RoutedEventArgs e)
        {
            pwdbox1s.Text = pwdbox1.Password;
            pwdbox1.Visibility = Visibility.Hidden;
            pwdbox1s.Visibility = Visibility.Visible;
        }

        private void HidePasswords(object sender, RoutedEventArgs e)
        {
            pwdbox1.Password = pwdbox1s.Text;
            pwdbox1s.Visibility = Visibility.Hidden;
            pwdbox1.Visibility = Visibility.Visible;
        }
    }
}
