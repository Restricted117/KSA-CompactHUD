using Brutal.Numerics;
using KSA;
using System.Xml;
using System.Xml.Linq;


namespace CompactHUD
{
    internal class Utilities
    {
        static public string MetersToString(double meters, bool speed = false)
        {
            string metersStr = meters.ToString("N2") + " m";

            if (Math.Abs(meters) / 1e15 >= 10)
                metersStr = (meters / 1e15).ToString("N2") + " Pm";
            else if (Math.Abs(meters) / 1e12 >= 10)
                metersStr = (meters / 1e12).ToString("N2") + " Tm";
            else if (Math.Abs(meters) / 1e9 >= 10)
                metersStr = (meters / 1e9).ToString("N2") + " Gm";
            else if (Math.Abs(meters) / 1e6 >= 10)
                metersStr = (meters / 1e6).ToString("N2") + " Mm";
            else if (Math.Abs(meters) / 1e3 >= 10)
                metersStr = (meters / 1e3).ToString("N2") + " Km";

            return speed ? metersStr + "/s" : metersStr;
        }


        static public string SecondsToString(double seconds)
        {
            string secondsStr = seconds.ToString("N2") + " s";

            if ((((Math.Abs(seconds) / 3600) / 24) / 365) / 100 >= 5)
                secondsStr = ((((seconds / 3600) / 24) / 365) / 100).ToString("N2") + " cent";
            else if (((Math.Abs(seconds) / 3600) / 24) / 365 >= 5)
                secondsStr = (((seconds / 3600) / 24) / 365).ToString("N2") + " yrs";
            else if ((Math.Abs(seconds) / 3600) / 24 >= 5)
                secondsStr = ((seconds / 3600) / 24).ToString("N2") + " days";
            else if (Math.Abs(seconds) / 3600 >= 5)
                secondsStr = (seconds / 3600).ToString("N2") + " hr";
            else if (Math.Abs(seconds) / 60 >= 5)
                secondsStr = (seconds / 60).ToString("N2") + " min";

            return secondsStr;
        }


        static public string Fit(string s)
        {
            s = s.ToUpperInvariant();
            return s.Length > 16 ? s.Substring(0, 16) : s;
        }


        static public void SetFC(Vehicle vehicle, Enum value, bool toggle)
        {
            bool rcsRateMode = vehicle.FlightComputer.AttitudeMode == FlightComputerAttitudeMode.Auto ? true : false;
            bool bTarget = false;
            bool bBurn = false;

            InputEvents.FlightComputerInputBuffer.Add(new InputEvents.FlightComputerInputData
            {
                Vehicle = vehicle,
                Toggle = toggle,
                EnumValue = value,
                Sound = null
            });
        }


        //window save data file path
        //static private string filePath = Path.Combine(AppContext.BaseDirectory, "Content", "CompactHUD", "WindowPositions.xml");
        static private string filePath = Path.Combine((Path.GetDirectoryName(typeof(CompactHUD).Assembly.Location)), "CompactHUD.xml");

        static public float2 LoadWinPos(string name)
        {
            float2 winPos = new float2(0.5f, 0.5f);

            if (!File.Exists(filePath))
            {
                VehicleInfoWindow.verticalLayout = false;
                return winPos;
            }
            try
            {
                XElement match = XDocument.Load(filePath).Root.Elements("Window").FirstOrDefault(p => (string)p.Element("Name") == name);

                if (name == VehicleInfoWindow.windowName)
                    VehicleInfoWindow.verticalLayout = (bool)match.Element("Vertical");

                return new float2((float)match.Element("X"), (float)match.Element("Y"));
            }
            catch
            {
                VehicleInfoWindow.verticalLayout = false;
                return winPos;
            }
        }


        static public void SaveWinPos(float2 winPos, string name, bool vertical = false)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                XDocument? doc = null;

                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    try
                    {
                        doc = XDocument.Load(filePath);
                    }
                    catch (XmlException)
                    {
                        doc = null;
                    }
                }

                if (doc is null || doc.Root is null)
                {
                    doc = new XDocument(new XElement("Windows"));
                }

                XElement existing = doc.Root.Elements("Window").FirstOrDefault(w => (string)w.Element("Name") == name);

                if (existing != null)
                {
                    existing.SetElementValue("X", winPos.X);
                    existing.SetElementValue("Y", winPos.Y);
                    existing.SetElementValue("Vertical", vertical);
                }
                else
                {
                    doc.Root.Add(new XElement("Window",
                        new XElement("Name", name),
                        new XElement("X", winPos.X),
                        new XElement("Y", winPos.Y),
                        new XElement("Vertical", vertical)));
                }

                doc.Save(filePath);
            }
            catch
            {
                Console.WriteLine("saving failed " + filePath.ToString());
            }
        }

    }
}
