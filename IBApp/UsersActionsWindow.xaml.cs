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
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            UserClass.LogOut();
            
            UsersLoginWindow usersLoginWindow = new UsersLoginWindow();
            Close();
            usersLoginWindow.Show();
        }
    }
}
