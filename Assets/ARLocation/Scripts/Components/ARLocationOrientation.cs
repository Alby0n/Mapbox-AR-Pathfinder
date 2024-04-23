using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// ReSharper disable UnusedMember.Global

namespace ARLocation
{
    using Utils;


    /// <summary>
    /// This component should be placed on the "ARLocationRoot" GameObject (which should be a child of the
    /// "AR Session Origin") for correctly aligning the coordinate system to the north/east geographical lines.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#arlocationorientation")]
    public class ARLocationOrientation : Singleton<ARLocationOrientation>
    {
        [Serializable]
        public class OnBeforeOrientationUpdatedEvent : UnityEvent<float> {}

        [Header("Update Settings")]

        [Tooltip("The maximum number of orientation updates. The updates will be paused after this amount. Zero means there is no limit and " +
        "the updates won't be paused automatically.")]
        public uint MaxNumberOfUpdates = 4;


        /// <summary>
        /// Only update after measuring the heading N times, and take the average.
        /// </summary>
        [Tooltip("Only update after measuring the heading N times, and take the average."), Range(1, 500)]
        [Header("Averaging")]
        public int AverageCount = 150;

        /// <summary>
        /// If set to true, use raw heading values until measuring the first average.
        /// </summary>
        [Tooltip("If set to true, use raw heading values until measuring the first average.")]
        public bool UseRawUntilFirstAverage = true;

        /// <summary>
        /// The smoothing factor. Zero means disabled. Values around 100 seem to give good results.
        /// </summary>
        [Tooltip("The smoothing factor. Zero means disabled.")]
        [Header("Smoothing")]
        [Range(0.0f, 1.0f)]
        public float MovementSmoothingFactor = 0.015f;

        /// <summary>
        /// A custom offset to the device-calculated true north direction.
        /// </summary>
        [Tooltip("A custom offset to the device-calculated true north direction. When set to a value other than zero, the device's true north will be ignored, and replaced by the " +
                 "magnetic heading added to this offset.")]
        [Header("Calibration")]
        public float TrueNorthOffset;


        [Tooltip("If true, apply a tilt-compensation algorithm on Android devices. Only disable this if you run into any issues.")]
        public bool ApplyCompassTiltCompensationOnAndroid = true;

        [Tooltip("This is the low pass filter factor applied to the heading values to reduce jitter. A zero value disables the low-pass filter, while a value" +
                 " of 1 will make the filter block all value changes. Not applied on iOS, only on Android when the tilt-compensation is enabled.")]
        [Range(0, 1)]
        public double LowPassFilterFactor = 0.9;

        [Header("Events")]
        [Tooltip("Called after the orientation has been updated.")]
        public UnityEvent OnOrientationUpdated = new UnityEvent();

        [Tooltip("Called just before the orientation has been updated.")]
        public OnBeforeOrientationUpdatedEvent OnBeforeOrientationUpdated = new OnBeforeOrientationUpdatedEvent();

        ARLocationProvider locationProvider;

        private int updateCounter;
        private List<float> values = new List<float>();
        private bool isFirstAverage = true;
        private float targetAngle;
        private bool isChangingOrientation;
        private Transform mainCameraTransform;
        private bool waitingForARTracking;

        /// <summary>
        /// Restarts the orientation tracking.
        /// </summary>
        public void Restart()
        {
            isFirstAverage = true;
            updateCounter = 0;
            values = new List<float>();
            targetAngle = 0;
            isChangingOrientation = false;

            targetAngle = mainCameraTransform ?
                mainCameraTransform.rotation.eulerAngles.y : 0;
        }

