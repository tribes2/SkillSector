// I guess there's two kinds of waypoints that I want to wrangle.
// 1. Zone based waypoints
// 2. Point to point waypoints

// Zone based waypoints are perfect for rooms full of teleporters / switches / etc.
// Zone based waypoint SimGroup should have a 'zone' waypoint that is only shown when the player is outside of the zone, allowing zones to be discovered.

// Point to point waypoints are perfect for races and leading someone from one end of the map to another. I dunno, like a tutorial.

// After a lot of reading scripts, engine source code and Ghidra decompiling, I've ascertained that... we're stuck with a bit of an odd structure.
// While it's true that you can allocate 512 TargetManager targets, HUDTargetList is limited to 32 targets exactly.
// Those 32 targets are split across three categories.
// 1 - ClientTarget::AssignedTask
// 15 - ClientTarget::PotentialTask
// 16 - ClientTarget::Waypoint

// Waypoints can't be removed per-client without extensive modifications, so they're still permament.
// While it's possible to swap the clients team to show them different team waypoints, it doesn't seem worth the hassle.

// What does that mean for my original intent with this script? Well, it's still achievable... but only by spamming the client.
// PotentialTasks are shown for 2 seconds after they're received.
// The player can show those tasks again by pressing n to summon the task list.
// Instead of trying to train players to press `n` when they want directions/more information... I'll spam them.
// Sending the task every 1900ms means it will arrive well before the last task expires.
// Players will always see a task when they're in the zone, and when they leave the zone, they will expire quickly.
// The oldest task is overwritten, so if someone starts using the chat menu or command circuit to create more tasks, that will still work.

datablock TriggerData(WaypointWranglerZone) {
   tickPeriodMS = 1500;
};

function WaypointWranglerZone__onAdd(%this, %obj) {
    // New zone added for waypoint wrangling.
    echo("Obj: " @ %obj);
    echo("New group: " @ %this.getGroup());
}

function scanGroupForWWZ(%group) {
    if (%group.getName() $= "MissionCleanup") {
        // Ignoring a busy group that probably doesn't have WaypointWrangler triggers
        return;
    }
    for (%i = 0; %i < %group.getCount(); %i++) {
        %obj = %group.getObject(%i);
        if (%obj.getClassName() $= "SimGroup") {
            echo("Group detected: " @ %obj.getName());
            scanGroupForWWZ(%obj);
        } else if (%obj.getClassName() $= "Trigger" && %obj.getDatablock().getName() $= "WaypointWranglerZone") {
            echo("Waypoint detected: " @ %obj.getName());
            $WPZones[$WPZNextFree] = %obj;
            $WPZNextFree++;
        }
    }
}

function WaypointWranglerInit() {
    // Reset WPZone system
    $WPZNextFree = 0;
    $WPZones[0] = 0;
    // Iterate over every group and find relevant triggers
    scanGroupForWWZ(MissionGroup);
    // Iterate over 
    // Use zone.getGroup to get their containing group
    // Iterate over all the objects in the containing group that have a WPN (waypoint name), put them into a SimSet or array with a name relating to the containing group
    // Whenever a player enters the Trigger zone, hide the pimary waypoint associated with the zone and show the other waypoints
    // Might make sense to make a flag for 'hide all other primary waypoints inside zone' (useful for point to point races and very busy zones like the Bank)
    // This flag might get set automatically if there are too many waypoints in a zone (>10?)

    // This system should prevent players for placing waypoints if they exceed the maximum waypoint count. I don't know how that works so I probably won't do it yet.
}

// MaxTargets = 512
// HUDTargetList = 32

//%target = createTarget(%flag.waypoint, CTFGame::getTeamName( CTFGame, %flag.team), "", "", 'Base', %flag.team, 0);

function showWaypoint(%client, %wp) {
    echo(%wp.getName());
    echo(%wp.getPosition());
    %client.setTargetId(%wp.target);
    commandToClient(%client, 'TaskInfo', %client, -1, false, %wp.getName());
    commandToClient(%client, 'PotentialTask', %client.name, "wub wub", "target name");
    %client.sendTargetTo(%client, false);
    //   %clRabbit.player.scopeToClient(%cl);
    //   %visMask = getSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup());
    //   %visMask |= (1 << %cl.getSensorGroup());
    //   setSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup(), %visMask);
    //   %cl.setTargetId(%clRabbit.target);
    //   commandToClient(%cl, 'TaskInfo', %cl, -1, false, "Kill the Rabbit!");
    //   commandToClient(%targetClient, 'PotentialTask', %client.name, %client.currentTaskDescription, %targetName);
    //   %client.sendTargetTo(%targetClient, false);
    //   %cl.sendTargetTo(%cl, true);
}

function hideWaypoint() {

//     function clientCmdAcceptedTask(%description) {
//     addMessageHudLine("\c3Your current task is:\cr " @ %description);
//     }

//    commandToClient(%client, 'TaskInfo', %issueClient, -1, %issueClient.name, %description);
//    commandToClient(%client, 'AcceptedTask', %description);
   
//    %client.sendTargetTo(%client, true);
}

function debugDispatch() {
    %count = ClientGroup.getCount();
    
	for (%i = 0; %i < %count; %i++) {
		%client = ClientGroup.getObject(%i);
        echo("Client: " @ %client);
        WWDispatchWaypoints(%client);
    }
}

function WWDispatchWaypoints(%client) {
    for (%i = 0; %i < $WPZNextFree; %i++) {
        %wp = $WPZones[%i];
        showWaypoint(%client, %wp);
        //allocTarget, createTarget
        // allocClientTarget is only relevant to other players, it's for creating targets that represent client connections
        //    Con::addCommand("ClientTarget", "sendToServer",       cTargetSendToServer,       "target.sendToServer()",                  2, 2);
        //    Con::addCommand("ClientTarget", "createWaypoint",     cCreateWaypoint,           "target.createWaypoint(text)",            3, 3);
        //    Con::addCommand("ClientTarget", "addPotentialTask",   cAddPotentialTask,         "target.addPotentialTask()",              2, 2);
        //    Con::addCommand("ClientTarget", "setText",            cSetText,                  "target.setText(text)",                   3, 3);
        //    Con::addCommand("ClientTarget", "getTargetId",        cGetTargetId,              "target.getTargetId()",                   2, 2);
        //    Con::addCommand("createClientTarget",                 cCreateClientTarget,       "createClientTarget(targetId, <x y z>)",  2, 3);
        //    Con::addCommand("removeClientTargetType",             cRemoveClientTargetType,   "removeClientTargetType(client, type)",   3, 3);
        //    Con::addVariable("clientTargetTimeout", TypeS32, &HUDTargetList::smTargetTimeout);
    }
}





// Revisiting this concept by looking through the code for dlgDrawText\(.* to find all locations where text is rendered
// Hopefully I can find a more suitable way to render text for these items, but I'm not overly optimistic
// guiShapeNameHud seems relevant to 'directly looking at an object and seeing some text' but that's not quite what I want
// shapeNameHud seems similar