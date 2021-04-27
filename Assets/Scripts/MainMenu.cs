using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public GameObject[] Menus; // 0 - main menu, 1 - start new, 2 - settings, 3 - LoadScreen
    public GameObject[] Variant;
    public InputField SeedField, JsonField;
    public Image LoadingImage;
    public Text LoadingText;
    public Dropdown VariantChooser;
    public Slider VolumeSlider;
    public AudioSource ASMain;
    public void ExitApp() { Application.Quit(); }
    public void OpenMenu(int menu) {
        for (int i = 0; i < Menus.Length; ++i) { Menus[i].SetActive(false); }
        Menus[menu].SetActive(true);
    }
    public void OpenVariant() {
        for (int i = 0; i < Variant.Length; ++i) { Variant[i].SetActive(false); }
        Variant[VariantChooser.value].SetActive(true);
        GameData.gamevariant = VariantChooser.value;
    }

    public void StartGame() { OpenMenu(3); StartCoroutine(SceneLoaderAsync(1)); }
    public void UpdateSeed() { GameData.seed = SeedField.text; }
    public void GenerateRandom() {
        const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789+-*/=';,";
        string outp = ""; int charamount = UnityEngine.Random.Range(5, 25);
        for (int i = 0; i < charamount; ++i) { outp += chars[UnityEngine.Random.Range(0, chars.Length)]; }
        SeedField.text = outp;
        GameData.seed = outp;
    }
    public void UpdateSlider() {
        VolumeSlider.value = GameData.SoundLevel;
    }
    public void UpdateSound() {
        GameData.SoundLevel = VolumeSlider.value;
        ASMain.volume = GameData.SoundLevel;
    }
    public void LinkOpener(string LNK) { Application.OpenURL(LNK); }

    public void JSONImport() {
        StreamReader file = new StreamReader("Assets/Saves/" + JsonField.text + ".json");
        string content = file.ReadToEnd();
        Debug.Log(content);
        GameData.JsonInfo = JsonUtility.FromJson<MapJSON>(content);
        file.Close();
    }

    IEnumerator SceneLoaderAsync(int indx) {
        AsyncOperation op = SceneManager.LoadSceneAsync(indx);
        while (!op.isDone) {
            LoadingImage.fillAmount = Mathf.Clamp01(op.progress / 0.9f);
            LoadingText.text = "Загружаем... " + Mathf.CeilToInt(op.progress * 100).ToString() + '%';
            yield return null;
        }
    }
}
public static class GameData {
    public static MapJSON JsonInfo = new MapJSON();
    public static string seed;
    public static int gamevariant = 1;
    public static float SoundLevel = 0.8f;
}

[Serializable]
public class MapJSON {
    public int id;
    public string type;
    public Map map;

    [Serializable]
    public class Map
    {
        public Building[] buildings;
        public Sign[] signs;
        public Road road;
        public int[] trafficLights = new int[0];

        [Serializable]
        public class Building
        {
            public int id;
            public string type;
            public int[] position = new int[2];
            public int[] size = new int[2];
        }
        [Serializable]
        public class Sign
        {
            public string type;
            public int value = -1;
            public int[] position = new int[2];

        }
        [Serializable]
        public class Road
        {
            public Vertex[] vertexes;
            public Edge[] edges;

            [Serializable]
            public class Vertex
            {
                public int id;
                public int[] position = new int[2];
            }
            [Serializable]
            public class Edge
            {
                public int from;
                public int to;
            }
        }
    }
}