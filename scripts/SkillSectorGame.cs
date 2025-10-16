// SkillSector has a README.md, read that instead.

// author: loop
// date: 10/2025
// url: https://github.com/tribes2/SkillSector

// Allows you to modify AimTrain placeholder entities.
// Leave it on when editing the map, leave it off when playing the game.
$DEVMODE = 1;

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

if ($DEVMODE) {
    moveMap.bind(keyboard, "f5", disconnect);
    ObserverHUDWeaponList.delete();
}

// Load the various modes, datablocks and functions.
exec("scripts/SkillSectorTeleporter.cs");
exec("scripts/SkillSectorAimTrainer.cs");
exec("scripts/SkillSectorTractorBeam.cs");