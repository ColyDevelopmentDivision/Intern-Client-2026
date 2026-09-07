using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GachaWorkshop.Flow
{
    /// <summary>
    /// リザルト画面の進行。10連の結果を一覧表示し、「トップへ戻る」ボタンでトップへ戻ります。
    /// ※ シーン遷移まわりの裏方で、今日の課題とは無関係です。見なくてOK・編集不要。
    /// </summary>
    public class GachaResultSceneFlow : MonoBehaviour
    {
        [SerializeField] private ItemMaster itemMaster;

        [Tooltip("結果1件分のアイコン（SummarySlot プレハブを流用）")]
        [SerializeField] private Image slotPrefab;

        [Tooltip("アイコンを並べる場所（Grid Layout Group 付き）")]
        [SerializeField] private Transform gridRoot;

        [Tooltip("トップへ戻るボタン（押すとトップシーンへ遷移）")]
        [SerializeField] private Button backButton;

        private bool _isLoading;

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }

            var result = GachaFlowSession.LastResult;
            if (result == null || itemMaster == null || slotPrefab == null || gridRoot == null)
            {
                // このシーンを単独で開いた場合など。空のまま表示し、ボタンで戻れる状態にはしておく
                Debug.LogWarning("[GachaResultSceneFlow] 表示する結果データまたは参照がありません（トップから通しで実行すると表示されます）。");
                return;
            }

            foreach (GachaDrawResult r in result.results)
            {
                ItemMaster.Entry entry = itemMaster.FindById(r.itemId);
                Image slot = Instantiate(slotPrefab, gridRoot);
                slot.sprite = (entry != null) ? entry.icon : null;
                slot.enabled = (slot.sprite != null);
                if (r.isNew)
                {
                    AddNewBadge(slot.transform);
                }
            }
        }

        /// <summary>スロットの右上に小さな「NEW」を付けます。</summary>
        private void AddNewBadge(Transform slot)
        {
            var go = new GameObject("NewBadge", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(slot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(8f, 12f);
            rect.sizeDelta = new Vector2(64f, 28f);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = "NEW";
            text.fontSize = 22f;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color32(0xE5, 0x3E, 0x3E, 0xFF);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }

        private void OnBack()
        {
            if (_isLoading) return; // 連打防止
            _isLoading = true;
            GachaFlowSession.LastResult = null; // 次の周回に持ち越さない
            SceneManager.LoadScene(GachaFlowSession.TopSceneName);
        }
    }
}
