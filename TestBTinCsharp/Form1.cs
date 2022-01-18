using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using InTheHand;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;
using InTheHand.Net.Sockets;
using System.IO;
using InTheHand.Net;
using InTheHand.Windows.Forms;
using System.Data.SqlClient;
using System.Data.OleDb;


namespace TestBTinCsharp
{
    public partial class Form1 : Form
    {
        OleDbConnection connect = new OleDbConnection();
        OleDbDataAdapter dataAdapter;
        DataTable localdatatable = new DataTable();
        OleDbDataAdapter dataAdapter2;
        DataTable localdatatable2 = new DataTable();
        OleDbDataAdapter TaxiAdapter;
        DataTable localTaxitable = new DataTable();
       // OleDbDataAdapter dataAdapter3;
        //DataTable localdatatable3 = new DataTable();
    
        List<string> items;
        List<BluetoothClient> connections;
        List<BluetoothAddress> address;
        int added = 0;
        int i = 0;
        string title;
        string details;
        public Form1()
        {
            items = new List<string>();
            InitializeComponent();
        }

        private void bGo_Click(object sender, EventArgs e)
        {
            if (serverStarted)
            {
                updateUI("server already started silly sausage !");
             //   return;
            }
            if (rbClient.Checked)
            {
                startscan(); 
            }
            else
            {
                connectAsServr();    
            }
        }

        private void startscan()
        {
            listbox.DataSource = null;
            listbox.Items.Clear();

            Thread bluetoothScanThread = new Thread(new ThreadStart(scan));
            bluetoothScanThread.Start();
        }
        BluetoothDeviceInfo[] devices;
        private void scan()
        {
            updateUI("Start Scan : ");
            BluetoothClient client = new BluetoothClient();
            devices = client.DiscoverDevicesInRange();
            updateUI("Scan Complete ^_^ ");
            updateUI(devices.Length.ToString() + " devices discoered");
            foreach (BluetoothDeviceInfo d in devices)
            {
                
                items.Add(d.DeviceName);
            }
            updateDeviceList();




        }

        private void connectAsServr()
        {

            Thread bluetoothConnectionControlThread = new Thread(new ThreadStart(ServerControlThread));
            bluetoothConnectionControlThread.IsBackground = true;
            bluetoothConnectionControlThread.Start();
            Thread BluetoothServerThread = new Thread(new ThreadStart(serverconnectedthread));
            BluetoothServerThread.Start();
            connections = new List<BluetoothClient>();
          
        }

       

        private void connectAsClient()
        {
            throw new NotImplementedException();
        }


        private void ServerControlThread()
        {
            while (true)
            {
                /*foreach (BluetoothClient bc in connections)
                {
                    if(!bc.Connected)
                    {
                        connections.Remove(bc);
                        break;
                    }
                }*/
                //  updateConnList(); // list with check for each client to send a message to all selected & connected clients
                Thread.Sleep(0);
            }
        }

        Guid mUUID = new Guid("00001101-0000-1000-8000-74F06DCF9AC9");            // B4527EC28F36      SP mobi
          
        bool serverStarted = false;
        public void serverconnectedthread()
        {

            updateUI("Server started , waiting for clients !!!");
            updateUI(DateTime.Now.ToString("HH:mm:ss tt"));
            BluetoothListener bluelistener;
            bluelistener = new BluetoothListener(mUUID);
            bluelistener.Start();
            serverStarted = true;
            while (true)
            {
             // handle server connection
                BluetoothClient conn = bluelistener.AcceptBluetoothClient();
                bool exist = true;
                int f = connections.Count;
                if(f==0)
                {
                    updateUI(conn.RemoteMachineName + " Has joined");
                    connections.Add(conn);
                    i++;
                   
                }
                else
                {
                    exist = found(conn.RemoteMachineName);
                    if(!exist)
                  {
                      updateUI(conn.RemoteMachineName + " Has joined");
                      exist = false;
                      i++;
                  }
                }
                
                //checkedListBox1.Items.Add(conn.RemoteMachineName);
                Thread app = new Thread(new ParameterizedThreadStart(ThreadForNewClientStream));
                app.IsBackground = true;
                app.Start(conn);
                
               

            }
        }

     
        // }
        //ThreadForNewClientStream
        private void ThreadForNewClientStream(object obj)
        {
            BluetoothClient client = (BluetoothClient)obj;
            Stream mStream = client.GetStream();
            mStream.ReadTimeout = 1000;
            while (client.Connected)
            {
                try
                {
                    int by = client.Available;
                    if (by > 0)
                    {
                        byte[] recived = new byte[by];
                        mStream.Read(recived, 0, recived.Length);
                        mStream.Flush();
                        String recMess = Encoding.ASCII.GetString(recived);
                        updateUI(client.RemoteMachineName + " sends : " + recMess);

                        string test = processReceivedMessage(recMess);
                        byte[] sent = new byte[1024];
                        if (test != null)
                        {
                            sent = Encoding.ASCII.GetBytes(test);
                            mStream.Write(sent, 0, sent.Length);
                            updateUI(Encoding.ASCII.GetString(sent));
                            mStream.Flush();
                        }
                    }
                }
                catch (IOException e)
                {
                    updateUI("client is disconnected !!!");
                    // mStream.Close(); 
                    i--;
                }
                Thread.Sleep(0);
            }
        }
       
