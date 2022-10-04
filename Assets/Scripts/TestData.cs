using UnityEngine;

[System.Serializable]
public class TestData
{
    public static TestData instance;

    public int testInt = 1;

    public TestData()
    {
        if(instance == null)
        instance = this;
    }
}
