using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Логика взаимодействия для UsersActionsWindow.xaml
    /// </summary>
    public partial class UsersActionsWindow : Window
    {
        public UsersActionsWindow()
        {
            InitializeComponent();
            loginlabel.Text = UserClass.Login;
            rolelabel.Text = UserClass.UserRole == UserClass.Role.Admin ? "Администратор" : "Пользователь";


            var imagesourceuri = UserClass.UserRole == UserClass.Role.Regular ? @"/img/user.png" : @"/img/admin.png";
            roleimage.Source = new BitmapImage(new Uri(imagesourceuri, UriKind.Relative));

            if (UserClass.UserRole == UserClass.Role.Admin)
            {
                AdminTools.Visibility = Visibility.Visible;
                foreach (var user in AdminToolsClass.GetAllUsers())
                {
                    if (user.Login != UserClass.Login)
                        usersdatagrid.Items.Add(user);
                }
            }
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            UserClass.LogOut();
            
            UsersLoginWindow usersLoginWindow = new UsersLoginWindow();
            Close();
            usersLoginWindow.Show();
        }

        private void ChangeCurrentUserPwd(object sender, RoutedEventArgs e)
        {
            ChangePwdWindow pwdWindow = new ChangePwdWindow();
            pwdWindow.ShowDialog();
        }

        private void BlockUser(object sender, RoutedEventArgs e)
        {
            ((UserModel)usersdatagrid.SelectedItem).BlockUser(true);
        }

        private void UnblockUser(object sender, RoutedEventArgs e)
        {
            ((UserModel)usersdatagrid.SelectedItem).BlockUser(false);
        }

        private void ChangeCurrentUserLogin(object sender, RoutedEventArgs e)
        {
            ChangeLoginWindow loginWindow = new ChangeLoginWindow();
            loginWindow.Closing += (object se, CancelEventArgs ee) =>  loginlabel.Text = UserClass.Login;
            loginWindow.ShowDialog();
        }

        private void usersdatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usersdatagrid.SelectedItems.Count > 0)
            {
                adminToolsPanel.IsEnabled = true;
                var curuser = (UserModel)usersdatagrid.SelectedItem;
                admuserlogin.Text = curuser.Login;
                admuserrole.SelectedIndex = curuser.Role == "Пользователь" ? 0 : 1;
            }
            else
            {
                adminToolsPanel.IsEnabled = false;
            }
        }

        private void admChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (usersdatagrid.SelectedItems.Count > 0)
            {
                ((UserModel)usersdatagrid.SelectedItem).ChangeRole(admuserrole.Text);
            }
        }

        private void admChangeLogin_Click(object sender, RoutedEventArgs e)
        {
            if (usersdatagrid.SelectedItems.Count > 0)
            {
                ((UserModel)usersdatagrid.SelectedItem).ChangeLogin(admuserlogin.Text);
            }
        }
    }

    
}
