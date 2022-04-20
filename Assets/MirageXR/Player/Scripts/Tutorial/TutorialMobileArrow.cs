using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// This is a concrete implementation of a TutorialArrow.
    /// It's intended use is in 2D application versions.
    /// </summary>
    public class TutorialMobileArrow : TutorialArrow
    {
        private GameObject target;

        [SerializeField] Button BtnExit;

        public override void PointTo(GameObject target, string instructionText,
            Vector3? positionOffset, Vector3? rotationOffset)
        {
            this.target = target;
            gameObject.transform.SetParent(RootView.Instance.transform);
            if (positionOffset != null)
            {
                gameObject.transform.position = (Vector3)(target.transform.position + positionOffset);
            }
            else
            {
                gameObject.transform.position = target.transform.position;
            }
            //gameObject.transform.rotation = target.transform.rotation;

            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = instructionText;
        }

        public override void Dissapear()
        {
            gameObject.SetActive(false);
        }



        public void Init()
        {
            BtnExit.onClick.AddListener(OnExitButtonClick);
        }

        public void OnExitButtonClick()
        {
            gameObject.SetActive(false);
            TutorialManager.Instance.CloseTutorial();
        }

        /*
        private void Update()
        {
            if (gameObject.activeInHierarchy && target != null)
            {
                transform.position = target.transform.position;
            }
        }
        */

    }
}
