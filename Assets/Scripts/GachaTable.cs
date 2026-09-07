using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaWorkshop
{
    /// <summary>
    /// 1つのガチャの排出テーブル。
    /// gacha_master（ガチャ名・コスト・開催期間など）と gacha_detail_master
    /// （アイテムごとの重み）をまとめた形です。1アセット = 1ガチャ。
    /// クライアント会では「通常ガチャ（gacha_id=1）」を固定で使います。
    /// entries はマスタシートと同じく itemId で ItemMaster の行と紐づけます
    /// （アセットの直接参照ではなく「IDで引く」＝実際のゲームのマスタと同じ構造）。
    /// コスト消費・開催期間の判定はサーバーの責務のため、ここでは値を
    /// 持つだけで判定には使いません（サーバー会・発展で使用予定）。
    /// ※ このファイルは編集しません。値の入力は SETUP_GUIDE.md の表を参照。
    /// </summary>
    [CreateAssetMenu(fileName = "GachaTable_", menuName = "1day Intern/Gacha Table")]
    public class GachaTable : ScriptableObject
    {
        [Tooltip("gacha_id（例: 1 = 通常ガチャ）")]
        public int gachaId = 1;

        [Tooltip("ガチャ名（例: 通常ガチャ）")]
        public string gachaName = "通常ガチャ";

        [Header("gacha_master のその他の列（クライアント会では未使用・控え）")]
        [Tooltip("cost_item_id（例: 1001 = 石）。消費・検証はサーバーの責務で、クライアントでは判定に使いません")]
        public int costItemId = 1001;

        [Tooltip("cost_amount（例: 300）。同上、クライアントでは判定に使いません")]
        public int costAmount = 300;

        [Tooltip("start_at（開催開始）。期間判定はサーバーの責務。表示用の控え")]
        public string startAt = "2026-01-01 00:00:00";

        [Tooltip("end_at（開催終了）。期間判定はサーバーの責務。表示用の控え")]
        public string endAt = "2099-12-31 23:59:59";

        /// <summary>gacha_detail_master の1行分（itemId と weight のID紐づけ）。</summary>
        [Serializable]
        public class Entry
        {
            [Tooltip("排出されるアイテムの item_id（ItemMaster の行とIDで紐づく）")]
            public int itemId;

            [Tooltip("weight：出やすさの比率（％ではない）。抽選で使うのはサーバーの責務で、クライアント会では使いません（登録順の固定結果で動く）")]
            public int weight = 1;
        }

        [Tooltip("gacha_detail_master にあたる排出リスト")]
        public List<Entry> entries = new List<Entry>();
    }
}
