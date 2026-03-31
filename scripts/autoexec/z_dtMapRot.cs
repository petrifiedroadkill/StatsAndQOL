//Custom Map Rotation
//Script By DarkTiger
//Note Requires ServerGui
$dtCMR::version = 1.0;

////////////////////////////////////////////////////////////////////////////////

function dtBuildMissionList(%reset){
   if(isObject(ML)){
      ML.delete();
   }
   if(isFile("serverStats/mapRot.cs") && !%reset){
      exec("serverStats/mapRot.cs");
      RootGroup.add(ML);
   }
   if(!isObject(ML)){
       new simGroup(ML);
       RootGroup.add(ML);
       ML.curMapList = 0;
   }
   %search = "missions/*.mis";
   %fobject = new FileObject();
   RootGroup.add(%fobject);
   for( %file = findFirstFile( %search ); %file !$= ""; %file = findNextFile( %search ) ){
      %fileName = fileBase( %file ); // get the name
      %name = cleanMapName(%fileName);
      if(!isObject(%name)){
         if ( !%fobject.openForRead( %file ) )
            continue;
         %mObj = new scriptObject(%name){
           file = %fileName;
           name = %name;
         };

         %typeList = "None";
         while ( !%fobject.isEOF() ){
            %line = %fobject.readLine();
            if ( getSubStr( %line, 0, 17 ) $= "// DisplayName = " ){
               %mObj.name = getSubStr( %line, 17, 1000 );
            }
            else if ( getSubStr( %line, 0, 18 ) $= "// MissionTypes = " ){
               %typeList = getSubStr( %line, 18, 1000 );
               if(strstr(%typeList,"CTF") != -1 && strstr(%typeList,"LCTF") == -1){
                  %typeList = %typeList SPC "LCTF";
               }
               if(strstr(%typeList,"CTF") != -1 && strstr(%typeList,"SCtF") == -1){
                  %typeList = %typeList SPC "SCtF";
               }
               break;
            }
         }
         %fobject.close();

         // Don't include single player missions:
         if ( strstr( %typeList, "SinglePlayer" ) != -1 || (strstr( %typeList, "TR2" ) != -1  && !$Host::ClassicLoadTR2Gametype)){
            %mObj.delete();
            continue;
         }



         %mObj.typeList = %typeList;
         for( %word = 0; ( %misType = strlwr(getWord( %typeList, %word )) ) !$= ""; %word++ ){
            for ( %i = 0; %i < ML.TypeCount; %i++ )
               if ( ML.TypeName[%i] $= %misType )
                  break;
            if ( %i == ML.TypeCount ){
               ML.TypeCount++;
               ML.TypeName[%i] = %misType;
               ML.TypeIndex[%misType] = %i;
            }
            %mObj.typeList[%misType] = 1;
            //%mObj.typeList = (%i == 0) ? ML.TypeIndex[%misType] : (%mObj.typeList SPC ML.TypeIndex[%misType]);
            // enable 0 voteOption 1  min 2   max 3 prio 4 week 5 weekBitAsk 6 monthRes 7 monthBitMask 8 eventMap 9 hour 10 min 11 month 12 day 13 year 14 eventSwitch 15 eventTime 16 unsued 17
            %mObj.typeOptions[%misType,0] = 0 TAB 0 TAB 0 TAB 64 TAB 3 TAB 0 TAB "1000000" TAB 0 TAB "1000000000000000000000000000000" TAB 0 TAB 12 TAB 60 TAB 28 TAB 12 TAB 2024 TAB 0 TAB 60 TAB 0;
            %mObj.typeOptions[%misType,1] = 0 TAB 0 TAB 0 TAB 64 TAB 3 TAB 0 TAB "1000000" TAB 0 TAB "1000000000000000000000000000000" TAB 0 TAB 12 TAB 60 TAB 28 TAB 12 TAB 2024 TAB 0 TAB 60 TAB 0;
            %mObj.typeOptions[%misType,2] = 0 TAB 0 TAB 0 TAB 64 TAB 3 TAB 0 TAB "1000000" TAB 0 TAB "1000000000000000000000000000000" TAB 0 TAB 12 TAB 60 TAB 28 TAB 12 TAB 2024 TAB 0 TAB 60 TAB 0;
         }
         ML.add(%mObj);
      }
   }
   %fobject.delete();

   $dtFixedMapCycle = 0;
   for ( %i = 0; %i <  ML.TypeCount; %i++ ){
       $dtFixedMapCount[ ML.TypeName[%i]] = 0;
   }

   if(isFile("serverStats/fixMapRot.cs") && !%reset){
      exec("serverStats/fixMapRot.cs");
   }

   for ( %i = 0; %i < ML.getCount(); %i++ ){// cleanup
      %mapObj = ML.getObject(%i);
      if(!isFile("missions/"@ %mapObj.file @".mis")){
         %mapObj.delete();
         %i--;
      }
   }

   if(!isEventPending($saveML)){
      $saveML = ML.schedule(2000,"save", "serverStats/mapRot.cs" , 0);
      ML.schedule(1000,"save", "serverStats/mapRotBackup.cs" , 0);
   }
}

