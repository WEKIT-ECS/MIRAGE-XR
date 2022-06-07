using UnityEngine;

public class GuestEnter : MonoBehaviour
{
    [SerializeField] private GameObject enterPrefab;
    [SerializeField] private GameObject loginPrefab;

    [SerializeField] private GameObject _registerPrefab;

    public void OnClickLogin()
    {
        if (loginPrefab != null)
        {
            Instantiate(loginPrefab);
            Destroy(enterPrefab);
        }
    }
    
    public void OnClickGuest()
    {
        Destroy(enterPrefab);
    }

    public void OnClickRegister()
    {
        Instantiate(_registerPrefab);
    }
    
}
