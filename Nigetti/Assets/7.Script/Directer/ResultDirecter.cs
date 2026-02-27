using UnityEngine;
using Fusion;
using ComonData;

public class ResultDirecter : DirecterComon
{
    const int ZITTAI = 1, RETAI = -1;

    [SerializeField] GameObject loadUI;
    [SerializeField] GameObject resultUI;
    [SerializeField] GameObject winnerUI;
    [SerializeField] GameObject loserUI;

    //[Networked] int nextScene { get; set; }

    int next;
    int playNum;
    PlayerController[] player;
    UISoundScript SE;

    private void Awake()
    {
        SE = FindAnyObjectByType<UISoundScript>();
    }

    /* 初期化 スタート処理 ----------------------------------------------------------------------------- */
    public override SceneDataHost InitNetwork(SceneDataHost _data)
    {
        _data.processState = State.UPDATA;
        if (Runner.IsServer)
        {
            player = new PlayerController[2];
            for (int i = 0; i < _data.playerRefs.Count; i++)
            {
                NetworkObject playerObj = Runner.Spawn(_data.prefab, _data.spawnPos[i].position, Quaternion.identity, _data.playerRefs[i]);
                _data._spawnedCharacters.Add(_data.playerRefs[i], playerObj);
                PlayerController data = playerObj.GetComponent<PlayerController>();
                data.Init(Scene.LOBBY, i, 0, ZITTAI, 100, 100, true);
                player[i] = data;
            }
        }
        player[0].SetOniGageFlag(true);
        player[1].SetOniGageFlag(true);
        player[0].SetMoveFlag(false, 0f);
        player[1].SetMoveFlag(false, 0f);

        return _data;
    }
    public override SceneDataPrivate Init(SceneDataPrivate _data)
    {
        _data.processState = State.UPDATA;
        playNum = _data.playNum;
        resultUI.SetActive(true);
        ResultDisp(_data.winner);
        UISkillDirecter ui_d = FindAnyObjectByType<UISkillDirecter>();
        ui_d.SetAnimFlag(false);
        SE.BGMplay(0);
        next = 0;

        return _data;
    }

    /* アップデート処理 --------------------------------------------------------------------------------- */
    public override SceneDataHost UpdataNetwork(SceneDataHost _data)
    {
        _data.processState = State.END;
        return _data;
    }
    public override SceneDataPrivate Updata(SceneDataPrivate _data)
    {
        if (next == 1) _data.processState = State.END;
        return _data;
    }

    /* 終了処理 後かたずけ ------------------------------------------------------------------------------ */
    public override SceneDataHost EndNetwork(SceneDataHost _data)
    {
        _data.sceneType = Scene.LOBBY;
        _data.processState = State.INIT;
        return _data;
    }
    public override SceneDataPrivate End(SceneDataPrivate _data)
    {
        winnerUI.SetActive(false);
        loserUI.SetActive(false);
        resultUI.SetActive(false);
        RpcMoveFlag(playNum);
        _data.sceneType = Scene.LOBBY;
        _data.processState = State.INIT;

        return _data;
    }

    /* ============================================================================================== */

    public void GetPlayerNum(int num)
    {
        playNum = num;
    }

    // リザルト表示
    void ResultDisp(int winner)
    {
        if (winner == -1) return;
        if(winner == playNum)
        {
            winnerUI.SetActive(true);
            SE.WinnerSEPlay();
        }
        else
        {
            loserUI.SetActive(true);
            SE.LoserSEPlay();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RpcMoveFlag(int num)
    {
        player[num].SetMoveFlag(true, 0f);
    }

    /* ======================================== コントローラー ================================================== */
    public override void Submit()
    {
        next = 1;
        SE.EnterSEPlay();
    }
}
