using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{

    public class VirtualInstructorRecoder : MonoBehaviour
    {
        [SerializeField] private Button _btnRecord;
        [SerializeField] private Button _btnSendRecord;
        [SerializeField] private GameObject _Record;
        [SerializeField] private GameObject _SendRecord;
        [SerializeField] private int _maxRecordTime;

        void Awake()
        {
            _btnRecord.onClick.AddListener(StartRecording);
            _btnSendRecord.onClick.AddListener(SendRecording);
        }
        

        IEnumerator CountdownCoroutine()
        {
            UnityEngine.Debug.Log("CountdownCoroutine");
            yield return new WaitForSeconds(_maxRecordTime);
            SendRecording();
        }



        private void StartRecording()
        {
            UnityEngine.Debug.Log("StartRecording");
            StartCoroutine(CountdownCoroutine());
            _SendRecord.gameObject.SetActive(true);
            _Record.gameObject.SetActive(false);
        }

        private void SendRecording()
        {
            UnityEngine.Debug.Log("SendRecording");
            _SendRecord.gameObject.SetActive(false);
            _Record.gameObject.SetActive(true);
        }

    }
}