function saveMapRot(){
   if(!isEventPending($saveML))
      $saveML = ML.schedule(1000,"save", "serverStats/mapRot.cs" , 0);
   if(!isEventPending($saveMR))
      $saveMR = schedule(2000, 0, "export", "$dtFixedMap*", "serverStats/fixMapRot.cs", false );
}

function eventGameStart(%time){
   Game.gameOver();
   CycleMissions();
}

function mapEventCheck(){
   //if(%event && getField(%ms,0)){//eventMap 9 hour 10 min 11 month 12 day 13 year 14;
      //$dtEventMap[$dtEventMapCount] = (%event == 3) TAB %mapObj.file TAB %gameType TAB getFields(%ms,10,14);
      //$dtEventMapCount++;
   //}
   %time = formattimestring("H\tn\td\tm\tyy");
   for(%i = 0; %i < $dtEventMapCount; %i++){
      %fields = $dtEventMap[%i];
      %eTime = getFields(%fields,4,8);
      if(strcmp(%eTime, %time) == 0){
         $voteNextType = 0;
         $voteNextMap = 0;
         $voteNext = 0;
         %mapObj = getField(%fields,2);
         %mapType = getField(%fields,3);
         deleteVariables("$HostMission*");
         deleteVariables("$HostType*");
         if(getField(%fields,0)){// next map
            messageAll('MsgEventMap', '\c2 The next map will be %1 do to a scheduled event, map voteing has been diabled~wfx/misc/hunters_horde.wav',%mapObj.name);
            $dtEventMap = %mapObj TAB %mapType TAB getField(%fields,1);
            $dtEventMapOldTime = $Host::TimeLimit;
            $dtEventMapOldType = $CurrentMissionType;
         }
         else{// force change
            %min = 5;
            messageAll('MsgEventMap', '\c2 Do to a sheduled event, the sever will force change to %1 in %2 min, map voteing has been diabled~wfx/misc/hunters_horde.wav',%mapObj.name,%min);
            schedule(60000*%min, 0, "eventGameStart",getField(%fields,1));
            %ms = $Host::TimeLimit * 60 * 1000;
            $missionStartTime = getSimTime() - (%ms - (60000*%min));
            //$missionStartTime = getSimTime() - (($Host::TimeLimit * 60 * 1000) - (60000*5));
            $dtEventMapOldTime = $Host::TimeLimit;
            $dtEventMap = %mapObj TAB %mapType TAB getField(%fields,1);
            $dtEventMapOldType = $CurrentMissionType;
         }
         break;
      }
   }
   schedule(45000, 0, "mapEventCheck");
}

