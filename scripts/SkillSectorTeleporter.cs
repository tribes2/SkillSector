// stolen from classic
datablock StaticShapeData(BankTeleporter): StaticShapeDamageProfile {
   className = Station;
   catagory = "Stations";
   shapeFile = "nexusbase.dts";
   maxDamage = 1.20;
   destroyedLevel = 1.20;
   disabledLevel = 0.84;
   explosion = ShapeExplosion;
   expDmgRadius = 10.0;
   expDamage = 0.4;
   expImpulse = 1500.0;
   dynamicType = $TypeMasks::StationObjectType;
   isShielded = true;
   energyPerDamagePoint = 33;
   maxEnergy = 250;
   rechargeRate = 0.31;
   humSound = StationVehicleHumSound;
   // don't let these be damaged in Siege missions
   noDamageInSiege = true;
   cmdCategory = "Support";
   cmdIcon = CMDVehicleStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_vehicle_pad_inventory";
   targetNameTag = 'MPB';
   targetTypeTag = 'Teleport Station';
   teleporter = 1;
};

//datablock ParticleData(mpbteleportparticle)
//datablock ParticleEmitterData(MPBTeleportEmitter)

function BankTeleporter::onCollision(%data, %obj, %collider) {
    // Teleporters are generally one way in Skill Sector
    // Teleporter entities are linked to 'spawn point' objects
    echo("BankTeleporter collision: " @ %obj @ " and collider: " @ %collider);
    echo("Banker: " @ %obj.desc);
    if (%obj.disabled) {
        messageClient(%collider.client, 'MsgStationDenied', '\c2Teleporter is recharging please stand by. ~wfx/powered/nexus_deny.wav');
        return;
    }
   //messageClient(%collider.client, 'MsgTeleportStart', '\c2Teleporter is calculating transport coherence... ~wfx/misc/nexus_idle.wav');
    %collider.setVelocity("0 0 0");
    %collider.setMoveState(true);
    %collider.startFade(1000, 0, true);
    %collider.playAudio($ActivateThread, StationVehicleAcitvateSound);
    %obj.disabled = 1;
    %obj.setThreadDir($ActivateThread, 1);
    %obj.playThread($ActivateThread, "activate");

    %data.sparkEmitter(%obj);
    %data.schedule(1500, "teleportout", %obj, %collider);
    %data.schedule(3000, "teleportingDone", %obj, %collider);
    if (%obj.desc !$= "") {
        echo("isObject passed");
        echo(%obj.desc);
        centerprint(%collider.client, %obj.desc, 5, 3);
    }
}

function BankTeleporter::teleportOut(%data, %obj, %player) {
    if(isObject(%obj.destination)) {
        %player.setTransform(%obj.destination.getTransform());
    } else {
        messageClient(%player.client, 'MsgTeleFailed', 'No valid teleporting destination.');
        %player.teleporting = 0;
    }
    %data.schedule(1000, "teleportIn", %player);
}

function BankTeleporter::teleportIn(%data, %player) {
    messageClient(%collider.client, 'MsgTeleportStart', '\c2Teleport to '@ %data.destination @' complete! ~wfx/powered/nexus_idle.wav');
   %data.sparkEmitter(%player); // z0dd - ZOD, 4/24/02. teleport sparkles
   %player.startFade(1000, 0, false);
   %player.playAudio($PlaySound, StationVehicleDeactivateSound);
}

function BankTeleporter::reEnable(%data, %obj) {
   %obj.disabled = 0;
}

function BankTeleporter::sparkEmitter(%data, %obj) {
   if (isObject(%obj.teleportEmitter))
      %obj.teleportEmitter.delete();

   %obj.teleportEmitter = new ParticleEmissionDummy() {
      position = %obj.position;
      rotation = "1 0 0 0";
      scale = "1 1 1";
      dataBlock = defaultEmissionDummy;
      emitter = "MPBTeleportEmitter";
      velocity = "1";
   };
   MissionCleanup.add(%obj.teleportEmitter);
   %obj.teleportEmitter.schedule(800, "delete");

   if (isObject(%obj.teleEmitter))
      %obj.teleEmitter.delete();

   %obj.teleEmitter = new ParticleEmissionDummy() {
      position = %obj.position;
      rotation = "1 0 0 0";
      scale = "1 1 1";
      dataBlock = defaultEmissionDummy;
      emitter = "FlyerJetEmitter";
      velocity = "1";
   };
   MissionCleanup.add(%obj.teleEmitter);
   %obj.teleEmitter.schedule(700, "delete");
}

function BankTeleporter::teleportingDone(%data, %obj, %player)
{
   %player.setMoveState(false);
   %player.teleporting = 0;
   %player.station = "";
   %data.reEnable(%obj);
   if(%player.getMountedImage($WeaponSlot) == 0)
   {
      if(%player.inv[%player.lastWeapon])
         %player.use(%player.lastWeapon); 
      else
         %player.selectWeaponSlot(0);
   }
}
