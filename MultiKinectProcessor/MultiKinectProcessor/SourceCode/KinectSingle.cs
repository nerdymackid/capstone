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
        /// Dynamic distance kinect is relative to user
        /// </summary>
        public double distance;

        /// <summary>
        /// Static distance kinect is relative to user determined from calibration
        /// </summary>
        public double distanceStatic;

        /// <summary>
        /// dynamic height of the kinect relative to user
        /// </summary>
        public double height;

        /// <summary>
        /// static height of the kinect relative to user determined from calibration
        /// </summary>
        public double heightStatic;

        /// <summary>
        /// dynamic angle of kinect relative to user
        /// </summary>
        public double theta;

        /// <summary>
        /// static angle of kinect to user obtained from calibraton
        /// </summary>
        public double thetaStatic;

        /// <summary>
        /// skeleton data
        /// </summary>
        private Skeleton[] skeletonData;

        /// <summary>
        /// skeleton id for the tracked individual
        /// </summary>
        private int skeletonId;

        //////////CALIBRATION STABILITY VARIABLES//////////

        /// <summary>
        /// 
        /// </summary>
        private int stabilityDistanceCount;
        private int stabilityThetaCount;
        private int stabilityHeightCount;
        private double stabilityTheta;
        private double stabilityDistance;
        private double stabilityHeight;
        private bool stableCheck;

        //////////CALIBRATION STATIC VARIABLES//////////
        readonly private double STABILITY_LEVEL=100;
        readonly private double DISTANCE_BUFFER = 0.1;
        readonly private double HEIGHT_BUFFER = 0.1;
        readonly private double ANGLE_BUFFER = 5;
        


        
      

        /// <summary>
        /// Class constructor
        /// Author: Jerry Peng
        /// </summary>
        public KinectSingle()
        {

            Initialize();
            
        }
        /// <summary>
        /// Description: initializes variables for calibration
        /// Complexity: O(k)
        /// Author: Jerry Peng
        /// </summary>
        private void Initialize()
        {
            stabilityDistanceCount = 0;
            stabilityThetaCount = 0;
            stabilityHeightCount = 0;
            stabilityDistance = 0;
            stabilityTheta = 0;
            stabilityHeight = 0;
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
            
            PositionStable(); //check position stability


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
                   
                    theta = Calculation.findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z);
                    theta = Calculation.radians2Degrees(theta);
                    distance = Calculation.findDistance(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z);
                    TestStable();
                    
                }

            }
        }

        /// <summary>
        /// checks position stable
        /// </summary>
        /// <returns></returns>
        private bool PositionStable()
        {
            while (stableCheck == false)
            {


            }
            return true;

            
        }
        private void TestStable()
        {
           // Debug.WriteLine("stabilityThetaCount: " + stabilityThetaCount);
           // Debug.WriteLine("stabilityThetaCount: " + stabilityThetaCount);
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
                else
                {
                    stabilityThetaCount = 0;
                }
                if (stabilityDistance < (distance + DISTANCE_BUFFER) && stabilityDistance > (distance - DISTANCE_BUFFER))
                {
                    stabilityDistanceCount++;
                }
                else
                {
                    stabilityDistanceCount = 0;
                }

                if (stabilityDistanceCount > STABILITY_LEVEL && stabilityThetaCount > STABILITY_LEVEL)
                {
                    stableCheck = true;
                    distanceStatic = stabilityDistance;
                    thetaStatic = stabilityTheta;
                }
                else
                {
                    stabilityDistance = distance;
                    stabilityTheta = theta;
                }
            }


            
        }
    }
}
