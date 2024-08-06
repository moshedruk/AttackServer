using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Attackserver
{
    internal class GetDataAsync
    {
        public GetDataAsync() { }
        public static async Task<string> GetDataAsync_fun()
        {
            string path = @"C:\Users\admin\source\repos\Attackserver\TextFile1.txt";
            string res = await Task.Run(() => File.ReadAllText(path));
            return res;
        }
        
        public static async Task<string> GetReadDataAsync_A() 
        {
            await Task.Delay(1000);
            return "data from A";
        }
        public static async Task<string> GetReadDataAsync_B()
        {
            await Task.Delay(2000);
            return "data from B";
        }


    }
}
