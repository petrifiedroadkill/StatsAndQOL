//Expanded Ban Sys
//Script By Darktiger
//Note Requires ServerGui
$dtBan::version = 1.0;

$dtStats::joinHist = 144;//51 per page
$dtStats::BanListFile = $Host::dtStatsBanListFile $= "" ? ($Host::dtStatsBanListFile = "prefs/dtBanlist.cs") : $Host::dtStatsBanListFile;
$dtStats::WhtListFile = $Host::dtStatsWhtListFile $= "" ? ($Host::dtStatsWhtListFile = "prefs/whtList.cs") : $Host::dtStatsWhtListFile;

////////////////////////////////////////////////////////////////////////////////
//Ban System
////////////////////////////////////////////////////////////////////////////////

//$Host::KickBanTime = 20; is 20 Minutes
//$Host::BanTime = 43200; is One Month
//$Host::BanTime = 129600; is Three Months
//$Host::BanTime = 259200; is Six Months
//$Host::BanTime = 518400; is 1 year
//$Host::BanTime = 1000000 or "BAN"; is Until you unban them (Forever)


function loadDTBanlist(){
   if(!$dtBanLoad){
      $dtBanList::Count = 0;
      $dtJoinListCount = 0;
      $dtBanLoad=1;
      if(isFile($dtStats::BanListFile)){
         exec($dtStats::BanListFile);
         if(isObject(dtBanList)){
            RootGroup.add(dtBanList);
            for (%i = 0; %i <  dtBanList.getCount(); %i++){//keeps less junk in the ban file
               %obj = dtBanList.getObject(%i);
               %delta =  getTimeDelta(%obj.banDateTime);
               if (%delta > %obj.banLengthMin){
                  unbanUserObj(%obj);
                  %i = 0;// reset
               }
               else{
                  if(%obj.guid !$= "")
                     $dtBanTemp::GUID[%obj.guid ] = %obj;
                  if(%obj.ip !$= "")
                     $dtBanTemp::IP[%obj.ip] = %obj;
               }
            }
         }
      }

      if(isFile($dtStats::WhtListFile)){
          $dtWhtList:WLCount = 0;
         exec($dtStats::WhtListFile);
         if(isObject(serverSafeList)){
           RootGroup.add(serverSafeList);
            for (%i = 0; %i <  serverSafeList.getCount(); %i++){
               %obj = serverSafeList.getObject(%index);
               $dtWhtList::WhiteList[%obj.guid] = %obj;
            }
         }
      }
   }
}

package dtBanSys{
   //Keep track of gags (Disconnecting and Reconnecting)
   function GameConnection::onDrop(%client, %reason){
      %ip = getClientCleanIP(%client);
      $chatGagged[%ip] =  $chatGagged[%client.guid] = (%client.isGagged == 1); //save status of this
      parent::onDrop(%client, %reason);
   }

   //Reapply the gag
   function GameConnection::onConnect( %client, %name, %raceGender, %skin, %voice, %voicePitch ){
      if (banList_checkClient(%client, getField(%client.t2csri_authInfo, 3))){
         return 0;
      }
      parent::onConnect( %client, %name, %raceGender, %skin, %voice, %voicePitch );
      %client.isGagged = ($chatGagged[getClientCleanIP(%client)]  || $chatGagged[%client.guid]); //restore status
   }

   function BanList::add(%guid, %ipAddress, %time){

      %time = (%time $= "") ? 100000 : %time;
      %name = getClientBanName(%guid, %ipAddress);
      %bareIP = getCleanIP(%ipAddress);
      //error("GUID" SPC %guid SPC "IP" SPC %bareIP);
      if(!isObject($dtBanTemp::GUID[%guid]) && !isObject($dtBanTemp::IP[%bareIP])){
         if(!isObject(dtBanList)){
            new simGroup(dtBanList);
            RootGroup.add(dtBanList);
         }
         %guid = (%guid > 0) ? %guid : 0;
         %banObj = new scriptObject(){
            name = %name;
            guid = %guid;
            ip =  %bareip;
            banDateTime = dtMarkDate(); 
            banLengthMin = %time;
         };
         dtBanList.add(%banObj);
         if(%bareIP !$= "0")
            $dtBanTemp::IP[%bareIP] = %banObj;
         if(%guid){
            $dtBanTemp::GUID[%guid] = %banObj;
            rmvWhiteListGuid(%guid);
         }
      }
      saveBanList();
   }

};

