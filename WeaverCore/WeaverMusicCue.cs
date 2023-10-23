﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore
{
	[CreateAssetMenu(fileName = "WeaverMusicCue", menuName = "WeaverCore/Weaver Music Cue", order = 1000)]
    public class WeaverMusicCue : MusicCue, ISerializationCallbackReceiver
	{
        static Func<MusicCue, MusicChannelInfo[]> channelInfosGetter;
        static Func<MusicCue, Alternative[]> alternativesGetter;

        static Action<MusicCue, MusicChannelInfo[]> channelInfosSetter;
        static Action<MusicCue, Alternative[]> alternativesSetter;

        static Func<MusicChannelInfo, MusicChannelSync> syncGetter;
        static Action<MusicChannelInfo, MusicChannelSync> syncSetter;

        static Func<MusicChannelInfo, AudioClip> clipGetter;
        static Action<MusicChannelInfo, AudioClip> clipSetter;


        /*[Serializable]
        struct DataContainer
        {
            public MusicChannelInfo[] channelInfos;
            public Alternative[] alternatives;
        }

		[SerializeField]
        [HideInInspector]
        string cueData;*/

        [SerializeField]
        [Tooltip("If set to true, then the audio snapshot specified below will be used when the music cue is applied")]
        bool useSnapshot = true;

        [SerializeField]
        Music.SnapshotType m_snapshot;

        public Music.SnapshotType SnapshotType => m_snapshot;


        [SerializeField]
        List<AudioClip> channelInfos_clip;

        [SerializeField]
        List<MusicChannelSync> channelInfos_sync;

        [SerializeField]
        List<string> alternatives_PlayerDataBoolKey;

        [SerializeField]
        List<MusicCue> alternatives_Cue;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            //cueData = "";


            if (channelInfos_clip == null)
            {
                channelInfos_clip = new List<AudioClip>();
            }

            if (channelInfos_sync == null)
            {
                channelInfos_sync = new List<MusicChannelSync>();
            }

            if (alternatives_PlayerDataBoolKey == null)
            {
                alternatives_PlayerDataBoolKey = new List<string>();
            }

            if (alternatives_Cue == null)
            {
                alternatives_Cue = new List<MusicCue>();
            }

            channelInfos_clip.Clear();
            channelInfos_clip.AddRange(channelInfosGetter(this).Select(C => C.Clip));

            channelInfos_sync.Clear();
            channelInfos_sync.AddRange(channelInfosGetter(this).Select(C => syncGetter(C)));

            alternatives_PlayerDataBoolKey.Clear();
            alternatives_PlayerDataBoolKey.AddRange(alternativesGetter(this).Select(a => a.PlayerDataBoolKey));

            alternatives_Cue.Clear();
            alternatives_Cue.AddRange(alternativesGetter(this).Select(a => a.Cue));

            /*cueData = JsonUtility.ToJson(new DataContainer
            {
                channelInfos = channelInfosGetter(this),
                alternatives = alternativesGetter(this)
            }, false);*/
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            //var container = JsonUtility.FromJson<DataContainer>(cueData);

            //channelInfosSetter(this, container.channelInfos);
            //alternativesSetter(this, container.alternatives);

            MusicChannelInfo[] channels = new MusicChannelInfo[channelInfos_clip.Count];

            for (int i = 0; i < channels.Length; i++)
            {
                channels[i] = new MusicChannelInfo();
                clipSetter(channels[i], channelInfos_clip[i]);
                syncSetter(channels[i], channelInfos_sync[i]);
            }

            channelInfosSetter(this, channels);

            Alternative[] alternatives = new Alternative[alternatives_Cue.Count];

            for (int i = 0; i < alternatives.Length; i++)
            {
                alternatives[i] = new Alternative()
                {
                    PlayerDataBoolKey = alternatives_PlayerDataBoolKey[i],
                    Cue = alternatives_Cue[i]
                };
            }

            alternativesSetter(this, alternatives);

#endif
        }

        /*void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            //cueData = "";

            cueData = JsonUtility.ToJson(new DataContainer
            {
                channelInfos = channelInfosGetter(this),
                alternatives = alternativesGetter(this)
            }, false);
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            var container = JsonUtility.FromJson<DataContainer>(cueData);

            channelInfosSetter(this, container.channelInfos);
            alternativesSetter(this, container.alternatives);
#endif
        }*/

        private void Reset()
        {
            channelInfosSetter(this,new MusicChannelInfo[6]);
        }

        [OnInit]
        static void Init()
        {
            channelInfosGetter = ReflectionUtilities.CreateFieldGetter<MusicCue, MusicChannelInfo[]>("channelInfos");
            alternativesGetter = ReflectionUtilities.CreateFieldGetter<MusicCue, Alternative[]>("alternatives");

            channelInfosSetter = ReflectionUtilities.CreateFieldSetter<MusicCue, MusicChannelInfo[]>("channelInfos");
            alternativesSetter = ReflectionUtilities.CreateFieldSetter<MusicCue, Alternative[]>("alternatives");

            syncGetter = ReflectionUtilities.CreateFieldGetter<MusicChannelInfo, MusicChannelSync>("sync");
            syncSetter = ReflectionUtilities.CreateFieldSetter<MusicChannelInfo, MusicChannelSync>("sync");

            clipGetter = ReflectionUtilities.CreateFieldGetter<MusicChannelInfo, AudioClip>("clip");
            clipSetter = ReflectionUtilities.CreateFieldSetter<MusicChannelInfo, AudioClip>("clip");
        }

        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var snapshotGetter = typeof(MusicCue).GetProperty(nameof(Snapshot)).GetGetMethod();

            var prefix = typeof(WeaverMusicCue).GetMethod(nameof(SnapshotGetterPrefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(snapshotGetter, prefix, null);
        }

        static bool SnapshotGetterPrefix(MusicCue __instance, ref AudioMixerSnapshot __result)
        {
            if (__instance is WeaverMusicCue wmc)
            {
                if (wmc.useSnapshot)
                {
                    __result = Music.GetSnapshot(wmc.SnapshotType);
                }
                else
                {
                    __result = null;
                }
                return false;
            }
            return true;
        }
    }
}
