# Sma5h.CLI + Sma5hMusic GUI
Script to add (not replace) musics for Smash Ultimate
Based on research & guide by Soneek

## What is it?
Sma5h.CLI and Sma5hMusic are a series of tools to import additional tracks to Smash Ultimate. 
This tool is highly experimental and may not always work as expect.
* **Always keep backups of your files and savegames.**
* **This mod is not safe online!**

## Thanks & Repos of the different tools
1.  Research: soneek
2.  Testing: Demonslayerx8, Segtendo
3.  Icon: Segtendo
4.  prcEditor: https://github.com/BenHall-7/paracobNET - BenHall-7
5.  paramLabels: https://github.com/ultimate-research/param-labels - BenHall-7, jam1garner, Dr-HyperCake, Birdwards, ThatNintendoNerd, ScanMountGoat, Meshima, Blazingflare, TheSmartKid, jugeeya, Demonslayerx8
6.  msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor - IcySon55, exelix11
7.  nus3audio: https://github.com/jam1garner/nus3audio-rs - jam1garner
8.  bgm-property: https://github.com/jam1garner/smash-bgm-property - jam1garner
9.  VGAudio: https://github.com/Thealexbarney/VGAudio - Thealexbarney, soneek, jam1garner, devlead, Raytwo, nnn1590
10.  vgmstream: https://github.com/vgmstream/vgmstream - bnnm, kode54, NicknineTheEagle, bxaimc, Thealexbarney
All contributors: https://github.com/vgmstream/vgmstream/graphs/contributors
11. CrossArc: https://github.com/Ploaj/ArcCross Ploaj, ScanMountGoat, BenHall-7, shadowninja108, jam1garner, M-1-RLG

## CLI
### Setup
1.  In the "Resources" folder, extract the msbt files in /game/ui/message/ using CrossArc
2.  In the "Resources" folder, extract ui_bgm_db.prc and ui_gametitle_db.prc in /game/ui/params/database/
3.  In the "Resources" folder, extract any nus3bank from Ultimate in /nus3bank and rename 'template.nus3bank'
4.  In the "Resources" folder, make sure nusbank_ids.csv, param_labels.csv is there
5.  In the "Tools" folder, make sure VGAudioCli.exe, paracobNET.dll, MsbtEditor.dll, Nus3Audio/nus3audio.exe and BgmProperty/bgm-property.exe/bgm_hashes.txt are there

### How to create Music Mods
1.  Create a folder in /Mods/MusicMods. This folder can contains several audio files and a metadata file 'metadata_mod.json' or 'metadata_mod.csv'. An example is provided.
2.  Since the GUI, the format of the metadata_mod.json changed. However existing mods should be automatically updated to support the new format.
3.  IDSP and LOPUS support. BRSTM support through on the fly conversion. Nus3audio can be imported "as is" but you need to make sure the file is properly generated.
4.  Keep all non localized fields lowercase.
5.  Make sure the series_id value exists within the game (check 'valid_series.txt' in Resources)!
6.  Make sure the fields values "record_type" exist within the game (original, arrange or new_arrange).
7.  If you enable AudioCaching, the nus3audio files will only be generated once. Be aware that if you use a prefix the name of the files might keep changing and thus generating more files in the cache.
8.  To add songs to a playlist, edit the file playlist_override.json

## GUI
For instructions check the wiki https://github.com/Deinonychus71/Sma5hMusic/wiki

## Daisy chaining mods - adding new functionalities (WIP)
1.  The code was reworked to allow for additional mods/code to execute on top of Sma5hMusic.
2.  A state manager takes care of load/parsing game file to c# models. Each new mod can request its own game resource in the form of /ui/param/database/xxx (for example)
3.  This object is kept in memory, and can be modified sequentially by a succession of mod
4.  When mods are finished running (or a user click Build in future UI), the state manager will write back all the game files to an output folder.
5.  Look at the Script.cs in Sma5h.CLI to get started
6.  For now only a few files / filetypes can be loaded from state manager, register new resource providers to support more formats.

## Todo
1.  Sma5h: start suddendeath, start transition and whatever can be figured out
2.  More cleanup
3.  Loading external mods/resource providers
4.  Add tests against collisions when adding new songs
