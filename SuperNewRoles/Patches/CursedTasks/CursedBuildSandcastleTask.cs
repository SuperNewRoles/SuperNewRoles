using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Rewired;
using UnityEngine;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace SuperNewRoles.Patches.CursedTasks;

public static class CursedBuildSandcastleTask
{
    // 追加で生成するバケツの位置。
    // - `UseAbsoluteLocalPositions == false` の場合: 元バケツ(root)の localPosition からのオフセット
    // - `UseAbsoluteLocalPositions == true` の場合: そのまま localPosition として使用
    public static bool UseAbsoluteLocalPositions = true;

    // 生成した clone 側の bucketBackSprite を前後にずらす（重なり順の調整用）
    public static float CloneBackSpriteZStep = 0.01f;

    public static readonly List<(Vector3 pos, Vector3 rotate)> ExtraBucketLocalPositions = new()
    {
        // 左
        (new (-1.107f, 0.539f, -0.03f), new(0f, 0f, 0f)),
        (new (-1.107f, 0.739f, -0.04f), new(0f, 0f, 0f)),
        // 真ん中
        (new (-0.007f, 0.339f, -0.031f), new(13f, 13f, 0f)),
        (new (-0.007f, 0.539f, -0.032f), new(13f, 13f, 0f)),
        (new (-0.007f, 0.739f, -0.033f), new(13f, 13f, 0f)),

        // 右
        (new (0.593f, 0.439f, -0.02f), new(39f, 0f, 0f)),
        (new (0.593f, 0.639f, -0.03f), new(39f, 0f, 0f)),
        (new (0.593f, 0.839f, -0.031f), new(39f, 0f, 0f)),
    };

    [HarmonyPatch(typeof(BuildSandcastleMinigame))]
    public static class BuildSandcastleMinigamePatch
    {
        private const float CloseEnoughToTop = 0.8f;

        private static readonly ConditionalWeakTable<BuildSandcastleMinigame, State> States = new();


