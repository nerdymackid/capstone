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
        public double distanceStatic;

        /// <summary>
        /// height of the kinect releative to user
        /// </summary>
        public double height;

        /// <summary>
        /// skeleton data
        /// </summary>
        private Skeleton[] skeletonData;

        /// <summary>
        /// skeleton id for the tracked individual
        /// </summary>
        private int skeletonId;

        private int stabilityDistanceCount;
        private int stabilityThetaCount;
        private double stabilityTheta;
        private double stabilityDistance;
        private bool stableCheck;

        readonly private double STABILITY_LEVEL = 10;
        readonly private double DISTANCE_BUFFER = 0.1;
        readonly private double ANGLE_BUFFER = 10;



        /// <summary>
        /// angle of kinect relative to user
        /// </summary>
        public double theta;
        public double thetaStatic;

        /// <summary>
        /// Variables for Debugging Purposes
        /// </summary>
        int count = 0;

        /// <summary>
        /// Class constructor
        /// </summary>
        public KinectSingle()
        {

            Initialize();

        }
        private void Initialize()
        {
            stabilityDistanceCount = 0;
            stabilityThetaCount = 0;
            stabilityDistance = 0;
            stabilityTheta = 0;
            stableCheck = false;
        }
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
            Initialize();// initialize variables for calibration

            Message.Info("Calibrate kinect with id: " + kinectSensor.UniqueKinectId);

            this.skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);

            //PositionStable(); //check position stability


            // kinectSensor.SkeletonStream.Disable(); //done with the kinect skeleton stream disable it

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
                    if (count > 25)
                    {
                        //Debug.WriteLine("id: " + skel.TrackingId + "shoulder L      X:" + skel.Joints[JointType.ShoulderLeft].Position.X + " Y: " + skel.Joints[JointType.ShoulderLeft].Position.Y + " Z: " + skel.Joints[JointType.ShoulderLeft].Position.Z);
                        //Debug.WriteLine("id: " + skel.TrackingId + "shoulder C      X:" + skel.Joints[JointType.ShoulderCenter].Position.X + "Y: " + skel.Joints[JointType.ShoulderCenter].Position.Y + " Z: " + skel.Joints[JointType.ShoulderCenter].Position.Z);
                        //Debug.WriteLine("id: " + skel.TrackingId + "shoulder R      X:" + skel.Joints[JointType.ShoulderRight].Position.X + "  Y: " + skel.Joints[JointType.ShoulderRight].Position.Y + " Z: " + skel.Joints[JointType.ShoulderRight].Position.Z);
                        theta = Calculation.findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z);
                        theta = Calculation.radians2Degrees(theta);
                        distance = Calculation.findDistance(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z);
                        //.WriteLine("theta: " + theta.ToString());
                        //Debug.WriteLine("distance: " + distance.ToString());
                        //Console.WriteLine("id: " + skel.TrackingId + " X: " + skel.Position.X + " Y: " + skel.Position.Y + " Z: " + skel.Position.Z);
                        count = 0;

                    }
                    //TestStable();

                }

            }
        }

        private bool PositionStable()
        {
            while (stableCheck == false)
            {


            }
            return true;


        }
        private void TestStable()
        {
            if (stabilityTheta == 0 && stabilityDistance == 0)//first
            {
                stabilityDistance = distance;
                stabilityTheta = theta;
            }
            else
            {
                if (stabilityTheta < (theta + ANGLE_BUFFER) && stabilityTheta > (theta - ANGLE_BUFFER))
                {
                    stabilityThetaCount++;

                }
                if (stabilityDistance < (distance + DISTANCE_BUFFER) && stabilityDistance > (distance - DISTANCE_BUFFER))
                {
                    stabilityDistanceCount++;
                }
                stabilityDistance = distance;
                stabilityTheta = theta;
            }

            if (stabilityDistanceCount > STABILITY_LEVEL && stabilityThetaCount > STABILITY_LEVEL)
            {
                stableCheck = true;

            }

        }
    }
}
