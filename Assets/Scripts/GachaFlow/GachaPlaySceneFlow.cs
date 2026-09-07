using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GachaWorkshop.Flow
{
    /// <summary>
    /// ガチャ演出シーンの進行。シーンが開いたら自動で10連を開始し、
    /// カードは画面タップで1枚ずつ進む。演出がすべて終わったら結果を持ってリザルトシーンへ。
    /// スキップボタンが押されたら演出を打ち切り、すぐリザルトシーンで全結果を見せる。
    /// ※ シーン遷移まわりの裏方で、今日の課題とは無関係です。見なくてOK・編集不要。
    ///   （課題の GachaController はこのシーンに置かれたまま、従来どおり動きます）
    /// </summary>
    public class GachaPlaySceneFlow : MonoBehaviour
    {
        [SerializeField] private GachaController controller;

        [Tooltip("演出を打ち切ってリザルトへ進むスキップボタン")]
        [SerializeField] private Button skipButton;

        [Tooltip("最後のカードのタップからリザルトへ進むまでの間（スキップ時は待たずに遷移）")]
        [SerializeField] private float transitionDelay = 0.35f;

        private bool _skipRequested;

        private void Start()
        {
            if (controller == null)
            {
                Debug.LogError("[GachaPlaySceneFlow] GachaController が未割り当てです。");
                return;
            }
            controller.RevealFinished += OnRevealFinished;
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButton);
            }
            StartCoroutine(PullRoutine());
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.RevealFinished -= OnRevealFinished;
            }
        }

        private IEnumerator PullRoutine()
        {
            yield return null; // 各コンポーネントの Start() が済んでから開始する
            // トップ画面のバナーで選ばれたテーブルで1回引く（未選択＝このシーンを直接再生した場合は
            // GachaController に割り当て済みのテーブルが使われる）
            controller.Pull(GachaFlowSession.SelectedTable);
        }

        private void OnSkipButton()
        {
            // 演出を打ち切って結果を確定させる（成功すると RevealFinished が即時に呼ばれる）。
            // まだ演出が始まっていない間（サーバー応答待ちなど）は何もしない
            _skipRequested = true;
            if (!controller.SkipReveal())
            {
                _skipRequested = false;
            }
        }

        private void OnRevealFinished(GachaDrawOutParam response)
        {
            GachaFlowSession.LastResult = response;
            StartCoroutine(LoadResultRoutine(_skipRequested ? 0f : transitionDelay));
        }

        private IEnumerator LoadResultRoutine(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            SceneManager.LoadScene(GachaFlowSession.ResultSceneName);
        }
    }
}
