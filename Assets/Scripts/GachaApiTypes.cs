using System;
using System.Collections.Generic;

namespace GachaWorkshop
{
    // ─────────────────────────────────────────────────────────────
    // IF（インターフェース）定義：POST /api/gacha/draw
    // IFシートの定義と同じ名前・同じ形にしてあります。
    // 午後のサーバー会で作るAPIと、この「契約」を共有します。
    // [Serializable] を付けてあるので JsonUtility でそのままJSON化できます。
    // ─────────────────────────────────────────────────────────────

    /// <summary>リクエスト：どのガチャを引くか、誰が引くか。</summary>
    [Serializable]
    public class GachaDrawInParam
    {
        public int gachaId;
        public long userId;
    }

    /// <summary>レスポンス：10連の結果一覧。</summary>
    [Serializable]
    public class GachaDrawOutParam
    {
        public List<GachaDrawResult> results = new List<GachaDrawResult>();
    }

    /// <summary>
    /// 排出結果の1件分。
    /// isNew は「この1回のガチャの中で初めて出たアイテムなら true、
    /// 同じ10連の中で2回目以降に出たら false」（IFシートの注記どおり。
    /// ユーザーDBを実装する発展課題まで進むと、過去の排出も反映されるようになる）。
    /// </summary>
    [Serializable]
    public class GachaDrawResult
    {
        public int itemId;
        public bool isNew;
    }
}
