﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class KinectSingle
    {
        /// <summary>
        /// kinect sensor pointer
        /// </summary>
        public KinectSensor kinectSensor;

        /// <summary>
        /// System Lock Object to lock the data copy operations
        /// </summary>
        private Object dataCopyLock = new Object();

        private Object CalibrateLock = new Object();

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

        /// <summary>
        /// keeps a record of number of skeletons tracked per frame
        /// </summary>
        private int trackedSkeletons;

        /// <summary>
        /// Semaphore used for calibration
        /// </summary>
        private static Semaphore calibrateBlock;

        //////////CALIBRATION STABILITY VARIABLES//////////

        /// <summary>
        /// variables for calibration stability
        /// </summary>
        private bool calibrationCheck;
        private int m_stabilityDistanceCount;
        private int m_stabilityThetaCount;
        private int m_stabilityHeightCount;
        private int stabilityDistanceCount;
        private int stabilityThetaCount;
        private int stabilityHeightCount;
        private int stabilityFaceTrackingCount;
        private double stabilityTheta;
        private double stabilityDistance;
        private double stabilityHeight;
        private bool stabilityFaceTracking;
        

        //////////CALIBRATION STATIC VARIABLES//////////
        readonly private double STABILITY_LEVEL = 100;
        readonly private double DISTANCE_BUFFER = 0.01;
        readonly private double HEIGHT_BUFFER = 0.1;
        readonly private double ANGLE_BUFFER = 1;

        /// <summary>
        /// Supports Facetracking
        /// </summary>
        private FaceTracker faceTracker;
        private FaceTrackFrame faceTrackFrame;
        private bool faceDetected;
        private bool faceTrackingStatic;
        private byte[] colorPixels;
        private short[] depthPixels;
        Skeleton skeletonOfInterest;






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
            stabilityFaceTrackingCount = 0;
            stabilityDistance = 0;
            stabilityTheta = 0;
            stabilityHeight = 0;
            stabilityFaceTracking = false;
            calibrationCheck = false;
            calibrateBlock = new Semaphore(0, 1);
            //Message.Info("stabilityThetaCount_after initialization: " + stabilityThetaCount);
            //Message.Info("stabilityDistanceCount_after initialization: " + stabilityDistanceCount);

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
            this.depthPixels = new short[kinectSensor.DepthStream.FramePixelDataLength];

            return true;
        }

        


        ///<Function: CalibrateKinect>
        ///<Description: Calibrates Single Kinect>
        ///<Complexity: O(k)>
        ///<Author: Jerry Peng>
        public bool CalibrateKinect()
        {
            if (calibrationCheck == false)
            {
                Initialize();// initialize variables for calibration

                calibrationCheck = true; //indicate start of calibration sequence

                Message.Info("Begin Calibrating kinect with id: " + kinectSensor.UniqueKinectId);

                kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_Calibrate);

                PositionStable(); //check position stability


                kinectSensor.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(kinect_Calibrate);

                // kinectSensor.SkeletonStream.Disable(); //done with the kinect skeleton stream disable it

                Message.Info("Calibration success!");
                Message.Info("Static Distance: " + distanceStatic.ToString());
                Message.Info("Static Theta: " + thetaStatic.ToString());
                
                calibrationCheck = false; //end of calibration sequence
                return true;
            }
            else
            {
                Message.Error("Already Calibrating");
                return false;

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
                    depthFrame.CopyPixelDataTo(this.depthPixels);
                }
            }

        }

        
