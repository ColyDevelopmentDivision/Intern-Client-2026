# 解答と解説（メンター用／実装が終わった人の振り返り用）

> ⚠️ このドキュメントには**すべての課題の答え**が書いてあります。
> 参加者に配るのは、課題が終わったあと（または共有タイム後）にしてください。
> 途中で詰まっている人には、まず `Docs/HINTS.md` の段階ヒントを案内します。

---

## TODO①：レアリティで演出を出し分ける

### 模範解答

`PlayResultEffect` の `effects.PlayCommon();` の1行を、次の `switch` に置き換えます：

```csharp
private void PlayResultEffect(Rarity rarity)
{
    switch (rarity)
    {
        case Rarity.SSR:
            effects.PlaySpecial();
            break;
        case Rarity.SR:
            effects.PlayRare();
            break;
        default: // R
            effects.PlayCommon();
            break;
    }
}
```

### 解説

- **switch 文**は「1つの値を複数の候補と見比べて分岐する」構文です。if を3つ並べても書けますが、
  「レアリティごとに1対1で割り当てる」という**意図が形に表れる**のが switch の良いところです。
- `rarity` は `enum Rarity { R, SR, SSR }` 型なので、比べる値は `Rarity.SSR` のように書きます。
- `default:` は「どの case にも当てはまらなかったとき」。ここでは SSR でも SR でもない＝ **R** が流れ着きます。
  将来レアリティが増えても（例：UR）、とりあえず控えめ演出で動き続ける安全網にもなっています。
- **割り当て自体に正解はありません**。「全部 PlaySpecial にすると何が起きるか」を試した人がいたら、
  “毎回派手だと特別感が消える＝演出はメリハリが価値” という気づきにつなげると良い議論になります。

### よくあるミス

| 症状 | 原因 | 対処 |
|---|---|---|
| `case` の後でコンパイルエラー | `break;` 忘れ | 各 case の最後に `break;` を書く |
| `SSR` だけ書いてエラー | enum の値は型名から書く | `Rarity.SSR` の形にする |

---

## TODO②：めくる直前の“予兆”

### 模範解答

`RevealRoutine` 内、TODO②コメントの位置（`ShowCard` の前）に追加します：

```csharp
if (item.rarity >= Rarity.SR)
{
    yield return effects.PlayOmen();
}
```

### 解説

- **`>=` で比較できる理由**：`Rarity` は `R → SR → SSR` の順に定義されており、enum は内部的には
  定義順の整数（0, 1, 2）です。だから `item.rarity >= Rarity.SR` は「SR か SSR」の意味になります。
  （このため **Rarity.cs の定義順は変更禁止**です）
- **`yield return` の意味**：`RevealRoutine` はコルーチン（少しずつ進む処理）で、
  `yield return effects.PlayOmen();` は「予兆演出が終わるまで、めくり進行をここで待つ」という書き方です。
  この“待ち”こそが演出の**溜め**の正体です。
- **書く場所が `ShowCard` の前**なのは、「めくる直前」に溜めを入れたいから。後ろに書くと
  カードが見えてから予兆が出る、という間の抜けた動きになります（試すと面白いので、あえて見せるのもアリ）。
- **基準の設計論**：「SR以上」だと10連中4回予兆が出て賑やかに、「SSRだけ」だと2回に絞られ特別感が増します。
  どちらが“ドキドキ”するかは好みが分かれるので、共有タイムで判断の違いを拾うと盛り上がります。

### よくあるミス

| 症状 | 原因 | 対処 |
|---|---|---|
| `yield return` の行でコンパイルエラー | `PlayResultEffect`（普通のメソッド）の中に書いた | `yield return` はコルーチン内でしか使えない。`RevealRoutine` の TODO②コメントの位置に書く |
| 予兆が全カードで出る | 条件を書き忘れて if の外に置いた | `if (item.rarity >= Rarity.SR)` の中に入れる |

---

## TODO③：予兆の色をランダムに

### 模範解答

TODO②で書いた if の**中身**を書き換えます：

```csharp
if (item.rarity >= Rarity.SR)
{
    int pattern = Random.Range(0, 3); // 0・1・2 のどれか
    yield return effects.PlayOmen(pattern);
}
```

パターン番号と色の対応：**0=金／1=紫／2=水色**（シーン上の OmenVariationFx_0〜2）。

### 解説

- **`PlayOmen()` と `PlayOmen(int)` が両方ある**のは、C# の**オーバーロード**（同じ名前で引数違いのメソッドを
  複数用意できる仕組み）です。呼ぶ側は渡す引数によって自動で使い分けられます。
- **`Random.Range(0, 3)` は 0〜2 を返します**（int 版は第2引数＝最大値を**含まない**）。
  「3種類からランダム」のときに `(0, 3)` と書くのは慣れないと間違えやすいポイントです。
  ちなみにこの `Random` は `UnityEngine.Random` です（このファイルではそのまま書けます）。
