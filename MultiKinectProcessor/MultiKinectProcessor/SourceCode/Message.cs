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

        /// <summary>
        /// Prints "ERROR: " + msg in red to debug console
        /// </summary>
        /// <param name="msg">String of your error message text</param>
        static public void Error(String msg)
        {
            SolidColorBrush red = new SolidColorBrush(Colors.Red);

            //DebugWindow.addtoDebugTextBox(msg);
            Debug.WriteLine("ERROR: " + msg);
        }

        /// <summary>
        /// Prints "Warning: " + msg in yelllow to debug console
        /// </summary>
        /// <param name="msg">String of your warning message text</param>
        static public void Warning(String msg)
        {
            SolidColorBrush orange = new SolidColorBrush(Colors.Orange);
            //DebugWindow.addtoDebugTextBox(msg);
            Debug.WriteLine("Warning: " + msg);
        }
        /// <summary>
        /// Prints msg in grey to debug console
        /// </summary>
        /// <param name="msg">String of your info message text</param>
        static public void Info(String msg)
        {
            SolidColorBrush green = new SolidColorBrush(Colors.Green);
            //DebugWindow.addtoDebugTextBox(msg);
            Debug.WriteLine(msg);
        }

    }
}
