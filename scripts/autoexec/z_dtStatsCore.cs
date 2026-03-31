
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	Stats system for classic and base
//	Script BY: DarkTiger
// Version 11.0 Refactor each system to its own file this is the core stats capture rest are supporting/addons 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//-----------Settings-----------
$dtStats::version = 11.0;
//disable stats system
$dtStats::Enable = $Host::dtStatsEnable $= "" ? ($Host::dtStatsEnable = 1) : $Host::dtStatsEnable;
if(!$dtStats::Enable){ return;}// so it disables with a restart
//set max number of individual game to record
//Note only tested to 100 games, hard cap at 300
$dtStats::MaxNumOfGames = 100;

//how high the player has to be off the ground before it will count
$dtStats::midAirHeight = 10;

//only enable if evo system is not available
$dtStats::midAirMessage =  $Host::dtStatsMidAirMessage $= "" ? ($Host::dtStatsMidAirMessage = 1) : $Host::dtStatsMidAirMessage;
$dtStats::midAirMessage = isFile("scripts/autoexec/MidairDetection.cs") == 0 ? $dtStats::midAirMessage : 0;
//capture best cap times restart required if changed
//only enable if evo system is not available
$dtStats::ctfTimes =  $Host::dtStatsCTFTimes $= "" ? ($Host::dtStatsCTFTimes = 1) : $Host::dtStatsCTFTimes;
$dtStats::ctfTimes =  $Host::ClassicEvoStats $= ""  ?  $dtStats::ctfTimes : 0;

//number of players before it starts counting captimes
$dtStats::ctfTimesPlayerLimit =  $Host::dtStatsCTFTimesPlayerLimit $= "" ? ($Host::dtStatsCTFTimesPlayerLimit = 8) : $Host::dtStatsCTFTimesPlayerLimit;

//Load/saving rates to prevent any server hitching
$dtStats::saveTime = 64;

//auto compiles tournament stats with main stats'
//Note atm tournament stats is hard coded and setup only for CTF
//outputs a ppm image in serverStats/statsImg this can be open/converted with most editors
$dtStats::tmModeCompile = 1;
$dtStats::tmMode =0;

//minimum number avg data to  consider for leaderboards
$dtStats::minAvg = 4;
//minimum  number of games for leaderboards
$dtStats::minGame = 1;

$dtStats::TBMinPlayers = 8;

//sorting speed
$dtStats::sortSpeed = 64;


//To rebuild the leaderboards manually type lStatsCycle(1) into the console;
//This time marks the end of day and to rebuild the leaderboards, best set this time when the server is normally empty or low numbers
$dtStats::buildSetTime = $Host::dtStatsBuildSetTime $= "" ? ($Host::dtStatsBuildSetTime = "5\t00\tam") : $Host::dtStatsBuildSetTime;

// top 15 players per cat, best not to change
$dtStats::topAmount = 15;

//File maintainers to deletes old files
$dtStats::fm  = 1;

//Set 2 or more to enable, this also contorls how much history you want, best to keep this count low
$dtStats::day = 0;//not used
$dtStats::week = 0;//~53
$dtStats::month = 4; //-12
$dtStats::quarter = 0;//not used
$dtStats::year = 0;//not
$dtStats::custom = 12;//not used
// you gain extra days based on time played extra days = gameCount * expireFactor;
// example being 100 games * factor of 0.596 = will gain you 60 extra days but if its over the 90 day max it will be deleted
$dtStats::expireMax = 90;
$dtStats::expireMin = 15;
$dtStats::expireFactor["CTFGame"] = 0.596;
$dtStats::expireFactor["LakRabbitGame"] = 2;
$dtStats::expireFactor["DMGame"] = 6;
$dtStats::expireFactor["LCTFGame"] = 1.2;
$dtStats::expireFactor["SCtFGame"] = 1.2;
$dtStats::expireFactor["ArenaGame"] = 2;
$dtStats::expireFactor["SiegeGame"] = 10;

//debug stuff
$dtStats::dev = isFile("scripts/autoexec/dev.cs");
$Host::ShowIngamePlayerScores  = 1;
$dtStats::enableRefresh = 0;// keep off unless testing, auto updates the score hud when open
$dtStats::debugEchos = $dtStats::dev;// echos function calls
$dtStats::returnToMenuTimer = ($dtStats::dev == 1) ?  ((60*1000) * 30) : ((60*1000)* 2);

//---------------------------------
//  Torque Markup Language - TML
//  Reference Tags
//---------------------------------
//<font:name:size>Sets the current font to the indicated name and size. Example: <font:Arial Bold:20>
//<tag:ID>Set a tag to which we can scroll a GuiScrollContentCtrl (parent control of the guiMLTextCtrl)
//<color:RRGGBBAA>Sets text color. Example: <color:c8c8c8> will display red text.
//<linkcolor:RRGGBBAA>Sets the color of a hyperlink.
//<linkcolorHL:RRGGBBAA>Sets the color of a hyperlink that is being clicked.
//<shadow:x:y>Add a shadow to the text, displaced by (x, y).
//<shadowcolor:RRGGBBAA>Sets the color of the text shadow.
//<bitmap:filePath>Displays the bitmap image of the given file. Note this is hard coded in t2 to only look in texticons in textures
//<spush>Saves the current text formatting so that temporary changes to formatting can be made. Used with <spop>.
//<spop>Restores the previously saved text formatting. Used with <spush>.
//<sbreak>Produces line breaks, similarly to <br>. However, while <br> keeps the current flow (for example, when flowing around the image), <sbreak> moves the cursor position to a new line in a more global manner (forces our text to stop flowing around the image, so text is drawn at a new line under the image).
//<just:left>Left justify the text.
//<just:right>Right justify the text.
//<just:center>Center the text.
//<a:URL>content</a>Inserts a hyperlink into the text. This can also be used to call a function class::onURL
//<lmargin:width>Sets the left margin.
//<lmargin%:width>Sets the left margin as a percentage of the full width.
//<rmargin:width>Sets the right margin.
//<clip:width>content</clip>Produces the content, but clipped to the given width.
//<div:bool>Use the profile's fillColorHL to draw a background for the text.
//<tab:##[,##[,##]]>Sets tab stops at the given locations.
//<br>Forced line break.

// colors used
//00dcd4 Darker blue
//0befe7 Lighter blue
//00dc00 Green
//0099FF Blue
//FF9A00 Orange
//05edad Teal
//FF0000 Red
//dcdcdc White
//02d404 T2 Green
//fb3939 Lighter Red

 

////////////////////////////////////////////////////////////////////////////////
//                           Supported Game Types
////////////////////////////////////////////////////////////////////////////////
//Array for processing stats
$dtStats::gameType[0] = "CTFGame";
$dtStats::gameType[1] = "LakRabbitGame";
$dtStats::gameType[2] = "DMGame";
$dtStats::gameType[3] = "LCTFGame";
$dtStats::gameType[4] = "SCtFGame";
$dtStats::gameType[5] = "ArenaGame";
//$dtStats::gameType[5] = "SiegeGame";
$dtStats::gameTypeCount = 6;
//short hand name
$dtStats::gtNameShort["CTFGame"] = "CTF";
$dtStats::gtNameShort["LakRabbitGame"] = "LakRabbit";
$dtStats::gtNameShort["DMGame"] = "DM";
$dtStats::gtNameShort["LCTFGame"] = "LCTF";
$dtStats::gtNameShort["SCtFGame"] = "LCTF";
$dtStats::gtNameShort["ArenaGame"] = "Arena";
//$dtStats::gtNameShort["SiegeGame"] = "Siege";

$dtStats::gtNameType["CTF"] = "CTFGame";
$dtStats::gtNameType["LCTF"] = "SCtFGame";
$dtStats::gtNameType["Arena"] = "ArenaGame";
//Display name
$dtStats::gtNameLong["CTFGame"] = "Capture the Flag";
$dtStats::gtNameLong["LakRabbitGame"] = "LakRabbit";
$dtStats::gtNameLong["DMGame"] = "Deathmatch";
$dtStats::gtNameLong["LCTFGame"] = "Light CTF";
$dtStats::gtNameLong["SCtFGame"] = "Light CTF";
$dtStats::gtNameLong["ArenaGame"] = "Arena";
//$dtStats::gtNameLong["SiegeGame"] = "Siege";

//varTypes
$dtStats::varType[0] = "Game";//Game only stat
$dtStats::varType[1] = "TG";  //Total & Game stat
$dtStats::varType[2] = "TTL"; //Total only stat
$dtStats::varType[3] = "Max"; //Largest value
$dtStats::varType[4] = "Min"; //Smallest value sorted inverse
$dtStats::varType[5] = "Avg"; //Average value
$dtStats::varType[6] = "AvgI";//Average value sorted inverse
$dtStats::varTypeCount = 7;

function dtStatsResetGobals(){
   for(%v = 0; %v < $dtStats::varTypeCount; %v++){
      %varType = $dtStats::varType[%v];
      $dtStats::FC[%varType] = 0;
      for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
         %gameType = $dtStats::gameType[%i];
         $dtStats::uGFC[%gameType] = 0;
         $dtStats::FC[%gameType,%varType] =0;
         $dtStats::FCG[%gameType,%varType] =0;
      }
   }
   $dtStats::unusedCount = 0;
}dtStatsResetGobals();

///////////////////////////////////////////////////////////////////////////////
//                             		CTF
///////////////////////////////////////////////////////////////////////////////
//gametype values with in the gametype file CTFGame.cs
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["CTFGame","Avg"]++,"CTFGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["CTFGame","Max"]++,"CTFGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagCaps";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "carrierKills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagReturns";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "scoreMidAir";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "scoreHeadshot";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "scoreRearshot";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "escortAssists";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "defenseScore";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "offenseScore";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagDefends";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genSolRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "SensorRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "TurretRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "StationRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "VStationRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "mpbtstationRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "solarRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "sentryRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depSensorRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depInvRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depTurretRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "tkDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "sensorDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "turretDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "iStationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "vstationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "mpbtstationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "solarDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "sentryDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depSensorDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depTurretDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depStationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "vehicleScore";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "vehicleBonus";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genDefends";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "turretKills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "mannedTurretKills";
/////////////////////////////////////////////////////////////////////////////
//gametype values in this script
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "winCount";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "lossCount";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "destruction";
$dtStats::FV[$dtStats::FC["CTFGame","Min"]++,"CTFGame","Min"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["CTFGame","AvgI"]++,"CTFGame","AvgI"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["CTFGame","Max"]++,"CTFGame","Max"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","Avg"]++,"CTFGame","Avg"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","Avg"]++,"CTFGame","Avg"] = "capEfficiency";
$dtStats::FV[$dtStats::FC["CTFGame","Avg"]++,"CTFGame","Avg"] = "winLostPct";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "wildRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "assaultRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "mobileBaseRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "scoutFlyerRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "bomberFlyerRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "hapcFlyerRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "wildRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "assaultRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "mobileBaseRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "scoutFlyerRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "bomberFlyerRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "hapcFlyerRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "roadKills";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "roadDeaths";
$dtStats::FV[$dtStats::FC["CTFGame","Game"]++,"CTFGame","Game"] = "dtTeam";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "repairs";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "MotionSensorDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "PulseSensorDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "SensorsDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "InventoryDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "TurretOutdoorDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "TurretIndoorDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "TurretsDep";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "TotalDep";
$dtStats::FV[$dtStats::FC["CTFGame","Game"]++,"CTFGame","Game"] = "teamScore";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "concussFlag";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "depInvyUse";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "mpbGlitch";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "flagCatch";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "maFlagCatch";
$dtStats::FV[$dtStats::FC["CTFGame","Max"]++,"CTFGame","Max"] = "flagCatchSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","Max"]++,"CTFGame","Max"] = "maFlagCatchSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "flagToss";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "flagTossCatch";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "flagTossGrab";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "interceptedFlag";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "maInterceptedFlag";
$dtStats::FV[$dtStats::FC["CTFGame","Max"]++,"CTFGame","Max"] = "interceptSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","Max"]++,"CTFGame","Max"] = "interceptFlagSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "friendlyFire";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeOnTeamZero";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeOnTeamOne";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeOnTeamTwo";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "matchRunTime";
$dtStats::FV[$dtStats::FC["CTFGame","Game"]++,"CTFGame","Game"] = "teamOneCapTimes";
$dtStats::FV[$dtStats::FC["CTFGame","Game"]++,"CTFGame","Game"] = "teamTwoCapTimes";
$dtStats::FV[$dtStats::FC["CTFGame","Game"]++,"CTFGame","Game"] = "returnPts";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "OffKills";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "DefKills";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "OffKillsL";// kill as armor size
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "OffKillsM";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "OffKillsH";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "DefKillsL";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "DefKillsM";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "DefKillsH";// kill as armor size

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeNearTeamFS";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeFarTeamFS";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeNearEnemyFS";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeFarEnemyFS";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeNearFlag";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "timeNearEnemyFlag";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "gravCycleDes";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "assaultTankDes";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "MPBDes";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "turbogravDes";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "bomberDes";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "heavyTransportDes";

$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "stalemateReturn";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "flagTimeMin";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "flagGrabAtStand";

