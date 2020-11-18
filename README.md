# Sm5sh.CLI + Sm5shMusic
Script to add (not replace) musics for Smash Ultimate
Based on research & guide by Soneek

**This is highly experimental! Bugs may happen.**

## Repos of the different tools
1. prcEditor: https://github.com/BenHall-7/paracobNET - BenHall-7
2. paramLabels: https://github.com/ultimate-research/param-labels
2. msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor - IcySon55 & exelix11
3. nus3audio:  https://github.com/jam1garner/nus3audio-rs - jam1garner
4. bgm-property:  https://github.com/jam1garner/smash-bgm-property - jam1garner
5. VGAudio:  https://github.com/Thealexbarney/VGAudio
6. CrossArc: https://github.com/Ploaj/ArcCross

## Setup
1.  In the "Resources" folder, extract the msbt files in /game/ui/message/ using CrossArc
2.  In the "Resources" folder, extract ui_bgm_db.prc and ui_gametitle_db.prc in /game/ui/params/database/
3.  In the "Resources" folder, extract any nus3bank from Ultimate in /nus3bank and rename 'template.nus3bank'

## How to create Music Mods
1.  Create a folder in /Mods/MusicMods. This folder can contains several audio files and a metadata file 'metadata_mod.json' or 'metadata_mod.csv'. An example is provided.
2.  IDSP and LOPUS support. BRSTM support through on the fly conversion.
3.  The 'prefix' can be used to give an unique internal id to songs in case of multiple mods. Keep it short/lowercase (3 characters top). This parameter is optional. If you leave it blank, the id of the song will be used instead.
4.  Keep all non localized fields lowercase.
5.  Make sure the series_id value exists within the game (check 'valid_series.txt' in Resources)!
6.  If you use a new playlist, you must edit the stage db manually to reference it.
7.  Make sure the fields values "record_type" exist within the game (original, arrange or new_arrange).
8.  If you enable AudioCaching, the nus3audio files will only be generated once. Be aware that if you use a prefix the name of the files might keep changing and thus generating more files in the cache.

## Daisy chaining mods - adding new functionalities (WIP)
1.  The code was reworked to allow for additional mods/code to execute on top of Sm5shMusic.
2.  A state manager takes care of load/parsing game file to c# models. Each new mod can request its own game resource in the form of /ui/param/database/xxx (for example)
3.  This object is kept in memory, and can be modified sequentially by a succession of mod
4.  When mods are finished running (or a user click Build in future UI), the state manager will write back all the game files to an output folder.
5.  Look at the Script.cs in Sm5sh.CLI to get started
6.  For now only a few files / filetypes can be loaded from state manager, register new resource providers to support more formats.

## Todo
1.  Sm5sh: Volume support, start suddendeath, start transition and whatever can be figured out
2.  Adding a lot more debug logging
3.  More cleanup
4.  Loading external mods/resource providers
5.  UI (maybe... don't expect it soon / help much appreciated)
