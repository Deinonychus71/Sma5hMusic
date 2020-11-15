# Sm5shMusic
Script to add (not replace) musics for Smash Ultimate
Based on research & guide by Soneek

**This is highly experimental! Bugs may happen.**

## Repos of the different tools
1. prcEditor: https://github.com/BenHall-7/paracobNET
2. msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor
3. nus3audio:  https://github.com/jam1garner/nus3audio-rs
4. bgm-property:  https://github.com/jam1garner/smash-bgm-property
5. VGAudio:  https://github.com/Thealexbarney/VGAudio
6. CrossArc: https://github.com/Ploaj/ArcCross

## Setup
1.  In the "Resources" folder, extract the msbt files in /game/ui/message/ using CrossArc
2.  In the "Resources" folder, extract ui_bgm_db.prc and ui_gametitle_db.prc in /game/ui/params/database/
3.  In the "Resources" folder, extract any nus3bank from Ultimate in /nus3bank and rename 'template.nus3bank'

## Mods
1.  Create a folder in /Mods/MusicMods. This folder can contains several audio files and a metadata file 'metadata_mod.json' or 'metadata_mod.csv'. An example is provided.
2.  IDSP and LOPUS support. BRSTM support through on the fly conversion.
3.  The 'prefix' can be used to give an unique internal id to songs in case of multiple mods. Keep it short/lowercase (3 characters top). This parameter is optional. If you leave it blank, the id of the song will be used instead.
4.  Keep all non localized fields lowercase.
5.  Make sure the series_id value exists within the game (check 'valid_series.txt' in Resources)!
6.  If you use a new playlist, you must edit the stage db manually to reference it.
7.  Make sure the fields values "record_type" exist within the game (original, arrange or new_arrange).
8.  If you enable AudioCaching, the nus3audio files will only be generated once. Be aware that if you use a prefix the name of the files might keep changing and thus generating more files in the cache.
