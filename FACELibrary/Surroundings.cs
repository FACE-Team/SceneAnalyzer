using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FACELibrary
{
    [Serializable()]
    public class Surroundings
    {
        private float SoundAngle;
        private float SoundEstimatedX;
        private string RecognizedWord = "";
        private float Saliency = (float)1;

        /// <summary>
        /// Set or get angle to the where comes the sound
        /// </summary>
        public float soundAngle
        {
            get { return SoundAngle; }
            set { SoundAngle = value; }
        }

        /// <summary>
        /// Set or get the estimated position (x) of the sound source (Math.Tan(Math.PI * soundAngle / 180))
        /// </summary>
        public float soundEstimatedX
        {
            get { return SoundEstimatedX; }
            set { SoundEstimatedX = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string recognizedWord
        {
            get { return RecognizedWord; }
            set { RecognizedWord = value; }
        }

        private List<float> VirtualSaliency_xy;
        /// <summary>
        /// Set or get the position (x,y) of the saliency
        /// </summary>
        public List<float> virtualsaliency_xy
        {
            get { return VirtualSaliency_xy; }
            set { VirtualSaliency_xy = value; }
        }

        /// <summary>
        /// Ser or get value of the saliency (default 1)
        /// </summary>
        public float saliency
        {
            get { return Saliency; }
            set { Saliency = value; }
        }

        private int NumberSubject = 0;
        /// <summary>
        /// Set or get the number of subject in the scene
        /// </summary>
        public int numberSubject
        {
            get { return NumberSubject; }
            set { NumberSubject = value; }
        }

        public Surroundings() { }

        public Surroundings(float angle, float x, string word, List<float> saliency_xy, float saliencyScore, int number)
        {
            soundAngle = angle;
            soundEstimatedX = x;
            RecognizedWord = word;
            VirtualSaliency_xy = saliency_xy;
            saliency = saliencyScore;
            numberSubject = number;
        }

    }
}