using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Audio
{
    public class MusicPlayer : Entity
    {
        AudioPlayer player = new AudioPlayer();
        public IReadOnlyList<SoundEffect> Tracks => tracks;
        private int trackIndex = 0;
        public SoundEffect CurrentTrack => Tracks.FirstOrDefault();
        List<SoundEffect> tracks = new List<SoundEffect>();
        public bool Repeat;

        public float Volume
        {
            get => player.Volume;
            set => player.Volume = value;
        }

        float fadeVolumeInitial;
        float fadeTimeInitial;
        float fadeTime;
        float fadeTarget;

        public MusicPlayer()
        {
            AllowReplicate = false;
            player.Loop = false;
            player.OnTrackEnd += Next;
        }
        public void SetTracks(IEnumerable<SoundEffect> tracks)
        {
            this.tracks = tracks.ToList();
            player.SoundEffect = CurrentTrack;
        }
        public void Next()
        {
            trackIndex++;
            if (trackIndex >= tracks.Count)
            {
                if (Repeat)
                    trackIndex = 0;
                else
                    return;
            }
            player.SoundEffect = tracks[trackIndex];
            player.State = AudioState.Playing;
        }
        public void Previous()
        {
            trackIndex--;
            if (trackIndex < 0)
            {
                if (Repeat)
                    trackIndex = tracks.Count - 1;
                else
                    trackIndex = 0;
            }
            player.SoundEffect = tracks[trackIndex];
        }
        public void FadeTo(float volumeTarget, float time)
        {
            fadeTarget = volumeTarget;
            fadeVolumeInitial = Volume;
            fadeTimeInitial = fadeTime = time;
        }
        public AudioState State
        {
            get => player.State;
            set
            {
                player.Volume = Volume;
                player.State = value;
            }
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            if (fadeTimeInitial != 0)
            {
                fadeTime -= dt;
                if (fadeTime > 0)
                {
                    var w = fadeTime / fadeTimeInitial;
                    Volume = fadeVolumeInitial * w + fadeTarget * (1 - w);
                }
                else
                {
                    fadeTimeInitial = 0;
                    Volume = fadeTarget;
                }
            }
            var track = tracks.FirstOrDefault();
        }
    }
}
