---
name: release
description: Rectorの新バージョンをリリースする。バージョンbump・タグ作成・GitHubリリース公開までの定型手順。ユーザーが「X.Y.Zをリリースしたい」「リリース作業をして」と言ったときに使う。
---

# Rector リリース手順

リリースは「リリースissue → バージョンbumpのPR → マージ → タグ → GitHubリリース公開」の流れ。
ビルド成果物は添付しない（タグとリリースノートのみ）。

## 1. 現状確認

```bash
git tag --sort=-creatordate | head -5        # 前回バージョン
grep bundleVersion ProjectSettings/ProjectSettings.asset
git log v<前回>..main --oneline              # 今回の変更一覧
```

バージョン番号はユーザーの指定に従う。指定がなければ変更内容からsemverで提案して確認する。

## 2. リリースissueとブランチ

- issueタイトル: `rector X.Y.Zをリリースする`
- issue本文: やること（bump / タグ / リリース公開）と、今回の主な変更のissue/PR番号一覧
- ブランチ: `issue-<issue番号>-version-X.Y.Z`

## 3. バージョンbump

変更するのは2ファイル・2箇所:

| ファイル | フィールド |
| --- | --- |
| `ProjectSettings/ProjectSettings.asset` | `bundleVersion` |
| `Assets/Rector/Settings/RectorSettings.asset` | `hudSettings.version`（HUD表示用） |

**エディタ起動中はディスク直編集しない**（エディタの次回保存で巻き戻される）。
`unity status` で確認し、起動中なら eval 経由で変更する。複数エディタがいる場合は
mainプロジェクトの `--port` を明示する:

```bash
unity command eval --port <port> 'UnityEditor.PlayerSettings.bundleVersion = "X.Y.Z";
var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>("Assets/Rector/Settings/RectorSettings.asset");
var so = new UnityEditor.SerializedObject(obj);
var prop = so.FindProperty("hudSettings.version");
prop.stringValue = "X.Y.Z";
so.ApplyModifiedProperties();
UnityEditor.EditorUtility.SetDirty(obj);
UnityEditor.AssetDatabase.SaveAssets();
return UnityEditor.PlayerSettings.bundleVersion + " / " + prop.stringValue;'
```

`git diff` で2行だけの差分になっていることを確認してからコミットする。

## 4. PR

- コミットメッセージ: `chore: bump version to X.Y.Z`
- PR タイトル: 同上。本文に `close #<リリースissue番号>` を入れる（マージでissueが自動クローズされる）

## 5. リリースノート下書き → ユーザー確認

前回リリース（`gh release view v<前回>`）とスタイルを揃える。日本語で:

- 機能ごとに `### <見出し> (#issue番号)` セクション。ノード名は **太字**
- 挙動の変更は1行ずつ箇条書き。**互換性が壊れる変更（プリセット・保存ファイルへの影響）は必ず明記**
- 内部修正は `### 修正` にまとめる
- 末尾: `**Full Changelog**: https://github.com/shivaduke28/Rector/compare/v<前回>...vX.Y.Z`

内容の正確性はissue本文とPR本文（`gh pr view`）で裏を取る。マージ直前に方針転換した
コミットがあることがあるので、最終仕様はPR本文を正とする。

**ここで必ず止まる**: リリースノート下書きと残り手順を提示し、shivadukeの承認を待つ
（PRマージはmainへのpushなので事前確認が必須）。マージはshivadukeが行うことが多い。

## 6. タグとリリース公開（承認後）

```bash
git checkout main && git pull
git tag vX.Y.Z && git push origin vX.Y.Z
gh release create vX.Y.Z --title "vX.Y.Z" --notes "<下書きしたノート>"
```

最後にリリースissueが自動クローズされたことを確認する（`gh issue view <番号> --json state`）。

## 過去のリリース例

- v1.3.0: issue #158 / PR #159
- v1.2.0: issue #145 / PR #146（リリースノートのスタイル参考）
