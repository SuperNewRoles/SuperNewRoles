using UnityEngine;
using UnityEngine.Audio;

namespace SuperNewRoles.Modules;

/// <summary>
/// 距離による減衰を自動的に処理するAudioSourceコンポーネント
/// 3D音源モードでも確実に聞こえるように、手動で音量を更新します
/// </summary>
public class AttenuatedAudioSource : MonoBehaviour
{
    /// <summary>
    /// 減衰設定のパラメータ
    /// </summary>
    public struct AttenuationSettings
    {
        public float maxVolume;
        public float minDistance;
        public float maxDistance;
        public AudioRolloffMode rolloffMode;
        public bool useRaycastAttenuation;
        public float hitModifier;
        public LayerMask raycastLayerMask;
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        public static AttenuationSettings Default => new()
        {
            maxVolume = 1f,
            minDistance = 1f,
            maxDistance = 5f,
            rolloffMode = AudioRolloffMode.Linear,
            useRaycastAttenuation = false,
            hitModifier = 0.25f,
            raycastLayerMask = Constants.ShipOnlyMask,
            mixerGroup = null
        };
    }

    private AudioSource audioSource;
    private Transform sourceTransform;

    /// <summary>最大音量（距離が最小距離以下の場合の音量）</summary>
    public float maxVolume = 1f;

    /// <summary>最小距離（この距離以下では最大音量）</summary>
    public float minDistance = 1f;

    /// <summary>最大距離（この距離以上では音量0）</summary>
    public float maxDistance = 5f;

    /// <summary>減衰カーブの種類</summary>
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    /// <summary>レイキャストによる壁の減衰を考慮するか</summary>
    public bool useRaycastAttenuation = false;

    /// <summary>壁による減衰の係数（レイキャストが当たった場合の減衰量）</summary>
    public float hitModifier = 0.25f;

    /// <summary>レイキャスト用のレイヤーマスク</summary>
    public LayerMask raycastLayerMask = Constants.ShipOnlyMask;

    private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

    /// <summary>
    /// 内部で使用するAudioSourceを設定します
    /// </summary>
    public void SetupAudioSource(AudioSource source)
    {
        audioSource = source;
        sourceTransform = transform;
    }

    /// <summary>
    /// AudioSourceを自動的に作成して設定します
    /// </summary>
    public AudioSource CreateAudioSource(AudioClip clip, bool loop = false, UnityEngine.Audio.AudioMixerGroup mixerGroup = null)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = maxVolume;
        audioSource.outputAudioMixerGroup = mixerGroup ?? SoundManager.Instance.SfxChannel;
        audioSource.spatialBlend = 0f; // 3D音源にするとバグるので2D音源に指定
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;
        audioSource.playOnAwake = false;

        sourceTransform = transform;
        return audioSource;
    }

    /// <summary>
    /// 現在の距離に基づいて音量を更新します
    /// </summary>
    public void UpdateVolume()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;

        if (PlayerControl.LocalPlayer == null)
        {
            audioSource.volume = 0f;
            return;
        }

        Vector2 soundPosition = sourceTransform.position;
        Vector2 listenerPosition = PlayerControl.LocalPlayer.GetTruePosition();
        float distance = Vector2.Distance(soundPosition, listenerPosition);

        // 最大距離を超えている場合は音量0
        if (distance > maxDistance)
        {
            audioSource.volume = 0f;
            return;
        }

        // 距離による減衰を計算
        float volume = CalculateDistanceVolume(distance);

        // レイキャストによる減衰を考慮
        if (useRaycastAttenuation)
        {
            volume = ApplyRaycastAttenuation(soundPosition, listenerPosition, distance, volume);
        }

        audioSource.volume = volume * maxVolume;
    }

    /// <summary>
    /// 距離による減衰を計算します
    /// </summary>
    private float CalculateDistanceVolume(float distance)
    {
        if (distance <= minDistance)
        {
            return 1f; // 最小距離以下は最大音量
        }

        if (distance >= maxDistance)
        {
            return 0f; // 最大距離以上は音量0
        }

        // 減衰カーブに応じて計算
        float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);

        switch (rolloffMode)
        {
            case AudioRolloffMode.Linear:
                return 1f - normalizedDistance;

            case AudioRolloffMode.Logarithmic:
                // 対数的な減衰（より滑らか）
                return 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);

            case AudioRolloffMode.Custom:
                // カスタムカーブ（デフォルトは線形）
                return 1f - normalizedDistance;

            default:
                return 1f - normalizedDistance;
        }
    }

    /// <summary>
    /// レイキャストによる壁の減衰を適用します
    /// </summary>
    private float ApplyRaycastAttenuation(Vector2 soundPosition, Vector2 listenerPosition, float distance, float baseVolume)
    {
        Vector2 direction = listenerPosition - soundPosition;
        ContactFilter2D contactFilter = default(ContactFilter2D);
        contactFilter.useTriggers = false;
        contactFilter.layerMask = raycastLayerMask;
        contactFilter.useLayerMask = true;

        int hitCount = Physics2D.Raycast(soundPosition, direction, contactFilter, volumeBuffer, distance);
        float attenuation = baseVolume - (hitCount * hitModifier);
        return Mathf.Clamp01(attenuation);
    }

    private void Update()
    {
        UpdateVolume();
    }

    /// <summary>
    /// 手動で音量を更新する場合に使用（Updateを使わない場合）
    /// </summary>
    public void ManualUpdate()
    {
        UpdateVolume();
    }
}

