using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRexAI
{
    public class GameObject
    {
        static int currentId = 0;

        public enum ObjectType { Dinosaur, Obstacle };

        public ObjectType objectType = ObjectType.Obstacle;
        public int id;
        public Vector2 position;//position of the top left corner of the object
        public Vector2 size;//dimensions in pixels
        public int speed;//Speed is measured in pixels per frame
        public int speedFramesTracked;//Number of consecutive frames with the same speed tracked

        public GameObject(int positionX, int positionY)
        {
            position = new Vector2(positionX, positionY);
            size = new Vector2(1, 1);

            //Give the object an ID
            currentId++;
            id = currentId;
        }
    }
}
