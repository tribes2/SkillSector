// I guess there's two kinds of waypoints that I want to wrangle.
// 1. Zone based waypoints
// 2. Point to point waypoints

// Zone based waypoints are perfect for rooms full of teleporters / switches / etc.
// Point to point waypoints are perfect for races and leading someone from one end of the map to another. I dunno, like a tutorial.

// Waypoints don't seem to be removable per-client without extensive modifications, so they're permanent.
// While it's possible to swap the clients team to show them different team waypoints, I like everyone being on the same sensor network.

// After a lot of reading scripts, engine source code and Ghidra decompiling, I've ascertained that... we're stuck with a bit of an odd structure.
// While it's true that you can allocate 512 TargetManager targets, HUDTargetList is limited to 32 targets exactly.
// Those 32 targets are split across three categories.
// 1 - ClientTarget::AssignedTask
// 15 - ClientTarget::PotentialTask
// 16 - ClientTarget::Waypoint

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

function WaypointWranglerZone::onEnterTrigger(%data, %trigger, %obj) {
    if(%obj.getDataBlock().className !$= "Armor") {
        return;
    }
    echo("Trigger entered!");
    WWStartWaypointDispatch(%obj.client, %trigger);
}

function WaypointWranglerZone::onLeaveTrigger(%this, %trigger, %obj) {
    cancel(%obj.client.wwsched);
}

function WaypointWranglerZone::onTickTrigger(%this, %trigger) {
}

function CLWaypointWrangler(%client) {
    // cancel any ongoing waypoint wrangler scheduler
    cancel(%client.wwsched);
}

function ShutdownWaypointWrangler() {
    %count = ClientGroup.getCount();
    for (%i = 0; %i < %count; %i++) {
        cancel(ClientGroup.getObject(%i).wwsched);
    }
}

function scanZoneForWaypoints(%group) {
    for (%i = 0; %i < %group.getCount(); %i++) {
        %obj = %group.getObject(%i);
        if (%obj.getClassName() $= "SimGroup") {
            echo("Traversing detected group: " @ %obj.getName());
            scanZoneForWaypoints(%obj);
        } else if (%obj.wrangle !$= "") {
            // echo("Found waypointable object in zone: " @ %obj.getName() @ " txt: " @ %obj.wrangle);
            $WPZPoints[$WPZNextFree, $WPIndex] = %obj;
            $WPIndex++;
        }
    }
}

function initWranglerZone(%obj) {
    echo("Waypoint zone detected: " @ %obj.getName() @ " tgt: " @ %obj.target);
    // Iterate over the containing group and find wrangled objects
    $WPIndex = 1;
    scanZoneForWaypoints(%obj.getGroup());
    // zero index is reserved for 'count of indexed waypoints'
    $WPZPoints[$WPZNextFree, 0] = $WPIndex-1;
    $WPZones[$WPZNextFree] = %obj;
    $WPZNextFree++;
}

function scanGroupForWWZ(%group) {
    if (%group.getName() $= "MissionCleanup") {
        // Ignoring a busy group that probably doesn't have WaypointWrangler triggers
        return;
    }
    for (%i = 0; %i < %group.getCount(); %i++) {
        %obj = %group.getObject(%i);
        if (%obj.getClassName() $= "SimGroup") {
            echo("Traversing detected group: " @ %obj.getName());
            scanGroupForWWZ(%obj);
        } else if (%obj.getClassName() $= "Trigger" && %obj.getDatablock().getName() $= "WaypointWranglerZone") {
            initWranglerZone(%obj);
        }
    }
}

function InitWaypointWrangler() {
    // Reset WPZone system
    $WPZNextFree = 0;
    $WPZones[0] = 0;
    $WPZPoints[0, 0] = 0;
    // Iterate over every group and find relevant triggers
    scanGroupForWWZ(MissionGroup);
}

function showWaypoint(%client, %wp) {
    // echo("Trying to emit waypoint for " @ %wp @ " and client " @ %client);
    %client.setTargetId(%wp.target);
    commandToClient(%client, 'TaskInfo', %client, -1, false, %wp.wrangle);
    // PotentialTask just shows a message in chat, not desirable for this configuration
    //commandToClient(%client, 'PotentialTask', %client.name, "", "target name");
    %client.sendTargetTo(%client, false);
}

function WWDispatch(%client, %zoneIndex) {
    for (%i = 1; %i < $WPZPoints[%zoneIndex, 0]+1; %i++) {
        showWaypoint(%client, $WPZPoints[%zoneIndex, %i]);
    }
    cancel(%client.wwsched); // can't be dispatched for more than one zone at once
    %client.wwsched = schedule(1900, 0, WWDispatch, %client, %zoneIndex);
}

// This function is unnecessarily expensive, this lookup could be done by attaching the zone and index to the trigger.
// I'm also too lazy to re-write it now
function WWStartWaypointDispatch(%client, %zone) {
    // echo("zone count:" @ $WPZNextFree);
    for (%i = 0; %i < $WPZNextFree; %i++) {
        %wpz = $WPZones[%i];
        // echo("testing " @ %zone @ " against " @ %wpz);
        if (%wpz == %zone) {
            // echo("Zone found! idx " @ %i @ " ct: " @ $WPZPoints[%i, 0]);
            WWDispatch(%client, %i);
        }
    }
}
