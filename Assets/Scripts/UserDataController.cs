using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class UserDataController : MonoBehaviour
{
    private void Awake()
    {
        // 최초 유저데이터 스태틱 메모리 생성
        if(UserData.instance == null)
        {
            UserData UserData = new UserData();
        }

        if (SceneManager.GetActiveScene().ToString().ToLower().Contains("linesmash"))
        {
            Debug.Log("자동 세이브로드 안함 : 메인게임이기때문에");
            return;
        }

        /*
        if (isFileExist() == false)
        {
            Debug.LogWarning("파일 없음");
        }*/

        Load();
    }
  
    // 스크립트데이터를 직렬화하여 파일에 저장
    public void Save()
    {
#if UNITY_WINDOW

        string fileName = "UserDataFile.json";
        fileName = Path.Combine(Application.streamingAssetsPath, fileName);

        string jsonData = JsonUtility.ToJson(UserData.instance);
        File.WriteAllText(fileName, jsonData);

        Debug.LogWarning(jsonData);

        //jsonData = JsonConvert.SerializeObject(UserData.instance, Formatting.Indented);

#elif UNITY_ANDROID

        /* 방법1 : Json, persistentDataPath
        string fileName = "UserDataFile.json";
        fileName = Path.Combine(Application.persistentDataPath, fileName);

        string jsonData = JsonUtility.ToJson(UserData.instance, true);
        File.WriteAllText(fileName, jsonData);

        Debug.LogWarning("File Exist : " + File.Exists(fileName));
        Debug.LogWarning("SaveFile Name : " + fileName);
        Debug.LogWarning("Save Data : " + jsonData);
        */

        /* 방법2 : Json, streamingAssetsPath
        string fileName = "UserDataFile.json";
        fileName = Path.Combine(Application.streamingAssetsPath, fileName);

        string jsonData = JsonUtility.ToJson(UserData.instance, true);
        File.WriteAllText(fileName, jsonData);

        Debug.LogWarning("File Exist : " + File.Exists(fileName));
        Debug.LogWarning("SaveFile Name : " + fileName);
        Debug.LogWarning("Save Data : " + jsonData);
        */

        // 방법3 Stream, BinaryFormatter
        string fileName = "UserDataFile.dat";
        fileName = Path.Combine(Application.persistentDataPath, fileName);

        Stream stream;

        if (File.Exists(fileName))
        {
            Debug.LogWarning("====== Save File Extists");
            stream = new FileStream(fileName, FileMode.Open);
        }
        else
        {
            Debug.LogWarning("====== Save File Not Extists");
            stream = new FileStream(fileName, FileMode.OpenOrCreate);
        }

        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, UserData.instance);

        Debug.LogWarning("Save Data UserName : " + UserData.instance.PlayerName);

        stream.Close();
        
#endif
    }

    // 파일 데이터를 역직렬화하여 스크립트에 저장
    public void Load()
    {
#if UNITY_WINDOW

        string fileName = "UserDataFile.json";
        fileName = Path.Combine(Application.streamingAssetsPath, fileName);

        WWW www = new WWW(fileName);
        while (!www.isDone) { }
        string jsonData = www.text;

        Debug.LogWarning(jsonData);

        UserData temp;
        temp = JsonUtility.FromJson<UserData>(jsonData);


        /*
        dataAsJson = File.ReadAllText(fileName);

        temp = JsonConvert.DeserializeObject<UserData>(dataAsJson);*/

#elif UNITY_ANDROID

        /* 방법1 : WWW, Json, persistentDataPath
        string fileName = "UserDataFile.json";
        fileName = Path.Combine(Application.persistentDataPath, fileName);

        WWW www = new WWW(fileName);
        while (!www.isDone) { }

        Debug.LogWarning(www.error);
        string jsonData = www.text;

        UserData temp;
        temp = JsonUtility.FromJson<UserData>(jsonData);

        Debug.LogWarning("File Exist : " + File.Exists(fileName));
        Debug.LogWarning("LoadFile Name : " + fileName);
        Debug.LogWarning("Load Data : " + jsonData);
        */

        /* 방법2 : Json, streamingAssetsPath
        string fileName = "UserDataFile.json";
        fileName = Path.Combine(Application.streamingAssetsPath, fileName);

        string jsonData = File.ReadAllText(fileName);
        UserData temp = JsonUtility.FromJson<UserData>(jsonData);

        Debug.LogWarning("File Exist : " + File.Exists(fileName));
        Debug.LogWarning("LoadFile Name : " + fileName);
        Debug.LogWarning("Load Data : " + jsonData);
        */

        // 방법3 : Stream, BinaryFormatter
        string fileName = "UserDataFile.dat";
        fileName = Path.Combine(Application.persistentDataPath, fileName);
        Stream stream;

        if (File.Exists(fileName))
        {
            Debug.LogWarning("====== Load File Extists");
            stream = new FileStream(fileName, FileMode.Open);

            BinaryFormatter formatter = new BinaryFormatter();

            UserData temp = formatter.Deserialize(stream) as UserData;

            stream.Close();

            Debug.LogWarning("temp PlayerName : " + temp.PlayerName);

            if (string.IsNullOrEmpty(temp.PlayerName) || temp.PlayerName == "new player")
            ResetProfile();
            else
            SetLoadData(temp);

            Debug.LogWarning("Load Data UserName : " + UserData.instance.PlayerName);
        }
        else
        {
            Debug.LogWarning("====== Load File Not Extists");
        }

#endif
        /* 방법1,2
        if (string.IsNullOrEmpty(jsonData))
        {
            ResetProfile();
        }
        else
        {
            SetLoadData(temp);
        }  
        */
    }

    // 임시로 프로필 세팅
    public void ResetProfile()
    {
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        UserData.instance.PlayerName = "new player";
        UserData.instance.Level = 1;
        UserData.instance.Exp = 0;
        UserData.instance.EquippedItemCode = 0;
        UserData.instance.ProfileImageCode = 0;

        UserData.instance.EffectVolume = 0.5f;
        UserData.instance.BGMVolume = 0.5f;
    }

    private void SetLoadData(UserData temp)
    {
        UserData.instance.PlayerName = temp.PlayerName;
        UserData.instance.Level = temp.Level;
        UserData.instance.Exp = temp.Exp;
        UserData.instance.EquippedItemCode = temp.EquippedItemCode;
        UserData.instance.ProfileImageCode = temp.ProfileImageCode;

        UserData.instance.EffectVolume = temp.EffectVolume;
        UserData.instance.BGMVolume = temp.BGMVolume;
    }

    public bool isFileExist()
    {
        string fileName = "UserDataFile.json";

        fileName = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(fileName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void DebugUserData()
    {
        Debug.LogWarning($"PlayerName : {UserData.instance.PlayerName}\nLevel : {UserData.instance.Level}\nExp : {UserData.instance.Exp}\n "+
            $"EquippedItemCode : {UserData.instance.EquippedItemCode}\nEffectVolume : {UserData.instance.EffectVolume}\nBGMVolume : {UserData.instance.BGMVolume}");
    }
}
