using System;
using System.Collections.Generic;
using System.Text;

namespace CSML.Generator;
internal class DebugReporter
{
    public static void Clear()
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        File.WriteAllText("""C:\Users\Magnus\OneDrive\Desktop\CSMLDebug.txt""", "");
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
    }

    public static void Report(string message)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        File.AppendAllText("""C:\Users\Magnus\OneDrive\Desktop\CSMLDebug.txt""", message + '\n');
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
    }
}
