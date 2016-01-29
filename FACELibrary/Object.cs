using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace FACELibrary
{
    [Serializable()]
    public class ObjectScene
    {
        private int Id;
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }

        public ObjectScene() { }

        public ObjectScene(int IdObj)
        {
            id = IdObj; 
        }

    }
}