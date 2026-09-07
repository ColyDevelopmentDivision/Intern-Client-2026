# 1day_Intern_2026 引継ぎドキュメント（Claude Code用）

このファイルはチャット版Claudeでの設計作業をClaude Codeへ引き継ぐためのもの。
プロジェクトルート（`C:\Users\kanazashi\Documents\Intern-Client-2026`。2026-07-16 に `1day_Intern_2026` から改名）に `CLAUDE.md` として配置すると自動で読み込まれる。既存の CLAUDE.md がある場合は統合すること。

---

## 0. 作業規約（必ず守る）

- 回答・コメント・ドキュメントは日本語。
- 調査時は必ず情報ソースを確認し、何を参考にしたか明文化する。
- **コミットは必ずユーザーの確認を取ってから行う。**
- コミットログは一行で明確に（Claude署名や詳細説明は不要）。
- 仕様の「正」は Google Sheets「1day_Intern_2026_企画たたき_ガチャ」（タブ: 企画概要／タイムスケジュール／クライアント会／サーバー会／事前準備項目／メモ用シート／マスタ／IF）。本書の値はその 2026-07-08 時点の転記。シートと食い違ったらシート優先で、本書と `Docs/` を更新する。

## 1. プロジェクト概要

- 1dayインターン（学生対象・計150分）。Unity/プログラミング初心者を含む前提で、ゲーム会社のエンジニア業務（演出づくり／システムづくり）に興味を持ってもらうことが目的。
- 題材はガチャ。**クライアント会（排出演出・Unity）** と **サーバー会（抽選API・ASP.NET Core）** を時間で区切って実施。同一リポジトリ・同一マスタ/IFを共有するが、各会は独立して完結する。
- 設計方針: 土台コードと素材は運営が用意し、参加者は「面白い部分・考えどころ」だけを穴埋めで実装する。**参加者が編集するのは各会1ファイル・TODO2か所のみ。**
- 進行（タイムスケジュールタブ準拠）: 全体導入10分 → クライアント会（デモ10／実装40／共有5）→ 橋渡し10分（問いかけ「ガチャ結果は何が決める？」）→ サーバー会（説明10／実装40〜50／共有5）→ まとめQ&A。
  - ※要確認: シートの「時間帯」と「所要(分)」列に不整合の疑いあり（サーバー実装のレンジ40分に対し所要50分表記など）。確定時にシートと `Docs/INSTRUCTOR_NOTES.md` を揃えること。

## 2. 環境・配置

| 項目 | 内容 |
|---|---|
| Unityプロジェクト | `C:\Users\kanazashi\Documents\Intern-Client-2026`（2026-07-16 に `1day_Intern_2026` から改名し git 管理開始。**Unity 6000.3.17f1**＝2026-07-16 に 6000.0.67f1 からアップグレードされ動作確認済み。※ルート README.md の記載は 6000.3.19f1 で**不一致・要確認**。参加者PCは実体版数に統一する。URP Universal 3Dテンプレート／TextMeshPro） |
| クライアントスクリプト | `Assets/Scripts` 直下に9本（配置済み。§4参照）＋ `Assets/Scripts/GachaFlow/` にシーン遷移用4本（2026-07-15追加仕様。**課題対象外・参加者は見なくてよい**。全ファイル冒頭に明記） |
| シーン構成 | `Assets/Scenes`: **GachaTopScene（バナー選択式: 登録テーブル数ぶんバナーが並び、選んだテーブルで10連。右上に所持石表示＝仮実装: 開始時100,000・引くたび costAmount 減）→GachaPlayScene（タップで1枚ずつ・右上スキップ ≫ で即リザルト）→GachaResultScene→(「トップへ戻る」ボタンで)Top** のフロー3シーンのみ（Build Settings 登録済み）。※単独版 GachaScene と SampleScene は 2026-07-16 のクリーンアップで削除済み |
| 運営用エディタツール | **削除済み（2026-07-16 クリーンアップ）**。シーン構築完了に伴い `Assets/Editor/`（GachaSceneBuilder.cs＝`1day Intern` メニュー各STEP／GachaFlowAutoSetup.cs＝自動セットアップ）を撤去した。復元は git 履歴から可能。SCENE_BUILD_CHECKLIST の STEP 3〜7.8 は構築時の記録として読むこと |
| マスタアセット | `Assets/MasterData`（ItemMaster ×1＝アイテム10件構成（村人B/C 2009・2010 を含む。シートは8件のまま→反映待ち）、GachaTable ×2＝通常／初心者応援（初心者応援は逆順=SSRがラスト、2009・2010のweightは暫定8【要確定】）。icon はプレースホルダ割当済み） |
| ドキュメント | `Docs/SETUP_GUIDE.md`（運営構築手順）／`Docs/INSTRUCTOR_NOTES.md`（進行・解答・発展）／`Docs/STUDENT_HANDOUT.md`（参加者配布） |
| サーバー（予定） | ASP.NET Core 最小API（C#/.NET 8、Swagger、旧ドラフトは `http://localhost:5080`）。**Assetsの外**（例: プロジェクト直下 `Server/`）に置くこと |

