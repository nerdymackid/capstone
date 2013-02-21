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
    /// Description: contains function related to Kinect sensor Operation
    /// NOTE: SP - A Singleton pattern is used here to avoid static issues, since there will ever only be one instance of KinectAll instantiated at any given time. 
    /// Original Author: Jerry Peng
    /// Modified by: Sea Pong
    /// </summary>
    public class KinectAll
    {



        /// <summary>
        /// Creates an internal private instance of this class
        /// </summary>
        private static KinectAll kinectAllPrivateInstance = new KinectAll();


        /// <summary>
        /// Use this public getter as a way to access the privately initialized KinectAll instance (KinectAll.kinectAll.keepgoin)
        /// </summary>
        public static KinectAll kinectAll
        {
            get { return kinectAllPrivateInstance; }
        }

        /// <summary>
        /// Sorry, Constructor is Private and an Instance of this Class is already created at startup, we should only ever have one instance of this class. Use the static getter method to use this Instance.
        /// </summary>
        private KinectAll()
        {
            kinectsList = new List<KinectSingle>();
        }



        /// <summary>
        /// Contains data regarding every attached kinect
        /// </summary>
        public List<KinectSingle> kinectsList;

        /// <summary>
        /// Variable used for debugging purposes
        /// </summary>
        private int count = 0;



        public int getKinectCount()
        {
            return count;
        }



        // SP/JP - Temporary function to just get one kinect pointer
        public KinectSensor getFirstKinect()
        {

            return kinectsList.First().kinectSensor;

        }



        /// <summary>
        /// Start/initializes all Kinects
        /// </summary>
        public void StartAllKinects()
        {
            // Get only the first kinect rewrite latter to include all kinects attached
            KinectSensor kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
            



            if (null == kinect)
            {
                Debug.WriteLine("Kinect failed to start...");
            }
            else
            {
                // Checks whether the kinect is successfully added to the list
                if (AddKinect(kinect) == false)
                {
                    Message.Warning("Addition of kinect with id: " + kinect.UniqueKinectId + " is unsucessful");
                }

                try
                {
                    kinect.Start();
                }
                catch (IOException)
                {
                    kinect = null;
                }

                Debug.WriteLine("Kinect started...");
                count++;

            }
        }

        private bool AddKinect(KinectSensor k)
        {
            if (kinectsList == null)
            {
                //FIXME - JP better way to add the first kinect?
                KinectSingle newKinect = new KinectSingle();
                newKinect.kinectSensor = k;
                kinectsList.Add(newKinect);
                newKinect.enableKinectSensors();
                return true;
            }
            else
            {
                foreach (KinectSingle kinect in kinectsList)
                {
                    if (kinect.kinectSensor.UniqueKinectId == k.UniqueKinectId)
                    {
                        return false; // This kinectSensor is already added
                    }
                }

                KinectSingle newKinect = new KinectSingle();
                newKinect.kinectSensor = k;
                kinectsList.Add(newKinect);
                newKinect.enableKinectSensors();

                return true; // This kinectSensor has been successfully added
            }

        }


        
        /// <summary>
        /// Calibrate all kinects connected
        /// </summary>
        public void CalibrateAll()
        {
            foreach (KinectSingle kinect in kinectsList)
            {
                if (kinect.CalibrateKinect() == false)
                {
                    Message.Warning("Unable to Calibrate Kinect with ID: " + kinect.kinectSensor.UniqueKinectId);
                }

            }

        }
    }
}
