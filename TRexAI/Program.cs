using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TRexAI
{
    class Program
    {
        //static int yJumpingThreshold = 15;//If the obstacle starts above this height, the T-Rex should crouch instead of jumping
        static int yJumpingThreshold = 5;//If the obstacle starts above this height, the T-Rex should crouch instead of jumping
        static List<GameObject> trackedObjects = new List<GameObject>();
        static Dinosaur dinosaur;
        static bool dinosaurGroundedButNotCrouched;

        static void UpdateListOfTrackedObjects(List<GameObject> newObjects)
        {
            dinosaurGroundedButNotCrouched = false;
            //First find the dinosaur in the list of new objects and remove it
            for (int i = 0; i < newObjects.Count; i++)
            {
                //Take into account the size of the dinosaur in normal and crouched states
                if (newObjects[i].position.x == dinosaur.position.x && (newObjects[i].size.x == dinosaur.size.x || newObjects[i].size.x == dinosaur.size.x + 15))
                {
                    //Check if dinosaur is on the ground, but not crouched
                    if (newObjects[i].position.y == 0 && newObjects[i].size.x == dinosaur.size.x) dinosaurGroundedButNotCrouched = true;

                    newObjects.RemoveAt(i);
                    break;
                }

                //Show error if the dinosaur wasn't found
                if (i == newObjects.Count - 1)
                {
                    Console.WriteLine("ERROR: Dinosaur not found");
                    return;
                }
            }

            //Sort the rest of the objects by X position
            newObjects.Sort((obj1, obj2) => {
                if (obj1.position.x > obj2.position.x) return 1;
                if (obj1.position.x == obj2.position.x) return 0;
                return -1;
            });

            var tempObjectList = new List<GameObject>(trackedObjects);
            trackedObjects.Clear();
            //Track the objects and check their velocity
            for (int i = 0; i < newObjects.Count; i++)
            {
                var previousObstacle = tempObjectList.Find(obj => { return (obj.size.x == newObjects[i].size.x && obj.position.x >= newObjects[i].position.x); });
                if (previousObstacle == null)
                {
                    //Put the new object in the list
                    trackedObjects.Add(newObjects[i]);
                }
                else
                {
                    //Check if we picked up a wrong obstacle
                    if (previousObstacle.position.x - newObjects[i].position.x > 20)
                        Console.WriteLine("ERROR, invalid tracked object");

                    //Remove the object from the temporary list, we don't need it anymore
                    tempObjectList.Remove(previousObstacle);

                    //Updated the object speed, position and put it back in the tracked object list
                    var speed = previousObstacle.position.x - newObjects[i].position.x;
                    if (previousObstacle.speedFramesTracked < 5)
                    {
                        if (Math.Abs(speed - previousObstacle.speed) <= 1)
                            previousObstacle.speedFramesTracked++;
                        else
                        {
                            previousObstacle.speed = speed;
                            previousObstacle.speedFramesTracked = 0;
                        }
                    }
                    previousObstacle.position = newObjects[i].position;
                    trackedObjects.Add(previousObstacle);
                }
            }
        }

        static void RunGameLogic()
        {
            //If we have obstacles on the map, and they are to the right of the dinosaur and we are approaching them, jump or crouch.
            if (trackedObjects.Count > 0 && trackedObjects[0].position.x + trackedObjects[0].size.x > dinosaur.position.x
                && trackedObjects[0].position.x - trackedObjects[0].speed * 10 < dinosaur.position.x + dinosaur.size.x)
            {
                if (trackedObjects[0].position.y < yJumpingThreshold)
                {
                    KeypressManager.UnCrouch();
                    KeypressManager.Jump();
                    Console.WriteLine("Jump");
                }
            }
            
            if (trackedObjects.Count == 0 || trackedObjects[0].position.x + trackedObjects[0].size.x < dinosaur.position.x
                || trackedObjects[0].position.x - trackedObjects[0].speed * 10 > dinosaur.position.x + dinosaur.size.x)
            {
                KeypressManager.Crouch(dinosaurGroundedButNotCrouched);
                Console.WriteLine("Crouch");
            }



            Console.WriteLine(trackedObjects.Count);
            for (int i = 0; i < trackedObjects.Count; i++)
                if (trackedObjects[i].speed > 0)
                    Console.WriteLine("position = " + trackedObjects[i].position + "; speed = " + trackedObjects[i].speed);
        }

        static void Main(string[] args)
        {
            //TO DO: Load screenshot area, y threshold for jumping and startup delay from config file

            //Wait for some time to allow the user to switch apps
            Thread.Sleep(10000);
            //Rectangle screenshotArea = new Rectangle(500, 150, 600, 120);
            Rectangle screenshotArea = new Rectangle(650, 120, 600, 120);

            //Restart the game by jumping once and then wait for it to start
            KeypressManager.TapJump();
            Thread.Sleep(1000);

            //Start Tracking Game Objects and save the tracked game objects in a dictionary
            var image = ScreenShotCapture.GetGrayscaleScreenCapture(screenshotArea, true);
            var objects = ImageProcessing.DetectObjectsInImage(image, screenshotArea.Width, screenshotArea.Height);
            if (objects.Count == 0)
            {
                Console.WriteLine("ERROR: No Objects Found");
                return;
            }

            objects.Sort((obj1, obj2) => {
                if (obj1.position.x > obj2.position.x) return 1;
                if (obj1.position.x == obj2.position.x) return 0;
                return -1;
            });

            //When starting the game, the first object is the dinosaur because it is the leftmost object in the frame.
            dinosaur = new Dinosaur(objects[0]);

            //Add the rest of the objects to the dictionary
            for (int i = 1; i < objects.Count; i++)
                trackedObjects.Add(objects[i]);
                
            
            while (true)
            {
                image = ScreenShotCapture.GetGrayscaleScreenCapture(screenshotArea, false);
                objects = ImageProcessing.DetectObjectsInImage(image, screenshotArea.Width, screenshotArea.Height);
                UpdateListOfTrackedObjects(objects);
                RunGameLogic();
                KeypressManager.RunUpdate();
            }
        }
    }
}
