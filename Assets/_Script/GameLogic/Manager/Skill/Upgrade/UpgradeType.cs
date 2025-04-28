
//like a visiter? a skill will "accept" an upgrader to let it modify it's data/functionality?
//it can event let me inject an other skill instance contructor to modify skill excute sequence?
//well, it mean a skill must know what can be and what can not be on an upgrade?
//like a range upgrade will just modify the range value, so it can be reuse in many place.
//but a functionality upgrade? it may become harder as a concrated inheritance of "Skill A" class can only modify this skill's behavior
//so any other skill can not re-use that behavior upgrade.
//the only solution is to make those behavior that i want to swap out a "additional module".
//the problem become the old one: which behavior is the general, the one that i "encapsule" out.
//at current level of complexity, i won't need those system yet. but let code it in a way that the future me will have a hint 
//about what the system is capable of or about how it can be extended.

public enum UpgradeType
{
    ComposeUpgrade,
    RangeUpgrade,
    TargetJellyIndexUpgrade,
    //can add functionality upgrade. like, changing the way we select target, etc
}
