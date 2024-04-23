using UnityEngine;
using System.Collections.Generic;
using System;

namespace ARLocation.MapboxRoutes
{
    using Vendor.SimpleJSON;

    [CreateAssetMenu(fileName = "ARLocationConfig", menuName = "AR+GPS/Route")]
    public class Route : ScriptableObject
    {
        [Serializable]
        public class Geometry
        {
            public List<Location> coordinates = new List<Location>();
            public string type;

            public static Geometry Parse(JSONNode node)
            {
                var result = new Geometry();

                var coordinatesNode = node["coordinates"];

                if (coordinatesNode == null)
                {
                    throw new System.NullReferenceException("No 'coordinates' field!");
                }

                var coordinatesArray = coordinatesNode.AsArray;

                if (coordinatesArray == null)
                {
                    throw new System.NullReferenceException("No 'coordinates' field!");
                }

                if (coordinatesArray.Count == 2 && coordinatesArray[0].AsArray == null)
                {
                    var x = coordinatesArray[0].AsFloat;
                    var y = coordinatesArray[1].AsFloat;
                    result.coordinates.Add(new Location(y, x, 0));
                }
                else
                {
                    for (int i = 0; i < coordinatesArray.Count; i++)
                    {
                        var x = coordinatesArray[i][0].AsFloat;
                        var y = coordinatesArray[i][1].AsFloat;

                        result.coordinates.Add(new Location(y, x, 0));
                    }
                }

                result.type = node["type"];

                return result;
            }

            public override string ToString()
            {
                var result = "";

                foreach (var c in coordinates)
                {
                    result += $"({c.Latitude}, {c.Longitude}, {c.Altitude})" + ",";
                }

                result += $"[{type}]";

                return result;
            }
        }

        [Serializable]
        public class RouteLeg
        {
            public float distance;
            public List<Step> steps;

            public override string ToString()
            {
                string result = "";

                result += $"RouteLeg{{ distance = {distance}, steps = [";

                foreach (var s in steps)
                {
                    result += s + ",";
                }

                result += "]}";

                return result;
            }

            public static RouteLeg Parse(JSONNode node)
            {
                var result = new RouteLeg();

                result.distance = node["distance"].AsFloat;
                result.steps = new List<Step>();

                var steps = node["steps"].AsArray;

                for (int i = 0; i < steps.Count; i++)
                {
                    result.steps.Add(Step.Parse(steps[i]));
                }

                return result;
            }
        }

        [Serializable]
        public class Step
        {
            public float distance;
            public Geometry geometry;
            public Maneuver maneuver;
            public string name;

            public override string ToString()
            {
                return $"Step{{ distance = {distance}, geometry = {geometry}, maneuver = {maneuver}, name = {name} }}";
            }

            public static Step Parse(JSONNode node)
            {
                var result = new Step();

                result.distance = node["distance"].AsFloat;
                result.geometry = Geometry.Parse(node["geometry"]);
                result.maneuver = Maneuver.Parse(node["maneuver"]);
                result.name = node["name"];

                return result;
            }
        }

        [Serializable]
        public class Maneuver
        {
            public int bearing_before;
            public int bearing_after;
            public string instruction;
            public Location location;
            public string type;

            public override string ToString()
            {
                string result = "";

                result += $"Maneuver{{ bearing_before = {bearing_before}, bearing_after = {bearing_after}, instruction = {instruction}, location = {location}, type = {type} }}";

                return result;
            }

            public static Maneuver Parse(JSONNode node)
            {
                var result = new Maneuver();

                result.bearing_before = node["bearing_before"].AsInt;
                result.bearing_after = node["bearing_after"].AsInt;
                result.instruction = node["instruction"];
                result.type = node["type"];
                var loc = node["location"].AsArray;
                result.location = new Location(loc[1].AsDouble, loc[0].AsDouble, 0);

                return result;
            }
        }

        public float distance;
        public Geometry geometry;
        public List<RouteLeg> legs;

        public static Route Parse(JSONNode node)
        {
            var result = ScriptableObject.CreateInstance<Route>(); //Route();

            result.distance = node["distance"].AsFloat;
            result.geometry = Geometry.Parse(node["geometry"]);
            result.legs = new List<RouteLeg>();

            var legs = node["legs"].AsArray;

            for (int i = 0; i < legs.Count; i++)
            {
                result.legs.Add(RouteLeg.Parse(legs[i]));
            }

            return result;
        }

        public override string ToString()
        {
            string result = "";

            result += $"Route{{ distance = {distance}, geometry = {geometry}, legs = [";

            foreach (var leg in legs)
            {
                result += leg;
            }

            result += "]}";

            return result;
        }
    }
}