## 3. 確定仕様（シート転記・2026-07-08時点）

### 3-1. マスタ（マスタタブ）

**gacha_master**（start/endはシートのExcelシリアル値 46023／73050.99998842593 を日時換算）

| gacha_id | name | start_at | end_at | cost_item_id | cost_amount |
|---|---|---|---|---|---|
| 1 | 通常ガチャ | 2026-01-01 00:00:00 | 2099-12-31 23:59:59 | 1001 | 300 |
| 2 | 初心者応援ガチャ | （同様） | （同様） | 1001 | 150 |

**item_master**（item_typeは全て character。**レアリティは R/SR/SSR の3段階、N廃止で確定**）

| item_id | name | rarity |
|---|---|---|
| 2001 | 炎の勇者 | SSR |
| 2002 | 氷の魔導士 | SSR |
| 2003 | 光の聖騎士 | SR |
| 2004 | 風の狩人 | SR |
| 2005 | 森の弓使い | R |
| 2006 | 鉄の戦士 | R |
| 2007 | 見習い魔法使い | R |
| 2008 | 村人 | R |

**gacha_detail_master**（weightは比率。gacha1は合計100 → SSR 2%／SR 18%／R 80%）

| item_id | gacha_id=1 | gacha_id=2 |
|---|---|---|
| 2001 | 1 | 3 |
| 2002 | 1 | 3 |
| 2003 | 8 | 12 |
| 2004 | 10 | 17 |
| 2005 | 30 | 25 |
| 2006 | 25 | 20 |
| 2007 | 15 | 12 |
| 2008 | 10 | 8 |

### 3-2. IF（IFタブ）

- エンドポイント: **`POST /api/gacha/draw`**（10連一本化。旧案の単発 `/gacha/draw`＋`/gacha/draw10` 分離は廃止。サーバー会タブに旧記述が残るがIFタブが正）
- リクエスト `GachaDrawInParam`: `{ "gachaId": 1 }`
- レスポンス `GachaDrawOutParam`: `{ "results": [ { "itemId": 2005, "isNew": true }, …計10件 ] }`
- **isNew の基本仕様**: ガチャ（1回のdraw呼び出し）ごとにリセットし、同一10連内で初出=true／2回目以降の重複=false。発展課題（ユーザーDB実装）まで進むと過去の排出結果が反映される仕様に拡張。
- エラー形（石不足・期間外など）は**未定義**。必要になったらサーバー会実装時に追加。クライアントは JsonUtility が未知フィールドを無視するため、レスポンスへの項目追加は前方互換。

## 4. クライアント実装の現状（完成・配置済み）

