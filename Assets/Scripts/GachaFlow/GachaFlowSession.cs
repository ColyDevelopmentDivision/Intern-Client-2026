using UnityEngine;

namespace GachaWorkshop.Flow
{
    /// <summary>
    /// シーンをまたいでガチャ結果を受け渡すための置き場。
    /// ※ シーン遷移まわりの裏方で、今日の課題とは無関係です。このフォルダのファイルは見なくてOK・編集不要。
    /// 本物のゲームではサーバーやセーブデータが担う部分の最小代用です。
    /// </summary>
    public static class GachaFlowSession
    {
        public const string TopSceneName = "GachaTopScene";
        public const string PlaySceneName = "GachaPlayScene";
        public const string ResultSceneName = "GachaResultScene";

        /// <summary>直近の10連結果（演出シーンが書き込み、リザルトシーンが読む）。</summary>
        public static GachaDrawOutParam LastResult;

        /// <summary>トップ画面のバナーで選ばれたガチャテーブル（演出シーンが読む。未選択なら null）。</summary>
        public static GachaTable SelectedTable;

        // ── 所持石（仮実装）─────────────────────────
        // ローカル保存はせず、再生を開始するたびに初期値へ戻る。
        // 本物の残高管理・消費判定はサーバーの責務（サーバー会の題材）で、ここは画面確認用のモック。

        /// <summary>再生開始時の所持石の初期値。</summary>
        public const int InitialGemCount = 100000;

        /// <summary>現在の所持石。ガチャを引くたびにテーブルの costAmount ぶん減る（保存しない仮実装）。</summary>
        public static int GemCount = InitialGemCount;

        /// <summary>所持石を消費する。足りない（または不正な量の）場合は何もせず false を返す。</summary>
        public static bool TrySpendGems(int amount)
        {
            if (amount < 0 || GemCount < amount) return false;
            GemCount -= amount;
            return true;
        }

        /// <summary>
        /// 再生開始のたびにセッション状態を初期化する
        /// （Enter Play Mode Options でドメインリロードを切っていても確実に初期値へ戻すため）。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            GemCount = InitialGemCount;
            LastResult = null;
            SelectedTable = null;
        }
    }
}