        // Use this for initialization
        void Start()
        {
            // Look for the LocationProvider
            locationProvider = ARLocationProvider.Instance;

            mainCameraTransform = ARLocationManager.Instance.MainCamera.transform;

            targetAngle = mainCameraTransform.rotation.eulerAngles.y;

            if (LowPassFilterFactor > 0)
            {
                locationProvider.Provider.SetCompassLowPassFactor(LowPassFilterFactor);
            }

            locationProvider.Provider.ApplyCompassTiltCompensationOnAndroid = ApplyCompassTiltCompensationOnAndroid;

            if (ARLocationManager.Instance.WaitForARTrackingToStart)
            {
                waitingForARTracking = true;
                ARLocationManager.Instance.OnARTrackingStarted(() =>
                {
                    waitingForARTracking = false;
                });
            }

            // Register compass update delegate
            locationProvider.OnCompassUpdatedEvent(OnCompassUpdatedHandler);
        }

        private void OnCompassUpdatedHandler(HeadingReading newHeading, HeadingReading lastReading)
        {
            if (waitingForARTracking) return;

            if (!newHeading.isMagneticHeadingAvailable)
            {
                Debug.LogWarning("[AR+GPS][ARLocationOrientation]: Magnetic heading data not available.");
                return;
            }

            if (MaxNumberOfUpdates > 0 && updateCounter >= MaxNumberOfUpdates)
            {
                return;
            }

            var trueHeading = (Mathf.Abs(TrueNorthOffset) > 0.000001f) ? newHeading.magneticHeading + TrueNorthOffset : newHeading.heading;


            float currentCameraHeading = mainCameraTransform.rotation.eulerAngles.y;
            float value = Misc.GetNormalizedDegrees(currentCameraHeading - ((float)trueHeading));

            if (Mathf.Abs(value) < 0.0000001f)
            {
                return;
            }

            // If averaging is not enabled
            if (AverageCount <= 1)
            {
                if (updateCounter == 0)
                {
                    transform.localRotation = Quaternion.AngleAxis(value, Vector3.up);
                    TrySetOrientation(value, true);
                }
                else
                {
                    TrySetOrientation(value);
                }

                return;
            }

            values.Add(value);

            if (updateCounter == 0 && values.Count == 1)
            {
                TrySetOrientation(value, true);
                return;
            }


            if (isFirstAverage && UseRawUntilFirstAverage)
            {
                TrySetOrientation(value, true);
                return;
            }

            if (values.Count >= AverageCount)
            {
                if (isFirstAverage)
                {
                    isFirstAverage = false;
                }

                var average = Misc.FloatListAverage(values);
                values.Clear();

                TrySetOrientation(average);
            }
        }

        private void TrySetOrientation(float angle, bool isFirstUpdate = false)
        {
            if (isFirstUpdate)
            {
                targetAngle = angle;

                OnBeforeOrientationUpdated?.Invoke(targetAngle);
                transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
                OnOrientationUpdated?.Invoke();

                updateCounter++;
                return;
            }

            if (MaxNumberOfUpdates > 0 && updateCounter >= MaxNumberOfUpdates)
            {
                return;
            }

            targetAngle = angle;
            OnBeforeOrientationUpdated?.Invoke(targetAngle);
            isChangingOrientation = true;
            updateCounter++;
        }

        private void Update()
        {
            if (locationProvider.Provider == null || !locationProvider.Provider.IsCompassEnabled)
            {
                return;
            }

            if (Mathf.Abs(transform.rotation.eulerAngles.y - targetAngle) <= 0.001f)
            {
                if (isChangingOrientation)
                {
                    isChangingOrientation = false;
                    OnOrientationUpdated?.Invoke();
                }
                return;
            }

            var t = 1.0f - Mathf.Pow(MovementSmoothingFactor, Time.deltaTime);
            var value = Mathf.LerpAngle(transform.rotation.eulerAngles.y, targetAngle, t);

            transform.localRotation = Quaternion.AngleAxis(value, Vector3.up);
        }

        private void OnDestroy()
        {
            locationProvider.OnCompassUpdateDelegate -= OnCompassUpdatedHandler;
        }

    }
}