`Assets/Scripts` 直下の9本。**参加者が編集するのは GachaController.cs のTODO2か所のみ**、他は用意済みで触らせない。
※ 2026-07-14: 配置されていた旧ドラフト（ItemMaster=1アセット1アイテム・GachaTable=アセット直接参照）を、本節の確定仕様（1アセット複数行＋FindById／itemIdのID紐づけ）どおりに改修済み。GachaController には ItemMaster の割当欄が増えている（SETUP_GUIDE 手順4）。
※ 2026-07-15: 追加仕様（金刺指示・シート外）としてシーン遷移フロー（トップ→演出→リザルト→「トップへ戻る」ボタンでトップ）を導入。フローコードは `Assets/Scripts/GachaFlow/` の4本（GachaFlowSession=結果・選択テーブルの受け渡し＋**所持石の仮ウォレット**（2026-07-16）／GachaTopSceneFlow=**テーブル数ぶんバナーを動的生成し選択**＋所持石の表示・消費／GachaPlaySceneFlow=選択テーブルで開幕10連＋**スキップボタン購読（SkipReveal→即リザルト）**／GachaResultSceneFlow=一覧表示+「トップへ戻る」ボタンで戻る）に分離し、**課題対象外・参加者は見なくてよい**。GachaController.cs への変更は最小限6点のみ（pullButton null許容／`Pull()`／`Pull(GachaTable)`＝バナー選択用／`RevealFinished` イベント／**タップ送り `WaitForTap()`**／**スキップ用 `SkipReveal()`**。いずれも編集不要領域・**TODO①②と参加者体験は不変**）。※単独版 GachaScene は 2026-07-16 のクリーンアップで削除済み。
※ 2026-07-15（同日・金刺指示）: 演出の進行を**自動送り（Reveal Interval 0.6）→ タップ送り**へ仕様変更。カード表示後 **Tap Guard Delay（0.25秒・連打ガード）** を置いて画面タップで次のカードへ（Input System／旧 Input 両対応の `WasTapThisFrame`）。演出シーン右上に**スキップボタン（スキップ ≫）**＝演出打ち切り→リザルトで全10件を即確認（結果は演出開始前に全件確定しているため打ち切っても安全）。あわせて GachaEffects の再生を **Stop→Play の再生し直し（Restart）** に変更（進行速度がユーザー依存になり、再生中 Play() 無視による演出抜け——固定結果では SSR が1・2枚目に連続——が顕在化するため）。シーンへ反映済み・**実機動作はユーザー確認済み**（反映に使ったツールは 2026-07-16 のクリーンアップで削除）。
※ 2026-07-16（金刺指示）: リザルト画面の戻り方を**「どこでもタップ」→「トップへ戻る」ボタン**へ仕様変更。全画面透明ボタン（BackButton）とヒント（HintText）を撤去し、画面下部中央に可視の **BackToTopButton（「トップへ戻る」）** を配置して `GachaResultSceneFlow.backButton` に配線（フローのロジックは元々ボタン購読式のためコード変更はコメントのみ）。シーンへ反映済み・**実機動作はユーザー確認済み**（反映に使ったツールは 2026-07-16 のクリーンアップで削除）。
※ 2026-07-16（同日・金刺指示）: トップ画面右上に**所持石の表示（GemCountText「所持石 100,000」）**を追加——**仮実装**（再生開始時 `GachaFlowSession.InitialGemCount`=100,000。バナーで引くたびに `TrySpendGems(table.costAmount)` で減少。不足時は引けず警告ログ。ローカル保存なし・`RuntimeInitializeOnLoadMethod(SubsystemRegistration)` で再生毎にリセット＝Enter Play Mode Options のドメインリロード無効でも初期化される）。実装は課題対象外の GachaFlow 側のみ（GachaFlowSession=仮ウォレット／GachaTopSceneFlow=表示更新と消費）。本物の残高管理・消費判定はサーバー責務のまま（サーバー会の「石を引く」課題の伏線）。シーンへ反映済み・**実機動作はユーザー確認済み**（反映に使ったツールは 2026-07-16 のクリーンアップで削除）。
※ 2026-07-16（クリーンアップ・金刺指示）: シーン構築の完了と実機確認を受けて、**運営用エディタツール一式（`Assets/Editor/`）と単独版 GachaScene・SampleScene・空フォルダ等を削除**し、コードコメントから日付・経緯の文言を除去（説明のみ残す）。リポジトリは「ガチャを引く一連の処理」に必要なもの＋ドキュメントのみ。削除物は git 履歴から復元可能。