///////////////////////////////////////////////////////////////////////////////
//                             		LCTF
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["LCTFGame","Avg"]++,"LCTFGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["LCTFGame","Max"]++,"LCTFGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagCaps";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "carrierKills";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagReturns";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "scoreMidAir";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "scoreHeadshot";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "scoreRearshot";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "escortAssists";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "defenseScore";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "offenseScore";
$dtStats::FVG[$dtStats::FCG["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagDefends";
// in this script only
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "winCount";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "lossCount";
$dtStats::FV[$dtStats::FC["LCTFGame","Min"]++,"LCTFGame","Min"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["LCTFGame","AvgI"]++,"LCTFGame","AvgI"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["LCTFGame","Max"]++,"LCTFGame","Max"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["LCTFGame","Avg"]++,"LCTFGame","Avg"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["LCTFGame","Avg"]++,"LCTFGame","Avg"] = "capEfficiency";
$dtStats::FV[$dtStats::FC["LCTFGame","Avg"]++,"LCTFGame","Avg"] = "winLostPct";
$dtStats::FV[$dtStats::FC["LCTFGame","Game"]++,"LCTFGame","Game"] = "dtTeam";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "destruction";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "repairs";
$dtStats::FV[$dtStats::FC["LCTFGame","Game"]++,"LCTFGame","Game"] = "teamScore";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "concussFlag";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagCatch";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "maFlagCatch";
$dtStats::FV[$dtStats::FC["LCTFGame","Max"]++,"LCTFGame","Max"] = "flagCatchSpeed";
$dtStats::FV[$dtStats::FC["LCTFGame","Max"]++,"LCTFGame","Max"] = "maFlagCatchSpeed";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagToss";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagTossCatch";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "interceptedFlag";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "maInterceptedFlag";
$dtStats::FV[$dtStats::FC["LCTFGame","Max"]++,"LCTFGame","Max"] = "interceptSpeed";
$dtStats::FV[$dtStats::FC["LCTFGame","Max"]++,"LCTFGame","Max"] = "interceptFlagSpeed";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "friendlyFire";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "stalemateReturn";
////////////////////////////Unused LCTF Vars/////////////////////////////////////
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "tkDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "genDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "sensorDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "turretDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "iStationDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "vstationDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "mpbtstationDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "solarDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "sentryDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "depSensorDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "depTurretDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "depStationDestroys";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "vehicleScore";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "vehicleBonus";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "genDefends";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "escortAssists";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "turretKills";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "mannedTurretKills";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "genRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "SensorRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "TurretRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "StationRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "VStationRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "mpbtstationRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "solarRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "sentryRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "depSensorRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "depInvRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "depTurretRepairs";
$dtStats::uGFV[$dtStats::uGFC["LCTFGame"]++,"LCTFGame"] = "returnPts";

$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeOnTeamZero";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeOnTeamOne";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeOnTeamTwo";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "matchRunTime";
$dtStats::FV[$dtStats::FC["LCTFGame","Game"]++,"LCTFGame","Game"] = "teamOneCapTimes";
$dtStats::FV[$dtStats::FC["LCTFGame","Game"]++,"LCTFGame","Game"] = "teamTwoCapTimes";

$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "OffKills";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "DefKills";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeNearTeamFS";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeFarTeamFS";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeNearEnemyFS";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeFarEnemyFS";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeNearFlag";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "timeNearEnemyFlag";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagTimeMin";
$dtStats::FV[$dtStats::FC["LCTFGame","TG"]++,"LCTFGame","TG"] = "flagGrabAtStand";
//////////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["SCtFGame","Max"]++,"SCtFGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagCaps";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "carrierKills";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagReturns";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "scoreMidAir";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "scoreHeadshot";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "scoreRearshot";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "escortAssists";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "defenseScore";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "offenseScore";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagDefends";
// in this script only
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "winCount";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "lossCount";
$dtStats::FV[$dtStats::FC["SCtFGame","Min"]++,"SCtFGame","Min"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["SCtFGame","AvgI"]++,"SCtFGame","AvgI"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["SCtFGame","Max"]++,"SCtFGame","Max"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "capEfficiency";
$dtStats::FV[$dtStats::FC["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "winLostPct";
$dtStats::FV[$dtStats::FC["SCtFGame","Game"]++,"SCtFGame","Game"] = "dtTeam";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "destruction";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "repairs";
$dtStats::FV[$dtStats::FC["SCtFGame","Game"]++,"SCtFGame","Game"] = "teamScore";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "concussFlag";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagCatch";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "maFlagCatch";
$dtStats::FV[$dtStats::FC["SCtFGame","Max"]++,"SCtFGame","Max"] = "flagCatchSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","Max"]++,"SCtFGame","Max"] = "maFlagCatchSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagToss";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagTossCatch";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "interceptedFlag";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "maInterceptedFlag";
$dtStats::FV[$dtStats::FC["SCtFGame","Max"]++,"SCtFGame","Max"] = "interceptSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","Max"]++,"SCtFGame","Max"] = "interceptFlagSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "friendlyFire";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "stalemateReturn";
////////////////////////////Unused LCTF Vars/////////////////////////////////////
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "tkDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "genDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "sensorDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "turretDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "iStationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "vstationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "mpbtstationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "solarDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "sentryDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depSensorDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depTurretDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depStationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "vehicleScore";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "vehicleBonus";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "genDefends";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "escortAssists";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "turretKills";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "mannedTurretKills";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "genRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "SensorRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "TurretRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "StationRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "VStationRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "mpbtstationRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "solarRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "sentryRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depSensorRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depInvRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depTurretRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "returnPts";

$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeOnTeamZero";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeOnTeamOne";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeOnTeamTwo";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "matchRunTime";
$dtStats::FV[$dtStats::FC["SCtFGame","Game"]++,"SCtFGame","Game"] = "teamOneCapTimes";
$dtStats::FV[$dtStats::FC["SCtFGame","Game"]++,"SCtFGame","Game"] = "teamTwoCapTimes";

$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "OffKills";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "DefKills";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeNearTeamFS";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeFarTeamFS";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeNearEnemyFS";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeFarEnemyFS";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeNearFlag";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "timeNearEnemyFlag";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagTimeMin";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagGrabAtStand";
///////////////////////////////////////////////////////////////////////////////
//                            	 LakRabbit
///////////////////////////////////////////////////////////////////////////////
//Game type values - out of LakRabbitGame.cs      %client.dtStats.stat["score"] = %client.score;
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","Avg"]++,"LakRabbitGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","Max"]++,"LakRabbitGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "morepoints";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "mas";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "MidairflagGrabs";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "MidairflagGrabPoints";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "flagTimeMS";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalChainAccuracy";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalChainHits";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalSnipeHits";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalSnipes";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalSpeed";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalDistance";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalShockHits";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "totalShocks";

$dtStats::FV[$dtStats::FC["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "flagTimeMin";
///////////////////////////////////////////////////////////////////////////////
//                            	 DMGame
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["DMGame","Avg"]++,"DMGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["DMGame","Max"]++,"DMGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["DMGame","Avg"]++,"DMGame","Avg"] = "efficiency";

$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "MidAir";
$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "Bonus";
$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "KillStreakBonus";
$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "killCounter";
///////////////////////////////////////////////////////////////////////////////
//                            	 ArenaGame
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["ArenaGame","Avg"]++,"ArenaGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["ArenaGame","Max"]++,"ArenaGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "snipeKills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "roundsWon";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "roundsLost";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "assists";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "roundKills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "hatTricks";
$dtStats::FV[$dtStats::FC["ArenaGame","Game"]++,"ArenaGame","Game"] = "dtTeam";
$dtStats::FV[$dtStats::FC["ArenaGame","Game"]++,"ArenaGame","Game"] = "teamScore";
$dtStats::FV[$dtStats::FC["ArenaGame","TG"]++,"ArenaGame","TG"] = "timeOnTeamZero";
$dtStats::FV[$dtStats::FC["ArenaGame","TG"]++,"ArenaGame","TG"] = "timeOnTeamOne";
$dtStats::FV[$dtStats::FC["ArenaGame","TG"]++,"ArenaGame","TG"] = "timeOnTeamTwo";
$dtStats::FV[$dtStats::FC["ArenaGame","TG"]++,"ArenaGame","TG"] = "matchRunTime";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "WLR";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "discMARatio";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "plasmaMARatio";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "laserMARatio";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "grenadeMARatio";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "shockMARatio";
$dtStats::FV[$dtStats::FC["ArenaGame","AVG"]++,"ArenaGame","AVG"] = "blasterMARatio";

///////////////////////////////////////////////////////////////////////////////
//                              Weapon/Misc Stats
///////////////////////////////////////////////////////////////////////////////
//these are field values from this script
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "explosionKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "impactKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "groundKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "aaTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "indoorDepTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outdoorDepTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sentryTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outOfBoundKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lavaKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shrikeBlasterKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bellyTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bomberBombsKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankChaingunKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankMortarKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "vehicleSpawnKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "forceFieldPowerUpKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "crashKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nexusCampingKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "inventoryKills";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "explosionDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "impactDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "groundDeaths";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "aaTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "indoorDepTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outdoorDepTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sentryTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outOfBoundDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lavaDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shrikeBlasterDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bellyTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bomberBombsDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankChaingunDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankMortarDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "vehicleSpawnDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "forceFieldPowerUpDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "crashDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nexusCampingDeaths";


//Damage Stats
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "roadDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "indoorDepTurretDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outdoorDepTurretDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankMortarDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankChaingunDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bomberBombsDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bellyTurretDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shrikeBlasterDmg";

//rounds fired
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelShotsFired";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserHSKills";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelHits";

//aoe hits
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDmgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDmgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDmgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDmgHits";

//misc
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserHeadShot";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockRearShot";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "minePlusDisc";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "minePlusDiscKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalTime";

$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitHeight";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitVV";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "airTime";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "airTime";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "groundTime";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "groundTime";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAkills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAEVHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAEVKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVHitWep";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVMAHit";

$dtStats::FV[$dtStats::FC["Game"]++,"Game"] = "tournamentMode";
$dtStats::FV[$dtStats::FC["Game"]++,"Game"] = "startPCT";
$dtStats::FV[$dtStats::FC["Game"]++,"Game"] = "endPCT";
$dtStats::FV[$dtStats::FC["Game"]++,"Game"] = "mapSkip";
$dtStats::FV[$dtStats::FC["Game"]++,"Game"] = "clientQuit";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalWepDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "timeTL";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "timeTL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killStreak";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "killStreak";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "assist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maxSpeed";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "avgSpeed";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "comboCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "distMov";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "firstKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lastKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathKills";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "kdr";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "ctrlKKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "concussHit";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "concussTaken";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "idleTime";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "idleTime";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "deadDist";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileTK";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "inventoryDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "repairEnemy";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "revenge";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shieldPackDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cloakerKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cloakersKilled";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "jammer";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discReflectHit";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discReflectKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterReflectHit";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterReflectKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "flareKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "flareHit";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discJump";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killerDiscJump";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillGround";

// nongame
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "leavemissionareaCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "teamkillCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "switchteamCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "flipflopCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "packpickupCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "weaponpickupCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "repairpackpickupCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "repairpackpickupEnemy";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "invyEatRepairPack";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "chatallCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "chatteamCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "voicebindsallCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "voicebindsteamCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "kickCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "obstimeoutkickCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "spawnobstimeoutCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "voteCount";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lagSpikes";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "packetLoss";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "txStop";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "lagSpikes";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "packetLoss";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "txStop";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "pingAvg";
$dtStats::FV[$dtStats::FC["AvgI"]++,"AvgI"] = "pingAvg";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lArmorTime";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mArmorTime";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hArmorTime";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorH";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLK";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMK";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHK";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLH";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorML";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMH";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHH";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "doubleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tripleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quadrupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quintupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sextupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "septupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "octupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nonupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "decupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nuclearKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "multiKill";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "doubleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tripleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quadrupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quintupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sextupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "septupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "octupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nonupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "decupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "chainKill";

//weapon combos
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelCom";



 //source hit velocity - note no mine
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileHitSV";

$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelHitVV";


//midairs

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelMA";


//ma dist
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileMAHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineMAHitDist";


$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelHitDist";

$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "weaponHitDist";


$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "cgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "discACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "grenadeACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "laserACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mortarACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "shockACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "plasmaACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "blasterACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "hGrenadeACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mineACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "satchelACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "missileACC";

$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "plasmaDmgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "discDmgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "grenadeDmgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mortarDmgACC";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "null";//rng number for testing
////////////////////////////////////////////////////////////////////////////////
//Unused vars that are not tracked but used for other things and need to be reset every round

$dtStats::unused[$dtStats::unusedCount++] = "timeToLive";
$dtStats::unused[$dtStats::unusedCount++] = "ksCounter";
////////////////////////////////////////////////////////////////////////////////


$dtStats::TBGC["CTF"] = -1;
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "score";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "defenseScore";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "offenseScore";

$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "kills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "deaths";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "suicides";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "teamKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "friendlyFire";

$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagCaps";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagGrabs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagGrabAtStand";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "carrierKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagReturns";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "escortAssists";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagDefends";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "concussFlag";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depInvyUse";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "concussFlag";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagCatch";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagToss";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "interceptedFlag";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "stalemateReturn";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flagTimeMin";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeNearTeamFS";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeFarTeamFS";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeNearEnemyFS";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeFarEnemyFS";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeNearFlag";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeNearEnemyFlag";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "capEfficiency";


$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "genRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "genSolRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "SensorRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "TurretRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "StationRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "VStationRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "solarRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "sentryRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depSensorRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depInvRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depTurretRepairs";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "repairs";

$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "tkDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "genDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "sensorDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "turretDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "iStationDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "vstationDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "solarDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "sentryDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depSensorDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depTurretDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "depStationDestroys";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "destruction";

$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "genDefends";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "turretKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "mannedTurretKills";




//$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "MotionSensorDep";
//$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "PulseSensorDep";
//$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "SensorsDep";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "InventoryDep";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "TurretOutdoorDep";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "TurretIndoorDep";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "TurretsDep";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "TotalDep";


$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "OffKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "DefKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "OffKillsL";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "OffKillsM";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "OffKillsH";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "DefKillsL";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "DefKillsM";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "DefKillsH";


$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "roadKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "gravCycleDes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "assaultTankDes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "MPBDes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "turbogravDes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "bomberDes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "heavyTransportDes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "wildRK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "assaultRK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "scoutFlyerRK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "bomberFlyerRK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "hapcFlyerRK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "tankMortarDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "tankChaingunDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "bomberBombsDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "bellyTurretDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "shrikeBlasterDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "shrikeBlasterKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "bellyTurretKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "bomberBombsKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "tankChaingunKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "tankMortarKills";


$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "kdr";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "assist";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "cgKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "discKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "grenadeKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "hGrenadeKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "laserKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "mortarKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "missileKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "shockKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "plasmaKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "blasterKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "mineKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "explosionKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "satchelKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "inventoryKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "cgDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "laserDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "blasterDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "discDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "grenadeDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "hGrenadeDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "mortarDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "missileDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "plasmaDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "shockDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "mineDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "satchelDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "indoorDepTurretDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "outdoorDepTurretDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "totalWepDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "elfShotsFired";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "laserHeadShot";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "shockRearShot";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "minePlusDisc";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "minePlusDiscKill";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "totalMA";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "airTime";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "groundTime";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "timeTL";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "killStreak";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "avgSpeed";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "concussHit";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "concussTaken";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "idleTime";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "repairEnemy";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "revenge";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "shieldPackDmg";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "cloakerKills";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "cloakersKilled";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "jammer";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "flipflopCount";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "repairpackpickupCount";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "repairpackpickupEnemy";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "invyEatRepairPack";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "lagSpikes";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "packetLoss";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "txStop";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "pingAvg";

$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorL";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorM";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorH";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorLK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorMK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorHK";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorLL";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorLM";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorLH";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorML";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorMM";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorMH";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorHL";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorHM";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "armorHH";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "discMA";
$dtStats::TBG[$dtStats::TBGC["CTF"]++,"CTF"] = "laserKillDist";
$dtStats::TBGC["CTF"]++;

$dtStats::TBGC["LCTF"] = -1;
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "score";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "defenseScore";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "offenseScore";

$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "kills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "deaths";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "suicides";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "teamKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "friendlyFire";

$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagCaps";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagGrabs";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagGrabAtStand";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "carrierKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagReturns";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "escortAssists";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagDefends";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "concussFlag";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "depInvyUse";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "concussFlag";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagCatch";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagToss";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "interceptedFlag";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "stalemateReturn";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flagTimeMin";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeNearTeamFS";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeFarTeamFS";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeNearEnemyFS";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeFarEnemyFS";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeNearFlag";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeNearEnemyFlag";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "capEfficiency";

$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "OffKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "DefKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "OffKillsL";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "OffKillsM";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "OffKillsH";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "DefKillsL";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "DefKillsM";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "DefKillsH";

$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "kdr";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "assist";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "cgKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "discKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "grenadeKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "hGrenadeKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "laserKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "mortarKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "missileKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "shockKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "plasmaKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "blasterKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "mineKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "explosionKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "satchelKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "inventoryKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "cgDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "laserDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "blasterDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "discDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "grenadeDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "hGrenadeDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "mortarDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "missileDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "plasmaDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "shockDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "mineDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "satchelDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "indoorDepTurretDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "outdoorDepTurretDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "totalWepDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "elfShotsFired";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "laserHeadShot";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "shockRearShot";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "minePlusDisc";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "minePlusDiscKill";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "totalMA";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "airTime";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "groundTime";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "timeTL";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "killStreak";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "avgSpeed";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "concussHit";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "concussTaken";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "idleTime";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "repairEnemy";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "revenge";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "shieldPackDmg";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "cloakerKills";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "cloakersKilled";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "jammer";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "flipflopCount";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "repairpackpickupCount";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "repairpackpickupEnemy";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "invyEatRepairPack";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "lagSpikes";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "packetLoss";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "txStop";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "pingAvg";

$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "armorL";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "discMA";
$dtStats::TBG[$dtStats::TBGC["LCTF"]++,"LCTF"] = "laserKillDist";
$dtStats::TBGC["LCTF"]++;


$dtStats::TBGC["Arena"] = -1;
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "score";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "kills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "deaths";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "suicides";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "teamKills";

$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "kdr";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "assist";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "cgKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "discKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "grenadeKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "hGrenadeKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "laserKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "mortarKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "missileKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "shockKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "plasmaKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "blasterKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "mineKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "explosionKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "satchelKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "inventoryKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "cgDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "laserDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "blasterDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "discDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "grenadeDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "hGrenadeDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "mortarDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "missileDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "plasmaDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "shockDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "mineDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "satchelDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "indoorDepTurretDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "outdoorDepTurretDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "totalWepDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "elfShotsFired";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "laserHeadShot";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "shockRearShot";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "minePlusDisc";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "minePlusDiscKill";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "totalMA";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "airTime";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "groundTime";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "timeTL";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "killStreak";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "avgSpeed";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "concussHit";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "concussTaken";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "idleTime";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "repairEnemy";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "revenge";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "shieldPackDmg";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "cloakerKills";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "cloakersKilled";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "jammer";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "flipflopCount";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "repairpackpickupCount";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "repairpackpickupEnemy";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "invyEatRepairPack";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "lagSpikes";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "packetLoss";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "txStop";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "pingAvg";

$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorL";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorM";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorLK";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorMK";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorLL";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorLM";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorML";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorMM";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorHL";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "armorHM";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "discMA";
$dtStats::TBG[$dtStats::TBGC["Arena"]++,"Arena"] = "laserKillDist";

$dtStats::TBGC["Arena"]++;


if(!isObject(statsGroup)){
   new SimGroup(statsGroup);
   RootGroup.add(statsGroup);
   statsGroup.resetCount = -1;
   statsGroup.serverStart = 0;
   $dtStats::leftID++;
}

function dtAICON(%client){
   dtStatsMissionDropReady(Game.getId(), %client);
}

package dtStats{
   function AIConnection::startMission(%client){// ai support
      parent::startMission(%client);
      schedule(15000,0,"dtAICON",%client);
   }

   function GameConnection::onDrop(%client, %reason){
      dtStatsClientLeaveGame(%client);//common
      parent::onDrop(%client, %reason);
   }

   function CTFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);

      if($dtStats::ctfTimes){
         %team1 = $dtServer::capTimes[cleanMapName($missionName),%game.class,1];
         %team2 = $dtServer::capTimes[cleanMapName($missionName),%game.class,1];
         %time1 = %game.formatTime(getField(%team1,0), true);
         %time2 = %game.formatTime(getField(%team2,0), true);
         %name1 = getField(%team1,1);
         %name2 = getField(%team1,2);
         BottomPrint(%client, "Best caps on " @ $CurrentMission @ ":\n" @ getTaggedString(%game.getTeamName(1)) @ ":" SPC %name1 @ " in " @ %time1 @ " seconds\n" @ getTaggedString(%game.getTeamName(2)) @ ":" SPC %name2 @ " in " @ %time2  @ " seconds", 20, 3);
      }
      dtStatsMissionDropReady(%game, %client);//common
   }

   function CTFGame::gameOver( %game ){
      dtStatsGameOver(%game);
      parent::gameOver(%game);
   }

   function CTFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }

   ////////////////////////////////////////////////////////////////////////////////
   function LakRabbitGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }

   function LakRabbitGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }

   function LakRabbitGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }

   function LakRabbitGame::recalcScore(%game, %client){
      if($missionRunning){
         parent::recalcScore(%game, %client);
      }
   }

   ////////////////////////////////////////////////////////////////////////////////
   function ArenaGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);
   }

   function ArenaGame::gameOver( %game ){
      dtStatsGameOver(%game);
      parent::gameOver(%game);
   }

   function ArenaGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }

   ////////////////////////////////////////////////////////////////////////////////
   function DMGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }

   function DMGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);
   }

   function DMGame::gameOver( %game ){
      dtStatsGameOver(%game);
      parent::gameOver(%game);
   }

   function DMGame::recalcScore(%game, %client){
	  if(!$missionRunning){
         return;
      }
      parent::recalcScore(%game, %client);
   }

   ////////////////////////////////////////////////////////////////////////////////
   function LCTFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::ctfTimes){
         %team1 = $dtServer::capTimes[cleanMapName($missionName),%game.class,1];
         %team2 = $dtServer::capTimes[cleanMapName($missionName),%game.class,1];
         %time1 = %game.formatTime(getField(%team1,0), true);
         %time2 = %game.formatTime(getField(%team2,0), true);
         %name1 = getField(%team1,1);
         %name2 = getField(%team1,2);
         BottomPrint(%client, "Best caps on " @ $CurrentMission @ ":\n" @ getTaggedString(%game.getTeamName(1)) @ ":" SPC %name1 @ " in " @ %time1 @ " seconds\n" @ getTaggedString(%game.getTeamName(2)) @ ":" SPC %name2 @ " in " @ %time2  @ " seconds", 20, 3);
      }
      dtStatsMissionDropReady(%game, %client);//common
   }

   function LCTFGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }

   function LCTFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if(%clKiller.team != %clVictim.team){
         %pos = isObject(%clKiller.player) ? %clKiller.player.getPosition() : %clKiller.lp;
         %dist = vectorDist($dtStats::FlagPos[%clKiller.team], %pos);
         if(%dist > ($dtStats::FlagTotalDist*0.5)){// kill made closer to the enemy flag
            %clKiller.dtStats.stat["OffKills"]++;
         }
         else{
            %clKiller.dtStats.stat["DefKills"]++;
         }
      }
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }

   function SCtFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::ctfTimes){
         %team1 = $dtServer::capTimes[cleanMapName($missionName),%game.class,1];
         %team2 = $dtServer::capTimes[cleanMapName($missionName),%game.class,1];
         %time1 = %game.formatTime(getField(%team1,0), true);
         %time2 = %game.formatTime(getField(%team2,0), true);
         %name1 = getField(%team1,1);
         %name2 = getField(%team1,2);
         BottomPrint(%client, "Best caps on " @ $CurrentMission @ ":\n" @ getTaggedString(%game.getTeamName(1)) @ ":" SPC %name1 @ " in " @ %time1 @ " seconds\n" @ getTaggedString(%game.getTeamName(2)) @ ":" SPC %name2 @ " in " @ %time2  @ " seconds", 20, 3);
      }
      dtStatsMissionDropReady(%game, %client);//common
   }

   function SCtFGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }

   function SCtFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      if(%clKiller.team != %clVictim.team && isObject(%clKiller.player)){
         %dist = vectorDist($dtStats::FlagPos[%clKiller.team], %clKiller.player.getPosition());
         if(%dist > ($dtStats::FlagTotalDist*0.5)){// kill made closer to the enemy flag
            %clKiller.dtStats.stat["OffKills"]++;
         }
         else{
            %clKiller.dtStats.stat["DefKills"]++;
         }
      }
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }

   ///////////////////////////////////////////////////////////////////////////////
   function DefaultGame::missionLoadDone(%game){
      parent::missionLoadDone(%game);
      if(isObject(dtGameStat)){
         dtGameStat.delete();
      }
      $dtStats::MapStart = 1;//rebuild custom map list after first load
      buildMissionList();// this way to prevent locking a person out of selecting a start map

      dtSaveServerVars();
      dtScanForRepair();
      if(%game.class $= "CTFGame" || %game.class $= "LCTFGame" || %game.class $= "SCtFGame"){
         $dtStats::FlagPos[1] =  $TeamFlag[1].getPosition();
         $dtStats::FlagPos[2] =  $TeamFlag[2].getPosition();
         $dtStats::FlagTotalDist = vectorDist($dtStats::FlagPos[1], $dtStats::FlagPos[2]);
      }
      $dtStats::gameID = formattimestring("yymmddHHnnss");
      if($dtStats::debugEchos)
         error("GAME ID" SPC $dtStats::gameID SPC "//////////////////////////////");
      if($TB::TBEnable[$dtStats::gtNameShort[%game.class]] && !$Host::TournamentMode){// note this happens before clients start there load
         ballenceTeams(%game,0);
      }
   }

   function DefaultGame::forceObserver( %game, %client, %reason ){
      parent::forceObserver( %game, %client, %reason );
      if(%reason $= "spawnTimeout"){
         %client.dtStats.stat["spawnobstimeoutCount"]++;
      }
      updateTeamTime(%client.dtStats,%client.dtStats.team);
      %client.dtStats.team = 0;
      %client.gt = %client.at = 0;//air time ground time reset
   }

   function chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 ){
      if ( getsubstr(detag(%a2),0,1) $= "#" ){
         error("dtchatcommandtest");
         return;
      }
      parent::chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
      %sender.dtStats.stat["chatallCount"]++;
    }
    function cannedChatMessageAll( %sender, %msgString, %name, %string, %keys ){
      parent::cannedChatMessageAll( %sender, %msgString, %name, %string, %keys );
      %sender.dtStats.stat["voicebindsallCount"]++;
   }

	function chatMessageTeam( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 ){
      parent::chatMessageTeam( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
      %sender.dtStats.stat["chatteamCount"]++;
    }
    function cannedChatMessageTeam( %sender, %team, %msgString, %name, %string, %keys ){
      parent::cannedChatMessageTeam( %sender, %team, %msgString, %name, %string, %keys );
      %sender.dtStats.stat["voicebindsteamCount"]++;
   }

	function kick( %client, %admin, %guid ){
      %client.dtStats.stat["kickCount"]++;
      parent::kick( %client, %admin, %guid );
   }

   function cmdAutoKickObserver(%client, %key){ // Edit GG
      parent::cmdAutoKickObserver(%client, %key);
      %client.dtStats.stat["obstimeoutkickCount"]++;
   }

   function CTFGame::leaveMissionArea(%game, %playerData, %player){
	   parent::leaveMissionArea(%game, %playerData, %player);
      %player.client.dtStats.stat["leavemissionareaCount"]++;
   }

   function LCTFGame::leaveMissionArea(%game, %playerData, %player){
	   parent::leaveMissionArea(%game, %playerData, %player);
      %player.client.dtStats.stat["leavemissionareaCount"]++;
   }
   
   function SCtFGame::leaveMissionArea(%game, %playerData, %player){
	   parent::leaveMissionArea(%game, %playerData, %player);
      %player.client.dtStats.stat["leavemissionareaCount"]++;
   }

   function DefaultGame::clientJoinTeam( %game, %client, %team, %respawn ){
      parent::clientJoinTeam( %game, %client, %team, %respawn );
      updateTeamTime(%client.dtStats, 0);
      %client.dtStats.team = %team;
   }

   function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs, %respawned){ // z0dd - ZOD, 6/06/02. Don't send a message if player used respawn feature. Added %respawned
	   if(isGameRun()){
         %client.dtStats.team = %team;
         %client.dtStats.stat["switchteamCount"]++;
         if(%fromObs)
            updateTeamTime(%client.dtStats, 0);
         else
            updateTeamTime(%client.dtStats,%client.team);
	   }
      parent::clientChangeTeam(%game, %client, %team, %fromObs, %respawned);
   }
   function DefaultGame::assignClientTeam(%game, %client, %respawn ){
      parent::assignClientTeam(%game, %client, %respawn );
      updateTeamTime(%client.dtStats, 0);
      %client.dtStats.team = %client.team;
   }

   function RepairPack::onThrow(%data,%obj,%shape){
      parent::onThrow(%data,%obj,%shape);
      %obj.team = %shape.client.team;
      %player.dtRepairPickup = 0;
   }

   function ItemData::onPickup(%this, %pack, %player, %amount){
      parent::onPickup(%this, %pack, %player, %amount);
      %dtStats = %player.client.dtStats;
      if(%this.getname() $= "RepairPack"){
         if(%pack.team > 0 && %pack.team != %player.client.team)
            %dtStats.stat["repairpackpickupEnemy"]++;
         %dtStats.stat["repairpackpickupCount"]++;
         %player.dtRepairPickup = 1;
      }
      %dtStats.stat["packpickupCount"]++;
   }

   function stationTrigger::onLeaveTrigger(%data, %obj, %colObj){
      if(isObject(%obj.station)){
         %name = %obj.station.getDataBlock().getName();
         if(%name $= "DeployedStationInventory" || %name $= "StationInventory"){
            if(%colObj.getMountedImage(2) > 0){
               if(%colObj.getMountedImage(2).getName() !$= "RepairPackImage" && %colObj.dtRepairPickup){
                  %colObj.client.dtStats.stat["invyEatRepairPack"]++;
               }
            }
            %player.dtRepairPickup = 0;
         }
         if(%name $= "DeployedStationInventory"){
            %ow = %obj.station.owner;
            if(isObject(%ow) && %ow != %colObj.client)
               %ow.dtStats.stat["depInvyUse"]++;
         }
      }
      parent::onLeaveTrigger(%data, %obj, %colObj);
   }

   function DefaultGame::playerSpawned(%game, %player){
      parent::playerSpawned(%game, %player);
      armorTimer(%player.client.dtStats, %player.getArmorSize(), 0);
   }

   function Player::setArmor(%this,%size){//for game types that use spawn favs
      parent::setArmor(%this,%size);
      armorTimer(%this.client.dtStats, %size, 0);
   }

   function Weapon::onPickup(%this, %obj, %shape, %amount){
		parent::onPickup(%this, %obj, %shape, %amount);
      %shape.client.dtStats.stat["weaponpickupCount"]++;
   }

   //////////////////////////////////////////////////////////////////////////////////
   function DefaultGame::activatePackages(%game){
      parent::activatePackages(%game);
      if(isActivePackage(dtStatsGame)){
         deactivatePackage(dtStatsGame);
         activatePackage(dtStatsGame);
      }
      else{
         activatePackage(dtStatsGame);
      }
   }

   function DefaultGame::deactivatePackages(%game){
      parent::deactivatePackages(%game);
      if(isActivePackage(dtStatsGame))
         deactivatePackage(dtStatsGame);
   }

   function ArenaGame::activatePackages(%game){
      parent::activatePackages(%game);
      if(isActivePackage(dtStatsGame)){
         deactivatePackage(dtStatsGame);
         activatePackage(dtStatsGame);
      }
      else{
         activatePackage(dtStatsGame);
      }
   }

   function ArenaGame::deactivatePackages(%game){
      parent::deactivatePackages(%game);
      if(isActivePackage(dtStatsGame))
         deactivatePackage(dtStatsGame);
   }

   //////////////////////////////////////////////////////////////////////////////////
   // Flag Escort Fixes
   function CTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){
       parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
      %clAttacker.stat["scoreHeadshot"] = %clAttacker.scoreHeadshot;
      %clAttacker.stat["scoreRearshot"] = %clAttacker.scoreRearshot;
       if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
         %clAttacker.dmgdFlagTime = getSimTime();
	}

	function CTFGame::testEscortAssist(%game, %victimID, %killerID){
	   if((getSimTime() - %victimID.dmgdFlagTime) < 5000 && %killerID.player.holdingFlag $= ""){
		  return true;
      }
	   return false;
	}

	function LCTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){
	   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
	   %clAttacker.stat["scoreHeadshot"] = %clAttacker.scoreHeadshot;
      %clAttacker.stat["scoreRearshot"] = %clAttacker.scoreRearshot;
	   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
		  %clAttacker.dmgdFlagTime = getSimTime();
	}

	function LCTFGame::testEscortAssist(%game, %victimID, %killerID){
	   if((getSimTime() - %victimID.dmgdFlagTime) < 5000 && %killerID.player.holdingFlag $= ""){
		  return true;
	   }
	   return false;
	}

   function SCtFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){
	   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
	   %clAttacker.stat["scoreHeadshot"] = %clAttacker.scoreHeadshot;
      %clAttacker.stat["scoreRearshot"] = %clAttacker.scoreRearshot;
	   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
		  %clAttacker.dmgdFlagTime = getSimTime();
	}

	function SCtFGame::testEscortAssist(%game, %victimID, %killerID){
	   if((getSimTime() - %victimID.dmgdFlagTime) < 5000 && %killerID.player.holdingFlag $= ""){
		  return true;
	   }
	   return false;
	}

	function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal){
	   %cl = %projectile.sourceObject.client;
      if(isObject(%cl)){
         %cl.lastExp = %data TAB %projectile.initialPosition TAB %position TAB %projectile.getWorldBox();
         %cl.lastExpTime = getSimTime();
      }
      parent::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal);
	}

   function ProjectileData::onExplode(%data, %proj, %pos, %mod){
      %dataName = %data.getName();
      %sourceClient = %proj.sourceObject.client;
      switch$(%dataName){
         case "DiscProjectile":
            %vec =  vectorNormalize(vectorSub(%pos,%proj.initialPosition));
            %initVec = %proj.initialDirection;
            %ray = containerRayCast(%proj.initialPosition, VectorAdd(%proj.initialPosition, VectorScale(VectorNormalize(%initVec), 5000)), $TypeMasks::WaterObjectType);
            if(%ray){
               %angleRad = mACos(vectorDot(%initVec, "0 0 1"));
               %angleDeg = mRadToDeg(%angleRad)-90;
               //echo(%angleDeg);
               if(%angleDeg <= 15 && %angleDeg > 0){
                  %wdist = vectorDist(getWords(%ray,1,3),%pos);
                  if(%wdist > 20)
                     %sourceClient.discReflect = getSimTime();
                  //error("disc bounce" SPC %angleDeg SPC %wdist);
               }
            }
            else{
               %sourceClient.discReflect = 0;
            }
            if(vectorDist(%pos,%proj.sourceObject.getPosition()) < 4){
                %sourceClient.lastDiscJump = getSimTime();
            }
         case "EnergyBolt":
            %vec =  vectorNormalize(vectorSub(%pos,%proj.initialPosition));
            %initVec = %proj.initialDirection;
            %angleRad = mACos(vectorDot(%vec, %initVec));
            %angleDeg = mRadToDeg(%angleRad);
            if(%angleDeg > 10){
               %sourceClient.blasterReflect = getSimTime();
            }
            else
               %sourceClient.blasterReflect = 0;
         case "ShoulderMissile":
            if(%proj.lastTargetType $= "FlareProjectile"){
               %sourceClient.stat["flareHit"] = getSimTime();
               %sourceClient.flareSource = %proj.targetSource.client;
            }
            else{
               %sourceClient.stat["flareHit"] = 0;
            }
      }

      if(isObject(%sourceClient)){
         if(%proj.dtShotSpeed > 0){
            %sourceClient.dtShotSpeed = %proj.dtShotSpeed;
         }
         else{
            %sourceClient.dtShotSpeed = mFloor(vectorLen(%proj.sourceObject.getVelocity()) * 3.6);
         }
         %sourceClient.lastExp = %data TAB %proj.initialPosition TAB %pos TAB %proj.getWorldBox();
         %sourceClient.lastExpTime = getSimTime();
      }
      parent::onExplode(%data, %proj, %pos, %mod);
   }

   //function MineDeployed::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType){
      //parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
      //if(%damageType == $DamageType::Disc && $dtStats::Enable){
         //%targetObject.mineDiscHit = 1;
         //error("boom" SPC %sourceObject.getClassName());
      //}
   //}

   function ShapeBaseImageData::onDeploy(%item, %plyr, %slot){
      %obj = parent::onDeploy(%item, %plyr, %slot);
      %dtStats = %plyr.client.dtStats;
      %itemDB = %item.item;
      switch$(%itemDB){
         case "MotionSensorDeployable":
            %dtStats.stat["MotionSensorDep"]++;
            %dtStats.stat["SensorsDep"]++;
            %dtStats.stat["TotalDep"]++;
         case "PulseSensorDeployable":
            %dtStats.stat["PulseSensorDep"]++;
            %dtStats.stat["SensorsDep"]++;
            %dtStats.stat["TotalDep"]++;
         case "InventoryDeployable":
            %dtStats.stat["InventoryDep"]++;
            %dtStats.stat["TotalDep"]++;
         case "TurretOutdoorDeployable":
            %dtStats.stat["TurretOutdoorDep"]++;
            %dtStats.stat["TurretsDep"]++;
            %dtStats.stat["TotalDep"]++;
         case "TurretIndoorDeployable":
            %dtStats.stat["TurretIndoorDep"]++;
            %dtStats.stat["TurretsDep"]++;
            %dtStats.stat["TotalDep"]++;
      }
      return %obj;
   }
   function Armor::applyConcussion( %this, %dist, %radius, %sourceObject, %targetObject ){
      if(%sourceObject.client.team != %targetObject.client.team){
         %sourceObject.client.dtStats.stat["concussHit"]++;
         %targetObject.client.dtStats.stat["concussTaken"]++;
         %targetObject.concussBy = %sourceObject.client.dtStats;
      }
       parent::applyConcussion( %this, %dist, %radius, %sourceObject, %targetObject );
   }

   function LCTFGame::applyConcussion(%game, %player){
      %dtStats = %player.concussBy;
      if(isObject(%dtStats) &&  %player.holdingFlag > 0)
         %dtStats.stat["concussFlag"]++;
      %player.concussBy = -1;
      parent::applyConcussion(%game, %player);
   }

   function SCtFGame::applyConcussion(%game, %player){
      %dtStats = %player.concussBy;
      if(isObject(%dtStats) &&  %player.holdingFlag > 0)
         %dtStats.stat["concussFlag"]++;
      %player.concussBy = -1;
      parent::applyConcussion(%game, %player);
   }

   function CTFGame::applyConcussion(%game, %player){
      %dtStats = %player.concussBy;
      if(isObject(%dtStats) &&  %player.holdingFlag > 0)
         %dtStats.stat["concussFlag"]++;
      %player.concussBy = -1;
      parent::applyConcussion(%game, %player);
   }

   function MobileBaseVehicle::playerMounted(%data, %obj, %player, %node){
      %obj.dtStats = %player.client.dtStats;
      parent::playerMounted(%data, %obj, %player, %node);
   }

   function MobileBaseVehicle::onDamage(%this, %obj){
      if(VectorLen(%obj.getVelocity()) > 200){
         if(isObject(%obj.dtStats))
            %obj.dtStats.stat["mpbGlitch"]++;
      }
      parent::onDamage(%this, %obj);
   }


   function CTFGame::recalcScore(%game, %cl){
      parent::recalcScore(%game, %cl);
      %dtStats = %cl.dtStats;
      %dtStats.stat["offenseScore"] = %cl.offenseScore;
      %dtStats.stat["defenseScore"] = %cl.defenseScore;
      %dtStats.stat["score"] = %cl.score;
      %dtStats.stat["dtTeam"] = %cl.team;
      %dtStats.stat["scoreMidAir"] = %cl.scoreMidAir;
   }
   function CTFGame::awardScoreKill(%game, %killerID){
      %val = parent::awardScoreKill(%game, %killerID);
      %killerID.dtStats.stat["kills"] = %killerID.kills;
      return %val;
   }
   function CTFGame::awardScoreTurretKill(%game, %victimID, %implement){
      parent::awardScoreTurretKill(%game, %victimID, %implement);
      if ((%killer = %implement.getControllingClient()) != 0){
         %killer.dtStats.stat["teamKills"] = %killer.teamKills;
         %killer.dtStats.stat["mannedTurretKills"] = %killer.mannedturretKills;
         %killer.dtStats.stat["turretKills"] =%killer.turretKills;
      }
   }
   function CTFGame::awardScoreTkDestroy(%game, %cl, %obj){
      parent::awardScoreTkDestroy(%game, %cl, %obj);
      %cl.dtStats.stat["tkDestroys"] = %cl.tkDestroys;
   }

   function CTFGame::awardScoreFlagCap(%game, %cl, %flag){
      parent::awardScoreFlagCap(%game, %cl, %flag);
      %cl.dtStats.stat["flagCaps"]++;
      dtMinMax("flagCaps", "flag", 1, %cl.dtStats.stat["flagCaps"], %cl);
   }
   function CTFGame::awardScoreFlagTouch(%game, %cl, %flag){
      parent::awardScoreFlagTouch(%game, %cl, %flag);
      %cl.dtStats.stat["flagGrabs"] = %cl.flagGrabs;
      dtMinMax("flagGrabs", "flag", 1, %cl.dtStats.stat["flagGrabs"], %cl); 
   }
   
   function CTFGame::awardScoreStaticShapeDestroy(%game, %cl, %obj){
      error("awardScoreStaticShapeDestroy");
      parent::awardScoreStaticShapeDestroy(%game, %cl, %obj);
      %dataName = %obj.getDataBlock().getName();
      switch$ ( %dataName ){
         case "GeneratorLarge":
            %cl.dtStats.stat["genDestroys"] = %cl.genDestroys;
         case "SolarPanel":
            %cl.dtStats.stat["solarDestroys"] = %cl.solarDestroys;
         case "SensorLargePulse" or "SensorMediumPulse":
            %cl.dtStats.stat["sensorDestroys"] = %cl.sensorDestroys;
         case "TurretBaseLarge":
            %cl.dtStats.stat["turretDestroys"] = %cl.turretDestroys;
         case "StationInventory":
            %cl.dtStats.stat["iStationDestroys"] = %cl.iStationDestroys;
         case "StationAmmo":
         case "StationVehicle":
            %cl.dtStats.stat["vstationDestroys"] = %cl.VStationDestroys;
         case "SentryTurret":
            %cl.dtStats.stat["sentryDestroys"] = %cl.sentryDestroys;
         case "DeployedMotionSensor" or "DeployedPulseSensor":
            %cl.dtStats.stat["depSensorDestroys"] = %cl.depSensorDestroys;
         case "TurretDeployedWallIndoor" or "TurretDeployedFloorIndoor" or "TurretDeployedCeilingIndoor" or "TurretDeployedOutdoor":
            %cl.dtStats.stat["depTurretDestroys"] = %cl.depTurretDestroys;
         case "DeployedStationInventory":
            %cl.dtStats.stat["depStationDestroys"] = %cl.depStationDestroys;
         case "MPBTeleporter":
            %cl.dtStats.stat["mpbtstationDestroys"] = %cl.mpbtstationDestroys;

      }
   }

   function CTFGame::awardScoreVehicleDestroyed(%game, %client, %vehicleType, %mult, %passengers){
       %val = parent::awardScoreVehicleDestroyed(%game, %client, %vehicleType, %mult, %passengers);
       switch$(%vehicleType){// add stas here
         case "Grav Cycle":
            %client.dtStats.stat["gravCycleDes"]++;
         case "Assault Tank":
            %client.dtStats.stat["assaultTankDes"]++;
         case "MPB":
            %client.dtStats.stat["MPBDes"]++;
         case "Turbograv":
            %client.dtStats.stat["turbogravDes"]++;
         case "Bomber":
            %client.dtStats.stat["bomberDes"]++;
         case "Heavy Transport":
            %client.dtStats.stat["heavyTransportDes"]++;
       }
       %client.dtStats.stat["vehicleScore"] = %client.vehicleScore;
       %client.dtStats.stat["vehicleBonus"] = %client.vehicleBonus;
      return %val;
   }

   function CTFGame::awardScoreFlagDefend(%game, %killerID){
      %val = parent::awardScoreFlagDefend(%game, %killerID);
      %killerID.dtStats.stat["flagDefends"] = %killerID.flagDefends;
      dtMinMax("flagDefends", "flag", 1, %killerID.dtStats.stat["flagDefends"], %killerID);
      return %val;
   }

   function CTFGame::awardScoreGenDefend(%game, %killerID){
      %val = parent::awardScoreGenDefend(%game, %killerID);
      %killerID.dtStats.stat["genDefends"] = %killerID.genDefends;
      return %val;
   }

   function CTFGame::awardScoreCarrierKill(%game, %killerID){
      %val = parent::awardScoreCarrierKill(%game, %killerID);
      %killerID.dtStats.stat["carrierKills"] = %killerID.carrierKills;
      dtMinMax("carrierKills", "flag", 1, %killerID.dtStats.stat["carrierKills"], %killerID);
      return %val;
   }

   function CTFGame::awardScoreEscortAssist(%game, %killerID){
      %val = parent::awardScoreEscortAssist(%game, %killerID);
      %killerID.dtStats.stat["escortAssists"] = %killerID.escortAssists;
      dtMinMax("escortAssists", "flag", 1, %killerID.dtStats.stat["escortAssists"], %killerID);
      return %val;
   }

   function CTFGame::awardScoreFlagReturn(%game, %cl, %perc){
      %val = parent::awardScoreFlagReturn(%game, %cl, %perc);
      %cl.dtStats.stat["flagReturns"]++;
      dtMinMax("flagReturns", "flag", 1, %cl.dtStats.stat["flagReturns"], %cl);
      %cl.dtStats.stat["returnPts"] = %cl.returnPts;
      return %val;
   }

   function CTFGame::staticShapeOnRepaired(%game, %obj, %objName){
      parent::staticShapeOnRepaired(%game, %obj, %objName);
      %client = %obj.repairedBy;
      %dataName = %obj.getDataBlock().getName();
      switch$ (%dataName){
         case "GeneratorLarge":
            %client.dtStats.stat["genRepairs"] = %client.genRepairs;
            %client.dtStats.stat["genSolRepairs"]++;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "SolarPanel":
            %client.dtStats.stat["solarRepairs"] = %client.solarRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
            %client.dtStats.stat["genSolRepairs"]++;
         case "SensorLargePulse" or "SensorMediumPulse":
            %client.dtStats.stat["SensorRepairs"] = %client.SensorRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "StationInventory" or "StationAmmo":
            %client.dtStats.stat["StationRepairs"] = %client.StationRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "StationVehicle":
            %client.dtStats.stat["VStationRepairs"] = %client.VStationRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "TurretBaseLarge":
            %client.dtStats.stat["TurretRepairs"] = %client.TurretRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "SentryTurret":
            %client.dtStats.stat["sentryRepairs"] = %client.sentryRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "DeployedMotionSensor" or "DeployedPulseSensor":
            %client.dtStats.stat["depSensorRepairs"] = %client.depSensorRepairs;
         case "TurretDeployedWallIndoor" or "TurretDeployedFloorIndoor" or "TurretDeployedCeilingIndoor" or "TurretDeployedOutdoor":
             %client.dtStats.stat["depTurretRepairs"] = %client.depTurretRepairs;
             dtMinMax("repairs", "misc", 3, 1, %client);
         case "DeployedStationInventory":
            %client.dtStats.stat["depInvRepairs"] = %client.depInvRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
         case "MPBTeleporter":
            %client.dtStats.stat["mpbtstationRepairs"] = %client.mpbtstationRepairs;
            dtMinMax("repairs", "misc", 3, 1, %client);
      }
   }

   function CTFGame::awardScoreStalemateReturn(%game, %cl){
      %val = parent::awardScoreStalemateReturn(%game, %cl);
      %cl.dtStats.stat["stalemateReturn"]++;
      dtMinMax("stalemateReturn", "flag", 1, %cl.dtStats.stat["stalemateReturn"], %cl);
      return %val;
   }


   function DMGame::recalcScore(%game, %client){
      parent::recalcScore(%game, %client);
      %client.dtStats.stat["kills"] = %client.kills;
      %client.dtStats.stat["deaths"]  = %client.deaths;
      %client.dtStats.stat["suicides"]  = %client.suicides;
      %client.dtStats.stat["MidAir"] = %client.MidAir;
      %client.dtStats.stat["Bonus"] = %client.Bonus;
      %client.dtStats.stat["KillStreakBonus"] = %client.KillStreakBonus;
      %client.dtStats.stat["killCounter"] = %client.killCounter;
      %client.dtStats.stat["score"] = %client.score;
      %client.dtStats.stat["efficiency"] = %client.efficiency;
   }

   function LakRabbitGame::recalcScore(%game, %client){
      parent::recalcScore(%game, %client);
      %client.dtStats.stat["score"] = %client.score;
      %client.dtStats.stat["kills"] = %client.kills;
      %client.dtStats.stat["deaths"]  = %client.deaths;
      %client.dtStats.stat["suicides"]  = %client.suicides;
      %client.dtStats.stat["flagGrabs"] = %client.flagGrabs;
      %client.dtStats.stat["flagTimeMS"] = %client.flagTimeMS;
      %client.dtStats.stat["morepoints"] = %client.morepoints;
      %client.dtStats.stat["mas"] = %client.mas;
      %client.dtStats.stat["totalSpeed"] = %client.totalSpeed;
      %client.dtStats.stat["totalDistance"] = %client.totalDistance;
      %client.dtStats.stat["totalChainAccuracy"] = %client.totalChainAccuracy;
      %client.dtStats.stat["totalChainHits"] = %client.totalChainHits;
      %client.dtStats.stat["totalSnipeHits"] = %client.totalSnipeHits;
      %client.dtStats.stat["totalSnipes"] = %client.totalSnipes;
      %client.dtStats.stat["totalShockHits"] = %client.totalShockHits;
      %client.dtStats.stat["totalShocks"] = %client.totalShocks;
   }

   function ArenaGame::recalcScore( %game, %client ){
      parent::recalcScore(%game, %client);
      %client.dtStats.stat["dtTeam"] = %client.team;
      %client.dtStats.stat["score"] = %client.score;
      %client.dtStats.stat["kills"] = %client.kills;
      %client.dtStats.stat["deaths"]  = %client.deaths;
      %client.dtStats.stat["teamKills"]  = %client.teamKills;
      %client.dtStats.stat["snipeKills"] = %client.snipeKills;
      %client.dtStats.stat["roundsWon"] = %client.roundsWon;
      %client.dtStats.stat["roundsLost"] = %client.roundsLost;
      %client.dtStats.stat["assists"] = %client.assists;
      %client.dtStats.stat["roundKills"] = %client.roundKills;
      %client.dtStats.stat["hatTricks"] = %client.hatTricks;

   }

   function LCTFGame::recalcScore(%game, %cl){
      parent::recalcScore(%game, %cl);
      %dtStats = %cl.dtStats;
      %dtStats.stat["score"] = %cl.score;
      %dtStats.stat["dtTeam"] = %cl.team;
      %dtStats.stat["offenseScore"] = %cl.offenseScore;
      %dtStats.stat["defenseScore"] = %cl.defenseScore;
      %dtStats.stat["scoreMidAir"] = %cl.scoreMidAir;
   }

   function LCTFGame::awardScoreKill(%game, %killerID){
      %val = parent::awardScoreKill(%game, %killerID);
      %killerID.dtStats.stat["kills"] = %killerID.kills;
      return %val;
   }

   function LCTFGame::awardScoreFlagCap(%game, %cl, %flag){
      parent::awardScoreFlagCap(%game, %cl, %flag);
      %cl.dtStats.stat["flagCaps"]++;
      dtMinMax("flagCaps", "flag", 1, %cl.dtStats.stat["flagCaps"], %cl);
   }

   function LCTFGame::awardScoreFlagTouch(%game, %cl, %flag){
      parent::awardScoreFlagTouch(%game, %cl, %flag);
      %cl.dtStats.stat["flagGrabs"] = %cl.flagGrabs;
      dtMinMax("flagGrabs", "flag", 1, %cl.dtStats.stat["flagGrabs"], %cl);
   }

   function LCTFGame::awardScoreCarrierKill(%game, %killerID){
      %val = parent::awardScoreCarrierKill(%game, %killerID);
      %killerID.dtStats.stat["carrierKills"] = %killerID.carrierKills;
      dtMinMax("carrierKills", "flag", 1, %killerID.dtStats.stat["carrierKills"], %killerID);
      return %val;
   }

   function LCTFGame::awardScoreFlagReturn(%game, %cl, %perc){
      %val = parent::awardScoreFlagReturn(%game, %cl, %perc);
      %cl.dtStats.stat["flagReturns"] = %cl.flagReturns;
      dtMinMax("flagReturns", "flag", 1, %cl.dtStats.stat["flagReturns"], %cl);
      %cl.dtStats.stat["returnPts"] = %cl.returnPts;
      return %val;
   }

   function LCTFGame::awardScoreEscortAssist(%game, %killerID){
      %val = parent::awardScoreEscortAssist(%game, %killerID);
      %killerID.dtStats.stat["escortAssists"] = %killerID.escortAssists;
      dtMinMax("escortAssists", "flag", 1, %killerID.dtStats.stat["escortAssists"], %killerID);
      return %val;
   }

   function LCTFGame::awardScoreFlagDefend(%game, %killerID){
      %val = parent::awardScoreFlagDefend(%game, %killerID);
      %killerID.dtStats.stat["flagDefends"] = %killerID.flagDefends;
      dtMinMax("flagDefends", "flag", 1, %killerID.dtStats.stat["flagDefends"], %killerID);
      return %val;
   }

   function LCTFGame::awardScoreStalemateReturn(%game, %cl){
      %val = parent::awardScoreStalemateReturn(%game, %cl);
      %cl.dtStats.stat["stalemateReturn"]++;
      return %val;
   }

   function SCtFGame::recalcScore(%game, %cl){
      parent::recalcScore(%game, %cl);
      %dtStats = %cl.dtStats;
      %dtStats.stat["score"] = %cl.score;
      %dtStats.stat["dtTeam"] = %cl.team;
      %dtStats.stat["offenseScore"] = %cl.offenseScore;
      %dtStats.stat["defenseScore"] = %cl.defenseScore;
      %dtStats.stat["scoreMidAir"] = %cl.scoreMidAir;
   }

   function SCtFGame::awardScoreKill(%game, %killerID){
      %val = parent::awardScoreKill(%game, %killerID);
      %killerID.dtStats.stat["kills"] = %killerID.kills;
      return %val;
   }

   function SCtFGame::awardScoreFlagCap(%game, %cl, %flag){
      parent::awardScoreFlagCap(%game, %cl, %flag);
      %cl.dtStats.stat["flagCaps"]++;
      dtMinMax("flagCaps", "flag", 1, %cl.dtStats.stat["flagCaps"], %cl);
   }

   function SCtFGame::awardScoreFlagTouch(%game, %cl, %flag){
      parent::awardScoreFlagTouch(%game, %cl, %flag);
      %cl.dtStats.stat["flagGrabs"] = %cl.flagGrabs;
      dtMinMax("flagGrabs", "flag", 1, %cl.dtStats.stat["flagGrabs"], %cl);
   }

   function SCtFGame::awardScoreCarrierKill(%game, %killerID){
      %val = parent::awardScoreCarrierKill(%game, %killerID);
      %killerID.dtStats.stat["carrierKills"] = %killerID.carrierKills;
      dtMinMax("carrierKills", "flag", 1, %killerID.dtStats.stat["carrierKills"], %killerID);
      return %val;
   }

   function SCtFGame::awardScoreFlagReturn(%game, %cl, %perc){
      %val = parent::awardScoreFlagReturn(%game, %cl, %perc);
      %cl.dtStats.stat["flagReturns"] = %cl.flagReturns;
      dtMinMax("flagReturns", "flag", 1, %cl.dtStats.stat["flagReturns"], %cl);
      %cl.dtStats.stat["returnPts"] = %cl.returnPts;
      return %val;
   }

   function SCtFGame::awardScoreEscortAssist(%game, %killerID){
      %val = parent::awardScoreEscortAssist(%game, %killerID);
      %killerID.dtStats.stat["escortAssists"] = %killerID.escortAssists;
      dtMinMax("escortAssists", "flag", 1, %killerID.dtStats.stat["escortAssists"], %killerID);
      return %val;
   }

   function SCtFGame::awardScoreFlagDefend(%game, %killerID){
      %val = parent::awardScoreFlagDefend(%game, %killerID);
      %killerID.dtStats.stat["flagDefends"] = %killerID.flagDefends;
      dtMinMax("flagDefends", "flag", 1, %killerID.dtStats.stat["flagDefends"], %killerID);
      return %val;
   }

   function SCtFGame::awardScoreStalemateReturn(%game, %cl){
      %val = parent::awardScoreStalemateReturn(%game, %cl);
      %cl.dtStats.stat["stalemateReturn"]++;
      return %val;
   }

   //function TurretData::replaceCallback(%this, %turret, %engineer){
      //parent::replaceCallback(%this, %turret, %engineer);
      //if (%engineer.getMountedImage($BackPackSlot) != 0 && $dtStats::Enable){
         //%dtStats = %engineer.client.dtStats;
         //%barrel = %engineer.getMountedImage($BackPackSlot).turretBarrel;
         //switch$(%barrel){
            //case "ELFBarrelPack":
               //%dtStats.ELFBarrelDep++;
            //case "MortarBarrelPack":
               //%dtStats.MortarBarrelDep++;
            //case "PlasmaBarrelPack":
               //%dtStats.PlasmaBarrelDep++;
            //case "AABarrelPack":
               //%dtStats.AABarrelDep++;
            //case "MissileBarrelPack":
               //%dtStats.MissileBarrelDep++;
        //}
      //}
   //}
};
//helps with game types that override functions and dont use parent
// that way we get called first then the gametype can do whatever


// there is no main function for these
function  LCTFGame::awardScoreDeath(%game, %victimID){
   parent::awardScoreDeath(%game, %victimID);
   %victimID.dtStats.stat["deaths"]  = %victimID.deaths;
}

function LCTFGame::awardScoreSuicide(%game, %victimID){
   parent::awardScoreSuicide(%game, %victimID);
   %victimID.dtStats.stat["suicides"]  = %victimID.suicides;
}

function LCTFGame::awardScoreTeamkill(%game, %victimID, %killerID){
   parent::awardScoreTeamkill(%game, %victimID, %killerID);
   %killerID.dtStats.stat["teamKills"] = %killerID.teamKills;
}

function  SCtFGame::awardScoreDeath(%game, %victimID){
   parent::awardScoreDeath(%game, %victimID);
   %victimID.dtStats.stat["deaths"]  = %victimID.deaths;
}

function SCtFGame::awardScoreSuicide(%game, %victimID){
   parent::awardScoreSuicide(%game, %victimID);
   %victimID.dtStats.stat["suicides"]  = %victimID.suicides;
}

function SCtFGame::awardScoreTeamkill(%game, %victimID, %killerID){
   parent::awardScoreTeamkill(%game, %victimID, %killerID);
   %killerID.dtStats.stat["teamKills"] = %killerID.teamKills;
}

function  CTFGame::awardScoreDeath(%game, %victimID){
   parent::awardScoreDeath(%game, %victimID);
   %victimID.dtStats.stat["deaths"]  = %victimID.deaths;
}

function CTFGame::awardScoreSuicide(%game, %victimID){
   parent::awardScoreSuicide(%game, %victimID);
   %victimID.dtStats.stat["suicides"]  = %victimID.suicides;
}

function CTFGame::awardScoreTeamkill(%game, %victimID, %killerID){
   parent::awardScoreTeamkill(%game, %victimID, %killerID);
   %killerID.dtStats.stat["teamKills"] = %killerID.teamKills;
}

function projectileTracker(%p){
   if(isObject(%p)){
      if(isObject(%p.getTargetObject())){
         %p.lastTargetType = %p.getTargetObject().getClassName();
         %p.targetSource = %p.getTargetObject().sourceObject;
      }
      schedule(256,0,"projectileTracker",%p);
   }
}

