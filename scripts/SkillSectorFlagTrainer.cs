// TODO: flag goals

// Time in seconds
$FlagTrainerTossReset = 45;
$FlagTrainerGrabReset = 90;
$FlagTrainerCannonInterval = 25;

function Flag::objectiveInit(%data, %flag) {
   %flag.originalPosition = %flag.getTransform();
   $flagPos[%flag.team] = %flag.originalPosition;
   %flag.isHome = true;
   %flag.carrier = 0;
   %flag.grabber = 0;
}

function SkillSectorGame::flagStandCollision(%game, %dataBlock, %obj, %colObj) {
    if (%colObj.getDatablock().getName() $= "FLAG" && !%colObj.isHome) {
        returnFlag(%colObj, 'StandCollide');
    }
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
    if (%stand.cannon) {
        // Stand is a flag cannon! Fire the missiles!
        $Cannons[$CannonCount] = %stand;
        $CannonCount++;
        flagCannon(%stand);
    }
}

function flagCannon(%stand) {
    // Fire flag and schedule another flag fire
    %flag = %stand.flag;
    if (isObject(%flag)) {
        %flag.setVelocity(%stand.flagvel);
        cancel(%stand.cannonSched); // don't allow accidental re-queue
        %stand.cannonSched = schedule($FlagTrainerCannonInterval*1000, 0, flagCannon, %stand);
        returnFlagAfter(%flag, $FlagTrainerCannonInterval-2, 'CannonReset');
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
        return;
    }
    $CannonCount = 0;
    // Find all the flag stands and give them a flag
    scanGroupForFlagStands(FlagTrain);
}

function SkillSectorGame::ShutdownFlagTrainer() {
    for (%i = 0; %i < $CannonCount; %i++) {
        cancel($Cannons[%i].cannonSched);
    }
}

function SkillSectorGame::playerTouchFlag(%game, %player, %flag) {
    if (isObject(%player.holdingFlag)) {
        messageClient(%player.client, 'MsgFlagAlready', '\c0You\'re already holding a flag - don\'t be greedy!');
        return;
    }
    if (isObject(%flag.carrier)) {
        echo("Can't pick up a flag that's being carried");
        return;
    }
    %game.playerTouchEnemyFlag(%player, %flag);
}

function SkillSectorGame::playerTouchEnemyFlag(%game, %player, %flag) {
    %client = %player.client;
    %player.holdingFlag = %flag;
    %flag.carrier = %player;
    %player.mountImage(FlagImage, $FlagSlot, true, 'dsword');

    %flag.hide(true);
    %flag.startFade(0, 0, false);
    if(%flag.stand) %flag.stand.getDataBlock().onFlagTaken(%flag.stand);
    returnFlagAfter(%flag, $FlagTrainerGrabReset, 'GrabReset');
}

function SkillSectorGame::dropFlag(%game, %player) {
    %player.throwObject(%player.holdingFlag);
}

function returnFlag(%flag, %reason) {
    if (isObject(%flag.carrier)) {
        %flag.carrier.unMountImage($FlagSlot);
        %flag.hide(false);
        %flag.carrier.holdingFlag = 0; // tell the player they've lost the flag
    }
    if(%flag.stand) %flag.stand.getDataBlock().onFlagReturn(%flag.stand);
    %flag.setVelocity("0 0 0");
    %flag.setTransform(%flag.stand.getTransform());
    %flag.isHome = true;
    //messageClient(%flag.carrier.client, 'MsgFlagReturned', '\c0Flag returned (%1)', %reason); // Kinda annoying, players will figure this out eventually
    %flag.carrier = 0;
}

function returnFlagAfter(%flag, %after, %reason) {
    %flag.isHome = false; // happens whenever player tosses or picks up flag. also happens when cannon fires
    cancel(%flag.returnSched);
    %flag.returnSched = schedule(%after*1000, 0, returnFlag, %flag, %reason);
    //messageClient(%flag.carrier.client, 'MsgFlagReturnCountdown', '\c0Flag will be returned to stand in %1 seconds (%2)', %after, %reason); // Kinda annoying, players will figure this out eventually
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
