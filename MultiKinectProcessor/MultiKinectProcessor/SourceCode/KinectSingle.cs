﻿using System;
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


using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace MultiKinectProcessor.SourceCode
{
    /// <summary>
    /// Description: Contains code for controlling a single Kinect
    /// Author: Jerry Peng
    /// </summary>
    public class KinectSingle : UserControl
    {
        /// <summary>
        /// kinect sensor pointer
        /// </summary>
        public KinectSensor kinectSensor;

        /// <summary>
        /// System Lock Object to lock the data copy operations
        /// </summary>
        private Object dataCopyLock = new Object();

        /// <summary>
        /// Dynamic distance kinect is relative to user
        /// </summary>
        private double distance;

        /// <summary>
        /// Static distance kinect is relative to user determined from calibration
        /// </summary>
        private double distanceStatic;

        /// <summary>
        /// dynamic height of the kinect relative to user
        /// </summary>
        private double height;

        /// <summary>
        /// static height of the kinect relative to user determined from calibration
        /// </summary>
        private double heightStatic;

        /// <summary>
        /// dynamic angle of kinect relative to user
        /// </summary>
        private double theta;

        /// <summary>
        /// static angle of kinect to user obtained from calibraton
        /// </summary>
        private double thetaStatic;

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
        /// variables for calibration stability
        /// </summary>
        private bool calibrationCheck;
        private int stabilityDistanceCount;
        private int stabilityThetaCount;
        private int stabilityHeightCount;
        private double stabilityTheta;
        private double stabilityDistance;
        private double stabilityHeight;
        private bool stableCheck;

        //////////CALIBRATION STATIC VARIABLES//////////
        readonly private double STABILITY_LEVEL = 100;
        readonly private double DISTANCE_BUFFER = 0.1;
        readonly private double HEIGHT_BUFFER = 0.1;
        readonly private double ANGLE_BUFFER = 5;

        /// <summary>
        /// Supports Facetracking
        /// </summary>
        private FaceTracker faceTracker;
        private FaceTrackFrame faceTrackFrame;
        private bool faceDetected;
        private byte[] colorPixels;
        private DepthImagePixel[] depthPixels;





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
            calibrationCheck = false;
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

            // Turn on the color stream to receive depth frames
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Message.Info("Depth Stream Enabled for " + kinectSensor.UniqueKinectId);

            return true;

        }


        /*
         
        /// <summary>
        /// Starts KinectSingle Skeleton Stream
        /// </summary>
        /// <returns>bool</returns>
        public bool StartSkelStream()
        {
            
            this.skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);

            return true;

        }

        // AS
        /// <summary>
        /// Starts KinectSingle Color Stream
        /// </summary>
        /// <returns>bool</returns>
        public bool StartColorStream()
        {
            // Allocate space to put the pixels we'll receive
            this.colorPixels = new byte[kinectSensor.ColorStream.FramePixelDataLength];

            kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinect_ColorFrameReady);

            return true;

        }

        // AS
        /// <summary>
        /// Starts KinectSingle Depth Stream
        /// </summary>
        /// <returns>bool</returns>
        public bool StartDepthStream()
        {

            this.depthPixels = new DepthImagePixel[kinectSensor.DepthStream.FramePixelDataLength];

            kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinect_DepthFrameReady);

            return true;

        }

        */

        /// <summary>
        /// Starts KinectSingle Skeleton, Color, and Depth Streams
        /// </summary>
        /// <returns></returns>
        public bool StartAllDataStreams()
        {
            try
            {
                Message.Info("Attempting to Start All Kinect Data Stream Handler");

                //KinectSingle.kinectSensorChangeProperty = DependencyProperty.Register(
                //    "KinectSingle",
                //    typeof(KinectSensor),
                //    typeof(KinectSingle),
                //    new PropertyMetadata(
                //        null, (o, args) => ((KinectSingle)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));
        
                kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);


            }
            catch
            {
                Message.Error("Could not create a DependencyProperty to sense data stream change. KinectSingle @ line 210");
                return false;
            }

            Message.Info("Successfully started All Data Stream Changed Event Handler");

            Message.Info("Allocating Memory for individual (skeleton, color, depth) streams");

            // Allocate Memory for Data Streams
            this.skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
            this.colorPixels = new byte[kinectSensor.ColorStream.FramePixelDataLength];
            this.depthPixels = new DepthImagePixel[kinectSensor.DepthStream.FramePixelDataLength];

            return true;
        }

        


        ///<Function: CalibrateKinect>
        ///<Description: Calibrates Single Kinect>
        ///<Complexity: O(k)>
        ///<Author: Jerry Peng>
        public bool CalibrateKinect()
        {
            if (calibrationCheck == true)
            {
                Initialize();// initialize variables for calibration

                Message.Info("Calibrate kinect with id: " + kinectSensor.UniqueKinectId);

                kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_Calibrate);

                PositionStable(); //check position stability
            }
            else
            {
                Message.Error("Calibration check boolean not set to true");

            }

            // kinectSensor.SkeletonStream.Disable(); //done with the kinect skeleton stream disable it

            return true;
        }

        public void kinect_Calibrate(object sender, SkeletonFrameReadyEventArgs e)
        {
            Message.Info("Calibration start...");

        }

        /*
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
                   
                    //TestStable();
                 

                }

            }
        }

        //  AS
        /// <summary>
        /// Event Handler for Color Frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a storage array
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                }
            }
        }

        //  AS
        /// <summary>
        /// Event Handler for Depth Frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        public void kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a storage array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
                }
            }

        }

        */

        /// <summary>
        /// Handler for all frames Frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="allFramesReadyEventArgs"></param>
        private void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (ColorImageFrame colorFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            using (DepthImageFrame depthFrame = allFramesReadyEventArgs.OpenDepthImageFrame())
            using (SkeletonFrame skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame())
            {
                if (colorFrame == null || depthFrame == null || skeletonFrame == null)
                {
                    Message.Warning("Not All Frames Present in Unified Event Handler. Skipping this data frame set...");
                    return;
                }


                lock (dataCopyLock)
                {
                    //// SKELETON ////
                    if (skeletonFrame != null) // check that a frame is available
                    {
                        //Message.Info("Copying Skeleton Data");
                        skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                    }

                    //// COLOR IMAGE ////
                    if (colorFrame != null)
                    {
                        // Copy the pixel data from the image to a storage array
                        colorFrame.CopyPixelDataTo(this.colorPixels);
                    }

                    //// DEPTH IMAGE ////
                    if (depthFrame != null)
                    {
                        // Copy the pixel data from the image to a storage array
                        depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
                    }
                }


                foreach (Skeleton skel in this.skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {

                        theta = Calculation.findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z);
                        theta = Calculation.radians2Degrees(theta);
                        distance = Calculation.findDistance(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z);

                        Message.Info("Theta: " + theta);

                        //TestStable();


                    }
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
            if (calibrationCheck == false)
            {
                return;
            }
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

        ////////////////////////////////////////////////Access Functions///////////////////////////////////////////

        /// <summary>
        /// gets pointer to kinectSensor
        /// </summary>
        /// <returns></returns>
        public KinectSensor GetKinectSensor()
        {
            return kinectSensor;
        }
        /// <summary>
        /// Description gets dynamic distance
        /// </summary>
        /// <returns>double</returns>
        public double GetDynamicDistance()
        {
            return distance;

        }
        /// <summary>
        /// Gets static distance obtained after calibration.
        /// </summary>
        /// <returns></returns>
        public double GetStaticDistance()
        {
            return distanceStatic;
        }
        /// <summary>
        /// Get dynamic angle  of kinect to user
        /// </summary>
        /// <returns></returns>
        public double GetDynamicAngle()
        {
            return theta;
        }
        /// <summary>
        /// Get static angle of kinect to user after calibration
        /// </summary>
        /// <returns></returns>
        public double GetStaticAngle()
        {
            return thetaStatic;
        }
        /// <summary>
        /// Get dynamic height of user NOTE: NOT IMPLEMENTED YET
        /// </summary>
        /// <returns></returns>
        public double GetDynamicHeight()
        {
            return height;
        }
        /// <summary>
        /// get Static height of user after calibration NOTE: NOT IMPLEMENTED YET
        /// </summary>
        /// <returns></returns>
        public double GetStaticHeight()
        {
            return heightStatic;
        }
        public Skeleton[] GetSkelData()
        {
            return skeletonData;
        }
        public byte[] GetColorPixels()
        {
            return colorPixels;
        }
        public DepthImagePixel[] GetDepthPixels()
        {
            return depthPixels;
        }
    }
}