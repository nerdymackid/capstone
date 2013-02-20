using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.IO;
using Microsoft.Kinect;
using System.Diagnostics;

namespace MultiKinectProcessor.SourceCode
{
    /// <summary>
    /// Description: Functions that style and prints error messages to some console/window
    /// Author: Jerry Peng and Sea Pong
    /// </summary>
    class Message
    {
        // SP - FIXME print to MainWindow
        static public void Error(String msg)
        {
            //MainWindow.addtoDebugTextBox(MainWindow.getDebugTextBox + msg, Brushes.Red);
            Debug.WriteLine("ERROR: " + msg);
        }
        static public void Warning(String msg)
        {
            Debug.WriteLine("WARNING: " + msg);
        }

        static public void Info(String msg)
        {
            Debug.WriteLine(msg);
        }

    }
}
