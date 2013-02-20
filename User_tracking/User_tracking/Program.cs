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
    
   
    class MainProgram
    {
        static void Main(string[] args)
        {
            KinectAll k = new KinectAll();
            k.StartKinectST();
            k.CalibrateAll();
           
            //Console.WriteLine(k.findUserTheta(5, 6, 7));
            while (true)
            {


            }

        }
        

        
    }
 
    
    /// <summary>
    /// <Class>KinectAll</Class> 
    /// <Description>contains function related to Kinect sensor Operation</Description>
    /// <Author>Jerry Peng</Author>
    /// </summary>
    class KinectAll
    {
        /// <summary>
        /// contains data regarding every attached kinect
        /// </summary>
        private List<KinectSingle> kinects;

        /// <summary>
        /// Variable used for debugging purposes
        /// </summary>
        private int count=0;


        /// <summary>
        /// Description: Start/initializes all Kinects
        /// Complexity: unknown
        /// Author: Jerry Peng
        /// </summary>
        public void StartKinectST()
        {
            // Get only the first kinect rewrite latter to include all kinects attached
            KinectSensor kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
            //Checks whether the kinect is successfully added to the list
            if (AddKinect(kinect)==false)
            {
                Message.Error("WARNING: Addition of kinect with id: " + kinect.UniqueKinectId + " is unsucessful");
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
            foreach (KinectSingle kinect in kinects)
            {
                if (kinect.kinect.UniqueKinectId == k.UniqueKinectId)
                {
                    return false; // This kinectSensor is already added
                }
            }
            KinectSingle newKinect = new KinectSingle();
            newKinect.kinect=k;
            kinects.Add(newKinect);
            return true; // This kinectSensor has been successfully added


        }
        //Calibrate all kinects connected
        public void CalibrateAll()
        {
            foreach (KinectSingle kinect in kinects)
            {
                if(kinect.CalibrateKinect()==false)
                {
                    Message.Warning("Uable to Calibrate Kinect with ID: " + kinect.kinect.UniqueKinectId);
                }

            }

        }
        

       
    }
    /// <summary>
    /// <Class: KinectSingle>
    /// <Description: Encapsulates data for single kinect
    /// </summary>
    class KinectSingle
    {
        public KinectSensor kinect;  //kinect sensor pointer
        public int id;  //kinect unique id
        public double distance; //distance kinect is relative to user
        public double height;  //height of the kinect releative to user
        private Skeleton[] skeletonData; // skeleton data
        public double theta //angle of kinect relative to user
        {
            get
            {
                return theta;
            }
            set
            {
                if (value >= 0 || value <= 360) //theta only between 0 and 360 degrees
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

        ///<Function: CalibrateKinect>
        ///<Description: Calibrates Single Kinect>
        ///<Complexity: O(k)>
        ///<Author: Jerry Peng>
       public bool CalibrateKinect()
        {
            kinect.SkeletonStream.Enable(); // Enable skeletal tracking
            Debug.WriteLine("skeleton enable");
            this.skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
            // kinect.SkeletonFrameReady += Kinect.kinect_SkeletonFrameReady

            kinect.SkeletonStream.Disable(); //done with the kinect skeleton stream disable it

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
                        double theta = Calcuation.findUserTheta(skel.Joints[JointType.ShoulderCenter].Position.X, skel.Joints[JointType.ShoulderCenter].Position.Z, skel.Joints[JointType.ShoulderRight].Position.X, skel.Joints[JointType.ShoulderRight].Position.Z);
                        theta = Calcuation.radians2Degrees(theta);
                        Console.WriteLine("theta: " + theta.ToString());
                        //Console.WriteLine("id: " + skel.TrackingId + " X: " + skel.Position.X + " Y: " + skel.Position.Y + " Z: " + skel.Position.Z);
                        count = 0;

                    }
                }

            }
        }



    }
   
    //class: calculation
    //description: class to containing all the calcuations needed
 
    class Calcuation
    {
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
    class Message
    {
        static public void Error(string msg)
        {
            Debug.WriteLine("WARNING: "+msg);
        }
        static public void Warning(string msg)
        {
            Debug.WriteLine("ERROR: "+msg);
        }


    }
}
