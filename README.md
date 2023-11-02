# SnakeGame
A minimal snake game that runs in the console. I made it to practice software development.


![Game Footage](./Imgs/SnakeGame.gif)
### Note: There is no flickering in the actual game.

# SnakeGame V2 (TODO)
Ok, so after seeing what the Google team made with the snake game (they made little animations for it when the sneak eats the apple) I needed it in my own game too. The hardest part is that my game runs at 15 Fps (Frames per second) so doing animations is impossible because they will seem like lagging. I decided to rewrite the game to use floats when the snake moves and set the framerate to 60 Fps this will help make the animations appear smother.

- Rewrite the game so it supports higher Fps. (Currently, in each frame the snake moves 1 block if you set the framerate to 60 the snake will move 60 blocks in 1 second instantly killing himself)
- When i generate the food i collect all the cells where the food can be. Rewrite it so i have an array that is updated when the snake moves, so i don't have to regenerate it every frame.
- Move the Console stuff out of the Game class i want to make a Winform port too.