function dtIsAdmin(%client,%guid){

   %totalRecords = getFieldCount( $Host::AdminList );
   for(%i = 0; %i < %totalRecords; %i++)
   {
      %record = getField( getRecord( $Host::AdminList, 0 ), %i);
      if(%record == %guid)
         return true;
   }

   %totalRecords = getFieldCount( $Host::superAdminList );
   for(%i = 0; %i < %totalRecords; %i++)
   {
      %record = getField( getRecord( $Host::superAdminList, 0 ), %i);
      if(%record == %guid)
         return true;
   }

   return false;
}

function banList_checkClient(%client, %guid){// only one we care about in whitelist mode
   //error("banlist check" SPC "client" SPC  %client SPC "Guid" SPC %guid);
   %objA = $dtBanTemp::GUID[%guid];
   %ip = getClientCleanIP(%client);
   %objB = $dtBanTemp::IP[%ip];
   %obj = (isObject(%objA) ==  1) ? %objA : %objB;
   if (isObject(%obj) && %obj.banDateTime > 0){
      %delta =  getTimeDelta(%obj.banDateTime);
      if (%delta < %obj.banLengthMin){
         pushFailJoin(%obj.name, %guid, 0, "Kick/Ban" SPC %obj.banLengthMin - %delta SPC "Minutes Left",1);

         %client.setDisconnectReason("You are not allowed to play on this server.");
         %client.delete();
         return 1;
      }
      else{
         unbanUserObj(%obj);
      }
   }
   if(isObject(%objA)){
      %realName = getField(%client.t2csri_authInfo, 0 );
      if(%realName !$= "")
         %name = trim(%realName);
      else
         %name = trim(stripChars( detag( getTaggedString( %fc.name ) ), "\cp\co\c6\c7\c8\c9\c0" ));

      %safe = ( dtIsAdmin(%client,%guid) || isObject($dtWhtList::WhiteList[%guid]));
      if(!%safe){
         pushFailJoin(%name, %guid, 0, "Not Whitelisted", 0);
         if($dtServerVars::WhiteListMode){
            %client.setDisconnectReason("Server is locked, please message admin or wait for approval");
            %client.delete();
            return 1;
         }
      }

     // this is here in case of banned ip is a whitelisted account
      if($dtServerVars::IPBanListMode && $dtIPList[%ip] && !isObject($dtWhtList::WhiteList[%guid])){
         pushFailJoin(%name, %client.guid, %ip, "IP Ban List", 2);
         %client.setDisconnectReason("You are not allowed to play on this server.");
         %client.delete();
         return 1;
      }
   }
   return 0;
}

function pushFailJoin(%name, %guid, %ip, %reason, %type){// rolling buffer
   if(%guid && ! $dtJoinListGuid[%guid]){
      if($dtJoinListCount < $dtStats::joinHist){// limit the list size
         if($dtJoinListCount > 0){
             for (%i = $dtJoinListCount - 1; %i >= 0; %i--) {
                 $dtJoinList[%i + 1] = $dtJoinList[%i];
             }
         }
         $dtJoinListGuid[%guid] = 1;
         $dtJoinList[0] = %name TAB %guid TAB %ip TAB %reason TAB %type;
         $dtJoinListCount++;
      }
      else{
         for (%i = $dtJoinListCount - 1; %i >= 0; %i--) {
            $dtJoinList[%i + 1] = $dtJoinList[%i];
         }
         $dtJoinListGuid[getField($dtJoinList[$dtJoinListCount],1)] = "";// clear out the last one
         $dtJoinList[0] = %name TAB %guid TAB %ip TAB %reason TAB %type;
      }
   }
}


function pushWhiteList(%guid,%name){
  if(!isObject(serverSafeList)){
     new simGroup(serverSafeList);
     RootGroup.add(serverSafeList);
  }
  if(!$dtWhtList::WhiteList[%guid]){
   if(%name $= "")
      %name = "NONAME" @ %guid;

   %id = new scriptObject(){
      name = %name;
      guid = %guid;
   };
    $dtWhtList::WhiteList[%guid] = %id;
    serverSafeList.add(%id);
  }
  saveWhtList();
}

