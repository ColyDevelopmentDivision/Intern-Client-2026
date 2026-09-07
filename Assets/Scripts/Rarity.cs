namespace GachaWorkshop
{
    /// <summary>
    /// レアリティ。item_master（マスタシート）に合わせて R / SR / SSR の3段階です。
    /// R → SR → SSR の順で定義しているため、「rarity >= Rarity.SR」のような
    /// 大小比較ができます（順番を入れ替えると比較が壊れるので注意）。
    /// ※ このファイルは編集しません。
    /// </summary>
    public enum Rarity
    {
        R,   // レア（いちばん出やすい）
        SR,  // スーパーレア
        SSR  // スーパースペシャルレア（いちばん貴重）
    }
}
