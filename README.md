# T-Rex-Run-AI
A small AI which captures the screen input and emulates keypresses to play the TRex Run game

## Usage
```
In the Program.cs file, set the value of "screenshotArea" to the rectangle in which the TRex game is located. Allign the rectangle so that horizontally the entire game is visible and vertically it captures the area that starts just above the TRex's feet and ends above the scoreboard. The rectangle position is set relatively to the top-left corner of the screen. To aid the allignment process, after running the app, look for "testCapture.png" in the executable folder to see the captured area.

After running the app, make sure to focus on the game by clicking on it. The delay before the AI starts playing the game is 10 seconds after launching the game. For best results, the game should be in the "Game Over" state when launching the app.
