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
            player.OnTrackEnd += startNextTrack;
        }
        public void SetTracks(IEnumerable<SoundEffect> tracks)
        {
            this.tracks = tracks.ToList();
            player.SoundEffect = CurrentTrack;
        }
        void startNextTrack()
        {
            if (Repeat)
                tracks.Add(tracks[0]);
            if (tracks.Any())
            {
                tracks.RemoveAt(0);
                var track = tracks.FirstOrDefault();
                player.SoundEffect = track;
            }
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
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (fadeTimeInitial != 0)
            {
                fadeTime -= gameTime.DT();
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
