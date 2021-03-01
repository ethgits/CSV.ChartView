using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSV.ChartView.Files;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace CSV.ChartView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool initTempFiles = true;
        Dictionary<string, List<CSVLog>> tempList = new Dictionary<string, List<CSVLog>>();
        List<CSVLog> tempListMerged = new List<CSVLog>();

        private static bool initDoorFiles = false;
        Dictionary<string, List<CSVLog>> doorList = new Dictionary<string, List<CSVLog>>();
        List<CSVLog> doorListMerged = new List<CSVLog>();
                
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            
            //plotter.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
            //plotter2.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
        }

        //void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Visible")
        //    {
        //        if ((sender as Viewport2D).Plotter == plotter)
        //            plotter2.Viewport.Visible = new DataRect(plotter.Viewport.Visible.XMin, plotter2.Viewport.Visible.YMin, plotter.Viewport.Visible.Width, plotter2.Viewport.Visible.Height);
        //        else if ((sender as Viewport2D).Plotter == plotter2)
        //            plotter.Viewport.Visible = new DataRect(plotter2.Viewport.Visible.XMin, plotter.Viewport.Visible.YMin, plotter2.Viewport.Visible.Width, plotter.Viewport.Visible.Height);
        //    }
        //}

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loadFiles();
        }

        private void loadFiles()
        {
            //TODO temp fix for scaling issue -> load one file if multiple selected + load all files afterwards
            if (initTempFiles)
            {
                loadFiles("Temp_ CSV Dateien (*.csv)|Temp_*.csv", ref tempList);
                mergeList(ref tempList, ref tempListMerged);
            }

            if (initDoorFiles)
            {
                loadFiles("Tor_ CSV Dateien (*.csv)|Tor_*.csv", ref doorList);
                mergeList(ref doorList, ref doorListMerged);    
            }

            populateGraphs();
        }

        private void populateGraphs()
        {
            clearGraph();

            if (initTempFiles)
            {
                populateGraph(ref tempListMerged, "Temperatur ");
            }

            if (initDoorFiles)
            {
                populateGraph(ref doorListMerged, "Tor ");
            }
        }

        private void loadFiles(string dialogFilter, ref Dictionary<string, List<CSVLog>> d)
        {
            d.Clear();

            OpenFileDialog ff = new OpenFileDialog();

            ff.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //"C:\\";
            ff.Filter = dialogFilter;
            ff.Multiselect = true;
            ff.FilterIndex = 1;
            ff.RestoreDirectory = false;

            if (ff.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    foreach (String file in ff.FileNames)
                    {
                        CSVLog rr = new CSVLog();
                        rr.SetFilePath(file);
                        if (!rr.Parse())
                        {
                            System.Windows.MessageBox.Show("Failed to parse file " + file);
                            continue;
                        }

                        if (!d.ContainsKey(rr.objectName))
                            d.Add(rr.objectName, new List<CSVLog> { rr });
                        else
                        {
                            d[rr.objectName].Add(rr);
                            d[rr.objectName].OrderBy(x => x.fileDateTime);
                        }
                    }
                }
                catch (Exception err)
                {
                    //Inform the user if we can't read the file
                    System.Windows.MessageBox.Show(err.Message);
                }
            }
        }
        private bool mergeList(ref Dictionary<string, List<CSVLog>> raw, ref List<CSVLog> merged)
        {
            if (raw.Count == 0)
                return false;

            merged.Clear();

            foreach (var entry in raw)
            {
                if (entry.Value.Count == 1)
                    merged.Add(entry.Value[0]);
                else
                {
                    CSVLog eFirst = entry.Value[0];
                    for (int i = 1; i < entry.Value.Count; i++)
                    {
                        eFirst.mergeData(entry.Value[i]);
                    }

                    merged.Add(eFirst);
                }
            }

            return true;
        }        
        private void clearGraph()
        {
            plotter.Children.Clear();
            //plotter2.Children.Clear();
        }
        private void populateGraph(ref List<CSVLog> list, string descPrefix)
        {
            for (int i = 0; i < list.Count; i++)
            {
                foreach (var entry in list[i].dataByColumn)
                {
                    var ds = new EnumerableDataSource<DataMapping>(entry.Value);
                    ds.SetXMapping(x => dateAxis.ConvertToDouble(x.Date));
                    ds.SetYMapping(y => y.Value);

                    LineGraph graph = new LineGraph
                    {
                        DataSource = ds,
                        LinePen = new Pen(new SolidColorBrush(Utils.RandomColor.Get()), 2)                        
                    };
                    Legend.SetDescription(graph, descPrefix + entry.Key);
                    
                    plotter.Children.Add(graph);
                }
            }
        }
        private void menu_exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void menu_load_Click(object sender, RoutedEventArgs e)
        {
            loadFiles();
        }

        private string getInfoText()
        {
            string s = "";
            s += "Clemens Larkowski\n";
            s += "c.larkowski@et-h.eu\n";
            s += "\n\n";
            s += "Version: 1.0.0\n";
            s += "\n\n";
            s += "Entwickelt für DB-Regio\n";
            s += "ET-Werkstatt\n";
            s += "Frankfurt am Main";

            return s;
        }

        private void menu_info_Click(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.SetText(getInfoText());
            info.Show();
        }
    }
}