function pushMissionList(){
   error("pushMissionList");
   deleteVariables("$HostMission*");
   deleteVariables("$HostType*");
   deleteVariables("$dtEventMap*");
   $dtEventMapCount = 0;
   if(ML.curMapList == 3){// Fixed rotation list
      for(%x = 0; %x < ML.TypeCount; %x++){
         %gameType = ML.TypeName[%x];
         for(%i = 0; %i < $dtFixedMapCount[strlwr(%gameType)]; %i++){
            %fields = $dtFixedMapList[%i, strlwr(%gameType)];
            %file = getField(%fields,0);
            %missionName = getField(%fields,1);
            %vote = getField(%fields,2);
            if(%vote){
               %found = false;
               for (%mis = 0; %mis < $HostMissionCount; %mis++){
                  if ($HostMissionFile[%mis] $=%file){
                     %found = true;
                     break;
                  }
               }
               // Not found, add to mission list
               if (!%found){
                  $HostMissionCount++;
                  $HostMissionFile[%mis] = %file;
                  $HostMissionName[%mis] = %file;
                  $BotEnabled[%mis] = isFile("terrains/" @ %file @".nav");
                  $HostMissionName[%mis] = %missionName;
               }

               // Check if gametype has already been loaded
               %found = false;
               for (%type = 0; %type < $HostTypeCount; %type++){
                  if ($HostTypeName[%type] $= %gameType){
                     %found = true;
                     break;
                  }
               }

               // Not found, add to gametype list
               if (!%found){
                  $HostTypeCount++;
                  $HostTypeName[%type] = %gameType;
                  $HostMissionCount[%type] = 0;
               }

               // Add the mission to the gametype
               $HostMission[%type, $HostMissionCount[%type]] = %mis;
               $HostMissionCount[%type]++;

               if($BotEnabled[%mis]){
                  $BotMissionCount[%type]++;
               }
            }
         }
      }
   }
   else{
      for ( %i = 0; %i < ML.getCount(); %i++ ){
         %mapObj = ML.getObject(%i);
         if(isFile("missions/"@ %mapObj.file @".mis")){
            for( %w = 0; ( %gameType = getWord( %mapObj.typeList, %w ) ) !$= ""; %w++ ){
               %ms = %mapObj.typeOptions[strlwr(%gameType), ML.curMapList];

               if(getField(%ms,9) && getField(%ms,0)){//eventMap 9 hour 10 min 11 month 12 day 13 year 14;
                  $dtEventMap[$dtEventMapCount] = getField(%ms,15) TAB getField(%ms,16) TAB %mapObj TAB %gameType TAB getFields(%ms,10,14);
                  $dtEventMapCount++;
               }

               if(getField(%ms,0) && getField(%ms,1) != 2){
                  %found = false;
                  for (%mis = 0; %mis < $HostMissionCount; %mis++){
                     if ($HostMissionFile[%mis] $= %mapObj.file){
                        %found = true;
                        break;
                     }
                  }
                  // Not found, add to mission list
                  if (!%found){
                     $HostMissionCount++;
                     $HostMissionFile[%mis] = %mapObj.file;
                     $HostMissionName[%mis] = %mapObj.file;
                     $BotEnabled[%mis] = isFile("terrains/" @ %mapObj.file @".nav");
                     $HostMissionName[%mis] = %mapObj.name;
                  }

                  // Check if gametype has already been loaded
                  %found = false;
                  for (%type = 0; %type < $HostTypeCount; %type++){
                     if ($HostTypeName[%type] $= %gameType){
                        %found = true;
                        break;
                     }
                  }

                  // Not found, add to gametype list
                  if (!%found){
                     $HostTypeCount++;
                     $HostTypeName[%type] = %gameType;
                     $HostMissionCount[%type] = 0;
                  }

                  // Add the mission to the gametype
                  $HostMission[%type, $HostMissionCount[%type]] = %mis;
                  $HostMissionCount[%type]++;

                  if($BotEnabled[%mis]){
                     $BotMissionCount[%type]++;
                  }
               }
            }
         }
      }
   }
   getMissionTypeDisplayNames();
}

