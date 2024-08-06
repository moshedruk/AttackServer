using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Attackserver
{    
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Task<string> taskA = GetDataAsync.GetReadDataAsync_A();
            Task<string> taskB = GetDataAsync.GetReadDataAsync_B();

            await Task.WhenAll(taskA, taskB); 
            
            string res =  await GetDataAsync.GetDataAsync_fun();
            Console.WriteLine(res);

            Console.ReadLine();
        }
    }
}
