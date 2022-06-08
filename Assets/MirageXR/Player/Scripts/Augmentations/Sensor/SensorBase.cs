using UnityEngine;

namespace MirageXR
{

    public class SensorBase : MonoBehaviour  // TODO: now is useless
    {
        // streaming data: list of registered callback listeners (registered to get an update for each data frame captured)
        public delegate void CallbackStack(SensorDataFrame currentFrame);
        public CallbackStack OnSensorStreamUpdate;
        [SerializeField] private float updateInterval = 0.04f; // 25x per second
        [SerializeField] private float updateInitialDelay = 0.3f; // 300ms delay

        // current frame with the sensor data
        public SensorDataFrame currentSensorDataFrame { get; set; }

        // boolean: recording or not?
        [SerializeField] private bool recording = false;
        [SerializeField] private bool connected = false;

        // Use this for initialization
        private void Start()
        {
            WarmUp();
        }

        /// <summary>
        /// FixedFrameUpdate is used (instead of Update) to set
        /// the sensor update frame rate to the desired interval.
        /// The routine is registered in Start(). The public float 
        /// updateInterval can be overridden in order to change the frequency.
        /// </summary>
        public virtual void FixedFrameUpdate()
        {
            // notify all registered callbacks with the new data
            if (OnSensorStreamUpdate != null)
            {
                OnSensorStreamUpdate?.Invoke(currentSensorDataFrame);
            }

        } // FixedFrameUpdate


        // WarmUp(): turn on the sensor: true if succesful
        public virtual bool WarmUp()
        {
            connected = true;

            // Register FixedFrameUpdate in updateInitialDelay sec, then every updateInterval secs
            InvokeRepeating(nameof(FixedFrameUpdate), updateInitialDelay, updateInterval);

            return (true);
        }

        // StartCapture: true if successful
        public virtual bool StartCapture()
        {
            recording = true;
            return (true);
        }

        // StopCapture: true if successful
        public virtual bool StopCapture()
        {
            recording = false;
            return (true);
        }

        public virtual void RegisterStreamListener ( CallbackStack fun ) 
        {
           OnSensorStreamUpdate += fun;
        }

        public virtual void UnregisterStreamListener( CallbackStack fun)
        {
            OnSensorStreamUpdate -= fun;
        }


    } // class sensorBase

} // namespace