package dtMapRotation{
   function loadMission( %missionName, %missionType, %firstMission ){
      parent::loadMission( %missionName, %missionType, %firstMission );

      %cName = cleanMapName(%missionName);
      if(ML.curMapList != 3){
         $dtMapPlayed[$CurrentMissionType,%cName] = 1;
         if(ML.saveMapPlayed && !isEventPending($saveMapRot)){
            $saveMapRot = schedule(15000, 0, "export", "$dtMapPlayed*", "serverStats/mapPlayRot.cs", false );
         }
      }
      else if(ML.curMapList == 3){
         if(!$dtMissionCycle){//map did not cycle lets update are pointer;
            %mapCount = $dtFixedMapCount[strlwr(%missionType)];
            for(%i = 0; %i < %mapCount; %i++){
               %mn = getField($dtFixedMapList[%i, strlwr(%missionType)],0);
               if(%mn $= %missionName){
                  $dtFixedMapCycle = (%i + 1) % %mapCount;
                  error("TEST" SPC %missionName);
                  if(!isEventPending($saveMR)){
                     $saveMR = schedule(15000, 0, "export", "$dtFixedMap*", "serverStats/fixMapRot.cs", false );
                  }
                  break;
               }
            }
         }
      }
      $dtMissionCycle = 0;
   }

   function CycleMissions(){
      $dtMissionCycle = 1;
      if(!ML.enable){
         parent::CycleMissions();
      }
      else{
         %eMapObj = getField($dtEventMap,0);
         %eMapType = getField($dtEventMap,1);
         %time = getField($dtEventMap,2);
         if(isObject(%eMapObj)){
            $dtEventMap = "";
            $lastMapEvent = 1;
            $Host::TimeLimit  = %time;
            loadMission( %eMapObj.file, %eMapType );
         }
         else{
            if($lastMapEvent){// was the last map an event map if so lets reset some things
               $Host::TimeLimit = $dtEventMapOldTime;
               $CurrentMissionType = $dtEventMapOldType;
               $lastMapEvent = 0;
               buildMissionList();// rebuild vote list
            }
            if(Game.scheduleVote !$= "") // a vote is still running, stop it
               stopCurrentVote();

            echo( "cycling mission. " @ ClientGroup.getCount() @ " clients in game." );

            %nextMission = dtNextMission($CurrentMissionType);
            if(%nextMission $= "") // z0dd - ZOD, 5/17/03. Make sure it's returning a mission, otherwise, repeat.
               %nextMission = $CurrentMission;

            messageAll( 'MsgClient', 'Loading %1 (%2)...', %nextMission, $MissionTypeDisplayName );
            loadMission( %nextMission, $CurrentMissionType);
         }
      }
   }
   function buildMissionList(){
      if(ML.enable && $dtStats::MapStart){
         pushMissionList();
      }
      else{
         parent::buildMissionList();
      }
   }

};

