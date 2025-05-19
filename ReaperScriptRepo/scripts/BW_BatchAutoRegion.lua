--[[
 @author Ben Wolland
 @version 1.0.0
 @description This scripts job is to generate regions around selected items
 There is a basic interface for allowing users to specific how many regions they want there to be
 the script will find all the numbers that divide evenly into the number of items selected
 it will then offer the user the ability to create regions that wrap the correct number of items with a region
 based on the target number of regions and the total number of items that need to be wrapped
 In other words, this script is only useful if you want to region groups of items that have the same number of items in those groups
 This is mainly useful for sound designers creating variations of sounds by duplicating items 

]]--



local ctx = reaper.ImGui_CreateContext('My script', reaper.ImGui_ConfigFlags_NoSavedSettings())
local sans_serif = reaper.ImGui_CreateFont('sans-serif', 13)
reaper.ImGui_Attach(ctx, sans_serif)
local buttonwidth, buttonheight = 100,25
local numRegions = reaper.CountSelectedMediaItems(0)
local oldNumRegions = numRegions
local regionTable = {}
local selectedItemTable = {}
local selItemKey = {}
local divisorsTable = {}

local function countTable(t)
  local count = 0
  for _,v in pairs(t) do
    count = count + 1
  end
  return count
end

function checkSelectionChanged()
  local count = reaper.CountSelectedMediaItems(0)
  if count ~= countTable(selectedItemTable) then
    return true
  else
    foundDifference = false
    for ix = 1, #selItemKey do
      local key = selItemKey[ix]
      for i = 0, count - 1 do
        local mediaitem = reaper.GetSelectedMediaItem(0,i) 
        if mediaitem == selectedItemTable[key] then
          -- found a match move on to next one
          foundDifference = false
          break
        end
        foundDifference = true
      end
    end
    return foundDifference
  end
end

local function updateItemTable()
  selectedItemTable = {}
  selItemKey = {}
  local itemcount = reaper.CountSelectedMediaItems(0)
  for i=0,itemcount-1 do
    local mediaitem = reaper.GetSelectedMediaItem(0,i)
    local pos = reaper.GetMediaItemInfo_Value(mediaitem,"D_POSITION")
    table.insert(selItemKey,pos)
    selectedItemTable[pos] = mediaitem
  end
  table.sort(selItemKey)
end


local function findDivisors(n)
    local divisors = {}
    for i = 1, math.sqrt(n) do
        if n % i == 0 then
            table.insert(divisors, i)
            if i ~= n / i then
                table.insert(divisors, n / i)
            end
        end
    end
    table.sort(divisors) -- Sort the divisors for clarity
    return divisors
end


function debugBreak(msg)
  reaper.ShowMessageBox(tostring(msg),"DEBUG BREAK!",0)
end

local function updateRegions()

  --clear regions that existed before
  for k,v in ipairs(regionTable) do
    reaper.DeleteProjectMarker(0,v,true)
  end
  regionTable = {}
  
  if not numRegions then 
    numRegions = 1
    oldNumRegions = numRegions
  end
  
  if numRegions > 0 then
    local divisor = math.ceil(reaper.CountSelectedMediaItems(0) / numRegions)
    
    for i=1, countTable(selectedItemTable),divisor do
      local key = selItemKey[i]
      local mediaitem = selectedItemTable[key]
      local tk = reaper.GetMediaItemTake(mediaitem,0)
      local retval, itemName = reaper.GetSetMediaItemTakeInfo_String( tk, "P_NAME", "", false)
      if divisor > 1 then
        mediaitem = selectedItemTable[selItemKey[i + divisor - 1]]
        local rgnstart,rgnend = key,
        reaper.GetMediaItemInfo_Value(mediaitem,"D_LENGTH")+ selItemKey[i + divisor - 1]
        table.insert(regionTable,reaper.AddProjectMarker(0,true,rgnstart,rgnend,itemName,0))
      else
        local rgnstart,rgnend = key,
        reaper.GetMediaItemInfo_Value(mediaitem,"D_LENGTH")+ key
        table.insert(regionTable,reaper.AddProjectMarker(0,true,rgnstart,rgnend,itemName,0))
      end
    end
  end 
end

function findClosestValue(numbers, target, direction)
    local closest = nil
    local minDifference = math.huge  -- Start with an infinitely large difference
    for _, num in ipairs(numbers) do
        if (direction == 1 and num >= target) or (direction == -1 and num <= target) then
            local difference = math.abs(num - target)
            if difference < minDifference then
                minDifference = difference
                closest = num
            end
        end
    end
    return closest
end

function myWindow()
  rv,numRegions = reaper.ImGui_InputInt(ctx, 'regions', numRegions)
  if checkSelectionChanged() then
    updateItemTable()
    numRegions = countTable(selectedItemTable)
    oldNumRegions = numRegions
    divisorsTable = findDivisors(numRegions)
    updateRegions() 
  end 
  --do not allow the number box to go higher than the number of items that are selected
  local direction = numRegions - oldNumRegions
  if direction ~= 0 then
    numRegions = findClosestValue(divisorsTable,numRegions,direction)
  end
  if numRegions ~= oldNumRegions then
    oldNumRegions = numRegions
    updateRegions()
  end
end

function main()
  reaper.ImGui_PushFont(ctx, sans_serif)
  reaper.ImGui_SetNextWindowSize(ctx, 300, 75, reaper.ImGui_Cond_FirstUseEver())
  local visible, open = reaper.ImGui_Begin(ctx, 'Batch Auto Region Tool', true)
  if visible then
    myWindow()
    reaper.ImGui_End(ctx)
  end
  reaper.ImGui_PopFont(ctx)
  
  if open then
    reaper.defer(main)
  end
end

reaper.defer(main)
