using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
//using System.Windows.Media;
using Microsoft.Kinect;
using System.Diagnostics;


namespace User_tracking
{
 
    
    class Program
    {
        static void Main(string[] args)
        {
            Kinect k = new Kinect();
            k.StartKinectST();
            //Console.WriteLine(k.findUserTheta(5, 6, 7));
            while (true)
            {


            }

        }
        

        
    }
    class Kinect
    {
        private KinectSensor kinect = null;
        private Skeleton[] skeletonData;
        private int count=0;
        public void StartKinectST()
        {
            kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected); // Get first Kinect Sensor
             // Get Ready for Skeleton Ready Events
            if (kinect != null)
            {
                kinect.SkeletonStream.Enable(); // Enable skeletal tracking

                skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

                //kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
                kinect.SkeletonFrameReady += this.kinect_SkeletonFrameReady;
                

                Debug.WriteLine("skeleton enable");
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
        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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
                        Console.WriteLine("id: " + skel.TrackingId + "shoulder C      X:" + skel.Joints[JointType.ShoulderCenter].Position.X + " Y: " + skel.Joints[JointType.ShoulderCenter].Position.Y + " Z: " + skel.Joints[JointType.ShoulderCenter].Position.Z);
                        Console.WriteLine("id: " + skel.TrackingId + "shoulder R      X:" + skel.Joints[JointType.ShoulderRight].Position.X + " Y: " + skel.Joints[JointType.ShoulderRight].Position.Y + " Z: " + skel.Joints[JointType.ShoulderRight].Position.Z);
                        double theta = findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z);
                        theta = radians2Degrees(theta);
                        Console.WriteLine("theta: " + theta.ToString());
                        //Console.WriteLine("id: " + skel.TrackingId + " X: " + skel.Position.X + " Y: " + skel.Position.Y + " Z: " + skel.Position.Z);
                        count = 0;
                    }
                }

            }
        }

        ////* Theta Calculation - Scarlett *//

        // Returns theta given the two location points for a user's right and left shoulder
        static public double findUserTheta(double c, double d, double e, double f)
        {
            double A, B, C, preAcos, theta;
            B = System.Math.Sqrt(((c * c) + (d * d)));
            C = System.Math.Sqrt(((e * e) + (f * f)));
            A = System.Math.Sqrt(((c - e) * (c - e) + (d - f) * (d - f)));
            preAcos = ((A * A) + (B * B) - (C * C)) / (2 * A * B);
            if (preAcos <= 1.0 && preAcos >= -1.0)
            {
                theta = System.Math.Acos(preAcos);
            }
            else
            {
                Console.WriteLine("Invalid TRIANGLE, CANNOT COMPUTE THETA");
                return -1;
            }
            return theta;
        }
        static public double radians2Degrees(double radians)
        {
            double degrees = (radians * 180.0) / System.Math.PI;
            return degrees;
        }
        /* End Theta Calculation - Scarlett */

    }
    class calibrate
    {
        private kinectData[] kinects;
        struct kinectData
        {
            public int id;
            public double theta;

            public double distance;
            public double Height;


        }

        public void calibate()
        {
            //kinects = new k

        }

    }
}
