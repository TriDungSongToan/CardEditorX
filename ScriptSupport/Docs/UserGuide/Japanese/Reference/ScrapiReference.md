# Scrapyard

## 1. 概要

Scrapyard では、YGO OCG-Core によって定義されている Functions、Constants、Namespaces、Enums、Tags、Types に関するドキュメントを提供します。

Script Support では、これらの要素を検索可能なカテゴリに分類して管理しています。これにより、ユーザーはソースファイルを手動で検索することなく、各要素の名前、説明、および関連ドキュメントを参照できます。

Scrapi のデータはナビゲーションパネルの **Scrapi Books** 領域に表示され、対応するドキュメントは **Scrapi Result** 領域に表示されます。

## 2. Scrapi の構成

Script Support では、Scrapi を次の 6 つのカテゴリに分類しています。

| カテゴリ      | 説明         |
|--------------|-------------|
| Functions    | YGO OCG-Core によって提供される Function です。 |
| Constants    | YGO OCG-Core によって定義される Constant 値です。 |
| Namespaces   | 関連する要素を整理するために使用される Namespace です。 |
| Enums        | Enumeration およびそれに対応する Member です。 |
| Tags         | YGO OCG-Core によって定義される Tag です。 |
| Types        | Scrapi の定義内で参照される Type です。 |

このユーザーガイドでは、これら 6 つのカテゴリを総称して **Scrapi** と呼びます。

## 3. Scrapi の検索

Scrapi Books 領域には、**Scr Name** と **Scr Desc** の 2 つの検索 Tab が用意されています。

### 3.1. 名前による検索

**Scr Name** Tab では、名前を指定して Scrapi の各要素を検索できます。

**Scrapi の名前を検索** フィールドに検索キーワードを入力すると、一致する Scrapi Entry が検索結果一覧に表示されます。

### 3.2. 説明による検索

**Scr Desc** Tab では、説明の内容に基づいて Scrapi の各要素を検索できます。

**Scrapi の説明を検索** フィールドに検索キーワードを入力すると、対応するドキュメントの説明内容に基づいて、一致する Entry が検索結果一覧に表示されます。

## 4. Scrapi 情報の表示

いずれかの検索 Tab から Scrapi Entry を選択すると、対応するドキュメントが **Scrapi Result** 領域に表示されます。

この領域では、選択した Entry に関連する情報を確認できます。これにより、Card Script の作業中に必要なドキュメントを参照できます。

## 5. 関連するカードスクリプト

Script Support では、特定の Scrapi 要素を使用している Card Script を特定するための機能を提供しています。

### 5.1. 自動検索

**自動検索** オプションを有効にすると、選択中の Scrapi Entry の名前を含む Card Script が自動的に検索されます。

このオプションが有効な場合、Scrapi Entry を選択すると、設定済みの Data Source 内にある既存の `.lua` ファイルの内容を対象としたテキスト検索が実行されます。

この検索では、選択した Scrapi の名前をテキストとして含む Script を特定します。一致した Script は Script Result 領域に表示され、Card Script 内で該当する Scrapi 要素がどのように使用されているかを確認できます。

この機能はテキストベースの検索を実行します。Lua の構文解析や式の解析は行わないため、Scrapi 要素が実際に呼び出されているか、参照されているかどうかまでは判定しません。

この機能の有効・無効を切り替えるには、次の設定にアクセスしてください。

`設定 -> 環境設定 -> データ処理 -> 自動検索`

自動検索が無効の場合、Scrapi Entry を選択しても、関連する Card Script の検索は自動的に実行されません。

---

次へ: [Script Editor →](../Reference/ScriptEditorReference.md)

前へ:

[← カードリファレンス](../Reference/CardReference.md)

[← はじめに](../GettingStarted.md)