if (!isActivePackage(dtMapRotation)){
   activatePackage(dtMapRotation);
   $dtStats::MapStart = 0;
   dtBuildMissionList(0);
   mapEventCheck();
}
function dtNextMission(%gameType){
   %cc = $dtFixedMapCount[strlwr(%gameType)];
   if(ML.curMapList == 3 && %cc > 0){
      if($dtFixedMapCycle >= %cc){
         $dtFixedMapCycle = 0;
      }
      %missionName = $dtFixedMapList[$dtFixedMapCycle, strlwr(%gameType)];
      $dtFixedMapCycle++;
      if(!isEventPending($saveMR)){
         $saveMR = schedule(15000, 0, "export", "$dtFixedMap*", "serverStats/fixMapRot.cs", false );
      }
      return getField(%missionName,0);
   }

   %gindex = ML.TypeIndex[%gameType];
   %plrCount = ClientGroup.getCount();

   %mapListA = 0;
   %mapListB = 0;
   %mapListF = 0;
   %prioA = 0;
   %prioB = 0;
   %wIndex["Sun"] = 0;%wIndex["Mon"] = 1;%wIndex["Tue"] = 2;%wIndex["Wed"] = 3;%wIndex["Thu"] = 5;%wIndex["Fri"] = 5;%wIndex["Sat"] = 6;
   for ( %i = 0; %i < ML.getCount(); %i++ ){
      %mapObj = ML.getObject(%i);
      // enable 0 voteonly 1  min 2   max 3 prio 4 week 5 weekBitMask 6 monthRes 7 monthBitMask 8 eventMap 9 hour 10 min 11 month 12 day 13 year 14;
      %options = %mapObj.typeOptions[%gameType,ML.curMapList];
      %cName = cleanMapName(%mapObj.file);
      if(getField(%options,0) && getField(%options,1) != 1){ // enable and voteonly check
         if(!$dtMapPlayed[%gameType,%cName]){
            if(%plrCount >= getField(%options,2) && %plrCount <= getField(%options,3) || (%plrCount > 64 && getField(%options,2) > 32)){// min max player
               if(getField(%options,5)){// week limits
                  %dindex = %wIndex[formattimestring("D")];
                  if(getSubStr(getField(%options,6),%dindex,1) == 1){
                     %prio += getField(%options,4);
                     %mapList[%mapListA] = %mapObj;
                     %mapListA++;
                  }
               }
               else if(getField(%options,7)){ // month limits
                  if(getSubStr(getField(%options,8),formattimestring("m")-1,1) == 1){
                     %prioA += getField(%options,4);
                     %mapList[%mapListA] = %mapObj;
                     %mapListA++;
                  }
               }
               else{
                  %prioA += getField(%options,4);
                  %mapList[%mapListA] = %mapObj;
                  %mapListA++;
               }
            }
            else{// does not fit are min max condition
               if(getField(%options,5)){
                  %dindex = %wIndex[formattimestring("D")];
                  if(getSubStr(getField(%options,6),%dindex,1) == 1){
                     %prioB += getField(%options,4);
                     %outlier[%mapListB] = %mapObj;
                     %mapListB++;
                  }
               }
               else if(getField(%options,7)){
                  if(getSubStr(getField(%options,8),formattimestring("m")-1,1) == 1){
                     %prioB += getField(%options,4);
                     %outlier[%mapListB] = %mapObj;
                     %mapListB++;

                  }
               }
               else{
                  %prioB += getField(%options,4);
                  %outlier[%mapListB] = %mapObj;
                  %mapListB++;
               }
            }
         }
         else{
            %failSafe[%mapListF] = %mapObj;
            %mapListF++;
         }
      }
   }

   if(%mapListA){ // min max list
      %randomNum  = getRandom() * %prioA;
      %weight = 0;
      for ( %i = 0; %i < %mapListA; %i++ ){
         %mapObj = %mapList[%i];
         %weight += getField(%mapObj.typeOptions[%gameType, ML.curMapList],4);
         if (%random < %weight){
            return %mapObj.file;
         }
      }
       return %mapList[getRandom(0, %mapListA-1)].file;
   }
   if(%mapListB){// Fail safe list
      deleteVariables("$dtMapPlayed*");
      %random  = getRandom() * %prioB;
      %weight = 0;
      for ( %i = 0; %i < %mapListB; %i++ ){
         %mapObj = %outlier[%i];
         %addWeight += getField(%mapObj.typeOptions[%gameType, ML.curMapList],4);
         if (%random < %weight){
            return %mapObj.file;
         }
      }
      return %outlier[getRandom(0, %mapListB-1)].file;
   }
   if(%mapListF){// Fail safe list
      error("Ran out of valid maps using fail safe list, add more maps for the system to pick from");
      deleteVariables("$dtMapPlayed*");
      return %failSafe[getRandom(0, %mapListF-1)].file;
   }
   error("Map rotation error, no maps found");
}







function mapCyleTest2(%amount){
   deleteVariables("$ttTestCounter*");
   $ttTestCounterC = 0;
   for ( %i = 0; %i < %amount; %i++ ){
      %nextMission = dtNextMission($CurrentMissionType);
      %cName = cleanMapName(%nextMission);
      if(ML.curMapList != 3){
         $dtMapPlayed[$CurrentMissionType,%cName] = 1;
         $ttTestCounter[$CurrentMissionType,%cName]++;
         if(!$ttTestCounterN[%cName]){
            $ttTestCounterN[%cName] = 1;
            $ttTestCounterCN[$ttTestCounterC] = %cName;
            $ttTestCounterC++;
         }
      }
      else{
        error(%cName);
      }
   }
   for ( %i = 0; %i < $ttTestCounterC; %i++ ){
      %cName = $ttTestCounterCN[%i];
      %count = $ttTestCounter[$CurrentMissionType,%cName];
      error(%cName SPC %count);
   }
}


function mapCyleTest(){
      %nextMission = dtNextMission($CurrentMissionType);
      %cName = cleanMapName(%nextMission);
      if(ML.curMapList != 3){
         $dtMapPlayed[$CurrentMissionType,%cName] = 1;
         error(%cName);
      }
      else{
         error("Mode 0" SPC %cName);
      }
}