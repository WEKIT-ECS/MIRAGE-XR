using UnityEngine;

namespace Utility.UiKit.Runtime.MVC
{
    public class ViewController<TViewController, TView> : MonoBehaviour
        where TViewController : ViewController<TViewController, TView>
        where TView : View
    {
        protected TView View { get; private set; }

        protected virtual void Awake()
        {
            View = GetComponentInChildren<TView>(true);
            
            if (View == null) return;
            OnInit();
            OnBind();
            
            //View.gameObject.SetActive(true);
        }

        protected void OnDestroy()
        {
            if (View == null) return;
            OnUnbind();
            OnUninit();

            View = null;
        }

        protected virtual void OnInit()
        {
        }
        
        protected virtual void OnBind()
        {
        }

        protected virtual void OnUninit()
        {
        }
        
        protected virtual void OnUnbind()
        {
        }

        protected virtual void OnViewActivated()
        {
        }

        protected virtual void OnViewDeactivated()
        {
        }

        public void SetViewVisible(bool value)
        {
            if (View == null) return;
            
            View.gameObject.SetActive(value);

            if (value)
            {
                OnViewActivated();
            }
            else
            {
                OnViewDeactivated();
            }
            
            if (value)
            {
                View.Show();
            }
            else
            {
                View.Hide();
            }            
        }
    }
}