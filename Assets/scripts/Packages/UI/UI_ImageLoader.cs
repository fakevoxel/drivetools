using UnityEngine.UI;
using UnityEngine;

public class UI_ImageLoader : MonoBehaviour
{
    public string directory;
    public RawImage previewImage;
    private string assetName;

    void Awake() {
        // open the load file window
        UIManager.Instance.LoadFile(Load);

        transform.GetChild(2).GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.ReturnAssetName(assetName)
        );
    }

    public void Load(string path) {
        previewImage.texture = SaveUtils.LoadPNG(path);
        assetName = FileUtils.GetFileName(path);

        AssetManager.Instance.AddImageAsset(path);
    }
}
