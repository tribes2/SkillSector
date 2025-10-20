// SkillSector has a README.md, read that instead.

// author: loop
// date: 10/2025
// url: https://github.com/tribes2/SkillSector

// Allows you to modify AimTrain placeholder entities.
// Leave it on when editing the map, leave it off when playing the game.
$DEVMODE = 1;

// Load the various modes, datablocks and functions.
exec("scripts/SkillSectorTeleporter.cs");
exec("scripts/SkillSectorAimTrainer.cs");
exec("scripts/SkillSectorTractorBeam.cs");
exec("scripts/SkillSectorWaypointWrangler.cs");

package SkillSector {
    function none() {}
};

function SkillSector::initGameVars(%game) {
    AimTrainerInit();
    WaypointWranglerInit();
}

// No longer dispatching 'primary' waypoints because they can't be made semi-permanent.
function SkillSector::clientMissionDropReady(%game, %client) {
   messageClient(%client, 'MsgClientReady',"", %game.class);
//    %game.resetScore(%client);
//    for(%i = 1; %i <= %game.numTeams; %i++) {
//       $Teams[%i].score = 0;
//       messageClient(%client, 'MsgCTFAddTeam', "", %i, %game.getTeamName(%i), $flagStatus[%i], $TeamScore[%i]);
//    }
   //%game.populateTeamRankArray(%client);
   //messageClient(%client, 'MsgYourRankIs', "", -1);
   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName); 
//    WWDispatchWaypoints(%client);
   DefaultGame::clientMissionDropReady(%game, %client);
}

if ($DEVMODE) {
    moveMap.unbind(keyboard, "f5");
    moveMap.unbind(keyboard, "f6");
    moveMap.bind(keyboard, "f5", dc);
    moveMap.bind(keyboard, "f6", ssrl);
    ObserverHUDWeaponList.delete();
}

// thanks DarkTiger (you can prob list them all via datablockGroup.getCount(); and iterate them all and do echo %obj.getName();)
function dumpDatablockNames() {
    for (%i = 0; %i < datablockgroup.getCount(); %i += 1) {
        echo("Datablock " @ %i @ " named " @ datablockgroup.getObject(%i).getName());
    }
}

// DEVMODE reload/test function
function ssl() {
    exec("scripts/SkillSectorGame.cs");
    findAndReplacePlaceholders();
}
function ssrl() {
    exec("scripts/SkillSectorGame.cs");
}

function dc() {
    disconnect();
}
