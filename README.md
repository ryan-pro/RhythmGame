# BeatNova
**BeatNova** is a rhythm game developed as a portfolio project. Styled after games like Chunithm, Arcaea, and Guitar Hero, players are tasked with tapping notes as they travel down a track in sync with the music.

This project is targeting mobile devices, and has been tested on Android. It is playable on PC in a development context. Using Unity 2022.3.29f for development.

## Project Overview
Rhythm games pose challenges unique to their genre, and it's what drew me to try making one myself. In particular, synchronization between audio and visual presentation is critical; if the music and notes are out of sync by even a little bit, then the game becomes a frustrating mess to play.

Further, even at high framerates, it's very likely that a song's beat could end up between two different frames. Tracking and recording time per-frame soon becomes too inaccurate, and relying on such a method would cause the visuals to become more and more desynchronized as time goes on. It became apparent that the timing had to be measured precisely and independently from framerate.

**RhythmConductor** is the component that drives audio-visual synchronization by tracking the song's beat. It uses [AudioSettings.dspTime](https://docs.unity3d.com/ScriptReference/AudioSettings-dspTime.html), rather than Time.time, to more accurately measure the current time. RhythmConductor is given the song's BPM, and determines the seconds-per-beat in order to calculate the song's current position in time. We accomplish two things this way: we know exactly when a beat will play, and we can avoid delta time inaccuracies by using beats-per-second as an incremental basis.

BeatNova, as a game, is very much a **work-in-progress**. This is largely a research project for me, and it is missing many critical features right now. I plan to continuously update this project as I find the time.

All art assets, unless stated otherwise, are AI-generated.

## Current Features
- **Basic gameplay mechanics:** Players can tap on each lane of the track. Tap-hold input is registered, but no gameplay elements yet use it.
- **Audio-visual synchronization:** Visual elements synchronize with the music beats.
- **Some user interface:** Title screen and basic main menu implemented, with music-synchronized elements. Simple results screen implemented.

## Roadmap
### Short-term Goals
- **Code comments:** As a portfolio project, more comments are needed to explain some areas of the codebase.
- **In-game feedback:** Game lacks any visual or other feedback for hitting notes. No in-game way to tell if you hit the note correctly.
- **Pause screen:** Some basic logic for pausing exists, but is not yet implemented.
- **Audio-visual sync calibrator:** Some devices introduce audio or visual lag; a calibration tool is essential.

### Long-term Goals
- **Enhanced visual effects:** Stage needs visual elements behind the main track visual.
- **Scoring system:** Player should be scored based on hit type, streak, etc.
- **Song selection menu:** Menu should list every playable song along with available song info when selected.
- **Working options menu:** Add a menu to change game settings, calibrate audio-visual synchronization, etc.
- **Better notes editor:** Placing notes is too manual and tedious to do. A better way can be made, either in-editor or with an external tool.
- **Download more songs remotely:** Using Addressables, implement the ability to find and download songs from a remote server.

## Acknowledgments
Shoutout to the following articles that set me on the right path when I first started this project:
- Hafiz Azman (fizzd) - [An LD48 Rhythm Game?! (Part 2)](https://archive.md/9qsLC)
- Graham Tattersall - [Coding to the Beat - Under the Hood of a Rhythm Game in Unity](https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity)

These two articles helped me form the foundation for what became the RhythmConductor component.
### Songs Included
- A Good Boi - [Dreams & Visions](https://soundcloud.com/agoodboi/dreams-visions)
  - Used under the [Creative Commons Sharealike license](https://creativecommons.org/licenses/by-sa/3.0/)
- rk - [Starry night](https://soundcloud.com/gpizly99xijy/starry-night)
  - Used under the [Creative Commons Sharealike license](https://creativecommons.org/licenses/by-sa/3.0/)
- Spiky Candy - Used under the [Creative Commons Attribution license](https://creativecommons.org/licenses/by/3.0/)
  - [Hello World!!](https://soundcloud.com/spikycandy/hello-world)
  - [Clear](https://soundcloud.com/spikycandy/clear)
  - [Ray of Light](https://soundcloud.com/spikycandy/ray-of-light)
  - [Rose](https://soundcloud.com/spikycandy/rose)
  - [Violet](https://soundcloud.com/spikycandy/violet)

## Third-Party Software
Outside of Unity-provided packages, this project utilizes the following third-party code:
- [UniTask](https://github.com/Cysharp/UniTask), used under [MIT license](https://github.com/Cysharp/UniTask?tab=MIT-1-ov-file#readme)
- [DOTween](https://github.com/Demigiant/dotween), provided under [proprietary license](https://dotween.demigiant.com/license.php)
