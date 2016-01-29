using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FACELibrary
{
    [Serializable()]
    public class Subject
    {
        private int IdKinect;
        private int Id = 0;
        private bool TrackedState = false;
        private List<string> Name = new List<string>(){ "unknown" };
        private string Gender = "unknown";
        private int Age;
        private float Speak_prob;
        private int Gesture = 0;
        private float Uptime;
        private float Angle;
        private float Happiness_ratio;
        private float Anger_ratio;
        private float Sadness_ratio;
        private float Surprise_ratio;
        private List<float> Head_xyz;
        private List<float> HeadOrient_rpy = new List<float>() { };

        private List<float> Spincenter_xyz;
        private List<float> NormalizedSpinCenter_xy;
        private List<float> Righthand_xyz;
        private List<float> Lefthand_xyz;
        private List<float> LeftWrist_xyz;
        private List<float> RightWrist_xyz;

        private List<float> LeftElbow_xyz;
        private List<float> RightElbow_xyz;

        private List<float> LeftShoulder_xyz;
        private List<float> RightShoulder_xyz;

        private List<float> MiddleEyes_xy;




        /// <summary>
        /// Indicates whether the kinect has generated the skeleton of the subject
        /// </summary>
        //[XmlIgnoreAttribute()]
        public int idKinect
        {
            get { return IdKinect; }
            set { IdKinect = value; }
        }

        /// <summary>
        /// Subject id
        /// </summary>
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
     
       /// <summary>
       /// Indicates subject
       /// </summary>
        [XmlIgnoreAttribute()]
        public bool trackedState
        {
            get { return TrackedState; }
            set { TrackedState = value; }
        }
     

        /// <summary>
        /// 
        /// </summary>
        public List<string> name
        {
            get { return Name; }
            set { Name = value; }
        }
        /// <summary>
        /// Set or get the sex of the subject
        /// </summary>
        public string gender
        {
            get { return Gender; }
            set { Gender = value; }
        }
       
        /// <summary>
        /// Set or get the age of the subject
        /// </summary>
        public int age
        {
            get { return Age; }
            set { Age = value; }
        }
       
        /// <summary>
        /// Set or get the probability that the subject is speaking
        /// </summary>
        public float speak_prob
        {
            get { return Speak_prob; }
            set { Speak_prob = value; }
        }
        /// <summary>
        /// Set or get the gesture of the subject (no gesture == 0, hand raised==1)
        /// </summary>
        public int gesture
        {
            get { return Gesture; }
            set { Gesture = value; }
        }

        /// <summary>
        /// Set or get how long the subject is present
        /// </summary>
        public float uptime
        {
            get { return Uptime; }
            set { Uptime = value; }
        }

        /// <summary>
        /// Set or get the angle of the subject in the scene
        /// </summary>
        public float angle
        {
            get { return Angle; }
            set { Angle = value; }
        }

        /// <summary>
        /// Set or get the probability of happiness a subject, expressed in percentage
        /// </summary>
        public float happiness_ratio
        {
            get { return Happiness_ratio; }
            set { Happiness_ratio = value; }
        }

        /// <summary>
        /// Set or get the probability that the subject is angry, expressed in percentage
        /// </summary>
        public float anger_ratio
        {
            get { return Anger_ratio; }
            set { Anger_ratio = value; }
        }
      
        /// <summary>
        /// Set or get the probability that the subject is sad, expressed as a percentage
        /// </summary>
        public float sadness_ratio
        {
            get { return Sadness_ratio; }
            set { Sadness_ratio = value; }
        }
       
        /// <summary>
        /// Set or get the probability that the subject is surprised, expressed as a percentage
        /// </summary>
        public float surprise_ratio
        {
            get { return Surprise_ratio; }
            set { Surprise_ratio = value; }
        }
       

        /// <summary>
        /// Set or get the position (x, y, z) of the subject's head   
        /// </summary>
        public List<float> head_xyz
        {
            get { return Head_xyz; }
            set { Head_xyz = value; }
        }


        /// <summary>
        /// Set or get the Orientation (Roll, Pitch, Yaw) of the subject's head   
        /// </summary>
        public List<float> headorient_rpy
        {
            get { return HeadOrient_rpy; }
            set { HeadOrient_rpy = value; }
        }
       
        /// <summary>
        /// Set or get the position (x,y,z) of the spin center of the subject
        /// </summary>
        public List<float> spincenter_xyz
        {
            get { return Spincenter_xyz; }
            set { Spincenter_xyz = value; }
        }
     
        /// <summary>
        /// Set or get the position (x,y,z) of the spin center of the subject normalized between [0-1]
        /// </summary>
        public List<float> normalizedspincenter_xy
        {
            get { return NormalizedSpinCenter_xy; }
            set { NormalizedSpinCenter_xy = value; }
        }
     

        /// <summary>
        /// Set or get the position (x, y, z) of the right hand of the subject
        /// </summary>
        public List<float> righthand_xyz
        {
            get { return Righthand_xyz; }
            set { Righthand_xyz = value; }
        }
      

        /// <summary>
        /// Set or get the position (x, y, z) of the left hand of the subject
        /// </summary>
        public List<float> lefthand_xyz
        {
            get { return Lefthand_xyz; }
            set { Lefthand_xyz = value; }
        }

        /// <summary>
        /// Set or get the position (x, y, z) of the left wrist of the subject
        /// </summary>
        public List<float> leftwrist_xyz
        {
            get { return LeftWrist_xyz; }
            set { LeftWrist_xyz = value; }
        }

        /// <summary>
        /// Set or get the position (x, y, z) of the right wrist of the subject
        /// </summary>
        public List<float> rightwrist_xyz
        {
            get { return RightWrist_xyz; }
            set { RightWrist_xyz = value; }
        }

        /// <summary>
        /// Set or get the position (x, y, z) of the left elbow  of the subject
        /// </summary>
        public List<float> leftelbow_xyz
        {
            get { return LeftElbow_xyz; }
            set { LeftElbow_xyz = value; }
        }

        
        /// Set or get the position (x, y, z) of the right elbow  of the subject
        /// </summary>
        public List<float> rightelbow_xyz
        {
            get { return RightElbow_xyz; }
            set { RightElbow_xyz = value; }
        }

        /// <summary>
        /// Set or get the position (x, y, z) of the left shoulder  of the subject
        /// </summary>
        public List<float> leftshoulder_xyz
        {
            get { return LeftShoulder_xyz; }
            set { LeftShoulder_xyz = value; }
        }

        /// <summary>
        /// Set or get the position (x, y, z) of the right shoulder of the subject
        /// </summary>
        public List<float> rightshoulder_xyz
        {
            get { return RightShoulder_xyz; }
            set { RightShoulder_xyz = value; }
        }

        /// <summary>
        /// Set or get the position (x, y, z) of the right shoulder of the subject
        /// </summary>
        public List<float> middleeyes_xy
        {
            get { return MiddleEyes_xy; }
            set { MiddleEyes_xy = value; }
        }


        public Subject() { }

        public Subject(int idSub, List<string> nameSub, string genderSub, int ageSub,
            float speakingProbabilitySub, int gestureSub, float uptimeSub, float angleSub,
            float happinessRatioSub, float angerRatioSub, float sadnessRatioSub, float surpriseRatioSub,
            List<float> headSub, List<float> spincenterSub, List<float> normalizedSpinCenterSub, List<float> righthandSub, List<float> lefthandSub)
        {
            Id = idSub;
            Name = nameSub;            
            Gender = genderSub;
            Age = ageSub;
            Speak_prob = speakingProbabilitySub;
            Gesture = gestureSub;
            Uptime = uptimeSub;
            Angle = angleSub;
            Happiness_ratio = happinessRatioSub;
            Anger_ratio = angerRatioSub;
            Sadness_ratio = sadnessRatioSub;
            Surprise_ratio = surpriseRatioSub;
            Head_xyz = headSub;
            Spincenter_xyz = spincenterSub;
            NormalizedSpinCenter_xy = normalizedSpinCenterSub;
            Righthand_xyz = righthandSub;
            Lefthand_xyz = lefthandSub;
        }

        
    }
}