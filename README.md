# にげっち
2年次にチーム制作で作った2人で対戦するオンライン鬼ごっこゲームです。<br>
オンラインゲームに初めて挑戦した作品です。

## 概要
UnityとPhotonFusionを使用して制作した、リアルタイム通信のオンラインアクションゲームです。<br>
プレイヤー入力は NetworkInputData を用いてネットワーク同期し、複数クライアント間で同一のゲーム挙動を再現しています。<br>
また、ゲーム進行は GameDirector によって各シーンを管理しています。

## 使用技術
- Unity 2022.3.19f1
- C#
- Photon Fusion / DOTween / UniTask / Cinemachine

## システム構成
　TitleDirecter<br>
GameDirecter ← オンライン接続<br>
 ├ LobbyDirecter<br>
 ├ SelectDirecter<br>
 ├ BattleDirecter<br>
 └ ResultDirecter<br>

## 見てほしいコード
- GameDirecter.cs
  `Assets/7.Script/GameDirecter.cs`
- PlayerController.cs
  `Assets/7.Script/Player/PlayerController.cs`
- BasicSpawner.cs
  `Assets/7.Script/BasicSpawner.cs`
- NetworkInputData.cs
  `Assets/7.Script/NetworkInputData.cs`

## 動作デモ
https://youtu.be/oRcOt79kO1s

## 制作期間
6ヶ月

## 制作体制
チーム制作（メインプログラマー担当）