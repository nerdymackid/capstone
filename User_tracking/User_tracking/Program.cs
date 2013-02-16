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
                        //Console.WriteLine("id: " + skel.TrackingId + " X: " + skel.Position.X + " Y: " + skel.Position.Y + " Z: " + skel.Position.Z);
                        count = 0;
                    }
                }

            }
        }

        /*
        private void DrawTrackedSkeletonJoints(JointCollection jointCollection)
        {
            // Render Head and Shoulders
            DrawBone(jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter]);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft]);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight]);

            // Render Left Arm
            //DrawBone(jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft]);
           // DrawBone(jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft]);
            //DrawBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft]);

            // Render Right Arm
            //DrawBone(jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight]);
           // DrawBone(jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight]);
            //DrawBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);

            // Render other bones...
        }
        private void DrawBone(Joint jointFrom, Joint jointTo)
        {
            if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
            jointTo.TrackingState == JointTrackingState.NotTracked)
            {
                return; // nothing to draw, one of the joints is not tracked
            }

            if (jointFrom.TrackingState == JointTrackingState.Inferred ||
            jointTo.TrackingState == JointTrackingState.Inferred)
            {
                DrawNonTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw thin lines if either one of the joints is inferred
            }

            if (jointFrom.TrackingState == JointTrackingState.Tracked &&
            jointTo.TrackingState == JointTrackingState.Tracked)
            {
                DrawTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw bold lines if the joints are both tracked
            }
        }

        */

    }
}