        //found function
        bool found(string name)
        {
            bool result = true;
            foreach(BluetoothClient bc in connections)
            {
                if (name != bc.RemoteMachineName)
                    result = false;
                else
                    result = true; ;
            }
            return result;
        }
       
     
        string returnedstring;

        private string processReceivedMessage(string recMess)
        {
            string o_c = null;
            string dayofweek = System.DateTime.Now.DayOfWeek.ToString();
            TimeSpan start = new TimeSpan(8, 0, 0); //10 o'clock
            TimeSpan end = new TimeSpan(21, 0, 0); //12 o'clock
            TimeSpan start2 = new TimeSpan(8, 0, 0); //10 o'clock
            TimeSpan end2 = new TimeSpan(16, 0, 0); //12 o'clock
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (dayofweek.Contains("Monday") || dayofweek.Contains("Tuesday") || dayofweek.Contains("Wednesday") || dayofweek.Contains("Thursday") || dayofweek.Contains("Friday"))
            {
                if ((now > start) && (now < end))
                {
                    o_c = "opened ^_^";
                }
                else
                { o_c = "closed -_-"; }
            }
            if (dayofweek.Contains("Saturday") || dayofweek.Contains("Sunday"))
            {
                if ((now > start2) && (now < end2))
                {
                    o_c = "opened ^_^";
                }
                else
                { o_c = "closed -_-"; }
            }
            
            //Taxiiiii
            //////////////////////
            if(recMess.Contains("Fadi Taxi"))
            { 
                object s = localTaxitable.Rows[0]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Ammar Taxi"))
            {
                object s = localTaxitable.Rows[1]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("George Taxi"))
            {
                object s = localTaxitable.Rows[2]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Samer Taxi"))
            {
                object s = localTaxitable.Rows[3]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Majd Taxi"))
            {
                object s = localTaxitable.Rows[4]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Nabil Taxi"))
            {
                object s = localTaxitable.Rows[5]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Asaad Taxi"))
            {
                object s = localTaxitable.Rows[6]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Tareq Taxi"))
            {
                object s = localTaxitable.Rows[7]["status"];
                returnedstring = s.ToString();
            }
            if (recMess.Contains("Basil Taxi"))
            {
                object s = localTaxitable.Rows[8]["status"];
                returnedstring = s.ToString();
            }
            


            ///////////////////////////


            if (recMess.Contains("scoop"))
            {object s = localdatatable.Rows[12]["storeinfo"];
             object s1 = localdatatable.Rows[12]["floor"];
             object s3 = localdatatable.Rows[12]["call"];
            returnedstring = s.ToString()+"\n" +o_c+"\n"+ s1.ToString() +"\n"+s3.ToString(); 
          //  returnedstring = o_c + "\n" + s.ToString();
            }
            if (recMess.Contains("Gate 7"))
            {
                //object s4 = localdatatable.Rows[1]["storeinfo"];
                //returnedstring = o_c + "\n" + s4.ToString();

                object s4 = localdatatable.Rows[13]["storeinfo"];
                object s11 = localdatatable.Rows[13]["floor"];
                object s12= localdatatable.Rows[13]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString(); 
            }
            if (recMess.Contains("Al Tahle"))
            {
                //object s5 = localdatatable.Rows[2]["storeinfo"];
                //returnedstring = o_c + "\n" + s5.ToString();
                object s5 = localdatatable.Rows[14]["storeinfo"];
                object s13 = localdatatable.Rows[14]["floor"];
                object s14 = localdatatable.Rows[14]["call"];
                returnedstring = s5.ToString() + "\n" + o_c + "\n" + s13.ToString() + "\n" + s14.ToString(); 

            }
            if (recMess.Contains("Gogo phone"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[15]["storeinfo"];
                object s11 = localdatatable.Rows[15]["floor"];
                object s12 = localdatatable.Rows[15]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString(); 
            }
            if (recMess.Contains("Haddad Baby Fation"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[0]["storeinfo"];
                object s11 = localdatatable.Rows[0]["floor"];
                object s12 = localdatatable.Rows[0]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Gusto"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[1]["storeinfo"];
                object s11 = localdatatable.Rows[1]["floor"];
                object s12 = localdatatable.Rows[1]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("New Moon"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[2]["storeinfo"];
                object s11 = localdatatable.Rows[2]["floor"];
                object s12 = localdatatable.Rows[2]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Lucky"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[3]["storeinfo"];
                object s11 = localdatatable.Rows[3]["floor"];
                object s12 = localdatatable.Rows[3]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("cello"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[4]["storeinfo"];
                object s11 = localdatatable.Rows[4]["floor"];
                object s12 = localdatatable.Rows[4]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }

            if (recMess.Contains("Decran"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[5]["storeinfo"];
                object s11 = localdatatable.Rows[5]["floor"];
                object s12 = localdatatable.Rows[5]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Al Brj"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[6]["storeinfo"];
                object s11 = localdatatable.Rows[6]["floor"];
                object s12 = localdatatable.Rows[6]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Hajjo"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[7]["storeinfo"];
                object s11 = localdatatable.Rows[7]["floor"];
                object s12 = localdatatable.Rows[7]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Design Fation"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[8]["storeinfo"];
                object s11 = localdatatable.Rows[8]["floor"];
                object s12 = localdatatable.Rows[8]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Oxegyn Fation"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[9]["storeinfo"];
                object s11 = localdatatable.Rows[9]["floor"];
                object s12 = localdatatable.Rows[9]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Vatrine Fation"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[10]["storeinfo"];
                object s11 = localdatatable.Rows[10]["floor"];
                object s12 = localdatatable.Rows[10]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
            if (recMess.Contains("Farfasha Fation"))
            {
                //object s6 = localdatatable.Rows[3]["storeinfo"];
                //returnedstring = o_c + "\n" + s6.ToString();
                object s4 = localdatatable.Rows[11]["storeinfo"];
                object s11 = localdatatable.Rows[11]["floor"];
                object s12 = localdatatable.Rows[11]["call"];
                returnedstring = s4.ToString() + "\n" + o_c + "\n" + s11.ToString() + "\n" + s12.ToString();
            }
      


            ////////////////////////////////

            if (recMess.Contains("title") && i>1 )

            {
                if (title == null || added<=0)
                { returnedstring = "noev"; }
                else
                {
                    returnedstring = title;
                    added--; ;
                }
            
            }

            if (recMess.Contains("title") && i<=1 )
            {
                if (title == null || added <= 0)
                { returnedstring = "noev"; }
                else
                {
                    returnedstring = title;
                    added=0; 
                }

            }
            if (recMess.Contains("m1"))
            {
                object k = localdatatable2.Rows[0]["details"];
                returnedstring =  k.ToString();
            }
            if (recMess.Contains("m2"))
            {
                object k1 = localdatatable2.Rows[1]["details"];
                returnedstring = k1.ToString();
            }
           

                if (recMess.Contains("New event at cello")) {
                    DataTable localdatatable3=new DataTable();
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at cello')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    // OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                    
                
                }
                if (recMess.Contains("New event at Al Brj"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Al Brj')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }

                if (recMess.Contains("New event at Design"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Design')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at scoop"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at scoop')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Al Tahl"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Al Tahl')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Gogo ph"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Gogo ph')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Gate 7"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Gate 7')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Haddad"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Haddad')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Gusto"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Gusto')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at New Moo"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at New Moo')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Lucky"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Lucky')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Decran"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Decran')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Hajjo"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Hajjo')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Oxegyn"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Oxegyn')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Vatrine"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Vatrine')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();
                    //OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                    //cmd.ExecuteNonQuery();
                }
                if (recMess.Contains("New event at Farfash"))
                {
                    DataTable localdatatable3 = new DataTable(); ;
                    OleDbDataAdapter dataAdapter3 = new OleDbDataAdapter("SELECT TOP 1 details FROM seconde WHERE (title = 'New event at Farfash')ORDER BY id DESC", connect);
                    dataAdapter3.Fill(localdatatable3);
                    object r2 = localdatatable3.Rows[0]["details"];
                    returnedstring = r2.ToString();
                    localdatatable3.Clear();

                }
               
                
              //  returnedstring = details;
            

            if (recMess.Contains("m3"))
            {
                object k2= localdatatable2.Rows[2]["details"];
                returnedstring =   k2.ToString();
            }
            if (recMess.Contains("m4"))
            {
                object k3 = localdatatable2.Rows[3]["details"];
                returnedstring = k3.ToString();
            }
            if (recMess.Contains("m5"))
            {
                object k4 = localdatatable2.Rows[4]["details"];
                returnedstring = k4.ToString();
            }
          return returnedstring;   
        }
        private void updateUI(string message)
        {
            Func<int> del = delegate()
                {
                 tbOutput.AppendText(message + System.Environment.NewLine);
                    return 0;
                
                };

              Invoke(del);
        }



        private void updateDeviceList()
        {
         Func<int> del= delegate(){
            listbox.DataSource = items;
            return 0;
             };

         Invoke(del);
        
        }



        BluetoothDeviceInfo deviceInfo;
        private void listbox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            deviceInfo = devices.ElementAt(listbox.SelectedIndex);
            updateUI(deviceInfo.DeviceName + " is selected , attempting connect");
            
            
            //if (pairedDevice())
            //{
            //    updateUI("device paired  . .");
            //    updateUI("Starting connect thread");
            //    Thread bluetoothClientThread = new Thread(new ThreadStart(ClientConnectThread));
            //    bluetoothClientThread.Start();
            //}
            //else
            //{
            //    updateUI("pair failed");
            
            //}
            Thread ClientConnect = new Thread(new ThreadStart(ClientConnectThread));
            ClientConnect.Start();
        }

        private void ClientConnectThread()
        {
            BluetoothClient client = new BluetoothClient();
            updateUI("attemping connect ");
           
         //   client.BeginConnect(deviceInfo.DeviceAddress, mUUID,this.BluetoothClientConnectCallback,client);

            try
            {
                client.Connect(deviceInfo.DeviceAddress, mUUID);
                updateUI("Success!!");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                updateUI(ex.ToString());
            }
        
        }

        void BluetoothClientConnectCallback(IAsyncResult result)
        {

            BluetoothClient client = (BluetoothClient)result.AsyncState;
            client.EndConnect(result);
            Stream stream = client.GetStream();
            stream.ReadTimeout = 1000;
            while (true)
            {
                while (!ready) ;
                stream.Write(message, 0, message.Length);
            }
        
        }


        string myPin = "1234";
        private bool pairedDevice()
        {
            if (!deviceInfo.Authenticated)
            {
               if (!BluetoothSecurity.PairRequest(deviceInfo.DeviceAddress, myPin))
                { return false;}
            }

            return true;
         }


        bool ready = false;
        byte[] message;
        private void tbText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                message = Encoding.ASCII.GetBytes(tbText.Text);
                ready = true;
                tbText.Clear();
            }
        }

     

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            added = added + 2;
            /////////////////
          //  connect.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\my project\trydb\TestBTinCsharp\TestBTinCsharp\bin\Debug\Database3.accdb";
            title ="New event at "+ comboBox1.SelectedItem.ToString();
            updateUI(title);
            details = textBox1.Text;
            updateUI(details);


           // connect.Open();
            OleDbCommand cmd = new OleDbCommand("INSERT INTO [seconde] (title,details)" + " VALUES (@title,@details)", connect);
            if (connect.State == ConnectionState.Open)
            {
                cmd.Parameters.Add("@title", OleDbType.Char, 20).Value = title;
                cmd.Parameters.Add("@details", OleDbType.Char, 20).Value = details;
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("data added to database");
                    textBox1.Text = "";
                 //   textBox2.Text = "";
                  //  connect.Close();


                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Source, "   faild");
                    connect.Close();

                }
            }

            else
            {

                MessageBox.Show("connection faild");

            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            connect.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\Lectures\5th year\Graduation Project\New folder\7\newApdateReem\TestBTinCsharp\TestBTinCsharp\bin\Debug\Database3.accdb";
         //   connect.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\my project\trydb\TestBTinCsharp\TestBTinCsharp\bin\Debug\Database3.accdb";
            connect.Open();
              dataAdapter = new OleDbDataAdapter("SELECT  [first].*   FROM [first]", connect);
            dataAdapter.Fill(localdatatable);

            dataAdapter2 = new OleDbDataAdapter("SELECT  [seconde].*   FROM [seconde]", connect);
            dataAdapter2.Fill(localdatatable2);
            TaxiAdapter = new OleDbDataAdapter("SELECT [third].* FROM [third] ", connect);
            TaxiAdapter.Fill(localTaxitable);



        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
                OleDbCommand cmd = new OleDbCommand("DELETE FROM seconde", connect);
                cmd.ExecuteNonQuery();
            
        }

        private void tbOutput_TextChanged(object sender, EventArgs e)
        {

        }

       
        
    }
}
