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
    /// Логика взаимодействия для CaptchaWindow.xaml
    /// </summary>
    public partial class CaptchaWindow : Window
    {
        public CaptchaWindow()
        {
            InitializeComponent();
            cApcha = new CApchaClass();
            CapCon.DataContext = cApcha;
        }

        internal bool isCapchaCorrect = false;
        CApchaClass cApcha;

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text.Length > 0)
            {
                isCapchaCorrect = cApcha.isCorrect(captchaInput.Text);
                if (isCapchaCorrect)
                    Close();
                else
                {
                    statuslabel.Text = "Ошибка. Попробуйте еще раз!";
                    captchaInput.Text = "";
                    cApcha = new CApchaClass();
                    CapCon.DataContext = cApcha;
                }
            }
            else
                statuslabel.Text = "Введите текст с картинки!";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cApcha = new CApchaClass();
            CapCon.DataContext = cApcha;
        }
    }
}