package dtStatsGame{
   function FlipFlop::playerTouch(%data, %flipflop, %player){
      parent::playerTouch(%data, %flipflop, %player);
      %player.client.dtStats.stat["flipflopCount"]++;
   }

   function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %teamSpecific, %msg){
      parent::serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %teamSpecific, %msg);
      if((!%isAdmin || (%isAdmin && %client.ForceVote))){
         %client.dtStats.stat["voteCount"]++;
         if(%typeName $= "VoteChangeMission"){
            %mission = $HostMissionFile[%arg3];
            %missionType = $HostTypeName[%arg4] @ "Game";
            %map = cleanMapName(%mission);
            $dtServer::voteFor[%map,%missionType]++;
            getMapID(%map,%missionType,0);
         }
      }
   }

   function detonateGrenade(%obj){// from lakRabbitGame.cs for grenade tracking
      %obj.dtNade = 1;
      $dtObjExplode = %obj;
      %obj.sourceObject.client.dtShotSpeed = mFloor(vectorLen(%obj.sourceObject.getVelocity()) * 3.6);
      parent::detonateGrenade(%obj);
   }

   function MineDeployed::onThrow(%this, %mine, %thrower){
      parent::onThrow(%this, %mine, %thrower);
      %thrower.client.lastMineThrow = getSimTime();
      %thrower.client.dtStats.stat["mineShotsFired"]++;
      %thrower.client.dtStats.stat["shotsFired"]++;
      %thrower.client.dtStats.stat["mineACC"] = (%thrower.client.dtStats.stat["mineHits"] / %thrower.client.dtStats.stat["mineShotsFired"]) * 100;
   }

   function SatchelChargeTossed::onThrow(%this, %sat, %thrower){
      parent::onThrow(%this, %sat, %thrower);
      %thrower.client.dtStats.stat["satchelShotsFired"]++;
      %thrower.client.dtStats.stat["shotsFired"]++;
      %thrower.client.dtStats.stat["satchelACC"] = (%thrower.client.dtStats.stat["satchelHits"] / %thrower.client.dtStats.stat["satchelShotsFired"]) * 100;
   }

   function GrenadeThrown::onThrow(%this, %gren,%thrower){
       parent::onThrow(%this, %gren);
      %thrower.client.dtStats.stat["hGrenadeShotsFired"]++;
      %thrower.client.dtStats.stat["shotsFired"]++;
      %thrower.client.dtStats.stat["hGrenadeACC"] = (%thrower.client.dtStats.stat["hGrenadeHits"] / %thrower.client.dtStats.stat["hGrenadeShotsFired"]) * 100;
   }

   function ShapeBaseImageData::onFire(%data, %obj, %slot){
      if(%obj.isCloaked()){
         %obj.isCloakTime = getSimTime();
      }
      %p = parent::onFire(%data, %obj, %slot);
      if(isObject(%p)){
         clientShotsFired(%data.projectile, %obj, %p);
      }
      return %p;
   }

   function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC){
      clientDmgStats(%data,%position,%sourceObject,%targetObject, %damageType,%amount);
      parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
   }

  function SensorJammerPackImage::onMount(%data, %obj, %slot){
      parent::onMount(%data, %obj, %slot);
      %obj.client.dtStats.stat["jammer"]++;
   }

   //0 Fire 1 ??? 2 jump 3 jet 4 gernade 5 mine
   function Armor::onTrigger(%data, %player, %triggerNum, %val){
      parent::onTrigger(%data, %player, %triggerNum, %val);
      if($dtStats::Enable){
         %client = %player.client;
         if(isObject(%player) && !%player.getObjectMount()){
            if(%val){//cut the amount of tiggers in half
//------------------------------------------------------------------------------
               %speed = mFloor(vectorLen(%player.getVelocity()) * 3.6);

               if(%speed > %client.dtStats.stat["maxSpeed"]){%client.dtStats.stat["maxSpeed"] = %speed;}
               %client.dtStats.avgTSpeed += %speed; %client.dtStats.avgSpeedCount++;
               %client.dtStats.stat["avgSpeed"] = %client.dtStats.avgTSpeed/%client.dtStats.avgSpeedCount;
               if(%client.dtStats.avgTSpeed > 999999){%client.dtStats.avgSpeedCount*= 0.5;%client.dtStats.avgTSpeed*=0.5;}
//------------------------------------------------------------------------------
               %xypos = getWords(%player.getPosition(),0,1) SPC 0;
               if(%client.lp !$= ""){
                  %dis = mFloor(vectorDist(%client.lp,%xypos));
                  %client.dtStats.stat["distMov"] = %client.dtStats.stat["distMov"] + (%dis/1000);
               }
               %client.lp = %xypos;
//------------------------------------------------------------------------------
            }
            if (%triggerNum == 3){ //jet triggers
               if(%val){
                  if(isEventPending(%player.jetTimeTest)){
                     cancel(%player.jetTimeTest);
                  }
                  if(%client.ground){
                     if(%client.gt > 0){
                        %client.dtStats.stat["groundTime"] += ((getSimTime() - %client.gt)/1000)/60;
                     }
                     %client.at =  getSimTime();
                  }
                  %client.ground = 0;
               }
               else{
                   if(!isEventPending(%player.jetTimeTest)){
                     %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
                     %rayStart = %player.getWorldBoxCenter();
                     %rayEnd = VectorAdd(%rayStart,"0 0" SPC (10000 * -1));
                     %raycast = ContainerRayCast(%rayStart, %rayEnd, %mask, %player);
                     %groundPos = getWords(%raycast, 1, 3);
                     %dis = vectorDist(%player.getPosition(),%groundPos);
                     %zv = getWord(%player.getVelocity(),2);
                     %time = (((%zv + mSqrt(mPow((%zv),2) + 2 * mAbs(getGravity()) * %dis)) / mAbs(getGravity()))* 1000);
                     %player.jetTimeTest = schedule(%time,0,"chkGrounded",%player);
                  }
               }
            }
         }
         else{
            %client.lp = "";
            %client.gt = %client.at = 0;
         }
      }
   }

   function StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType){
      clientDmgStats(%data,%position,%sourceObject,%targetObject, %damageType,%amount);
      parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
   }

   function SniperRifleImage::onFire(%data,%obj,%slot){
      parent::onFire(%data,%obj,%slot);
      clientShotsFired(%data.projectile, %obj, %obj.lastProjectile);
   }

   function ShockLanceImage::onFire(%this, %obj, %slot){
      if(%obj.isCloaked()){
         %obj.isCloakTime = getSimTime();
      }
      %p = parent::onFire(%this, %obj, %slot);
      clientShotsFired(ShockLanceImage.projectile, %obj, %p);
      return %p;
   }

   function Armor::onMount(%this,%obj,%vehicle,%node){
      parent::onMount(%this,%obj,%vehicle,%node);
      %obj.client.vehDBName = %vehicle.getDataBlock().getName();
      %obj.client.gt = %obj.client.at = 0;// resets fly/ground time
   }

   function RepairGunImage::onRepair(%this, %obj, %slot){
       Parent::onRepair(%this, %obj, %slot);
       %target = %obj.repairing;
       if(%target && %target.team != %obj.team){
          if(%target != %obj.rpEnemy){
             %obj.rpEnemy = %target;
             %obj.client.dtStats.stat["repairEnemy"]++;
          }
       }
   }

   ////////////////////////////////////////////////////////////////////////////////
   function CTFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %ftime = getSimTime() - %game.dtTotalFlagTime[%flag];
      %player.client.dtStats.stat["flagTimeMin"] += (%ftime/1000)/60;
      %game.dtTotalFlagTime[%flag] = 0;
      if(%player.getState() !$= "Dead"){
         %player.client.dtStats.stat["flagToss"]++;
         %flag.pass = 1;
         %flag.lastDTStat = %player.client.dtStats;
      }
      else{
         %flag.pass = 0;
      }
      parent::playerDroppedFlag(%game, %player);
   }

   function CTFGame::boundaryLoseFlag(%game, %player){
      %flag = %player.holdingFlag;

      %ftime = getSimTime() - %game.dtTotalFlagTime[%flag];
      %player.client.dtStats.stat["flagTimeMin"] += (%ftime/1000)/60;

      %game.dtTotalFlagTime[%flag] = 0;
     parent::boundaryLoseFlag(%game, %player);
   }

   function CTFGame::updateFlagTransform(%game, %flag){
      parent::updateFlagTransform(%game, %flag);
      %flag.speed = vectorLen(%flag.getVelocity());
   }

   function CTFGame::playerTouchEnemyFlag(%game, %player, %flag){
      if(%flag.isHome){
         %game.dtTotalFlagTime[%flag] = getSimTime();
         %player.client.dtStats.stat["flagGrabAtStand"]++;
      }
      if(!%player.flagTossWait){
         if(%flag.speed > 10 && %flag.pass && %player.client.dtStats != %flag.lastDTStat){
            %player.client.dtStats.stat["flagCatch"]++;
            %speed = vectorLen(%player.getVelocity()) * 3.6;
            %player.client.dtStats.stat["flagCatchSpeed"] = (%player.client.dtStats.stat["flagCatchSpeed"] > %speed) ? %player.client.dtStats.stat["flagCatchSpeed"] : %speed;
            if(rayTest(%player, $dtStats::midAirHeight)){
               %player.client.dtStats.stat["maFlagCatch"]++;
               %player.client.dtStats.stat["maFlagCatchSpeed"] = (%player.client.dtStats.stat["maFlagCatchSpeed"] > %speed) ? %player.client.dtStats.stat["maFlagCatchSpeed"] : %speed;
            }
            if(isObject(%flag.lastDTStat)){
               %flag.lastDTStat.stat["flagTossCatch"]++;
               %flag.lastDTStat = -1;
            }
            %flag.speed = 0;
            %flag.pass = 0;
         }
         else if(%flag.pass && %player.client.dtStats != %flag.lastDTStat){
            if(isObject(%flag.lastDTStat)){
               %flag.lastDTStat.flagTossGrab++;
               %flag.lastDTStat = -1;
            }
            %flag.speed = 0;
            %flag.pass = 0;
         }
         %grabspeed = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);
         if(%grabSpeed > %player.client.dtStats.stat["grabSpeed"] || !%player.client.dtStats.stat["grabSpeed"]){
            %player.client.dtStats.stat["grabSpeed"]  = %grabSpeed;
            dtMinMax("grabSpeed", "flag", 1, %player.client.dtStats.stat["grabSpeed"],  %player.client);
         }
      }
      parent::playerTouchEnemyFlag(%game, %player, %flag);
   }

   function CTFGame::flagCap(%game, %player){
      %flag = %player.holdingFlag;
      %clTeam = %player.client.team;
      %dtStats = %player.client.dtStats;
      %time = ((getSimTime() - $missionStartTime)/1000)/60;
      if(%clTeam == 1){
         $dtStats::teamOneCapCount++;
         if($dtStats::teamOneCapCount == 1)
            $dtStats::teamOneCapTimes = 0 @ "," @ cropFloat(%time,1);
         else
            $dtStats::teamOneCapTimes = $dtStats::teamOneCapTimes  @ "," @ cropFloat(%time,1);
      }
      else{
         $dtStats::teamTwoCapCount++;
         if($dtStats::teamTwoCapCount == 1)
            $dtStats::teamTwoCapTimes = 0 @ "," @ cropFloat(%time,1);
         else
            $dtStats::teamTwoCapTimes  = $dtStats::teamTwoCapTimes  @ "," @ cropFloat(%time,1);
      }
      if(%game.dtTotalFlagTime[%flag]){
         %heldTime = (getSimTime() - %game.dtTotalFlagTime[%flag])/1000;
         %dtStats.stat["flagTimeMin"] += %heldTime/60;

         if(%heldTime < %dtStats.stat["heldTimeSec"] || !%dtStats.stat["heldTimeSec"]){
            %dtStats.stat["heldTimeSec"]  = %heldTime;
            dtMinMax("heldTimeSec", "flag", 2, %heldTime,  %player.client);


         }
         if($dtStats::ctfTimes){
            %heldTimeMS = getSimTime() - %game.dtTotalFlagTime[%flag];
            %fTime = %game.formatTime(%heldTimeMS, true);
            bottomprint(%player.client, "You captured the flag in" SPC %fTime SPC "seconds.", 10, 1);
            if(($HostGamePlayerCount - $HostGameBotCount) >= $dtStats::ctfTimesPlayerLimit){
               %mapName  = cleanMapName($missionName);
               if(%heldTimeMS < getField($dtServer::capTimes[%mapName,%game.class,%clTeam], 0) || getFieldCount($dtServer::capTimes[%mapName,%game.class,%clTeam]) != 2){
                  if(getFieldCount($dtServer::capTimes[%mapName,%game.class,%clTeam]) == 2){
                     %oldTime = getField($dtServer::capTimes[%mapName,%game.class,%clTeam], 0);
                     %saved = "\c2Saved: \c3-" @ %game.formatTime(%oldTime - %heldTimeMS, true) @ "\c2";
                  }
                  //schedule(2000, 0, "messageAll", 'MsgCTFNewRecord', "\c2It's a new record! Time: \c3"@ %fTime @"\c2 " @ %saved  @ "~wfx/misc/hunters_horde.wav");
                  schedule(4000, 0, "messageAll", 'MsgCTFNewRecord', '\c2It\'s a new %3 record! Time: \c3%1 \c2%2  ~wfx/misc/hunters_horde.wav',%fTime,%saved,$TeamName[%clTeam]);
                  $dtServer::capTimes[%mapName,%game.class,%clTeam] = %heldTimeMS TAB %dtStats.name;
               }
            }
         }
      }
      parent::flagCap(%game, %player);
   }

   function CTFGame::playerTouchOwnFlag(%game, %player, %flag){
      if(!%flag.isHome){
         if(%flag.speed > 10 && %flag.pass){
            %player.client.dtStats.stat["interceptedFlag"]++;
            %speed = vectorLen(%player.getVelocity() * 3.6);
            %player.client.dtStats.stat["interceptSpeed"] = (%player.client.dtStats.stat["interceptSpeed"] > %speed) ? %player.client.dtStats.stat["interceptSpeed"] : %speed;
            %player.client.dtStats.stat["interceptFlagSpeed"] = (%player.client.dtStats.stat["interceptFlagSpeed"] > %flag.speed) ? %player.client.dtStats.stat["interceptFlagSpeed"] : %flag.speed;
            if(rayTest(%player, $dtStats::midAirHeight))
               %player.client.dtStats.stat["maInterceptedFlag"]++;
         }
      }
      parent::playerTouchOwnFlag(%game, %player, %flag);
   }

/////////////////////////////////////////////////////////////////////////////
   function LCTFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;

      %ftime = getSimTime() - %game.dtTotalFlagTime[%flag];
      %player.client.dtStats.stat["flagTimeMin"] += (%ftime/1000)/60;

      %game.dtTotalFlagTime[%flag] = 0;
      if(%player.getState() !$= "Dead"){
         %player.client.dtStats.stat["flagToss"]++;
         %flag.pass = 1;
         %flag.lastDTStat = %player.client.dtStats;
      }
      else{
         %flag.pass = 0;
      }
      parent::playerDroppedFlag(%game, %player);
   }

   function LCTFGame::boundaryLoseFlag(%game, %player){
      %flag = %player.holdingFlag;

      %ftime = getSimTime() - %game.dtTotalFlagTime[%flag];
      %player.client.dtStats.stat["flagTimeMin"] += (%ftime/1000)/60;

      %game.dtTotalFlagTime[%flag] = 0;
      parent::boundaryLoseFlag(%game, %player);
   }

   function LCTFGame::updateFlagTransform(%game, %flag){
      parent::updateFlagTransform(%game, %flag);
      %vel = %flag.getVelocity();
      %flag.speed = vectorLen(%vel) ;
   }

   function LCTFGame::playerTouchEnemyFlag(%game, %player, %flag){
      if(%flag.isHome){
         %game.dtTotalFlagTime[%flag] = getSimTime();
         %player.client.dtStats.stat["flagGrabAtStand"]++;
      }
      if(!%player.flagTossWait){
         if(%flag.speed > 10 && %flag.pass && %player.client.dtStats != %flag.lastDTStat){
            %player.client.dtStats.stat["flagCatch"]++;
            %speed = vectorLen(%player.getVelocity() * 3.6);
            %player.client.dtStats.stat["flagCatchSpeed"] = (%player.client.dtStats.stat["flagCatchSpeed"] > %speed) ? %player.client.dtStats.stat["flagCatchSpeed"] : %speed;
            if(rayTest(%player, $dtStats::midAirHeight)){
               %player.client.dtStats.stat["maFlagCatch"]++;
               %player.client.dtStats.stat["maFlagCatchSpeed"] = (%player.client.dtStats.stat["maFlagCatchSpeed"] > %speed) ? %player.client.dtStats.stat["maFlagCatchSpeed"] : %speed;
            }
            if(isObject(%flag.lastDTStat)){
               %flag.lastDTStat.stat["flagTossCatch"]++;
               %flag.lastDTStat = -1;
            }
            %flag.speed = 0;
            %flag.pass = 0;

         }
         %grabspeed = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);
         if(%grabSpeed > %player.client.dtStats.stat["grabSpeed"] || !%player.client.dtStats.stat["grabSpeed"]){
            %player.client.dtStats.stat["grabSpeed"]  = %grabSpeed;
            dtMinMax("grabSpeed", "flag", 1, %player.client.dtStats.stat["grabSpeed"],  %player.client);
         }
      }
      parent::playerTouchEnemyFlag(%game, %player, %flag);
   }

   function LCTFGame::flagCap(%game, %player){
      %flag = %player.holdingFlag;
      %clTeam = %player.client.team;
      %dtStats = %player.client.dtStats;
      %time = ((getSimTime() - $missionStartTime)/1000)/60;
       if(%clTeam == 1){
         $dtStats::teamOneCapCount++;
         if($dtStats::teamOneCapCount == 1)
            $dtStats::teamOneCapTimes = 0 @ "," @ cropFloat(%time,1);
         else
            $dtStats::teamOneCapTimes = $dtStats::teamOneCapTimes  @ "," @ cropFloat(%time,1);
      }
      else{
         $dtStats::teamTwoCapCount++;
         if($dtStats::teamTwoCapCount == 1)
            $dtStats::teamTwoCapTimes = 0 @ "," @ cropFloat(%time,1);
         else
            $dtStats::teamTwoCapTimes  = $dtStats::teamTwoCapTimes  @ "," @ cropFloat(%time,1);
      }
      if(%game.dtTotalFlagTime[%flag]){
         %heldTime = (getSimTime() - %game.dtTotalFlagTime[%flag])/1000;
         %dtStats.stat["flagTimeMin"]  += %heldTime/60;
         if(%heldTime < %dtStats.stat["heldTimeSec"] || !%dtStats.stat["heldTimeSec"]){
            %dtStats.stat["heldTimeSec"]  = %heldTime;
            dtMinMax("heldTimeSec", "flag", 2, %heldTime,  %player.client);
         }
         if($dtStats::ctfTimes){
            %heldTimeMS = getSimTime() - %game.dtTotalFlagTime[%flag];
            %fTime = %game.formatTime(%heldTimeMS, true);
            bottomprint(%player.client, "You captured the flag in" SPC %fTime SPC "seconds.", 10, 1);
            if(($HostGamePlayerCount - $HostGameBotCount) >= $dtStats::ctfTimesPlayerLimit){
               %mapName  = cleanMapName($missionName);
               if(%heldTimeMS < getField($dtServer::capTimes[%mapName,%game.class,%clTeam], 0) || getFieldCount($dtServer::capTimes[%mapName,%game.class,%clTeam]) != 2){
                  if(getFieldCount($dtServer::capTimes[%mapName,%game.class,%clTeam]) == 2){
                     %oldTime = getField($dtServer::capTimes[%mapName,%game.class,%clTeam], 0);
                     %saved = "\c2Saved: \c3-" @ %game.formatTime(%oldTime - %heldTimeMS, true) @ "\c2";
                  }
                  //schedule(2000, 0, "messageAll", 'MsgCTFNewRecord', "\c2It's a new record! Time: \c3"@ %fTime @"\c2 " @ %saved  @ "~wfx/misc/hunters_horde.wav");
                  schedule(4000, 0, "messageAll", 'MsgCTFNewRecord', '\c2It\'s a new %3 record! Time: \c3%1 \c2%2  ~wfx/misc/hunters_horde.wav',%fTime,%saved,$TeamName[%clTeam]);
                  $dtServer::capTimes[%mapName,%game.class,%clTeam] = %heldTimeMS TAB %dtStats.name;
               }
            }
         }
      }
      parent::flagCap(%game, %player);
   }

   function LCTFGame::playerTouchOwnFlag(%game, %player, %flag){
      if(!%flag.isHome){
         if(%flag.speed > 10 && %flag.pass){
            %player.client.dtStats.stat["interceptedFlag"]++;
            %speed = vectorLen(%player.getVelocity() * 3.6);
            %player.client.dtStats.stat["interceptSpeed"] = (%player.client.dtStats.stat["interceptSpeed"] > %speed) ? %player.client.dtStats.stat["interceptSpeed"] : %speed;
            %player.client.dtStats.stat["interceptFlagSpeed"] = (%player.client.dtStats.stat["interceptFlagSpeed"] > %flag.speed) ? %player.client.dtStats.stat["interceptFlagSpeed"] : %flag.speed;
            if(rayTest(%player, $dtStats::midAirHeight))
               %player.client.dtStats.stat["maInterceptedFlag"]++;
         }
      }
      parent::playerTouchOwnFlag(%game, %player, %flag);
   }

   function SCtFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;

      %ftime = getSimTime() - %game.dtTotalFlagTime[%flag];
      %player.client.dtStats.stat["flagTimeMin"] += (%ftime/1000)/60;

      %game.dtTotalFlagTime[%flag] = 0;
      if(%player.getState() !$= "Dead"){
         %player.client.dtStats.stat["flagToss"]++;
         %flag.pass = 1;
         %flag.lastDTStat = %player.client.dtStats;
      }
      else{
         %flag.pass = 0;
      }
      parent::playerDroppedFlag(%game, %player);
   }

   function SCtFGame::boundaryLoseFlag(%game, %player){
      %flag = %player.holdingFlag;

      %ftime = getSimTime() - %game.dtTotalFlagTime[%flag];
      %player.client.dtStats.stat["flagTimeMin"] += (%ftime/1000)/60;

      %game.dtTotalFlagTime[%flag] = 0;
      parent::boundaryLoseFlag(%game, %player);
   }

   function SCtFGame::updateFlagTransform(%game, %flag){
      parent::updateFlagTransform(%game, %flag);
      %vel = %flag.getVelocity();
      %flag.speed = vectorLen(%vel) ;
   }

   function SCtFGame::playerTouchEnemyFlag(%game, %player, %flag){
      if(%flag.isHome){
         %game.dtTotalFlagTime[%flag] = getSimTime();
         %player.client.dtStats.stat["flagGrabAtStand"]++;
      }
      if(!%player.flagTossWait){
         if(%flag.speed > 10 && %flag.pass && %player.client.dtStats != %flag.lastDTStat){
            %player.client.dtStats.stat["flagCatch"]++;
            %speed = vectorLen(%player.getVelocity() * 3.6);
            %player.client.dtStats.stat["flagCatchSpeed"] = (%player.client.dtStats.stat["flagCatchSpeed"] > %speed) ? %player.client.dtStats.stat["flagCatchSpeed"] : %speed;
            if(rayTest(%player, $dtStats::midAirHeight)){
               %player.client.dtStats.stat["maFlagCatch"]++;
               %player.client.dtStats.stat["maFlagCatchSpeed"] = (%player.client.dtStats.stat["maFlagCatchSpeed"] > %speed) ? %player.client.dtStats.stat["maFlagCatchSpeed"] : %speed;
            }
            if(isObject(%flag.lastDTStat)){
               %flag.lastDTStat.stat["flagTossCatch"]++;
               %flag.lastDTStat = -1;
            }
            %flag.speed = 0;
            %flag.pass = 0;

         }
         %grabspeed = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);
         if(%grabSpeed > %player.client.dtStats.stat["grabSpeed"] || !%player.client.dtStats.stat["grabSpeed"]){
            %player.client.dtStats.stat["grabSpeed"]  = %grabSpeed;
            dtMinMax("grabSpeed", "flag", 1, %player.client.dtStats.stat["grabSpeed"],  %player.client);
         }
      }
      parent::playerTouchEnemyFlag(%game, %player, %flag);
   }

   function SCtFGame::flagCap(%game, %player){
      %flag = %player.holdingFlag;
      %clTeam = %player.client.team;
      %dtStats = %player.client.dtStats;
      %time = ((getSimTime() - $missionStartTime)/1000)/60;
       if(%clTeam == 1){
         $dtStats::teamOneCapCount++;
         if($dtStats::teamOneCapCount == 1)
            $dtStats::teamOneCapTimes = 0 @ "," @ cropFloat(%time,1);
         else
            $dtStats::teamOneCapTimes = $dtStats::teamOneCapTimes  @ "," @ cropFloat(%time,1);
      }
      else{
         $dtStats::teamTwoCapCount++;
         if($dtStats::teamTwoCapCount == 1)
            $dtStats::teamTwoCapTimes = 0 @ "," @ cropFloat(%time,1);
         else
            $dtStats::teamTwoCapTimes  = $dtStats::teamTwoCapTimes  @ "," @ cropFloat(%time,1);
      }
      if(%game.dtTotalFlagTime[%flag]){
         %heldTime = (getSimTime() - %game.dtTotalFlagTime[%flag])/1000;
         %dtStats.stat["flagTimeMin"]  += %heldTime/60;
         if(%heldTime < %dtStats.stat["heldTimeSec"] || !%dtStats.stat["heldTimeSec"]){
            %dtStats.stat["heldTimeSec"]  = %heldTime;
            dtMinMax("heldTimeSec", "flag", 2, %heldTime,  %player.client);
         }
         if($dtStats::ctfTimes){
            %heldTimeMS = getSimTime() - %game.dtTotalFlagTime[%flag];
            %fTime = %game.formatTime(%heldTimeMS, true);
            bottomprint(%player.client, "You captured the flag in" SPC %fTime SPC "seconds.", 10, 1);
            if(($HostGamePlayerCount - $HostGameBotCount) >= $dtStats::ctfTimesPlayerLimit){
               %mapName  = cleanMapName($missionName);
               if(%heldTimeMS < getField($dtServer::capTimes[%mapName,%game.class,%clTeam], 0) || getFieldCount($dtServer::capTimes[%mapName,%game.class,%clTeam]) != 2){
                  if(getFieldCount($dtServer::capTimes[%mapName,%game.class,%clTeam]) == 2){
                     %oldTime = getField($dtServer::capTimes[%mapName,%game.class,%clTeam], 0);
                     %saved = "\c2Saved: \c3-" @ %game.formatTime(%oldTime - %heldTimeMS, true) @ "\c2";
                  }
                  //schedule(2000, 0, "messageAll", 'MsgCTFNewRecord', "\c2It's a new record! Time: \c3"@ %fTime @"\c2 " @ %saved  @ "~wfx/misc/hunters_horde.wav");
                  schedule(4000, 0, "messageAll", 'MsgCTFNewRecord', '\c2It\'s a new %3 record! Time: \c3%1 \c2%2  ~wfx/misc/hunters_horde.wav',%fTime,%saved,$TeamName[%clTeam]);
                  $dtServer::capTimes[%mapName,%game.class,%clTeam] = %heldTimeMS TAB %dtStats.name;
               }
            }
         }
      }
      parent::flagCap(%game, %player);
   }
   function SCtFGame::playerTouchOwnFlag(%game, %player, %flag){
      if(!%flag.isHome){
         if(%flag.speed > 10 && %flag.pass){
            %player.client.dtStats.stat["interceptedFlag"]++;
            %speed = vectorLen(%player.getVelocity() * 3.6);
            %player.client.dtStats.stat["interceptSpeed"] = (%player.client.dtStats.stat["interceptSpeed"] > %speed) ? %player.client.dtStats.stat["interceptSpeed"] : %speed;
            %player.client.dtStats.stat["interceptFlagSpeed"] = (%player.client.dtStats.stat["interceptFlagSpeed"] > %flag.speed) ? %player.client.dtStats.stat["interceptFlagSpeed"] : %flag.speed;
            if(rayTest(%player, $dtStats::midAirHeight))
               %player.client.dtStats.stat["maInterceptedFlag"]++;
         }
      }
      parent::playerTouchOwnFlag(%game, %player, %flag);
   }
};
activatePackage(dtStats);

function chkGrounded(%player){
   if(isObject(%player)){
      %client =  %player.client;
      if(!%client.ground){
         if(%client.at > 0){
            %client.dtStats.stat["airTime"] += ((getSimTime() - %client.at)/1000)/60;
         }
         %client.gt =  getSimTime();
      }
      %client.ground = 1;
      %player.jetTimeTest = 0;
   }
 // error(%client.stat["airTime"] SPC %client.stat["groundTime"]);
}

function dtScanForRepair(){
   InitContainerRadiusSearch("0 0 0",  9000, $TypeMasks::ItemObjectType);
   while ((%itemObj = containerSearchNext()) != 0){
      if(%itemObj.getDatablock().getName() $= "RepairPack"){
         %repairList[%c++] = %itemObj;
      }
   }
  for(%i = 1; %i <= %c; %i++){
     %itemObj = %repairList[%i];
      InitContainerRadiusSearch("0 0 0",  9000, $TypeMasks::ItemObjectType | $TypeMasks::StationObjectType | $TypeMasks::SensorObjectType | $TypeMasks::GeneratorObjectType  | $TypeMasks::TurretObjectType);           //| $TypeMasks::PlayerObjectType
      %disMin = 0;
      while ((%teamObj = containerSearchNext()) != 0){
         if(%teamObj.getType() & $TypeMasks::ItemObjectType && %teamObj.team == 0)
            continue;
         if(%teamObj.team > -1){
            %dis  = vectorDist(%itemObj.getPosition(),%teamObj.getPosition());
            if(%dis < %disMin || %disMin == 0){
               %disMin = %dis;
               %itemObj.team = %teamObj.team;
            }
         }
      }
  }
}

function dtStatsMissionDropReady(%game, %client){ // called when client has finished loading
   if($dtStats::debugEchos){error("dtStatsMissionDropReady GUID = "  SPC %client.guid);}
   if($HostGamePlayerCount > $dtServer::maxPlayers[cleanMapName($CurrentMission),%game.class])
      $dtServer::maxPlayers[cleanMapName($CurrentMission),%game.class] = $HostGamePlayerCount;

   %client.lp = "";//last position for distMove
   %client.lgame = %game.class;
   %foundOld = 0;
   %mrx = setGUIDName(%client);// make sure we  have a guid if not make one for this name
   %authInfo = %client.getAuthInfo();
   %realName = getField( %authInfo, 0 );
   if(%realName !$= "")
      %name = %realName;
   else
      %name =  stripChars( detag( getTaggedString( %client.name ) ), "\cp\co\c6\c7\c8\c9\c0" );

   if(!isObject(%client.dtStats)){
      for (%i = 0; %i < statsGroup.getCount(); %i++){ // check to see if my old data is still there
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.guid == %client.guid){
            %foundOld =1;
            %client.dtStats = %dtStats;
            %dtStats.client = %client;
            %dtStats.clientLeft = 0;
            %dtStats.stat["clientQuit"] = 0;
            %dtStats.markForDelete = 0;
            if(%dtStats.leftID == $dtStats::leftID){
               $dtServer::mapReconnects[cleanMapName($CurrentMission),%game.class]++;
            }
            if(isGameRun() && %dtStats.leftID == $dtStats::leftID && %dtStats.stat["score"] != 0){// make sure game is running and we are on the same map
               resGameStats(%client,%game.class); // restore stats;
            }
            else{
               resetDtStats(%dtStats,%game.class,1);
            }
            if(%client.stat["score"] != 0){
               messageClient(%client, 'MsgClient', '\crWelcome back %1. Your score has been restored.~wfx/misc/rolechange.wav', %client.name);
            }
            break;
         }
      }
      if(!%foundOld){
         %dtStats = new scriptObject(); // object used stats storage
         statsGroup.add(%dtStats);
         %client.dtStats = %dtStats;
         %dtStats.client =%client;
         %dtStats.guid = %client.guid;
         %dtStats.clientLeft = 0;
         %dtStats.stat["clientQuit"] = 0;
         %dtStats.markForDelete = 0;
         %dtStats.name = %name;
         $dtStats::tbLookUP[%client.guid] = %dtStats;
      }
   }
   else{
     %dtStats = %client.dtStats;
   }

   %dtStats.joinPCT = (isGameRun() == 1) ? %game.getGamePct() : 0;
   updateTeamTime(%dtStats, -1);
   %dtStats.team = %client.team;// should be 0
   if(isObject(%dtStats) && %dtStats.gameData[%game.class] != 1){ // game type change
      %dtStats.gameStats["totalGames","g",%game.class] = 0;
      %dtStats.gameStats["statsOverWrite","g",%game.class] = -1;
      %dtStats.gameStats["fullSet","g",%game.class] = 0;
      resetDtStats(%dtStats,%game.class,1);
      %dtStats.gameData[%game.class] = 0;
   }
   %dtStats.mapTime = getSimTime();
}

function dtStatsClientLeaveGame(%client){
   $dtServerVars::lastPlayerCount =  $HostGamePlayerCount - $HostGameBotCount;

   if(isGameRun()){// if they dc during game over dont count it
      $dtServer::mapDisconnects[cleanMapName($CurrentMission),Game.class]++;
      if(%client.score != 0)
         $dtServer::mapDisconnectsScore[cleanMapName($CurrentMission),Game.class]++;
   }

   if(isObject(%client.dtStats)){
      %client.dtStats.clientLeft = 1;
      %client.dtStats.isBot = (%client.isWatchOnly == 1);
      %dtStats.stat["clientQuit"] = isGameRun();
      %client.dtStats.leftTime = getSimTime();
      %client.dtStats.leftID = $dtStats::leftID;
      if(isObject(Game)){
         %client.dtStats.leftPCT = Game.getGamePct();
         if(isGameRun() && %client.score != 0){
            updateTeamTime(%client.dtStats, %client.dtStats.team);
            armorTimer(%client.dtStats, 0, 1);
         }
      }
      else{
         %client.dtStats.leftPCT  = 100;
      }
   }
}

function dtStatsGameOver( %game ){
   if($dtStats::debugEchos){error("dtStatsGameOver");}
   
   $dtStats::tmMode = $Host::TournamentMode;
   
   if(!$dtStats::statsSave){//in case of admin skip map and it has not finished saving the old map'
   $dtStats::serverHang = $dtStats::hostHang = 0;
   $dtStats::LastMissionDN = $MissionDisplayName;
   $dtStats::LastMissionCM = $CurrentMission;
   $dtStats::LastGameType = %game.class;
   $dtStats::LastGameID = $dtStats::gameID;
   $dtStats::statsSave = 1;
      if(%game.class $= "CTFGame" || %game.class $= "LCTFGame" || %game.class $= "SCtFGame"  || %game.class $= "ArenaGame" ){
         if($dtStats::tmMode){
            if(!isObject(pugList)){
               new simGroup(pugList);
               rootGroup.add(pugList);
            }
            %tmClass = "TM" @ %game.class;
            if(!isObject(%tmClass)){
               new simGroup(%tmClass){
                  game = %game.class;
               };
               pugList.add(%tmClass);
            }
            %so = new scriptObject(){
               pugID = $dtStats::gameID;
               mapName = $MissionDisplayName;
               date =  formattimestring("M-d-yy");
               teamOne = $TeamScore[1];
               teamTwo = $TeamScore[2];
               gameType = %game.class;
               count = %tmClass.count;
               mark = dtMarkDate();
            };
            %tmClass.add(%so);
            if(%tmClass.getCount() > 50){
               %max = 0;
               for(%i = 0; %i < %tmClass.getCount(); %i++){
                  %obj =  %tmClass.getObject(%i);
                  %delta = getTimeDelta(%obj.mark);
                  if(%max < %delta){
                     %max = %delta;
                     %delObj = %obj;
                  }
               }
               if(isObject(%delObj)){
                  %delObj.delete();
               }
            }
         }
         else{
            if(!isObject(pubList)){
               new simGroup(pubList);
               rootGroup.add(pubList);
            }
            %gmClass = "GM" @ %game.class;
            if(!isObject(%gmClass)){
               new simGroup(%gmClass){
                  game = %game.class;
               };
               pubList.add(%gmClass);
            }
            %so = new scriptObject(){
               pugID = $dtStats::gameID;
               mapName = $MissionDisplayName;
               date =  formattimestring("M-d-yy");
               teamOne = $TeamScore[1];
               teamTwo = $TeamScore[2];
               gameType = %game.class;
               count = %gmClass.count;
               mark = dtMarkDate();
            };
            %gmClass.add(%so);
            if(%gmClass.getCount() > 100){
               %max = 0;
               for(%i = 0; %i < %gmClass.getCount(); %i++){
                  %obj =  %gmClass.getObject(%i);
                  %delta = getTimeDelta(%obj.mark);
                  if(%max < %delta){
                     %max = %delta;
                     %delObj = %obj;
                  }
               }
               if(isObject(%delObj)){
                  %delObj.delete();
               }
            }
         }
      }

      if(%game.getGamePct() > 90){
         $dtServer::playCount[cleanMapName($CurrentMission),%game.class]++;
         $dtServer::lastPlay[cleanMapName($CurrentMission),%game.class] = getDayNum() TAB getYear() TAB formattimestring("mm/dd/yy hh:nn:a");
         if(%game.numTeams > 1){
            if($teamScore[1] > $teamScore[2]){
               $dtServer::teamOneWin[cleanMapName($CurrentMission),%game.class]++;
               $dtServer::teamTwoLoss[cleanMapName($CurrentMission),%game.class]++;
            }
            else if ($teamScore[2] > $teamScore[1]){
               $dtServer::teamOneLoss[cleanMapName($CurrentMission),%game.class]++;
               $dtServer::teamTwoWin[cleanMapName($CurrentMission),%game.class]++;
            }
         }
      }
      else
         $dtServer::skipCount[cleanMapName($CurrentMission),%game.class]++;

      statsGroup.stat["firstKill"] = 0;
      if($dtStats::debugEchos){error("dtStatsGameOver2");}
      if(%game.numTeams > 1){
         statsGroup.team[1] = $teamScore[1];
         statsGroup.team[2] = $teamScore[2];
      }
      %timeNext =0;
      %time = 2000;
      for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients data
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.clientLeft || !isObject(%dtStats.client)){ // find any that left during the match and
            %dtStats.markForDelete = 1;
            if($dtStats::Enable){
               %game.postGameStats(%dtStats);
               if(!%dtStats.gameData[%game.class,$dtStats::tmMode]){
                  %time += $dtStats::saveTime;
                  schedule(%time,0,"loadGameStats",%dtStats,%game.class);
               }
               else if($dtStats::reloadTotal){
                     %time += $dtStats::saveTime;
                   schedule(%time,0,"loadGameTotalStats",%dtStats,%game.class);
               }
               %time += $dtStats::saveTime;
               schedule(%time,0,"incGameStats",%dtStats,%game.class);
               %time += $dtStats::saveTime;
               schedule(%time,0,"saveGameTotalStats",%dtStats,%game.class);
            }
         }
         else if(isObject(%dtStats.client)){// make sure client is still a thing
            %client = %dtStats.client;
            %client.dtStats.isBot = (%client.isWatchOnly == 1);
            %client.viewMenu = %client.viewClient = %client.viewStats = 0;//reset hud
            %client.lastPage   = 1; %client.lgame = %game;

            if($dtStats::Enable){
               %game.postGameStats(%dtStats);
               if(!%dtStats.gameData[%game.class, $dtStats::tmMode]){
                  %time += $dtStats::saveTime;
                  schedule(%time,0,"loadGameStats",%dtStats,%game.class);
               }
                else if($dtStats::reloadTotal){
                   %time += $dtStats::saveTime;
                   schedule(%time,0,"loadGameTotalStats",%dtStats,%game.class);
               }
               %time += $dtStats::saveTime; // this will chain them
               schedule(%time ,0,"incGameStats",%dtStats,%game.class); //resetDtStats after incGame
               %time += $dtStats::saveTime;
               schedule(%time,0,"saveGameTotalStats",%dtStats,%game.class); //
            }
            else{
               resetDtStats(%dtStats,%game.class,0);
            }
         }
         else{
            error("Logic issue in dtStatsGameOver" SPC %dtStats SPC %client SPC %game.class);
            %dtStats.delete();
         }
      }
      %time += $dtStats::saveTime;
      schedule(%time,0,"dtSaveDone");
      endGameTB(%game);
      $dtStats::reloadTotal = 0;
   }
}

// happens in gameover
function CTFGame::setupClientTeams(%game){
   if(!$TB::TBEnable[$dtStats::gtNameShort[%game.class]]){
      parent::setupClientTeams(%game);
   }
}

function LCTFGame::setupClientTeams(%game){
   if(!$TB::TBEnable[$dtStats::gtNameShort[%game.class]]){
      parent::setupClientTeams(%game);
   }
}

function SCtFGame::setupClientTeams(%game){
   if(!$TB::TBEnable[$dtStats::gtNameShort[%game.class]]){
      parent::setupClientTeams(%game);
   }
}

function ArenaGame::setupClientTeams(%game){
   if(!$TB::TBEnable[$dtStats::gtNameShort[%game.class]]){
      parent::setupClientTeams(%game);
   }
}

function endGameTB(%game){
   if(!$TB::TBEnable[$dtStats::gtNameShort[%game.class]]){
      if($Host::TournamentMode){// load map data so we can update it as it does not load by default with TournamentMode
         loadTBMap(%game);
      }
      if($TB::TBLog[$dtStats::gtNameShort[%game.class]]){
         logTB(%game);// log the outcome
      }
      for(%x = 0; %x < statsGroup.getCount(); %x++){
         %dtStats = statsGroup.getObject(%x);
         calcTBScores(%dtStats,%game);
      }
      saveTBMap(%game);// save map stats
   }
}

