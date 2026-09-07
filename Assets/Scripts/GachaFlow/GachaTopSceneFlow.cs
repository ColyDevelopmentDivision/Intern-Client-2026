using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GachaWorkshop.Flow
{
    /// <summary>
    /// ガチャトップ画面の進行。設定された GachaTable の数だけバナーを並べ、
    /// 押されたバナーのテーブルで演出シーンの10連が実行されます。
    /// ※ シーン遷移まわりの裏方で、今日の課題とは無関係です。見なくてOK・編集不要。
    /// </summary>
    public class GachaTopSceneFlow : MonoBehaviour
    {
        [Tooltip("開催中のガチャテーブル一覧（登録した数だけバナーが並ぶ）")]
        [SerializeField] private GachaTable[] gachaTables;

        [Tooltip("バナーを並べる場所（Vertical Layout Group 付き）")]
        [SerializeField] private Transform bannerRoot;

        [Tooltip("バナーの雛形（シーン上では非アクティブのまま置いておく）")]
        [SerializeField] private Button bannerTemplate;

        [Tooltip("所持石の表示（仮実装: 再生開始時 100,000 個。引くたびにテーブルの costAmount ぶん減る）")]
        [SerializeField] private TMP_Text gemCountText;

        private bool _isLoading;

        private void Start()
        {
            if (gachaTables == null || gachaTables.Length == 0 || bannerRoot == null || bannerTemplate == null)
            {
                Debug.LogError("[GachaTopSceneFlow] ガチャテーブルまたはバナーUIが未割り当てです。Inspector を確認してください。");
                return;
            }

            // テーブルの数だけバナーを生成（マスタを増やせばバナーも増える）
            foreach (GachaTable table in gachaTables)
            {
                if (table == null) continue;
                Button banner = Instantiate(bannerTemplate, bannerRoot);
                banner.gameObject.SetActive(true);
                TMP_Text label = banner.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = $"{table.gachaName}\n<size=55%>10連ガチャを引く（コスト {table.costAmount} 石）</size>";
                }
                GachaTable captured = table; // クロージャ用にループ変数を固定
                banner.onClick.AddListener(() => OnBannerPressed(captured));
            }

            RefreshGemCount();
        }

        private void OnBannerPressed(GachaTable table)
        {
            if (_isLoading) return; // 連打防止

            // 所持石を消費してから引く（仮実装: 保存しない。本物の残高管理・判定はサーバーの責務）
            if (!GachaFlowSession.TrySpendGems(table.costAmount))
            {
                Debug.LogWarning($"[GachaTopSceneFlow] 石が足りません（所持 {GachaFlowSession.GemCount:N0} ／ 必要 {table.costAmount:N0}）。");
                return;
            }
            RefreshGemCount();

            _isLoading = true;
            GachaFlowSession.SelectedTable = table; // 選択されたテーブルを演出シーンへ引き継ぐ
            SceneManager.LoadScene(GachaFlowSession.PlaySceneName);
        }

        /// <summary>所持石の表示を現在値に合わせる（未割り当てでも動く null 許容）。</summary>
        private void RefreshGemCount()
        {
            if (gemCountText != null)
            {
                gemCountText.text = $"所持石 {GachaFlowSession.GemCount:N0}";
            }
        }
    }
}
