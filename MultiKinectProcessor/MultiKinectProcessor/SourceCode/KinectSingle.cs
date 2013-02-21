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
    /// Description: Contains code for controlling a single Kinect
    /// Author: Jerry Peng
    /// </summary>
    public class KinectSingle
    {
        /// <summary>
        /// kinect sensor pointer
        /// </summary>
        public KinectSensor kinectSensor;
        
        /// <summary>
        /// kinect unique id
        /// </summary>
        public int id;

        /// <summary>
        /// distance kinect is relative to user
        /// </summary>
        public double distance;

        /// <summary>
        /// height of the kinect releative to user
        /// </summary>
        public double height;

        /// <summary>
        /// skeleton data
        /// </summary>
        private Skeleton[] skeletonData;

        /// <summary>
        /// angle of kinect relative to user
        /// </summary>
        public double theta
        {
            get
            {
                return theta;
            }
            set
            {
                //theta should only between 0 and 360 degrees
                if (value >= 0 || value <= 360) 
                {
                    theta = value;
                }
                else
                {
                    Message.Error("Error: Set invalid theta value for Kinect: " + id);
                }
            }
        }
        //Variables for Debugging Purposes
        int count = 0;

        /// <summary>
        /// Enables Kinect Sensors
        /// </summary>
        /// <returns></returns>
        public bool enableKinectSensors()
        {
            //////////////////////////////
            //// SP - TURN ON SENSORS ////
            //////////////////////////////

            // Turn on the skeleton stream to receive skeleton frames
            kinectSensor.SkeletonStream.Enable();
            Message.Info("Skeleton Stream Enabled for " + kinectSensor.UniqueKinectId);

            // Turn on the color stream to receive color frames
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            Message.Info("Color Stream Enabled for " + kinectSensor.UniqueKinectId);

            return true;

        }


        ///<Function: CalibrateKinect>
        ///<Description: Calibrates Single Kinect>
        ///<Complexity: O(k)>
        ///<Author: Jerry Peng>
        public bool CalibrateKinect()
        {


            this.skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
            // kinect.SkeletonFrameReady += Kinect.kinect_SkeletonFrameReady

            kinectSensor.SkeletonStream.Disable(); //done with the kinect skeleton stream disable it

            return true;
        }

        ///<Function: kinect_SkeletonFrameReady>
        ///<Description: skeleton frame stream event handler>
        ///<Complexity: O(n)>
        ///<Author: Jerry Peng>
        public void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Debug.WriteLine("In kinect skeleton event handler");


            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                }
            }
            // Debug.WriteLine(this.skeletonData.Length);

            foreach (Skeleton skel in this.skeletonData)
            {
                if (skel.TrackingState == SkeletonTrackingState.Tracked)
                {
                    count++;
                    if (count > 50)
                    {
                        Console.WriteLine("id: " + skel.TrackingId + "shoulder L      X:" + skel.Joints[JointType.ShoulderLeft].Position.X + " Y: " + skel.Joints[JointType.ShoulderLeft].Position.Y + " Z: " + skel.Joints[JointType.ShoulderLeft].Position.Z);
                        Console.WriteLine("id: " + skel.TrackingId + "shoulder C      X:" + skel.Joints[JointType.ShoulderCenter].Position.X + "Y: " + skel.Joints[JointType.ShoulderCenter].Position.Y + " Z: " + skel.Joints[JointType.ShoulderCenter].Position.Z);
                        Console.WriteLine("id: " + skel.TrackingId + "shoulder R      X:" + skel.Joints[JointType.ShoulderRight].Position.X + "  Y: " + skel.Joints[JointType.ShoulderRight].Position.Y + " Z: " + skel.Joints[JointType.ShoulderRight].Position.Z);
                        double theta = Calculation.findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z);
                        theta = Calculation.radians2Degrees(theta);
                        Console.WriteLine("theta: " + theta.ToString());
                        //Console.WriteLine("id: " + skel.TrackingId + " X: " + skel.Position.X + " Y: " + skel.Position.Y + " Z: " + skel.Position.Z);
                        count = 0;

                    }
                }

            }
        }




    }
}
