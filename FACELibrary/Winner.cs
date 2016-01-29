using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FACELibrary
{
    [Serializable()]
    public class Winner
    {
        private int Id;
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }

        private float SpinX;
        public float spinX
        {
            get { return SpinX; }
            set { SpinX = value; }
        }

        private float SpinY;
        public float spinY
        {
            get { return SpinY; }
            set { SpinY = value; }
        }
        
        public Winner() { }

        public Winner(int idWin, float SpinXWin, float SpinYWin)
        {
            id = idWin;
            spinX = SpinXWin;
            spinY = SpinYWin;
        }
    }
}