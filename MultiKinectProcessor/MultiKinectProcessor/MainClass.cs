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
using MultiKinectProcessor.SourceCode;

namespace MultiKinectProcessor
{
    public partial class MainClass : System.Windows.Application
    {



        /// <summary>
        /// Description: Application Entry Point.
        /// Original Author: Sea Pong
        /// Modified by: Jerry Peng
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        static void Main()
        {
            Debug.WriteLine("Entering Main Function");


            KinectAll allKinects = new KinectAll();

            allKinects.StartAllKinects();

            Debug.WriteLine("LIST: " + allKinects.kinects.First().kinect.UniqueKinectId);



            // SP - Create an instance of the MainClass
            MainClass mainClass = new MainClass();

            // SP - Create a debug window with a reference to it
            Window debugWindow = new MainWindow(allKinects);

 

            // SP - Run the mainClass instance and open the debugWindow
            mainClass.Run(debugWindow);

            // SP - Now waiting for application to close


        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            //this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
        }

    }
}
