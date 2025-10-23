// TODO: flag cannon
// TODO: flag collide with stand, return to stand
// TODO: flag goals

// Time in seconds
$FlagTrainerTossReset = 45;
$FlagTrainerGrabReset = 90;

function Flag::objectiveInit(%data, %flag) {
   %flag.originalPosition = %flag.getTransform();
   $flagPos[%flag.team] = %flag.originalPosition;
   %flag.isHome = true;
   %flag.carrier = 0;
   %flag.grabber = 0;
}

function initFlagStand(%stand) {
    // Give the stand a flag
    echo("Found a stand");
    %flag = new Item() {
        position = %stand.getPosition();
        className = FlagObj;
        dataBlock = "FLAG";
        static = false;
        rotate = true;
    };
    FlagTrain.add(%flag);
    %stand.flag = %flag;
    %flag.stand = %stand;
    if(%flag.stand) {
        %flag.stand.getDataBlock().onFlagReturn(%flag.stand);
    }
}

function scanGroupForFlagStands(%group) {
    for (%i = 0; %i < %group.getCount(); %i++) {
        %obj = %group.getObject(%i);
        if (%obj.getClassName() $= "SimGroup") {
            scanGroupForFlagStands(%obj);
        } else if (%obj.getClassName() $= "StaticShape" && %obj.getDatablock().getName() $= "ExteriorFlagStand") {
            initFlagStand(%obj);
        }
    }
}

function SkillSectorGame::InitFlagTrainer() {
    if ($DEVMODE) {
        // echo("Not putting a flags on stands");
        // return;
    }
    // Find all the flag stands and give them a flag
    scanGroupForFlagStands(FlagTrain);
}

function SkillSectorGame::playerTouchFlag(%game, %player, %flag) {
    if (isObject(%player.holdingFlag)) {
        echo("Already holding a flag");
        return;
    }
    if (isObject(%flag.carrier)) {
        echo("Can't pick up a flag that's being carried")
        return;
    }
    %game.playerTouchEnemyFlag(%player, %flag);
//    %client = %player.client;
//    if ((%flag.carrier $= "") && (%player.getState() !$= "Dead"))
//    {
//       // z0dd - ZOD, 5/07/04. Cancel the lava return.
//       if(isEventPending(%obj.lavaEnterThread)) 
//          cancel(%obj.lavaEnterThread);

//       //flag isn't held and has been touched by a live player
//       if (%client.team == %flag.team)
//          %game.playerTouchOwnFlag(%player, %flag);
//       else
//          %game.playerTouchEnemyFlag(%player, %flag);
//    }
//    // toggle visibility of the flag
//    setTargetRenderMask(%flag.waypoint.getTarget(), %flag.isHome ? 0 : 1);
}

function SkillSectorGame::playerTouchEnemyFlag(%game, %player, %flag) {
    %client = %player.client;
    %player.holdingFlag = %flag;  //%player has this flag
    %flag.carrier = %player;  //this %flag is carried by %player
    %player.mountImage(FlagImage, $FlagSlot, true, 'dsword');

   %flag.hide(true);
   %flag.startFade(0, 0, false);
   if(%flag.stand)
      %flag.stand.getDataBlock().onFlagTaken(%flag.stand);//animate, if exterior stand
    returnFlagAfter(%flag, $FlagTrainerGrabReset, 'GrabReset');
}

function SkillSectorGame::dropFlag(%game, %player)
{
    %player.throwObject(%player.holdingFlag);
//    if(%player.holdingFlag > 0)
//    {
//       if (!%player.client.outOfBounds)
//          %player.throwObject(%player.holdingFlag);
//       else
//          %game.boundaryLoseFlag(%player);
//    }
}

function returnFlag(%flag, %reason) {
    if (isObject(%flag.carrier)) {
        %flag.carrier.unMountImage($FlagSlot);
        %flag.hide(false);
        %flag.carrier.holdingFlag = 0; // tell the player they've lost the flag
    }
    %flag.stand.getDataBlock().onFlagReturn(%flag.stand);
    %flag.setTransform(%flag.stand.getTransform());
    //messageClient(%flag.carrier.client, 'MsgFlagReturned', '\c0Flag returned (%1)', %reason);
    %flag.carrier = 0;
}

function returnFlagAfter(%flag, %after, %reason) {
    cancel(%flag.returnSched);
    %flag.returnSched = schedule(%after*1000, 0, returnFlag, %flag, %reason);
    //messageClient(%flag.carrier.client, 'MsgFlagReturnCountdown', '\c0Flag will be returned to stand in %1 seconds (%2)', %after, %reason);
}

function SkillSectorGame::playerDroppedFlag(%game, %player) {
    %client = %player.client;
    %flag = %player.holdingFlag;

    %player.unMountImage($FlagSlot);
    %flag.hide(false);
    
    %flag.setTransform(%flag.getTransform());
    returnFlagAfter(%flag, $FlagTrainerTossReset, 'TossReset');
    %player.holdingFlag = 0;
    %flag.carrier = 0;
}
