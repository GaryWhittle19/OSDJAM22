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

    public int dialogueInfoCount = 0;

    private string stringDisplay = "";
    private string stringQueue = "";
    private float textResetTime = 0.0f;
    private List<DialogueInfo> dialogueInfo = new List<DialogueInfo>();
    private int alienCounter = 0;

    private void Awake()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/DialogueJsons/AlienDialogue");
        FileInfo[] info = dir.GetFiles("*.json");

        foreach (var diagInfo in info)
        {
            TextAsset diagAsset = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/DialogueJsons/AlienDialogue/" + diagInfo.Name, typeof(TextAsset));
            dialogueInfo.Add(CreateFromJson(diagAsset.ToString()));
        }

        dialogueInfoCount = dialogueInfo.Count;
    }

    // Start is called before the first frame update
    void Start()
    {
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
        Debug.Log("Alien counter now " + alienCounter);

        return lines;
    }

    private static DialogueInfo CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<DialogueInfo>(jsonString);
    }
}
