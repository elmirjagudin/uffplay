using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class LoadVideo : MonoBehaviour
{
    public InputField FileName;
    public Button LoadButton;

    void Start()
    {
        FileName.text = Persisted.LastVideoFile;
    }

    public void InputChanged(string fileName)
    {
        LoadButton.interactable = File.Exists(fileName);
    }

    public void Load()
    {
        gameObject.SetActive(false);
        Persisted.LastVideoFile = FileName.text;
        Log.Msg("Load '{0}'", FileName.text);
    }
}
