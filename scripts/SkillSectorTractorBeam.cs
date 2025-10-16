// added for the TractorBeam PhysicalZone
datablock ForceFieldBareData(TractorBeamFF)
{
   fadeMS           = 1000;
   baseTranslucency = 0.30;
   powerOffTranslucency = 0.30;
   teamPermiable    = true;
   otherPermiable   = true;
   color            = "0.0 0.55 0.99";
   powerOffColor    = "0.0 0.0 0.0";
//    targetNameTag    = 'Force Field';
//    targetTypeTag    = 'ForceField'; 

//    texture[0] = "skins/forcef1";
//    texture[1] = "skins/forcef2";
//    texture[2] = "skins/forcef3";
//    texture[3] = "skins/forcef4";
//    texture[4] = "skins/forcef5";

   framesPerSec = 5;
   numFrames = 5;
   scrollSpeed = 15;
   umapping = 1.0;
   vmapping = 0.15;
};

// Original 'tractor beam' physical zone
// new PhysicalZone(TractorBeamFront) {
// 	position = "-142.042 106.838 82.3519";
// 	rotation = "1 0 0 0";
// 	scale = "1 1 1";
// 	velocityMod = "1";
// 	gravityMod = "-3";
// 	appliedForce = "0 0 0";
// 	polyhedron = "0.0000000 0.0000000 0.0000000 10.0000000 0.0000000 0.0000000 -0.0000000 -40.0000000 -0.0000000 -0.0000000 -0.0000000 160.0000000";
// };
function TractorBeamFF::onAdd(%data, %obj)
{
    GameBaseData::onAdd(%data, %obj);
   // z0dd - ZOD, 5/09/04. From Syrinx mod - Associate this PZ with the force field directly
   %obj.pz = new PhysicalZone() {
      position = %obj.position;
      rotation = %obj.rotation;
      scale    = %obj.scale;
      polyhedron = "0.000000 1.0000000 0.0000000 1.0000000 0.0000000 0.0000000 0.0000000 -1.0000000 0.0000000 0.0000000 0.0000000 1.0000000";
      velocityMod  = %obj.velocityMod;
      gravityMod   = %obj.gravityMod;
      appliedForce = %obj.appliedForce;
      ffield = %obj;
   };
   %obj.originalscale = %obj.getscale();
   %pzGroup = nameToID("MissionCleanup/PZones");
   if(%pzGroup <= 0)
   {
      %pzGroup = new SimGroup("PZones");
      MissionCleanup.add(%pzGroup);
   }
   %pzGroup.add(%obj.pz);
}