using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEditor;

public class DialogueController : MonoBehaviour
{
    struct DialogueInfo
    {
        public string characterName;
        public string[] dialogueLines;
    }

    [SerializeField] private TextMeshProUGUI DialogueBox;
    [SerializeField] private float textResetSpeed = 0.2f;

    private string stringDisplay = "";
    private string stringQueue = "";
    private float textResetTime = 0.0f;
    private List<DialogueInfo> dialogueInfo = new List<DialogueInfo>();
    private int alienCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/DialogueJsons/AlienDialogue");
        FileInfo[] info = dir.GetFiles("*.json");
        Debug.Log("Info len: " + info.Length);
        foreach (var diagInfo in info)
        {
            TextAsset diagAsset = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/DialogueJsons/AlienDialogue/" + diagInfo.Name, typeof(TextAsset));
            Debug.Log("Assets/DialogueJsons/AlienDialogue" + diagInfo.Name);
            dialogueInfo.Add(CreateFromJson(diagAsset.ToString()));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (stringDisplay != stringQueue)
        {
            if (textResetTime <= 0.0f && stringQueue.Length > 0)
            {
                textResetTime = textResetSpeed;
                char letter = stringQueue[0];
                stringDisplay += letter;
                stringQueue = stringQueue.Substring(1);
                DialogueBox.SetText(stringDisplay);
            }
            textResetTime -= Time.deltaTime;
        }
    }

    public void SetText(string textIn)
    {
        stringDisplay = "";
        stringQueue = textIn;
    }

    public string[] RequestLines()
    {
        string[] lines;
        if (alienCounter < dialogueInfo.Count) { lines = dialogueInfo[alienCounter].dialogueLines; }
        else { lines = new string[0]; lines[0] = "Error: Ran out of dialogue options"; }
        alienCounter++;

        return lines;
    }

    private static DialogueInfo CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<DialogueInfo>(jsonString);
    }
}
