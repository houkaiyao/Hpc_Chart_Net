using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Hpc_Chart_Net
{
    internal class MainViewModel : ObservableObject
    {
        private PlotModel plotModel;
        private int a;
        private DateTimeAxis xAxis;
        private LineSeries series;
        private IPlotController plotModelController;

        public MainViewModel()
        {
            plotModel = new PlotModel() { Title = "HPC Chart" };

             var myController = new PlotController();
            PlotModelController = myController;
            myController.BindMouseDown(OxyMouseButton.Right, PlotCommands.PanAt);
            myController.BindMouseWheel(PlotCommands.ZoomWheel);

            BtnClickCommand = new RelayCommand<object>(BtnClick);

            PickerValue = DateTime.Now.ToString("yyyy/MM/dd");
            PlotModel = new PlotModel { Title = "HPC Chart" };

            StartTime = DateTime.Now.ToString("HH:mm:00");
            EndTime = DateTime.Now.ToString("HH:mm:00");
        }

        private void BtnClick(object parameter)
        {
            if (parameter is string tag)
            {
                switch (parameter.ToString())
                {
                    case "SearchTag":
                        if (ComboBoxSource.Contains(ComboBoxValue) && StartTime!=null && EndTime!=null)
                        {
                            GetDataSource();
                        }
                        break;

                    default: break;
                }
            }
        }

        #region 资料处理
        private void GetDataSource()
        {
            string _Path, _pickerVlaue;
            _pickerVlaue = PickerValue.Replace('/', '_');

            DateTime startTime = DateTime.ParseExact(StartTime, "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.ParseExact(EndTime, "HH:mm:ss", CultureInfo.InvariantCulture);

            int _startHour = Convert.ToInt32(startTime.Hour);
            int _endHour = Convert.ToInt32(endTime.Hour);
            dataModels.Clear();
            for (int i = _startHour; i <= _endHour; i++)
            {
                _Path = string.Format(@"D:\WinPc1\Sys\Data\Data\@{0}\@{1}_{2}.log", _pickerVlaue, _pickerVlaue, i.ToString());
                try
                {
                    using (StreamReader reader = new StreamReader(_Path))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] dataItems = line.Split(',');
                            DateTime datetime = DateTime.ParseExact(dataItems[0], "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

                            TimeSpan timePart = datetime.TimeOfDay;

                            if (timePart >= startTime.TimeOfDay && timePart <= endTime.TimeOfDay)
                            {
                                DataModel dataModel = new DataModel();
                                dataModel.dataTime = dataItems[0].Replace('/', '-');
                                dataModel.Ch1.Arm2N2 = float.Parse(dataItems[1]);
                                dataModel.Ch1.BSNN2 = float.Parse(dataItems[2]);
                                dataModel.Ch1.BSRDI = float.Parse(dataItems[3]);
                                dataModel.Ch1.Arm2DI = float.Parse(dataItems[4]);
                                dataModel.Ch1.Arm1DI = float.Parse(dataItems[5]);
                                dataModel.Ch1.Arm3N2 = float.Parse(dataItems[6]);
                                dataModel.Ch1.Arm1Press = float.Parse(dataItems[7]);

                                dataModel.Ch2.Arm2N2 = float.Parse(dataItems[8]);
                                dataModel.Ch2.BSNN2 = float.Parse(dataItems[9]);
                                dataModel.Ch2.BSRDI = float.Parse(dataItems[10]);
                                dataModel.Ch2.Arm2DI = float.Parse(dataItems[11]);
                                dataModel.Ch2.Arm1DI = float.Parse(dataItems[12]);
                                dataModel.Ch2.Arm3N2 = float.Parse(dataItems[13]);
                                dataModel.Ch2.Arm1Press = float.Parse(dataItems[14]);
                                dataModel.Ch1.eFlow = float.Parse(dataItems[15]);
                                dataModels.Add(dataModel);
                            }
                            else if (timePart > endTime.TimeOfDay)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("查询区间或目录有误");
                    break;
                }
            }
            RefreshSeries(ComboBoxValue);
        }

        private void RefreshSeries(string comboValue)
        {
            int indexValue = ComboBoxSource.IndexOf(comboValue);
            List<float> YSource = new List<float>();
            List<DateTime> XSoure = new List<DateTime>();
            YSource = GetYListSoure(indexValue);
            XSoure = GetXListSoure();
            plotModel.Series.Clear();
            plotModel.Axes.Clear();
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                Title = "时间"
            };
            PlotModel.Axes.Add(xAxis);
            var series = new LineSeries
            {
                Title = "数据",
                Color = OxyColors.DeepSkyBlue,
                
                TrackerFormatString = String.Format("时间: {{2:HH:mm:ss}}\n数据: {{4:0.0}} {0}", GetDataUnit[indexValue]),
                
            };

            for (int sourceIndex = 0; sourceIndex < XSoure.Count; sourceIndex++)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(XSoure[sourceIndex]), YSource[sourceIndex]));
            }
            plotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        }

        private List<float> GetYListSoure(int index)
        {
            List<float> YSource = new List<float>();
            switch (index)
            {
                case 0:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm1Press);
                    }
                    break;

                case 1:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm1DI);
                    }
                    break;

                case 2:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm2DI);
                    }
                    break;

                case 3:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm2N2);
                    }
                    break;

                case 4:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm3N2);
                    }
                    break;

                case 5:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.BSRDI);
                    }
                    break;

                case 6:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.BSNN2);
                    }
                    break;

                case 7:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm1Press);
                    }
                    break;

                case 8:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm1DI);
                    }
                    break;

                case 9:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm2DI);
                    }
                    break;

                case 10:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm2N2);
                    }
                    break;

                case 11:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm3N2);
                    }
                    break;

                case 12:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.BSRDI);
                    }
                    break;

                case 13:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.BSNN2);
                    }
                    break;

                case 14:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.eFlow);
                    }
                    break;
            }
            return YSource;
        }

        private List<DateTime> GetXListSoure()
        {
            List<DateTime> XSource = new List<DateTime>();
            foreach (DataModel data in dataModels)
            {
                DateTime dateTime;
                if (DateTime.TryParse(data.dataTime, out dateTime))
                {
                    XSource.Add(dateTime);
                };
            }
            return XSource;
        }
        #endregion

        #region 属性封装
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { SetProperty(ref plotModel, value); }
        }

        public DateTimeAxis XAxis
        {
            get { return xAxis; }
            set { SetProperty(ref xAxis, value); }
        }

        public LineSeries Series
        {
            get { return series; }
            set { SetProperty(ref series, value); }
        }

        public IPlotController PlotModelController
        {
            get { return plotModelController; }
            set
            {
                SetProperty(ref plotModelController, value);
            }
        }

        public string PickerValue { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ComboBoxValue { get; set; }

        public List<String> ComboBoxSource { get; } = new List<string>()
        {
            "Ch1 Pressure","Ch1Arm1 DI","Ch1Arm2 DI","Ch1Arm2 N2","Ch1Arm3 N2","Ch1 BSR DI","Ch1 BSN N2",
            "Ch2 Pressure","Ch2Arm1 DI","Ch2Arm2 DI","Ch2Arm2 N2","Ch2Arm3 N2","Ch2 BSN DI","Ch2 BSN N2","Eflow"
        };

        public List<String> GetDataUnit { get; } = new List<string>()
        {
            "Psi","L/Min","ML/Min","ML/Min","ML/Min","ML/Min","ML/Min",
            "Psi","L/Min","ML/Min","ML/Min","ML/Min","ML/Min","ML/Min","Ω"
        };

        public RelayCommand<object> BtnClickCommand { get; private set; }
        public ICommand LoadCommand { get; set; }
        public List<DataModel> dataModels { get; private set; } = new List<DataModel>();
        #endregion

    }

    #region 存储数据格式类

    internal class DataModel
    {
        public string dataTime { get; set; }
        public ChanmberModel Ch1 { get; set; } = new ChanmberModel();
        public ChanmberModel Ch2 { get; set; } = new ChanmberModel();
    }

    internal class ChanmberModel
    {
        public float Arm1Press { get; set; }
        public float Arm1DI { get; set; }
        public float Arm2DI { get; set; }
        public float Arm2N2 { get; set; }
        public float Arm3N2 { get; set; }
        public float BSRDI { get; set; }
        public float BSNN2 { get; set; }
        public float eFlow { get; set; }
    }

    #endregion 存储数据格式类
}