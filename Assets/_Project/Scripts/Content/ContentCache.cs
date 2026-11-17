using System;
using System.Collections.Generic;
using AddressablesDelivery.Loading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬을 옮겨도 남는 Addressables 리소스 보관소. 주소마다 핸들을 하나만 쥔다.
    /// - PrepareAsync(목록): 목록에서 아직 없는 주소만 한꺼번에 불러오고 모두 끝날 때까지 기다린다. 이미 다 있으면 기다리지 않고 바로 끝난다.
    ///   씬 전환과 씬의 불러오기가 같은 목록을 동시에 요청해도 주소마다 한 번만 불러온다.
    /// - ReleaseExcept(목록): 목록에 없고 Persistent 가 아닌 리소스를 반환한다. 새 씬이 열린 뒤에 부른다.
    ///   앞 씬이 아직 그려지는 동안 반환하면 번들이 내려가 그 씬의 모델과 재질이 빈다.
    /// - 반환과 보유 수는 Addressables Delivery 의 ResourceCounters 에 기록한다(반복 전환 뒤 보유 수가 돌아오는지 본다).
    /// </summary>
    public static class ContentCache
    {
        private sealed class Entry
        {
            public AsyncOperationHandle Handle;
            public bool Persistent;
        }

        private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();

        /// <summary>지금 쥐고 있는 주소 수(불러오는 중 포함).</summary>
        public static int Count => entries.Count;

        // 에디터에서 도메인 재로드 없이 플레이를 다시 시작해도 지난 플레이의 핸들을 들고 가지 않는다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => entries.Clear();

        /// <summary>목록의 리소스가 모두 불러와져 있는지. 그렇다면 씬을 열 때 로딩 화면이 필요 없다.</summary>
        public static bool IsReady(SceneContentSet set)
        {
            if (set == null) return false;
            foreach (SceneContentSet.Entry wanted in set.Entries)
            {
                if (!entries.TryGetValue(wanted.address, out Entry entry) || !entry.Handle.IsDone) return false;
            }
            return true;
        }

        /// <summary>목록에서 없는 주소를 불러오고 모두 끝날 때까지 기다린다. progress 는 0~1. 하나라도 실패하면 예외를 던진다.</summary>
        public static async Awaitable PrepareAsync(SceneContentSet set, Action<float> progress = null)
        {
            if (set == null) return;
            foreach (SceneContentSet.Entry wanted in set.Entries)
            {
                if (entries.TryGetValue(wanted.address, out Entry existing))
                {
                    existing.Persistent |= wanted.retention == SceneContentSet.Retention.Persistent;
                    continue;
                }
                AsyncOperationHandle handle = wanted.kind == SceneContentSet.Kind.Sprite
                    ? Addressables.LoadAssetAsync<Sprite>(wanted.address)
                    : Addressables.LoadAssetAsync<Object>(wanted.address);
                ResourceCounters.AssetAcquired();
                entries[wanted.address] = new Entry { Handle = handle, Persistent = wanted.retention == SceneContentSet.Retention.Persistent };
            }

            while (true)
            {
                float sum = 0f;
                bool done = true;
                foreach (SceneContentSet.Entry wanted in set.Entries)
                {
                    if (!entries.TryGetValue(wanted.address, out Entry entry)) continue;   // 기다리는 사이 실패로 빠졌다
                    sum += entry.Handle.IsDone ? 1f : entry.Handle.PercentComplete;
                    done &= entry.Handle.IsDone;
                }
                progress?.Invoke(set.Entries.Count > 0 ? sum / set.Entries.Count : 1f);
                if (done) break;
                await Awaitable.NextFrameAsync();
            }

            var failed = new List<string>();
            foreach (SceneContentSet.Entry wanted in set.Entries)
            {
                if (!entries.TryGetValue(wanted.address, out Entry entry) || entry.Handle.Status == AsyncOperationStatus.Succeeded) continue;
                failed.Add(wanted.address);
                Release(wanted.address, entry);
            }
            if (failed.Count > 0) throw new MissingReferenceException("[ContentCache] 불러오지 못했다: " + string.Join(", ", failed));
        }

        /// <summary>목록에 없고 Persistent 가 아닌 리소스를 반환한다. 반환한 수를 돌려준다.</summary>
        public static int ReleaseExcept(SceneContentSet keep)
        {
            var released = new List<string>();
            foreach (KeyValuePair<string, Entry> pair in entries)
            {
                if (pair.Value.Persistent || (keep != null && keep.Contains(pair.Key))) continue;
                if (!pair.Value.Handle.IsDone) continue;   // 불러오는 중인 것은 다음 전환에서 정리한다
                released.Add(pair.Key);
            }
            foreach (string address in released) Release(address, entries[address]);
            return released.Count;
        }

        /// <summary>불러온 리소스. 목록에 없었거나 아직 불러오는 중이면 null.</summary>
        public static T Get<T>(string address) where T : Object
        {
            if (!entries.TryGetValue(address, out Entry entry) || !entry.Handle.IsDone || entry.Handle.Status != AsyncOperationStatus.Succeeded) return null;
            return entry.Handle.Result as T;
        }

        private static void Release(string address, Entry entry)
        {
            entries.Remove(address);
            Addressables.Release(entry.Handle);
            ResourceCounters.AssetReleased();
        }
    }
}
