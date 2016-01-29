using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FACELibrary
{
    [Serializable()]
    public class FaceExpression
    {
        private float Valence;
        public float valence
        {
            get { return Valence; }
            set { Valence = value; }
        }

        private float Arousal;
        public float arousal
        {
            get { return Arousal; }
            set { Arousal = value; }
        }


        public FaceExpression() { }

        public FaceExpression(float v, float a)
        {
            Valence = v;
            Arousal = a;
        }
    }
}