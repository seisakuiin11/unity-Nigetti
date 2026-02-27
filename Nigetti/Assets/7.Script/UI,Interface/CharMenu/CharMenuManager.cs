using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharMenuManager : MonoBehaviour
{
    [SerializeField] Image char_im;
    [SerializeField] GameObject[] char_btns;
    [SerializeField] TextMeshProUGUI c_name;
    [SerializeField] TextMeshProUGUI c_skillName;
    [SerializeField] TextMeshProUGUI c_description;
    [SerializeField] EventSystem uiSystem;
    [SerializeField] Sprite[] charImages;
    TextAsset csvFile;
    List<string[]> csvData = new List<string[]>(); // CSVファイルの中身を入れるリスト
    int charNum;

    // Start is called before the first frame update
    void Awake()
    {
        csvFile = Resources.Load("キャラクターデータs") as TextAsset; // ResourcesにあるCSVファイルを格納
        StringReader reader = new StringReader(csvFile.text); // TextAssetをStringReaderに変換
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine(); // 1行ずつ読み込む
            csvData.Add(line.Split(',')); // csvDataリストに追加する
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //オープン時に呼ぶ処理
    public void OpenCharMenu(int num)
    {
        SetCharMenu(num);
        uiSystem.SetSelectedGameObject(char_btns[charNum]);
    }
    public void SetCharMenu(int num)
    {
        charNum = num;
        char_im.sprite = charImages[charNum];
        c_name.text = csvData[charNum + 1][0];
        c_skillName.text = csvData[charNum + 1][1];
        c_description.text = csvData[charNum + 1][2];
    }

    //ボタンにセットする処理
    public void SetCharNum(int num)
    {
        charNum = num;
        SetCharMenu(num);
    }
}