/// <summary>
/// 減衰対応オーディオソースのセットアップ用staticユーティリティ
/// </summary>
public static class AttenuatedAudioSourceUtility
{
    /// <summary>
    /// GameObjectに減衰対応のAudioSourceを設定します
    /// </summary>
    /// <param name="gameObject">AudioSourceを追加するGameObject</param>
    /// <param name="clip">再生するAudioClip</param>
    /// <param name="loop">ループ再生するか</param>
    /// <param name="settings">減衰設定（nullの場合はデフォルト設定を使用）</param>
    /// <returns>設定されたAttenuatedAudioSourceコンポーネント</returns>
    public static AttenuatedAudioSource Setup(GameObject gameObject, AudioClip clip, bool loop = false, AttenuatedAudioSource.AttenuationSettings? settings = null)
    {
        if (gameObject == null)
            throw new System.ArgumentNullException(nameof(gameObject));
        if (clip == null)
            throw new System.ArgumentNullException(nameof(clip));

        var attenuationSettings = settings ?? AttenuatedAudioSource.AttenuationSettings.Default;
        var mixerGroup = attenuationSettings.mixerGroup ?? SoundManager.Instance.SfxChannel;

        // AttenuatedAudioSourceコンポーネントを追加
        var attenuatedAudio = gameObject.AddComponent<AttenuatedAudioSource>();
        attenuatedAudio.maxVolume = attenuationSettings.maxVolume;
        attenuatedAudio.minDistance = attenuationSettings.minDistance;
        attenuatedAudio.maxDistance = attenuationSettings.maxDistance;
        attenuatedAudio.rolloffMode = attenuationSettings.rolloffMode;
        attenuatedAudio.useRaycastAttenuation = attenuationSettings.useRaycastAttenuation;
        attenuatedAudio.hitModifier = attenuationSettings.hitModifier;
        attenuatedAudio.raycastLayerMask = attenuationSettings.raycastLayerMask;

        // AudioSourceを作成して設定
        attenuatedAudio.CreateAudioSource(clip, loop, mixerGroup);

        return attenuatedAudio;
    }

    /// <summary>
    /// GameObjectに減衰対応のAudioSourceを設定します（簡易版）
    /// </summary>
    /// <param name="gameObject">AudioSourceを追加するGameObject</param>
    /// <param name="clip">再生するAudioClip</param>
    /// <param name="loop">ループ再生するか</param>
    /// <param name="maxDistance">最大距離</param>
    /// <param name="minDistance">最小距離</param>
    /// <returns>設定されたAttenuatedAudioSourceコンポーネント</returns>
    public static AttenuatedAudioSource SetupSimple(GameObject gameObject, AudioClip clip, bool loop = false, float maxDistance = 5f, float minDistance = 1f)
    {
        var settings = AttenuatedAudioSource.AttenuationSettings.Default;
        settings.maxDistance = maxDistance;
        settings.minDistance = minDistance;
        return Setup(gameObject, clip, loop, settings);
    }

    /// <summary>
    /// 既存のAudioSourceに減衰対応を追加します
    /// </summary>
    /// <param name="gameObject">AudioSourceがアタッチされているGameObject</param>
    /// <param name="audioSource">既存のAudioSource</param>
    /// <param name="settings">減衰設定（nullの場合はデフォルト設定を使用）</param>
    /// <returns>設定されたAttenuatedAudioSourceコンポーネント</returns>
    public static AttenuatedAudioSource SetupExisting(GameObject gameObject, AudioSource audioSource, AttenuatedAudioSource.AttenuationSettings? settings = null)
    {
        if (gameObject == null)
            throw new System.ArgumentNullException(nameof(gameObject));
        if (audioSource == null)
            throw new System.ArgumentNullException(nameof(audioSource));

        var attenuationSettings = settings ?? AttenuatedAudioSource.AttenuationSettings.Default;

        // AttenuatedAudioSourceコンポーネントを追加
        var attenuatedAudio = gameObject.AddComponent<AttenuatedAudioSource>();
        attenuatedAudio.maxVolume = attenuationSettings.maxVolume;
        attenuatedAudio.minDistance = attenuationSettings.minDistance;
        attenuatedAudio.maxDistance = attenuationSettings.maxDistance;
        attenuatedAudio.rolloffMode = attenuationSettings.rolloffMode;
        attenuatedAudio.useRaycastAttenuation = attenuationSettings.useRaycastAttenuation;
        attenuatedAudio.hitModifier = attenuationSettings.hitModifier;
        attenuatedAudio.raycastLayerMask = attenuationSettings.raycastLayerMask;

        // 既存のAudioSourceを設定
        attenuatedAudio.SetupAudioSource(audioSource);

        // AudioSourceの設定を更新
        audioSource.spatialBlend = 1f; // 3D音源にする
        audioSource.minDistance = attenuationSettings.minDistance;
        audioSource.maxDistance = attenuationSettings.maxDistance;
        audioSource.rolloffMode = attenuationSettings.rolloffMode;
        audioSource.outputAudioMixerGroup = attenuationSettings.mixerGroup ?? SoundManager.Instance.SfxChannel;

        return attenuatedAudio;
    }
}

