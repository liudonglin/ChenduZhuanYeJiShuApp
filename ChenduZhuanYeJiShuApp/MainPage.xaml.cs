using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using ChenduZhuanYeJiShuApp.Entity;
using ChenduZhuanYeJiShuApp.ViewModel;

namespace ChenduZhuanYeJiShuApp
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        private MainWindow _window;
        private MainPageVM _vm;

        public MainPage(MainWindow window)
        {
            InitializeComponent();
            this._window = window;
            this._vm = new MainPageVM();
            this.DataContext = this._vm;

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var trainplanUrl = "http://web.chinahrt.com/trainplan";
            var pageHtmlStr = HttpUtility.HttpGet(trainplanUrl,Encoding.UTF8);

            var document = new HtmlParser().Parse(pageHtmlStr);
            var courseDoms = document.QuerySelectorAll("div.Course-List li");

            ParseCourseDom(courseDoms);
        }

        private void ParseCourseDom(IHtmlCollection<IElement> courseDoms)
        {
            foreach (var item in courseDoms)
            {
                CourseInfo cinfo = new CourseInfo();
                cinfo.CourseName = item.QuerySelector("div.course-list-li-test a").TextContent.Trim();

                var img = (IHtmlImageElement)item.QuerySelector("div.course-div-padding p img[width=210]");
                cinfo.CourseImageUrl = img.Source;

                cinfo.Image = new BitmapImage();
                cinfo.Image.BeginInit();
                cinfo.Image.StreamSource = new MemoryStream(HttpUtility.HttpDownloadFile(cinfo.CourseImageUrl));
                cinfo.Image.EndInit();

                var detailUrl = item.QuerySelector("div.playin_grey.tc").Attributes["onclick"].Value;
                detailUrl = detailUrl.Replace("window.open('", string.Empty).Replace("');", string.Empty);
                cinfo.DetailUrl = detailUrl;

                cinfo.Guid = Guid.NewGuid().ToString();

                this._vm.CourseInfos.Add(cinfo);
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            string guid = image.Tag.ToString();
            var curCourse = this._vm.CourseInfos.FirstOrDefault(x => x.Guid.Equals(guid));

            LoadCourseDetailInfos(curCourse);
        }

        private void LoadCourseDetailInfos(CourseInfo curCourse)
        {
            var detailListUrl = "http://" + HttpUtility.HOST + curCourse.DetailUrl;
            var pageHtmlStr = HttpUtility.HttpGet(detailListUrl, Encoding.UTF8);

            var document = new HtmlParser().Parse(pageHtmlStr);
            var courseDetailDoms = document.QuerySelectorAll("div.memu-in ul li");

            this.Dispatcher.Invoke(new Action(()=> 
            {
                this._vm.CourseDetailInfos.Clear();

                foreach (var item in courseDetailDoms)
                {
                    CourseDetailInfo detailInfo = new CourseDetailInfo();
                    var aDom = item.QuerySelector("a");
                    detailInfo.Name = aDom.TextContent.Trim();
                    detailInfo.Url = aDom.Attributes["href"].Value;

                    detailInfo.Guid = Guid.NewGuid().ToString();
                    detailInfo.ParentGuid = curCourse.Guid;

                    detailInfo.Status = item.QuerySelector("span").TextContent.Trim(); ;
                    this._vm.CourseDetailInfos.Add(detailInfo);
                }
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            string guid = button.Tag.ToString();
            var curCourseDetail = this._vm.CourseDetailInfos.FirstOrDefault(x => x.Guid.Equals(guid));

            Task.Factory.StartNew(() =>
            {
                try
                {
                    StudyCourse(curCourseDetail);
                }
                catch
                {
                    MessageBox.Show("发生错误请稍后重试！");
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    button.IsEnabled = true;
                }));
            });
        }

        private void StudyCourse(CourseDetailInfo curCourseDetail)
        {
            var playFrameUrl = "http://" + HttpUtility.HOST + curCourseDetail.Url;
            var pageHtmlStr = HttpUtility.HttpGet(playFrameUrl, Encoding.UTF8);
            var document = new HtmlParser().Parse(pageHtmlStr);
            var playFrameDom = document.QuerySelector("iframe[id=iframe]");

            var playUrl = playFrameDom.Attributes["src"].Value;
            pageHtmlStr = HttpUtility.HttpGet(playUrl, Encoding.UTF8);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("businessId", "gp5");
            parameters.Add("time", "24000");

            var recordIdStartIndex = pageHtmlStr.IndexOf("attrset.recordId=\"");
            var recordId = pageHtmlStr.Substring(recordIdStartIndex + "attrset.recordId=\"".Length, 32);
            parameters.Add("recordId", recordId);

            var updateRedisMapIndex = pageHtmlStr.IndexOf("attrset.updateRedisMap=\"");
            var updateRedisMap = pageHtmlStr.Substring(updateRedisMapIndex + "attrset.updateRedisMap=\"".Length, 1);
            parameters.Add("updateRedisMap", updateRedisMap);

            var studyCodeIndex = pageHtmlStr.IndexOf("attrset.studyCode=\"");
            var studyCode = pageHtmlStr.Substring(studyCodeIndex + "attrset.studyCode=\"".Length, 32);
            parameters.Add("studyCode", studyCode);

            var sectionIdIndex = pageHtmlStr.IndexOf("attrset.sectionId=\"");
            var sectionId = pageHtmlStr.Substring(sectionIdIndex + "attrset.sectionId=\"".Length, 36);
            parameters.Add("sectionId", sectionId);

            var signIdIndex = pageHtmlStr.IndexOf("attrset.signId=\"");
            var signId = pageHtmlStr.Substring(signIdIndex + "attrset.signId=\"".Length);
            signIdIndex = signId.IndexOf("\";");
            signId = signId.Substring(0, signIdIndex);
            parameters.Add("signId", signId);

            var takeRecordUrl = "http://videoadmin.chinahrt.com.cn/videoPlay/takeRecord";
            var result = HttpUtility.HttpPost(takeRecordUrl, null, parameters);

            var curCourse = this._vm.CourseInfos.FirstOrDefault(x => x.Guid.Equals(curCourseDetail.ParentGuid));
            LoadCourseDetailInfos(curCourse);

            parameters.Clear();
            parameters.Add("recordId", recordId);
            parameters.Add("time", "100");
            parameters.Add("studyCode", studyCode);
            parameters.Add("viewTime", "100");

            var endPalyUrl = "http://videoadmin.chinahrt.com.cn/videoPlay/endPlay";
            HttpUtility.HttpGet(endPalyUrl, null, parameters);
        }
    }
}
