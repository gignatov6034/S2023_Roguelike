using UnityEngine;

//Singleton - creational design pattern 
//-----> sure a class only has one instance, and provide
//       a global point of access to it
//
//Pass another class T that inherits from MonoBehaviour as well
//
//ATTENTION:
//If change scenes the singleton gets destroyed 
//-----> add DontDestroyOnLoad(gameObject) at the Awake to prevent that
public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T: MonoBehaviour
{
    static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    //Populates the instance field 
    //Should be accessed in inherited classes 
    //Should be able to be overwriten by inherited classes
    //
    //If more then 1 instance exists - delete all dublicates 
    protected virtual void Awake()
    {
        //If null set the value, destroy otherwise
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
