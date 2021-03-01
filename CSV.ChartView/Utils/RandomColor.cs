using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CSV.ChartView.Utils
{
    public static class RandomColor
    {
        private static int AccessCounter = 0;
        private static DateTime DTInit = DateTime.Now;

        public static Color Get()
        {
            AccessCounter++;

            Random rnd = new Random(DTInit.Day * DTInit.Hour + AccessCounter * 200);
            return Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
        }
    }
}
