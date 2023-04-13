using System;
using System.Collections.Generic;

namespace MirageXR
{
    [Serializable]
    public class Workplace
    {
        public string id = "";
        public string name = "";

        // NOT IN ARLEM SPEC. Version checking is used for defining should local content be used or not.
        public string version = "";


        public List<Thing> things = new List<Thing>();
        public List<Place> places = new List<Place>();
        public List<Person> persons = new List<Person>();

        public List<Sensor> sensors = new List<Sensor>();
        public List<Device> devices = new List<Device>();
        public List<App> apps = new List<App>();
        public List<Detectable> detectables = new List<Detectable>();

        public List<Primitive> predicates = new List<Primitive>();
        public List<Primitive> warnings = new List<Primitive>();
        public List<Primitive> hazards = new List<Primitive>();
    }

    [Serializable]
    public class Thing
    {
        public string id = "";
        public string name = "";
        public string urn = "";
        public string detectable = "";

        public List<Poi> pois = new List<Poi>();

        // NOT IN ARLEM SPEC. Used for new tracking types enabled by Hololens
        public string type = ""; // fixed, handheld or raw (default)
        public float radius = 0.75f; // Tracking active area Radius

        // NON IN ARLEM SPEC. For linking sensors to objects.
        public string sensor = "";
    }

    [Serializable]
    public class Poi
    {
        public string id = "";
        public float x_offset = 0f;
        public float y_offset = 0f;
        public float z_offset = 0f;

        // NOT IN ARLEM SPEC. Just a nicer way to write the offset instead of using the separate XYZ variables...
        public string offset = ""; // Vector 3 format x,y,z
        public string rotation = ""; // Also in a vector3 format x,y,z
        public string scale = ""; // Also in a vector3 format x,y,z
    }

    [Serializable]
    public class Place
    {
        public string id = "";
        public string name = "";
        public string detectable = "";
        public string type = "";

        // Not in ARLEM spec.
        public List<Poi> pois = new List<Poi>();

        // Not in ARLEM spec.
        public string sensor = "";
    }

    [Serializable]
    public class Person
    {
        public string id = "";
        public string name = "";
        public string twitter = "";
        public string mbox = "";
        public string detectable = "";
        public string persona = "";

        // Not in ARLEM spec.
        public List<Poi> pois = new List<Poi>();

        // Not in ARLEM spec.
        public string sensor = "";
    }

    [Serializable]
    public class Sensor
    {
        public string id = "";
        public string uri = "";
        public string username = "";
        public string password = "";

        public List<Data> data = new List<Data>();

        // NOT IN ARLEM SPEC. For displaying sensor name in UI.
        public string name = "";

        // NOT IN ARLEM SPEC. For defining sensor type since the different types are handled differently...
        public string type = "";
    }

    [Serializable]
    public class Data
    {
        public string key = "";
        public string type = "";

        // NOT IN ARLEM SPEC
        public string unit = ""; // For defining variable unit e.g. %

        // NOT IN ARLEM SPEC. For displaying value name in UI.
        public string name = "";

        // NOT IN ARLEM SPEC. For defining limits.
        public string normal = "";
        public string green = "";
        public string yellow = "";
        public string red = "";
        public string disabled = "";

        // NOT IN ARLEM SPEC. Just to make the MVC model work...
        public string value = "";

        // NOT IN ARLEM SPEC. We need to define range for the dynamic UI elements.
        public string range = "";
    }

    [Serializable]
    public class Device
    {
        public string id = "";
        public string type = "";
        public string name = "";
        public string owner = "";
    }

    [Serializable]
    public class App
    {
        public string type = "";
        public string id = "";
        public string name = "";
        public string manifest = "";
    }

    [Serializable]
    public class Detectable
    {
        public string id = "";
        public string sensor = "";
        public string type = "";
        public string url = "";

        // NOT IN ARLEM SPEC. For defining a zero point offset from the marker default origin.
        public string origin_position = ""; // Vector 3 format 0, 0, 0
        public string origin_rotation = ""; // Vector 3 format 0, 0, 0
    }

    [Serializable]
    public class Primitive
    {
        public string id = "";
        public string type = "";
        public string symbol = "";
        public float size = 0f;
    }
}