/// <summary>
/// Thread that calibrates kinect
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

        private void kinect_Calibrate(object sender, SkeletonFrameReadyEventArgs e)
        {
            foreach (Skeleton skel in this.skeletonData)
            {
                if (skel.TrackingState == SkeletonTrackingState.Tracked)
                {
                    TestStable();
                }
            }
		}
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

                //Thread skeletonCopyThread = new Thread(() => skeletonFrame.CopySkeletonDataTo(this.skeletonData));
                //Thread colorCopyThread = new Thread(() => colorFrame.CopyPixelDataTo(this.colorPixels));
                //Thread depthCopyThread = new Thread(() => depthFrame.CopyDepthImagePixelDataTo(this.depthPixels));


                // SP
                // Lock this stream of code to make sure nothing happens between/during the 3 copy operations
                // Can thread each of these, but no significant performance gain
                lock (dataCopyLock)
                {
                    //// SKELETON ////
                    if (skeletonFrame != null)
                    {
                        // Copy the skeleton data from the image to a storage array
                        //skeletonCopyThread.Start();
                        skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                    }

                    //// COLOR IMAGE ////
                    if (colorFrame != null)
                    {
                        // Copy the pixel data from the image to a storage array
                        //colorCopyThread.Start();
                        colorFrame.CopyPixelDataTo(this.colorPixels);
                    }

                    //// DEPTH IMAGE ////
                    if (depthFrame != null)
                    {
                        // Copy the pixel data from the image to a storage array
                        //depthCopyThread.Start();
                        depthFrame.CopyPixelDataTo(this.depthPixels);
                        
                    }

                    //skeletonCopyThread.Join();
                    //colorCopyThread.Join();
                    //depthCopyThread.Join();
                }

                // Reset Counter Variables
                this.trackedSkeletons = 0;

                foreach (Skeleton skel in this.skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
						skeletonOfInterest = skel;
                        this.trackedSkeletons++;

                        theta = Calculation.radians2Degrees(Calculation.findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z));

                        distance = Calculation.findDistance(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z);

                        height = skel.Position.Y;
                        // Message.Info("Theta: " + theta);

                        //TestStable();
                    }
                }

                //// Face Detection - AS ///

                if (faceTracker != null)
                {
                    faceTrackFrame = faceTracker.Track(this.kinectSensor.ColorStream.Format, this.colorPixels, this.kinectSensor.DepthStream.Format, this.depthPixels, skeletonOfInterest);
                    faceDetected = faceTrackFrame.TrackSuccessful;
                    //Message.Info("FaceDetected: " + this.FaceDetected());
                }
                else
                {
                    try
                    {
                        faceTracker = new FaceTracker(this.kinectSensor);
                        Message.Info("Frame processed");
                    }
                    catch (InvalidOperationException) { Message.Info("Frame not processed"); }

                }
                //// Face Detection - AS ///
              

            }
        }




        

        /// <summary>
        /// checks position stable
        /// </summary>
        /// <returns></returns>
        private bool PositionStable()
        {
            calibrateBlock.WaitOne();
            return true;


        }
        private void TestStable()
        {
            Message.Info("stabilityThetaCount: " + stabilityThetaCount);
            Message.Info("stabilityDistanceCount: " + stabilityDistanceCount);
            Message.Info("stabilityHeightCount: " + stabilityHeightCount);
            Message.Info("stabilityFaceTracking: " + stabilityFaceTrackingCount);
            //Message.Info("Theta: " + theta);
            //Message.Info("Distance: " + distance);
           
            if (stabilityTheta == 0 && stabilityDistance == 0)//first
            {
                stabilityDistance = distance;
                stabilityTheta = theta;
                //Message.Info("Theta: " + theta);
                //Message.Info("Distance: " + distance);
            }
            /////////////////////////State machine//////////////////////////////
            else
            {
                //Angle
                if (stabilityTheta < (theta + ANGLE_BUFFER) && stabilityTheta > (theta - ANGLE_BUFFER))
                {
                    stabilityThetaCount++;
                }
                else
                {
                    stabilityThetaCount = 0;
                }
                //Distance
                if (stabilityDistance < (distance + DISTANCE_BUFFER) && stabilityDistance > (distance - DISTANCE_BUFFER))
                {
                    stabilityDistanceCount++;
                }
                else
                {
                    stabilityDistanceCount = 0;
                }
                //Height
                if (stabilityHeight < (height + HEIGHT_BUFFER) && stabilityHeight > (height - HEIGHT_BUFFER))
                {
                    stabilityHeightCount++;
                }
                else
                {
                    stabilityHeightCount = 0;
                }
                //faceTracking
                if (stabilityFaceTracking == faceDetected)
                {
                    stabilityFaceTrackingCount++;
                }
                else
                {
                    stabilityFaceTrackingCount = 0;
                }
                //////////////////////////////check/////////////////////////////////
                if (stabilityDistanceCount > STABILITY_LEVEL && stabilityThetaCount > STABILITY_LEVEL && stabilityHeightCount > STABILITY_LEVEL)
                {
                    
                    
                    distanceStatic = stabilityDistance;
                    thetaStatic = stabilityTheta;
                    heightStatic = stabilityHeight;
                    faceTrackingStatic = stabilityFaceTracking;
                    calibrateBlock.Release();
                }
                else
                {
                    stabilityDistance = distance;
                    stabilityTheta = theta;
                    stabilityHeight = height;
                    stabilityFaceTracking = faceDetected;
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
        public short[] GetDepthPixels()
        {
            return depthPixels;
        }

        /// AS
        /// <summary>
        /// Returns True if face is detected
        /// </summary>
        /// <returns></returns>
        public bool FaceDetected()
        {
            return faceDetected;
        }
       
    }
}
