//Evo Style Debrief
//Script By: DarkTiger
$dtDebrief::Version = 1.0;

//converts the debrief into easer to read teams for ctf and lctf
$dtStats::teamDebrief = $Host::dtStatsTeamDebrief $= "" ? ($Host::dtStatsTeamDebrief = 1) : $Host::dtStatsTeamDebrief;
$dtStats::teamDebrief = isFile("scripts/autoexec/EvoStats.cs") == 0 ? $dtStats::teamDebrief : 0;

//extends the debrief with extra stats done in the evo style
$dtStats::evoStyleDebrief = $Host::dtStatsEvoStyleDebrief $= "" ? ($Host::dtStatsEvoStyleDebrief = 1) : $Host::dtStatsEvoStyleDebrief ;
$dtStats::evoStyleDebrief = isFile("scripts/autoexec/EvoStats.cs") == 0 ? $dtStats::evoStyleDebrief : 0;


package dtDebrief{

   function DefaultGame::sendDebriefing( %game, %client ){
      parent::sendDebriefing( %game, %client );
      extendedDebrief(%game, %client);
   }
   function ArenaGame::sendDebriefing(%game, %client){
      if(%client.isWatchOnly){
         parent::sendDebriefing(%game, %client);
         return;
      }
      messageClient( %client, 'MsgClearDebrief', "" );
      if($dtStats::teamDebrief == 1){
         %topScore = "";
         %topCount = 0;
         for ( %team = 1; %team <= %game.numTeams; %team++ ){
            if ( %topScore $= "" || $TeamScore[%team] > %topScore ){
               %topScore = $TeamScore[%team];
               %firstTeam = %team;
               %topCount = 1;
            }
            else if ( $TeamScore[%team] == %topScore ){
               %secondTeam = %team;
               %topCount++;
            }
         }

         // Mission result:
         if ( %topCount == 1 )
            messageClient( %client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', %game.getTeamName(%firstTeam) );
         else if ( %topCount == 2 )
            messageClient( %client, 'MsgDebriefResult', "", '<just:center>Team %1 and Team %2 tie!', %game.getTeamName(%firstTeam), %game.getTeamName(%secondTeam) );
         else
            messageClient( %client, 'MsgDebriefResult', "", '<just:center>The mission ended in a tie.' );

         if ( $Arena::Pref::TrackHighScores && Game.class $= "ArenaGame" ){
            if ( %game.newHighScoreFlag )
              messageClient( %client, 'MsgDebriefResult', "", '<spush><color:3cb4b4><font:univers condensed:18>%1 has set a NEW INDIVIDUAL RECORD for this mission with a score of %2!<spop>', $Arena::HighScores::Name[$currentMission], $Arena::HighScores::Score[$currentMission] );
            else if ( $Arena::HighScores::Name[$currentMission] !$= "" && $Arena::HighScores::Score[$currentMission] !$= "" )
              messageClient( %client, 'MsgDebriefResult', "", '<spush><color:3cb4b4><font:univers condensed:18>%1 holds the individual record for this mission with a score of %2.<spop>', $Arena::HighScores::Name[$currentMission], $Arena::HighScores::Score[$currentMission] );
            else
              messageClient( %client, 'MsgDebriefResult', "", '<spush><color:3cb4b4><font:univers condensed:18>There is no high score recorded for this mission.<spop>' );
            if ( %game.notEnoughHumansFlag )
              messageClient( %client, 'MsgDebriefResult', "", '<spush><color:3cb4b4><font:univers condensed:18>But there are not enough human players here to set a new high score.<spop>' );
         }
         messageClient( %client, 'MsgDebriefAddLine', "", ' ' );


         messageClient( %client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:20>TEAM<lmargin%%:37>SCORE<lmargin%%:49>KILLS<lmargin%%:61>KDR<lmargin%%:73>ASSISTS<lmargin%%:85>DISC MA<spop>' );

         for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
            %count[%team] = 0;

         %notDone = true;
         while ( %notDone )
         {
            // Get the highest remaining score:
            %highScore = "";
            for ( %team = 1; %team <= %game.numTeams; %team++ )
            {
               if ( %count[%team] < $TeamRank[%team, count] && ( %highScore $= "" || $TeamRank[%team, %count[%team]].score > %highScore ) )
               {
                  %highScore = $TeamRank[%team, %count[%team]].score;
                  %highTeam = %team;
               }
            }

            // Send the debrief line:
            %cl = $TeamRank[%highTeam, %count[%highTeam]];
            %score = %cl.score $= "" ? 0 : %cl.score;
            %kills = %cl.kills $= "" ? 0 : %cl.kills;
            %deaths = %cl.deaths $= "" ? 0 : %cl.deaths;
            if(%client == %cl){
               %line = '<lmargin:0><color:ffff00><clip%%:40>%1</clip><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:37>%3<lmargin%%:49>%4<lmargin%%:61>%6<lmargin%%:73>%7<lmargin%%:85>%8<color:3cb4b4>';
            }
            else{
               %line = '<lmargin:0><color:c8c8c8><clip%%:40>%1</clip><color:3cb4b4><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:37>%3<lmargin%%:49>%4<lmargin%%:61>%6<lmargin%%:73>%7<lmargin%%:85>%8';
            }
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(getTaggedString(%cl.name)), %game.getTeamName(%cl.team), %score, %kills , %deaths, cropFloat(%cl.dtStats.stat["kdr"],2), %cl.dtStats.stat["assist"], %cl.dtStats.stat["discMA"] );

            %count[%highTeam]++;
            %notDone = false;
            for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
            {
               if ( %count[%team] < $TeamRank[%team, count] )
               {
                  %notDone = true;
                  break;
               }
            }
         }

         //now go through an list all the observers:
         %count = ClientGroup.getCount();
         %printedHeader = false;
         for (%i = 0; %i < %count; %i++)
         {
            %cl = ClientGroup.getObject(%i);
            if (%cl.team <= 0)
            {
               //print the header only if we actually find an observer
               if (!%printedHeader)
               {
                  %printedHeader = true;
                  %score = %cl.score $= "" ? 0 : %cl.score;
                  %kills = %cl.kills $= "" ? 0 : %cl.kills;
                  %deaths = %cl.deaths $= "" ? 0 : %cl.deaths;

                  messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<lmargin%%:37>SCORE<lmargin%%:49>KILLS<lmargin%%:61>KDR<lmargin%%:73>ASSISTS<lmargin%%:85>DISC MA<spop>');

               }

               //print out the client
                  %score = %cl.score $= "" ? 0 : %cl.score;//<bitmap:bullet_2>
                  if(%client == %cl){
                     %line = '<lmargin:0><color:ffff00><clip%%:40>%1</clip><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:37>%3<lmargin%%:49>%4<lmargin%%:61>%6<lmargin%%:73>%7<lmargin%%:85>%8<color:3cb4b4>';
                  }
                  else{
                     %line = '<lmargin:0><color:c8c8c8><clip%%:40>%1</clip><color:3cb4b4><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:37>%3<lmargin%%:49>%4<lmargin%%:61>%6<lmargin%%:73>%7<lmargin%%:85>%8';
                  }
                  messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(getTaggedString(%cl.name)), "", %score, %kills , %deaths, cropFloat(%cl.dtStats.stat["kdr"],2), %cl.dtStats.stat["assist"], %cl.dtStats.stat["discMA"] );

            }
         }
         extendedDebrief(%game, %client);

      }
      else{
         parent::sendDebriefing(%game, %client);// note not default game
         extendedDebrief(%game, %client);
      }
   }

    // new debriefing stuff
function LakRabbitGame::sendDebriefing( %game, %client ){
      if(%client.isWatchOnly){
         parent::sendDebriefing(%game, %client);
         return;
      }
      messageClient( %client, 'MsgClearDebrief', "" );
      if($dtStats::teamDebrief == 1){
         messageClient( %client, 'MsgDebriefAddLine', "", '<spush><lmargin:0><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:23>SCORE<lmargin%%:34>KILLS<lmargin%%:44>MAS<lmargin%%:52>SPEED<lmargin%%:62>DIST<lmargin%%:70>TOT DIST<lmargin%%:80>SHOCK<lmargin%%:90>SL HITS<spop>' );
         //													%cl.name, %score, %kills, %mas, %avgSpeed, %avgDistance, %alltotdistance, %shockPercent, %totshockhits
         // Scores:
         %totscore	= 0;
         %totkills	= 0;
         %totmas		= 0;
         %totspeed	= 0;
         %totdistance	= 0;
         //%totchainacc	= 0;
         //%totsnipepercent= 0;
         %totshockpercent= 0;
         %speeds		= 0;
         %dists		= 0;
         //%chains		= 0;
         //%snipes		= 0;
         %shocks		= 0;
         %alltotdistance		= 0;
         %totshockhits		= 0;

         %count = $TeamRank[0, count];
         for(%i = 0; %i < %count; %i++){
            // Send the debrief line:
            %cl = $TeamRank[0, %i];

            if(%cl.score == 0)	%score = 0;
            else			%score = %cl.score;
            if(%cl.kills == 0)	%kills = 0;
            else			%kills = %cl.kills;
            if(%cl.mas == 0)	%mas = 0;
            else			%mas = %cl.mas;

            //if(%cl.totalSnipes == 0) %cl.totalSnipes = 1;
            if(%cl.totalShocks == 0) %cl.totalShocks = 1;

            if(%cl.totalSpeed == 0)	%avgSpeed = 0;
            else				%avgSpeed = mFloor(%cl.totalSpeed/%cl.mas);
            if(%cl.totalDistance == 0)	%avgDistance = 0;
            else				%avgDistance = mFloor(%cl.totalDistance/%cl.mas);
            //if(%cl.totalChainAccuracy == 0)	%avgChainAcc = 0;
            //else				%avgChainAcc = mFloor(%cl.totalChainAccuracy/%cl.totalChainHits);
            //if(%cl.totalSnipeHits == 0)	%snipePercent = 0;
            //else				%snipePercent = mFloor(%cl.totalSnipeHits/%cl.totalSnipes*100);
            if(%cl.totalShockHits == 0)	%shockPercent = 0;
            else				%shockPercent = mFloor(%cl.totalShockHits/%cl.totalShocks*100);
            if(%cl.totalDistance == 0) %othertotdistance = 0;
            else				%othertotdistance = mFloor(%cl.totalDistance);
            if(%cl.totalShockHits == 0) %shockhits = 0;
            else				%shockhits = mFloor(%cl.totalShockHits);

            if(%client == %cl){
               messageClient( %client, 'MsgDebriefAddLine', "", '<color:ffff00><lmargin:0><clip%%:18> %1</clip><lmargin%%:23>%2<lmargin%%:34>%3<lmargin%%:44>%4<lmargin%%:52>%5<lmargin%%:62>%6<lmargin%%:70>%7<lmargin%%:80>%8%%<lmargin%%:90>%9',
                  StripMLControlChars(getTaggedString(%cl.name)), %score, %kills, %mas, %avgSpeed, %avgDistance, %othertotdistance, %shockPercent, %shockhits);
            }
            else{
                messageClient( %client, 'MsgDebriefAddLine', "", '<color:c8c8c8><lmargin:0><clip%%:18> %1</clip><lmargin%%:23>%2<lmargin%%:34>%3<lmargin%%:44>%4<lmargin%%:52>%5<lmargin%%:62>%6<lmargin%%:70>%7<lmargin%%:80>%8%%<lmargin%%:90>%9',
                  StripMLControlChars(getTaggedString(%cl.name)), %score, %kills, %mas, %avgSpeed, %avgDistance, %othertotdistance, %shockPercent, %shockhits);
            }

            if(%score)		%totscore		+= %score;
            if(%kills)		%totkills		+= %kills;
            if(%mas)		%totmas			+= %mas;
            if(%avgSpeed){		%totspeed		+= %avgSpeed;		%speeds++; }
            if(%avgDistance){	%totdistance		+= %avgDistance;	%dists++; }
            //if(%avgChainAcc){	%totchainacc		+= %avgChainAcc;	%chains++; }
            //if(%snipePercent){	%totsnipepercent	+= %snipePercent;	%snipes++; }
            if(%shockPercent){	%totshockpercent	+= %shockPercent;	%shocks++; }
            if(%othertotdistance){	%alltotdistance			+= %othertotdistance;  }
            if(%shockhits){			%totshockhits			+= %shockhits;  }

         }
         messageClient( %client, 'MsgDebriefAddLine', "", '<spush><lmargin:0><Font:Arial:15><color:00FF7F>%1<lmargin%%:23>%2<lmargin%%:34>%3<lmargin%%:44>%4<lmargin%%:52>%5<lmargin%%:62>%6<lmargin%%:70>%7<lmargin%%:80>%8%%<lmargin%%:90>%9<spop>\n',
            "   Totals:", %totscore, %totkills, %totmas, mFloor(%totspeed/%speeds), mFloor(%totdistance/%dists), %alltotdistance, mFloor(%totshockpercent/%shocks), %totshockhits);
      extendedDebrief(%game, %client);
      }
      else{
         parent::sendDebriefing(%game, %client);
         extendedDebrief(%game, %client);
      }
   }
   function CTFGame::sendDebriefing(%game, %client){
   if(%client.isWatchOnly){
      parent::sendDebriefing(%game, %client);
      return;
   }
   messageClient( %client, 'MsgClearDebrief', "" );
   if($dtStats::teamDebrief == 1){
      %game.sendCTFDebrif(%client);
      extendedDebrief(%game, %client);
   }
   else{
      parent::sendDebriefing(%game, %client);
   }
}

   function LCTFGame::sendDebriefing(%game, %client){
      if(%client.isWatchOnly){
         parent::sendDebriefing(%game, %client);
         return;
      }
      messageClient( %client, 'MsgClearDebrief', "" );
      if($dtStats::teamDebrief == 1){
         %game.sendCTFDebrif(%client);
         extendedDebrief(%game, %client);
      }
      else{
         parent::sendDebriefing(%game, %client);
      }
   }

   function SCtFGame::sendDebriefing(%game, %client){
      if(%client.isWatchOnly){
         parent::sendDebriefing(%game, %client);
         return;
      }
      messageClient( %client, 'MsgClearDebrief', "" );
      if($dtStats::teamDebrief == 1){
         %game.sendCTFDebrif(%client);
         extendedDebrief(%game, %client);
      }
      else{
         parent::sendDebriefing(%game, %client);
      }
   }
};