function calcTBScores(%dtStats,%game){
   if(%game.class $= "CTFGame" || %game.class $= "LCTFGame" || %game.class $= "SCtFGame" || %game.class $= "ArenaGame"){
      if($dtStats::debugEchos){error("calcTBScores"  SPC %dtStats SPC %game);}

      if(%dtStats.tScore $= ""){
         %dtStats.tScore = 0;
      }
      if(%dtStats.mTScore $= ""){
         %dtStats.mTScore = 0;
      }
      if(!getFieldCount(%dtStats.gScore)){
         %dtStats.gScore = "0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0";
      }
      if(!getFieldCount(%dtStats.mGScore)){
         %dtStats.mGScore = "0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0\t0";
      }

      if(isObject(%dtStats) && ((getSimTime() - $missionStartTime)  >  ((15 * 60) * 1000))){// make sre we dident short cycle
         %tgame = $dtStats::gtNameShort[%game.class];
         %tScore = 0;
         for(%i = 0; %i < $TB::statGroupCount[%tgame]; %i++){
            %gScore = 0;
            if($TB::statCount[%tgame, %i] !$= ""){
               for(%x = 0; %x < $TB::statCount[%tgame, %i]; %x++){
                  %var = $TB::statName[%x,%tgame, %i];
                  %w = $TB::statWeight[%x,%tgame, %i];
                  %gScore += %dtstats.stat[%var] * (%w/100);
                  %tScore += %dtstats.stat[%var] * (%w/100);
               }
            }

            if(%tScore > 0){
               %dtStats.tScore =  expoMovAvg(%dtStats.tScore, %tScore);
               %dtStats.mTScore =  expoMovAvg(%dtStats.mTScore, %tScore);
            }

            if(%gScore > 0){

               %value  = getField(%dtStats.gScore,%i);
               %newValue = expoMovAvg(%value , %gScore);
               %dtStats.gScore = setField(%dtStats.gScore, %i, %newValue);


               %value  = getField(%dtStats.mGScore,%i);
               %newValue = expoMovAvg(%value , %gScore);
               %dtStats.mGScore = setField(%dtStats.mGScore, %i, %newValue);
            }

         }
      }
   }
}


function logTB(%game) {
   %log = new fileObject();
   RootGroup.add(%log);
   %log.openForWrite("serverStats/TB/Logs/" @ $dtStats::gameID @ ".txt");
   %numRoles = statCol.getCount();
   %log.writeLine($dtStats::LastMissionCM);
   for (%t = 1; %t <= %game.numTeams; %t++) { // teams
      %log.writeLine("Game Score" SPC $TeamScore[%t]);
      %output = "Team" @ %t  SPC "Rating" SPC  $dtTeamScore[%t]  @ %t;

      // Print role headers
      for (%r = 0; %r < %numRoles; %r++) {
         %output = %output @ "\tRole" @ %r;
      }
      echo(%output);
      %log.writeLine(%output);
      %maxClients = 0;

      // Find the max number of clients in any role to determine rows needed
      for (%r = 0; %r < %numRoles; %r++) {
         if ($TT::LOGC[%t, %r] > %maxClients) {
            %maxClients = $TT::LOGC[%t, %r];
         }
      }
      // Print users under each role in rows
      for (%c = 0; %c < %maxClients; %c++) {
         %row = " "; // First column is blank for alignment
         for (%r = 0; %r < %numRoles; %r++) {
            if (%c < $TT::LOGC[%t, %r]) {
               %dt = $TT::LOG[%t, %r, %c];
               %row = %row @ "\t" @ %dt.name @ "-" @ getField(%dt.mGScore,%r);
            } else {
               %row = %row @ "\t"; // Empty space for alignment
            }
         }
         echo(%row);
         %log.writeLine(%row);
      }

      echo("\n"); // Extra line break for clarity
   }

   %log.writeLine("");
   %log.writeLine("");
   %log.writeLine("Teams Summary:");
   for (%i = 0; %i < %game.numTeams; %i++) {
      %log.writeLine("Team " @ %i + 1 @ " - " @  $dtTeamScore[%i + 1]);
      // Print role headers
      %output = "\t";
      for (%r = 0; %r < %numRoles; %r++) {
         %output = %output @ "\tRole" @ %r;
      }
      %log.writeLine(%output);
      for (%j = 0; %j < $dtTeamCount[%i+1]; %j++) {
         %dtStats = $dtTeamList[%i+1,%j];
         %msg = "\t" SPC %dtStats.name;
         for (%v  = 0; %v < %numRoles; %v++) {
            %msg = %msg SPC "\t" SPC getField(%dtStats.mGScore,%v);
            %teamStats[%i+1,%v] += getField(%dtStats.mGScore,%v);
         }
         %log.writeLine(%msg);
      }
      %teamMsg ="\t";
      for (%v  = 0; %v < $weightsTestCount; %v++) {
         %teamMsg = %teamMsg  SPC "\t" SPC %teamStats[%i+1,%v];
      }
      %log.writeLine(%teamMsg);
   }
   %log.writeLine("Team Score Dif" SPC mabs($dtTeamScore[1] - $dtTeamScore[2]));

   %log.close();
   %log.delete();
}

function listTBInfo(%val){
   for(%x = 0; %x < statsGroup.getCount(); %x++){
      %dtStats = statsGroup.getObject(%x);
      if(%val){
         calcTBScores(%dtStats,Game);
      }
      echo(%dtStats.sel SPC "Score" SPC %dtStats.name SPC  %dtStats.tScore SPC %dtStats.gScore);
   }
   if(%val == 2){
      ballenceTeams(Game,0);
   }
}

function listStat(){
   for(%x = 0; %x < statsGroup.getCount(); %x++){
      %dtStats = statsGroup.getObject(%x);
      echo(%dtStats.name SPC %dtStats.stat["flagGrabs"] SPC %dtStats.client.flagGrabs);
   }
}

function forceTeamBal(){
   for(%x = 0; %x < statsGroup.getCount(); %x++){
      %dtStats = statsGroup.getObject(%x);
      calcTBScores(%dtStats,Game);
   }
   ballenceTeams(Game,1);
}

function ballenceTeams(%game,%forceTeam){
   if(%game.class $= "CTFGame" || %game.class $= "LCTFGame" || %game.class $= "SCtFGame" || %game.class $= "ArenaGame"){
      if($dtStats::debugEchos){error("ballenceTeams"  SPC %game.class SPC %forceTeam);}
      if(statsGroup.getCount() < 1){
        return;
      }
      loadTBMap(%game);// load up exisitng map stats
      %tgame = $dtStats::gtNameShort[%game.class];
      if(isObject(statCol)){
         statCol.delete();
      }
      new simGroup(statCol);
      statCol.tzc = 0;
      statCol.rbc = 0;
      RootGroup.add(statCol);

      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.team){
            statCol.teamZero[statCol.tzc] = %client;
            statCol.tzc++;
         }
      }


      for (%y  = 0; %y < $TB::statGroupCount[%tgame]; %y++) {
         %set = new simSet();
         %set.rbc = 0;
         statCol.add(%set);
         for(%x = 0; %x < statsGroup.getCount(); %x++){
            %dtStats = statsGroup.getObject(%x);
            %dtStats.sel = 0;
            if(isObject(%dtStats.client)){
               %map = getField(%dtStats.mGScore,%y);
               %lg = getField(%dtStats.gScore,%y);
               %rateing = (%map > (%lg*0.75)) ? %map :%lg;//0.75 makes it so it favors map stats unless theres a massive disparity
               if(%dtStats.client.team != 0 && (%rateing > 0 || %y == 0)){
                  %set.add(%dtStats);
               }
            }
         }

         %len = %set.getCount();
         // sort are groups by there group scores
         for (%i = 0; %i < %len - 1; %i++) {
            for (%j = 0; %j < %len - %i - 1; %j++) {
               %aObj = %set.getObject(%j);
               %bObj = %set.getObject(%j + 1);

               %aMap = getField(%aObj.mGScore,%y); %aLG = getField(%aObj.gScore,%y);
               %A = (%aMap > (%aLG * 0.75)) ? %aMap : %aLG;//derate the last game score  so it favors the map score unless it not avalable or lacking

               %bMap = getField(%bObj.mGScore,%y); %bLG = getField(%bObj.gScore,%y);
               %B = (%bMap > (%bLG * 0.75)) ? %bMap : %bLG;
               if (%A < %B) {
                  %set.bringToFront(%bObj);
               }
            }
         }
      }

      deleteVariables("$TT::*");
      for (%i = 0; %i < %game.numTeams; %i++) {
         $dtTeamScore[%i+1] = 0;
         $dtTeamCount[%i+1] = 0;
         for(%x = 0; %x < statsGroup.getCount(); %x++){
            $TT::LOGC[%i+1,%x] = 0;
         }
      }

      %i = 0;
      %rtfc = 0;
      %x = (statCol.getCount() > 1) ? 1 : 0; // Start at 1 if more than one role
      %rt = 0;
      %end = 0;
      %lockedGroups = 0;

      while (!%end) {
         %x = (%rt++ % %game.numTeams == 0) ? %x + 1 : %x; // Cycle every `numTeams` loops
         if (%x >= statCol.getCount()) {
            %x = 1;
            %rt = 1;
            if (%lockedGroups >= statCol.getCount() - 1) { // All groups are empty
               %x = 0;
            }
         }


         %role = statCol.getObject(%x);
         if (!%role || %role.rbc) { // If group is locked, continue
            if (%x == 0) {
               %end = 1;
            }
            continue;
         }

         %found = 0;
         for (%w = 0; %w < %role.getCount(); %w++) {
            %dtStats = %role.getObject(%w);
            if (!%dtStats.sel) {
               %dtStats.sel = 1;
               %found = 1;
               break;
            }
         }

         if (!%found) {
            %role.rbc = 1; // Lock this group
            %lockedGroups++;
            continue;
         }

         if (isObject(%dtStats.client)) {
            %y = (%i % %game.numTeams) + 1;
            %i++;
            $dtTeamList[%y, $dtTeamCount[%y]] = %dtStats;
            $dtTeamCount[%y]++;
            $dtTeamScore[%y] += %dtStats.tScore;
            $TT::LOG[%y, %x, $TT::LOGC[%y, %x]] = %dtStats;
            $TT::LOGC[%y, %x]++;

            if(%forceTeam){
               Game.clientChangeTeam( %dtStats.client, %y, 0);
            }
            else{
               %dtStats.client.lastTeam = %y;
            }
         }
      }


      // team zero
      for(%i = 0; %i < statCol.tzc; %i++){
          %client = statCol.teamZero[%i];
          %client.lastTeam = 0;
      }
   }
}

function expoMovAvg(%ema, %value){
   %alpha = 0.2;
   return %alpha * %value + (1 - %alpha) * %ema;
}

function saveTBVars(){
   if($dtStats::debugEchos){error("saveTBVars");}
 if(!isEventPending($tbSystem))
   $tbSystem = schedule(10000, 0, "export", "$TB::*", "serverStats/tbVars.cs", false);
}

function saveTBMap(%game){
   if($dtStats::debugEchos){error("saveTBMap"  SPC %game);}
   if($TB::TBEnable[$dtStats::gtNameShort[%game.class]]){//($HostGamePlayerCount - $HostGameBotCount) >=  $dtStats::TBMinPlayers)
      %fobj = new fileObject();
      RootGroup.add(%fobj);
      %path = "serverStats/TB/map/" @ $dtStats::LastMissionCM @ "-" @ %game.class @ ".cs"; //note $dtStats::LastMissionCM is set in gameover
      %fobj.openForWrite(%path);
      %fobj.writeLine("res");
      %fobj.writeLine("res");
      %fobj.writeLine("res");
      %fobj.writeLine("res");
      %fobj.writeLine("res");
      %fobj.writeLine("res");
      %fobj.writeLine("res");
      %fobj.writeLine("res");

      for(%x = 0; %x < statsGroup.getCount(); %x++){
         %dtStats = statsGroup.getObject(%x);
         if(getFieldCount($tempMap::data[%dtStats.guid]) < 5){ // write new entires
            %newData = %dtStats.guid TAB %dtStats.name TAB dtMarkDate() TAB %dtStats.mTScore TAB %dtStats.mGScore;
            %line = strreplace(%newData ,"\t","%t");
            %fobj.writeLine(%line);
         }
      }
      for(%i = 0; %i < $tempMap::count; %i++){
         %guid =  $tempMap::guid[%i];
         %dtStats = $dtStats::tbLookUP[%guid];
         if(isObject(%dtStats)){
            %newData = %guid TAB %dtStats.name TAB dtMarkDate() TAB %dtStats.mTScore TAB %dtStats.mGScore;
            %line = strreplace(%newData ,"\t","%t");
            %fobj.writeLine(%line);
         }
         else{
            %date = getField($tempMap::data[%guid],2);
            if(getTimeDelta(%date) <  (1440*180)){// skip over old old stuff
               %line = strreplace($tempMap::data[%guid],"\t","%t");
               %fobj.writeLine(%line);
            }
         }
      }
      %fobj.close();
      %fobj.delete();
   }
}

function loadTBMap(%game){
   if($dtStats::debugEchos){error("loadTBMap"  SPC %game);}
   if($TB::TBEnable[$dtStats::gtNameShort[%game.class]]){
      deleteVariables("$tempMap::*");
      %path = "serverStats/TB/map/" @ $CurrentMission @ "-" @ %game.class @ ".cs";
      if(isFile(%path)){
         %fobj = new fileObject();
         RootGroup.add(%fobj);
         %fobj.openForRead(%path);
         %unused = %fobj.readline(); //reserved
         %unused = %fobj.readline();
         %unused = %fobj.readline();
         %unused = %fobj.readline();
         %unused = %fobj.readline();
         %unused = %fobj.readline();
         %unused = %fobj.readline();
         %unused = %fobj.readline();
         $tempMap::count = 0;
         while( !%fobj.isEOF() ){
            %line = strreplace(%fobj.readline(),"%t","\t");
            %guid = getField(%line, 0);
            %name = getField(%line, 1);
            %date = getField(%line, 2);
            %tScore = getField(%line, 3);
            %gScore = getFields(%line, 4 ,4+16);
            $tempMap::guid[$tempMap::count] = %guid;
            $tempMap::count++;
            $tempMap::data[%guid] = %line;
            %dtStats = $dtStats::tbLookUP[%guid];
            if(isObject(%dtStats)){
               %dtStats.mGScore = %gScore;
               %dtStats.mTScore = %tScore;
            }
         }
         %fobj.close();
         %fobj.delete();
      }
   }
}

function dtSaveDone(){
   $dtStats::statsSave = 0;
   $dtStats::leftID++;
   $dtStats::teamOneCapTimes = 0;
   $dtStats::teamTwoCapTimes = 0;
   $dtStats::teamOneCapCount = 0;
   $dtStats::teamTwoCapCount = 0;
}

////////////////////////////////////////////////////////////////////////////////
//							Supporting Functions							  //
////////////////////////////////////////////////////////////////////////////////
function DefaultGame::postGameStats(%game,%dtStats){ //stats to add up at the end of the match
   if($dtStats::debugEchos){error("postGameStats GUID = "  SPC %dtStats.guid);}
   if(!isObject(%dtStats))
      return;
   %dtStats.stat["tournamentMode"]  = $Host::TournamentMode;

   %dtStats.stat["null"] = getRandom(1,100);

   %dtStats.stat["kdr"] = %dtStats.stat["deaths"] ? (%dtStats.stat["kills"]/%dtStats.stat["deaths"]) : %dtStats.stat["kills"];

   %dtStats.stat["lastKill"] = (statsGroup.stat["lastKill"] == %dtStats);

   %dtStats.stat["totalTime"] = (%dtStats.clientLeft == 1) ?  ((%dtStats.leftTime - %dtStats.joinTime)/1000)/60 : ((getSimTime() - %dtStats.joinTime)/1000)/60;

   %dtStats.stat["matchRunTime"] =((getSimTime() - $missionStartTime)/1000)/60;

   %dtStats.stat["startPCT"] = %dtStats.joinPCT;
   %dtStats.stat["endPCT"] =  (%dtStats.clientLeft == 1) ? %dtStats.leftPCT : %game.getGamePct();
   %dtStats.gamePCT = mFloor(%dtStats.stat["endPCT"] -  %dtStats.stat["startPCT"]);
   %dtStats.stat["mapSkip"] = (%game.getGamePct() < 99);
   //error(%dtStats.stat["endPCT"] SPC %dtStats.stat["startPCT"] SPC %dtStats.gamePCT SPC %dtStats.stat["mapSkip"]);

   %dtStats.stat["totalMA"] = %dtStats.stat["discMA"] +
                     %dtStats.stat["grenadeMA"] +
                     %dtStats.stat["laserMA"] +
                     %dtStats.stat["mortarMA"] +
                     %dtStats.stat["shockMA"] +
                     %dtStats.stat["plasmaMA"] +
                     %dtStats.stat["blasterMA"] +
                     %dtStats.stat["hGrenadeMA"] +
                     %dtStats.stat["mineMA"];


   %dtStats.stat["EVKills"] =   %dtStats.stat["explosionKills"] +
                        %dtStats.stat["groundKills"] +
                        %dtStats.stat["outOfBoundKills"] +
                        %dtStats.stat["lavaKills"] +
                        %dtStats.stat["lightningKills"] +
                        %dtStats.stat["vehicleSpawnKills"] +
                        %dtStats.stat["forceFieldPowerUpKills"] +
                        %dtStats.stat["nexusCampingKills"];

   %dtStats.stat["totalWepDmg"] = %dtStats.stat["cgDmg"] +
                          %dtStats.stat["laserDmg"] +
                          %dtStats.stat["blasterDmg"] +
                          %dtStats.stat["discDmg"] +
                          %dtStats.stat["grenadeDmg"] +
                          %dtStats.stat["hGrenadeDmg"] +
                          %dtStats.stat["mortarDmg"] +
                          %dtStats.stat["missileDmg"] +
                          %dtStats.stat["plasmaDmg"] +
                          %dtStats.stat["shockDmg"] +
                          %dtStats.stat["mineDmg"] +
                          %dtStats.stat["satchelDmg"];


   if(%dtStats.stat["cgShotsFired"] < 100)
      %dtStats.stat["cgACC"] = 0;

   if(%dtStats.stat["discShotsFired"] < 15){
      %dtStats.stat["discACC"] = 0;
      %dtStats.stat["discDmgACC"] = 0;
   }

   if(%dtStats.stat["grenadeShotsFired"] < 10){
      %dtStats.stat["grenadeACC"] = 0;
      %dtStats.stat["grenadeDmgACC"] = 0;
   }

   if(%dtStats.stat["laserShotsFired"] < 10)
      %dtStats.stat["laserACC"] = 0;

   if(%dtStats.stat["mortarShotsFired"] < 10){
      %dtStats.stat["mortarACC"] = 0;
      %dtStats.stat["mortarDmgACC"] = 0;
   }

   if(%dtStats.stat["shockShotsFired"] < 10)
      %dtStats.stat["shockACC"] = 0;

   if(%dtStats.stat["plasmaShotsFired"] < 20){
      %dtStats.stat["plasmaACC"] = 0;
      %dtStats.stat["plasmaDmgACC"] = 0;
   }

   if(%dtStats.stat["blasterShotsFired"] < 15)
      %dtStats.stat["blasterACC"] = 0;

   if(%dtStats.stat["missileShotsFired"] < 8)
      %dtStats.stat["missileACC"] = 0;

   if(%dtStats.stat["hGrenadeShotsFired"] < 6)
      %dtStats.stat["hGrenadeACC"] = 0;

   if(%dtStats.stat["mineShotsFired"] < 6)
      %dtStats.stat["mineACC"] = 0;

   if(%dtStats.stat["satchelShotsFired"] < 5)
      %dtStats.stat["satchelACC"] = 0;



   if(%game.class $= "CTFGame" || %game.class $= "LCTFGame" || %game.class $= "SCtFGame"){
      %dtStats.stat["teamOneCapTimes"]  = $dtStats::teamOneCapTimes;
      %dtStats.stat["teamTwoCapTimes"]  = $dtStats::teamTwoCapTimes;
      %dtStats.stat["teamScore"] =  $TeamScore[%dtStats.stat["dtTeam"]];

      %dtStats.stat["destruction"] =  %dtStats.stat["genDestroys"] +
                              %dtStats.stat["solarDestroys"] +
                              %dtStats.stat["sensorDestroys"] +
                              %dtStats.stat["turretDestroys"] +
                              %dtStats.stat["iStationDestroys"] +
                              %dtStats.stat["vstationDestroys"] +
                              %dtStats.stat["sentryDestroys"] +
                              %dtStats.stat["depSensorDestroys"] +
                              %dtStats.stat["depTurretDestroys"] +
                              %dtStats.stat["depStationDestroys"] +
                              %dtStats.stat["mpbtstationDestroys"];

      %dtStats.stat["repairs"] =   %dtStats.stat["genRepairs"] +
                           %dtStats.stat["SensorRepairs"] +
                           %dtStats.stat["TurretRepairs"] +
                           %dtStats.stat["StationRepairs"] +
                           %dtStats.stat["VStationRepairs"] +
                           %dtStats.stat["mpbtstationRepairs"] +
                           %dtStats.stat["solarRepairs"] +
                           %dtStats.stat["sentryRepairs"] +
                           %dtStats.stat["depSensorRepairs"] +
                           %dtStats.stat["depInvRepairs"] +
                           %dtStats.stat["depTurretRepairs"];

      %dtStats.stat["capEfficiency"] = (%dtStats.stat["flagGrabs"] > 0)  ? (%dtStats.stat["flagCaps"] / %dtStats.stat["flagGrabs"]) : 0;


      if(statsGroup.team[1] == statsGroup.team[2]){
         %dtStats.stat["winCount"] = 0;
         %dtStats.stat["lossCount"] = 0;
      }
      else if(statsGroup.team[1] > statsGroup.team[2] && %dtStats.stat["dtTeam"] == 1)
         %dtStats.stat["winCount"] = 1;
      else if(statsGroup.team[2] > statsGroup.team[1]  && %dtStats.stat["dtTeam"] == 2)
         %dtStats.stat["winCount"] = 1;
      else if(%dtStats.stat["dtTeam"] > 0)
         %dtStats.stat["lossCount"] = 1;

      %winCount = getField(%dtStats.gameStats["winCountTG","t",%game.class],5) + %dtStats.stat["winCount"];
      %lostCount = getField(%dtStats.gameStats["lossCountTG","t",%game.class],5) + %dtStats.stat["lossCount"];
      %lostCount = %lostCount ? %lostCount : 1;
      %winCount = %winCount ? %winCount : 0;
      %dtStats.stat["winLostPct"] = (%winCount / %lostCount);
   }
   else if(%game.class $= "LakRabbitGame"){
      %dtStats.stat["flagTimeMin"] = (%dtStats.flagTimeMS / 1000)/60;
   }
   else if(%game.class $= "ArenaGame"){
      %dtStats.stat["WLR"] = (%dtStats.stat["roundsLost"] > 0) ? %dtStats.stat["roundsWon"] / %dtStats.stat["roundsLost"] : %dtStats.stat["roundsWon"];
      if(%dtStats.stat["discShotsFired"]){
         %dtStats.stat["discMARatio"] = %dtStats.stat["discMA"] / %dtStats.stat["discShotsFired"];
      }
      if(%dtStats.stat["plasmaShotsFired"]){
         %dtStats.stat["plasmaMARatio"] = %dtStats.stat["plasmaMA"] / %dtStats.stat["plasmaShotsFired"];
      }
      if(%dtStats.stat["laserShotsFired"]){
         %dtStats.stat["laserMARatio"] = %dtStats.stat["laserMA"] / %dtStats.stat["laserShotsFired"];
      }
      if(%dtStats.stat["grenadeShotsFired"]){
         %dtStats.stat["grenadeMARatio"] = %dtStats.stat["grenadeMA"] / %dtStats.stat["grenadeShotsFired"];
      }
      if(%dtStats.stat["shockShotsFired"]){
         %dtStats.stat["shockMARatio"] = %dtStats.stat["shockMA"] / %dtStats.stat["shockShotsFired"];
      }
      if(%dtStats.stat["blasterShotsFired"]){
         %dtStats.stat["blasterMARatio"] = %dtStats.stat["blasterMA"] / %dtStats.stat["blasterShotsFired"];
      }
   }
}

function isGameRun(){//
   return  (($MatchStarted + $missionRunning) == 2);
}

function DefaultGame::getGamePct(%game){
   %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
   %timePct = (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   %timePct = (%timePct > 100) ?  100 : %timePct;
   return %timePct;
}

function CTFGame::getGamePct(%game){
      %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
      %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
      %timePct = (%timePct > 100) ?  100 : %timePct;

      %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
      if(%scoreLimit $= "")
         %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;

      if($TeamScore[1] > $TeamScore[2])
         %pct = ($TeamScore[1] / %scoreLimit) * 100;
      else
         %pct =  ($TeamScore[2] / %scoreLimit) * 100;

      %scorePct =  (%pct > 100) ? 100 : %pct;
      if(%scorePct > %timePct)
         return %scorePct;
      else
         return %timePct;
}

function LakRabbitGame::getGamePct(%game){
   %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   %timePct = (%timePct > 100) ?  100 : %timePct;
   %scoreLimit = MissionGroup.Rabbit_scoreLimit;
   if(%scoreLimit $= "")
      %scoreLimit = 2000;
   %lScore = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if(%lScore < %client.score){
         %lScore = %client.score;
      }
   }
   %pct =  (%lScore / %scoreLimit) * 100;
   %scorePct =  (%pct > 100) ? 100 : %pct;
   if(%scorePct > %timePct)
      return %scorePct;
   else
      return %timePct;
}

function DMGame::getGamePct(%game){
   %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   %timePct = (%timePct > 100) ?  100 : %timePct;
   %scoreLimit =  MissionGroup.DM_scoreLimit;
   if(%scoreLimit $= "")
      %scoreLimit = 25;

   for (%i = 0; %i < ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if(%lScore < %client.score){
         %lScore = %client.score;
      }
   }
   %pct =  (%lScore / %scoreLimit) * 100;
   %scorePct =  (%pct > 100) ? 100 : %pct;

   if(%scorePct > %timePct)
      return %scorePct;
   else
      return %timePct;
}

function LCTFGame::getGamePct(%game){
   %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   %timePct = (%timePct > 100) ?  100 : %timePct;
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;

   if($TeamScore[1] > $TeamScore[2])
      %pct = ($TeamScore[1] / %scoreLimit) * 100;
   else
      %pct =  ($TeamScore[2] / %scoreLimit) * 100;

   %scorePct =  (%pct > 100) ? 100 : %pct;
   if(%scorePct > %timePct)
      return %scorePct;
   else
      return %timePct;
}

function SCtFGame::getGamePct(%game){
   %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   %timePct = (%timePct > 100) ?  100 : %timePct;
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;

   if($TeamScore[1] > $TeamScore[2])
      %pct = ($TeamScore[1] / %scoreLimit) * 100;
   else
      %pct =  ($TeamScore[2] / %scoreLimit) * 100;

   %scorePct =  (%pct > 100) ? 100 : %pct;
   if(%scorePct > %timePct)
      return %scorePct;
   else
      return %timePct;
}

function ArenaGame::getGamePct(%game){
   %curTimeLeftMS =  ((getSimTime() - $missionStartTime)/1000)/60;
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   %timePct = (%timePct > 100) ?  100 : %timePct;
   %scorePct = 0;
   for ( %i = 1; %i <= %game.numTeams; %i++ ){
      %score = ($TeamScore[%i] / %game.roundLimit) * 100;
      %scorePct = (%score > %scorePct) ? %score : %scorePct;
   }
   if(%scorePct > %timePct)
      return %scorePct;
   else
      return %timePct;
}

function msToMinSec(%time)
{
   %sec = mFloor(%time / 1000);
   %min = mFloor(%sec / 60);
   %sec -= %min * 60;

   // pad it
   if(%min < 10)
      %min = "0" @ %min;
   if(%sec < 10)
      %sec = "0" @ %sec;

   return(%min @ ":" @ %sec);
}

function secToMinSec(%sec){
   %min = mFloor(%sec / 60);
   %sec -= %min * 60;

   // pad it
   if(%min < 10)
      %min = "0" @ %min;
   if(%sec < 10)
      %sec = "0" @ %sec;

   return(%min @ ":" @ %sec);
}

function dtFormatTime(%ms)
{
   %sec = mFloor(%ms / 1000);
   %min = mFloor(%sec / 60);
   %hour = mFloor(%min / 60);
   %days = mFloor(%hour / 24);
   %sec -= %min * 60;
   %min -= %hour * 60;
   %hour -= %days * 24;
   // pad it
   if(%day < 10)
      %day = "0" @ %day;
   if(%hour < 10)
      %hour = "0" @ %hour;
   if(%min < 10)
      %min = "0" @ %min;
   if(%sec < 10)
      %sec = "0" @ %sec;

   return(%days @ ":" @ %hour @ ":" @ %min @ ":" @ %sec);
}

function getCNameToCID(%name){
   if(%name !$= ""){
      if(isObject(%name)){
         if(%name.getClassName() $= "GameConnection" || %name.getClassName() $= "AIConnection")
            return %name; // not a name its a client so return it
      }
      else{
         %name = stripChars(%name, "\cp\co\c6\c7\c8\c9\c0" );
         for (%i = 0; %i < ClientGroup.getCount(); %i++){
            %client = ClientGroup.getObject(%i);
            if(stripChars( getTaggedString( %client.name ), "\cp\co\c6\c7\c8\c9\c0" ) $=  %name){
               return %client;
            }
         }
      }
   }
   return 0;
}

function cleanName(%nm){
   %validChars = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
   %inValid = " !\"#$%&'()*+,-./:;<=>?@[\\]^_'{|}~\t\n\r";
   for(%a=0; %a < strlen(%nm); %a++){
      %c = getSubStr(%nm,%a,1);
      %vc = strpos(%validChars,%c);
      %iv = strpos(%inValid,%c);
      if(%vc !$= -1){
         %name = %name @ %c;
      }
      else if(%iv !$= -1){ // replace invlaid with number
         %name = %name @ %iv;
      }
   }
   return %name;
}

function cleanMapName(%nm){
 return stripChars(%nm," !_\"#$%&'()*+,-./:;<=>?@[\\]^'{|}~\t\n\r0123456789");
}

function setGUIDName(%client){
   if(isFile("serverStats/genGUIDList.cs") && $genGUIDList != 1){
      exec("serverStats/genGUIDList.cs");
     $genGUIDList = 1;
   }
   if(%client.guid){
      return 0;
   }
   else{
      %name  = cleanName(getTaggedString(%client.name));
      if($guidGEN::ID[%name]){
         %client.guid = $guidGEN::ID[%name];
      }
      else{
         $guidGEN::ID[%name] = $guidGEN::Count--;
         export( "$guidGEN::*", "serverStats/genGUIDList.cs", false );
          %client.guid = $guidGEN::ID[%name];
      }
      return 1;
   }
}

function getMapID(%map,%game,%clean){
   if(%clean)
      %map = cleanMapName(%map);
   if(%game !$= "" && %map !$= ""){
      if($mapID::ID[%map,%game])
         return $mapID::ID[%map,%game];
      else{
         $mapID::count++;
         $mapID::countGame[%game]++;

         $mapID::ID[%map,%game] = $mapID::count;
         $mapID::IDGame[%map,%game] = $mapID::countGame[%game];

         $mapID::IDNameGame[$mapID::countGame[%game],%game] = %map;
         $mapID::IDName[$mapID::count] = %map;

         export( "$mapID::*", "serverStats/mapIDList.cs", false);
         return $mapID::ID[%map,%game];
      }
   }
   else
      error("getMapID no %map or %game in function call");
}

function loadMapIdList(){
   if(isFile("serverStats/mapIDList.cs") && $genMapId != 1){
      $genMapId = 1;
      exec("serverStats/mapIDList.cs");
   }
}

function getDayNum(){
   %date = formattimestring("mm dd yy");
  %m = getWord(%date,0);%d = getWord(%date,1);%y = getWord(%date,2);
   %count = 0;
  if(%y % 4 < 1){%days[2] = "29";}else{%days[2] = "28";} // leap year
  %days[1] = "31";%days[3] = "31";
  %days[4] = "30"; %days[5] = "31"; %days[6] = "30";
  %days[7] = "31"; %days[8] = "31"; %days[9] = "30";
  %days[10] = "31"; %days[11] = "30"; %days[12] = "31";
  for(%i = 1; %i <= %m-1; %i++){
     %count += %days[%i];
  }
  return %count + %d;
}

function getDayNumDMY(%d, %m, %y){
   %count = 0;
  if(%y % 4 < 1){%days[2] = "29";}else{%days[2] = "28";} // leap year
  %days[1] = "31";%days[3] = "31";
  %days[4] = "30"; %days[5] = "31"; %days[6] = "30";
  %days[7] = "31"; %days[8] = "31"; %days[9] = "30";
  %days[10] = "31"; %days[11] = "30"; %days[12] = "31";
  for(%i = 1; %i <= %m-1; %i++){
     %count += %days[%i];
  }
  return %count + %d;
}

function getWeekNum(){
    return mCeil(getDayNum() / 7);
}

function getMonthNum(){
    return formattimestring("mm") + 0;
}

function getQuarterNum(){
    return mCeil((formattimestring("mm"))/3);
}

function getYear(){
    return formattimestring("yy") +0;
}

function monthString(%num){
 %i[1] = "January";  %i[2] = "February";  %i[3] = "March";
 %i[4] = "April";    %i[5] = "May";       %i[6] = "June";
 %i[7] = "July";     %i[8] = "August";    %i[9] = "September";
 %i[10] = "October"; %i[11] = "November"; %i[12] = "December";
   return %i[%num];
}

////////////////////////////////////////////////////////////////////////////////
//							Load Save Management							  //
////////////////////////////////////////////////////////////////////////////////

function loadGameStats(%dtStats,%game){// called when client joins server.cs onConnect
   if($dtStats::debugEchos){error("loadGameStats GUID = "  SPC %dtStats.guid);}
   if(%dtStats.guid !$= ""){
      loadGameTotalStats(%dtStats,%game);
      if($dtStats::tmMode){
         %filename = "serverStats/statsTM/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
      }
      else{
         %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
      }

      if(isFile(%filename)){
         %file = new FileObject();
         RootGroup.add(%file);
         %file.OpenForRead(%filename);
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var = getField(%line,0);
            %dtStats.gameStats[%var,"g",%game,$dtStats::tmMode] =  getFields(%line,1,getFieldCount(%line)-1);
         }
         %dtStats.gameData[%game,$dtStats::tmMode]= 1;
         %file.close();
         %file.delete();
      }
      else
       %dtStats.gameData[%game,$dtStats::tmMode]= 1;
   }
}

function loadGameTotalStats(%dtStats,%game){
   if($dtStats::debugEchos){error("loadGameTotalStats GUID = "  SPC %dtStats.guid);}
   if($dtStats::tmMode){
      %filename = "serverStats/statsTM/" @ %game @ "/" @ %dtStats.guid  @ "t.cs";
   }
   else{
      %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "t.cs";
   }
   %d = $dtStats::curDay; %w = $dtStats::curWeek; %m = $dtStats::curMonth; %q = $dtStats::curQuarter; %y = $dtStats::curYear; %c = $dtStats::curCustom;
   if(isFile(%filename)){
      %file = new FileObject();
      RootGroup.add(%file);
      %file.OpenForRead(%filename);

      %day  = %week = %month = %quarter = %year = %custom = 0;
      %dateLine = strreplace(%file.readline(),"%t","\t"); // first line should allways be the date line
      if(getField(%dateLine,0) $= "days"){
         if(getField(%dateLine,2) != %d){%day = 1;} // see what has changed sence we last played
         if(getField(%dateLine,4) != %w){%week = 1;}
         if(getField(%dateLine,6) != %m){%month = 1;}
         if(getField(%dateLine,8) != %q){%quarter = 1;}
         if(getField(%dateLine,10) != %y){%year = 1;}
         if(getField(%dateLine,12) != %c){%custom = 1;}

         %d0 = getField(%dateLine,1);%d1 = getField(%dateLine,2);
         %w0 = getField(%dateLine,3);%w1 = getField(%dateLine,4);
         %m0 = getField(%dateLine,5);%m1 = getField(%dateLine,6);
         %q0 = getField(%dateLine,7);%q1 = getField(%dateLine,8);
         %y0 = getField(%dateLine,9);%y1 = getField(%dateLine,10);
         %c0 = getField(%dateLine,11);%c1 = getField(%dateLine,12);

         if(%day){ %d0 = %d1; %d1 = %d;} //if there was a change flip new with old and reset new
         if(%week){%w0 = %w1;%w1 = %w;}
         if(%month){%m0 = %m1;%m1 = %m;}
         if(%quarter){%q0 = %q1;%q1 = %q;}
         if(%year){ %y0 = %y1; %y1 = %y;}
         if(%custom){ %c0 = %c1; %c1 = %c;}
         %dtStats.gameStats["dwmqy","t",%game,$dtStats::tmMode] =  %d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1 TAB %c0 TAB %c1; // update line
      }
      while( !%file.isEOF() ){
         %line = strreplace(%file.readline(),"%t","\t");
         %var = getField(%line,0);
         if(%var !$= "playerName" && %var !$= "versionNum"){
            %d0 = getField(%line,1);%d1 = getField(%line,2);
            %w0 = getField(%line,3);%w1 = getField(%line,4);
            %m0 = getField(%line,5);%m1 = getField(%line,6);
            %q0 = getField(%line,7);%q1 = getField(%line,8);
            %y0 = getField(%line,9);%y1 = getField(%line,10);
            %c0 = getField(%line,11);%c1 = getField(%line,12);

            if(%day){ %d0 = %d1; %d1 = 0;} //if there was a change flip new with old and reset new
            if(%week){%w0 = %w1;%w1 = 0;}
            if(%month){%m0 = %m1;%m1 = 0;}
            if(%quarter){%q0 = %q1;%q1 = 0;}
            if(%year){ %y0 = %y1;%y1 = 0;}
            if(%custom){ %c0 = %c1;%c1 = 0;}
            %dtStats.gameStats[%var,"t",%game,$dtStats::tmMode] = %d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1 TAB %c0 TAB %c1;
         }
      }
      %file.close();
      %file.delete();
   }
   else// must be new person so be sure to set the dates
      %dtStats.gameStats["dwmqy","t",%game, $dtStats::tmMode] =  %d TAB %d TAB %w TAB %w TAB %m TAB %m TAB %q TAB %q TAB %y TAB %y TAB %c TAB %c;
}

