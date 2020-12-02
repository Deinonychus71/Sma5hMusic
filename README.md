# Sm5sh.CLI + Sm5shMusic GUI
Script to add (not replace) musics for Smash Ultimate
Based on research & guide by Soneek

## What is it?
Sm5sh.CLI and Sm5shMusic are a series of tools to import additional tracks to Smash Ultimate. 
This tool is highly experimental and may not always work as expect. **Always keep backups of your files.**

## Thanks & Repos of the different tools
1.  Research: soneek
2.  Testing: Demonslayerx8
3.  prcEditor: https://github.com/BenHall-7/paracobNET - BenHall-7
4.  paramLabels: https://github.com/ultimate-research/param-labels - BenHall-7, jam1garner, Dr-HyperCake, Birdwards, ThatNintendoNerd, ScanMountGoat, Meshima, Blazingflare, TheSmartKid, jugeeya, Demonslayerx8
5.  msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor - IcySon55, exelix11
6.  nus3audio: https://github.com/jam1garner/nus3audio-rs - jam1garner
7.  bgm-property: https://github.com/jam1garner/smash-bgm-property - jam1garner
8.  VGAudio: https://github.com/Thealexbarney/VGAudio - Thealexbarney, soneek, jam1garner, devlead, Raytwo, nnn1590
9.  vgmstream: https://github.com/vgmstream/vgmstream - bnnm, kode54, NicknineTheEagle, bxaimc, Thealexbarney
All contributors: https://github.com/vgmstream/vgmstream/graphs/contributors
10. CrossArc: https://github.com/Ploaj/ArcCross Ploaj, ScanMountGoat, BenHall-7, shadowninja108, jam1garner, M-1-RLG

## Changes
1.  Playlists are now treated separately. Playlist settings can now be found under Mods/MusicOverride along with new settings
2.  With this separation, music mods can now be taken in and out while keeping track order, playlist order and other changes out of it.

## Setup
1.  In the "Resources" folder, extract the msbt files in /game/ui/message/ using CrossArc
2.  In the "Resources" folder, extract ui_bgm_db.prc and ui_gametitle_db.prc in /game/ui/params/database/
3.  In the "Resources" folder, extract any nus3bank from Ultimate in /nus3bank and rename 'template.nus3bank'
4.  In the "Resources" folder, make sure nusbank_ids.csv, param_labels.csv is there
5.  In the "Tools" folder, make sure VGAudioCli.exe, paracobNET.dll, MsbtEditor.dll, Nus3Audio/nus3audio.exe and BgmProperty/bgm-property.exe/bgm_hashes.txt are there

### Setup (Optional) - Song play in GUI
1.  In the "Resources" folder, if you want access to the core songs, extract all nus3audio songs from Ultimate in /game/stream;/sound/bgm
2.  Make sure libvgmstream.dll (from this repo) is accessible to Windows (in doubt, put it next to the exe file)
3.  Also make sure all the requierements from vgmstream (Needed extra files (for Windows) - https://github.com/vgmstream/vgmstream) are met.

## CLI - How to create Music Mods
1.  Create a folder in /Mods/MusicMods. This folder can contains several audio files and a metadata file 'metadata_mod.json' or 'metadata_mod.csv'. An example is provided.
2.  Since the GUI, the format of the metadata_mod.json changed. However existing mods should be automatically updated to support the new format.
3.  IDSP and LOPUS support. BRSTM support through on the fly conversion. Nus3audio can be imported "as is" but you need to make sure the file is properly generated.
4.  Keep all non localized fields lowercase.
5.  Make sure the series_id value exists within the game (check 'valid_series.txt' in Resources)!
6.  Make sure the fields values "record_type" exist within the game (original, arrange or new_arrange).
7.  If you enable AudioCaching, the nus3audio files will only be generated once. Be aware that if you use a prefix the name of the files might keep changing and thus generating more files in the cache.
8.  To add songs to a playlist, edit the file playlist_override.json

## GUI
1.  The GUI's primary goal is to provide a way to generate mod files (metadata_mod.json). All the processing work is still done with the CLI.
2.  On top of that, the GUI can easily generate the override files. You can drag & drop songs within the GUI, add songs in any location in the playlists, and more.
3.  The GUI still needs work, especially optimization. There also seems to be a potential crash when opening a BGM modal. I'm looking into it...

## Daisy chaining mods - adding new functionalities (WIP)
1.  The code was reworked to allow for additional mods/code to execute on top of Sm5shMusic.
2.  A state manager takes care of load/parsing game file to c# models. Each new mod can request its own game resource in the form of /ui/param/database/xxx (for example)
3.  This object is kept in memory, and can be modified sequentially by a succession of mod
4.  When mods are finished running (or a user click Build in future UI), the state manager will write back all the game files to an output folder.
5.  Look at the Script.cs in Sm5sh.CLI to get started
6.  For now only a few files / filetypes can be loaded from state manager, register new resource providers to support more formats.

## Todo
1.  Sm5sh: start suddendeath, start transition and whatever can be figured out
2.  Adding a lot more debug logging
3.  More cleanup
4.  Loading external mods/resource providers
5.  Add tests against collisions when adding new songs
