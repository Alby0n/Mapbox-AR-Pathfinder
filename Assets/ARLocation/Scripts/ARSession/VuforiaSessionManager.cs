#if ARGPS_USE_VUFORIA
using Logger = ARLocation.Utils.Logger;
using System;
using Vuforia;

namespace ARLocation.Session
{
    public class VuforiaSessionManager : IARSessionManager
    {
        private PositionalDeviceTracker positionalDeviceTracker;
        private string sessionInfoString;
        private bool trackingStarted;
        private Action trackingStartedCallback;
        private Action onAfterReset;
        private Action trackingRestoredCallback;
        private TrackableBehaviour.Status currentStatus = TrackableBehaviour.Status.NO_POSE;
        private Action trackingLostCallback;

        public bool DebugMode { get; set; }

        public VuforiaSessionManager()
        {
            positionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

            if (positionalDeviceTracker == null)
            {
                sessionInfoString = "NO POSITIONAL TRACKER";
            }

            VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
            VuforiaARController.Instance.RegisterOnPauseCallback(OnVuforiaPaused);
            DeviceTrackerARController.Instance.RegisterTrackerStartedCallback(OnTrackerStarted);
            DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
        }

        private void OnDevicePoseStatusChanged(TrackableBehaviour.Status arg1, TrackableBehaviour.StatusInfo arg2)
        {
            sessionInfoString = $"OnDevicePoseStatusChanged: {arg1} - {arg2}";

            Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", sessionInfoString, DebugMode);

            if (arg1 != TrackableBehaviour.Status.NO_POSE)
            {
                if (!trackingStarted)
                {
                    trackingStarted = true;
                    Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Tracking Started!.", DebugMode);
                    trackingStartedCallback?.Invoke();
                }
                else if (currentStatus == TrackableBehaviour.Status.NO_POSE)
                {
                    Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Tracking Restored!", DebugMode);
                    trackingRestoredCallback?.Invoke();
                }

                if (onAfterReset != null)
                {
                    Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Emitting 'OnAfterReset' event.", DebugMode);
                    onAfterReset.Invoke();
                    onAfterReset = null;
                }
            }
            else if (currentStatus != TrackableBehaviour.Status.NO_POSE)
            {
                Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Tracking Lost!", DebugMode);
                trackingLostCallback?.Invoke();
            }

            currentStatus = arg1;
        }

        private void OnTrackerStarted()
        {
            sessionInfoString = $"OnTrackerStarted";
        }

        private void OnVuforiaPaused(bool obj)
        {
            sessionInfoString = $"OnVuforiaPaused";
        }

        private void OnVuforiaStarted()
        {
            sessionInfoString = $"OnVuforiaStarted";
        }


        public void Reset(Action callback)
        {
            positionalDeviceTracker?.Reset();
            onAfterReset += callback;
        }

        public string GetSessionInfoString()
        {
            return sessionInfoString;
        }

        public string GetProviderString()
        {
            return "Vuforia (" + VuforiaUnity.GetVuforiaLibraryVersion() + ")";
        }

        public void OnARTrackingStarted(Action callback)
        {
            if (trackingStarted)
            {
                callback?.Invoke();
                return;
            }

            trackingStartedCallback += callback;
        }

        public void OnARTrackingRestored(Action callback)
        {
            trackingRestoredCallback += callback;
        }

        public void OnARTrackingLost(Action callback)
        {
            trackingLostCallback += callback;
        }
    }
}
#endif