function saveGameTotalStats(%dtStats,%game){
   if($dtStats::debugEchos){error("saveGameTotalStats GUID = "  SPC %dtStats.guid);}
      if(%dtStats.guid !$= "" && !%dtStats.isBot){// dont save if we are dont have a guid or is a bot

         if(%dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode] $= ""){%dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode] = 0;}
         %fileTotal = new FileObject();
         RootGroup.add(%fileTotal);
         if($dtStats::tmMode){
            %fileNameTotal = "serverStats/statsTM/"@ %game @ "/" @ %dtStats.guid  @ "t.cs";
         }
         else{
            %fileNameTotal = "serverStats/stats/"@ %game @ "/" @ %dtStats.guid  @ "t.cs";
         }
         %fileTotal.OpenForWrite(%fileNameTotal);
         %fileTotal.writeLine("days" @ "%t" @ strreplace(%dtStats.gameStats["dwmqy","t",%game,$dtStats::tmMode],"\t","%t"));
         %fileTotal.writeLine("gameCount" @ "%t" @ strreplace(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],"\t","%t"));
         %fileTotal.writeLine("playerName" @ "%t" @  %dtStats.name);
         %fileTotal.writeLine("versionNum" @ "%t" @  $dtStats::version);

         %fileGame = new FileObject();
         RootGroup.add(%fileGame);
         if($dtStats::tmMode){
            %fileNameGame = "serverStats/statsTM/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
         }
         else{
            %fileNameGame = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
         }
         %fileGame.OpenForWrite(%fileNameGame);
         %fileGame.writeLine("playerName" @ "%t" @ trim(%dtStats.name));
         %fileGame.writeLine("statsOverWrite" @ "%t" @ %dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode]);
         %fileGame.writeLine("totalGames" @ "%t" @  %dtStats.gameStats["totalGames","g",%game,$dtStats::tmMode]);
         %fileGame.writeLine("fullSet" @ "%t" @  %dtStats.gameStats["fullSet","g",%game,$dtStats::tmMode]);
         %fileGame.writeLine("dayStamp" @ "%t" @ strreplace(%dtStats.gameStats["dayStamp","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("weekStamp" @ "%t" @ strreplace(%dtStats.gameStats["weekStamp","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("monthStamp" @ "%t" @ strreplace(%dtStats.gameStats["monthStamp","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("quarterStamp" @ "%t" @ strreplace(%dtStats.gameStats["quarterStamp","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("yearStamp" @ "%t" @ strreplace(%dtStats.gameStats["yearStamp","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("dateStamp" @ "%t" @ strreplace(%dtStats.gameStats["dateStamp","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("timeDayMonth" @ "%t" @ strreplace(%dtStats.gameStats["timeDayMonth","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("map" @ "%t" @ strreplace(%dtStats.gameStats["map","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("mapID" @ "%t" @ strreplace(%dtStats.gameStats["mapID","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("mapGameID" @ "%t" @ strreplace(%dtStats.gameStats["mapGameID","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("gameID" @ "%t" @ strreplace(%dtStats.gameStats["gameID","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("gamePCT" @ "%t" @ strreplace(%dtStats.gameStats["gamePCT","g",%game,$dtStats::tmMode],"\t","%t"));
         %fileGame.writeLine("versionNum" @ "%t" @ strreplace(%dtStats.gameStats["versionNum","g",%game,$dtStats::tmMode],"\t","%t"));

         for(%q = 0; %q < $statsVars::count[%game]; %q++){
            %varNameType = $statsVars::varNameType[%q,%game];
            %varType =  $statsVars::varType[%varNameType,%game];
            if(%varType !$= "TTL"){
               %val = %dtStats.gameStats[%varNameType,"g",%game,$dtStats::tmMode];
               %fileGame.writeLine(%varNameType @ "%t" @ strreplace(%val,"\t","%t"));
            }
            if(%varType !$= "Game"){
               %val = %dtStats.gameStats[%varNameType,"t",%game,$dtStats::tmMode];
               %fileTotal.writeLine(%varNameType @ "%t" @ strreplace(%val,"\t","%t"));
            }
         }
         %fileTotal.close();
         %fileGame.close();
         %fileTotal.delete();
         %fileGame.delete();
      }
      if(%dtStats.markForDelete){
         if($dtStats::debugEchos){error("Client Left, Deleting Stat Object = "  SPC %dtStats SPC %dtStats.guid);}
         $dtStats::tbLookUP[%dtStats.guid] = "";
         %dtStats.delete();
      }
}

function incGameStats(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("incGameStats GUID = "  SPC %dtStats.guid);}

   %c = %dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode]++;
   if(%dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode]  > $dtStats::MaxNumOfGames-1 || %dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode]  > 99){
      %c = %dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode] = 0;
      %dtStats.gameStats["fullSet","g",%game,$dtStats::tmMode] = 1;
   }

   %dtStats.gameStats["totalGames","g",%game,$dtStats::tmMode]++;

   %c1 = getField(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],1);
   setValueField(%dtStats,"gameCount","t",%game,1,%c1++);
   %c7 = getField(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],3);
   setValueField(%dtStats,"gameCount","t",%game,3,%c7++);
   %c30 = getField(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],5);
   setValueField(%dtStats,"gameCount","t",%game,5,%c30++);
   %c90 = getField(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],7);
   setValueField(%dtStats,"gameCount","t",%game,7,%c90++);
   %c365 = getField(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],9);
   setValueField(%dtStats,"gameCount","t",%game,9,%c365++);
   %cc = getField(%dtStats.gameStats["gameCount","t",%game,$dtStats::tmMode],11);
   setValueField(%dtStats,"gameCount","t",%game,11,%cc++);

   setValueField(%dtStats,"dayStamp","g",%game,%c,$dtStats::curDay);
   setValueField(%dtStats,"weekStamp","g",%game,%c,$dtStats::curWeek);
   setValueField(%dtStats,"monthStamp","g",%game,%c,$dtStats::curMonth);
   setValueField(%dtStats,"quarterStamp","g",%game,%c,$dtStats::curQuarter);
   setValueField(%dtStats,"yearStamp","g",%game,%c,$dtStats::curYear);
   setValueField(%dtStats,"dateStamp","g",%game,%c,formattimestring("yy-mm-dd HH:nn:ss"));
   setValueField(%dtStats,"timeDayMonth","g",%game,%c,formattimestring("hh:nn:a, mm-dd"));
   setValueField(%dtStats,"map","g",%game,%c,$dtStats::LastMissionDN);
   setValueField(%dtStats,"mapID","g",%game,%c,getMapID($dtStats::LastMissionCM,%game,1));
   setValueField(%dtStats,"mapGameID","g",%game,%c,getMapID($dtStats::LastMissionCM,%game,1));
   setValueField(%dtStats,"gameID","g",%game,%c,$dtStats::gameID);
   setValueField(%dtStats,"gamePCT","g",%game,%c,%dtStats.gamePCT);
   setValueField(%dtStats,"versionNum","g",%game,%c,$dtStats::version);

   for(%q = 0; %q < $statsVars::count[%game]; %q++){
      %varNameType = $statsVars::varNameType[%q,%game];
      %varName = $statsVars::varName[%q,%game];
      %varType =  $statsVars::varType[%varNameType,%game];

      switch$(%varType){
         case "Game":
            %val = %dtStats.stat[%varName];
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);

         case "TG":
            %val = %dtStats.stat[%varName];
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);

            for(%x = 1; %x <= 11; %x+=2){
               %t = getField(%dtStats.gameStats[%varNameType,"t",%game,$dtStats::tmMode],%x);
               setValueField(%dtStats,%varNameType,"t",%game,%x,addNum(%t,%val));
            }
         case "TTL":
            %val = %dtStats.stat[%varName];
            for(%x = 1; %x <= 11; %x+=2){
               %t = getField(%dtStats.gameStats[%varNameType,"t",%game,$dtStats::tmMode],%x);
               setValueField(%dtStats,%varNameType,"t",%game,%x,addNum(%t,%val));
            }
         case "Max":
            %val = %dtStats.stat[%varName];
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);
            for(%x = 1; %x <= 11; %x+=2){
               %t =    getField(%dtStats.gameStats[%varNameType,"t",%game,$dtStats::tmMode],%x);
               if(%val > %t){
                  setValueField(%dtStats,%varNameType,"t",%game,%x,%val);
               }
               else{
                  setValueField(%dtStats,%varNameType,"t",%game,%x,%t);
               }
            }
         case "Min":
            %val = %dtStats.stat[%varName];
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);

            for(%x = 1; %x <= 11; %x+=2){
               %t = getField(%dtStats.gameStats[%varNameType,"t",%game,$dtStats::tmMode],%x);
               if(%val < %t && %val != 0 || !%t){
                  setValueField(%dtStats,%varNameType,"t",%game,%x,%val);
               }
               else{
                  setValueField(%dtStats,%varNameType,"t",%game,%x,%t);
               }
            }
         case "Avg" or "AvgI":
            %val = %dtStats.stat[%varName];
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);

            for(%x = 1; %x <= 11; %x+=2){
               %t = strreplace(getField(%dtStats.gameStats[%varNameType,"t",%game,$dtStats::tmMode],%x),"%a","\t");
               if(%val != 0){
                  %total = getField(%t,1) + %val;
                  if(%total<950000){
                     %gameCount = getField(%t,2) + 1;
                     %avg = %total/%gameCount;
                  }
                  else{
                     %total =  mFloor(%total * 0.9);
                     %gameCount = mFloor((getField(%t,2) + 1) * 0.9);
                     %avg = %total/%gameCount;
                  }
               }
               else{
                  %avg = getField(%t,0);
                  %total = getField(%t,1);
                  %gameCount = getField(%t,2);
               }
               setValueField(%dtStats,%varNameType,"t",%game,%x, hasValue(%avg) @ "%a" @ hasValue(%total) @ "%a" @ hasValue(%gameCount));
         }
      }
   }
   resetDtStats(%dtStats,%game,0); // reset to 0 for next game
}

function cropDec(%num){
   %length = strlen(%num);
   %dot = strPos(%num,".");
   if(%dot == -1)
      return %num @ "x";
   else
      return getSubStr(%num,0,%dot) @ "x";
}

function cropFloat(%num,%x){
   %length = strlen(%num);
   %dot = strPos(%num,".");
   if(%dot != -1){
       %int =getSubStr(%num,0,%dot);
      %decLen = %length - strLen(%int)-1;
      %x  = %decLen >= %x ? %x : %decLen;
      //error(%x);
      %dec = getSubStr(%num,%dot,%dot+%x);
      return %int @ %dec;
   }
   else
      return %num;
}

function roundDec(%num, %places) {
   %factor = mPow(10, %places);
   return mCeil(%num * %factor - 0.5) / %factor;
}

function addNum(%a,%b){
   if(strPos(%a,"x") == -1 && strPos(%b,"x") == -1){
      %ab = %a + %b;
      if(%ab < 999999){
         return %ab;
      }
   }

   if(strPos(%a,"x") == -1)
      %a = cropDec(%a);
   if(strPos(%b,"x") == -1)
      %b = cropDec(%b);

   if(strPos(%b,"-") == 0){
      %bn = strreplace(%b,"-","");
      if(xlCompare(%a,%bn) $= "<"){
         return 0;
      }
      else{
         %r = addSubX(%a,%bn);
         if(strPos(%r,"-") == 0)
            return 0;
         return %r;
      }
   }

   %n1 = strLen(%a);
   %n2 = strLen(%b);
   %cc = (%n1 > %n2) ? %n1 : %n2;
   for(%x = 1; %x < %cc; %x++){
      %q = (%x < %n1) ? getSubStr(%a,(%n1 - %x)-1,1) : 0;
      %w = (%x < %n2) ? getSubStr(%b,(%n2 - %x)-1,1) : 0;
      %sum = %q+%w+%c;//18  = 9 + 9 + 0
      %newVal = (%sum % 10) @ %newVal;//8 = 18 % 10
      %c = mFloor(%sum/10); //1 = 18/10
   }
   return %c ? %c  @ %newVal : %newVal;
}
function addSubX(%a,%b){// auto flips so its subing form the largest basicly absolute value
   if(strPos(%a,"x") == -1 && strPos(%b,"x") == -1){
      %ab = %a - %b;
      if(%ab < 0){
         return 0;
      }
      return %ab;
   }
   if(strPos(%a,"x") == -1)
      %a = cropDec(%a);
   if(strPos(%b,"x") == -1)
      %b = cropDec(%b);

   %n1 = strLen(%a);
   %n2 = strLen(%b);
   %cc = (%n1 > %n2) ? %n1 : %n2;
   %c = 0;
   for(%x = 1; %x < %cc; %x++){
      %q = (%x < %n1) ? getSubStr(%a,(%n1 - %x)-1,1) : 0;
      %w = (%x < %n2) ? getSubStr(%b,(%n2 - %x)-1,1) : 0;
      %sub = %q-%w-%c;
      if(%x == %cc-1 && %sub == 0)
         break;
      if(%sub >= 0){
         %newVal = %sub  @ %newVal;
         %c = 0;
      }
      else{
         %newVal = %sub+10  @ %newVal;
         %c = 1;
      }
   }
   return trimZeroLeft(%newVal);
}
function trimZeroLeft(%val){
   %ln = strLen(%val);
   for(%x = 0; %x < %ln; %x++){
      %num = getSubStr(%val,%x,1);
      if(%num != 0)
       break;
   }
   if(%x == %ln)
      return 0;
   return getSubStr(%val,%x,%ln);
}
function xlCompare(%a,%b){
   if(strPos(%a,"x") == -1 && strPos(%b,"x") == -1){
      if(%a > %b)
         return ">";
      else if(%a < %b)
         return "<";
      else if (%a $= %b)
         return "=";
   }

   if(strPos(%a,"x") == -1)
      %a = %a @ "x";
   if(strPos(%b,"x") == -1)
      %b = %b @ "x";

   %n1 = strLen(%a);
   %n2 = strLen(%b);
   if(%n1 > %n2)
      return ">";
   else if(%n1 < %n2)
      return "<";
   else{
      if(%a $= %b)
         return "=";
      %g = %l = 0;
      for(%x = 0; %x < %n1-1; %x++){
         %q = getSubStr(%a,%x,1);
         %w = getSubStr(%b,%x,1);
         if(%q > %w)
            return ">";
         else if(%q < %w)
            return "<";
      }
   }
}
function getTimeDif(%time){
   %x = formattimestring("hh");
   %y = formattimestring("nn");
   %z = formattimestring("a");
   %a = getField(%time,0);
   %b = getField(%time,1);
   %c = getField(%time,2);
   if(%c $= "pm" && %a < 12)
      %a += 12;
   else if(%c $= "am" && %a == 12)
      %a = 0;
   if(%z $= "pm" && %x < 12)
      %x += 12;
   else if(%z $= "am" && %x == 12)
      %x = 0;

   %v = %a + (%b/60);
   %w = %x + (%y/60);
   %h = (%v >  %w) ? (%h = mabs(%v - %w)) : (24 - mabs(%v - %w));
   %min = %h - mfloor(%h);
   %ms = mfloor(%h) * ((60 * 1000)* 60); // 60 * 1000 1 min * 60  =  one hour
   %ms += mFloor((%min*60)+0.5) * (60 * 1000); // %min * 60 to convert back to mins , * 60kms for one min
   return mFloor(%ms);
}
function genBlanks(){ // optimization thing saves on haveing to do it with every setValueField
   if($dtStats::MaxNumOfGames > 300){
      $dtStats::MaxNumOfGames  = 300; //cap it
   }
   $dtStats::blank["g"] = $dtStats::blank["t"] = 0;
   for(%i=0; %i < $dtStats::MaxNumOfGames-1; %i++){
      $dtStats::blank["g"] = $dtStats::blank["g"] TAB 0;
   }
   for(%i=0; %i < 11; %i++){
      $dtStats::blank["t"] = $dtStats::blank["t"] TAB 0;
   }
}
function setValueField(%dtStats,%var,%type,%game,%c,%val){
   if(%type $= "g"){
      %fc = getFieldCount(%dtStats.gameStats[%var,%type,%game,$dtStats::tmMode]);
      if(%fc < 2){
         %dtStats.gameStats[%var,%type,%game,$dtStats::tmMode] = $dtStats::blank["g"];
      }
      else if( %fc > $dtStats::MaxNumOfGames){// trim it down as it as the MaxNumOfGames have gotten smaller
         %dtStats.gameStats[%var,%type,%game,$dtStats::tmMode] = getFields(%dtStats.gameStats[%var,%type,%game,$dtStats::tmMode],0,$dtStats::MaxNumOfGames-1);
      }
      %dtStats.gameStats[%var,%type,%game,$dtStats::tmMode] =   setField(%dtStats.gameStats[%var,%type,%game,$dtStats::tmMode],%c, hasValue(%val));
   }
   else if(%type $= "t"){
      %fc = getFieldCount(%dtStats.gameStats[%var,%type,%game,$dtStats::tmMode]);
      if(%fc < 2){
         %dtStats.gameStats[%var,%type,%game,$dtStats::tmMode] = $dtStats::blank["t"];
      }
      %dtStats.gameStats[%var,%type,%game,$dtStats::tmMode] =   setField(%dtStats.gameStats[%var,%type,%game,$dtStats::tmMode],%c, hasValue(%val));
   }
}

function hasValue(%val){//make sure we have at least something in the field spot
  if(%val $= ""){return 0;}
  return %val;
}

function resGameStats(%client,%game){// copy data back over to client
   if($dtStats::debugEchos){error("resGameStats GUID = "  SPC %client.guid);}
   %dtStats = %client.dtStats;
   %client.offenseScore = %dtStats.stat["offenseScore"];
   %client.kills = %dtStats.stat["kills"];
   %client.deaths = %dtStats.stat["deaths"];
   %client.suicides = %dtStats.stat["suicides"];
   %client.escortAssists = %dtStats.stat["escortAssists"];
   %client.teamKills = %dtStats.stat["teamKills"];
   %client.tkDestroys = %dtStats.stat["tkDestroys"];
   %client.flagCaps = %dtStats.stat["flagCaps"];
   %client.flagGrabs = %dtStats.stat["flagGrabs"];
   %client.genDestroys = %dtStats.stat["genDestroys"];
   %client.sensorDestroys = %dtStats.stat["sensorDestroys"];
   %client.turretDestroys = %dtStats.stat["turretDestroys"];
   %client.iStationDestroys = %dtStats.stat["iStationDestroys"];
   %client.vstationDestroys = %dtStats.stat["vstationDestroys"];
   %client.mpbtstationDestroys = %dtStats.stat["mpbtstationDestroys"];
   %client.solarDestroys = %dtStats.stat["solarDestroys"];
   %client.sentryDestroys = %dtStats.stat["sentryDestroys"];
   %client.depSensorDestroys = %dtStats.stat["depSensorDestroys"];
   %client.depTurretDestroys = %dtStats.stat["depTurretDestroys"];
   %client.depStationDestroys = %dtStats.stat["depStationDestroys"];
   %client.vehicleScore = %dtStats.stat["vehicleScore"];
   %client.vehicleBonus  = %dtStats.stat["vehicleBonus"];

   %client.flagDefends = %dtStats.stat["flagDefends"];
   %client.defenseScore = %dtStats.stat["defenseScore"];
   %client.genDefends = %dtStats.stat["genDefends"];
   %client.carrierKills = %dtStats.stat["carrierKills"];
   %client.escortAssists = %dtStats.stat["escortAssists"];
   %client.turretKills = %dtStats.stat["turretKills"];
   %client.mannedTurretKills = %dtStats.stat["mannedTurretKills"];
   %client.flagReturns = %dtStats.stat["flagReturns"];
   %client.genRepairs = %dtStats.stat["genRepairs"];
   %client.SensorRepairs = %dtStats.stat["SensorRepairs"];
   %client.TurretRepairs = %dtStats.stat["TurretRepairs"];
   %client.StationRepairs = %dtStats.stat["StationRepairs"];
   %client.VStationRepairs = %dtStats.stat["VStationRepairs"];
   %client.mpbtstationRepairs = %dtStats.stat["mpbtstationRepairs"];
   %client.solarRepairs = %dtStats.stat["solarRepairs"];
   %client.sentryRepairs = %dtStats.stat["sentryRepairs"];
   %client.depSensorRepairs = %dtStats.stat["depSensorRepairs"];
   %client.depInvRepairs = %dtStats.stat["depInvRepairs"];
   %client.depTurretRepairs = %dtStats.stat["depTurretRepairs"];
   %client.returnPts = %dtStats.stat["returnPts"];
   %client.score = %dtStats.stat["score"];

   %client.efficiency = %dtStats.stat["efficiency"];
   %client.MidAir = %dtStats.stat["MidAir"];
   %client.Bonus = %dtStats.stat["Bonus"];
   %client.KillStreakBonus = %dtStats.stat["KillStreakBonus"];
   %client.killCounter = %dtStats.stat["killCounter"];
   %client.objScore = %dtStats.stat["objScore"];
   %client.flipFlopDefends = %dtStats.stat["flipFlopDefends"];
   //%client.outOfBounds = %dtStats.stat["outOfBounds"];
	%client.flagTimeMS = %dtStats.stat["flagTimeMS"];
	%client.morepoints = %dtStats.stat["morepoints"];
	%client.mas = %dtStats.stat["mas"];
	%client.totalSpeed = %dtStats.stat["totalSpeed"];
	%client.totalDistance = %dtStats.stat["totalDistance"];
	%client.totalChainAccuracy = %dtStats.stat["totalChainAccuracy"];
	%client.totalChainHits = %dtStats.stat["totalChainHits"];
	%client.totalSnipeHits = %dtStats.stat["totalSnipeHits"];
	%client.totalSnipes = %dtStats.stat["totalSnipes"];
	%client.totalShockHits = %dtStats.stat["totalShockHits"];
   %client.totalShocks = %dtStats.stat["totalShocks"];

   %client.snipeKills = %dtStats.stat["snipeKills"];
   %client.roundsWon = %dtStats.stat["roundsWon"];
   %client.roundsLost = %dtStats.stat["roundsLost"];
   %client.assists = %dtStats.stat["assists"];
   %client.roundKills = %dtStats.stat["roundKills"];
   %client.hatTricks = %dtStats.stat["hatTricks"];
}

function resetChain(%game,%dtStats,%count,%last){
   //if($dtStats::debugEchos){error("resetChain" SPC %last SPC %count);}
   for(%i = %last; %i < %count; %i++){
      %var = $statsVars::varName[%i,%game];
      %dtStats.stat[%var]= 0;
   }
}
function resetDtStats(%dtStats,%game,%slow){
   if($dtStats::debugEchos){error("resetDtStats GUID = "  SPC %dtStats.guid);}
   if(isObject(%dtStats)){
      %dtStats.joinTime = getSimTime();
      if(%slow){// low server impact
         %amount =  100;
         %count = mFloor($statsVars::count[%game]/%amount);
         %leftOver = $statsVars::count[%game] - (%count * %amount);
         for(%i = 0; %i < %count; %i++){
            %x  += %amount;
            schedule(32*%i,0,"resetChain",%game,%dtStats,%x,(%i * %amount));
         }
         schedule(32*(%i+1),0,"resetChain",%game,%dtStats,(%x+%leftOver),(%i * %amount));
         for(%i = 1; %i <= $dtStats::unusedCount; %i++){//script unused
            %var = $dtStats::unused[%i];
            %dtStats.stat[%var]= 0;
         }
      }
      else{
         for(%q = 0; %q < $statsVars::count[%game]; %q++){
            %var = $statsVars::varName[%q,%game];
            %dtStats.stat[%var]= 0;
         }
      }
      //for(%i = 1; %i <= $dtStats::uGFC[%game]; %i++){//script unused
         //%var = $dtStats::uGFV[%i,%game];;
         //%dtStats.stat[%var]= 0;
      //}

      for(%i = 1; %i <= $dtStats::unusedCount; %i++){//script unused
         %var = $dtStats::unused[%i];
         %dtStats.stat[%var]= 0;
      }
   }
}
function buildVarList(){
   deleteVariables("$statsVars::*");
   for(%g = 0; %g < $dtStats::gameTypeCount; %g++){
      %game = $dtStats::gameType[%g];
      $statsVars::count[%game] = -1;
      for(%v = 0; %v < $dtStats::varTypeCount; %v++){
         %varType = $dtStats::varType[%v];
         for(%i = 1; %i <= $dtStats::FCG[%game,%varType]; %i++){// game types
            %var = $dtStats::FVG[%i,%game,%varType] @ %varType;
            if($statsVars::varType[%var,%game] $= ""){
               $statsVars::varType[%var,%game] = %varType;
               $statsVars::varNameType[$statsVars::count[%game]++,%game] = %var;
               $statsVars::varName[$statsVars::count[%game],%game] = $dtStats::FVG[%i,%game,%varType];
            }
            else{
               error("Error buildVarList duplicate var:" SPC %var );
            }
         }
         for(%i = 1; %i <= $dtStats::FC[%game,%varType]; %i++){// game type script
            %var = $dtStats::FV[%i,%game,%varType] @ %varType;
            if($statsVars::varType[%var,%game] $= ""){
               $statsVars::varType[%var,%game] = %varType;
               $statsVars::varNameType[$statsVars::count[%game]++,%game] = %var;
               $statsVars::varName[$statsVars::count[%game],%game] = $dtStats::FV[%i,%game,%varType];
            }
            else{
               error("Error buildVarList duplicate var:" SPC %var );
            }
         }
         for(%i = 1; %i <= $dtStats::FC[%varType]; %i++){// script
            %var = $dtStats::FV[%i,%varType] @ %varType;
            if($statsVars::varType[%var,%game] $= ""){
               $statsVars::varType[%var,%game] = %varType;
               $statsVars::varNameType[$statsVars::count[%game]++,%game] = %var;
               $statsVars::varName[$statsVars::count[%game],%game] = $dtStats::FV[%i,%varType];
            }
            else{
               error("Error buildVarList duplicate var:" SPC %var );
            }
         }
      }
      $statsVars::count[%game] += 1;
   }
}
////////////////////////////////////////////////////////////////////////////////
//Stats Collecting
////////////////////////////////////////////////////////////////////////////////
function armorTimer(%dtStats, %size, %death){
   if(%dtStats.lastArmor $= "Light" && %dtStats.ArmorTime[%dtStats.lastArmor] > 0){
      %dtStats.stat["lArmorTime"] += ((getSimTime() - %dtStats.ArmorTime[%dtStats.lastArmor])/1000)/60;
      %dtStats.ArmorTime[%dtStats.lastArmor] = 0;
      %dtStats.lastArmor = 0;
   }
   else if(%dtStats.lastArmor $= "Medium" && %dtStats.ArmorTime[%dtStats.lastArmor] > 0){
      %dtStats.stat["mArmorTime"] += ((getSimTime() - %dtStats.ArmorTime[%dtStats.lastArmor])/1000)/60;
      %dtStats.ArmorTime[%dtStats.lastArmor] = 0;
      %dtStats.lastArmor = 0;
   }
   else if(%dtStats.lastArmor $= "Heavy" && %dtStats.ArmorTime[%dtStats.lastArmor] > 0){
      %dtStats.stat["hArmorTime"] += ((getSimTime() - %dtStats.ArmorTime[%dtStats.lastArmor])/1000)/60;
      %dtStats.ArmorTime[%dtStats.lastArmor] = 0;
      %dtStats.lastArmor = 0;
   }
   if(!%death){
      %dtStats.ArmorTime[%size] = getSimTime();
      %dtStats.lastArmor = %size;
   }
   //error(%dtStats.stat["lArmorTime"] SPC %dtStats.stat["mArmorTime"] SPC %dtStats.stat["hArmorTime"]);
}
function updateTeamTime(%dtStats,%team){
   if(Game.numTeams > 1){
      %time = getSimTime() - %dtStats.teamTime;
      if(%team == 1){
         %dtStats.stat["timeOnTeamOne"] += (%time/1000)/60;
         %dtStats.teamTime = getSimTime();
      }
      else if(%team == 2){
         %dtStats.stat["timeOnTeamTwo"] += (%time/1000)/60;
         %dtStats.teamTime = getSimTime();
      }
      else if(%team == 0){
         %dtStats.stat["timeOnTeamZero"] += (%time/1000)/60;
         %dtStats.teamTime = getSimTime();
      }
      else{
         %dtStats.teamTime = getSimTime();
      }
   }
   //error(%team SPC  %dtStats.stat["timeOnTeamZero"] SPC %dtStats.stat["timeOnTeamOne"] SPC %dtStats.stat["timeOnTeamTwo"]);
}
function deadDist(%pos,%pl){
   if(isObject(%pl)){
      %dist =  vectorDist(getWords(%pos,0,1) SPC 0, getWords(%pl.getPosition(),0,1) SPC 0); // 2d distance
      //%dist =  vectorDist(%pos,%pl.getPosition());
      if(%dist > %pl.client.dtStats.stat["deadDist"])
         %pl.client.dtStats.stat["deadDist"] = %dist;
   }
}

function dtMinMax(%statName,%group,%minMax,%value,%client){
   if($dtStats::evoStyleDebrief){
      if(!isObject(dtGameStat)){
         new scriptObject(dtGameStat);
         rootGroup.add(dtGameStat);
      }
      if(%value != 0){
         dtGameStat.gc[%group]++;
         switch(%minMax){
            case 1:
               if(dtGameStat.stat[%statName] < %value || dtGameStat.stat[%statName] $= ""){
                  dtGameStat.stat[%statName] = %value;
                  dtGameStat.name[%statName] =  getTaggedString(%client.name);
                  dtGameStat.client[%statName] = %client;
               }
            case 2:
               if(dtGameStat.stat[%statName] > %value || dtGameStat.stat[%statName] $= ""){
                  dtGameStat.stat[%statName] = %value;
                  dtGameStat.name[%statName] =  getTaggedString(%client.name);
                  dtGameStat.client[%statName] = %client;
               }
            case 3://value counter;
               dtGameStat.statTrack[%statName, %client] += %value;
               %curValue = dtGameStat.statTrack[%statName, %client];
               if(dtGameStat.stat[%statName] < %curValue || dtGameStat.stat[%statName] $= ""){
                  dtGameStat.stat[%statName] = %curValue;
                  dtGameStat.name[%statName] =  getTaggedString(%client.name);
                  dtGameStat.client[%statName] = %client;
               }
         }
      }
   }
}

function clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation){
   if(%clKiller.team != %clVictim.team && isObject(%clKiller.player)){// note test for vehicles
      %dist = vectorDist($dtStats::FlagPos[%clKiller.team], %clKiller.player.getPosition());
      if(%dist > ($dtStats::FlagTotalDist*0.5)){// kill made closer to the enemy flag
         %clKiller.dtStats.stat["OffKills"]++;
            %armorSize = %clKiller.player.getArmorSize();
         switch$(%armorSize){
            case "Light":%clKiller.dtStats.stat["OffKillsL"]++;
            case "Medium":%clKiller.dtStats.stat["OffKillsM"]++;
            case "Heavy": %clKiller.dtStats.stat["OffKillsH"]++;
         }
      }
      else{
         %clKiller.dtStats.stat["DefKills"]++;
         %armorSize = %clKiller.player.getArmorSize();
         switch$(%armorSize){
            case "Light":%clKiller.dtStats.stat["DefKillsL"]++;
            case "Medium":%clKiller.dtStats.stat["DefKillsM"]++;
            case "Heavy": %clKiller.dtStats.stat["DefKillsH"]++;
         }
      }
   }
   if(%damageType == $DamageType::Explosion || %damageType == $DamageType::Ground ||
      %damageType == $DamageType::OutOfBounds ||  %damageType == $DamageType::Lava ||
      %damageType == $DamageType::VehicleSpawn || %damageType == $DamageType::ForceFieldPowerup ||
      %damageType == $DamageType::Lightning  ||   %damageType == $DamageType::NexusCamping ||
      %damageType == $DamageType::Suicide	){
      if((getSimTime() - %clVictim.lastHitTime) < 3000)
         %clKiller = %clVictim.lastHitBy;
         %clVictim.lastHitBy = 0;
   }
   else if(!isObject(%clKiller) && isObject(%implement)){
      if(%damageType == $DamageType::IndoorDepTurret || %damageType == $DamageType::OutdoorDepTurret){
         %clKiller = %implement.owner;
      }
      else
         %clKiller = %implement.getControllingClient();
   }
   %killerDT = %clKiller.dtStats;
   %victimDT = %clVictim.dtStats;

   if(%clKiller.killBy  == %clVictim){
      %clKiller.killBy = 0;
      %killerDT.stat["revenge"]++;
   }
   %clVictim.killBy = %clKiller;
   //fail safe in case  package is out of order
   %victimPlayer = isObject(%clVictim.player) ? %clVictim.player : %clVictim.lastPlayer;
   %killerPlayer = isObject(%clKiller.player) ? %clKiller.player : %clKiller.lastPlayer;
   %clVictim.lp = "";//last position for distMove
   armorTimer(%victimDT, 0, 1);
//------------------------------------------------------------------------------
   %victimDT.timeToLive += getSimTime() - %clVictim.spawnTime;
   %victimDT.stat["timeTL"] = mFloor(((%victimDT.timeToLive/(%clVictim.stat["deaths"]+%clVictim.stat["suicides"] ? %clVictim.stat["deaths"]+%clVictim.stat["suicides"] : 1))/1000)/60);
//------------------------------------------------------------------------------
   if(%clKiller.team == %clVictim.team && %clKiller != %clVictim){
      %killerDT.stat["teamkillCount"]++;
      if(%damageType  == $DamageType::Missile){
         %killerDT.stat["missileTK"]++;
         if(getSimTime() - %clKiller.stat["flareHit"] < 256){
            %clKiller.flareSource.dtStats.stat["flareKill"]++;
         }
      }
   }
   if(getSimtime() - %clKiller.lastDiscJump < 256){
      if(%clKiller == %clVictim){
         %killerDT.stat["discJump"]--;// we killed are self so remove stat
      }
      else{
         %killerDT.stat["killerDiscJump"]++;
      }
   }
//------------------------------------------------------------------------------
   if(%clKiller.team != %clVictim.team){
      if(isObject(%victimPlayer) && isObject(%killerPlayer) && %damageType != $DamageType::IndoorDepTurret && %damageType != $DamageType::OutdoorDepTurret){
            schedule($CorpseTimeoutValue - 2000,0,"deadDist",%victimPlayer.getPosition(),%victimPlayer);
//------------------------------------------------------------------------------
            %killerDT.ksCounter++; %victimDT.ksCounter = 0;
            if(%clVictim == %clKiller || %damageType == $DamageType::Suicide || %damageType == $DamageType::Lava || %damageType == $DamageType::OutOfBounds || %damageType == $DamageType::Ground || %damageType == $DamageType::Lightning){
              %victimDT.ksCounter = %killerDT.ksCounter = 0;
            }
            if(%killerDT.stat["killStreak"] < %killerDT.ksCounter){
               %killerDT.stat["killStreak"] = %killerDT.ksCounter;
            }
//------------------------------------------------------------------------------
            if(%victimPlayer.hitBy[%clKiller]){
               %killerDT.stat["assist"]--;
            }
//------------------------------------------------------------------------------
            %isCombo = 0;
            if(%killerPlayer.combo[%victimPlayer] > 1){
               %killerDT.stat["comboCount"]++;
               %isCombo =1;
            }
//------------------------------------------------------------------------------
         if(!statsGroup.stat["firstKill"] && isGameRun()){
            statsGroup.stat["firstKill"] = 1;
            %killerDT.stat["firstKill"] = 1;
         }
//------------------------------------------------------------------------------
         statsGroup.stat["lastKill"] = %killerDT;
//------------------------------------------------------------------------------
         if(%killerPlayer.getState() $= "Dead"){
            %killerDT.stat["deathKills"]++;
         }
//------------------------------------------------------------------------------
         if(getSimTime() - %clKiller.mKill < 256){
            %clKiller.mkCounter++;
            if(!isEventPending(%clKiller.mkID))
               %clKiller.mkID = schedule(256,0,"multiKillDelayer",%clKiller,%killerDT);
         }
         else{
            if(!isEventPending(%clKiller.mkID))
               %clKiller.mkCounter = 1;
         }%clKiller.mKill =  getSimTime();
//------------------------------------------------------------------------------
         if(getSimTime() - %clKiller.mCKill < 10000){
               if(%clKiller.chainCount++ > 1)
                  chainKill(%killerDT,%clKiller);
         }
         else{
            %clKiller.chainCount = 1;
         }%clKiller.mCKill =  getSimTime();
//------------------------------------------------------------------------------

         if(%victimPlayer.inStation){
            %victimDT.stat["inventoryDeaths"]++;
            %killerDT.stat["inventoryKills"]++;
         }

//------------------------------------------------------------------------------
         if(rayTest(%victimPlayer, $dtStats::midAirHeight)){%vcAir =1;}else{%vcAir =2;}
         if(rayTest(%killerPlayer, $dtStats::midAirHeight)){%kcAir =1;}else{%kcAir =2;}
         %vdis = rayTestDis(%victimPlayer);

         switch$(%victimPlayer.getArmorSize()){
            case "Light":%killerDT.stat["armorL"]++;
               switch$(%killerPlayer.getArmorSize()){
                  case "Light": %killerDT.stat["armorLL"]++;%killerDT.stat["armorLK"]++;
                  case "Medium":%killerDT.stat["armorML"]++;%killerDT.stat["armorMK"]++;
                  case "Heavy": %killerDT.stat["armorHL"]++;%killerDT.stat["armorHK"]++;
               }
            case "Medium": %killerDT.stat["armorM"]++;
               switch$(%killerPlayer.getArmorSize()){
                  case "Light": %killerDT.stat["armorLM"]++;%killerDT.stat["armorLK"]++;
                  case "Medium":%killerDT.stat["armorMM"]++;%killerDT.stat["armorMK"]++;
                  case "Heavy": %killerDT.stat["armorHM"]++;%killerDT.stat["armorHK"]++;
               }
            case "Heavy":%killerDT.stat["armorH"]++;
               switch$(%killerPlayer.getArmorSize()){
                  case "Light": %killerDT.stat["armorLH"]++;%killerDT.stat["armorLK"]++;
                  case "Medium":%killerDT.stat["armorMH"]++;%killerDT.stat["armorMK"]++;
                  case "Heavy": %killerDT.stat["armorHH"]++;%killerDT.stat["armorHK"]++;
               }
         }
//------------------------------------------------------------------------------
         %dis = vectorDist(%killerPlayer.getPosition(),%victimPlayer.getPosition());
         %victimVel =  mFloor(vectorLen(%victimPlayer.getVelocity()) * 3.6);
      }
      else{
         %kcAir = %vcAir = 0;
         %dis = 0;
      }
//------------------------------------------------------------------------------
      if(%clVictim.EVDamageType && %clVictim.EVDamageType != %damageType && (getSimTime() - %clVictim.EVDamagetime) < 10000){ // they were hit by something befor they were killed
         %killerDT.EVKillsWep++;
         %victimDT.EVDeathsWep++;
         if(rayTest(%victimPlayer, $dtStats::midAirHeight)){
            if(%clVictim.EVDamageType == $DamageType::Lightning && (getSimTime() - %clVictim.EVDamagetime) < 3000){
               %killerDT.stat["lightningMAkills"]++;
               %clKiller.dtMessage("Lightning MidAir Kill","fx/misc/MA2.wav",1);
            }
            else
               %killerDT.EVMA++;
         }
         %clVictim.EVDamageType = 0;
      }
//------------------------------------------------------------------------------
      if(getSimTime() - %victimPlayer.isCloakTime < 2000 && %victimPlayer.isCloakTime  > 0){
         %killerDT.stat["cloakersKilled"]++;
      }
      if(getSimTime() - %killerPlayer.isCloakTime < 2000 && %killerPlayer.isCloakTime  > 0){
         %killerDT.stat["cloakerKills"]++;
      }

      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %killerDT.stat["cgKills"]++;
            %victimDT.stat["cgDeaths"]++;
            if(%killerDT.stat["cgKillDist"] < %dis){%killerDT.stat["cgKillDist"] = %dis;}

            if(%isCombo){%killerDT.stat["cgCom"]++;}
            dtMinMax("cgKills", "wep", 1, %killerDT.stat["cgKills"], %clKiller);
         case $DamageType::Disc:
            %killerDT.stat["discKills"]++;
            %victimDT.stat["discDeaths"]++;
            if(%vdis < 6){%killerDT.stat["discKillGround"]++;}
            if(%killerDT.stat["discKillDist"] < %dis){%killerDT.stat["discKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["discCom"]++;}
            if(%clKiller.mdHit){%killerDT.stat["minePlusDiscKill"]++;}

            if(getSimTime() - %clKiller.discReflect < 256){%killerDT.stat["discReflectKill"]++;}

            dtMinMax("discKills", "wep", 1, %killerDT.stat["discKills"], %clKiller);
            dtMinMax("minePlusDiscKill", "wep", 1, %killerDT.stat["minePlusDiscKill"], %clKiller);
         case $DamageType::Grenade:
            if($dtObjExplode.dtNade){
               %killerDT.stat["hGrenadeKills"]++;
               %victimDT.stat["hGrenadeDeaths"]++;
               if(%killerDT.stat["hGrenadeKillDist"] < %dis){%killerDT.stat["hGrenadeKillDist"] = %dis;}
               if(%isCombo){%killerDT.stat["hGrenadeCom"]++;}
               dtMinMax("hGrenadeKills", "wep", 1, %killerDT.stat["hGrenadeKills"], %clKiller);
            }
            else{
               %killerDT.stat["grenadeKills"]++;
               %victimDT.stat["grenadeDeaths"]++;
               if(%killerDT.stat["grenadeKillDist"] < %dis){%killerDT.stat["grenadeKillDist"] = %dis;}
               if(%isCombo){%killerDT.stat["grenadeCom"]++;}
               dtMinMax("grenadeKills", "wep", 1, %killerDT.stat["grenadeKills"], %clKiller);
            }
         case $DamageType::Laser:
            if(%killerDT.lastHeadShotTime && (getSimTime() - %killerDT.lastHeadShotTime) < 128){
               %killerDT.stat["laserHSKills"]++;//headshot
            }
            %killerDT.stat["laserKills"]++;
            %victimDT.stat["laserDeaths"]++;
            if(%killerDT.stat["laserKillDist"] < %dis){%killerDT.stat["laserKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["laserCom"]++;}
            dtMinMax("laserKills", "wep", 1, %killerDT.stat["laserKills"], %clKiller);
         case $DamageType::Mortar:
            %killerDT.stat["mortarKills"]++;
            %victimDT.stat["mortarDeaths"]++;
            if(%killerDT.stat["mortarKillDist"] < %dis){%killerDT.stat["mortarKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["mortarCom"]++;}
            dtMinMax("mortarKills", "wep", 1, %killerDT.stat["mortarKills"], %clKiller);
         case $DamageType::Missile:
            %killerDT.stat["missileKills"]++;
            %victimDT.stat["missileDeaths"]++;
            if(%killerDT.stat["missileKillDist"] < %dis){%killerDT.stat["missileKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["missileCom"]++;}
            dtMinMax("missileKills", "wep", 1, %killerDT.stat["missileKills"], %clKiller);
         case $DamageType::ShockLance:
            %killerDT.stat["shockKills"]++;
            %victimDT.stat["shockDeaths"]++;
            if(%killerDT.stat["shockKillDist"] < %dis){%killerDT.stat["shockKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["shockCom"]++;}
            dtMinMax("shockKills", "wep", 1, %killerDT.stat["shockKills"], %clKiller);
         case $DamageType::Plasma:
            %killerDT.stat["plasmaKills"]++;
            %victimDT.stat["plasmaDeaths"]++;
            if(%killerDT.stat["plasmaKillDist"] < %dis){%killerDT.stat["plasmaKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["plasmaCom"]++;}
            dtMinMax("plasmaKills", "wep", 1, %killerDT.stat["plasmaKills"], %clKiller);
         case $DamageType::Blaster:
            %killerDT.stat["blasterKills"]++;
            %victimDT.stat["blasterDeaths"]++;
            if(%killerDT.stat["blasterKillDist"] < %dis){%killerDT.stat["blasterKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["blasterCom"]++;}
            if(getSimTime() - %clKiller.blasterReflect < 256){%killerDT.stat["blasterReflectKill"]++;}
            dtMinMax("blasterKills", "wep", 1, %killerDT.stat["blasterKills"], %clKiller);
         case $DamageType::ELF:
            %killerDT.elfKills++;
            %victimDT.elfDeaths++;
         case $DamageType::Mine:
            %killerDT.stat["mineKills"]++;
            %victimDT.stat["mineDeaths"]++;
            if(%killerDT.stat["mineKillDist"] < %dis){%killerDT.stat["mineKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["mineCom"]++;}
            if(%clKiller.mdHit){%killerDT.stat["minePlusDiscKill"]++;}
            dtMinMax("mineKills", "wep", 1, %killerDT.stat["mineKills"], %clKiller);
             dtMinMax("minePlusDiscKill", "wep", 1, %killerDT.stat["minePlusDiscKill"], %clKiller);
         case $DamageType::SatchelCharge:
            %killerDT.stat["satchelKills"]++;
            %victimDT.stat["satchelDeaths"]++;
            if(%killerDT.stat["satchelKillDist"] < %dis){%killerDT.stat["satchelKillDist"] = %dis;}
            if(%isCombo){%killerDT.stat["satchelCom"]++;}
            dtMinMax("satchelKills", "wep", 1, %killerDT.stat["satchelKills"], %clKiller);
         case $DamageType::Explosion:
            if(%clKiller){%killerDT.stat["explosionKills"]++;}
            %victimDT.stat["explosionDeaths"]++;
         case $DamageType::Impact:
            if(isObject(%clKiller.vehicleMounted)){
               %veh =   %clKiller.vehicleMounted.getDataBlock().getName();
               %killerDT.stat["roadKills"]++;  %victimDT.stat["roadDeaths"]++;
               dtMinMax("roadKills", "wep", 1, %killerDT.stat["roadKills"], %clKiller);
               switch$(%veh){
                  case "ScoutVehicle":     %killerDT.stat["wildRK"]++;       %victimDT.stat["wildRD"]++;
                  case "AssaultVehicle":   %killerDT.stat["assaultRK"]++;    %victimDT.stat["assaultRD"]++;
                  case "MobileBaseVehicle":%killerDT.stat["mobileBaseRK"]++; %victimDT.stat["mobileBaseRD"]++;
                  case "ScoutFlyer":       %killerDT.stat["scoutFlyerRK"]++; %victimDT.stat["scoutFlyerRD"]++;
                  case "BomberFlyer":      %killerDT.stat["bomberFlyerRK"]++;%victimDT.stat["bomberFlyerRD"]++;
                  case "HAPCFlyer":        %killerDT.stat["hapcFlyerRK"]++;  %victimDT.stat["hapcFlyerRD"]++;
               }
            }
            %killerDT.stat["impactKills"]++;
            %victimDT.stat["impactDeaths"]++;
         case $DamageType::Ground:
            if(%clKiller){%killerDT.stat["groundKills"]++;}
            %victimDT.stat["groundDeaths"]++;
         case $DamageType::PlasmaTurret:
            %killerDT.stat["plasmaTurretKills"]++;
            %victimDT.stat["plasmaTurretDeaths"]++;
         case $DamageType::AATurret:
            %killerDT.stat["aaTurretKills"]++;
            %victimDT.stat["aaTurretDeaths"]++;
         case $DamageType::ElfTurret:
            %killerDT.stat["elfTurretKills"]++;
            %victimDT.stat["elfTurretDeaths"]++;
         case $DamageType::MortarTurret:
            %killerDT.stat["mortarTurretKills"]++;
            %victimDT.stat["mortarTurretDeaths"]++;
         case $DamageType::MissileTurret:
            %killerDT.stat["missileTurretKills"]++;
            %victimDT.stat["missileTurretDeaths"]++;
         case $DamageType::IndoorDepTurret:
            %killerDT.stat["indoorDepTurretKills"]++;
            %victimDT.stat["indoorDepTurretDeaths"]++;
            dtMinMax("indoorDepTurretKills", "wep", 1, %killerDT.stat["indoorDepTurretKills"], %clKiller);
         case $DamageType::OutdoorDepTurret:
            %killerDT.stat["outdoorDepTurretKills"]++;
            %victimDT.stat["outdoorDepTurretDeaths"]++;
            dtMinMax("outdoorDepTurretKills", "wep", 1, %killerDT.stat["outdoorDepTurretKills"], %clKiller);
         case $DamageType::SentryTurret:
            %killerDT.stat["sentryTurretKills"]++;
            %victimDT.stat["sentryTurretDeaths"]++;
         case $DamageType::OutOfBounds:
            if(%clKiller){%killerDT.stat["outOfBoundKills"]++;}
            %victimDT.stat["outOfBoundDeaths"]++;
         case $DamageType::Lava:
            if(%clKiller){%killerDT.stat["lavaKills"]++;}
            %victimDT.stat["lavaDeaths"]++;
         case $DamageType::ShrikeBlaster:
            %killerDT.stat["shrikeBlasterKills"]++;
            %victimDT.stat["shrikeBlasterDeaths"]++;
            dtMinMax("shrikeBlasterKills", "wep", 1, %killerDT.stat["shrikeBlasterKills"], %clKiller);
         case $DamageType::BellyTurret:
            %killerDT.stat["bellyTurretKills"]++;
            %victimDT.stat["bellyTurretDeaths"]++;
             dtMinMax("bomberBombsKills", 1, %killerDT.stat["bellyTurretKills"], %clKiller);
         case $DamageType::BomberBombs:
            %killerDT.stat["bomberBombsKills"]++;
            %victimDT.stat["bomberBombsDeaths"]++;
             dtMinMax("bomberBombsKills", "wep", 1, %killerDT.stat["bomberBombsKills"], %clKiller);
         case $DamageType::TankChaingun:
            %killerDT.stat["tankChaingunKills"]++;
            %victimDT.stat["tankChaingunDeaths"]++;
            dtMinMax("tankChaingunKills", "wep", 1, %killerDT.stat["tankChaingunKills"], %clKiller);
         case $DamageType::TankMortar:
            %killerDT.stat["tankMortarKills"]++;
            %victimDT.stat["tankMortarDeaths"]++;
            dtMinMax("tankMortarKills", "wep", 1, %killerDT.stat["tankMortarKills"], %clKiller);
         case $DamageType::Lightning:
            if(%clKiller){
               %killerDT.stat["lightningKills"]++;
               if(%vcAir == 1 && (getSimTime() - %clVictim.lastHitTime) < 3000 && %clVictim.lastHitMA){
                  %killerDT.stat["lightningMAEVKills"]++;
                  %killerDT.stat["lightningMAkills"]++;
                  %clKiller.dtMessage("Lightning MidAir EV Kill","fx/misc/MA2.wav",1);
               }
            }
            %victimDT.stat["lightningDeaths"]++;
         case $DamageType::VehicleSpawn:
            if(%clKiller){%killerDT.stat["vehicleSpawnKills"]++;}
            %victimDT.stat["vehicleSpawnDeaths"]++;
         case $DamageType::ForceFieldPowerup:
            if(%clKiller){%killerDT.stat["forceFieldPowerUpKills"]++;}
            %victimDT.stat["forceFieldPowerUpDeaths"]++;
         case $DamageType::Crash:
            %killerDT.stat["crashKills"]++;
            %victimDT.stat["crashDeaths"]++;
         case $DamageType::NexusCamping:
            if(%clKiller){%killerDT.stat["nexusCampingKills"]++;}
            %victimDT.stat["nexusCampingDeaths"]++;
         case $DamageType::Suicide:
            if(%clKiller){%killerDT.stat["ctrlKKills"]++;}
            //%victimDT.stat["ctrlKKills"]++;
      }
   }
}

function multiKillDelayer(%clKiller,%killerDT){
   switch(%clKiller.mkCounter){
      case 2:
         %killerDT.stat["doubleKill"]++;
      case 3:
         %killerDT.stat["tripleKill"]++;
      case 4:
         %killerDT.stat["quadrupleKill"]++;
      case 5:
         %killerDT.stat["quintupleKill"]++;
      case 6:
         %killerDT.stat["sextupleKill"]++;
      case 7:
         %killerDT.stat["septupleKill"]++;
      case 8:
         %killerDT.stat["octupleKill"]++;
      case 9:
         %killerDT.stat["nonupleKill"]++;
      case 10:
         %killerDT.stat["decupleKill"]++;
      default:
         if(%clKiller.mkCounter > 10)
            %killerDT.stat["nuclearKill"]++;
   }
   %killerDT.stat["multiKill"]++;
   %clKiller.mkCounter = 1;
}

function chainKill(%killerDT,%clKiller){
   %killerDT.stat["chainKill"]++;
   switch(%clKiller.chainCount){
      case 2:
         %killerDT.stat["doubleChainKill"]++;
      case 3:
         %killerDT.stat["tripleChainKill"]++;
      case 4:
         %killerDT.stat["quadrupleChainKill"]++;
      case 5:
         %killerDT.stat["quintupleChainKill"]++;
      case 6:
         %killerDT.stat["sextupleChainKill"]++;
      case 7:
         %killerDT.stat["septupleChainKill"]++;
      case 8:
         %killerDT.stat["octupleChainKill"]++;
      case 9:
         %killerDT.stat["nonupleChainKill"]++;
      case 10:
         %killerDT.stat["decupleChainKill"]++;
   }
}

function GameConnection::dtMessage(%this,%message,%sfx,%bypass){
   if(!%this.isAIControlled()){
      %diff =  getSimTime() - %this.dtLastMessage;
      if(%sfx !$= "" && %bypass){
         %this.dtLastMessage = getSimTime();
         messageClient(%this,'MsgClient', "\c2" @ %message @ "~w" @ %sfx);
      }
      else if(%sfx !$= "" && %diff > 256){// limits sound spam
         %this.dtLastMessage = getSimTime();
         messageClient(%this,'MsgClient', "\c2" @ %message @ "~w" @ %sfx);
      }
      else
         messageClient(%this,'MsgClient', "\c2" @ %message);
      BottomPrint( %this, "\n" @ %message, 2, 3 );
   }
}

function rayTest(%targetObject,%dis){
   if(isObject(%targetObject)){
      %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::ForceFieldObjectType | $TypeMasks::VehicleObjectType;
      %rayStart = %targetObject.getWorldBoxCenter();
      %rayEnd = VectorAdd(%rayStart,"0 0" SPC ((%dis+1.15) * -1));
      %ground = !ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject);
      return %ground;
   }
   else
      return 0;
}

function rayTestDis(%targetObject){
   if(isObject(%targetObject)){
      %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::ForceFieldObjectType | $TypeMasks::VehicleObjectType;
      %rayStart = %targetObject.getWorldBoxCenter();
      %rayEnd = VectorAdd(%rayStart,"0 0" SPC -5000);
      %ray = ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject);
      if(!%ray)
         return 0;
      return vectorDist(%rayStart,getWords(%ray,1,3)) - 1.15;
   }
   else
      return 0;
}
//%cl.lastExp = %data TAB %projectile.initialPosition TAB %position TAB %projectile.getWorldBox();
function testHit3(%sClient,%tgClient){
   %plr = %tgClient.player;
   %pos = getField(%sClient.lastExp,2);
   %dist = vectorDist(%tgClient.getWorldBoxCenter(), %pos);
   %hit = (%dist < 2);
   if(%hit && (!%sClient.lastDHit || (getSimTime() - %sClient.lastDHit > 128))){
      %sClient.lastDHit = getSimTime();// lock out double hits
      return 1;
   }
   return 0;
}

function testHit2(%sClient,%tgClient){
   %plr = %tgClient.player;
   %b = getField(%sClient.lastExp,3);
   %a = %plr.getWorldBox();

   %hit  =  (getWord(%a, 0) <= getWord(%b, 3) && getWord(%a, 3) >= getWord(%b, 0)) &&
         (getWord(%a, 1) <= getWord(%b, 4) && getWord(%a, 4) >= getWord(%b, 1)) &&
         (getWord(%a, 2)<= getWord(%b, 5) && getWord(%a, 5) >= getWord(%b, 2));
   if(%hit && (!%sClient.lastDHit || (getSimTime() - %sClient.lastDHit > 128))){
      %sClient.lastDHit = getSimTime();// lock out double hits
      return 1;
   }
   return 0;
}

function testHit(%client){
   if(isObject(%client)){
      %field = %client.lastExp;
      %data = getField(%field,0); %sPos = getField(%field,1); %ePos = getField(%field,2);
      if(%data.hasDamageRadius){
         %mask = $TypeMasks::PlayerObjectType;
         %vec = vectorNormalize(vectorSub(%ePos,%sPos));// some how this vector works
         %ray = containerRayCast(%ePos, VectorAdd(%ePos, VectorScale(VectorNormalize(%vec), 5)), %mask, %client.player);
         if(%ray &&  (!%client.lastDHit || (getSimTime() - %client.lastDHit > 128))){
            %client.lastDHit = getSimTime();
            //%dmgType = %data.radiusDamageType;
            return 1;
         }
      }
   }
   return 0;
}

function clientDmgStats(%data, %position, %sourceObject, %targetObject, %damageType, %amount){
   if(%damageType == $DamageType::Explosion || %damageType == $DamageType::Ground ||
         %damageType == $DamageType::OutOfBounds ||  %damageType == $DamageType::Lava ||
         %damageType == $DamageType::VehicleSpawn || %damageType == $DamageType::ForceFieldPowerup ||
         %damageType == $DamageType::Lightning  ||   %damageType == $DamageType::NexusCamping){
      if(isObject(%targetObject)){
         %targetObject.client.EVDamageType = %damageType;
         %targetObject.client.EVDamagetime = getSimTime();
      }
      if(getSimTime() - %targetClient.lastHitTime < 5000){
         %sourceClient = %targetClient.lastHitBy;
         if(rayTest(%targetObject, $dtStats::midAirHeight) && %damageType == $DamageType::Lightning)
            %sourceClient.dtStats.stat["lightningMAEVHits"]++;
         else
            %sourceClient.dtStats.stat["EVMAHit"]++;
      }
      return;
   }
//------------------------------------------------------------------------------
   if(%amount > 0 && %damageType > 0){
      if(isObject(%sourceObject)){
         %sourceClass = %sourceObject.getClassName();
         if(%sourceClass $= "Player"){
            %sourceClient = %sourceObject.client;
            %sourceClient.lastPlayer = %sourceClient.player;
            %sourceDT = %sourceClient.dtStats;
            %sv = mFloor(vectorLen(%sourceObject.getVelocity()) * 3.6);
         }
         else if(%sourceClass $= "Turret"){
            %sourceClient = %sourceObject.owner;
            if(!isObject(%sourceClient)){
               %sourceClient = %sourceObject.getControllingClient();
            }
            %sourceDT = %sourceClient.dtStats;
         }
         else if(%sourceClass $= "VehicleTurret" || %sourceClass $= "FlyingVehicle" || %sourceClass $= "HoverVehicle" || %sourceClass $= "WheeledVehicle"){
            %sourceClient = %sourceObject.getControllingClient();
            %sourceDT = %sourceClient.dtStats;
         }
      }
      if(isObject(%targetObject)){
         %targetClass  = %targetObject.getClassName();
         if(%targetClass $= "Player"){
            %targetClient = %targetObject.client;
            %targetClient.lastPlayer = %targetClient.player;//used for when some how client kill is out of order
            %targetDT = %targetClient.dtStats;
            %vv = mFloor(vectorLen(%targetObject.getVelocity()) * 3.6);
            if(%sourceClass $= "Player" && %sourceObject == %targetObject && %damageType == $DamageType::Disc){
               if(getSimtime() - %sourceClient.lastDiscJump < 256)
                  %sourceDT.stat["discJump"]++;
            }
            if(%sourceClass $= "Player" && %targetClient.team == %sourceClient.team && %sourceObject != %targetObject){
               %sourceDT.stat["friendlyFire"]++;
               if(getSimTime() - %sourceClient.stat["flareHit"] < 256){%sourceClient.flareSource.dtStats.stat["flareHit"]++;}
            }
            %ssc = (%sourceClass $= "VehicleTurret" || %sourceClass $= "FlyingVehicle" || %sourceClass $= "HoverVehicle" || %sourceClass $= "WheeledVehicle" || %sourceClass $= "Player" || %sourceClass $= "Turret");
            if(%ssc && %targetClient.team != %sourceClient.team && %sourceObject != %targetObject){
               if((getSimTime() - %sourceClient.lastExpTime) < 32){
                  %dis = vectorDist(getField(%sourceClient.lastExp,1),getField(%sourceClient.lastExp,2));
               }
               else{
                  %dis = vectorDist(%targetObject.getPosition(),%sourceObject.getPosition());
               }
               if(!%targetObject.combo[%sourceClient,%damageType]){
                  %targetObject.combo[%sourceClient,%damageType] = 1;
                  %sourceClient.player.combo[%targetObject]++;
               }

               if(!%targetObject.hitBy[%sourceClient]){
                  %sourceDT.stat["assist"]++;
                  %targetObject.hitBy[%sourceClient] = 1;
               }

               %targetClient.lastHitBy = %sourceClient;
               %targetClient.lastHitTime = getSimTime();

               if(%targetObject.isShielded && %damageType != $DamageType::Blaster){
                  %amount = %data.checkShields(%targetObject, %position, %amount, %damageType);
                  if(!%amount){
                     %targetDT.stat["shieldPackDmg"] += %amount;
                  }
               }


               if(%targetClient.EVDamageType && %targetClient.EVDamageType != %damageType && (getSimTime() - %targetClient.EVDamagetime) < 3000){ // they were hit by something befor they were killed
                  %sourceDT.stat["EVHitWep"]++;
                  if(rayTest(%targetObject, $dtStats::midAirHeight) && %damageType != $DamageType::Bullet){
                     if(%targetClient.EVDamageType == $DamageType::Lightning){
                        %sourceDT.stat["lightningMAHits"]++;
                        //%sourceClient.dtMessage("Lightning MidAir Hit","fx/Bonuses/down_perppass3_bunnybump.wav",0);
                     }
                     else
                        %sourceDT.stat["EVMAHit"]++;
                  }
                  if((getSimTime() - %targetClient.EVDamagetime) > 3000){
                     %targetClient.EVDamageType = 0;
                  }
               }


               if(%targetObject.isCloaked()){
                  %targetObject.isCloakTime = getSimTime();
               }

               //%dmgL = %targetObject.getDamageLocation(%position);
               %rayTest = rayTestDis(%targetObject);
               if(%rayTest >= $dtStats::midAirHeight && %damageType == $DamageType::Disc){
                  if(%sourceDT.stat["maHitDist"] < %dis){%sourceDT.stat["maHitDist"] = %dis;}
                  if(%sourceDT.stat["maHitHeight"] < %rayTest){%sourceDT.stat["maHitHeight"] = %rayTest;}
                  if(%sourceDT.stat["maHitSV"] < %sv){%sourceDT.stat["maHitSV"] = %sv;}
                  if(%sourceDT.stat["maHitVV"] < %vv){%sourceDT.stat["maHitVV"] = %vv;}
                  %targetClient.lastHitMA = 1;
               }
               else{
                  %targetClient.lastHitMA = 0;
               }
               switch$(%damageType){// list of all damage types to track see damageTypes.cs
                  case $DamageType::Blaster:
                     %sourceDT.stat["blasterDmg"] += %amount;
                     dtMinMax("blasterDmg", "wep", 1, %sourceDT.stat["blasterDmg"], %sourceClient);
                     %sourceDT.stat["blasterHits"]++;
                     %sourceDT.stat["blasterACC"] =  (%sourceDT.stat["blasterHits"] / (%sourceDT.stat["blasterShotsFired"] ? %sourceDT.stat["blasterShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["blasterHitDist"] < %dis){%sourceDT.stat["blasterHitDist"] = %dis;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%sourceDT.stat["blasterHitSV"] < %sv){%sourceDT.stat["blasterHitSV"]  = %sv;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.stat["blasterMAHitDist"] < %dis){%sourceDT.stat["blasterMAHitDist"] = %dis;}
                        %sourceDT.stat["blasterMA"]++;
                        dtMinMax("blasterMA", "ma", 1, %sourceDT.stat["blasterMA"], %sourceClient);
                        dtMinMax("blasterMAHitDist", "ma", 1, %sourceDT.stat["blasterMAHitDist"], %sourceClient);
                        dtMidAirMessage(%sourceClient,"Blaster", %dis, %sourceDT.stat["blasterMA"]);
                     }
                     if(getSimTime() - %sourceObject.client.blasterReflect < 256){%sourceDT.stat["blasterReflectHit"]++;}

                  case $DamageType::Plasma:
                     %sourceDT.stat["plasmaDmg"] += %amount;
                     dtMinMax("plasmaDmg", "wep", 1, %sourceDT.stat["plasmaDmg"], %sourceClient);
                     %directHit = testHit2(%sourceClient,%targetObject.client);
                     if(%directHit){%sourceDT.stat["plasmaHits"]++;%sourceDT.stat["plasmaDmgHits"]++;}
                     else{%sourceDT.stat["plasmaDmgHits"]++;}
                     %sourceDT.stat["plasmaACC"] = (%sourceDT.stat["plasmaHits"] / (%sourceDT.stat["plasmaShotsFired"] ? %sourceDT.stat["plasmaShotsFired"] : 1)) * 100;
                     %sourceDT.stat["plasmaDmgACC"] = (%sourceDT.stat["plasmaDmgHits"] / (%sourceDT.stat["plasmaShotsFired"] ? %sourceDT.stat["plasmaShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["plasmaHitDist"] < %dis){%sourceDT.stat["plasmaHitDist"] = %dis;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%sourceDT.stat["plasmaHitSV"] < %vv){%sourceDT.stat["plasmaHitSV"]  = %sv;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%directHit){
                           if(%sourceDT.stat["plasmaMAHitDist"] < %dis){%sourceDT.stat["plasmaMAHitDist"] = %dis;}
                           %sourceDT.stat["plasmaMA"]++;
                           dtMidAirMessage(%sourceClient,"Plasma Rifle", %dis, %sourceDT.stat["plasmaMA"]);
                           dtMinMax("plasmaMA", "ma", 1, %sourceDT.stat["plasmaMA"], %sourceClient);
                           dtMinMax("plasmaMAHitDist", "ma", 1, %sourceDT.stat["plasmaMAHitDist"], %sourceClient);
                        }
                     }
                  case $DamageType::Bullet:
                     %sourceDT.stat["cgDmg"] += %amount;
                     dtMinMax("cgDmg", "wep", 1, %sourceDT.stat["cgDmg"], %sourceClient);
                     %sourceDT.stat["cgHits"]++;

                     %sourceDT.stat["cgACC"] = (%sourceDT.stat["cgHits"] / (%sourceDT.stat["cgShotsFired"] ? %sourceDT.stat["cgShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["cgHitDist"] < %dis){%sourceDT.stat["cgHitDist"] = %dis;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%sourceDT.stat["cgHitSV"] < %sv){%sourceDT.stat["cgHitSV"]  = %sv;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.stat["cgMAHitDist"] < %dis){%sourceDT.stat["cgMAHitDist"] = %dis;}
                        %sourceDT.stat["cgMA"]++;
                     }
                  case $DamageType::Disc:
                     %sourceDT.stat["discDmg"] += %amount;
                     dtMinMax("discDmg", "wep", 1, %sourceDT.stat["discDmg"], %sourceClient);
                     %directHit = testHit2(%sourceClient,%targetObject.client);
                     if(%directHit){%sourceDT.stat["discHits"]++;%sourceDT.stat["discDmgHits"]++;}
                     else{%sourceDT.stat["discDmgHits"]++;}
                     %sourceDT.stat["discACC"] = (%sourceDT.stat["discHits"] / (%sourceDT.stat["discShotsFired"] ? %sourceDT.stat["discShotsFired"] : 1)) * 100;
                     %sourceDT.stat["discDmgACC"] = (%sourceDT.stat["discDmgHits"] / (%sourceDT.stat["discShotsFired"] ? %sourceDT.stat["discShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["discHitDist"] < %dis){%sourceDT.stat["discHitDist"] = %dis;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%sourceDT.stat["discHitSV"] < %sv){%sourceDT.stat["discHitSV"]  = %sv;}
                     %sourceClient.mdHit = 0;
                     if((getSimTime() - %targetClient.mdTime1) < 256){%sourceDT.stat["minePlusDisc"]++; %sourceClient.mdHit = 1;}
                     %targetClient.mdTime2 = getSimTime();
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%directHit){
                           if(%sourceDT.stat["discMAHitDist"] < %dis){%sourceDT.stat["discMAHitDist"] = %dis;}
                           %sourceDT.stat["discMA"]++;
                           dtMidAirMessage(%sourceClient,"Spinfusor", %dis, %sourceDT.stat["discMA"]);
                           dtMinMax("discMA", "ma", 1, %sourceDT.stat["discMA"], %sourceClient);
                           dtMinMax("discMAHitDist", "ma", 1, %sourceDT.stat["discMAHitDist"], %sourceClient);
                        }
                     }
                     if(getSimTime() - %sourceObject.client.discReflect < 256){%sourceDT.stat["discReflectHit"]++;}
                     dtMinMax("minePlusDisc", "wep", 1, %sourceDT.stat["minePlusDisc"], %sourceClient);
                  case $DamageType::Grenade:
                     if($dtObjExplode.dtNade){
                        %sourceDT.stat["hGrenadeDmg"] += %amount;
                        dtMinMax("hGrenadeDmg", "wep", 1, %sourceDT.stat["hGrenadeDmg"], %sourceClient);
                        %sourceDT.stat["hGrenadeHits"]++;
                        %sourceDT.stat["hGrenadeACC"] = (%sourceDT.stat["hGrenadeHits"] / (%sourceDT.stat["hGrenadeShotsFired"] ? %sourceDT.stat["hGrenadeShotsFired"] : 1)) * 100;
                        if(%sourceDT.stat["hGrenadeHitDist"] < %dis){%sourceDT.stat["hGrenadeHitDist"] = %dis;}
                        if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                        if(%sourceDT.stat["hGrenadeHitSV"] < %sv){%sourceDT.stat["hGrenadeHitSV"]  = %sv;}
                        if(%rayTest >= $dtStats::midAirHeight){
                           if(%sourceDT.stat["hGrenadeMAHitDist"] < %dis){%sourceDT.stat["hGrenadeMAHitDist"] = %dis;}
                           %sourceDT.stat["hGrenadeMA"]++;
                        }
                     }
                     else{
                        %sourceDT.stat["grenadeDmg"] += %amount;
                        dtMinMax("grenadeDmg", "wep", 1, %sourceDT.stat["grenadeDmg"], %sourceClient);
                        %directHit = testHit2(%sourceClient,%targetObject.client);
                        if(%directHit){%sourceDT.stat["grenadeHits"]++;%sourceDT.stat["grenadeDmgHits"]++;}
                        else{%sourceDT.stat["grenadeDmgHits"]++;}
                        %sourceDT.stat["grenadeACC"] = (%sourceDT.stat["grenadeHits"] / (%sourceDT.stat["grenadeShotsFired"] ? %sourceDT.stat["grenadeShotsFired"] : 1)) * 100;
                        %sourceDT.stat["grenadeDmgACC"] = (%sourceDT.stat["grenadeDmgHits"] / (%sourceDT.stat["grenadeShotsFired"] ? %sourceDT.stat["grenadeShotsFired"] : 1)) * 100;
                        if(%sourceDT.stat["grenadeHitDist"] < %dis){%sourceDT.stat["grenadeHitDist"] = %dis;}
                        if(%sourceDT.stat["grenadeHitSV"] < %sv){%sourceDT.stat["grenadeHitSV"]  = %sv;}
                        if(%rayTest >= $dtStats::midAirHeight){
                           if(%directHit){
                              if(%sourceDT.stat["grenadeMAHitDist"] < %dis){%sourceDT.stat["grenadeMAHitDist"] = %dis;}
                              %sourceDT.stat["grenadeMA"]++;
                              dtMidAirMessage(%sourceClient, "Grenade Launcher", %dis, %sourceDT.stat["grenadeMA"]);
                              dtMinMax("grenadeMA", "ma", 1, %sourceDT.stat["grenadeMA"], %sourceClient);
                              dtMinMax("grenadeMAHitDist", "ma", 1, %sourceDT.stat["grenadeMAHitDist"], %sourceClient);
                           }
                        }
                     }
                  case $DamageType::Laser:
                     if(%targetObject.getClassName() $= "Player"){
                        %damLoc = %targetObject.getDamageLocation(%position);
                        if(getWord(%damLoc,0) $= "head" && %sourceClient.team != %targetClient.team){
                           %sourceDT.stat["laserHeadShot"]++;
                           %sourceDT.lastHeadShotTime = getSimTime();
                           dtHeadShotMessage(%sourceClient, %dis);
                        }
                        else{
                           dtLaserShotMessage(%sourceClient, %dis);
                        }
                     }
                     %sourceDT.stat["laserDmg"] += %amount;
                     %sourceDT.stat["laserHits"]++;
                     %sourceDT.stat["laserACC"] = (%sourceDT.stat["laserHits"] / (%sourceDT.stat["laserShotsFired"] ? %sourceDT.stat["laserShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["laserHitDist"] < %dis){%sourceDT.stat["laserHitDist"] = %dis;}
                     if(%sourceDT.stat["laserHitSV"] < %sv){%sourceDT.stat["laserHitSV"]  = %sv;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.stat["laserMAHitDist"] < %dis){%sourceDT.stat["laserMAHitDist"] = %dis;}
                        %sourceDT.stat["laserMA"]++;
                     }
                     dtMinMax("laserHeadShot", "misc", 1, %sourceDT.stat["laserHeadShot"], %sourceClient);
                     dtMinMax("laserHitDist", "misc", 1, %sourceDT.stat["laserHitDist"], %sourceClient);
                     dtMinMax("laserDmg", "wep", 1, %sourceDT.stat["laserDmg"], %sourceClient);
                  case $DamageType::Mortar:
                     %sourceDT.stat["mortarDmg"] += %amount;
                     dtMinMax("mortarDmg","wep",  1, %sourceDT.stat["mortarDmg"], %sourceClient);
                     %directHit = testHit2(%sourceClient,%targetObject.client);
                     if(%directHit){%sourceDT.stat["mortarHits"]++;%sourceDT.stat["mortarDmgHits"]++;}
                     else{%sourceDT.stat["mortarDmgHits"]++;}
                     %sourceDT.stat["mortarACC"] = (%sourceDT.stat["mortarHits"] / (%sourceDT.stat["mortarShotsFired"] ? %sourceDT.stat["mortarShotsFired"] : 1)) * 100;
                     %sourceDT.stat["mortarDmgACC"] = (%sourceDT.stat["mortarDmgHits"] / (%sourceDT.stat["mortarShotsFired"] ? %sourceDT.stat["mortarShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["mortarHitDist"] < %dis){%sourceDT.stat["mortarHitDist"] = %dis;}
                     if(%sourceDT.stat["mortarHitSV"] < %sv){%sourceDT.stat["mortarHitSV"]  = %sv;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%directHit){
                           if(%sourceDT.stat["mortarMAHitDist"] < %dis){%sourceDT.stat["mortarMAHitDist"] = %dis;}
                           %sourceDT.stat["mortarMA"]++;
                           dtMidAirMessage(%sourceClient,"Fusion Mortar", %dis, %sourceDT.stat["mortarMA"]);
                           dtMinMax("mortarMA", "ma", 1, %sourceDT.stat["mortarMA"], %sourceClient);
                           dtMinMax("mortarMAHitDist", "ma", 1, %sourceDT.stat["mortarMAHitDist"], %sourceClient);
                        }
                     }
                  case $DamageType::Missile:
                     %sourceDT.stat["missileDmg"] += %amount;
                     dtMinMax("missileDmg", "wep", 1, %sourceDT.stat["missileDmg"], %sourceClient);
                     %sourceDT.stat["missileHits"]++;
                     %sourceDT.stat["missileACC"] = (%sourceDT.stat["missileHits"] / (%sourceDT.stat["missileShotsFired"] ? %sourceDT.stat["missileShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["missileHitDist"] < %dis){%sourceDT.stat["missileHitDist"] = %dis;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%sourceDT.stat["missileHitSV"] < %sv){%sourceDT.stat["missileHitSV"]  = %sv;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.stat["missileMAHitDist"] < %dis){%sourceDT.stat["missileMAHitDist"] = %dis;}
                        %sourceDT.stat["missileMA"]++;
                     }
                  case $DamageType::ShockLance:
                      %dot = vectorDot(%sourceObject.getForwardVector(), %targetObject.getForwardVector());
                     if(%dot >= mCos(1.05)){
                        %sourceDT.stat["shockRearShot"]++;
                         dtMinMax("shockRearShot", "misc", 1, %sourceDT.stat["shockRearShot"], %sourceClient);
                     }
                     %sourceDT.stat["shockDmg"] += %amount;
                     %sourceDT.stat["shockHits"]++;
                     %sourceDT.stat["shockACC"] = (%sourceDT.stat["shockHits"] / (%sourceDT.stat["shockShotsFired"] ? %sourceDT.stat["shockShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["shockHitDist"] < %dis){%sourceDT.stat["shockHitDist"] = %dis;}
                     if(%sourceDT.stat["weaponHitDist"] < %dis){%sourceDT.stat["weaponHitDist"] = %dis;}
                     if(%sourceDT.stat["shockHitSV"] < %sv){%sourceDT.stat["shockHitSV"]  = %sv;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.stat["shockMAHitDist"] < %dis){%sourceDT.stat["shockMAHitDist"] = %dis;}
                        %sourceDT.stat["shockMA"]++;
                        if(Game.class $= "ArenaGame"){
                           dtMidAirMessage(%sourceClient, "Shocklance", %dis, %sourceDT.stat["shockMA"]);
                        }
                     }
                     dtMinMax("shockDmg", "wep", 1, %sourceDT.stat["shockDmg"], %sourceClient);
                  case $DamageType::Mine:
                     %sourceDT.stat["mineDmg"] += %amount;
                     %sourceDT.stat["mineHits"]++;
                     %sourceDT.stat["mineACC"] = (%sourceDT.stat["mineHits"] / (%sourceDT.stat["mineShotsFired"] ? %sourceDT.stat["mineShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["mineHitDist"] < %dis){%sourceDT.stat["mineHitDist"] = %dis;}
                     if(%sourceDT.stat["mineHitVV"] < %vv){%sourceDT.stat["mineHitVV"]  = %vv;}
                     %sourceClient.mdHit = 0;
                     if((getSimTime() - %targetClient.mdTime2) < 256){%sourceDT.stat["minePlusDisc"]++; %sourceClient.mdHit = 1;}
                     %targetClient.mdTime1 = getSimTime();
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.stat["mineMAHitDist"] < %dis){%sourceDT.stat["mineMAHitDist"] = %dis;}
                        %sourceDT.stat["mineMA"]++;
                     }
                     dtMinMax("mineDmg", "wep", 1, %sourceDT.stat["mineDmg"], %sourceClient);
                     dtMinMax("minePlusDisc", "wep", 1, %sourceDT.stat["minePlusDisc"], %sourceClient);
                  case $DamageType::SatchelCharge:
                     %sourceDT.stat["satchelDmg"] += %amount;
                     %sourceDT.stat["satchelHits"]++;
                     %sourceDT.stat["satchelACC"] = (%sourceDT.stat["satchelHits"] / (%sourceDT.stat["satchelShotsFired"] ? %sourceDT.stat["satchelShotsFired"] : 1)) * 100;
                     if(%sourceDT.stat["satchelHitDist"] < %dis){%sourceDT.stat["satchelHitDist"] = %dis;}
                     if(%sourceDT.stat["satchelHitVV"] < %vv){%sourceDT.stat["satchelHitVV"]  = %vv;}
                     if(%rayTest >= $dtStats::midAirHeight){%sourceDT.stat["satchelMA"]++;}
                     dtMinMax("satchelDmg", "wep", 1, %sourceDT.stat["satchelDmg"], %sourceClient);
                  case $DamageType::Impact:
                     %sourceDT.stat["roadDmg"] += %amount;
                     dtMinMax("roadDmg", "wep", 1, %sourceDT.stat["roadDmg"], %sourceClient);
                  case $DamageType::IndoorDepTurret:
                     %sourceDT.stat["indoorDepTurretDmg"] += %amount;
                     dtMinMax("indoorDepTurretDmg", "wep", 1, %sourceDT.stat["indoorDepTurretDmg"], %sourceClient);
                  case $DamageType::OutdoorDepTurret:
                     %sourceDT.stat["outdoorDepTurretDmg"] += %amount;
                     dtMinMax("outdoorDepTurretDmg", "wep", 1, %sourceDT.stat["outdoorDepTurretDmg"], %sourceClient);
                  case $DamageType::TankMortar:
                     %sourceDT.stat["tankMortarDmg"] += %amount;
                     dtMinMax("tankMortarDmg", "wep", 1, %sourceDT.stat["tankMortarDmg"], %sourceClient);
                  case $DamageType::TankChaingun:
                     %sourceDT.stat["tankChaingunDmg"] += %amount;
                     dtMinMax("tankChaingunDmg", "wep", 1, %sourceDT.stat["tankChaingunDmg"], %sourceClient);
                  case $DamageType::BomberBombs:
                     %sourceDT.stat["bomberBombsDmg"] += %amount;
                     dtMinMax("bomberBombsDmg", "wep", 1, %sourceDT.stat["bomberBombsDmg"], %sourceClient);
                  case $DamageType::BellyTurret:
                     %sourceDT.stat["bellyTurretDmg"] += %amount;
                     dtMinMax("bellyTurretDmg", "wep", 1, %sourceDT.stat["bellyTurretDmg"], %sourceClient);
                  case $DamageType::ShrikeBlaster:
                     %sourceDT.stat["shrikeBlasterDmg"] += %amount;
                     dtMinMax("shrikeBlasterDmg", "wep", 1, %sourceDT.stat["shrikeBlasterDmg"], %sourceClient);
               }
            }
         }
      }
   }
}

function dtMidAirMessage(%client,%porjName,%distance, %count){
   if($dtStats::midAirMessage && Game.class !$= "LakRabbitGame" && !%client.isAIControlled()){
      bottomPrint(%client, "Midair" SPC %porjName SPC "(" @ %count @ ") Distance of " @ mFloor(%distance) @ " meters.", 4);
      messageClient(%client, 'MsgMidAir', '~wfx/misc/bounty_bonus.wav');
      if(%porjName !$= "Blaster"){
         messageTeamExcept(%client, 'MsgMidAir', '\c5%1 hit a mid air shot. [%2m, %3]', %client.name, mFloor(%distance), %porjName);
      }
      Game.recalcScore(%client);
   }
}

function dtHeadShotMessage(%client,%distance){
   if($dtStats::midAirMessage && Game.class !$= "LakRabbitGame" && !%client.isAIControlled()){
      bottomPrint(%client, "Headshot! Distance of " @ mFloor(%distance) @ " meters.", 4);
      messageClient(%client, 'MsgMidAir', '\c0Headshot distance of [%1m]~wfx/misc/bounty_bonus.wav', mFloor(%distance));
      //messageTeamExcept(%client, 'MsgMidAir', '\c5%1 hit a head shot. [%2m, %3]', %client.name, mFloor(%distance), %porjName);
   }
}

function dtLaserShotMessage(%client,%distance){
   if($dtStats::midAirMessage && Game.class !$= "LakRabbitGame" && !%client.isAIControlled()){
      bottomPrint(%client, "HIT! Distance is " @ mFloor(%distance) @ " meters.", 4);
      //messageTeamExcept(%client, 'MsgMidAir', '\c5%1 hit a head shot. [%2m, %3]', %client.name, mFloor(%distance), %porjName);
   }
}


function clientShotsFired(%data, %sourceObject, %projectile){ // could do a fov check to see if we are trying to aim at a player

   %dtStats = %sourceObject.client.dtStats;
   if(!isObject(%dtStats))
      return;
   if(%data.hasDamageRadius || %data $= "BasicShocker")
      %damageType =  %data.radiusDamageType;
   else
      %damageType = %data.directDamageType;

   %dtStats.stat["shotsFired"]++;
   %sourceClient.dtShotSpeed = %projectile.dtShotSpeed = mFloor(vectorLen(%sourceObject.getVelocity()) * 3.6);

   switch$(%damageType){// list of all damage types to track see damageTypes.cs
      case $DamageType::Bullet:
         %dtStats.stat["cgShotsFired"]++;
         %dtStats.stat["cgACC"] = (%dtStats.stat["cgHits"] / (%dtStats.stat["cgShotsFired"] ? %dtStats.stat["cgShotsFired"] : 1)) * 100;
      case $DamageType::Disc:
         //if(getSimTime() - %sourceClient.lastMineThrow < 5000)
            //%dtStats.mineDiscShots++;
         %dtStats.stat["discShotsFired"]++;
         %dtStats.stat["discACC"] = (%dtStats.stat["discHits"] / (%dtStats.stat["discShotsFired"] ? %dtStats.stat["discShotsFired"] : 1)) * 100;
         %dtStats.stat["discDmgACC"] = (%dtStats.stat["discDmgHits"] / (%dtStats.stat["discShotsFired"] ? %dtStats.stat["discShotsFired"] : 1)) * 100;
      case $DamageType::Grenade:
         %dtStats.stat["grenadeShotsFired"]++;
         %dtStats.stat["grenadeACC"] = (%dtStats.stat["grenadeHits"] / (%dtStats.stat["grenadeShotsFired"] ? %dtStats.stat["grenadeShotsFired"] : 1)) * 100;
         %dtStats.stat["grenadeDmgACC"] = (%dtStats.stat["grenadeDmgHits"] / (%dtStats.stat["grenadeShotsFired"] ? %dtStats.stat["grenadeShotsFired"] : 1)) * 100;
      case $DamageType::Laser:
         %dtStats.stat["laserShotsFired"]++;
         %dtStats.stat["laserACC"] = (%dtStats.stat["laserHits"] / (%dtStats.stat["laserShotsFired"] ? %dtStats.stat["laserShotsFired"] : 1)) * 100;
      case $DamageType::Mortar:
         %dtStats.stat["mortarShotsFired"]++;
         %dtStats.stat["mortarACC"] = (%dtStats.stat["mortarHits"] / (%dtStats.stat["mortarShotsFired"] ? %dtStats.stat["mortarShotsFired"] : 1)) * 100;
         %dtStats.stat["mortarDmgACC"] = (%dtStats.stat["mortarDmgHits"] / (%dtStats.stat["mortarShotsFired"] ? %dtStats.stat["mortarShotsFired"] : 1)) * 100;
      case $DamageType::Missile:
         projectileTracker(%projectile);
         %dtStats.stat["missileShotsFired"]++;
         %dtStats.stat["missileACC"] = (%dtStats.stat["missileHits"] / (%dtStats.stat["missileShotsFired"] ? %dtStats.stat["missileShotsFired"] : 1)) * 100;
      case $DamageType::ShockLance:
         %dtStats.stat["shockShotsFired"]++;
         %dtStats.stat["shockACC"] = (%dtStats.stat["shockHits"] / (%dtStats.stat["shockShotsFired"] ? %dtStats.stat["shockShotsFired"] : 1)) * 100;
      case $DamageType::Plasma:
         %dtStats.stat["plasmaShotsFired"]++;
         %dtStats.stat["plasmaACC"] = (%dtStats.stat["plasmaHits"] / (%dtStats.stat["plasmaShotsFired"] ? %dtStats.stat["plasmaShotsFired"] : 1)) * 100;
         %dtStats.stat["plasmaDmgACC"] = (%dtStats.stat["plasmaDmgHits"] / (%dtStats.stat["plasmaShotsFired"] ? %dtStats.stat["plasmaShotsFired"] : 1)) * 100;
      case $DamageType::Blaster:
         %dtStats.stat["blasterShotsFired"]++;
         %dtStats.stat["blasterACC"] = (%dtStats.stat["blasterHits"] / (%dtStats.stat["blasterShotsFired"] ? %dtStats.stat["blasterShotsFired"] : 1)) * 100;
      case $DamageType::ELF:
         %dtStats.stat["elfShotsFired"]++;
   }
}
////////////////////////////////////////////////////////////////////////////////
//								Menu Stuff									  //
////////////////////////////////////////////////////////////////////////////////

function getArmorBreakDown(%game,%dtStats){
   %avg[0] = getGameDataAvg(%game, %dtStats, "lArmorTimeTG");
   %avg[1] = getGameDataAvg(%game, %dtStats, "mArmorTimeTG");
   %avg[2] = getGameDataAvg(%game, %dtStats, "hArmorTimeTG");
   %armor[0] = "Light";
   %armor[1] = "Medium";
   %armor[2] = "Heavy";
   %total = %lavg + %mavg + %havg;
   %l = %max = 0;
      for(%x = 0; %x < 3; %x++){
         %total += %avg[%x];
         if(%avg[%x] > %max){
             %max = %avg[%x];
             %l = %x;
         }
      }
   return mFloor((%avg[0]/%total)*100) TAB  mFloor((%avg[1]/%total)*100) TAB  mFloor((%avg[2]/%total)*100) TAB  mFloor((%avg[%l]/%total)*100) TAB %armor[%l];
}

function getGameDataAvg(%game,%dtStats,%var){
   if(%dtStats.gameData[%game,$dtStats::tmMode] && %dtStats.gameStats["totalGames","g",%game,$dtStats::tmMode] != 0){
      %c = 0;
      %x = %dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode];
      for(%i=0; %i < 32; %i++){
         %v = %x - %i;
         if(%v < 0)
            %v = $dtStats::MaxNumOfGames + %v;
         %pct = getField(%dtStats.gameStats["gamePCT","g",%game,$dtStats::tmMode],%v);
         if(%pct > 90){
            %num = getField(%dtStats.gameStats[%var,"g",%game,$dtStats::tmMode],%v);
            if(%num){
               %val += %num;
               %c++;
               if(%c > 16)
                  break;
            }
         }
      }
      if(%c > 0)
         return numReduce(mCeil(%val / %c),1);
   }
   return 0;
}

function getGameData(%game,%client,%var,%type,%value){
   if(%type $= "game"){
      %total = getField(%client.dtStats.gameStats[%var,"g",%game,$dtStats::tmMode],%value);
      if(%total !$= "")
         return mFloatLength(%total,2) + 0;
      else
         error("Error getGameData" SPC %game SPC %client SPC %var SPC %type SPC %value);
   }
   else if(%type $= "total"){
      %total = getField(%client.dtStats.gameStats[%var,"t",%game,$dtStats::tmMode],%value);
      if(strpos(%total,"%a") != -1){
        %total = getField(strreplace(%total,"%a","\t"),0);
      }
      if(%total !$= "")
         return numReduce(%total,1);
      else
         error("Error getGameData" SPC %game SPC %client SPC %var SPC %type SPC %value);
   }
   else if(%type $= "avg"){
      if(%client.dtStats.gameStats["totalGames","g",%game,$dtStats::tmMode] != 0){
      %c = 0;
      %x = %client.dtStats.gameStats["statsOverWrite","g",%game,$dtStats::tmMode];
      for(%i=0; %i < 16; %i++){
         %v = %x - %i;
         if(%v < 0)
            %v = $dtStats::MaxNumOfGames + %v;
         %num = getField(%client.dtStats.gameStats[%var,"g",%game,$dtStats::tmMode],%v);
         if(%num $= ""){
            error("Error getGameData" SPC %game SPC %client SPC %var SPC %type SPC %value);
            break;
         }
         if(%num > 0 || %num < 0){
            %val += %num;
            %c++;
            if(%c >= %value)
               break;
         }
      }
      if(%c > 0)
         return numReduce(mCeil(%val / %c),1);
      }
   }
   return 0;
}

function numReduce(%num, %des) {
    if (%num $= "")
        return 0;

    if (strPos(%num, "x") != -1)
        return %num;

    %affixes = "KMGTPZY";
    %c = 0;

    while (%num > 1000 && %c < 7) {
        %num /= 1000;
        %c++;
    }

    %affix = %c > 0 ? getSubStr(%affixes, %c - 1, 1) : "";
    %formatted = mFloatLength(%num, %des) + 0;

    if (%c > 1 && %c < 10) {
        %seg = strReplace(getSubStr(%formatted, 0, strLen(%formatted) - 1), ".", "");
        %formatted = %seg $= "0" || %seg $= "00" ? getSubStr(%formatted, strLen(%formatted) - 1, 1) : %formatted;
    }

    return %formatted @ %affix;
}

function menuReset(%client){
   %client.viewMenu = 0;
   %client.viewClient = 0;
   %client.viewStats = 0;

   %client.lastPage = 0;
}
function clipStr(%str,%len){
   %slen = strLen(%str);
   if(%slen > %len){
      return getSubStr(%str,0,%len-2) @ "..";
   }
   return %str;
}

function  getTimeDayDelta(%d, %year){
   %dif = $dtStats::curYear - %year;
   %days += 365 * (%dif-1);
   %days += 366 - %d;
   %days += $dtStats::curDay;
   return %days;
}

function autoCompileStats(){
   if(!$Host::TournamentMode){
      if(!$dtStats::building){
         lStatsCycle(1, 1);
      }
      else{
         error("Stats Already Compiling");
      }
   }
}

function compileStats(){
    if(!$dtStats::building){
         lStatsCycle(1, 1);
    }
    else{
      error("Stats Already Compiling");
    }
}

function lStatsCycle(%build,%runReset){ // starts and manages the build/sort cycle
  if($dtStats::debugEchos){error("lStatsCycle" SPC $dtStats::build["day"] SPC $dtStats::build["week"] SPC
  $dtStats::build["month"] SPC $dtStats::build["quarter"] SPC $dtStats::build["year"] SPC $dtStats::build["custom"] SPC $dtStats::lCount);}
   if(%runReset){
      if(!$dtStats::statsSave){
         $dtStats::statReset = 1;
         dtStatClear();
         $dtStats::hostTimeLimit = $Host::TimeLimit;
         if(isGameRun()){//if for some reason the game is running extend the time limit untill done
            Game.voteChangeTimeLimit(1,$Host::TimeLimit+120);
            messageAll('MsgStats', '\c3Stats build started, adjusting time limit temporarily~wfx/misc/hunters_horde.wav');
            $dtStats::timeChange =1;
         }
      }
      else{
         schedule(5000,0,"lStatsCycle",1,1);//waiting on other stuff to finish
         return;
      }
   }
   if(%build){//reset
      if(!$dtStats::statsSave && !$dtStats::statReset){// make sure we are not inbetween missions and saveing
         $dtStats::build["day"] = 0;
         $dtStats::build["week"] = 0;
         $dtStats::build["month"] = 0;
         $dtStats::build["quarter"] = 0;
         $dtStats::build["year"] = 0;
         $dtStats::build["custom"] = 0;
         $dtStats::lCount = 0;
         $dtStats::building = 1;
         if(!$dtStats::timeChange){
            $dtStats::hostTimeLimit = $Host::TimeLimit;
            if(isGameRun()){//if for some reason the game is running extend the time limit untill done
               Game.voteChangeTimeLimit(1,$Host::TimeLimit+120);
               messageAll('MsgStats', '\c3Stats build started, adjusting time limit temporarily');
               $dtStats::timeChange =1;
            }
         }
      }
      else{
         schedule(5000,0,"lStatsCycle",1,0);//waiting on other stuff to finish
         return;
      }
   }
   if($dtStats::day > 0 && !$dtStats::build["day"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["day"] = 1; // mark as done
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"day");
   }
   else if($dtStats::week > 0 && !$dtStats::build["week"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["week"] = 1; // mark as done
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"week");
   }
   else if($dtStats::month > 0  && !$dtStats::build["month"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["month"] = 1; // mark as done
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"month");
   }
   else if($dtStats::quarter > 0 && !$dtStats::build["quarter"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["quarter"] = 1; // mark as done
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"quarter");
   }
   else if($dtStats::year > 0 && !$dtStats::build["year"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["year"] = 1; // mark as done
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"year");
   }
   else if($dtStats::custom > 0 && !$dtStats::build["custom"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["custom"] = 1; // mark as done
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"custom");
   }
   else{
       if($dtStats::debugEchos){error("leaderBoards finished building");}
      schedule(5000,0,"loadLeaderboards",1);// reset and reload leaderboards
      $dtServerVars::lastBuildTime = formattimestring("hh:nn:a mm-dd-yy");
      dtSaveServerVars();
      if(isObject(Game)){
         Game.voteChangeTimeLimit(1,$dtStats::hostTimeLimit);//put back to normal
         messageAll( 'MsgStats', '\c3Stats build complete, reverting time back to normal');
         $dtStats::timeChange = 0;
      }
   }
}

// only load one gameType/leaderboard at at time to reduce memory allocation
function preLoadStats(%game,%lType){ //queue up files for processing
  if($dtStats::debugEchos){error("preLoadStats queuing up files for" SPC %game SPC %lType);}
   %folderPath = "serverStats/stats/" @ %game @ "/*t.cs";
   %count = getFileCount(%folderPath);
   if(!%count){
      lStatsCycle(0,0);
   }
   if(!isObject(serverStats)){new SimGroup(serverStats);RootGroup.add(serverStats);}
   else{serverStats.delete(); new SimGroup(serverStats);RootGroup.add(serverStats);}
   for (%i = 0; %i < %count; %i++){
      %file = findNextfile(%folderPath);
      schedule(%i * 32, 0,"loadStatsData",%file,%game,%lType,%i,%count);
   }
}

function markNewDay(){// updates are dates when the server is ready to cycle over to a new day
   $dtStats::curDay = getDayNum();
   $dtStats::curWeek = getWeekNum();
   $dtStats::curMonth = getMonthNum();
   $dtStats::curQuarter = getQuarterNum();
   $dtStats::curYear = getYear();
   $dtStats::curCustom = $dtServerVars::custom > 1 ? $dtServerVars::custom : 1;
   if($dtStats::debugEchos){error("MarkNewDay =" SPC $dtStats::curDay SPC $dtStats::curWeek SPC $dtStats::curMonth SPC $dtStats::curQuarter SPC $dtStats::curYear SPC $dtStats::curCustom);}
}
// var old new old  new  old   new    old     new     old  new
// var day day week week month month  quarter quarter year year
// 0    1   2   3    4    5     6      7       8       9    10
function loadStatsData(%filepath,%game,%lType,%fileNum,%total){
   if($dtStats::debugEchos){error("loadStatsData" SPC %filePath SPC %fileNum SPC %total);}
   switch$(%lType){
      case "day":    %mon = $dtStats::curDay;     %fieldOld = 1;  %fieldNew = 2;
      case "week":   %mon = $dtStats::curWeek;    %fieldOld = 3;  %fieldNew = 4;
      case "month":  %mon = $dtStats::curMonth;   %fieldOld = 5;  %fieldNew = 6;
      case "quarter":%mon = $dtStats::curQuarter; %fieldOld = 7;  %fieldNew = 8;
      case "year":   %mon = $dtStats::curYear;    %fieldOld = 9;  %fieldNew = 10;
      case "custom": %mon = $dtStats::curCustom;    %fieldOld = 11;  %fieldNew = 12;
      default:       %mon = getMonthNum();   %fieldOld = 5;  %fieldNew = 6;
   }
   %file = new FileObject();
   RootGroup.add(%file);
   %file.OpenForRead(%filepath);
   %day = strreplace(%file.readline(),"%t","\t");
   if(getFieldCount(%day) >= 9) {
      %guid = getField(strreplace(getField(strreplace(%filepath,"/","\t"),3),"t","\t"),0);
      %gameCount = strreplace(%file.readline(),"%t","\t");
      %name = getField(strreplace(%file.readline(),"%t","\t"),1);
      %monOld = getField(%day,%fieldOld);
      %monNew = getField(%day,%fieldNew);// should allways be this one
      %found = -1;
      if(%monNew == %mon){%found = %fieldNew;}
      else if(%monold == %mon){%found = %fieldOld;}
      %gameCount = getField(%gameCount,%found);
      if(%found > -1 && %gameCount > $dtStats::minGame){
         %obj = new scriptObject();
         serverStats.add(%obj);
         %obj.name = %name;
         %obj.gameCount = getField(%gameCount,%found);
         %obj.guid =  %guid;
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var  = getField(%line,0);
            %obj.LStats[%var,%game] = getField(%line, %found);
         }
      }
   }
   %file.close();
   %file.delete();
   if(%fileNum >= %total-1){
      if(serverStats.getCount()){// make sure we have data to sort
         sortLStats(0,%game,%lType);
      }
      else{
         if($dtStats::debugEchos){error("No Valid Data For" SPC %lType SPC %mon);}
         lStatsCycle(0,0);
      }
   }
}

function  sortLStats(%c,%game,%lType){
   if($dtStats::debugEchos){error("sortLStats" SPC %c SPC %game SPC %lType);}
   %var = $statsVars::varNameType[%c,%game];
   %cat = $statsVars::varType[%var,%game];
   if(%cat !$= "Game"){
      %sortCount = 0;
      if(!isObject(LFData)){
         switch$(%lType){
            case "day":    %mon = $dtStats::curDay;
            case "week":   %mon = $dtStats::curWeek;
            case "month":  %mon = $dtStats::curMonth;
            case "quarter":%mon = $dtStats::curQuarter;
            case "year":   %mon = $dtStats::curYear;
            case "custom": %mon = $dtStats::curCustom;
            default:       error("ltype is not set"); return;
         }
         //%fc = getFileCount("serverStats/LData/-CTFGame*.cs");
         new FileObject(LFData);
         RootGroup.add(LFData);
         LFData.openForWrite("serverStats/lData/" @ "-" @ %game @ "-" @ %mon @ "-" @ $dtStats::curYear @ "-" @ %lType @"-.cs");
      }

      %n = %var @ "%tname";// name list
      %s = %var @ "%tdata"; // data list
      %g = %var @ "%tguid"; // data list
      %statsCount = serverStats.getCount();
      if(%cat $= "AvgI" || %cat $= "Min"){
         %invCount = 0;
         for (%i = 0; %i < %statsCount; %i++){//selection sort
            %maxCount = %i;
            for (%j = %i+1; %j < %statsCount; %j++){
               if(%cat $= "AvgI"){
                  %aVal = getField(strreplace(serverStats.getObject(%j).LStats[%var,%game],"%a","\t"),2) >= $dtStats::minAvg ? getField(strreplace(serverStats.getObject(%j).LStats[%var,%game],"%a","\t"),0) : 0;
                  %bVal = getField(strreplace(serverStats.getObject(%maxCount).LStats[%var,%game],"%a","\t"),2) >= $dtStats::minAvg ? getField(strreplace(serverStats.getObject(%maxCount).LStats[%var,%game],"%a","\t"),0) : 0;
                  if (%aVal < %bVal)
                     %maxCount = %j;
               }
               else{
                  if (serverStats.getObject(%j).LStats[%var,%game] < serverStats.getObject(%maxCount).LStats[%var,%game])
                     %maxCount = %j;
               }
            }
            %obj = serverStats.getObject(%maxCount);
            serverStats.bringToFront(%obj);// push the ones we have sorted to the front so we dont pass over them again
            if(%cat $= "AvgI")
               %num = getField(strreplace(%obj.LStats[%var,%game],"%a","\t"),2) >= $dtStats::minAvg ? getField(strreplace(%obj.LStats[%var,%game],"%a","\t"),0) : 0;
            else
               %num = %obj.LStats[%var,%game];
            if(%num != 0){
               %invCount++;
               %n = %n @ "%t" @ %obj.name;
               %s = %s @ "%t" @ %num;
               %g = %g @ "%t" @ %obj.guid;
            }
            if(%invCount >= $dtStats::topAmount){
                break;
            }
         }
         if(!%invCount){
            %n = %n @ "%t" @ "NA";
            %s = %s @ "%t" @ 0;
            %g = %g @ "%t" @ 0;
         }
      }
      else{
         %invCount = 0;
         for (%i = 0; %i < %statsCount && %i < $dtStats::topAmount; %i++){//selection sort
            %maxCount = %i;
            for (%j = %i+1; %j < %statsCount; %j++){
               if(%cat $= "Avg"){
                  %aVal = getField(strreplace(serverStats.getObject(%j).LStats[%var,%game],"%a","\t"),2) >= $dtStats::minAvg ? getField(strreplace(serverStats.getObject(%j).LStats[%var,%game],"%a","\t"),0) : 0;
                  %bVal = getField(strreplace(serverStats.getObject(%maxCount).LStats[%var,%game],"%a","\t"),2) >= $dtStats::minAvg ? getField(strreplace(serverStats.getObject(%maxCount).LStats[%var,%game],"%a","\t"),0) : 0;
                  if (%aVal > %bVal)
                     %maxCount = %j;
               }
               else{
                  if (xlCompare(serverStats.getObject(%j).LStats[%var,%game] , serverStats.getObject(%maxCount).LStats[%var,%game]) $= ">")
                     %maxCount = %j;
               }
            }            %obj = serverStats.getObject(%maxCount);
            serverStats.bringToFront(%obj);// push the ones we have sorted to the front so we dont pass over them again
            if(%cat $= "Avg")
               %num = getField(strreplace(%obj.LStats[%var,%game],"%a","\t"),2) >= $dtStats::minAvg ? getField(strreplace(%obj.LStats[%var,%game],"%a","\t"),0) : 0;
            else
               %num = %obj.LStats[%var,%game];

            if(%num != 0){
               %invCount++;
               %n = %n @ "%t" @ %obj.name;
               %s = %s @ "%t" @ %num;
               %g = %g @ "%t" @ %obj.guid;
            }
         }
         if(!%invCount){
            %n = %n @ "%t" @ "NA";
            %s = %s @ "%t" @ 0;
            %g = %g @ "%t" @ 0;
         }
      }
      LFData.writeLine(%n);
      LFData.writeLine(%s);
      LFData.writeLine(%g);
   }

   if(%c++ < $statsVars::count[%game]){
      schedule($dtStats::sortSpeed,0,"sortLStats",%c,%game,%lType);
   }
   else{
      LFData.close();
      LFData.delete();
      lStatsCycle(0,0); // kick off the next one
   }
}

function loadLeaderboards(%reset){ // loads up leaderboards
   if($dtStats::debugEchos){error("loadLeaderboards reset =" SPC %reset);}
   if(%reset){deleteVariables("$lData::*");}
   if(!$lData::load){$lData::load = 1;}
   else{return;}// exit  if we have all ready loaded
   markNewDay();//called when server starts and when build completes
   dtCleanUp(0);
   if(!isEventPending($dtStats::buildEvent))
      $dtStats::buildEvent = schedule(getTimeDif($dtStats::buildSetTime),0,"autoCompileStats");
   $dtStats::building = 0;
   if(isFile("serverStats/saveVars.cs"))
      exec("serverStats/saveVars.cs");
   %oldFileCount = 0;
   %file = new FileObject();
   RootGroup.add(%file);
   %folderPath = "serverStats/LData/*.cs";
   %count = getFileCount(%folderPath);
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%folderPath);
      %fieldPath =strreplace(%filePath,"-","\t");
      %game = getField(%fieldPath,1);
      %mon = getField(%fieldPath,2); // 0 path / 1  game / 2 mon / 3 year / 4 type / 5 .cs
      %year = getField(%fieldPath,3);
      %lType = getField(%fieldPath,4);
      //echo(isFileExpired(%lType,%mon,%year) SPC %lType SPC %mon SPC %year);
      if(!isFileExpired(%lType,%mon,%year)){
         $lData::mon[%lType, %game, $lData::monCount[%game,%lType]++] = %mon TAB %year;
         if(!$lData::hasData[%lType,%game]){
           %sortArray[%sortCount++] = %lType TAB %game;
         }
         $lData::hasData[%lType,%game] = 1;
         %file.OpenForRead(%filepath);
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var  = getField(%line,0);
            %stack  = getField(%line,1);
            if(%stack $= "name"){
               %name = getFields(%line,2,getFieldCount(%line)-1);
               $lData::name[%var,%game,%lType,%mon,%year] = %name;

            }
            else if(%stack $= "data"){
               %data = getFields(%line,2,getFieldCount(%line)-1);
               $lData::data[%var,%game,%lType,%mon,%year] = %data;
            }
            else if(%stack $= "guid"){
               %guid = getFields(%line,2,getFieldCount(%line)-1);
               $lData::guid[%var,%game,%lType,%mon,%year] = %guid;
            }
         }
         %file.close();
      }
      else{// not valid any more delete;
         if($dtStats::fm){
            if($dtStats::debugEchos){error("Deleting old file" SPC %filepath);}
            schedule((%i+1)  * 256,0,"deleteFile",%filepath);
         }
         else{
             %oldFileCount++;
         }
      }
   }
   %file.close();
   %file.delete();
   error("Found" SPC %oldFileCount SPC "Expired Leaderboard Files");
   if(%sortCount > 1){// sorts what the data we loaded by date as windows vs linux will return diffrent file orders
      for(%i = 1; %i <= %sortCount; %i++){
         sortMon(getField(%sortArray[%i],0),getField(%sortArray[%i],1));
      }
   }
}schedule(5000,0,"loadLeaderboards",0);// delay this so supporting functions are exec first

function sortMon(%lType, %game) {
   %n = $lData::monCount[%game, %lType];
   if (%n > 1) { // Ensure we have enough elements worth sorting
      for (%i = 1; %i <= %n - 1; %i++) {
         %m = %i;
         for (%j = %i + 1; %j <= %n; %j++) {
            // Compare year first, then month if years are equal
            %year1 = getField($lData::mon[%lType, %game, %j], 1);
            %year2 = getField($lData::mon[%lType, %game, %m], 1);
            %month1 = getField($lData::mon[%lType, %game, %j], 0);
            %month2 = getField($lData::mon[%lType, %game, %m], 0);

            if ((%year1 > %year2) || (%year1 == %year2 && %month1 > %month2)) {
               %m = %j;
            }
         }
         // Swap the elements
         %low = $lData::mon[%lType, %game, %m];
         %high = $lData::mon[%lType, %game, %i];
         $lData::mon[%lType, %game, %m] = %high;
         $lData::mon[%lType, %game, %i] = %low;
      }
   }
   // Debug
   for (%i = 1; %i <= %n; %i++) {
      echo($lData::mon[%lType, %game, %i] SPC %game);
   }
}

function dtCleanUp(%force){
   %filename = "serverStats/stats/*t.cs";
   %count = getFileCount(%filename);
   %file = new FileObject();
   RootGroup.add(%file);
   %oldFileCount = 0;
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%filename);
      %file.OpenForRead(%filepath);
      %game  = getField(strreplace(%filePath,"/","\t"),2);
      %dateLine = strreplace(%file.readline(),"%t","\t");
      %gameCountLine = strreplace(%file.readline(),"%t","\t");
      %day = getField(%dateLine,2);
      %year = getField(%dateLine,10);
      %file.close();
      //%d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1;
      %dayCount = isFileExpired("getCount",%day,%year);
      if(%dayCount > $dtStats::expireMin){
         %gcCM = getField(%gameCountLine,6);
         %gcPM = getField(%gameCountLine,5);
         %gc =  (%gcCM > %gcPM) ? %gcCM : %gcPM;
         %extraDays = mCeil((%gc * $dtStats::expireFactor[%game]));
         //error(%extraDays SPC %dayCount);
         if(%dayCount > %extraDays || %dayCount > $dtStats::expireMax){
            if($dtStats::fm || %force){
               if($dtStats::debugEchos){error("Deleting old file" SPC %dayCount SPC %extraDays SPC %filepath);}
               if(isFile(%filepath)){
                  schedule(%v++ * 256,0,"deleteFile",%filepath);
                  %oldFileCount++;
               }
               %gPath = strreplace(%filepath,"t.cs","g.cs");
               if(isFile(%gPath)){
                  schedule(%v++ * 256,0,"deleteFile",%gPath);
                  %oldFileCount++;
               }
            }
            else{
               %oldFileCount++;
            }
         }
      }
   }
   if($dtStats::fm || %force){
      error("Found" SPC %oldFileCount SPC "Expired Player Files");
   }
   else{
      error("Found" SPC %oldFileCount SPC "Expired Player Files, Type dtCleanUp(1) to force clean and delete");
   }
   %file.delete();
}

function  getTimeDelta(%dateTime){
  //banDateTime = "05\t01\t2024\t13\t00";
   %d = getWord(%dateTime,0); %m = getWord(%dateTime,1); %y = getWord(%dateTime,2);
   %h = getWord(%dateTime,3); %n = getWord(%dateTime,4);

   %curDD = formattimestring("dd");%curMM = formattimestring("mm");%curYY = formattimestring("yy");
   %dcA = %dcB = 0;

   %days[2] = (%y % 4 == 0) ? "29" : "28";
   %days[1] = "31";%days[3] = "31"; %days[4] = "30";
   %days[5] = "31"; %days[6] = "30"; %days[7] = "31";
   %days[8] = "31"; %days[9] = "30"; %days[10] = "31";
   %days[11] = "30"; %days[12] = "31";

   for(%i = 1; %i <= %m-1; %i++){
      %dcA += %days[%i];
   }

   %dcA += %d;

   %days[2] = (%curYY % 4 == 0) ? "29" : "28";
   for(%i = 1; %i <= %curMM-1; %i++){
      %dcB += %days[%i];
   }
   %dcB += %curDD;

   %dif = formattimestring("yy") - %y;
   %tDays += 365 * (%dif-1);
   %tDays += 365 - %dcA;
   %tDays += %dcB;
   %ht = %nt = 0;
   if(formattimestring("H") > %h){
      %ht = formattimestring("H") - %h;
   }
   else if(formattimestring("H") < %h){
      %ht = 24 - %h;
      %ht = formattimestring("H")+ %ht;
   }
   if(formattimestring("n") > %n){
      %nt = formattimestring("n") - %n;
   }
   else if(formattimestring("n") < %n){
      %nt = 60 - %n;
      %nt = formattimestring("n") + %nt;
   }
   %totalTime = mfloor((%tDays * 1440) +  (%ht*60) + %nt);
   %totalTime = (%totalTime >= 0) ? %totalTime : 0;
   return %totalTime;
}

function dtMarkDate(){
   return formattimestring("dd") SPC formattimestring("mm") SPC formattimestring("yy") SPC formattimestring("H")SPC formattimestring("nn");
}

function isFileExpired(%lType,%d,%year){
   switch$(%lType){
      case "expire":
         if($dtStats::expireMax > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            if(%days > $dtStats::expireMax){
               return 1;
            }
            else{
               return 0;
            }
         }
         else{
            return 1;
         }
      case "getCount":
         if($dtStats::expireMax > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            return %days;
         }
         else{
            return -1;
         }
      case "mapData":
         %dif = $dtStats::curYear - %year;
         %days += 12 * (%dif-1);
         %days += 13 - %d;
         %days += $dtStats::curMonth;
         //error(%days);
         if(%days > 2){
            return 1;
         }
         else{
            return 0;
         }
      case "day":
         if($dtStats::day > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            if(%days > $dtStats::day){
               return 1;
            }
            else{
               return 0;
            }
         }
         else{
            return 1;
         }
      case "week":
         if($dtStats::week > 1){
            %dif = $dtStats::curYear - %year;
            %days += 53 * (%dif-1);
            %days += 54 - %d;
            %days += $dtStats::curWeek;
               if(%days > $dtStats::week){
                  return 1;
               }
               else{
                  return 0;
               }
            }
            else{
               return 1;
            }
      case "month":
         if($dtStats::month > 1){
            %dif = $dtStats::curYear - %year;
            %days += 12 * (%dif-1);
            %days += 13 - %d;
            %days += $dtStats::curMonth;
            //error(%days);
            if(%days > $dtStats::month){
               return 1;
            }
            else{
               return 0;
            }
         }
         else{
            return 1;
         }
      case "quarter":
         if($dtStats::quarter > 1){
            %dif = $dtStats::curYear - %year;
            %days += 4 * (%dif-1);
            %days += 5 - %d;
            %days += $dtStats::curQuarter;
            if(%days > $dtStats::quarter){
               return 1;
            }
            else{
               return 0;
            }
         }
         else{
            return 1;
         }
      case "year":
         %mon = $dtStats::curYear - %d;
         if(%mon <= $dtStats::year){
            return 0;
         }
      case "custom":
         return 0;
   }
   return 1;
}

function dayDelta(%d, %year){
   %dif = $dtStats::curYear - %year;
   %days += 365 * (%dif-1);
   %days += 366 - %d;
   %days += $dtStats::curDay;
   return %days;
}

function dtStatClear(){
   %fc = 0;
   for(%g = 0; %g < $dtStats::gameTypeCount; %g++){
      %game = $dtStats::gameType[%g];
      for(%q = 0; %q < $statsVars::count[%game]; %q++){
         %varNameType = $statsVars::varNameType[%q,%game];
         if($dtStats::resetList[%varNameType]){
            %fc++;
            %stats = (%fc == 1) ? %varNameType : %stats TAB %varNameType;
            $dtStats::resetList[%varNameType] = 0;
         }
      }
   }
   if(!%fc){
      $dtStats::statReset = 0;
      return 0;
   }
   %file = new FileObject();
   RootGroup.add(%file);
   %time = 64;
   %tcount = getFileCount("serverStats/stats/*t.cs");
   %c = -1;
   for (%i = 0; %i < %tcount; %i++){
      %filepath = findNextfile("serverStats/stats/*t.cs");
      schedule(%c++ *%time, 0, "clearStatFile", %file, %filepath, %i, %tcount, %stats,"t");
   }
   %gcount = getFileCount("serverStats/stats/*g.cs");
   for (%i = 0; %i < %gcount; %i++){
      %filepath = findNextfile("serverStats/stats/*g.cs");
      schedule(%c++ * %time, 0, "clearStatFile", %file, %filepath, %i, %gcount, %stats,"g");
   }
   %file.schedule(%c++ * %time, "delete");
   schedule(%c++ * %time, 0, "doneStatClear");
}

function doneStatClear(){
   $dtStats::statReset = 0;
}

function clearStatFile(%file,%filepath,%i,%count, %stats,%type){
   error("Clearing Stats" SPC %i SPC %count-1 SPC %filepath);
   %file.OpenForRead(%filepath);
   %lc = -1;
   %found  = 0;
   while( !%file.isEOF() ){// load the rest of the file
      %line = %file.readline();
      %field = strreplace(%line,"%t","\t");
      %safe = 1;
      for(%f = 0; %f < getFieldCount(%stats); %f++){
         if(getField(%field,0) $= getField(%stats,%f)){
            %safe = 0;
            %found = 1;
            break;
         }
      }
      if(%safe)
         %line[%lc++] = %line;
      else{
         %line[%lc++] = strreplace(getField(%field,0) TAB $dtStats::blank[%type], "\t", "%t");
      }
   }
   %file.close();
   if(%found){
      %file.OpenForWrite(%filepath);
      %lc++;
      for (%x = 0; %x < %lc; %x++){
         %file.writeLine(%line[%x]);
      }
      %file.close();
   }
}

function buildTest(%mode){
   if(!$dtStats::tmCompile){
      %list = !%mode ? pubList : pugList;
      for(%i = 0; %i < %list.getCount(); %i++){
         %gameType = %list.getObject(%i);
         $dtStats::pugCount[%gameType.game] = 0;
         for(%x = 0; %x < %gameType.getCount(); %x++){
            %game = %gameType.getObject(%x);
            if(%game.select){
               %count = $dtStats::pugCount[%gameType.game];
               $dtStats::pugIDS[%gameType.game,%count] = %game.pugID;
               $dtStats::pugMap[%gameType.game,%count] = %game.mapName;
               $dtStats::pugDate[%gameType.game,%count]  = %game.date;
               $dtStats::pugFS[%gameType.game,%count] = %game.teamOne SPC %game.teamTwo;
               $dtStats::pugCount[%gameType.game]++;
               error( $dtStats::pugCount[%gameType.game]);
            }
         }
      }
      $dtStats::path  = (!%mode) ? "stats" : "statsTM";
      preLoadTurStats(0);
   }
}

function preLoadTurStats(%gameIndex){ //queue up files for processing
   if(!%gameIndex){
      $dtGameIndex = 0;
      $dtStats::tmCompile = 1;
   }
   if(%gameIndex < $dtStats::gameTypeCount){
      %game = $dtStats::gameType[$dtGameIndex];
      if($dtStats::debugEchos){error("preLoadTurStats queuing up files for" SPC %game SPC $dtStats::pugCount[%game]);}
      if($dtStats::pugCount[%game] > 0){
         %folderPath = "serverStats/" @ $dtStats::path  @ "/" @ %game @ "/*g.cs";
         %total = getFileCount(%folderPath);
         if(!%total){
            return;
         }
         if(!isObject(serverStats)){new SimGroup(serverStats);RootGroup.add(serverStats);}
         else{serverStats.delete(); new SimGroup(serverStats);RootGroup.add(serverStats);}
         for (%i = 0; %i < %total; %i++){
            %file = findNextfile(%folderPath);
            schedule(%i * 32, 0,"loadTurStatsData",%file,%game,%i,%total);
         }
      }
      else{
         preLoadTurStats($dtGameIndex++);
      }
   }
   else{
      dtSaveServerVars();
      compileGameImage(-1);
   }
}

function loadTurStatsData(%file,%game,%fileNum,%total){
   if($dtStats::debugEchos){error("loadTurStatsData" SPC %file SPC %fileNum SPC %total);}
   %fObj = new FileObject();
   RootGroup.add(%fObj);
   %fObj.OpenForRead(%file);
   %guid = getField(strreplace(getField(strreplace(%file,"/","\t"),3),"g","\t"),0);

   %playerName = getField(strreplace(%fObj.readline(),"%t","\t"),1);//1
   %statsOverWrite = getField(strreplace(%fObj.readline(),"%t","\t"),1);//2
   %totalGames = getField(strreplace(%fObj.readline(),"%t","\t"),1);//3
   %fullSet = getField(strreplace(%fObj.readline(),"%t","\t"),1);//4
   %dayStamp = %fObj.readline();//5
   %weekStamp = %fObj.readline();//6
   %monthStamp = %fObj.readline();//7
   %quarterStamp = %fObj.readline();//8
   %yearStamp = %fObj.readline();//9
   %dateStamp = %fObj.readline();//10
   %timeDayMonth = %fObj.readline();//11
   %mapName = %fObj.readline();//12
   %mapID = %fObj.readline();//13
   %mapGameID = %fObj.readline();//14
   %gameIDLine = strreplace(%fObj.readline(),"%t","\t");//15
   //%gamePCT = %fObj.readline();//16
   //%versionNum = %fObj.readline();//17
   //%dtTeamGame = strreplace(%fObj.readline(),"%t","\t");//18

   %found = 0;
   for(%x = 0; %x < $dtStats::pugCount[%game]; %x++){
      for(%i = 0; %i < getFieldCount(%gameIDLine); %i++){
         %gid = getField(%gameIDLine,%i);
         if(%gid $= $dtStats::pugIDS[%game,%x]){
            %gameList[%x] = %i;
            %found = 1;
            break;
         }
         else{
            %gameList[%x] = -1;
         }
      }
   }

   %gListCount = 0;
   if(%found){// only if we found a matching ID
      %obj = new scriptObject();
      serverStats.add(%obj);
      %obj.name = %playerName;
      %obj.guid =  %guid;
      while( !%fObj.isEOF() ){
         %line = strreplace(%fObj.readline(),"%t","\t");
         %var  = getField(%line,0);
         %cat = $statsVars::varType[%var,%game];
         %gListCount = 0;
         for(%x = 0; %x < $dtStats::pugCount[%game]; %x++){
            %gameIndex = %gameList[%x];
            if(%gameIndex != -1){
               %gListCount++;
               %obj.LStats[%var] = setField(%obj.LStats[%var], %x, getField(%line,%gameIndex));
               switch$(%cat){
                  case "TG"://ttl is not used in game stats
                     %obj.LStatsT[%var] += getField(%line,%gameList[%x]);
                  case "Max":
                     if(getField(%line,%gameList[%x]) > %obj.LStatsT[%var] || %gListCount == 0)
                        %obj.LStatsT[%var] = getField(%line,%gameList[%x]);
                  case "Min":
                     if(getField(%line,%gameList[%x]) < %obj.LStatsT[%var] || %gListCount == 0)
                        %obj.LStatsT[%var] = getField(%line,%gameList[%x]);
                  case "Avg" or "AvgI":
                        %temp[%var] += getField(%line,%gameList[%x]);
                        %obj.LStatsT[%var] = %temp[%var] / %gListCount;
               }
            }
            else{
               %obj.LStats[%var] = setField(%obj.LStats[%var],%x, 0);
            }
         }
      }
   }

   %fObj.close();
   %fObj.delete();
   if(%fileNum >= %total-1){
      if(serverStats.getCount()){// make sure we have data to sort
         sortTurStats(0,0,%game);
      }
      else{
         if($dtStats::debugEchos){error("No Valid Data For" SPC %game SPC %map);}
          preLoadTurStats($dtGameIndex++);
      }
   }
}

function sortTurStats(%c, %gameIndex, %game){
   if($dtStats::debugEchos){error("sortTurStats" SPC %c SPC %gameIndex SPC %game);}
   if(!isObject(LFData)){
      new FileObject(LFData);
      RootGroup.add(LFData);
      LFData.openForWrite("serverStats/gmData/" @ cleanMapName($dtStats::pugMap[%game, %gameIndex]) @ "-" @ %game @ "-" @ $dtStats::pugIDS[%game, %gameIndex] @ "-G.cs");
      LFData.writeLine($dtStats::pugMap[%game,%gameIndex] @ "%t" @ %game @ "%t" @ $dtStats::pugIDS[%game,%gameIndex] @ "%t" @ $dtStats::pugDate[%game, %gameIndex] @ "%t" @ $dtStats::pugFS[%game,%gameIndex]);

      // build out header
      %var = "scoreTG";
      %len = serverStats.getCount();
      for (%i = 0; %i < %len - 1; %i++) {
         for (%j = 0; %j < %len - %i - 1; %j++) {
            // If the current element is less than the next element, bring the next element to the front
            %aObj = serverStats.getObject(%j);
            %bObj = serverStats.getObject(%j + 1);
            %A = getField(%aObj.LStats[%var],%gameIndex);
            %B = getField(%bObj.LStats[%var],%gameIndex);
            if (%A < %B) {
               serverStats.bringToFront(%bObj);
            }
         }
      }

      %teamOneNameLine[0] = 1 TAB "name";
      %teamOneDataLine[1] = 1 TAB "score";
      %teamOneDataLine[2] = 1 TAB "off";
      %teamOneDataLine[3] = 1 TAB "def";
      %teamOneDataLine[4] = 1 TAB "kills";
      %teamOneDataLine[5] = 1 TAB "caps";

      %teamTwoNameLine[0] = 2 TAB "name";
      %teamTwoDataLine[1] = 2 TAB "score";
      %teamTwoDataLine[2] = 2 TAB "off";
      %teamTwoDataLine[3] = 2 TAB "def";
      %teamTwoDataLine[4] = 2 TAB "kills";
      %teamTwoDataLine[5] = 2 TAB "caps";

      for (%i = 0; %i < %len; %i++) {
         %sObj = serverStats.getObject(%i);
         %team = getField(%sObj.LStats["dtTeamGame"], %gameIndex);
         if(%team == 1){
            %teamOneNameLine[0] =  %teamOneNameLine[0]  TAB %sObj.name;
            %teamOneDataLine[1] =  %teamOneDataLine[1]  TAB  getField(%sObj.LStats[%var], %gameIndex);
            %teamOneDataLine[2] =  %teamOneDataLine[2]  TAB  getField(%sObj.LStats["offenseScoreTG"], %gameIndex);
            %teamOneDataLine[3] =  %teamOneDataLine[3]  TAB  getField(%sObj.LStats["defenseScoreTG"], %gameIndex);
            %teamOneDataLine[4] =  %teamOneDataLine[4]  TAB  getField(%sObj.LStats["killsTG"], %gameIndex);
            %teamOneDataLine[5] =  %teamOneDataLine[5]  TAB  getField(%sObj.LStats["flagCapsTG"], %gameIndex);
         }
         else if(%team == 2){
            %teamTwoNameLine[0] =  %teamTwoNameLine[0]  TAB %sObj.name;
            %teamTwoDataLine[1] =  %teamTwoDataLine[1]  TAB  getField(%sObj.LStats[%var], %gameIndex);
            %teamTwoDataLine[2] =  %teamTwoDataLine[2]  TAB  getField(%sObj.LStats["offenseScoreTG"], %gameIndex);
            %teamTwoDataLine[3] =  %teamTwoDataLine[3]  TAB  getField(%sObj.LStats["defenseScoreTG"], %gameIndex);
            %teamTwoDataLine[4] =  %teamTwoDataLine[4]  TAB  getField(%sObj.LStats["killsTG"], %gameIndex);
            %teamTwoDataLine[5] =  %teamTwoDataLine[5]  TAB  getField(%sObj.LStats["flagCapsTG"], %gameIndex);
         }
      }
      LFData.writeLine(strreplace(%teamOneNameLine[0],"\t","%t"));
      LFData.writeLine(strreplace(%teamOneDataLine[1],"\t","%t"));
      LFData.writeLine(strreplace(%teamOneDataLine[2],"\t","%t"));
      LFData.writeLine(strreplace(%teamOneDataLine[3],"\t","%t"));
      LFData.writeLine(strreplace(%teamOneDataLine[4],"\t","%t"));
      //LFData.writeLine(strreplace(%teamOneDataLine[5],"\t","%t"));

      LFData.writeLine(strreplace(%teamTwoNameLine[0],"\t","%t"));
      LFData.writeLine(strreplace(%teamTwoDataLine[1],"\t","%t"));
      LFData.writeLine(strreplace(%teamTwoDataLine[2],"\t","%t"));
      LFData.writeLine(strreplace(%teamTwoDataLine[3],"\t","%t"));
      LFData.writeLine(strreplace(%teamTwoDataLine[4],"\t","%t"));
      //LFData.writeLine(strreplace(%teamTwoDataLine[5],"\t","%t"));
   }

   %var = $statsVars::varNameType[%c,%game];
   %cat = $statsVars::varType[%var,%game];
   if(%cat !$= "Game"){
      if(%cat $= "AvgI" || %cat $= "Min"){
         %len = serverStats.getCount();
         for (%i = 0; %i < %len - 1; %i++) {
            for (%j = 0; %j < %len - %i - 1; %j++) {
               // If the current element is less than the next element, bring the next element to the front
               %aObj = serverStats.getObject(%j);
               %bObj = serverStats.getObject(%j + 1);
               %A = getField(%aObj.LStats[%var],%gameIndex);
               %B = getField(%bObj.LStats[%var],%gameIndex);
               if (%A > %B) {
                  serverStats.bringToFront(%bObj);
               }
            }
         }
      }
      else{
         %len = serverStats.getCount();
         for (%i = 0; %i < %len - 1; %i++) {
            for (%j = 0; %j < %len - %i - 1; %j++) {
               // If the current element is less than the next element, bring the next element to the front
               %aObj = serverStats.getObject(%j);
               %bObj = serverStats.getObject(%j + 1);
               %A = getField(%aObj.LStats[%var],%gameIndex);
               %B = getField(%bObj.LStats[%var],%gameIndex);
               if (%A < %B) {
                  serverStats.bringToFront(%bObj);
               }
            }
         }
      }


      %teamOneNameLine = 1 TAB "name" TAB %var;
      %teamOneDataLine = 1 TAB "data" TAB %var;

      %teamTwoNameLine = 2 TAB "name" TAB %var;
      %teamTwoDataLine = 2 TAB "data" TAB %var;

      %teamAllNameLine = 0 TAB "name" TAB %var;
      %teamAllDataLine = 0 TAB "data" TAB %var;
      %teamAllTeamLine = 0 TAB "team" TAB %var;

      %write0 = 0;
      %write1 = 0;
      %write2 = 0;
      for (%i = 0; %i < %len; %i++) {
         %sObj = serverStats.getObject(%i);
         %team = getField(%sObj.LStats["dtTeamGame"], %gameIndex);
         if(%team == 1){
            %fv = getField(%sObj.LStats[%var], %gameIndex);
            if(%fv != 0){
               %teamOneNameLine =  %teamOneNameLine TAB %sObj.name;
               %teamOneDataLine =  %teamOneDataLine  TAB  %fv;
               %write1 = 1;
            }
         }
         else if(%team == 2){
            %fv = getField(%sObj.LStats[%var], %gameIndex);
            if(%fv != 0){
               %teamTwoNameLine =  %teamTwoNameLine TAB %sObj.name;
               %teamTwoDataLine =  %teamTwoDataLine  TAB   %fv;
               %write2 = 1;
            }
         }
         %fv = getField(%sObj.LStats[%var], %gameIndex);
         if(%fv != 0){
            %teamAllTeamLine = %teamAllTeamLine TAB %team;
            %teamAllNameLine =  %teamAllNameLine TAB %sObj.name;
            %teamAllDataLine =  %teamAllDataLine  TAB   %fv;
            %write0 = 1;
         }
      }
      if(%write1){
         LFData.writeLine(strreplace(%teamOneNameLine,"\t","%t"));
         LFData.writeLine(strreplace(%teamOneDataLine,"\t","%t"));
      }
      if(%write2){
         LFData.writeLine(strreplace(%teamTwoNameLine,"\t","%t"));
         LFData.writeLine(strreplace(%teamTwoDataLine,"\t","%t"));
      }
      if(%write0){
         LFData.writeLine(strreplace(%teamAllTeamLine,"\t","%t"));
         LFData.writeLine(strreplace(%teamAllNameLine,"\t","%t"));
         LFData.writeLine(strreplace(%teamAllDataLine,"\t","%t"));
      }
   }
   if(%c++ < $statsVars::count[%game])
      schedule($dtStats::sortSpeed,0,"sortTurStats",%c,%gameIndex,%game);
   else if(%gameIndex++ < $dtStats::pugCount[%game]){
      LFData.close();
      LFData.delete();
      schedule($dtStats::sortSpeed,0,"sortTurStats",0,%gameIndex,%game);
   }
   else{
      LFData.close();
      LFData.delete();
      sortTurStatsT(0, %game);
   }
}

function  sortTurStatsT(%c, %game){
   if($dtStats::debugEchos){error("sortTurStatsT" SPC %c SPC %game);}
   if(!isObject(LFData)){
      new FileObject(LFData);
      RootGroup.add(LFData);
      %file = "serverStats/gmData/" @ "-" @ %game @ "-" @ dtMarkDate() @"-T.cs";
      $dtStats::totalPath = %file;
      LFData.openForWrite(%file);
      //LFData.writeLine(strreplace($dtStats::pugIDS[%game],"\t","%t"));
      //LFData.writeLine(strreplace($dtStats::pugMap[%game],"\t","%t"));
      //LFData.writeLine(strreplace($dtStats::pugDate[%game],"\t","%t"));
      //LFData.writeLine(strreplace($dtStats::pugFS[%game],"\t","%t"));
   }

   %var = $statsVars::varNameType[%c,%game];
   %cat = $statsVars::varType[%var,%game];
   if(%cat !$= "Game"){
      if(%cat $= "AvgI" || %cat $= "Min"){
         %len = serverStats.getCount();
         for (%i = 0; %i < %len - 1; %i++) {
            for (%j = 0; %j < %len - %i - 1; %j++) {
               // If the current element is less than the next element, bring the next element to the front
               %aObj = serverStats.getObject(%j);
               %bObj = serverStats.getObject(%j + 1);
               %A = %aObj.LStatsT[%var];
               %B = %bObj.LStatsT[%var];
               if (%A > %B) {
                  serverStats.bringToFront(%bObj);
               }
            }
         }
      }
      else{

         %len = serverStats.getCount();
         for (%i = 0; %i < %len - 1; %i++) {
            for (%j = 0; %j < %len - %i - 1; %j++) {
               // If the current element is less than the next element, bring the next element to the front
               %aObj = serverStats.getObject(%j);
               %bObj = serverStats.getObject(%j + 1);
               %A = %aObj.LStatsT[%var];
               %B = %bObj.LStatsT[%var];
               if (%A < %B) {
                  serverStats.bringToFront(%bObj);
               }
            }
         }
      }

      %teamAllNameLine = "name" TAB %var;
      %teamAllDataLine = "data" TAB %var;

      %write = 0;
      for (%i = 0; %i < %len; %i++) {
         %sObj = serverStats.getObject(%i);
         %fv = %sObj.LStatsT[%var];
         if(%fv != 0){
            %teamAllNameLine =  %teamAllNameLine TAB %sObj.name;
            %teamAllDataLine =  %teamAllDataLine  TAB %fv;
            %write = 1;
         }
      }
      if(%write){
         LFData.writeLine(strreplace(%teamAllNameLine,"\t","%t"));
         LFData.writeLine(strreplace(%teamAllDataLine,"\t","%t"));
         $gData::data[%var,%game] = getFields(%teamAllDataLine, 2, getFieldCount(%teamAllDataLine)-1);
         $gData::name[%var,%game] = getFields(%teamAllNameLine, 2, getFieldCount(%teamAllNameLine)-1);
      }
   }
   if(%c++ < $statsVars::count[%game])
      schedule($dtStats::sortSpeed,0,"sortTurStatsT",%c,%game);
   else{
      LFData.close();
      LFData.delete();
      serverStats.delete();
      preLoadTurStats($dtGameIndex++);
   }
}
////////////////////////////////////////////////////////////////////////////////
//Server Stats
////////////////////////////////////////////////////////////////////////////////

$dtStats::prefTestTime = 512;// the lower the better tracking
$dtStats::prefTestIdleTime = 60*1000;// if no one is playing just run slow
$dtStats::prefTolerance = 128;//this number is to account for base line performance and differences between engine simTime and realtime
$dtStats::prefLog = 0; // enable logging of server hangs
$dtStats::eventLockout = 15*1000;//every 10 sec
$dtStats::tsLimit = 0.22; //note this value is heavly effected by packet rate so if you change this be sure to test low and high client rates
$dtStats::tsCountLimit = 8;
$dtStats::tsStat = 0;
function prefTest(%time,%skip){
   %real  = getRealTime();
   %plCount = $HostGamePlayerCount - $HostGameBotCount;
   if(isGameRun() && !$dtStats::building){// only track during run time
      %dif = (%real - %time) - $dtStats::prefTestTime;
      if(%dif > $dtStats::prefTolerance && !%skip && %plCount > 2){
         %msg = "Server Hang" SPC dtFormatTime(getSimTime()) SPC "Delay =" SPC %dif @ "ms" SPC "Player Count =" SPC %plCount SPC $CurrentMission;
         $dtServer::serverHangTotal++;
         $dtServer::serverHangMap[cleanMapName($CurrentMission),Game.class]++;
         $dtServer::serverHangLast = formattimestring("hh:nn:a mm-dd-yy");
         $dtServer::serverHangTime = %dif;
         dtEventLog(%msg, 0);
         %skip = 1;
      }
      else{
         %skip = 0;
      }
      if(%plCount > 0)
         dtPingStats();
   }
   if($dtStats::prefEnable){
      if(isGameRun() && !$dtStats::building && %plCount > 0)
         schedule($dtStats::prefTestTime, 0, "prefTest",%real,%skip);
      else{
         schedule($dtStats::prefTestIdleTime, 0, "prefTest",%real,1);
      }
   }
}

function getRealFlagPos(%team){
   if (isObject($TeamFlag[%team].carrier)){
      %pos = $TeamFlag[%team].carrier.getPosition();
   }
   else{
      %pos = $TeamFlag[%team].getPosition();
   }
   return %pos;
}

function dtPingStats(){
   %ping = %pingT = %pc = %txStop = %lowAvg = 0;
   %xPing = %plCount = %hpCount = %lowPing = 0;
   %min = 100000;
   %max = -100000;
   for(%i = 0; %i < ClientGroup.getCount(); %i++){
      %cl = ClientGroup.getObject(%i);
      //pathDataPoint(%cl);
      if(isObject(%cl.dtStats)){
         if(isObject(%cl.player)){
            %tform = %cl.player.getTransform();
            if(isObject(Game) && ( Game.class $= "CTFGame" || Game.class $= "LCTFGame" || Game.class $= "SCtFGame")){
               %fPos = $dtStats::FlagPos[%cl.team];
               %oTeam = (%cl.team == 1) ?  2 : 1;
               %fePos = $dtStats::FlagPos[%oTeam];
               %fDist = vectorDist(%fPos,getWords(%tform,0,2));
               if(%fDist < 50){
                  %cl.dtStats.stat["timeNearTeamFS"] += ($dtStats::prefTestTime/1000)/60;
               }
               else{
                  %cl.dtStats.stat["timeFarTeamFS"] += ($dtStats::prefTestTime/1000)/60;
               }

               %feDist = vectorDist(%fePos,getWords(%tform,0,2));
               if(%fDist < 50){
                  %cl.dtStats.stat["timeNearEnemyFS"] += ($dtStats::prefTestTime/1000)/60;
               }
               else{
                  %cl.dtStats.stat["timeFarEnemyFS"] += ($dtStats::prefTestTime/1000)/60;
               }
               %rfpos = getRealFlagPos(%cl.team);
               %oTeam = (%cl.team == 1) ?  2 : 1;
               %rfEPos = getRealFlagPos(%oTeam);

               if(vectorDist(%rfpos, getWords(%tform,0,2)) < 50){
                  %cl.dtStats.stat["timeNearFlag"] += ($dtStats::prefTestTime/1000)/60;
               }
               else if(vectorDist(%rfEPos, getWords(%tform,0,2)) < 50){
                  %cl.dtStats.stat["timeNearEnemyFlag"] += ($dtStats::prefTestTime/1000)/60;
               }
            }
            if(%tform $= %cl.tform){
               %cl.dtStats.stat["idleTime"] += ($dtStats::prefTestTime/1000)/60;
            }
            %cl.tform = %tform;
         }
         else if(%cl.team == 0)
            %cl.dtStats.stat["idleTime"] += ($dtStats::prefTestTime/1000)/60;
         if(!%cl.isAIControlled()){
            %ping = %cl.getPing();
            %cl.pingTotal += %ping;
            %cl.pingCount++;
            %cl.dtStats.stat["pingAvg"] = %cl.pingTotal / %cl.pingCount;
            if(%cl.pingTotal > 99999){
               %cl.pingTotal *= 0.5;
               %cl.pingCount *= 0.5;
            }
            %min  =  (%ping < %min) ? %ping : %min;
            %max  =  (%ping > %max) ? %ping : %max;
            if(%ping == %cl.lastPing){
               %cl.lpC++;
               if(%cl.lpC > 2){
                  %cl.dtStats.stat["txStop"]++;
                  %txStop++;
               }
            }
            else
               %cl.lpC = 0;

            %cl.lastPing = %ping;
            if(%ping > 500){
               %cl.dtStats.stat["lagSpikes"]++;
               %hpCount++;
            }
            else{
               %lowCount++;
               %lowPing += %ping;
            }
            %pl = %cl.getPacketLoss();
            if( %pl > 0){
               %cl.dtStats.stat["packetLoss"]++;
               %plCount++;
            }
            %pc++;
            %pingT += %ping;
         }
      }
      if (isObject(%player)&& $dtStats::tsStat){//&& !%client.isAIControlled()
			if (%player.dtLV !$= "" &&  %ping < 500 && !%txStop && %pl < 25){
				%deltaP = VectorDist(%player.dtLP, %player.getPosition());
				%vel = %player.getVelocity();
				%speed = vectorLen(%vel);
				%iVel = VectorLen(%player.dtLV) / %factor;
				%fVel = %speed / %factor;

				%player.vdot[%player.vdotCount++ % 3] = VectorDot(VectorNormalize(%vel), VectorNormalize(%player.lastVelocity));
				%dotTotal = (%player.vdotCount > 3) ? ((%player.vdot[0] + %player.vdot[1] + %player.vdot[2])/3) : 0;
				if (%speed > 6 && %iVel > 0.01 && %fVel > 0.01 && %deltaP > 0.01  && %dotTotal > 0.9){
               %least = mAbs(%deltaP - %iVel);
               %least2 = mAbs(%fVel - %deltaP);
               %least =  (%least2 < %least) ? %least2 : %least;
               %distortion = (%least / %deltaP);

               %client.tsDistortion = %distortion;

               if(%distortion < 1){// clamp  it so random large values dont mess things up
                  %client.dstTotal += %distortion; %client.dstCount++;
                  %client.dstAvg = %client.dstTotal / %client.dstCount;

                  if(%client.dstCount > 30){// limit sample size
                    %client.dstTotal *= (1 - 0.3);
                    %client.dstCount *= (1 - 0.3);
                  }

                  if( %client.dstAvg > $tsLimit && %client.dstCount > 15){
                     %client.dstHighAvg++;
                  }

                  if(%distortion > $dtStats::tsLimit){
                     if(%client.distortionCount < $dtStats::tsCountLimit){
                        %client.distortionCount++;
                     }
                     if(%client.distortionCount >= $tsCountLimit){
                        %client.tsc++;
                     }
                  }
                  else if(%distortion < $tsLimit && %client.distortionCount > 0)
                    %client.distortionCount--;
               }
            }
			}
			%player.dtLV = %player.getVelocity();
			%player.dtLP = %player.getPosition();
		}
   }
   if(%pc > 3){
      %lowAvg =  (%lowCount > 0) ? (%lowPing/%lowCount) : 0;
      $dtStats::pingAvg = %pingT / %pc;
      if(%txStop / %pc  > 0.5){
         if(getSimTime() - $dtStats:evTime[0] > $dtStats::eventLockout){
            %msg = "TX Loss" SPC dtFormatTime(getSimTime()) SPC "TX Loss Count =" SPC %txStop SPC "Player Count =" SPC %pc;
            dtEventLog(%msg, 0);
            $dtServer::hostHangTotal++;
            $dtServer::hostHangLast = formattimestring("hh:nn:a mm-dd-yy");
            $dtServer::hostHangTime = %pingT / %pc;
            $dtStats:evTime[0] = getSimTime();
         }
      }

      if(%plCount / %pc > 0.5){
         if(getSimTime() - $dtStats:evTime[1] > $dtStats::eventLockout){
            %msg = "Packet Loss" SPC dtFormatTime(getSimTime()) SPC "Packet Loss Count =" SPC %plCount SPC "Player Count =" SPC %pc;
            dtEventLog(%msg, 0);
            $dtServer::hostHangTotal++;
            $dtServer::hostHangLast = formattimestring("hh:nn:a mm-dd-yy");
            $dtServer::hostHangTime = %pingT / %pc;
            $dtStats:evTime[1] = getSimTime();
         }
      }
      %hpct =  (%hpCount > 0) ? (%hpCount/%pc) : 0;
      if(%hpct > 0.5){
         if($dtStats::pingAvg > 1000){//network issues
            if(getSimTime() - $dtStats:evTime[2] > $dtStats::eventLockout){
               %msg = "Host Hang" SPC dtFormatTime(getSimTime()) SPC "Avg:" @ $dtStats::pingAvg @ "/" @ %lowAvg SPC "Min:" @ %min SPC "Max:" @ %max SPC "Counts =" SPC %hpCount  @ "/" @ %pc;
               dtEventLog(%msg, 0);
               $dtServer::hostHangMap[cleanMapName($CurrentMission),Game.class]++;
               $dtServer::hostHangTotal++;
               $dtServer::hostHangLast = formattimestring("hh:nn:a mm-dd-yy");
               $dtServer::hostHangTime = %pingT / %pc;
               $dtStats:evTime[2] = getSimTime();
            }
         }
         else if($dtStats::pingAvg > 500){
            if(getSimTime() - $dtStats:evTime[3] > $dtStats::eventLockout){
               %msg = "500+ Ping" SPC dtFormatTime(getSimTime()) SPC "Avg:" @ $dtStats::pingAvg @ "/" @ %lowAvg SPC "Min:" @ %min SPC "Max:" @ %max SPC "Counts =" SPC %hpCount @ "/" @ %pc;
               dtEventLog(%msg, 0);
               $dtServer::hostHangTotal++;
               $dtServer::hostHangLast = formattimestring("hh:nn:a mm-dd-yy");
               $dtServer::hostHangTime = %pingT / %pc;
               $dtStats:evTime[3] = getSimTime();
            }
         }
      }
      if(%min > 200){
         if(getSimTime() - $dtStats:evTime[5] > $dtStats::eventLockout){
            %msg = "Ping Min Event" SPC dtFormatTime(getSimTime()) SPC "Min:" SPC %min SPC "Max:" SPC %max SPC "Player Count =" SPC %pc;
            dtEventLog(%msg, 0);
            $dtServer::hostHangTotal++;
            $dtServer::hostHangLast = formattimestring("hh:nn:a mm-dd-yy");
            $dtServer::hostHangTime = %pingT / %pc;
            $dtStats:evTime[5] = getSimTime();
         }
      }
   }
}

$dtStats::eventMax = 32;
function dtEventLog(%log,%save){

   if(%count >= $dtStats::eventMax){
      $dtServer::eventLogCount = 0;
   }
   $dtServer::eventLog[$dtServer::eventLogCount] = %log;
   $dtServer::eventLogCount++;
   $dtStats:lastEvent = getSimTime();
   error(%log);
   if(%save && !isEventPending($eventSaveId))
      $eventSaveId =  schedule(10000, 0, "export", "$dtServer::event*", "serverStats/eventLog.cs", false );
}

function startMonitor(){
   if(!$dtStats::prefEnable){// if we are running dont start again
      $dtStats::prefEnable =1;
      if($dtStats::prefTestTime < 128){$dtStats::prefTestTime = 128;}
      prefTest(getRealTime(),1);
   }
}

function dtSaveServerVars(){
   $dtServerVars::lastSimTime = getSimTime();
   $dtServerVars::lastDate = formattimestring("mm/dd/yy hh:nn:a");
   $dtServerVars::lastMission = cleanMapName($CurrentMission);
   $dtServerVars::lastGameType = Game.class;
   $dtServerVars::lastPlayerCount =  $HostGamePlayerCount - $HostGameBotCount;
   %i = 0;
   schedule(1000 * %i++,0,"export", "$dtServerVars::*", "serverStats/serverVars.cs", false );
   schedule(1000 * %i++,0,"export", "$mapID::*", "serverStats/mapIDList.cs", false );
   schedule(1000 * %i++,0,"export", "$dtServer::event*", "serverStats/eventLog.cs", false );
   if(isObject(pugList)){
      pugList.schedule(1000 * %i++,"save","serverStats/pugLog.cs", 0);
   }
   if(isObject(pubList)){
      pubList.schedule(1000 * %i++,"save","serverStats/pubLog.cs", 0);
   }
   if($dtStats::ctfTimes)
      schedule(1000 * %i++,0,"export", "$dtServer::capTimes*", "serverStats/capTimes.cs", false );
}

function dtLoadServerVars(){// keep function at the bottom
   if(!statsGroup.serverStart){
      statsGroup.serverStart = 1;
      $dtStats::teamOneCapTimes = 0;
      $dtStats::teamTwoCapTimes = 0;
      $dtStats::teamOneCapCount = 0;
      $dtStats::teamTwoCapCount = 0;
      $dtServerVars::upTimeCount = -1;
      $dtStats::conLogCount  = 0;
      $dtServerVars::WhiteListMode = 0;
      $dtServerVars::IPBanListMode = 0;
      if(isFile("serverStats/serverVars.cs")){
         exec("serverStats/serverVars.cs");
         %date = $dtServerVars::lastDate;
         %upTime = dtFormatTime($dtServerVars::lastSimTime);
         %mis = $dtServerVars::lastMission;
         if($dtStats::debugEchos){schedule(6000,0,"error","last server uptime = " SPC %date @ "-" @ %upTime @ "-" @ %mis);}
         $dtServerVars::upTime[$dtServerVars::upTimeCount++] = %date @ "-" @ %upTime @ "-" @ %mis;
         if($dtServerVars::lastPlayerCount > 3){
            $dtServerVars::serverCrash[%mis, $dtServerVars::lastGameType]++;
            $dtServerVars::crashLog[$dtServerVars::crashLogCount++] = %date @ "-" @ %upTime @ "-" @ %mis @ "-" @  $dtServerVars::lastGameType @ "-" @ $dtServerVars::lastPlayerCount;
            schedule(30000,0,"dtEventLog","Server Crash" SPC %date SPC "Pl Count =" SPC $dtServerVars::lastPlayerCount SPC "Map =" SPC %mis, 0);
         }
         schedule(30001,0,"dtEventLog","Last Server Uptime =" SPC %date SPC "Up Time =" SPC %upTime SPC %mis, 0);
      }
      if($dtServerVars::upTimeCount >= 30)
         $dtServerVars::upTimeCount = 0;
      if($dtServerVars::crashLogCount >= 15)
         $dtServerVars::crashLogCount = 0;

      $dtServerVars::lastPlayerCount = 0;
      $dtServerVars::lastSimTime = getSimTime();
      $dtServerVars::lastDate =  formattimestring("mm/dd/yy hh:nn:a");
      export( "$dtServerVars::*", "serverStats/serverVars.cs", false );
      if(isFile("serverStats/capTimes.cs") && $dtStats::ctfTimes)
         exec("serverStats/capTimes.cs");
      if(isFile("serverStats/mapPlayRot.cs"))
         exec("serverStats/mapPlayRot.cs");
      if(isFile("serverStats/pugLog.cs"))
         exec("serverStats/pugLog.cs");
      if(isFile("serverStats/pubLog.cs"))
         exec("serverStats/pubLog.cs");
      if(isFile("serverStats/tbVars.cs"))
         exec("serverStats/tbVars.cs");
      $dtServer::eventLogCount = 0;
      if(isFile("serverStats/eventLog.cs"))
         exec("serverStats/eventLog.cs");



      dtEventLog("Server Start" SPC formattimestring("hh:nn:a mm-dd-yy"), 0);

      genBlanks();
      buildVarList();
      startMonitor();
      loadMapIdList();
   }
}dtLoadServerVars();


////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
function testVarsRandomAll(%max){
   %game = Game.class;
   for(%q = 0; %q < $statsVars::count[%game]; %q++){
      %varNameType = $statsVars::varNameType[%q,%game];
      %varName = $statsVars::varName[%q,%game];
      for(%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         %val = getRandom(0,%max);
         %client.dtStats.stat[%varName] = %val;
         dtMinMax(%varName, "wep", 1, %val, %client);
         dtMinMax(%varName, "flag", 1, %val, %client);
         dtMinMax(%varName, "misc", 1, %val, %client);
      }
   }
}