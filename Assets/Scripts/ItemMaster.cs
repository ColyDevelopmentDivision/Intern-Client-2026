using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaWorkshop
{
    /// <summary>
    /// item_master（マスタシート）にあたるデータ。シートと同じく
    /// 1アセットの中に複数行（今回は8行）を持ちます。
    /// 行の追加・削除は Inspector のリスト（＋/−ボタン）で行います。
    /// Project ウィンドウで右クリック → Create > 1day Intern > Item Master から作成できます。
    /// ※ このファイルは編集しません。値の入力は SETUP_GUIDE.md の表を参照。
    /// </summary>
    [CreateAssetMenu(fileName = "ItemMaster", menuName = "1day Intern/Item Master")]
    public class ItemMaster : ScriptableObject
    {
        /// <summary>item_master の1行分。</summary>
        [Serializable]
        public class Entry
        {
            [Tooltip("item_id（例: 2001）")]
            public int itemId;

            [Tooltip("item_type（今回はすべて character）")]
            public string itemType = "character";

            [Tooltip("name（画面に表示する名前）")]
            public string itemName;

            [Tooltip("rarity（R / SR / SSR）")]
            public Rarity rarity;

            [Tooltip("結果画面に表示する画像（単色スクエア等でOK）")]
            public Sprite icon;
        }

        [Tooltip("item_master の行一覧（マスタシートと同じ8行を登録）")]
        public List<Entry> entries = new List<Entry>();

        /// <summary>itemId から行を引きます（見つからなければ null）。</summary>
        public Entry FindById(int itemId)
        {
            foreach (var e in entries)
            {
                if (e != null && e.itemId == itemId)
                {
                    return e;
                }
            }
            return null;
        }
    }
}
