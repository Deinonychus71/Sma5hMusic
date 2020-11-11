# Sm5shMusic
Script to add (not replace) musics for Smash Ultimate
Based on research & guide by Soneek

This is highly experimental! Bugs may happen.

## Repos of the different tools
1. prcEditor: https://github.com/BenHall-7/paracobNET
2. msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor
3. nus3audio:  https://github.com/jam1garner/nus3audio-rs
4. bgm-property:  https://github.com/jam1garner/smash-bgm-property
5. VGAudio:  https://github.com/Thealexbarney/VGAudio
6. CrossArc: https://github.com/Ploaj/ArcCross

## Setup
1.  In the "Resources" folder, extract the msbt files in the /msbt using CrossArc
2.  In the "Resources" folder, extract ui_bgm_db.prc and ui_gametitle_db.prc in /params
3.  In the "Resources" folder, extract ui_bgm_db.prc and ui_gametitle_db.prc in /params
4.  In the "Resources" folder, extract any nus3bank from Ultimate in /nus3bank and rename 'template.nus3bank'

## Mods
1.  Create a folder in /MusicMods. This folder can contains several audio files and a metadata file 'metadata_mod.json'. An example is provided.
2.  The 'prefix' value will be used to give an unique internal id to songs. Keep it short (3 characters top)
2.  Only the data within 'songs' matters.
3.  Keep all non localized fields lowercase.
4.  Make sure the series_id value exists within the game (will provide enum later)!
5.  Make sure the bgm playlist (playlist_id) exists within the game (will provide enum later). The script will be updated to support new playlists.
6.  Make sure the fields values for "rarity" and "record_type" exist within the game (will provide enum later).
