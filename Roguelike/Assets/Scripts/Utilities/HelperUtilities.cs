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


    //Returns true (error) if the list is empty or contains null value check 
    public static bool ValidateCheckEnumerableValues(Object thisObj, string fieldName, IEnumerable enumerableObjToCheck)
    {
        bool error = false;
        int count = 0;

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
}
