using ChenduZhuanYeJiShuApp.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChenduZhuanYeJiShuApp
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        private MainWindow _window;

        public LoginPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _window = mainWindow;

            this.Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadValidImage();
        }

        private void valid_Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadValidImage();
        }

        private void LoadValidImage()
        {
            var url = "http://web.chinahrt.com/servlet/Image.Servlet";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = HttpUtility.AllCookie;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var cookieStr = response.Headers["Set-Cookie"];
            if (!string.IsNullOrWhiteSpace(cookieStr))
            {
                var cookieValue = cookieStr.Substring(cookieStr.IndexOf("=") + 1, 36);
                HttpUtility.AllCookie.Add(new Cookie("JSESSIONID", cookieValue,"/", HttpUtility.HOST));
            }

            var img = new BitmapImage();
            using (Stream responseStream = response.GetResponseStream())
            {
                List<byte> list = new List<byte>();
                while (true)
                {
                    int data = responseStream.ReadByte();
                    if (data == -1)
                        break;
                    list.Add((byte)data);
                }
                img.BeginInit();
                img.StreamSource = new MemoryStream(list.ToArray());
                img.EndInit();
            }
            this.valid_Image.Source = img;
        }

        private void DoLogin()
        {
            var checkCaptchaUrl = "http://web.chinahrt.com/user/checkCaptcha";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Captcha", valid_Txt.Text.Trim());
            var checkResult = HttpUtility.HttpPost(checkCaptchaUrl,null, parameters);

            if (!"true".Equals(checkResult))
            {
                MessageBox.Show("验证码错误！");
                return;
            }

            var loginValidUrl = "http://web.chinahrt.com/loginValid";
            parameters.Clear();
            parameters.Add("userName", loginName_Txt.Text.Trim());
            parameters.Add("password", EncryptPassword(passwordTxt.Password.Trim()));
            parameters.Add("platformId", "18");

            var loginResultSrt = HttpUtility.HttpPost(loginValidUrl,Encoding.UTF8, parameters,true);

            var loginResult = JsonConvert.DeserializeObject<LoginResult>(loginResultSrt);
            if (!"success".Equals(loginResult.status))
            {
                MessageBox.Show(loginResult.status);
                return;
            }

            HttpUtility.AllCookie.Add(new Cookie("chinahrtPlatformId", "18", "/", HttpUtility.HOST));
            MessageBox.Show("登陆成功！");

            _window.GoToMainPage();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        /// <summary>
        /// 密码MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string EncryptPassword(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                byte[] buffer = new MD5CryptoServiceProvider().ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(password));
                StringBuilder builder = new StringBuilder(0x20);
                for (int i = 0; i < buffer.Length; i++)
                {
                    builder.Append(buffer[i].ToString("x").PadLeft(2, '0'));
                }
                return builder.ToString();
            }
            return string.Empty;
        }
    }
}
