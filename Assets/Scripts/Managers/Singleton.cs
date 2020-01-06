using UnityEngine;

public class Singleton <T> : MonoBehaviour where T: MonoBehaviour
{
    public static T _Inst { get; private set; }

    protected virtual void Awake()
    {
        if (_Inst == null)
            _Inst = GetComponent<T>();
        else
            Destroy(gameObject);
    }
}
