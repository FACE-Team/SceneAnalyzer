using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace YarpManagerCS
{
    public class YarpBottle : Bottle 
    {
        public void addList(List<object> list)
        {

            Bottle ListBottle = this.addList();

            foreach (object stringa in list)
            {
                switch (stringa.GetType().Name) 
                {
                    case "String":
                        ListBottle.addString(stringa.ToString());
                        break;
                    case "Int":
                        ListBottle.addInt((int)stringa);
                        break;
                    case "Double":
                        ListBottle.addDouble(Convert.ToDouble(stringa));
                        break;
                    case "Float":
                        ListBottle.addDouble(Convert.ToDouble(stringa));
                        break;
                    default:
                       // ListBottle.addString(stringa.ToString());
                        break;
                }
            }

            //return ListBottle;
        }
    }
    
    public class YarpPort
    {
        private Port outPort;
        private BufferedPortBottle input;
        private ContactStyle style;

        private YarpBottle bot = new YarpBottle();
         
          
        /// <summary>
        /// 
        /// </summary>
        /// <param name="namePort"></param>
        public void openSender(string namePort) 
        {
            if (!String.IsNullOrEmpty(namePort))
            {
                Network.init();
                outPort = new Port();
                outPort.open(namePort);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameInputPort"></param>
        /// <param name="nameOutputPort"></param>
        public void openReceiver(string nameOutputPort,string nameInputPort) 
        {
            if (!String.IsNullOrEmpty(nameInputPort) && !String.IsNullOrEmpty(nameOutputPort))
            {
                Network.init();
                input = new BufferedPortBottle();       
                input.open(nameInputPort);
               
                style = new ContactStyle();
                style.persistent=true;

                if(Network.isConnected(nameOutputPort,nameInputPort,style))
                    Network.disconnect(nameOutputPort,nameInputPort,style);

                if (Network.connect(nameOutputPort, nameInputPort, style))
                    Console.WriteLine(nameInputPort + " port is connected to " + nameOutputPort);

            }
        }

        public void openReceiverReplyDb(string nameOutputPort, string nameInputPort)
        {
            if (!String.IsNullOrEmpty(nameInputPort) && !String.IsNullOrEmpty(nameOutputPort))
            {
                Network.init();
                input = new BufferedPortBottle();
                input.open(nameInputPort);
               
                style = new ContactStyle();
                style.persistent = true;

                //if (Network.isConnected(nameOutputPort, nameInputPort, style))
                    Network.disconnect(nameOutputPort, nameInputPort, style);

                if (Network.connect(nameOutputPort, nameInputPort, style))
                    Console.WriteLine(nameInputPort + " port is connected to " + nameOutputPort);

            }
        }

        public void openInputPort(string namePort)
        {
            if (!String.IsNullOrEmpty(namePort))
            {
                Network.init();
                input = new BufferedPortBottle();
                input.open(namePort);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameOutputPort"></param>
        /// <param name="namePortDb"></param>
        /// <returns></returns>
        public bool openConnectionToDb(string nameOutputPort, string namePortDb)
        {
            if (!String.IsNullOrEmpty(namePortDb) && !String.IsNullOrEmpty(nameOutputPort))
            {
                Network.init();

                outPort = new Port();
                outPort.open(nameOutputPort);

                style = new ContactStyle();
                style.persistent = true;

                if (PortExists(namePortDb))
                {
                    //if (Network.isConnected(nameOutputPort, namePortDb, style))
                        Network.disconnect(nameOutputPort, namePortDb,style);

                        if (Network.connect(nameOutputPort, namePortDb, style))
                    {
                        Console.WriteLine(namePortDb + " port is connected to " + nameOutputPort);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;

            }
            else
                return false;
        }

        #region SenderData
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void sendData(string data) 
        {
            //devo convertire la stringa in bottle
            Bottle bt = new Bottle();
            bt.add(data);
           
            outPort.write(bt);
            bt.clear();
            bt.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void sendData(float data)
        {
            //devo convertire la stringa in bottle
            Bottle bt = new Bottle();
            bt.addDouble(data);

            outPort.write(bt);
            bt.clear();
            bt.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void sendData(List<float> data)
        {
            //devo convertire la stringa in bottle
            Bottle bt = new Bottle();
            foreach (float ft in data)
            {
                //Bottle bt2 = new Bottle();
                bt.addDouble(ft);

                //bt.addList() = bt2;

            }
           

            outPort.write(bt);
            bt.clear();
            bt.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void sendData(Bottle data) 
        {
            //Bottle bt = new Bottle();
            //bt = outPort.prepare();
            //bt = data;
            outPort.write(data);
            // bt.clear();
        }

        #endregion

        public void sendComandOPC(string comand, YarpBottle bot) 
        {
            Bottle temp = new Bottle();
            temp.addString(comand);
            Bottle booo= temp.addList();

            booo.append(bot);

            outPort.write(temp);

            Console.WriteLine(temp.toString_c());
        }

        public void sendComandOPC(string comand, string data)
        {
            Bottle temp = new Bottle();
            temp.fromString(comand + " ("+data+")");

            outPort.write(temp);

        }

    
        public void receivedData(out string received) 
        {
            Bottle b = new Bottle();
            b=input.read();

            if (b != null)
                received = b.toString_c();
            else
                received = "";

            
            b.Dispose();
        }


        public YarpBottle Data 
        {
            get { return bot; }
            set { bot = value; }
        }


        public int getInputConnectionCount() 
        {
            return input.getInputCount();
        }
        
        public int getOutputConnectionCount() 
        {
            return outPort.getOutputCount();
        }

        #region Tools
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool NetworkExists() 
        {
            return Network.checkNetwork();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namePort"></param>
        /// <returns></returns>
        /// 
        public bool PortExists(string namePort) 
        {
            if (!string.IsNullOrEmpty(namePort))
                return Network.exists(namePort);
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameInputPort"></param>
        /// <param name="nameOutputPort"></param>
        /// <returns></returns>
        /// 
        public bool ConnectionExists(string nameInputPort, string nameOutputPort) 
        {
            if (!String.IsNullOrEmpty(nameInputPort) && !String.IsNullOrEmpty(nameOutputPort))
                return Network.isConnected(nameOutputPort, nameInputPort, style);
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameInputPort"></param>
        /// <param name="nameOutputPort"></param>
        /// 
        public void Disconect(string nameInputPort, string nameOutputPort)
        {
            if (!String.IsNullOrEmpty(nameInputPort) && !String.IsNullOrEmpty(nameOutputPort))
            {               
                style.persistent = true;

               
                    if(Network.disconnect(nameOutputPort, nameInputPort, style))
                        Console.WriteLine(nameInputPort + " port is disconnected to " + nameOutputPort);

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void Close()
        {
            if (input != null)
            {
                input.interrupt();
                input.close();
                input.Dispose();
            }

            if (outPort != null)
            {
                outPort.interrupt();
                outPort.close();
                //deletoutPortBottle;
            }

            Network.fini();
        }


        #endregion
    }
}
