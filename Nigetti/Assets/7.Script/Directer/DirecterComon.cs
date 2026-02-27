using System.Collections.Generic;
using UnityEngine;
using Fusion;
using ComonData;
using System;

public class DirecterComon : NetworkBehaviour
{
    public struct SceneDataHost
    {
        public Scene sceneType;
        public State processState;
        public NetworkPrefabRef prefab;
        public List<PlayerRef> playerRefs;
        public Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
        public List<Transform> spawnPos;
        public List<int> charNum;
    }
    public SceneDataHost hostData;

    public struct SceneDataPrivate
    {
        public Scene sceneType;
        public State processState;
        public sbyte playNum;
        public List<int> charNum;
        public sbyte winner;
    }
    public SceneDataPrivate privateData;

    public Func<SceneDataHost,SceneDataHost>[] hostProcess;
    public Func<SceneDataPrivate,SceneDataPrivate>[] privateProcess;

    // Start is called before the first frame update
    void Start()
    {
        hostProcess = new Func<SceneDataHost, SceneDataHost>[]
        {
            InitNetwork,
            UpdataNetwork,
            EndNetwork
        };
        privateProcess = new Func<SceneDataPrivate, SceneDataPrivate>[]
        {
            Init,
            Updata,
            End
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* ================================================================================================ */

    /// <summary>
    /// ホストデータの初期化　戻り値：SceneDataHost
    /// </summary>
    public SceneDataHost InitHost(NetworkPrefabRef _prefab, List<Transform> _spawnPos)
    {
        SceneDataHost Data = new SceneDataHost() { 
            sceneType = Scene.LOBBY,
            processState = State.INIT,
            prefab = _prefab,
            playerRefs = new List<PlayerRef>(),
            _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>(),
            spawnPos = _spawnPos,
            charNum = new List<int>(),
        };

        return Data;
    }

    /// <summary>
    /// 個人データの初期化　戻り値：SceneDataPrivate
    /// </summary>
    public SceneDataPrivate InitPrivate(int _playNum)
    {
        SceneDataPrivate Data = new SceneDataPrivate() {
            sceneType = Scene.LOBBY,
            processState = State.INIT,
            playNum = (sbyte)_playNum,
            charNum = new List<int>(),
            winner = -1
        };

        return Data;
    }

    public void ProcessHost(SceneDataHost _data, out SceneDataHost data)
    {
        data = hostProcess[ (int)_data.processState ](_data);
    }
    public void ProcessPrivate(SceneDataPrivate _data, out SceneDataPrivate data)
    {
        data = privateProcess[ (int)_data.processState ](_data);
    }

    /* ======================= Base =========================== */
    // 初期化
    public virtual SceneDataHost InitNetwork(SceneDataHost _data)
    {
        return _data;
    }
    public virtual SceneDataPrivate Init(SceneDataPrivate _data)
    {
        return _data;
    }
    // アップデート
    public virtual SceneDataHost UpdataNetwork(SceneDataHost _data)
    {
        return _data;
    }
    public virtual SceneDataPrivate Updata(SceneDataPrivate _data)
    {
        return _data;
    }
    // 終了
    public virtual SceneDataHost EndNetwork(SceneDataHost _data)
    {
        return _data;
    }
    public virtual SceneDataPrivate End(SceneDataPrivate _data)
    {
        return _data;
    }
    // 描画処理
    public virtual void RenderNetwork()
    {
        return;
    }
    // プレイヤーが途中退場した場合
    public virtual void LeftPlayerHost(SceneDataHost _data, out SceneDataHost data)
    {
        data = _data;
    }
    public virtual void LeftPlayerPrivate(SceneDataPrivate _data, out SceneDataPrivate data)
    {
        data = _data;
    }
    /* ============ コントローラー =============== */
    public virtual void Submit()
    {
        return;
    }
    public virtual void Cancel()
    {
        return;
    }
    public virtual void Menu()
    {
        return;
    }
    public virtual void CharMenu()
    {
        return;
    }
    public virtual void Stanby()
    {
        return;
    }
    public virtual void BattleStart()
    {
        return;
    }
}