function rmvWhiteListGuid(%guid){
   %obj = $dtWhtList::WhiteList[%guid];
   if(isObject(%obj)){
      error("Player" SPC %obj.name SPC %obj.guid SPC "Removed");
      $dtWhtList::WhiteList[%guid] = "";
      %obj.delete();
      saveWhtList();
   }
}
function rmvWhiteListIndex(%index){
   %obj = serverSafeList.getObject(%index);
   if(isObject(%obj)){
      $dtWhtList::WhiteList[%obj.guid] = "";
      error("Player" SPC %obj.name SPC %obj.guid SPC "Removed");
      %obj.delete();
      saveWhtList();
   }
}

function unbanUserObj(%obj){
   if(isObject(%obj)){
      $dtBanTemp::IP[%obj.ip] = "";
      $dtBanTemp::GUID[%obj.guid] = "";
      error(%obj.name SPC %obj.guid SPC "UNBANNED");
      %obj.delete();
      saveBanList();
   }
}

function unbanIndex(%index){
   if(!%index){
       for (%i = 0; %i <  dtBanList.getCount(); %i++){//keeps less junk in the ban file
         %obj = dtBanList.getObject(%i);
         error(%i SPC %obj.name SPC %obj.guid SPC strReplace(%obj.ip, "_", "."));
       }
       error("Type unbanIndex(%index) replace %index with the number next to the players name, NOTE numbers change after each unban");
   }
   else{
      %obj = dtBanList.getObject(%index);
      if(isObject(%obj)){
         $dtBanTemp::IP[%obj.ip] = "";
         $dtBanTemp::GUID[%obj.guid] = "";
         error(%obj.name SPC %obj.guid SPC "UNBANNED");
         %obj.delete();
         saveBanList();
      }
   }
}


function getClientBanName(%guid, %ip){
   %found = 0;
   for (%i = 0; %i <  ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if((%guid > 0 && %client.guid $= %guid) || %client.getAddress() $= %ip){
         %found = 1;
        break;
      }
   }
   if(%found){
      %authInfo = %client.getAuthInfo();
      %realName = getField( %authInfo, 0 );
      if(%realName !$= "")
         %name = %realName;
      else
         %name =  stripChars( detag( getTaggedString( %client.name ) ), "\cp\co\c6\c7\c8\c9\c0" );
      return trim(%name);
  }
  return "NONAME";
}

function getClientCleanIP(%client){// variable access bug workaround
   %ip = %client.getAddress();
   %ip = getSubStr(%ip, 3, strLen(%ip));
   %ip = getSubStr(%ip, 0, strstr(%ip, ":"));
   %ip = strReplace(%ip, ".", "_");
   return %ip;
}

function getCleanIP(%ip){ // variable access bug workaround
    if (getSubStr(%ip, 0, 3) $= "IP:"){
      %ip = getSubStr(%ip, 3, strLen(%ip));
      %ip = getSubStr(%ip, 0, strstr(%ip, ":"));
      %ip = strReplace(%ip, ".", "_");
      return %ip;
    }
    return 0;
}

function saveBanList(){
 if(!isEventPending($banEvent))
   $banEvent = schedule(1000, 0, "banSaveExport", $dtStats::BanListFile);
}

function banSaveExport(%file){
   %fobj = new fileObject();
   RootGroup.add(%fobj);
   %fobj.openForWrite(%file);
   %fobj.writeLine("new SimGroup(dtBanList) {");
   for(%i = 0; %i < dtBanList.getCount(); %i++){
      %obj = dtBanList.getObject(%i);
      %fobj.writeLine("\tnew ScriptObject() { guid = \"" @ %obj.guid @ "\"; ip = \"" @ %obj.ip @ "\"; banDateTime = \"" @ %obj.banDateTime @ "\"; name = \"" @ %obj.name @ "\"; banLengthMin = \"" @ %obj.banLengthMin @ "\"; hide = \"" @ %obj.hide @ "\"; };");
   }
   %fobj.writeLine("};");
   %fobj.close();
   %fobj.delete();
}

