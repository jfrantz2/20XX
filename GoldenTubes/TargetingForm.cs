using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace GoldenTubes
{
    public partial class TargetingForm : Form
    {
        //====================VARIABLE DECLARATIONS====================//
        private WebClient client = new WebClient();
        private Dictionary<string, Dictionary<string, string>> regionDict;
        private List<string> regionList;
        private Dictionary<string, Dictionary<string, string>> nationDict;
        private List<string> nationList;
        private List<int>UpdateLength;
        string YearMonthDay = "";
        string Target = "";
        bool running = false;
        Thread upThread;
        Thread elThread;
        string User = "";
        string IP = "";
        //============================================================//

        public TargetingForm()
        {
            InitializeComponent();

            //This sets the "YearMonthDay" variable that is used in filenames. Format is YYYY-MM-DD
            YearMonthDay = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + DateTime.Today.Day.ToString();
            IP = new WebClient().DownloadString("http://icanhazip.com").Replace("\n","");
            try
            {
                //Attempts to load JSON data parsed from the day's daily data dump
                regionDict = getJSONData<Dictionary<string, Dictionary<string, string>>>(YearMonthDay + "RDict.JSON");
                regionList = getJSONData<List<string>>(YearMonthDay + "RList.JSON");
                nationDict = getJSONData<Dictionary<string, Dictionary<string, string>>>(YearMonthDay + "NDict.JSON");
                nationList = getJSONData<List<string>>(YearMonthDay + "NList.JSON");
                UpdateLength = getJSONData<List<int>>("UpdateLength.JSON");
            }
            catch
            {
                //If it fails, it will notify the user that no JSON data was found, and will instantiate al lthe variables.
                MessageBox.Show("No JSON data found. Updating, please wait...", "Notice");
                regionDict = new Dictionary<string, Dictionary<string, string>>();
                regionList = new List<string>();
                nationDict = new Dictionary<string, Dictionary<string, string>>();
                nationList = new List<string>();
                UpdateLength = new List<int>();
                //It will then update the data
                updateData();
            }
            //Starts the variance calculator thread
            running = true;

            upThread = new Thread(ThreadUClock);
            upThread.Start();
            while (!upThread.IsAlive) ; //Waits for the thread to begin

            elThread = new Thread(UpdateElements);
            elThread.Start();
            while (!elThread.IsAlive) ; //HURRY UP, GOD.
        }

        private void TargetButton_Click(object sender, EventArgs e)
        {
            if(TargetName.Text.Trim() != "")
            {
                Target = TargetName.Text;
            }
            else
            {
                MessageBox.Show("No target entered.", "Error");
            }
        }

        //Helper function for loading JSON data from files
        private T getJSONData<T>(string file)
        {
            StreamReader fs = new StreamReader(file);
            string JSON = fs.ReadToEnd();
            T returnData = JsonConvert.DeserializeObject<T>(JSON);
            return returnData;
        }

        //Helper function for saving JSON data files
        private void saveJSONData(string data, string fileName)
        {
            FileStream fstream = new FileStream(fileName, FileMode.Create);
            byte[] outstream = Encoding.ASCII.GetBytes(data.ToCharArray(), 0, data.Length);
            fstream.Write(outstream, 0, outstream.Length);
        }

        //Helper function for getting the index of a nation in the nation list. Returns -1 if it is not in the dictionary.
        private int getNationIndex(string nation)
        {
            if (nationDict.ContainsKey(nation))
                return Convert.ToInt32(nationDict[nation]["Index"]);
            else
                return -1;
            
        }

        private void updateData()
        {
            byte[] file; //This will contain the region data dump file.
            try
            {
                //If we already have the region data dump, don't download it again.
                file = File.ReadAllBytes(YearMonthDay + "regions.gz");
            }
            catch
            {
                //If we don't have it, download it
                client.DownloadFile("http://www.nationstates.net/pages/regions.xml.gz", YearMonthDay + "regions.gz");
                file = File.ReadAllBytes(YearMonthDay + "regions.gz");
            }
            string text; //The JSON string
            using (GZipStream stream = new GZipStream(new MemoryStream(file), CompressionMode.Decompress))
            {
                //Decrompress the data dump. I don't know remember what half this shit does
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    byte[] decompressed = memory.ToArray();
                    text = System.Text.ASCIIEncoding.ASCII.GetString(decompressed);
                }

            }

            //Parse the XML string from the data dump into the LINQ XDocument object
            XDocument xmlDoc = XDocument.Parse(text);
            //How many regions we've iterated through
            int counter = 0;
            //how many nations we've iterated through
            int nationcounter = 0;
            foreach (XElement Region in xmlDoc.Descendants("REGION"))
            {
                counter++;
                Dictionary<string, string> tmp = new Dictionary<string, string>(); //Create a dictionary that will store region data
                tmp.Add("Name", Region.Element("NAME").Value); //Add a name attribute
                tmp.Add("Index", counter.ToString()); //Add an index attribute (useful in conjunction with the region list)
                tmp.Add("NumNations", Region.Element("NUMNATIONS").Value); //Add an attribute for the number of nations in the region
                tmp.Add("FirstNation", Region.Element("NATIONS").ToString().Replace("<NATIONS>", "").Replace("</NATIONS>", "").Split(':')[0]); //Firstnation is the first updating nation in the region
                foreach (string Nation in Region.Element("NATIONS").ToString().Replace("<NATIONS>", "").Replace("</NATIONS>", "").Split(':'))
                {
                    nationcounter++;
                    nationList.Add(Nation); //Add to the nation list
                    Dictionary<string, string> tmp2 = new Dictionary<string, string>(); //Create a dictionary that will store nation data
                    tmp2.Add("Name", Nation); //Name Attribute
                    tmp2.Add("Index", nationcounter.ToString()); //Index attribute (Equivalent to #of nations before it)
                    if (Nation == tmp["FirstNation"])
                        tmp.Add("FirstNationIndex", nationcounter.ToString()); //If it's the first nation in the region, store it's index in the region.
                    if (Nation.Trim() != "" && !nationDict.ContainsKey(Nation.ToLower().Replace(' ', '_')))
                        nationDict.Add(Nation.ToLower().Replace(' ', '_'), tmp2); //There was a few issues with natons lacking a name, so we take these.
                }
                if (!regionDict.ContainsKey(Region.Element("NAME").Value.ToLower().Replace(' ', '_')))
                    regionDict.Add(Region.Element("NAME").Value.ToLower().Replace(' ', '_'), tmp); //If it doesn't exist in the regiondict, add that shit
                if (!regionList.Contains(Region.Element("NAME").Value.ToLower().Replace(' ', '_')))
                    regionList.Add(Region.Element("NAME").Value.ToLower().Replace(' ', '_')); //If it doens't exist in the list add that shit.
            }

            //Finally, save al lof them into JSON format files
            saveJSONData(JsonConvert.SerializeObject(regionDict), YearMonthDay + "RDict.JSON");
            saveJSONData(JsonConvert.SerializeObject(regionList), YearMonthDay + "RList.JSON");
            saveJSONData(JsonConvert.SerializeObject(nationDict), YearMonthDay + "NDict.JSON");
            saveJSONData(JsonConvert.SerializeObject(nationList), YearMonthDay + "NList.JSON");
        }

        //A function for use with threads :DDDDDDDDDDDDDDDDDD
        private void ThreadUClock()
        {
            TimeSpan TodayStamp; //Timestamp of the current date at the Major/Minor update
            if (DateTime.Now.Hour >= 12)
                TodayStamp = ((new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 16, 0, 0, DateTimeKind.Utc)) - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            else
                TodayStamp = ((new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 4, 0, 0, DateTimeKind.Utc)) - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            while (running) //DO THIS SHIT UNTIL I TELL YOU TO STOP
            {
                Thread.Sleep(1000);
                try
                {
                    //I SWEAR VIOLET, I'LL BE A GOOD BOY
                    client.Headers.Add("user-agent", "ALL HAIL 20XX. Currently in use by " + User + " at " + IP + ". Main Dev - doomjaw@hotmail.com");
                    //Grab the latest CTE like it's a juicy dick
                    string xmlSrc = client.DownloadString("http://www.nationstates.net/cgi-bin/api.cgi?q=happenings;filter=change;limit=5");
                    XDocument xmlDoc = XDocument.Parse(xmlSrc); //Parsing.
                    double NationStamp = 0;
                    XElement EventWeCareAbout = null; //Self explanatory
                    foreach (XElement Event in xmlDoc.Root.Element("HAPPENINGS").Elements())
                    {
                        if (Event.Element("TEXT").Value.Contains("influence in") || Event.Element("TEXT").Value.Contains("was ranked in the"))
                        {
                            EventWeCareAbout = Event;
                        }
                        NationStamp = Convert.ToInt64(Event.Element("TIMESTAMP").Value); //Parsing
                                                                                         //Console.Write(Event.Element("TEXT").Value);
                    }

                    if (EventWeCareAbout != null)
                    {
                        NationStamp = Convert.ToInt64(EventWeCareAbout.Element("TIMESTAMP").Value); //Parsing
                        Console.Write(NationStamp);
                    }
                    else if (NationStamp == 0)
                    {
                        continue;
                    }

                    int NationIndex = (EventWeCareAbout != null) ? getNationIndex(EventWeCareAbout.Element("TEXT").ToString().Split('@')[2]) : -1; //Parsing
                    double NationTime; //This is the time that we expect the CTE'd nation to have updated at, given our estimates
                    double TimePerNation; //This is how long nations take to update.
                    if (NationIndex != -1) //Only do what's in here if we have data on the CTE'd nation
                    {
                        //Calculate the time per nation (SecondsIntoUpdate / NationsUpdatedBefore)
                        string NationName = nationList[NationIndex];
                        if(nationDict.ContainsKey(NationName))
                            TimePerNation = (NationStamp - TodayStamp.TotalSeconds) / Convert.ToInt64(nationDict[NationName]["Index"]);
                        else
                        {
                            TimePerNation = 0.03f;
                        }

                        //Use the time per nation to extrapolate how long the update length is (We use this outside of the update)
                        if (UpdateLength.Count != 0)
                            UpdateLength[(DateTime.Now.Hour >= 12) ? 1 : 0] = (int)(nationList.Count * TimePerNation);
                        else
                        {
                            UpdateLength[0] = 5400;
                            UpdateLength[1] = 3600;
                        }

                        //We now calculate what time the CTE'd nation should be updating at given the data we've collected
                        NationTime = NationIndex * TimePerNation;
                    }
                    else
                    {
                        //We need this so that our program doesn't get dicked on if someone creates a nation like 'fuckshitpiss' and it get's CTE'd
                        //TimePerNation = UpdateLength/NumberOfNations
                        TimePerNation = (double)UpdateLength[(DateTime.Now.Hour >= 12) ? 1 : 0] / (double)nationList.Count;
                        //We don't actually have any data, so we'll just calculate how many seconds into the update the CTE happened.
                        NationTime = NationStamp - TodayStamp.TotalSeconds;
                    }

                    Console.Write(NationTime.ToString() + " " + TimePerNation.ToString() + "\n");

                    //Our estimate - UpdateStartTime + Seconds into the update we believe the CTE'd nation should be updating
                    double Estimate = TodayStamp.TotalSeconds + NationTime;
                    //Variance = Actual - Estimate. We compare the time the nation actually updates compared to our estimate
                    double Variance = NationStamp - Estimate;

                    Console.Write(Estimate.ToString() + " " + Variance.ToString() + "\n");

                    if (Target.Trim() != "" && regionList.Contains(Target.ToLower().Replace(' ', '_'))) //If we have a target
                    {
                        //Our approximate is the first updating nation in that region * the time it takes each nation to update
                        //We can safely use that nation's index because it's index represents the number of nations before it (as lists start with 0, not 1)
                        double approxtime = (Convert.ToInt32(regionDict[Target.ToLower().Replace(' ', '_')]["FirstNationIndex"]) * TimePerNation);
                        //We add the variance in, and convert it to a time...
                        TimeSpan t = TimeSpan.FromSeconds(approxtime + Variance);
                        //And spit it out for use by the user in the format of HH:MM:SS
                        UpdateTime(t.ToString(@"hh\:mm\:ss").ToString());
                    }
                    else
                    {
                        //If we have no target, just spit out 00:00:00
                        UpdateTime("00:00:00");
                    }

                    //No matter what happens, sleep 5 seconds because I'M A GOOD BOY VIOLET
                }
                catch(WebException e)
                {
                    Console.Write(e.Data.ToString());
                    Thread.Sleep(10000);
                }
            }
        }

        //The only thing this little shit does is provide a thread-safe way to change the "UpdateTime" label
        private delegate void delUpdateTime(string Time);
        private void UpdateTime(string Time)
        {
            if (InvokeRequired)
            {
                //Honestly, this is just a really complicated way to say "Call this shit with these arguments."
                Invoke(new delUpdateTime(UpdateTime), new object[] { Time });
            }
            else
            {
                //We called shit, and now we just set shit.
                EstimateTime.Text = Time;
            }
        }

        private void TargetingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Set running to false to tell that loop up in the thread function to STOP SLAPPING YOUR DICK AROUND
            running = false;
            //If it didn't get the message, jam a coathanger in it's vagina.
            upThread.Abort();
            elThread.Abort();
            //Wait for these fuckers to stop whining.
            while (upThread.IsAlive && elThread.IsAlive) ;
            //Save the update length
            saveJSONData(JsonConvert.SerializeObject(UpdateLength), "UpdateLength.JSON");
        }

        private delegate void delUpdateshit(string currentTime);
        private void Updateshit(string currentTime)
        {
            if (InvokeRequired)
            {
                //Honestly, this is just a really complicated way to say "Call this shit with these arguments."
                try { Invoke(new delUpdateTime(Updateshit), new object[] { currentTime }); } catch { } //AHAHAHAHAHAHAHAHAHAHAHAHAHA
            }
            else
            {
                //We called shit, and now we just set shit.
                CurrentTime.Text = currentTime;

                //Logic for the search
                
            }
        }

        //More thread bullshit.
        private void UpdateElements()
        {
            while(running) //You get the fucking idea.
            {
                //UpdateTime(t.ToString(@"hh\:mm\:ss").ToString());
                Updateshit(DateTime.Now.ToString(@"hh\:mm\:ss"));
            }
        }

        private void UserName_TextChanged(object sender, EventArgs e)
        {
            User = UserName.Text;
        }
    }
}
