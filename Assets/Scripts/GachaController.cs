using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GachaWorkshop
{
    /// <summary>
    /// ガチャの流れを管理するクラス。
    /// ★ 参加者が手を入れるのは「TODO」と書かれた 3 か所だけです。
    ///   それ以外は用意済みなので、まずはそのまま動かしてみましょう。
    ///
    /// 流れ：ボタン → 10連の結果を受け取る（今日は「登録順の固定結果」）
    ///       → 1枚めくって演出 → 画面タップで次のカードへ（10枚目まで）
    ///       ※ スキップボタン（シーン遷移版）で演出を打ち切って結果画面へ飛べます
    /// ※ 「何が出るか」を決める抽選は、このアプリの中ではしていません。本来それは
    ///   サーバーの仕事です。午後のサーバー会で本物の抽選を作り、Inspector の
    ///   Result Source を Server に切り替えると、この画面がサーバーの結果で動きます。
    /// </summary>
    public class GachaController : MonoBehaviour
    {
        [Header("UI（必須）")]
        [SerializeField] private Button pullButton;
        [SerializeField] private Image resultIcon;
        [SerializeField] private TMP_Text resultText;

        [Header("UI（NEWバッジ・10連履歴：当日使用。未設定でも動く安全設計）")]
        [SerializeField] private GameObject newBadge;        // isNew のとき表示する「NEW」
        [SerializeField] private Transform summaryRoot;      // 10連の履歴を並べる場所
        [SerializeField] private Image summarySlotPrefab;    // 履歴用の小さい画像プレハブ

        [Header("マスタ（ItemMaster と通常ガチャのテーブルを割り当て）")]
        [SerializeField] private ItemMaster itemMaster;
        [SerializeField] private GachaTable gachaTable;

        [Header("演出（用意済み）")]
        [SerializeField] private GachaEffects effects;

        [Header("結果の取得元（用意済み。午後のサーバー会で Server に切り替える）")]
        [Tooltip("ClientFixed=テーブルの登録順の固定結果（今日のクライアント会）／Server=サーバーAPIから取得")]
        [SerializeField] private GachaResultSource resultSource = GachaResultSource.ClientFixed;

        [Tooltip("Result Source が Server のときの接続先（サーバー会の構成に合わせる）")]
        [SerializeField] private string serverUrl = "http://localhost:5109";

        [Tooltip("Result Source が Server のときに送る userId。0=匿名（所持品なし扱い）。動作確認用にInspectorで切り替え可能。サーバー追加課題Aでガチャを引くユーザーを切り替えたいときに使用してください。")]
        [SerializeField] private long userId = 0;

        [Header("タイミング調整")]
        [SerializeField] private float firstDelay = 0.4f;     // 引いてから1枚目までの間
        [SerializeField] private float tapGuardDelay = 0.25f; // カード表示後にタップを受け付けるまでの間（連打で飛びすぎるのを防ぐ）

        private bool _isPulling;
        private Coroutine _revealCoroutine;          // スキップで止めるために保持（編集不要）
        private GachaDrawOutParam _revealingResponse; // スキップ時に結果を確定させるために保持（編集不要）

        private void Start()
        {
            ClearResultViews();
            if (pullButton != null)
            {
                // ボタンが無いシーン（シーン遷移版の演出シーン）では、外部から Pull() で開始します
                pullButton.onClick.AddListener(OnPullButton);
            }
        }

        private void OnPullButton()
        {
            if (_isPulling) return; // 演出中・結果待ち中の連打を防ぐ

            if (itemMaster == null)
            {
                Debug.LogError("[GachaController] ItemMaster が未割り当てです。Inspector を確認してください。");
                return;
            }

            _isPulling = true; // 結果が届く前の連打も防ぐ（サーバー接続時は少し時間がかかるため）

            // 10連の結果をまとめて受け取る（IFと同じ形。取得元は Inspector の Result Source。用意済み・編集不要）
            IGachaService service = GachaServiceFactory.Create(resultSource, serverUrl);
            service.Draw(gachaTable, userId, OnDrawCompleted);
        }

        /// <summary>10連の結果が届いたら演出を開始します（用意済み・編集不要）。</summary>
        private void OnDrawCompleted(GachaDrawOutParam response)
        {
            if (response == null || response.results.Count == 0)
            {
                _isPulling = false; // 失敗時はもう一度押せるようにする
                return;
            }
            _revealingResponse = response;
            _revealCoroutine = StartCoroutine(RevealRoutine(response));
        }

        // ──────────── シーン遷移版のフロー用（用意済み・編集不要）────────────

        /// <summary>10連の演出がすべて終わったときに結果を通知します（シーン遷移版が購読。編集不要）。</summary>
        public event System.Action<GachaDrawOutParam> RevealFinished;

        /// <summary>ボタン以外（シーン遷移版のフローなど）から10連を開始します（編集不要）。</summary>
        public void Pull()
        {
            OnPullButton();
        }

        /// <summary>
        /// テーブルを指定して10連を開始します（トップ画面のバナー選択用。編集不要）。
        /// null のときは Inspector に割り当て済みのテーブルをそのまま使います。
        /// </summary>
        public void Pull(GachaTable table)
        {
            if (table != null)
            {
                gachaTable = table; // バナーで選ばれたテーブルに差し替える
            }
            OnPullButton();
        }

        /// <summary>
        /// 演出を途中で打ち切り、結果を確定として通知します（スキップボタン用。編集不要）。
        /// 結果の10件は演出開始前にすべて届いているので、途中で止めてもそのまま確定できます。
        /// 演出中でなければ何もせず false を返します。
        /// </summary>
        public bool SkipReveal()
        {
            if (_revealCoroutine == null) return false;
            StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
            _isPulling = false;
            GachaDrawOutParam response = _revealingResponse;
            _revealingResponse = null;
            RevealFinished?.Invoke(response);
            return true;
        }

        /// <summary>
        /// 10連の結果を1枚ずつめくって表示します。
        /// </summary>
        private IEnumerator RevealRoutine(GachaDrawOutParam response)
        {
            _isPulling = true;
            ClearResultViews();
            yield return new WaitForSeconds(firstDelay);

            bool isFirstCard = true;
            foreach (GachaDrawResult result in response.results)
            {
                // 結果は itemId だけなので、表示に使う名前・画像はマスタ（ItemMaster）から引く
                ItemMaster.Entry item = itemMaster.FindById(result.itemId);
                if (item == null)
                {
                    Debug.LogError($"[GachaController] itemId={result.itemId} が ItemMaster に見つかりません。");
                    continue;
                }

                if (!isFirstCard)
                {
                    HideCurrentCard(); // 前のカードを消してから次の演出へ（重なり表示防止・編集不要）
                }
                isFirstCard = false;

                // ───────────────────────────────────────────────
                // TODO ②：レアが出る直前の「予兆」で“溜め”を作ろう。（書く場所はここ）
                //───────────────────────────────────────────────
                if (item.rarity>= Rarity.SR){
                    int patern = Random.Range(0, 3);
                    yield return effects.PlayOmen(patern);
                }

                // TODO ③（TODO②ができたら）：予兆の色をランダムにしよう。
                //   （TODO②で書いた処理を書き換える）
                //   問題文は課題シート（Docs/EXERCISES.md）、
                //   詰まったらヒント集（Docs/HINTS.md）を見てください。
                // ───────────────────────────────────────────────

                ShowCard(item, result.isNew);   // カードを表示（用意済み）
                PlayResultEffect(item.rarity);  // レアリティに合わせた演出（TODO①はこの中）
                AddSummaryIcon(item);           // 履歴に小さく追加（用意済み）

                yield return WaitForTap();      // 画面タップで次のカードへ（編集不要）
            }

            _revealCoroutine = null;
            _revealingResponse = null;
            _isPulling = false;
            RevealFinished?.Invoke(response); // シーン遷移版へ「演出が終わった」を通知（編集不要）
        }

        /// <summary>次のカードへ進むタップ（クリック）を待ちます（編集不要）。</summary>
        private IEnumerator WaitForTap()
        {
            yield return new WaitForSeconds(tapGuardDelay); // 表示直後の連打で飛びすぎるのを防ぐ小休止
            while (!WasTapThisFrame())
            {
                yield return null;
            }
        }

        /// <summary>このフレームに画面のどこかがタップ（クリック）されたか（編集不要）。</summary>
        private static bool WasTapThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Pointer pointer = UnityEngine.InputSystem.Pointer.current;
            return pointer != null && pointer.press.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        /// <summary>
        /// レアリティに合わせて演出を出し分けます。
        /// </summary>
        private void PlayResultEffect(Rarity rarity)
        {
            // ───────────────────────────────────────────────
            // TODO ①：レアリティごとに演出を出し分けよう。
            //   問題文は課題シート（Docs/EXERCISES.md）、
            //   詰まったらヒント集（Docs/HINTS.md）を見てください。
            // ───────────────────────────────────────────────
            switch(rarity){
                case Rarity.SSR:
                    effects.PlaySpecial();
                    break;

                case Rarity.SR:
                    effects.PlayRare();
                    break;

                case Rarity.R:
                    effects.PlayCommon();
                    break;
            }

            //effects.PlayCommon(); // ← いまは全部これで演出を発生させている状態。まずはここを書き換えよう
        }

        // ──────────── ここから下は表示まわり（用意済み・編集不要）────────────

        private void ShowCard(ItemMaster.Entry item, bool isNew)
        {
            if (resultIcon != null)
            {
                resultIcon.sprite = item.icon;
                resultIcon.enabled = (item.icon != null);
            }
            if (resultText != null)
            {
                resultText.text = $"{item.itemName}（{item.rarity}）";
            }
            if (newBadge != null)
            {
                newBadge.SetActive(isNew);
            }
        }

        private void AddSummaryIcon(ItemMaster.Entry item)
        {
            if (summaryRoot == null || summarySlotPrefab == null) return;
            Image slot = Instantiate(summarySlotPrefab, summaryRoot);
            slot.sprite = item.icon;
            slot.enabled = (item.icon != null);
        }

        private void ClearResultViews()
        {
            HideCurrentCard();
            if (summaryRoot != null)
            {
                for (int i = summaryRoot.childCount - 1; i >= 0; i--)
                {
                    Destroy(summaryRoot.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>表示中のカード（アイコン・名前・NEW）だけを消します。履歴は残します。</summary>
        private void HideCurrentCard()
        {
            if (resultIcon != null) resultIcon.enabled = false;
            if (resultText != null) resultText.text = string.Empty;
            if (newBadge != null) newBadge.SetActive(false);
        }
    }
}
