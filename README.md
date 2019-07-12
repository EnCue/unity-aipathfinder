# unity-aipathfinder
 NPC movement script for player tracking obstacle avoidance and path finding


Instructions: The public variables "chaseSpeed," and "idleDistance" must be assigned
on the npcMovement script. These variables determine the speed of the NPC when following
the player, and the minimum proximity to the player (at what distance from the player 
the NPC will stop), respectively. Additionally, the "Target" variable sets the
GameObject which the NPC will follow. This can be done in either the inspector, or
through another script, as the variable is public.

To designate an obstacle for NPCs, a child gameobject must be added to the initial
object, with a collider. This gameobject must be tagged as "Scenery," and will
determine the borders with its collider.


A functional version of the dialogue system can be found under the "Unity Demonstration" folder of the repository.
