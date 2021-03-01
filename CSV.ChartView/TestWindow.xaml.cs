using System.Windows;
using System.Windows.Threading;
using System;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace CSV.ChartView
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window, INotifyPropertyChanged
    {
        private int _maxVoltage;
        public int MaxVoltage
        {
            get { return _maxVoltage; }
            set { _maxVoltage = value; this.OnPropertyChanged("MaxVoltage"); }
        }

        private int _minVoltage;
        public int MinVoltage
        {
            get { return _minVoltage; }
            set { _minVoltage = value; this.OnPropertyChanged("MinVoltage"); }
        }

        public VoltagePointCollection voltagePointCollection;
        DispatcherTimer updateCollectionTimer;
        private int i = 0;

        public TestWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            voltagePointCollection = new VoltagePointCollection();

            updateCollectionTimer = new DispatcherTimer();
            updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(100);
            updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
            updateCollectionTimer.Start();

            var ds = new EnumerableDataSource<VoltagePoint>(voltagePointCollection);
            ds.SetXMapping(x => dateAxis.ConvertToDouble(x.Date));
            ds.SetYMapping(y => y.Voltage);
            plotter.AddLineChart(ds); // to use this method you need "using Microsoft.Research.DynamicDataDisplay;"

            MaxVoltage = 1;
            MinVoltage = -1;
        }
        void updateCollectionTimer_Tick(object sender, EventArgs e)
        {
            i++;
            voltagePointCollection.Add(new VoltagePoint(Math.Sin(i * 0.1), DateTime.Now));
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class VoltagePointCollection : RingArray<VoltagePoint>
    {
        private const int TOTAL_POINTS = 300;

        public VoltagePointCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }

    public class VoltagePoint
    {
        public DateTime Date { get; set; }

        public double Voltage { get; set; }

        public VoltagePoint(double voltage, DateTime date)
        {
            this.Date = date;
            this.Voltage = voltage;
        }
    }
}
