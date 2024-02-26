using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class MovementManager : MonoBehaviour
    {
        [SerializeField] private Toggle pathLoop;
        [SerializeField] private Toggle followPlayer;
        [SerializeField] private Text saveWarningText;
        [SerializeField] private Button assignPictureButton;
        [SerializeField] private Button resetPictureButton;
        [SerializeField] private Toggle animationLoop;


        private void Start()
        {
            followPlayer.onValueChanged.AddListener(delegate { InteractManagerOnFollowPlayer(); });
            pathLoop.onValueChanged.AddListener(delegate { InteractManagerOnPathLoop(); });
        }

        private void InteractManagerOnFollowPlayer()
        {
            pathLoop.interactable = !followPlayer.isOn;
            saveWarningText.enabled = followPlayer.isOn;
        }


        private void InteractManagerOnPathLoop()
        {
            assignPictureButton.interactable = !pathLoop.isOn;
            resetPictureButton.interactable = !pathLoop.isOn;
            animationLoop.interactable = !pathLoop.isOn;
        }


        public Toggle PathLoop
        {
            get { return pathLoop; }
        }

        public Toggle FollowPlayer
        {
            get { return followPlayer; }
        }
    }
}