function DefaultGame::sendCTFDebrif(%game,%client){
   %topScore = "";
   %topCount = 0;
   for ( %team = 1; %team <= %game.numTeams; %team++ )
   {
      if ( %topScore $= "" || $TeamScore[%team] > %topScore )
      {
         %topScore = $TeamScore[%team];
         %firstTeam = %team;
         %topCount = 1;
      }
      else if ( $TeamScore[%team] == %topScore )
      {
         %secondTeam = %team;
         %topCount++;
      }
   }

   // Mission result:
   if ( %topCount == 1 )
      messageClient( %client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', %game.getTeamName(%firstTeam) );
   else if ( %topCount == 2 )
      messageClient( %client, 'MsgDebriefResult', "", '<just:center>Team %1 and Team %2 tie!', %game.getTeamName(%firstTeam), %game.getTeamName(%secondTeam) );
   else
      messageClient( %client, 'MsgDebriefResult', "", '<just:center>The mission ended in a tie.' );

   // Team scores:
   messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>TEAM<lmargin%%:35>SCORE<spop>' );
   for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
   {
      if ( $TeamScore[%team] $= "" )
         %score = 0;
      else
         %score = $TeamScore[%team];
      messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:35><clip%%:40> %2</clip>', %game.getTeamName(%team), %score );
   }

   // Player scores:
   messageClient( %client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:20>TEAM<lmargin%%:35>SCORE<lmargin%%:45>KILLS<lmargin%%:55>Assists<lmargin%%:65>OffKills<lmargin%%:75>DefKills<lmargin%%:85>Disc MA<spop>' );
   for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
      %count[%team] = 0;

   %notDone = true;
   while ( %notDone )
   {
      // Get the highest remaining score:
      %highScore = "";
      for ( %team = 1; %team <= %game.numTeams; %team++ )
      {
         if ( %count[%team] < $TeamRank[%team, count] && ( %highScore $= "" || $TeamRank[%team, %count[%team]].score > %highScore ) )
         {
            %highScore = $TeamRank[%team, %count[%team]].score;
            %highTeam = %team;
         }
      }

      // Send the debrief line:
      %cl = $TeamRank[%highTeam, %count[%highTeam]];
      %score = %cl.score $= "" ? 0 : %cl.score;
      %kills = %cl.kills $= "" ? 0 : %cl.kills;
      %nameColor = %cl == %client ?  "<color:ffff00>" : "<color:c8c8c8>";//<color:3cb4b4>
      if(%cl == %client){
         %line = '<lmargin:0>%9<clip%%:40>%1</clip><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:35>%3<lmargin%%:45>%4<lmargin%%:55>%5<lmargin%%:65>%6<lmargin%%:75>%7<lmargin%%:85>%8';

      }else{
         %line = '<lmargin:0>%9<clip%%:40>%1</clip><color:3cb4b4><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:35>%3<lmargin%%:45>%4<lmargin%%:55>%5<lmargin%%:65>%6<lmargin%%:75>%7<lmargin%%:85>%8';
      }
      messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(getTaggedString(%cl.name)), %game.getTeamName(%cl.team), %score, %kills, %cl.dtStats.stat["assist"], %cl.dtStats.stat["OffKills"], %cl.dtStats.stat["DefKills"], %cl.dtStats.stat["discMA"],%nameColor);

      %count[%highTeam]++;
      %notDone = false;
      for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
      {
         if ( %count[%team] < $TeamRank[%team, count] )
         {
            %notDone = true;
            break;
         }
      }
   }

   //now go through an list all the observers:
   %count = ClientGroup.getCount();
   %printedHeader = false;
   for (%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team <= 0)
      {
         //print the header only if we actually find an observer
         if (!%printedHeader)
         {
            %printedHeader = true;
            messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<lmargin%%:35>SCORE<lmargin%%:45>KILLS<lmargin%%:55>Assists<lmargin%%:65>OffKills<lmargin%%:75>DefKills<lmargin%%:85>Disc MA<spop>');
         }

         //print out the client
         %score = %cl.score $= "" ? 0 : %cl.score;//<bitmap:bullet_2>
         %kills = %cl.kills $= "" ? 0 : %cl.kills;
         %nameColor = %cl == %client ?  "<color:ffff00>" : "<color:c8c8c8>";
         if(%cl == %client){
            %line = '<lmargin:0>%9<clip%%:40>%1</clip><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:35>%3<lmargin%%:45>%4<lmargin%%:55>%5<lmargin%%:65>%6<lmargin%%:75>%7<lmargin%%:85>%8<color:3cb4b4>';
         }
         else{
            %line = '<lmargin:0>%9<clip%%:40>%1</clip><color:3cb4b4><lmargin%%:20><clip%%:30> %2</clip><lmargin%%:35>%3<lmargin%%:45>%4<lmargin%%:55>%5<lmargin%%:65>%6<lmargin%%:75>%7<lmargin%%:85>%8';
         }
         messageClient( %client, 'MsgDebriefAddLine', "", %line,StripMLControlChars(getTaggedString(%cl.name)), "", %score, %kills, %cl.dtStats.stat["assist"], %cl.dtStats.stat["OffKills"], %cl.dtStats.stat["DefKills"], %cl.dtStats.stat["discMA"],%nameColor );
      }
   }
}

