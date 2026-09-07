using System;

namespace GachaWorkshop
{
    /// <summary>
    /// 10連の結果をどこから取ってくるか。GachaController の Inspector で切り替えます。
    /// </summary>
    public enum GachaResultSource
    {
        ClientFixed, // クライアント内の固定結果（今日のクライアント会はこちら）
        Server,      // サーバーAPI POST /api/gacha/draw（午後のサーバー会で切り替える）
    }

    /// <summary>
    /// 「10連の結果を取ってくる係」の共通の形（インターフェース）。
    /// 取得元がクライアント内の固定結果でも本物のサーバーでも、
    /// GachaController からは同じ呼び方になります。
    /// </summary>
    public interface IGachaService
    {
        /// <summary>10連の結果を取得し、届いたら onCompleted に渡します（失敗時は空の結果）。</summary>
        void Draw(GachaTable table, long userId, Action<GachaDrawOutParam> onCompleted);
    }

    /// <summary>Result Source の設定から実装を選ぶ係。</summary>
    public static class GachaServiceFactory
    {
        /// <summary>1回のガチャで引く枚数（IFどおり10連固定）。</summary>
        public const int DrawCount = 10;

        /// <summary>
        /// 一つ目のテーブル（gacha_master の1行目＝通常ガチャ）の gacha_id。
        /// サーバーに選択中のテーブルが無い場合、ServerGachaService はこのIDで再試行します。
        /// </summary>
        public const int PrimaryGachaId = 1;

        public static IGachaService Create(GachaResultSource source, string serverUrl)
        {
            switch (source)
            {
                case GachaResultSource.Server:
                    return new ServerGachaService(serverUrl);
                default:
                    return new FixedGachaService();
            }
        }
    }
}