| ファイル | 内容 | 備考 |
|---|---|---|
| Rarity.cs | `enum Rarity { R, SR, SSR }` | 定義順に依存して `>=` 比較を使う（順序変更禁止） |
| ItemMaster.cs | SO。**1アセットに複数行**（`List<Entry>`: itemId/itemType/itemName/rarity/icon）＋`FindById` | 行の追加削除はUnity標準リストの＋−ボタン。カスタムエディタ不使用 |
| GachaTable.cs | SO。gacha_master+detailの合成（gachaId/gachaName/costItemId/costAmount/startAt/endAt＋entries）| entriesは `{itemId, weight}` の**ID紐づけ**（シートと同構造）。期間は「控え」で判定に使わない。costAmount は 2026-07-16 からトップの**所持石表示（仮実装）の減算に参照**するが、正式な残高管理・判定は引き続き**サーバー責務** |
| GachaApiTypes.cs | IF準拠DTO（GachaDrawInParam/OutParam/Result、[Serializable]） | サーバー会と共有する"契約"。名前・形はIFタブと完全一致させること |
| GachaService.cs | 結果取得の共通IF（`IGachaService`）＋取得元enum（`GachaResultSource`）＋工場（`GachaServiceFactory`、DrawCount=10） | コールバック式（サーバーの非同期に対応）。**2026-07-15 仕様変更で LocalGachaService（重み付き抽選）を置換** |
| FixedGachaService.cs | クライアント会用: **抽選なし・entries の登録順に固定**で10件（登録数超過は先頭へ戻る→9・10件目が isNew=false になり決定的） | isNew=HashSetで同一ドロー内初出判定。コスト・期間・抽選は**サーバー責務** |
| ServerGachaService.cs | サーバー会用: IFどおり `POST {serverUrl}/api/gacha/draw`（UnityWebRequest＋await。タイムアウト10秒） | 失敗時はエラーログ＋空結果（ボタンは再度押せる）。**疎通未確認** |
| GachaEffects.cs | 演出一式: PlayCommon/PlayRare/PlaySpecial()/PlaySpecial(int)/PlayOmen(コルーチン)/URP Bloomフラッシュ | 全てnull-safe。数値はInspector調整。再生は **Stop→Play の Restart 方式**（連続再生でも抜けない。2026-07-15） |
| GachaController.cs | 進行本体。10連を**タップ送り**で1枚ずつめくり（Tap Guard Delay 0.25 の連打ガード付き）、`SkipReveal()` で演出打ち切り可。NEWバッジ、履歴サマリー、連打・未割当ガード。Inspector に **Result Source**（ClientFixed/Server）と Server Url を持ち、取得元を即切替できる | **TODO①**: `PlayResultEffect`内のswitch（現状は全部PlayCommon）／**TODO②**: めくりループ内の予兆if（`item.rarity >= Rarity.SR` で `yield return effects.PlayOmen();`） |

### 引き継ぐべき設計判断

1. **信頼境界**: 抽選・コスト消費・開催期間の「正」はサーバー。**クライアントは抽選そのものを持たない**（2026-07-15仕様変更: FixedGachaService＝登録順の固定結果。weightはマスタに保持するがサーバーだけが使う）。トップの所持石（2026-07-16）は**画面確認用のモック**（保存なし・再生毎リセット）で、この境界を変えるものではない。
2. **IF共通化による継ぎ目**: クライアントDTO＝サーバーDTO（同名同形）。サーバー接続は **GachaController の Inspector「Result Source」を ClientFixed→Server に切り替えるだけ**（ServerGachaService 実装済み・接続先は Server Url 欄。コード変更不要。**疎通未確認**）。
3. **参加者体験の不変条件**: 開始状態=「10連は動くが演出は全カード同じ（Common）」。TODOの難易度（switch1つ＋if1つ）と、ヒントコメント・考えどころ（予兆はSR以上かSSRだけか）を壊す変更はしない。
4. **失敗系**: 割当漏れ・ID不一致・weight合計0はすべて実行時ログで検出可能にしてある（SETUP_GUIDEのトラブルシューティング表と対応）。

## 5. 確定・保留事項

**確定（2026-07-08）**: N廃止（R/SR/SSR）／NEWバッジ・10連履歴UIは当日用意（必須）／コスト・開催期間はクライアント会スコープ外（値は控えとして保持、判定はサーバー責務）／ItemMasterは1アセット複数行方式／マスタアセット置き場は `Assets/MasterData`。

