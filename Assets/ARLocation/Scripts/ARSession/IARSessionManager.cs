using System;

namespace ARLocation.Session
{
    public interface IARSessionManager
    {
        bool DebugMode { get; set; }
        void Reset(Action callback);
        string GetSessionInfoString();
        string GetProviderString();
        void OnARTrackingStarted(Action callback);
        void OnARTrackingRestored(Action callback);
        void OnARTrackingLost(Action callback);
    }
}
