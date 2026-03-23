# FriendlyFire-StS2 — Slay the Spire 2 フレンドリーファイア Mod

**🌐 Language / 言語：** [English](README_EN.md) | 日本語 | [한국어](README_KO.md) | [中文](README.md)

> `Alt` キーを押しながら、攻撃カードで味方を「フレンドリーに」斬りつけよう。

![Slay the Spire 2](https://img.shields.io/badge/Slay%20the%20Spire%202-Mod-red?style=flat-square)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square)
![Harmony 2.4.2](https://img.shields.io/badge/Harmony-2.4.2-green?style=flat-square)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)
![AI Assisted](https://img.shields.io/badge/AI%20Assisted-Claude-blueviolet?style=flat-square)

---

## 機能紹介

| 機能 | 説明 |
|------|------|
| **単体攻撃フレンドリーファイア** | `Alt` キーを押しながら、`AnyEnemy` タイプの攻撃カードで味方をターゲットに選択可能 |
| **AOE 拡張攻撃** | `Alt` キーを押しながら、`AllEnemies` タイプの AOE カードが**他のプレイヤーのキャラクター**にもヒット（自分と自分の召喚物は除外） |
| **特殊効果適用** | カードのデバフ（脆弱、弱体化など）が味方にも通常通り適用 |
| **JSON ホワイトリスト** | 設定ファイルでフレンドリーファイアを許可するカードを細かく制御 |
| **危険カード保護** | `Monster` プロパティにアクセスするカードを自動ブロックしてクラッシュを防止 |
| **画面インジケーター** | トグルキーを押している間、画面上部に赤い「フレンドリーファイア ON」バナーを表示 |
| **マルチプレイ同期安全** | TargetId シグナルメカニズムにより全クライアントの状態が同期 |

---

## インストールガイド

> **重要：Mod をインストールする前に、必ずセーブデータをバックアップしてください！**
>
> セーブデータの場所：
> - **Windows:** `%APPDATA%\..\Roaming\SlayTheSpire2\`
> - **macOS:** `~/Library/Application Support/SlayTheSpire2/`

### ステップ 1：ゲームディレクトリを確認

| プラットフォーム | パス |
|----------------|------|
| **Windows** | `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\` |
| **macOS** | `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/` |

> **ヒント：** Steam でゲームを右クリック → 管理 → ローカルファイルを閲覧

### ステップ 2：mods フォルダを作成

ゲームルートディレクトリに `mods` フォルダを作成（既にある場合はスキップ）。

### ステップ 3：BaseLib（前提 Mod）をインストール

本 Mod は [Alchyr/BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) が必要です。**先にインストールしてください。**

1. [BaseLib-StS2 Releases](https://github.com/Alchyr/BaseLib-StS2/releases) から最新版をダウンロード
2. 解凍した `BaseLib` フォルダを `mods/` に配置

### ステップ 4：FriendlyFire をインストール

1. [Releases](../../releases) から最新の `FriendlyFire.zip` をダウンロード
2. 解凍した `FriendlyFire` フォルダを `mods/` に配置

```
mods/
  +-- BaseLib/                      <-- 前提 Mod（ステップ 3）
  +-- FriendlyFire/                 <-- 本 Mod
        +-- FriendlyFire.dll
        +-- FriendlyFire.pck
        +-- mod_manifest.json
        +-- friendly_fire_config.cfg
```

### ステップ 5：ゲームを起動

1. Slay the Spire 2 を起動
2. メインメニュー → **Mod マネージャー**
3. **BaseLib** と **Friendly Fire** を有効化
4. 協力バトルを開始

### 使い方

| 操作 | 効果 |
|------|------|
| **Alt なし**で攻撃カードを使用 | 通常の動作（バニラと同じ） |
| **Alt を押しながら**単体攻撃カードを使用 | 味方をターゲットとして選択可能、赤いインジケーターが表示 |
| **Alt を押しながら** AOE カードを使用 | AOE が全敵 + 他プレイヤーのキャラクターにヒット（自分と召喚物は除外） |

> **マルチプレイ注意：** 全プレイヤーが**同じバージョン**の Mod と**同一の**ホワイトリスト設定を使用する必要があります。ホストが設定ファイルを配布することを推奨。

### アンインストール

1. `mods/FriendlyFire/` フォルダを削除
2. ゲームを再起動 — セーブデータに影響なし

---

## 設定

`friendly_fire_config.cfg` を編集して Mod の動作をカスタマイズ。変更後は**ゲームを再起動**。

```jsonc
{
  // このキーを押してフレンドリーファイアを有効化。選択肢: Alt, Shift, Ctrl, Tab, Space, F1~F4
  "toggle_key": "Alt",

  // 単体攻撃カードのホワイトリスト（カードクラス名）
  // 空配列 [] = 全ての単体攻撃カードを許可
  "single_target_whitelist": [],

  // AOE フレンドリーファイア拡張を有効化
  "aoe_enabled": true,

  // AOE カードのホワイトリスト（上記と同じルール）
  "aoe_whitelist": [],

  // 危険カードのブラックリスト（Target.Monster にアクセスしてクラッシュするカード）
  "dangerous_cards_blacklist": []
}
```

---

## FAQ

**Q: フレンドリーファイアは自分にダメージを与えますか？**
> いいえ。AOE は攻撃者本人と全ての召喚物/ペットを除外します。

**Q: シングルプレイで機能しますか？**
> 単体フレンドリーファイアにはターゲットとなる味方がいません。本 Mod は**協力マルチプレイ**向けに設計されています。

**Q: マルチプレイで切断されますか？**
> いいえ。Mod は TargetId シグナルメカニズムを使用して、全クライアントが同一のターゲット計算を行います。全プレイヤーが同じ Mod バージョンと設定を使用する必要があります。

---

## AI 開発について

本プロジェクトは AI（Claude）を活用して開発されました。詳細な技術アーキテクチャについては[中国語 README](README.md) をご参照ください。

---

## ライセンス

[MIT License](LICENSE) — 自由に使用、改変、配布可能。
