using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ChenduZhuanYeJiShuApp.ViewModel
{
    public class CourseInfo
    {
        public string CourseName { get; set; }

        public string CourseImageUrl { get; set; }

        public string DetailUrl { get; set; }

        public BitmapImage Image { get; set; }

        public string Guid { get; set; }
    }
}
