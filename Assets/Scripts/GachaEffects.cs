using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GachaWorkshop
{
    /// <summary>
    /// 演出をまとめたクラス（中身は用意済み。このファイルは編集しません）。
    /// パーティクルの量・色・大きさや光の強さは、シーン上のオブジェクトと
    /// この Inspector の数値で調整してください。
    /// </summary>
    public class GachaEffects : MonoBehaviour
    {
        [Header("レアリティ別パーティクル")]
        [SerializeField] private ParticleSystem commonFx;   // R 向け（控えめ）
        [SerializeField] private ParticleSystem rareFx;     // SR 向け（中くらい）
        [SerializeField] private ParticleSystem specialFx;  // SSR 向け（派手）

        [Header("SSR演出のバリエーション（メンターから個別に渡す上位課題で使用）")]
        [SerializeField] private ParticleSystem[] specialVariations;

        [Header("予兆（TODO②③で使用）")]
        [SerializeField] private ParticleSystem omenFx;
        [SerializeField] private ParticleSystem[] omenVariations; // TODO③で使用
        [SerializeField] private float omenDuration = 1.2f;

        [Header("光の演出（任意：URP の Volume + Bloom）")]
        [SerializeField] private Volume volume;
        [SerializeField] private float rareBloom = 2f;
        [SerializeField] private float specialBloom = 6f;
        [SerializeField] private float bloomFadeSpeed = 4.5f;

        private Bloom _bloom;
        private float _baseBloom;
        private Coroutine _bloomRoutine;

        private void Awake()
        {
            if (volume != null && volume.profile != null && volume.profile.TryGet(out _bloom))
            {
                _baseBloom = _bloom.intensity.value;
            }
        }

        // ── 控えめな演出（R 向け）──
        public void PlayCommon()
        {
            Restart(commonFx);
        }

        // ── 中くらいの演出（SR 向け）──
        public void PlayRare()
        {
            Restart(rareFx);
            BloomFlash(rareBloom);
        }

        // ── 派手な演出（SSR 向け）──
        public void PlaySpecial()
        {
            Restart(specialFx);
            BloomFlash(specialBloom);
        }

        // ── メンター配布の上位課題用：パターン番号でSSR演出を切り替える ──
        public void PlaySpecial(int pattern)
        {
            if (specialVariations != null && specialVariations.Length > 0)
            {
                int index = Mathf.Clamp(pattern, 0, specialVariations.Length - 1);
                Restart(specialVariations[index]);
            }
            else
            {
                Restart(specialFx);
            }
            BloomFlash(specialBloom);
        }

        // ── 予兆演出：再生して少し待つ。コルーチンから「yield return」で呼びます ──
        public IEnumerator PlayOmen()
        {
            Restart(omenFx);
            yield return new WaitForSeconds(omenDuration);
        }

        // ── TODO③用：パターン番号で予兆の見た目を切り替える ──
        public IEnumerator PlayOmen(int pattern)
        {
            ParticleSystem fx = omenFx;
            if (omenVariations != null && omenVariations.Length > 0)
            {
                int index = Mathf.Clamp(pattern, 0, omenVariations.Length - 1);
                fx = omenVariations[index];
            }
            Restart(fx);
            yield return new WaitForSeconds(omenDuration);
        }

        // ── 先頭から再生し直す（再生中の非ループ ParticleSystem は Play() を無視するため、
        //    タップ送りで同じ演出が連続しても抜けないように一度止めてから再生する）──
        private static void Restart(ParticleSystem fx)
        {
            if (fx == null) return;
            fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fx.Play();
        }

        // ── 一瞬だけ Bloom を強くして元に戻す（光るフラッシュ表現）──
        private void BloomFlash(float intensity)
        {
            if (_bloom == null) return;
            if (_bloomRoutine != null) StopCoroutine(_bloomRoutine);
            _bloomRoutine = StartCoroutine(BloomFlashRoutine(intensity));
        }

        private IEnumerator BloomFlashRoutine(float intensity)
        {
            _bloom.intensity.value = intensity;
            while (_bloom.intensity.value > _baseBloom + 0.01f)
            {
                _bloom.intensity.value =
                    Mathf.Lerp(_bloom.intensity.value, _baseBloom, Time.deltaTime * bloomFadeSpeed);
                yield return null;
            }
            _bloom.intensity.value = _baseBloom;
        }
    }
}