- **`yield return` を忘れると予兆が出なくなる**——単にエラーになるのではなく“無反応”になるのが罠です。
  `PlayOmen` はコルーチン（`IEnumerator` を返すメソッド）なので、呼んだだけでは**中身が1行も実行されません**。
  `yield return` で「進行役」に渡してはじめて動きます。「エラーは出ないのに動かない」ときの典型例として
  紹介すると、コルーチンの理解が一段深まります。
- **発展的な問い**：いまは色が完全ランダムですが、本物のソシャゲには「予兆の色でレア度を匂わせる」
  文化があります（例：金の予兆ならSSR確定）。「色に意味を持たせるなら、pattern をどう決める？」
  という問いは、`Random.Range` を `item.rarity` ベースの分岐に置き換える良い思考実験になります。

### よくあるミス

| 症状 | 原因 | 対処 |
|---|---|---|
| 予兆がまったく出なくなった | `yield return` を消して `effects.PlayOmen(pattern);` だけ書いた | `yield return effects.PlayOmen(pattern);` の形に戻す |
| 色が3色より少ない気がする | ランダムなので偏ることはある | 何度か引き直す。`Random.Range(0, 2)` になっていないかも確認（それだと2色） |

---

## 完成形（TODO①②③をすべて反映した該当部分）

```csharp
// RevealRoutine 内（めくりループの中・ShowCard の前）
if (item.rarity >= Rarity.SR)
{
    int pattern = Random.Range(0, 3);
    yield return effects.PlayOmen(pattern);
}

ShowCard(item, result.isNew, cardNumber, totalCount);   // 「3/10」の枚数表示付き（用意済み）
PlayResultEffect(item.rarity);
AddSummaryIcon(item);
```

```csharp
// PlayResultEffect 内
switch (rarity)
{
    case Rarity.SSR:
        effects.PlaySpecial();
        break;
    case Rarity.SR:
        effects.PlayRare();
        break;
    default:
        effects.PlayCommon();
        break;
}
```

---

## 発展課題：ガチャの種類を増やす

### 模範手順

1. Project ウィンドウで `Assets/MasterData` を開き、`GachaTable_1_通常ガチャ` を選択して **Ctrl+D** で複製
2. 複製したアセットの名前を変更（例：`GachaTable_3_〇〇ガチャ`）
3. Inspector で中身を書き換える：
   - `Gacha Id`：**3**（既存の 1・2 と被らない値）
   - `Gacha Name`：バナーに表示したい名前
   - `Cost Amount`：1回のコスト（所持石の初期値は 100,000）
   - `Entries`：itemId（2001〜2010）と weight の組を、**出したい順に**並べる
4. `Assets/Scenes/GachaTopScene` を開き、Hierarchy の `GachaTopSceneFlow` を選択
5. Inspector の **Gacha Tables** 配列サイズを 2→3 にして、3枠目に複製したアセットをドラッグ＆ドロップ
6. Play → 3つ目のバナーが表示され、自分の並び順で10連が出れば完成

### 解説：なぜコードなしで増やせるのか

この課題の狙いは、**「ゲームの中身はコードではなくデータ（マスタ）で決まっている」**を体感してもらうことです。

- バナーを並べる処理（`GachaTopSceneFlow`）は、最初から「**登録されたテーブルの数だけ**バナーを生成する」
  実装になっています。だからテーブルというデータを1つ足せば、コードに触れずに画面が変わります。
- 名前もコストも `GachaTable` アセットの値を表示しているだけなので、企画の変更＝データの変更で完結します。
- これは実際のゲーム運用と同じ構造です。**毎週の新ガチャはプログラマがコードを書いて出しているのではなく、
  企画者がマスタデータを追加して出しています**。午前に見た「マスタ」の意味が、ここで実感につながります。

### 観察ポイント（余裕がある人向けの仕掛け）

- **Entries を10件未満にすると**、10連の途中で先頭に戻って同じキャラが再登場します。
  再登場したカードには **NEW が付きません**（isNew は同一10連内の初出判定）。
  「NEWの付く/付かないは誰がどう決めている？」を観察する良い素材になります。
- **weight の値は今日は使われません**（登録順の固定結果のため）。weight が意味を持つのは、
  抽選を実装する午後のサーバー会——「同じマスタをクライアントとサーバーで別の目的に使う」ことの伏線です。
- 並び順の演出効果も比較できます：初心者応援ガチャは通常ガチャの**逆順**（SSRが9・10枚目）に
  してあり、「最後に盛り上がる」タイプの体験です。自分のガチャはどちらのタイプ？

### よくあるミス

| 症状 | 原因 | 対処 |
|---|---|---|
| バナーが増えない | Gacha Tables 配列に未登録／別のシーンを開いている | 手順4〜5を確認（シーンは GachaTopScene） |
| 引いても演出が始まらない | `Entries` が空（Console に `[FixedGachaService]` のエラー） | Entries に1件以上登録する |
| 一部のカードでエラーが出る | ItemMaster にない itemId（2001〜2010 以外） | Console の「itemId=xxxx が見つかりません」を手がかりに修正 |
| バナーを押しても反応しない（警告ログ） | Cost Amount が所持石（100,000）を超えている | Cost Amount を下げる（所持石は再生のたびに 100,000 にリセット） |

---