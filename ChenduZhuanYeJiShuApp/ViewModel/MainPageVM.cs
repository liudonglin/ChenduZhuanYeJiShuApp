using ChenduZhuanYeJiShuApp.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ChenduZhuanYeJiShuApp.ViewModel
{
    public class MainPageVM
    {
        public ObservableCollection<CourseInfo> CourseInfos { get; set; }

        public ObservableCollection<CourseDetailInfo> CourseDetailInfos { get; set; }

        public MainPageVM()
        {
            CourseInfos = new ObservableCollection<CourseInfo>();

            CourseDetailInfos = new ObservableCollection<CourseDetailInfo>();
        }
    }
}
