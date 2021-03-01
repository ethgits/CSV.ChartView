using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV.ChartView.Files
{
    public class DataMapping
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public DataMapping(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }
    }
    public class CSVLog
    {
        private string filePath { get; set; }
        List<string> dataColumns { get; set; }        
        public Dictionary<string, List<DataMapping>> dataByColumn { get; set; }
        public string fileName { get; set; }
        public string objectName { get; set; }
        public DateTime fileDateTime { get; set; }
        public CSVLog()
        {
            dataColumns = new List<string>();
            dataByColumn = new Dictionary<string, List<DataMapping>>();
        }

        public void SetFilePath(string fPath)
        {
            filePath = fPath;
        }
        public bool Parse()
        {
            if (filePath == null)
                return false;

            string[] pieces;

            fileName = Path.GetFileName(filePath);
            string[] dtPieces = fileName.Replace(".csv", "").Split('_');
            int pSelector = dtPieces.Length - 1;
            string sSeconds = dtPieces[pSelector--];
            string sMinutes = dtPieces[pSelector--];
            string sHours = dtPieces[pSelector--];
            pSelector--;
            string sYear = dtPieces[pSelector--];
            string sMonth = dtPieces[pSelector--];
            string sDay = dtPieces[pSelector--];

            string b = dtPieces[pSelector--];
            string a = dtPieces[pSelector--];
            objectName = String.Concat(a, "-", b);

            fileDateTime = new DateTime(int.Parse(sYear), int.Parse(sMonth), int.Parse(sDay), int.Parse(sHours), int.Parse(sMinutes), int.Parse(sSeconds));

            string[] lines = File.ReadAllLines(filePath);

            dataColumns = lines[0].Replace("\"", "").Split(';').ToList();
            dataColumns.RemoveAt(0);
            
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] == String.Empty)
                    continue;

                pieces = lines[i].Replace("\"", "").Split(';');

                string dtString = pieces[0];
                DateTime dt;
                if (DateTime.TryParse(dtString, out dt))
                {
                    for (int p = 1; p < pieces.Length; p++)
                    {
                        double val;
                        if (!double.TryParse(pieces[p], out val))
                            return false;

                        if (!dataByColumn.ContainsKey(dataColumns[p - 1]))
                            dataByColumn.Add(dataColumns[p - 1], new List<DataMapping> { new DataMapping(dt,val) });
                        else
                            dataByColumn[dataColumns[p - 1]].Add(new DataMapping(dt, val));
                    }                    
                }
                else
                    return false;

            }

            return true;
        }        
        public bool isExistingMapping(string obj, DateTime dt)
        {
            if (!dataByColumn.ContainsKey(obj))
                return false;

            for (int i = 0; i < dataByColumn[obj].Count; i++)
                if (dataByColumn[obj][i].Date == dt)
                    return true;

            return false;
        }
        public void mergeData(CSVLog t)
        {
            foreach(var objEntry in t.dataByColumn)
            {
                List<DataMapping> tmpBuffer = dataByColumn[objEntry.Key];
                for(int i = 0; i < objEntry.Value.Count; i++)
                {
                    if (!isExistingMapping(objEntry.Key, objEntry.Value[i].Date))
                        tmpBuffer.Add(objEntry.Value[i]);
                }

                dataByColumn[objEntry.Key] = tmpBuffer.OrderBy(x => x.Date).ToList();
            }
        }
    }
}
