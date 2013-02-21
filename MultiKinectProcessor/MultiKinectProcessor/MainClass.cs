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
    /// <summary>
    /// Description: contains the Main application entry point
    /// NOTE: SP - A Singleton pattern is used here to avoid static issues, since there will ever only be one instance of MinClass instantiated at any given time. 
    /// Modified by: Sea Pong
    /// </summary>
    public partial class MainClass : System.Windows.Application
    {


        /// <summary>
        /// Description: Application Entry Point
        /// Original Author: Sea Pong
        /// Modified by: Jerry Peng
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        static void Main()
        {
            Debug.WriteLine("Entering Main Function");

 

            // JP - Start all Kinects
            KinectAll.kinectAll.StartAllKinects();

            if (KinectAll.kinectAll.getKinectCount() > 0)
                Message.Info("KINECTS LIST: " + KinectAll.kinectAll.getFirstKinect().UniqueKinectId);
            else
                Message.Warning("No Kinects Found");



            // SP - Run the mainClass instance and open the debugWindow
            MainClass.mainClass.Run(DebugWindow.debugWindow);
            

            // SP - Now waiting for application to close


        }


        // Dont get rid of this
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            //this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
        }



        /// <summary>
        /// Creates an internal private instance of this class
        /// </summary>
        private static MainClass mainClassPrivateInstance = new MainClass();


        /// <summary>
        /// Use this public getter as a way to access the privately initialized KinectAll instance (KinectAll.kinectAll.keepgoin)
        /// </summary>
        public static MainClass mainClass
        {
            get { return mainClassPrivateInstance; }
        }

        /// <summary>
        /// Sorry, Constructor is Private and an Instance of this Class is already created at startup, we should only ever have one instance of this class. Use the static getter method to use this Instance.
        /// </summary>
        private MainClass()
        {
           
        }



    }
}
