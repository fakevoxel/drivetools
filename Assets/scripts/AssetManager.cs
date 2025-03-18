using System.Collections.Generic;
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
    }

    // the assets that will get written to the default asset directory, and always loaded in
    // these include icons for boolean nodes, etc.
    public Texture2D[] defaultAssets;
    // list of currently loaded image assets, textures that are loaded by the user go here
    public List<Texture2D> imageAssets;
    // the UI component that lists all of the loaded assets
    public UI_InputList assetList;


    // TODO: just save the default asset directories as normal assets, don't load them separately
    public void PopulateAssets() {
        List<string> tempDirectories = new List<string>();

        for (int i = 0; i < AppData.Instance.imageAssetDirectories.Count; i++) {
            tempDirectories.Add(AppData.Instance.imageAssetDirectories[i]);
        }
        AppData.Instance.imageAssetDirectories.Clear();

        string[] defaultAssetPaths = SaveUtils.GetAllDefaultAssetDirectories();

        for (int i = 0; i < defaultAssetPaths.Length; i++) {
            assetList.AddNewElement();
            UIManager.Instance.PassLoadedFile(defaultAssetPaths[i]);
        }

        // for (int i = 0; i < tempDirectories.Count; i++) {
        //     // then the UI part
        //     assetList.AddNewElement();
        //     UIManager.Instance.PassLoadedFile(tempDirectories[i]);
        // }
    }

    // given an input field, presumably with the file path, load and add a new image asset
    // TODO: dealing with invalid paths?
    public void AddImageAsset(TMP_InputField input) {
        Texture2D newAsset = SaveUtils.LoadPNG(input.text);
        newAsset.name = FileUtils.GetFileName(input.text);

        imageAssets.Add(newAsset);
        AppData.Instance.imageAssetDirectories.Add(input.text);
    }
    // given the string of the file path, load and add an image asset
    public void AddImageAsset(string input) {
        Texture2D newAsset = SaveUtils.LoadPNG(input);
        newAsset.name = FileUtils.GetFileName(input);

        imageAssets.Add(newAsset);
        AppData.Instance.imageAssetDirectories.Add(input);
    }

    // get the Texture2D of an asset, when provided the asset's name
    // used for imagedisplay nodes, which store the asset name AND NOT THE TEXTURE
    public Texture2D GetImageOfName(string name) {
        for (int i = 0; i < imageAssets.Count; i++) {
            if (imageAssets[i].name == name) {
                return imageAssets[i];
            }
        }

        return null;
    }
}