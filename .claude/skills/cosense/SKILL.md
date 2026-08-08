---
name: cosense
description: Rectorのユーザー向けドキュメント（Cosense wiki）を読む・更新する。ノードや操作方法などRectorの使い方を調べるとき、実装変更に合わせてwikiを追従させるときに使う。
---

# Cosense (Rector wiki)

Rectorのユーザー向けドキュメントは Cosense（旧Scrapbox）にある。

- プロジェクトURL: https://scrapbox.io/Rector/
- CLI: `cosense`（[@helpfeel/cosense-cli](https://github.com/helpfeel/cosense-cli)。`cosense --help` / `cosense <command> --help` でコマンドの詳細が出る）
- 認証: `cosense login` でPersonal Access Tokenを保存する。`cosense whoami https://scrapbox.io` で現在のユーザーを確認できる

## 何がどこに書いてあるか

| 対象 | 置き場所 |
| --- | --- |
| Rectorの**使い方**（ノードの機能、操作方法、シーン構成、カスタマイズ手順） | Cosense wiki |
| **開発**の作法（コーディング規約、Unity CLI、ビルド手順、AIへの指示） | リポジトリの `CLAUDE.md` |

wikiはVJとしてRectorを使う人向け。実装の内部構造やクラス名は書かない。逆に開発フローの話をwikiに書かない。

## 権限（重要）

**書き込みできるのは shivaduke28（プロジェクトオーナー）だけ。**

- Rectorをforkして使う人にとって、このwikiは**読み取り専用**。`previewEdit` / `submitEdit` は403で失敗する
- fork先で独自のwikiを運用する場合は、以下のURLを自分のプロジェクトのものに読み替えれば手順はそのまま使える
- 読み取り（`readPage` などの調査系コマンド）は誰でも実行できる

## 読む

```bash
# 1ページ読む（本文だけ）
cosense readPage https://scrapbox.io/Rector/操作方法 | jq -r '.lines[].text'

# メタデータ込みで読む（AI向け整形）
cosense browsePage https://scrapbox.io/Rector/操作方法

# ページ一覧（更新日時降順。pinnedページが先頭に来る）
cosense listPages https://scrapbox.io/Rector/ --sort updated --limit 100

# タイトル+本文の要約を一覧で見る
cosense listPages https://scrapbox.io/Rector/ --sort updated --limit 100 \
  | jq -r '.pages[] | [.title, (.updated|split("T")[0])] | @tsv'

# 検索
cosense searchFullText https://scrapbox.io/Rector/ <query>   # 全文
cosense searchVector   https://scrapbox.io/Rector/ <query>   # 意味検索（タイトル+リンク記法のみ）

# 関連ページ
cosense browseRelatedPages https://scrapbox.io/Rector/node
cosense list1hopLinks      https://scrapbox.io/Rector/node
```

ページタイトルは日本語のままURLに含めてよい（シェルのクォートは必要）。

## ページの書き方

- 日本語。です・ます調ではなく簡潔な体言止め・箇条書きが基本
- Scrapbox記法:
  - インデント箇条書きは**タブ**（スペースではない）
  - `[ページ名]` で内部リンク、`[表示テキスト URL]` で外部リンク
  - `[* 強調]`、`[** 見出し]`
  - `[画像URL]` を単独行に置くと画像が埋め込まれる
  - `` `コード` ``、行頭 `code:filename` でコードブロック
  - `[shivaduke28.icon]` でアイコン。一言コメントの主を示すのに使う
  - 一覧表は `table:名前` の行に続けて、**タブ1つ下げた行をタブ区切り**で並べる（`Node一覧` や `操作方法` がこの形）
- **タグは行頭に `#タグ名`** を書く。`#function` `#custom` `#node` `#bug` が運用中
  - タグページ（`function` / `custom` / `node` / `bug`）は画像1枚だけの実体で、**タグを付けたページが逆リンクで自動的に集まる**
  - つまり**新規ページを作るときに索引ページを編集する必要はない**。行頭タグを付ければよい
- 1ノード1ページが原則だが、単純な演算ノードは `Node一覧` にまとめる

## 書き込む（オーナー専用）

書き込みは2段階。`previewEdit` でdry-runして `previewId` を取り、`submitEdit` で確定する。

**previewId は5分で失効し、1回使うと消える。** 本文のドラフトを固めてから preview → submit を連続で回すこと。先に全ページ分のpreviewをまとめて取ると失効する。

### 新規ページ

stdin/ファイルの**1行目がページタイトル**、2行目以降が本文。

```bash
cat > /tmp/page.txt <<'EOF'
OSC入力
#function

	外部アプリからOSCでRectorを操作する
EOF

cosense previewEdit --new --input-file /tmp/page.txt https://scrapbox.io/Rector
# → previewId を確認して
cosense submitEdit https://scrapbox.io/Rector <previewId>
```

タブインデントを含む本文をヒアドキュメントで書くときは `<<'EOF'`（クォート付き）を使い、タブが潰れないようファイル経由で渡す。

### 既存ページの編集

`readPage` でページIDと行IDを取ってから ops JSON を組む。

```bash
cosense readPage https://scrapbox.io/Rector/操作方法 | jq '{id, lines: [.lines[] | {id, text}]}'
```

```bash
cat > /tmp/ops.json <<'EOF'
{
  "ops": [
    {"replace": "<lineId>", "text": "決定: Space, Z"},
    {"insertBefore": "<lineId>", "text": "新しい行1\n新しい行2"},
    {"insertBefore": "_end", "text": "末尾に追加"},
    {"delete": "<lineId>"}
  ]
}
EOF

cosense previewEdit --input-file /tmp/ops.json https://scrapbox.io/Rector <pageId>
cosense submitEdit https://scrapbox.io/Rector <previewId>
```

- `replace` は**単行のみ**。改行を含むtextは422で拒否される。複数行にしたいときは `replace` + `insertBefore` を組み合わせる
- `insertBefore` の text は `\n` で複数行を一度に挿入できる
- anchorに指定する lineId は**適用時点で存在している**必要がある。同じopsの中で削除した行をanchorにすると422
- ページ全体を書き換えるなら、全行 `delete` + `insertBefore: "_end"` より、既存行を活かした差分opsのほうが履歴が読みやすい
- 節ごと差し替えるときは「**残す行をanchorにして新本文を `insertBefore` → 旧行を `delete`**」を1つのopsにまとめる。opsは配列順に適用されるので、insertを先に置けばanchorが消える心配がない
- 行IDと本文の対応は `readPage ... | jq -r '.lines[] | .id + "\t" + .text'` で並べると組みやすい。opsの組み立て自体をスクリプトにするとタブや改行の取り回しが安全

### エラー

| コード | 意味 | 対処 |
| --- | --- | --- |
| 403 | 権限不足 | オーナー以外は書き込めない。読み取りに切り替える |
| 404 (submit時) | previewIdが失効/consume済み | `previewEdit` からやり直す |
| 409 NotFastForward | preview後にページが更新された | `readPage` から取り直して ops を作り直す |
| 422 | opsが不正 | 多行`replace`、存在しないlineIdをまず疑う |

### 確認のルール

- **既存ページの書き換えは、preview結果（差分）をshivadukeに見せてから `submitEdit` する。** 消える行があるときは必ず確認を取る
- 新規ページの作成、および明らかな事実誤り（バージョン番号やキーバインドの不一致）の修正は、ドラフトの承認が取れていればpreview→submitまで通してよい
- shivadukeが「まとめてやっていい」と言った場合はその範囲で通しで実行してよい

## 棚卸し（実装への追従）

wikiは放っておくと実装から乖離する。定期的に、または大きな機能追加のあとに実施する。

1. **鮮度を測る**

   ```bash
   cosense listPages https://scrapbox.io/Rector/ --sort updated --limit 100 \
     | jq -r '.pages[] | [(.updated|split("T")[0]), .title] | @tsv' | sort
   git log --oneline origin/main --merges --since=<最も古い更新日>
   ```

   ページの更新日より後に入ったPRが、そのページの守備範囲に触れていないかを見る。

2. **調査を並列化する。** ページをテーマ別（ノード系 / 入力・操作系 / シーン・設定系 / メタ・bug系）に分け、サブエージェントに「担当ページを `readPage` で読む + 対応コードを読む → keep / 要更新 / 廃止 の判定と修正案」を出させる。**調査エージェントには書き込みを禁止する**（読み取り専用と明示する）

3. **判定の材料はコードに置く。** バージョン番号、キーバインド、保存先パス、ポート番号などの具体値は必ずソース（`RectorInput.inputactions`、`ProjectVersion.txt`、`NodeTemplateRegisterer.cs`、各ノードの実装）から確認する。
   - **PRの本文を現状だと思ってはいけない。** 後続のPRが同じ場所を作り直していることがある（例: #46 が入れた NodeModifier は #51 のキー入力刷新で別物になっている）。必ず現在のソースを見る
   - **「修正済み」と書く前にリリース状況を確認する。** `ProjectSettings.asset` の `bundleVersion` と `gh release list` を突き合わせる。mainに入っているだけなら「修正済み。まだリリースには入っていない」と書く

4. **リンク切れを潰す。** 既存ページが `[スロット選択モード]` のように未作成ページを指していることがある。棚卸しのついでに実体を作るか、リンクを張り替える。

   ```bash
   # 新規・更新したページから出ているリンクが全て実在するか
   cosense readPage https://scrapbox.io/Rector/<page> | jq -r '.links[]?' | while read -r l; do
     [ "$(cosense readPage "https://scrapbox.io/Rector/$l" | jq -r .persistent)" = true ] || echo "RED: $l"
   done
   ```

5. **レポートを1本にまとめてshivadukeに提示**してから書き込みフェーズに入る。ページ単位で update / new / keep が分かる形にする

6. **書き込みは1ページずつ preview → submit** で回す（previewIdの5分制限のため）

7. **検証**

   ```bash
   cosense readPage <submitEditが返したURL> | jq -r '.lines[].text'   # 反映確認
   cosense list1hopLinks https://scrapbox.io/Rector/node              # タグの逆リンクに載ったか
   cosense listPages https://scrapbox.io/Rector/ --sort updated --limit 100 \
     | jq -r '.pages[] | [(.updated|split("T")[0]), .title] | @tsv' | head -20
   ```
