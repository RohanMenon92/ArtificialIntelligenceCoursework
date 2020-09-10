## Tank FSM and ML Agents
For the AI course I decided to write a program that generates an enemy tank AI which isable to respond to player actions and figure out how to fire and block projectiles. The player decides the movement of the tank, firing a bullet and using a shield to block anattack. 

The FSM agent patrols and attacks when the player is in range. If it has low health, it will try to run away and shield after shooting.

The application used to showcase and train the AI is Unity with C# as the primaryscripting language for Unity. For training and using the ML-Agents algorithm and togenerate a usable Neural Network, I am using the ml agents package provided by Unityto use Tensorflow provided by google with Python as the primary scripting language.

The key learning for the AI should be to move towards the player and when looking atthe player, it should try to shoot. The game is relatively straight forward but defining anAI that does these movements in a “non predictable” fashion is quite challenging due tothe simplicity of the game.

Video: https://youtu.be/4TnemHMEzzQ
Report: https://github.com/RohanMenon92/ArtificialIntelligenceCoursework/blob/master/Report%20(Updated).pdf

## Scenes
There are 3 scenes:
- FSMScene: Play against a tank controlled by a finite state machine
- MLAgentFSMScene: To train a ML agent against the FSM
- MLAgentMLScene: To train an ML Agent against another ML Agent
- MLPlayingScene: Play against an ML agent

## Instructions
- W,A,S,D to move
- SPACE to shoot
- Shift to shield

## Screenshots
![Agent fighting](https://github.com/RohanMenon92/ArtificialIntelligenceCoursework/blob/master/Screenshots/Agents%20fighting.PNG)
![FSM Scene](https://github.com/RohanMenon92/ArtificialIntelligenceCoursework/blob/master/Screenshots/FSMScene.PNG)
![Training](https://github.com/RohanMenon92/ArtificialIntelligenceCoursework/blob/master/Screenshots/IncreasingRewards.PNG)
