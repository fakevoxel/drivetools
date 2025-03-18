using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

// script for managing assets, for now just images used in image display nodes
public class AssetManager : MonoBehaviour
{
    private static AssetManager _instance;

    public static AssetManager Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("Duplicate UIManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake() {
        Instance = this;

        for (int i = 0; i < defaultAssets.Length; i++) {
            SaveUtils.SavePNG(defaultAssets[i]);
        }
    }

    void Start() {
        GameObject.Find("woah").GetComponent<RawImage>().texture = SaveUtils.LoadPNG(
            SaveUtils.saveDirectory + "/" + "v" + SaveUtils.buildVersion + "/default assets/icon_false");
    }

    public Texture2D[] defaultAssets;
    public List<Texture2D> imageAssets;
    public UI_InputList assetList;

    public void PopulateAssets() {
        List<string> tempDirectories = new List<string>();

        for (int i = 0; i < AppData.Instance.imageAssetDirectories.Count; i++) {
            tempDirectories.Add(AppData.Instance.imageAssetDirectories[i]);
        }
        AppData.Instance.imageAssetDirectories.Clear();

        for (int i = 0; i < tempDirectories.Count; i++) {
            // then the UI part
            assetList.AddNewElement();
            UIManager.Instance.PassLoadedFile(tempDirectories[i]);
        }
    }

    public void AddImageAsset(TMP_InputField input) {
        Texture2D newAsset = SaveUtils.LoadPNG(input.text);
        newAsset.name = FileUtils.GetFileName(input.text);

        imageAssets.Add(newAsset);
        AppData.Instance.imageAssetDirectories.Add(input.text);
    }
    public void AddImageAsset(string input) {
        Texture2D newAsset = SaveUtils.LoadPNG(input);
        newAsset.name = FileUtils.GetFileName(input);

        imageAssets.Add(newAsset);
        AppData.Instance.imageAssetDirectories.Add(input);
    }

    public Texture2D GetImageOfName(string name) {
        for (int i = 0; i < imageAssets.Count; i++) {
            if (imageAssets[i].name == name) {
                return imageAssets[i];
            }
        }

        return null;
    }
}