        [HarmonyPatch(nameof(BuildSandcastleMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(BuildSandcastleMinigame __instance)
        {
            if (!Main.IsCursed) return;
            EnsureState(__instance);
        }

        [HarmonyPatch("Update"), HarmonyPrefix]
        public static bool UpdatePrefix(BuildSandcastleMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            if (!__instance) return true;
            if (__instance.MyNormTask.IsComplete) return false;

            State state = EnsureState(__instance);
            if (state == null || state.Completed) return false;

            Transform start = __instance.startPosition;
            Transform end = __instance.endPosition;
            if (!start || !end) return true;

            float liftLerpSpeed = __instance.liftLerpSpeed;
            bool anyGrabStarted = false;

            if (state.Buckets.Count > 0)
            {
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
                {
                    Player player = __instance.player;
                    if (player != null)
                    {
                        float axis = player.GetAxisRaw(14);
                        bool hasInput = axis > 0.01f;
                        for (int i = 0; i < state.Buckets.Count; i++)
                        {
                            Bucket bucket = state.Buckets[i];
                            bool prev = bucket.HasInput;
                            bucket.HasInput = hasInput;

                            if (prev && !bucket.HasInput) bucket.FallVelocity = 0f;
                            if (!prev && bucket.HasInput) anyGrabStarted = true;
                            if (bucket.HasInput) bucket.SetBucketY(axis, instant: false, liftLerpSpeed);
                        }
                    }
                }
                else
                {
                    Controller controller = __instance.controller;
                    controller.Update();

                    float startEndDistance = Vector2.Distance(start.position, end.position);
                    if (startEndDistance < 0.0001f) startEndDistance = 0.0001f;

                    // y座標が高い順（上の方から）にバケツをソート
                    var sortedBuckets = state.Buckets.OrderByDescending(bucket => bucket.MoveTransform.position.y);

                    foreach (Bucket bucket in sortedBuckets)
                    {
                        bool prev = bucket.HasInput;

                        bucket.HasInput = false;
                        if (bucket.Collider && bucket.Collider.enabled)
                        {
                            DragState dragState = controller.CheckDrag(bucket.Collider);
                            if ((uint)(dragState - 1) <= 2u)
                            {
                                bucket.HasInput = true;
                                float num = end.position.y - controller.DragPosition.y;
                                float newValue = 1f - Mathf.Clamp01(num / startEndDistance);
                                bucket.SetBucketY(newValue, instant: true, liftLerpSpeed);
                            }
                        }

                        if (prev && !bucket.HasInput) bucket.FallVelocity = 0f;
                        if (!prev && bucket.HasInput) anyGrabStarted = true;
                    }
                }
            }

            if (anyGrabStarted && Constants.ShouldPlaySfx())
            {
                AudioClip grabSound = __instance.grabSound;
                if (grabSound) SoundManager.Instance.PlaySound(grabSound, loop: false);
            }

            float fallGravity = __instance.fallGravity;
            for (int i = 0; i < state.Buckets.Count; i++)
            {
                Bucket bucket = state.Buckets[i];
                if (!bucket.HasInput && bucket.TrueBucketY > 0f)
                {
                    bucket.FallVelocity += fallGravity;
                    bucket.SetBucketY(bucket.TrueBucketY - bucket.FallVelocity * Time.deltaTime, instant: true, liftLerpSpeed);
                }
                bucket.UpdateBucketPosition(start, end);

                if (!bucket.Completed && bucket.TrueBucketY >= CloseEnoughToTop)
                {
                    bucket.Completed = true;
                    if (bucket.Collider) bucket.Collider.enabled = false;
                    if (bucket.Glyph) bucket.Glyph.SetActive(false);
                    bucket.HasInput = false;
                    bucket.TargetBucketY = 1f;
                    bucket.TrueBucketY = 1f;

                    List<IEnumerator> fades = new(2)
                    {
                        Effects.ColorFade(bucket.FrontSprite, Color.white, Color.clear, 0.5f),
                        Effects.ColorFade(bucket.BackSprite, Color.white, Color.clear, 0.5f)
                    };
                    __instance.StartCoroutine(Effects.All(fades.ToArray()));
                }
            }

            bool allComplete = state.Buckets.Count > 0;
            for (int i = 0; i < state.Buckets.Count; i++)
            {
                if (!state.Buckets[i].Completed)
                {
                    allComplete = false;
                    break;
                }
            }

            if (allComplete)
            {
                SuccessAll(__instance, state);
            }

            return false;
        }

        private static State EnsureState(BuildSandcastleMinigame instance)
        {
            if (States.TryGetValue(instance, out State existing) && existing != null) return existing;

            State created = new();
            States.Add(instance, created);

            EnsureExtraBuckets(instance);
            BuildBuckets(instance, created);

            return created;
        }

        private static void EnsureExtraBuckets(BuildSandcastleMinigame instance)
        {
            if (ExtraBucketLocalPositions.Count == 0) return;
            if (!instance) return;

            Collider2D bucketCollider = instance.bucketCollider;
            SpriteRenderer bucketSprite = instance.bucketSprite;
            SpriteRenderer bucketBackSprite = instance.bucketBackSprite;
            GameObject bucketGlyph = instance.bucketGlyph;

            if (!bucketCollider || !bucketSprite || !bucketBackSprite || !bucketGlyph) return;

            Transform bucketRoot = FindCommonAncestor(
                bucketCollider.transform,
                bucketSprite.transform,
                bucketBackSprite.transform,
                bucketGlyph.transform);
            if (!bucketRoot) return;
            if (bucketRoot.GetComponent<CursedSandcastleBucketRootMarker>()) return;

            bucketRoot.gameObject.AddComponent<CursedSandcastleBucketRootMarker>();

            Vector3 baseLocalPos = bucketRoot.localPosition;
            Transform parent = bucketRoot.parent;
            if (!parent) return;

            string backSpritePath = GetRelativePath(bucketRoot, bucketBackSprite.transform);

            for (int i = 0; i < ExtraBucketLocalPositions.Count; i++)
            {
                GameObject clone = Object.Instantiate(bucketRoot.gameObject, parent);
                clone.name = $"{bucketRoot.gameObject.name}_CursedClone{i}";
                clone.AddComponent<CursedSandcastleBucketCloneMarker>();

                Vector3 localPos = ExtraBucketLocalPositions[i].pos;
                clone.transform.localPosition = UseAbsoluteLocalPositions ? localPos : baseLocalPos + localPos;
                clone.transform.localRotation = Quaternion.Euler(ExtraBucketLocalPositions[i].rotate);
                clone.transform.localScale = bucketRoot.localScale;

                if (!string.IsNullOrEmpty(backSpritePath) && CloneBackSpriteZStep != 0f)
                {
                    Transform cloneBackSpriteTransform = GetTransformByPath(clone.transform, backSpritePath);
                    if (cloneBackSpriteTransform != null)
                    {
                        Vector3 pos = cloneBackSpriteTransform.localPosition;
                        pos.z += CloneBackSpriteZStep * (i + 1);
                        cloneBackSpriteTransform.localPosition = pos;
                    }
                }
            }
        }

        private static void BuildBuckets(BuildSandcastleMinigame instance, State state)
        {
            Collider2D originalCollider = instance.bucketCollider;
            SpriteRenderer originalFront = instance.bucketSprite;
            SpriteRenderer originalBack = instance.bucketBackSprite;
            GameObject originalGlyph = instance.bucketGlyph;

            if (!originalCollider || !originalFront || !originalBack || !originalGlyph) return;

            Transform originalRoot = FindCommonAncestor(
                originalCollider.transform,
                originalFront.transform,
                originalBack.transform,
                originalGlyph.transform);
            if (!originalRoot) return;

            Transform parent = originalRoot.parent;
            if (!parent) return;

            string colliderPath = GetRelativePath(originalRoot, originalCollider.transform);
            string frontPath = GetRelativePath(originalRoot, originalFront.transform);
            string backPath = GetRelativePath(originalRoot, originalBack.transform);
            string glyphPath = GetRelativePath(originalRoot, originalGlyph.transform);

            var roots = new List<Transform> { originalRoot };
            foreach (CursedSandcastleBucketCloneMarker marker in parent.GetComponentsInChildren<CursedSandcastleBucketCloneMarker>(includeInactive: true))
            {
                if (marker && marker.transform && marker.transform.parent == parent) roots.Add(marker.transform);
            }

            state.Buckets.Clear();
            for (int i = 0; i < roots.Count; i++)
            {
                Transform root = roots[i];
                if (!root) continue;

                Transform colliderT = ResolvePath(root, colliderPath);
                Transform frontT = ResolvePath(root, frontPath);
                Transform backT = ResolvePath(root, backPath);
                Transform glyphT = ResolvePath(root, glyphPath);

                Collider2D collider = colliderT ? colliderT.GetComponent<Collider2D>() : null;
                SpriteRenderer front = frontT ? frontT.GetComponent<SpriteRenderer>() : null;
                SpriteRenderer back = backT ? backT.GetComponent<SpriteRenderer>() : null;
                GameObject glyph = glyphT ? glyphT.gameObject : null;

                if (!collider || !front || !back || !glyph) continue;

                // これしないとクルーメイトが動いちゃう。
                collider.isTrigger = true;

                state.Buckets.Add(new Bucket(root, collider, front, back, glyph));
            }
        }

        private static void SuccessAll(BuildSandcastleMinigame instance, State state)
        {
            if (state.Completed) return;
            state.Completed = true;

            for (int i = 0; i < state.Buckets.Count; i++)
            {
                Bucket bucket = state.Buckets[i];
                if (bucket.Collider) bucket.Collider.enabled = false;
                if (bucket.Glyph) bucket.Glyph.SetActive(false);
            }

            instance.StartCoroutine(instance.CoStartClose());
            instance.MyNormTask.NextStep();

            if (Constants.ShouldPlaySfx())
            {
                AudioClip completeSound = instance.completeSound;
                if (completeSound) SoundManager.Instance.PlaySound(completeSound, loop: false);
            }
        }

        private static Transform FindCommonAncestor(params Transform[] transforms)
        {
            if (transforms == null || transforms.Length == 0) return null;
            Transform start = transforms[0];
            if (!start) return null;

            for (Transform current = start; current; current = current.parent)
            {
                bool ok = true;
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform t = transforms[i];
                    if (!t || (!t.IsChildOf(current) && t != current))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) return current;
            }

            return null;
        }

        private static Transform ResolvePath(Transform root, string path)
        {
            if (!root) return null;
            if (string.IsNullOrEmpty(path)) return root;
            return root.Find(path);
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (!target || !root) return null;
            if (target == root) return string.Empty;
            if (!target.IsChildOf(root)) return null;

            var parts = new List<string>();
            Transform current = target;
            while (current && current != root)
            {
                parts.Add(current.name);
                current = current.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static Transform GetTransformByPath(Transform root, string path)
        {
            if (!root || string.IsNullOrEmpty(path)) return null;

            string[] parts = path.Split('/');
            Transform current = root;

            foreach (string part in parts)
            {
                current = current.Find(part);
                if (!current) return null;
            }

            return current;
        }

        public sealed class CursedSandcastleBucketRootMarker : MonoBehaviour { }

        public sealed class CursedSandcastleBucketCloneMarker : MonoBehaviour { }

        private sealed class State
        {
            public readonly List<Bucket> Buckets = new();
            public bool Completed;
        }

        private sealed class Bucket
        {
            public readonly Transform MoveTransform;
            public readonly Collider2D Collider;
            public readonly SpriteRenderer FrontSprite;
            public readonly SpriteRenderer BackSprite;
            public readonly GameObject Glyph;

            public bool HasInput;
            public float FallVelocity;
            public float TargetBucketY;
            public float TrueBucketY;

            public bool Completed;

            private bool hasCachedLocalOffset;
            private Vector3 cachedLocalOffset;
            private bool hasCachedWorldOffset;
            private Vector3 cachedWorldOffset;

            public Bucket(Transform moveTransform, Collider2D collider, SpriteRenderer frontSprite, SpriteRenderer backSprite, GameObject glyph)
            {
                MoveTransform = moveTransform;
                Collider = collider;
                FrontSprite = frontSprite;
                BackSprite = backSprite;
                Glyph = glyph;

                HasInput = false;
                FallVelocity = 0f;
                TargetBucketY = 0f;
                TrueBucketY = 0f;

                Completed = false;

                hasCachedLocalOffset = false;
                cachedLocalOffset = Vector3.zero;
                hasCachedWorldOffset = false;
                cachedWorldOffset = Vector3.zero;
            }

            public void SetBucketY(float newValue, bool instant, float liftLerpSpeed)
            {
                TargetBucketY = Mathf.Clamp01(newValue);
                if (instant)
                {
                    TrueBucketY = TargetBucketY;
                }
                else
                {
                    TrueBucketY = Mathf.Lerp(TrueBucketY, TargetBucketY, Time.deltaTime * liftLerpSpeed);
                }
            }

            public void UpdateBucketPosition(Transform start, Transform end)
            {
                if (!MoveTransform || !start || !end) return;

                bool useLocal = MoveTransform.parent && MoveTransform.parent == start.parent && MoveTransform.parent == end.parent;

                if (useLocal)
                {
                    if (!hasCachedLocalOffset)
                    {
                        cachedLocalOffset = MoveTransform.localPosition - start.localPosition;
                        hasCachedLocalOffset = true;
                    }
                    MoveTransform.localPosition = Vector3.Lerp(start.localPosition, end.localPosition, TrueBucketY) + cachedLocalOffset;
                }
                else
                {
                    if (!hasCachedWorldOffset)
                    {
                        cachedWorldOffset = MoveTransform.position - start.position;
                        hasCachedWorldOffset = true;
                    }
                    MoveTransform.position = Vector3.Lerp(start.position, end.position, TrueBucketY) + cachedWorldOffset;
                }
            }
        }
    }
}