**仕様変更（2026-07-15・金刺指示、シート外）**: ①シーン遷移フロー追加（トップ→演出→リザルト→「トップへ戻る」ボタンでトップ。§2参照）②**クライアント会の結果は重み付き抽選ではなく entries の登録順固定**（抽選はサーバー会で実装）③サーバー切替は GachaController の Inspector「Result Source」で即時に行える設計（IGachaService の継ぎ目＋ServerGachaService 実装済み）④**ガチャテーブルは2つ**（通常/初心者応援）。トップは登録テーブル数に応じたバナー選択式で、選んだテーブル参照で10連実行。**サーバーがテーブル1つのみでも動くよう、選択テーブルの結果を取得できない場合は gachaId=1 で自動再試行**（ServerGachaService の予防動作）⑤**演出はタップ送り＋スキップ**（自動送りを廃止。画面タップで1枚ずつ、右上スキップ ≫ で演出打ち切り→リザルトで全件即確認。§4参照）⑥**リザルトは「トップへ戻る」ボタンで遷移**（2026-07-16。どこでもタップで戻る方式を廃止。§4参照）⑦**トップに所持石の表示（仮実装）**（2026-07-16。再生開始時 100,000 個・バナーで引くたびに選択テーブルの costAmount ぶん減少・ローカル保存なし＝再生毎にリセット・不足時は引けず警告ログ。本物の残高管理・消費判定はサーバー責務のまま。§4参照）。

**保留**: ①~~初心者応援ガチャ（gacha_id=2）のアセット作成~~ → **解決（2026-07-15）**: 生成・配置済み（2009・2010 の weight は暫定8で**シート側の確定待ち**）②サーバーでのコスト・期間の使用有無（**使うことになっても現設計で壊れない**ことは担保済み）③タイムスケジュールの所要分の不整合確認（§1）④**Unityバージョン**: 2026-07-16 に **6000.3.17f1** へアップグレードされ実機動作確認済み（§2参照）。ルート README.md の記載 6000.3.19f1 との**不一致は要確認**。参加者PCは確定した実体版数に統一する。

## 6. 残タスク（Code側の想定作業）

1. **サーバー会一式の改訂（最優先）**: チャット段階の旧ドラフト（`GachaWorkshop_Server`: csproj/.NET8＋Swashbuckle 6.5.0、`/gacha/draw`＋`/gacha/draw10`、レアリティ4段階、固定スライム開始状態、port 5080）が存在するが、**確定IF・マスタと乖離**しているため実質作り直し。要件: `POST /api/gacha/draw` 一本化／マスタ3テーブルをシート値どおり保持／isNew実装／参加者編集は1ファイル・TODO2か所（例: 重み付き抽選＋10連ループ）の穴埋め構成を維持／開始状態は「仮の固定結果」で動くこと／Swaggerで動作確認／発展課題（排出率simulate、R以上確定枠、ユーザーDB=isNew永続化・石消費・期間判定）。配置は `Server/` などAssets外。ドキュメント3点（SETUP_GUIDE/INSTRUCTOR_NOTES/STUDENT_HANDOUT）もサーバー版を確定仕様で用意。
2. ~~Unity実機確認・シーン素材の作り込み~~ → **完了（2026-07-16）**: フロー一式（バナー選択→タップ送り10連＋スキップ→リザルト→トップへ戻るボタン→所持石減少）の実機動作をユーザー確認済み。シーン・素材・フォントも構築済み。
3. 保留事項の確定を受けた反映（本書§5と `Docs/` の更新トリガー参照）。特に **STEP 8 の網羅チェック消化と参加者PC展開（STEP 9）** は当日までに実施。

## 7. 参照ドキュメントの役割

- `Docs/SETUP_GUIDE.md` … 運営向け環境・シーン構築手順（マスタ入力値の完全な表、動作確認チェックリスト、トラブルシューティング付き）
- `Docs/SCENE_BUILD_CHECKLIST.md` … シーン構築・素材準備の詳細手順書（SETUP_GUIDE 手順3〜6 の実地調査ベース詳細化。素材TODO一覧・パーティクル設定値・Inspector割当表・落とし穴表・未決事項）
- `Docs/INSTRUCTOR_NOTES.md` … 当日の進行台本・TODO解答・発展課題（SSRランダム/ガセ予兆/ラス1溜め）・橋渡しトーク・サーバー接続付録
- `Docs/STUDENT_HANDOUT.md` … 参加者配布の課題シート
- いずれも末尾に更新トリガーを記載。仕様変更時は本書と併せて更新すること。
