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

function DermDummy::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType) {
    if (%sourceObject == 0) {
        return;
    }
    echo("Damage object received: " @ %targetObject @ " from " @ %sourceObject);
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


function findAndReplacePlaceholders() {
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

// Don't replace placeholder entities in development mode
if ($DEVMODE == 0) {
    findAndReplacePlaceholders();
}