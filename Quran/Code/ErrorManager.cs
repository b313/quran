using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Code
{
    internal static class ErrorManager
    {
        internal static void Handle(string origin, Exception ex)
        {
            using (var sw = new StreamWriter( "error.log", true, Encoding.Unicode ))
            {
                sw.WriteLine( "=========" + DateTime.Now.ToString() + "=========" );
                sw.WriteLine( "Origin: " +  origin );
                sw.WriteLine( "Error: " + ex.Message);
                if( ex.InnerException != null )
                    sw.WriteLine("Inner: " + ex.InnerException.Message);
                sw.WriteLine("");
                sw.WriteLine(ex.StackTrace);
                sw.WriteLine( "--------------------------------------------------------------------------------");
                sw.WriteLine( " ");
                sw.WriteLine( " ");
            }
        }
    }
}
