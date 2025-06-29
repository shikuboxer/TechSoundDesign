--[[
 @author Ben Wolland
 @version 1.0.0
 @description The purpose of this script is to basically create a sampler which behaves like a random container in an audio engine using reasamploatic500 and the round robin feature once you've created the sampler set up on a track using this script, you just need to add midi triggers to it in order to trigger a random sample from your selection

]]--
function Main()

  local selTrackCount =  reaper.CountSelectedTracks( 0 )

  if selTrackCount > 0 then
    --grab media explorer reference
    local mediaExplorer = reaper.JS_Window_Find(reaper.JS_Localize("Media Explorer","common"), true)
    --grab a reference to the list whatever that is
    local mediaExplorerList = reaper.JS_Window_FindEx(mediaExplorer, nil, "SysListView32", "")
    --count how many items have been selected for our loop
    local sel_count, sel_indexes = reaper.JS_ListView_ListAllSelItems(mediaExplorerList)
    local increment = 1.0 / sel_count
    if sel_count > 0 then
      --loop through each selected item by index and grab the filename and path
      local trackfxindx = 0
      for ndx in string.gmatch(sel_indexes, '[^,]+') do 
        local index = tonumber(ndx)
        local filename = reaper.JS_ListView_GetItemText(mediaExplorerList, index, 0)
        local combo = reaper.JS_Window_FindChildByID(mediaExplorer, 1002)
        local edit = reaper.JS_Window_FindChildByID(combo, 1001)
        local path = reaper.JS_Window_GetTitle(edit)
        local filelocation = path.."\\"..filename
        reaper.InsertMedia( filelocation, 1025 )
        local track =  reaper.GetSelectedTrack( 0, 0 )
        --local retval, fxName = reaper.TrackFX_GetFXName( track, i )
        --retval, minval, maxval = reaper.TrackFX_GetParam( track, i, 21 )
        reaper.TrackFX_SetParam( track, trackfxindx, 21, 1.0 )
        reaper.TrackFX_SetParam( track, trackfxindx, 20, 1.0 )
        reaper.TrackFX_SetParam( track, trackfxindx, 8, 0.3 )
        reaper.TrackFX_SetParam( track, trackfxindx, 19, trackfxindx*increment+increment )
        trackfxindx = trackfxindx + 1
      end
    else
      reaper.ShowConsoleMsg("No items selected in the media explorer!")
    end
  else
    reaper.ShowConsoleMsg("No tracks selected to run this script on")
  end

  
end

Main()
