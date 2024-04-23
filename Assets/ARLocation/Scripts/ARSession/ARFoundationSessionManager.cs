
#if !ARGPS_USE_VUFORIA
using UnityEngine.XR.ARFoundation;
using System;
using ARLocation.Utils;

#endif

#if !ARGPS_USE_VUFORIA
public static class ARSessionStateExtensions
{
    public static string ToInfoString(this ARSessionState state)
    {
        switch (state)
        {
            case ARSessionState.None:
                return "None";
            case ARSessionState.Unsupported:
                return "Unsupported";
            case ARSessionState.CheckingAvailability:
                return "CheckingAvailability";
            case ARSessionState.NeedsInstall:
                return "NeedsInstall";
            case ARSessionState.Installing:
                return "Installing";
            case ARSessionState.Ready:
                return "Ready";
            case ARSessionState.SessionInitializing:
                return "SessionInitializing";
            case ARSessionState.SessionTracking:
                return "SessionTracking";
            default:
                return "None";
        }
    }
}
#endif

namespace ARLocation.Session
{
#if !ARGPS_USE_VUFORIA
    public class ARFoundationSessionManager : IARSessionManager
    {
        private readonly ARSession arSession;
        private Action onAfterReset;
        private string infoString;
        private bool trackingStarted;
        private Action trackingStartedCallback;
        private Action trackingRestoredCallback;
        private Action trackingLostCallback;
        private ARSessionState currentStatus;

        public bool DebugMode { get; set; }

        public ARFoundationSessionManager(ARSession session)
        {
            arSession = session;
            ARSession.stateChanged += ARSessionOnStateChanged;
        }

        private void ARSessionOnStateChanged(ARSessionStateChangedEventArgs args)
        {
            infoString = args.state.ToInfoString();

            Logger.LogFromMethod("ARFoundationSessionManager", "ARSessionOnStateChanged", infoString, DebugMode);

            if (args.state == ARSessionState.SessionTracking)
            {
                if (!trackingStarted)
                {
                    trackingStarted = true;
                    Logger.LogFromMethod("ARFoundationSessionManager", "ARSessionOnStateChanged", "Tracking Started!.", DebugMode);
                    trackingStartedCallback?.Invoke();
                }
                else if (currentStatus != ARSessionState.SessionTracking )
                {
                    Logger.LogFromMethod("ARFoundationSessionManager", "ARSessionOnStateChanged", "Tracking Restored!", DebugMode);
                    trackingRestoredCallback?.Invoke();
                }

                if (onAfterReset != null)
                {
                    Logger.LogFromMethod("ARFoundationSessionManager", "ARSessionOnStateChanged", "Emitting 'OnAfterReset' event.", DebugMode);
                    onAfterReset.Invoke();
                    onAfterReset = null;
                }
            }
            else if (currentStatus == ARSessionState.SessionTracking)
            {
                Logger.LogFromMethod("ARFoundationSessionManager", "ARSessionOnStateChanged", "Tracking Lost!", DebugMode);
                trackingLostCallback?.Invoke();
            }

            currentStatus = args.state;
        }

        public void Reset(Action callback)
        {
            arSession.Reset();
            onAfterReset += callback;
        }

        public string GetSessionInfoString()
        {
            return infoString;
        }

        public string GetProviderString()
        {
            return "ARFoundation";
        }

        public void OnARTrackingStarted(Action callback)
        {
            if (trackingStarted)
            {
                callback.Invoke();
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
#else
    public class ARFoundationSessionManager {}
#endif
}