function saveWhtList(){
 if(!isEventPending($whtEvent))
   $whtEvent = serverSafeList.schedule(1000,"save",$dtStats::WhtListFile, 0);
}

function buildServerGuidList(){
   deleteVariables("$guidInfo*");
   deleteVariables("$guidList*");
   $guidAvgs = "";
   %fobj = new fileObject();
   RootGroup.add(%fobj);
   $guidListCount = 0;
   %td = 0; %tw = 0; %tm = 0; %tq = 0; %ty = 0;
   for(%r = 0; %r < $dtStats::gameTypeCount; %r++){
      %game = $dtStats::gameType[%r];
      %folderPath = "serverStats/stats/" @ %game @ "/*t.cs";
      %count = getFileCount(%folderPath);
      if(%count){
         for (%i = 0; %i < %count; %i++){
            %file = findNextfile(%folderPath);
            %guid = getField(strreplace(getField(strreplace(%file,"/","\t"),3),"t","\t"),0);
            if(getFieldCount($dtBanTemp::GUID[%guid]) > 0)// skip banned clients
               continue;

            %fobj.openForRead(%file);
            %fobj.readline(); //skip
            %gameCount = strreplace(%fobj.readline(),"%t","\t");

            %d0 = getField(%gameCount,1);%d1 = getField(%gameCount,2);
            %d = (%d0 > %d1) ? %d0 : %d1;
            %w0 = getField(%gameCount,3);%w1 = getField(%gameCount,4);
            %w = (%w0 > %w1) ? %w0 : %w1;
            %m0 = getField(%gameCount,5);%m1 = getField(%gameCount,6);
            %m = (%m0 > %m1) ? %m0 : %m1;
            %q0 = getField(%gameCount,7);%q1 = getField(%gameCount,8);
            %q = (%q0 > %q1) ? %q0 : %q1;
            %y0 = getField(%gameCount,9);%y1 = getField(%gameCount,10);
            %y = (%y0 > %y1) ? %y0 : %y1;

            if(getFieldCount($guidInfo[%guid]) == 6){
               if(getField($guidInfo[%guid],5) < %y){// update if this info is better
                  $guidInfo[%guid] = %name TAB %d TAB %w TAB %m TAB %q TAB %y;
               }
            }
            else{
               if(%d || %w || %m || %q || %y){
                  %td += %d; %tw += %w; %tm += %m; %tq += %q; %ty += %y;
                  %name = getField(strreplace(%fobj.readline(),"%t","\t"),1);
                  $guidInfo[%guid] = %name TAB %d TAB %w TAB %m TAB %q TAB %y;
                  $guidList[$guidListCount]= %guid; $guidListCount++;
                  $guidAvgs = mFloor(%td/$guidListCount) TAB mFloor(%tw/$guidListCount) TAB mFloor(%tm/$guidListCount) TAB mFloor(%tq/$guidListCount) TAB mFloor(%ty/$guidListCount);
               }
            }
            %fobj.close();
         }
      }
   }
   %fobj.delete();
}


if (!isActivePackage(dtBanSys)){
   if(!isFile("scripts/autoexec/dtBanSystem.cs")){
	   activatePackage(dtBanSys);
   }
   else{
      error("Error old ban system in place, delete scripts/autoexec/dtBanSystem.cs and its .dso");
   }
	loadDTBanlist();
	buildServerGuidList();
}

function banList_bareIP(%ip){
   %ip = strReplace(%ip, ".", "_");
   if($dtIPList[%ip]){
      pushFailJoin(%name, %client.guid, %ip, "IP Ban List", 2);
      return 2;
   }
   %obj = $dtBanTemp::IP[%ip];
   if(isObject(%obj) && %obj.banDateTime > 0){
      %delta =  getTimeDelta(%obj.banDateTime);
      if (%delta < %obj.banLengthMin){
         pushFailJoin(%obj.name, %obj.gui, 0, "Kick/Ban" SPC %obj.banDateTime - %delta SPC "Minutes Left", 1);
         return 1;
      }
      else{
         unbanUserObj(%obj);
      }
   }
   return 0;
}

