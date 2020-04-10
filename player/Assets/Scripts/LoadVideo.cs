using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class LoadVideo : MonoBehaviour
{
    public InputField FileName;
    public Button LoadButton;
    public Video Video;

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

        var file = FileName.text;
        Persisted.LastVideoFile = file;
        Video.Open(file);
    }
}
