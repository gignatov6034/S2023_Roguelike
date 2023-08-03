using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities 
{
    public static bool ValidateCheckEmptyString(Object thisObj, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " is empty and must contain a value in object " + thisObj.name.ToString());
            return true;
        }
        return false;
    }

    //Null value debug check 
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, UnityEngine.Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log(fieldName + " is null and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }


    //Returns true (error) if the list is empty or contains null value check 
    public static bool ValidateCheckEnumerableValues(Object thisObj, string fieldName, IEnumerable enumerableObjToCheck)
    {
        bool error = false;
        int count = 0;


        //return if null
        if (enumerableObjToCheck == null)
        {
            return true;
        }

        foreach (var item in enumerableObjToCheck)
        {
            //Check for any nukll value items
            if (item == null)
            {
                Debug.Log(fieldName + " has null values in object " + thisObj.name.ToString());
                error = true;
            }
            else        
                count++;
        }

        //if no items 
        if (count == 0)
        {
            Debug.Log(fieldName + " has no values in object " + thisObj.name.ToString());
            error = true;
        }

        return error;
    }

    //Positive value debug check - if zero is allowed set isZeroAllowed to true. Returns true if theere is an error
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, int valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else 
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }
}
