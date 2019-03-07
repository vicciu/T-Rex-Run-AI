using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRexAI
{
    public static class ImageProcessing
    {
        //If the point is inside the image and the color coresponds to an obstacle, return true; otherwise false
        static bool IsPointObstacle(Vector2 position, byte[] rawImage, int imageWidth, int imageHeight, bool isNight)
        {
            //Check if position is outside frame
            if (position.x < 0 || position.y < 0 || position.x >= imageWidth || position.y >= imageHeight) return false;

            //Check if the color of the point doesn't indicate an obstacle
            var color = rawImage[position.y * imageWidth + position.x];
            if ((color < 128 && !isNight) || (color > 128 && isNight)) return true;

            return false;
        }

        //Check if the point is an obstacle and mark it as processed. Return true if the point was part of an obstacle; false otherwise
        static bool CheckAndMarkObstacle(Vector2 position, byte[] rawImage, int imageWidth, int imageHeight, bool isNight)
        {
            //check if point is an obstacle
            if (!IsPointObstacle(position, rawImage, imageWidth, imageHeight, isNight)) return false;

            //mark the point as processed
            rawImage[position.y * imageWidth + position.x] = isNight ? (byte)0 : (byte)255;
            return true;
        }

        static void ProcessDetectedObject(Vector2 startPosition, GameObject obj, byte[] rawImage, int imageWidth, int imageHeight, bool isNight)
        {
            Stack<Vector2> processedCoordinates = new Stack<Vector2>();
            processedCoordinates.Push(startPosition);

            while (processedCoordinates.Count > 0)
            {
                var currentPosition = processedCoordinates.Pop();

                //If the current point defines new boundaries for the object, save the new boundaries
                if (currentPosition.x < obj.position.x)
                {
                    obj.size.x += obj.position.x - currentPosition.x;
                    obj.position.x = currentPosition.x;
                }

                if (currentPosition.y < obj.position.y)
                {
                    obj.size.y += obj.position.y - currentPosition.y;
                    obj.position.y = currentPosition.y;
                }

                if (currentPosition.x >= obj.position.x + obj.size.x)
                    obj.size.x = currentPosition.x - obj.position.x + 1;

                if (currentPosition.y >= obj.position.y + obj.size.y)
                    obj.size.y = currentPosition.y - obj.position.y + 1;

                //Find nearby points that are part of the same object and add them to the stack
                //TO DO: Find a cleaner way to search for the next point
                var nextPosition = new Vector2(currentPosition.x + 1, currentPosition.y);
                var wasObstacle = CheckAndMarkObstacle(nextPosition, rawImage, imageWidth, imageHeight, isNight);
                if (wasObstacle) processedCoordinates.Push(nextPosition);

                nextPosition = new Vector2(currentPosition.x - 1, currentPosition.y);
                wasObstacle = CheckAndMarkObstacle(nextPosition, rawImage, imageWidth, imageHeight, isNight);
                if (wasObstacle) processedCoordinates.Push(nextPosition);

                nextPosition = new Vector2(currentPosition.x, currentPosition.y - 1);
                wasObstacle = CheckAndMarkObstacle(nextPosition, rawImage, imageWidth, imageHeight, isNight);
                if (wasObstacle) processedCoordinates.Push(nextPosition);

                nextPosition = new Vector2(currentPosition.x, currentPosition.y + 1);
                wasObstacle = CheckAndMarkObstacle(nextPosition, rawImage, imageWidth, imageHeight, isNight);
                if (wasObstacle) processedCoordinates.Push(nextPosition);
            }
        }

        public static List<GameObject> DetectObjectsInImage(byte[] rawImage, int width, int height)
        {
            //Because the game has day-night cycle, check if we are playing at night or day
            var colorSum = 0;
            for (int i = 0; i < rawImage.Length; i++)
                colorSum += rawImage[i];
            colorSum = colorSum / rawImage.Length;
            bool isNight = (colorSum < 128);

            //Detect all the objects in the frame and add them to the list
            List<GameObject> gameObjects = new List<GameObject>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (CheckAndMarkObstacle(new Vector2(x, y), rawImage, width, height, isNight))
                    {
                        GameObject obj = new GameObject(x, y);
                        ProcessDetectedObject(new Vector2(x, y), obj, rawImage, width, height, isNight);

                        //If the object is big enough, add it to the list of objects
                        if (obj.size.x > 5 || obj.size.y > 5)
                            gameObjects.Add(obj);
                    }
                }
            }
            return gameObjects;
        }
    }
}
