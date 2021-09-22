using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RemnantBuildRandomizer
{
    public static class MyExtensions
    {

        private static Random rng = MainWindow.rd;
        public static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        } 
    }
}
