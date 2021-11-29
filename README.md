# toolin

This tool will allow enemies, or other objects to find specific points and avoid obstacles.
The scripts should not be modified unless additional functionality is required, the only script that should be editted for your needs is the Unit script.
The unit script handles implimentation of the scripts, it currently does nothing. To see the pathfinding in action use the commented out code in the unit script:
    // Example of how to call the movement within the script
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Backspace))
    //    {
    //        MoveUnit(currentTarget);
    //    }
    //}
    
Uncomment this code and pressing space bar will make the seeker object in the example scene follow the player. The player cannot move around as it has no controller.
But if manually moved around the scene using the editor, you can see the enemy updates its position accordingly.

Alternatively insert the MoveUnit function in update and an enemy will follow a set target, but it will need a reference to a target.
