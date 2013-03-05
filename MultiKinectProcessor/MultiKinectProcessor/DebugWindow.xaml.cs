/*
 * DebugWindow.xaml.cs
 * C# file to contruct the Debug Console UI
 * 
 * Original Author: Sea Pong <sea@seapong.com>
 * 
 */


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
using System.Windows.Threading;
using System.Threading;
using System.Globalization;
using System.IO;
using Microsoft.Kinect;
using System.Diagnostics;
using MultiKinectProcessor.SourceCode;


namespace MultiKinectProcessor
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// NOTE: SP - A Singleton pattern is used here to avoid static fucntion issues, since there will ever only be one instance of DebugWindow instantiated at any given time. 
    /// Original Author: Sea Pong
    /// </summary>
    /// 

    public partial class DebugWindow : Window
    {
        /// <summary>
        /// Creates an internal private instance of this class
        /// </summary>
        private static DebugWindow debugWindowPrivateInstance = new DebugWindow();

        private Thread calibrateThread = new Thread(new ThreadStart(KinectAll.kinectAll.CalibrateAll));
        

        /// <summary>
        /// Sets a public static getter that gets the internal private instance
        /// </summary>
        public static DebugWindow debugWindow
        {
            get { return debugWindowPrivateInstance; }
        }

        /// <summary>
        /// Sorry, Constructor is Private and an Instance of this Class is already created at startup, we should only ever have one instance of this class. Use the static getter method to use this Instance
        /// </summary>
        private DebugWindow()
        {
            calibrateThread.Name = "Calibration Thread";
            InitializeComponent();
            
        }

        /// <summary>
        /// Allows appending a line to the debug text box
        /// </summary>
        static public void addtoDebugTextBox(String input)
        {
            // Using Dispatcher so it foesnt conflict with another thread

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                debugWindow.debugTextBox.AppendText(input + "\r");
            }));
  
        }


        /// <summary>
        /// Allows getting the contents of the debug text box
        /// </summary>
        static public String getDebugTextBox()
        {
            //String text = new TextRange(debugWindow.debugTextBox.start
            //return debugWindow.debugTextBox.Document.;
            return null;
        }

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used to Kinect point
        /// </summary>
        private readonly Brush kinectPointBrush = Brushes.Orange;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Brush used for printing red error messages
        /// </summary>        
        private readonly Brush infoMessage = Brushes.Gray;

        /// <summary>
        /// Brush used for printing red error messages
        /// </summary>        
        private readonly Brush errorMessage = Brushes.Red;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroupTopView;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSourceTopView;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        private byte[] colorPixels;
        


        /// <summary>
        /// short designed to create a "clock" for user interface design
        /// </summary>
        private short clockCounter = 0;

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {



            if (KinectAll.kinectAll.getKinectCount() > 0)
            {
                Message.Info("debugWindow connected to sensor " + KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.UniqueKinectId);



                ////////////////////////////////////////
                //// SP - PREP SKELETON DATA OUTPUT ////
                ////////////////////////////////////////

                // Create the drawing group we'll use for drawing
                this.drawingGroup = new DrawingGroup();

                // Create an image source that we can use in our image control
                this.imageSource = new DrawingImage(this.drawingGroup);

                // Display the drawing using our image control
                // SP - Send data to the UI here
                SkeletonCanvas.Source = this.imageSource;


                ///////////////////////////////////////
                //// SP - PREP TOPVIEW DATA OUTPUT ////
                ///////////////////////////////////////

                // Create the drawing group we'll use for drawing
                this.drawingGroupTopView = new DrawingGroup();

                // Create an image source that we can use in our image control
                this.imageSourceTopView = new DrawingImage(this.drawingGroupTopView);

                // Display the drawing using our image control
                // SP - Send data to the UI here
                TopView.Source = this.imageSourceTopView;



                ////////////////////////////////////////////
                //// SP - PREP COLOR STREAM DATA OUTPUT ////
                ////////////////////////////////////////////

                this.colorPixels = new byte[KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.ColorStream.FramePixelDataLength];
               
                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.ColorStream.FrameWidth, KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                // SP - Send data to the UI here
                this.Video.Source = this.colorBitmap;

                /*
                ///////////////////////////////////////////////////////////
                //// SP - CALL HANDLERS TO PROCESS SKELETON/COLOR DATA ////
                //// these functions are called at 30 calls per sec    ////
                ///////////////////////////////////////////////////////////

                // Add an event handler to be called whenever there is new skeleton frame data
                // KinectAll.kinectAll.getFirstKinect().SkeletonFrameReady += this.SensorSkeletonFrameReady;
                try
                {
                    KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SensorSkeletonFrameReady);
                    Message.Info("Skeleton Frame Handler in debugWindow started");

                }
                catch
                {
                    Message.Warning("Skeleton Frame Handler in debugWindow not started");

                }


                // Add an event handler to be called whenever there is new color frame data
                // KinectAll.kinectAll.getFirstKinect().ColorFrameReady += this.SensorColorFrameReady;
                try
                {
                   KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(SensorColorFrameReady);
                   Message.Info("Color Frame Handler in debugWindow started");

                }
                catch
                {
                    Message.Warning("Color Frame Handler in debugWindow not started");

                }

                */

                KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(SensorAllFramesReady);

 
            }

            if (null == KinectAll.kinectAll.getFirstKinectSingle().kinectSensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != KinectAll.kinectAll.getFirstKinectSingle().kinectSensor)
            {
                KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];


            // SP - Import Skeleton Data
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }



            // SP - Draw Single Kinect Skeleton View
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        //RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }


            // SP - Draw Top View Scene View
            using (DrawingContext dctv = this.drawingGroupTopView.Open())
            {
                // Draw a transparent background to set the render size
                dctv.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));


        

                if (skeletons.Length != 0)
                {
                    int tracked = 0;
                    foreach (Skeleton skel in skeletons)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            tracked++;
                            if (tracked > 1)
                            {
                                // SP - Write error that only one skeleton can exist right now
                                this.statusBarText.Foreground = errorMessage;
                                this.statusBarText.Text = "During calibration, only 1 person may be in scene as the controller. " + skeletons.Length + " skeletons detected";

                                break;
                            }
                        }

                    }

                    if (tracked == 1)
                    {
                        clockCounter++;

                        // SP - Write Currently calibrating message
                        this.statusBarText.Foreground = infoMessage;
                        this.statusBarText.Text = "Calibration in Progress";

                        dctv.DrawEllipse(this.centerPointBrush, null, new Point(RenderWidth / 2.0, RenderHeight / 2.0), 30, 30);
                        
                        Point kinectLocation = new Point();
                        kinectLocation = KinectPointToScreen(KinectAll.kinectAll.kinectsList.First().GetDynamicDistance(), KinectAll.kinectAll.kinectsList.First().GetDynamicAngle());
                        
                        dctv.DrawEllipse(this.kinectPointBrush, null, kinectLocation, 20, 20);
                        dctv.DrawLine(inferredBonePen, new Point(RenderWidth / 2.0, RenderHeight / 2.0), kinectLocation);
                    }
                    else
                    {
                        // SP - WriteStep into frame message
                        this.statusBarText.Foreground = infoMessage;
                        this.statusBarText.Text = "Please step into the Kinect Frame";
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroupTopView.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null /* && KinectAll.kinectAll.getFirstKinectSingle().GetColorPixels() != null */)
                {


                    // Copy the pixel data from the image to a storage array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        KinectAll.kinectAll.getFirstKinectSingle().GetColorPixels(),
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }



        /// <summary>
        /// Event handler for Kinect sensor's AllFramesReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            Skeleton[] skeletons = new Skeleton[0];


            // SP - Import Skeleton Data
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }



            // SP - Draw Single Kinect Skeleton View
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        //RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }


            // SP - Draw Top View Scene View
            using (DrawingContext dctv = this.drawingGroupTopView.Open())
            {
                // Draw a transparent background to set the render size
                dctv.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));




                if (skeletons.Length != 0)
                {
                    int tracked = 0;
                    foreach (Skeleton skel in skeletons)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            tracked++;
                            if (tracked > 1)
                            {
                                // SP - Write error that only one skeleton can exist right now
                                this.statusBarText.Foreground = errorMessage;
                                this.statusBarText.Text = "During calibration, only 1 person may be in scene as the controller. " + skeletons.Length + " skeletons detected";

                                break;
                            }
                        }

                    }

                    if (tracked == 1)
                    {
                        clockCounter++;

                        // SP - Write Currently calibrating message
                        this.statusBarText.Foreground = infoMessage;
                        this.statusBarText.Text = "Calibration in Progress";

                        dctv.DrawEllipse(this.centerPointBrush, null, new Point(RenderWidth / 2.0, RenderHeight / 2.0), 30, 30);

                        Point kinectLocation = new Point();
                        kinectLocation = KinectPointToScreen(KinectAll.kinectAll.kinectsList.First().GetDynamicDistance(), KinectAll.kinectAll.kinectsList.First().GetDynamicAngle());

                        dctv.DrawEllipse(this.kinectPointBrush, null, kinectLocation, 20, 20);
                        dctv.DrawLine(inferredBonePen, new Point(RenderWidth / 2.0, RenderHeight / 2.0), kinectLocation);
                    }
                    else
                    {
                        // SP - WriteStep into frame message
                        this.statusBarText.Foreground = infoMessage;
                        this.statusBarText.Text = "Please step into the Kinect Frame";
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroupTopView.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null /* && KinectAll.kinectAll.getFirstKinectSingle().GetColorPixels() != null */)
                {


                    // Copy the pixel data from the image to a storage array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        KinectAll.kinectAll.getFirstKinectSingle().GetColorPixels(),
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }


        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            // this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            
            /*
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            /*
            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
            */

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                // SP - Only draw shoulders
                if (joint.JointType == JointType.ShoulderCenter)
                {
                    if (joint.TrackingState == JointTrackingState.Tracked)
                    {
                        drawBrush = this.trackedJointBrush;
                    }

                    // SP - Don't draw inferred points
                    /*
                    else if (joint.TrackingState == JointTrackingState.Inferred)
                    {
                        drawBrush = this.inferredJointBrush;
                    }
                    */
                    Typeface objectCaption = new Typeface("Segoe UI");
                    FormattedText text = new FormattedText("X:" + joint.Position.X.ToString() + " Z:" + joint.Position.Z.ToString(), System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, objectCaption, 12.0, Brushes.Gray);

                    if (drawBrush != null)
                    {
                        drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                        Point textOffset = this.SkeletonPointToScreen(joint.Position);
                        textOffset.Offset(20,-20);
                        drawingContext.DrawText(text, textOffset);
                        
                    }
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Maps a KinectPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private float maxScale = 0;
        private Point KinectPointToScreen(double distanceraw, double theta)
        {
            double distance = 0;
            distance = distanceraw * 20;

            float Xoffset, Yoffset;
            Xoffset = (float) (distance * System.Math.Cos(Calculation.degrees2Radians(theta)));
            Yoffset = (float) (distance * System.Math.Sin(Calculation.degrees2Radians(theta)));

    

            // Update Max Values
            if (System.Math.Abs(Xoffset) > maxScale)
            {
                maxScale = System.Math.Abs(Xoffset);
            }
            if (System.Math.Abs(Yoffset) > maxScale)
            {
                maxScale = System.Math.Abs(Yoffset);
            }



            double Xtarget, Ytarget;
            Xtarget = (Xoffset * ((RenderWidth / 2.0) / maxScale)) + (RenderWidth / 2.0);
            Ytarget = (Yoffset * ((RenderHeight / 2.0) / maxScale)) - (RenderHeight / 2.0);




            return new Point(Xtarget, Ytarget);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }
            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
            


        }




        //// UI EVENT HANDLERS ////

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != KinectAll.kinectAll.getFirstKinectSingle().kinectSensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    KinectAll.kinectAll.getFirstKinectSingle().kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }

        private void BeginCalibration_Click(object sender, RoutedEventArgs e)
        {
            
            if (calibrateThread.IsAlive == false)
            {

                Message.Info("Begin Caibration Button Clicked");

                if (calibrateThread != null)
                {
                    calibrateThread = new Thread(new ThreadStart(KinectAll.kinectAll.CalibrateAll));
                    calibrateThread.Name = "Calibration Thread";
                }
                calibrateThread.Start();
                //KinectAll.kinectAll.CalibrateAll();

                //Debug.WriteLine("static distance: " + KinectAll.kinectAll.kinectsList.First().GetStaticDistance());
                //Debug.WriteLine("static theta: " + KinectAll.kinectAll.kinectsList.First().GetStaticAngle());
            }
            else
            {
                MessageBox.Show("Calibration already in progress");
                Message.Error("Calibration already in progress");

            }
        }
    }
}