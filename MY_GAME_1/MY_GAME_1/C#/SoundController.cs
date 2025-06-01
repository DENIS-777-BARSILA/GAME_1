using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;


namespace MY_GAME_1;

public static class SoundController
{
    //music
    private static Song MainMenuBack;
    private static Song MainGameBack;


    //effect
    private static SoundEffect ChoiseMenu;
    private static SoundEffect Jump;
    private static SoundEffect Shoot;
    private static SoundEffect Collect;
    private static SoundEffect GameOver;
    private static List<SoundEffect> Chomp;
    private static SoundEffect OpenDoor;




    private static Song currentSong;
    private static bool _gameOverSoundPlayed = false;

    public static float MusicVolume { get; set; } = 0.8f;
    public static float EffectsVolume { get; set; } = 0.5f;

    public static void LoadContent()
    {
        MainMenuBack = GameWorld.Content.Load<Song>("main_menu");
        MainGameBack = GameWorld.Content.Load<Song>("main_menu");

        Chomp = new List<SoundEffect>();
        GameOver = GameWorld.Content.Load<SoundEffect>("game_over");
        ChoiseMenu = GameWorld.Content.Load<SoundEffect>("menu_choise");
        Jump = GameWorld.Content.Load<SoundEffect>("game_over");
        Collect = GameWorld.Content.Load<SoundEffect>("bring");
        Shoot = GameWorld.Content.Load<SoundEffect>("shot");
        Chomp.Add(GameWorld.Content.Load<SoundEffect>("chomp_1"));
        Chomp.Add(GameWorld.Content.Load<SoundEffect>("chomp_2"));
        Chomp.Add(GameWorld.Content.Load<SoundEffect>("chomp_3"));
        OpenDoor = GameWorld.Content.Load<SoundEffect>("open_door");
    }

    public static void Update()
    {
        switch (GameState.CurrentState)
        {
            case GameStates.MainMenu:
                PlaySong(MainMenuBack);
                _gameOverSoundPlayed = false;
                break;

            case GameStates.Playing:
                PlaySong(MainMenuBack);
                _gameOverSoundPlayed = false;
                break;

            case GameStates.Paused:
                PlaySong(MainMenuBack);
                break;

            case GameStates.GameOver:
                if (!_gameOverSoundPlayed)
                {
                    PlayEffect(GameOver);
                    _gameOverSoundPlayed = true;
                }
                break;
        }
    }

    private static void PlaySong(Song song)
    {
        if (song == null || currentSong == song) return;

        currentSong = song;
        MediaPlayer.Volume = MusicVolume;
        MediaPlayer.Play(song);
        MediaPlayer.IsRepeating = true;
    }

    private static void PlayEffect(SoundEffect effect)
    {
        if (effect != null)
        {
            effect.Play(EffectsVolume, 0f, 0f);
        }
    }

    private static void PlayEffect(List<SoundEffect> effects)
    {
        Random _random = new Random();
        if (effects == null || effects.Count == 0)
            return;

        var randomIndex = _random.Next(effects.Count);
        var randomEffect = effects[randomIndex];

        PlayEffect(randomEffect);
    }

    public static void PlayMenuChoiceSound()
    {
        PlayEffect(ChoiseMenu);
    }

    public static void PlayJumpSound()
    {
        PlayEffect(Jump);
    }

    public static void PlayShootSound()
    {
        PlayEffect(Shoot);
    }

    public static void PlayCollectSound()
    {
        PlayEffect(Collect);
    }

    public static void PlayRandomChomp()
    {
        PlayEffect(Chomp);
    }

    public static void PlayMenuChoice()
    {
        PlayEffect(ChoiseMenu);
    }

    public static void PlayOpenDoor()
    {
        PlayEffect(OpenDoor);
    }
}