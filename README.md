# SteeringBehaviors
Implementing steering behaviour for agents (creatures).<br>
Using: [MonoGame](http://www.monogame.net)<br>
<br>
Resources:<br>
[Steering Behaviors For Autonomous Characters (whitepaper)](http://www.red3d.com/cwr/steer/)<br>
[P5.js implementation](https://github.com/shiffman/The-Nature-of-Code-Examples-p5.js/tree/master/chp06_agents)<br>
<br>
Todo:
- [ ] Seek and Flee
  - [x] Seek
  - [ ] Flee
- [ ] Pursue and Evade
- [ ] Wander
- [ ] Arrival
- [ ] Obstacle Avoidance
	- [x] Avoid obstacles
	- [x] Make it so that the creature takes more room when going around obstacles (added vision range)
	- [ ] Small bugg when 2 obstacles are inside eachother, creature will avoid 1 but go through the other in the process (creature will die when colliding when we implement genetic algorithms)
- [ ] Containment
- [ ] Wall Following
- [ ] Path Following
- [ ] Flow Field Following
- [ ] Genetic Algortihm (find the best values for maxForce, maxSpeed and visionLengh) Example: Low vision range needs high maxForce, because it will run into obstacles otherwise
	- [ ] Let creatures die when they collide with an obstacle
