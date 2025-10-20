$AimTrainDuration = 60;

datablock PlayerData(DermDummy) : LightMaleBiodermArmor {
    canObserve = false;
    groundImpactMinSpeed = 0.01;
    minImpactSpeed = 5;
    speedDamageScale = 0.004;
};

function resetDummyPosition(%this) {
    %this.setTransform(%this.original_transform);
    %this.setvelocity("0 0 0");
}

function atsessionEnd(%player) {
    echo("session ended");
    %player.aim_training = 0;
    %msg = "\c2AimTrain complete!\nScore: " @ %player.atscore;
    messageClient(%player.client, 'MsgAimTrainFinish', %msg);
    CenterPrint(%player.client, %msg, 3.5, 3);
}

function atsessionCountdown(%player, %count) {
    messageClient(%player.client, 'MsgAimTrainCountdown', '\c2AimTrain session ends in %1 second(s).~wfx/misc/hunters_%1.wav', %count);
}

function atsessionCheck(%player) {
    if (isObject(%player.client)) {
        if (%player.aim_training) {
            echo("aim training already active for player " @ %player);
        } else {
            messageClient(%player.client, 'MsgAimTrainStart', '\c2AimTrain start! You have %1 seconds.', $AimTrainDuration);
            echo("starting aim training for player " @ %player);
            %player.aim_training = 1;
            %player.atscore = 0.0;
            for (%i = 1; %i < 6; %i += 1) {
                schedule(($AimTrainDuration-%i)*1000, 0, "atsessionCountdown", %player, %i);
            }
            schedule($AimTrainDuration*1000, 0, "atsessionEnd", %player);
        }
    }
}

// $DamageType::Default= 0; $DamageType::Blaster= 1; $DamageType::Plasma= 2; $DamageType::Bullet= 3; $DamageType::Disc= 4; $DamageType::Grenade= 5; $DamageType::Laser= 6; $DamageType::ELF= 7; $DamageType::Mortar= 8; $DamageType::Missile= 9; $DamageType::ShockLance= 10; $DamageType::Mine= 11; $DamageType::Explosion= 12; $DamageType::Impact= 13;	// Object to object collisions $DamageType::Ground= 14;	// Object to ground collisions $DamageType::Turret= 15;
function calcScore(%targetObject, %sourceObject, %damageType, %damage, %position) {
    // This is kinda half assed. But what more do you really need?
    %score = %damage * 100;
    %directHitOnly = !$AimTrainLowDummies.isMember(%targetObject);
	switch$(%damageType) {
        case $DamageType::Disc:
            if (%damage != 0.5 && %directHitOnly) {
                echo("Indirect hit, doesn't count");
                return 0;
            }
    }
    %sourceObject.atscore += %score;
    BottomPrint(%sourceObject.client, "DMG: " @ %damage @ " TYPE: " @ %damageType @ " SCORE: " @ %score @ " DHO " @ %directHitOnly, 2, 3);
}

function DermDummy::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType) {
    if (%sourceObject == 0) {
        // Fall damage
        return;
    }
    echo("Damage object received: " @ %targetObject @ " from " @ %sourceObject);

    // find out if the player is already in a score session
    atsessionCheck(%sourceObject);

    // how much damage was received?
    calcScore(%targetObject, %sourceObject, %damageType, %amount, %position);

    // Reset training dummy
    cancel(%targetObject.resetter); // Avoid double resets
    // 750 allows for the original splash damage to decay
    // 500 results in a double reset because the dummy is hit by the original explosion twice
    %targetObject.resetter = schedule(750, 0, "resetDummyPosition", %targetObject);
}

// function TSStatic::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType) {
//     echo("Damage object received: " @ %targetObject.name);
// }

// function TSStatic::damage(%this, %sourceObject, %position, %amount, %damageType) {
//     echo("wtf is this then: " @ %this @ " tgt " @ %sourceObject);
// }

function AimTrainerInit() {
    if (!$DEVMODE) {
        findAndReplacePlaceholders();
    }
}

function findAndReplacePlaceholders() {
    // This is not re-entrant safe, don't call it more than once!
    $AimTrainLowDummies = new SimSet();
    MissionCleanup.add($AimTrainLowDummies);
    // Replace placeholders in the aim training system
    %trash = new SimSet();
    for (%i = 0; %i < AimTrainLow.getCount(); %i += 1) {
        if (AimTrainLow.getObject(%i).placeholder) {
            %obj = AimTrainLow.getObject(%i);
            echo("Replace : " @ %obj.getName());
            %trans = %obj.getTransform();
            %replace = new Player() {
                datablock = "DermDummy";
            };
            AimTrainLow.add(%replace);
            $AimTrainLowDummies.add(%replace);
            %replace.setName(%obj.getName());
            %replace.setTransform(%trans);
            %replace.original_transform = %trans;
            %trash.add(%obj);
        }
    }
    while (%trash.getCount() > 0) {
        %obj = %trash.getObject(0);
        %obj.delete();
    }
    %trash.delete();
}
