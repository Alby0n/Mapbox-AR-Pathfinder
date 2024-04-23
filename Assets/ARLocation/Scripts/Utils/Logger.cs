using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace ARLocation.Utils
{
    public static class Logger
    {
        public static void Log(string msg)
        {
            Debug.Log(msg);
        }

        public static void Warn(string msg)
        {
            Debug.LogWarning(msg);
        }

        public static void Error(string msg)
        {
            Debug.LogError(msg);
        }

        public static void LogFromMethod(string className, string methodName, string message)
        {
            Log("[AR+GPS][" + className + "#" + methodName + "]: " + message);
        }

        public static void LogFromMethod(string className, string methodName, string message, bool output)
        {
            if (!output) return;

            LogFromMethod(className, methodName, message);
        }

        public static void LogFromMethod(string className, string methodName, Transform transform, string prefix = "")
        {
            var message = transform.name + " - position = " + transform.position + ", localPosition = " + transform.localPosition + ", hasChanged = " + transform.hasChanged;
            Log("[AR+GPS][" + className + "#" + methodName + "]: " + prefix + " - " + message);
        }

        public static void LogFromMethod(string className, string methodName, Transform[] transform, string prefix = "")
        {
            var i = 0;
            foreach (var transform1 in transform)
            {
                i++;
                LogFromMethod(className, methodName, transform1, prefix + " (" + i + ")");
            }
        }

        public static void WarnFromMethod(string className, string methodName, string message)
        {
            Warn("[AR+GPS][" + className + "#" + methodName + "]: " + message);
        }

        public static void ErrorFromMethod(string className, string methodName, string message)
        {
            Error("[AR+GPS][" + className + "#" + methodName + "]: " + message);
        }
    }
}