function extendedDebrief(%game, %client){
   if($dtStats::evoStyleDebrief && !%client.isWatchOnly){
      if(dtGameStat.gc["flag"] > 0){
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0> ' );
         messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>FLAG STATS\tPLAYER\t' );
         if(dtGameStat.stat["heldTimeSec"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Fastest Cap\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2 Sec', StripMLControlChars(hasValueS(dtGameStat.name["heldTimeSec"],"NA")), dtGameStat.stat["heldTimeSec"],(%client == dtGameStat.client["heldTimeSec"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["grabSpeed"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Flaming Ass\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2 Kmh', StripMLControlChars(hasValueS(dtGameStat.name["grabSpeed"],"NA")), dtGameStat.stat["grabSpeed"],(%client == dtGameStat.client["grabSpeed"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["flagCaps"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Cap Mastah\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["flagCaps"],"NA")), dtGameStat.stat["flagCaps"],(%client == dtGameStat.client["flagCaps"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["flagGrabs"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Grabz0r\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["flagGrabs"],"NA")), dtGameStat.stat["flagGrabs"],(%client == dtGameStat.client["flagGrabs"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["carrierKills"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>FC killer\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["carrierKills"],"NA")), dtGameStat.stat["carrierKills"],(%client == dtGameStat.client["carrierKills"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["flagDefends"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Flag Guardian\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["flagDefends"],"NA")), dtGameStat.stat["flagDefends"],(%client == dtGameStat.client["flagDefends"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["escortAssists"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Flag Escort\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["escortAssists"],"NA")), dtGameStat.stat["escortAssists"],(%client == dtGameStat.client["escortAssists"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["stalemateReturn"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Stalemate Breaker\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["stalemateReturn"],"NA")), dtGameStat.stat["stalemateReturn"],(%client == dtGameStat.client["stalemateReturn"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["flagReturns"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280><color:00dc00>Flag Returns\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2', StripMLControlChars(hasValueS(dtGameStat.name["flagReturns"],"NA")), dtGameStat.stat["flagReturns"],(%client == dtGameStat.client["flagReturns"]) ? "<color:ffff00>" : "<color:c8c8c8>");
      }
      if(dtGameStat.gc["ma"] > 0){
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0> ' );
         messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280,360,510><color:00dc00>\tPLAYER\tMA\tPLAYER\tDISTANCE');
         if(dtGameStat.stat["discMA"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Disc\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4m';
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["discMA"],"NA")), dtGameStat.stat["discMA"], StripMLControlChars(hasValueS(dtGameStat.name["discMAHitDist"],"NA")), mFormatFloat(dtGameStat.stat["discMAHitDist"], "%.2f"),(%client == dtGameStat.client["discMA"]) ? "<color:ffff00>" : "<color:c8c8c8>", (%client == dtGameStat.client["discMAHitDist"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         }
         if(dtGameStat.stat["plasmaMA"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Plasma\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4m';
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["plasmaMA"],"NA")), dtGameStat.stat["plasmaMA"], StripMLControlChars(hasValueS(dtGameStat.name["plasmaMAHitDist"],"NA")), mFormatFloat(dtGameStat.stat["plasmaMAHitDist"], "%.2f"),(%client == dtGameStat.client["plasmaMA"]) ? "<color:ffff00>" : "<color:c8c8c8>", (%client == dtGameStat.client["plasmaMAHitDist"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         }
         if(dtGameStat.stat["blasterMA"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Blaster\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4m';
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["blasterMA"],"NA")), dtGameStat.stat["blasterMA"], StripMLControlChars(hasValueS(dtGameStat.name["blasterMAHitDist"],"NA")), mFormatFloat(dtGameStat.stat["blasterMAHitDist"], "%.2f"),(%client == dtGameStat.client["blasterMA"]) ? "<color:ffff00>" : "<color:c8c8c8>", (%client == dtGameStat.client["blasterMAHitDist"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         }
         if(dtGameStat.stat["grenadeMA"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Grenade Launcher\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4m';
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["grenadeMA"],"NA")), dtGameStat.stat["grenadeMA"], StripMLControlChars(hasValueS(dtGameStat.name["grenadeMAHitDist"],"NA")), mFormatFloat(dtGameStat.stat["grenadeMAHitDist"], "%.2f"),(%client == dtGameStat.client["grenadeMA"]) ? "<color:ffff00>" : "<color:c8c8c8>", (%client == dtGameStat.client["grenadeMAHitDist"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         }
         if(dtGameStat.stat["mortarMA"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Mortar\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4m';
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["mortarMA"],"NA")), dtGameStat.stat["mortarMA"], StripMLControlChars(hasValueS(dtGameStat.name["mortarMAHitDist"],"NA")), mFormatFloat(dtGameStat.stat["mortarMAHitDist"], "%.2f"),(%client == dtGameStat.client["mortarMA"]) ? "<color:ffff00>" : "<color:c8c8c8>", (%client == dtGameStat.client["mortarMAHitDist"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         }
      }
      if(dtGameStat.gc["misc"] > 0){
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0> ' );
         messageClient( %client, 'MsgDebriefAddLine', "", '<color:00dc00>MISC' );
         if(dtGameStat.stat["laserHeadShot"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280,360><color:00dc00>Headhunter\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2',StripMLControlChars(dtGameStat.name["laserHeadShot"]),dtGameStat.stat["laserHeadShot"],(%client == dtGameStat.client["flagReturns"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["laserHitDist"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280,360><color:00dc00>Longest Snipe\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2m',StripMLControlChars(dtGameStat.name["laserHitDist"]),mFormatFloat(dtGameStat.stat["laserHitDist"], "%.2f"),(%client == dtGameStat.client["laserHitDist"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["shockRearShot"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280,360><color:00dc00>Taser Tailgater\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2',StripMLControlChars(dtGameStat.name["shockRearShot"]),dtGameStat.stat["shockRearShot"],(%client == dtGameStat.client["shockRearShot"]) ? "<color:ffff00>" : "<color:c8c8c8>");
         if(dtGameStat.stat["repairs"] > 0)
            messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280,360><color:00dc00>Fixer Upper\t%3<clip:150>%1</clip>\t<color:3cb4b4>%2',StripMLControlChars(dtGameStat.name["repairs"]),dtGameStat.stat["repairs"],(%client == dtGameStat.client["repairs"]) ? "<color:ffff00>" : "<color:c8c8c8>");
      }

      if(dtGameStat.gc["wep"] > 0){
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0> ' );
         messageClient( %client, 'MsgDebriefAddLine', "", '<tab:130,280,360,510><color:00dc00>\tPLAYER\tDMG\tPLAYER\tKILLS');

         if(dtGameStat.stat["blasterKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Blaster Master\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["blasterDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["blasterKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["blasterDmg"],"NA")), mFormatFloat(dtGameStat.stat["blasterDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["blasterKills"],"NA")), dtGameStat.stat["blasterKills"],%color1,%color2);
         }
         if(dtGameStat.stat["plasmaKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Plasma Roaster\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["plasmaDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["plasmaKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["plasmaDmg"],"NA")), mFormatFloat(dtGameStat.stat["plasmaDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["plasmaKills"],"NA")), dtGameStat.stat["plasmaKills"],%color1,%color2);
         }
         if(dtGameStat.stat["discKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Disc-O-maniac\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["discDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["discKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["discDmg"],"NA")), mFormatFloat(dtGameStat.stat["discDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["discKills"],"NA")), dtGameStat.stat["discKills"],%color1,%color2);
         }
         if(dtGameStat.stat["cgKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Chainwh0re\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["cgDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["cgKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["cgDmg"],"NA")), mFormatFloat(dtGameStat.stat["cgDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["cgKills"],"NA")), dtGameStat.stat["cgKills"],%color1,%color2);
         }
         if(dtGameStat.stat["hGrenadeKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Grenade puppy\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["hGrenadeDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["hGrenadeKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["hGrenadeDmg"],"NA")), mFormatFloat(dtGameStat.stat["hGrenadeDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["hGrenadeKills"],"NA")), dtGameStat.stat["hGrenadeKills"],%color1,%color2);
         }
         if(dtGameStat.stat["laserKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Laser Turret\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["laserDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["laserKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["laserDmg"],"NA")), mFormatFloat(dtGameStat.stat["laserDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["laserKills"],"NA")), dtGameStat.stat["laserKills"],%color1,%color2);
         }
         if(dtGameStat.stat["mortarKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Mortar Maniac\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["mortarDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["mortarKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["mortarDmg"],"NA")), mFormatFloat(dtGameStat.stat["mortarDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["mortarKills"],"NA")), dtGameStat.stat["mortarKills"],%color1,%color2);
         }
         if(dtGameStat.stat["missileKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Missile Lamer\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["missileDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["missileKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["missileDmg"],"NA")), mFormatFloat(dtGameStat.stat["missileDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["missileKills"],"NA")), dtGameStat.stat["missileKills"],%color1,%color2);
         }
         if(dtGameStat.stat["shockKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Shocklance Bee\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["shockDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["shockKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["shockDmg"],"NA")), mFormatFloat(dtGameStat.stat["shockDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["shockKills"],"NA")), dtGameStat.stat["shockKills"],%color1,%color2);
         }
         if(dtGameStat.stat["mineKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Mine Mayhem\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["mineDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["mineKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["mineDmg"],"NA")), mFormatFloat(dtGameStat.stat["mineDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["mineKills"],"NA")), dtGameStat.stat["mineKills"],%color1,%color2);
         }
         if(dtGameStat.stat["outdoorDepTurretKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Spike Farmer\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["outdoorDepTurretDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["outdoorDepTurretKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["outdoorDepTurretDmg"],"NA")), mFormatFloat(dtGameStat.stat["outdoorDepTurretDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["outdoorDepTurretKills"],"NA")), dtGameStat.stat["outdoorDepTurretKills"],%color1,%color2);
         }
         if(dtGameStat.stat["indoorDepTurretKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Clamp Farmer\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["indoorDepTurretDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["indoorDepTurretKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["indoorDepTurretDmg"],"NA")), mFormatFloat(dtGameStat.stat["indoorDepTurretDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["indoorDepTurretKills"],"NA")), dtGameStat.stat["indoorDepTurretKills"],%color1,%color2);
         }
         if(dtGameStat.stat["roadKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Road Killer\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["roadDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["roadKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["roadDmg"],"NA")), mFormatFloat(dtGameStat.stat["roadDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["roadKills"],"NA")), dtGameStat.stat["roadKills"],%color1,%color2);
         }
         if(dtGameStat.stat["shrikeBlasterKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Shrike Gunner\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["shrikeBlasterDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["shrikeBlasterKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["shrikeBlasterDmg"],"NA")), mFormatFloat(dtGameStat.stat["shrikeBlasterDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["shrikeBlasterKills"],"NA")), dtGameStat.stat["shrikeBlasterKills"],%color1,%color2);
         }
         if(dtGameStat.stat["bellyTurretKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Tailgunner\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["bellyTurretDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["bellyTurretKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["bellyTurretDmg"],"NA")), mFormatFloat(dtGameStat.stat["bellyTurretDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["bellyTurretKills"],"NA")), dtGameStat.stat["bellyTurretKills"],%color1,%color2);
         }
         if(dtGameStat.stat["bomberBombsKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Bomber Bombs\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["bomberBombsDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["bomberBombsKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["bomberBombsDmg"],"NA")), mFormatFloat(dtGameStat.stat["bomberBombsDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["bomberBombsKills"],"NA")), dtGameStat.stat["bomberBombsKills"],%color1,%color2);
         }
         if(dtGameStat.stat["tankChaingunKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Tank (chain)\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["tankChaingunDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["tankChaingunKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["tankChaingunDmg"],"NA")), mFormatFloat(dtGameStat.stat["tankChaingunDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["tankChaingunKills"],"NA")), dtGameStat.stat["tankChaingunKills"],%color1,%color2);
         }
         if(dtGameStat.stat["tankMortarKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Tank (mortar)\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["tankMortarDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["tankMortarKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["tankMortarDmg"],"NA")), mFormatFloat(dtGameStat.stat["tankMortarDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["tankMortarKills"],"NA")), dtGameStat.stat["tankMortarKills"],%color1,%color2);
         }
         if(dtGameStat.stat["satchelKills"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Satchel Punk\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["satchelDmg"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["satchelKills"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["satchelDmg"],"NA")), mFormatFloat(dtGameStat.stat["satchelDmg"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["satchelKills"],"NA")), dtGameStat.stat["satchelKills"],%color1,%color2);
         }
         if(dtGameStat.stat["minePlusDiscKill"] > 0){
            %line = '<tab:130,280,360,510><color:00dc00>Combo King\t%5<clip:150>%1</clip>\t<color:3cb4b4>%2\t%6<clip:150>%3</clip>\t<color:3cb4b4>%4';
            %color1 = (%client == dtGameStat.client["minePlusDisc"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            %color2 = (%client == dtGameStat.client["minePlusDiscKill"]) ? "<color:ffff00>" : "<color:c8c8c8>";
            messageClient( %client, 'MsgDebriefAddLine', "", %line, StripMLControlChars(hasValueS(dtGameStat.name["minePlusDisc"],"NA")), mFormatFloat(dtGameStat.stat["minePlusDisc"], "%.2f"), StripMLControlChars(hasValueS(dtGameStat.name["minePlusDiscKill"],"NA")), dtGameStat.stat["minePlusDiscKill"],%color1,%color2);
         }
      }
   }
}

if(!isActivePackage(dtDebrief)){
   activatePackage(dtDebrief);
}