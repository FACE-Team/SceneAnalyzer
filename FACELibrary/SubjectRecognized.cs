using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FACELibrary
{
    [Serializable()]
    public class SubjectRecognized
    {

        private int Id;
        private string Name;
        private List<float> Position_xy;

        /// <summary>
        /// Subject id
        /// </summary>
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
      
      
        /// <summary>
        /// 
        /// </summary>
       
        public string name
        {
            get { return Name; }
            set { Name = value; }
        }
        /// <summary>
        /// Subject id
        /// </summary>
        public List<float> position_xy
        {
            get { return Position_xy; }
            set { Position_xy = value; }
        }
        

        public SubjectRecognized() { }

        public SubjectRecognized(int idSubject, string nameSubject, List<float> posSubject)
        {
            id = idSubject;
            name = nameSubject;
            position_xy = posSubject;
        }
    }
}
