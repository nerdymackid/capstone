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
    /// Author: Jerry Peng
    /// </summary>
    public class KinectAll
    {
        /// <summary>
        /// Contains data regarding every attached kinect
        /// </summary>
        public List<KinectSingle> kinects;

        /// <summary>
        /// Variable used for debugging purposes
        /// </summary>
        private int count = 0;



        /// <summary>
        /// Class Constructor
        /// </summary>
        public KinectAll()
        {
            kinects = new List<KinectSingle>();
        }



        // SP/JP - Temporary function to just get one kinect pointer
        public List<KinectSingle> getKinectList()
        {
            return kinects;
        }


        /// <summary>
        /// Start/initializes all Kinects
        /// </summary>
        public void StartAllKinects()
        {
            // Get only the first kinect rewrite latter to include all kinects attached
            KinectSensor kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
            


            //Checks whether the kinect is successfully added to the list
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
            if (null == kinect)
            {
                Debug.WriteLine("Kinect failed to start...");
            }
            else
            {
                Debug.WriteLine("Kinect started...");

            }
        }

        private bool AddKinect(KinectSensor k)
        {
            if (kinects == null)
            {
                //FIXME
                KinectSingle newKinect = new KinectSingle();
                newKinect.kinect = k;
                kinects.Add(newKinect);
                newKinect.enableKinectSensors();
                return true;
            }
            else
            {
                foreach (KinectSingle kinect in kinects)
                {
                    if (kinect.kinect.UniqueKinectId == k.UniqueKinectId)
                    {
                        return false; // This kinectSensor is already added
                    }
                }

                KinectSingle newKinect = new KinectSingle();
                newKinect.kinect = k;
                kinects.Add(newKinect);
                newKinect.enableKinectSensors();

                return true; // This kinectSensor has been successfully added
            }

        }
        //Calibrate all kinects connected
        public void CalibrateAll()
        {
            foreach (KinectSingle kinect in kinects)
            {
                if (kinect.CalibrateKinect() == false)
                {
                    Message.Warning("Uable to Calibrate Kinect with ID: " + kinect.kinect.UniqueKinectId);
                }

            }

        }
    }
}
