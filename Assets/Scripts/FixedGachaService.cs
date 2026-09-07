using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaWorkshop
{
    /// <summary>
    /// クライアント会用の結果サービス。**抽選はしません**。
    /// GachaTable の entries に登録されている**上から順**に固定で返します
    /// （10連に対して登録が足りない分は先頭に戻る。8件登録なら9・10枚目は
    /// 1・2件目の再登場になり、NEW が付かない例も毎回確認できます）。
    /// 「何が出るか」を決める本物の抽選はサーバーの仕事——午後のサーバー会で作り、
    /// GachaController の Result Source を Server に切り替えるとそちらが使われます。
    /// 返す形（GachaDrawOutParam）はIFと同じです。
    /// userId はサーバー用の情報のため、こちらでは使用しません。
    /// </summary>
    public class FixedGachaService : IGachaService
    {
        public void Draw(GachaTable table, long userId, Action<GachaDrawOutParam> onCompleted)
        {
            var response = new GachaDrawOutParam();

            // 登録順の itemId リストを作る（空行はスキップ）
            var itemIds = new List<int>();
            if (table != null && table.entries != null)
            {
                foreach (var e in table.entries)
                {
                    if (e != null) itemIds.Add(e.itemId);
                }
            }
            if (itemIds.Count == 0)
            {
                Debug.LogError("[FixedGachaService] GachaTable が未設定、または entries が空です。");
                onCompleted?.Invoke(response);
                return;
            }

            // isNew 判定用：この1回のガチャの中で既に出た itemId を覚えておく
            var seenInThisDraw = new HashSet<int>();

            for (int i = 0; i < GachaServiceFactory.DrawCount; i++)
            {
                int id = itemIds[i % itemIds.Count]; // 上から順。登録数を超えたら先頭に戻る
                bool isNew = seenInThisDraw.Add(id); // 初出なら true
                response.results.Add(new GachaDrawResult { itemId = id, isNew = isNew });
            }

            onCompleted?.Invoke(response);
        }
    }
}
